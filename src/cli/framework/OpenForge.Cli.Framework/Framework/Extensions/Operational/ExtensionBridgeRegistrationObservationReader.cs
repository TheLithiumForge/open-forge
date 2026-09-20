using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models.Registration;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Extensions.Operational;

internal sealed class ExtensionBridgeRegistrationObservationReader
{
    private const string ProjectionHost = "# Projection\n\n## Entries\n\n"
        + MarkdownEntriesSectionReader.EmptyEntry + "\n";
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly SourceCatalogueReader _catalogueReader = new();
    private readonly GeneratedNavigationFormationBuilder _formationBuilder = new();
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly SourceAuthoredMetadataParser _metadataParser = new();
    private readonly GeneratedNavigationProjector _projector = new();

    internal async ValueTask<ExtensionBridgeRegistrationFacts> ReadAsync(
        CliWorkspace workspace,
        WorkspaceOwnershipDocument ownership,
        IReadOnlyList<ExtensionSourceObservation> sources,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(ownership);
        ArgumentNullException.ThrowIfNull(sources);
        if (cancellationToken.IsCancellationRequested)
        {
            return ExtensionBridgeRegistrationFacts.Incomplete(
                "Extension bridge-registration observation was interrupted.");
        }

        var candidateRead = ReadCandidates(ownership, sources);
        if (candidateRead.Boundary is { } candidateBoundary)
        {
            return candidateBoundary;
        }

        var candidates = candidateRead.Candidates;
        if (candidates.Count == 0)
        {
            return ExtensionBridgeRegistrationFacts.Complete([]);
        }

        var catalogue = await _catalogueReader.ReadAsync(
            new SourceCatalogueRequest(workspace, [SourceLogicalPath.AgentsRoot]),
            cancellationToken).ConfigureAwait(false);
        if (catalogue.IsCancelled)
        {
            return ExtensionBridgeRegistrationFacts.Incomplete(
                "Extension bridge-registration topology observation was interrupted.");
        }

        var materialIssues = catalogue.Issues
            .Where(issue => issue.Code != SourceCatalogueIssueCode.RootMissing
                && (!candidates.Any(candidate => string.Equals(
                        candidate.TargetPath,
                        issue.AttemptedCanonicalPath,
                        StringComparison.Ordinal))
                    || IsAmbiguous(issue)))
            .ToArray();
        if (materialIssues.Any(IsAmbiguous))
        {
            return ExtensionBridgeRegistrationFacts.Blocked(
                "The current route catalogue contains an ambiguous registration mapping.");
        }

        if (materialIssues.Length > 0)
        {
            return ExtensionBridgeRegistrationFacts.Incomplete(
                "The current route catalogue is not completely readable.");
        }

        var bytesByPath = candidates
            .GroupBy(candidate => candidate.TargetPath, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => ReadSharedBytes(group),
                StringComparer.Ordinal);
        if (bytesByPath.Values.Any(value => value is null))
        {
            return ExtensionBridgeRegistrationFacts.Blocked(
                "Lifecycle owners disagree on exact reviewed bridge-registration source bytes.");
        }

        var overlay = bytesByPath.ToDictionary(
            pair => pair.Key,
            pair => pair.Value ?? throw new InvalidOperationException(
                "A completed bridge-registration byte group requires bytes."),
            StringComparer.Ordinal);
        var intendedSources = catalogue.Sources
            .Where(source => !overlay.ContainsKey(source.Identity.CanonicalBasePath))
            .Concat(overlay.Keys.Select(path => CreateSource(workspace, path)))
            .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        var formation = _formationBuilder.Build(catalogue, intendedSources);
        if (formation.Ambiguities.Count > 0 || formation.IntendedTargetCollisions.Count > 0)
        {
            return ExtensionBridgeRegistrationFacts.Blocked(
                "Reviewed Extension targets do not form one unambiguous generated-navigation mapping.");
        }

        var documents = await ReadDocumentsAsync(
            intendedSources,
            overlay,
            cancellationToken).ConfigureAwait(false);
        if (documents is null)
        {
            return ExtensionBridgeRegistrationFacts.Incomplete(
                "Current authored route documents are not completely readable.");
        }

        GeneratedNavigationProjection projection;
        try
        {
            var metadata = intendedSources
                .Where(source => source.Base.Form != SourceDocumentForm.Loader)
                .Select(source => new GeneratedNavigationMetadata(
                    source,
                    _metadataParser.Parse(
                        _markdownParser.Parse(documents[source.Identity.CanonicalBasePath]),
                        source.Base.Form)))
                .ToArray();
            var projectionDocument = _markdownParser.Parse(ProjectionHost);
            var regions = intendedSources
                .Where(source => source.Base.Form == SourceDocumentForm.Loader
                    || SourceFormClassifier.IsEntrypoint(source.Base.Form))
                .Select(source => new GeneratedNavigationRegionInput(source, projectionDocument))
                .ToArray();
            projection = _projector.Project(new GeneratedNavigationProjectionRequest(
                formation,
                regions,
                metadata));
        }
        catch (Exception exception) when (exception is ArgumentException
            or DecoderFallbackException
            or InvalidDataException)
        {
            return ExtensionBridgeRegistrationFacts.Incomplete(
                $"Reviewed Extension registration projection is unavailable: {exception.Message}");
        }

        if (!projection.IsComplete)
        {
            return ExtensionBridgeRegistrationFacts.Incomplete(
                projection.Regions.First(region => region.State != GeneratedNavigationRegionState.Available)
                    .Cause
                    ?? "Reviewed Extension registration projection is unavailable.");
        }

        var observations = new List<ExtensionBridgeRegistrationObservation>();
        foreach (var candidate in candidates.OrderBy(
                     candidate => candidate.TargetPath,
                     StringComparer.Ordinal).ThenBy(candidate => candidate.PackageId, StringComparer.Ordinal))
        {
            var matches = projection.Regions
                .SelectMany(region => region.Entries.Select(entry => (Region: region, Entry: entry)))
                .Where(value => string.Equals(
                    value.Entry.CanonicalPath,
                    candidate.TargetPath,
                    StringComparison.Ordinal))
                .ToArray();
            if (matches.Length == 0)
            {
                continue;
            }

            if (matches.Length > 1)
            {
                return ExtensionBridgeRegistrationFacts.Blocked(
                    "One reviewed Extension target maps to multiple generated-navigation registrations.");
            }

            var match = matches[0];
            var observed = await ObserveAsync(
                candidate,
                match.Region,
                match.Entry,
                cancellationToken).ConfigureAwait(false);
            if (observed.Boundary is { } observationBoundary)
            {
                return observationBoundary;
            }

            observations.Add(observed.Observation
                ?? throw new InvalidOperationException(
                    "A complete bridge-registration observation requires facts."));
        }

        return ExtensionBridgeRegistrationFacts.Complete(observations);
    }

