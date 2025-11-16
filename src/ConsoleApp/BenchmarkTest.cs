using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using ClassLibrary1;

namespace SVM;
[MemoryDiagnoser]
[ThreadingDiagnoser]
[MaxIterationCount(16)]
public class BenchmarkTest
{
    [Benchmark]
    public void Main()
    {
        SvmConfig s = SvmConfig.GetDefault() with { MaxIter = 100 };
        Digits digits = new(["0"],10);
        digits.Train(s);
    }
}