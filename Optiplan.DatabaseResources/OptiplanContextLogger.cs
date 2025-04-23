using System.Text;

namespace Optiplan.DatabaseResources;

public class OptiplanContextLogger
{
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private static readonly string _logFilePath = Path.Combine("Optiplan.DatabaseResources", "Logs", "Optiplan.db.log");
    private static readonly int _streamBufferSize = 4096;

    public static async Task WriteLineAsync(string message)
    {

        string resolvedPath = OptiplanContextExtensions.ResolveFilePath(_logFilePath);

        try
        {
            await _semaphore.WaitAsync();
            Directory.CreateDirectory(Path.GetDirectoryName(resolvedPath)!);
            string logEntry = $"{DateTime.Now:O} - {message}{Environment.NewLine}";
            byte[] encodedText = Encoding.UTF8.GetBytes(logEntry);
            using FileStream fileStream = new FileStream(
                resolvedPath, FileMode.Append, FileAccess.Write, FileShare.Read, _streamBufferSize, true
            );
            await fileStream.WriteAsync(encodedText, 0, encodedText.Length);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // Synchronous wrapper
    public static void WriteLine(string message)
    {
        _ = WriteLineAsync(message);
    }

}