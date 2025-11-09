namespace ClassLibrary1;

public class Logger
{
    private static readonly string LogFile = "log2.txt";

    public static void Log(string message)
    {
        Console.WriteLine(message);
        //File.AppendAllText(LogFile,
        //    $"{message}{Environment.NewLine}");
    }
}