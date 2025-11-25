using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BitSoft.BinaryTools.Patch;

internal interface IPatchSegment
{
}

internal sealed class DataPatchSegment : IPatchSegment
{
    public DataPatchSegment(ReadOnlyMemory<byte> data)
    {
        Data = data;
    }

    public ReadOnlyMemory<byte> Data { get; }
}

internal sealed class CopyPatchSegment : IPatchSegment
{
    public int BlockIndex { get; }

    public int BlockLength { get; }

    public CopyPatchSegment(int blockIndex, int blockLength)
    {
        BlockIndex = blockIndex;
        BlockLength = blockLength;
    }
}

internal sealed class PatchReader : IDisposable
{
    private readonly Stream _source;
    private readonly BinaryReader _reader;

    private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;
    private byte[]? _buffer;

    public IPatchSegment? Segment { get; private set; } = null;

    public PatchReader(Stream source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _reader = new BinaryReader(source, encoding: ProtocolConst.DefaultEncoding, leaveOpen: true);
    }

    public ValueTask<int> InitializeAsync(CancellationToken cancellationToken)
    {
        var protocolVersion = _reader.ReadInt32();

        if (protocolVersion > ProtocolConst.ProtocolVersion)
            throw new InvalidOperationException($"Invalid protocol version '{protocolVersion}'.");

        var blockSize = _reader.ReadInt32();

        _buffer = Pool.Rent(blockSize);

        return ValueTask.FromResult(blockSize);
    }

    public ValueTask<bool> ReadAsync(CancellationToken cancellationToken)
    {
        var segmentType = _reader.ReadByte();

        switch (segmentType)
        {
            case ProtocolConst.SegmentTypes.EndPatchSegment:
                Segment = null;
                return ValueTask.FromResult(false);
            case ProtocolConst.SegmentTypes.CopyPatchSegment:
                var blockIndex = _reader.ReadInt32();
                var blockLength = _reader.ReadInt32();
                Segment = new CopyPatchSegment(blockIndex: blockIndex, blockLength: blockLength);
                break;
            case ProtocolConst.SegmentTypes.DataPatchSegment:
                var length = _reader.ReadInt32();
                var span = _buffer.AsSpan(start: 0, length: length);
                var count = _reader.Read(span);
                Segment = new DataPatchSegment(data: _buffer.AsMemory(start: 0, length: count));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(segmentType), segmentType, null);
        }

        return ValueTask.FromResult(true);
    }

    public void Dispose()
    {
        _reader.Dispose();

        if (_buffer is not null)
            Pool.Return(_buffer);
    }
}