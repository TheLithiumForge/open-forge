using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Observation;

internal sealed partial class WorkspaceOutsideRouteReader(PhysicalPathResolver paths, WorkspaceLayout layout)
{
    private readonly Dictionary<string, FileExpectation> _expectations = new(StringComparer.Ordinal);
    private readonly FileExpectationValidator _validator = new(paths);

    internal IEnumerable<FileExpectation> Expectations => _expectations.Values;

    internal async ValueTask<ImmutableArray<WorkspaceOutsideRouteRead>> ReadAsync(CancellationToken cancellationToken)
    {
        var reads = ImmutableArray.CreateBuilder<WorkspaceOutsideRouteRead>();
        foreach (var route in layout.OutsideRoutes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            reads.Add(ObserveDirectory(route, cancellationToken));
        }
        ApplyOverlaps(reads, cancellationToken);
        for (var index = 0; index < reads.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (reads[index].State == WorkspaceOutsideRouteReadState.Available)
                reads[index] = await ObserveCatalogueAsync(reads[index], cancellationToken).ConfigureAwait(false);
        }
        return reads.ToImmutable();
    }

    private WorkspaceOutsideRouteRead ObserveDirectory(WorkspaceOutsideRoute route, CancellationToken cancellationToken)
    {
        var resolution = Resolve(route.DirectoryPath);
        var directory = ObserveAncestry(route.DirectoryPath, cancellationToken);
        var read = new WorkspaceOutsideRouteRead
        {
            Descriptor = route,
            PhysicalDirectory = resolution.State == PhysicalPathState.Contained && directory.State != NoFollowLeafState.OrdinaryFile
                ? resolution.GetContainedPhysicalPath() : null,
            DirectoryResolution = resolution,
            DirectoryObservation = directory,
            State = WorkspaceOutsideRouteReadState.Available,
        };
        if (directory.State == NoFollowLeafState.Missing)
            return Problem(read, WorkspaceOutsideRouteReadState.Missing, WorkspaceLayoutIssueCode.OutsideDirectoryMissing,
                directory.LogicalPath, "The registered directory or an ancestor is missing.");
        if (directory.State != NoFollowLeafState.Directory)
            return LeafProblem(read, directory, WorkspaceLayoutIssueCode.OutsideDirectoryUnsafe);
        if (resolution.State != PhysicalPathState.Contained)
            return Problem(read, ResolutionState(resolution), WorkspaceLayoutIssueCode.OutsideDirectoryUnsafe,
                Logical(route.DirectoryPath), "The registered directory cannot be resolved safely.") with
            { Failure = resolution.Failure };
        return read;
    }

    private async ValueTask<WorkspaceOutsideRouteRead> ObserveCatalogueAsync(WorkspaceOutsideRouteRead read, CancellationToken cancellationToken)
    {
        var route = read.Descriptor;
        try
        {
            var catalogue = NoFollowLeafObserver.Observe(paths, layout.Workspace, Logical(route.CataloguePath), cancellationToken);
            read = read with { CatalogueObservation = catalogue };
            if (catalogue.State == NoFollowLeafState.Missing)
            {
                var snapshot = FileStateSnapshot.Missing(catalogue.LogicalPath);
                Capture(snapshot.Expectation);
                read = read with { CatalogueSnapshot = snapshot };
            }
            else if (catalogue.State == NoFollowLeafState.OrdinaryFile)
            {
                var anchor = Resolve(route.CataloguePath);
                read = read with { CatalogueResolution = anchor };
                if (anchor.State != PhysicalPathState.Contained)
                    return Problem(read, ResolutionState(anchor), WorkspaceLayoutIssueCode.OutsideCatalogueUnsafe,
                        catalogue.LogicalPath, "The outside catalogue cannot be resolved safely.") with
                    { Failure = anchor.Failure };
                var physical = anchor.GetContainedPhysicalPath();
                if (!PhysicalContainment.Contains(read.DirectoryResolution.GetContainedPhysicalPath(), physical))
                    return Problem(read, WorkspaceOutsideRouteReadState.Unsafe, WorkspaceLayoutIssueCode.OutsideCatalogueUnsafe,
                        catalogue.LogicalPath, "The outside catalogue leaves its registered directory.");
                var bytes = await File.ReadAllBytesAsync(physical, cancellationToken).ConfigureAwait(false);
                var snapshot = FileStateSnapshot.File(catalogue.LogicalPath, physical, bytes);
                Capture(snapshot.Expectation);
                read = read with { CatalogueSnapshot = snapshot };
            }
            else return LeafProblem(read, catalogue, WorkspaceLayoutIssueCode.OutsideCatalogueUnsafe);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return Problem(read, WorkspaceOutsideRouteReadState.Unavailable, WorkspaceLayoutIssueCode.OutsideObservationUnavailable,
                Logical(route.CataloguePath), "The outside catalogue could not be read.") with
            { Failure = Failure(exception) };
        }

        foreach (var expectation in RouteExpectations(route))
        {
            var check = await _validator.ValidateAsync(layout.Workspace, expectation, cancellationToken).ConfigureAwait(false);
            if (check.State == FileExpectationValidationState.Cancelled) cancellationToken.ThrowIfCancellationRequested();
            if (check.State != FileExpectationValidationState.Matched)
                return Problem(read, WorkspaceOutsideRouteReadState.Unavailable, WorkspaceLayoutIssueCode.OutsideObservationUnavailable,
                    expectation.LogicalPath, "The outside registration changed while it was being observed.") with
                { Failure = check.Failure };
        }
        return read;
    }

