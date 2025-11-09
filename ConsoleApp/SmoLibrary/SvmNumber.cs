namespace ClassLibrary1;

public record SvmNumber(int i, IEnumerable<double> XDataPoints, double YLabel, double Alpha)
{
    public double Alpha { get; set; } = Alpha;
    public bool Optimized { get; set; }
    public double ErrorCache { get; set; }
}