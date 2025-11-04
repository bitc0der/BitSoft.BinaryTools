using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BitSoft.BinaryTools.Patch.Stream;

namespace BitSoft.BinaryTools.Tests.Patch.Stream;

[TestFixture]
public class BinaryPatchStreamTests
{
    private static IEnumerable<TestCaseData> GetTestCaseData()
    {
        yield return new TestCaseData(new byte[] { 0x0, 0x1, 0x2 }, new byte[] { 0x0, 0x1, 0x3 }, 1);
        yield return new TestCaseData(new byte[] { 0x0, 0x1, 0x2 }, new byte[] { 0x0, 0x3 }, 1);
        yield return new TestCaseData(new byte[] { 0x0, 0x1 }, new byte[] { 0x0, 0x1, 0x3 }, 1);
        yield return new TestCaseData(new byte[] { 0x0, 0x1, 0x2 }, new byte[] { 0x0, 0x1, 0x3 }, 2);
        yield return new TestCaseData(new byte[] { 0x0, 0x1, 0x2 }, new byte[] { 0x0, 0x1, 0x3, 0x4 }, 2);
        yield return new TestCaseData(new byte[] { 0x0, 0x1 }, new byte[] { 0x0, 0x1, 0x3 }, 2);
        yield return new TestCaseData(new byte[] { 0x0, 0x1, 0x2, 0x3 }, new byte[] { 0x0, 0x1, 0x3 }, 2);
    }

    [TestCaseSource(nameof(GetTestCaseData))]
    public async Task Should_WriteToStream(byte[] original, byte[] modified, int segmentSize)
    {
        // Arrange
        using var originStream = new MemoryStream(original);
        using var modifiedStream = new MemoryStream(modified);
        using var patchStream = new MemoryStream();

        // Act
        await BinaryPatchWriter.WritePatchAsync(originStream, modifiedStream, patchStream, segmentSize: segmentSize);

        // Assert
        originStream.Position = 0;
        patchStream.Position = 0;

        using var patchedStream = new MemoryStream();

        await BinaryPatchReader.ApplyAsync(originStream, patchStream, patchedStream);

        var patched = patchedStream.ToArray();

        Assert.That(patched, Is.EqualTo(modified));
    }
}