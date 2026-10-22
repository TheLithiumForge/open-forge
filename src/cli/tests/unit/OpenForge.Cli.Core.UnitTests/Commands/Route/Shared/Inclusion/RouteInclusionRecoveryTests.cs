using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Inclusion;

public sealed class RouteInclusionRecoveryTests
{
    [Theory(DisplayName = "Route Include and Exclude recovery attribution round-trips its operation"), Trait("Feature", "route-inclusion"), Trait("Evidence", "Unit")]
    [InlineData(true, "include")]
    [InlineData(false, "exclude")]
    public void AttributionRoundTrips(bool include, string name)
    {
        var operation = include ? RecoveryBundleOperation.Include : RecoveryBundleOperation.Exclude;
        var workspace = RouteInclusionTestData.Invocation().Workspace
            ?? throw new InvalidOperationException("The fixture requires its workspace.");
        var attribution = RecoveryBundleAttribution.Create(RecoveryBundleProducer.Route, operation, workspace);
        var serialized = RecoveryBundleAttributionCodec.Serialize(attribution);
        Assert.Equal(name, serialized.Operation);
        Assert.True(RecoveryBundleAttributionCodec.TryRead(serialized, out var restored));
        Assert.Equal(attribution, restored);
        Assert.Throws<ArgumentException>(() => RecoveryBundleAttribution.Create(RecoveryBundleProducer.Framework, operation, workspace));
    }
}
