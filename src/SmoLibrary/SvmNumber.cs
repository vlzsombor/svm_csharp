namespace ClassLibrary1;

public record SvmNumber2(int i, double[] XDataPoints, double YLabel, double Alpha)
{
    public double Alpha { get; set; } = Alpha;
    public bool Optimized { get; set; }
    public double ErrorCache { get; set; }
}

public class SvmData
{
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

    public string[] YLabels { get; }

    public double[][] XDataPoints { get; }

    public int Length { get; }
}

public class SvmNumberSoa
{
    public SvmNumberSoa(SvmData data)
    {
        Length = data.Length;
        SvmData = data;
        this.Alpha = new double[Length];
        this.Optimized = new bool[Length];
        ErrorCache = new double[Length];
        LabelConvert = new sbyte[Length];
    }

    public void SetLabel(string labelToIdentify)
    {
        for (int i = 0; i < SvmData.YLabels.Length; i++)
        {
            LabelConvert[i] = (sbyte)(SvmData.YLabels[i] == labelToIdentify ? 1 : -1);
        }
    }
    public sbyte[] LabelConvert { get; }
    public double[][] XDataPoints => SvmData.XDataPoints;

    public SvmData SvmData { get; }

    public int Length { get; }

    public double[] Alpha { get; }
    public bool[] Optimized { get; }
    
    public double[] ErrorCache { get; } 
    
    public void UpdateErrorCache(Func<int, double> errorCalculation)
    {
        for (int i = 0; i < ErrorCache.Length; i++)
        {
            ErrorCache[i] = errorCalculation(i);
        }
    }
}