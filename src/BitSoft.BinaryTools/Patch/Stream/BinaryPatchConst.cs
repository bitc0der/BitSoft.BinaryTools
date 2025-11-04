using System.Text;

namespace BitSoft.BinaryTools.Patch.Stream;

internal static class BinaryPatchConst
{
    public static Encoding Encoding { get; } = Encoding.UTF8;

    public const string Prefix = "BINPATCH";

    public const int ProtocolVersion = 1;

    public static class SegmentType
    {
        public const byte Header = 1;
        public const byte Data = 2;

        public const byte End = byte.MaxValue;
    }
}