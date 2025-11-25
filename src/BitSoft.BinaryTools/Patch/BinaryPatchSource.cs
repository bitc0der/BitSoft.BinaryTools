using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BitSoft.BinaryTools.Patch;

public sealed class BinaryPatchSource
{
    private readonly BlockInfoContainer _blockInfoContainer;
    private readonly int _blockSize;

    private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;

    private BinaryPatchSource(BlockInfoContainer blockInfoContainer, int blockSize)
    {
        _blockInfoContainer = blockInfoContainer ?? throw new ArgumentNullException(nameof(blockInfoContainer));
        _blockSize = blockSize;
    }

    public static async ValueTask<BinaryPatchSource> CreateAsync(Stream original, int blockSize = 4 * 1024)
    {
        ArgumentNullException.ThrowIfNull(original);
        var blockInfoContainer = await CalculateHashesAsync(original, blockSize);
        return new BinaryPatchSource(blockInfoContainer, blockSize);
    }

    public async ValueTask<BinaryPatch> CalculateAsync(Stream modified, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(modified);

        var segments = new List<IBinaryPatchSegment>();

        var bufferLength = _blockSize * 2;
        var buffer = Pool.Rent(minimumLength: bufferLength);
        try
        {
            var length = await modified.ReadAsync(buffer.AsMemory(start: 0, length: bufferLength), cancellationToken);
            if (length == 0)
                return new BinaryPatch(segments: [], _blockSize);

            const int NotDefined = -1;

            var segmentStart = NotDefined;
            var position = 0;

            RollingHash rollingHash = default;
            var resetHash = true;

            while (true)
            {
                while (position < length)
                {
                    if (resetHash)
                    {
                        var spanLength = Math.Min(_blockSize, length);
                        var bufferSpan = buffer.AsSpan(start: 0, length: spanLength);
                        rollingHash = RollingHash.Create(bufferSpan);
                        resetHash = false;
                    }

                    var block = _blockInfoContainer.Match(rollingHash);

                    if (block is null)
                    {
                        if (length <= _blockSize)
                        {
                            var dataPatchSegment = new DataPatchSegment(
                                memory: buffer.AsMemory(start: position, length: length)
                            );
                            segments.Add(dataPatchSegment);
                            position = 0;
                            break;
                        }

                        if (segmentStart == NotDefined)
                        {
                            segmentStart = position;
                        }
                        else if (position - segmentStart + 1 == _blockSize)
                        {
                            var dataPatchSegment = new DataPatchSegment(
                                memory: buffer.AsMemory(start: segmentStart, length: position - segmentStart + 1)
                            );
                            segments.Add(dataPatchSegment);

                            buffer
                                .AsSpan(start: position + 1, length: bufferLength - position - 2)
                                .CopyTo(buffer.AsSpan(start: 0));

                            segmentStart = NotDefined;
                            resetHash = true;

                            break;
                        }

                        position += 1;

                        if (position == length)
                        {
                            var dataPatchSegment = new DataPatchSegment(
                                memory: buffer.AsMemory(start: segmentStart, length: position - segmentStart)
                            );
                            segments.Add(dataPatchSegment);
                            position = 0;
                            break;
                        }

                        if (position + _blockSize < length)
                        {
                            var removedByte = buffer[position - 1];
                            var addedByte = buffer[position + _blockSize - 1];
                            rollingHash.Update(removed: removedByte, added: addedByte);
                        }
                        else
                        {
                            resetHash = true;
                        }
                    }
                    else
                    {
                        if (segmentStart != NotDefined)
                        {
                            var dataPatchSegment = new DataPatchSegment(
                                memory: buffer.AsMemory(start: segmentStart, length: position - segmentStart)
                            );
                            segments.Add(dataPatchSegment);
                            segmentStart = NotDefined;
                        }

                        var copyPatchSegment = new CopyPatchSegment(blockIndex: block.BlockIndex, length: block.Length);
                        segments.Add(copyPatchSegment);

                        buffer
                            .AsSpan(start: position + block.Length, length: bufferLength - position - block.Length - 1)
                            .CopyTo(buffer.AsSpan(start: 0));

                        resetHash = true;

                        break;
                    }
                }

                length = await modified.ReadAsync(
                    buffer.AsMemory(start: position, length: bufferLength - position - 1),
                    cancellationToken: cancellationToken
                );

                length += position;
                position = 0;

                if (length == 0)
                    break;
            }
        }
        finally
        {
            Pool.Return(buffer);
        }

        return new BinaryPatch(segments, _blockSize);
    }

    public static void Apply(ReadOnlyMemory<byte> original, BinaryPatch patch, Stream target)
    {
        ArgumentNullException.ThrowIfNull(patch);
        ArgumentNullException.ThrowIfNull(target);

        foreach (var segment in patch.Segments)
        {
            switch (segment)
            {
                case DataPatchSegment dataPatchSegment:
                    target.Write(dataPatchSegment.Memory.Span);
                    break;
                case CopyPatchSegment copyPatchSegment:
                    var slice = original.Slice(
                        start: copyPatchSegment.BlockIndex * patch.BlockSize,
                        length: copyPatchSegment.Length
                    );
                    target.Write(slice.Span);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }

    private static async ValueTask<BlockInfoContainer> CalculateHashesAsync(
        Stream source,
        int blockSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (!source.CanRead)
            throw new ArgumentException("source stream must be readable.", nameof(source));

        var blockInfoContainer = new BlockInfoContainer();

        var blockIndex = 0;

        var buffer = Pool.Rent(blockSize);
        try
        {
            while (true)
            {
                var length = await source.ReadAsync(buffer.AsMemory(start: 0, length: blockSize), cancellationToken);
                if (length == 0)
                    break;

                var hash = RollingHash.Create(buffer.AsSpan(start: 0, length: length));

                blockInfoContainer.Process(hash: hash, blockIndex: blockIndex, blockLength: length);

                if (length < blockSize)
                    break;

                blockIndex += 1;
            }
        }
        finally
        {
            Pool.Return(buffer);
        }

        return blockInfoContainer;
    }
}