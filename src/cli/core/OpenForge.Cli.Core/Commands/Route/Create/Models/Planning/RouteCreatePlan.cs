using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;

internal sealed record RouteCreatePlan
{
    public required RouteCreateRequest Request { get; init; }

    public required RouteCreateResultFormation Preview { get; init; }

    public required GeneratedNavigationFormation NavigationFormation { get; init; }

    public required FileStateSnapshot TargetSnapshot { get; init; }

    public required SourceLogicalSource TargetSource { get; init; }

    public required SourceLogicalSource ParentSource { get; init; }

    public SourceLogicalSource? TemplateSource { get; init; }

    public required ImmutableArray<byte> IntendedTargetBytes { get; init; }

    public required ImmutableArray<PlannedFileChange> FileChanges { get; init; }

    public required ImmutableArray<RecoveryBundleTarget> RecoveryTargets { get; init; }

    internal bool IsNoOp => FileChanges.Length == 0;
}

internal sealed record RouteCreatePlanBuild
{
    public required RouteCreatePlan? Plan { get; init; }

    public required RouteCreateResultFormation Formation { get; init; }
}
