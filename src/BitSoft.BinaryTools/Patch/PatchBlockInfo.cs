namespace BitSoft.BinaryTools.Patch;

public sealed class PatchBlockInfo
{
    public PatchBlockInfo(int blockIndex, uint hash, int length)
    {
        BlockIndex = blockIndex;
        Hash = hash;
        Length = length;
    }

    public int BlockIndex { get; }

    public uint Hash { get; }

    public int Length { get; }
}