# Benchmarks

```
| Method            | BufferLength | ChangedBlocks | ChangeSize | BlockSize | Mean     | Error    | StdDev   | Allocated |
|------------------ |------------- |-------------- |----------- |---------- |---------:|---------:|---------:|----------:|
| CreateBinaryPatch | 1048576      | 5             | 512        | 1024      |  1.692 s | 0.0159 s | 0.0009 s |   2.29 MB |
| CreateBinaryPatch | 1048576      | 5             | 512        | 4096      |  5.764 s | 0.1578 s | 0.0087 s |   2.18 MB |
| CreateBinaryPatch | 10485760     | 5             | 512        | 1024      | 16.832 s | 0.6871 s | 0.0377 s |  34.85 MB |
| CreateBinaryPatch | 10485760     | 5             | 512        | 4096      | 57.220 s | 0.9950 s | 0.0545 s |  32.69 MB |
```
## Legends
```
  BufferLength  : Value of the 'BufferLength' parameter
  ChangedBlocks : Value of the 'ChangedBlocks' parameter
  ChangeSize    : Value of the 'ChangeSize' parameter
  BlockSize     : Value of the 'BlockSize' parameter
  Mean          : Arithmetic mean of all measurements
  Error         : Half of 99.9% confidence interval
  StdDev        : Standard deviation of all measurements
  Allocated     : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
  1 us          : 1 Microsecond (0.000001 sec)
```

## Additional info
```
BenchmarkDotNet v0.15.6, macOS 26.1 (25B78) [Darwin 25.1.0]
Apple M1 Pro, 1 CPU, 10 logical and 10 physical cores
.NET SDK 10.0.100
  [Host]   : .NET 8.0.22 (8.0.22, 8.0.2225.52707), Arm64 RyuJIT armv8.0-a
  ShortRun : .NET 8.0.22 (8.0.22, 8.0.2225.52707), Arm64 RyuJIT armv8.0-a
```
