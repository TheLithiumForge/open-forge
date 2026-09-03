using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Move;

internal static class RouteMoveTestData
{
    internal const string SourceId = "memory/guides/old guide";
    internal const string SourcePath = ".agents/memory/guides/old guide.md";
    internal const string DestinationId = "memory/archive/new guide";
    internal const string DestinationPath = ".agents/memory/archive/new guide.md";
    internal const string ParentId = "memory/archive";
    internal const string ParentPath = ".agents/memory/archive/_archive.md";

    internal static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(
            Path.Combine(Path.GetTempPath(), "open-forge-route-move-contract"));
        return new CliWorkspace(
            root,
            root,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    internal static RouteMoveRequest Request(RouteMoveMode mode = RouteMoveMode.Apply)
        => new(
            Workspace(),
            SourceId,
            DestinationPath,
            mode);

    internal static RouteMoveResultFormation Formation(
        RouteMoveMode mode = RouteMoveMode.Apply,
        params RouteMoveFinding[] findings)
        => new()
        {
            Workspace = Workspace(),
            Mode = mode,
            Source = new RouteMoveSource
            {
                Requested = SourceId,
                SelectedBy = RouteMoveSourceSelection.SourceId,
                Id = SourceId,
                Path = SourcePath,
                Form = RouteMoveSourceForm.OrdinaryMarkdown,
            },
            Destination = new RouteMoveDestination
            {
                Requested = DestinationPath,
                Id = DestinationId,
                Path = DestinationPath,
                ParentId = ParentId,
                ParentPath = ParentPath,
            },
            Subject = new RouteMoveSubject
            {
                Kind = RouteMoveSubjectKind.Leaf,
                Layers =
                [
                    new RouteMoveSubjectLayer
                    {
                        Layer = RouteMoveLayerKind.Base,
                        SourcePath = SourcePath,
                        DestinationPath = DestinationPath,
                    },
                ],
                Items = [],
            },
            Ownership = new RouteMoveOwnership
            {
                State = RouteMoveOwnershipState.Unmanaged,
                Framework = RouteMoveOwnershipTrust.Trusted,
                Extensions = RouteMoveOwnershipTrust.Trusted,
                Claims = [],
            },
            References = new RouteMoveReferences
            {
                Coverage = RouteMoveCoverage.Complete,
                ScannedSourceCount = 3,
                InspectedSourceCount = 3,
                OccurrenceCount = 1,
                Rewrites = [],
            },
            GeneratedNavigation = new RouteMoveGeneratedNavigation
            {
                Coverage = RouteMoveCoverage.Complete,
                Regions = [],
            },
            Plan = new RouteMovePlanFacts
            {
                Completeness = RouteMovePlanCompleteness.Complete,
                Safety = RouteMovePlanSafety.Safe,
            },
            Effects = [],
            UnchangedPaths = ImmutableArray.Create(SourcePath),
            Recovery = new RouteMoveRecovery
            {
                State = mode == RouteMoveMode.DryRun
                    ? RouteMoveRecoveryState.NotCreated
                    : RouteMoveRecoveryState.NotRequired,
                ProtectedPaths = [],
                ResidualPath = null,
            },
            Verification = mode == RouteMoveMode.DryRun
                ? RouteMoveVerificationState.NotRequested
                : RouteMoveVerificationState.Verified,
            Findings = ImmutableArray.CreateRange(findings),
        };

    internal static RouteMoveFinding Finding(
        RouteMoveFindingCode code,
        OpenForge.Cli.Core.Shell.Definitions.CliSemanticStatus status)
        => new(code, status, SourcePath, $"Independent test cause for {code}.");
}
