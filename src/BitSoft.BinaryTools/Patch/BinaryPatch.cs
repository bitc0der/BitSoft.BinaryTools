using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BitSoft.BinaryTools.Patch;

public static class BinaryPatch
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

        using var reader = new StreamWindowReader(modified, Pool, windowSize: blockSize);
        using var writer = new PatchWriter(output);
        await writer.WriteHeaderAsync(blockSize: blockSize, cancellationToken);

        if (!await reader.MoveAsync(cancellationToken))
        {
            await writer.CompleteAsync(cancellationToken);
            return;
        }

        RollingHash rollingHash = default;
        var resetHash = true;

        using var strongHashCalculator = new HashCalculator();

        while (true)
        {
            if (resetHash)
            {
                rollingHash = RollingHash.Create(reader.Window.Span);
                resetHash = false;
            }

            var strongHash = strongHashCalculator.CalculatedHash(reader.Window.Span);

            var block = blockInfoContainer.Match(rollingHash, strongHash);

            if (block is null)
            {
                if (!reader.IsPinned)
                {
                    reader.PinPosition();
                }

                if (reader.Finished)
                {
                    if (reader.IsPinned)
                        await writer.WriteDataAsync(reader.PinnedWindowWithCurrent, cancellationToken);
                    else
                        await writer.WriteDataAsync(reader.Window, cancellationToken);
                    break;
                }

                if (reader.PinnedWindowWithCurrent.Length == blockSize)
                {
                    await writer.WriteDataAsync(reader.PinnedWindowWithCurrent, cancellationToken);
                    reader.ResetPinnedPosition();
                }

                var firstByte = reader.Window.Span[0];
                if (await reader.MoveAsync(cancellationToken))
                {
                    var newByte = reader.Window.Span[reader.Window.Length - 1];
                    rollingHash.Update(removed: firstByte, added: newByte);
                }
                else
                {
                    break;
                }
            }
            else
            {
                if (reader.IsPinned)
                {
                    await writer.WriteDataAsync(reader.PinnedWindow, cancellationToken);
                    reader.ResetPinnedPosition();
                }

                if (block is PatchBlockInfoWithLength blockInfoWithLength)
                {
                    await writer.WriteCopyBlockWithLengthAsync(
                        blockIndex: block.BlockIndex,
                        blockLength: blockInfoWithLength.Length,
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    await writer.WriteCopyBlockAsync(
                        blockIndex: block.BlockIndex,
                        cancellationToken: cancellationToken
                    );
                }

                if (await reader.SlideWindowAsync(cancellationToken))
                {
                    resetHash = true;
                }
                else
                {
                    break;
                }
            }
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
                case CopyBlockSegment copyBlockSegment:
                    await CopyBlockSegmentAsync(
                        blockIndex: copyBlockSegment.BlockIndex,
                        blockLength: blockSize
                    );
                    break;
                case CopyBlockWithLengthSegment copyPatchSegment:
                    await CopyBlockSegmentAsync(
                        blockIndex: copyPatchSegment.BlockIndex,
                        blockLength: copyPatchSegment.BlockLength
                    );
                    break;
                default:
                    throw new NotSupportedException();
            }

            continue;

            async ValueTask CopyBlockSegmentAsync(int blockIndex, int blockLength)
            {
                var targetPosition = blockSize * blockIndex;
                source.Seek(targetPosition, SeekOrigin.Begin);
                var buffer = Pool.Rent(blockLength);
                try
                {
                    var memory = buffer.AsMemory(start: 0, length: blockLength);
                    var count = await source.ReadAsync(memory, cancellationToken);
                    if (count != blockLength) throw new InvalidOperationException();
                    await output.WriteAsync(memory, cancellationToken);
                }
                finally
                {
                    Pool.Return(buffer);
                }
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

        using var hashCalculator = new HashCalculator();

        var buffer = Pool.Rent(blockSize);
        try
        {
            while (true)
            {
                var length = await source.ReadAsync(buffer.AsMemory(start: 0, length: blockSize), cancellationToken);
                if (length == 0)
                    break;

                var span = buffer.AsSpan(start: 0, length: length);

                var hash = RollingHash.Create(span);
                var strongHash = hashCalculator.CalculatedHash(buffer, offset: 0, count: length);

                if (length == blockSize)
                {
                    blockInfoContainer.Process(blockIndex: blockIndex, hash: hash, strongHash: strongHash);
                }
                else
                {
                    blockInfoContainer.Process(blockIndex: blockIndex, blockLength: length, hash: hash, strongHash);
                }

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
