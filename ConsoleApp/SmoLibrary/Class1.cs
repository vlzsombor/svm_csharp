namespace ClassLibrary1;

public enum KernelType
{
    Linear,
    Gaussian
}

public record SvmConfig(double C, double KktThr, KernelType KernelType)
{
    public static SvmConfig GetDefault()
    {
        return new SvmConfig(1.0, 0.001, KernelType.Linear);
    }
}

public class SvmOptimizer
{
    public const int MAX_ITER = 1_000;
    public readonly IEnumerable<SvmNumber> _dataPoints;
    private readonly SvmConfig _svmConfig;

    public SvmOptimizer(IEnumerable<SvmNumber> dataPoints, SvmConfig? svmConfig = null)
    {
        svmConfig ??= SvmConfig.GetDefault();
        _svmConfig = svmConfig;
        _dataPoints = dataPoints;
    }

    public double[] W => WCalculation();
    public double B { get; set; }

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

//        var errorCache = _dataPoints.Select(Error).ToArray();

        //error_cache = self.predict(x_train)[1] - y_train  # auxilary array for heuristics
        _dataPoints.ToList().ForEach(x => x.ErrorCache = Error(x));
        while (i < MAX_ITER)
        {
            SvmNumber? heuristic2 = Heuristic2();

            if (heuristic2 is null) break;
            SvmNumber heuristic1 = Heuristic1(heuristic2);

            if (heuristic1 == heuristic2) continue;

            (double L, double H) = ComputeBoundaries(heuristic1, heuristic2);

            if (L == H) continue;

            double eta = CalculateEta(heuristic1, heuristic2);

            if (eta == 0) continue;

//            double e1 = Error(heuristic1);
            //           double e2 = Error(heuristic2);


            var e1 = Error(heuristic1);
            var e2 = Error(heuristic2);

            double alpha2new = NewAlpha2(heuristic2, e1, e2, eta, H, L);

            double alpha1new = NewAlpha1(heuristic1, heuristic2, alpha2new);

            B = CalculateB(heuristic1, heuristic2, alpha1new, alpha2new, e1, e2);

            heuristic1.Alpha = alpha1new;
            heuristic2.Alpha = alpha2new;
            heuristic1.ErrorCache = Error(heuristic1);
            heuristic2.ErrorCache = Error(heuristic2);


            Logger.Log(i.ToString());
            Logger.Log("\t W " + string.Join(" ", W));
            Logger.Log("\t B " + B);
            var ec = _dataPoints.Select(x => x.ErrorCache).ToArray();
            i++;
        }

        List<SvmNumber> res = _dataPoints.Where(x => x.Alpha is > 0 and < 1).ToList();
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
                    //.ThenByDescending(x=>x.i)
                    .First();

            return nonBound
                .OrderByDescending(x => x.ErrorCache)
                //.ThenByDescending(x=>x.i)
                .First();
        }


//        List<double> errorCache = _dataPoints.Select(Error).ToList();
//        IEnumerable<double> values = _dataPoints.Select(x => Math.Abs(Error(x) - alphaError));
        return _dataPoints
            .OrderByDescending(x => Math.Abs(x.ErrorCache - alphaError))
            //.ThenByDescending(x=>x.i)
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

//        //var firstOrDefault = _dataPoints.FirstOrDefault(x => !x.Optimized && !Check_KKT(x) );
//        if (firstOrDefault is not null)
//        {
//            firstOrDefault.Optimized = true;
//            return firstOrDefault;
//        }
        _dataPoints.ToList().ForEach(x => x.Optimized = Check_KKT(x));

        foreach (SvmNumber dp in _dataPoints.Where(x => !x.Optimized))
        {
            dp.Optimized = true;
            if (Check_KKT(dp)) return dp;
        }

        return null;
    }


    public IEnumerable<SvmNumber> NonBoundExamples()
    {
        return _dataPoints.Where(x => x.Alpha > 0 && x.Alpha < _svmConfig.C);
    }

    public double Error(SvmNumber svmNumber)
    {
        return Predict(svmNumber) - svmNumber.YLabel;
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
        //return RbfKernel(xj.ToArray(), x.ToArray(), 0.5);
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

    public double CalculateB(SvmNumber s1, SvmNumber s2, double alphaNew1, double alphaNew2, double e1, double e2)
    {
        double b1 = B - e1 -
                    s1.YLabel * (alphaNew1 - s1.Alpha) * LinearKernel(s1.XDataPoints, s1.XDataPoints)
                    - s2.YLabel * (alphaNew2 - s2.Alpha) * LinearKernel(s1.XDataPoints, s2.XDataPoints);

        double b2 = B - e2 -
                    s1.YLabel * (alphaNew1 - s1.Alpha) * LinearKernel(s1.XDataPoints, s2.XDataPoints)
                    - s2.YLabel * (alphaNew2 - s2.Alpha) * LinearKernel(s2.XDataPoints, s2.XDataPoints);

        if (0 < alphaNew1 && alphaNew1 < _svmConfig.C) return b1;

        if (0 < alphaNew2 && alphaNew2 < _svmConfig.C) return b2;

        return (b1 + b2) / 2;
    }
}

public record SvmNumber(int i, IEnumerable<double> XDataPoints, double YLabel, double Alpha)
{
    
    public double Alpha { get; set; } = Alpha;
    public bool Optimized { get; set; }
    public double ErrorCache { get; set; }
}