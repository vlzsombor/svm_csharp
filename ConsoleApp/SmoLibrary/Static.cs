using System.Globalization;
using System.IO.Compression;
using System.Numerics;

//Der Iris-Datensatz (nur zwei Klassen, z. B. Setosa vs. Versicolor) ist linear separierbar bei den Features sepal length und petal length.
namespace ClassLibrary1;

public static class Static
{
    
    public static double InnerProduct(this double[] u, double[] v)
    {
        
        int length = u.Length;
        int simdLength = Vector<double>.Count;
        int i = 0;
        Vector<double> acc = Vector<double>.Zero;

        for (; i <= length - simdLength; i += simdLength)
        {
            var vu = new Vector<double>(u, i);
            var vv = new Vector<double>(v, i);
            acc += vu * vv;
        }

        double sum = 0.0;
        for (int j = 0; j < simdLength; j++)
            sum += acc[j];

        for (; i < length; i++)
            sum += u[i] * v[i];

        return sum;
        return u.Zip(v, (a, b) => a * b).Sum();
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

        if (Directory.Exists(dataName)) Directory.Delete(dataName, true);
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

    public static string GetSavePath(string directory, string filePath)
    {
        if (Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return Path.Combine(directory, filePath);
    }
    public static string PathCombine(this DirectoryInfo directoryInfo, string path) => Path.Combine(directoryInfo.FullName, path);
}