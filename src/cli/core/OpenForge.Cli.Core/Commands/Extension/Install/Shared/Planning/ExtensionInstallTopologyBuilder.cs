using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;

internal sealed class ExtensionInstallTopologyBuilder
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly SourceCatalogueReader _catalogueReader = new();
    private readonly GeneratedNavigationFormationBuilder _formationBuilder = new();
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly SourceAuthoredMetadataParser _metadataParser = new();
    private readonly GeneratedNavigationProjector _projector = new();

    internal async ValueTask<ExtensionInstallTopology> BuildAsync(
        ExtensionInstallRequest request,
        IReadOnlyList<ExtensionPackageFact> packages,
        CancellationToken cancellationToken)
    {
        var catalogue = await _catalogueReader.ReadAsync(
            new SourceCatalogueRequest(request.Workspace, [SourceLogicalPath.AgentsRoot]),
            cancellationToken).ConfigureAwait(false);
        if (catalogue.IsCancelled)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        var packageBytes = packages
            .SelectMany(package => package.Payload)
            .Select(file => ReadPayload(file))
            .GroupBy(payload => payload.PortableKey, StringComparer.Ordinal)
            .Select(ReadSharedPayload)
            .ToDictionary(payload => payload.Path, payload => payload.Bytes, StringComparer.Ordinal);
        var packagePaths = packageBytes.Keys.ToHashSet(StringComparer.Ordinal);
        var eligibleOverlayPaths = await ReadEligibleOverlayPathsAsync(
            catalogue,
            packagePaths,
            cancellationToken).ConfigureAwait(false);
        if (catalogue.Issues.Any(issue => issue.Code != SourceCatalogueIssueCode.RootMissing
            && !IsExactOverlayIssue(issue, eligibleOverlayPaths)))
        {
            throw new InvalidDataException(
                "Current authored source catalogue facts are unsafe or unavailable.");
        }
        var packageSources = packageBytes
            .Where(pair => ExtensionDestinationPolicy.IsImplicit(pair.Key) && SourceFormClassifier.TryClassify(pair.Key, out _))
            .Select(pair => CreatePackageSource(request, pair.Key))
            .ToDictionary(
                source => source.Identity.CanonicalBasePath,
                StringComparer.Ordinal);
        var intendedSources = catalogue.Sources
            .Where(source => !packageSources.ContainsKey(source.Identity.CanonicalBasePath))
            .Concat(packageSources.Values)
            .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        var formation = _formationBuilder.Build(catalogue, intendedSources);
        if (formation.Ambiguities.Count > 0
            || formation.IntendedTargetCollisions.Count > 0)
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
            .Where(value => value.Document.GeneratedRegion.State == MarkdownGeneratedRegionState.Complete)
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
        var regions = new List<ExtensionInstallGeneratedRegion>();
        foreach (var region in projection.Regions.OrderBy(
                     value => value.CanonicalPath,
                     StringComparer.Ordinal))
        {
            var change = region.Change
                ?? throw new InvalidDataException(
                    "An available generated region requires its bounded change.");
            regions.Add(new ExtensionInstallGeneratedRegion(
                region.CanonicalPath,
                change.IsUnchanged
                    ? ExtensionInstallGeneratedRegionState.Unchanged
                    : ExtensionInstallGeneratedRegionState.Changed));
            if (packageBytes.ContainsKey(region.CanonicalPath))
            {
                packageBytes[region.CanonicalPath] = [.. change.ExpectedDocumentBytes];
            }
            else
            {
                generated.Add(region.CanonicalPath, [.. change.ExpectedDocumentBytes]);
            }
        }

        var protectedPaths = catalogue.Candidates
            .Select(candidate => candidate.CanonicalPath)
            .Order(StringComparer.Ordinal)
            .ToArray();
        return ExtensionInstallTopology.Create(
            packageBytes,
            generated,
            regions,
            protectedPaths,
            eligibleOverlayPaths);
    }

    private static PayloadBytes ReadPayload(ExtensionPackageFileFact file)
    {
        if (!PortableWorkspacePath.TryNormalize(file.TargetPath, out var normalized)
            || file.Bytes is not { } bytes
            || file.Sha256 is not { } sha256)
        {
            throw new InvalidDataException(
                "A selected Extension payload lost its normalized target or reviewed bytes.");
        }

        return new PayloadBytes(
            normalized,
            PortableWorkspacePath.CreatePortableKey(normalized),
            sha256,
            bytes.ToArray());
    }

    private static PayloadBytes ReadSharedPayload(IGrouping<string, PayloadBytes> group)
    {
        var values = group.OrderBy(payload => payload.Path, StringComparer.Ordinal).ToArray();
        var canonical = values[0];
        if (values.Skip(1).Any(value => !string.Equals(
                canonical.Sha256,
                value.Sha256,
                StringComparison.Ordinal)
            || !canonical.Bytes.AsSpan().SequenceEqual(value.Bytes)))
        {
            throw new InvalidDataException(
                $"Selected packages disagree on portable target '{canonical.Path}'.");
        }

        return canonical;
    }

    private async ValueTask<HashSet<string>> ReadEligibleOverlayPathsAsync(
        SourceCatalogue catalogue,
        HashSet<string> packagePaths,
        CancellationToken cancellationToken)
    {
        var eligible = new HashSet<string>(StringComparer.Ordinal);
        foreach (var candidate in catalogue.Candidates.Where(candidate =>
                     packagePaths.Contains(candidate.CanonicalPath)))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (candidate.PhysicalState != PhysicalPathState.Contained
                || candidate.PhysicalPath is not { } physicalPath
                || candidate.Form is not { } form
                || form == SourceDocumentForm.OverwriteCompanion)
            {
                continue;
            }

            try
            {
                var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken)
                    .ConfigureAwait(false);
                var document = _markdownParser.Parse(StrictUtf8.GetString(bytes));
                var metadata = _metadataParser.Parse(document, form);
                if (metadata.State == SourceAuthoredMetadataState.Missing
                    && document.GeneratedRegion.State == MarkdownGeneratedRegionState.Absent)
                {
                    _ = eligible.Add(candidate.CanonicalPath);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception) when (exception is ArgumentException
                or DecoderFallbackException
                or InvalidDataException
                or IOException
                or UnauthorizedAccessException)
            {
                // Unreadable or unclassifiable current candidates stay protected.
            }
        }

        return eligible;
    }

    private static bool IsExactOverlayIssue(
        SourceCatalogueIssue issue,
        HashSet<string> packagePaths)
        => issue.Code == SourceCatalogueIssueCode.IdentityUnavailable
            && packagePaths.Contains(issue.AttemptedCanonicalPath)
            && issue.RelatedPaths.All(packagePaths.Contains);

    private static SourceLogicalSource CreatePackageSource(
        ExtensionInstallRequest request,
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
