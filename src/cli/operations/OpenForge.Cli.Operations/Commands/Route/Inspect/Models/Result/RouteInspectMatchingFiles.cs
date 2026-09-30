using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;

internal enum RouteInspectMatchingFilesScope
{
    GitTrackedAndUntracked,
    WorkspaceFiles,
    AllFiles,
}

internal enum RouteInspectMatchingFilesReason
{
    SourceUnavailable,
    ConditionUnavailable,
    GitUnavailable,
    ScanTimeout,
    ScanFailed,
    FilesUnavailable,
    UnsafePath,
    Cancelled,
}

internal sealed record RouteInspectMatchingFiles
{
    private readonly int _skippedIgnoreLines;
    internal const int MaximumPaths = 100;
    internal static readonly TimeSpan ScanDeadline = TimeSpan.FromSeconds(30);
    internal const string NoMatchesNote = "No current files match this condition. Planned files may still match.";
    internal const string AllFilesNote = "No effective applyTo restriction. Every file applies. No scan was run.";

    private RouteInspectMatchingFiles(
        RouteInspectMatchingFilesScope? scope,
        int? count,
        ImmutableArray<string> paths,
        RouteInspectMatchingFilesReason? reason,
        int skippedIgnoreLines = 0)
    {
        Scope = scope;
        Count = count;
        Paths = paths;
        Reason = reason;
        _skippedIgnoreLines = skippedIgnoreLines;
    }

    internal RouteInspectMatchingFilesScope? Scope { get; }
    internal bool Complete => Reason is null;
    internal int? Count { get; }
    internal ImmutableArray<string> Paths { get; }
    internal int PathLimit => MaximumPaths;
    internal bool Truncated => Complete && Count > MaximumPaths;
    internal RouteInspectMatchingFilesReason? Reason { get; }
    internal string? Note
    {
        get
        {
            if (Scope == RouteInspectMatchingFilesScope.AllFiles)
            {
                return AllFilesNote;
            }

            var zeroNote = Complete && Count == 0 ? NoMatchesNote : null;
            if (_skippedIgnoreLines == 0)
            {
                return zeroNote;
            }

            var skippedNote = $"Skipped unsupported .gitignore lines: {_skippedIgnoreLines.ToString(System.Globalization.CultureInfo.InvariantCulture)}.";
            return zeroNote is null ? skippedNote : zeroNote + " " + skippedNote;
        }
    }

    internal static RouteInspectMatchingFiles AllFiles()
        => new(RouteInspectMatchingFilesScope.AllFiles, null, [], null);

    internal static RouteInspectMatchingFiles Success(RouteInspectMatchingFilesScope scope, IEnumerable<string> paths, int skippedIgnoreLines = 0)
        => Materialize(scope, paths, CancellationToken.None, skippedIgnoreLines);

    internal static RouteInspectMatchingFiles Materialize(
        RouteInspectMatchingFilesScope scope,
        IEnumerable<string> paths,
        CancellationToken cancellationToken,
        int skippedIgnoreLines = 0)
    {
        ArgumentNullException.ThrowIfNull(paths);
        if (!Enum.IsDefined(scope) || scope == RouteInspectMatchingFilesScope.AllFiles)
        {
            throw new ArgumentOutOfRangeException(nameof(scope));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(skippedIgnoreLines);
        cancellationToken.ThrowIfCancellationRequested();
        var distinct = new HashSet<string>(StringComparer.Ordinal);
        var firstPaths = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var path in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!distinct.Add(path))
            {
                continue;
            }

            firstPaths.Add(path);
            if (firstPaths.Count > MaximumPaths)
            {
                firstPaths.Remove(firstPaths.Max ?? throw new InvalidOperationException("A full path set requires a maximum."));
            }
        }

        var result = new RouteInspectMatchingFiles(scope, distinct.Count, firstPaths.ToImmutableArray(), null, skippedIgnoreLines);
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }

    internal static RouteInspectMatchingFiles Unavailable(
        RouteInspectMatchingFilesScope? scope,
        RouteInspectMatchingFilesReason reason)
    {
        if ((scope.HasValue && !Enum.IsDefined(scope.Value)) || scope == RouteInspectMatchingFilesScope.AllFiles || !Enum.IsDefined(reason))
        {
            throw new ArgumentOutOfRangeException(nameof(reason));
        }

        return new(scope, null, [], reason);
    }
}
