```

BenchmarkDotNet v0.15.6, Windows 11 (10.0.26200.7171)
AMD Ryzen 7 7735HS with Radeon Graphics 3.20GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.416
  [Host]     : .NET 8.0.22 (8.0.22, 8.0.2225.52707), X64 RyuJIT x86-64-v3
  Job-MTJJIS : .NET 8.0.22 (8.0.22, 8.0.2225.52707), X64 RyuJIT x86-64-v3

MaxIterationCount=16  

```
| Method | Mean    | Error    | StdDev   | Gen0        | Completed Work Items | Lock Contentions | Gen1       | Gen2      | Allocated |
|------- |--------:|---------:|---------:|------------:|---------------------:|-----------------:|-----------:|----------:|----------:|
| Main   | 2.785 s | 0.1825 s | 0.1707 s | 308000.0000 |           20149.0000 |                - | 96000.0000 | 1000.0000 |    2.4 GB |
