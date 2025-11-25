using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BitSoft.BinaryTools.Patch;

public sealed class BinaryPatch
{
    private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;

    public static async ValueTask CreateAsync(
        Stream source,
        Stream modified,
        Stream output,
        int blockSize = 4 * 1024,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(modified);
        ArgumentNullException.ThrowIfNull(output);

        if (!modified.CanRead)
            throw new ArgumentException($"{nameof(modified)} does not support reading.", nameof(modified));
        if (!output.CanWrite)
            throw new ArgumentException($"{nameof(output)} does not support writing.", nameof(output));

        var blockInfoContainer = await CalculateHashesAsync(source, blockSize, cancellationToken);

        using var writer = new PatchWriter(output);

        await writer.WriteHeaderAsync(blockSize: blockSize, cancellationToken);

        var bufferLength = blockSize * 2;
        var buffer = Pool.Rent(minimumLength: bufferLength);
        try
        {
            var length = await modified.ReadAsync(buffer.AsMemory(start: 0, length: bufferLength), cancellationToken);
            if (length == 0)
                return;

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
                        var spanLength = Math.Min(blockSize, length);
                        var bufferSpan = buffer.AsSpan(start: 0, length: spanLength);
                        rollingHash = RollingHash.Create(bufferSpan);
                        resetHash = false;
                    }

                    var block = blockInfoContainer.Match(rollingHash);

                    if (block is null)
                    {
                        if (length <= blockSize)
                        {
                            var memory = buffer.AsMemory(start: position, length: length);
                            await writer.WriteDataAsync(memory, cancellationToken);
                            position = 0;
                            break;
                        }

                        if (segmentStart == NotDefined)
                        {
                            segmentStart = position;
                        }
                        else if (position - segmentStart + 1 == blockSize)
                        {
                            var memory = buffer.AsMemory(start: segmentStart, length: position - segmentStart + 1);
                            await writer.WriteDataAsync(memory, cancellationToken);

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
                            var memory = buffer.AsMemory(start: segmentStart, length: position - segmentStart);
                            await writer.WriteDataAsync(memory, cancellationToken);
                            position = 0;
                            break;
                        }

                        if (position + blockSize < length)
                        {
                            var removedByte = buffer[position - 1];
                            var addedByte = buffer[position + blockSize - 1];
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
                            var memory = buffer.AsMemory(start: segmentStart, length: position - segmentStart);
                            await writer.WriteDataAsync(memory, cancellationToken);
                            segmentStart = NotDefined;
                        }

                        await writer.WriteCopyAsync(
                            blockIndex: block.BlockIndex,
                            blockLength: block.Length,
                            cancellationToken: cancellationToken
                        );

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

        await writer.CompleteAsync(cancellationToken);
    }

    public static async ValueTask ApplyAsync(
        Stream source,
        Stream patch,
        Stream output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(patch);
        ArgumentNullException.ThrowIfNull(output);

        if (!source.CanRead)
            throw new ArgumentException("source stream must be readable.", nameof(source));
        if (!source.CanSeek)
            throw new ArgumentException("source stream must be seekable.", nameof(source));
        if (!patch.CanRead)
            throw new ArgumentException("patch stream must be readable.", nameof(patch));
        if (!output.CanWrite)
            throw new ArgumentException("output stream must be writable.", nameof(output));

        using var reader = new PatchReader(patch);
        var blockSize = await reader.InitializeAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            switch (reader.Segment)
            {
                case DataPatchSegment dataPatchSegment:
                    await output.WriteAsync(dataPatchSegment.Data, cancellationToken);
                    break;
                case CopyPatchSegment copyPatchSegment:
                    var targetPosition = blockSize * copyPatchSegment.BlockIndex;
                    source.Seek(targetPosition, SeekOrigin.Begin);
                    var buffer = Pool.Rent(copyPatchSegment.BlockLength);
                    try
                    {
                        var count = await source.ReadAsync(buffer,
                            offset: 0,
                            count: copyPatchSegment.BlockLength,
                            cancellationToken);
                        await output.WriteAsync(buffer, offset: 0, count: count, cancellationToken);
                    }
                    finally
                    {
                        Pool.Return(buffer);
                    }

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