namespace ClassLibrary1;

public class OneVsAllClassifier
{
    public OneVsAllClassifier(DataLabelSoa result, SvmConfig config)
    {
        string[] labelsDiff = result.Label.Select(x=> config.LabelsToIdentify.Contains(x) ? x : "-1").Distinct().ToArray();   //new string[result.Label.Length];
        SvmData svmData = new(result.Points.ToArray(), result.Label);

        var svmNumberSoa = new SvmNumberSoa(svmData);
        
        for (int i = 0; i < labelsDiff.Length; i++)
        {
            var label = labelsDiff[i];
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
        return Smos
            .Where(x=>x.SupportVectors != null)
            .OrderByDescending(x => x.Predict(doubles)).First().LabelToIdentify;
    }
}