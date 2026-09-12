using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Init;

public sealed class RouteInitPresentationTests
{
    [Fact(DisplayName = "Route Init complete no-op human result retains exact completion facts without prompting"),
     Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void CompleteNoOpHumanResultRetainsExactCompletionFacts()
    {
        var result = new RouteInitResultBuilder().Build(
            RouteInitRedTestData.Formation(effects: [], entrypoints: [], verification: RouteInitVerificationState.Verified));
        var text = RouteInitHumanRenderer.Render(new CliPresentationRequest<RouteInitResult>(
            result, new CliPresentation(CliOutputFormat.Human, CliView.Compact, CliVerbosity.Normal)));

        Assert.Contains("The route is initialized.", text, StringComparison.Ordinal);
        Assert.Contains("Status: complete", text, StringComparison.Ordinal);
        Assert.Contains("No files changed.", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Next:", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Apply this", text, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Route Init invalid and incomplete human results retain exact status streams and guidance"),
     InlineData((int)RouteInitFindingCode.InvalidTarget, "invalid", "Correct the named Route Init input"),
     InlineData((int)RouteInitFindingCode.MetadataIncomplete, "incomplete", "inspect the unavailable route, metadata, projection, lifecycle, or recovery facts"),
     Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void InvalidAndIncompleteHumanResultsRetainStatusAndGuidance(
        int findingValue,
        string expectedStatus,
        string expectedNext)
    {
        var finding = RouteInitRedTestData.Finding((RouteInitFindingCode)findingValue);
        var result = new RouteInitResultBuilder().Build(RouteInitRedTestData.Formation(findings: [finding]));
        var presentation = CliPresentationStage.Create(
            result,
            new CliPresentation(CliOutputFormat.Human, CliView.Expanded, CliVerbosity.Normal));
        var rendered = CliRenderingStage.Render(
            presentation,
            new CliRendererSet<RouteInitResult>(RouteInitHumanRenderer.Render, RouteInitJsonRenderer.Render),
            RouteInitDiagnosticRenderer.Render);

        Assert.Contains($"Status: {expectedStatus}", rendered.PrimaryContent, StringComparison.Ordinal);
        Assert.Contains(expectedNext, rendered.PrimaryContent, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(
            expectedStatus == "invalid" ? CliOutputTarget.StandardError : CliOutputTarget.StandardOutput,
            rendered.PrimaryTarget);
        Assert.Equal(expectedStatus == "invalid" ? 4 : 3, CliStatusDefinitions.Read(result.Status).Disposition.ExitCode);
    }

    [Fact(DisplayName = "Route Init dry-run JSON preserves ordered schema and literal preview facts"),
     Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void DryRunJsonPreservesOrderedSchemaAndPreviewFacts()
    {
        var formation = RouteInitRedTestData.Formation(mode: RouteInitMode.DryRun,
            recovery: new RouteInitRecovery(RouteInitRecoveryState.NotCreated, null), verification: RouteInitVerificationState.NotRequested);
        var presentation = new CliPresentationRequest<RouteInitResult>(RouteInitRedTestData.Result(formation),
            new CliPresentation(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal));
        using var document = JsonDocument.Parse(RouteInitJsonRenderer.Render(presentation));
        var root = document.RootElement;
        var result = root.GetProperty("result");

        Assert.Equal(["schemaVersion", "command", "status", "workspace", "result", "next"], root.EnumerateObject().Select(property => property.Name));
        Assert.Equal(
            ["mode", "scaffold", "target", "plan", "framework", "entrypoints", "effects", "unchangedPaths", "lifecycle", "recovery", "verification", "findings"],
            result.EnumerateObject().Select(property => property.Name));
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.Equal("generic", result.GetProperty("scaffold").GetString());
        Assert.Equal("complete", result.GetProperty("plan").GetProperty("completeness").GetString());
        Assert.Equal("safe", result.GetProperty("plan").GetProperty("safety").GetString());
        Assert.Equal(JsonValueKind.Null, result.GetProperty("framework").ValueKind);
        var metadata = Assert.Single(result.GetProperty("entrypoints").EnumerateArray()).GetProperty("metadata");
        Assert.Equal("Draft route for memory/project-alpha/documents", metadata.GetProperty("description").GetString());
        Assert.Equal(["NeedsAuthoring"], metadata.GetProperty("tags").EnumerateArray().Select(tag => tag.GetString()));
        var effect = Assert.Single(result.GetProperty("effects").EnumerateArray());
        Assert.Equal("planned", effect.GetProperty("outcome").GetString());
        Assert.Equal(JsonValueKind.Null, effect.GetProperty("change").GetProperty("before").ValueKind);
        Assert.Equal("draft-entrypoint-bytes", effect.GetProperty("change").GetProperty("expected").GetString());
        Assert.Equal("not-created", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal("not-requested", result.GetProperty("verification").GetString());
        Assert.Empty(result.GetProperty("findings").EnumerateArray());
    }

    [Fact(DisplayName = "Route Init JSON projection preserves the complete typed envelope graph and nullable fields"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void JsonProjectionPreservesCompleteTypedEnvelopeGraphAndNullableFields()
    {
        var result = RouteInitRedTestData.Result();
        var projected = RouteInitJsonProjection.Create(result);

        Assert.Equal(1, projected.SchemaVersion);
        Assert.Equal("route init", projected.Command);
        Assert.Equal("complete", projected.Status);
        Assert.NotNull(projected.Workspace);
        Assert.Equal("apply", projected.Result.Mode);
        Assert.Equal("generic", projected.Result.Scaffold);
        Assert.Equal("memory/project-alpha/documents", projected.Result.Target.Requested);
        Assert.Equal("memory/project-alpha/documents", projected.Result.Target.Id);
        Assert.Equal("complete", projected.Result.Plan.Completeness);
        Assert.Equal("safe", projected.Result.Plan.Safety);
        Assert.Null(projected.Result.Framework);
        Assert.NotNull(projected.Result.Entrypoints);
        Assert.NotNull(projected.Result.Effects);
        Assert.NotNull(projected.Result.UnchangedPaths);
        Assert.NotNull(projected.Result.Lifecycle);
        Assert.NotNull(projected.Result.Recovery);
        Assert.Equal("verified", projected.Result.Verification);
        Assert.NotNull(projected.Result.Findings);
        Assert.Null(projected.Next);
    }

    [Fact(DisplayName = "Route Init JSON renderer emits one structured result without human text"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void JsonRendererEmitsOneStructuredResultWithoutHumanText()
    {
        var presentation = new CliPresentationRequest<RouteInitResult>(
            RouteInitRedTestData.Result(),
            new CliPresentation(
                CliOutputFormat.Json,
                CliView.Expanded,
                CliVerbosity.Normal));
        var json = RouteInitJsonRenderer.Render(presentation);

        using var document = JsonDocument.Parse(json);
        Assert.Equal("route init", document.RootElement.GetProperty("command").GetString());
        Assert.DoesNotContain("The route", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Workspace:", json, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route Init compact human renderer retains workspace target mode status and next guidance"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void CompactHumanRendererRetainsIdentityModeStatusAndNextGuidance()
    {
        var finding = RouteInitRedTestData.Finding(RouteInitFindingCode.NeedsAuthoring);
        var result = RouteInitRedTestData.Result(
            RouteInitRedTestData.Formation(
                mode: RouteInitMode.DryRun,
                findings: [finding]),
            [finding],
            CliSemanticStatus.Attention,
            new CliNextAction(
                "open-forge route update",
                "Author each reported NeedsAuthoring entrypoint before relying on its description or tags."));
        var presentation = new CliPresentationRequest<RouteInitResult>(
            result,
            new CliPresentation(
                CliOutputFormat.Human,
                CliView.Compact,
                CliVerbosity.Normal));

        var text = RouteInitHumanRenderer.Render(presentation);

        Assert.Contains("Workspace:", text, StringComparison.Ordinal);
        Assert.Contains("Target:", text, StringComparison.Ordinal);
        Assert.Contains("dry-run", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Status: requires attention", text, StringComparison.Ordinal);
        Assert.Contains("Next: author each NeedsAuthoring entrypoint", text, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route Init diagnostic renderer stays bounded and names the direct finding"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void DiagnosticRendererStaysBoundedAndNamesDirectFinding()
    {
        var finding = RouteInitRedTestData.Finding(
            RouteInitFindingCode.GeneratedRegionUnsafe,
            cause: "The generated Entries boundary is unsafe.");
        var result = RouteInitRedTestData.Result(
            RouteInitRedTestData.Formation(findings: [finding]),
            [finding],
            CliSemanticStatus.Blocked,
            new CliNextAction(
                "open-forge doctor",
                "Inspect the blocked workspace, route, identity, lifecycle, generated-region, or recovery boundary before rerunning Route Init."));
        var presentation = new CliPresentationRequest<RouteInitResult>(
            result,
            new CliPresentation(
                CliOutputFormat.Human,
                CliView.Expanded,
                CliVerbosity.Verbose));

        var diagnostic = RouteInitDiagnosticRenderer.Render(presentation);

        var text = Assert.IsType<string>(diagnostic);
        Assert.InRange(text.Length, 1, 4096);
        Assert.Contains("generated", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("unsafe", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Route Init help formation exposes only accepted target and option spellings"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void HelpFormationExposesAcceptedTargetAndOptionSpellings()
    {
        var help = RouteInitHelpSections.Create();
        var text = string.Join(Environment.NewLine, help.Sections.Select(section => section.Body));

        Assert.Contains("route init", text, StringComparison.Ordinal);
        Assert.Contains(
            "open-forge route init <route-target> [--framework] [--description <text>] [--responsibility <text>] "
            + "[--tag=<tag>]... [--dry-run] [global flags]", text, StringComparison.Ordinal);
        Assert.Contains("<route-target>", text, StringComparison.Ordinal);
        Assert.Contains("--framework", text, StringComparison.Ordinal);
        Assert.Contains("--description", text, StringComparison.Ordinal);
        Assert.Contains("--responsibility", text, StringComparison.Ordinal);
        Assert.Contains("--tag=", text, StringComparison.Ordinal);
        Assert.Contains("--dry-run", text, StringComparison.Ordinal);
        Assert.DoesNotContain("--automatic", text, StringComparison.Ordinal);
        Assert.DoesNotContain("--scope", text, StringComparison.Ordinal);
    }
}
