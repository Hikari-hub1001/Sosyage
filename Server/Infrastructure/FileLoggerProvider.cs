using System.Text;
using Microsoft.Extensions.Logging;

namespace Server.Infrastructure;

public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly FileLoggerOptions _options;
    private readonly object _lock = new();
    private StreamWriter? _writer;

    public FileLoggerProvider(FileLoggerOptions options)
    {
        _options = options;
        EnsureWriter();
    }

    public ILogger CreateLogger(string categoryName)
    {
        EnsureWriter();
        return new FileLogger(categoryName, WriteLine);
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _writer?.Dispose();
            _writer = null;
        }
    }

    private void EnsureWriter()
    {
        if (string.IsNullOrWhiteSpace(_options.FilePath))
        {
            return;
        }

        lock (_lock)
        {
            if (_writer is not null)
            {
                return;
            }

            var directory = Path.GetDirectoryName(_options.FilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var stream = new FileStream(_options.FilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            _writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };
        }
    }

    private void WriteLine(string message)
    {
        if (string.IsNullOrWhiteSpace(_options.FilePath))
        {
            return;
        }

        lock (_lock)
        {
            _writer?.WriteLine(message);
        }
    }
}
