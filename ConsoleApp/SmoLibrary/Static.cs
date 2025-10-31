using System.IO.Compression;

//Der Iris-Datensatz (nur zwei Klassen, z. B. Setosa vs. Versicolor) ist linear separierbar bei den Features sepal length und petal length.
namespace ClassLibrary1;

public static class Static
{
    public static double InnerProduct(this IEnumerable<double> u, IEnumerable<double> v)
    {
        return u.Zip(v, (a, b) => a * b).Sum();
    }

    public static IEnumerable<double> InnerProduct(this IEnumerable<IEnumerable<double>> u, IEnumerable<double> v)
    {
        return u.Select(x => x.InnerProduct(v));
    }
    
    
    public static double DoLogic(string fileName, double split)
    {
        IEnumerable<string> lines = File.ReadLines(fileName); // Lazily read lines
        var result = lines.Skip(1)
            .Select(line =>
            {
                string[] r = line.Split(',');
                //Iris-setosa
                //Iris-versicolor
                //Iris-virginica
                var input = r[1..^1].Select(Convert.ToDouble).ToArray();
                var filteredInput = new double[] { input[0], input[2] };
                string label = r[^1];
                return (filteredInput, label);
            }).ToArray();

        Random r = new();
        result = result.Where(x => x.label is "Iris-setosa" or "Iris-versicolo").ToArray();
        r.Shuffle(result);

        int length = (int)(result.Length * split);
        var train = result.Take(length);
        var test = result.Skip(length).ToList();
        OneVsAllClassifier oneVsAllClassifier = new(train);

        oneVsAllClassifier.fit();
        int correctCount = test.Count(x => oneVsAllClassifier.Predict(x.filteredInput) == x.label);
        var finalR = (double)correctCount / test.Count;
        File.WriteAllText($"result_{r.Next()}_{DateTime.Now.Minute}", $"Result %: {finalR}");
        Console.WriteLine(finalR);
        return finalR;
    }
    
    

    public static async Task<string> KaggleDownload(string username, string dataName)
    {
        if (File.Exists(Path.Combine(dataName, dataName + ".csv"))) return Path.Combine(dataName, dataName + ".csv");
        ;

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

        if (Directory.Exists(dataName))
        {
            Directory.Delete(dataName, true);
        }
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