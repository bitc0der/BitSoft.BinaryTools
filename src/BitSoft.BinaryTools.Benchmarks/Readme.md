# Benchmarks

```
| Method            | BufferLength | ChangedBlocks | ChangeSize | BlockSize | Mean     | Error    | StdDev   | Allocated |
|------------------ |------------- |-------------- |----------- |---------- |---------:|---------:|---------:|----------:|
| CreateBinaryPatch | 1048576      | 5             | 512        | 1024      | 31.39 ms | 13.92 ms | 0.763 ms |   2.21 MB |
| CreateBinaryPatch | 1048576      | 5             | 512        | 4096      | 34.66 ms | 13.49 ms | 0.739 ms |   2.15 MB |
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
