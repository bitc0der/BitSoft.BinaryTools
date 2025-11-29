using System.Text;

namespace BitSoft.BinaryTools.Patch;

public static class ProtocolConst
{
    public static Encoding DefaultEncoding => Encoding.UTF8;

    public const int ProtocolVersion = 1;

    public static class SegmentTypes
    {
        public const byte CopyBlock = 0x1;
        public const byte CopyBlockWithLength = 0x2;
        public const byte DataPatchSegment = 0x3;

        public const byte EndPatchSegment = byte.MaxValue;
    }
}
