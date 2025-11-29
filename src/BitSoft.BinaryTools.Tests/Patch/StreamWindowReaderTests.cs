using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BitSoft.BinaryTools.Patch;

namespace BitSoft.BinaryTools.Tests.Patch;

[TestFixture]
public class StreamWindowReaderTests
{
    [Test]
    public async Task Should_ReturnInitialWindow()
    {
        // Arrange
        var source = new byte[] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6 };
        using var sourceStream = new MemoryStream(source);

        // Act
        using var reader = new StreamWindowReader(sourceStream, ArrayPool<byte>.Shared, windowSize: 2);

        await reader.MoveAsync(CancellationToken.None);

        // Assert
        Assert.That(reader.Window.Length, Is.EqualTo(2));
        Assert.That(reader.Window.ToArray(), Is.EqualTo(source.AsMemory(start: 0, length: 2).ToArray()).AsCollection);
    }

    [Test]
    public async Task Should_ReturnMovedWindow()
    {
        // Arrange
        var source = new byte[] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6 };
        using var sourceStream = new MemoryStream(source);

        // Act
        using var reader = new StreamWindowReader(sourceStream, ArrayPool<byte>.Shared, windowSize: 2);

        await reader.MoveAsync(CancellationToken.None); // 0
        await reader.MoveAsync(CancellationToken.None); // 1

        // Assert
        Assert.That(reader.Window.Length, Is.EqualTo(2));
        Assert.That(reader.Window.ToArray(), Is.EqualTo(source.AsMemory(start: 1, length: 2).ToArray()).AsCollection);
    }

    [Test]
    public async Task Should_ReturnMovedWindow_When_Overlapped()
    {
        // Arrange
        var source = new byte[] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6 };
        using var sourceStream = new MemoryStream(source);

        // Act
        using var reader = new StreamWindowReader(sourceStream, ArrayPool<byte>.Shared, windowSize: 2);

        await reader.MoveAsync(CancellationToken.None); // 0
        await reader.MoveAsync(CancellationToken.None); // 1
        await reader.MoveAsync(CancellationToken.None); // 2
        await reader.MoveAsync(CancellationToken.None); // 3

        // Assert
        Assert.That(reader.Window.Length, Is.EqualTo(2));
        Assert.That(reader.Window.ToArray(), Is.EqualTo(source.AsMemory(start: 3, length: 2).ToArray()).AsCollection);
    }

    [Test]
    public async Task Should_ReturnMovedWindow_When_Pinned()
    {
        // Arrange
        var source = new byte[] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6 };
        using var sourceStream = new MemoryStream(source);

        // Act
        using var reader = new StreamWindowReader(sourceStream, ArrayPool<byte>.Shared, windowSize: 2);

        await reader.MoveAsync(CancellationToken.None); // 0
        reader.PinPosition();
        await reader.MoveAsync(CancellationToken.None); // 1

        // Assert
        Assert.That(reader.Window.Length, Is.EqualTo(2));
        Assert.That(reader.PinnedWindow.ToArray(),
            Is.EqualTo(source.AsMemory(start: 0, length: 1).ToArray()).AsCollection);
    }

    [Test]
    public async Task Should_ReturnMovedWindow_When_FinalBlockIsShort()
    {
        // Arrange
        var source = new byte[] { 0x0, 0x1, 0x2, 0x3, 0x4 };
        using var sourceStream = new MemoryStream(source);

        // Act
        using var reader = new StreamWindowReader(sourceStream, ArrayPool<byte>.Shared, windowSize: 2);

        await reader.MoveAsync(CancellationToken.None); // 0
        await reader.MoveAsync(CancellationToken.None); // 1
        await reader.MoveAsync(CancellationToken.None); // 2
        await reader.MoveAsync(CancellationToken.None); // 3
        await reader.MoveAsync(CancellationToken.None); // 4

        // Assert
        Assert.That(reader.Window.Length, Is.EqualTo(1));
        Assert.That(reader.Window.ToArray(), Is.EqualTo(source.AsMemory(start: 4, length: 1).ToArray()).AsCollection);
    }

    [Test]
    public async Task Should_ReturnMovedWindow_When_ReachEnd()
    {
        // Arrange
        var source = new byte[] { 0x0, 0x1, 0x2 };
        using var sourceStream = new MemoryStream(source);

        // Act
        using var reader = new StreamWindowReader(sourceStream, ArrayPool<byte>.Shared, windowSize: 2);

        await reader.MoveAsync(CancellationToken.None); // 0
        await reader.MoveAsync(CancellationToken.None); // 1
        await reader.MoveAsync(CancellationToken.None); // 2
        var result = await reader.MoveAsync(CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
    }
}
