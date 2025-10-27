// See https://aka.ms/new-console-template for more information


using SVM;

//SVM.SimplifiedSMO simplifiedSmo = new SimplifiedSMO();


double[] a = { 0.0, 1, 2, 3, 4 };

double[] b = { 1,3,4 };

var resultadsfas = a.Zip(b, (a, b) => a * b);


var lines = File.ReadLines("archive/Iris.csv");  // Lazily read lines
var result = lines.Skip(1)
    .Select(line =>
    {
        string[] r = line.Split(',');
        var input = r[1..^1].Select(x=>Convert.ToDouble(x));
        var label = r[^1];
        var returnLabel = label == "Iris-setosa" ? 1 : -1;
        return (input, returnLabel);
    }); // Transform each line into an array of values
//.ToList();  // Materialize the results if needed

var input = result.Select(x => x.Item1.ToArray()).ToArray();
var labels = result.Select(x => x.Item2).ToArray();

Console.WriteLine("hello");
SVM.SupportVectorMachine machine = new SupportVectorMachine(input, labels, KernelType.Linear);

var res = machine.Predict([[4.9, 3.0, 1.4, 0.2],[5.5,2.3,4.0,1.3]]);


Console.WriteLine(res[0]+ " "+ res[1]);