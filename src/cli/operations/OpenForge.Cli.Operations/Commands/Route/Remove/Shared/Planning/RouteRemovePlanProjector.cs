using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal static class RouteRemovePlanProjector
{
    internal static RouteRemoveResultFormation CreateDryRunPreview(RouteRemovePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return plan.Preview with { Mode = RouteRemoveMode.DryRun };
    }

    internal static RouteRemovePlan Build(
        RouteRemovePlanProjectionInput input,
        RouteRemoveCategoryInventory inventory)
    {
        ArgumentNullException.ThrowIfNull(input);
        var request = input.Subject.Request;
        var preview = new RouteRemoveResultFormation
        {
            Workspace = request.Workspace,
            Mode = request.Mode,
            Source = input.Subject.Source,
            Subject = new RouteRemoveSubject
            {
                Kind = input.Subject.Kind,
                Layers = input.Subject.Layers.Select(layer => new RouteRemoveSubjectLayer
                {
                    Layer = layer.Layer.Kind == Framework.Sources.Models.Inventory.SourceLayerKind.Base
                        ? RouteRemoveLayerKind.Base
                        : RouteRemoveLayerKind.Overwrite,
                    SourcePath = Canonical(request.Workspace.LexicalRoot, layer.Snapshot.LogicalPath),
                }).ToImmutableArray(),
                Items = input.Subject.Kind == RouteRemoveSubjectKind.Category
                    ? inventory.Items.Select(item => new RouteRemoveSubjectItem
                    {
                        Kind = item.Kind,
                        Layer = item.Layer,
                        SourceId = item.SourceId,
                        SourcePath = Canonical(request.Workspace.LexicalRoot, item.SourcePath),
                        RelativePath = item.RelativePath,
                    }).ToImmutableArray()
                    : [],
            },
            Ownership = RouteRemoveOwnershipProjector.Project(input.Ownership, input.Subject),
            References = input.References.References,
            GeneratedNavigation = input.Navigation.GeneratedNavigation,
            Plan = new RouteRemovePlanFacts
            {
                Completeness = RouteRemovePlanCompleteness.Complete,
                Safety = RouteRemovePlanSafety.Safe,
            },
            Effects = BuildEffects(input),
            UnchangedPaths = input.Navigation.GeneratedNavigation.Regions
                .Where(region => region.State == RouteRemoveGeneratedState.Unchanged)
                .Select(region => region.Path)
                .Order(StringComparer.Ordinal)
                .ToImmutableArray(),
            Recovery = new RouteRemoveRecovery
            {
                State = input.RecoveryTargets.IsEmpty
                    ? RouteRemoveRecoveryState.NotRequired
                    : RouteRemoveRecoveryState.NotCreated,
                ProtectedPaths = input.RecoveryTargets
                    .Select(target => Canonical(request.Workspace.LexicalRoot, target.Change.LogicalPath))
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToImmutableArray(),
            },
            Persistence = Persistence(input),
            Verification = RouteRemoveVerificationState.NotRequested,
        };
        return new RouteRemovePlan { Request = request, Preview = preview, Projection = input };
    }

    private static RouteRemovePersistence Persistence(RouteRemovePlanProjectionInput input)
        => new()
        {
            Settings = new RouteRemoveSettingsRemoval
            {
                Outcome = input.SettingsChange is null
                    ? RouteRemovePersistenceOutcome.Unchanged
                    : RouteRemovePersistenceOutcome.Planned,
                Path = WorkspaceSettingsDefinitions.RelativePath,
                Categories = input.RemovalSelection.Categories,
                Files = input.RemovalSelection.Files,
                Directories = input.RemovalSelection.Directories,
            },
            Ownership = new RouteRemoveOwnershipRelease
            {
                Outcome = input.OwnershipChange is null
                    ? RouteRemovePersistenceOutcome.Unchanged
                    : RouteRemovePersistenceOutcome.Planned,
                Claims = input.ClaimsToRelease,
            },
        };

    private static ImmutableArray<RouteRemoveEffect> BuildEffects(RouteRemovePlanProjectionInput input)
    {
        var root = input.Subject.Request.Workspace.LexicalRoot;
        return input.FileChanges.Select(change => FileEffect(root, change))
            .Concat(input.DirectoryDeletions.Select(deletion => new RouteRemoveEffect
            {
                Path = Canonical(root, deletion.LogicalPath),
                Kind = RouteRemoveEffectKind.Directory,
                Action = RouteRemoveEffectAction.Delete,
                Before = PathState(deletion.Expectation),
                Expected = new RouteRemovePathState(RouteRemovePathStateKind.Missing, null),
                Outcome = RouteRemoveEffectOutcome.Planned,
                Residual = RouteRemoveEffectResidual.None,
            }))
            .ToImmutableArray();
    }

    private static RouteRemoveEffect FileEffect(string root, PlannedFileChange change)
        => new()
        {
            Path = Canonical(root, change.LogicalPath),
            Kind = change.Kind switch
            {
                PlannedFileChangeKind.ReplaceGeneratedRegion => RouteRemoveEffectKind.GeneratedRegion,
                PlannedFileChangeKind.Replace => RouteRemoveEffectKind.ReferenceSource,
                PlannedFileChangeKind.Delete => RouteRemoveEffectKind.RemovedFile,
                _ => throw new ArgumentOutOfRangeException(nameof(change), change.Kind, null),
            },
            Action = change.Kind == PlannedFileChangeKind.Delete
                ? RouteRemoveEffectAction.Delete
                : RouteRemoveEffectAction.Replace,
            Before = PathState(change.Expectation),
            Expected = change.Kind == PlannedFileChangeKind.Delete
                ? new RouteRemovePathState(RouteRemovePathStateKind.Missing, null)
                : new RouteRemovePathState(
                    RouteRemovePathStateKind.File,
                    FileExpectation.Hash(change.IntendedBytes.AsSpan())),
            Outcome = RouteRemoveEffectOutcome.Planned,
            Residual = RouteRemoveEffectResidual.None,
        };

    private static RouteRemovePathState PathState(FileExpectation expectation)
        => expectation.Kind switch
        {
            FileExpectationKind.Missing => new RouteRemovePathState(RouteRemovePathStateKind.Missing, null),
            FileExpectationKind.File => new RouteRemovePathState(RouteRemovePathStateKind.File, expectation.ContentHash),
            FileExpectationKind.Directory => new RouteRemovePathState(RouteRemovePathStateKind.Directory, null),
            _ => throw new ArgumentOutOfRangeException(nameof(expectation), expectation.Kind, null),
        };

    private static string Canonical(string root, string path)
        => Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');
}
