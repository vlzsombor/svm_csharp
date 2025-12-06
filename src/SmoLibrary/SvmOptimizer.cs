using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Principal;

namespace ClassLibrary1;

public class SvmOptimizer
{
    public SvmConfig _svmConfig { get; set; }
    public string LabelToIdentify { get; init; }
    public double B { get; set; }
    public SvmNumberSoa SupportVectors { get; set; } 
    
    public SvmOptimizer(SvmNumberSoa svmNumberSoa, SvmConfig config, string labelToIdentify)
    {
        SupportVectors = svmNumberSoa;
        _svmConfig = config;
        LabelToIdentify = labelToIdentify;
    }

    public SvmOptimizer()
    {
        
    }

    public void Fit()
    {
        SupportVectors.UpdateErrorCache(Error);
        SupportVectors.SetLabel(LabelToIdentify);
        for (int itrationIndex = 0; itrationIndex < _svmConfig.MaxIter; itrationIndex++)
        {
            int? h2 = Heuristic2();
            if (!h2.HasValue) break;
            int heuristic2 = h2.Value;
            var heuristic1 = Heuristic1(heuristic2);

            if (heuristic1 == heuristic2) continue;

            (double L, double H) = ComputeBoundaries(heuristic1, heuristic2);
            if (Math.Abs(L - H) < _svmConfig.Tolarance) continue;
            double eta = CalculateEta(heuristic1, heuristic2);
            if (eta == 0) continue;

            var e1 = Error(heuristic1);
            var e2 = Error(heuristic2);

            double alpha2New = NewAlpha2(heuristic2, e1, e2, eta, H, L);

            double alpha1New = NewAlpha1(heuristic1, heuristic2, alpha2New);

            B = CalculateB(heuristic1, heuristic2, alpha1New, alpha2New, e1, e2);

            SupportVectors.Alpha[heuristic1] = alpha1New;
            SupportVectors.Alpha[heuristic2] = alpha2New;

            SupportVectors.ErrorCache[heuristic1] = Error(heuristic1);
            SupportVectors.ErrorCache[heuristic2] = Error(heuristic2);
            if (itrationIndex % 1000 == 0)
            {
                Logger.Log(LabelToIdentify + " " + itrationIndex);
            }
        }
        SupportVectors = SvmNumberSoa.Clone(SupportVectors, SupportVectors.Alpha.Select((x,i)=> (x,i)).Where(x=>x.x>0).Select(x=> x.i).ToArray());
        SupportVectors.SetLabel(LabelToIdentify);
        Logger.Log("B: " + this.B + " Supportvectors: " + this.SupportVectors.Alpha.Count(x=>x > 0));
        Logger.Log("svm trained");
    }

