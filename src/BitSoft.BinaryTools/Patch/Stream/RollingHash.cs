using System;

namespace BitSoft.BinaryTools.Patch.Stream;

public struct RollingHash
{
    private const uint Base = 65521;

    private uint _a;
    private uint _b;
    private uint _sumOfWindow;
    private readonly uint _length;

    public RollingHash(uint a, uint b, uint sumOfWindow, uint length)
    {
        _a = a;
        _b = b;
        _sumOfWindow = sumOfWindow;
        _length = length;
    }

    public static RollingHash Create(ReadOnlySpan<byte> data)
    {
        uint a = 1;
        uint b = 0;
        uint sumOfWindow = 0;

        for (var i = 0; i < data.Length; i++)
        {
            var value = data[i];

            a = (a + value) % Base;
            sumOfWindow = (sumOfWindow + value) % Base;
            b = (b + a) % Base;
        }

        return new RollingHash(a: a, b: b, sumOfWindow: sumOfWindow, length: (uint)data.Length);
    }

    public void Update(byte removed, byte added)
    {
        _a = (_a - removed + added) % Base;
        _sumOfWindow = (_sumOfWindow - removed + added) % Base;
        _b = (_b - _length * removed + _sumOfWindow) % Base;
    }

    public uint GetChecksum() => (_b << 16) | _a;
}