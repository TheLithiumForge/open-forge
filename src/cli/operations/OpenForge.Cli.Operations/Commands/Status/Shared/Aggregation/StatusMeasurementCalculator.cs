using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Context;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusMeasurementCalculator
{
    internal static StatusIntegerValue Project(OperationalIntegerObservation observation)
    {
        Validate(observation.State, observation.Value, nameof(observation));
        return new StatusIntegerValue(StatusStateMap.Value(observation.State), observation.Value);
    }

    internal static StatusIntegerValue Difference(StatusIntegerValue current, StatusIntegerValue initial)
    {
        Validate(current.State, current.Value, nameof(current));
        Validate(initial.State, initial.Value, nameof(initial));
        if (current.State == StatusValueState.NotApplicable
            || initial.State == StatusValueState.NotApplicable)
        {
            return new StatusIntegerValue(StatusValueState.NotApplicable, null);
        }

        if (current.State == StatusValueState.Unavailable
            || initial.State == StatusValueState.Unavailable)
        {
            return new StatusIntegerValue(StatusValueState.Unavailable, null);
        }

        return new StatusIntegerValue(
            StatusValueState.Available,
            current.Value.GetValueOrDefault() - initial.Value.GetValueOrDefault());
    }

    internal static StatusDecimalValue StartupPercentage(StatusIntegerValue startup, StatusIntegerValue total)
    {
        Validate(startup.State, startup.Value, nameof(startup));
        Validate(total.State, total.Value, nameof(total));
        if (startup.State == StatusValueState.NotApplicable
            || total.State == StatusValueState.NotApplicable)
        {
            return new StatusDecimalValue(StatusValueState.NotApplicable, null);
        }

        if (startup.State == StatusValueState.Unavailable
            || total.State == StatusValueState.Unavailable)
        {
            return new StatusDecimalValue(StatusValueState.Unavailable, null);
        }

        var totalValue = total.Value.GetValueOrDefault();
        var startupValue = startup.Value.GetValueOrDefault();
        if (totalValue == 0)
        {
            var state = startupValue == 0
                ? StatusValueState.NotApplicable
                : StatusValueState.Unavailable;
            return new StatusDecimalValue(state, null);
        }

        return new StatusDecimalValue(
            StatusValueState.Available,
            startupValue * 100m / totalValue);
    }

    internal static StatusMeasurement Project(ContextMeasurementObservation observation)
    {
        return new StatusMeasurement(
            Project(observation.Files),
            Project(observation.Characters),
            Project(observation.Utf8Bytes),
            Project(observation.EstimatedTokens));
    }

    internal static StatusMeasurement Difference(StatusMeasurement current, StatusMeasurement initial)
    {
        return new StatusMeasurement(
            Difference(current.Files, initial.Files),
            Difference(current.Characters, initial.Characters),
            Difference(current.Utf8Bytes, initial.Utf8Bytes),
            Difference(current.EstimatedTokens, initial.EstimatedTokens));
    }

    internal static StatusIntegerValue Value(StatusValueState state, long value)
        => state == StatusValueState.Available
            ? new StatusIntegerValue(state, value)
            : new StatusIntegerValue(state, null);

    private static void Validate(StatusValueState state, long? value, string name)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(name, state, "The Status value state is not defined.");
        }

        var valid = state == StatusValueState.Available ? value.HasValue : value is null;
        if (!valid)
        {
            throw new ArgumentException("The Status value does not match its state.", name);
        }
    }

    private static void Validate(OperationalValueState state, long? value, string name)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(name, state, "The operational value state is not defined.");
        }

        var valid = state == OperationalValueState.Available ? value.HasValue : value is null;
        if (!valid)
        {
            throw new ArgumentException("The operational value does not match its state.", name);
        }
    }
}
