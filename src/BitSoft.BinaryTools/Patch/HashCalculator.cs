using System;
using System.Buffers;
using System.Security.Cryptography;

namespace BitSoft.BinaryTools.Patch;

internal sealed class HashCalculator : IDisposable
{
    private readonly MD5 _md5 = MD5.Create();
    private readonly ArrayPool<byte> _pool;
    private readonly byte[] _buffer;

    public HashCalculator(ArrayPool<byte> pool)
    {
        _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        _buffer = _pool.Rent(minimumLength: 16);
    }

    public byte[] CalculatedHash(byte[] source, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(source);

        PatchMetrics.AddHashCalculation();

        return _md5.ComputeHash(source, offset: offset, count: count);
    }

    public ReadOnlySpan<byte> CalculatedHash(ReadOnlySpan<byte> source)
    {
        PatchMetrics.AddHashCalculation();

        if (_md5.TryComputeHash(source: source, destination: _buffer, out var bytesWritten))
        {
            return _buffer.AsSpan(start: 0, length: bytesWritten);
        }

        throw new InvalidOperationException("Hash calculation failed.");
    }

    public void Dispose()
    {
        _md5.Dispose();
        _pool.Return(_buffer);
    }
}