    private static CandidateRead ReadCandidates(
        WorkspaceOwnershipDocument ownership,
        IReadOnlyList<ExtensionSourceObservation> sources)
    {
        var packages = ownership.Extensions.ToDictionary(package => package.Id, StringComparer.Ordinal);
        var candidates = new List<BridgeCandidate>();
        foreach (var path in ownership.Extensions.SelectMany(package => package.Paths).Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal))
        {
            var ownerCandidates = new List<BridgeCandidate>();
            foreach (var owner in ownership.OwnersOf(path).Order(StringComparer.Ordinal))
            {
                if (!packages.TryGetValue(owner, out var installed))
                {
                    return CandidateRead.Stop(
                        "Lifecycle ownership does not identify one exact reviewed Extension source.");
                }

                var sourceMatches = sources.Where(source => string.Equals(
                    source.RecordedSource,
                    installed.Source,
                    StringComparison.Ordinal)).ToArray();
                if (sourceMatches.Length > 1)
                {
                    return CandidateRead.Blocked(
                        "A lifecycle owner maps to multiple reviewed Extension source observations.");
                }

                if (sourceMatches.Length == 0
                    || sourceMatches[0].Read.State != Extensions.Models.ExtensionSourceReadState.Complete)
                {
                    return CandidateRead.Stop(
                        sourceMatches.SingleOrDefault()?.Read.Cause
                            ?? "An exact reviewed Extension source is unavailable.");
                }

                var source = sourceMatches[0].Read;
                var packageMatches = source.Packages.Where(package => string.Equals(
                    package.Id,
                    owner,
                    StringComparison.Ordinal)).ToArray();
                if (packageMatches.Length != 1)
                {
                    return CandidateRead.Blocked(
                        "A reviewed Extension source does not identify one exact lifecycle owner package.");
                }

                var files = packageMatches[0].Payload.Where(file => string.Equals(
                    file.TargetPath,
                    path,
                    StringComparison.Ordinal)).ToArray();
                if (files.Length > 1)
                {
                    return CandidateRead.Blocked(
                        "A reviewed Extension package maps multiple payload facts to one lifecycle target.");
                }

                if (files.Length == 0
                    || files[0].State != Extensions.Models.ExtensionPackageFileReadState.Available
                    || files[0].Bytes is not { } bytes)
                {
                    return CandidateRead.Stop(
                        "A lifecycle-owned Extension target has no exact readable reviewed source payload fact.");
                }

                if (!SourceFormClassifier.TryClassify(path, out var form)
                    || form == SourceDocumentForm.OverwriteCompanion)
                {
                    continue;
                }

                ownerCandidates.Add(new BridgeCandidate(
                    path,
                    ownership.OwnersOf(path),
                    owner,
                    source.Identity,
                    files[0].Path,
                    bytes.ToArray()));
            }

            if (ownerCandidates.Count == 0)
            {
                continue;
            }

            var canonical = ownerCandidates[0];
            if (ownerCandidates.Skip(1).Any(candidate =>
                    !candidate.Bytes.AsSpan().SequenceEqual(canonical.Bytes)))
            {
                return CandidateRead.Blocked(
                    "Lifecycle owners disagree on exact reviewed bridge-registration source bytes.");
            }

            candidates.Add(canonical);
        }

