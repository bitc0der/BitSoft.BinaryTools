using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5 },
            new byte[] { 0x1, 0x1, 0x2, 0x3, 0x4 },
            2
        );
        yield return new TestCaseData(
            new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5 },
            new byte[] { 0x1, 0x1, 0x2, 0x3, 0x4 },
            3
        );
        yield return new TestCaseData(
            new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5 },
            new byte[] { 0x1, 0x2, 0x3, 0x4 },
            3
        );
        yield return new TestCaseData(
            new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5 },
            new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6 },
            3
        );
        yield return new TestCaseData(
            new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5 },
            new byte[] { 0x1, 0x7, 0x3, 0x4, 0x5, 0x6 },
            3
        );
        yield return new TestCaseData(
            new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5 },
            new byte[] { 0x1, 0x7, 0x4, 0x5, 0x6 },
            3
        );
        yield return new TestCaseData(
            new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5 },
            new byte[] { 0x1, 0x7, 0x8, 0x9, 0x2 },
            3
        );
        yield return new TestCaseData(
            new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5 },
            new byte[] { 0x1, 0x2, 0x3, 0x9, 0x9 },
            3
        );
        yield return new TestCaseData(
            new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5 },
            new byte[] { 0x1, 0x2, 0x9, 0x9, 0x9 },
            3
        );
        yield return new TestCaseData(
            new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5 },
            new byte[] { 0x9, 0x9, 0x1, 0x2, 0x3 },
            3
        );
        yield return new TestCaseData(
            new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8 },
            new byte[] { 0x1, 0x2, 0x9, 0x4, 0x5, 0x6, 0x7, 0x8 },
            3
        );
        yield return new TestCaseData(
            new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC },
            new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0xA, 0xA, 0xA, 0xA, 0xB, 0xA, 0xC },
            3
        );
        yield return new TestCaseData(
            new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9 },
            new byte[] { 0x1, 0x2, 0xA, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9 },
            4
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

    [Ignore("Performance test")]
    [TestCase(1024 * 1024, 256, 0, 128)]
    [TestCase(1024 * 1024, 256, 1, 128)]
    [TestCase(1024 * 1024, 256, 5, 128)]
    public async Task Should_CreatePatch(int bufferLength, int blockSize, int changedBlocks, int changeSize)
    {
        // Arrange
        var source = new byte[bufferLength];
        var modified = new byte[bufferLength];

        Random.Shared.NextBytes(source);

        Array.Copy(sourceArray: source, destinationArray: modified, length: source.Length);

        if (changedBlocks > 0)
        {
            var changeBlockSize = source.Length / (changedBlocks + 1);

            for (var b = 1; b <= changedBlocks; b++)
            {
                var position = changeBlockSize * b;

                var span = modified.AsSpan(start: position, length: changeSize);

                Random.Shared.NextBytes(span);
            }
        }

        using var sourceStream = new MemoryStream(source);
        using var modifiedStream = new MemoryStream(modified);
        using var patchStream = new MemoryStream();

        // Act
        var stopwatch = Stopwatch.StartNew();

        await BinaryPatch.CreateAsync(
            source: sourceStream,
            modified: modifiedStream,
            output: patchStream,
            blockSize: blockSize
        );

        stopwatch.Stop();

        // Assert
        Console.WriteLine("Source length: {0}", sourceStream.Length);
        Console.WriteLine("Block size: {0}", blockSize);
        Console.WriteLine("Patch length: {0}", patchStream.Position);
        Console.WriteLine("Create time: {0:g}", stopwatch.Elapsed);

        sourceStream.Position = 0;
        patchStream.Position = 0;

        using var patchedStream = new MemoryStream();

        stopwatch.Restart();
        await BinaryPatch.ApplyAsync(source: sourceStream, patch: patchStream, output: patchedStream);
        stopwatch.Stop();

        Console.WriteLine("Apply time: {0:g}", stopwatch.Elapsed);

        Assert.That(patchedStream.ToArray(), Is.EqualTo(modified));
    }
}
