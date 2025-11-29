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
        SvmConfig s = SvmConfig.GetDefault(["0", "1"], 1.0/784, ClassLibrary1.KernelType.Gaussian) with { MaxIter = 1000 };
        Digits digits = new(10, s);
        await digits.Train(s);
        
    }
}