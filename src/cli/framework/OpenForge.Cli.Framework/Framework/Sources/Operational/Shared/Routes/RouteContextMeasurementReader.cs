using System.Text;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Context;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;

internal static class RouteContextMeasurementReader
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal static ContextMeasurementObservation Measure(
        IEnumerable<RouteContextSource> sources,
        bool isComplete)
    {
        var layers = sources.SelectMany(source => source.Layers).ToArray();
        if (!isComplete || layers.Any(layer => layer.Text is null))
        {
            return Unavailable();
        }

        var characters = layers.Sum(layer =>
            layer.Text?.EnumerateRunes().LongCount() ?? 0L);
        var bytes = layers.Sum(layer =>
            (long)StrictUtf8.GetByteCount(layer.Text ?? string.Empty));
        return new ContextMeasurementObservation(
            Available(layers.LongLength),
            Available(characters),
            Available(bytes),
            Available(characters / 4L + (characters % 4L == 0 ? 0L : 1L)));
    }

    internal static IReadOnlyList<ContextSourceContributionObservation> Contributions(
        RouteContextClosure closure)
    {
        if (!closure.IsComplete)
        {
            return [];
        }

        return closure.Continuity
            .Where(source => source.Id is not null
                && source.Layers.All(layer => layer.Text is not null))
            .Select(ReadContribution)
            .ToArray();
    }

    internal static bool IsUnavailable(ContextMeasurementObservation value)
        => value.Files.State == OperationalValueState.Unavailable;

    internal static ContextMeasurementObservation Unavailable()
    {
        var value = new OperationalIntegerObservation(
            OperationalValueState.Unavailable,
            null);
        return new ContextMeasurementObservation(value, value, value, value);
    }

    private static ContextSourceContributionObservation ReadContribution(RouteContextSource source)
    {
        var sourceId = source.Id
            ?? throw new InvalidOperationException("A continuity contribution requires a source ID.");
        var layers = new List<ContextLayerContributionObservation>();
        var totalBytes = 0L;
        foreach (var layer in source.Layers)
        {
            var bytes = StrictUtf8.GetByteCount(layer.Text ?? string.Empty);
            totalBytes = checked(totalBytes + bytes);
            layers.Add(new ContextLayerContributionObservation(layer.Path, bytes));
        }

        return new ContextSourceContributionObservation
        {
            SourceId = sourceId,
            Utf8Bytes = totalBytes,
            Layers = layers.ToArray(),
        };
    }

    private static OperationalIntegerObservation Available(long value)
        => new(OperationalValueState.Available, value);
}
