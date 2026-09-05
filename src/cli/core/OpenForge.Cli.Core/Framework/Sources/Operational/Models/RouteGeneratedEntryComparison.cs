using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Models;

internal enum RouteGeneratedEntryComparisonKind
{
    Missing,
    Extra,
    Order,
    Path,
    Description,
    Tags,
}

internal sealed class RouteGeneratedEntryComparison
{
    internal RouteGeneratedEntryComparison(
        RouteGeneratedEntryComparisonKind kind,
        string? expected,
        string? actual,
        SourceLocation? location)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The generated-entry comparison kind is not defined.");
        }

        var compatible = kind switch
        {
            RouteGeneratedEntryComparisonKind.Missing => expected is not null && actual is null && location is null,
            RouteGeneratedEntryComparisonKind.Extra => expected is null && actual is not null && location is not null,
            RouteGeneratedEntryComparisonKind.Order
                or RouteGeneratedEntryComparisonKind.Path
                or RouteGeneratedEntryComparisonKind.Description
                or RouteGeneratedEntryComparisonKind.Tags =>
                expected is not null && actual is not null && location is not null,
            _ => false,
        };
        if (!compatible || string.Equals(expected, actual, StringComparison.Ordinal))
        {
            throw new ArgumentException("The generated-entry comparison values do not match their exact kind.", nameof(kind));
        }

        Kind = kind;
        Expected = expected;
        Actual = actual;
        Location = location;
    }

    internal RouteGeneratedEntryComparisonKind Kind { get; }

    internal string? Expected { get; }

    internal string? Actual { get; }

    internal SourceLocation? Location { get; }
}
