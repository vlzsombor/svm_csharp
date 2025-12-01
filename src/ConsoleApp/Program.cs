using System.Diagnostics;
using BenchmarkDotNet.Running;
using ClassLibrary1;
using SVM;

public class Program
{
    public static readonly List<string> easyToRecognize = ["1","5"];
    public static readonly List<string> all = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"];
    public static async Task Main()
    {
        Console.WriteLine("SOA");
//        var summary = BenchmarkRunner.Run<BenchmarkTest>();
//        return;
        var dateTimeNow = DateTime.Now;
        var sw = Stopwatch.StartNew();
        BenchmarkTest benchmarkTest = new BenchmarkTest();
        SvmConfig s = SvmConfig.GetDefault(["0", "1", "2"], 0.05, 5, ClassLibrary1.KernelType.Gaussian) with { MaxIter = 2000 };
        
        Digits digits2 = new(600, s);
//        await digits2.TrainAndAccuracy(s);
        await digits2.MainLoad(@"mnist_data/C_5_gamma_0_05_OneVsAllClassifier-0-LabelsToIdentify-0-1-2-3-4-5-6-7-8-9_5077.json");
        sw.Stop();
        Console.WriteLine("Time difference" + sw.Elapsed.TotalMinutes);
        return;
        var svmTargets = Environment.GetEnvironmentVariable("svm_targets") ?? "0";
        var targets = svmTargets.Split('-');
        var svmSize = Environment.GetEnvironmentVariable("svm_size");
        var kernelType = Environment.GetEnvironmentVariable("svm_kernel");
        Logger.Log($"Params: {svmSize} {svmTargets}");
        var success = Int32.TryParse(svmSize, out int size);
        if (!success) size = int.MaxValue;
        Logger.Log($"Params: {size} {string.Join(" ", targets)}");
        SvmConfig config = kernelType == "rbf" ? SvmConfig.GetDefault(targets,1.0/784, 10, ClassLibrary1.KernelType.Gaussian) : SvmConfig.GetDefault(targets, 1.0/784,1.0, ClassLibrary1.KernelType.Linear);
        
        Digits digits = new(size, config);
        Logger.Log("started");
        Logger.Log($"Kerneltype: {config.KernelType}");
//        await digits.TrainAndAccuracy(config);
        await digits.MainLoad(
            "mnist_data/OneVsAllClassifier-2147483647-labelsToIdentify-0-1-2-3-4-5-6-7-8-9_9058.json");
        Logger.Log("end ---------------------------------------------------------------");
    }
    
}