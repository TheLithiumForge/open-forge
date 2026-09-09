using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Source;

namespace OpenForge.Cli.Core.Framework.Libraries.Shared.Inventory;

internal static class LibraryInventoryReader
{
    internal static ValueTask<LibraryInventoryRead> ReadAsync(PhysicalPathResolver resolver, LibrarySourceRootObservation source, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(source);
        cancellationToken.ThrowIfCancellationRequested();
        var fresh = LibrarySourceRootReader.Read(resolver, source.Request, cancellationToken);
        if (!MatchesAvailableSource(source, fresh))
        {
            return ValueTask.FromResult(new LibraryInventoryRead
            {
                Source = source,
                Inventory = null,
                ExcludedPaths = [],
                UnavailablePaths = [new() { Path = ".", Cause = fresh.Cause ?? "The Library source root changed before inventory." }],
            });
        }

        var result = new LibraryInventoryCollector(fresh).Read(cancellationToken);
        var confirmed = LibrarySourceRootReader.Read(resolver, source.Request, cancellationToken);
        if (MatchesAvailableSource(fresh, confirmed))
        {
            return ValueTask.FromResult(result);
        }
        var inventory = result.Inventory ?? throw new InvalidOperationException("A collected Library inventory must retain its source facts.");
        const string cause = "The Library source root changed during inventory.";
        return ValueTask.FromResult(result with
        {
            Inventory = LibraryInventory.Classified(inventory.SourceRoot, inventory.PhysicalSourceRoot,
                LibrarySourceRootState.Available, LibraryInventoryState.Incomplete, cause, inventory.Entries),
            UnavailablePaths = [.. result.UnavailablePaths, new() { Path = ".", Cause = cause }],
        });
    }

    private static bool MatchesAvailableSource(LibrarySourceRootObservation expected, LibrarySourceRootObservation actual)
        => expected.State == LibrarySourceRootState.Available
            && actual.State == LibrarySourceRootState.Available
            && PhysicalIdentityTracker.PathComparer.Equals(expected.PhysicalSourceRoot, actual.PhysicalSourceRoot);
}
