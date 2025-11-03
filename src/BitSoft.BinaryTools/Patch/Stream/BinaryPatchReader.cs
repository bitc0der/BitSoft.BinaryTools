using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BitSoft.BinaryTools.Patch.Stream;

public static class BinaryPatchReader
{
    private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;

    private static readonly Encoding Encoding = Encoding.UTF8;

    public static async ValueTask ApplyAsync(
        System.IO.Stream original,
        System.IO.Stream binaryPatch,
        System.IO.Stream output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(binaryPatch);
        ArgumentNullException.ThrowIfNull(output);

        using var originalRreader = new BinaryReader(original, Encoding);
        using var binaryPathReader = new BinaryReader(binaryPatch, Encoding);
        await using var writer = new BinaryWriter(output, Encoding);

        byte[]? originalBuffer = null;
        byte[]? patchBuffer = null;

        int segmentSize = 0;
        long blockIndex = 0;

        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var segmentType = binaryPathReader.ReadByte();

                switch (segmentType)
                {
                    case BinaryPatchConst.SegmentType.Header:
                        segmentSize = ReadHeader(binaryPathReader);
                        originalBuffer = Pool.Rent(segmentSize);
                        patchBuffer = Pool.Rent(segmentSize);
                        break;
                    case BinaryPatchConst.SegmentType.Data:
                        if (originalBuffer is null || patchBuffer is null)
                            throw new InvalidOperationException("Wrong segment sequence");

                        var (patchSegmentIndex, segmentLength) = ReadDataSegment(binaryPathReader, patchBuffer);

                        while (blockIndex < patchSegmentIndex)
                        {
                            var length = originalRreader.Read(originalBuffer, index: 0, count: segmentSize);
                            if (length > 0)
                                writer.Write(originalBuffer, index: 0, count: length);
                            blockIndex += 1;
                        }

                        writer.Write(patchBuffer, index: 0, count: segmentLength);
                        break;
                    default:
                        throw new InvalidOperationException("Invalid segment type");
                }
            }
        }
        finally
        {
            if (originalBuffer is not null)
                Pool.Return(originalBuffer);
            if (patchBuffer is not null)
                Pool.Return(patchBuffer);
        }
    }

    private static (long BlockIndex, int SegmentLength) ReadDataSegment(BinaryReader binaryPatch, byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(binaryPatch);

        var blockIndex = binaryPatch.ReadInt64();
        var bufferLength = binaryPatch.ReadInt32();
        if (bufferLength > buffer.Length)
            throw new InvalidOperationException("Invalid segment length");

        var length = binaryPatch.Read(buffer, 0, bufferLength);
        if (length != bufferLength)
            throw new InvalidOperationException("Invalid result length");

        return (BlockIndex: blockIndex, SegmentLength: length);
    }

    private static int ReadHeader(BinaryReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        var protocolVersion = reader.ReadInt32();

        if (protocolVersion > BinaryPatchConst.ProtocolVersion)
            throw new InvalidOperationException("Invalid protocol version");

        var expectedLength = reader.ReadInt32();

        var buffer = Pool.Rent(minimumLength: expectedLength);
        try
        {
            var length = reader.Read(buffer, index: 0, count: expectedLength);

            if (length != expectedLength)
                throw new InvalidOperationException("Invalid prefix length");

            var value = Encoding.UTF8.GetString(buffer, index: 0, count: length);

            if (value != BinaryPatchConst.Header)
                throw new InvalidOperationException($"Invalid header value '{value}'");
        }
        finally
        {
            Pool.Return(buffer);
        }

        var segmentSize = reader.ReadInt32();

        return segmentSize;
    }
}