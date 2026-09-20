using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Result;
using OpenForge.Cli.Core.Presentation.Route.Init;
using OpenForge.Cli.Core.Presentation.Route.Init.Shared.Help;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Init;

public sealed class RouteInitPresentationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Init complete no-op uses the frozen no-op headline without a status row"),
     Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void CompleteNoOpUsesFrozenNoOpHeadline()
    {
        var result = new RouteInitResultBuilder().Build(
            RouteInitRedTestData.Formation(effects: [], entrypoints: [], verification: RouteInitVerificationState.Verified));
        var output = Render(result, CliFormat.Text, CliDetail.Minimal);

        Assert.Contains("memory/project-alpha/documents is already initialized. Nothing to do.", output.PrimaryContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", output.PrimaryContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Next:", output.PrimaryContent, StringComparison.Ordinal);
        Assert.Equal(CliOutputTarget.StandardOutput, output.PrimaryTarget);
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Route Init invalid and incomplete results use their status streams and next guidance"),
     InlineData((int)RouteInitFindingCode.InvalidTarget, "Cannot initialize memory/project-alpha/documents: not a route ID or an entrypoint path under .agents.", 4, (int)CliOutputTarget.StandardError),
     InlineData((int)RouteInitFindingCode.MetadataIncomplete, "The route could not be initialized:", 3, (int)CliOutputTarget.StandardOutput),
     Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void InvalidAndIncompleteResultsUseFrozenStreams(
        int findingValue,
        string expectedHeadline,
        int expectedExit,
        int expectedTarget)
    {
        var finding = RouteInitRedTestData.Finding((RouteInitFindingCode)findingValue);
        var result = new RouteInitResultBuilder().Build(RouteInitRedTestData.Formation(findings: [finding]));
        var output = Render(result, CliFormat.Text, CliDetail.Standard);

        Assert.Contains(expectedHeadline, output.PrimaryContent, StringComparison.Ordinal);
        Assert.Contains("Next:", output.PrimaryContent, StringComparison.Ordinal);
        Assert.Equal((CliOutputTarget)expectedTarget, output.PrimaryTarget);
        Assert.Equal(expectedExit, CliStatusDefinitions.Read(result.Status).Disposition.ExitCode);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Init JSON exposes the level-specific native data shape"),
     Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void JsonExposesNativeDataShape()
    {
        var formation = RouteInitRedTestData.Formation(
            mode: RouteInitMode.DryRun,
            recovery: new RouteInitRecovery(RouteInitRecoveryState.NotCreated, null),
            verification: RouteInitVerificationState.NotRequested);
        var result = new RouteInitResultBuilder().Build(formation);
        using var document = JsonDocument.Parse(Render(result, CliFormat.Json, CliDetail.Standard).PrimaryContent);
        var root = document.RootElement;
        var data = root.GetProperty("data");

        Assert.Equal(
            ["schemaVersion", "command", "status", "detail", "filter", "workspace", "summary", "findings", "effects", "counts", "limitations", "data", "recovery", "next"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal(
            ["mode", "target", "scaffold", "entrypoints", "listedIn", "metadata"],
            data.EnumerateObject().Select(property => property.Name));
        Assert.Equal("dry-run", data.GetProperty("mode").GetString());
        Assert.Equal("generic", data.GetProperty("scaffold").GetString());
        var entrypoint = Assert.Single(data.GetProperty("entrypoints").EnumerateArray());
        Assert.Equal("planned", entrypoint.GetProperty("outcome").GetString());
        Assert.True(entrypoint.GetProperty("needsAuthoring").GetBoolean());
        Assert.Equal(JsonValueKind.Object, data.GetProperty("metadata").ValueKind);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Init full text preserves authored entrypoint bytes and never prints escaped bodies"),
     Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void FullTextPreservesAuthoredBytes()
    {
        var formation = RouteInitRedTestData.Formation(
            effects:
            [
                new(
                    ".agents/memory/project-alpha/documents/_documents.md",
                    RouteInitEffectKind.Entrypoint,
                    RouteInitEffectAction.Create,
                    null,
                    new(null, "---\nvalue: <literal>\n---\n"),
                    RouteInitEffectOutcome.Verified,
                    RouteInitEffectResidual.None),
            ],
            entrypoints:
            [
                new(
                    "memory/project-alpha/documents",
                    ".agents/memory/project-alpha/documents/_documents.md",
                    RouteInitEntrypointForm.Canonical,
                    RouteInitEntrypointCurrent.Missing,
                    RouteInitEntrypointOwnership.User,
                    new(
                        "Description",
                        RouteInitDescriptionSource.Explicit,
                        null,
                        RouteInitResponsibilitySource.DefaultOmitted,
                        ["Docs"],
                        RouteInitTagsSource.Explicit),
                    null,
                    RouteInitEntrypointOutcome.Created),
            ]);
        var result = new RouteInitResultBuilder().Build(formation);
        var text = Render(result, CliFormat.Text, CliDetail.Full).PrimaryContent;

        Assert.Contains("--- .agents/memory/project-alpha/documents/_documents.md (new file) ---", text, StringComparison.Ordinal);
        Assert.Contains("value: <literal>", text, StringComparison.Ordinal);
        Assert.DoesNotContain(@"\nvalue:", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Init needs-authoring is completed and remains an info finding with an advisory line"),
     Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void NeedsAuthoringIsCompletedInfoWithAdvisory()
    {
        var finding = RouteInitRedTestData.Finding(RouteInitFindingCode.NeedsAuthoring);
        var result = new RouteInitResultBuilder().Build(
            RouteInitRedTestData.Formation(findings: [finding]));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var text = Render(result, CliFormat.Text, CliDetail.Minimal).PrimaryContent;
        Assert.Contains("Its description and tags are placeholders. Edit them before relying on this route.", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Next:", text, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(Render(result, CliFormat.Json, CliDetail.Full).PrimaryContent);
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
        Assert.Equal("info", document.RootElement.GetProperty("findings")[0].GetProperty("severity").GetString());
        Assert.Equal("route-init.needs-authoring", document.RootElement.GetProperty("findings")[0].GetProperty("code").GetString());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Init debug publication includes bounded diagnostics without changing primary output"),
     Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void DebugPublicationIncludesBoundedDiagnostics()
    {
        var finding = RouteInitRedTestData.Finding(
            RouteInitFindingCode.GeneratedRegionUnsafe,
            cause: "The generated Entries boundary is unsafe.");
        var result = new RouteInitResultBuilder().Build(
            RouteInitRedTestData.Formation(findings: [finding]));
        var output = Render(result, CliFormat.Text, CliDetail.Debug);

        Assert.NotNull(output.DiagnosticContent);
        Assert.Contains("generated", output.DiagnosticContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("unsafe", output.DiagnosticContent, StringComparison.OrdinalIgnoreCase);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Init help formation exposes only accepted target and option spellings"),
     Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void HelpFormationExposesAcceptedTargetAndOptionSpellings()
    {
        var help = RouteInitHelpSections.Create();
        var text = string.Join(Environment.NewLine, help.Sections.Select(section => section.Body));

        Assert.Contains("route init", text, StringComparison.Ordinal);
        Assert.Contains(
            "open-forge route init <route-target> [--framework] [--description <text>] [--responsibility <text>] "
            + "[--tag <tag>]... [--dry-run] [global options]", text, StringComparison.Ordinal);
        Assert.Contains("<route-target>", text, StringComparison.Ordinal);
        Assert.Contains("--framework", text, StringComparison.Ordinal);
        Assert.Contains("--description", text, StringComparison.Ordinal);
        Assert.Contains("--responsibility", text, StringComparison.Ordinal);
        Assert.Contains("--tag ", text, StringComparison.Ordinal);
        Assert.Contains("--dry-run", text, StringComparison.Ordinal);
        Assert.DoesNotContain("--automatic", text, StringComparison.Ordinal);
        Assert.DoesNotContain("--scope", text, StringComparison.Ordinal);
    }

    private static CliRenderedOutput Render(
        RouteInitResult result,
        CliFormat format,
        CliDetail detail)
        => CliRenderingStage.Render(
            new CliPresentationRequest<RouteInitResult>(
                result,
                new CliPresentation(format, detail, null)),
            RouteInitPresentation.Rendering);
}
