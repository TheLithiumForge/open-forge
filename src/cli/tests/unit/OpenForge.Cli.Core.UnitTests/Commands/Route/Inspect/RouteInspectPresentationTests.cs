using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Inspect;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Help;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect;

public sealed class RouteInspectPresentationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Inspect minimal output keeps the three question blocks and removes selection restatements")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void MinimalOutputKeepsQuestionBlocks()
    {
        var output = RenderText(RouteInspectPresentationTestData.CompleteResult(), CliDetail.Minimal);

        Assert.StartsWith(
            "root/item  .agents/root/item/_item.md\n\nWhere this source belongs\n",
            output,
            StringComparison.Ordinal);
        Assert.Contains("Route chain: root -> item", output, StringComparison.Ordinal);
        Assert.Contains("Parent: root", output, StringComparison.Ordinal);
        Assert.Contains("Direct children: 2 files, 1 entrypoint", output, StringComparison.Ordinal);
        Assert.Contains("Descendants: 4 files, 2 entrypoints", output, StringComparison.Ordinal);
        Assert.Contains("At task start or resume: yes", output, StringComparison.Ordinal);
        Assert.Contains("Read automatically when root is read", output, StringComparison.Ordinal);
        Assert.Contains("May be read again: yes", output, StringComparison.Ordinal);
        Assert.Contains("This file: 2 files, 24 B, about 5 tokens", output, StringComparison.Ordinal);
        Assert.Contains("Selecting this route adds: 1 file, 24 B, about 4 tokens", output, StringComparison.Ordinal);
        Assert.Contains("Overwrite file: .agents/root/item/_item.overwrite.md", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Axioms", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Selection:", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", output, StringComparison.Ordinal);
        Assert.DoesNotContain("1 files", output, StringComparison.Ordinal);
        Assert.DoesNotContain("·", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Next:", output, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Inspect standard output adds workspace axioms and tags without authored bodies")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void StandardOutputAddsSharedContext()
    {
        var output = RenderText(RouteInspectPresentationTestData.CompleteResult(), CliDetail.Standard);

        Assert.Contains("Workspace:", output, StringComparison.Ordinal);
        Assert.Contains("Axioms", output, StringComparison.Ordinal);
        Assert.Contains("Inherited rules from: loader, root", output, StringComparison.Ordinal);
        Assert.Contains("Local rules: no", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Selection:", output, StringComparison.Ordinal);
        Assert.DoesNotContain("custom item", output, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Inspect full output adds status reason selected closure selection and physical layers")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void FullOutputAddsSelectedFacts()
    {
        var output = RenderText(RouteInspectPresentationTestData.CompleteResult(), CliDetail.Full);

        Assert.Contains("Why: all applicable route facts are available", output, StringComparison.Ordinal);
        Assert.Contains("Selected context: 4 files, 64 B, about 12 tokens", output, StringComparison.Ordinal);
        Assert.Contains("Already in startup context: 3 files, 40 B, about 8 tokens", output, StringComparison.Ordinal);
        Assert.Contains("Selection: source ID; automatic ID; requested \"root/item\"", output, StringComparison.Ordinal);
        Assert.Contains("Physical layers", output, StringComparison.Ordinal);
        Assert.Contains("base .agents/root/item/_item.md", output, StringComparison.Ordinal);
        Assert.Contains("overwrite .agents/root/item/_item.overwrite.md", output, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Inspect rich presentation fixture uses a canonical entrypoint with applicable topology and LoadNow facts")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void RichPresentationFixtureUsesEntrypointFacts()
    {
        var result = RouteInspectPresentationTestData.CompleteResult();
        var identity = Assert.IsType<RouteInspectIdentity>(result.Identity);
        var profile = Assert.IsType<RouteInspectProfile>(result.Profile);
        var topology = Assert.IsType<RouteInspectTopology>(profile.Topology.Value);

        Assert.Equal(RouteInspectSourceKind.Entrypoint, identity.Kind);
        Assert.Equal(RouteInspectSourceForm.CanonicalEntrypoint, identity.Form);
        Assert.Equal(".agents/root/item/_item.md", identity.CanonicalWorkspaceRelativePath);
        Assert.Equal(RouteInspectFactState.Value, topology.Counts.State);
        Assert.NotNull(topology.Counts.Value);
        Assert.Equal(RouteInspectFactState.Value, profile.Measurements.LoadNowDescendants.State);
        Assert.Equal(
            0,
            Assert.IsType<RouteInspectMeasurement>(profile.Measurements.LoadNowDescendants.Value).PhysicalFileCount);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Inspect help owns exact grammar flags statuses examples related commands and JSON view relationship")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void HelpSectionsDocumentAcceptedPresentationSurface()
    {
        var help = RouteInspectHelpSections.CreateInspect();
        var text = string.Join(Environment.NewLine, help.Sections.Select(section => section.Body));

        Assert.Contains("open-forge route inspect <source-reference>", text, StringComparison.Ordinal);
        Assert.Contains("source-id", text, StringComparison.Ordinal);
        Assert.Contains(".agents/", text, StringComparison.Ordinal);
        Assert.Contains("./.agents/", text, StringComparison.Ordinal);
        Assert.Contains("--workspace <path>", text, StringComparison.Ordinal);
        Assert.Contains("--format", text, StringComparison.Ordinal);
        Assert.Contains("--detail=<minimal|standard|full|debug>", text, StringComparison.Ordinal);
        Assert.Contains("--detail-filter", text, StringComparison.Ordinal);
        Assert.Contains("completed", text, StringComparison.Ordinal);
        Assert.Contains("completed-with-warnings", text, StringComparison.Ordinal);
        Assert.Contains("incomplete", text, StringComparison.Ordinal);
        Assert.Contains("invalid-input", text, StringComparison.Ordinal);
        Assert.Contains("blocked", text, StringComparison.Ordinal);
        Assert.Contains("failed", text, StringComparison.Ordinal);
        Assert.Contains("cancelled", text, StringComparison.Ordinal);
        Assert.Contains("exit 0", text, StringComparison.Ordinal);
        Assert.Contains("exit 130", text, StringComparison.Ordinal);
        Assert.Contains("open-forge route inspect memory/working", text, StringComparison.Ordinal);
        Assert.Contains("route inspect --help", text, StringComparison.Ordinal);
        Assert.Contains("route inspect --version", text, StringComparison.Ordinal);
        Assert.Contains("--detail", text, StringComparison.Ordinal);
        Assert.Contains("JSON", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("route list", text, StringComparison.Ordinal);
        Assert.Contains("context — use", text, StringComparison.Ordinal);
        Assert.Contains("open-forge context [source-reference...]", text, StringComparison.Ordinal);
        Assert.DoesNotContain("context — unavailable", text, StringComparison.Ordinal);
        Assert.Contains("doctor — diagnose workspace conditions without changing them.", text, StringComparison.Ordinal);
        Assert.DoesNotContain("doctor — unavailable", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Route Inspect output uses the native finding shape without legacy status or selection rows")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    [InlineData("observation")]
    [InlineData("condition")]
    public void OutputUsesNativeFindingShape(string scenario)
    {
        var result = scenario == "observation"
            ? RouteInspectPresentationTestData.ExactPathAttentionResult()
            : RouteInspectPresentationTestData.InvalidResult();
        var output = RenderText(result, CliDetail.Standard);

        Assert.DoesNotContain("Status:", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Selected by:", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Route:", output, StringComparison.Ordinal);
        Assert.DoesNotContain("The automatic source ID is not unique.", output, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Route Inspect KeepInMind explanations name the actual typed event")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    [InlineData((int)RouteInspectAutomaticReadingEvent.ExposingParentRead, "its exposing parent is read")]
    [InlineData((int)RouteInspectAutomaticReadingEvent.LaterReview, "a later review point is reached while its scope is active")]
    public void KeepInMindExplanationUsesActualEvent(int eventValue, string expected)
    {
        var reading = new RouteInspectAutomaticReading(
            RouteInspectAutomaticReadingKind.EntrypointKeepInMind,
            "parent",
            [(RouteInspectAutomaticReadingEvent)eventValue]);

        Assert.Equal(expected, RouteInspectWording.AutomaticExplanation(reading));
    }

    private static string RenderText(RouteInspectResult result, CliDetail detail)
        => CliRenderingStage.Render(
            RouteInspectPresentationTestData.Presentation(result, detail),
            RouteInspectPresentation.Rendering).PrimaryContent;
}
