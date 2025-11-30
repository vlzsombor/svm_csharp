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
        var oneVsAllClassifier = await Runner.DoLogic(FilePath, Func, config);
        Logger.Log($"End fitting {nameof(TrainAndAccuracy)}");
        Logger.Log($"start accuracy measurement  {nameof(TrainAndAccuracy)}");
        
        await Runner.LoadSvmAccuracy(oneVsAllClassifier, TestFilePath, Func);
        Logger.Log($"end accuracy measurement  {nameof(TrainAndAccuracy)}");
    }

    public async Task TrainAndAccuracySingleSvm(SvmConfig config)
    {
        Logger.Log($"entered {nameof(TrainAndAccuracy)}");
        var r = await Runner.DoLogicSimple(FilePath, Func, config);
        Logger.Log($"End fitting {nameof(TrainAndAccuracy)}");
        Logger.Log($"start accuracy measurement  {nameof(TrainAndAccuracy)}");
        await Runner.LoadSvmAccuracySimple(r, TestFilePath, Func);
        Logger.Log($"end accuracy measurement  {nameof(TrainAndAccuracy)}");
    }
    public async Task<OneVsAllClassifier> Train(SvmConfig config)
    {
        Logger.Log($"entered {nameof(Train)}");
        var r = await Runner.DoLogic(FilePath, Func, config);
        return r;
    }
    private static Func<string[],string[], int, DataLabelSoa> Func2 =>
        (s, labelsToIdentify, size) =>
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>()
            {
                {"-1", 0}
            };
            foreach (var li in labelsToIdentify)
            {
                dictionary.Add(li, 0);
            }
            string[] labels = new string[dictionary.Count * size];
            double[][] dataPoints = new double[dictionary.Count * size][];
            int newIndex = 0;
            for (int i = 0; i < s.Length; i++)
            {
                var str = s[i];
                var str2 = str.Split(',');
                
                if (labelsToIdentify.Contains(str2[0]))
                {
                    var r = dictionary[str2[0]]++;

                    if (r >= size)
                    {
                        continue;
                    }
                }
                else
                {
                    var r= dictionary["-1"]++;
                    
                    if (r >= size)
                    {
                        continue;
                    }
                }

                labels[newIndex] = str2[0];


                
                    
                dataPoints[newIndex] = new double[str2.Length - 1];
                for (int j = 0; j < dataPoints[newIndex].Length; j++)
                {
                    dataPoints[newIndex][j] = Convert.ToDouble(str2[j+1]) / 255.0;
                }
                
                newIndex++;
            }
                
//                var res = s.Select(s => s.Split(','));
//                var labels = res.Select(x=>x.First()).ToArray();
//                var trains = res.Select(x => x.Skip(1)).Select(x => s.Select(y=> Convert.ToDouble(y)/ 255.0).ToArray()).ToArray();
            return new(dataPoints, labels);

        };   
    
    
    private static Func<string[],string[], int, DataLabelSoa> Func =>
        (s, labelsToIdentify, size) =>
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>()
            {
                {"-1", 0}
            };
            foreach (var li in labelsToIdentify)
            {
                dictionary.Add(li, 0);
            }
            
            string[] labels = new string[s.Length];
            double[][] dataPoints = new double[s.Length][];
            if (size != 0)
            {
                labels = new string[dictionary.Count * size];
                dataPoints = new double[dictionary.Count * size][];
            }
            int newIndex = 0;
            for (int i = 0; i < s.Length; i++)
            {
                var str = s[i];
                var str2 = str.Split(',');
                if(size != 0){
                    if (labelsToIdentify.Contains(str2[0]))
                    {
                        var r = dictionary[str2[0]]++;

                        if (r >= size)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        var r = dictionary["-1"]++;

                        if (r >= size)
                        {
                            continue;
                        }
                    }
                }

                labels[newIndex] = str2[0];


                
                    
                dataPoints[newIndex] = new double[str2.Length - 1];
                for (int j = 0; j < dataPoints[newIndex].Length; j++)
                {
                    dataPoints[newIndex][j] = Convert.ToDouble(str2[j+1]) / 255.0;
                }
                
                newIndex++;
            }
                
//                var res = s.Select(s => s.Split(','));
//                var labels = res.Select(x=>x.First()).ToArray();
//                var trains = res.Select(x => x.Skip(1)).Select(x => s.Select(y=> Convert.ToDouble(y)/ 255.0).ToArray()).ToArray();
            return new(dataPoints, labels);

        };

    public async Task MainLoad(string path)
    {
        Logger.Log($"entered {nameof(MainLoad)}");
        await Runner.LoadSvmAccuracy(path, TestFilePath, Func);
    }
}