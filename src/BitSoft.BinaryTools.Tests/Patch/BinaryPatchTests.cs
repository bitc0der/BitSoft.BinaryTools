using System.Linq;
using BitSoft.BinaryTools.Patch;

namespace BitSoft.BinaryTools.Tests.Patch;

[TestFixture]
public class BinaryPatchTests
{
    [Test]
    public void Should_CreateBinaryPatch()
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
        
        var binatyPatchSegment = segment as BinaryPatchSegment;
        
        Assert.That(binatyPatchSegment, Is.Not.Null);
        Assert.That(binatyPatchSegment.Offset, Is.EqualTo(1));
        Assert.That(binatyPatchSegment.Length, Is.EqualTo(3));
        Assert.That(binatyPatchSegment.Memory.Length, Is.EqualTo(3));
    }
}