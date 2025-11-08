using System;
using System.Collections.Generic;
using System.IO;

namespace BitSoft.BinaryTools.Patch;

public sealed class BinaryPatchSource
{
    private readonly BlockInfoContainer _blockInfoContainer;
    private readonly int _blockSize;

    private BinaryPatchSource(BlockInfoContainer blockInfoContainer, int blockSize)
    {
        _blockInfoContainer = blockInfoContainer ?? throw new ArgumentNullException(nameof(blockInfoContainer));
        _blockSize = blockSize;
    }

    public static BinaryPatchSource Create(ReadOnlyMemory<byte> original, int blockSize = 4 * 1024)
    {
        var hashes = CalculateHashes(original, blockSize);
        return new BinaryPatchSource(hashes, blockSize);
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
            var checksum = rollingHash.GetChecksum();

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

    private static BlockInfoContainer CalculateHashes(ReadOnlyMemory<byte> original, int blockSize)
    {
        var blockInfoContainer = new BlockInfoContainer(length: original.Length, blockSize: blockSize);

        var blockIndex = 0;

        while (true)
        {
            var offset = blockIndex * blockSize;
            var left = original.Length - offset;
            if (left == 0)
                break;
            var length = Math.Min(left, blockSize);

            var slice = original.Slice(start: offset, length: length);

            var hash = RollingHash.Create(slice.Span);

            blockInfoContainer.Process(hash: hash, blockIndex: blockIndex, blockLength: length);

            if (length < blockSize)
                break;

            blockIndex += 1;
        }

        return blockInfoContainer;
    }
}