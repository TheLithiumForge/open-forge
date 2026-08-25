using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal sealed record RouteListDirectoryEntry(
    string Name,
    string CanonicalLogicalPath);

internal sealed class RouteListDirectoryEnumeration
{
    internal RouteListDirectoryEnumeration(
        DirectoryEnumerationState state,
        string canonicalLogicalPath,
        IEnumerable<RouteListDirectoryEntry>? entries,
        FilesystemFailure? failure)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The directory enumeration state is not defined.");
        }

        if (!SourceLogicalPath.IsCanonicalRoot(canonicalLogicalPath))
        {
            throw new ArgumentException("The directory logical path is not canonical.", nameof(canonicalLogicalPath));
        }

        var materializedEntries = entries?.ToArray();
        if (state == DirectoryEnumerationState.Complete)
        {
            ArgumentNullException.ThrowIfNull(materializedEntries);
            if (failure is not null)
            {
                throw new ArgumentException("A complete directory enumeration cannot carry a failure.", nameof(failure));
            }
        }
        else if (materializedEntries is not null)
        {
            throw new ArgumentException("An incomplete directory enumeration cannot carry entries.", nameof(entries));
        }

        var expectedFailureKind = state switch
        {
            DirectoryEnumerationState.AccessDenied => FilesystemFailureKind.AccessDenied,
            DirectoryEnumerationState.InputOutputFailure => FilesystemFailureKind.InputOutput,
            _ => (FilesystemFailureKind?)null,
        };
        if (expectedFailureKind is null && failure is not null
            || expectedFailureKind is not null && failure?.Kind != expectedFailureKind)
        {
            throw new ArgumentException("The directory state and failure do not agree.", nameof(failure));
        }

        State = state;
        CanonicalLogicalPath = canonicalLogicalPath;
        Entries = materializedEntries is null
            ? null
            : new ReadOnlyCollection<RouteListDirectoryEntry>(materializedEntries);
        Failure = failure;
    }

    internal DirectoryEnumerationState State { get; }

    internal string CanonicalLogicalPath { get; }

    internal IReadOnlyList<RouteListDirectoryEntry>? Entries { get; }

    internal FilesystemFailure? Failure { get; }

    internal FilesystemFailure ReadFailure()
    {
        if (State is not (DirectoryEnumerationState.AccessDenied or DirectoryEnumerationState.InputOutputFailure)
            || Failure is not { } failure)
        {
            throw new InvalidOperationException(
                "A failed directory enumeration requires a failure state and failure.");
        }

        return failure;
    }
}

internal sealed class RouteListDirectoryEnumerator
{
    internal RouteListDirectoryEnumeration Enumerate(
        string provenPhysicalPath,
        string canonicalLogicalPath,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provenPhysicalPath);
        if (!SourceLogicalPath.IsCanonicalRoot(canonicalLogicalPath))
        {
            throw new ArgumentException("The directory logical path is not canonical.", nameof(canonicalLogicalPath));
        }

        var result = DirectoryEntryEnumerator.Enumerate(
            provenPhysicalPath,
            canonicalLogicalPath,
            cancellationToken);
        if (result.State != DirectoryEnumerationState.Complete)
        {
            return new RouteListDirectoryEnumeration(
                result.State,
                canonicalLogicalPath,
                null,
                result.Failure);
        }

        if (result.Entries is not { } enumeratedEntries)
        {
            throw new InvalidOperationException("A complete directory enumeration must carry entries.");
        }

        var entries = enumeratedEntries
            .Select(ReadEntryName)
            .Where(SourceLogicalPath.IsCanonicalSegment)
            .Select(name => new RouteListDirectoryEntry(
                name,
                SourceLogicalPath.Combine(canonicalLogicalPath, name)))
            .OrderBy(entry => entry.Name, StringComparer.Ordinal)
            .ThenBy(entry => entry.CanonicalLogicalPath, StringComparer.Ordinal)
            .ToArray();
        return new RouteListDirectoryEnumeration(
            DirectoryEnumerationState.Complete,
            canonicalLogicalPath,
            entries,
            null);
    }

    private static string ReadEntryName(string entryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entryPath);
        return Path.GetFileName(entryPath);
    }
}
