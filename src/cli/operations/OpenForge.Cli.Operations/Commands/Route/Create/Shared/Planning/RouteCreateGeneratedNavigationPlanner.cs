using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;

internal sealed class RouteCreateGeneratedNavigationPlanner
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private readonly GeneratedNavigationFormationBuilder _formationBuilder = new();
    private readonly GeneratedNavigationProjector _projector = new();
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly SourceAuthoredMetadataParser _metadataParser = new();
    private readonly FrameworkMarkdownDocumentWriter _documentWriter = new();
    private readonly PhysicalPathResolver _physicalPathResolver = new();
    private readonly SourceRouteFactsResolver _routeFactsResolver = new();

    internal async ValueTask<RouteCreateNavigationPlanBuild> BuildAsync(
        RouteCreateDestinationPlan destination,
        CancellationToken cancellationToken)
    {
        var inspection = destination.Inspection;
        var targetPath = inspection.Target.Path
            ?? throw new InvalidOperationException(
                "A resolved Route Create target requires a canonical path.");

        var chain = await BuildChainAsync(destination, cancellationToken)
            .ConfigureAwait(false);
        if (chain.Boundary is { } chainBoundary)
        {
            return RouteCreateNavigationPlanBuild.Stop(chainBoundary);
        }

        var resolvedChain = chain.Chain
            ?? throw new InvalidOperationException(
                "A successful Route Create chain build requires its chain.");
        var intendedSources = inspection.Catalogue.Sources
            .Where(source => !string.Equals(
                source.Identity.CanonicalBasePath,
                targetPath,
                StringComparison.Ordinal)
                && resolvedChain.Parents.All(parent => !string.Equals(
                    parent.Identity.CanonicalBasePath,
                    source.Identity.CanonicalBasePath,
                    StringComparison.Ordinal)))
            .Concat(resolvedChain.Parents)
            .Append(destination.TargetSource)
            .ToArray();
        var formation = _formationBuilder.Build(inspection.Catalogue, intendedSources);
        if (HasTargetCollision(formation, targetPath))
        {
            return Stop(
                destination,
                parent: null,
                RouteCreateFindingCode.IdentityCollision,
                "The intended Route Create target collides with another physical source.",
                isIncomplete: false);
        }

        var targetNode = formation.Topology.FindByPath(targetPath);
        if (targetNode is null || targetNode.ParentState == SourceRouteParentState.None)
        {
            return Stop(
                destination,
                parent: null,
                RouteCreateFindingCode.ParentMissing,
                "The Route Create target requires one existing routable parent.",
                isIncomplete: false);
        }

        if (targetNode.ParentState == SourceRouteParentState.Ambiguous)
        {
            return Stop(
                destination,
                parent: null,
                RouteCreateFindingCode.RouteAmbiguous,
                "The Route Create target has more than one routable parent.",
                isIncomplete: false);
        }

        var parentSource = resolvedChain.Parents[^1];
        var chainPaths = resolvedChain.Parents
            .Select(parent => parent.Identity.CanonicalBasePath)
            .Append(targetPath)
            .ToHashSet(StringComparer.Ordinal);
        if (formation.Ambiguities.FirstOrDefault(ambiguity =>
                ambiguity.IntendedSources.Any(source => chainPaths.Contains(
                    source.Identity.CanonicalBasePath))
                || ambiguity.Candidates.Any(candidate => chainPaths.Contains(
                    candidate.CanonicalPath))) is { } chainAmbiguity)
        {
            var code = chainAmbiguity.Kind == GeneratedNavigationFormationAmbiguityKind.PhysicalAlias
                ? RouteCreateFindingCode.IdentityCollision
                : RouteCreateFindingCode.RouteAmbiguous;
            return Stop(
                destination,
                parentSource,
                code,
                chainAmbiguity.Kind == GeneratedNavigationFormationAmbiguityKind.PhysicalAlias
                    ? "A prospective Route Create source aliases another physical source."
                    : "A prospective Route Create route has more than one safe parent or root entrypoint.",
                isIncomplete: false);
        }

        if (!string.Equals(
                targetNode.ParentPaths[0],
                parentSource.Identity.CanonicalBasePath,
                StringComparison.Ordinal))
        {
            return Stop(
                destination,
                parentSource,
                RouteCreateFindingCode.RouteAmbiguous,
                "The Route Create target's prospective parent chain is not stable.",
                isIncomplete: false);
        }

        var reader = new SourceDocumentReader(inspection.Request.Workspace);
        var sourceTexts = new Dictionary<string, string>(StringComparer.Ordinal);
        var parentSnapshots = new Dictionary<string, FileStateSnapshot>(StringComparer.Ordinal);
        var regionInputs = new List<GeneratedNavigationRegionInput>(resolvedChain.Parents.Count);
        foreach (var parent in resolvedChain.Parents)
        {
            var path = parent.Identity.CanonicalBasePath;
            if (resolvedChain.SyntheticBytes.TryGetValue(path, out var syntheticBytes))
            {
                var syntheticText = StrictUtf8.GetString(syntheticBytes.AsSpan());
                sourceTexts[path] = syntheticText;
                regionInputs.Add(new GeneratedNavigationRegionInput(
                    parent,
                    _markdownParser.Parse(syntheticText)));
                parentSnapshots[path] = FileStateSnapshot.Missing(
                    SourceLogicalPath.ToLexicalPath(
                        inspection.Request.Workspace.LexicalRoot,
                        path));
                continue;
            }

            var read = await reader.ReadAsync(parent.Base, cancellationToken)
                .ConfigureAwait(false);
            if (!TryReadText(read, path, out var text, out var readFailure))
            {
                return Stop(destination, parent, readFailure);
            }

            sourceTexts[path] = text;
            regionInputs.Add(new GeneratedNavigationRegionInput(
                parent,
                _markdownParser.Parse(text)));
            parentSnapshots[path] = await new SourceDocumentSnapshotReader()
                .ReadAsync(inspection.Request.Workspace, read, cancellationToken)
                .ConfigureAwait(false);
        }

        var metadata = await BuildMetadataAsync(
                destination,
                formation,
                resolvedChain.Parents,
                resolvedChain.SyntheticBytes,
                sourceTexts,
                reader,
                cancellationToken)
            .ConfigureAwait(false);
        if (metadata.Finding is { } metadataFinding)
        {
            return Stop(destination, parentSource, metadataFinding);
        }

        var projection = _projector.Project(new GeneratedNavigationProjectionRequest(
            formation,
            regionInputs,
            metadata.Values));
        var changes = ImmutableArray.CreateBuilder<RouteCreateNavigationChange>(resolvedChain.Parents.Count);
        foreach (var parent in resolvedChain.Parents)
        {
            var region = projection.Regions.Single(candidate => string.Equals(
                candidate.Source.Identity.CanonicalBasePath,
                parent.Identity.CanonicalBasePath,
                StringComparison.Ordinal));
            if (region.State != GeneratedNavigationRegionState.Available
                || region.Change is not { } navigationChange)
            {
                return Stop(destination, parent, ProjectionFinding(region));
            }

            var path = parent.Identity.CanonicalBasePath;
            changes.Add(new RouteCreateNavigationChange
            {
                Source = parent,
                Before = parentSnapshots[path],
                IntendedBytes = navigationChange.ExpectedDocumentBytes,
                RegionChange = resolvedChain.SyntheticBytes.ContainsKey(path)
                    ? null
                    : navigationChange,
            });
        }

        return RouteCreateNavigationPlanBuild.Complete(
            new RouteCreateNavigationPlan
            {
                Formation = formation,
                ParentSource = parentSource,
                NavigationChanges = changes.MoveToImmutable(),
                DirectoryCreations = resolvedChain.DirectoryCreations,
            });
    }

    private async ValueTask<NavigationChainBuild> BuildChainAsync(
        RouteCreateDestinationPlan destination,
        CancellationToken cancellationToken)
    {
        var inspection = destination.Inspection;
        var targetPath = inspection.Target.Path
            ?? throw new InvalidOperationException(
                "A resolved Route Create target requires a canonical path.");
        if (cancellationToken.IsCancellationRequested)
        {
            return StopChain(
                destination,
                [],
                RouteCreateFindingCode.Interrupted,
                "Route Create navigation planning was interrupted.",
                isIncomplete: true);
        }

        var targetDirectory = SourceLogicalPath.ReadParent(targetPath);
        var directories = ReadDirectoryChain(targetDirectory);
        if (directories.Length == 0)
        {
            return StopChain(
                destination,
                [],
                RouteCreateFindingCode.InvalidTarget,
                "The Route Create target is not below an existing recognized route root.",
                isIncomplete: false);
        }

        var parents = new List<SourceLogicalSource>(directories.Length);
        var directoryCreations = ImmutableArray.CreateBuilder<PlannedDirectoryCreation>();
        var syntheticBytes = new Dictionary<string, ImmutableArray<byte>>(StringComparer.Ordinal);
        var entrypointsByDirectory = inspection.Catalogue.Sources
            .Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form))
            .GroupBy(
                source => SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath),
                StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        SourceRouteFacts routeFacts;
        try
        {
            routeFacts = await _routeFactsResolver.ResolveAsync(
                    new SourceRouteFactsRequest(
                        inspection.Catalogue,
                        inspection.Catalogue.SelectAll()),
                    new SourceDocumentReader(inspection.Request.Workspace),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return StopChain(
                destination,
                parents,
                RouteCreateFindingCode.Interrupted,
                "Route Create Loader route planning was interrupted.",
                isIncomplete: true);
        }
        catch (Exception exception)
        {
            return StopChain(
                destination,
                parents,
                RouteCreateFindingCode.ProjectionIncomplete,
                exception.Message,
                isIncomplete: true);
        }

        if (routeFacts.IsCancelled)
        {
            return StopChain(
                destination,
                parents,
                RouteCreateFindingCode.Interrupted,
                "Route Create Loader route planning was interrupted.",
                isIncomplete: true);
        }

        if (ReadLoaderBoundary(routeFacts, directories) is { } loaderBoundary)
        {
            return StopChain(
                destination,
                parents,
                loaderBoundary.Code,
                loaderBoundary.Cause,
                loaderBoundary.IsIncomplete);
        }

        var startIndex = -1;
        for (var index = 0; index < directories.Length; index++)
        {
            if (entrypointsByDirectory.TryGetValue(directories[index], out var existing)
                && existing.Any(source => IsExposed(routeFacts, source)))
            {
                startIndex = index;
            }
        }

        if (startIndex < 0)
        {
            return StopChain(
                destination,
                parents,
                RouteCreateFindingCode.InvalidTarget,
                "The Route Create target is not below an existing recognized route root.",
                isIncomplete: false);
        }

        foreach (var group in entrypointsByDirectory
                     .Where(pair => directories
                         .Skip(startIndex)
                         .Contains(pair.Key, StringComparer.Ordinal))
                     .Select(pair => pair.Value))
        {
            if (group.Length > 1)
            {
                return StopChain(
                    destination,
                    parents,
                    RouteCreateFindingCode.RouteAmbiguous,
                    "A prospective Route Create ancestor has more than one routable entrypoint.",
                    isIncomplete: false);
            }
        }

        for (var index = startIndex; index < directories.Length; index++)
        {
            var directory = directories[index];
            var directoryPath = SourceLogicalPath.ToLexicalPath(
                inspection.Request.Workspace.LexicalRoot,
                directory);
            var directoryResolution = _physicalPathResolver.ResolveCandidate(
                inspection.Request.Workspace.LexicalRoot,
                inspection.Request.Workspace.PhysicalRoot,
                directoryPath);
            if (directoryResolution.State == PhysicalPathState.Contained)
            {
                var physicalDirectory = directoryResolution.GetContainedPhysicalPath();
                FileAttributes attributes;
                try
                {
                    attributes = File.GetAttributes(physicalDirectory);
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                {
                    return StopChain(
                        destination,
                        parents,
                        RouteCreateFindingCode.TargetUnsafe,
                        exception.Message,
                        isIncomplete: false);
                }

                if ((attributes & FileAttributes.Directory) == 0
                    || (attributes & FileAttributes.ReparsePoint) != 0)
                {
                    return StopChain(
                        destination,
                        parents,
                        RouteCreateFindingCode.TargetUnsafe,
                        "A prospective Route Create ancestor is not a safely observed ordinary directory.",
                        isIncomplete: false);
                }
            }
            else if (directoryResolution.State == PhysicalPathState.Missing)
            {
                if (index == startIndex)
                {
                    return StopChain(
                        destination,
                        parents,
                        RouteCreateFindingCode.TargetUnsafe,
                        "The recognized Route Create root directory is unavailable.",
                        isIncomplete: false);
                }

                directoryCreations.Add(PlannedDirectoryCreation.Create(
                    FileExpectation.Missing(directoryPath)));
            }
            else
            {
                return StopChain(
                    destination,
                    parents,
                    RouteCreateFindingCode.TargetUnsafe,
                    directoryResolution.Failure?.DirectCause
                        ?? "A prospective Route Create ancestor directory is unsafe.",
                    isIncomplete: false);
            }

            var entrypointPath = EntrypointPath(directory);
            if (entrypointsByDirectory.TryGetValue(directory, out var existing)
                && existing is [{ } source])
            {
                if (!IsExposed(routeFacts, source))
                {
                    return StopChain(
                        destination,
                        parents,
                        RouteCreateFindingCode.TargetUnsafe,
                        "A prospective Route Create entrypoint is not exposed by the Loader route.",
                        isIncomplete: false);
                }

                parents.Add(source);
                continue;
            }

            if (index == startIndex)
            {
                return StopChain(
                    destination,
                    parents,
                    RouteCreateFindingCode.ParentMissing,
                    "The Route Create target requires one existing routable parent.",
                    isIncomplete: false);
            }

            if (inspection.Catalogue.FindCandidateByPath(entrypointPath) is not null)
            {
                return StopChain(
                    destination,
                    parents,
                    RouteCreateFindingCode.TargetUnsafe,
                    "A prospective Route Create entrypoint path is already occupied by an unrecognized source.",
                    isIncomplete: false);
            }

            var entrypointResolution = _physicalPathResolver.ResolveCandidate(
                inspection.Request.Workspace.LexicalRoot,
                inspection.Request.Workspace.PhysicalRoot,
                SourceLogicalPath.ToLexicalPath(
                    inspection.Request.Workspace.LexicalRoot,
                    entrypointPath));
            if (entrypointResolution.State != PhysicalPathState.Missing)
            {
                return StopChain(
                    destination,
                    parents,
                    RouteCreateFindingCode.TargetUnsafe,
                    "A prospective Route Create entrypoint path is already occupied or unsafe.",
                    isIncomplete: false);
            }

            var id = SourceIdentity.DeriveId(entrypointPath)
                ?? throw new InvalidOperationException(
                    "A synthetic Route Create entrypoint requires a stable automatic ID.");
            var synthetic = new SourceLogicalSource(
                new SourceLogicalIdentity(id, entrypointPath),
                new SourceLayer(
                    entrypointPath,
                    SourceLogicalPath.ToLexicalPath(
                        inspection.Request.Workspace.PhysicalRoot,
                        entrypointPath),
                    SourceDocumentForm.CanonicalEntrypoint,
                    SourceLayerKind.Base));
            parents.Add(synthetic);
            syntheticBytes.Add(
                entrypointPath,
                _documentWriter.WriteOptional(
                    new FrameworkDocumentMetadataEmission(null, [], null),
                    IntermediateBody(id)));
        }

        return NavigationChainBuild.Complete(
            parents,
            directoryCreations.ToImmutable(),
            syntheticBytes);
    }

    private async ValueTask<NavigationMetadataBuild> BuildMetadataAsync(
        RouteCreateDestinationPlan destination,
        GeneratedNavigationFormation formation,
        IReadOnlyList<SourceLogicalSource> parents,
        IReadOnlyDictionary<string, ImmutableArray<byte>> syntheticBytes,
        IReadOnlyDictionary<string, string> sourceTexts,
        SourceDocumentReader reader,
        CancellationToken cancellationToken)
    {
        var targetSource = destination.TargetSource;
        var values = new Dictionary<string, GeneratedNavigationMetadata>(StringComparer.Ordinal);
        AddMetadata(
            values,
            targetSource,
            _metadataParser.Parse(
                _markdownParser.Parse(StrictUtf8.GetString(destination.IntendedBytes.AsSpan())),
                targetSource.Base.Form));

        foreach (var entry in syntheticBytes)
        {
            var source = formation.FindSource(entry.Key)
                ?? throw new InvalidOperationException(
                    "A synthetic Route Create entrypoint must remain in the intended formation.");
            AddMetadata(
                values,
                source,
                _metadataParser.Parse(
                    _markdownParser.Parse(StrictUtf8.GetString(entry.Value.AsSpan())),
                    source.Base.Form));
        }

        foreach (var parent in parents)
        {
            var node = formation.Topology.FindByPath(parent.Identity.CanonicalBasePath)
                ?? throw new InvalidOperationException(
                    "A prospective Route Create parent must remain in the intended topology.");
            foreach (var childPath in node.ChildPaths)
            {
                var child = formation.FindSource(childPath)
                    ?? throw new InvalidOperationException(
                        "A prospective Route Create child must remain in the intended formation.");
                if (child.Base.Form == SourceDocumentForm.OverwriteCompanion
                    || values.ContainsKey(child.Identity.CanonicalBasePath))
                {
                    continue;
                }

                if (!sourceTexts.TryGetValue(child.Identity.CanonicalBasePath, out var text))
                {
                    var read = await reader.ReadAsync(child.Base, cancellationToken)
                        .ConfigureAwait(false);
                    if (!TryReadText(
                            read,
                            child.Identity.CanonicalBasePath,
                            out text,
                            out var readFailure))
                    {
                        return NavigationMetadataBuild.Stop(readFailure);
                    }
                }

                AddMetadata(
                    values,
                    child,
                    _metadataParser.Parse(
                        _markdownParser.Parse(text),
                        child.Base.Form));
            }
        }

        return NavigationMetadataBuild.Complete(values.Values.ToArray());
    }

    private static void AddMetadata(
        IDictionary<string, GeneratedNavigationMetadata> values,
        SourceLogicalSource source,
        SourceAuthoredMetadataFacts facts)
        => values[source.Identity.CanonicalBasePath] = new GeneratedNavigationMetadata(source, facts);

    private static string[] ReadDirectoryChain(string targetDirectory)
    {
        var values = new List<string>();
        var current = targetDirectory;
        while (!string.Equals(current, SourceLogicalPath.AgentsRoot, StringComparison.Ordinal))
        {
            values.Add(current);
            current = SourceLogicalPath.ReadParent(current);
        }

        values.Reverse();
        return values.ToArray();
    }

    private static bool IsExposed(
        SourceRouteFacts routeFacts,
        SourceLogicalSource source)
    {
        var path = source.Identity.CanonicalBasePath;
        return routeFacts.Topology.LoaderRootPaths.Contains(path, StringComparer.Ordinal)
            || routeFacts.Topology.ReadAbsoluteDepth(path) is not null;
    }

    private static LoaderBoundary? ReadLoaderBoundary(
        SourceRouteFacts routeFacts,
        IReadOnlyList<string> directories)
    {
        foreach (var issue in routeFacts.Issues)
        {
            switch (issue.Code)
            {
                case SourceRouteIssueCode.LoaderDestinationMissing
                    when IsRelevantLoaderDestination(issue, directories):
                    return new LoaderBoundary(
                        RouteCreateFindingCode.ParentMissing,
                        "The Route Create target requires one existing routable parent.",
                        IsIncomplete: false);
                case SourceRouteIssueCode.LoaderUnsafe:
                    return new LoaderBoundary(
                        RouteCreateFindingCode.GeneratedRegionUnsafe,
                        issue.Cause,
                        IsIncomplete: false);
                case SourceRouteIssueCode.LoaderMalformed
                    or SourceRouteIssueCode.LoaderDuplicateRoot:
                    return new LoaderBoundary(
                        RouteCreateFindingCode.GeneratedRegionUnsafe,
                        issue.Cause,
                        IsIncomplete: false);
                case SourceRouteIssueCode.LoaderUnavailable
                    or SourceRouteIssueCode.LoaderUnreadable
                    or SourceRouteIssueCode.RouteSupportUnavailable:
                    return new LoaderBoundary(
                        RouteCreateFindingCode.ProjectionIncomplete,
                        issue.Cause,
                        IsIncomplete: true);
                case SourceRouteIssueCode.RouteAmbiguous
                    when IsRelevantRouteIssue(issue, directories):
                    return new LoaderBoundary(
                        RouteCreateFindingCode.RouteAmbiguous,
                        issue.Cause,
                        IsIncomplete: false);
            }
        }

        return null;
    }

    private static bool IsRelevantLoaderDestination(
        SourceRouteIssue issue,
        IReadOnlyList<string> directories)
    {
        if (directories.Contains(issue.CanonicalPath, StringComparer.Ordinal))
        {
            return true;
        }

        return SourceLogicalPath.IsCanonicalSource(issue.CanonicalPath)
            && directories.Contains(
                SourceLogicalPath.ReadParent(issue.CanonicalPath),
                StringComparer.Ordinal);
    }

    private static bool IsRelevantRouteIssue(
        SourceRouteIssue issue,
        IReadOnlyList<string> directories)
        => IsRelevantRoutePath(issue.CanonicalPath, directories)
            || issue.RelatedPaths.Any(path => IsRelevantRoutePath(path, directories));

    private static bool IsRelevantRoutePath(
        string path,
        IReadOnlyList<string> directories)
        => directories.Contains(path, StringComparer.Ordinal)
            || (SourceLogicalPath.IsCanonicalSource(path)
                && directories.Contains(
                    SourceLogicalPath.ReadParent(path),
                    StringComparer.Ordinal));

    private static string EntrypointPath(string directory)
        => SourceLogicalPath.Combine(
            directory,
            $"_{SourceLogicalPath.ReadFileName(directory)}.md");

    private static string IntermediateBody(string title)
        => $"""

            # {title}

            ## Axioms

            - inherited - No local axioms; loaded ancestor axioms remain active.

            ## Entries

            - none - No entries - #Empty
            """.TrimEnd('\n');

    private static bool TryReadText(
        SourceDocumentReadResult read,
        string target,
        out string text,
        [NotNullWhen(false)] out RouteCreateFinding? finding)
    {
        text = string.Empty;
        finding = null;
        if (read.Verification.State == SourceLayerVerificationState.Verified
            && read.Read?.State == Framework.Filesystem.TypedReads.Models.FileReadState.Complete
            && read.Read.Value is { } value)
        {
            text = value;
            return true;
        }

        finding = new RouteCreateFinding(
            RouteCreateFindingCode.ProjectionIncomplete,
            read.Read?.Failure?.DirectCause ?? "The routed source content is unavailable.",
            target);

        finding = read.Verification.State switch
        {
            SourceLayerVerificationState.Unsafe
                or SourceLayerVerificationState.Changed => new RouteCreateFinding(
                    RouteCreateFindingCode.GeneratedRegionUnsafe,
                    "The routed source physical identity is unsafe or changed.",
                    target),
            SourceLayerVerificationState.Cancelled => new RouteCreateFinding(
                RouteCreateFindingCode.Interrupted,
                "Route Create source reading was interrupted.",
                target),
            SourceLayerVerificationState.Verified
                or SourceLayerVerificationState.Missing
                or SourceLayerVerificationState.Unavailable => finding,
            _ => throw new ArgumentOutOfRangeException(
                nameof(read),
                read.Verification.State,
                "The source-layer verification state is not defined."),
        };
        return false;
    }

    private static bool HasTargetCollision(
        GeneratedNavigationFormation formation,
        string targetPath)
        => formation.IntendedTargetCollisions.Count != 0
            || formation.Ambiguities.Any(ambiguity =>
                ambiguity.Kind == GeneratedNavigationFormationAmbiguityKind.PhysicalAlias
                && ambiguity.IntendedSources.Any(source => string.Equals(
                    source.Identity.CanonicalBasePath,
                    targetPath,
                    StringComparison.Ordinal)));

    private static RouteCreateFinding ProjectionFinding(
        GeneratedNavigationRegion region)
    {
        var code = region.UnavailableReason switch
        {
            GeneratedNavigationRegionUnavailableReason.MetadataInvalid
                or GeneratedNavigationRegionUnavailableReason.MetadataUnrepresentable
                or GeneratedNavigationRegionUnavailableReason.TopologyUnsafe
                or GeneratedNavigationRegionUnavailableReason.DestinationUnsafe
                or GeneratedNavigationRegionUnavailableReason.DestinationConflict
                or GeneratedNavigationRegionUnavailableReason.GeneratedRegionInvalid
                or GeneratedNavigationRegionUnavailableReason.GeneratedRegionLineEndingUnsupported
                => RouteCreateFindingCode.GeneratedRegionUnsafe,
            GeneratedNavigationRegionUnavailableReason.MetadataUnavailable
                or GeneratedNavigationRegionUnavailableReason.ProjectionUnavailable
                or GeneratedNavigationRegionUnavailableReason.SourceDocumentUnavailable
                or GeneratedNavigationRegionUnavailableReason.TopologyUnavailable
                or GeneratedNavigationRegionUnavailableReason.GeneratedRegionMissing
                or GeneratedNavigationRegionUnavailableReason.GeneratedRegionUnavailable
                => RouteCreateFindingCode.ProjectionIncomplete,
            _ => RouteCreateFindingCode.GeneratedRegionUnsafe,
        };
        return new RouteCreateFinding(
            code,
            region.Cause ?? "The parent generated navigation projection is unavailable.",
            region.Source.Identity.CanonicalBasePath);
    }

    private static RouteCreateNavigationPlanBuild Stop(
        RouteCreateDestinationPlan destination,
        SourceLogicalSource? parent,
        RouteCreateFinding finding)
        => Stop(
            destination,
            parent,
            finding.Code,
            finding.Cause,
            finding.Code is RouteCreateFindingCode.MetadataIncomplete
                or RouteCreateFindingCode.ProjectionIncomplete
                or RouteCreateFindingCode.Interrupted);

    private static RouteCreateNavigationPlanBuild Stop(
        RouteCreateDestinationPlan destination,
        SourceLogicalSource? parent,
        RouteCreateFindingCode code,
        string cause,
        bool isIncomplete)
        => RouteCreateNavigationPlanBuild.Stop(Boundary(
            destination,
            parent is null ? null : RouteCreatePlanResultProjector.Parent(parent),
            new RouteCreateFinding(
                code,
                cause,
                destination.Inspection.Target.Path
                    ?? destination.Inspection.Target.Requested),
            isIncomplete));

    private static NavigationChainBuild StopChain(
        RouteCreateDestinationPlan destination,
        IReadOnlyList<SourceLogicalSource> parents,
        RouteCreateFindingCode code,
        string cause,
        bool isIncomplete)
    {
        var parent = parents.Count == 0 ? null : parents[^1];
        return NavigationChainBuild.Stop(Boundary(
            destination,
            parent is null ? null : RouteCreatePlanResultProjector.Parent(parent),
            new RouteCreateFinding(
                code,
                cause,
                destination.Inspection.Target.Path
                    ?? destination.Inspection.Target.Requested),
            isIncomplete));
    }

    private static RouteCreatePlanningBoundary Boundary(
        RouteCreateDestinationPlan destination,
        RouteCreateParent? parent,
        RouteCreateFinding finding,
        bool isIncomplete)
        => new()
        {
            Target = destination.Inspection.Target,
            Parent = parent,
            Template = destination.Template.Template,
            Finding = finding,
            IsIncomplete = isIncomplete,
        };

    private sealed class NavigationChainBuild
    {
        private NavigationChainBuild(
            NavigationChain? chain,
            RouteCreatePlanningBoundary? boundary)
        {
            Chain = chain;
            Boundary = boundary;
        }

        internal NavigationChain? Chain { get; }

        internal RouteCreatePlanningBoundary? Boundary { get; }

        internal static NavigationChainBuild Complete(
            IReadOnlyList<SourceLogicalSource> parents,
            ImmutableArray<PlannedDirectoryCreation> directoryCreations,
            IReadOnlyDictionary<string, ImmutableArray<byte>> syntheticBytes)
            => new(
                new NavigationChain(parents, directoryCreations, syntheticBytes),
                boundary: null);

        internal static NavigationChainBuild Stop(RouteCreatePlanningBoundary boundary)
            => new(chain: null, boundary);
    }

    private sealed record NavigationChain(
        IReadOnlyList<SourceLogicalSource> Parents,
        ImmutableArray<PlannedDirectoryCreation> DirectoryCreations,
        IReadOnlyDictionary<string, ImmutableArray<byte>> SyntheticBytes);

    private sealed record LoaderBoundary(
        RouteCreateFindingCode Code,
        string Cause,
        bool IsIncomplete);

    private sealed class NavigationMetadataBuild
    {
        private NavigationMetadataBuild(
            IReadOnlyList<GeneratedNavigationMetadata> values,
            RouteCreateFinding? finding)
        {
            Values = values;
            Finding = finding;
        }

        internal IReadOnlyList<GeneratedNavigationMetadata> Values { get; }

        internal RouteCreateFinding? Finding { get; }

        internal static NavigationMetadataBuild Complete(
            IReadOnlyList<GeneratedNavigationMetadata> values)
            => new(values, finding: null);

        internal static NavigationMetadataBuild Stop(RouteCreateFinding finding)
            => new([], finding);
    }
}
