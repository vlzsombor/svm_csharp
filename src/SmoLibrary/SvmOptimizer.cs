using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Principal;

namespace ClassLibrary1;

public class SvmOptimizer
{
//    private IEnumerable<SvmNumber> _dataPoints;

    private readonly SvmConfig _svmConfig;
    public string LabelToIdentify { get; }
    private readonly SvmNumberSoa _svmNumberSoa;

    public SvmOptimizer(SvmNumberSoa svmNumberSoa, SvmConfig config, string labelToIdentify)
    {
        _svmNumberSoa = svmNumberSoa;
        _svmConfig = config;
        LabelToIdentify = labelToIdentify;
        _convertedLabel = new int[svmNumberSoa.SvmData.YLabels.Length];
        for (int i = 0; i < svmNumberSoa.SvmData.YLabels.Length; i++)
        {
            _convertedLabel[i] = Runner.LabelFilter(svmNumberSoa.SvmData.YLabels[i], config) == LabelToIdentify ? 1 : -1;
        }
    }

    public SvmOptimizer()
    {
        
    }
    
    public double B { get; set; }
    public SvmNumberSoa SupportVectors { get; set; }

    public readonly double Tolarance = 0.01;
    private readonly int[] _convertedLabel;

    public void Fit()
    {
        SupportVectors = _svmNumberSoa;
        _svmNumberSoa.UpdateErrorCache(Error);
        for (int itrationIndex = 0; itrationIndex < _svmConfig.MaxIter; itrationIndex++)
        {
            int? h2 = Heuristic2();
            if (!h2.HasValue) break;
            int heuristic2 = h2.Value;
            var heuristic1 = Heuristic1(heuristic2);

            if (heuristic1 == heuristic2) continue;

            (double L, double H) = ComputeBoundaries(heuristic1, heuristic2);
            if (Math.Abs(L - H) < Tolarance) continue;
            double eta = CalculateEta(heuristic1, heuristic2);
            if (eta == 0) continue;

            var e1 = Error(heuristic1);
            var e2 = Error(heuristic2);

            double alpha2New = NewAlpha2(heuristic2, e1, e2, eta, H, L);

            double alpha1New = NewAlpha1(heuristic1, heuristic2, alpha2New);

            B = CalculateB(heuristic1, heuristic2, alpha1New, alpha2New, e1, e2);

            _svmNumberSoa.Alpha[heuristic1] = alpha1New;
            _svmNumberSoa.Alpha[heuristic2] = alpha2New;

            _svmNumberSoa.ErrorCache[heuristic1] = Error(heuristic1);
            _svmNumberSoa.ErrorCache[heuristic2] = Error(heuristic2);
//            Logger.Log($"B: {B}");
        }
        var ints = _svmNumberSoa.Alpha.Where((x, i)=>x > 0).Select((x,i) => i).ToArray();
        SupportVectors = _svmNumberSoa.SetSupportVector();
        Logger.Log("svm trained");
    }

    public int Heuristic1(int alpha1Index)
    {
        //alpha1 ??= Heuristic2();
        var nonBound = NonBoundExamples().ToArray();

        double alphaError = _svmNumberSoa.ErrorCache[alpha1Index];
        if (nonBound.Any())
        {
            if (alphaError >= 0)
                return nonBound
                    .OrderBy(x => _svmNumberSoa.ErrorCache[x])
                    .First();
            return nonBound
                .OrderByDescending(i=> _svmNumberSoa.ErrorCache[i])
                .First();
        }

        double max = -1;
        int maxIndex = 0;
        for (int i = 0; i < _svmNumberSoa.Length; i++)
        {
            var a = _svmNumberSoa.ErrorCache[i];
            var aa = Math.Abs(a - alphaError);
            if (!(aa > max)) continue;
            max = aa;
            maxIndex = i;
        }

//        return Doubles2(x=>(Math.Abs(_dataPoints.ErrorCache[x] - alphaError), x)).OrderByDescending(x=>x.Item1).First().x;
        return maxIndex;
    }

    public double CalculateEta(int index1, int index2)
    {
        var dp1 = _svmNumberSoa.SvmData.XDataPoints[index1];
        var dp2 = _svmNumberSoa.SvmData.XDataPoints[index2];
        return Kernel(dp1, dp1)
               + Kernel(dp2, dp2)
               - 2 * Kernel(dp1, dp2);
    }

    public (double lb, double ub) ComputeBoundaries(int alpha1Index, int alpha2Index)
    {
        Func<double> lbF = () => _svmNumberSoa.Alpha[alpha1Index] + _svmNumberSoa.Alpha[alpha2Index] - _svmConfig.C;
        Func<double> ubF = () => _svmNumberSoa.Alpha[alpha1Index] + _svmNumberSoa.Alpha[alpha2Index];
        if (Math.Abs(_convertedLabel[alpha1Index] - _convertedLabel[alpha2Index]) > Tolarance)
        {
            lbF = () => _svmNumberSoa.Alpha[alpha2Index] - _svmNumberSoa.Alpha[alpha1Index];
            ubF = () => _svmConfig.C + _svmNumberSoa.Alpha[alpha2Index] - _svmNumberSoa.Alpha[alpha1Index];
        }
        return (lb: Math.Max(0, lbF()), ub: Math.Min(_svmConfig.C, ubF()));
    }

