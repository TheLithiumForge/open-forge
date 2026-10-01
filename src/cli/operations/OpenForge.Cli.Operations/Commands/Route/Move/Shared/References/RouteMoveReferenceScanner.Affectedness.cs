using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.References;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.References;

internal sealed partial class RouteMoveReferenceScanner
{
    private static bool IsProvenUnaffectedMissing(
        RouteMoveReferenceScanInput input,
        string sourcePath,
        string intendedSourcePath,
        SourceLinkDestinationFacts facts,
        CancellationToken cancellationToken)
    {
        if (facts.Finding?.Code != SourceLinkDestinationFindingCode.TargetMissing
            || facts.Target.Kind != SourceLinkTargetKind.Local
            || facts.Target.Resolution != SourceLinkTargetResolution.Missing
            || facts.Target.Path is not { } targetPath
            || !string.Equals(sourcePath, intendedSourcePath, StringComparison.Ordinal))
        {
            return false;
        }

        if (IntersectsMoveCoordinates(input.Request.Destination, targetPath))
        {
            return false;
        }

        return HasProvenOrdinaryAncestry(
            input.Request.Destination.Inventory.Subject.Request.Workspace.PhysicalRoot,
            targetPath,
            cancellationToken);
    }

    private static bool IntersectsMoveCoordinates(
        RouteMoveResolvedDestination destination,
        string targetPath)
    {
        var subject = destination.Inventory.Subject;
        if (subject.Kind == RouteMoveSubjectKind.Category)
        {
            return IntersectsCategoryCoordinates(subject, destination.Destination.Path, targetPath);
        }

        if (subject.Kind == RouteMoveSubjectKind.Leaf)
        {
            return IntersectsLeafCoordinates(subject, destination.Destination.Path, targetPath);
        }

        return true;
    }

    private static bool IntersectsCategoryCoordinates(
        RouteMoveResolvedSubject subject,
        string? destinationEntrypoint,
        string targetPath)
    {
        var sourceEntrypoint = subject.SelectedSource.Identity.CanonicalBasePath;
        if (destinationEntrypoint is null
            || !SourceLogicalPath.IsCanonicalSource(sourceEntrypoint)
            || !SourceLogicalPath.IsCanonicalSource(destinationEntrypoint))
        {
            return true;
        }

        var sourceRoot = SourceLogicalPath.ReadParent(sourceEntrypoint);
        var destinationRoot = SourceLogicalPath.ReadParent(destinationEntrypoint);
        return IsWithinCoordinate(targetPath, sourceRoot)
            || IsWithinCoordinate(targetPath, destinationRoot);
    }

    private static bool IntersectsLeafCoordinates(
        RouteMoveResolvedSubject subject,
        string? destinationBasePath,
        string targetPath)
    {
        var sourceBasePath = subject.SelectedSource.Base.CanonicalPath;
        if (destinationBasePath is null
            || !SourceLogicalPath.IsCanonicalSource(sourceBasePath)
            || !SourceLogicalPath.IsCanonicalSource(destinationBasePath))
        {
            return true;
        }

        var sourceOverwritePath = SourceOverwritePath.ReadAdjacentPath(sourceBasePath);
        var destinationOverwritePath = SourceOverwritePath.ReadAdjacentPath(destinationBasePath);
        return IsSameCoordinate(targetPath, sourceBasePath)
            || IsSameCoordinate(targetPath, sourceOverwritePath)
            || IsSameCoordinate(targetPath, destinationBasePath)
            || IsSameCoordinate(targetPath, destinationOverwritePath);
    }

    private static bool IsWithinCoordinate(string targetPath, string root)
        => string.Equals(targetPath, root, StringComparison.OrdinalIgnoreCase)
            || targetPath.StartsWith($"{root}/", StringComparison.OrdinalIgnoreCase);

    private static bool IsSameCoordinate(string targetPath, string coordinate)
        => string.Equals(targetPath, coordinate, StringComparison.OrdinalIgnoreCase);

    private static bool HasProvenOrdinaryAncestry(
        string physicalRoot,
        string targetPath,
        CancellationToken cancellationToken)
    {
        if (!IsCanonicalWorkspacePath(targetPath))
        {
            return false;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = physicalRoot;
            if (!IsOrdinaryDirectory(LinkTargetReader.Read(current)))
            {
                return false;
            }

            foreach (var segment in targetPath.Split('/', StringSplitOptions.None))
            {
                cancellationToken.ThrowIfCancellationRequested();
                current = Path.Combine(current, segment);
                var component = LinkTargetReader.Read(current);
                if (component.State == PathComponentState.Missing)
                {
                    return true;
                }

                if (!IsOrdinaryDirectory(component))
                {
                    return false;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return false;
        }

        return false;
    }

    private static bool IsCanonicalWorkspacePath(string path)
        => !string.IsNullOrEmpty(path)
            && path != "."
            && path.Split('/', StringSplitOptions.None)
                .All(SourceLogicalPath.IsCanonicalSegment);

    private static bool IsOrdinaryDirectory(PathComponent component)
        => component.State == PathComponentState.Ordinary
            && component.Attributes is { } attributes
            && (attributes & FileAttributes.Directory) != 0
            && (attributes & (FileAttributes.ReparsePoint | FileAttributes.Device)) == 0;
}
