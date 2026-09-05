using System.Text;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;
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
            .Select(source => new ContextSourceContributionObservation
            {
                SourceId = source.Id
                    ?? throw new InvalidOperationException(
                        "A continuity contribution requires a source ID."),
                Utf8Bytes = source.Layers.Sum(layer =>
                    (long)StrictUtf8.GetByteCount(layer.Text ?? string.Empty)),
                Layers = source.Layers.Select(layer =>
                    new ContextLayerContributionObservation(
                        layer.Path,
                        StrictUtf8.GetByteCount(layer.Text ?? string.Empty))).ToArray(),
            })
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

    private static OperationalIntegerObservation Available(long value)
        => new(OperationalValueState.Available, value);
}
