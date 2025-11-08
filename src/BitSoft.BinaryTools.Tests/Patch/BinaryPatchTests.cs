using System.IO;
using System.Threading.Tasks;
using BitSoft.BinaryTools.Patch;

namespace BitSoft.BinaryTools.Tests.Patch;

[TestFixture]
public class BinaryPatchTests
{
    [Test]
    public void Should_DeserializePatch_FromStream()
    {
        // Arrange
        var data = new byte[] { 0x0, 0x1, 0x0, 0x1, 0x0 };
        var sourcePatch = new BinaryPatch(segments:
        [
            new CopyPatchSegment(blockIndex: 5, length: 34),
            new DataPatchSegment(memory: data)
        ], blockSize: 1024);

        // Act
        using var patchStream = new MemoryStream();
        sourcePatch.Write(patchStream);
        patchStream.Position = 0;
        var restoredPatch = BinaryPatch.Read(patchStream);

        // Assert
        Assert.That(restoredPatch, Is.Not.Null);
        Assert.That(restoredPatch.BlockSize, Is.EqualTo(sourcePatch.BlockSize));
        Assert.That(restoredPatch.Segments, Is.Not.Empty);
        Assert.That(restoredPatch.Segments.Count, Is.EqualTo(sourcePatch.Segments.Count));
    }

    [Test]
    public async Task Should_ReturnBinaryPatchSegment_When_ModifiedSameLength()
    {
        // Arrange
        var original = new byte[] { 0x0, 0x1, 0x0, 0x1, 0x0 };
        var modified = new byte[] { 0x0, 0x0, 0x1, 0x0, 0x0 };

        // Act
        using var originalStream = new MemoryStream(original);
        var patchSource = await BinaryPatchSource.CreateAsync(originalStream, blockSize: 2);
        var patch = patchSource.Calculate(modified);

        // Assert
        Assert.That(patch, Is.Not.Null);
        Assert.That(patch.Segments, Is.Not.Empty);
        Assert.That(patch.Segments.Count, Is.EqualTo(3));

        var segment = patch.Segments[0];

        Assert.That(segment, Is.Not.Null);
        var binaryPatchSegment = segment as DataPatchSegment;
        Assert.That(binaryPatchSegment, Is.Not.Null);
        Assert.That(binaryPatchSegment.Memory.Length, Is.EqualTo(1));

        segment = patch.Segments[1];

        Assert.That(segment, Is.Not.Null);
        var copyPatchSegment = segment as CopyPatchSegment;
        Assert.That(copyPatchSegment, Is.Not.Null);
        Assert.That(copyPatchSegment.BlockIndex, Is.EqualTo(0));
        Assert.That(copyPatchSegment.Length, Is.EqualTo(2));

        segment = patch.Segments[2];

        Assert.That(segment, Is.Not.Null);
        binaryPatchSegment = segment as DataPatchSegment;
        Assert.That(binaryPatchSegment, Is.Not.Null);
        Assert.That(binaryPatchSegment.Memory.Length, Is.EqualTo(2));

        using var patchedStream = new MemoryStream();

        BinaryPatchSource.Apply(original, patch, patchedStream);

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

        // Act
        using var originalStream = new MemoryStream(original);
        var patchSource = await BinaryPatchSource.CreateAsync(originalStream, blockSize: 2);
        var patch = patchSource.Calculate(modified);

        // Assert
        Assert.That(patch, Is.Not.Null);
        Assert.That(patch.Segments, Is.Not.Empty);
        Assert.That(patch.Segments.Count, Is.EqualTo(1));

        var segment = patch.Segments[0];

        Assert.That(segment, Is.Not.Null);
        var binaryPatchSegment = segment as DataPatchSegment;
        Assert.That(binaryPatchSegment, Is.Not.Null);
        Assert.That(binaryPatchSegment.Memory.Length, Is.EqualTo(1));
    }

    [Test]
    public async Task Should_ReturnBinaryPatchSegment_When_ModifiedLongerThanOriginal()
    {
        // Arrange
        var original = new byte[] { 0x0 };
        var modified = new byte[] { 0x0, 0x1 };

        // Act
        using var originalStream = new MemoryStream(original);
        var patchSource = await BinaryPatchSource.CreateAsync(originalStream, blockSize: 2);
        var patch = patchSource.Calculate(modified);

        // Assert
        Assert.That(patch, Is.Not.Null);
        Assert.That(patch.Segments, Is.Not.Empty);
        Assert.That(patch.Segments.Count, Is.EqualTo(1));

        var firstSegment = patch.Segments[0];

        Assert.That(firstSegment, Is.Not.Null);

        var binaryPatchSegment = firstSegment as DataPatchSegment;

        Assert.That(binaryPatchSegment, Is.Not.Null);
        Assert.That(binaryPatchSegment.Memory.Length, Is.EqualTo(2));
    }

    [Test]
    public async Task Should_ReturnBinaryPatchSegment_When_ModifiedLongerAndDifferent()
    {
        // Arrange
        var original = new byte[] { 0x0, 0x0 };
        var modified = new byte[] { 0x0, 0x1, 0x0 };

        // Act
        using var originalStream = new MemoryStream(original);
        var patchSource = await BinaryPatchSource.CreateAsync(originalStream);
        var patch = patchSource.Calculate(modified);

        // Assert
        Assert.That(patch, Is.Not.Null);
        Assert.That(patch.Segments, Is.Not.Empty);
        Assert.That(patch.Segments.Count, Is.EqualTo(1));

        var firstSegment = patch.Segments[0];

        Assert.That(firstSegment, Is.Not.Null);

        var binaryPatchSegment = firstSegment as DataPatchSegment;

        Assert.That(binaryPatchSegment, Is.Not.Null);
        Assert.That(binaryPatchSegment.Memory.Length, Is.EqualTo(3));
    }
}