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
    public async Task Main()
    {
        SvmConfig s = SvmConfig.GetDefault(["0"], 0.5, ClassLibrary1.KernelType.Linear) with { MaxIter = 1000 };
        Digits digits = new(1000, s);
        await digits.Train(s);
        
    }
}