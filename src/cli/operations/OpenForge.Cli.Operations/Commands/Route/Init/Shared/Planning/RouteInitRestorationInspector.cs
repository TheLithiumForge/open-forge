using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal sealed class RouteInitRestorationInspector
{
    private readonly PhysicalPathResolver _resolver = new();

    internal async ValueTask<RouteInitInspectionResult> InspectAsync(
        RouteInitRequest request,
        RouteInitInspectionFacts inspection,
        CancellationToken cancellationToken)
    {
        if (inspection.Framework is not { Alignment.IsCanonicalRestoration: true } framework)
        {
            return new RouteInitInspectionCompleted(inspection);
        }

        try
        {
            return new RouteInitInspectionCompleted(await BuildAsync(request, inspection, cancellationToken).ConfigureAwait(false));
        }
        catch (RouteInitPlanningException exception)
        {
            return new RouteInitInspectionStopped(new RouteInitPlanningBoundary(
                exception.Code, exception.Message, exception.Incomplete, inspection.Target,
                Alignment: framework.Alignment, Payload: framework.Payload));
        }
    }

    private async ValueTask<RouteInitInspectionFacts> BuildAsync(
        RouteInitRequest request,
        RouteInitInspectionFacts inspection,
        CancellationToken cancellationToken)
    {
        var framework = inspection.Framework ?? throw new InvalidOperationException("Restoration requires Framework facts.");
        var targetPath = inspection.Target.CanonicalPath ?? throw new InvalidOperationException("Restoration requires a canonical target.");
        var directory = SourceLogicalPath.ReadParent(targetPath);
        var settings = inspection.Settings;

        var ancestor = FindExcludedAncestor(directory, inspection, settings);
        if (ancestor is not null)
        {
            throw new RouteInitPlanningException(RouteInitFindingCode.LifecycleBlocked,
                $"The ancestor '{ancestor}' is excluded. Explicitly restore it first with 'open-forge route init {ancestor[".agents/".Length..]} --framework'.",
                incomplete: false);
        }

        var removal = new WorkspaceRemovalSelection
        {
            Categories = inspection.Target.RequestedSegments.Count == 1 ? [inspection.Target.RequestedSegments[0]] : [],
            Directories = [directory],
            Files = [targetPath],
        };
        var settingsChange = WorkspaceSettingsChangePlanner.PlanRestoration(settings, removal);
        var effective = settingsChange is null ? settings.Document
            : WorkspaceSettingsCodec.Read(settingsChange.IntendedBytes.AsMemory()).Document
                ?? throw new InvalidOperationException("A planned restoration settings change must remain valid.");
        var assets = framework.Payload.Assets.Where(asset =>
            asset.Path.StartsWith(directory + "/", StringComparison.Ordinal)
            && FrameworkPayloadSelection.IncludesPath(asset.Path, effective)).ToArray();
        var chain = inspection.Current.Chain.ToList();
        var aligned = framework.Alignment.Segments.ToList();
        var files = ImmutableArray.CreateBuilder<RouteInitRestorationFile>();
        var validator = new FileExpectationValidator(_resolver);
        foreach (var asset in assets)
        {
            var source = framework.Alignment.PayloadSources.SingleOrDefault(value => value.Identity.CanonicalBasePath == asset.Path);
            if (source is not null && SourceFormClassifier.IsEntrypoint(source.Base.Form))
            {
                if (!chain.Any(entry => entry.Id == source.Identity.AutomaticId))
                {
                    var occupants = inspection.Catalogue.Catalogue.FindAllById(source.Identity.AutomaticId);
                    var entry = new RouteInitCurrentChainEntry(source.Identity.AutomaticId, asset.Path, occupants,
                        occupants.Where(value => SourceFormClassifier.IsEntrypoint(value.Base.Form)));
                    if (entry.IsAmbiguous || entry.IsMissing && entry.IdentityOccupants.Count > 0)
                    {
                        throw new RouteInitPlanningException(RouteInitFindingCode.RouteAmbiguous,
                            $"The canonical restoration route '{entry.Id}' is ambiguous or occupied.", incomplete: false);
                    }
                    chain.Add(entry);
                    aligned.Add(new RouteInitFrameworkAlignedSegment(entry.Id.Split('/')[^1],
                        RouteInitFrameworkSegmentRole.Managed, source, asset.Path));
                }
                continue;
            }

            var logical = SourceLogicalPath.ToLexicalPath(request.Workspace.LexicalRoot, asset.Path);
            var observed = await validator.ValidateAsync(request.Workspace, FileExpectation.Missing(logical), cancellationToken).ConfigureAwait(false);
            if (observed.State == FileExpectationValidationState.Cancelled)
            {
                throw new RouteInitPlanningException(RouteInitFindingCode.Interrupted,
                    "Restoration payload inspection was interrupted.", incomplete: true);
            }
            if (observed.State is not (FileExpectationValidationState.Matched or FileExpectationValidationState.Mismatched)
                || observed.Actual is not { } snapshot || snapshot.Kind == FileExpectationKind.Directory)
            {
                throw new RouteInitPlanningException(RouteInitFindingCode.TargetUnsafe,
                    observed.Cause ?? $"The restoration payload target '{asset.Path}' is unsafe.", incomplete: false);
            }
            var intendedBytes = snapshot.Kind == FileExpectationKind.Missing
                ? RouteInitRestorationEffects.Render(asset, settings.Document.Frontmatter)
                : asset.Bytes.AsMemory();
            files.Add(new RouteInitRestorationFile(asset, snapshot, source, intendedBytes));
        }

        var alignment = new RouteInitFrameworkAlignment(framework.Alignment.Target, aligned,
            framework.Alignment.PayloadSources, framework.Alignment.PayloadTopology)
        { IsCanonicalRestoration = true };
        var companions = ImmutableArray.CreateBuilder<FileStateSnapshot>();
        var snapshotReader = new SourceDocumentSnapshotReader();
        foreach (var read in inspection.Current.Reads)
        {
            if (read.Overwrite is { } overwrite)
            {
                companions.Add(await snapshotReader.ReadAsync(request.Workspace, overwrite, cancellationToken).ConfigureAwait(false));
            }
        }
        return inspection with
        {
            Framework = framework with { Alignment = alignment },
            Current = new RouteInitCurrentStateFacts(inspection.Catalogue, inspection.Target, chain, inspection.Current.Reads),
            Restoration = new RouteInitRestoration
            {
                Settings = settings,
                SettingsChange = settingsChange,
                Files = files.ToImmutable(),
                CompanionSnapshots = companions.ToImmutable(),
            },
        };
    }

    private static string? FindExcludedAncestor(string directory, RouteInitInspectionFacts inspection, WorkspaceSettingsRead settings)
    {
        var categories = settings.Document.RemovedCategories.Select(category => $".agents/{category}");
        var ancestor = categories.Concat(settings.Document.RemovedDirectories).FirstOrDefault(path =>
            directory.StartsWith(path + "/", StringComparison.Ordinal));
        if (ancestor is not null)
        {
            return ancestor;
        }

        return inspection.Current.Chain.Take(inspection.Current.Chain.Count - 1).FirstOrDefault(entry =>
            WorkspaceRemovals.IsPathRemoved(entry.CanonicalMissingPath, settings.Document)) is { } excluded
            ? SourceLogicalPath.ReadParent(excluded.CanonicalMissingPath) : null;
    }
}
