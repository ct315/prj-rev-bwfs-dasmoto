using System.Text;

namespace NotebookLmSender.Host;

internal sealed class HostLogger
{
    private readonly string _logPath;
    private readonly bool _debug;

    public HostLogger(bool debug)
    {
        _debug = debug;
        var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NotebookLmUrlSender", "logs");
        Directory.CreateDirectory(logDir);
        _logPath = Path.Combine(logDir, $"host-{DateTime.UtcNow:yyyyMMdd}.log");
    }

    public void Info(string message) => Write("INFO", message);
    public void Error(string message) => Write("ERROR", message);
    public void Debug(string message)
    {
        if (_debug)
        {
            Write("DEBUG", message);
        }
    }

    private void Write(string level, string message)
    {
        var line = $"{DateTime.UtcNow:O} [{level}] {Sanitize(message)}{Environment.NewLine}";
        File.AppendAllText(_logPath, line, Encoding.UTF8);
    }

    private static string Sanitize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        return text.Replace("\r", " ").Replace("\n", " ");
    }
}
