using System.Text.Json;
using ClassLibrary1;

namespace SVM;

public class Digits
{
    public const string FilePath = "archive/mnist/train.csv";
    public Runner Runner;

    public Digits(string[] labelsToIdentify, int size)
    {
        if (labelsToIdentify is null or { Length: 0 })
        {
            labelsToIdentify = new[] { "0", "1", "" };
        }
        
        Runner = new Runner(labelsToIdentify, size);
    }
    public void Main()
    {
        Logger.Log($"entered {nameof(Main)}");
        var result = new List<double>();
        var r = Runner.DoLogic(FilePath, Func);
        result.Add(r);
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
        await Runner.LoadSvmAccuracy("archive/mnist/OneVsAllClassifier-0-1-labelsToIdentify-0-1.json", FilePath, 50_000, Func);
    }
}