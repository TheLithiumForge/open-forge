namespace OpenForge.Cli.Core.Shell.Invocation.Models;

internal sealed record CliProcessIdentity
{
    internal CliProcessIdentity(string executableName, string informationalVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(informationalVersion);
        ExecutableName = executableName;
        InformationalVersion = informationalVersion;
    }

    internal string ExecutableName { get; }

    internal string InformationalVersion { get; }
}
