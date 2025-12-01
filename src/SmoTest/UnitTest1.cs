using System.IO.Compression;
using ClassLibrary1;

namespace SmoTest;

public class Tests
{
    private double[][] input;
    private double[] labels;
//    private IEnumerable<SvmNumber> svmNumbers;
/*
    [SetUp]
    public void Setup()
    {
        IEnumerable<string> lines = File.ReadLines("archive/Iris.csv"); // Lazily read lines
        var result = lines.Skip(1)
            .Select(line =>
            {
                string[] r = line.Split(',');
                //Iris-setosa
                //Iris-versicolor
                //Iris-virginica
                IEnumerable<double> input = r[1..^1].Select(x => Convert.ToDouble(x));
                string label = r[^1];
                double returnLabel = label == "Iris-virginica" ? 1.0f : -1.0f;
                return (input, returnLabel);
            }); // Transform each line into an array of values
//.ToList();  // Materialize the results if needed

        input = result.Select(x => x.Item1.ToArray()).ToArray();
        labels = result.Select(x => x.Item2).ToArray();

        int i = 0;
        svmNumbers = result.Select(r => new SvmNumber(
            i++,
            r.input.ToArray(),
            r.returnLabel,
            0.0f));
    }


    [Test]
    public void Test1()
    {
        SvmOptimizer SvmOptimizer = new(svmNumbers, SvmConfig.GetDefault());
        bool res = SvmOptimizer.Check_KKT(svmNumbers.First());
        Assert.False(res);
    }

    [Test]
    public void Test2()
    {
        int length = (int)(input.Length * 0.7);
        Random r = new();
        SvmNumber[] svmNumbers = this.svmNumbers.ToArray();

        r.Shuffle(svmNumbers);

        SvmOptimizer SvmOptimizer = new(svmNumbers.Take(length), SvmConfig.GetDefault());
        SvmOptimizer.Fit();
        int correct = 0;
        int total = 0;
        foreach (SvmNumber v in svmNumbers.Skip(length))
        {
            var aaa = SvmOptimizer.Predict(v);
            bool label = aaa > 0;

            bool trueLabel = v.YLabel > 0;

            if (trueLabel == label) correct++;

            total++;
        }

        float res = (float)correct / total;
        Console.WriteLine(res);
    }

    */
public const string irisSetosa = "Iris-setos";
public const string irisVerisicolor = "Iris-versicolor";
public const string irisVirginica = "Iris-virginica";
    
    [Test]
    public async Task Iris()
    {
        var fileName = "archive/IrisTrain.csv";
        var svmConfig = SvmConfig.GetDefault([irisSetosa, irisVirginica, irisVerisicolor], 0.5, 1,KernelType.Gaussian);
        Runner runner = new Runner(300, svmConfig);
        Func<string[], string[], int, DataLabelSoa> func = (lines, identify, size) =>
        {
            double[][] points = new double[lines.Length][];
            string[] labels = new string[lines.Length];
            for (int j = 0; j < lines.Length; j++)
            {
                labels[j] = "";
                string[] r = lines[j].Split(',');
                string label = r[^1];
                labels[j] = label;
                double[] input = r[1..^1].Where(x=>!string.IsNullOrEmpty(x)).Select(Convert.ToDouble).ToArray();
                points[j] = input;
            }
            DataLabelSoa dataLabelSoa = new DataLabelSoa(points, labels);
            return dataLabelSoa;
        };
        var r= await runner.DoLogic(fileName, func, svmConfig);

        await runner.LoadSvmAccuracy(r, "archive/IrisTest.csv", func);
    }

    [Test]
    public async Task Moons()
    {
        var fileName = "archive/breast-cancer/breast-cancer.csv";
        var svmConfig = SvmConfig.GetDefault(["1","0"], 0.5, 1, KernelType.Gaussian);
        Runner runner = new Runner(300, svmConfig);
        Func<string[], string[], int, DataLabelSoa> func = (lines, identify, size) =>
        {
            double[][] points = new double[lines.Length][];
            string[] labels = new string[lines.Length];
            for (int j = 0; j < lines.Length; j++)
            {
                labels[j] = "";
                string[] r = lines[j].Split(',');
                string label = r[1];
                labels[j] = label;
                double[] input = r[1..].Where(x=>!string.IsNullOrEmpty(x)).Select(Convert.ToDouble).ToArray();
                points[j] = input;
            }
            DataLabelSoa dataLabelSoa = new DataLabelSoa(points, labels);
            return dataLabelSoa;
        };
        var r= await runner.DoLogic(fileName, func, svmConfig);

        await runner.LoadSvmAccuracy(r, "archive/breast-cancer/breast-cancer-test.csv", func);
    }
}