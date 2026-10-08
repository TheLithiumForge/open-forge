using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Sharing;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Configuration;

internal sealed class InstallConfigurationIntendedStateBuilder(PhysicalPathResolver paths)
{
    private static readonly UTF8Encoding Utf8 = new(false, true);
    private readonly InstallTargetReader _reader = new(paths);
    private readonly SourceCatalogueReader _catalogue = new();
    private readonly MarkdownDocumentParser _markdown = new();
    private readonly SourceAuthoredMetadataParser _metadata = new();
    private readonly InstallContentIdentity _identity = new();
    private readonly InstallWorkspaceAdoptionBuilder _adoption = new(paths);

    internal async ValueTask<InstallIntendedStateBuild> BuildAsync(
        InstallRequest request, FrameworkPayload payload, WorkspaceOwnershipRead ownership,
        WorkspaceSettingsRead settings, CancellationToken token)
    {
        try
        {
            return await BuildCoreAsync(request, payload, ownership, settings, token).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is InvalidDataException or ArgumentException or InvalidOperationException or DecoderFallbackException)
        {
            return Boundary(exception.Message, InstallFindingCode.TargetUnsafe);
        }
    }

    private async ValueTask<InstallIntendedStateBuild> BuildCoreAsync(
        InstallRequest request, FrameworkPayload payload, WorkspaceOwnershipRead ownership,
        WorkspaceSettingsRead settings, CancellationToken token)
    {
        var configuration = request.Configuration;
        if (ownership.State is not (WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete))
            return Boundary(ownership.Cause ?? "Configuration requires trustworthy ownership facts.", InstallFindingCode.LifecycleBlocked);
        var settingsChange = InstallConfigurationSettings.Plan(settings, configuration, request.Frontmatter);
        var intendedSettings = settings.Document;
        if (settingsChange is { } change)
            intendedSettings = WorkspaceSettingsCodec.Read(change.IntendedBytes.ToArray()).Document
                ?? throw new InvalidDataException("The planned settings must decode.");
        foreach (var row in configuration?.Routes.Where(row => row.Action != InstallRouteAction.Remove) ?? [])
        {
            if (WorkspaceRemovals.IsPathRemoved(InstallConfigurationChoices.Entrypoint(row.Id), intendedSettings))
                throw new InvalidDataException($"A broader exclusion prevents adding '{row.Id}'. Configure its required ancestor first.");
        }
        var selected = payload.Assets.Where(asset => asset.Path.StartsWith(".agents/", StringComparison.Ordinal)
            && FrameworkPayloadSelection.IncludesPath(asset.Path, intendedSettings)).ToArray();
        var renderedPayload = InstallPayloadRendering.Render(payload, request.Frontmatter?.Form ?? intendedSettings.Frontmatter);
        InstallTargetRead? ignore = null;
        if (configuration is not null)
        {
            ignore = await _reader.ReadAsync(request.Workspace, InstallIgnoreSection.Path, token).ConfigureAwait(false);
            var ignoreSnapshot = ignore.Snapshot ?? throw new InvalidDataException(ignore.Cause ?? "The Git-ignore file is unsafe or unavailable.");
            _ = InstallIgnoreSection.Read(ignoreSnapshot.Bytes.AsSpan());
        }
        InstallConfigurationFrontmatterPlan? frontmatterPlan = null;
        if (request.Frontmatter is { ConvertOwnedFiles: true } selection)
            frontmatterPlan = await new InstallConfigurationFrontmatterPlanner(paths).PlanAsync(new()
            {
                Workspace = request.Workspace,
                Ownership = ownership,
                Payload = payload,
                RenderedPayload = renderedPayload,
                IntendedSettings = intendedSettings,
                Selection = selection,
            }, token).ConfigureAwait(false);

        var catalogue = await _catalogue.ReadAsync(new(request.Workspace, [SourceLogicalPath.AgentsRoot]), token).ConfigureAwait(false);
        token.ThrowIfCancellationRequested();
        if (catalogue.Issues.Any(issue => issue.Code != SourceCatalogueIssueCode.RootMissing))
            return Boundary("Current source topology is unsafe or ambiguous for configuration.", InstallFindingCode.GeneratedRegionUnsafe);
        var topology = _adoption.PlanTopology(catalogue.Sources,
            selected.Select(asset => FrameworkPayloadSourceProjection.Create(request.Workspace, asset)).ToArray(), intendedSettings, ownership);
        if (topology.Cause is { } topologyCause)
            return Boundary(topologyCause, InstallFindingCode.GeneratedRegionUnsafe);
        var sources = catalogue.Sources.ToDictionary(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal);
        var documents = new Dictionary<string, string>(StringComparer.Ordinal);
        var targetBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        var observations = new List<InstallProjectionInputObservation>();
        var existingPaths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var source in catalogue.Sources)
        {
            var read = await _reader.ReadAsync(request.Workspace, source.Identity.CanonicalBasePath, token).ConfigureAwait(false);
            var snapshot = RequireFile(read);
            documents.Add(source.Identity.CanonicalBasePath, Utf8.GetString(snapshot.Bytes.AsSpan()));
            observations.Add(Observation(source, source.Base, snapshot));
            existingPaths.Add(source.Identity.CanonicalBasePath);
            if (source.Overwrite is { } overwrite)
            {
                var companion = await _reader.ReadAsync(request.Workspace, overwrite.CanonicalPath, token).ConfigureAwait(false);
                observations.Add(Observation(source, overwrite, RequireFile(companion)));
            }
        }
        foreach (var asset in selected)
        {
            if (topology.ReusedEntrypoints.TryGetValue(asset.Path, out var actualPath) && actualPath != asset.Path)
            {
                targetBytes.Add(actualPath, Utf8.GetBytes(documents[actualPath]));
                continue;
            }
            if (sources.ContainsKey(asset.Path))
            {
                targetBytes.Add(asset.Path, Utf8.GetBytes(documents[asset.Path]));
                continue;
            }
            var read = await _reader.ReadAsync(request.Workspace, asset.Path, token).ConfigureAwait(false);
            if (read.State != InstallTargetReadState.Missing)
                throw new InvalidDataException($"The selected payload destination '{asset.Path}' is occupied without a safe source identity.");
            if (configuration is null) continue;
            var source = FrameworkPayloadSourceProjection.Create(request.Workspace, asset);
            sources.Add(asset.Path, source);
            var bytes = renderedPayload[asset.Path];
            documents.Add(asset.Path, Utf8.GetString(bytes));
            targetBytes.Add(asset.Path, bytes);
        }
        foreach (var replacement in frontmatterPlan?.Replacements ?? [])
        {
            if (documents.ContainsKey(replacement.Path)) documents[replacement.Path] = Utf8.GetString(replacement.IntendedBytes);
            if (targetBytes.ContainsKey(replacement.Path)) targetBytes[replacement.Path] = replacement.IntendedBytes;
        }
        var configurationPlan = new InstallConfigurationPlan(settings, ignore, settingsChange, IgnoreChange: null)
        { Frontmatter = request.Frontmatter, FrontmatterPlan = frontmatterPlan, GitIgnoredRoutes = ownership.Document.Framework?.GitIgnoredRoutes ?? [] };
        if (configuration is not null)
            configurationPlan = InstallConfigurationSharing.Complete(configurationPlan, configuration, sources.Values);
        var sharing = new SourceSharing(configurationPlan.GitIgnoredRoutes);
        var sharedCatalogue = sharing.Project(catalogue);
        var sharedSources = sources.Values.Where(source => sharing.Includes(source.Identity.CanonicalBasePath)).ToArray();
        var formation = new GeneratedNavigationFormationBuilder().Build(sharedCatalogue, sharedSources);
        if (formation.Ambiguities.Count > 0 || formation.IntendedTargetCollisions.Count > 0)
            throw new InvalidDataException("Configuration would form an ambiguous route or target.");
        if (FrameworkPayloadSelection.FindMissingRequiredAncestor(payload, intendedSettings, formation) is { } ancestor)
            throw new InvalidDataException($"Excluded ancestor '{ancestor}' is required by a selected route.");
        var parsed = sharedSources.ToDictionary(source => source.Identity.CanonicalBasePath,
            source => _markdown.Parse(documents[source.Identity.CanonicalBasePath]), StringComparer.Ordinal);
        var metadata = sharedSources.Where(source => source.Base.Form != SourceDocumentForm.Loader)
            .Select(source => new GeneratedNavigationMetadata(source, _metadata.Parse(parsed[source.Identity.CanonicalBasePath], source.Base.Form))).ToArray();
        var payloadPaths = payload.Assets.Select(asset => asset.Path).Concat(topology.ReusedEntrypoints.Values).ToHashSet(StringComparer.Ordinal);
        var conversionPaths = frontmatterPlan?.Replacements.Select(replacement => replacement.Path).ToHashSet(StringComparer.Ordinal) ?? [];
        var keptPaths = frontmatterPlan?.Kept.Select(file => file.Path).ToHashSet(StringComparer.Ordinal) ?? [];
        var regionSources = sharedSources.Where(source => !keptPaths.Contains(source.Identity.CanonicalBasePath)
            && (configuration is not null || conversionPaths.Contains(source.Identity.CanonicalBasePath))
            && (payloadPaths.Contains(source.Identity.CanonicalBasePath) || conversionPaths.Contains(source.Identity.CanonicalBasePath))
            && (source.Base.Form == SourceDocumentForm.Loader || SourceFormClassifier.IsEntrypoint(source.Base.Form))).ToArray();
        var projection = new GeneratedNavigationProjector().Project(new(formation,
            regionSources.Select(source => new GeneratedNavigationRegionInput(source, parsed[source.Identity.CanonicalBasePath])), metadata));
        foreach (var region in projection.Regions)
        {
            if (region.State != GeneratedNavigationRegionState.Available || region.Change is not { } regionChange)
                throw new InvalidDataException(region.Cause ?? "Configuration requires safe generated Entries boundaries.");
            if (targetBytes.ContainsKey(region.CanonicalPath)) targetBytes[region.CanonicalPath] = regionChange.ExpectedDocumentBytes.ToArray();
        }
        if (frontmatterPlan is not null)
        {
            var projected = projection.Regions.ToDictionary(region => region.CanonicalPath, StringComparer.Ordinal);
            frontmatterPlan = frontmatterPlan with
            {
                Replacements = frontmatterPlan.Replacements.Select(replacement => projected.TryGetValue(replacement.Path, out var region)
                    ? replacement with { IntendedBytes = region.Change?.ExpectedDocumentBytes.ToArray() ?? replacement.IntendedBytes }
                    : replacement).ToImmutableArray(),
            };
            configurationPlan = configurationPlan with { FrontmatterPlan = frontmatterPlan };
        }
        var anchors = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        foreach (var path in new[] { FrameworkPayloadAsset.RootAgentPath, FrameworkPayloadAsset.RootClaudePath })
        {
            if (FrameworkPayloadSelection.IncludesPath(path, intendedSettings))
                anchors[path] = renderedPayload[path];
        }
        if (ownership.Document.Framework is { } framework)
        {
            var loaderAsset = payload.Find(FrameworkPayloadAsset.LoaderPath) ?? throw new InvalidDataException("The loader payload is missing.");
            if (existingPaths.Contains(loaderAsset.Path) && framework.Paths.Contains(loaderAsset.Path, StringComparer.Ordinal)
                && _identity.ReadSourceFingerprint(Utf8.GetBytes(documents[loaderAsset.Path])) != _identity.ReadSourceFingerprint(renderedPayload[loaderAsset.Path]))
                return Boundary("The managed loader differs from the embedded Framework. Use Update.", InstallFindingCode.ManagedDivergence);
            foreach (var anchor in anchors)
            {
                if (!framework.Regions.Any(region => region.Path == anchor.Key && region.Region == "open-forge")) continue;
                var read = await _reader.ReadAsync(request.Workspace, anchor.Key, token).ConfigureAwait(false);
                var resolution = _identity.ResolveManagedBlock(RequireFile(read).Bytes.AsSpan(), anchor.Value);
                if (resolution.State != ManagedBlockState.Present || resolution.ExistingBlockBytes is not { } block
                    || _identity.ReadSourceFingerprint(block) != _identity.ReadSourceFingerprint(anchor.Value))
                    return Boundary("A managed workspace host differs from the embedded Framework. Use Update.", InstallFindingCode.ManagedDivergence);
            }
        }
        return new()
        {
            State = InstallIntendedStateBuildState.Complete,
            Cause = null,
            IntendedState = new()
            {
                Frontmatter = request.Frontmatter,
                TargetBytes = targetBytes,
                ManagedBlockBytes = anchors,
                UserOwnedPaths = existingPaths.Where(path => !conversionPaths.Contains(path)).ToHashSet(StringComparer.Ordinal),
                GeneratedRegionPaths = projection.Regions.Select(region => region.CanonicalPath).ToHashSet(StringComparer.Ordinal),
                ProjectionInputs = observations.OrderBy(observation => observation.CanonicalLayerPath, StringComparer.Ordinal).ToArray(),
                AdoptionOwnershipExpectation = ownership.Snapshot?.Expectation,
                Configuration = configurationPlan,
            },
        };
    }

    private static FileStateSnapshot RequireFile(InstallTargetRead read)
        => read.State == InstallTargetReadState.File && read.Snapshot is { } snapshot
            ? snapshot : throw new InvalidDataException(read.Cause ?? $"Source '{read.RelativePath}' changed or became unsafe.");

    private static InstallProjectionInputObservation Observation(SourceLogicalSource source, SourceLayer layer, FileStateSnapshot snapshot)
        => new()
        {
            AutomaticId = source.Identity.AutomaticId,
            CanonicalBasePath = source.Identity.CanonicalBasePath,
            CanonicalLayerPath = layer.CanonicalPath,
            Form = layer.Form,
            Kind = layer.Kind,
            Expectation = snapshot.Expectation
        };

    private static InstallIntendedStateBuild Boundary(string cause, InstallFindingCode code)
        => new() { State = InstallIntendedStateBuildState.Blocked, Cause = cause, FindingCode = code, IntendedState = null };
}
