using System;
using System.Buffers;
using System.Security.Cryptography;

namespace BitSoft.BinaryTools.Patch;

internal sealed class HashCalculator : IDisposable
{
    private static ArrayPool<byte> Pool { get; } = ArrayPool<byte>.Shared;

    private readonly MD5 _md5 = MD5.Create();
    private readonly byte[] _buffer = Pool.Rent(minimumLength: 32);

    public byte[] CalculatedHash(byte[] source, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(source);

        return _md5.ComputeHash(source, offset: offset, count: count);
    }

    public ReadOnlySpan<byte> CalculatedHash(ReadOnlySpan<byte> source)
    {
        if (_md5.TryComputeHash(source: source, destination: _buffer, out var bytesWritten))
        {
            return _buffer.AsSpan(start: 0, length: bytesWritten);
        }

        throw new InvalidOperationException("Hash calculation failed.");
    }

    public void Dispose()
    {
        _md5.Dispose();
    }
}
