``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1848/22H2/2022Update/SunValley2)
Intel Core i7-10750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.203
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessNoEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|              Method |       Mean |    Error |   StdDev |       Gen0 |      Gen1 |      Gen2 | Allocated |
|-------------------- |-----------:|---------:|---------:|-----------:|----------:|----------:|----------:|
|    NoTempFileStream |   520.5 ms |  3.96 ms |  5.93 ms |  1000.0000 | 1000.0000 | 1000.0000 |  78.11 MB |
|  GetTempFileAsBytes |   561.8 ms |  6.64 ms |  9.31 ms |  1000.0000 | 1000.0000 | 1000.0000 |  78.11 MB |
| GetTempFileAsString | 1,134.4 ms | 27.98 ms | 41.88 ms | 15000.0000 | 9000.0000 | 3000.0000 | 189.21 MB |
