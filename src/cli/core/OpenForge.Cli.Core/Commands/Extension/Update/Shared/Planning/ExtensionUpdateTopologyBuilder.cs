using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning.Topology;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Shared.Planning;

internal sealed class ExtensionUpdateTopologyAliasException(string message)
    : Exception(message);

internal sealed class ExtensionUpdateTopologyBuilder
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly SourceCatalogueReader _catalogueReader = new();
    private readonly GeneratedNavigationFormationBuilder _formationBuilder = new();
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly SourceAuthoredMetadataParser _metadataParser = new();
    private readonly GeneratedNavigationProjector _projector = new();

    internal async ValueTask<ExtensionUpdateTopology> BuildAsync(
        ExtensionUpdateTopologyInput input,
        CancellationToken cancellationToken)
    {
        var request = input.Request;
        var catalogue = await _catalogueReader.ReadAsync(
            new SourceCatalogueRequest(request.Workspace, [SourceLogicalPath.AgentsRoot]),
            cancellationToken).ConfigureAwait(false);
        if (catalogue.IsCancelled)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        var packageBytes = input.Packages
            .SelectMany(package => package.Payload)
            .Select(ReadPayload)
            .GroupBy(payload => payload.PortableKey, StringComparer.Ordinal)
            .Select(ReadSharedPayload)
            .ToDictionary(payload => payload.Path, payload => payload.Bytes, StringComparer.Ordinal);
        var packagePaths = packageBytes.Keys.ToHashSet(StringComparer.Ordinal);
        foreach (var excluded in input.Admission.Exclusions)
        {
            _ = packageBytes.Remove(excluded);
        }

        foreach (var pair in input.Admission.Overrides)
        {
            packageBytes[pair.Key] = [.. pair.Value];
        }
        if (catalogue.Issues.Any(issue => issue.Code != SourceCatalogueIssueCode.RootMissing
            && !(issue.Code == SourceCatalogueIssueCode.IdentityUnavailable
                && packagePaths.Contains(issue.AttemptedCanonicalPath)
                && issue.RelatedPaths.All(packagePaths.Contains))))
        {
            throw new InvalidDataException(
                "Current authored source catalogue facts are unsafe or unavailable.");
        }

        var packageSources = packageBytes
            .Where(pair => ExtensionDestinationPolicy.IsImplicit(pair.Key) && SourceFormClassifier.TryClassify(pair.Key, out _))
            .Select(pair => CreatePackageSource(request, pair.Key))
            .ToDictionary(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal);
        var intendedSources = catalogue.Sources
            .Where(source => !input.RetiredPaths.Contains(source.Identity.CanonicalBasePath)
                && !packageSources.ContainsKey(source.Identity.CanonicalBasePath))
            .Concat(packageSources.Values)
            .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        var formation = _formationBuilder.Build(catalogue, intendedSources);
        if (formation.Ambiguities.Count > 0 || formation.IntendedTargetCollisions.Count > 0)
        {
            throw new InvalidDataException(
                "The intended Extension topology contains an ambiguous route, alias, or target collision.");
        }

        var documents = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var source in intendedSources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = source.Identity.CanonicalBasePath;
            var bytes = packageBytes.TryGetValue(path, out var packaged)
                ? packaged
                : await File.ReadAllBytesAsync(source.Base.PhysicalPath, cancellationToken)
                    .ConfigureAwait(false);
            documents.Add(path, StrictUtf8.GetString(bytes));
        }

        var metadata = intendedSources
            .Where(source => source.Base.Form != SourceDocumentForm.Loader)
            .Select(source => new GeneratedNavigationMetadata(
                source,
                _metadataParser.Parse(
                    _markdownParser.Parse(documents[source.Identity.CanonicalBasePath]),
                    source.Base.Form)))
            .ToArray();
        var regionInputs = intendedSources
            .Where(source => source.Base.Form == SourceDocumentForm.Loader
                || SourceFormClassifier.IsEntrypoint(source.Base.Form))
            .Select(source => new
            {
                Source = source,
                Document = _markdownParser.Parse(documents[source.Identity.CanonicalBasePath]),
            })
            .Where(value => value.Document.GeneratedRegion.State
                == MarkdownGeneratedRegionState.Complete)
            .Select(value => new GeneratedNavigationRegionInput(value.Source, value.Document))
            .ToArray();
        var projection = _projector.Project(new GeneratedNavigationProjectionRequest(
            formation,
            regionInputs,
            metadata));
        if (projection.Regions.Any(region => region.State != GeneratedNavigationRegionState.Available))
        {
            var boundary = projection.Regions.First(region =>
                region.State != GeneratedNavigationRegionState.Available);
            throw new InvalidDataException(
                boundary.Cause ?? "The intended generated navigation projection is unavailable.");
        }

        var generated = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        var generatedEntries = new Dictionary<string, IReadOnlyList<GeneratedNavigationEntry>>(
            StringComparer.Ordinal);
        var regions = new List<ExtensionUpdateGeneratedRegion>();
        foreach (var region in projection.Regions.OrderBy(
                     value => value.CanonicalPath,
                     StringComparer.Ordinal))
        {
            var change = region.Change
                ?? throw new InvalidDataException(
                    "An available generated region requires its bounded change.");
            regions.Add(new ExtensionUpdateGeneratedRegion(
                region.CanonicalPath,
                change.IsUnchanged
                    ? ExtensionUpdateGeneratedRegionState.Unchanged
                    : ExtensionUpdateGeneratedRegionState.Changed));
            generatedEntries.Add(region.CanonicalPath, region.Entries);
            if (packageBytes.ContainsKey(region.CanonicalPath))
            {
                packageBytes[region.CanonicalPath] = [.. change.ExpectedDocumentBytes];
            }
            else
            {
                generated.Add(region.CanonicalPath, [.. change.ExpectedDocumentBytes]);
            }
        }

        return new ExtensionUpdateTopology
        {
            IntendedTargetBytes = packageBytes,
            GeneratedTargetBytes = generated,
            Regions = regions,
            GeneratedEntries = generatedEntries,
            ProtectedPaths = catalogue.Candidates
                .Select(candidate => candidate.CanonicalPath)
                .ToHashSet(StringComparer.Ordinal),
        };
    }

    private static PayloadBytes ReadPayload(ExtensionPackageFileFact file)
    {
        if (!PortableWorkspacePath.TryNormalize(file.TargetPath, out var normalized)
            || file.Bytes is not { } bytes
            || file.Sha256 is not { } sha256
            || !ExtensionDestinationPolicy.IsAllowed(normalized))
        {
            throw new InvalidDataException(
                "A selected Extension payload lost its safe normalized target or reviewed bytes.");
        }

        var portableKey = PortableWorkspacePath.CreatePortableKey(normalized);
        if (portableKey == PortableWorkspacePath.CreatePortableKey(LifecycleSchema.RelativePath)
            || SourceFormClassifier.TryClassify(portableKey, out var form)
            && form == SourceDocumentForm.OverwriteCompanion)
        {
            throw new InvalidDataException(
                "An Extension payload targets a reserved lifecycle or overwrite-companion path.");
        }

        return new PayloadBytes(
            normalized,
            portableKey,
            sha256,
            bytes.ToArray());
    }

    private static PayloadBytes ReadSharedPayload(IGrouping<string, PayloadBytes> group)
    {
        var values = group.OrderBy(payload => payload.Path, StringComparer.Ordinal).ToArray();
        var canonical = values[0];
        if (values.Skip(1).Any(value => !string.Equals(
                canonical.Path,
                value.Path,
                StringComparison.Ordinal)
            || !string.Equals(canonical.Sha256, value.Sha256, StringComparison.Ordinal)
            || !canonical.Bytes.AsSpan().SequenceEqual(value.Bytes)))
        {
            throw new ExtensionUpdateTopologyAliasException(
                $"Selected packages disagree on portable target '{canonical.Path}'.");
        }

        return canonical;
    }

    private static SourceLogicalSource CreatePackageSource(
        ExtensionUpdateRequest request,
        string path)
    {
        if (!SourceFormClassifier.TryClassify(path, out var form))
        {
            throw new ArgumentException("The Extension package path is not an authored source.", nameof(path));
        }

        var id = SourceIdentity.DeriveId(path)
            ?? throw new InvalidDataException(
                $"The Extension package source '{path}' has no canonical identity.");
        var physical = Path.GetFullPath(Path.Combine(
            request.Workspace.PhysicalRoot,
            path.Replace('/', Path.DirectorySeparatorChar)));
        return new SourceLogicalSource(
            new SourceLogicalIdentity(id, path),
            new SourceLayer(path, physical, form, SourceLayerKind.Base));
    }

    private sealed record PayloadBytes(
        string Path,
        string PortableKey,
        string Sha256,
        byte[] Bytes);
}
