using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Models;

internal enum RouteTitleState
{
    Valid,
    Missing,
    Invalid,
    Unavailable,
    NotApplicable,
}

internal enum RouteAxiomsState
{
    Valid,
    Missing,
    Invalid,
    Unavailable,
    NotApplicable,
}

internal sealed record RouteSourceStructureObservation(
    RouteTitleObservation Title,
    RouteAxiomsObservation Axioms);

internal sealed class RouteTitleObservation
{
    private RouteTitleObservation(RouteTitleState state, string? value, SourceLocation? location)
    {
        State = state;
        Value = value;
        Location = location;
    }

    internal RouteTitleState State { get; }

    internal string? Value { get; }

    internal SourceLocation? Location { get; }

    internal static RouteTitleObservation Valid(string value, SourceLocation location)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        ArgumentNullException.ThrowIfNull(location);
        return new(RouteTitleState.Valid, value, location);
    }

    internal static RouteTitleObservation Boundary(RouteTitleState state, SourceLocation? location = null)
    {
        if (state is not (RouteTitleState.Missing or RouteTitleState.Invalid or RouteTitleState.Unavailable)
            || state != RouteTitleState.Invalid && location is not null)
        {
            throw new ArgumentException("The route title boundary state and location are incompatible.", nameof(state));
        }

        return new(state, value: null, location);
    }

    internal static RouteTitleObservation NotApplicable()
        => new(RouteTitleState.NotApplicable, value: null, location: null);
}

internal sealed class RouteAxiomsObservation
{
    private RouteAxiomsObservation(RouteAxiomsState state, SourceLocation? location)
    {
        State = state;
        Location = location;
    }

    internal RouteAxiomsState State { get; }

    internal SourceLocation? Location { get; }

    internal static RouteAxiomsObservation Valid(SourceLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        return new(RouteAxiomsState.Valid, location);
    }

    internal static RouteAxiomsObservation Boundary(RouteAxiomsState state, SourceLocation? location = null)
    {
        if (state is not (RouteAxiomsState.Missing or RouteAxiomsState.Invalid or RouteAxiomsState.Unavailable)
            || state != RouteAxiomsState.Invalid && location is not null)
        {
            throw new ArgumentException("The route Axioms boundary state and location are incompatible.", nameof(state));
        }

        return new(state, location);
    }

    internal static RouteAxiomsObservation NotApplicable()
        => new(RouteAxiomsState.NotApplicable, location: null);
}

internal enum RouteWorkspaceSourceIssueKind
{
    FrontmatterMalformed,
    FrontmatterDuplicate,
    ParseIncomplete,
}

internal sealed class RouteWorkspaceSourceIssue
{
    private RouteWorkspaceSourceIssue(
        RouteWorkspaceSourceIssueKind kind,
        string path,
        SourceLocation? location)
    {
        if (!SourceLogicalPath.IsCanonicalSource(path))
        {
            throw new ArgumentException(
                "A workspace source issue requires a canonical source path.",
                nameof(path));
        }
        var compatible = kind switch
        {
            RouteWorkspaceSourceIssueKind.FrontmatterDuplicate => location is not null,
            RouteWorkspaceSourceIssueKind.FrontmatterMalformed => true,
            RouteWorkspaceSourceIssueKind.ParseIncomplete => location is null,
            _ => false,
        };
        if (!compatible)
        {
            throw new ArgumentException("The workspace source issue kind and location are incompatible.", nameof(kind));
        }

        Kind = kind;
        Path = path;
        Location = location;
    }

    internal RouteWorkspaceSourceIssueKind Kind { get; }

    internal string Path { get; }

    internal SourceLocation? Location { get; }

    internal static RouteWorkspaceSourceIssue At(
        RouteWorkspaceSourceIssueKind kind,
        string path,
        SourceLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        return new(kind, path, location);
    }

    internal static RouteWorkspaceSourceIssue WithoutLocation(
        RouteWorkspaceSourceIssueKind kind,
        string path)
        => new(kind, path, location: null);
}
