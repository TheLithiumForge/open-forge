using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Route.Init.Models.Result;

internal sealed record RouteInitResult : ICliCommandResult
{
    internal RouteInitResult(
        RouteInitResultFormation formation,
        ImmutableArray<RouteInitFinding> orderedFindings,
        CliSemanticStatus status,
        CliNextAction? next)
    {
        ArgumentNullException.ThrowIfNull(formation);
        Workspace = formation.Workspace;
        Mode = formation.Mode;
        Scaffold = formation.Scaffold;
        Target = formation.Target;
        Plan = formation.Plan;
        Framework = formation.Framework;
        Entrypoints = formation.Entrypoints;
        Effects = formation.Effects;
        UnchangedPaths = formation.UnchangedPaths;
        Lifecycle = formation.Lifecycle;
        Recovery = formation.Recovery;
        Verification = formation.Verification;
        Findings = orderedFindings;
        Status = status;
        Next = next;
    }

    public string Command => RouteInitDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    public CliNextAction? Next { get; }

    internal RouteInitMode Mode { get; }

    internal RouteInitScaffold Scaffold { get; }

    internal RouteInitTarget Target { get; }

    internal RouteInitPlanFacts Plan { get; }

    internal RouteInitFramework? Framework { get; }

    internal ImmutableArray<RouteInitEntrypoint> Entrypoints { get; }

    internal ImmutableArray<RouteInitEffect> Effects { get; }

    internal ImmutableArray<string> UnchangedPaths { get; }

    internal RouteInitLifecycle Lifecycle { get; }

    internal RouteInitRecovery Recovery { get; }

    internal RouteInitVerificationState Verification { get; }

    internal ImmutableArray<RouteInitFinding> Findings { get; }
}
