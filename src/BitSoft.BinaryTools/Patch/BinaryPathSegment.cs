namespace BitSoft.BinaryTools.Patch;

public sealed class BinaryPathSegment
{
    public int Offset { get; }
    public int Length { get; }

    public BinaryPathSegment(int offset, int length)
    {
        Offset = offset;
        Length = length;
    }
}