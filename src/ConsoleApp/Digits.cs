using System.Text.Json;
using ClassLibrary1;

namespace SVM;

public class Digits
{
    public const string FilePath = Runner.MNT_PATH + "mnist_data/mnist_train.csv";
    public const string TestFilePath = Runner.MNT_PATH + "mnist_data/mnist_test.csv";
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
    public async Task Main(SvmConfig config)
    {
        Logger.Log($"entered {nameof(Main)}");
        var r = Runner.DoLogic(FilePath, Func, config);
        Logger.Log($"End fitting {nameof(Main)}");
        Logger.Log($"start accuracy measurement  {nameof(Main)}");
        await Runner.LoadSvmAccuracy(r, TestFilePath, Func);
        Logger.Log($"end accuracy measurement  {nameof(Main)}");
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

    public async Task MainLoad()
    {
        Logger.Log($"entered {nameof(MainLoad)}");
        await Runner.LoadSvmAccuracy("archive/mnist/OneVsAllClassifier-2147483647-labelsToIdentify-0-1-2-3-4-5-6-7-8_4212.json", TestFilePath, Func);
    }
}