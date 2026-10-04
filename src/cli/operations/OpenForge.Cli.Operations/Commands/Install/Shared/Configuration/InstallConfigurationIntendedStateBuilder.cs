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

namespace OpenForge.Cli.Core.Commands.Install.Shared.Configuration;

internal sealed class InstallConfigurationIntendedStateBuilder(PhysicalPathResolver paths)
{
    private static readonly UTF8Encoding Utf8 = new(false, true);
    private readonly InstallTargetReader _reader = new(paths);
    private readonly SourceCatalogueReader _catalogue = new();
    private readonly MarkdownDocumentParser _markdown = new();
    private readonly SourceAuthoredMetadataParser _metadata = new();
    private readonly InstallContentIdentity _identity = new();

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
        var configuration = request.Configuration ?? throw new InvalidOperationException("Configuration selection is required.");
        if (ownership.State is not (WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete))
            return Boundary(ownership.Cause ?? "Configuration requires trustworthy ownership facts.", InstallFindingCode.LifecycleBlocked);
        var settingsChange = InstallConfigurationSettings.Plan(settings, configuration);
        var intendedSettings = settings.Document;
        if (settingsChange is { } change)
            intendedSettings = WorkspaceSettingsCodec.Read(change.IntendedBytes.ToArray()).Document
                ?? throw new InvalidDataException("The planned settings must decode.");
        foreach (var row in configuration.Routes.Where(row => row.Action != InstallRouteAction.Remove))
        {
            if (WorkspaceRemovals.IsPathRemoved(InstallConfigurationChoices.Entrypoint(row.Id), intendedSettings))
                throw new InvalidDataException($"A broader exclusion prevents adding '{row.Id}'. Configure its required ancestor first.");
        }
        var selected = payload.Assets.Where(asset => asset.Path.StartsWith(".agents/", StringComparison.Ordinal)
            && FrameworkPayloadSelection.IncludesPath(asset.Path, intendedSettings)).ToArray();
        var ignore = await _reader.ReadAsync(request.Workspace, InstallIgnoreSection.Path, token).ConfigureAwait(false);
        var ignoreSnapshot = ignore.Snapshot ?? throw new InvalidDataException(ignore.Cause ?? "The Git-ignore file is unsafe or unavailable.");
        var ignoreBytes = InstallIgnoreSection.Rewrite(ignoreSnapshot.Bytes.AsSpan(), configuration.Routes);
        PlannedFileChange? ignoreChange = null;
        if (!ignoreSnapshot.Bytes.AsSpan().SequenceEqual(ignoreBytes))
            ignoreChange = ignore.State == InstallTargetReadState.Missing
                ? PlannedFileChange.Create(ignoreSnapshot.Expectation, ignoreBytes)
                : PlannedFileChange.ReplaceGeneratedRegion(ignoreSnapshot.Expectation, ignoreBytes);

        var catalogue = await _catalogue.ReadAsync(new(request.Workspace, [SourceLogicalPath.AgentsRoot]), token).ConfigureAwait(false);
        token.ThrowIfCancellationRequested();
        if (catalogue.Issues.Any(issue => issue.Code != SourceCatalogueIssueCode.RootMissing))
            return Boundary("Current source topology is unsafe or ambiguous for configuration.", InstallFindingCode.GeneratedRegionUnsafe);
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
            if (sources.ContainsKey(asset.Path))
            {
                targetBytes.Add(asset.Path, Utf8.GetBytes(documents[asset.Path]));
                continue;
            }
            var read = await _reader.ReadAsync(request.Workspace, asset.Path, token).ConfigureAwait(false);
            if (read.State != InstallTargetReadState.Missing)
                throw new InvalidDataException($"The selected payload destination '{asset.Path}' is occupied without a safe source identity.");
            var source = FrameworkPayloadSourceProjection.Create(request.Workspace, asset);
            sources.Add(asset.Path, source);
            documents.Add(asset.Path, Utf8.GetString(asset.Bytes.AsSpan()));
            targetBytes.Add(asset.Path, asset.Bytes.ToArray());
        }
        var formation = new GeneratedNavigationFormationBuilder().Build(catalogue, sources.Values.ToArray());
        if (formation.Ambiguities.Count > 0 || formation.IntendedTargetCollisions.Count > 0)
            throw new InvalidDataException("Configuration would form an ambiguous route or target.");
        if (FrameworkPayloadSelection.FindMissingRequiredAncestor(payload, intendedSettings, formation) is { } ancestor)
            throw new InvalidDataException($"Excluded ancestor '{ancestor}' is required by a selected route.");
        var parsed = sources.Values.ToDictionary(source => source.Identity.CanonicalBasePath,
            source => _markdown.Parse(documents[source.Identity.CanonicalBasePath]), StringComparer.Ordinal);
        var metadata = sources.Values.Where(source => source.Base.Form != SourceDocumentForm.Loader)
            .Select(source => new GeneratedNavigationMetadata(source, _metadata.Parse(parsed[source.Identity.CanonicalBasePath], source.Base.Form))).ToArray();
        var payloadPaths = payload.Assets.Select(asset => asset.Path).ToHashSet(StringComparer.Ordinal);
        var regionSources = sources.Values.Where(source => payloadPaths.Contains(source.Identity.CanonicalBasePath)
            && (source.Base.Form == SourceDocumentForm.Loader || SourceFormClassifier.IsEntrypoint(source.Base.Form))).ToArray();
        var projection = new GeneratedNavigationProjector().Project(new(formation,
            regionSources.Select(source => new GeneratedNavigationRegionInput(source, parsed[source.Identity.CanonicalBasePath])), metadata));
        foreach (var region in projection.Regions)
        {
            if (region.State != GeneratedNavigationRegionState.Available || region.Change is not { } regionChange)
                throw new InvalidDataException(region.Cause ?? "Configuration requires safe generated Entries boundaries.");
            targetBytes[region.CanonicalPath] = regionChange.ExpectedDocumentBytes.ToArray();
        }
        var anchors = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        foreach (var path in new[] { FrameworkPayloadAsset.RootAgentPath, FrameworkPayloadAsset.RootClaudePath })
        {
            if (FrameworkPayloadSelection.IncludesPath(path, intendedSettings))
                anchors[path] = payload.Find(path)?.Bytes.ToArray() ?? throw new InvalidDataException("A managed host payload is missing.");
        }
        if (ownership.Document.Framework is { } framework)
        {
            var loaderAsset = payload.Find(FrameworkPayloadAsset.LoaderPath) ?? throw new InvalidDataException("The loader payload is missing.");
            if (existingPaths.Contains(loaderAsset.Path) && framework.Paths.Contains(loaderAsset.Path, StringComparer.Ordinal)
                && _identity.ReadSourceFingerprint(Utf8.GetBytes(documents[loaderAsset.Path])) != _identity.ReadSourceFingerprint(loaderAsset.Bytes.AsSpan()))
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
                TargetBytes = targetBytes,
                ManagedBlockBytes = anchors,
                UserOwnedPaths = existingPaths,
                GeneratedRegionPaths = projection.Regions.Select(region => region.CanonicalPath).ToHashSet(StringComparer.Ordinal),
                ProjectionInputs = observations.OrderBy(observation => observation.CanonicalLayerPath, StringComparer.Ordinal).ToArray(),
                AdoptionOwnershipExpectation = ownership.Snapshot?.Expectation,
                Configuration = new(settings, ignore, settingsChange, ignoreChange),
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