    private NoFollowLeafObservation ObserveAncestry(string relative, CancellationToken cancellationToken)
    {
        var segments = relative.Split('/');
        var current = string.Empty;
        foreach (var segment in segments)
        {
            current = current.Length == 0 ? segment : current + "/" + segment;
            var logical = Logical(current);
            var leaf = NoFollowLeafObserver.Observe(paths, layout.Workspace, logical, cancellationToken);
            if (leaf.State == NoFollowLeafState.Missing)
            {
                Capture(FileExpectation.Missing(logical));
                return leaf;
            }
            if (leaf.State != NoFollowLeafState.Directory) return leaf;
            var resolution = Resolve(current);
            if (resolution.State != PhysicalPathState.Contained)
                return NoFollowLeafObservation.Classified(logical, NoFollowLeafState.Unknown,
                    resolution.Failure ?? new(FilesystemFailureKind.Unsupported, "The outside ancestry cannot be safely resolved."));
            try
            {
                _ = Directory.EnumerateFileSystemEntries(resolution.GetContainedPhysicalPath()).Take(1).Any();
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                return NoFollowLeafObservation.Classified(logical,
                    exception is UnauthorizedAccessException ? NoFollowLeafState.Inaccessible : NoFollowLeafState.Unknown, Failure(exception));
            }
            Capture(FileExpectation.Directory(logical, resolution.GetContainedPhysicalPath()));
        }
        return NoFollowLeafObservation.Directory(Logical(relative));
    }

    private IEnumerable<FileExpectation> RouteExpectations(WorkspaceOutsideRoute route)
    {
        var directory = Logical(route.DirectoryPath);
        var catalogue = Logical(route.CataloguePath);
        return _expectations.Values.Where(expectation => expectation.LogicalPath == catalogue
            || PhysicalContainment.Contains(expectation.LogicalPath, directory));
    }

    private WorkspaceOutsideRouteRead LeafProblem(WorkspaceOutsideRouteRead read, NoFollowLeafObservation leaf, WorkspaceLayoutIssueCode unsafeCode)
    {
        var unavailable = leaf.State is NoFollowLeafState.Inaccessible or NoFollowLeafState.Unknown
            && leaf.Failure?.Kind is not FilesystemFailureKind.Unsupported;
        return Problem(read, unavailable ? WorkspaceOutsideRouteReadState.Unavailable : WorkspaceOutsideRouteReadState.Unsafe,
            unavailable ? WorkspaceLayoutIssueCode.OutsideObservationUnavailable : unsafeCode,
            leaf.LogicalPath, "The outside boundary requires an accessible ordinary directory and an ordinary or missing catalogue.")
            with
        { Failure = leaf.Failure };
    }

    private WorkspaceOutsideRouteRead Problem(WorkspaceOutsideRouteRead read, WorkspaceOutsideRouteReadState state,
        WorkspaceLayoutIssueCode code, string logical, string cause)
        => read with
        {
            State = state,
            Issues = [.. read.Issues, new(code, Path.GetRelativePath(layout.Workspace.LexicalRoot, logical).Replace(Path.DirectorySeparatorChar, '/'), cause, [])],
        };

    private static WorkspaceOutsideRouteReadState ResolutionState(PhysicalPathResolution resolution)
        => resolution.State is PhysicalPathState.Inaccessible or PhysicalPathState.InputOutputFailure
            ? WorkspaceOutsideRouteReadState.Unavailable : WorkspaceOutsideRouteReadState.Unsafe;

    private static FilesystemFailure Failure(Exception exception)
        => FilesystemFailure.FromException(FilesystemFailure.ClassifyException(exception), exception);

    private void Capture(FileExpectation expectation) => _expectations.TryAdd(expectation.LogicalPath, expectation);
    private string Logical(string relative) => Path.Combine(layout.Workspace.LexicalRoot, relative.Replace('/', Path.DirectorySeparatorChar));
    private PhysicalPathResolution Resolve(string relative) => paths.ResolveCandidate(layout.Workspace.LexicalRoot, layout.Workspace.PhysicalRoot, Logical(relative));
}
