using System.Buffers;

namespace BitSoft.BinaryTools.Patch.Stream;

public class BinaryPatchReaderSettings
{
    public static BinaryPatchReaderSettings Default { get; } = new();

    public ArrayPool<byte> ArrayPool { get; set; } = ArrayPool<byte>.Shared;
}