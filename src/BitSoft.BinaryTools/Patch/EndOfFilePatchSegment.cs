namespace BitSoft.BinaryTools.Patch;

public sealed class EndOfFilePatchSegment : IBinaryPatchSegment
{
    public EndOfFilePatchSegment(int offset)
    {
        Offset = offset;
    }

    public int Offset { get; }
}