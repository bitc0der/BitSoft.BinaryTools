using BitSoft.BinaryTools.Patch;

namespace BitSoft.BinaryTools.Tests.Patch;

[TestFixture]
public class BinaryPatchTests
{
    [Test]
    public void Should_ReturnBinaryPatchSegment_When_ModifiedSameLength()
    {
        // Arrange
        var original = new byte[] { 0x0, 0x1, 0x0, 0x1, 0x0 };
        var modified = new byte[] { 0x0, 0x0, 0x1, 0x0, 0x0 };

        // Act
        var patch = BinaryPatch.Calculate(original, modified, blockSize: 2);

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
    }

    [Test]
    public void Should_ReturnEndOfFilePatchSegment_When_ModifiedShorterThanOriginal()
    {
        // Arrange
        var original = new byte[] { 0x0, 0x1 };
        var modified = new byte[] { 0x0 };

        // Act
        var patch = BinaryPatch.Calculate(original, modified);

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
    public void Should_ReturnBinaryPatchSegment_When_ModifiedLongerThanOriginal()
    {
        // Arrange
        var original = new byte[] { 0x0 };
        var modified = new byte[] { 0x0, 0x1 };

        // Act
        var patch = BinaryPatch.Calculate(original, modified);

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
    public void Should_ReturnBinaryPatchSegment_When_ModifiedLongerAndDifferent()
    {
        // Arrange
        var original = new byte[] { 0x0, 0x0 };
        var modified = new byte[] { 0x0, 0x1, 0x0 };

        // Act
        var patch = BinaryPatch.Calculate(original, modified);

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