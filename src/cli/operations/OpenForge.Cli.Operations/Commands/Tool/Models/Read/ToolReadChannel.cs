namespace OpenForge.Cli.Core.Commands.Tool.Models.Read;

internal sealed record ToolReadChannel
{
    public required string Id { get; init; }
    public required string Kind { get; init; }
    public string? Package { get; init; }
    public string? Repository { get; init; }
    public string? RequiredClient { get; init; }
    public required IReadOnlyDictionary<string, string> MinimumClientVersions { get; init; }
    public string? Runtime { get; init; }
    public string? RuntimeRange { get; init; }
    public required IReadOnlyList<ToolReadTarget> Targets { get; init; }
    public IReadOnlyList<ToolReadAsset> Assets { get; init; } = [];
}
internal sealed record ToolReadTarget(string OperatingSystem, string Cpu, string? LinuxLibc, string? MinimumOperatingSystemVersion, string? MinimumLibcVersion, string? MinimumLinuxKernelVersion, string? MinimumLibStdCppVersion);
internal sealed record ToolReadAsset(string Version, ToolReadTarget Target, ToolReadBundledRuntime? BundledRuntime);
internal sealed record ToolReadBundledRuntime(string Runtime, string Version, string RuntimeRelativePath, string EntryRelativePath, IReadOnlyList<string> Arguments);
internal sealed record ToolReadManager(string Client, string Version, string? RuntimePath, string? RuntimeVersion, string? GlobalBinPath, string GlobalBinReadiness, bool SupportsGlobalInstall);
