using System;

namespace BitSoft.BinaryTools.Patch;

public sealed class DataPatchSegment : IBinaryPatchSegment
{
    public ReadOnlyMemory<byte> Memory { get; }

    public DataPatchSegment(ReadOnlyMemory<byte> memory)
    {
        Memory = memory;
    }
}