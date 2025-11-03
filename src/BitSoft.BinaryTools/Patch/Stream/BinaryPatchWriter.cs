using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BitSoft.BinaryTools.Patch.Stream;

public static class BinaryPatchWriter
{
    private static readonly Encoding Encoding = Encoding.UTF8;

    public static async ValueTask WritePatchAsync(
        System.IO.Stream original,
        System.IO.Stream modified,
        System.IO.Stream output,
        int segmentSize = 1024,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(modified);
        ArgumentNullException.ThrowIfNull(output);

        await using var writer = new BinaryWriter(output, encoding: Encoding.UTF8, leaveOpen: true);

        WriteHeaderSegment(writer, segmentSize);

        var pool = ArrayPool<byte>.Shared;

        var leftBuffer = pool.Rent(segmentSize);
        var rightBuffer = pool.Rent(segmentSize);

        long blockIndex = 1;

        try
        {
            while (true)
            {
                var leftCount = original.CanRead
                    ? await original.ReadAsync(leftBuffer, offset: 0, count: segmentSize, cancellationToken)
                    : 0;
                var rightCount = modified.CanRead
                    ? await modified.ReadAsync(rightBuffer, offset: 0, count: segmentSize, cancellationToken)
                    : 0;

                if (leftCount == 0 && rightCount == 0)
                    break;

                if (leftCount < rightCount)
                {
                    WriteDataSegment(writer, blockIndex, leftBuffer, leftCount);
                }
                else if (leftCount >= rightCount)
                {
                    for (var i = 0; i < leftCount; i++)
                    {
                        var left = leftBuffer[i];
                        var right = rightBuffer[i];

                        if (left == right)
                            continue;

                        WriteDataSegment(writer, blockIndex, leftBuffer, leftCount);

                        break;
                    }
                }

                blockIndex += 1;
            }
        }
        finally
        {
            pool.Return(leftBuffer);
            pool.Return(rightBuffer);
        }
    }

    private static void WriteHeaderSegment(BinaryWriter writer, int segmentSize)
    {
        ArgumentNullException.ThrowIfNull(writer);

        const string header = BinaryPatchConst.Header;
        var headerLength = Encoding.GetByteCount(header);

        writer.Write(BinaryPatchConst.SegmentType.Header);
        writer.Write(BinaryPatchConst.ProtocolVersion);
        writer.Write(headerLength);
        writer.Write(header);
        writer.Write(segmentSize);
    }

    private static void WriteDataSegment(BinaryWriter writer, long blockIndex, byte[] buffer, int length)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.Write(BinaryPatchConst.SegmentType.Data);
        writer.Write(blockIndex);
        writer.Write(length);
        writer.Write(buffer, index: 0, count: length);
    }
}