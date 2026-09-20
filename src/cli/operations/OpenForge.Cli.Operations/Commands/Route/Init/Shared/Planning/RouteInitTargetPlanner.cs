using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal enum RouteInitTargetKind
{
    SourceId,
    ExactPath,
}

internal enum RouteInitTargetResolutionState
{
    Resolved,
    RequiresFrameworkAlignment,
    Invalid,
}

internal sealed record RouteInitTargetFacts
{
    internal RouteInitTargetFacts(
        string requested,
        RouteInitTargetKind kind,
        IEnumerable<string> requestedSegments,
        IEnumerable<string>? canonicalSegments,
        string? id,
        string? canonicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requested);
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Route Init target kind is not defined.");
        }

        Requested = requested;
        Kind = kind;
        RequestedSegments = CopySegments(requestedSegments, nameof(requestedSegments));
        CanonicalSegments = canonicalSegments is null
            ? null
            : CopySegments(canonicalSegments, nameof(canonicalSegments));
        Id = id;
        CanonicalPath = canonicalPath;
    }

    internal string Requested { get; }

    internal RouteInitTargetKind Kind { get; }

    internal IReadOnlyList<string> RequestedSegments { get; }

    internal IReadOnlyList<string>? CanonicalSegments { get; }

    internal string? Id { get; }

    internal string? CanonicalPath { get; }

    private static IReadOnlyList<string> CopySegments(
        IEnumerable<string> values,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        var materialized = values
            .Select(value => value ?? throw new ArgumentException(
                "Route Init target segments cannot contain null members.",
                parameterName))
            .ToArray();
        if (materialized.Length == 0)
        {
            throw new ArgumentException("A Route Init target requires at least one segment.", parameterName);
        }

        return new ReadOnlyCollection<string>(materialized);
    }
}

internal sealed record RouteInitTargetResolution(
    RouteInitTargetResolutionState State,
    RouteInitTargetFacts? Target,
    string? Cause)
{
    internal bool IsResolved => State is RouteInitTargetResolutionState.Resolved
        or RouteInitTargetResolutionState.RequiresFrameworkAlignment;
}

internal sealed class RouteInitTargetPlanner
{
    internal RouteInitTargetResolution Resolve(RouteInitRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return IsExactPath(request.RouteTarget)
            ? ResolveExactPath(request.RouteTarget)
            : ResolveId(request);
    }

    internal RouteInitTargetFacts ResolveAlignedFrameworkTarget(
        RouteInitTargetFacts requested,
        IEnumerable<string> concreteSegments)
    {
        ArgumentNullException.ThrowIfNull(requested);
        var segments = concreteSegments.ToArray();
        if (segments.Length == 0 || segments.Any(segment => !SourceLogicalPath.IsCanonicalSegment(segment)))
        {
            throw new ArgumentException(
                "An aligned Framework target requires canonical concrete segments.",
                nameof(concreteSegments));
        }

        var id = string.Join('/', segments);
        var path = requested.Kind == RouteInitTargetKind.ExactPath
            ? requested.CanonicalPath
                ?? throw new ArgumentException(
                    "An exact Framework target requires its requested entrypoint path.",
                    nameof(requested))
            : ReadCanonicalEntrypointPath(segments);
        return new RouteInitTargetFacts(
            requested.Requested,
            requested.Kind,
            requested.RequestedSegments,
            segments,
            id,
            path);
    }

    internal static IReadOnlyList<string> ReadChainPaths(RouteInitTargetFacts target)
    {
        ArgumentNullException.ThrowIfNull(target);
        var segments = target.CanonicalSegments
            ?? throw new ArgumentException(
                "Route Init chain paths require an aligned target.",
                nameof(target));
        return Enumerable.Range(1, segments.Count)
            .Select(length => ReadCanonicalEntrypointPath(segments.Take(length).ToArray()))
            .ToArray();
    }

    private static RouteInitTargetResolution ResolveExactPath(string value)
    {
        var parsed = SourceReferenceParser.Parse(value);
        if (parsed.State != SourceReferenceParseState.Valid
            || parsed.Kind != SourceReferenceKind.SourcePath
            || parsed.AttemptedPath is not { } path)
        {
            return Invalid(parsed.Cause ?? "The exact Route Init target path is invalid.");
        }

        if (string.Equals(path, SourceLogicalPath.LoaderPath, StringComparison.Ordinal)
            || !SourceFormClassifier.TryClassify(path, out var form)
            || !SourceFormClassifier.IsEntrypoint(form))
        {
            return Invalid("An exact Route Init target must identify one recognized entrypoint and cannot identify the Loader.");
        }

        var id = SourceIdentity.DeriveId(path);
        if (id is null)
        {
            return Invalid("The exact Route Init target has no canonical route identity.");
        }

        var segments = id.Split('/', StringSplitOptions.None);
        return Resolved(new RouteInitTargetFacts(
            value,
            RouteInitTargetKind.ExactPath,
            segments,
            segments,
            id,
            path));
    }

    private static RouteInitTargetResolution ResolveId(RouteInitRequest request)
    {
        var segments = request.RouteTarget.Split('/', StringSplitOptions.None);
        if (segments.Length == 0
            || segments.Any(IsStructurallyInvalidSegment)
            || string.Equals(request.RouteTarget, "loader", StringComparison.Ordinal))
        {
            return Invalid("The Route Init target ID contains an empty, traversal, control, or Loader segment.");
        }

        if (request.Scaffold == RouteInitScaffold.Framework)
        {
            return new RouteInitTargetResolution(
                RouteInitTargetResolutionState.RequiresFrameworkAlignment,
                new RouteInitTargetFacts(
                    request.RouteTarget,
                    RouteInitTargetKind.SourceId,
                    segments,
                    canonicalSegments: null,
                    id: null,
                    canonicalPath: null),
                Cause: null);
        }

        if (!SourceIdentity.IsValidId(request.RouteTarget))
        {
            return Invalid("The generic Route Init target is not a canonical source ID.");
        }

        return Resolved(new RouteInitTargetFacts(
            request.RouteTarget,
            RouteInitTargetKind.SourceId,
            segments,
            segments,
            request.RouteTarget,
            ReadCanonicalEntrypointPath(segments)));
    }

    private static bool IsExactPath(string value)
        => value.StartsWith(".agents/", StringComparison.Ordinal)
            || value.StartsWith("./.agents/", StringComparison.Ordinal);

    private static bool IsStructurallyInvalidSegment(string segment)
        => string.IsNullOrEmpty(segment)
            || segment is "." or ".."
            || segment.IndexOfAny(['/', '\\']) >= 0
            || segment.Any(char.IsControl);

    private static string ReadCanonicalEntrypointPath(IReadOnlyList<string> segments)
    {
        var folder = $"{SourceLogicalPath.AgentsRoot}/{string.Join('/', segments)}";
        return $"{folder}/_{segments[^1]}.md";
    }

    private static RouteInitTargetResolution Resolved(RouteInitTargetFacts target)
        => new(RouteInitTargetResolutionState.Resolved, target, Cause: null);

    private static RouteInitTargetResolution Invalid(string cause)
        => new(RouteInitTargetResolutionState.Invalid, Target: null, cause);
}
