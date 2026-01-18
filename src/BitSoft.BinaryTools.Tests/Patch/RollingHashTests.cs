using BitSoft.BinaryTools.Patch;

namespace BitSoft.BinaryTools.Tests.Patch;

using System;

[TestFixture]
public sealed class RollingHashTests
{
    [TestCase(1024, 32)]
    [TestCase(1024, 64)]
    [TestCase(1024, 128)]
    [TestCase(1024, 256)]
    [TestCase(1024, 512)]
    [TestCase(1024 * 1024, 512)]
    [TestCase(1024 * 1024, 1024)]
    public void Should_CalculateHash(int bufferLength, int bufferSize)
    {
        // Arrange
        var buffer = new byte[bufferLength];

        Random.Shared.NextBytes(buffer);

        // Act & Assert
        var initialSpan = buffer.AsSpan(start: 0, length: bufferSize);
        var rollingHash = RollingHash.Create(initialSpan);

        for (var i = 0; i < bufferLength - bufferSize; i++)
        {
            var span = buffer.AsSpan(start: i, length: bufferSize);

            var spanHash = RollingHash.Create(span);

            Assert.That(
                actual: rollingHash.GetChecksum(),
                expression: Is.EqualTo(spanHash.GetChecksum()),
                message: $"Failed as position '{i}'"
            );

            if (i < bufferLength - bufferSize - 1)
            {
                var oldByte = buffer[i];
                var newByte = buffer[i + bufferSize];

                rollingHash.Update(removed: oldByte, added: newByte);
            }
        }
    }
}
