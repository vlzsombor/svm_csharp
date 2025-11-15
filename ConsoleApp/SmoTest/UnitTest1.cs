using System.IO.Compression;
using ClassLibrary1;

namespace SmoTest;

public class Tests
{
    private double[][] input;
    private double[] labels;
    private IEnumerable<SvmNumber> svmNumbers;

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
        SvmOptimizer SvmOptimizer = new(svmNumbers);
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

        SvmOptimizer SvmOptimizer = new(svmNumbers.Take(length));
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
    [Test]
    public async Task Iris()
    {
        double split = 0.7;
        string dataName = "breast-cancer-wisconsin-data";
        var fileName = "archive/Iris.csv";
        List<double> results = [];
        Runner runner = new Runner(["Iris-setosa", "Iris-virginica", "Iris-virginica"], 50);
        for (int i = 0; i < 5; i++)
        {
            var r= runner.DoLogic(fileName, line =>
            {
                string[] r = line.Split(',');
                double[] input = r[..^1].Where(x=>!string.IsNullOrEmpty(x)).Select(Convert.ToDouble).ToArray();
                string label = r[^1];
                return new DataLabel(input, label);
            });
            results.Add(r);
            
        }

        Console.WriteLine("average" + results.Average());

    }
}