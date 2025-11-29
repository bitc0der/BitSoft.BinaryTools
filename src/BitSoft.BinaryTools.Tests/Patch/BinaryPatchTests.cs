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
    [TestCase(4 * 4, 4)]
    [TestCase(10 * 1024 * 1024, 1024)]
    [TestCase(10 * 1024 * 1024, 4 * 1024)]
    public async Task Should_CreatePatch(int bufferLength, int blockSize)
    {
        // Arrange
        var source = new byte[bufferLength];
        var modified = new byte[bufferLength];

        Random.Shared.NextBytes(source);

        Array.Copy(sourceArray: source, destinationArray: modified, length: source.Length);

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
        Console.WriteLine("Time: {0:g}", stopwatch.Elapsed);
    }

    [Test]
    public void ArrayTest()
    {
        // Arrange
        var sourceArray = new[] { 1, 2, 3, 4 };

        // Act
        Array.Copy(sourceArray: sourceArray, sourceIndex: 2, destinationArray: sourceArray, destinationIndex: 0, length: 2);
    }
}
