using System;

namespace BitSoft.BinaryTools.Patch;

internal sealed class DataPatchSegment(ReadOnlyMemory<byte> data) : IPatchSegment
{
    public ReadOnlyMemory<byte> Data { get; } = data;
}
