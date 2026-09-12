using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;

internal sealed class RouteCreateTargetInspector
{
    private readonly RouteCreateTargetPlanner _targetPlanner = new();
    private readonly SourceCatalogueReader _catalogueReader = new();
    private readonly PhysicalPathResolver _physicalPathResolver = new();

    internal async ValueTask<RouteCreateTargetInspectionBuild> InspectAsync(
        RouteCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Stop(
                Target(request.FileTarget),
                RouteCreateFindingCode.Interrupted,
                "Route Create planning was interrupted.",
                isIncomplete: true);
        }

        var resolution = _targetPlanner.Resolve(request);
        if (resolution.State != RouteCreateTargetResolutionState.Resolved
            || resolution.Identity is not { } identity
            || resolution.Target.Path is not { } targetPath)
        {
            return Stop(
                resolution.Target,
                RouteCreateFindingCode.InvalidTarget,
                resolution.Cause ?? "The Route Create target is invalid.",
                isIncomplete: true);
        }

        var catalogue = await _catalogueReader.ReadAsync(
                new SourceCatalogueRequest(request.Workspace, [SourceLogicalPath.AgentsRoot]),
                cancellationToken)
            .ConfigureAwait(false);
        if (catalogue.IsCancelled)
        {
            return Stop(
                resolution.Target,
                RouteCreateFindingCode.Interrupted,
                "Route Create workspace inspection was interrupted.",
                isIncomplete: true);
        }

        if (catalogue.Issues.FirstOrDefault(issue =>
                issue.Stage == SourceCatalogueIssueStage.Root) is { } rootIssue)
        {
            var code = rootIssue.Code == SourceCatalogueIssueCode.RootUnsafe
                ? RouteCreateFindingCode.WorkspaceUnsafe
                : RouteCreateFindingCode.WorkspaceUnavailable;
            return Stop(
                resolution.Target,
                code,
                rootIssue.Failure?.DirectCause ?? "The .agents workspace root is unavailable.",
                isIncomplete: code == RouteCreateFindingCode.WorkspaceUnavailable);
        }

        if (HasPhysicalAlias(catalogue, targetPath))
        {
            return Stop(
                resolution.Target,
                RouteCreateFindingCode.IdentityCollision,
                "The Route Create target aliases another physical source.",
                isIncomplete: false);
        }

        var snapshot = await ReadSnapshotAsync(
                request,
                resolution.Target,
                targetPath,
                catalogue,
                cancellationToken)
            .ConfigureAwait(false);
        if (snapshot.Boundary is { } boundary)
        {
            return RouteCreateTargetInspectionBuild.Stop(boundary);
        }

        return RouteCreateTargetInspectionBuild.Complete(
            new RouteCreateTargetInspection
            {
                Request = request,
                Target = resolution.Target,
                Identity = identity,
                Catalogue = catalogue,
                Snapshot = snapshot.Snapshot
                    ?? throw new InvalidOperationException(
                        "Successful Route Create target inspection requires a snapshot."),
            });
    }

    private async ValueTask<TargetSnapshotRead> ReadSnapshotAsync(
        RouteCreateRequest request,
        RouteCreateTarget target,
        string targetPath,
        SourceCatalogue catalogue,
        CancellationToken cancellationToken)
    {
        var existingSource = catalogue.FindByPath(targetPath);
        if (existingSource?.Overwrite is not null
            || catalogue.Issues.Any(issue => issue.Code == SourceCatalogueIssueCode.OrphanOverwrite
                && string.Equals(
                    issue.AttemptedCanonicalPath,
                    targetPath[..^".md".Length] + ".overwrite.md",
                    StringComparison.Ordinal)))
        {
            return TargetSnapshotRead.Stop(Boundary(
                target,
                RouteCreateFindingCode.IdentityCollision,
                "The Route Create target has an existing overwrite companion.",
                isIncomplete: false));
        }

        var lexicalPath = SourceLogicalPath.ToLexicalPath(request.Workspace.LexicalRoot, targetPath);
        var resolution = _physicalPathResolver.ResolveCandidate(
            request.Workspace.LexicalRoot,
            request.Workspace.PhysicalRoot,
            lexicalPath);
        if (resolution.State == PhysicalPathState.Missing)
        {
            return TargetSnapshotRead.Complete(FileStateSnapshot.Missing(lexicalPath));
        }

        if (resolution.State != PhysicalPathState.Contained)
        {
            return TargetSnapshotRead.Stop(Boundary(
                target,
                RouteCreateFindingCode.TargetUnsafe,
                resolution.Failure?.DirectCause
                    ?? "The Route Create target physical identity is unsafe.",
                isIncomplete: false));
        }

        var physicalPath = resolution.GetContainedPhysicalPath();
        try
        {
            if ((File.GetAttributes(physicalPath) & FileAttributes.Directory) != 0)
            {
                return TargetSnapshotRead.Stop(Boundary(
                    target,
                    RouteCreateFindingCode.TargetUnsafe,
                    "The Route Create target is not an ordinary file.",
                    isIncomplete: false));
            }

            var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken)
                .ConfigureAwait(false);
            return TargetSnapshotRead.Complete(
                FileStateSnapshot.File(lexicalPath, physicalPath, bytes));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return TargetSnapshotRead.Stop(Boundary(
                target,
                RouteCreateFindingCode.Interrupted,
                "Route Create target inspection was interrupted.",
                isIncomplete: true));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return TargetSnapshotRead.Stop(Boundary(
                target,
                RouteCreateFindingCode.TargetUnsafe,
                exception.Message,
                isIncomplete: false));
        }
    }

    private static bool HasPhysicalAlias(SourceCatalogue catalogue, string targetPath)
        => catalogue.Issues.Any(issue =>
            issue.Code == SourceCatalogueIssueCode.PhysicalAlias
            && (string.Equals(issue.AttemptedCanonicalPath, targetPath, StringComparison.Ordinal)
                || issue.RelatedPaths.Contains(targetPath, StringComparer.Ordinal)));

    private static RouteCreateTargetInspectionBuild Stop(
        RouteCreateTarget target,
        RouteCreateFindingCode code,
        string cause,
        bool isIncomplete)
        => RouteCreateTargetInspectionBuild.Stop(Boundary(target, code, cause, isIncomplete));

    private static RouteCreatePlanningBoundary Boundary(
        RouteCreateTarget target,
        RouteCreateFindingCode code,
        string cause,
        bool isIncomplete)
        => new()
        {
            Target = target,
            Parent = null,
            Template = null,
            Finding = new RouteCreateFinding(code, cause, target.Path ?? target.Requested),
            IsIncomplete = isIncomplete,
        };

    private static RouteCreateTarget Target(string requested)
        => new()
        {
            Requested = requested,
            Id = null,
            Path = null,
        };

    private sealed class TargetSnapshotRead
    {
        private TargetSnapshotRead(
            FileStateSnapshot? snapshot,
            RouteCreatePlanningBoundary? boundary)
        {
            Snapshot = snapshot;
            Boundary = boundary;
        }

        internal FileStateSnapshot? Snapshot { get; }

        internal RouteCreatePlanningBoundary? Boundary { get; }

        internal static TargetSnapshotRead Complete(FileStateSnapshot snapshot)
            => new(snapshot: snapshot, boundary: null);

        internal static TargetSnapshotRead Stop(RouteCreatePlanningBoundary boundary)
            => new(snapshot: null, boundary: boundary);
    }
}
