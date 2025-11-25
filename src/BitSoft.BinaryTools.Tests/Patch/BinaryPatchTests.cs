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
        var source = new byte[] { 0x0, 0x1, 0x0, 0x1, 0x0 };
        var modified = new byte[] { 0x0, 0x0, 0x1, 0x0, 0x0 };

        using var sourceStream = new MemoryStream(source);
        using var modifiedStream = new MemoryStream(modified);
        using var patchStream = new MemoryStream();

        // Act
        await BinaryPatch.CreateAsync(source: sourceStream, modified: modifiedStream, output: patchStream, blockSize: 2);

        // Assert
        sourceStream.Position = 0;
        patchStream.Position = 0;

        using var patchedStream = new MemoryStream();

        await BinaryPatch.ApplyAsync(source: sourceStream, patch: patchStream, output: patchedStream);

        var patched = patchedStream.ToArray();

        Assert.That(patched, Is.Not.Null);
        Assert.That(patched.Length, Is.EqualTo(modified.Length));
        Assert.That(patched, Is.EqualTo(modified));
    }

    [Test]
    public async Task Should_ReturnEndOfFilePatchSegment_When_ModifiedShorterThanOriginal()
    {
        // Arrange
        var source = new byte[] { 0x0, 0x1 };
        var modified = new byte[] { 0x0 };

        using var sourceStream = new MemoryStream(source);
        using var modifiedStream = new MemoryStream(modified);
        using var patchStream = new MemoryStream();

        // Act
        await BinaryPatch.CreateAsync(source: sourceStream, modified: modifiedStream, output: patchStream, blockSize: 2);

        // Assert
        sourceStream.Position = 0;
        patchStream.Position = 0;

        using var patchedStream = new MemoryStream();

        await BinaryPatch.ApplyAsync(source: sourceStream, patch: patchStream, output: patchedStream);

        var patched = patchedStream.ToArray();

        Assert.That(patched, Is.Not.Null);
        Assert.That(patched.Length, Is.EqualTo(modified.Length));
        Assert.That(patched, Is.EqualTo(modified));
    }

    [Test]
    public async Task Should_ReturnBinaryPatchSegment_When_ModifiedLongerThanOriginal()
    {
        // Arrange
        var source = new byte[] { 0x0 };
        var modified = new byte[] { 0x1, 0x2 };

        using var sourceStream = new MemoryStream(source);
        using var modifiedStream = new MemoryStream(modified);
        using var patchStream = new MemoryStream();

        // Act
        await BinaryPatch.CreateAsync(source: sourceStream, modified: modifiedStream, output: patchStream, blockSize: 2);

        // Assert
        sourceStream.Position = 0;
        patchStream.Position = 0;

        using var patchedStream = new MemoryStream();

        await BinaryPatch.ApplyAsync(source: sourceStream, patch: patchStream, output: patchedStream);

        var patched = patchedStream.ToArray();

        Assert.That(patched, Is.Not.Null);
        Assert.That(patched.Length, Is.EqualTo(modified.Length));
        Assert.That(patched, Is.EqualTo(modified));
    }

    [Test]
    public async Task Should_ReturnBinaryPatchSegment_When_ModifiedLongerAndDifferent()
    {
        // Arrange
        var source = new byte[] { 0x1, 0x2 };
        var modified = new byte[] { 0x3, 0x4, 0x5 };

        using var sourceStream = new MemoryStream(source);
        using var modifiedStream = new MemoryStream(modified);
        using var patchStream = new MemoryStream();

        // Act
        await BinaryPatch.CreateAsync(source: sourceStream, modified: modifiedStream, output: patchStream, blockSize: 2);

        // Assert
        sourceStream.Position = 0;
        patchStream.Position = 0;

        using var patchedStream = new MemoryStream();

        await BinaryPatch.ApplyAsync(source: sourceStream, patch: patchStream, output: patchedStream);

        var patched = patchedStream.ToArray();

        Assert.That(patched, Is.Not.Null);
        Assert.That(patched.Length, Is.EqualTo(modified.Length));
        Assert.That(patched, Is.EqualTo(modified));
    }
}