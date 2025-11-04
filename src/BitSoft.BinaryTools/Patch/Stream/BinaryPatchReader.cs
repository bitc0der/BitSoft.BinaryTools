using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BitSoft.BinaryTools.Patch.Stream;

public static class BinaryPatchReader
{
    public static async ValueTask ApplyAsync(
        System.IO.Stream original,
        System.IO.Stream binaryPatch,
        System.IO.Stream output,
        BinaryPatchReaderSettings? settings = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(binaryPatch);
        ArgumentNullException.ThrowIfNull(output);

        settings ??= new BinaryPatchReaderSettings();

        using var originalReader = new BinaryReader(original, BinaryPatchConst.Encoding);
        using var binaryPatchReader = new BinaryReader(binaryPatch, BinaryPatchConst.Encoding);
        await using var resultWriter = new BinaryWriter(output, BinaryPatchConst.Encoding);

        var pool = settings.ArrayPool;

        byte[]? originalBuffer = null;
        byte[]? patchBuffer = null;

        int segmentSize = 0;
        long blockIndex = 0;

        try
        {
            while (binaryPatchReader.BaseStream.CanRead)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var segmentType = binaryPatchReader.ReadByte();

                switch (segmentType)
                {
                    case BinaryPatchConst.SegmentType.Header:
                        segmentSize = ReadHeader(binaryPatchReader);
                        originalBuffer = pool.Rent(segmentSize);
                        patchBuffer = pool.Rent(segmentSize);
                        break;
                    case BinaryPatchConst.SegmentType.Data:
                        if (originalBuffer is null || patchBuffer is null)
                            throw new InvalidOperationException("Wrong segment sequence");

                        var (patchSegmentIndex, segmentLength) = ReadDataSegment(binaryPatchReader, patchBuffer);

                        while (blockIndex < patchSegmentIndex)
                        {
                            var length = originalReader.Read(originalBuffer, index: 0, count: segmentSize);
                            if (length > 0)
                                resultWriter.Write(originalBuffer, index: 0, count: length);
                            blockIndex += 1;
                        }

                        resultWriter.Write(patchBuffer, index: 0, count: segmentLength);
                        break;
                    case BinaryPatchConst.SegmentType.End:
                        return;
                    default:
                        throw new InvalidOperationException("Invalid segment type");
                }
            }
        }
        finally
        {
            if (originalBuffer is not null)
                pool.Return(originalBuffer);
            if (patchBuffer is not null)
                pool.Return(patchBuffer);
        }
    }

    private static (long BlockIndex, int SegmentLength) ReadDataSegment(BinaryReader binaryPatch, byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(binaryPatch);
        ArgumentNullException.ThrowIfNull(buffer);

        var blockIndex = binaryPatch.ReadInt64();
        var bufferLength = binaryPatch.ReadInt32();
        if (bufferLength > buffer.Length)
            throw new InvalidOperationException("Invalid segment length");

        var length = binaryPatch.Read(buffer, index: 0, count: bufferLength);
        if (length != bufferLength)
            throw new InvalidOperationException("Invalid result length");

        return (BlockIndex: blockIndex, SegmentLength: length);
    }

    private static int ReadHeader(BinaryReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        var prefix = reader.ReadString();

        if (prefix != BinaryPatchConst.Prefix)
            throw new InvalidOperationException("Invalid prefix.");

        var protocolVersion = reader.ReadInt32();

        if (protocolVersion > BinaryPatchConst.ProtocolVersion)
            throw new InvalidOperationException("Invalid protocol version");

        var segmentSize = reader.ReadInt32();

        return segmentSize;
    }
}