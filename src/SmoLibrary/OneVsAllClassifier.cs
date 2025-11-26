namespace ClassLibrary1;

public class OneVsAllClassifier
{
    public OneVsAllClassifier(DataLabelSoa result, SvmConfig config)
    {
//        string[] labelsDiff = result.Label.Select(x=> config.LabelsToIdentify.Contains(x) ? x : "-1").Distinct().ToArray();   //new string[result.Label.Length];
        SvmData svmData = new(result.Points.ToArray(), result.Label);

        var svmNumberSoa = new SvmNumberSoa(svmData);
        
        for (int i = 0; i < config.LabelsToIdentify.Length; i++)
        {
            var label = config.LabelsToIdentify[i];
            Smos.Add(new SvmOptimizer(svmNumberSoa, config, label));
        }
    }

    public OneVsAllClassifier()
    {
        
    }

    public List<SvmOptimizer> Smos { get; set; } = [];

    public void Fit()
    {
        
        foreach (var smo in Smos)
        {
            Logger.Log($"started fitting: {smo.LabelToIdentify}");
            smo.Fit();
        }
//        Parallel.ForEach(Smos, item =>
//        { 
//        });
    }

    public string Predict(double[] doubles)
    {
        var aaa = Smos
            .Select(x => (x.LabelToIdentify, x.Predict(doubles))).ToList();

        var r2=  aaa.OrderByDescending(x => x.Item2).FirstOrDefault(x => x.Item2 > 0);
        return r2.LabelToIdentify ?? "-1";
        
        if (Smos.Count == 1)
        {
            var r = Smos.Single().Predict(doubles);
            return r > 1 ? "1" : "-1";
        }
        
        var result = Smos
            .Where(x => x.SupportVectors != null)
            .OrderByDescending(x => x.Predict(doubles)).ToArray();
        return result.First().LabelToIdentify;
    }
}