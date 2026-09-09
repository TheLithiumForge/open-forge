using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusMeasurementCalculator
{
    internal static StatusIntegerValue Project(OperationalIntegerObservation observation)
    {
        Validate(observation.State, observation.Value, nameof(observation));
        return new StatusIntegerValue(observation.State, observation.Value);
    }

    internal static StatusIntegerValue Difference(StatusIntegerValue current, StatusIntegerValue initial)
    {
        Validate(current.State, current.Value, nameof(current));
        Validate(initial.State, initial.Value, nameof(initial));
        if (current.State == OperationalValueState.NotApplicable
            || initial.State == OperationalValueState.NotApplicable)
        {
            return new StatusIntegerValue(OperationalValueState.NotApplicable, null);
        }

        if (current.State == OperationalValueState.Unavailable
            || initial.State == OperationalValueState.Unavailable)
        {
            return new StatusIntegerValue(OperationalValueState.Unavailable, null);
        }

        return new StatusIntegerValue(
            OperationalValueState.Available,
            current.Value.GetValueOrDefault() - initial.Value.GetValueOrDefault());
    }

    internal static StatusDecimalValue StartupPercentage(StatusIntegerValue startup, StatusIntegerValue total)
    {
        Validate(startup.State, startup.Value, nameof(startup));
        Validate(total.State, total.Value, nameof(total));
        if (startup.State == OperationalValueState.NotApplicable
            || total.State == OperationalValueState.NotApplicable)
        {
            return new StatusDecimalValue(OperationalValueState.NotApplicable, null);
        }

        if (startup.State == OperationalValueState.Unavailable
            || total.State == OperationalValueState.Unavailable)
        {
            return new StatusDecimalValue(OperationalValueState.Unavailable, null);
        }

        var totalValue = total.Value.GetValueOrDefault();
        var startupValue = startup.Value.GetValueOrDefault();
        if (totalValue == 0)
        {
            var state = startupValue == 0
                ? OperationalValueState.NotApplicable
                : OperationalValueState.Unavailable;
            return new StatusDecimalValue(state, null);
        }

        return new StatusDecimalValue(
            OperationalValueState.Available,
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

    internal static StatusIntegerValue Value(OperationalValueState state, long value)
        => state == OperationalValueState.Available
            ? new StatusIntegerValue(state, value)
            : new StatusIntegerValue(state, null);

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
