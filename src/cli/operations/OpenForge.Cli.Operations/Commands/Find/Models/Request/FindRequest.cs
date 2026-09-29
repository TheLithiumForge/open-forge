using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Find.Models.Request;

internal sealed record FindUniverseFilter
{
    internal FindUniverseFilter(IEnumerable<string> include, IEnumerable<string> exclude)
    {
        Include = Snapshot(include, nameof(include));
        Exclude = Snapshot(exclude, nameof(exclude));
    }

    internal IReadOnlyList<string> Include { get; }

    internal IReadOnlyList<string> Exclude { get; }

    private static IReadOnlyList<string> Snapshot(IEnumerable<string> values, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        var materialized = values.ToArray();
        if (materialized.Any(value => value is null))
        {
            throw new ArgumentException("Find selector values cannot contain null members.", parameterName);
        }

        return Array.AsReadOnly(materialized);
    }
}

internal sealed record FindRequest
{
    internal FindRequest(
        CliWorkspace workspace,
        FindUniverseFilter universeFilter,
        FindQuery query,
        FindPresentationSelection presentation)
        : this(workspace, universeFilter, [], query, presentation)
    {
    }

    internal FindRequest(
        CliWorkspace workspace,
        FindUniverseFilter universeFilter,
        IEnumerable<string> workingPaths,
        FindQuery query,
        FindPresentationSelection presentation)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(universeFilter);
        ArgumentNullException.ThrowIfNull(workingPaths);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(presentation);
        Workspace = workspace;
        UniverseFilter = universeFilter;
        WorkingPaths = Snapshot(workingPaths, nameof(workingPaths));
        var normalizedWorkingPaths = SourceWorkingPathNormalizer.Normalize(
            workspace.LexicalRoot,
            WorkingPaths);
        if (normalizedWorkingPaths.InvalidPaths.Count != 0)
        {
            throw new ArgumentException(
                $"--for paths must resolve within the selected workspace: {string.Join(", ", normalizedWorkingPaths.InvalidPaths)}.",
                nameof(workingPaths));
        }

        NormalizedWorkingPaths = normalizedWorkingPaths.Paths;
        Query = query;
        Presentation = presentation;
    }

    internal CliWorkspace Workspace { get; }

    internal FindUniverseFilter UniverseFilter { get; }

    internal IReadOnlyList<string> WorkingPaths { get; }

    internal IReadOnlyList<string> NormalizedWorkingPaths { get; }

    internal FindQuery Query { get; }

    internal FindPresentationSelection Presentation { get; }

    private static IReadOnlyList<string> Snapshot(IEnumerable<string> values, string parameterName)
    {
        var materialized = values.ToArray();
        if (materialized.Any(value => value is null))
        {
            throw new ArgumentException("Find working paths cannot contain null members.", parameterName);
        }

        return Array.AsReadOnly(materialized);
    }
}
