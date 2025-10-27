using ClassLibrary1;

namespace SmoTest;

public class Tests
{
    private double[][] input;
    private double[] labels;
    private IEnumerable<SvmNumber> svmNumbers;

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

         int i = 0;
         svmNumbers = result.Select(r => new SvmNumber(
             i++,
             r.input, 
             r.returnLabel, 
             0.0, 
             true));
    }

    [Test]
    public void Test1()
    {
        Smo smo = new Smo(input, labels, KernelType.Linear);
        var res= smo._svmOptimizer.Check_KKT(svmNumbers.First());
        Assert.False(res);
    }
    
    [Test]
    public void Test2()
    {
        Smo smo = new Smo(input, labels, KernelType.Linear);
        smo.Fit();
    }
    
    [Test]
    public void Test3()
    {
        Smo smo = new Smo(input, labels, KernelType.Linear);
        var res = smo._svmOptimizer.Predict(svmNumbers.First(x => x.Id == 50));
        Assert.Pass();
    }
       
    [Test]
    public void Test4()
    {
        Smo smo = new Smo(input, labels, KernelType.Linear);
        var res = smo._svmOptimizer.Predict(svmNumbers.First(x => x.Id == 50));
        Assert.Pass();
    } 
    
}