using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

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
        => (await BuildWithFindingsAsync(request, packages, cancellationToken).ConfigureAwait(false)).Topology;

    internal async ValueTask<ExtensionInstallTopologyBuild> BuildWithFindingsAsync(
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
        var metadataByPath = metadata.ToDictionary(
            value => value.Source.Identity.CanonicalBasePath,
            value => value.Facts,
            StringComparer.Ordinal);
        var affectedHosts = ReadAffectedHosts(formation, packagePaths);
        var affectedClosure = ReadAffectedClosure(formation, packagePaths, affectedHosts);
        var omitted = projection.Regions
            .Select(region => (Region: region, MalformedChildren: ReadOmittableMalformedChildren(
                region,
                formation,
                regionInputs,
                metadata,
                metadataByPath,
                affectedClosure,
                _projector)))
            .Where(value => value.MalformedChildren is not null)
            .ToArray();
        var unavailable = projection.Regions
            .Where(region => region.State != GeneratedNavigationRegionState.Available
                && !omitted.Any(value => ReferenceEquals(value.Region, region)))
            .ToArray();
        if (unavailable.Length > 0)
        {
            var boundary = unavailable[0];
            throw new InvalidDataException(
                boundary.Cause ?? "The intended generated navigation projection is unavailable.");
        }

        var generated = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        var regions = new List<ExtensionInstallGeneratedRegion>();
        foreach (var region in projection.Regions
                     .Where(value => value.State == GeneratedNavigationRegionState.Available)
                     .OrderBy(value => value.CanonicalPath, StringComparer.Ordinal))
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
        var findings = omitted
            .OrderBy(value => value.Region.CanonicalPath, StringComparer.Ordinal)
            .SelectMany(value => (value.MalformedChildren
                ?? throw new InvalidOperationException(
                    "An omitted generated region requires malformed child identities.")
            ).Order(StringComparer.Ordinal)
                .Select(path => new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.MetadataProjectionSkipped,
                    value.Region.Cause ?? "The authored source metadata is malformed.",
                    path)))
            .ToArray();
        return new ExtensionInstallTopologyBuild(
            ExtensionInstallTopology.Create(
                packageBytes,
                generated,
                regions,
                protectedPaths,
                eligibleOverlayPaths),
            findings);
    }

    private static IReadOnlySet<string> ReadAffectedHosts(
        GeneratedNavigationFormation formation,
        IReadOnlySet<string> packagePaths)
    {
        var affected = new HashSet<string>(StringComparer.Ordinal);
        foreach (var packagePath in packagePaths.Order(StringComparer.Ordinal))
        {
            var source = formation.FindSource(packagePath);
            if (source is null)
            {
                continue;
            }

            AddRegionHost(source, affected);
            if (source.Base.Form == SourceDocumentForm.Loader)
            {
                continue;
            }

            var node = formation.Topology.FindByPath(source.Identity.CanonicalBasePath);
            while (node?.ParentState == SourceRouteParentState.Resolved)
            {
                var parentPath = node.ParentPaths[0];
                var parent = formation.FindSource(parentPath);
                if (parent is not null)
                {
                    AddRegionHost(parent, affected);
                }

                node = formation.Topology.FindByPath(parentPath);
            }

            if (formation.Loader is not null
                && formation.Topology.LoaderRootPaths.Contains(
                    source.Identity.CanonicalBasePath,
                    StringComparer.Ordinal))
            {
                _ = affected.Add(formation.Loader.Identity.CanonicalBasePath);
            }
        }

        return affected;
    }

    private static IReadOnlySet<string> ReadAffectedClosure(
        GeneratedNavigationFormation formation,
        IReadOnlySet<string> packagePaths,
        IReadOnlySet<string> affectedHosts)
    {
        var closure = new HashSet<string>(packagePaths, StringComparer.Ordinal);
        foreach (var hostPath in affectedHosts)
        {
            _ = closure.Add(hostPath);
            var host = formation.FindSource(hostPath);
            if (host is null)
            {
                continue;
            }

            var childPaths = ReadDirectChildPaths(formation, host);
            if (childPaths is not null)
            {
                closure.UnionWith(childPaths);
            }
        }

        return closure;
    }

    private static IReadOnlyList<string>? ReadOmittableMalformedChildren(
        GeneratedNavigationRegion region,
        GeneratedNavigationFormation formation,
        IReadOnlyList<GeneratedNavigationRegionInput> regionInputs,
        IReadOnlyList<GeneratedNavigationMetadata> metadata,
        IReadOnlyDictionary<string, SourceAuthoredMetadataFacts> metadataByPath,
        IReadOnlySet<string> affectedClosure,
        GeneratedNavigationProjector projector)
    {
        if (region.State == GeneratedNavigationRegionState.Available
            || region.UnavailableReason != GeneratedNavigationRegionUnavailableReason.MetadataInvalid
            || affectedClosure.Contains(region.CanonicalPath))
        {
            return null;
        }

        var childPaths = ReadDirectChildPaths(formation, region.Source);
        if (childPaths is null)
        {
            return null;
        }

        var malformedChildren = new List<string>();
        foreach (var childPath in childPaths)
        {
            if (!metadataByPath.TryGetValue(childPath, out var childMetadata))
            {
                return null;
            }

            if (childMetadata.State == SourceAuthoredMetadataState.Malformed)
            {
                if (affectedClosure.Contains(childPath)
                    || formation.FindSource(childPath)?.Base.Form is not { } form
                    || !(form == SourceDocumentForm.Markdown || SourceFormClassifier.IsEntrypoint(form)))
                {
                    return null;
                }

                malformedChildren.Add(childPath);
                continue;
            }

            if (childMetadata.State == SourceAuthoredMetadataState.Missing
                && formation.FindSource(childPath)?.Base.Form == SourceDocumentForm.Skill)
            {
                return null;
            }

            if (childMetadata.State is SourceAuthoredMetadataState.NotApplicable
                || childMetadata.State == SourceAuthoredMetadataState.Complete
                    && childMetadata.Description is null)
            {
                return null;
            }
        }

        if (malformedChildren.Count == 0)
        {
            return null;
        }

        var regionInput = regionInputs.FirstOrDefault(input =>
            string.Equals(
                input.Source.Identity.CanonicalBasePath,
                region.CanonicalPath,
                StringComparison.Ordinal));
        if (regionInput is null)
        {
            return null;
        }

        var proofMetadata = metadata
            .Select(value => malformedChildren.Contains(
                    value.Source.Identity.CanonicalBasePath,
                    StringComparer.Ordinal)
                ? new GeneratedNavigationMetadata(
                    value.Source,
                    SourceAuthoredMetadataFacts.Complete(
                        "Independent malformed metadata",
                        []))
                : value)
            .ToArray();
        var proof = projector.Project(new GeneratedNavigationProjectionRequest(
            formation,
            [regionInput],
            proofMetadata));
        return proof.Regions.Count == 1
            && proof.Regions[0].State == GeneratedNavigationRegionState.Available
            ? malformedChildren
            : null;
    }

    private static IReadOnlyList<string>? ReadDirectChildPaths(
        GeneratedNavigationFormation formation,
        SourceLogicalSource host)
    {
        var childPaths = host.Base.Form == SourceDocumentForm.Loader
            ? formation.Topology.LoaderRootPaths
            : formation.Topology.FindByPath(host.Identity.CanonicalBasePath)?.ChildPaths;
        if (childPaths is null)
        {
            return null;
        }

        var directChildren = new List<string>(childPaths.Count);
        foreach (var childPath in childPaths)
        {
            var child = formation.FindSource(childPath);
            if (child is null)
            {
                return null;
            }

            if (child.Base.Form != SourceDocumentForm.OverwriteCompanion)
            {
                directChildren.Add(childPath);
            }
        }

        return directChildren;
    }

    private static void AddRegionHost(
        SourceLogicalSource source,
        ISet<string> affected)
    {
        if (source.Base.Form == SourceDocumentForm.Loader
            || SourceFormClassifier.IsEntrypoint(source.Base.Form))
        {
            _ = affected.Add(source.Identity.CanonicalBasePath);
        }
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
