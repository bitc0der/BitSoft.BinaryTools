using System;
using System.Collections.Generic;

namespace BitSoft.BinaryTools.Patch;

public class BinaryPatch
{
    private readonly LinkedList<IBinaryPatchSegment> _segments;

    public IReadOnlyCollection<IBinaryPatchSegment> Segments => _segments;

    private BinaryPatch(LinkedList<IBinaryPatchSegment> segments)
    {
        _segments = segments ?? throw new ArgumentNullException(nameof(segments));
    }

    public static BinaryPatch Calculate(
        ReadOnlyMemory<byte> original,
        ReadOnlyMemory<byte> modified,
        int blockSize = 1024)
    {
        var hashes = CalculateHashes(original, blockSize);

        var segments = new LinkedList<IBinaryPatchSegment>();

        var modifiedSpan = modified.Span;
        var initialSpan = modified.Span[..Math.Min(modifiedSpan.Length, blockSize)];
        var rollingHash = RollingHash.Create(initialSpan);

        const int NotDefined = -1;

        var segmentStart = NotDefined;
        var position = 0;

        while (position < modifiedSpan.Length)
        {
            var checksum = rollingHash.GetChecksum();

            if (hashes.TryGetValue(checksum, out var blocks))
            {
                if (segmentStart != NotDefined)
                {
                    var dataPatchSegment = new DataPatchSegment(
                        memory: modified.Slice(start: segmentStart, length: position - segmentStart)
                    );
                    segments.AddLast(dataPatchSegment);
                    segmentStart = NotDefined;
                }

                var block = blocks[0];
                var copyPatchSegment = new CopyPatchSegment(blockIndex: block.BlockIndex, length: block.Length);
                segments.AddLast(copyPatchSegment);
                position += block.Length;

                if (position == modifiedSpan.Length)
                    break;

                var span = modifiedSpan.Slice(
                    start: position,
                    length: Math.Min(modifiedSpan.Length - position, blockSize)
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
                        segments.AddLast(dataPatchSegment);
                    }

                    break;
                }

                var removedByte = modifiedSpan[position - 1];
                var addedByte = modifiedSpan[position + blockSize - 1];

                rollingHash.Update(removed: removedByte, added: addedByte);
            }
        }

        return new BinaryPatch(segments);
    }

    private static IReadOnlyDictionary<uint, List<Block>> CalculateHashes(ReadOnlyMemory<byte> original, int blockSize)
    {
        var hashes = new Dictionary<uint, List<Block>>();

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
            var checksum = hash.GetChecksum();
            var block = new Block(blockIndex, checksum, length);

            if (!hashes.TryGetValue(checksum, out var blocks))
            {
                hashes[checksum] = blocks = [];
            }

            blocks.Add(block);

            if (length < blockSize)
                break;

            blockIndex += 1;
        }

        return hashes;
    }

    private sealed class Block
    {
        public Block(int blockIndex, uint hash, int length)
        {
            BlockIndex = blockIndex;
            Hash = hash;
            Length = length;
        }

        public int BlockIndex { get; }

        public uint Hash { get; }

        public int Length { get; }
    }
}