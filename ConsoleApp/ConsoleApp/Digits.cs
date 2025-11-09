using System.Text.Json;
using ClassLibrary1;

namespace SVM;

public class Digits
{
    private const string FilePath = "archive/mnist/train.csv";
    
    public void Main()
    {
        var result = new List<double>();
        for (int i = 0; i < 5; i++)
        {
            var r = Static.DoLogic(FilePath, 0.8, Func);
            result.Add(r);
        }

        foreach (var r in result)
        {
            Console.WriteLine(r);
        }
    }

    private static Func<string, (double[], string label)> Func
    {
        get
        {
            Func<string, (double[], string label)> func = s =>
            {
                try
                {

                    var res = s.Split(',');
                    var label = res[0];
                    var train = res[1..].Select(x=> Convert.ToDouble(x) / 255.0).ToArray();
                    return (train, label);
                }
                catch (Exception e)
                {
                }

                return ([], "");
            };
            return func;
        }
    }

    public async Task MainLoad()
    {
        var ones = "archive/mnist/ones.csv";
        var nonones = "archive/mnist/nonOnes.csv";
       // var result = await Static.LoadSvm(FilePath, nonones, Func);
        
        await Static.LoadSvmAccuracy("archive/mnist/oneVsAllClassifier7.json", FilePath, 50_000, Func);
    }
}