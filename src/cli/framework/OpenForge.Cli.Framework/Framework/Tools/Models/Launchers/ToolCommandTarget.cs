namespace OpenForge.Cli.Core.Framework.Tools.Models.Launchers;

// Setup and probes use a real executable and argument prefix, including on Windows.
// A .cmd launcher is a host entry, not the executable of a shell-free setup process.
internal sealed record ToolCommandTarget
{
    public required string ExecutablePath { get; init; }
    public required IReadOnlyList<string> PrefixArguments { get; init; }
}
