using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Paths;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Source;

namespace OpenForge.Cli.Core.Framework.Libraries.Shared.Inventory;

internal static class LibraryInventoryReader
{
    internal static ValueTask<LibraryInventoryRead> ReadAsync(
        PhysicalPathResolver resolver,
        LibrarySourceRootObservation source,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(source);
        cancellationToken.ThrowIfCancellationRequested();
        var fresh = LibrarySourceRootReader.Read(
            resolver,
            source.Request,
            cancellationToken);
        if (!MatchesAvailableSource(source, fresh))
        {
            return ValueTask.FromResult(UnavailableSource(
                source,
                fresh.Cause ?? "The Library source boundary changed before inventory."));
        }

        var entries = new List<EligibleSourceFile>();
        var excluded = new List<LibraryInventoryExclusion>();
        var unavailable = new List<LibraryInventoryUnavailablePath>();
        var sourceRoot = fresh.PhysicalSourceRoot
            ?? throw new InvalidOperationException("An available Library source requires its physical root.");
        var agents = fresh.PhysicalAgentsDirectory
            ?? throw new InvalidOperationException("An available Library source requires its .agents directory.");
        ObserveDirectory(
            agents,
            sourceRoot,
            entries,
            excluded,
            unavailable,
            cancellationToken);

        var confirmed = LibrarySourceRootReader.Read(
            resolver,
            source.Request,
            cancellationToken);
        if (!MatchesAvailableSource(fresh, confirmed))
        {
            unavailable.Add(new LibraryInventoryUnavailablePath
            {
                Path = ".agents",
                Cause = confirmed.Cause
                    ?? "The Library source boundary changed during inventory.",
            });
        }

        entries.Sort((left, right) => string.CompareOrdinal(
            left.SourcePath.Value,
            right.SourcePath.Value));
        excluded.Sort((left, right) => string.CompareOrdinal(left.Path, right.Path));
        unavailable.Sort((left, right) => string.CompareOrdinal(left.Path, right.Path));
        var inventory = unavailable.Count == 0
            ? LibraryInventory.Complete(
                source.Request.SourceRoot,
                agents,
                entries)
            : LibraryInventory.Classified(
                source.Request.SourceRoot,
                agents,
                LibrarySourceRootState.Available,
                LibraryInventoryState.Incomplete,
                "One or more eligible Library source paths were unavailable.",
                entries);
        return ValueTask.FromResult(new LibraryInventoryRead
        {
            Source = source,
            Inventory = inventory,
            ExcludedPaths = [.. excluded],
            UnavailablePaths = [.. unavailable],
        });
    }

    private static void ObserveDirectory(
        string directory,
        string sourceRoot,
        List<EligibleSourceFile> entries,
        List<LibraryInventoryExclusion> excluded,
        List<LibraryInventoryUnavailablePath> unavailable,
        CancellationToken cancellationToken)
    {
        if (!TryAdmitDirectory(
                directory,
                sourceRoot,
                excluded,
                unavailable))
        {
            return;
        }

        string[] children;
        try
        {
            children = Directory.GetFileSystemEntries(directory);
            Array.Sort(children, StringComparer.Ordinal);
        }
        catch (UnauthorizedAccessException exception)
        {
            unavailable.Add(Unavailable(sourceRoot, directory, exception));
            return;
        }
        catch (IOException exception)
        {
            unavailable.Add(Unavailable(sourceRoot, directory, exception));
            return;
        }

        if (!TryAdmitDirectory(
                directory,
                sourceRoot,
                excluded,
                unavailable))
        {
            return;
        }

        foreach (var child in children)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Relative(sourceRoot, child);
            var component = LinkTargetReader.Read(child);
            switch (component.State)
            {
                case PathComponentState.Link:
                    excluded.Add(Excluded(path, LibraryInventoryExclusionKind.Link));
                    continue;
                case PathComponentState.Unsupported:
                    excluded.Add(Excluded(path, LibraryInventoryExclusionKind.ReparsePoint));
                    continue;
                case PathComponentState.Inaccessible:
                case PathComponentState.InputOutputFailure:
                case PathComponentState.Missing:
                    unavailable.Add(new LibraryInventoryUnavailablePath
                    {
                        Path = path,
                        Cause = component.Failure?.DirectCause
                            ?? "The Library source path changed or could not be observed.",
                    });
                    continue;
                case PathComponentState.Ordinary:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(directory),
                        component.State,
                        "The path component state is not defined.");
            }

