using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

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

    [Theory(DisplayName = "Route Remove human views do not discard detachment or generated evidence"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitBehavior")]
    [InlineData(false)]
    [InlineData(true)]
    public void CompactRetentionKeepsDetachmentAndGeneratedEvidence(bool compact)
    {
        var formation = RouteRemoveTestData.Formation();
        var result = new RouteRemoveResult(
            formation,
            CliSemanticStatus.Complete,
            null);

        var text = RouteRemoveHumanRenderer.Render(new CliPresentationRequest<RouteRemoveResult>(result,
            new CliPresentation(CliOutputFormat.Human, compact ? CliView.Compact : CliView.Expanded, CliVerbosity.Normal)));
        var detachment = Assert.Single(result.References.Detachments);
        Assert.Contains($"{detachment.SourcePath}:3:5", text, StringComparison.Ordinal);
        Assert.Contains(detachment.Before, text, StringComparison.Ordinal);
        Assert.Contains(detachment.Expected, text, StringComparison.Ordinal);
        Assert.Contains(".agents/open-forge.lifecycle.json", text, StringComparison.Ordinal);
        Assert.Contains(Assert.Single(result.GeneratedNavigation.Regions).Path, text, StringComparison.Ordinal);
        Assert.Contains(Assert.Single(result.Effects).Path, text, StringComparison.Ordinal);
        Assert.Single(result.GeneratedNavigation.Regions);
        Assert.Single(result.Effects);
        Assert.Contains(
            result.UnchangedPaths,
            path => path == ".agents/open-forge.lifecycle.json");
    }
}
