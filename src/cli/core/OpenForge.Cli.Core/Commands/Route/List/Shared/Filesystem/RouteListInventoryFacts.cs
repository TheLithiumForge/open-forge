using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal enum RouteListInventoryState
{
    Complete,
    Incomplete,
    Blocked,
    Interrupted,
}

internal sealed class RouteListInventoryFacts
{
    private readonly SourceCatalogue _sourceCatalogue;
    private readonly RouteSourceProjectionBuildResult _projectionBuildResult;

    private RouteListInventoryFacts(
        IEnumerable<RouteListInventorySource> sources,
        IEnumerable<RouteListFilesystemFinding> findings,
        IEnumerable<RouteListPhysicalAlias> physicalAliases,
        SourceCatalogue sourceCatalogue,
        RouteSourceProjectionBuildResult projectionBuildResult)
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(findings);
        ArgumentNullException.ThrowIfNull(physicalAliases);
        ArgumentNullException.ThrowIfNull(sourceCatalogue);
        ArgumentNullException.ThrowIfNull(projectionBuildResult);

        var orderedSources = sources
            .Select(RequireSource)
            .OrderBy(source => source.Source.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        if (orderedSources
            .Select(source => source.Source.CanonicalPath)
            .Distinct(StringComparer.Ordinal)
            .Count() != orderedSources.Length)
        {
            throw new ArgumentException("Route-list inventory sources require unique canonical paths.", nameof(sources));
        }

        var orderedFindings = findings
            .Select((finding, index) => (Finding: RequireFinding(finding), Index: index))
            .OrderBy(item => item.Finding.CanonicalLogicalSubject, StringComparer.Ordinal)
            .ThenBy(item => item.Finding.MachineCode, StringComparer.Ordinal)
            .ThenBy(item => item.Index)
            .Select(item => item.Finding)
            .ToArray();
        var orderedAliases = physicalAliases
            .Select(RequireAlias)
            .OrderBy(alias => alias.CanonicalLogicalPath, StringComparer.Ordinal)
            .ThenBy(alias => alias.PhysicalPath, StringComparer.Ordinal)
            .ThenBy(alias => alias.FirstCanonicalLogicalPath, StringComparer.Ordinal)
            .ToArray();
        if (orderedAliases
            .Select(alias => alias.CanonicalLogicalPath)
            .Distinct(StringComparer.Ordinal)
            .Count() != orderedAliases.Length)
        {
            throw new ArgumentException("A logical path can appear in the physical alias snapshot only once.", nameof(physicalAliases));
        }

        State = DeriveState(orderedFindings);
        Sources = new ReadOnlyCollection<RouteListInventorySource>(orderedSources);
        Findings = new ReadOnlyCollection<RouteListFilesystemFinding>(orderedFindings);
        PhysicalAliases = new ReadOnlyCollection<RouteListPhysicalAlias>(orderedAliases);
        ValidateNeutralInputs(
            sourceCatalogue,
            projectionBuildResult,
            orderedSources);
        _sourceCatalogue = sourceCatalogue;
        _projectionBuildResult = projectionBuildResult;
        OverwriteFacts = projectionBuildResult.ProjectionSet.OverwriteFacts;
    }

    internal RouteListInventoryState State { get; }

    internal IReadOnlyList<RouteListInventorySource> Sources { get; }

    internal IReadOnlyList<RouteListFilesystemFinding> Findings { get; }

    internal IReadOnlyList<RouteListPhysicalAlias> PhysicalAliases { get; }

    internal IReadOnlyList<RouteOverwriteFact> OverwriteFacts { get; }

    internal SourceCatalogue SourceCatalogue => _sourceCatalogue;

    internal RouteSourceProjectionBuildResult ProjectionBuildResult => _projectionBuildResult;

    internal static RouteListInventoryFacts Create(
        SourceCatalogue sourceCatalogue,
        RouteSourceProjectionBuildResult projectionBuildResult,
        IEnumerable<RouteListInventorySource> sources,
        IEnumerable<RouteListFilesystemFinding> findings,
        IEnumerable<RouteListPhysicalAlias> physicalAliases)
    {
        ArgumentNullException.ThrowIfNull(sourceCatalogue);
        ArgumentNullException.ThrowIfNull(projectionBuildResult);
        return new RouteListInventoryFacts(
            sources,
            findings,
            physicalAliases,
            sourceCatalogue,
            projectionBuildResult);
    }

