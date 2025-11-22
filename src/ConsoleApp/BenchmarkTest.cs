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
        SvmConfig s = SvmConfig.GetDefault() with { MaxIter = 1000 };
        Digits digits = new(["0", "1"],1000);
        digits.Train(s);
    }
}