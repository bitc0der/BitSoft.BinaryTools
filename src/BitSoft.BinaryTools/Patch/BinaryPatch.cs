using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BitSoft.BinaryTools.Patch;

public sealed class BinaryPatch
{
    public int BlockSize { get; }

    public IReadOnlyList<IBinaryPatchSegment> Segments { get; }

    public static Encoding DefaultEncoding => Encoding.UTF8;

    public BinaryPatch(IReadOnlyList<IBinaryPatchSegment> segments, int blockSize)
    {
        Segments = segments ?? throw new ArgumentNullException(nameof(segments));
        BlockSize = blockSize;
    }

    public void Write(Stream target, Encoding? encoding = null)
    {
        ArgumentNullException.ThrowIfNull(target);

        encoding ??= DefaultEncoding;

        using var binaryWriter = new BinaryWriter(target, encoding, leaveOpen: true);
        binaryWriter.Write(BlockSize);
        for (var i = 0; i < Segments.Count; i++)
        {
            var segment = Segments[i];

            var segmentType = segment switch
            {
                CopyPatchSegment => SegmentTypes.CopyPatchSegment,
                DataPatchSegment => SegmentTypes.DataPatchSegment,
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

        binaryWriter.Write(SegmentTypes.EndPatchSegment);
    }

    public static BinaryPatch Read(Stream source, Encoding? encoding = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        encoding ??= DefaultEncoding;

        using var binaryReader = new BinaryReader(source, encoding, leaveOpen: true);
        var blockSize = binaryReader.ReadInt32();

        var segments = new List<IBinaryPatchSegment>();

        while (true)
        {
            var segmentType = binaryReader.ReadByte();
            IBinaryPatchSegment? segment = null;
            switch (segmentType)
            {
                case SegmentTypes.CopyPatchSegment:
                {
                    var blockIndex = binaryReader.ReadInt32();
                    var length = binaryReader.ReadInt32();
                    segment = new CopyPatchSegment(blockIndex: blockIndex, length: length);
                    break;
                }
                case SegmentTypes.DataPatchSegment:
                {
                    var length = binaryReader.ReadInt32();
                    var bytes = binaryReader.ReadBytes(length);
                    segment = new DataPatchSegment(bytes);
                    break;
                }
                case SegmentTypes.EndPatchSegment:
                    // do nothing
                    break;
                default:
                    throw new InvalidOperationException($"Invalid segment type Id '{segmentType}'.");
            }

            if (segment is not null)
                segments.Add(segment);
            else
                break;
        }

        return new BinaryPatch(segments, blockSize);
    }

    private static class SegmentTypes
    {
        public const byte CopyPatchSegment = 0x1;
        public const byte DataPatchSegment = 0x2;

        public const byte EndPatchSegment = byte.MaxValue;
    }
}