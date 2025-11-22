namespace ClassLibrary1;

public class OneVsAllClassifier
{
    public OneVsAllClassifier(IEnumerable<DataLabel> result, SvmConfig config)
    {
        IEnumerable<DataLabel> dataLabels = result as DataLabel[] ?? result.ToArray();
        List<string> labels = dataLabels.GroupBy(x => x.Label).Select(x => x.Key).Distinct().ToList();
        Dictionary<string, SvmNumberSoa> list = [];
        labels.ForEach(label =>
        {
            list.TryAdd(label, new SvmNumberSoa(dataLabels.Select(x=>x.Points).ToArray(), dataLabels.Select(x=>x.Label == label ? 1.0 : -1.0).ToArray()));
        });
        foreach (KeyValuePair<string, SvmNumberSoa> e in list) Smos.Add(e.Key, new SvmOptimizer(e.Value, config));
    }

    public OneVsAllClassifier()
    {
        
    }

    public Dictionary<string, SvmOptimizer> Smos { get; set; } = [];

    public void fit()
    {
        Parallel.ForEach(Smos, item =>
        { 
            Logger.Log($"started fitting: {item.Key}");
            item.Value.Fit();
        });
    }

    public string Predict(double[] doubles)
    {
        return Smos
            .Where(x=>x.Value.SupportVectors != null)
            .OrderByDescending(x => x.Value.Predict(doubles)).First().Key;
    }

    public IEnumerable<string> Predict(IEnumerable<double[]> svmNumbers)
    {
        return svmNumbers.Select(number =>
            Smos.OrderByDescending(y => y.Value.Predict(number))
                .First().Key);
    }

}