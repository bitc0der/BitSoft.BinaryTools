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
    private bool _continureRead = true;

    public ReadOnlyMemory<byte> Window
    {
        get
        {
            return _position >= 0
                ? _buffer.AsMemory(start: _position, length: Math.Min(_windowSize, _size - _position))
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

    public ReadOnlyMemory<byte> PinnedWindowWithCurrent
    {
        get
        {
            return _pinnedPosition == NotDefined
                ? throw new InvalidOperationException("Pinned position was not set.")
                : _buffer.AsMemory(start: _pinnedPosition, length: _position - _pinnedPosition + 1);
        }
    }

    public bool Pinned => _pinnedPosition != NotDefined;

    public bool Finished => _position == _size - 1;

    public StreamWindowReader(Stream stream, ArrayPool<byte> pool, int windowSize)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        _windowSize = windowSize;
        _bufferSize = windowSize * 2;

        _buffer = _pool.Rent(minimumLength: _bufferSize);
    }

    public async ValueTask<bool> SlideWindowAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < _windowSize; i++)
        {
            if (await MoveAsync(cancellationToken))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    public async ValueTask<bool> MoveAsync(CancellationToken cancellationToken)
    {
        if (_position != NotDefined && _position == _size)
            return false;

        if (_position == NotDefined)
        {
            var count = await _stream.ReadAsync(_buffer.AsMemory(start: 0, length: _bufferSize), cancellationToken);
            if (count == 0)
                return false;
            _position = 0;
            _size = count;
            return true;
        }

        _position += 1;

        if (_continureRead && _position == _bufferSize - _windowSize)
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

                _position = 0;
                _size = length + count;

                if (count < _windowSize)
                {
                    _continureRead = false;
                }
            }
            else
            {
                throw new InvalidOperationException($"Pinned position '{_pinnedPosition}' for buffer '{_bufferSize}' with window '{_windowSize}' was not reset.");
            }
        }

        if (_position == _size)
            return false;

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
