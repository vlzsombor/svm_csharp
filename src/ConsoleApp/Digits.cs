using System.Text.Json;
using ClassLibrary1;

namespace SVM;

public class Digits
{
    public string FilePath = Runner.MNT_PATH + "mnist_data/mnist_train.csv";
    public string TestFilePath = Runner.MNT_PATH + "mnist_data/mnist_test.csv";
    //public const string TestFilePath ="archive/mnist/mynumber.csv";// "archive/mnist/test.csv";
    public Runner Runner;

    public Digits(string[] labelsToIdentify, int size)
    {
        if (labelsToIdentify is null or { Length: 0 })
        {
            labelsToIdentify = new[] { "0", "1", "" };
        }
        
        Runner = new Runner(labelsToIdentify, size);
    }
    public async Task TrainAndAccuracy(SvmConfig config)
    {
        Logger.Log($"entered {nameof(TrainAndAccuracy)}");
        var r = Runner.DoLogic(FilePath, Func, config);
        Logger.Log($"End fitting {nameof(TrainAndAccuracy)}");
        Logger.Log($"start accuracy measurement  {nameof(TrainAndAccuracy)}");
        await Runner.LoadSvmAccuracy(r, TestFilePath, Func);
        Logger.Log($"end accuracy measurement  {nameof(TrainAndAccuracy)}");
    }

    public void Train(SvmConfig config)
    {
        Logger.Log($"entered {nameof(Train)}");
        var r = Runner.DoLogic(FilePath, Func, config);
    }
    private static Func<string, DataLabel> Func =>
        s =>
        {
            try
            {

                var res = s.Split(',');
                var label = res[0];
                var train = res[1..].Select(x=> Convert.ToDouble(x) / 255.0).ToArray();
                return new(train, label);
            }
            catch
            {
                // ignored
            }
            return new([], "");
        };

    public async Task MainLoad(string path)
    {
        Logger.Log($"entered {nameof(MainLoad)}");
        await Runner.LoadSvmAccuracy(path, TestFilePath, Func);
    }
}