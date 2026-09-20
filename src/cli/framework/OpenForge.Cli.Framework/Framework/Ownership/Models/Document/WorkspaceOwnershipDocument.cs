using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Framework.Ownership.Models.Document;

/// <summary>
/// One path the CLI created, or one region it generated inside a file somebody
/// else authored. The distinction matters at removal: a path may be deleted, a
/// region may only be rewritten, and the file holding it is never the tool's to
/// delete.
/// </summary>
internal sealed record OwnedRegion(string Path, string Region);

/// <summary>Which release wrote a set of entries.</summary>
internal sealed record OwnedSource(string Id, string? Version);

internal sealed record FrameworkOwnership(
    OwnedSource Source,
    ImmutableArray<string> Paths,
    ImmutableArray<OwnedRegion> Regions);

internal sealed record ExtensionOwnership(
    string Id,
    string? Version,
    string? Source,
    ImmutableArray<string> Dependencies,
    ImmutableArray<string> Paths,
    ImmutableArray<OwnedRegion> Regions);

internal sealed record LibraryOwnership(
    string Id,
    string SourceRoot,
    string DestinationRoot,
    ImmutableArray<string> Paths);

/// <summary>
/// What the workspace lock says this tool wrote.
///
/// Every entry is a receipt: it is recorded when a verified mutation actually
/// wrote the thing it names, not derived from what a payload claims it would
/// write. That is what makes the list answerable across releases — a newer
/// binary describing a file differently cannot change what an older one did.
///
/// Ownership of one path by several extensions is not stored. It is derived by
/// intersecting the per-extension lists, so an install only ever appends its own
/// receipt instead of merging into a shared table.
/// </summary>
internal sealed record WorkspaceOwnershipDocument(
    int SchemaVersion,
    FrameworkOwnership? Framework,
    ImmutableArray<ExtensionOwnership> Extensions,
    ImmutableArray<LibraryOwnership> Libraries)
{
    /// <summary>
    /// No recorded ownership. An absent, unreadable, or unintelligible lock reads
    /// as this, so a caller always has a document to act on and reports the state
    /// separately. Nothing is owned, so nothing is deleted — the lock fails toward
    /// a missed deletion rather than an over-deletion.
    /// </summary>
    internal static readonly WorkspaceOwnershipDocument Empty = new(
        WorkspaceOwnershipDefinitions.SchemaVersion,
        Framework: null,
        Extensions: [],
        Libraries: []);

    /// <summary>
    /// Whether this release understands the recorded shape. A document that does
    /// not is still read for the keys this release knows; the answer is reported,
    /// never enforced.
    /// </summary>
    internal bool IsKnownSchemaVersion => SchemaVersion == WorkspaceOwnershipDefinitions.SchemaVersion;

    internal IEnumerable<OwnedPath> ManagedPaths()
        => (Framework?.Paths.Select(path => new OwnedPath(path, OwnedPathManager.Framework, Framework.Source.Id)) ?? [])
            .Concat(Extensions.SelectMany(extension => extension.Paths.Select(path =>
                new OwnedPath(path, OwnedPathManager.Extension, extension.Id))))
            .Distinct().OrderBy(claim => claim.Path, StringComparer.Ordinal)
            .ThenBy(claim => claim.Manager).ThenBy(claim => claim.Owner, StringComparer.Ordinal);

    /// <summary>
    /// The extensions that own <paramref name="path"/>, in recorded order. Remove
    /// uses this to tell a path it may delete from one it must retain because
    /// another extension still owns it.
    /// </summary>
    internal ImmutableArray<string> OwnersOf(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return
        [
            .. Extensions
                .Where(extension => extension.Paths.Contains(path, StringComparer.Ordinal))
                .Select(extension => extension.Id),
        ];
    }
}

internal sealed record WorkspaceOwnershipDecode(
    WorkspaceOwnershipDocument? Document,
    string? Cause);
