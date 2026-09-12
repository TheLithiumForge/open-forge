using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Update;

public sealed class RouteUpdatePresentationTests
{
    [Theory(DisplayName = "Route Update invalid blocked and incomplete human results retain exact disposition"),
     InlineData((int)RouteUpdateFindingCode.InvalidPatch, "invalid", 4, (int)CliOutputTarget.StandardError),
     InlineData((int)RouteUpdateFindingCode.IdentityCollision, "blocked", 5, (int)CliOutputTarget.StandardError),
     InlineData((int)RouteUpdateFindingCode.WorkspaceUnavailable, "incomplete", 3, (int)CliOutputTarget.StandardOutput),
     Trait("Feature", "route-update"), Trait("Evidence", "Unit")]
    public void InvalidBlockedAndIncompleteResultsRetainHumanDisposition(int findingValue, string expectedStatus, int expectedExit, int expectedTarget)
    {
        var result = RouteUpdateTestData.Result(RouteUpdateTestData.VerifiedNoOpFormation(
            findings: [RouteUpdateTestData.Finding((RouteUpdateFindingCode)findingValue)]));
        var rendered = CliRenderingStage.Render(
            Presentation(result, CliOutputFormat.Human),
            new CliRendererSet<RouteUpdateResult>(RouteUpdateHumanRenderer.Render, RouteUpdateJsonRenderer.Render),
            RouteUpdateDiagnosticRenderer.Render);

        Assert.Contains($"Status: {expectedStatus}", rendered.PrimaryContent, StringComparison.Ordinal);
        Assert.Equal((CliOutputTarget)expectedTarget, rendered.PrimaryTarget);
        Assert.Equal(expectedExit, CliStatusDefinitions.Read(result.Status).Disposition.ExitCode);
    }

