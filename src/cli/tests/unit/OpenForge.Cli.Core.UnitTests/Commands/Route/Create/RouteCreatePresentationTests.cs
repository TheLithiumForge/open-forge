using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Create;

public sealed class RouteCreatePresentationTests
{
    [Fact(DisplayName = "Route Create human renderer emits the accepted complete projection"), Trait("Feature", "route-create"), Trait("Evidence", "UnitBehavior")]
    public void HumanRendererEmitsAcceptedCompleteProjection()
    {
        var text = RouteCreateHumanRenderer.Render(Presentation(CliOutputFormat.Human));

        Assert.Contains("Project overview", text, StringComparison.Ordinal);
        Assert.Contains(RouteCreateTestData.TargetPath, text, StringComparison.Ordinal);
        Assert.Contains("complete", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Route Create JSON renderer emits the ordered envelope"), Trait("Feature", "route-create"), Trait("Evidence", "UnitBehavior")]
    public void JsonRendererEmitsOrderedEnvelope()
    {
        var formation = RouteCreateTestData.PreviewFormation() with
        {
            Effects =
            [
                RouteCreateTestData.CreateEffect() with
                {
                    Outcome = RouteCreateEffectOutcome.VerificationFailed,
                    Residual = RouteCreateEffectResidual.Retained,
                },
            ],
            Verification = RouteCreateVerificationState.Failed,
            Findings =
            [
                RouteCreateTestData.Finding(
                    RouteCreateFindingCode.VerificationFailed,
                    cause: "The destination verification failed."),
            ],
        };
        var presentation = new CliPresentationRequest<RouteCreateResult>(
            RouteCreateTestData.Result(formation),
            new CliPresentation(
                CliOutputFormat.Json,
                CliView.Expanded,
                CliVerbosity.Normal));

        var json = RouteCreateJsonRenderer.Render(presentation);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var result = root.GetProperty("result");
        var effect = Assert.Single(result.GetProperty("effects").EnumerateArray());
        var finding = Assert.Single(result.GetProperty("findings").EnumerateArray());

        AssertPropertyOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");
        AssertPropertyOrder(result, "mode", "target", "parent", "metadata", "template", "plan", "effects", "unchangedPaths", "recovery", "verification", "findings");
        AssertPropertyOrder(result.GetProperty("target"), "requested", "id", "path");
        AssertPropertyOrder(result.GetProperty("parent"), "id", "path", "form");
        AssertPropertyOrder(result.GetProperty("metadata"), "description", "responsibility", "tags");
        AssertPropertyOrder(result.GetProperty("plan"), "completeness", "safety");
        AssertPropertyOrder(effect, "path", "kind", "action", "change", "outcome", "residual");
        AssertPropertyOrder(effect.GetProperty("change"), "before", "expected");
        AssertPropertyOrder(result.GetProperty("recovery"), "state", "residualPath");
        AssertPropertyOrder(finding, "code", "status", "target", "cause");
        AssertPropertyOrder(root.GetProperty("next"), "command", "reason");
    }

    [Fact(DisplayName = "Route Create diagnostic renderer names the direct finding"), Trait("Feature", "route-create"), Trait("Evidence", "UnitBehavior")]
    public void DiagnosticRendererNamesDirectFinding()
    {
        var formation = RouteCreateTestData.BoundaryFormation() with
        {
            Findings =
            [
                RouteCreateTestData.Finding(
                    RouteCreateFindingCode.GeneratedRegionUnsafe,
                    cause: "The parent generated region is unsafe."),
            ],
        };
        var presentation = new CliPresentationRequest<RouteCreateResult>(
            RouteCreateTestData.Result(formation),
            new CliPresentation(
                CliOutputFormat.Human,
                CliView.Expanded,
                CliVerbosity.Verbose));

        var diagnostic = Assert.IsType<string>(
            RouteCreateDiagnosticRenderer.Render(presentation));

        Assert.Contains("generated", diagnostic, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("unsafe", diagnostic, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Route Create help exposes only the accepted command surface"), Trait("Feature", "route-create"), Trait("Evidence", "UnitBehavior")]
    public void HelpExposesOnlyAcceptedCommandSurface()
    {
        var help = RouteCreateHelpSections.Create();
        var text = string.Join(Environment.NewLine, help.Sections.Select(section => section.Body));

        Assert.Contains("route create", text, StringComparison.Ordinal);
        Assert.Contains("<file-target>", text, StringComparison.Ordinal);
        Assert.Contains("--description", text, StringComparison.Ordinal);
        Assert.Contains("--tag=", text, StringComparison.Ordinal);
        Assert.Contains("--responsibility", text, StringComparison.Ordinal);
        Assert.Contains("--template", text, StringComparison.Ordinal);
        Assert.Contains("--dry-run", text, StringComparison.Ordinal);
        Assert.DoesNotContain("--automatic", text, StringComparison.Ordinal);
        Assert.DoesNotContain("--force", text, StringComparison.Ordinal);
    }

    private static CliPresentationRequest<RouteCreateResult> Presentation(
        CliOutputFormat format)
        => new(
            RouteCreateTestData.Result(),
            new CliPresentation(
                format,
                CliView.Expanded,
                CliVerbosity.Normal));

    private static void AssertPropertyOrder(
        JsonElement element,
        params string[] expected)
        => Assert.Equal(
            expected,
            element.EnumerateObject().Select(property => property.Name));
}
