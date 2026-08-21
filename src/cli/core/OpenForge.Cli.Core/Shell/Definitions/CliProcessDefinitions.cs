namespace OpenForge.Cli.Core.Shell.Definitions;

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

internal sealed record CliNextAction
{
    internal CliNextAction(string command, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        Command = command;
        Reason = reason;
    }

    internal string Command { get; }

    internal string Reason { get; }
}

internal sealed record CliProcessCompletion(
    CliSemanticStatus Status,
    int ExitCode,
    CliOutputTarget PrimaryOutputTarget);

internal static class CliProcessCompletionPolicy
{
    internal static CliProcessCompletion Complete(CliSemanticStatus status, CliOutputTarget primaryOutputTarget)
    {
        var definition = CliStatusDefinitions.Read(status);
        CliPresentationDefinitions.ValidateTarget(primaryOutputTarget);
        return new CliProcessCompletion(status, definition.Disposition.ExitCode, primaryOutputTarget);
    }
}
