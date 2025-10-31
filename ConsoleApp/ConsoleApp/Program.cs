// See https://aka.ms/new-console-template for more information


using System.IO.Compression;
using ClassLibrary1;
using SVM;
using KernelType = SVM.KernelType;

//SVM.SimplifiedSMO simplifiedSmo = new SimplifiedSMO();

var splitString = args.FirstOrDefault();
double split = 0.8;
if (splitString is not null)
{
    split = Convert.ToDouble(splitString);
}

string dataName = "thyroid-disease-data-set";
var fileName = await Static.KaggleDownload("yasserhessein", dataName);
var result = Static.DoLogic(fileName, 0.7);

/*string dataName = "breast-cancer-wisconsin-data";
var fileName = await Static.KaggleDownload("uciml", dataName);

Parallel.ForEach(Enumerable.Range(0, 10),  i =>
{
    var result = Static.DoLogic(fileName, split);
    File.AppendAllText($"result_main", $"Result %: {result}\n");
});
return;


*/


/*

    double[] a = { 0.0, 1, 2, 3, 4 };

    double[] b = { 1, 3, 4 };

    IEnumerable<double> resultadsfas = a.Zip(b, (a, b) => a * b);


    IEnumerable<string> lines = File.ReadLines("archive/Iris.csv"); // Lazily read lines
    IEnumerable<(IEnumerable<double> input, int returnLabel)> result = lines.Skip(1)
        .Select(line =>
        {
            string[] r = line.Split(',');
            IEnumerable<double> input = r[1..^1].Select(x => Convert.ToDouble(x));
            string label = r[^1];
            int returnLabel = label == "Iris-setosa" ? 1 : -1;
            return (input, returnLabel);
        }); 

    double[][] input = result.Select(x => x.Item1.ToArray()).ToArray();
    int[] labels = result.Select(x => x.Item2).ToArray();

    Console.WriteLine("hello");
    SupportVectorMachine machine = new(input, labels, KernelType.Linear);

    int[] res = machine.Predict([[4.9, 3.0, 1.4, 0.2], [5.5, 2.3, 4.0, 1.3]]);


    
    Console.WriteLine(res[0] + " " + res[1]);
    */