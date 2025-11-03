using System;

namespace BitSoft.BinaryTools.Patch;

public sealed class BinaryPatchSegment : IBinaryPatchSegment
{
    public int Offset { get; }
    public int Length { get; }
    
    public ReadOnlyMemory<byte> Memory { get; }

    public BinaryPatchSegment(int offset, int length, ReadOnlyMemory<byte> memory)
    {
        Offset = offset;
        Length = length;
        Memory = memory;
    }
}