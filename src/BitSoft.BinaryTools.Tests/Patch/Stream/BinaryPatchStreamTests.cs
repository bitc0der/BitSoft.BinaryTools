using System;
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
        using var originalStream = new MemoryStream(original);
        using var modifiedStream = new MemoryStream(modified);
        using var patchStream = new MemoryStream();

        // Act
        await BinaryPatchWriter.WritePatchAsync(
            original: originalStream,
            modified: modifiedStream,
            output: patchStream,
            settings: new BinaryPatchWriterSettings
            {
                SegmentSize = segmentSize
            });

        // Assert
        originalStream.Position = 0;
        patchStream.Position = 0;

        using var patchedStream = new MemoryStream();

        await BinaryPatchReader.ApplyAsync(
            original: originalStream,
            binaryPatch: patchStream,
            output: patchedStream
        );

        var patched = patchedStream.ToArray();

        Assert.That(patched, Is.EqualTo(modified));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Should_Throw_When_SegmentSizeIsInvalid(int segmentSize)
    {
        using var originalStream = new MemoryStream([0x0]);
        using var modifiedStream = new MemoryStream([0x0]);
        using var patchStream = new MemoryStream();

        // Act
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await BinaryPatchWriter.WritePatchAsync(
            original: originalStream,
            modified: modifiedStream,
            output: patchStream,
            settings: new BinaryPatchWriterSettings
            {
                SegmentSize = segmentSize
            })
        );
    }
}