using System;
using System.Collections.Generic;

namespace BitSoft.BinaryTools.Patch;

internal sealed class BlockInfoContainer
{
    private readonly Dictionary<uint, List<PatchBlockInfo>> _hashes = new();

    public void Process(int blockIndex, RollingHash hash, byte[] strongHash)
    {
        var checksum = hash.GetChecksum();
        var block = new PatchBlockInfo(blockIndex: blockIndex, hash: checksum, strongHash);
        if (!_hashes.TryGetValue(checksum, out var blocks))
        {
            _hashes[block.Hash] = blocks = [];
        }

        blocks.Add(block);
    }

    public void Process(int blockIndex, int blockLength, RollingHash hash, byte[] strongHash)
    {
        var checksum = hash.GetChecksum();
        var block = new PatchBlockInfoWithLength(
            blockIndex: blockIndex,
            length: blockLength,
            hash: checksum,
            strongHash: strongHash
        );
        if (!_hashes.TryGetValue(checksum, out var blocks))
        {
            _hashes[block.Hash] = blocks = [];
        }

        blocks.Add(block);
    }

    public PatchBlockInfo? Match(RollingHash hash, ReadOnlySpan<byte> strongHash)
    {
        var checksum = hash.GetChecksum();
        if (_hashes.TryGetValue(checksum, out var blocks))
        {
            foreach (var block in blocks)
            {
                if (block.StrongHash.SequenceEqual(strongHash))
                    return block;
            }
        }

        return null;
    }
}
