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
            r.input,
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
        var fileName = "archive/Iris.csv"; //await KaggleDownload("uciml", dataName);
        List<double> results = [];
        for (int i = 0; i < 10; i++)
        {
            var r= Static.DoLogic(fileName, split, line =>
            {
                string[] r = line.Split(',');
                //Iris-setosa
                //Iris-versicolor
                //Iris-virginica
                double[] input = r[..^1].Where(x=>!string.IsNullOrEmpty(x)).Select(Convert.ToDouble).ToArray();
                //double[] filteredInput = new[] { input[0], input[2] };
                string label = r[^1];
                return (input, label);
            });
            results.Add(r);
            
        }

        Console.WriteLine("average" + results.Average());


    }
    [Test]
    public async Task Test3()
    {
        float split = 0.7f;

        string dataName = "breast-cancer-wisconsin-data";
        var fileName = await KaggleDownload("uciml", dataName);
        for (int i = 0; i < 1; i++)
        {

            IEnumerable<string> lines = File.ReadLines(fileName); // Lazily read lines
            (IEnumerable<float> input, string label)[] result = lines.Skip(1)
                .Select(line =>
                {
                    string[] r = line.Split(',');
                    //Iris-setosa
                    //Iris-versicolor
                    //Iris-virginica
                    IEnumerable<float> input = r[2..].Select(x => Convert.ToSingle(x));
                    var filteredInput = input;
                    string label = r[1];
                    return (filteredInput, label);
                }).ToArray();

            Random r = new();
            r.Shuffle(result);

            int length = (int)(result.Length * split);
            List<(IEnumerable<float> filteredInput, string label)> train = result.Take(length).ToList();
            List<(IEnumerable<float> filteredInput, string label)> test = result.Skip(length).ToList();
            //OneVsAllClassifier oneVsAllClassifier = new(train);

//            oneVsAllClassifier.fit();

//            int correctCount = test.Count(x => oneVsAllClassifier.Predict(x.input) == x.label);
//            Console.WriteLine((float)correctCount / test.Count);

//        oneVsAllClassifier.Predict(test.Select(x=>x.input)).Select();
        }
    }


    [Test]
    public async Task Test5()
    {
        string dataName = "wine-quality-dataset";
        var fileName = await Static.KaggleDownload("yasserh", dataName);
        var result = Static.DoLogic(fileName, 0.7f, s =>
        {
            try
            {

                var split = s.Split(',');

                var train = split[..^1].Select(Convert.ToDouble);
                var label = split[^1];

                return (train.ToArray(), label);
            }
            catch (Exception e)
            {
            }

            return ([], "null");
        } );

    }
    [Test]
    public async Task Test6()
    {
        //string dataName = "thyroid-disease-data-set";
        //var fileName = await Static.KaggleDownload("yasserhessein", dataName);
        var result = Static.DoLogic("archive/penguins_size.csv", 0.8f, x =>
        {
            try
            {

                var s = x.Split(',');
                var label = s[0];
                var train = s[2..4].Select(Convert.ToDouble);

                return (train.ToArray(), label);
            }
            catch
            {
            }

            return ([], "null");
        });

    }

    public async Task<string> KaggleDownload(string username, string dataName)
    {
        if (File.Exists(Path.Combine(dataName, dataName + ".csv"))) return Path.Combine(dataName, dataName + ".csv");

        using HttpClient httpClient = new();
//        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer YOUR_KAGGLE_KEY");
        HttpResponseMessage response =
            await httpClient.GetAsync($"https://www.kaggle.com/api/v1/datasets/download/{username}/{dataName}");
        string zipName = dataName + ".zip";
        await using (FileStream fileStream = File.Create(zipName))
        {
            await response.Content.CopyToAsync(fileStream);
        }

        await using FileStream fileStreamRead = File.OpenRead(zipName);
        Directory.Delete(dataName, true);
        Directory.CreateDirectory(dataName);

        ZipFile.ExtractToDirectory(fileStreamRead, dataName);
        string[] files = Directory.GetFiles(dataName);
        if (files.Length > 0)
        {
            string originalFile = files[0];
            string newFileName = Path.Combine(dataName, dataName + ".csv");
            File.Move(originalFile, newFileName);
            return newFileName;
        }

        throw new ArgumentException();
    }
}