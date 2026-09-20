using OpenForge.Cli.Core.Commands.Status.Models.Result;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal sealed class StatusFindingCollector
{
    private readonly List<StatusFinding> findings = [];

    internal void Add(StatusFindingCode code, string? subject, string cause, string? owner = null, string? source = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        if (findings.Any(finding => finding.Code == code
            && string.Equals(finding.Subject, subject, StringComparison.Ordinal)
            && string.Equals(finding.Owner, owner, StringComparison.Ordinal)
            && string.Equals(finding.Source, source, StringComparison.Ordinal)))
        {
            return;
        }

        findings.Add(new StatusFinding
        {
            Code = code,
            Status = StatusDefinitions.ReadFindingStatus(code),
            Subject = subject,
            Cause = cause,
            Owner = owner,
            Source = source,
        });
    }

    internal IReadOnlyList<StatusFinding> Complete()
        => findings
            .OrderBy(
                finding => StatusDefinitions.ReadFindingCode(finding.Code),
                StringComparer.Ordinal)
            .ThenBy(finding => finding.Subject, StringComparer.Ordinal)
            .ThenBy(finding => finding.Cause, StringComparer.Ordinal)
            .ToArray();
}
