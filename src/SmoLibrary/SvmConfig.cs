namespace ClassLibrary1;

public record SvmConfig(double C, double KktThr, KernelType KernelType, int MaxIter, string[] LabelsToIdentify, double Gamma, double Tolarance = 0.01)
{

    public static SvmConfig GetDefault(string[] labelsToIdentify, double gamma, KernelType kernelType = KernelType.Linear) 
    {
        return new SvmConfig(1.0, 0.001, kernelType, 1000, labelsToIdentify, gamma);
    }
}    