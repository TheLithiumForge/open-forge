namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;

internal sealed class RouteInspectMeasurement
{
    internal RouteInspectMeasurement(
        long physicalFileCount,
        long unicodeScalarCount,
        long utf8ByteCount)
    {
        if (physicalFileCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(physicalFileCount),
                physicalFileCount,
                "Physical-file count cannot be negative.");
        }

        if (unicodeScalarCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(unicodeScalarCount),
                unicodeScalarCount,
                "Unicode scalar count cannot be negative.");
        }

        if (utf8ByteCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(utf8ByteCount),
                utf8ByteCount,
                "UTF-8 byte count cannot be negative.");
        }

        PhysicalFileCount = physicalFileCount;
        UnicodeScalarCount = unicodeScalarCount;
        Utf8ByteCount = utf8ByteCount;
    }

    internal long PhysicalFileCount { get; }

    internal long UnicodeScalarCount { get; }

    internal long Utf8ByteCount { get; }

    internal long EstimatedTokens =>
        UnicodeScalarCount / 4 + (UnicodeScalarCount % 4 == 0 ? 0 : 1);
}

internal sealed class RouteInspectMeasurements
{
    internal RouteInspectMeasurements(
        RouteInspectFact<RouteInspectMeasurement> ownSource,
        RouteInspectFact<RouteInspectMeasurement> selectedClosure,
        RouteInspectFact<RouteInspectMeasurement> taskStartOverlap,
        RouteInspectFact<RouteInspectMeasurement> selectionAddition,
        RouteInspectFact<RouteInspectMeasurement> loadNowDescendants)
    {
        ArgumentNullException.ThrowIfNull(ownSource);
        ArgumentNullException.ThrowIfNull(selectedClosure);
        ArgumentNullException.ThrowIfNull(taskStartOverlap);
        ArgumentNullException.ThrowIfNull(selectionAddition);
        ArgumentNullException.ThrowIfNull(loadNowDescendants);
        OwnSource = ownSource;
        SelectedClosure = selectedClosure;
        TaskStartOverlap = taskStartOverlap;
        SelectionAddition = selectionAddition;
        LoadNowDescendants = loadNowDescendants;
    }

    internal RouteInspectFact<RouteInspectMeasurement> OwnSource { get; }

    internal RouteInspectFact<RouteInspectMeasurement> SelectedClosure { get; }

    internal RouteInspectFact<RouteInspectMeasurement> TaskStartOverlap { get; }

    internal RouteInspectFact<RouteInspectMeasurement> SelectionAddition { get; }

    internal RouteInspectFact<RouteInspectMeasurement> LoadNowDescendants { get; }
}
