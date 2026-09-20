using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
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
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(universeFilter);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(presentation);
        Workspace = workspace;
        UniverseFilter = universeFilter;
        Query = query;
        Presentation = presentation;
    }

    internal CliWorkspace Workspace { get; }

    internal FindUniverseFilter UniverseFilter { get; }

    internal FindQuery Query { get; }

    internal FindPresentationSelection Presentation { get; }
}
