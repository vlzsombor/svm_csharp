using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ClassLibrary1;

public class Runner(IEnumerable<string> labelsToIdentify, int size)
{
    
    public static string MNT_PATH
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return string.Empty;
            return "/app/data/";
        }
    }
    public async Task<IEnumerable<string>> LoadSvm(string jsonPath, string testDataSetPath,  Func<string, (double[], string label)> func)
    {
        var jsonConfig = await File.ReadAllTextAsync(jsonPath);
        OneVsAllClassifier oneVsAllClassifier = JsonSerializer.Deserialize<OneVsAllClassifier>(jsonConfig) ?? throw new InvalidOperationException();
        
            
        IEnumerable<string> lines = File.ReadLines(testDataSetPath).Skip(1).ToArray(); // Lazily read lines

        var result = lines 
            .Select(func).Where(x=> !string.IsNullOrEmpty(x.label) && x.label != "null").ToArray();

        return result.Select(x => oneVsAllClassifier.Predict(x.Item1));
    }
    public async Task LoadSvmAccuracy(string jsonPath, string testDataSetPath, Func<string, DataLabel> func)
    {
        var jsonConfig = await File.ReadAllTextAsync(jsonPath);
        OneVsAllClassifier oneVsAllClassifier = JsonSerializer.Deserialize<OneVsAllClassifier>(jsonConfig) ?? throw new InvalidOperationException();
        
        IEnumerable<string> lines = File.ReadLines(testDataSetPath).Skip(1).ToArray(); // Lazily read lines

        var result = lines 
            .Select(func).Where(x=> !string.IsNullOrEmpty(x.Label) && x.Label != "null").ToArray();
        Accuracy(oneVsAllClassifier, result);
    }

    public async Task LoadSvmAccuracy(OneVsAllClassifier oneVsAllClassifier, string testDataSetPath, Func<string, DataLabel> func)
    {
        IEnumerable<string> lines = File.ReadLines(testDataSetPath).Skip(1).ToArray(); // Lazily read lines

        var result = lines 
            .Select(func).Where(x=> !string.IsNullOrEmpty(x.Label) && x.Label != "null").ToArray();
        Accuracy(oneVsAllClassifier, result);
    }

    public double Accuracy(OneVsAllClassifier oneVsAllClassifier, IEnumerable<DataLabel> dataLabels)
    {
        int allCount = 0;
        int correctCount = 0;
        Parallel.ForEach(dataLabels, x=>
        {
            var predicted = oneVsAllClassifier.Predict(x.Points);
            bool r = LabelFilter(predicted) == LabelFilter(x.Label);
            if (r) correctCount++;
            allCount++;
        });
        var result = (double)correctCount / allCount;
        Logger.Log("The accuracy is: "+ result.ToString(CultureInfo.InvariantCulture));
        return result;
    }

    public string LabelFilter(string s)
    {
        return labelsToIdentify.Contains(s) ? s : "-1";
    }
    public IEnumerable<DataLabel> FilterTargetLabels(IEnumerable<DataLabel> dataLabel)
    {
        var res = dataLabel.ToArray();

        foreach (var r in res)
        {
            r.Label = LabelFilter(r.Label);
        }
        var pos = labelsToIdentify 
            .Select(digit =>
                res.Where(label => label.Label == digit).Take(size)).SelectMany(x=>x);
        var rest = res.Where(l => !labelsToIdentify.Contains(l.Label)).Take(size);
        return pos.Concat(rest);
    }

    public OneVsAllClassifier DoLogic(string fileName, Func<string, DataLabel> func, SvmConfig config)
    {
        var lines = File.ReadLines(fileName).Skip(1).ToArray(); // Lazily read lines
        
        Random r = new();
        var linesfiltered = lines[..];
        var result = linesfiltered 
            .Select(func).Where(x=> !string.IsNullOrEmpty(x.Label) && x.Points.Length != 0 && x.Label != "null").ToArray();
        result = FilterTargetLabels(result).ToArray();
        r.Shuffle(result);
        Logger.Log($"The result size: {result.Length}");
        Logger.Log($"The file path for result: {fileName}");
        var train = result;
        OneVsAllClassifier oneVsAllClassifier = new(train, config);
        Logger.Log($"{oneVsAllClassifier.Smos.Count}, {string.Join(" ",oneVsAllClassifier.Smos.Keys.ToList())}");
        Logger.Log("Fit start:");
        oneVsAllClassifier.fit();
        string jsonString = JsonSerializer.Serialize(oneVsAllClassifier);
        var directoryInfo = Directory.CreateDirectory($"{MNT_PATH}{DateTime.Now:yy-MM-dd}");
        File.WriteAllText($"{directoryInfo.PathCombine($"{nameof(OneVsAllClassifier)}-{size}-{nameof(labelsToIdentify)}-{string.Join('-', labelsToIdentify)}_{new Random().Next(10000)}.json")}", jsonString);
        Logger.Log("Fit end");
        return oneVsAllClassifier;
    }
    
}