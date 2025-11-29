namespace BitSoft.BinaryTools.Patch;

internal class PatchBlockInfo(int blockIndex, uint hash, byte[] strongHash)
{
    public int BlockIndex { get; } = blockIndex;

    public uint Hash { get; } = hash;

    public byte[] StrongHash { get; } = strongHash;
}
