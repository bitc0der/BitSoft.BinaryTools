namespace BitSoft.BinaryTools.Patch;

public sealed class EndOfFilePathSegment : IBinaryPatchSegment
{
    public EndOfFilePathSegment(int offset)
    {
        Offset = offset;
    }

    public int Offset { get; }
}