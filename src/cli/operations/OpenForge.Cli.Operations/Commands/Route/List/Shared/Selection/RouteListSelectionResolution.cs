using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

internal enum RouteListSelectionResolutionState
{
    Resolved,
    Invalid,
    Blocked,
    Incomplete,
    Interrupted,
}

internal sealed class RouteListSelectionIssue
{
    internal RouteListSelectionIssue(
        RouteListFindingCode code,
        string? subject,
        string cause,
        IEnumerable<string>? candidatePaths = null)
        : this(code, ReadDefaultStatus(code), subject, cause, candidatePaths)
    {
    }

    internal RouteListSelectionIssue(
        RouteListFindingCode code,
        CliSemanticStatus status,
        string? subject,
        string cause,
        IEnumerable<string>? candidatePaths = null)
    {
        _ = RouteListDefinitions.ReadFindingCode(code);
        if (!RouteListDefinitions.IsFindingStatusAllowed(code, status))
        {
            throw new ArgumentException("The issue status does not match the route-list finding code.", nameof(status));
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

        if (candidates.Distinct(StringComparer.Ordinal).Count() != candidates.Length)
        {
            throw new ArgumentException("Candidate paths must be unique.", nameof(candidatePaths));
        }

        if (code == RouteListFindingCode.AmbiguousSource && candidates.Length == 0)
        {
            throw new ArgumentException("An ambiguous source issue requires every candidate path.", nameof(candidatePaths));
        }

        Code = code;
        Status = status;
        Subject = subject;
        Cause = cause;
        CandidatePaths = new ReadOnlyCollection<string>(
            candidates.OrderBy(path => path, StringComparer.Ordinal).ToArray());
    }

    internal RouteListFindingCode Code { get; }

    internal CliSemanticStatus Status { get; }

    internal string? Subject { get; }

    internal string Cause { get; }

    internal IReadOnlyList<string> CandidatePaths { get; }

    private static CliSemanticStatus ReadDefaultStatus(RouteListFindingCode code)
    {
        return code switch
        {
            RouteListFindingCode.InvalidSourceReference
                or RouteListFindingCode.UnknownSource
                or RouteListFindingCode.UnsupportedSource
                or RouteListFindingCode.LoaderSubject => CliSemanticStatus.Invalid,
            RouteListFindingCode.AmbiguousSource
                or RouteListFindingCode.UnsafeSource
                or RouteListFindingCode.RouteAmbiguous
                or RouteListFindingCode.PhysicalBoundary => CliSemanticStatus.Blocked,
            RouteListFindingCode.LoaderUnavailable => CliSemanticStatus.Incomplete,
            RouteListFindingCode.LoaderMalformed => CliSemanticStatus.Blocked,
            RouteListFindingCode.Interrupted => CliSemanticStatus.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The finding code is not owned by source selection."),
        };
    }
}

internal sealed class RouteListSelectionResolution
{
    internal RouteListSelectionResolution(
        RouteListSelectionResolutionState state,
        RouteListSelection selection,
        IReadOnlyList<RouteSource> selectedSources,
        IReadOnlyList<RouteListSelectionIssue> issues)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The selection resolution state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(selectedSources);
        ArgumentNullException.ThrowIfNull(issues);
        if (selectedSources.Any(source => source is null))
        {
            throw new ArgumentException("Selected sources cannot contain null.", nameof(selectedSources));
        }

        if (issues.Any(issue => issue is null))
        {
            throw new ArgumentException("Selection issues cannot contain null.", nameof(issues));
        }

        State = state;
        Selection = selection;
        SelectedSources = new ReadOnlyCollection<RouteSource>(selectedSources.ToArray());
        Issues = new ReadOnlyCollection<RouteListSelectionIssue>(issues.ToArray());
    }

    internal RouteListSelectionResolutionState State { get; }

    internal RouteListSelection Selection { get; }

    internal IReadOnlyList<RouteSource> SelectedSources { get; }

    internal IReadOnlyList<RouteListSelectionIssue> Issues { get; }
}
