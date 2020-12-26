``` ini

BenchmarkDotNet=v0.12.1, OS=macOS Mojave 10.14.6 (18G6042) [Darwin 18.7.0]
Intel Core i5-7360U CPU 2.30GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=5.0.100
  [Host]     : .NET Core 5.0.0 (CoreCLR 5.0.20.51904, CoreFX 5.0.20.51904), X64 RyuJIT
  DefaultJob : .NET Core 5.0.0 (CoreCLR 5.0.20.51904, CoreFX 5.0.20.51904), X64 RyuJIT


```
| Method |     Mean |   Error |  StdDev |   Median |
|------- |---------:|--------:|--------:|---------:|
|    Api | 151.9 μs | 1.04 μs | 0.92 μs | 152.0 μs |
|   CApi | 187.2 μs | 2.95 μs | 7.23 μs | 183.6 μs |
