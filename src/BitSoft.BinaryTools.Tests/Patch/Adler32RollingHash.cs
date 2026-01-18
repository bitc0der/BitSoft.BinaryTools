using System;

public class Adler32RollingHash
{
    private const uint ModAdler = 65521;
    private uint _s1 = 1;
    private uint _s2 = 0;
    private readonly int _windowSize;

    public Adler32RollingHash(int windowSize)
    {
        if (windowSize <= 0)
            throw new ArgumentException("Window size must be positive.", nameof(windowSize));
        _windowSize = windowSize;
    }

    public void CalculateInitialHash(ReadOnlySpan<byte> data)
    {
        if (data.Length != _windowSize)
            throw new ArgumentException("Initial data length must match window size.");

        _s1 = 1; _s2 = 0;
        int len = data.Length; int i = 0;
        while (len > 0)
        {
            int k = Math.Min(len, 3800); len -= k;
            while (k-- > 0) { _s1 += data[i++]; _s2 += _s1; }
            _s1 %= ModAdler; _s2 %= ModAdler;
        }
    }

    public void Roll(byte byteOut, byte byteIn)
    {
        var s1_new = (int)_s1 - byteOut + byteIn;
        _s1 = (uint)((s1_new % ModAdler + ModAdler) % ModAdler);

        long diff = (long)_windowSize * byteOut;
        long tempS2 = (long)_s2 - diff + (long)_s1 - 1;
        _s2 = (uint)((tempS2 % ModAdler + ModAdler) % ModAdler);
    }

    public uint Checksum => (_s2 << 16) | _s1;

    public static uint CalculateFullChecksum(ReadOnlySpan<byte> data)
    {
        // Simple, non-optimized full checksum for verification
        uint s1 = 1;
        uint s2 = 0;
        foreach (byte b in data)
        {
            s1 = (s1 + b) % ModAdler;
            s2 = (s2 + s1) % ModAdler;
        }
        return (s2 << 16) | s1;
    }
}
