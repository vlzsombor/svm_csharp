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
                //Iris-setosa
                //Iris-versicolor
                //Iris-virginica
                var input = r[1..^1].Select(x=>Convert.ToDouble(x));
                var label = r[^1];
                var returnLabel = label == "Iris-virginica" ? 1.0 : -1.0;
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
        var length = (int)(input.Length * 0.7);
        Random r = new Random();
        var svmNumbers = this.svmNumbers.ToArray();
        
        r.Shuffle(svmNumbers);
        
        Smo smo = new Smo(svmNumbers.Take(length).Select(x=>x.XDataPoints.ToArray()).ToArray(), svmNumbers.Take(length).Select(x=>x.YLabel).ToArray(), KernelType.Linear);
        smo.Fit();
        int correct = 0;
        int total = 0;
        foreach (var v in svmNumbers.Skip(length))
        {
            var aaa = smo._svmOptimizer.Predict(v);
            var label = aaa > 0;

            var trueLabel = v.YLabel > 0;

            if (trueLabel == label)
            {
                correct++;
            }

            total++;
        }

        double res = (double)correct / total;
        Console.WriteLine(res);
        
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