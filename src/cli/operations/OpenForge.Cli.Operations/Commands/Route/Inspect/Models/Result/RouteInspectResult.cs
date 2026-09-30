using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;

internal sealed partial class RouteInspectResult : ICliCommandResult
{
    private RouteInspectResult(
        CliSemanticStatus status,
        CliWorkspace? workspace,
        RouteInspectSelection selection,
        RouteInspectIdentity? identity,
        RouteInspectProfile? profile,
        IReadOnlyList<string>? workingPaths,
        RouteInspectApplicability? applicability,
        IReadOnlyList<RouteInspectObservation> observations,
        IReadOnlyList<RouteInspectCondition> conditions,
        CliNextAction? next,
        RouteInspectMatchingFiles? matchingFiles)
    {
        Status = status;
        MatchingFiles = matchingFiles;
        Workspace = workspace;
        Selection = selection;
        Identity = identity;
        Profile = profile;
        WorkingPaths = workingPaths;
        Applicability = applicability;
        Observations = observations;
        Conditions = conditions;
        Next = next;
    }

    internal int SchemaVersion => RouteInspectDefinitions.SchemaVersion;

    internal RouteInspectMatchingFiles? MatchingFiles { get; }

    public string Command => RouteInspectDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    internal string? WorkspacePath => Workspace?.LexicalRoot;

    internal bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;

    internal RouteInspectSelection Selection { get; }

    internal RouteInspectIdentity? Identity { get; }

    internal RouteInspectProfile? Profile { get; }

    internal IReadOnlyList<string>? WorkingPaths { get; }

    internal RouteInspectApplicability? Applicability { get; }

    internal IReadOnlyList<RouteInspectObservation> Observations { get; }

    internal IReadOnlyList<RouteInspectCondition> Conditions { get; }

    public CliNextAction? Next { get; }

    internal static RouteInspectResult Create(
        CliSemanticStatus status,
        CliWorkspace? workspace,
        RouteInspectSelection selection,
        RouteInspectIdentity? identity,
        RouteInspectProfile? profile,
        IEnumerable<RouteInspectObservation> observations,
        IEnumerable<RouteInspectCondition> conditions,
        CliNextAction? next,
        RouteInspectApplicability? applicability = null,
        IEnumerable<string>? workingPaths = null,
        RouteInspectMatchingFiles? matchingFiles = null)
    {
        _ = CliStatusDefinitions.Read(status);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(observations);
        ArgumentNullException.ThrowIfNull(conditions);
        var materializedObservations = observations.ToArray();
        var materializedConditions = conditions.ToArray();
        var materializedWorkingPaths = workingPaths?.ToArray();
        if (materializedObservations.Any(observation => observation is null))
        {
            throw new ArgumentException("Route-inspect observations cannot contain null.", nameof(observations));
        }

        if (materializedConditions.Any(condition => condition is null))
        {
            throw new ArgumentException("Route-inspect conditions cannot contain null.", nameof(conditions));
        }

        if (materializedWorkingPaths is not null
            && (materializedWorkingPaths.Any(string.IsNullOrWhiteSpace)
                || materializedWorkingPaths.Distinct(StringComparer.Ordinal).Count() != materializedWorkingPaths.Length))
        {
            throw new ArgumentException("Route-inspect working paths must be nonblank and unique.", nameof(workingPaths));
        }

        ValidateStatus(status, selection, workspace, identity, profile, materializedObservations, materializedConditions, next, matchingFiles);
        return new RouteInspectResult(
            status,
            workspace,
            selection,
            identity,
            profile,
            materializedWorkingPaths is null
                ? null
                : new ReadOnlyCollection<string>(materializedWorkingPaths),
            applicability,
            new ReadOnlyCollection<RouteInspectObservation>(materializedObservations),
            new ReadOnlyCollection<RouteInspectCondition>(materializedConditions),
            next,
            matchingFiles);
    }
}
