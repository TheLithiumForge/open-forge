using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List;

internal sealed class RouteListFinding
{
    internal RouteListFinding(
        RouteListFindingCode code,
        CliSemanticStatus status,
        string? subject,
        string cause,
        IEnumerable<string>? candidatePaths = null)
    {
        _ = RouteListDefinitions.ReadFindingCode(code);
        if (!RouteListDefinitions.IsFindingStatusAllowed(code, status))
        {
            throw new ArgumentException("The finding status does not match the route-list finding code.", nameof(status));
        }

        if (subject is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        var candidates = candidatePaths?.ToArray() ?? [];
        if (candidates.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Candidate paths cannot contain an empty value.", nameof(candidatePaths));
        }

        if (code == RouteListFindingCode.AmbiguousSource && candidates.Length == 0)
        {
            throw new ArgumentException("An ambiguous source finding requires every candidate path.", nameof(candidatePaths));
        }

        Code = code;
        Status = status;
        Subject = subject;
        Cause = cause;
        CandidatePaths = new ReadOnlyCollection<string>(candidates);
    }

    internal RouteListFindingCode Code { get; }

    internal string MachineCode => RouteListDefinitions.ReadFindingCode(Code);

    internal CliSemanticStatus Status { get; }

    internal string? Subject { get; }

    internal string Cause { get; }

    internal IReadOnlyList<string> CandidatePaths { get; }
}
