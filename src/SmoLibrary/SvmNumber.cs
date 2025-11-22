namespace ClassLibrary1;

public record SvmNumber2(int i, double[] XDataPoints, double YLabel, double Alpha)
{
    public double Alpha { get; set; } = Alpha;
    public bool Optimized { get; set; }
    public double ErrorCache { get; set; }
}

public class SvmNumberSoa
{
    public SvmNumberSoa(double[][] xDataPoints, double[] yLabel)
    {
        Length = yLabel.Length;
        if (xDataPoints.Length != Length) 
        {
            throw new ArgumentException("Imputs with different length");
        }
        this.XDataPoints = xDataPoints;
        this.YLabel = yLabel;
        this.ErrorCache = new double[Length];
        this.Alpha = new double[Length];
        this.Optimized = new bool[Length];
    }

    public SvmNumberSoa Clone(int[] ints)
    {
        int length = ints.Length;
        SvmNumberSoa clone = new(new double[length][], new double[length]);

        for (int i = 0; i < length; i++)
        {
            var index = ints[i];
            clone.Alpha[i] = Alpha[index];
            clone.Optimized[i] = Optimized[index];
            clone.XDataPoints[i] = XDataPoints[index];
            clone.YLabel[i] = YLabel[index];
            clone.ErrorCache[i] = ErrorCache[index];
        }

        return clone;
    }

    public int Length { get; }

    public double[] Alpha { get; set; }
    public bool[] Optimized { get; set; }
    public double[][] XDataPoints { get; init; }
    public double[] YLabel { get; init; }
    public double[] ErrorCache { get; init; }

    public void UpdateErrorCache(Func<int, double> errorCalculation)
    {
        for (int i = 0; i < ErrorCache.Length; i++)
        {
            ErrorCache[i] = errorCalculation(i);
        }
    }
}