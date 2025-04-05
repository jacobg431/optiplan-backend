using static System.Environment;

namespace Optiplan.DatabaseResources;

public class OptiplanContextLogger
{
    public static void WriteLine(string message)
    {
        string logFileName = "Optiplan.db.log";
        string path = Path.Combine(GetFolderPath(SpecialFolder.DesktopDirectory), logFileName);
        StreamWriter logFile = File.AppendText(path);
        logFile.WriteLine(message);
        logFile.Close();
    }
}