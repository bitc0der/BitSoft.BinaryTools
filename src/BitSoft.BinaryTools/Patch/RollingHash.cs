using System;

namespace BitSoft.BinaryTools.Patch;

public struct RollingHash
{
    private const uint Base = 65521;

    private long _a;
    private long _b;
    private readonly long _length;

    private RollingHash(long a, long b, int length)
    {
        _a = a;
        _b = b;
        _length = length;
    }

    public static RollingHash Create(ReadOnlySpan<byte> data)
    {
        long a = 1;
        long b = 0;

        foreach (var value in data)
        {
            a = (a + value) % Base;
            b = (b + a) % Base;
        }

        return new RollingHash(a: a, b: b, length: data.Length);
    }

    public void Update(byte removed, byte added)
    {
        // Use int for calculations within s1 update
        var s1_new = _a - removed + added;
        // Correct potential negative result back into positive range before modulo
        _a = (s1_new % Base + Base) % Base;

        // Use long for calculations within s2 update to handle large windowSize * byteOut
        var tempS2 = _b - _length * removed + _a - 1;

        // Correct potential negative result back into positive range before modulo
        _b = (tempS2 % Base + Base) % Base;
    }

    public uint GetChecksum() => (uint) ((_b << 16) | _a);
}
