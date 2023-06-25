``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1848/22H2/2022Update/SunValley2)
Intel Core i7-10750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.203
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessNoEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                     Method |       Mean |    Error |   StdDev |       Gen0 |      Gen1 |      Gen2 | Allocated |
|--------------------------- |-----------:|---------:|---------:|-----------:|----------:|----------:|----------:|
|  BenchmarkNoTempFileStream |   986.7 ms |  8.60 ms | 12.87 ms | 15000.0000 | 9000.0000 | 3000.0000 | 187.66 MB |
| BenchmarkGetTempFileStream | 1,101.5 ms | 11.60 ms | 17.37 ms | 15000.0000 | 9000.0000 | 3000.0000 |  189.2 MB |
