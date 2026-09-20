using OpenForge.Cli.Core.Shell.Definitions.Models;

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

internal static class CliStatusDefinitions
{
    private static readonly CliStatusDefinition[] Definitions =
    [
        new(CliSemanticStatus.Complete, "completed", new CliProcessDisposition(0, CliOutputTarget.StandardOutput)),
        new(CliSemanticStatus.Failed, "failed", new CliProcessDisposition(1, CliOutputTarget.StandardError)),
        new(CliSemanticStatus.Attention, "completed-with-warnings", new CliProcessDisposition(2, CliOutputTarget.StandardOutput)),
        new(CliSemanticStatus.Incomplete, "incomplete", new CliProcessDisposition(3, CliOutputTarget.StandardOutput)),
        new(CliSemanticStatus.Invalid, "invalid-input", new CliProcessDisposition(4, CliOutputTarget.StandardError)),
        new(CliSemanticStatus.Blocked, "blocked", new CliProcessDisposition(5, CliOutputTarget.StandardError)),
        new(CliSemanticStatus.Interrupted, "cancelled", new CliProcessDisposition(130, CliOutputTarget.StandardError)),
    ];

    /// <summary>
    /// The order in which one status wins over another. A result carries many
    /// findings and reports one status, so every command needs this order; it is
    /// declared once here beside the rest of the status vocabulary.
    /// </summary>
    private static readonly CliSemanticStatus[] Precedence =
    [
        CliSemanticStatus.Failed,
        CliSemanticStatus.Interrupted,
        CliSemanticStatus.Invalid,
        CliSemanticStatus.Blocked,
        CliSemanticStatus.Incomplete,
        CliSemanticStatus.Attention,
    ];

    /// <summary>
    /// Reduces the statuses a result's findings carry to the one the result
    /// reports. Absent any ranked status — including an empty set — the result is
    /// <see cref="CliSemanticStatus.Complete"/>, because a finding never carries
    /// complete status.
    /// </summary>
    internal static CliSemanticStatus Collapse(IEnumerable<CliSemanticStatus> statuses)
    {
        ArgumentNullException.ThrowIfNull(statuses);
        var collapsed = CliSemanticStatus.Complete;
        var rank = Precedence.Length;
        foreach (var status in statuses)
        {
            var candidate = Array.IndexOf(Precedence, status);
            if (candidate >= 0 && candidate < rank)
            {
                collapsed = status;
                rank = candidate;
            }
        }

        return collapsed;
    }

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
