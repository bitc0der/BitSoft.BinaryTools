using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BitSoft.BinaryTools.Patch;

internal sealed class PatchWriter : IDisposable
{
    private readonly Stream _output;

    private readonly BinaryWriter _writer;

    public PatchWriter(Stream output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _writer = new BinaryWriter(_output, encoding: ProtocolConst.DefaultEncoding, leaveOpen: true);
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

    public ValueTask WriteCopyAsync(int blockIndex, int blockLength, CancellationToken cancellationToken)
    {
        _writer.Write(ProtocolConst.SegmentTypes.CopyPatchSegment);
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