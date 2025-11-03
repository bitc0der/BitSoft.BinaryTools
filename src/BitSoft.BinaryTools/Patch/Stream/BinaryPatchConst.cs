namespace BitSoft.BinaryTools.Patch.Stream;

internal static class BinaryPatchConst
{
    public const string Header = "BPT";

    public const int ProtocolVersion = 1;
    
    public static class SegmentType
    {
        public const byte Header = 1;
        public const byte Data = 2;
    }
}