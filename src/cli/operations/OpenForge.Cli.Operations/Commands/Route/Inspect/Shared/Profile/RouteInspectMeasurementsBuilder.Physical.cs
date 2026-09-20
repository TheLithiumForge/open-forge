using System.Text;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;

internal sealed partial class RouteInspectMeasurementsBuilder
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly RouteSourceProjectionSet _projectionSet;
    private readonly CancellationToken _cancellationToken;

    private RouteInspectFact<RouteInspectMeasurement> Measure(
        IEnumerable<string> paths,
        string unavailableReason,
        ISet<string>? excludedPhysicalPaths = null)
    {
        var accumulator = new MeasurementAccumulator(excludedPhysicalPaths);
        foreach (var path in paths.OrderBy(path => path, StringComparer.Ordinal))
        {
            _cancellationToken.ThrowIfCancellationRequested();
            var source = _projectionSet.FindByPath(path);
            if (source is null || !MeasureSource(source, accumulator))
            {
                return Unavailable(unavailableReason);
            }
        }

        return RouteInspectFact<RouteInspectMeasurement>.Available(
            new RouteInspectMeasurement(
                accumulator.PhysicalPaths.Count,
                accumulator.UnicodeScalars,
                accumulator.Utf8Bytes));
    }

    private static bool MeasureSource(RouteSource source, MeasurementAccumulator accumulator)
    {
        if (!MeasureDocument(source.Base, accumulator))
        {
            return false;
        }

        return source.Overwrite is null
            || MeasureDocument(source.Overwrite, accumulator);
    }

    private static bool MeasureDocument(RouteSourceDocument document, MeasurementAccumulator accumulator)
    {
        if (accumulator.ExcludedPhysicalPaths?.Contains(document.PhysicalPath) is true)
        {
            return true;
        }

        if (!accumulator.PhysicalPaths.Add(document.PhysicalPath))
        {
            return true;
        }

        if (document.ReadState != FileReadState.Complete || document.Body is null)
        {
            return false;
        }

        try
        {
            var bytes = StrictUtf8.GetBytes(document.Body);
            if (!string.Equals(StrictUtf8.GetString(bytes), document.Body, StringComparison.Ordinal))
            {
                return false;
            }

            accumulator.UnicodeScalars = checked(
                accumulator.UnicodeScalars + document.Body.EnumerateRunes().LongCount());
            accumulator.Utf8Bytes = checked(accumulator.Utf8Bytes + bytes.LongLength);
            return true;
        }
        catch (EncoderFallbackException)
        {
            return false;
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    private HashSet<string> ReadPhysicalPaths(IEnumerable<string> paths)
    {
        var physicalPaths = new HashSet<string>(PhysicalIdentityTracker.PathComparer);
        foreach (var path in paths.OrderBy(path => path, StringComparer.Ordinal))
        {
            _cancellationToken.ThrowIfCancellationRequested();
            var source = _projectionSet.FindByPath(path);
            if (source is null)
            {
                continue;
            }

            physicalPaths.Add(source.Base.PhysicalPath);
            if (source.Overwrite is not null)
            {
                physicalPaths.Add(source.Overwrite.PhysicalPath);
            }
        }

        return physicalPaths;
    }

    private sealed class MeasurementAccumulator
    {
        internal MeasurementAccumulator(ISet<string>? excludedPhysicalPaths)
        {
            ExcludedPhysicalPaths = excludedPhysicalPaths;
        }

        internal ISet<string> PhysicalPaths { get; } = new HashSet<string>(PhysicalIdentityTracker.PathComparer);

        internal ISet<string>? ExcludedPhysicalPaths { get; }

        internal long UnicodeScalars { get; set; }

        internal long Utf8Bytes { get; set; }
    }
}
