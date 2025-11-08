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

    public BinaryPatch Calculate(ReadOnlyMemory<byte> modified)
    {
        var maxSize = modified.Length / _blockSize;
        var segments = new List<IBinaryPatchSegment>(capacity: maxSize);

        var modifiedSpan = modified.Span;
        var initialSpan = modified.Span[..Math.Min(modifiedSpan.Length, _blockSize)];
        var rollingHash = RollingHash.Create(initialSpan);

        const int NotDefined = -1;

        var segmentStart = NotDefined;
        var position = 0;

        while (position < modifiedSpan.Length)
        {
            var block = _blockInfoContainer.Match(rollingHash);

            if (block is not null)
            {
                if (segmentStart != NotDefined)
                {
                    var dataPatchSegment = new DataPatchSegment(
                        memory: modified.Slice(start: segmentStart, length: position - segmentStart)
                    );
                    segments.Add(dataPatchSegment);
                    segmentStart = NotDefined;
                }

                var copyPatchSegment = new CopyPatchSegment(blockIndex: block.BlockIndex, length: block.Length);
                segments.Add(copyPatchSegment);
                position += block.Length;

                if (position >= modifiedSpan.Length)
                    break;

                var span = modifiedSpan.Slice(
                    start: position,
                    length: Math.Min(modifiedSpan.Length - position, _blockSize)
                );
                rollingHash = RollingHash.Create(span);
            }
            else
            {
                if (segmentStart == NotDefined)
                    segmentStart = position;

                position += 1;

                if (position == modifiedSpan.Length)
                {
                    if (segmentStart != NotDefined)
                    {
                        var dataPatchSegment = new DataPatchSegment(
                            memory: modified.Slice(start: segmentStart, length: position - segmentStart)
                        );
                        segments.Add(dataPatchSegment);
                    }

                    break;
                }

                var removedByte = modifiedSpan[position - 1];
                var addedByte =
                    position + _blockSize <= modifiedSpan.Length
                        ? modifiedSpan[position + _blockSize - 1]
                        : modifiedSpan[modifiedSpan.Length - 1];

                rollingHash.Update(removed: removedByte, added: addedByte);
            }
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