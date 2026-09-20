using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Remove;

internal static class RouteRemoveTestData
{
    internal const string LeafId = "guidance/old guide";
    internal const string LeafPath = ".agents/guidance/old guide.md";
    internal const string OverwritePath = ".agents/guidance/old guide.overwrite.md";
    internal const string CategoryId = "guidance/topics";
    internal const string CategoryPath = ".agents/guidance/topics/_topics.md";

    internal static CliWorkspace Workspace(string suffix = "contract")
    {
        var root = Path.GetFullPath(
            Path.Combine(Path.GetTempPath(), $"open-forge-route-remove-{suffix}"));
        return new CliWorkspace(
            root,
            root,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    internal static RouteRemoveRequest Request(
        CliWorkspace? workspace = null,
        string sourceReference = LeafId,
        RouteRemoveMode mode = RouteRemoveMode.Apply)
        => new(
            workspace ?? Workspace(),
            sourceReference,
            mode);

    internal static RouteRemoveSource Source(
        string requested = LeafId,
        RouteRemoveSourceSelection selectedBy = RouteRemoveSourceSelection.SourceId,
        string? id = LeafId,
        string? path = LeafPath,
        RouteRemoveSourceForm? form = RouteRemoveSourceForm.OrdinaryMarkdown)
        => new()
        {
            Requested = requested,
            SelectedBy = selectedBy,
            Id = id,
            Path = path,
            Form = form,
        };

    internal static RouteRemoveSubject LeafSubject()
        => new()
        {
            Kind = RouteRemoveSubjectKind.Leaf,
            Layers =
            [
                new RouteRemoveSubjectLayer
                {
                    Layer = RouteRemoveLayerKind.Base,
                    SourcePath = LeafPath,
                },
                new RouteRemoveSubjectLayer
                {
                    Layer = RouteRemoveLayerKind.Overwrite,
                    SourcePath = OverwritePath,
                },
            ],
            Items =
            [
                new RouteRemoveSubjectItem
                {
                    Kind = RouteRemoveItemKind.RoutedMarkdown,
                    Layer = RouteRemoveLayerKind.Base,
                    SourceId = LeafId,
                    SourcePath = LeafPath,
                    RelativePath = "old guide.md",
                },
                new RouteRemoveSubjectItem
                {
                    Kind = RouteRemoveItemKind.RoutedMarkdown,
                    Layer = RouteRemoveLayerKind.Overwrite,
                    SourceId = LeafId,
                    SourcePath = OverwritePath,
                    RelativePath = "old guide.overwrite.md",
                },
            ],
        };

    internal static RouteRemoveSubject CategorySubject()
        => new()
        {
            Kind = RouteRemoveSubjectKind.Category,
            Layers =
            [
                new RouteRemoveSubjectLayer
                {
                    Layer = RouteRemoveLayerKind.Base,
                    SourcePath = CategoryPath,
                },
            ],
            Items =
            [
                new RouteRemoveSubjectItem
                {
                    Kind = RouteRemoveItemKind.Entrypoint,
                    Layer = RouteRemoveLayerKind.Base,
                    SourceId = CategoryId,
                    SourcePath = CategoryPath,
                    RelativePath = "_topics.md",
                },
                new RouteRemoveSubjectItem
                {
                    Kind = RouteRemoveItemKind.UnroutedMarkdown,
                    Layer = null,
                    SourceId = null,
                    SourcePath = ".agents/guidance/topics/notes.md",
                    RelativePath = "notes.md",
                },
                new RouteRemoveSubjectItem
                {
                    Kind = RouteRemoveItemKind.Resource,
                    Layer = null,
                    SourceId = null,
                    SourcePath = ".agents/guidance/topics/assets/settings.json",
                    RelativePath = "assets/settings.json",
                },
            ],
        };

    internal static RouteRemoveOwnership Ownership(
        RouteRemoveOwnershipState state = RouteRemoveOwnershipState.Unmanaged,
        RouteRemoveOwnershipTrust framework = RouteRemoveOwnershipTrust.Trusted,
        RouteRemoveOwnershipTrust extensions = RouteRemoveOwnershipTrust.Trusted,
        ImmutableArray<RouteRemoveOwnershipClaim> claims = default)
        => new()
        {
            State = state,
            Framework = framework,
            Extensions = extensions,
            Claims = claims.IsDefault ? [] : claims,
        };

    internal static RouteRemoveReferences References(
        RouteRemoveCoverage coverage = RouteRemoveCoverage.Complete,
        int scanned = 4,
        int inspected = 4,
        int occurrences = 1,
        ImmutableArray<RouteRemoveReferenceDetachment> detachments = default)
        => new()
        {
            Coverage = coverage,
            ScannedSourceCount = scanned,
            InspectedSourceCount = inspected,
            OccurrenceCount = occurrences,
            Detachments = detachments.IsDefault ? [Detachment()] : detachments,
        };

    internal static RouteRemoveReferenceDetachment Detachment(
        string sourcePath = "README.md",
        string before = "See [Old guide](.agents/guidance/old%20guide.md#part).",
        string expected = "See Old guide.")
        => new()
        {
            SourcePath = sourcePath,
            Layer = null,
            Location = new SourceLocation(3, 5, 12, Encoding.UTF8.GetByteCount(before)),
            Before = before,
            Expected = expected,
            OriginalDestination = ".agents/guidance/old%20guide.md#part",
            VisibleLabel = "Old guide",
        };

    internal static RouteRemoveGeneratedNavigation GeneratedNavigation(
        RouteRemoveCoverage coverage = RouteRemoveCoverage.Complete,
        RouteRemoveGeneratedState state = RouteRemoveGeneratedState.Changed)
        => new()
        {
            Coverage = coverage,
            Regions =
            [
                new RouteRemoveGeneratedRegion
                {
                    Path = ".agents/guidance/_guidance.md",
                    Reasons = [RouteRemoveGeneratedReason.OldParent],
                    State = state,
                },
            ],
        };

    internal static RouteRemovePlanFacts Plan(
        RouteRemovePlanCompleteness completeness = RouteRemovePlanCompleteness.Complete,
        RouteRemovePlanSafety safety = RouteRemovePlanSafety.Safe)
        => new()
        {
            Completeness = completeness,
            Safety = safety,
        };

    internal static RouteRemovePathState FileState(string hash = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")
        => new(RouteRemovePathStateKind.File, hash);

    internal static RouteRemoveEffect Effect(
        string path = LeafPath,
        RouteRemoveEffectKind kind = RouteRemoveEffectKind.RemovedFile,
        RouteRemoveEffectAction action = RouteRemoveEffectAction.Delete,
        RouteRemoveEffectOutcome outcome = RouteRemoveEffectOutcome.Planned,
        RouteRemoveEffectResidual residual = RouteRemoveEffectResidual.None)
        => new()
        {
            Path = path,
            Kind = kind,
            Action = action,
            Before = FileState(),
            Expected = new RouteRemovePathState(RouteRemovePathStateKind.Missing, null),
            Outcome = outcome,
            Residual = residual,
        };

    internal static RouteRemoveRecovery Recovery(
        RouteRemoveRecoveryState state = RouteRemoveRecoveryState.NotCreated,
        string? residualPath = null)
        => new()
        {
            State = state,
            ProtectedPaths = state == RouteRemoveRecoveryState.NotRequired ? [] : ["recovery.zip"],
            ResidualPath = residualPath,
        };

    internal static RouteRemoveResultFormation Formation(
        CliWorkspace? workspace = null,
        RouteRemoveMode mode = RouteRemoveMode.Apply,
        RouteRemoveSubject? subject = null,
        RouteRemoveResultFormation? overrides = null)
    {
        var formation = new RouteRemoveResultFormation
        {
            Workspace = workspace ?? Workspace(),
            Mode = mode,
            Source = Source(),
            Subject = subject ?? LeafSubject(),
            Ownership = Ownership(),
            References = References(),
            GeneratedNavigation = GeneratedNavigation(),
            Plan = Plan(),
            Effects = [Effect()],
            UnchangedPaths = [".agents/open-forge.lock.json"],
            Recovery = Recovery(),
            Verification = RouteRemoveVerificationState.NotRequested,
            Findings = [],
        };

        return overrides is null
            ? formation
            : overrides with
            {
                Workspace = overrides.Workspace ?? formation.Workspace,
            };
    }

    internal static RouteRemoveFinding Finding(
        RouteRemoveFindingCode code,
        CliSemanticStatus status = CliSemanticStatus.Blocked,
        string? target = LeafPath,
        string cause = "A bounded Route Remove finding.")
        => new(code, status, target, cause);
}
