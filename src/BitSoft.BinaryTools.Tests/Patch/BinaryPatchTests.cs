using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BitSoft.BinaryTools.Patch;

namespace BitSoft.BinaryTools.Tests.Patch;

[TestFixture]
public class BinaryPatchTests
{
    private static IEnumerable<TestCaseData> TestCases()
    {
        yield return new TestCaseData(
            new byte[] { 0x0, 0x1, 0x0, 0x1, 0x0 },
            new byte[] { 0x0, 0x0, 0x1, 0x0, 0x0 },
            2
        );
        yield return new TestCaseData(
            new byte[] { 0x0, 0x1 },
            new byte[] { 0x0 },
            2
        );
        yield return new TestCaseData(
            new byte[] { 0x0 },
            new byte[] { 0x1, 0x2 },
            2
        );
        yield return new TestCaseData(
            new byte[] { 0x1, 0x2 },
            new byte[] { 0x3, 0x4, 0x5 },
            2
        );
    }

    [TestCaseSource(nameof(TestCases))]
    public async Task Should_CreatPatch(byte[] source, byte[] modified, int blockSize)
    {
        using var sourceStream = new MemoryStream(source);
        using var modifiedStream = new MemoryStream(modified);
        using var patchStream = new MemoryStream();

        // Act
        await BinaryPatch.CreateAsync(
            source: sourceStream,
            modified: modifiedStream,
            output: patchStream,
            blockSize: blockSize);

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