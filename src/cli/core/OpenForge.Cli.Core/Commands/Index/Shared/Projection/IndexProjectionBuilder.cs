using OpenForge.Cli.Core.Commands.Index.Models.Projection;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Commands.Index.Shared.Selection;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Projection;

internal sealed class IndexProjectionBuilder
{
    private const string CatalogueUnavailableCause =
        "Relevant source catalogue facts are unavailable for this target projection.";
    private const string AcquisitionUnavailableCause =
        "Required document acquisition is unavailable for this target projection.";

    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly SourceAuthoredMetadataParser _metadataParser = new();
    private readonly GeneratedNavigationProjector _projector = new();

    internal async ValueTask<IndexProjectionFormation> BuildAsync(
        IndexProjectionContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (!context.Selection.IsComplete)
        {
            throw new ArgumentException(
                "Index projection requires a complete target selection.",
                nameof(context));
        }

        var catalogueReadiness = ResolveCatalogue(context.Formation, context.Selection);
        var findings = catalogueReadiness.Findings.ToList();
        var unavailableTargetPaths = catalogueReadiness.UnavailableTargetPaths.ToHashSet(StringComparer.Ordinal);
        var regions = new List<IndexProjectionRegionInput>();
        var metadata = new Dictionary<string, GeneratedNavigationMetadata>(StringComparer.Ordinal);
        foreach (var target in context.Selection.Targets)
        {
            var targetPath = target.Identity.CanonicalBasePath;
            if (unavailableTargetPaths.Contains(targetPath))
            {
                regions.Add(IndexProjectionRegionInput.Unavailable(target, CatalogueUnavailableCause));
                continue;
            }

            var targetDocument = await ReadDocumentAsync(
                    context,
                    target,
                    IndexProjectionDocumentRole.Target,
                    cancellationToken)
                .ConfigureAwait(false);
            if (!targetDocument.IsComplete)
            {
                findings.Add(CreateAcquisitionFinding(
                    targetDocument.ReadFindingCode(),
                    target,
                    context.Formation));
                unavailableTargetPaths.Add(targetPath);
                regions.Add(IndexProjectionRegionInput.Unavailable(target, AcquisitionUnavailableCause));
                continue;
            }

            var targetAvailable = true;
            foreach (var child in ReadDirectChildren(context.Formation, target))
            {
                if (metadata.ContainsKey(child.Identity.CanonicalBasePath))
                {
                    continue;
                }

                var childDocument = await ReadDocumentAsync(
                        context,
                        child,
                        IndexProjectionDocumentRole.Metadata,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (!childDocument.IsComplete)
                {
                    findings.Add(CreateAcquisitionFinding(
                        childDocument.ReadFindingCode(),
                        child,
                        context.Formation));
                    targetAvailable = false;
                    continue;
                }

                metadata.Add(
                    child.Identity.CanonicalBasePath,
                    new GeneratedNavigationMetadata(
                        child,
                        _metadataParser.Parse(childDocument.ReadDocument(), child.Base.Form)));
            }

            if (!targetAvailable)
            {
                unavailableTargetPaths.Add(targetPath);
                regions.Add(IndexProjectionRegionInput.Unavailable(target, AcquisitionUnavailableCause));
                continue;
            }

            var document = targetDocument.ReadDocument();
            regions.Add(IndexProjectionRegionInput.FromDocument(target, document));
        }

        var readiness = new IndexProjectionReadiness(
            DeduplicateFindings(findings),
            unavailableTargetPaths);
        return Form(new IndexProjectionAssembly
        {
            Formation = context.Formation,
            Selection = context.Selection,
            Regions = regions,
            Metadata = metadata.Values,
            Readiness = readiness,
        });
    }

    internal IndexProjectionFormation Form(IndexProjectionAssembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        var regionInputs = assembly.Regions
            .Select(region => region ?? throw new ArgumentException(
                "Index projection inputs cannot contain null members.",
                nameof(assembly)))
            .ToArray();
        var projection = _projector.Project(new GeneratedNavigationProjectionRequest(
            assembly.Formation,
            regionInputs.Select(region => region.Navigation),
            assembly.Metadata));
        var countByPath = regionInputs.ToDictionary(
            region => region.Navigation.Source.Identity.CanonicalBasePath,
            region => region.BeforeEntryCount,
            StringComparer.Ordinal);
        var findings = assembly.Readiness.Findings.ToList();
        var unavailableTargetPaths = assembly.Readiness.UnavailableTargetPaths.ToHashSet(StringComparer.Ordinal);
        foreach (var region in projection.Regions.Where(region =>
                     region.State == GeneratedNavigationRegionState.Unavailable
                     && !unavailableTargetPaths.Contains(region.CanonicalPath)))
        {
            var reason = region.UnavailableReason
                ?? throw new InvalidOperationException("An unavailable generated region requires a typed reason.");
            findings.Add(new IndexFinding(
                ReadFindingCode(reason),
                sourceOccurrence: null,
                source: IndexLogicalSourceProjector.Project(
                    source: region.Source,
                    formation: assembly.Formation),
                cause: ReadProjectionCause(reason),
                candidates: []));
        }

        return new IndexProjectionFormation(
            selection: assembly.Selection,
            projection: projection,
            regions: projection.Regions.Select(region => new IndexProjectedRegion(
                region: region,
                source: IndexLogicalSourceProjector.Project(
                    source: region.Source,
                    formation: assembly.Formation),
                beforeEntryCount: countByPath[region.CanonicalPath])),
            findings: DeduplicateFindings(findings));
    }

    internal static IndexProjectionReadiness ResolveCatalogue(
        GeneratedNavigationFormation formation,
        IndexSelectionResolution selection)
    {
        ArgumentNullException.ThrowIfNull(formation);
        ArgumentNullException.ThrowIfNull(selection);
        if (!selection.IsComplete)
        {
            throw new ArgumentException(
                "Index catalogue projection requires a complete target selection.",
                nameof(selection));
        }

        var findings = new List<IndexFinding>();
        var unavailableTargetPaths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var target in selection.Targets)
        {
            var slice = formation.Catalogue.Select(CreateSlice(formation, target));
            var targetFindings = slice.Issues
                .Select(issue => ReadCatalogueFinding(issue, target, formation))
                .Where(finding => finding is not null)
                .Cast<IndexFinding>()
                .ToArray();
            if (targetFindings.Length == 0)
            {
                continue;
            }

            findings.AddRange(targetFindings);
            unavailableTargetPaths.Add(target.Identity.CanonicalBasePath);
        }

        return new IndexProjectionReadiness(
            DeduplicateFindings(findings),
            unavailableTargetPaths);
    }

