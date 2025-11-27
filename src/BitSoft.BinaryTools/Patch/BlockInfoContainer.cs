using System.Collections.Generic;

namespace BitSoft.BinaryTools.Patch;

internal sealed class BlockInfoContainer
{
    private readonly Dictionary<uint, List<PatchBlockInfo>> _hashes = new();

    public void Process(RollingHash hash, int blockIndex)
    {
        var checksum = hash.GetChecksum();
        var block = new PatchBlockInfo(blockIndex: blockIndex, hash: checksum);
        if (!_hashes.TryGetValue(checksum, out var blocks))
        {
            _hashes[block.Hash] = blocks = [];
        }

        blocks.Add(block);
    }

    public void Process(RollingHash hash, int blockIndex, int blockLength)
    {
        var checksum = hash.GetChecksum();
        var block = new PatchBlockInfoWithLength(blockIndex: blockIndex, hash: checksum, length: blockLength);
        if (!_hashes.TryGetValue(checksum, out var blocks))
        {
            _hashes[block.Hash] = blocks = [];
        }

        blocks.Add(block);
    }

    public PatchBlockInfo? Match(RollingHash hash)
    {
        var checksum = hash.GetChecksum();
        if (_hashes.TryGetValue(checksum, out var blocks))
        {
            foreach (var block in blocks)
            {
                return block;
            }
        }

        return null;
    }
}
