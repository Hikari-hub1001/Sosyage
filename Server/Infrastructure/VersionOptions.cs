namespace Server.Infrastructure;

public sealed record VersionOptions
{
    public string AppVersion { get; init; } = "";
    public string AssetVersion { get; init; } = "";
}
