namespace ClassLibrary1;

public class Svm
{
}

public enum KernelType
{
    Linear,
    Gaussian
}


public class Smo
{
    public const int LOWER_BOUND = 0;
    private readonly int _c;
    private readonly double[][] _xDataPoints;
    private readonly double[] _yLabels;
    private readonly KernelType _kernelType;
    private readonly double[] _alphas;
    private readonly int _b;
    private readonly double _kktThr;
    private readonly int N;
    private readonly int M;
    private double[] _supportLabels { get; }
    private double[][] _supportVectors { get; }

    public Smo(double[][] xDataPoints, double[] yLabels, KernelType kernelType, int c = 1, double kktThr = 0.01)
    {
        if (xDataPoints.Length != yLabels.Length)
        {
            throw new ArgumentException($"{xDataPoints.Length} != {yLabels.Length} ");
        }

        N = xDataPoints.Length;
        M = xDataPoints[0].Length;
        _xDataPoints = xDataPoints;
        _yLabels = yLabels;
        _kernelType = kernelType;
        _alphas = new double[N];
        _b = 0;
        _c = c;
        _kktThr = kktThr;
        _supportLabels = yLabels;
        _supportVectors = xDataPoints;
    }

    public void Fit()
    {
        int[] nonKktIndices = Enumerable.Range(0, _yLabels.Length).ToArray();
        
    }

    private void Heuristic1(List<int> nonKktIndices)
    {
        foreach (var nonKktIndex in nonKktIndices.ToList())
        {
            nonKktIndices.Remove(nonKktIndex);
        }
    }

    private IEnumerable<double> Kernel(double[][] x, double[] y)
    {
        if (_kernelType == KernelType.Linear)
        {
            
        }

        return x.InnerProduct(y);
    }
    private double Predict(double[] x)
    {
        var w = _supportLabels.Zip(_alphas, (sl, a) => sl * a);
        var xNew = Kernel(_supportVectors, x);
        var scores = w.InnerProduct(x) + _b;
        return scores;
    }
    public bool Check_KKT(int index)
    {
        var alpha = _alphas[index];
        var score = Predict(_supportVectors[index]);
        var label = _supportLabels[index];
        var ro = label * score - 1;
        var cond1 = (alpha < _c) & (ro < -_kktThr);
        var cond2 = (alpha > 0) & (ro > _kktThr);
        return !(cond1 || cond2);
    }
/*
    public Func<double, double, bool> Check_KKT()
    {
        Func<double, double, bool> c1 = (alpha, ro) => alpha < _c && ro < -_kktThr;
        Func<double, double, bool> c2 = (alpha, ro) => alpha > LOWER_BOUND && ro > _kktThr;
        return (alpha, ro) => c1(alpha, ro) || c2(alpha, ro);
    }

    public Func<double, bool> NonBoundExamples()
        => a => a >= LOWER_BOUND && a <= _c;*/
        
}