using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;

internal sealed record RouteRemoveResult : ICliCommandResult
{
    internal RouteRemoveResult(
        RouteRemoveResultFormation formation,
        CliSemanticStatus status,
        CliNextAction? next)
    {
        ArgumentNullException.ThrowIfNull(formation);
        _ = CliStatusDefinitions.Read(status);
        Workspace = formation.Workspace;
        Mode = formation.Mode;
        Source = formation.Source;
        Subject = formation.Subject;
        Ownership = formation.Ownership;
        References = formation.References;
        GeneratedNavigation = formation.GeneratedNavigation;
        Plan = formation.Plan;
        Effects = formation.Effects;
        UnchangedPaths = formation.UnchangedPaths;
        Recovery = formation.Recovery;
        Persistence = formation.Persistence;
        Verification = formation.Verification;
        Findings = formation.Findings;
        Status = status;
        Next = next;
    }

    public string Command => RouteRemoveDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    public CliNextAction? Next { get; }

    internal string? WorkspacePath => Workspace?.LexicalRoot;

    internal bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;

    internal RouteRemoveMode Mode { get; }

    internal RouteRemoveSource Source { get; }

    internal RouteRemoveSubject Subject { get; }

    internal RouteRemoveOwnership Ownership { get; }

    internal RouteRemoveReferences References { get; }

    internal RouteRemoveGeneratedNavigation GeneratedNavigation { get; }

    internal RouteRemovePlanFacts Plan { get; }

    internal ImmutableArray<RouteRemoveEffect> Effects { get; }

    internal ImmutableArray<string> UnchangedPaths { get; }

    internal RouteRemoveRecovery Recovery { get; }

    internal RouteRemovePersistence Persistence { get; }

    internal RouteRemoveVerificationState Verification { get; }

    internal ImmutableArray<RouteRemoveFinding> Findings { get; }
}
