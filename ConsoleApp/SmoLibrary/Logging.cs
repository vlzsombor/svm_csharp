namespace ClassLibrary1;

public class Logger
{
    private static readonly string LogFile = $"log2.txt";
    private static readonly string LogDirectory = $"{Runner.MNT_PATH}/log";

    public static void Log(string message)
    {
        if (!File.Exists(LogFile)) Directory.CreateDirectory($"{LogDirectory}");
        Console.WriteLine(message);
//        File.AppendAllText(Path.Combine(LogDirectory, LogFile), $"{message}{Environment.NewLine}");
    }
}