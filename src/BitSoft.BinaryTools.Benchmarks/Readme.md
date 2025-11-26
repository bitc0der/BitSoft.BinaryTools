# Benchmarks

```
| Method            | BufferLength | ChangedBlocks | ChangeSize | BlockSize | Mean     | Error      | StdDev   | Allocated |
|------------------ |------------- |-------------- |----------- |---------- |---------:|-----------:|---------:|----------:|
| CreateBinaryPatch | 1048576      | 3             | 128        | 512       | 16.80 us |   7.952 us | 0.436 us |     448 B |
| CreateBinaryPatch | 1048576      | 3             | 128        | 1024      | 20.50 us |  55.666 us | 3.051 us |     448 B |
| CreateBinaryPatch | 1048576      | 3             | 128        | 4096      | 15.00 us |  47.853 us | 2.623 us |     448 B |
| CreateBinaryPatch | 1048576      | 3             | 512        | 512       | 16.68 us |  61.173 us | 3.353 us |     448 B |
| CreateBinaryPatch | 1048576      | 3             | 512        | 1024      | 18.53 us |  65.508 us | 3.591 us |     448 B |
| CreateBinaryPatch | 1048576      | 3             | 512        | 4096      | 20.43 us |  37.933 us | 2.079 us |     448 B |
| CreateBinaryPatch | 1048576      | 5             | 128        | 512       | 20.77 us |  59.992 us | 3.288 us |     448 B |
| CreateBinaryPatch | 1048576      | 5             | 128        | 1024      | 14.37 us |  35.673 us | 1.955 us |     448 B |
| CreateBinaryPatch | 1048576      | 5             | 128        | 4096      | 15.17 us |  38.672 us | 2.120 us |     448 B |
| CreateBinaryPatch | 1048576      | 5             | 512        | 512       | 15.95 us |  61.300 us | 3.360 us |     448 B |
| CreateBinaryPatch | 1048576      | 5             | 512        | 1024      | 16.00 us |  46.548 us | 2.551 us |     448 B |
| CreateBinaryPatch | 1048576      | 5             | 512        | 4096      | 17.50 us |   4.827 us | 0.265 us |     448 B |
| CreateBinaryPatch | 10485760     | 3             | 128        | 512       | 15.08 us |  45.621 us | 2.501 us |     448 B |
| CreateBinaryPatch | 10485760     | 3             | 128        | 1024      | 17.53 us |  47.934 us | 2.627 us |     448 B |
| CreateBinaryPatch | 10485760     | 3             | 128        | 4096      | 15.60 us |  17.968 us | 0.985 us |     448 B |
| CreateBinaryPatch | 10485760     | 3             | 512        | 512       | 17.13 us |  68.075 us | 3.731 us |     448 B |
| CreateBinaryPatch | 10485760     | 3             | 512        | 1024      | 15.67 us |  42.798 us | 2.346 us |     448 B |
| CreateBinaryPatch | 10485760     | 3             | 512        | 4096      | 18.53 us |  46.381 us | 2.542 us |     448 B |
| CreateBinaryPatch | 10485760     | 5             | 128        | 512       | 17.00 us |  26.311 us | 1.442 us |     448 B |
| CreateBinaryPatch | 10485760     | 5             | 128        | 1024      | 16.05 us |   4.827 us | 0.265 us |     448 B |
| CreateBinaryPatch | 10485760     | 5             | 128        | 4096      | 15.82 us |  53.200 us | 2.916 us |     448 B |
| CreateBinaryPatch | 10485760     | 5             | 512        | 512       | 15.73 us |  36.866 us | 2.021 us |     448 B |
| CreateBinaryPatch | 10485760     | 5             | 512        | 1024      | 17.63 us |  26.895 us | 1.474 us |     448 B |
| CreateBinaryPatch | 10485760     | 5             | 512        | 4096      | 16.13 us |  70.288 us | 3.853 us |     448 B |
| CreateBinaryPatch | 104857600    | 3             | 128        | 512       | 19.27 us |  12.147 us | 0.666 us |     448 B |
| CreateBinaryPatch | 104857600    | 3             | 128        | 1024      | 20.00 us |  94.375 us | 5.173 us |     448 B |
| CreateBinaryPatch | 104857600    | 3             | 128        | 4096      | 22.37 us |  13.693 us | 0.751 us |     448 B |
| CreateBinaryPatch | 104857600    | 3             | 512        | 512       | 20.40 us |   1.824 us | 0.100 us |     448 B |
| CreateBinaryPatch | 104857600    | 3             | 512        | 1024      | 19.00 us |  27.608 us | 1.513 us |     448 B |
| CreateBinaryPatch | 104857600    | 3             | 512        | 4096      | 21.50 us |  47.818 us | 2.621 us |     448 B |
| CreateBinaryPatch | 104857600    | 5             | 128        | 512       | 19.70 us |  41.722 us | 2.287 us |     448 B |
| CreateBinaryPatch | 104857600    | 5             | 128        | 1024      | 19.20 us |  37.699 us | 2.066 us |     448 B |
| CreateBinaryPatch | 104857600    | 5             | 128        | 4096      | 16.63 us |  36.956 us | 2.026 us |     448 B |
| CreateBinaryPatch | 104857600    | 5             | 512        | 512       | 18.03 us |  18.274 us | 1.002 us |     448 B |
| CreateBinaryPatch | 104857600    | 5             | 512        | 1024      | 21.80 us | 145.642 us | 7.983 us |     448 B |
| CreateBinaryPatch | 104857600    | 5             | 512        | 4096      | 16.13 us |  31.086 us | 1.704 us |     448 B |
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
BenchmarkDotNet v0.15.6, Windows 11 (10.0.26200.7171)
AMD Ryzen 7 5800U with Radeon Graphics 1.90GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.100
  [Host]   : .NET 8.0.22 (8.0.22, 8.0.2225.52707), X64 RyuJIT x86-64-v3
  ShortRun : .NET 8.0.22 (8.0.22, 8.0.2225.52707), X64 RyuJIT x86-64-v3
```