    public int Heuristic1(int alpha1Index)
    {
        //alpha1 ??= Heuristic2();
        var nonBound = NonBoundExamples();

        double alphaError = SupportVectors.ErrorCache[alpha1Index];
        if (nonBound.Any())
        {
            if (alphaError >= 0)
                return nonBound
                    .OrderBy(x => SupportVectors.ErrorCache[x])
                    .First();
            return nonBound
                .OrderByDescending(i=> SupportVectors.ErrorCache[i])
                .First();
        }

        double max = -1;
        int maxIndex = 0;
        for (int i = 0; i < SupportVectors.Length; i++)
        {
            var a = SupportVectors.ErrorCache[i];
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
        var dp1 = SupportVectors.SvmData.XDataPoints[index1];
        var dp2 = SupportVectors.SvmData.XDataPoints[index2];
        return Kernel(dp1, dp1)
               + Kernel(dp2, dp2)
               - 2 * Kernel(dp1, dp2);
    }

    public (double lb, double ub) ComputeBoundaries(int alpha1Index, int alpha2Index)
    {
        Func<double> lbF = () => SupportVectors.Alpha[alpha1Index] + SupportVectors.Alpha[alpha2Index] - _svmConfig.C;
        Func<double> ubF = () => SupportVectors.Alpha[alpha1Index] + SupportVectors.Alpha[alpha2Index];
        if (Math.Abs(SupportVectors.LabelConvert[alpha1Index] - SupportVectors.LabelConvert[alpha2Index]) > _svmConfig.Tolarance)
        {
            lbF = () => SupportVectors.Alpha[alpha2Index] - SupportVectors.Alpha[alpha1Index];
            ubF = () => _svmConfig.C + SupportVectors.Alpha[alpha2Index] - SupportVectors.Alpha[alpha1Index];
        }
        return (lb: Math.Max(0, lbF()), ub: Math.Min(_svmConfig.C, ubF()));
    }

    public int? Heuristic2()
    {
//        var list = _dataPoints.Where(x => !x.Optimized);
        for (int i = 0; i < SupportVectors.Length; i++)
        {
            if (SupportVectors.Optimized[i])
            {
                continue;
            }
            SupportVectors.Optimized[i] = true;
            if (!Check_KKT(i))
            {
                return i;
            }
        }

        for (int i = 0; i < SupportVectors.Length; i++)
        {
            SupportVectors.Optimized[i] = false;
        }
//        _svmNumberSoa.UpdateErrorCache(Error);

        for (int i = 0; i < SupportVectors.Length; i++)
        {
            if (SupportVectors.Optimized[i])
            {
                continue;
            }
            SupportVectors.Optimized[i] = true;
            if (!Check_KKT(i)) return i;
        }
        return null;
    }


    public IEnumerable<T> Doubles2<T>(Func<int, T> predicate)
    {
        for (int i = 0; i < SupportVectors.Length ; i++)
        {
            yield return predicate(i);
        }
    }
    public IEnumerable<int> Doubles(Predicate<int> predicate)
    {
        for (int i = 0; i < SupportVectors.Length ; i++)
        {
            if (predicate(i))
            {
                yield return i;
            }
        }
    }
    public IEnumerable<int> NonBoundExamples()
    {
        return Doubles(x => SupportVectors.Alpha[x] > 0 && SupportVectors.Alpha[x] < _svmConfig.C);
    }

    public double Error(int i)
    {
        var error = Predict(SupportVectors.SvmData.XDataPoints[i]) - SupportVectors.LabelConvert[i];
        return error;
    }

    public double NewAlpha1(int index1, int index2, double alphanew)
    {
        return SupportVectors.Alpha[index1] + SupportVectors.LabelConvert[index1] * SupportVectors.LabelConvert[index2] * (SupportVectors.Alpha[index2] - alphanew);
    }

    public double NewAlpha2(int index, double e1, double e2, double eta, double H, double L)
    {
        double alpha2New = SupportVectors.Alpha[index] + SupportVectors.LabelConvert[index] * (e1 - e2) / eta;
        alpha2New = Math.Min(alpha2New, H);
        return Math.Max(alpha2New, L);
    }

    public bool Check_KKT(int i)
    {
        double score = Predict(SupportVectors.SvmData.XDataPoints[i]);
        double ro = -SupportVectors.LabelConvert[i] * score - 1;
        bool cond1 = (SupportVectors.Alpha[i] < _svmConfig.C) && (ro < -_svmConfig.KktThr);
        bool cond2 = (SupportVectors.Alpha[i] > 0) && (ro > _svmConfig.KktThr);
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
        return B + Enumerable.Range(0, SupportVectors.Length) 
            .Select( i => SupportVectors.LabelConvert[i] * SupportVectors.Alpha[i] * Kernel(SupportVectors.SvmData.XDataPoints[i], inputSvmNumber))
            .AsParallel()
            .Sum();       
    }

    public sbyte ConvertLabel2(int i)
    {
        return (sbyte)(SupportVectors.SvmData.YLabels[i] == LabelToIdentify ? 1 : -1);
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
                    SupportVectors.LabelConvert[index1] * (alphaNew1 - SupportVectors.Alpha[index1]) * Kernel(SupportVectors.SvmData.XDataPoints[index1], SupportVectors.SvmData.XDataPoints[index1])
                    - SupportVectors.LabelConvert[index2] * (alphaNew2 - SupportVectors.Alpha[index2]) * Kernel(SupportVectors.SvmData.XDataPoints[index1], SupportVectors.SvmData.XDataPoints[index2]);

        double b2 = B - e2 -
                    SupportVectors.LabelConvert[index1] * (alphaNew1 - SupportVectors.Alpha[index1]) * Kernel(SupportVectors.SvmData.XDataPoints[index1], SupportVectors.SvmData.XDataPoints[index2])
                    - SupportVectors.LabelConvert[index2] * (alphaNew2 - SupportVectors.Alpha[index2]) * Kernel(SupportVectors.SvmData.XDataPoints[index2], SupportVectors.SvmData.XDataPoints[index2]);

        if (0 < alphaNew1 && alphaNew1 < _svmConfig.C) return b1;

        if (0 < alphaNew2 && alphaNew2 < _svmConfig.C) return b2;

        return (b1 + b2) / 2;
    }
}