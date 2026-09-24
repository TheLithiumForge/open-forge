using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Remove;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Remove;

public sealed class RouteRemovePresentationTests
{
    [Trait("Boundary", "Output")]
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

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Remove completed absence names the requested source in its no-op headline"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitBehavior")]
    public void CompletedAbsentSourceUsesRequestedIdentity()
    {
        const string requested = "guidance/notes";
        var formation = RouteRemoveTestData.Formation() with
        {
            Source = RouteRemoveTestData.Source(
                requested: requested,
                id: null,
                path: ".agents/guidance/notes.md"),
            Effects = [],
        };
        var result = new RouteRemoveResult(formation, CliSemanticStatus.Complete, null);

        var report = RouteRemovePresentation.Rendering.Selector(
            result,
            new CliSelection(CliDetail.Minimal));

        Assert.Equal("Nothing to do for guidance/notes.", report.Headline.Sentence);
        Assert.Equal(CliHeadlineKind.NothingToDo, report.Headline.Kind);
    }

    [Trait("Boundary", "Output")]
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

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Route Remove human views do not discard detachment or generated evidence"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitBehavior")]
    [InlineData(false)]
    [InlineData(true)]
    public void CompactRetentionKeepsDetachmentAndGeneratedEvidence(bool compact)
    {
        var parent = RouteRemoveTestData.Effect(
            path: ".agents/guidance/_guidance.md",
            kind: RouteRemoveEffectKind.GeneratedRegion,
            action: RouteRemoveEffectAction.Replace,
            outcome: RouteRemoveEffectOutcome.Verified);
        var formation = RouteRemoveTestData.Formation() with
        {
            Effects =
            [
                RouteRemoveTestData.Effect(outcome: RouteRemoveEffectOutcome.Verified),
                parent,
            ],
        };
        var result = new RouteRemoveResult(
            formation,
            CliSemanticStatus.Complete,
            null);

        var text = CliRenderingStage.Render(
            new CliPresentationRequest<RouteRemoveResult>(result,
                new CliPresentation(CliFormat.Text, compact ? CliDetail.Minimal : CliDetail.Standard, null)),
            RouteRemovePresentation.Rendering).PrimaryContent;
        var detachment = Assert.Single(result.References.Detachments);
        Assert.Contains($"{detachment.SourcePath}:3:5", text, StringComparison.Ordinal);
        if (compact)
        {
            Assert.DoesNotContain(detachment.Before, text, StringComparison.Ordinal);
            Assert.DoesNotContain(detachment.Expected, text, StringComparison.Ordinal);
        }
        else
        {
            Assert.Contains("[Old guide](.agents/guidance/old%20guide.md#part) -> Old guide", text, StringComparison.Ordinal);
        }

        Assert.Contains(parent.Path, text, StringComparison.Ordinal);
        Assert.Contains(Assert.IsType<string>(result.Source.Path), text, StringComparison.Ordinal);
        Assert.Single(result.GeneratedNavigation.Regions);
        Assert.Equal(2, result.Effects.Length);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Remove reports planned settings exclusions and ownership release separately from subject effects"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitBehavior")]
    public void PlannedPersistenceIsTypedAndTruthful()
    {
        var formation = RouteRemoveTestData.Formation() with
        {
            Persistence = new RouteRemovePersistence
            {
                Settings = new RouteRemoveSettingsRemoval
                {
                    Outcome = RouteRemovePersistenceOutcome.Planned,
                    Path = ".agents/open-forge.json",
                    Files = [RouteRemoveTestData.LeafPath, RouteRemoveTestData.OverwritePath],
                },
                Ownership = new RouteRemoveOwnershipRelease
                {
                    Outcome = RouteRemovePersistenceOutcome.Planned,
                    Claims =
                    [
                        new RouteRemoveOwnershipClaim
                        {
                            Path = RouteRemoveTestData.LeafPath,
                            Manager = RouteRemoveOwnershipManager.Extension,
                            Owner = "toolkit",
                        },
                    ],
                },
            },
        };
        var result = new RouteRemoveResult(formation, CliSemanticStatus.Complete, null);

        var text = CliRenderingStage.Render(
            new CliPresentationRequest<RouteRemoveResult>(result,
                new CliPresentation(CliFormat.Text, CliDetail.Full, null)),
            RouteRemovePresentation.Rendering).PrimaryContent;

        Assert.Contains("Would record persistent route removal", text, StringComparison.Ordinal);
        Assert.Contains("Would release 1 managed content ownership claim", text, StringComparison.Ordinal);
        Assert.Contains($"Extension owner toolkit: {RouteRemoveTestData.LeafPath}", text, StringComparison.Ordinal);
        Assert.DoesNotContain("open-forge.json", result.Effects.Select(effect => effect.Path));
        Assert.DoesNotContain("lock.json", result.Effects.Select(effect => effect.Path));
    }
}
