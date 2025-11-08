namespace BitSoft.BinaryTools.Patch;

public sealed class CopyPatchSegment : IBinaryPatchSegment
{
    public int BlockIndex { get; }

    public int Length { get; }

    public CopyPatchSegment(int blockIndex, int length)
    {
        BlockIndex = blockIndex;
        Length = length;
    }
}