        return CandidateRead.Complete(candidates);
    }

    private async ValueTask<IReadOnlyDictionary<string, string>?> ReadDocumentsAsync(
        IReadOnlyList<SourceLogicalSource> sources,
        IReadOnlyDictionary<string, byte[]> overlay,
        CancellationToken cancellationToken)
    {
        var documents = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var source in sources)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            var path = source.Identity.CanonicalBasePath;
            if (overlay.TryGetValue(path, out var bytes))
            {
                try
                {
                    documents.Add(path, StrictUtf8.GetString(bytes));
                }
                catch (DecoderFallbackException)
                {
                    return null;
                }

                continue;
            }

            var read = await StrictUtf8FileReader.ReadAsync(
                source.Base.PhysicalPath,
                path,
                cancellationToken).ConfigureAwait(false);
            if (read.State != FileReadState.Complete || read.Value is not { } text)
            {
                return null;
            }

            documents.Add(path, text);
        }

        return documents;
    }

    private async ValueTask<ObservationRead> ObserveAsync(
        BridgeCandidate candidate,
        GeneratedNavigationRegion region,
        GeneratedNavigationEntry expected,
        CancellationToken cancellationToken)
    {
        var read = await StrictUtf8FileReader.ReadAsync(
            region.Source.Base.PhysicalPath,
            region.CanonicalPath,
            cancellationToken).ConfigureAwait(false);
        if (read.State == FileReadState.Cancelled)
        {
            return ObservationRead.Stop(
                "Extension bridge-registration observation was interrupted.");
        }

        if (read.State != FileReadState.Complete || read.Value is not { } text)
        {
            return ObservationRead.Complete(Create(
                candidate,
                new BridgeObservationComparison
                {
                    ParentPath = region.CanonicalPath,
                    ExpectedEntry = expected.Line,
                    State = ExtensionBridgeRegistrationState.Unreadable,
                    Cause = read.Failure?.DirectCause
                        ?? "The generated-navigation parent is unreadable.",
                }));
        }

        var document = _markdownParser.Parse(text);
        var current = SourceGeneratedEntriesParser.Parse(document);
        if (current.State != SourceGeneratedEntriesState.Complete)
        {
            return ObservationRead.Complete(Create(
                candidate,
                new BridgeObservationComparison
                {
                    ParentPath = region.CanonicalPath,
                    ExpectedEntry = expected.Line,
                    State = ExtensionBridgeRegistrationState.Unreadable,
                    Cause = current.Cause
                        ?? "The generated-navigation parent Entries region is unreadable.",
                }));
        }

        var comparisons = RouteGeneratedEntryComparisonReader.Read(
            region.Source,
            current,
            region.Entries,
            new Utf8SourceMap(text));
        var missing = comparisons.Any(comparison =>
            comparison.Kind == RouteGeneratedEntryComparisonKind.Missing
            && string.Equals(comparison.Expected, expected.Destination, StringComparison.Ordinal));
        var entries = current.Entries.Where(entry => string.Equals(
            SourceGeneratedDestinationResolver.Resolve(
                region.CanonicalPath,
                region.Source.Base.Form == SourceDocumentForm.Loader,
                entry.Destination),
            candidate.TargetPath,
            StringComparison.Ordinal)).ToArray();
        if (entries.Length == 0)
        {
            if (!missing)
            {
                return ObservationRead.Blocked(
                    "Generated-entry comparison did not retain one exact missing target mapping.");
            }

            return ObservationRead.Complete(Create(
                candidate,
                new BridgeObservationComparison
                {
                    ParentPath = region.CanonicalPath,
                    ExpectedEntry = expected.Line,
                    State = ExtensionBridgeRegistrationState.Missing,
                }));
        }

        if (entries.Length > 1)
        {
            return ObservationRead.Blocked(
                "The current generated-navigation parent maps multiple entries to one Extension target.");
        }

        var span = entries[0].Span;
        var actual = text[span.Start..span.End];
        var comparison = comparisons.FirstOrDefault(value => value.Kind switch
        {
            RouteGeneratedEntryComparisonKind.Order => true,
            RouteGeneratedEntryComparisonKind.Path =>
                string.Equals(value.Expected, expected.Destination, StringComparison.Ordinal),
            RouteGeneratedEntryComparisonKind.Description =>
                string.Equals(value.Expected, expected.Description, StringComparison.Ordinal),
            RouteGeneratedEntryComparisonKind.Tags =>
                string.Equals(
                    value.Expected,
                    string.Join(',', expected.Tags),
                    StringComparison.Ordinal),
            RouteGeneratedEntryComparisonKind.Missing
                or RouteGeneratedEntryComparisonKind.Extra => false,
            _ => throw new ArgumentOutOfRangeException(
                nameof(value),
                value.Kind,
                "The generated-entry comparison kind is not defined."),
        });
        var currentEntry = comparison?.Kind
            == RouteGeneratedEntryComparisonKind.Order
            ? comparison.Actual
                ?? throw new InvalidOperationException(
                    "An order comparison requires its actual order.")
            : actual;
        return ObservationRead.Complete(Create(
            candidate,
            new BridgeObservationComparison
            {
                ParentPath = region.CanonicalPath,
                ExpectedEntry = expected.Line,
                ActualEntry = currentEntry,
                State = comparison is null
                    && string.Equals(expected.Line, actual, StringComparison.Ordinal)
                    ? ExtensionBridgeRegistrationState.Current
                    : ExtensionBridgeRegistrationState.Inconsistent,
            }));
    }

    private static ExtensionBridgeRegistrationObservation Create(
        BridgeCandidate candidate,
        BridgeObservationComparison comparison)
        => new(candidate, comparison);

    private static byte[]? ReadSharedBytes(IEnumerable<BridgeCandidate> candidates)
    {
        var values = candidates.ToArray();
        var first = values[0].Bytes;
        return values.Skip(1).All(value => value.Bytes.AsSpan().SequenceEqual(first))
            ? first
            : null;
    }

    private static SourceLogicalSource CreateSource(CliWorkspace workspace, string path)
    {
        if (!SourceFormClassifier.TryClassify(path, out var form))
        {
            throw new InvalidDataException("A reviewed Extension target is not an authored source.");
        }

        var id = SourceIdentity.DeriveId(path)
            ?? throw new InvalidDataException("A reviewed Extension target has no canonical identity.");
        var physicalPath = Path.GetFullPath(Path.Combine(
            workspace.PhysicalRoot,
            path.Replace('/', Path.DirectorySeparatorChar)));
        return new SourceLogicalSource(
            new SourceLogicalIdentity(id, path),
            new SourceLayer(path, physicalPath, form, SourceLayerKind.Base));
    }

    private static bool IsAmbiguous(SourceCatalogueIssue issue)
        => issue.Code is SourceCatalogueIssueCode.IdentityUnavailable
            or SourceCatalogueIssueCode.EntrypointAmbiguous
            or SourceCatalogueIssueCode.EntrypointCompatibilityCollision
            or SourceCatalogueIssueCode.IdentityCollision
            or SourceCatalogueIssueCode.PhysicalAlias;

    private sealed record CandidateRead(
        IReadOnlyList<BridgeCandidate> Candidates,
        ExtensionBridgeRegistrationFacts? Boundary)
    {
        internal static CandidateRead Complete(IReadOnlyList<BridgeCandidate> candidates)
            => new(candidates, Boundary: null);

        internal static CandidateRead Stop(string cause)
            => new([], ExtensionBridgeRegistrationFacts.Incomplete(cause));

        internal static CandidateRead Blocked(string cause)
            => new([], ExtensionBridgeRegistrationFacts.Blocked(cause));
    }

    private sealed record ObservationRead(
        ExtensionBridgeRegistrationObservation? Observation,
        ExtensionBridgeRegistrationFacts? Boundary)
    {
        internal static ObservationRead Complete(ExtensionBridgeRegistrationObservation observation)
            => new(observation, Boundary: null);

        internal static ObservationRead Stop(string cause)
            => new(Observation: null, ExtensionBridgeRegistrationFacts.Incomplete(cause));

        internal static ObservationRead Blocked(string cause)
            => new(Observation: null, ExtensionBridgeRegistrationFacts.Blocked(cause));
    }
}
