namespace BitSoft.BinaryTools.Patch;

public sealed class PatchBlockInfo(int blockIndex, uint hash, int length)
{
    public int BlockIndex { get; } = blockIndex;

    public uint Hash { get; } = hash;

    public int Length { get; } = length;
}
