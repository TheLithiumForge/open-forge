using OpenForge.Cli.Core.Shell.Definitions;
using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Shared.Entries;
using OpenForge.Cli.Core.Framework.Documents.Shared.Entries.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Shared.Destinations;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;

internal sealed class RouteInclusionPlanBuilder
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal async ValueTask<RouteInclusionPlanBuild> BuildAsync(RouteInclusionRequest request, CancellationToken token)
    {
        try { return await BuildCoreAsync(request, token).ConfigureAwait(false); }
        catch (DecoderFallbackException) { return Stop(CliSemanticStatus.Blocked, RouteInclusionFindingCode.WorkspaceUnsafe, "Save the loader as UTF-8 text, then retry."); }
        catch (ArgumentException exception) { return Stop(CliSemanticStatus.Invalid, RouteInclusionFindingCode.InvalidInput, exception.Message); }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { return Stop(CliSemanticStatus.Interrupted, RouteInclusionFindingCode.Interrupted, "The request was interrupted."); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        { return Stop(CliSemanticStatus.Blocked, RouteInclusionFindingCode.WorkspaceUnsafe, exception.Message); }
    }

    private static RouteInclusionPlanBuild Stop(CliSemanticStatus status, RouteInclusionFindingCode code, string cause)
        => new(new RouteInclusionOutcome { Status = status, Code = code, Cause = cause });

    private static async ValueTask<RouteInclusionPlanBuild> BuildCoreAsync(RouteInclusionRequest request, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var layoutRead = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(request.Workspace, token).ConfigureAwait(false);
        if (layoutRead.Layout is not { } layout)
            return Stop(CliSemanticStatus.Blocked, RouteInclusionFindingCode.WorkspaceUnsafe, layoutRead.Issues.FirstOrDefault()?.Cause ?? "Repair the workspace settings, then retry.");
        var path = RouteInclusionTargetResolver.Resolve(layout, request);
        if (layoutRead.State is not (WorkspaceLayoutReadState.Ready or WorkspaceLayoutReadState.Uninstalled))
            return Stop(CliSemanticStatus.Blocked, RouteInclusionFindingCode.WorkspaceUnsafe, layoutRead.Issues.FirstOrDefault()?.Cause ?? "Repair the workspace layout, then retry.");
        var include = request.Action == RouteInclusionAction.Include;
        var external = !layout.IsImplicitContentDestination(path);
        var loader = await RouteInclusionTargetResolver.ReadAsync(request.Workspace, layout.LoaderPath, token).ConfigureAwait(false);
        if (loader.Kind != FileExpectationKind.File)
            return Stop(CliSemanticStatus.Blocked, RouteInclusionFindingCode.WorkspaceUnsafe, "The workspace loader is missing. Install Open Forge first.");
        var source = StrictUtf8.GetString(loader.Bytes.AsSpan());
        var document = new MarkdownDocumentParser().Parse(source);
        var parsed = SourceLoaderEntriesParser.Parse(layout, source);
        if (parsed.State != SourceLoaderEntriesParseState.Valid
            || parsed.Destinations.Any(destination => destination.State is not (SourceLoaderDestinationParseState.Valid or SourceLoaderDestinationParseState.Linked))
            || document.GeneratedRegion.EntriesBlock is not { } block)
            return Stop(CliSemanticStatus.Blocked, RouteInclusionFindingCode.WorkspaceUnsafe, parsed.Cause ?? "Repair the loader's Entries section, then retry.");
        var rows = source[block.Span.Start..block.Span.End].Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n')
            .Where(line => MarkdownEntryRowParser.TryParse(line, out _, out _)).ToArray();
        var matching = rows.Where(line =>
        {
            MarkdownEntryRowParser.TryParse(line, out var row, out _);
            return row is not null && SourceLoaderDestinationParser.Parse(layout, row.Destination).CanonicalPath == path;
        }).ToArray();
        if (include && matching.Length > 1)
            throw RouteInclusionTargetResolver.Invalid(request, "Several loader rows list this target. Exclude it, then include it again with the intended metadata.");
        var before = matching.Length == 0 ? null : string.Join("\n", matching);
        string? after = null;
        if (include && external)
        {
            MarkdownEntryRow? existing = null;
            if (matching.Length > 0) MarkdownEntryRowParser.TryParse(matching[0], out existing, out _);
            after = MergeRow(layout, request, path, existing);
            if (existing is not null && SameFields(request, existing)) after = matching[0];
        }
        var settings = layoutRead.Settings;
        if (settings.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete)
            || settings.Snapshot is not { } settingsSnapshot)
            return Stop(CliSemanticStatus.Blocked, RouteInclusionFindingCode.WorkspaceUnsafe, settings.Cause ?? "Repair the workspace settings, then retry.");
        PlannedFileChange? settingsChange = null;
        if (external)
        {
            settingsChange = include ? WorkspaceSettingsChangePlanner.PlanRootRegistration(settings, path)
                : WorkspaceSettingsChangePlanner.PlanRootDeregistration(settings, path);
        }
        var observations = ImmutableArray.CreateBuilder<FileStateSnapshot>();
        observations.Add(loader);
        observations.Add(settingsSnapshot);
        if (include)
            observations.Add(await RouteInclusionTargetResolver.ReadAsync(request.Workspace, path, token).ConfigureAwait(false));
        var intendedLoader = loader.Bytes;
        if (!(include && external && matching.Length > 0 && after == matching[0])
            && (include || matching.Length > 0))
        {
            var catalogue = await new SourceCatalogueReader().ReadAsync(new SourceCatalogueRequest(layout, layout.SourceRoots), token).ConfigureAwait(false);
            var formation = new GeneratedNavigationFormationBuilder().Build(catalogue);
            var loaderSource = formation.FindSource(layout.LoaderPath)
                ?? throw new IOException("The loader could not be identified. Repair its route before retrying.");
            var metadata = new List<GeneratedNavigationMetadata>();
            var activeRoots = parsed.Destinations.Select(destination => destination.CanonicalPath).ToHashSet(StringComparer.Ordinal);
            foreach (var root in formation.Topology.LoaderRootPaths)
            {
                if ((!include && root == path) || (!activeRoots.Contains(root) && !(include && root == path))) continue;
                var rootSource = formation.FindSource(root);
                if (rootSource is null) continue;
                var snapshot = await RouteInclusionTargetResolver.ReadAsync(request.Workspace, root, token).ConfigureAwait(false);
                observations.Add(snapshot);
                var facts = new SourceAuthoredMetadataParser().Parse(new MarkdownDocumentParser().Parse(StrictUtf8.GetString(snapshot.Bytes.AsSpan())), rootSource.Base.Form);
                metadata.Add(new GeneratedNavigationMetadata(rootSource, facts));
            }
            var projection = new GeneratedNavigationProjectionRequest(formation, [new GeneratedNavigationRegionInput(loaderSource, document)], metadata)
            {
                PreserveLoaderMembership = true,
                TurnedOnLoaderRoots = LoaderMembership.ReadTurnedOn(formation.Topology.LoaderRootPaths, _ => true, include && !external ? [path] : []),
                TurnedOffLoaderRoots = !include && !external ? new HashSet<string>([path], StringComparer.Ordinal) : new HashSet<string>(StringComparer.Ordinal),
                LoaderEdit = external ? new GeneratedNavigationLoaderEdit(path, after) : null,
            };
            var region = new GeneratedNavigationProjector().Project(projection).Regions.Single();
            if (region.Change is not { } change)
                return Stop(CliSemanticStatus.Blocked, RouteInclusionFindingCode.WorkspaceUnsafe, region.Cause ?? "Repair the loader's catalogues, then retry.");
            intendedLoader = change.ExpectedDocumentBytes;
            if (!external && include)
            {
                after = region.Entries.FirstOrDefault(entry => entry.CanonicalPath == path)?.Line;
                if (after is null)
                    throw RouteInclusionTargetResolver.Invalid(request, $"Use a recognized direct child catalogue at '{path}'.");
            }
        }
        var changes = ImmutableArray.CreateBuilder<PlannedFileChange>();
        var recovery = ImmutableArray.CreateBuilder<RecoveryBundleTarget>();
        if (!loader.Bytes.AsSpan().SequenceEqual(intendedLoader.AsSpan()))
        {
            var loaderChange = PlannedFileChange.ReplaceGeneratedRegion(loader.Expectation, intendedLoader.AsSpan());
            changes.Add(loaderChange);
            recovery.Add(RecoveryBundleTarget.Create(loaderChange, loader));
        }
        if (settingsChange is not null)
        {
            changes.Add(settingsChange);
            recovery.Add(settingsChange.Kind == PlannedFileChangeKind.Create
                ? RecoveryBundleTarget.CreateReversible(settingsChange, settingsSnapshot)
                : RecoveryBundleTarget.Create(settingsChange, settingsSnapshot));
        }
        var registered = settings.Document.Roots.Contains(path, StringComparer.Ordinal);
        var noOp = changes.Count == 0;
        var preview = new RouteInclusionOutcome
        {
            Path = path,
            Before = before,
            After = after,
            RegisteredBefore = registered,
            RegisteredAfter = external && include,
            NoOp = noOp,
            Verified = noOp,
            Recovery = noOp ? RouteInclusionRecoveryState.NotRequired : RouteInclusionRecoveryState.NotCreated,
            Effects = changes.Select(change => new RouteInclusionEffect(
                Path.GetRelativePath(request.Workspace.LexicalRoot, change.LogicalPath).Replace('\\', '/'),
                change.Kind == PlannedFileChangeKind.Create ? null : StrictUtf8.GetString(observations.First(item => item.LogicalPath == change.LogicalPath).Bytes.AsSpan()),
                StrictUtf8.GetString(change.IntendedBytes.AsSpan()), RouteInclusionEffectState.Planned,
                change.LogicalPath == loader.LogicalPath ? RouteInclusionEffectKind.LoaderEntries : RouteInclusionEffectKind.Settings)).ToImmutableArray(),
        };
        return new RouteInclusionPlanBuild(preview, new RouteInclusionPlan
        {
            Request = request,
            LayoutRead = layoutRead,
            Preview = preview,
            Changes = changes.ToImmutable(),
            RecoveryTargets = recovery.ToImmutable(),
            Observations = observations.DistinctBy(item => item.LogicalPath, StringComparer.Ordinal).ToImmutableArray(),
        });
    }

    private static bool SameFields(RouteInclusionRequest request, MarkdownEntryRow row)
        => (request.Description is null || EncodeDescription(request.Description) == row.Description)
            && (request.Tags is null || request.Tags.Value.SequenceEqual(row.Tags))
            && (request.ApplyTo is null || request.ApplyTo.Value.Select(pattern => pattern.Text).SequenceEqual(row.ApplyTo.Select(pattern => pattern.Text)));

    private static string MergeRow(WorkspaceLayout layout, RouteInclusionRequest request, string path, MarkdownEntryRow? existing)
    {
        if (request.Description is null && existing is null)
            throw RouteInclusionTargetResolver.Invalid(request, "Supply --description for a new loader row.");
        var description = request.Description is { } supplied ? EncodeDescription(supplied) : existing?.Description;
        var tags = request.Tags is { } suppliedTags ? suppliedTags.ToArray() : existing?.Tags ?? [];
        var patterns = request.ApplyTo is { } suppliedPatterns ? suppliedPatterns.ToArray() : existing?.ApplyTo ?? [];
        var destination = SourceDestinationEncoder.EncodePath(Path.GetRelativePath(
            Path.GetFullPath(layout.MainRoot, request.Workspace.LexicalRoot),
            Path.GetFullPath(path, request.Workspace.LexicalRoot)).Replace('\\', '/'));
        var suffix = tags.Count == 0 ? string.Empty : $" - {string.Join(' ', tags.Select(tag => $"#{tag}"))}";
        return $"- [{description}]({destination}){suffix}{MarkdownEntryRowFormatter.FormatApplyToSuffix(patterns)}";
    }

    private static string EncodeDescription(string value) => value.Replace("\\", "\\\\", StringComparison.Ordinal);
}
