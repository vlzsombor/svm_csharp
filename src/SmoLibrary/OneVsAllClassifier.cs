namespace ClassLibrary1;

public class OneVsAllClassifier
{
    public OneVsAllClassifier(IEnumerable<DataLabel> result)
    {
        List<string> labels = result.GroupBy(x => x.Label).Select(x => x.Key).Distinct().ToList();
        Dictionary<string, List<SvmNumber>> list = [];
        int i = 0;
        foreach (var r in result)
        {
            labels.ForEach(x =>
            {
                list.TryAdd(x, new List<SvmNumber>());
                list[x].Add(new SvmNumber(i, r.Points, r.Label == x ? 1 : -1, 0));
            });

            i++;
        }

        foreach (KeyValuePair<string, List<SvmNumber>> e in list) Smos.Add(e.Key, new SvmOptimizer(e.Value));
    }

    public OneVsAllClassifier()
    {
        
    }

    public Dictionary<string, SvmOptimizer> Smos { get; set; } = [];

    public void fit()
    {
        Parallel.ForEach(Smos, item =>
        { 
            Logger.Log("started: ");
            item.Value.Fit();
        });
    }

    public string Predict(double[] doubles)
    {
//        
//        var r=  Smos.First(x=>x.Key == "1").Value.Predict2(doubles);
////            .Where(x=>x.Value.IsFitted)
//
//        if (r > 0)
//        {
//            return "1";
//        }
//
//        return "-1";
        
        return Smos
//            .Where(x=>x.Value.IsFitted)
            .Where(x=>x.Value.SupportVectors != null)
            .OrderByDescending(x => x.Value.Predict2(doubles)).First().Key;
    }

    public IEnumerable<string> Predict(IEnumerable<double[]> svmNumbers)
    {
        return svmNumbers.Select(number =>
            Smos.OrderByDescending(y => y.Value.Predict2(number))
                .First().Key);
    }

    public IEnumerable<string> Predict(IEnumerable<SvmNumber> svmNumbers)
    {
        return Predict(svmNumbers.Select(x => x.XDataPoints));
    }
}