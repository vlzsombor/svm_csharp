namespace ClassLibrary1;

public class DataLabelSoa(double[][] points, string[] label)
{
    public double[][] Points { get; set; } = points;
    public string[] Label { get; set; } = label;
}


public readonly ref struct DataLabelSoa2(ReadOnlySpan<string> label, ReadOnlySpan<double[]> values)
{
    public readonly ReadOnlySpan<string> Label = label;
    public readonly ReadOnlySpan<double[]> Values = values;
}