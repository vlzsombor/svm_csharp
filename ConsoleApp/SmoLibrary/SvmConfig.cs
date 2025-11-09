namespace ClassLibrary1;

public record SvmConfig(double C, double KktThr, KernelType KernelType)
{
    
    public const int MAX_ITER = 1000;
    public static SvmConfig GetDefault()
    {
        return new SvmConfig(1.0, 0.001, KernelType.Gaussian);
    }
}