using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal static class RouteMovePlanProjector
{
    internal static RouteMovePlan Build(RouteMovePlanProjectionInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var destination = input.Destination;
        var request = destination.Inventory.Subject.Request;
        var preview = new RouteMoveResultFormation
        {
            Workspace = request.Workspace,
            Mode = request.Mode,
            Source = destination.Inventory.Subject.Source,
            Destination = destination.Destination,
            Subject = destination.Subject,
            Ownership = RouteMoveOwnershipProjector.Project(
                input.Ownership,
                destination.Inventory.Subject),
            References = input.References.References,
            GeneratedNavigation = input.Navigation.GeneratedNavigation,
            Plan = new RouteMovePlanFacts
            {
                Completeness = RouteMovePlanCompleteness.Complete,
                Safety = RouteMovePlanSafety.Safe,
            },
            Effects = BuildEffects(input),
            UnchangedPaths = input.Navigation.GeneratedNavigation.Regions
                .Where(region => region.State == RouteMoveGeneratedState.Unchanged)
                .Select(region => region.Path)
                .Order(StringComparer.Ordinal)
                .ToImmutableArray(),
            Recovery = new RouteMoveRecovery
            {
                State = input.RecoveryTargets.IsEmpty
                    ? RouteMoveRecoveryState.NotRequired
                    : RouteMoveRecoveryState.NotCreated,
                ProtectedPaths = input.RecoveryTargets.Select(target => Canonical(
                        request.Workspace.LexicalRoot,
                        target.Change.LogicalPath))
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToImmutableArray(),
            },
            Verification = RouteMoveVerificationState.NotRequested,
        };
        return new RouteMovePlan
        {
            Request = request,
            Preview = preview,
            Projection = input,
        };
    }

    private static ImmutableArray<RouteMoveEffect> BuildEffects(
        RouteMovePlanProjectionInput input)
    {
        var workspaceRoot = input.Destination.Inventory.Subject.Request.Workspace.LexicalRoot;
        var effects = new List<RouteMoveEffect>();
        effects.AddRange(input.DirectoryCreations.Select(creation => new RouteMoveEffect
        {
            Path = Canonical(workspaceRoot, creation.LogicalPath),
            Kind = RouteMoveEffectKind.Directory,
            Action = RouteMoveEffectAction.Create,
            Before = PathState(creation.Expectation),
            Expected = new RouteMovePathState(RouteMovePathStateKind.Directory, contentSha256: null),
            Outcome = RouteMoveEffectOutcome.Planned,
            Residual = RouteMoveEffectResidual.None,
        }));
        effects.AddRange(input.FileChanges.Select(change => FileEffect(workspaceRoot, change)));
        effects.AddRange(input.DirectoryDeletions.Select(deletion => new RouteMoveEffect
        {
            Path = Canonical(workspaceRoot, deletion.LogicalPath),
            Kind = RouteMoveEffectKind.Directory,
            Action = RouteMoveEffectAction.Delete,
            Before = PathState(deletion.Expectation),
            Expected = new RouteMovePathState(RouteMovePathStateKind.Missing, contentSha256: null),
            Outcome = RouteMoveEffectOutcome.Planned,
            Residual = RouteMoveEffectResidual.None,
        }));
        return effects.ToImmutableArray();
    }

    private static RouteMoveEffect FileEffect(
        string workspaceRoot,
        PlannedFileChange change)
        => new()
        {
            Path = Canonical(workspaceRoot, change.LogicalPath),
            Kind = EffectKind(change.Kind),
            Action = change.Kind switch
            {
                PlannedFileChangeKind.Create => RouteMoveEffectAction.Create,
                PlannedFileChangeKind.Replace or PlannedFileChangeKind.ReplaceGeneratedRegion =>
                    RouteMoveEffectAction.Replace,
                PlannedFileChangeKind.Delete => RouteMoveEffectAction.Delete,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(change),
                    change.Kind,
                    "The planned file-change kind is not defined."),
            },
            Before = PathState(change.Expectation),
            Expected = change.Kind == PlannedFileChangeKind.Delete
                ? new RouteMovePathState(RouteMovePathStateKind.Missing, contentSha256: null)
                : new RouteMovePathState(
                    RouteMovePathStateKind.File,
                    FileExpectation.Hash(change.IntendedBytes.AsSpan())),
            Outcome = RouteMoveEffectOutcome.Planned,
            Residual = RouteMoveEffectResidual.None,
        };

    private static RouteMoveEffectKind EffectKind(PlannedFileChangeKind kind)
        => kind switch
        {
            PlannedFileChangeKind.Create or PlannedFileChangeKind.Delete =>
                RouteMoveEffectKind.MovedFile,
            PlannedFileChangeKind.Replace => RouteMoveEffectKind.ReferenceSource,
            PlannedFileChangeKind.ReplaceGeneratedRegion => RouteMoveEffectKind.GeneratedRegion,
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The planned file-change kind is not defined."),
        };

    private static RouteMovePathState PathState(FileExpectation expectation)
        => expectation.Kind switch
        {
            FileExpectationKind.Missing => new RouteMovePathState(
                RouteMovePathStateKind.Missing,
                contentSha256: null),
            FileExpectationKind.File => new RouteMovePathState(
                RouteMovePathStateKind.File,
                expectation.ContentHash),
            FileExpectationKind.Directory => new RouteMovePathState(
                RouteMovePathStateKind.Directory,
                contentSha256: null),
            _ => throw new ArgumentOutOfRangeException(
                nameof(expectation),
                expectation.Kind,
                "The file expectation kind is not defined."),
        };

    private static string Canonical(string workspaceRoot, string path)
        => Path.GetRelativePath(workspaceRoot, path)
            .Replace(Path.DirectorySeparatorChar, '/');
}
