using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Route.Init.Models.Result;

internal sealed record RouteInitResultFormation
{
    internal RouteInitResultFormation(
        CliWorkspace? workspace,
        RouteInitMode mode,
        RouteInitScaffold scaffold,
        RouteInitTarget target,
        RouteInitPlanFacts plan,
        RouteInitFramework? framework,
        IEnumerable<RouteInitEntrypoint> entrypoints,
        IEnumerable<RouteInitEffect> effects,
        IEnumerable<string> unchangedPaths,
        RouteInitLifecycle lifecycle,
        RouteInitRecovery recovery,
        RouteInitVerificationState verification,
        IEnumerable<RouteInitFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(entrypoints);
        ArgumentNullException.ThrowIfNull(effects);
        ArgumentNullException.ThrowIfNull(unchangedPaths);
        ArgumentNullException.ThrowIfNull(lifecycle);
        ArgumentNullException.ThrowIfNull(recovery);
        ArgumentNullException.ThrowIfNull(findings);
        Workspace = workspace;
        Mode = mode;
        Scaffold = scaffold;
        Target = target;
        Plan = plan;
        Framework = framework;
        Entrypoints = Copy(entrypoints, "Route Init entrypoints cannot contain null members.");
        Effects = Copy(effects, "Route Init effects cannot contain null members.");
        UnchangedPaths = unchangedPaths
            .Select(path => path ?? throw new ArgumentException(
                "Route Init unchanged paths cannot contain null members.",
                nameof(unchangedPaths)))
            .ToImmutableArray();
        Lifecycle = lifecycle;
        Recovery = recovery;
        Verification = verification;
        Findings = Copy(findings, "Route Init findings cannot contain null members.");
    }

    internal CliWorkspace? Workspace { get; }

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

    private static ImmutableArray<T> Copy<T>(
        IEnumerable<T> values,
        string nullMemberMessage)
        where T : class
        => values
            .Select(value => value ?? throw new ArgumentException(nullMemberMessage, nameof(values)))
            .ToImmutableArray();
}
