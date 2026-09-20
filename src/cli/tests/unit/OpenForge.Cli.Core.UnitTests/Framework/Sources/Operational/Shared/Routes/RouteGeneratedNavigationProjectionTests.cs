using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Operational.Shared.Routes;

public sealed class RouteGeneratedNavigationProjectionTests
{
    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Every named navigation unavailability reason retains its operational state"),
        InlineData("RegionSourceUnsupported", "Unavailable"),
        InlineData("SourceDocumentUnavailable", "Unavailable"),
        InlineData("GeneratedRegionMissing", "Missing"),
        InlineData("GeneratedRegionInvalid", "Unavailable"),
        InlineData("GeneratedRegionUnavailable", "Unavailable"),
        InlineData("GeneratedRegionLineEndingUnsupported", "Unavailable"),
        InlineData("TopologyUnsafe", "Blocked"),
        InlineData("TopologyUnavailable", "Unavailable"),
        InlineData("MetadataUnavailable", "Unavailable"),
        InlineData("MetadataInvalid", "Unavailable"),
        InlineData("MetadataUnrepresentable", "Unavailable"),
        InlineData("DestinationUnsafe", "Blocked"),
        InlineData("DestinationConflict", "Blocked"),
        InlineData("ProjectionUnavailable", "Unavailable"),
        Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void NamedReasonPreservesOperationalState(string reasonName, string expectedStateName)
    {
        var reason = Enum.Parse<GeneratedNavigationRegionUnavailableReason>(reasonName);
        var expectedState = Enum.Parse<OperationalGeneratedNavigationState>(expectedStateName);

        var state = RouteGeneratedNavigationProjection.ReadUnavailableState(reason, "region");

        Assert.Equal(expectedState, state);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Undefined navigation reasons preserve caller-specific error attribution"),
        InlineData("region"),
        InlineData("reason"),
        Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void UndefinedReasonPreservesCallerErrorAttribution(string parameterName)
    {
        var reason = (GeneratedNavigationRegionUnavailableReason)int.MaxValue;

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            RouteGeneratedNavigationProjection.ReadUnavailableState(reason, parameterName));

        Assert.Equal(reason, Assert.IsType<GeneratedNavigationRegionUnavailableReason>(exception.ActualValue));
        Assert.Equal(parameterName, exception.ParamName);
        Assert.StartsWith(
            "The generated-navigation unavailable reason is not defined.",
            exception.Message,
            StringComparison.Ordinal);
    }
}
