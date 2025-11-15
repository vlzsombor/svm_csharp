using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Principal;

namespace ClassLibrary1;

public class SvmOptimizer
{
    private IEnumerable<SvmNumber> _dataPoints;

    //public bool IsFitted { get; private set; }
    private SvmConfig _svmConfig => SvmConfig.GetDefault();

    public SvmOptimizer(IEnumerable<SvmNumber> dataPoints, SvmConfig? svmConfig = null)
    {
        svmConfig ??= SvmConfig.GetDefault();
        _dataPoints = dataPoints;
    }

    public SvmOptimizer()
    {
        
    }

    //public double[] W => WCalculation();
    public double B { get; set; }
    public List<SvmNumber> SupportVectors { get; set; }
    public double[] WCalculation()
    {
        int featureCount = _dataPoints.First().XDataPoints.Count();
        double[] w = new double[featureCount]; // Initialize to zeros

        foreach (SvmNumber i in _dataPoints.Where(x => x.Alpha != 0))
        {
            double scale = i.Alpha * i.YLabel;
            double[] trainingPoints = i.XDataPoints.ToArray();
            for (int j = 0; j < featureCount; j++) w[j] += scale * trainingPoints[j];
        }

        return w;
    }

    public void Fit()
    {
        int i = 0;
        SupportVectors = _dataPoints.ToList();
        _dataPoints.ToList().ForEach(x => x.ErrorCache = Error(x));
        while (i < SvmConfig.MAX_ITER)
        {
            i++;
            SvmNumber? heuristic2 = Heuristic2();

            if (heuristic2 is null) break;
            SvmNumber heuristic1 = Heuristic1(heuristic2);

            if (heuristic1 == heuristic2) continue;

            (double L, double H) = ComputeBoundaries(heuristic1, heuristic2);

            if (L == H) continue;
            double eta = CalculateEta(heuristic1, heuristic2);
            if (eta == 0) continue;

            var e1 = Error(heuristic1);
            var e2 = Error(heuristic2);

            double alpha2new = NewAlpha2(heuristic2, e1, e2, eta, H, L);

            double alpha1new = NewAlpha1(heuristic1, heuristic2, alpha2new);

            B = CalculateB(heuristic1, heuristic2, alpha1new, alpha2new, e1, e2);

            heuristic1.Alpha = alpha1new;
            heuristic2.Alpha = alpha2new;
            heuristic1.ErrorCache = Error(heuristic1);
            heuristic2.ErrorCache = Error(heuristic2);
        }

        SupportVectors = _dataPoints.Where(x=>x.Alpha > 0).ToList();
        Logger.Log("svm trained");
    }

    public SvmNumber Heuristic1(SvmNumber alpha1)
    {
        //alpha1 ??= Heuristic2();
        IEnumerable<SvmNumber> nonBound = NonBoundExamples();

        double alphaError = alpha1.ErrorCache;
        if (nonBound.Any())
        {
            if (alphaError >= 0)
                return nonBound
                    .OrderBy(x => x.ErrorCache)
                    .First();

            return nonBound
                .OrderByDescending(x => x.ErrorCache)
                .First();
        }

        return _dataPoints
            .OrderByDescending(x => Math.Abs(x.ErrorCache - alphaError))
            .First();
    }

    public double CalculateEta(SvmNumber x1, SvmNumber x2)
    {
        return Kernel(x1.XDataPoints, x1.XDataPoints)
               + Kernel(x2.XDataPoints, x2.XDataPoints)
               - 2 * Kernel(x1.XDataPoints, x2.XDataPoints);
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
        var list = _dataPoints.Where(x => !x.Optimized);

        foreach (SvmNumber dp in list)
        {
            dp.Optimized = true;
            if (!Check_KKT(dp))
            {
                
                var c = list.Count();
                return dp;
            }
        }

        _dataPoints.ToList().ForEach(x => x.Optimized = Check_KKT(x));

        foreach (SvmNumber dp in _dataPoints.Where(x => !x.Optimized))
        {
            dp.Optimized = true;
            if (!Check_KKT(dp)) return dp;
        }

        return null;
    }


    public IEnumerable<SvmNumber> NonBoundExamples()
    {
        return _dataPoints.Where(x => x.Alpha > 0 && x.Alpha < _svmConfig.C);
    }

    public double Error(SvmNumber svmNumber)
    {
        var error = Predict(svmNumber) - svmNumber.YLabel;
        return error;
    }

