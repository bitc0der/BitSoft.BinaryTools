using System.IO;
using System.Threading.Tasks;
using BitSoft.BinaryTools.Patch;

namespace BitSoft.BinaryTools.Tests.Patch;

[TestFixture]
public class BinaryPatchTests
{
    [Test]
    public async Task Should_ReturnBinaryPatchSegment_When_ModifiedSameLength()
    {
        // Arrange
        var original = new byte[] { 0x0, 0x1, 0x0, 0x1, 0x0 };
        var modified = new byte[] { 0x0, 0x0, 0x1, 0x0, 0x0 };

        using var originalStream = new MemoryStream(original);
        using var modifiedStream = new MemoryStream(modified);
        using var patchStream = new MemoryStream();

        // Act
        var patchSource = await BinaryPatchSource.CreateAsync(originalStream, blockSize: 2);
        await patchSource.CreateAsync(modifiedStream, output: patchStream);

        // Assert
        originalStream.Position = 0;
        patchStream.Position = 0;

        using var patchedStream = new MemoryStream();

        await BinaryPatchSource.ApplyAsync(source: originalStream, patch: patchStream, output: patchedStream);

        var patched = patchedStream.ToArray();

        Assert.That(patched, Is.Not.Null);
        Assert.That(patched.Length, Is.EqualTo(modified.Length));
        Assert.That(patched, Is.EqualTo(modified));
    }

    [Test]
    public async Task Should_ReturnEndOfFilePatchSegment_When_ModifiedShorterThanOriginal()
    {
        // Arrange
        var original = new byte[] { 0x0, 0x1 };
        var modified = new byte[] { 0x0 };

        using var originalStream = new MemoryStream(original);
        using var modifiedStream = new MemoryStream(modified);
        using var patchStream = new MemoryStream();

        // Act
        var patchSource = await BinaryPatchSource.CreateAsync(originalStream, blockSize: 2);
        await patchSource.CreateAsync(modifiedStream, output: patchStream);

        // Assert
        originalStream.Position = 0;
        patchStream.Position = 0;

        using var patchedStream = new MemoryStream();

        await BinaryPatchSource.ApplyAsync(source: originalStream, patch: patchStream, output: patchedStream);

        var patched = patchedStream.ToArray();

        Assert.That(patched, Is.Not.Null);
        Assert.That(patched.Length, Is.EqualTo(modified.Length));
        Assert.That(patched, Is.EqualTo(modified));
    }

    [Test]
    public async Task Should_ReturnBinaryPatchSegment_When_ModifiedLongerThanOriginal()
    {
        // Arrange
        var original = new byte[] { 0x0 };
        var modified = new byte[] { 0x1, 0x2 };

        using var originalStream = new MemoryStream(original);
        using var modifiedStream = new MemoryStream(modified);
        using var patchStream = new MemoryStream();

        // Act
        var patchSource = await BinaryPatchSource.CreateAsync(originalStream, blockSize: 2);
        await patchSource.CreateAsync(modifiedStream, output: patchStream);

        // Assert
        originalStream.Position = 0;
        patchStream.Position = 0;

        using var patchedStream = new MemoryStream();

        await BinaryPatchSource.ApplyAsync(source: originalStream, patch: patchStream, output: patchedStream);

        var patched = patchedStream.ToArray();

        Assert.That(patched, Is.Not.Null);
        Assert.That(patched.Length, Is.EqualTo(modified.Length));
        Assert.That(patched, Is.EqualTo(modified));
    }

    [Test]
    public async Task Should_ReturnBinaryPatchSegment_When_ModifiedLongerAndDifferent()
    {
        // Arrange
        var original = new byte[] { 0x1, 0x2 };
        var modified = new byte[] { 0x3, 0x4, 0x5 };

        using var originalStream = new MemoryStream(original);
        using var modifiedStream = new MemoryStream(modified);
        using var patchStream = new MemoryStream();

        // Act
        var patchSource = await BinaryPatchSource.CreateAsync(originalStream, blockSize: 2);
        await patchSource.CreateAsync(modifiedStream, output: patchStream);

        // Assert
        originalStream.Position = 0;
        patchStream.Position = 0;

        using var patchedStream = new MemoryStream();

        await BinaryPatchSource.ApplyAsync(source: originalStream, patch: patchStream, output: patchedStream);

        var patched = patchedStream.ToArray();

        Assert.That(patched, Is.Not.Null);
        Assert.That(patched.Length, Is.EqualTo(modified.Length));
        Assert.That(patched, Is.EqualTo(modified));
    }
}