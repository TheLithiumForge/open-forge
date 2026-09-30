using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.MatchingFiles.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.MatchingFiles;

internal sealed class RouteInspectMatchingFilesScanner(
    RouteInspectFileEnumerator enumerate,
    TimeProvider? timeProvider = null,
    Action<RouteInspectScanStage>? observeStage = null)
{
    internal async ValueTask<RouteInspectMatchingFiles> ScanAsync(
        string workspaceRoot,
        SourceApplicabilityResult? applicability,
        CancellationToken cancellationToken)
    {
        if (applicability is null || applicability.State == SourceApplicabilityState.Invalid)
        {
            return RouteInspectMatchingFiles.Unavailable(null, RouteInspectMatchingFilesReason.ConditionUnavailable);
        }

        RouteInspectMatchingFilesScope? scope = null;
        var clock = timeProvider ?? TimeProvider.System;
        var started = clock.GetTimestamp();
        using var timeout = new CancellationTokenSource(RouteInspectMatchingFiles.ScanDeadline, clock);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);

        void CheckDeadline()
        {
            if (clock.GetElapsedTime(started) >= RouteInspectMatchingFiles.ScanDeadline)
            {
                timeout.Cancel();
            }

            deadline.Token.ThrowIfCancellationRequested();
        }

        try
        {
            var restrictiveConditions = ImmutableArray.CreateBuilder<SourceApplyToCondition>();
            foreach (var condition in applicability.Conditions)
            {
                CheckDeadline();
                if (condition.Metadata.State == ApplyToMetadataState.Valid
                    && !HasMatchAll(condition.Metadata.Patterns, deadline.Token))
                {
                    restrictiveConditions.Add(condition);
                }
            }

            CheckDeadline();
            if (restrictiveConditions.Count == 0)
            {
                return RouteInspectMatchingFiles.AllFiles();
            }

            var inventory = await enumerate(workspaceRoot, deadline.Token).ConfigureAwait(false);
            scope = inventory.Scope;
            CheckDeadline();
            if (inventory.Failure is { } failure)
            {
                return RouteInspectMatchingFiles.Unavailable(scope, failure);
            }

            var resolver = new PhysicalPathResolver();
            var root = resolver.ResolveRoot(workspaceRoot);
            if (root.State != PhysicalPathState.Contained)
            {
                return RouteInspectMatchingFiles.Unavailable(scope, RouteInspectMatchingFilesReason.FilesUnavailable);
            }

            var directories = new RouteInspectDirectoryEligibilityCache(root, deadline.Token);
            var candidates = new HashSet<string>(StringComparer.Ordinal);
            foreach (var path in inventory.Paths)
            {
                CheckDeadline();
                var absolute = Path.GetFullPath(path, workspaceRoot);
                if (!PhysicalContainment.Contains(workspaceRoot, absolute))
                {
                    return RouteInspectMatchingFiles.Unavailable(scope, RouteInspectMatchingFilesReason.UnsafePath);
                }

                var relative = Path.GetRelativePath(workspaceRoot, absolute);
                if (relative == "." || string.Equals(Path.GetFileName(absolute), ".git", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var parent = Path.GetDirectoryName(absolute)
                    ?? throw new InvalidOperationException("A contained file requires a parent directory.");
                var physical = directories.Read(parent);
                if (physical.State == PhysicalPathState.External)
                {
                    return RouteInspectMatchingFiles.Unavailable(scope, RouteInspectMatchingFilesReason.UnsafePath);
                }

                if (physical.State == PhysicalPathState.Missing)
                {
                    continue;
                }

                if (physical.State != PhysicalPathState.Contained)
                {
                    return RouteInspectMatchingFiles.Unavailable(scope, RouteInspectMatchingFilesReason.FilesUnavailable);
                }

                FileAttributes attributes;
                try
                {
                    attributes = File.GetAttributes(absolute);
                }
                catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
                {
                    continue;
                }

                if ((attributes & FileAttributes.ReparsePoint) != 0)
                {
                    continue;
                }

                if ((attributes & FileAttributes.Directory) != 0)
                {
                    directories.ObserveAttributes(absolute, attributes);
                    continue;
                }

                // Stable directory mappings and an ordinary leaf preserve the cached containment proof.
                candidates.Add(relative.Replace(Path.DirectorySeparatorChar, '/'));
            }

            var paths = new List<string>();
            foreach (var candidate in candidates)
            {
                CheckDeadline();
                var matches = true;
                foreach (var condition in restrictiveConditions)
                {
                    CheckDeadline();
                    var matched = SourceApplicabilityEvaluator.Evaluate([condition], [candidate]);
                    observeStage?.Invoke(RouteInspectScanStage.Matching);
                    CheckDeadline();
                    if (matched.State != SourceApplicabilityState.Matched)
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    paths.Add(candidate);
                }
            }

            CheckDeadline();
            var result = RouteInspectMatchingFiles.Materialize(
                scope ?? throw new InvalidOperationException("A successful inventory requires a scope."), paths, deadline.Token, inventory.SkippedIgnoreLines);
            observeStage?.Invoke(RouteInspectScanStage.Materialized);
            CheckDeadline();
            return result;
        }
        catch (OperationCanceledException)
        {
            return RouteInspectMatchingFiles.Unavailable(scope, cancellationToken.IsCancellationRequested
                ? RouteInspectMatchingFilesReason.Cancelled
                : RouteInspectMatchingFilesReason.ScanTimeout);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return RouteInspectMatchingFiles.Unavailable(scope, RouteInspectMatchingFilesReason.FilesUnavailable);
        }
        catch (Exception)
        {
            return RouteInspectMatchingFiles.Unavailable(scope, RouteInspectMatchingFilesReason.ScanFailed);
        }
    }

    private static bool HasMatchAll(ImmutableArray<ApplyToPattern> patterns, CancellationToken cancellationToken)
    {
        foreach (var pattern in patterns)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (pattern.Alternatives.IsDefaultOrEmpty)
            {
                if (IsMatchAllAlternative(pattern.Segments, cancellationToken))
                {
                    return true;
                }

                continue;
            }

            foreach (var segments in pattern.Alternatives)
            {
                if (IsMatchAllAlternative(segments, cancellationToken))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsMatchAllAlternative(ImmutableArray<string> segments, CancellationToken cancellationToken)
    {
        var hasRecursiveWildcard = false;
        var otherSegments = 0;
        foreach (var segment in segments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (segment == "**")
            {
                hasRecursiveWildcard = true;
                continue;
            }

            if (++otherSegments > 1 || segment.Length == 0)
            {
                return false;
            }

            foreach (var character in segment)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (character != '*')
                {
                    return false;
                }
            }
        }

        return hasRecursiveWildcard;
    }

    internal static bool HasGitMarker(string directory)
    {
        try
        {
            _ = File.GetAttributes(Path.Combine(directory, ".git"));
            return true;
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return false;
        }
    }
}
