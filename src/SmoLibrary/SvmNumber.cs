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
        this.ErrorCache = new double[Length];
        this.Alpha = new double[Length];
        this.Optimized = new bool[Length];
    }

    public double[][] XDataPoints => SvmData.XDataPoints;

    public SvmData SvmData { get; set; }

    public SvmNumberSoa SetSupportVector()
    {
        SupportVectors = Alpha.Where(a => a > 0).Select((a, i) => SvmData.XDataPoints[i]).ToArray();
        return this;
    }
    public double[][] SupportVectors { get; private set; }

    public int Length { get; }

    public double[] Alpha { get; set; }
    public bool[] Optimized { get; set; }
//    public double[][] XDataPoints { get; init; }
//    public double[] YLabel { get; init; }
    public double[] ErrorCache { get; init; }

    public void UpdateErrorCache(Func<int, double> errorCalculation)
    {
        for (int i = 0; i < ErrorCache.Length; i++)
        {
            ErrorCache[i] = errorCalculation(i);
        }
    }
}