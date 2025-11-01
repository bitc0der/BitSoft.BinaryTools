using System.Linq;
using BitSoft.BinaryTools.Patch;

namespace BitSoft.BinaryTools.Tests.Patch;

[TestFixture]
public class BinaryPatchTests
{
    [Test]
    public void Should_CreateBinaryPatch()
    {
        // Arrance
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
        Assert.That(segment.Offset, Is.EqualTo(1));
        Assert.That(segment.Length, Is.EqualTo(3));
    }
}