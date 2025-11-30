namespace ClassLibrary1;

public class OneVsAllClassifier
{
    public SvmConfig _config { get; set; }
    public OneVsAllClassifier(DataLabelSoa result, SvmConfig config)
    {
        _config = config;
//        SvmData svmData = new(result.Points, result.Label);

        Smos = [];
        for (int i = 0; i < config.LabelsToIdentify.Length; i++)
        {
            
            SvmData svmData = new(result.Points, result.Label);
            string label = config.LabelsToIdentify[i];
            SvmNumberSoa svmsoa = new(svmData);
            Smos.Add(new SvmOptimizer(svmsoa, config, label));
        }
    }

    public OneVsAllClassifier()
    {
    }

    public List<SvmOptimizer> Smos { get; set; }

    public void Fit()
    {
        Parallel.ForEach(Smos, smo =>
        {
            Logger.Log($"started fitting: {smo.LabelToIdentify}");
            smo.Fit();
        });
        foreach (var smo in Smos)
        {
        }
    }

    public string Predict(double[] doubles)
    {
        IEnumerable<(string LabelToIdentify, double)> aaa = Smos
            .Select(x => (x.LabelToIdentify, x.Predict(doubles)));

        (string LabelToIdentify, double) r2 = aaa.OrderByDescending(x => x.Item2).FirstOrDefault(x => x.Item2 > 0);
        return r2.LabelToIdentify ?? "-1";

        if (Smos.Count == 1)
        {
            double r = Smos.Single().Predict(doubles);
            return r > 1 ? "1" : "-1";
        }

        SvmOptimizer[] result = Smos
            .Where(x => x.SupportVectors != null)
            .OrderByDescending(x => x.Predict(doubles)).ToArray();
        return result.First().LabelToIdentify;
    }
}