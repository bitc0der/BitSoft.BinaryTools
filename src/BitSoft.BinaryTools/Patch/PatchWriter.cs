using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BitSoft.BinaryTools.Patch;

internal sealed class PatchWriter : IDisposable
{
    private readonly BinaryWriter _writer;

    public PatchWriter(Stream output)
    {
        ArgumentNullException.ThrowIfNull(output);

        _writer = new BinaryWriter(output, encoding: ProtocolConst.DefaultEncoding, leaveOpen: true);
    }

    public ValueTask WriteHeaderAsync(int blockSize, CancellationToken cancellationToken)
    {
        _writer.Write(ProtocolConst.ProtocolVersion);
        _writer.Write(blockSize);

        return ValueTask.CompletedTask;
    }

    public ValueTask WriteDataAsync(ReadOnlyMemory<byte> memory, CancellationToken cancellationToken)
    {
        _writer.Write(ProtocolConst.SegmentTypes.DataPatchSegment);
        _writer.Write(memory.Length);
        _writer.Write(memory.Span);

        return ValueTask.CompletedTask;
    }

    public ValueTask WriteCopyBlockAsync(int blockIndex, CancellationToken cancellationToken)
    {
        _writer.Write(ProtocolConst.SegmentTypes.CopyBlock);
        _writer.Write(blockIndex);

        return ValueTask.CompletedTask;
    }

    public ValueTask WriteCopyBlockWithLengthAsync(int blockIndex, int blockLength, CancellationToken cancellationToken)
    {
        _writer.Write(ProtocolConst.SegmentTypes.CopyBlockWithLength);
        _writer.Write(blockIndex);
        _writer.Write(blockLength);

        return ValueTask.CompletedTask;
    }

    public ValueTask CompleteAsync(CancellationToken cancellationToken)
    {
        _writer.Write(ProtocolConst.SegmentTypes.EndPatchSegment);
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        _writer.Dispose();
    }
}
