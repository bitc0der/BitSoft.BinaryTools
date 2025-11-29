using System;

namespace BitSoft.BinaryTools.Patch;

public struct RollingHash
{
    private const uint Base = 65521;

    private uint _a;
    private uint _b;
    private readonly uint _length;

    private RollingHash(uint a, uint b, uint length)
    {
        _a = a;
        _b = b;
        _length = length;
    }

    public static RollingHash Create(ReadOnlySpan<byte> data)
    {
        uint a = 1;
        uint b = 0;

        for (var i = 0; i < data.Length; i++)
        {
            var value = data[i];

            a = (a + value) % Base;
            b = (b + a) % Base;
        }

        return new RollingHash(a: a, b: b, length: (uint)data.Length);
    }

    public void Update(byte removed, byte added)
    {
        _a = (_a - removed + added) % Base;
        _b = (_b - _length * removed + _a - 1) % Base;
    }

    public uint GetChecksum() => (_b << 16) | _a;
}
