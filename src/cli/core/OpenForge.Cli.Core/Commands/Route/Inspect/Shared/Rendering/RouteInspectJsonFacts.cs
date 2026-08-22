using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectJsonFacts
{
    internal static RouteInspectJsonFact<TJson> Create<TSource, TJson>(
        RouteInspectFact<TSource> fact,
        Func<TSource, TJson> projection)
        where TJson : class
    {
        ArgumentNullException.ThrowIfNull(fact);
        ArgumentNullException.ThrowIfNull(projection);
        if (fact.State != RouteInspectFactState.Value)
        {
            return new RouteInspectJsonFact<TJson>
            {
                State = RouteInspectJsonNames.FactState(fact.State),
                Value = null,
                Reason = fact.Reason,
            };
        }

        if (fact.Value is null)
        {
            throw new InvalidOperationException("An available route-inspect fact must contain a value.");
        }

        return new RouteInspectJsonFact<TJson>
        {
            State = RouteInspectJsonNames.FactState(fact.State),
            Value = projection(fact.Value),
            Reason = null,
        };
    }

    internal static RouteInspectJsonBooleanFact Boolean(RouteInspectFact<bool> fact)
    {
        ArgumentNullException.ThrowIfNull(fact);
        return new RouteInspectJsonBooleanFact
        {
            State = RouteInspectJsonNames.FactState(fact.State),
            Value = fact.State == RouteInspectFactState.Value ? fact.Value : null,
            Reason = fact.Reason,
        };
    }

    internal static RouteInspectJsonFact<RouteInspectJsonMeasurement> Measurement(
        RouteInspectFact<RouteInspectMeasurement> fact)
    {
        return Create(fact, value => new RouteInspectJsonMeasurement
        {
            PhysicalFileCount = value.PhysicalFileCount,
            UnicodeScalarCount = value.UnicodeScalarCount,
            Utf8ByteCount = value.Utf8ByteCount,
            EstimatedTokens = value.EstimatedTokens,
        });
    }

    internal static RouteInspectJsonFact<RouteInspectJsonTopologyCounts> Counts(
        RouteInspectFact<RouteInspectTopologyCounts> fact)
    {
        return Create(fact, value => new RouteInspectJsonTopologyCounts
        {
            DirectRoutedFileCount = value.DirectRoutedFileCount,
            DirectEntrypointCount = value.DirectEntrypointCount,
            DescendantRoutedFileCount = value.DescendantRoutedFileCount,
            DescendantEntrypointCount = value.DescendantEntrypointCount,
        });
    }

    internal static RouteInspectJsonFact<string> LocalAxioms(
        RouteInspectFact<RouteInspectAxiomsLocalState> fact)
    {
        return Create(fact, RouteInspectJsonNames.LocalAxioms);
    }
}
