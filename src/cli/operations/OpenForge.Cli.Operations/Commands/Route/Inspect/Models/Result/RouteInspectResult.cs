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
        IReadOnlyList<RouteInspectObservation> observations,
        IReadOnlyList<RouteInspectCondition> conditions,
        CliNextAction? next)
    {
        Status = status;
        Workspace = workspace;
        Selection = selection;
        Identity = identity;
        Profile = profile;
        Observations = observations;
        Conditions = conditions;
        Next = next;
    }

    internal int SchemaVersion => RouteInspectDefinitions.SchemaVersion;

    public string Command => RouteInspectDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    internal string? WorkspacePath => Workspace?.LexicalRoot;

    internal bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;

    internal RouteInspectSelection Selection { get; }

    internal RouteInspectIdentity? Identity { get; }

    internal RouteInspectProfile? Profile { get; }

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
        CliNextAction? next)
    {
        _ = CliStatusDefinitions.Read(status);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(observations);
        ArgumentNullException.ThrowIfNull(conditions);
        var materializedObservations = observations.ToArray();
        var materializedConditions = conditions.ToArray();
        if (materializedObservations.Any(observation => observation is null))
        {
            throw new ArgumentException("Route-inspect observations cannot contain null.", nameof(observations));
        }

        if (materializedConditions.Any(condition => condition is null))
        {
            throw new ArgumentException("Route-inspect conditions cannot contain null.", nameof(conditions));
        }

        ValidateStatus(status, selection, workspace, identity, profile, materializedObservations, materializedConditions, next);
        return new RouteInspectResult(
            status,
            workspace,
            selection,
            identity,
            profile,
            new ReadOnlyCollection<RouteInspectObservation>(materializedObservations),
            new ReadOnlyCollection<RouteInspectCondition>(materializedConditions),
            next);
    }
}
