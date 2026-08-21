namespace OpenForge.Cli.Core.Shell.Definitions;

internal enum CliSemanticStatus
{
    Complete,
    Failed,
    Attention,
    Incomplete,
    Invalid,
    Blocked,
    Interrupted,
}

internal sealed record CliProcessDisposition(
    int ExitCode,
    CliOutputTarget HumanOutputTarget);

internal sealed record CliStatusDefinition(
    CliSemanticStatus Status,
    string MachineName,
    CliProcessDisposition Disposition);

internal static class CliStatusDefinitions
{
    private static readonly CliStatusDefinition[] Definitions =
    [
        new(CliSemanticStatus.Complete, "complete", new CliProcessDisposition(0, CliOutputTarget.StandardOutput)),
        new(CliSemanticStatus.Failed, "failed", new CliProcessDisposition(1, CliOutputTarget.StandardError)),
        new(CliSemanticStatus.Attention, "attention", new CliProcessDisposition(2, CliOutputTarget.StandardOutput)),
        new(CliSemanticStatus.Incomplete, "incomplete", new CliProcessDisposition(3, CliOutputTarget.StandardOutput)),
        new(CliSemanticStatus.Invalid, "invalid", new CliProcessDisposition(4, CliOutputTarget.StandardError)),
        new(CliSemanticStatus.Blocked, "blocked", new CliProcessDisposition(5, CliOutputTarget.StandardError)),
        new(CliSemanticStatus.Interrupted, "interrupted", new CliProcessDisposition(130, CliOutputTarget.StandardError)),
    ];

    internal static CliStatusDefinition Read(CliSemanticStatus status)
    {
        var index = (int)status;
        if ((uint)index >= (uint)Definitions.Length || Definitions[index].Status != status)
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "The semantic status is not defined.");
        }

        return Definitions[index];
    }

    internal static bool TryParse(string machineName, out CliSemanticStatus status)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(machineName);
        foreach (var definition in Definitions)
        {
            if (string.Equals(machineName, definition.MachineName, StringComparison.Ordinal))
            {
                status = definition.Status;
                return true;
            }
        }

        status = default;
        return false;
    }
}
