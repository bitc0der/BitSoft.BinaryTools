using System;
using System.Collections.Generic;
using System.IO;

namespace BitSoft.BinaryTools.Patch;

public sealed class BinaryPatch
{
    public int BlockSize { get; }

    public IReadOnlyList<IBinaryPatchSegment> Segments { get; }

    internal BinaryPatch(IReadOnlyList<IBinaryPatchSegment> segments, int blockSize)
    {
        Segments = segments ?? throw new ArgumentNullException(nameof(segments));
        BlockSize = blockSize;
    }

    public void Write(Stream target)
    {
        ArgumentNullException.ThrowIfNull(target);

        using var binaryWriter = new BinaryWriter(target);
        binaryWriter.Write(BlockSize);
        for (var i = 0; i < Segments.Count; i++)
        {
            var segment = Segments[i];
            byte segmentType = segment switch
            {
                CopyPatchSegment => 0x1,
                DataPatchSegment => 0x2,
                _ => throw new InvalidOperationException($"Invalid segment type '{segment.GetType()}'.")
            };
            binaryWriter.Write(segmentType);
            switch (segment)
            {
                case CopyPatchSegment copyPatchSegment:
                    binaryWriter.Write(copyPatchSegment.BlockIndex);
                    binaryWriter.Write(copyPatchSegment.Length);
                    break;
                case DataPatchSegment dataPatchSegment:
                    binaryWriter.Write(dataPatchSegment.Memory.Length);
                    binaryWriter.Write(dataPatchSegment.Memory.Span);
                    break;
            }
        }
    }

    public void Read(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);

        using var binaryReader = new BinaryReader(source);
        var blockSize = binaryReader.ReadInt32();
    }
}