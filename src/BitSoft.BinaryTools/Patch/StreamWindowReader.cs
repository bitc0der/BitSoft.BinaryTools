using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BitSoft.BinaryTools.Patch;

public class StreamWindowReader : IDisposable
{
    private readonly Stream _stream;
    private readonly ArrayPool<byte> _pool;
    private readonly int _windowSize;
    private readonly int _bufferSize;
    private readonly byte[] _buffer;

    private const int NotDefined = -1;

    private int _position = NotDefined;
    private int _pinnedPosition = NotDefined;
    private int _size = NotDefined;

    public ReadOnlyMemory<byte> Window
    {
        get
        {
            return _position >= 0
                ? _buffer.AsMemory(start: _position, length: _windowSize)
                : throw new InvalidOperationException("The stream does not contain the window.");
        }
    }

    public ReadOnlyMemory<byte> PinnedWindow
    {
        get
        {
            return _pinnedPosition == NotDefined
                ? throw new InvalidOperationException("Pinned position was not set.")
                : _buffer.AsMemory(start: _pinnedPosition, length: _position - _pinnedPosition);
        }
    }

    public bool Pinned => _pinnedPosition != NotDefined;

    public StreamWindowReader(Stream stream, ArrayPool<byte> pool, int windowSize)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        _windowSize = windowSize;
        _bufferSize = windowSize * 2;

        _buffer = _pool.Rent(minimumLength: _bufferSize);
    }

    public async ValueTask<bool> MoveAsync(CancellationToken cancellationToken)
    {
        if (_position == NotDefined)
        {
            var count = await _stream.ReadAsync(_buffer.AsMemory(start: 0, length: _bufferSize), cancellationToken);
            if (count == 0)
                return false;
            _position = 0;
            _size = count;
            return true;
        }

        if (_position == _bufferSize - _windowSize)
        {
            if (_pinnedPosition == NotDefined)
            {
                var length = _size - _position;

                Array.Copy(
                    sourceArray: _buffer,
                    sourceIndex: _position,
                    destinationArray: _buffer,
                    destinationIndex: 0,
                    length: length
                );

                var count = await _stream.ReadAsync(
                    _buffer.AsMemory(start: length, length: length),
                    cancellationToken
                );

                _position = 1;
                _size = length + count;
            }
            else
            {
                throw new InvalidOperationException("Pinned position was not reset.");
            }
        }
        else
        {
            _position += 1;
        }

        return true;
    }

    public void PinPosition()
    {
        _pinnedPosition = _position;
    }

    public void ResetPinnedPosition()
    {
        _pinnedPosition = NotDefined;
    }

    public void Dispose()
    {
        _pool.Return(_buffer);
    }
}