    [Fact(DisplayName = "Route Update human renderer exposes the complete no-op facts in stable order"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void HumanRendererExposesCompleteNoOpFactsInStableOrder()
    {
        var text = RouteUpdateHumanRenderer.Render(
            Presentation(RouteUpdateTestData.Result(), CliOutputFormat.Human));

        AssertInOrder(
            text,
            "The routed source is up to date.",
            $"Workspace: {RouteUpdateTestData.Workspace().LexicalRoot}",
            "Selected by: --workspace",
            $"ID: {RouteUpdateTestData.TargetId}",
            $"Path: {RouteUpdateTestData.TargetPath}",
            "Mode: apply",
            "Status: complete",
            "Plan: completeness=complete, safety=safe, body=preserved",
            "No files changed.",
            "Unchanged:",
            $"  {RouteUpdateTestData.ParentPath}",
            $"  {RouteUpdateTestData.TargetPath}",
            "Recovery: not-required",
            "Verification: verified");
    }

    [Fact(DisplayName = "Route Update human renderer states protected Template attention and next action"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void HumanRendererStatesProtectedTemplateAttentionAndNextAction()
    {
        var result = ProtectedBodyResult();

        var text = RouteUpdateHumanRenderer.Render(
            Presentation(result, CliOutputFormat.Human));

        Assert.Contains(
            "The routed source requires attention.",
            text,
            StringComparison.Ordinal);
        Assert.Contains("Status: requires attention", text, StringComparison.Ordinal);
        Assert.Contains("Template: templates/topic (.agents/templates/topic.md) / authored-body-protected", text, StringComparison.Ordinal);
        Assert.Contains(
            "Template body not applied: the target already has authored body content.",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            "Next: open-forge route update — review the authored body; the Template body was not applied.",
            text,
            StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route Update human renderer names changed fields and generated effect paths"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void HumanRendererNamesChangedFieldsAndEffectPaths()
    {
        var formation = RouteUpdateTestData.VerifiedNoOpFormation(
            CopiedTemplate(),
            RouteUpdateBodyState.TemplateCopied) with
        {
            Patch = ChangedPatch(),
            Effects =
            [
                RouteUpdateTestData.Effect(),
                RouteUpdateTestData.Effect(
                    RouteUpdateTestData.ParentPath,
                    RouteUpdateEffectKind.GeneratedRegion),
            ],
            UnchangedPaths = [],
            Recovery = new RouteUpdateRecovery
            {
                State = RouteUpdateRecoveryState.Removed,
                ResidualPath = null,
            },
        };

        var text = RouteUpdateHumanRenderer.Render(
            Presentation(RouteUpdateTestData.Result(formation), CliOutputFormat.Human));

        Assert.StartsWith("The routed source was updated.", text, StringComparison.Ordinal);
        Assert.Contains(
            "Changed: description, responsibility, tags, Template body",
            text,
            StringComparison.Ordinal);
        Assert.Contains(RouteUpdateTestData.ParentPath, text, StringComparison.Ordinal);

        var compact = RouteUpdateHumanRenderer.Render(
            Presentation(
                RouteUpdateTestData.Result(formation),
                CliOutputFormat.Human,
                CliView.Compact));

        Assert.Contains("Before: before-hash", compact, StringComparison.Ordinal);
        Assert.Contains("Expected: expected-hash", compact, StringComparison.Ordinal);

        var dryRun = formation with
        {
            Mode = RouteUpdateMode.DryRun,
            Effects =
            [
                .. formation.Effects.Select(effect => effect with
                {
                    Outcome = RouteUpdateEffectOutcome.Planned,
                }),
            ],
            Recovery = new RouteUpdateRecovery
            {
                State = RouteUpdateRecoveryState.NotCreated,
                ResidualPath = null,
            },
            Verification = RouteUpdateVerificationState.NotRequested,
        };
        var dryRunText = RouteUpdateHumanRenderer.Render(
            Presentation(RouteUpdateTestData.Result(dryRun), CliOutputFormat.Human));

        Assert.StartsWith(
            "The routed source would be updated.",
            dryRunText,
            StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route Update dry-run no-op human summary remains up to date"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void DryRunNoOpHumanSummaryRemainsUpToDate()
    {
        var formation = RouteUpdateTestData.VerifiedNoOpFormation() with
        {
            Mode = RouteUpdateMode.DryRun,
            Recovery = new RouteUpdateRecovery
            {
                State = RouteUpdateRecoveryState.NotCreated,
                ResidualPath = null,
            },
            Verification = RouteUpdateVerificationState.NotRequested,
        };

        var text = RouteUpdateHumanRenderer.Render(
            Presentation(RouteUpdateTestData.Result(formation), CliOutputFormat.Human));

        Assert.StartsWith("The routed source is up to date.", text, StringComparison.Ordinal);
        Assert.DoesNotContain("would be updated", text, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Route Update compact errors name command target and direct cause"),
     InlineData((int)RouteUpdateFindingCode.InvalidInput),
     InlineData((int)RouteUpdateFindingCode.WorkspaceUnsafe),
     InlineData((int)RouteUpdateFindingCode.WorkspaceUnavailable),
     InlineData((int)RouteUpdateFindingCode.WriteFailed),
     InlineData((int)RouteUpdateFindingCode.Interrupted),
     Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void CompactErrorsNameCommandTargetAndDirectCause(int codeValue)
    {
        const string cause = "The direct primary cause.";
        var code = (RouteUpdateFindingCode)codeValue;
        var formation = RouteUpdateTestData.VerifiedNoOpFormation(
            findings: [RouteUpdateTestData.Finding(code, cause: cause)]) with
        {
            Target = RouteUpdateTestData.Target() with
            {
                SelectedBy = null,
                Id = null,
                Path = null,
                Form = null,
            },
        };

        var text = RouteUpdateHumanRenderer.Render(
            Presentation(
                RouteUpdateTestData.Result(formation),
                CliOutputFormat.Human,
                CliView.Compact));

        Assert.Contains("Route Update", text, StringComparison.Ordinal);
        Assert.Contains(RouteUpdateTestData.TargetId, text, StringComparison.Ordinal);
        Assert.Contains(cause, text, StringComparison.Ordinal);
        Assert.Contains($"Target: {RouteUpdateTestData.TargetId}", text, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route Update JSON renderer projects exact schema-v1 facts and nulls"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void JsonRendererProjectsExactSchemaV1FactsAndNulls()
    {
        var json = RouteUpdateJsonRenderer.Render(
            Presentation(RouteUpdateTestData.Result(), CliOutputFormat.Json));
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var result = root.GetProperty("result");
        var target = result.GetProperty("target");
        var patch = result.GetProperty("patch");

        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("route update", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal(
            RouteUpdateTestData.Workspace().LexicalRoot,
            root.GetProperty("workspace").GetProperty("path").GetString());
        Assert.Equal(
            "explicit-workspace",
            root.GetProperty("workspace").GetProperty("selectedBy").GetString());
        Assert.Equal("apply", result.GetProperty("mode").GetString());
        Assert.Equal(RouteUpdateTestData.TargetId, target.GetProperty("requested").GetString());
        Assert.Equal("source-id", target.GetProperty("selectedBy").GetString());
        Assert.Equal(RouteUpdateTestData.TargetId, target.GetProperty("id").GetString());
        Assert.Equal(RouteUpdateTestData.TargetPath, target.GetProperty("path").GetString());
        Assert.Equal("ordinary-markdown", target.GetProperty("form").GetString());
        Assert.Empty(target.GetProperty("overwritePaths").EnumerateArray());
        Assert.Equal("Before", patch.GetProperty("description").GetProperty("before").GetString());
        Assert.Equal("Before", patch.GetProperty("description").GetProperty("expected").GetString());
        Assert.Equal("unchanged", patch.GetProperty("description").GetProperty("state").GetString());
        Assert.Equal(JsonValueKind.Null, result.GetProperty("template").ValueKind);
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal(
            [RouteUpdateTestData.ParentPath, RouteUpdateTestData.TargetPath],
            result.GetProperty("unchangedPaths").EnumerateArray().Select(item => item.GetString()));
        Assert.Equal("not-required", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal(JsonValueKind.Null, result.GetProperty("recovery").GetProperty("residualPath").ValueKind);
        Assert.Equal("verified", result.GetProperty("verification").GetString());
        Assert.Empty(result.GetProperty("findings").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
    }

    [Fact(DisplayName = "Route Update JSON projection retains complete failed result and Next facts"),
     Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void JsonProjectionRetainsCompleteFailedResultAndNextFacts()
    {
        const string residualPath = "/tmp/open-forge-route-update-recovery.zip";
        const string cause = "The exact applied target could not be verified.";
        var formation = RouteUpdateTestData.VerifiedNoOpFormation(
            findings:
            [
                RouteUpdateTestData.Finding(
                    RouteUpdateFindingCode.VerificationFailed,
                    cause: cause),
            ]) with
        {
            Effects =
            [
                RouteUpdateTestData.Effect() with
                {
                    Outcome = RouteUpdateEffectOutcome.VerificationFailed,
                    Residual = RouteUpdateEffectResidual.Retained,
                },
            ],
            UnchangedPaths = [],
            Recovery = new RouteUpdateRecovery
            {
                State = RouteUpdateRecoveryState.Retained,
                ResidualPath = residualPath,
            },
            Verification = RouteUpdateVerificationState.Failed,
        };

        var json = RouteUpdateJsonRenderer.Render(
            Presentation(RouteUpdateTestData.Result(formation), CliOutputFormat.Json));
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var result = root.GetProperty("result");
        var effect = Assert.Single(result.GetProperty("effects").EnumerateArray());
        var finding = Assert.Single(result.GetProperty("findings").EnumerateArray());
        var next = root.GetProperty("next");

        Assert.Equal(
            ["schemaVersion", "command", "status", "workspace", "result", "next"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal("failed", root.GetProperty("status").GetString());
        Assert.Equal(
            [
                "mode", "target", "patch", "template", "plan", "effects",
                "unchangedPaths", "recovery", "verification", "findings",
            ],
            result.EnumerateObject().Select(property => property.Name));
        Assert.Equal("verification-failed", effect.GetProperty("outcome").GetString());
        Assert.Equal("retained", effect.GetProperty("residual").GetString());
        Assert.Equal("retained", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal(
            residualPath,
            result.GetProperty("recovery").GetProperty("residualPath").GetString());
        Assert.Equal("failed", result.GetProperty("verification").GetString());
        Assert.Equal("route-update.verification-failed", finding.GetProperty("code").GetString());
        Assert.Equal("failed", finding.GetProperty("status").GetString());
        Assert.Equal(cause, finding.GetProperty("cause").GetString());
        Assert.Equal(
            ["command", "reason"],
            next.EnumerateObject().Select(property => property.Name));
        Assert.Equal("open-forge route update --verbose", next.GetProperty("command").GetString());
        Assert.Equal(
            "Report the failure and retry the same Route Update request with bounded diagnostics.",
            next.GetProperty("reason").GetString());
    }

    [Fact(DisplayName = "Route Update diagnostic renderer emits exact bounded direct finding facts"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void DiagnosticRendererEmitsExactBoundedDirectFindingFacts()
    {
        var result = ProtectedBodyResult();

        var diagnostic = RouteUpdateDiagnosticRenderer.Render(
            Presentation(result, CliOutputFormat.Human));

        Assert.Equal(
            "status=attention; mode=apply; target=memory/topic; completeness=complete; safety=safe; body=authored-body-protected; "
            + "effects=0; recovery=not-required; verification=verified; findings=1; "
            + "finding=route-update.template-body-protected:target=.agents/memory/topic.md:cause=The authored body is protected.",
            diagnostic);
    }

    [Fact(DisplayName = "Route Update help exposes exactly the accepted public surface"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void HelpExposesExactlyAcceptedPublicSurface()
    {
        var help = RouteUpdateHelpSections.Create();

        Assert.Equal(
        [
            "Syntax",
            "Target",
            "Metadata",
            "Template",
            "Write policy",
            "Examples",
            "Notes",
        ],
        help.Sections.Select(section => section.Heading));
        var text = string.Join(Environment.NewLine, help.Sections.Select(section => section.Body));
        Assert.Contains("open-forge route update <source-reference>", text, StringComparison.Ordinal);
        Assert.Contains("--description <text>", text, StringComparison.Ordinal);
        Assert.Contains("--responsibility <text>", text, StringComparison.Ordinal);
        Assert.Contains("--tag=<tag>", text, StringComparison.Ordinal);
        Assert.Contains("--template <template-reference>", text, StringComparison.Ordinal);
        Assert.Contains("--dry-run", text, StringComparison.Ordinal);
        Assert.Contains("exact empty responsibility removes", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("authored body", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("--force", text, StringComparison.Ordinal);
        Assert.DoesNotContain("--yes", text, StringComparison.Ordinal);
        Assert.DoesNotContain("--body", text, StringComparison.Ordinal);
        Assert.DoesNotContain("wizard", text, StringComparison.OrdinalIgnoreCase);
    }

    private static RouteUpdateResult ProtectedBodyResult()
    {
        var formation = RouteUpdateTestData.VerifiedNoOpFormation(
            RouteUpdateTestData.ProtectedTemplate(),
            RouteUpdateBodyState.AuthoredBodyProtected,
            findings:
            [
                RouteUpdateTestData.Finding(
                    RouteUpdateFindingCode.TemplateBodyProtected,
                    cause: "The authored body is protected."),
            ]);
        return RouteUpdateTestData.Result(formation);
    }

    private static RouteUpdateTemplate CopiedTemplate()
        => RouteUpdateTestData.ProtectedTemplate() with
        {
            Decision = RouteUpdateTemplateDecision.Copied,
        };

    private static RouteUpdatePatch ChangedPatch()
        => new()
        {
            Description = new RouteUpdateDescriptionPatch
            {
                Requested = true,
                Before = "Before",
                Expected = "After",
                State = RouteUpdatePatchState.Changed,
            },
            Responsibility = new RouteUpdateResponsibilityPatch
            {
                Requested = true,
                Operation = RouteUpdateResponsibilityOperation.Set,
                Before = "Before responsibility",
                Expected = "After responsibility",
                State = RouteUpdatePatchState.Changed,
            },
            Tags = new RouteUpdateTagsPatch
            {
                Requested = true,
                Before = ["Before"],
                Expected = ["After"],
                State = RouteUpdatePatchState.Changed,
            },
        };

    private static CliPresentationRequest<RouteUpdateResult> Presentation(
        RouteUpdateResult result,
        CliOutputFormat format,
        CliView view = CliView.Expanded)
        => new(
            result,
            new CliPresentation(
                format,
                view,
                CliVerbosity.Verbose));

    private static void AssertInOrder(
        string text,
        params string[] expected)
    {
        var offset = 0;
        foreach (var item in expected)
        {
            var next = text.IndexOf(item, offset, StringComparison.Ordinal);
            Assert.True(next >= offset, $"Expected '{item}' after offset {offset}.{Environment.NewLine}{text}");
            offset = next + item.Length;
        }
    }
}
