namespace BitSoft.BinaryTools.Patch;

internal sealed class PatchBlockInfoWithLength(int blockIndex, uint hash, int length)
    : PatchBlockInfo(blockIndex, hash)
{
    public int Length { get; } = length;
}
