using ClassLibrary1;

namespace SmoTest;

public class Tests
{
    private double[][] input;
    private double[] labels;

    [SetUp]
    public void Setup()
    {
        var lines = File.ReadLines("archive/Iris.csv");  // Lazily read lines
        var result = lines.Skip(1)
            .Select(line =>
            {
                string[] r = line.Split(',');
                var input = r[1..^1].Select(x=>Convert.ToDouble(x));
                var label = r[^1];
                var returnLabel = label == "Iris-setosa" ? 1.0 : -1.0;
                return (input, returnLabel);
            }); // Transform each line into an array of values
//.ToList();  // Materialize the results if needed

         input = result.Select(x => x.Item1.ToArray()).ToArray();
         labels = result.Select(x => x.Item2).ToArray();
    }

    [Test]
    public void Test1()
    {
        Smo smo = new Smo(input, labels, KernelType.Linear);
        var result = smo.Check_KKT(0);
        Assert.False(result);
    }
    
}