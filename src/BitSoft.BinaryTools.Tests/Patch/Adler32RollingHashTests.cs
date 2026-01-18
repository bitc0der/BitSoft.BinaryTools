using System;

public class Adler32RollingHashTests_Scalability
{
    [Test]
    public void TestRollingHashWithMegaByteDataBuffer()
    {
        int Megabyte = 1024 * 1024;
        int bufferSize = 1 * Megabyte;
        int windowSize = 4096;

        var data = new byte[bufferSize];
        Random.Shared.NextBytes(data);

        Adler32RollingHash rollingHash = new (windowSize);
        rollingHash.CalculateInitialHash(data.AsSpan(start: 0, length: windowSize));

        for (int i = 0; i < data.Length - windowSize; i++)
        {
            byte byteOut = data[i];
            byte byteIn = data[i + windowSize];

            rollingHash.Roll(byteOut, byteIn);

            var span = data.AsSpan(start: i + 1, length:  windowSize);

            var expectedChecksum = Adler32RollingHash.CalculateFullChecksum(span);
            Assert.That(expectedChecksum, Is.EqualTo(rollingHash.Checksum));
        }
    }
}
