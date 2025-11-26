using System;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BitSoft.BinaryTools.Benchmarks.Utils;
using BitSoft.BinaryTools.Patch;

namespace BitSoft.BinaryTools.Benchmarks;

[ShortRunJob]
[MemoryDiagnoser]
public class BinaryPatchBenchmark
{
    private byte[]? _source;
    private byte[]? _modified;

    private Stream? _sourceStream;
    private Stream? _modifiedStream;
    private Stream? _patchStream;

    [Params(1024 * 1024, 10 * 1024 * 1024, 100 * 1024 * 1024)]
    public int BufferLength { get; set; }

    [Params(3, 5)] public int ChangedBlocks { get; set; }

    [Params(128, 512)] public int ChangeSize { get; set; }

    [Params(512, 1024, 4096)] public int BlockSize { get; set; }

    [GlobalSetup]
    public void GlobalSetUp()
    {
        _source = new byte[BufferLength];
        _modified = new byte[BufferLength];

        Create.RandomData(_source);

        Array.Copy(sourceArray: _source, destinationArray: _modified, length: _source.Length);

        var changeBlockSize = _source.Length / (ChangedBlocks + 1);

        for (var b = 1; b <= ChangedBlocks; b++)
        {
            var position = changeBlockSize * b;

            var span = _modified.AsSpan(start: position, length: ChangeSize);

            Create.RandomData(span);
        }

        _sourceStream = new MemoryStream(_source);
        _modifiedStream = new MemoryStream(_modified);
    }

    [IterationSetup]
    public void SetUp()
    {
        _patchStream = new MemoryStream();
    }

    [IterationCleanup]
    public void Cleanup()
    {
        _patchStream?.Dispose();
    }

    [GlobalCleanup]
    public void GlobalCleanUp()
    {
        _sourceStream?.Dispose();
        _modifiedStream?.Dispose();
    }

    [Benchmark]
    public async Task CreateBinaryPatch()
    {
        await BinaryPatch.CreateAsync(
            source: _sourceStream!,
            modified: _modifiedStream!,
            output: _patchStream!,
            blockSize: BlockSize
        );
    }
}
