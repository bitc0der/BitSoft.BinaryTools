using System.Linq;
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
        var patch = BinaryPatch.Calculate(original, modified);

        // Assert
        Assert.That(patch, Is.Not.Null);
        Assert.That(patch.Segments, Is.Not.Empty);
        Assert.That(patch.Segments.Count, Is.EqualTo(1));

        var segment = patch.Segments.First();

        Assert.That(segment, Is.Not.Null);

        var binaryPatchSegment = segment as BinaryPatchSegment;

        Assert.That(binaryPatchSegment, Is.Not.Null);
        Assert.That(binaryPatchSegment.Offset, Is.EqualTo(1));
        Assert.That(binaryPatchSegment.Length, Is.EqualTo(3));
        Assert.That(binaryPatchSegment.Memory.Length, Is.EqualTo(3));
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

        var segment = patch.Segments.First();

        Assert.That(segment, Is.Not.Null);
        var binaryPatchSegment = segment as EndOfFilePatchSegment;
        Assert.That(binaryPatchSegment, Is.Not.Null);
        Assert.That(binaryPatchSegment.Offset, Is.EqualTo(1));
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

        var segment = patch.Segments.First();

        Assert.That(segment, Is.Not.Null);

        var binaryPatchSegment = segment as BinaryPatchSegment;

        Assert.That(binaryPatchSegment, Is.Not.Null);
        Assert.That(binaryPatchSegment.Offset, Is.EqualTo(1));
        Assert.That(binaryPatchSegment.Length, Is.EqualTo(1));
        Assert.That(binaryPatchSegment.Memory.Length, Is.EqualTo(1));
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

        var firstSegment = patch.Segments.First();

        Assert.That(firstSegment, Is.Not.Null);

        var binaryPatchSegment = firstSegment as BinaryPatchSegment;

        Assert.That(binaryPatchSegment, Is.Not.Null);
        Assert.That(binaryPatchSegment.Offset, Is.EqualTo(1));
        Assert.That(binaryPatchSegment.Length, Is.EqualTo(2));
        Assert.That(binaryPatchSegment.Memory.Length, Is.EqualTo(2));
    }
}