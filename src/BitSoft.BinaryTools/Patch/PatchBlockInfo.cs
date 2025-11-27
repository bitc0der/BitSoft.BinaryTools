namespace BitSoft.BinaryTools.Patch;

internal class PatchBlockInfo(int blockIndex, uint hash)
{
    public int BlockIndex { get; } = blockIndex;

    public uint Hash { get; } = hash;
}
