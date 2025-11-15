namespace ClassLibrary1;

public record DataLabel(double[] Points, string Label)
{
    public string Label { get; set; } = Label;
}