using System.Diagnostics;
using BenchmarkDotNet.Running;
using ClassLibrary1;
using SVM;

public class Program
{
    public static async Task Main()
    {
//        var summary = BenchmarkRunner.Run<BenchmarkTest>();
//        return;
        var dateTimeNow = DateTime.Now;
        var sw = Stopwatch.StartNew();
        BenchmarkTest benchmarkTest = new BenchmarkTest();
        SvmConfig s = SvmConfig.GetDefault(["1"], 1.0/784, ClassLibrary1.KernelType.Gaussian) with { MaxIter = 1000 };
            
        Digits digits2 = new(2000, s);
        await digits2.TrainAndAccuracy(s);
        sw.Stop();
        var dateTimeNew = DateTime.Now;
        var diff = dateTimeNow - dateTimeNew;
        Console.WriteLine("Time difference" + diff.TotalSeconds);
        return;
        var svmTargets = Environment.GetEnvironmentVariable("svm_targets") ?? "0";
        var targets = svmTargets.Split('-');
        var svmSize = Environment.GetEnvironmentVariable("svm_size");
        var kernelType = Environment.GetEnvironmentVariable("svm_kernel");
        Logger.Log($"Params: {svmSize} {svmTargets}");
        var success = Int32.TryParse(svmSize, out int size);
        if (!success) size = int.MaxValue;
        Logger.Log($"Params: {size} {string.Join(" ", targets)}");
        SvmConfig config = kernelType == "rbf" ? SvmConfig.GetDefault(targets,1.0/784, ClassLibrary1.KernelType.Gaussian) : SvmConfig.GetDefault(targets, 1.0/784, ClassLibrary1.KernelType.Linear);
        
        Digits digits = new(size, config);
        Logger.Log("started");
        Logger.Log($"Kerneltype: {config.KernelType}");
//        await digits.TrainAndAccuracy(config);
        await digits.MainLoad(
            "mnist_data/OneVsAllClassifier-2147483647-labelsToIdentify-0-1-2-3-4-5-6-7-8-9_9058.json");
        Logger.Log("end ---------------------------------------------------------------");
    }
    
}