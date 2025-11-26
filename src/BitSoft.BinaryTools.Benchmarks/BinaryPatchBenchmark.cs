using System;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
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

    [GlobalSetup]
    public void GlobalSetUp()
    {
        _source = new byte[1024];
        _modified = new byte[1024];

        Array.Copy(sourceArray: _source, destinationArray: _modified, length: _source.Length);

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
        await BinaryPatch.CreateAsync(source: _sourceStream!, modified: _modifiedStream!, output: _patchStream!);
    }
}
