using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal sealed record RouteMovePostMoveExpectationProjection
{
    public ImmutableArray<RouteMovePostMoveObservation> DestinationLayout { get; init; } = [];

    public ImmutableArray<RouteMovePostMoveObservation> OldSourceAbsence { get; init; } = [];

    public ImmutableArray<RouteMovePostMoveObservation> ReferenceDocuments { get; init; } = [];

    public ImmutableArray<RouteMovePostMoveObservation> GeneratedDocuments { get; init; } = [];

    public required RouteMovePostMoveObservation Lifecycle { get; init; }

    internal static RouteMovePostMoveExpectationProjection Build(RouteMovePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var effects = plan.Projection.FileChanges.ToDictionary(
            change => change.LogicalPath,
            StringComparer.Ordinal);
        return new RouteMovePostMoveExpectationProjection
        {
            DestinationLayout = BuildDestinationLayout(plan),
            OldSourceAbsence = BuildOldSourceAbsence(plan),
            ReferenceDocuments = BuildReferenceDocuments(plan, effects),
            GeneratedDocuments = BuildGeneratedDocuments(plan, effects),
            Lifecycle = new RouteMovePostMoveObservation(
                "lifecycle",
                plan.Projection.Ownership.Snapshot?.Expectation
                    ?? throw new InvalidOperationException(
                        "A complete Route Move plan requires one exact lifecycle expectation.")),
        };
    }

    internal IEnumerable<RouteMovePostMoveObservation> All()
        => DestinationLayout.Concat(OldSourceAbsence)
            .Concat(ReferenceDocuments)
            .Concat(GeneratedDocuments)
            .Append(Lifecycle);

    private static ImmutableArray<RouteMovePostMoveObservation> BuildDestinationLayout(
        RouteMovePlan plan)
    {
        var values = new List<RouteMovePostMoveObservation>();
        values.AddRange(plan.Projection.DirectoryCreations.Select(creation =>
            new RouteMovePostMoveObservation(
                "destination-directory",
                DirectoryAfter(plan, creation.LogicalPath))));
        values.AddRange(plan.Projection.FileChanges.Where(change =>
                change.Kind != PlannedFileChangeKind.Delete)
            .Select(change => new RouteMovePostMoveObservation(
                "destination-file",
                FileAfter(plan, change))));
        return [.. values];
    }

    private static ImmutableArray<RouteMovePostMoveObservation> BuildOldSourceAbsence(
        RouteMovePlan plan)
        => [.. plan.Projection.FileChanges.Where(change =>
                change.Kind == PlannedFileChangeKind.Delete)
            .Select(change => new RouteMovePostMoveObservation(
                "old-source-file",
                FileExpectation.Missing(change.LogicalPath)))
            .Concat(plan.Projection.DirectoryDeletions.Select(deletion =>
                new RouteMovePostMoveObservation(
                    "old-source-directory",
                    FileExpectation.Missing(deletion.LogicalPath))))];

    private static ImmutableArray<RouteMovePostMoveObservation> BuildReferenceDocuments(
        RouteMovePlan plan,
        IReadOnlyDictionary<string, PlannedFileChange> effects)
    {
        var root = plan.Request.Workspace.LexicalRoot;
        return [.. plan.Projection.References.Documents.Select(document =>
        {
            var path = Path.Combine(
                root,
                document.DestinationSourcePath.Replace('/', Path.DirectorySeparatorChar));
            return new RouteMovePostMoveObservation(
                "reference-document",
                ExpectedDocument(plan, effects, document.Snapshot, path));
        })];
    }

    private static ImmutableArray<RouteMovePostMoveObservation> BuildGeneratedDocuments(
        RouteMovePlan plan,
        IReadOnlyDictionary<string, PlannedFileChange> effects)
    {
        var root = plan.Request.Workspace.LexicalRoot;
        var references = plan.Projection.References.Documents.ToDictionary(
            document => Path.Combine(
                root,
                document.DestinationSourcePath.Replace('/', Path.DirectorySeparatorChar)),
            StringComparer.Ordinal);
        return [.. plan.Projection.Navigation.GeneratedNavigation.Regions.Select(region =>
        {
            var path = Path.Combine(root, region.Path.Replace('/', Path.DirectorySeparatorChar));
            if (effects.TryGetValue(path, out var effect))
            {
                return new RouteMovePostMoveObservation(
                    "generated-document",
                    FileAfter(plan, effect));
            }

            var document = references.GetValueOrDefault(path)
                ?? throw new InvalidOperationException(
                    "Every selected generated region requires one observed Markdown document.");
            return new RouteMovePostMoveObservation(
                "generated-document",
                document.Snapshot.Expectation);
        })];
    }

    private static FileExpectation ExpectedDocument(
        RouteMovePlan plan,
        IReadOnlyDictionary<string, PlannedFileChange> effects,
        FileStateSnapshot snapshot,
        string path)
    {
        if (effects.TryGetValue(path, out var effect))
        {
            return FileAfter(plan, effect);
        }

        if (string.Equals(path, snapshot.LogicalPath, StringComparison.Ordinal))
        {
            return snapshot.Expectation;
        }

        throw new InvalidOperationException(
            "A moved Markdown observation requires one exact destination effect.");
    }

    private static FileExpectation FileAfter(RouteMovePlan plan, PlannedFileChange change)
        => FileExpectation.File(
            change.LogicalPath,
            PhysicalPath(plan, change.LogicalPath),
            FileExpectation.Hash(change.IntendedBytes.AsSpan()));

    private static FileExpectation DirectoryAfter(RouteMovePlan plan, string logicalPath)
        => FileExpectation.Directory(logicalPath, PhysicalPath(plan, logicalPath));

    private static string PhysicalPath(RouteMovePlan plan, string logicalPath)
        => Path.Combine(
            plan.Request.Workspace.PhysicalRoot,
            Path.GetRelativePath(plan.Request.Workspace.LexicalRoot, logicalPath));
}
