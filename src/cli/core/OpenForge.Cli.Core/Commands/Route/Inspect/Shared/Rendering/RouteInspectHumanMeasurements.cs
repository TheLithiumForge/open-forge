using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectHumanMeasurements
{
    internal static string Measurement(RouteInspectFact<RouteInspectMeasurement> fact)
    {
        return fact.State switch
        {
            RouteInspectFactState.Value => Value(fact.Value!),
            RouteInspectFactState.Unavailable => $"unavailable ({RouteInspectHumanValues.Text(fact.Reason!)})",
            RouteInspectFactState.NotApplicable => $"not applicable ({RouteInspectHumanValues.Text(fact.Reason!)})",
            _ => throw new ArgumentOutOfRangeException(nameof(fact), fact.State, "The fact state is not defined."),
        };
    }

    internal static string Fact<T>(RouteInspectFact<T> fact)
    {
        return fact.State switch
        {
            RouteInspectFactState.Value => "value",
            RouteInspectFactState.Unavailable => $"unavailable ({RouteInspectHumanValues.Text(fact.Reason!)})",
            RouteInspectFactState.NotApplicable => $"not applicable ({RouteInspectHumanValues.Text(fact.Reason!)})",
            _ => throw new ArgumentOutOfRangeException(nameof(fact), fact.State, "The fact state is not defined."),
        };
    }

    private static string Value(RouteInspectMeasurement measurement)
    {
        return string.Join(
            " · ",
            $"{measurement.PhysicalFileCount.ToString(CultureInfo.InvariantCulture)} files",
            Bytes(measurement.Utf8ByteCount),
            $"~{measurement.EstimatedTokens.ToString(CultureInfo.InvariantCulture)} tokens");
    }

    private static string Bytes(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes.ToString(CultureInfo.InvariantCulture)} B";
        }

        if (bytes < 1024 * 1024)
        {
            return $"{(bytes / 1024d).ToString("0.##", CultureInfo.InvariantCulture)} KiB";
        }

        return $"{(bytes / (1024d * 1024d)).ToString("0.##", CultureInfo.InvariantCulture)} MiB";
    }
}
