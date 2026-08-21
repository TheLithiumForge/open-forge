using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
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
    private RouteListInventoryFacts(
        IEnumerable<RouteListInventorySource> sources,
        IEnumerable<RouteListFilesystemFinding> findings,
        IEnumerable<RouteListPhysicalAlias> physicalAliases)
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(findings);
        ArgumentNullException.ThrowIfNull(physicalAliases);

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
        Catalogue = new RouteListSourceCatalogue(orderedSources.Select(source => source.Source));
    }

    internal RouteListInventoryState State { get; }

    internal IReadOnlyList<RouteListInventorySource> Sources { get; }

    internal IReadOnlyList<RouteListFilesystemFinding> Findings { get; }

    internal IReadOnlyList<RouteListPhysicalAlias> PhysicalAliases { get; }

    internal RouteListSourceCatalogue Catalogue { get; }

    internal static RouteListInventoryFacts Create(
        IEnumerable<RouteListInventorySource> sources,
        IEnumerable<RouteListFilesystemFinding> findings,
        IEnumerable<RouteListPhysicalAlias> physicalAliases)
    {
        return new RouteListInventoryFacts(sources, findings, physicalAliases);
    }

    internal static RouteListInventoryFacts Interrupted(
        IEnumerable<RouteListInventorySource> sources,
        IEnumerable<RouteListFilesystemFinding> knownFindings,
        IEnumerable<RouteListPhysicalAlias> physicalAliases,
        RouteListFilesystemFinding interruption)
    {
        ArgumentNullException.ThrowIfNull(interruption);
        if (interruption.Code != RouteListFindingCode.Interrupted
            || interruption.Status != CliSemanticStatus.Interrupted)
        {
            throw new ArgumentException("The cancellation-retention factory requires an interrupted finding.", nameof(interruption));
        }

        return new RouteListInventoryFacts(
            sources,
            knownFindings.Append(interruption),
            physicalAliases);
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
