namespace Server.Infrastructure;

public sealed record FileLoggerOptions
{
    public string FilePath { get; init; } = "";
}
