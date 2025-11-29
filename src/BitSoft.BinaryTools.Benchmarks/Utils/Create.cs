using System;

namespace BitSoft.BinaryTools.Benchmarks.Utils;

public static class Create
{
    public static void RandomData(Span<byte> buffer)
    {
        Random.Shared.NextBytes(buffer);
    }
}