            var attributes = component.Attributes
                ?? throw new InvalidOperationException("An ordinary source path requires attributes.");
            if ((attributes & FileAttributes.Directory) != 0)
            {
                ObserveDirectory(
                    child,
                    sourceRoot,
                    entries,
                    excluded,
                    unavailable,
                    cancellationToken);
                continue;
            }

            if ((attributes & (FileAttributes.Device | FileAttributes.ReparsePoint)) != 0)
            {
                excluded.Add(Excluded(path, LibraryInventoryExclusionKind.Special));
                continue;
            }

            if (LibraryEligiblePathPolicy.TryClassifyExclusion(path, out var kind))
            {
                excluded.Add(Excluded(path, kind));
                continue;
            }

            try
            {
                using var handle = File.OpenHandle(
                    child,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete);
                if (handle.IsInvalid || handle.IsClosed)
                {
                    unavailable.Add(new LibraryInventoryUnavailablePath
                    {
                        Path = path,
                        Cause = "The eligible Library source file could not be opened for observation.",
                    });
                    continue;
                }
            }
            catch (UnauthorizedAccessException exception)
            {
                unavailable.Add(Unavailable(sourceRoot, child, exception));
                continue;
            }
            catch (IOException)
            {
                excluded.Add(Excluded(path, LibraryInventoryExclusionKind.Special));
                continue;
            }

            var confirmed = LinkTargetReader.Read(child);
            if (confirmed.State != PathComponentState.Ordinary
                || confirmed.Attributes != component.Attributes)
            {
                unavailable.Add(new LibraryInventoryUnavailablePath
                {
                    Path = path,
                    Cause = "The eligible Library source file changed during observation.",
                });
                continue;
            }

            entries.Add(EligibleSourceFile.Create(
                SourceRelativeEligiblePath.Create(path),
                child));
        }
    }

    private static bool TryAdmitDirectory(
        string directory,
        string sourceRoot,
        List<LibraryInventoryExclusion> excluded,
        List<LibraryInventoryUnavailablePath> unavailable)
    {
        var path = Relative(sourceRoot, directory);
        var component = LinkTargetReader.Read(directory);
        switch (component.State)
        {
            case PathComponentState.Ordinary:
                if (component.Attributes is { } attributes
                    && (attributes & FileAttributes.Directory) != 0
                    && (attributes & (FileAttributes.Device | FileAttributes.ReparsePoint)) == 0)
                {
                    return true;
                }

                excluded.Add(Excluded(path, LibraryInventoryExclusionKind.Special));
                return false;
            case PathComponentState.Link:
                excluded.Add(Excluded(path, LibraryInventoryExclusionKind.Link));
                return false;
            case PathComponentState.Unsupported:
                excluded.Add(Excluded(path, LibraryInventoryExclusionKind.ReparsePoint));
                return false;
            case PathComponentState.Inaccessible:
            case PathComponentState.InputOutputFailure:
            case PathComponentState.Missing:
                unavailable.Add(new LibraryInventoryUnavailablePath
                {
                    Path = path,
                    Cause = component.Failure?.DirectCause
                        ?? "The Library source directory changed or could not be observed.",
                });
                return false;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(directory),
                    component.State,
                    "The path component state is not defined.");
        }
    }

    private static bool MatchesAvailableSource(
        LibrarySourceRootObservation expected,
        LibrarySourceRootObservation actual)
        => expected.State == LibrarySourceRootState.Available
            && actual.State == LibrarySourceRootState.Available
            && PhysicalIdentityTracker.PathComparer.Equals(
                expected.PhysicalSourceRoot,
                actual.PhysicalSourceRoot)
            && PhysicalIdentityTracker.PathComparer.Equals(
                expected.PhysicalAgentsDirectory,
                actual.PhysicalAgentsDirectory);

    private static LibraryInventoryRead UnavailableSource(
        LibrarySourceRootObservation source,
        string cause)
        => new()
        {
            Source = source,
            Inventory = null,
            ExcludedPaths = [],
            UnavailablePaths =
            [
                new LibraryInventoryUnavailablePath
                {
                    Path = ".agents",
                    Cause = cause,
                },
            ],
        };

    private static LibraryInventoryExclusion Excluded(
        string path,
        LibraryInventoryExclusionKind kind)
        => new()
        {
            Path = path,
            Kind = kind,
        };

    private static LibraryInventoryUnavailablePath Unavailable(
        string sourceRoot,
        string path,
        Exception exception)
        => new()
        {
            Path = Relative(sourceRoot, path),
            Cause = exception is UnauthorizedAccessException
                ? "The Library source path is inaccessible."
                : "The Library source path is unavailable.",
        };

    private static string Relative(string sourceRoot, string path)
        => Path.GetRelativePath(sourceRoot, path)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
}
