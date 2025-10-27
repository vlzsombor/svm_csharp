namespace ClassLibrary1;

public static class Static
{
    public static double InnerProduct(this IEnumerable<double> u, double[] v) 
        => u.Zip(v, (a, b) => a * b).Sum();
    
    public static IEnumerable<double> InnerProduct(this IEnumerable<IEnumerable<double>> u, double[] v) 
        => u.Select(x => x.InnerProduct(v));
}