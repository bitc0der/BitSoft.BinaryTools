namespace BitSoft.BinaryTools.Patch;

internal sealed class CopyBlockSegment(int blockIndex) : IPatchSegment
{
    public int BlockIndex { get; } = blockIndex;
}
