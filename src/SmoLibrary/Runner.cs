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
        
            
        IEnumerable<string> lines = File.ReadLines(testDataSetPath).Skip(1); // Lazily read lines

        var result = lines 
            .Select(func).Where(x=> !string.IsNullOrEmpty(x.label) && x.label != "null");

        return result.Select(x => oneVsAllClassifier.Predict(x.Item1));
    }
    public async Task LoadSvmAccuracy(string jsonPath, string testDataSetPath, Func<string[], string[], int, DataLabelSoa> func)
    {
        var jsonConfig = await File.ReadAllTextAsync(jsonPath);
        OneVsAllClassifier oneVsAllClassifier = JsonSerializer.Deserialize<OneVsAllClassifier>(jsonConfig) ?? throw new InvalidOperationException();
        IEnumerable<string> lines = File.ReadLines(testDataSetPath).Skip(1); // Lazily read lines
        var result = func(lines.ToArray(), oneVsAllClassifier.Smos.Select(x => x.LabelToIdentify).ToArray(), 10_000);
        oneVsAllClassifier.Smos =
            oneVsAllClassifier.Smos.Where(x => svmConfig.LabelsToIdentify.Contains(x.LabelToIdentify)).ToList();
        oneVsAllClassifier.Smos.ForEach(x=>x.SupportVectors.SetLabel(x.LabelToIdentify));
        await Accuracy(oneVsAllClassifier, result);
    }

    public async Task LoadSvmAccuracy(OneVsAllClassifier oneVsAllClassifier, string testDataSetPath, Func<string[],string[], int, DataLabelSoa> func)
    {
        string[] allLines = (await File.ReadAllLinesAsync(testDataSetPath)); // Lazily read lines
        string[] dataLines = new string[allLines.Length - 1];
        Array.Copy(allLines, 1, dataLines, 0, dataLines.Length);        
        var result = func(dataLines, svmConfig.LabelsToIdentify.ToArray(), (int)(size*0.8));
        
        await Accuracy(oneVsAllClassifier, result);
    }
    
    public async Task LoadSvmAccuracySimple(SvmOptimizer optimizer, string testDataSetPath, Func<string[],string[], int, DataLabelSoa> func)
    {
        string[] allLines = (await File.ReadAllLinesAsync(testDataSetPath)); // Lazily read lines
        string[] dataLines = new string[allLines.Length - 1];
        Array.Copy(allLines, 1, dataLines, 0, dataLines.Length);        
        var result = func(dataLines, svmConfig.LabelsToIdentify.ToArray(), (int)((size * 2) * 0.8));
        await AccuracySimple(optimizer, result);
    }
    public async Task<double> AccuracySimple(SvmOptimizer optimizer, DataLabelSoa dataLabels)
    {
        int truePositive = 0;
        int trueNegative = 0;
        int falsePositive = 0;
        int falseNegative = 0;
        int correctCount = 0;
        int allCount = 0;
        Thread.Sleep(1000);
        for (int i = 0; i < dataLabels.Label.Length; i++)
        {
            if (dataLabels.Points[i] == null)
            {
                continue;
            }
            
            var predictedLabel = optimizer.Predict(dataLabels.Points[i]) > 0 ? optimizer.LabelToIdentify : "-1";
            var realLabel = LabelFilter(dataLabels.Label[i]);

            if (predictedLabel == realLabel)
            {
                if (predictedLabel == "-1")
                {
                    trueNegative++;
                }

                if (predictedLabel != "-1")
                {
                    truePositive++;
                }
            }

            if (predictedLabel != realLabel)
            {
                if (predictedLabel == "-1")
                {
                    falseNegative++;
                }

                if (predictedLabel != "-1")
                {
                    falsePositive++;
                }
            }
            if (predictedLabel == realLabel) correctCount++;
            allCount++;
            if (i % 1000 == 0)
            {
                Console.WriteLine((double)correctCount/allCount + " " + predictedLabel + " vs" + dataLabels.Label[i]);
            }
        }


        try
        {
            var precision = (double)truePositive / (truePositive + falsePositive);
            var recall = (double)truePositive / (truePositive + falseNegative);
            var f1Score = 2 * (precision * recall) / (precision + recall);

            var specificity = (double)trueNegative / (trueNegative + falsePositive);
            Logger.Log("Precision " + precision);
            Logger.Log("Recall " + recall);
            Logger.Log("F1 " + f1Score);
            Logger.Log("Specificity " + specificity);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        var result = (double)correctCount / allCount;
        Logger.Log($"{correctCount}/{allCount}");
        Logger.Log("The accuracy is: "+ result.ToString(CultureInfo.InvariantCulture));
        return result;
    }
    
    public async Task<double> Accuracy(OneVsAllClassifier oneVsAllClassifier, DataLabelSoa dataLabels)
    {
        int truePositive = 0;
        int trueNegative = 0;
        int falsePositive = 0;
        int falseNegative = 0;
        int correctCount = 0;
        int allCount = 0;
        for (int i = 0; i < dataLabels.Label.Length; i++)
        {
            if (dataLabels.Points[i] == null)
            {
                continue;
            }
            var predictedLabel = oneVsAllClassifier.Predict(dataLabels.Points[i]);
            var realLabel = LabelFilter(dataLabels.Label[i]);

            if (predictedLabel == realLabel)
            {
                if (predictedLabel == "-1")
                {
                    trueNegative++;
                }

                if (predictedLabel != "-1")
                {
                    truePositive++;
                }
            }

            if (predictedLabel != realLabel)
            {
                if (predictedLabel == "-1")
                {
                    falseNegative++;
                }

                if (predictedLabel != "-1")
                {
                    falsePositive++;
                }
            }
            if (predictedLabel == realLabel) correctCount++;
            allCount++;
            if (i % 1000 == 0)
            {
                Console.WriteLine((double)correctCount/allCount + " " + predictedLabel + " vs" + dataLabels.Label[i]);
            }
        }

        try
        {
            var precision = (double)truePositive / (truePositive + falsePositive);
            var recall = (double)truePositive / (truePositive + falseNegative);
            var f1Score = 2 * (precision * recall) / (precision + recall);

            var specificity = (double)trueNegative / (trueNegative + falsePositive);
            Logger.Log("Precision " + precision);
            Logger.Log("Recall " + recall);
            Logger.Log("F1 " + f1Score);
            Logger.Log("Specificity " + specificity);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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

    public async Task<OneVsAllClassifier> DoLogic(string fileName, Func<string[],string[], int, DataLabelSoa> func, SvmConfig config)
    {
        string[] allLines = await File.ReadAllLinesAsync(fileName); // Lazily read lines
        string[] dataLines = new string[allLines.Length - 1];
        Array.Copy(allLines, 1, dataLines, 0, dataLines.Length);

        Random r = new();
        r.Shuffle(dataLines);
        var result = func(dataLines, svmConfig.LabelsToIdentify, size);
        Logger.Log($"The result size: {result.Label.Length}");
        Logger.Log($"The file path for result: {fileName}");
        Logger.Log($"Max iter: {config.MaxIter}");
        
        OneVsAllClassifier oneVsAllClassifier = new(result, config);
        Logger.Log($"{oneVsAllClassifier.Smos.Count}, {string.Join(" ",oneVsAllClassifier.Smos.Select(x=>x.LabelToIdentify).ToList())}");
        Logger.Log("Fit start:");
        oneVsAllClassifier.Fit();
        string jsonString = JsonSerializer.Serialize(oneVsAllClassifier);
        var directoryInfo = Directory.CreateDirectory($"{MNT_PATH}{DateTime.Now:yy-MM-dd}");
        File.WriteAllText($"{directoryInfo.PathCombine($"{nameof(OneVsAllClassifier)}-{size}-{nameof(svmConfig.LabelsToIdentify)}-{string.Join('-', svmConfig.LabelsToIdentify)}_{new Random().Next(10000)}.json")}", jsonString);
        Logger.Log("Fit end");
        return oneVsAllClassifier;
    }
    

    public async Task<SvmOptimizer> DoLogicSimple(string fileName, Func<string[],string[], int, DataLabelSoa> func, SvmConfig config)
    {
        string[] allLines = await File.ReadAllLinesAsync(fileName); // Lazily read lines
        string[] dataLines = new string[allLines.Length - 1];
        Array.Copy(allLines, 1, dataLines, 0, dataLines.Length);

        Random r = new();
        r.Shuffle(dataLines);

        var result = func(dataLines.ToArray(), svmConfig.LabelsToIdentify, size);
        Logger.Log($"The result size: {result.Label.Length}");
        Logger.Log($"The file path for result: {fileName}");
        
        SvmData svmData = new(result.Points, result.Label);
        var svmNumberSoa = new SvmNumberSoa(svmData);
        SvmOptimizer optimizer = new SvmOptimizer(svmNumberSoa, config, config.LabelsToIdentify.First());
        Logger.Log("Fit start:");
        optimizer.Fit();
        string jsonString = JsonSerializer.Serialize(optimizer);
        var directoryInfo = Directory.CreateDirectory($"{MNT_PATH}{DateTime.Now:yy-MM-dd}");
        File.WriteAllText($"{directoryInfo.PathCombine($"optimizer-{nameof(OneVsAllClassifier)}-{size}-{nameof(svmConfig.LabelsToIdentify)}-{string.Join('-', svmConfig.LabelsToIdentify)}_{new Random().Next(10000)}.json")}", jsonString);
        Logger.Log("Fit end");
        return optimizer;
    }
}