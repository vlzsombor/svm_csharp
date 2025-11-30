using System.Text.Json.Serialization;

namespace ClassLibrary1;

public class SvmData
{
    public SvmData()
    {
        
    }
    public SvmData(double[][] xDatapoints, string[] yLabels)
    {
        Length = yLabels.Length;
        if (Length != xDatapoints.Length)
        {
            throw new ArgumentException();
        }
        this.XDataPoints = xDatapoints;
        this.YLabels = yLabels;
    }

    public string[] YLabels { get; init; }

    public double[][] XDataPoints { get; init; }

    public int Length { get; init; }
}
public class SvmNumberSoa
{
    public SvmNumberSoa()
    {
        
    }
    public SvmNumberSoa(SvmData data)
    {
        SvmData = data;
        this.Alpha = new double[Length];
        this.Optimized = new bool[Length];
        ErrorCache = new double[Length];
        LabelConvert = new sbyte[Length];
    }

    public static SvmNumberSoa Clone(SvmNumberSoa numberSoa, int[] indiciesToCopy)
    {
        int length = indiciesToCopy.Length;
        double[][] dataPoints = new double[length][];
        string[] labels = new string[length];
        double[] alpha = new double[length];
        sbyte[] labelConverted = new sbyte[length];
        var features = numberSoa.SvmData.XDataPoints[0].Length;
        for (int i = 0; i < indiciesToCopy.Length; i++)
        {
            int index = indiciesToCopy[i];
            dataPoints[i] = new double[features];
            dataPoints[i] = numberSoa.SvmData.XDataPoints[index];
            labels[i] = numberSoa.SvmData.YLabels[index];
            alpha[i] = numberSoa.Alpha[index];
            labelConverted[i] = numberSoa.LabelConvert[index];
        }
        var svmNumberSoa = new SvmNumberSoa(new SvmData(dataPoints, labels))
        {
            Alpha = alpha,
            LabelConvert = labelConverted
        };
        return svmNumberSoa;
    }

    public void SetLabel(string labelToIdentify)
    {
        for (int i = 0; i < SvmData.YLabels.Length; i++)
        {
            LabelConvert[i] = (sbyte)(SvmData.YLabels[i] == labelToIdentify ? 1 : -1);
        }
    }

    public sbyte[] LabelConvert { get; set; }

    public SvmData SvmData { get; init; }

    [JsonIgnore]
    public int Length => SvmData.Length;

    public double[] Alpha { get; init; }
    
    [JsonIgnore]
    public readonly bool[] Optimized;

    [JsonIgnore]
    public readonly double[] ErrorCache;
    
    public void UpdateErrorCache(Func<int, double> errorCalculation)
    {
        for (int i = 0; i < ErrorCache.Length; i++)
        {
            ErrorCache[i] = errorCalculation(i);
        }
    }
}