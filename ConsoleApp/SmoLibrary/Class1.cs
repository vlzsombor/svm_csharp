using System.Runtime.CompilerServices;

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
    private readonly KernelType _kernelType;
    private readonly int _b;
    private readonly double _kktThr;
    private readonly int N;
    private readonly int M;

    public readonly SvmOptimizer _svmOptimizer;
    //    private readonly IEnumerable<SvmNumber> _supportSvmNumbers;

    public Smo(double[][] xDataPoints, double[] yLabels, KernelType kernelType, int c = 1, double kktThr = 0.01)
    {
        if (xDataPoints.Length != yLabels.Length)
        {
            throw new ArgumentException($"{xDataPoints.Length} != {yLabels.Length} ");
        }

        N = xDataPoints.Length;
        M = xDataPoints[0].Length;

        int i = 0;
        var _dataPoints = xDataPoints.Zip(yLabels, (x, y) => new SvmNumber(
            i++,
            x,
            y,
            0.0,
            true)).ToList();
        _kernelType = kernelType;
        _b = 0;
        _c = c;
        _kktThr = kktThr;
        //       _supportSvmNumbers = _dataPoints.ToList();
        _svmOptimizer = new SvmOptimizer(SvmConfig.GetDefault(), _dataPoints);
    }

    public void Fit()
    {
        while (true)
        {
            var svmNumber1 = _svmOptimizer.Heuristic1();

            if (svmNumber1 is null)
            {
                return;
            }

            var svmNumber2 = _svmOptimizer.Heuristic2(svmNumber1);

            if (svmNumber2 == svmNumber1)
            {
                continue;
            }

            (double L, double H) = _svmOptimizer.ComputeBoundaries(svmNumber1, svmNumber2);

            if (L == H)
            {
                continue;
            }
            
            
            break;
        }

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

public record SvmConfig(double C, double KktThr, KernelType KernelType)
{
    public static SvmConfig GetDefault()
    {
        return new(1.0, 0.01, KernelType.Linear);
    }
}

public class SvmOptimizer
{
    private readonly SvmConfig _svmConfig;
    private readonly IEnumerable<SvmNumber> _dataPoints;
    public double[] W { get; set; }
    public double B { get; set; }

    public SvmOptimizer(SvmConfig svmConfig, List<SvmNumber> dataPoints)
    {
        _svmConfig = svmConfig;
        _dataPoints = dataPoints;
    }

    public SvmNumber Heuristic2(SvmNumber? alpha1 = null)
    {
        alpha1 ??= Heuristic1();
        var nonBound = NonBoundExamples();
        /// todo ??????????????
        var alphaError = Error(alpha1);

        var errorCache = _dataPoints.Select(Error);
        var values = _dataPoints.Select(x => Math.Abs(Error(x) - alphaError));
        return _dataPoints 
            .OrderByDescending(x => Math.Abs(Error(x) - alphaError))
            .First();
    }

    public (double lb, double ub) ComputeBoundaries(SvmNumber alpha1, SvmNumber alpha2)
    {
        Func<double> lbF = () => alpha1.Alpha + alpha2.Alpha - _svmConfig.C;
        Func<double> ubF = () => alpha1.Alpha + alpha2.Alpha;
        
        if (alpha1.YLabel != alpha2.YLabel)
        {
            lbF = () => alpha2.Alpha - alpha1.Alpha;
            ubF = () => _svmConfig.C + alpha2.Alpha - alpha1.Alpha;
        }

        return (lb: Math.Max(0, lbF()), ub: Math.Min(_svmConfig.C, ubF()));
    }
    public SvmNumber? Heuristic1()
    {
        var firstOrDefault = _dataPoints.FirstOrDefault(x => !Check_KKT(x));
        if (firstOrDefault is not null)
        {
            return firstOrDefault;
        }

        throw new NotImplementedException();
    }

    
    public IEnumerable<SvmNumber> NonBoundExamples()
    {
        return _dataPoints.Where(x => (x.Alpha > 0 && x.Alpha < _svmConfig.C));
    }

    public double Error(SvmNumber svmNumber)
        => Predict(svmNumber) - svmNumber.YLabel;

    public bool Check_KKT(SvmNumber svmNumber)
    {
        var score = Predict(svmNumber);
        var ro = svmNumber.YLabel * score - 1;
        var cond1 = (svmNumber.Alpha < _svmConfig.C) & (ro < -_svmConfig.KktThr);
        var cond2 = (svmNumber.Alpha > 0) & (ro > _svmConfig.KktThr);
        return !(cond1 || cond2);
    }
    
    public double Predict(SvmNumber inputSvmNumber)
    {
        if (_svmConfig.KernelType == KernelType.Linear)
        {
            
        }
        return _dataPoints 
            .Select(sn => sn.YLabel 
                          * sn.Alpha 
                          * LinearKernel(sn.XDataPoints, inputSvmNumber.XDataPoints))
            .Sum() - B;
    }
    
    
    public static double LinearKernel(IEnumerable<double> xj, IEnumerable<double> x) 
        => xj.InnerProduct(x);
    
    
}
public record SvmNumber(int Id, IEnumerable<double> XDataPoints, double YLabel, double Alpha, bool SupportVector);