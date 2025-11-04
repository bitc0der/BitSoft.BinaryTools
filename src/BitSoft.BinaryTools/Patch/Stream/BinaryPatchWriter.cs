using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BitSoft.BinaryTools.Patch.Stream;

public static class BinaryPatchWriter
{
    public static async ValueTask WritePatchAsync(
        System.IO.Stream original,
        System.IO.Stream modified,
        System.IO.Stream output,
        BinaryPatchWriterSettings? settings = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(modified);
        ArgumentNullException.ThrowIfNull(output);

        settings ??= BinaryPatchWriterSettings.Default;

        await using var writer = new BinaryWriter(output, encoding: BinaryPatchConst.Encoding, leaveOpen: true);

        var segmentSize = settings.SegmentSize;

        if (segmentSize <= 0)
            throw new ArgumentOutOfRangeException(message: "Invalid segment size.", paramName: nameof(segmentSize));

        WriteHeaderSegment(writer, segmentSize);

        var pool = settings.ArrayPool;

        var leftBuffer = pool.Rent(segmentSize);
        var rightBuffer = pool.Rent(segmentSize);

        long blockIndex = 0;

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

                if (rightCount == 0)
                    break;

                if (leftCount < rightCount)
                {
                    WriteDataSegment(writer, blockIndex, rightBuffer, rightCount);
                }
                else if (leftCount == rightCount)
                {
                    for (var i = 0; i < leftCount; i++)
                    {
                        var left = leftBuffer[i];
                        var right = rightBuffer[i];

                        if (left == right)
                            continue;

                        WriteDataSegment(writer, blockIndex, rightBuffer, rightCount);

                        break;
                    }
                }
                else
                {
                    WriteDataSegment(writer, blockIndex, rightBuffer, rightCount);
                }

                blockIndex += 1;

                if (blockIndex == long.MaxValue)
                    throw new InvalidOperationException("Block size is too small");
            }

            writer.Write(BinaryPatchConst.SegmentType.End);
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

        writer.Write(BinaryPatchConst.SegmentType.Header);
        writer.Write(BinaryPatchConst.Prefix);
        writer.Write(BinaryPatchConst.ProtocolVersion);
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