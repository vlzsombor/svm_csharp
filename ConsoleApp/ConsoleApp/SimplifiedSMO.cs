namespace SVM;

class SimplifiedSMO
{
    public double[] ALPHAs;
    private double[][] INPUTVALUES;
    private int[] LABELS;
    public double b;
    private double tol = 1e-10;
    private int Max_Pass = 100;
    private double C = 1;
    KernelType Tp;
    public SimplifiedSMO(double[][] input_values, int[] labels,double C,KernelType Kerneltype)
    {
        this.INPUTVALUES = input_values;
        this.ALPHAs = new double[input_values.Length];
        Tp = Kerneltype;
        this.LABELS = labels;
        b = 0;
        int passes = 0;
        this.C = C;
        Random rdn = new Random();
            
        while (passes < Max_Pass)
        {
            int num_changed_alpha = 0;
            for (int i = 0; i < INPUTVALUES.Length; i++)
            {
                double Ei =E_function(INPUTVALUES[i],i);
                if ((Ei * LABELS[i] < -tol && ALPHAs[i] < C) || (Ei * LABELS[i] > tol && ALPHAs[i] > 0))
                {
                    int j = 0;                        
                    j = rdn.Next(0, INPUTVALUES.Length - 1);
                    //Console.WriteLine(j);
                    while (j == i)j=rdn.Next(0, INPUTVALUES.Length - 1);
                    double Ej = E_function(INPUTVALUES[j],j);
                    //Console.WriteLine("Ej:{0}", Ej);
                    double ai = ALPHAs[i], aj = ALPHAs[j];
                    double L, H;
                    if (LABELS[i] != LABELS[j])
                    {
                        L = 0 > aj - ai ? 0 : aj - ai;
                        H = C < C + aj - ai ? C : C + aj - ai;
                    }
                    else
                    {
                        L = 0 > ai + aj - C ? 0 : ai + aj - C;
                        H = C < ai + aj ? C : ai + aj;
                    }
                    if (L == H)
                    {
                        //next i

                    }
                    else
                    {
                        double tau = 2 * InnerProduct(INPUTVALUES[i], INPUTVALUES[j]) - InnerProduct(INPUTVALUES[i], INPUTVALUES[i]) - InnerProduct(INPUTVALUES[j], INPUTVALUES[j]);
                        if (tau >= 0)
                        {
                            //next i
                        }
                        else
                        {
                            ALPHAs[j] = ALPHAs[j]-(E_function(INPUTVALUES[i],i) - E_function(INPUTVALUES[j], j)) *LABELS[j] / tau;
                            if (ALPHAs[j] > H) ALPHAs[j] = H;
                            else if (ALPHAs[j] < H && ALPHAs[j] > L) { }
                            else ALPHAs[j] = L;
                            if (Math.Abs(ALPHAs[j] - aj) < tol)
                            {
                                //next i
                            }
                            else
                            {
                                ALPHAs[i] = ALPHAs[i] + LABELS[i] * LABELS[j] * (aj - ALPHAs[j]);
                                double b1 = b - E_function(INPUTVALUES[i], i) - LABELS[i] * ( ALPHAs[i]-ai) * InnerProduct(INPUTVALUES[i], INPUTVALUES[i]) - LABELS[j] * ( ALPHAs[j]-aj) * InnerProduct(INPUTVALUES[i], INPUTVALUES[j]);
                                double b2 = b - E_function(INPUTVALUES[j], j) - LABELS[i] * (ALPHAs[i]-ai) * InnerProduct(INPUTVALUES[i], INPUTVALUES[j]) - LABELS[j] * (ALPHAs[j]-aj) * InnerProduct(INPUTVALUES[j], INPUTVALUES[j]);
                                if (ALPHAs[i] > 0 && ALPHAs[i] < C) b = b1;
                                else if (ALPHAs[j] > 0 && ALPHAs[j] < C) b = b2;
                                else b = (b1 + b2) / 2;
                                num_changed_alpha += 1;
                            }
                        }
                    }
                }
            }
            if (num_changed_alpha == 0)
            {
                passes += 1;
                // Console.WriteLine("PASSES:{0}", passes);
            }
            else passes = 0;
        }
    }
    private static double InnerProduct(double[] u, double[] v)
    {
        double res = 0.0;
        for (int i = 0; i < u.Length; i++)
        {
            res += u[i] * v[i];
        }
        return res;
    }
    private static double LinearKernel(double[] u, double[] v)
    {
        return InnerProduct(u, v);
    }
    private static double RBF_Kernel(double[] u, double[] v,double Theta)
    {
        double res = 0.0;
        for (int i = 0; i < u.Length; i++)
        {
            res += (u[i] - v[i]) * (u[i] - v[i]);
        }
        return Math.Pow(Math.E, -res * Theta);
    }
    private  double E_function(double[]RowEle,int index_Label)
    {
        double res = 0.0;
        for (int i = 0; i < ALPHAs.Length; i++)
        {
            if (Tp is KernelType.InnerProduct or KernelType.Linear)
            {
                res += ALPHAs[i] * LABELS[i] * LinearKernel(INPUTVALUES[i], RowEle);
            }
            else if (Tp == KernelType.RBF)
            {
                res += ALPHAs[i] * LABELS[i] *RBF_Kernel(INPUTVALUES[i], RowEle,0.5);
            }
        }
        return res +b -LABELS[index_Label];
    }
}