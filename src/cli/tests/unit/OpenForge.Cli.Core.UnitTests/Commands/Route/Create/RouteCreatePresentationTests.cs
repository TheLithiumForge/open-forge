using OpenForge.Cli.Core.Shell.Pipeline;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Create;
using OpenForge.Cli.Core.Presentation.Route.Create.Shared.Help;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Create;

public sealed class RouteCreatePresentationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Create dry-run JSON retains literal changes and unavailable values"),
     Trait("Feature", "route-create"), Trait("Evidence", "Unit")]
    public void DryRunJsonRetainsExactChangeValues()
    {
        var formation = RouteCreateTestData.PreviewFormation(mode: OpenForge.Cli.Core.Commands.Route.Create.Models.Request.RouteCreateMode.DryRun) with
        {
            Effects = [RouteCreateTestData.CreateEffect(), RouteCreateTestData.ParentEffect()],
            Recovery = new RouteCreateRecovery { State = RouteCreateRecoveryState.NotCreated, ResidualPath = null },
        };
        var presentation = new CliPresentationRequest<RouteCreateResult>(RouteCreateTestData.Result(formation),
            new CliPresentation(CliFormat.Json, CliDetail.Minimal, null));
        using var document = JsonDocument.Parse(
            CliRenderingStage.Render(presentation, RouteCreatePresentation.Rendering).PrimaryContent);
        var root = document.RootElement;
        var data = root.GetProperty("data");

        Assert.Equal("completed", root.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        Assert.Equal("dry-run", data.GetProperty("mode").GetString());
        Assert.Equal(RouteCreateTestData.ParentPath, data.GetProperty("listedIn").GetString());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("template").ValueKind);
        var effects = root.GetProperty("effects").EnumerateArray().ToArray();
        Assert.Equal([RouteCreateTestData.TargetPath, RouteCreateTestData.ParentPath], effects.Select(effect => effect.GetProperty("path").GetString()));
        Assert.Equal(["file", "section"], effects.Select(effect => effect.GetProperty("kind").GetString()));
        Assert.Equal(["created", "rewritten"], effects.Select(effect => effect.GetProperty("action").GetString()));
        Assert.All(effects, effect =>
        {
            Assert.Equal("planned", effect.GetProperty("outcome").GetString());
            Assert.Equal(JsonValueKind.Null, effect.GetProperty("reason").ValueKind);
            Assert.Equal(JsonValueKind.Null, effect.GetProperty("owner").ValueKind);
            Assert.False(effect.TryGetProperty("before", out _));
            Assert.False(effect.TryGetProperty("after", out _));
        });
        Assert.Equal("not-required", root.GetProperty("recovery").GetProperty("disposition").GetString());
        Assert.Empty(root.GetProperty("findings").EnumerateArray());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Create human renderer emits the accepted complete projection"), Trait("Feature", "route-create"), Trait("Evidence", "UnitBehavior")]
    public void HumanRendererEmitsAcceptedCompleteProjection()
    {
        var result = RouteCreateTestData.Result(RouteCreateTestData.VerifiedFormation() with
        {
            Effects =
            [
                RouteCreateTestData.CreateEffect() with
                {
                    Outcome = RouteCreateEffectOutcome.Verified,
                },
                RouteCreateTestData.ParentEffect() with
                {
                    Outcome = RouteCreateEffectOutcome.Verified,
                },
            ],
        });
        var text = CliRenderingStage.Render(new CliPresentationRequest<RouteCreateResult>(
            result,
            new CliPresentation(CliFormat.Text, CliDetail.Standard, null)), RouteCreatePresentation.Rendering).PrimaryContent;

        Assert.StartsWith("Created ", text, StringComparison.Ordinal);
        Assert.Contains(RouteCreateTestData.TargetPath, text, StringComparison.Ordinal);
        Assert.Contains("description: Project overview", text, StringComparison.Ordinal);
        Assert.Contains("Listed in ", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Route Create human views retain partial effects and recovery findings")]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("Feature", "route-create"), Trait("Evidence", "UnitBehavior")]
    public void HumanViewsRetainPartialEffects(bool compact)
    {
        var formation = RouteCreateTestData.PreviewFormation() with
        {
            Effects = [RouteCreateTestData.CreateEffect() with
            {
                Outcome = RouteCreateEffectOutcome.VerificationFailed,
                Residual = RouteCreateEffectResidual.Retained,
            }],
            Recovery = new RouteCreateRecovery { State = RouteCreateRecoveryState.Retained, ResidualPath = "/recovery/create.zip" },
            Verification = RouteCreateVerificationState.Failed,
            Findings = [RouteCreateTestData.Finding(RouteCreateFindingCode.VerificationFailed,
                cause: "The destination verification failed.")],
        };
        var result = RouteCreateTestData.Result(formation);
        var text = CliRenderingStage.Render(new CliPresentationRequest<RouteCreateResult>(result,
            new CliPresentation(CliFormat.Text, compact ? CliDetail.Minimal : CliDetail.Standard, null)),
            RouteCreatePresentation.Rendering).PrimaryContent;

        Assert.StartsWith("Route create stopped after 0 of 1 changes.", text, StringComparison.Ordinal);
        Assert.Contains("did not verify after it was written", text, StringComparison.Ordinal);
        Assert.Contains("Verification failed", text, StringComparison.Ordinal);
        Assert.Contains(RouteCreateTestData.TargetPath, text, StringComparison.Ordinal);
        Assert.Contains("open-forge route create --detail debug", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
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
                CliFormat.Json,
                CliDetail.Standard, null));

        var json = CliRenderingStage.Render(presentation, RouteCreatePresentation.Rendering).PrimaryContent;
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var data = root.GetProperty("data");
        var effect = Assert.Single(root.GetProperty("effects").EnumerateArray());
        var finding = Assert.Single(root.GetProperty("findings").EnumerateArray());

        AssertPropertyOrder(root, "schemaVersion", "command", "status", "detail", "filter", "workspace", "summary", "findings", "effects", "counts", "limitations", "data", "recovery", "next");
        AssertPropertyOrder(data, "mode", "target", "listedIn", "template", "metadata");
        AssertPropertyOrder(data.GetProperty("target"), "id", "path");
        AssertPropertyOrder(data.GetProperty("metadata"), "description", "responsibility", "tags");
        AssertPropertyOrder(effect, "path", "kind", "action", "outcome", "reason", "owner");
        AssertPropertyOrder(finding, "severity", "code", "title", "message", "subject", "category", "resolution", "actions");
        AssertPropertyOrder(root.GetProperty("recovery"), "path", "disposition");
        AssertPropertyOrder(root.GetProperty("next"), "kind", "command", "reason");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Create optional metadata remains nullable in JSON and names a route update"), Trait("Feature", "route-create"), Trait("Evidence", "UnitBehavior")]
    public void OptionalMetadataJsonRemainsNullableAndNamesRouteUpdate()
    {
        var result = RouteCreateTestData.Result(
            RouteCreateTestData.PreviewFormation() with
            {
                Metadata = new RouteCreateMetadata
                {
                    Description = null,
                    Responsibility = null,
                    Tags = [],
                },
                Findings = [RouteCreateTestData.Finding(RouteCreateFindingCode.OptionalMetadata)],
            });
        var presentation = new CliPresentationRequest<RouteCreateResult>(
            result,
            new CliPresentation(CliFormat.Json, CliDetail.Standard, null));

        using var document = JsonDocument.Parse(
            CliRenderingStage.Render(presentation, RouteCreatePresentation.Rendering).PrimaryContent);
        var root = document.RootElement;
        var metadata = root.GetProperty("data").GetProperty("metadata");
        Assert.Equal(JsonValueKind.Null, metadata.GetProperty("description").ValueKind);
        Assert.Equal(JsonValueKind.Null, metadata.GetProperty("responsibility").ValueKind);
        Assert.Empty(metadata.GetProperty("tags").EnumerateArray());
        Assert.Equal(
            "route-create.optional-metadata",
            Assert.Single(root.GetProperty("findings").EnumerateArray()).GetProperty("code").GetString());
        Assert.Equal(
            "open-forge route update memory/project-alpha/overview",
            root.GetProperty("next").GetProperty("command").GetString());
        Assert.Equal(
            "Add an optional description or tag with open-forge route update when useful.",
            root.GetProperty("next").GetProperty("reason").GetString());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Create optional metadata text keeps guidance on the Next line"), Trait("Feature", "route-create"), Trait("Evidence", "UnitBehavior")]
    public void OptionalMetadataTextKeepsGuidanceOnNextLine()
    {
        var result = RouteCreateTestData.Result(
            RouteCreateTestData.VerifiedFormation() with
            {
                Metadata = new RouteCreateMetadata
                {
                    Description = null,
                    Responsibility = null,
                    Tags = [],
                },
                Findings = [RouteCreateTestData.Finding(RouteCreateFindingCode.OptionalMetadata)],
            });
        var command = "open-forge route update memory/project-alpha/overview";
        var text = CliRenderingStage.Render(
            new CliPresentationRequest<RouteCreateResult>(
                result,
                new CliPresentation(CliFormat.Text, CliDetail.Minimal, null)),
            RouteCreatePresentation.Rendering).PrimaryContent;

        Assert.Contains(
            $"Next: {command} (add an optional description or tag when useful).",
            text,
            StringComparison.Ordinal);
        Assert.Equal(1, text.Split("Next: ", StringSplitOptions.None).Length - 1);

        using var document = JsonDocument.Parse(
            CliRenderingStage.Render(
                new CliPresentationRequest<RouteCreateResult>(
                    result,
                    new CliPresentation(CliFormat.Json, CliDetail.Standard, null)),
                RouteCreatePresentation.Rendering).PrimaryContent);
        var next = document.RootElement.GetProperty("next");
        Assert.Equal(command, next.GetProperty("command").GetString());
        Assert.Equal(
            "Add an optional description or tag with open-forge route update when useful.",
            next.GetProperty("reason").GetString());
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Route Create optional metadata uses mode-aware warning wording"),
        InlineData((int)OpenForge.Cli.Core.Commands.Route.Create.Models.Request.RouteCreateMode.DryRun, false, "Would create", "without optional description or tags"),
        InlineData((int)OpenForge.Cli.Core.Commands.Route.Create.Models.Request.RouteCreateMode.Apply, false, "Created", "without optional description or tags"),
        InlineData((int)OpenForge.Cli.Core.Commands.Route.Create.Models.Request.RouteCreateMode.Apply, true, "already has the requested content", "has no optional description or tags"),
        Trait("Feature", "route-create"), Trait("Evidence", "UnitBehavior")]
    public void OptionalMetadataUsesModeAwareWarningWording(
        int modeValue,
        bool repeat,
        string headline,
        string finding)
    {
        var mode = (OpenForge.Cli.Core.Commands.Route.Create.Models.Request.RouteCreateMode)modeValue;
        var formation = (mode == OpenForge.Cli.Core.Commands.Route.Create.Models.Request.RouteCreateMode.DryRun
                ? RouteCreateTestData.PreviewFormation(mode: mode)
                : RouteCreateTestData.VerifiedFormation()) with
        {
            Metadata = new RouteCreateMetadata
            {
                Description = null,
                Responsibility = null,
                Tags = [],
            },
            Effects = repeat ? [] : mode == OpenForge.Cli.Core.Commands.Route.Create.Models.Request.RouteCreateMode.DryRun
                ? [RouteCreateTestData.CreateEffect()]
                : [RouteCreateTestData.CreateEffect() with { Outcome = RouteCreateEffectOutcome.Verified }],
            Verification = repeat
                ? RouteCreateVerificationState.Verified
                : mode == OpenForge.Cli.Core.Commands.Route.Create.Models.Request.RouteCreateMode.DryRun
                    ? RouteCreateVerificationState.NotRequested
                    : RouteCreateVerificationState.Verified,
            Findings = [RouteCreateTestData.Finding(RouteCreateFindingCode.OptionalMetadata)],
        };
        var text = CliRenderingStage.Render(
            new CliPresentationRequest<RouteCreateResult>(
                RouteCreateTestData.Result(formation),
                new CliPresentation(CliFormat.Text, CliDetail.Standard, null)),
            RouteCreatePresentation.Rendering).PrimaryContent;

        Assert.Contains(headline, text, StringComparison.Ordinal);
        Assert.Contains(finding, text, StringComparison.Ordinal);
        Assert.Contains(RouteCreateTestData.TargetPath, text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
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
                CliFormat.Text,
                CliDetail.Debug, null));

        var diagnostic = Assert.IsType<string>(
            CliRenderingStage.Render(presentation, RouteCreatePresentation.Rendering).DiagnosticContent);

        Assert.Contains("generated", diagnostic, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("unsafe", diagnostic, StringComparison.OrdinalIgnoreCase);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Create help exposes only the accepted command surface"), Trait("Feature", "route-create"), Trait("Evidence", "UnitBehavior")]
    public void HelpExposesOnlyAcceptedCommandSurface()
    {
        var help = RouteCreateHelpSections.Create();
        var text = string.Join(Environment.NewLine, help.Sections.Select(section => section.Body));

        Assert.Contains("route create", text, StringComparison.Ordinal);
        Assert.Contains("<file-target>", text, StringComparison.Ordinal);
        Assert.Contains("--description", text, StringComparison.Ordinal);
        Assert.Contains("--tag ", text, StringComparison.Ordinal);
        Assert.Contains("--responsibility", text, StringComparison.Ordinal);
        Assert.Contains("--template", text, StringComparison.Ordinal);
        Assert.Contains("--dry-run", text, StringComparison.Ordinal);
        Assert.DoesNotContain("--automatic", text, StringComparison.Ordinal);
        Assert.DoesNotContain("--force", text, StringComparison.Ordinal);
    }

    private static CliPresentationRequest<RouteCreateResult> Presentation(
        CliFormat format)
        => new(
            RouteCreateTestData.Result(),
            new CliPresentation(
                format,
                CliDetail.Standard, null));

    private static void AssertPropertyOrder(
        JsonElement element,
        params string[] expected)
        => Assert.Equal(
            expected,
            element.EnumerateObject().Select(property => property.Name));
}