    internal static RouteListInventoryFacts Interrupted(
        SourceCatalogue sourceCatalogue,
        RouteSourceProjectionBuildResult projectionBuildResult,
        IEnumerable<RouteListInventorySource> sources,
        IEnumerable<RouteListFilesystemFinding> knownFindings,
        IEnumerable<RouteListPhysicalAlias> physicalAliases,
        RouteListFilesystemFinding interruption)
    {
        ArgumentNullException.ThrowIfNull(sourceCatalogue);
        ArgumentNullException.ThrowIfNull(projectionBuildResult);
        ValidateInterruption(interruption);
        return new RouteListInventoryFacts(
            sources,
            knownFindings.Append(interruption),
            physicalAliases,
            sourceCatalogue,
            projectionBuildResult);
    }

    private static void ValidateNeutralInputs(
        SourceCatalogue sourceCatalogue,
        RouteSourceProjectionBuildResult projectionBuildResult,
        IReadOnlyList<RouteListInventorySource> sources)
    {
        foreach (var projection in projectionBuildResult.Projections)
        {
            if (!ReferenceEquals(
                    sourceCatalogue.FindByPath(projection.LogicalSource.Identity.CanonicalBasePath),
                    projection.LogicalSource))
            {
                throw new ArgumentException(
                    "Every Route projection must retain a source-catalogue member.",
                    nameof(projectionBuildResult));
            }
        }

        foreach (var read in projectionBuildResult.ReadResults)
        {
            var candidate = sourceCatalogue.FindCandidateByPath(read.Layer.CanonicalPath);
            if (candidate is null
                || !string.Equals(candidate.PhysicalPath, read.Layer.PhysicalPath, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Every source-less Route read must retain a source-catalogue candidate.",
                    nameof(projectionBuildResult));
            }
        }

        var projectedSources = projectionBuildResult.ProjectionSet.Sources
            .ToHashSet(ReferenceEqualityComparer.Instance);
        if (projectedSources.Count != sources.Count
            || sources.Any(source => !projectedSources.Contains(source.Source)))
        {
            throw new ArgumentException(
                "Route-list inventory sources must be the projection-set source instances.",
                nameof(sources));
        }

    }

    private static void ValidateInterruption(RouteListFilesystemFinding interruption)
    {
        ArgumentNullException.ThrowIfNull(interruption);
        if (interruption.Code != RouteListFindingCode.Interrupted
            || interruption.Status != CliSemanticStatus.Interrupted)
        {
            throw new ArgumentException("The cancellation-retention factory requires an interrupted finding.", nameof(interruption));
        }
    }

    private static RouteListInventoryState DeriveState(IReadOnlyList<RouteListFilesystemFinding> findings)
    {
        if (findings.Any(finding => finding.Status == CliSemanticStatus.Interrupted))
        {
            return RouteListInventoryState.Interrupted;
        }

        if (findings.Any(finding => finding.Status == CliSemanticStatus.Blocked))
        {
            return RouteListInventoryState.Blocked;
        }

        if (findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete))
        {
            return RouteListInventoryState.Incomplete;
        }

        return RouteListInventoryState.Complete;
    }

    private static RouteListInventorySource RequireSource(RouteListInventorySource? source)
    {
        return source ?? throw new ArgumentException("Route-list inventory sources cannot contain null.", nameof(source));
    }

    private static RouteListFilesystemFinding RequireFinding(RouteListFilesystemFinding? finding)
    {
        return finding ?? throw new ArgumentException("Route-list inventory findings cannot contain null.", nameof(finding));
    }

    private static RouteListPhysicalAlias RequireAlias(RouteListPhysicalAlias? alias)
    {
        return alias ?? throw new ArgumentException("Route-list inventory aliases cannot contain null.", nameof(alias));
    }
}
