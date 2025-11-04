using System.IO;
using System.Threading.Tasks;
using BitSoft.BinaryTools.Patch.Stream;

namespace BitSoft.BinaryTools.Tests.Patch.Stream;

[TestFixture]
public class BinaryPatchStreamTests
{
    [Test]
    public async Task Should_WriteToStream()
    {
        // Arrange
        var original = new byte[] { 0x0, 0x1, 0x2 };
        var modified = new byte[] { 0x0, 0x1, 0x3 };

        using var originStream = new MemoryStream(original);
        using var modifiedStream = new MemoryStream(modified);
        using var outputStream = new MemoryStream();

        // Act
        await BinaryPatchWriter.WritePatchAsync(originStream, modifiedStream, outputStream, segmentSize: 1);

        // Assert
        originStream.Position = 0;
        outputStream.Position = 0;

        using var patchedStream = new MemoryStream();

        await BinaryPatchReader.ApplyAsync(originStream, outputStream, patchedStream);

        var patched = patchedStream.ToArray();

        Assert.That(patched, Is.EqualTo(modified));
    }
}