    internal static IndexFindingCode? ReadAcquisitionFindingCode(
        SourceDocumentReadResult result,
        IndexProjectionDocumentRole role)
    {
        ArgumentNullException.ThrowIfNull(result);
        _ = role switch
        {
            IndexProjectionDocumentRole.Target or IndexProjectionDocumentRole.Metadata => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(role),
                role,
                "The Index projection document role is not defined."),
        };

        return result.Verification.State switch
        {
            SourceLayerVerificationState.Verified => ReadVerifiedFindingCode(result, role),
            SourceLayerVerificationState.Missing
                or SourceLayerVerificationState.Changed => IndexFindingCode.TargetChanged,
            SourceLayerVerificationState.Unsafe => IndexFindingCode.TargetUnsafe,
            SourceLayerVerificationState.Unavailable => ReadUnavailableFindingCode(role),
            SourceLayerVerificationState.Cancelled => IndexFindingCode.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(result),
                result.Verification.State,
                "The source-layer verification state is not defined."),
        };
    }

    internal static IndexFindingCode ReadFindingCode(
        GeneratedNavigationRegionUnavailableReason reason)
        => reason switch
        {
            GeneratedNavigationRegionUnavailableReason.RegionSourceUnsupported
                or GeneratedNavigationRegionUnavailableReason.TopologyUnsafe
                or GeneratedNavigationRegionUnavailableReason.DestinationUnsafe
                or GeneratedNavigationRegionUnavailableReason.DestinationConflict => IndexFindingCode.TargetUnsafe,
            GeneratedNavigationRegionUnavailableReason.GeneratedRegionMissing
                or GeneratedNavigationRegionUnavailableReason.GeneratedRegionInvalid
                or GeneratedNavigationRegionUnavailableReason.GeneratedRegionLineEndingUnsupported => IndexFindingCode.GeneratedRegionUnsafe,
            GeneratedNavigationRegionUnavailableReason.MetadataInvalid
                or GeneratedNavigationRegionUnavailableReason.MetadataUnrepresentable => IndexFindingCode.MetadataUnsafe,
            GeneratedNavigationRegionUnavailableReason.TopologyUnavailable => IndexFindingCode.DiscoveryIncomplete,
            GeneratedNavigationRegionUnavailableReason.MetadataUnavailable => IndexFindingCode.MetadataIncomplete,
            GeneratedNavigationRegionUnavailableReason.SourceDocumentUnavailable
                or GeneratedNavigationRegionUnavailableReason.GeneratedRegionUnavailable
                or GeneratedNavigationRegionUnavailableReason.ProjectionUnavailable => IndexFindingCode.ProjectionIncomplete,
            _ => throw new ArgumentOutOfRangeException(
                nameof(reason),
                reason,
                "The generated-region unavailable reason is not defined."),
        };

    private async ValueTask<IndexProjectionDocumentAcquisition> ReadDocumentAsync(
        IndexProjectionContext context,
        SourceLogicalSource source,
        IndexProjectionDocumentRole role,
        CancellationToken cancellationToken)
    {
        var result = await context.Reader
            .ReadAsync(source.Base, cancellationToken)
            .ConfigureAwait(false);
        var findingCode = ReadAcquisitionFindingCode(result, role);
        if (findingCode is not null)
        {
            return IndexProjectionDocumentAcquisition.Unavailable(findingCode.Value);
        }

        var sourceText = result.Read?.Value
            ?? throw new InvalidOperationException("A complete Index source acquisition requires its text.");
        return IndexProjectionDocumentAcquisition.Complete(_markdownParser.Parse(sourceText));
    }

    private static SourceCatalogueSelectionRequest CreateSlice(
        GeneratedNavigationFormation formation,
        SourceLogicalSource target)
    {
        var children = ReadDirectChildren(formation, target);
        var sources = children
            .Prepend(target)
            .DistinctBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal);
        var includedScope = CreateScope(target);
        var excludedScopes = children
            .Where(child => SourceFormClassifier.IsEntrypoint(child.Base.Form))
            .Select(CreateScope)
            .DistinctBy(scope => scope.CanonicalDirectoryPath, StringComparer.Ordinal);
        return new SourceCatalogueSelectionRequest(sources, [includedScope], excludedScopes);
    }

    private static SourceCatalogueSelectionScope CreateScope(SourceLogicalSource source)
    {
        var physicalDirectory = Path.GetDirectoryName(source.Base.PhysicalPath)
            ?? throw new InvalidOperationException("An Index projection source requires a physical parent directory.");
        return new SourceCatalogueSelectionScope(
            SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath),
            physicalDirectory);
    }

    private static IReadOnlyList<SourceLogicalSource> ReadDirectChildren(
        GeneratedNavigationFormation formation,
        SourceLogicalSource target)
    {
        IReadOnlyList<string> childPaths = target.Base.Form == SourceDocumentForm.Loader
            ? formation.Topology.LoaderRootPaths
            : formation.Topology.FindByPath(target.Identity.CanonicalBasePath)?.ChildPaths
                ?? throw new InvalidOperationException("An Index projection target must retain its topology node.");
        return childPaths
            .Select(path => formation.FindSource(path)
                ?? throw new InvalidOperationException("An Index projection child path must retain its formation source."))
            .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
    }

    private static IndexFinding? ReadCatalogueFinding(
        SourceCatalogueIssue issue,
        SourceLogicalSource target,
        GeneratedNavigationFormation formation)
    {
        IndexFindingCode? code;
        if (issue.Code == SourceCatalogueIssueCode.PhysicalAlias)
        {
            if (ReadAliasCompatibility(issue, formation) == GeneratedNavigationPhysicalAliasCompatibility.Compatible)
            {
                return null;
            }

            code = IndexFindingCode.TargetUnsafe;
        }
        else
        {
            code = ReadCatalogueFindingCode(issue.Code);
        }
        if (code is null)
        {
            return null;
        }

        return new IndexFinding(
            code.Value,
            sourceOccurrence: null,
            source: IndexLogicalSourceProjector.Project(
                source: target,
                formation: formation),
            cause: ReadCatalogueCause(issue.Code),
            candidates: []);
    }

    internal static IndexFindingCode? ReadCatalogueFindingCode(SourceCatalogueIssueCode code)
    {
        return code switch
        {
            SourceCatalogueIssueCode.RootMissing
                or SourceCatalogueIssueCode.RootUnsafe
                or SourceCatalogueIssueCode.RootUnavailable => null,
            SourceCatalogueIssueCode.DirectoryUnavailable
                or SourceCatalogueIssueCode.CandidateUnavailable
                or SourceCatalogueIssueCode.IdentityUnavailable => IndexFindingCode.DiscoveryIncomplete,
            SourceCatalogueIssueCode.CandidateUnsafe => IndexFindingCode.TargetUnsafe,
            SourceCatalogueIssueCode.IdentityCollision
                or SourceCatalogueIssueCode.OrphanOverwrite => null,
            SourceCatalogueIssueCode.PhysicalAlias => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "A physical-alias issue requires formation compatibility."),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The source catalogue issue code is not defined."),
        };
    }

    private static GeneratedNavigationPhysicalAliasCompatibility ReadAliasCompatibility(
        SourceCatalogueIssue issue,
        GeneratedNavigationFormation formation)
    {
        var issuePaths = issue.RelatedPaths
            .Append(issue.AttemptedCanonicalPath)
            .ToHashSet(StringComparer.Ordinal);
        var groups = formation.PhysicalAliasGroups
            .Where(group => group.Candidates.Count(candidate => issuePaths.Contains(candidate.CanonicalPath)) >= 2)
            .ToArray();
        return groups.Length == 1
            ? groups[0].Compatibility
            : throw new InvalidOperationException(
                "A selected physical-alias issue must retain one exact formation alias group.");
    }

    private static string ReadCatalogueCause(SourceCatalogueIssueCode code)
    {
        return code switch
        {
            SourceCatalogueIssueCode.DirectoryUnavailable =>
                "A directory required for the selected target projection could not be completely inspected.",
            SourceCatalogueIssueCode.CandidateUnsafe =>
                "A source candidate required by the selected target projection crosses an unsafe boundary.",
            SourceCatalogueIssueCode.CandidateUnavailable =>
                "A source candidate required by the selected target projection could not be inspected.",
            SourceCatalogueIssueCode.IdentityUnavailable =>
                "A source identity required by the selected target projection could not be established.",
            SourceCatalogueIssueCode.PhysicalAlias =>
                "Direct routed children have incompatible current-host physical aliases.",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The catalogue issue does not produce a projection finding."),
        };
    }

    private static IndexFindingCode? ReadVerifiedFindingCode(
        SourceDocumentReadResult result,
        IndexProjectionDocumentRole role)
    {
        var state = result.Read?.State
            ?? throw new InvalidOperationException("A verified source layer requires a document read.");
        return state switch
        {
            FileReadState.Complete => null,
            FileReadState.Missing => IndexFindingCode.TargetChanged,
            FileReadState.InvalidEncoding
                or FileReadState.InvalidSyntax => ReadInvalidDocumentFindingCode(role),
            FileReadState.AccessDenied
                or FileReadState.InputOutputFailure => ReadUnavailableFindingCode(role),
            FileReadState.Cancelled => IndexFindingCode.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(result), state, "The source document read state is not defined."),
        };
    }

    private static IndexFindingCode ReadUnavailableFindingCode(IndexProjectionDocumentRole role)
        => role switch
        {
            IndexProjectionDocumentRole.Target => IndexFindingCode.ProjectionIncomplete,
            IndexProjectionDocumentRole.Metadata => IndexFindingCode.MetadataIncomplete,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "The Index projection document role is not defined."),
        };

    private static IndexFindingCode ReadInvalidDocumentFindingCode(IndexProjectionDocumentRole role)
        => role switch
        {
            IndexProjectionDocumentRole.Target => IndexFindingCode.TargetUnsafe,
            IndexProjectionDocumentRole.Metadata => IndexFindingCode.MetadataUnsafe,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "The Index projection document role is not defined."),
        };

    private static IndexFinding CreateAcquisitionFinding(
        IndexFindingCode code,
        SourceLogicalSource source,
        GeneratedNavigationFormation formation)
    {
        var cause = code switch
        {
            IndexFindingCode.TargetChanged =>
                "A selected target or direct routed dependency changed after catalogue formation.",
            IndexFindingCode.TargetUnsafe =>
                "A selected target or direct routed dependency no longer has its accepted contained identity.",
            IndexFindingCode.MetadataUnsafe =>
                "A direct routed child document cannot be safely decoded for authored metadata.",
            IndexFindingCode.MetadataIncomplete =>
                "A direct routed child document could not be read for authored metadata.",
            IndexFindingCode.ProjectionIncomplete =>
                "A selected target document could not be read for exact generated navigation projection.",
            IndexFindingCode.Interrupted =>
                "Index projection was interrupted before document acquisition completed.",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The finding is not a document-acquisition finding."),
        };
        return new IndexFinding(
            code,
            sourceOccurrence: null,
            source: IndexLogicalSourceProjector.Project(
                source: source,
                formation: formation),
            cause: cause,
            candidates: []);
    }

    private static string ReadProjectionCause(GeneratedNavigationRegionUnavailableReason reason)
    {
        return ReadFindingCode(reason) switch
        {
            IndexFindingCode.TargetUnsafe =>
                "The selected target or one of its direct routed dependencies is unsafe for generated navigation.",
            IndexFindingCode.GeneratedRegionUnsafe =>
                "The selected target does not contain one safe complete generated Entries region.",
            IndexFindingCode.MetadataUnsafe =>
                "Authored metadata for one direct routed child cannot be safely projected.",
            IndexFindingCode.DiscoveryIncomplete =>
                "The selected target topology could not be completely established.",
            IndexFindingCode.MetadataIncomplete =>
                "Required authored metadata for one direct routed child is unavailable.",
            IndexFindingCode.ProjectionIncomplete =>
                "The exact expected generated navigation projection is unavailable.",
            _ => throw new InvalidOperationException("The generated-region reason did not map to a projection finding."),
        };
    }

    private static IReadOnlyList<IndexFinding> DeduplicateFindings(IEnumerable<IndexFinding> findings)
    {
        return findings
            .DistinctBy(finding => (
                finding.Code,
                finding.SourceOccurrence,
                SourcePath: finding.Source?.Path,
                SourceId: finding.Source?.Id,
                finding.Cause))
            .ToArray();
    }

    private sealed record IndexProjectionDocumentAcquisition
    {
        private IndexProjectionDocumentAcquisition(
            MarkdownDocumentFacts? document,
            IndexFindingCode? findingCode)
        {
            Document = document;
            FindingCode = findingCode;
        }

        internal bool IsComplete => FindingCode is null;

        private MarkdownDocumentFacts? Document { get; }

        private IndexFindingCode? FindingCode { get; }

        internal MarkdownDocumentFacts ReadDocument()
            => Document
                ?? throw new InvalidOperationException("A complete Index document acquisition requires its document.");

        internal IndexFindingCode ReadFindingCode()
            => FindingCode
                ?? throw new InvalidOperationException("An unavailable Index document acquisition requires its finding code.");

        internal static IndexProjectionDocumentAcquisition Complete(MarkdownDocumentFacts document)
        {
            ArgumentNullException.ThrowIfNull(document);
            return new(document, null);
        }

        internal static IndexProjectionDocumentAcquisition Unavailable(IndexFindingCode findingCode)
        {
            return new(null, findingCode);
        }
    }
}

internal enum IndexProjectionDocumentRole
{
    Target,
    Metadata,
}