    public double NewAlpha1(SvmNumber n1, SvmNumber n2, double alphanew)
    {
        return n1.Alpha + n1.YLabel * n2.YLabel * (n2.Alpha - alphanew);
    }

    public double NewAlpha2(SvmNumber number, double e1, double e2, double eta, double H, double L)
    {
        double alpha2new = number.Alpha + number.YLabel * (e1 - e2) / eta;
        alpha2new = Math.Min(alpha2new, H);
        return Math.Max(alpha2new, L);
    }

    public bool Check_KKT(SvmNumber svmNumber)
    {
        double score = Predict(svmNumber);
        double ro = svmNumber.YLabel * score - 1;
        bool cond1 = (svmNumber.Alpha < _svmConfig.C) && (ro < -_svmConfig.KktThr);
        bool cond2 = (svmNumber.Alpha > 0) && (ro > _svmConfig.KktThr);
        return !(cond1 || cond2);
    }
/*
    public double Predict(IEnumerable<double> inputSvmNumber)
    {
        if (_svmConfig.KernelType == KernelType.Linear)
        {
        }

        return _dataPoints
            .Select(sn => sn.YLabel
                          * sn.Alpha
                          * Kernel(sn.XDataPoints, inputSvmNumber))
            .Sum() + B;
    }*/
    public double Predict2(double[] inputSvmNumber)
    {
        
        double result = B;
        result += ParallelEnumerable.Range(0, SupportVectors.Count)
            .Sum(i => SupportVectors[i].YLabel * SupportVectors[i].Alpha * Kernel(SupportVectors[i].XDataPoints, inputSvmNumber));

        return result;
        return SupportVectors 
            .Select(sn => sn.YLabel
                          * sn.Alpha
                          * Kernel(sn.XDataPoints, inputSvmNumber))
            .Sum() + B;
    }
    public double Predict(SvmNumber inputSvmNumber)
    {
        return Predict2(inputSvmNumber.XDataPoints);
    }


    public double Kernel(double[] xj, double[] x)
    {
        return _svmConfig.KernelType switch
        {
            KernelType.Gaussian => RbfKernel(xj, x, SvmConfig.GAMMA),
            KernelType.Linear => xj.InnerProduct(x)
        };
    }

    public static double RbfKernel(double[] x1, double[] x2, double gamma)
    {
        return RbfSimd(x1, x2, gamma);
        double squaredDistance = 0.0f;
        for (int i = 0; i < x1.Length; i++)
        {
            double diff = x1[i] - x2[i];
            squaredDistance += diff * diff;
        }

        return (double)Math.Exp(-gamma * squaredDistance);
    }
    public static double RbfSimd(double[] xi, double[] xj, double gamma)
    {
        int simdLength = Vector<double>.Count;
        int n = xi.Length;
        int i = 0;
        Vector<double> sumVec = Vector<double>.Zero;

        for (; i <= n - simdLength; i += simdLength)
        {
            var v1 = new Vector<double>(xi, i);
            var v2 = new Vector<double>(xj, i);
            var diff = v1 - v2;
            sumVec += diff * diff;
        }

        double sum = 0;
        for (int k = 0; k < simdLength; k++)
            sum += sumVec[k];
        for (; i < n; i++)
        {
            double diff = xi[i] - xj[i];
            sum += diff * diff;
        }

        return Math.Exp(-gamma * sum);
    }
    public double CalculateB(SvmNumber s1, SvmNumber s2, double alphaNew1, double alphaNew2, double e1, double e2)
    {
        double b1 = B - e1 -
                    s1.YLabel * (alphaNew1 - s1.Alpha) * Kernel(s1.XDataPoints, s1.XDataPoints)
                    - s2.YLabel * (alphaNew2 - s2.Alpha) * Kernel(s1.XDataPoints, s2.XDataPoints);

        double b2 = B - e2 -
                    s1.YLabel * (alphaNew1 - s1.Alpha) * Kernel(s1.XDataPoints, s2.XDataPoints)
                    - s2.YLabel * (alphaNew2 - s2.Alpha) * Kernel(s2.XDataPoints, s2.XDataPoints);

        if (0 < alphaNew1 && alphaNew1 < _svmConfig.C) return b1;

        if (0 < alphaNew2 && alphaNew2 < _svmConfig.C) return b2;

        return (b1 + b2) / 2;
    }
}