using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Create.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Create;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Route.Create;

[Trait("Feature", "route-create"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
public sealed class RouteCreateEffectPresentationTests
{
    [Theory(DisplayName = "Route Create lists the destination after supporting entrypoints in text and JSON effect order")]
    [InlineData(true)]
    [InlineData(false)]
    public void SupportingEntrypointPrecedesDestination(bool dryRun)
    {
        var mode = dryRun ? RouteCreateMode.DryRun : RouteCreateMode.Apply;
        var outcome = dryRun ? RouteCreateEffectOutcome.Planned : RouteCreateEffectOutcome.Verified;
        var entrypoint = RouteCreateTestData.CreateEffect() with
        {
            Path = ".agents/memory/project-alpha/_project-alpha.md",
            Kind = RouteCreateEffectKind.Entrypoint,
            Outcome = outcome,
        };
        var target = RouteCreateTestData.CreateEffect() with { Outcome = outcome };
        var parent = RouteCreateTestData.ParentEffect() with
        {
            Path = ".agents/memory/_memory.md",
            Outcome = outcome,
        };
        var result = RouteCreateTestData.Result(RouteCreateTestData.PreviewFormation(mode: mode) with
        {
            Effects = [entrypoint, target, parent],
        });

        foreach (var detail in Enum.GetValues<CliDetail>())
        {
            var report = RouteCreateReportSelector.Select(result, new CliSelection(detail, null));
            Assert.Equal(CliSemanticStatus.Complete, report.Status);
            Assert.Equal([entrypoint.Path, target.Path, parent.Path], report.Effects.Select(effect => effect.Path));
            Assert.Collection(report.Data.TextRows,
                row => Assert.Contains(entrypoint.Path, row, StringComparison.Ordinal),
                row => Assert.Contains(target.Path, row, StringComparison.Ordinal),
                row => Assert.Contains(parent.Path, row, StringComparison.Ordinal));
        }
    }
}