    public int? Heuristic2()
    {
//        var list = _dataPoints.Where(x => !x.Optimized);
        for (int i = 0; i < _svmNumberSoa.Length; i++)
        {
            if (_svmNumberSoa.Optimized[i])
            {
                continue;
            }
            _svmNumberSoa.Optimized[i] = true;
            if (!Check_KKT(i))
            {
                return i;
            }
        }

        for (int i = 0; i < _svmNumberSoa.Length; i++)
        {
            _svmNumberSoa.Optimized[i] = false;
        }
//        _svmNumberSoa.UpdateErrorCache(Error);

        for (int i = 0; i < _svmNumberSoa.Length; i++)
        {
            if (_svmNumberSoa.Optimized[i])
            {
                continue;
            }
            _svmNumberSoa.Optimized[i] = true;
            if (!Check_KKT(i)) return i;
        }
        return null;
    }


    public IEnumerable<T> Doubles2<T>(Func<int, T> predicate)
    {
        for (int i = 0; i < _svmNumberSoa.Length ; i++)
        {
            yield return predicate(i);
        }
    }
    public IEnumerable<int> Doubles(Predicate<int> predicate)
    {
        for (int i = 0; i < _svmNumberSoa.Length ; i++)
        {
            if (predicate(i))
            {
                yield return i;
            }
        }
    }
    public IEnumerable<int> NonBoundExamples()
    {
        return Doubles(x => _svmNumberSoa.Alpha[x] > 0 && _svmNumberSoa.Alpha[x] < _svmConfig.C);
    }

    public double Error(int i)
    {
        var error = Predict(_svmNumberSoa.SvmData.XDataPoints[i]) - _convertedLabel[i];
        return error;
    }

    public double NewAlpha1(int index1, int index2, double alphanew)
    {
        return _svmNumberSoa.Alpha[index1] + _convertedLabel[index1] * _convertedLabel[index2] * (_svmNumberSoa.Alpha[index2] - alphanew);
    }

    public double NewAlpha2(int index, double e1, double e2, double eta, double H, double L)
    {
        double alpha2New = _svmNumberSoa.Alpha[index] + _convertedLabel[index] * (e1 - e2) / eta;
        alpha2New = Math.Min(alpha2New, H);
        return Math.Max(alpha2New, L);
    }

    public bool Check_KKT(int i)
    {
        double score = Predict(_svmNumberSoa.SvmData.XDataPoints[i]);
        double ro = -_convertedLabel[i] * score - 1;
        bool cond1 = (_svmNumberSoa.Alpha[i] < _svmConfig.C) && (ro < -_svmConfig.KktThr);
        bool cond2 = (_svmNumberSoa.Alpha[i] > 0) && (ro > _svmConfig.KktThr);
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
    public double Predict(double[] inputSvmNumber)
    {
        double result = B;
        result += ParallelEnumerable.Range(0, SupportVectors.Length)
            .Sum(i => _convertedLabel[i] * SupportVectors.Alpha[i] * Kernel(SupportVectors. XDataPoints[i], inputSvmNumber));

        return result;
    }


    public double Kernel(double[] xj, double[] x)
    {
        return _svmConfig.KernelType switch
        {
            KernelType.Gaussian => xj.RbfKernel(x, _svmConfig.Gamma),
            KernelType.Linear => xj.InnerProduct(x)
        };
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
    public double CalculateB(int index1, int index2, double alphaNew1, double alphaNew2, double e1, double e2)
    {
        double b1 = B - e1 -
                    _convertedLabel[index1] * (alphaNew1 - _svmNumberSoa.Alpha[index1]) * Kernel(_svmNumberSoa.XDataPoints[index1], _svmNumberSoa.XDataPoints[index1])
                    - _convertedLabel[index2] * (alphaNew2 - _svmNumberSoa.Alpha[index2]) * Kernel(_svmNumberSoa.XDataPoints[index1], _svmNumberSoa.XDataPoints[index2]);

        double b2 = B - e2 -
                    _convertedLabel[index1] * (alphaNew1 - _svmNumberSoa.Alpha[index1]) * Kernel(_svmNumberSoa.XDataPoints[index1], _svmNumberSoa.XDataPoints[index2])
                    - _convertedLabel[index2] * (alphaNew2 - _svmNumberSoa.Alpha[index2]) * Kernel(_svmNumberSoa.XDataPoints[index2], _svmNumberSoa.XDataPoints[index2]);

        if (0 < alphaNew1 && alphaNew1 < _svmConfig.C) return b1;

        if (0 < alphaNew2 && alphaNew2 < _svmConfig.C) return b2;

        return (b1 + b2) / 2;
    }
}