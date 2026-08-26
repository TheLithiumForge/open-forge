using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Context.Models.Result;

internal static class ContextResultCollections
{
    internal static IReadOnlyList<T> Snapshot<T>(IEnumerable<T> values, string parameterName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        var materialized = values
            .Select(value => value ?? throw new ArgumentException("Context result collections cannot contain null members.", parameterName))
            .ToArray();
        return new ReadOnlyCollection<T>(materialized);
    }
}
