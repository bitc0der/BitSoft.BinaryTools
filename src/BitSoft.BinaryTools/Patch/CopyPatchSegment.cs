namespace BitSoft.BinaryTools.Patch;

internal sealed class CopyPatchSegment(int blockIndex, int blockLength) : IPatchSegment
{
    public int BlockIndex { get; } = blockIndex;

    public int BlockLength { get; } = blockLength;
}
