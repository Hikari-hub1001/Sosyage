using Microsoft.Extensions.Logging;

namespace Server.Infrastructure;

public sealed class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly Action<string> _writeLine;

    public FileLogger(string categoryName, Action<string> writeLine)
    {
        _categoryName = categoryName;
        _writeLine = writeLine;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (exception is not null)
        {
            message = $"{message} {exception}";
        }

        var line = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss} {logLevel}: {_categoryName}[{eventId.Id}] {message}";
        _writeLine(line);
    }
}
