using System.Buffers;

namespace BitSoft.BinaryTools.Patch.Stream;

public class BinaryPatchWriterSettings
{
    public static BinaryPatchWriterSettings Default { get; } = new();

    public int SegmentSize { get; init; } = 1024;

    public ArrayPool<byte> ArrayPool { get; init; } = ArrayPool<byte>.Shared;
}