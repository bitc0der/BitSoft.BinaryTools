namespace BitSoft.BinaryTools.Patch;

internal sealed class PatchBlockInfoWithLength(int blockIndex, uint hash, byte[] strongHash, int length)
    : PatchBlockInfo(blockIndex, hash, strongHash)
{
    public int Length { get; } = length;
}
