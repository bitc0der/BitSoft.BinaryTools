namespace BitSoft.BinaryTools.Patch;

public static class Const
{
    public const int ProtocolVersion = 1;
    
    public static class SegmentTypes
    {
        public const byte CopyPatchSegment = 0x1;
        public const byte DataPatchSegment = 0x2;

        public const byte EndPatchSegment = byte.MaxValue;
    }
}