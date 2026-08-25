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
        ArgumentNullException.ThrowIfNull(projection);
        var state = RouteInspectJsonNames.FactState(fact.State);
        if (fact.State != RouteInspectFactState.Value)
        {
            return new RouteInspectJsonFact<TJson>
            {
                State = state,
                Value = null,
                Reason = fact.ReadReason(),
            };
        }

        var value = fact.ReadValue();
        return new RouteInspectJsonFact<TJson>
        {
            State = state,
            Value = projection(value),
            Reason = null,
        };
    }

    internal static RouteInspectJsonBooleanFact Boolean(RouteInspectFact<bool> fact)
    {
        var state = RouteInspectJsonNames.FactState(fact.State);
        if (fact.State == RouteInspectFactState.Value)
        {
            return new RouteInspectJsonBooleanFact
            {
                State = state,
                Value = fact.ReadValue(),
                Reason = null,
            };
        }

        return new RouteInspectJsonBooleanFact
        {
            State = state,
            Value = null,
            Reason = fact.ReadReason(),
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
