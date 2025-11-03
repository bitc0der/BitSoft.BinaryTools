using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BitSoft.BinaryTools.Patch.Stream;

public sealed class BinaryPatchReader
{
    private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;

    private static readonly Encoding Encoding = Encoding.UTF8;

    public async ValueTask ApplyAsync(
        System.IO.Stream original,
        System.IO.Stream binaryPatch,
        System.IO.Stream output,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(binaryPatch);
        ArgumentNullException.ThrowIfNull(output);

        using var binaryPathReader = new BinaryReader(binaryPatch, Encoding);
        await using var writer = new BinaryWriter(output, Encoding);

        byte[]? buffer = null;

        try
        {
            while (true)
            {
                var segmentType = binaryPathReader.ReadByte();

                switch (segmentType)
                {
                    case BinaryPatchConst.SegmentType.Header:
                        var segmentSize = ReadHeader(binaryPathReader);
                        buffer = Pool.Rent(segmentSize);
                        break;
                    case BinaryPatchConst.SegmentType.Data:
                        if (buffer is null) throw new InvalidOperationException("Wrong segment sequence");
                        var length = ReadDataSegment(binaryPathReader, buffer);
                        break;
                    default:
                        throw new InvalidOperationException("Invalid segment type");
                }
            }
        }
        finally
        {
            Pool.Return(buffer);
        }
    }

    private static int ReadDataSegment(BinaryReader binaryPatch, byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(binaryPatch);

        var blockIndex = binaryPatch.ReadInt64();
        var length = binaryPatch.ReadInt32();

        if (length > buffer.Length)
            throw new InvalidOperationException("Invalid segment length");

        var result = binaryPatch.Read(buffer, 0, length);

        if (result != length)
            throw new InvalidOperationException("Invalid result length");

        return result;
    }

    private static int ReadHeader(BinaryReader binaryPatch)
    {
        ArgumentNullException.ThrowIfNull(binaryPatch);

        var segmentType = binaryPatch.ReadByte();

        if (segmentType != BinaryPatchConst.SegmentType.Header)
            throw new InvalidOperationException("Invalid segment type");

        var expectedLength = Encoding.GetByteCount(BinaryPatchConst.Header);

        var buffer = Pool.Rent(minimumLength: expectedLength);
        try
        {
            var length = binaryPatch.Read(buffer, index: 0, count: expectedLength);

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

        var protocolVersion = binaryPatch.ReadInt32();

        if (protocolVersion > BinaryPatchConst.ProtocolVersion)
            throw new InvalidOperationException("Invalid protocol version");

        var segmentSize = binaryPatch.ReadInt32();

        return segmentSize;
    }
}