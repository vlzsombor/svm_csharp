using System.Text.Json;
using ClassLibrary1;

namespace SVM;

public class Digits
{
    public string FilePath = Runner.MNT_PATH + "mnist_data/mnist_train.csv";
    public string TestFilePath = Runner.MNT_PATH + "mnist_data/mnist_test.csv";
    //public const string TestFilePath ="archive/mnist/mynumber.csv";// "archive/mnist/test.csv";
    public Runner Runner;

    public Digits(int size, SvmConfig svmConfig)
    {
        Runner = new Runner(size, svmConfig);
    }
    public async Task TrainAndAccuracy(SvmConfig config)
    {
        Logger.Log($"entered {nameof(TrainAndAccuracy)}");
        var r = await Runner.DoLogic(FilePath, Func, config);
        Logger.Log($"End fitting {nameof(TrainAndAccuracy)}");
        Logger.Log($"start accuracy measurement  {nameof(TrainAndAccuracy)}");
        await Runner.LoadSvmAccuracy(r, TestFilePath, Func);
        Logger.Log($"end accuracy measurement  {nameof(TrainAndAccuracy)}");
    }

    public async Task<OneVsAllClassifier> Train(SvmConfig config)
    {
        Logger.Log($"entered {nameof(Train)}");
        var r = await Runner.DoLogic(FilePath, Func, config);
        return r;
    }
    private static Func<string[], DataLabelSoa> Func =>
        (s) =>
        {
                string[] labels = new string[s.Length];
                double[][] dataPoints = new double[s.Length][];
                for (int i = 0; i < s.Length; i++)
                {
                    var str = s[i];
                    var str2 = str.Split(',');
                    labels[i] = str2[0];
                    
                    dataPoints[i] = new double[str2.Length - 1];
                    for (int j = 0; j < dataPoints[i].Length; j++)
                    {
                        dataPoints[i][j] = Convert.ToDouble(str2[j+1]) / 255.0;
                    }
                }
                
//                var res = s.Select(s => s.Split(','));
//                var labels = res.Select(x=>x.First()).ToArray();
//                var trains = res.Select(x => x.Skip(1)).Select(x => s.Select(y=> Convert.ToDouble(y)/ 255.0).ToArray()).ToArray();
                return new(dataPoints, labels);

            return new([], []);
        };

    public async Task MainLoad(string path)
    {
        Logger.Log($"entered {nameof(MainLoad)}");
//        await Runner.LoadSvmAccuracy(path, TestFilePath, Func);
    }
}