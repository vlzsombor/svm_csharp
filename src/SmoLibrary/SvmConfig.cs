namespace ClassLibrary1;

public record SvmConfig(double C, double KktThr, KernelType KernelType, int MaxIter, string[] labelsToIdentify)
{
    public const double GAMMA = 1.0/784;
    public static SvmConfig GetDefault(string[] labelsToIdentify, KernelType kernelType = KernelType.Linear) 
    {
        return new SvmConfig(1.0, 0.001, kernelType, 1000, labelsToIdentify);
    }
}    