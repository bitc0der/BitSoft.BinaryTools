using System;

namespace BitSoft.BinaryTools.Patch;

public sealed class BinaryPathSegment
{
    public int Offset { get; }
    public int Length { get; }
    
    public ReadOnlyMemory<byte> Memory { get; }

    public BinaryPathSegment(int offset, int length, ReadOnlyMemory<byte> memory)
    {
        Offset = offset;
        Length = length;
        Memory = memory;
    }
}