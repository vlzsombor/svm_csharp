using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ClassLibrary1;

public class Runner(int size, SvmConfig svmConfig)
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
//    public async Task LoadSvmAccuracy(string jsonPath, string testDataSetPath, Func<string, DataLabelSoa> func)
//    {
//        var jsonConfig = await File.ReadAllTextAsync(jsonPath);
//        OneVsAllClassifier oneVsAllClassifier = JsonSerializer.Deserialize<OneVsAllClassifier>(jsonConfig) ?? throw new InvalidOperationException();
//        
//        IEnumerable<string> lines = File.ReadLines(testDataSetPath).Skip(1).ToArray(); // Lazily read lines
//
//        var result = lines 
//            .Select(func).Where(x=> !string.IsNullOrEmpty(x.Label) && x.Label != "null").ToArray();
//        Accuracy(oneVsAllClassifier, result);
//    }

    public async Task LoadSvmAccuracy(OneVsAllClassifier oneVsAllClassifier, string testDataSetPath, Func<string[], DataLabelSoa> func)
    {
        string[] allLines = await File.ReadAllLinesAsync(testDataSetPath); // Lazily read lines
        var result = func(allLines);
        Accuracy(oneVsAllClassifier, result);
    }
    
    
    public double Accuracy(OneVsAllClassifier oneVsAllClassifier, DataLabelSoa dataLabels)
    {
        int allCount = 0;
        int correctCount = 0;

        for (int i = 0; i < dataLabels.Label.Length; i++)
        {
            var predicted = oneVsAllClassifier.Predict(dataLabels.Points[i]);
            bool r = LabelFilter(predicted) == LabelFilter(dataLabels.Label[i]);
            if (r) correctCount++;
            Console.WriteLine((double)correctCount/allCount);
            allCount++;
        }
        var result = (double)correctCount / allCount;
        Logger.Log($"{correctCount}/{allCount}");
        Logger.Log("The accuracy is: "+ result.ToString(CultureInfo.InvariantCulture));
        return result;
    }

    
    public string LabelFilter(string s)
    {
        return LabelFilter(s, svmConfig);
    }
    public static string LabelFilter(string s, SvmConfig svmConfig)
    {
        return svmConfig.LabelsToIdentify.Contains(s) ? s : "-1";
    }
/*
    public IEnumerable<DataLabel> FilterTargetLabels(IEnumerable<DataLabel> dataLabel)
    {
        var res = dataLabel.ToArray();

        for (int index = 0; index < res.Length; index++)
        {
            DataLabel r = res[index];
            r.Label = LabelFilter(r.Label);
        }

        var pos = labelsToIdentify
            .Select(digit =>
                res.Where(label => label.Label == digit).Take(size)).SelectMany(x=>x);
        var rest = res.Where(l => !labelsToIdentify.Contains(l.Label)).Take(size);
        return pos.Concat(rest);
    }
*/
    public async Task<OneVsAllClassifier> DoLogic(string fileName, Func<string[], DataLabelSoa> func, SvmConfig config)
  {
       string[] allLines = await File.ReadAllLinesAsync(fileName); // Lazily read lines
//       string[] dataLines = new string[allLines.Length - 1];
//       Array.Copy(allLines, 1, dataLines, 0, dataLines.Length);
//        //string[] dataLines = new string[size * config.labelsToIdentify.Length];
//        using (var reader = new StreamReader(fileName))
//        {
//            for (int i = -1; i<size; i++)
//            {
//                string? line = await reader.ReadLineAsync();
//                if(i == -1) continue;
//                if (line == null) break;
//                dataLines[i] = line;
//            }
//        }

      Random r = new();
      r.Shuffle(allLines);
        var dataLines = allLines.Skip(1).Where(x => !config.LabelsToIdentify.Contains(x.Split(',').First())).Take(size / 2)
            .Concat(allLines.Where(x => config.LabelsToIdentify.Contains(x.Split(',').First())).Take(size/2)).ToArray();
        r.Shuffle(dataLines);
//      var dataLines = allLines[46..56];
        var result = func(dataLines);
        
        //result = FilterTargetLabels(result).ToArray();
        Logger.Log($"The result size: {result.Label.Length}");
        Logger.Log($"The file path for result: {fileName}");
        OneVsAllClassifier oneVsAllClassifier = new(result, config);
        Logger.Log($"{oneVsAllClassifier.Smos.Count}, {string.Join(" ",oneVsAllClassifier.Smos.ToList())}");
        Logger.Log("Fit start:");
        oneVsAllClassifier.Fit();
        string jsonString = JsonSerializer.Serialize(oneVsAllClassifier);
        var directoryInfo = Directory.CreateDirectory($"{MNT_PATH}{DateTime.Now:yy-MM-dd}");
        File.WriteAllText($"{directoryInfo.PathCombine($"{nameof(OneVsAllClassifier)}-{size}-{nameof(svmConfig.LabelsToIdentify)}-{string.Join('-', svmConfig.LabelsToIdentify)}_{new Random().Next(10000)}.json")}", jsonString);
        Logger.Log("Fit end");
        return oneVsAllClassifier;
    }
    
}