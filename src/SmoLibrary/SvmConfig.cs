namespace ClassLibrary1;

public record SvmConfig(double C, double KktThr, KernelType KernelType)
{
    public const int MAX_ITER = 10_000;
    public const double GAMMA = 1.0/784;
    public static SvmConfig GetDefault(KernelType kernelType = KernelType.Linear)
    {
        return new SvmConfig(1.0, 0.001, kernelType);
    }
}    