using System.Runtime.CompilerServices;

namespace ClassLibrary1;

public enum KernelType
{
    Linear,
    Gaussian
}

public class OneVsAllClassifier
{
    public Dictionary<string, Smo> Smos { get; set; }
    public OneVsAllClassifier()
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
                return (input, label);
            });

        var input = result.Select(x => x.Item1.ToArray()).ToArray();
        var labels = result.Select(x => x.Item2).ToArray();

        int i = 0;
 
//        Smos = new Dictionary<string, Smo>({})

        var aaa = result.GroupBy(x => x.label)
            .Select(x=> x.Key)
            .Distinct();

        foreach (var label in aaa)
        {
            
        }

        
        var svmNumbers = result.Select(r => new SvmNumber(
            i++,
            r.input, 
            r.label == "Iris-setos" ? 1 : -1, 
            0.0, 
            true));
        
        
    }
    public void fit(IEnumerable<SvmNumber> svmNumbers)
    {
        foreach (var number in svmNumbers)
        {
            
        }
    }
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
    public const int MAX_ITER = 1_000;
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
        int i = 0;
        while (i < MAX_ITER)
        {
            var heuristic2 = _svmOptimizer.Heuristic2();

            if (heuristic2  is null)
            {
                break;
            }
            var heuristic1 = _svmOptimizer.Heuristic1(heuristic2 );

            if (heuristic1 == heuristic2 )
            {
                continue;
            }

            (double L, double H) = _svmOptimizer.ComputeBoundaries(heuristic1, heuristic2);

            if (L == H)
            {
                continue;
            }

            var eta = _svmOptimizer.CalculateEta(heuristic1 , heuristic2);

            if (eta == 0)
            {
                continue;
            }

            var e1 = _svmOptimizer.Error(heuristic1 );
            var e2 = _svmOptimizer.Error(heuristic2);

            var alpha2new = _svmOptimizer.NewAlpha2(heuristic2, e1, e2, eta, H, L);

            var alpha1new = _svmOptimizer.NewAlpha1(heuristic1 , heuristic2, alpha2new);

            _svmOptimizer.B = _svmOptimizer.CalculateB(heuristic1, heuristic2, alpha1new, alpha2new);
            
            heuristic2.Alpha = alpha2new;
            heuristic1.Alpha = alpha1new;
            i++;
        }

        var res = _svmOptimizer._dataPoints.Where(x => x.Alpha is > 0 and < 1).ToList();
        Console.WriteLine(res);
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
    public readonly IEnumerable<SvmNumber> _dataPoints;
    public double[] W { get; set; }
    public double B { get; set; }

    public SvmOptimizer(SvmConfig svmConfig, List<SvmNumber> dataPoints)
    {
        _svmConfig = svmConfig;
        _dataPoints = dataPoints;
    }

    public SvmNumber Heuristic1(SvmNumber? alpha1 = null)
    {
        //alpha1 ??= Heuristic2();
        var nonBound = NonBoundExamples();

        var alphaError = Error(alpha1);
        if (nonBound.Any())
        {
            if (alphaError >= 0)
            {
                return nonBound 
                    .OrderBy(Error)
                    .First();
            }

            return nonBound 
                .OrderByDescending(Error)
                .First();
        }
        

        var errorCache = _dataPoints.Select(Error).ToList();
        var values = _dataPoints.Select(x => Math.Abs(Error(x) - alphaError));
        return _dataPoints 
            .OrderByDescending(x => Math.Abs(Error(x) - alphaError))
            .First();
    }

    public double CalculateEta(SvmNumber x1, SvmNumber x2)
    {
        return LinearKernel(x1.XDataPoints, x1.XDataPoints) 
               + LinearKernel(x2.XDataPoints, x2.XDataPoints) 
               - 2 * LinearKernel(x1.XDataPoints, x2.XDataPoints);
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
    public SvmNumber? Heuristic2()
    {
        foreach (var dp in _dataPoints.Where(x=>!x.Optimized))
        {
            dp.Optimized = true;
            if (!Check_KKT(dp))
            {
                return dp;
            }
        }
        
//        //var firstOrDefault = _dataPoints.FirstOrDefault(x => !x.Optimized && !Check_KKT(x) );
//        if (firstOrDefault is not null)
//        {
//            firstOrDefault.Optimized = true;
//            return firstOrDefault;
//        }
        _dataPoints.ToList().ForEach(x=>x.Optimized = Check_KKT(x));
        
        foreach (var dp in _dataPoints.Where(x=>!x.Optimized))
        {
            dp.Optimized = true;
            if (Check_KKT(dp))
            {
                return dp;
            }
        }

        return null;
    }

    
    public IEnumerable<SvmNumber> NonBoundExamples()
    {
        return _dataPoints.Where(x => (x.Alpha > 0 && x.Alpha < _svmConfig.C));
    }

    public double Error(SvmNumber svmNumber)
    {
        var aa = Predict(svmNumber) - svmNumber.YLabel;

        return Predict(svmNumber) - svmNumber.YLabel;
    }

    public double NewAlpha1(SvmNumber n1, SvmNumber n2, double alphanew)
    {
        return n1.Alpha + n1.YLabel * n2.YLabel * (n2.Alpha - alphanew);
    }
    public double NewAlpha2(SvmNumber number, double e1, double e2, double eta, double H, double L)
    {
        var alpha2new = (number.Alpha + number.YLabel * (e1 - e2))/eta;
        alpha2new = Math.Min(alpha2new, H);
        return alpha2new = Math.Max(alpha2new, L);
    }
    public bool Check_KKT(SvmNumber svmNumber)
    {
        var score = Predict(svmNumber);
        var ro = svmNumber.YLabel * score - 1;
        var cond1 = (svmNumber.Alpha < _svmConfig.C) & (ro < -_svmConfig.KktThr);
        var cond2 = (svmNumber.Alpha > 0) & (ro > _svmConfig.KktThr);
        return !(cond1 || cond2);
    }
    
    public double Predict(IEnumerable<double> inputSvmNumber)
    {
        if (_svmConfig.KernelType == KernelType.Linear)
        {
            
        }
        return _dataPoints 
            .Select(sn => sn.YLabel 
                          * sn.Alpha 
                          * LinearKernel(sn.XDataPoints, inputSvmNumber))
            .Sum() + B;
    }
    public double Predict(SvmNumber inputSvmNumber)
    {
        return Predict(inputSvmNumber.XDataPoints);
    }
    
    
    public static double LinearKernel(IEnumerable<double> xj, IEnumerable<double> x)
    {
        return RbfKernel(xj.ToArray(), x.ToArray(), 0.5);
        return xj.InnerProduct(x);
    }
    public static double RbfKernel(double[] x1, double[] x2, double gamma)
    {
        double squaredDistance = 0.0;
        for (int i = 0; i < x1.Length; i++)
        {
            double diff = x1[i] - x2[i];
            squaredDistance += diff * diff;
        }
        return Math.Exp(-gamma * squaredDistance);
    }

    public double CalculateB(SvmNumber s1, SvmNumber s2, double alphaNew1, double alphaNew2)
    {
        var b1 = B - Error(s1) - s1.YLabel * (alphaNew1 - s1.Alpha) * LinearKernel(s1.XDataPoints, s1.XDataPoints)
                 - s2.YLabel * (alphaNew2 - s2.Alpha) * LinearKernel(s1.XDataPoints, s2.XDataPoints);

        var b2 = B - Error(s2) - s1.YLabel * (alphaNew1 - s1.Alpha) * LinearKernel(s1.XDataPoints, s2.XDataPoints)
                 - s2.YLabel * (alphaNew2 - s2.Alpha) * LinearKernel(s2.XDataPoints, s2.XDataPoints);

        if (0 < alphaNew1 && alphaNew1 < _svmConfig.C)
        {
            return b1;
        }

        if (0 < alphaNew2 && alphaNew2 < _svmConfig.C)
        {
            return b2;
        }

        return (b1 + b2) / 2;
    }
}
public record SvmNumber(int Id, IEnumerable<double> XDataPoints, double YLabel, double Alpha, bool SupportVector)
{
    public double Alpha { get; set; } = Alpha;
    public bool Optimized { get; set; } = false;
}