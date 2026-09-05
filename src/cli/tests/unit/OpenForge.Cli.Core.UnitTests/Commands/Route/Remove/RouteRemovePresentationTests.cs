using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Remove;

public sealed class RouteRemovePresentationTests
{
    [Fact(DisplayName = "Route Remove result exposes the same command identity for every semantic status"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitBehavior")]
    public void ResultCommandIdentityIsStableAcrossStatuses()
    {
        foreach (var status in Enum.GetValues<CliSemanticStatus>())
        {
            var result = new RouteRemoveResult(
                RouteRemoveTestData.Formation(),
                status,
                status == CliSemanticStatus.Complete
                    ? null
                    : new CliNextAction("open-forge doctor", "Inspect the boundary."));

            Assert.Equal("route remove", result.Command);
            Assert.Equal(status, result.Status);
            Assert.NotNull(result.Source);
            Assert.NotNull(result.Subject);
            Assert.NotNull(result.Ownership);
            Assert.NotNull(result.References);
            Assert.NotNull(result.GeneratedNavigation);
            Assert.NotNull(result.Plan);
            Assert.NotNull(result.Recovery);
        }
    }

    [Fact(DisplayName = "Route Remove public status policy retains the shared exit and human stream mapping"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitBehavior")]
    public void PublicStatusPolicyRetainsSharedExitAndStreamMapping()
    {
        var expected = new Dictionary<CliSemanticStatus, (int Exit, CliOutputTarget Target)>
        {
            [CliSemanticStatus.Complete] = (0, CliOutputTarget.StandardOutput),
            [CliSemanticStatus.Failed] = (1, CliOutputTarget.StandardError),
            [CliSemanticStatus.Attention] = (2, CliOutputTarget.StandardOutput),
            [CliSemanticStatus.Incomplete] = (3, CliOutputTarget.StandardOutput),
            [CliSemanticStatus.Invalid] = (4, CliOutputTarget.StandardError),
            [CliSemanticStatus.Blocked] = (5, CliOutputTarget.StandardError),
            [CliSemanticStatus.Interrupted] = (130, CliOutputTarget.StandardError),
        };

        Assert.Equal(expected.Count, Enum.GetValues<CliSemanticStatus>().Length);
        foreach (var status in Enum.GetValues<CliSemanticStatus>())
        {
            var definition = CliStatusDefinitions.Read(status);
            Assert.Equal(expected[status].Exit, definition.Disposition.ExitCode);
            Assert.Equal(expected[status].Target, definition.Disposition.HumanOutputTarget);
        }
    }

    [Fact(DisplayName = "Route Remove compact retention does not discard detachment or generated evidence"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitBehavior")]
    public void CompactRetentionKeepsDetachmentAndGeneratedEvidence()
    {
        var formation = RouteRemoveTestData.Formation();
        var result = new RouteRemoveResult(
            formation,
            CliSemanticStatus.Complete,
            null);

        Assert.Single(result.References.Detachments);
        Assert.Single(result.GeneratedNavigation.Regions);
        Assert.Single(result.Effects);
        Assert.Contains(
            result.UnchangedPaths,
            path => path == ".agents/open-forge.lifecycle.json");
    }
}
