using System;

namespace BitSoft.BinaryTools.Patch;

public sealed class CopyPatchSegment : IBinaryPatchSegment
{
    public int BlockIndex { get; }

    public int Length { get; }

    public CopyPatchSegment(int blockIndex, int length)
    {
        BlockIndex = blockIndex;
        Length = length;
    }
}

public sealed class DataPatchSegment : IBinaryPatchSegment
{
    public int Offset { get; }

    public ReadOnlyMemory<byte> Memory { get; }

    public DataPatchSegment(int offset, ReadOnlyMemory<byte> memory)
    {
        Offset = offset;
        Memory = memory;
    }
}