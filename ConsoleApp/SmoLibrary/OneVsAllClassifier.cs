namespace ClassLibrary1;

public class OneVsAllClassifier
{
    public OneVsAllClassifier(IEnumerable<(double[] filteredInput, string label)> result)
    {
        List<string> labels = result.GroupBy(x => x.label).Select(x => x.Key).Distinct().ToList();
        Dictionary<string, List<SvmNumber>> list = [];
        int i = 0;
        foreach ((IEnumerable<double> input, string label) r in result)
        {
            labels.ForEach(x =>
            {
                list.TryAdd(x, new List<SvmNumber>());
                list[x].Add(new SvmNumber(i, r.input, r.label == x ? 1 : -1, 0));
            });

            i++;
        }

        foreach (KeyValuePair<string, List<SvmNumber>> e in list) Smos.Add(e.Key, new SvmOptimizer(e.Value));
    }

    public Dictionary<string, SvmOptimizer> Smos { get; set; } = [];

    public void fit()
    {
        foreach (KeyValuePair<string, SvmOptimizer> smo in Smos) smo.Value.Fit();
    }

    public string Predict(IEnumerable<double> doubles)
    {

        
        return Smos.OrderByDescending(x => x.Value.Predict(doubles)).First().Key;
    }

    public IEnumerable<string> Predict(IEnumerable<IEnumerable<double>> svmNumbers)
    {
        return svmNumbers.Select(number =>
            Smos.OrderByDescending(y => y.Value.Predict(number))
                .First().Key);
    }

    public IEnumerable<string> Predict(IEnumerable<SvmNumber> svmNumbers)
    {
        return Predict(svmNumbers.Select(x => x.XDataPoints));
    }
}