using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.References;

internal enum RouteMarkdownCatalogueCoverage
{
    Complete,
    Incomplete,
    Blocked,
    Interrupted,
}

internal enum RouteMarkdownCatalogueFindingCode
{
    IncludedRootUnavailable,
    IncludedRootUnsafe,
    ExcludedPathUnsafe,
    EnumerationUnavailable,
    MarkdownPathUnsafe,
    MarkdownInspectionUnavailable,
    Interrupted,
}

internal sealed record RouteMarkdownCatalogueFilters
{
    internal RouteMarkdownCatalogueFilters(IEnumerable<string> supportedExtensions)
    {
        ArgumentNullException.ThrowIfNull(supportedExtensions);
        var extensions = supportedExtensions
            .Select(extension => SourceWorkspaceRelativePath.ValidateExtension(
                extension,
                nameof(supportedExtensions)))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToImmutableArray();
        if (extensions.IsEmpty)
        {
            throw new ArgumentException(
                "At least one supported Markdown extension is required.",
                nameof(supportedExtensions));
        }

        SupportedExtensions = extensions;
    }

    internal ImmutableArray<string> SupportedExtensions { get; }
}

internal sealed record RouteMarkdownCatalogueRequest
{
    internal RouteMarkdownCatalogueRequest(
        CliWorkspace workspace,
        IEnumerable<string> includedRoots,
        IEnumerable<string> excludedPaths,
        RouteMarkdownCatalogueFilters filters)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(includedRoots);
        ArgumentNullException.ThrowIfNull(excludedPaths);
        ArgumentNullException.ThrowIfNull(filters);
        Workspace = workspace;
        IncludedRoots = SnapshotPaths(includedRoots, nameof(includedRoots), requireValues: true);
        ExcludedPaths = SnapshotPaths(excludedPaths, nameof(excludedPaths), requireValues: false);
        Filters = filters;
    }

    internal CliWorkspace Workspace { get; }

    internal ImmutableArray<string> IncludedRoots { get; }

    internal ImmutableArray<string> ExcludedPaths { get; }

    internal RouteMarkdownCatalogueFilters Filters { get; }

    private static ImmutableArray<string> SnapshotPaths(
        IEnumerable<string> paths,
        string parameterName,
        bool requireValues)
    {
        var values = paths
            .Select(path => SourceWorkspaceRelativePath.Validate(
                path,
                parameterName,
                allowWorkspaceRoot: true))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToImmutableArray();
        if (requireValues && values.IsEmpty)
        {
            throw new ArgumentException(
                "At least one included Markdown root is required.",
                parameterName);
        }

        return values;
    }
}

internal sealed record RouteMarkdownCatalogueFinding
{
    internal RouteMarkdownCatalogueFinding(
        RouteMarkdownCatalogueFindingCode code,
        string? path,
        string cause)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The Route Markdown catalogue finding code is not defined.");
        }

        if (path is not null)
        {
            _ = SourceWorkspaceRelativePath.Validate(
                path,
                nameof(path),
                allowWorkspaceRoot: true);
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        Code = code;
        Path = path;
        Cause = cause;
    }

    internal RouteMarkdownCatalogueFindingCode Code { get; }

    internal string? Path { get; }

    internal string Cause { get; }
}

internal sealed record RouteMarkdownCatalogue
{
    internal RouteMarkdownCatalogue(
        RouteMarkdownCatalogueCoverage coverage,
        IEnumerable<string> selectedPaths,
        IEnumerable<RouteMarkdownCatalogueFinding> findings)
    {
        if (!Enum.IsDefined(coverage))
        {
            throw new ArgumentOutOfRangeException(
                nameof(coverage),
                coverage,
                "The Route Markdown catalogue coverage is not defined.");
        }

        ArgumentNullException.ThrowIfNull(selectedPaths);
        ArgumentNullException.ThrowIfNull(findings);
        Coverage = coverage;
        SelectedPaths = selectedPaths
            .Select(path => SourceWorkspaceRelativePath.ValidateMarkdown(
                path,
                nameof(selectedPaths)))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToImmutableArray();
        Findings = findings
            .Select(finding => finding
                ?? throw new ArgumentException(
                    "Route Markdown catalogue findings cannot contain null members.",
                    nameof(findings)))
            .Distinct()
            .OrderBy(finding => finding.Code)
            .ThenBy(finding => finding.Path, StringComparer.Ordinal)
            .ThenBy(finding => finding.Cause, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    internal RouteMarkdownCatalogueCoverage Coverage { get; }

    internal ImmutableArray<string> SelectedPaths { get; }

    internal ImmutableArray<RouteMarkdownCatalogueFinding> Findings { get; }
}
