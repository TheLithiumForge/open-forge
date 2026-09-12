using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect;

public sealed class RouteInspectPresentationTests
{
    [Fact(DisplayName = "Route Inspect compact output retains identity reading route topology measurements overwrite and status")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void CompactOutputRetainsRequiredFacts()
    {
        var output = RouteInspectHumanRenderer.Render(
            RouteInspectPresentationTestData.Presentation(
                RouteInspectPresentationTestData.CompleteResult(),
                CliView.Compact));

        Assert.StartsWith($"Route: root/item{Environment.NewLine}Status: complete", output, StringComparison.Ordinal);
        Assert.Contains("Workspace:", output, StringComparison.Ordinal);
        Assert.Contains("Selected by:", output, StringComparison.Ordinal);
        Assert.Contains("Route: root/item", output, StringComparison.Ordinal);
        Assert.Contains("Path:", output, StringComparison.Ordinal);
        Assert.Contains("Source state:", output, StringComparison.Ordinal);
        Assert.Contains("Route state:", output, StringComparison.Ordinal);
        Assert.Contains("Route chain:", output, StringComparison.Ordinal);
        Assert.Contains("Parent:", output, StringComparison.Ordinal);
        Assert.Contains("Depth: 2", output, StringComparison.Ordinal);
        Assert.Contains("Direct children:", output, StringComparison.Ordinal);
        Assert.Contains("Descendants:", output, StringComparison.Ordinal);
        Assert.Contains("Read at task start or resume: yes", output, StringComparison.Ordinal);
        Assert.Contains("Read automatically when:", output, StringComparison.Ordinal);
        Assert.Contains("May be read again: yes", output, StringComparison.Ordinal);
        Assert.Contains("Own source:", output, StringComparison.Ordinal);
        Assert.Contains("Selecting this route adds:", output, StringComparison.Ordinal);
        Assert.Contains("Automatically read below it through #LoadNow:", output, StringComparison.Ordinal);
        Assert.Contains("Overwrite:", output, StringComparison.Ordinal);
        Assert.Contains("Status: complete", output, StringComparison.Ordinal);
        Assert.Contains("Completeness: complete", output, StringComparison.Ordinal);
        Assert.Contains("Safety: safe", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Next:", output, StringComparison.Ordinal);
        Assert.DoesNotContain("ParentLoadNow", output, StringComparison.Ordinal);
        Assert.DoesNotContain("RouteSelected", output, StringComparison.Ordinal);
        Assert.DoesNotContain("custom item", output, StringComparison.Ordinal);
        Assert.DoesNotContain("scope count", output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("health", output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("recommend", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Route Inspect expanded output adds plain explanations and Axioms provenance without authored bodies")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ExpandedOutputAddsExplanationsAndProvenance()
    {
        var output = RouteInspectHumanRenderer.Render(
            RouteInspectPresentationTestData.Presentation(
                RouteInspectPresentationTestData.CompleteResult(),
                CliView.Expanded));

        Assert.Contains("Axioms", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("loader", output, StringComparison.Ordinal);
        Assert.Contains("Why", output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ParentLoadNow", output, StringComparison.Ordinal);
        Assert.DoesNotContain("RouteSelected", output, StringComparison.Ordinal);
        Assert.DoesNotContain("custom item", output, StringComparison.Ordinal);
        Assert.DoesNotContain("scope count", output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("health grade", output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("recommendation", output, StringComparison.OrdinalIgnoreCase);
    }

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
        Assert.Contains("--json", text, StringComparison.Ordinal);
        Assert.Contains("--view=<compact|expanded>", text, StringComparison.Ordinal);
        Assert.Contains("--verbose", text, StringComparison.Ordinal);
        Assert.Contains("complete", text, StringComparison.Ordinal);
        Assert.Contains("attention", text, StringComparison.Ordinal);
        Assert.Contains("incomplete", text, StringComparison.Ordinal);
        Assert.Contains("invalid", text, StringComparison.Ordinal);
        Assert.Contains("blocked", text, StringComparison.Ordinal);
        Assert.Contains("failed", text, StringComparison.Ordinal);
        Assert.Contains("interrupted", text, StringComparison.Ordinal);
        Assert.Contains("exit 0", text, StringComparison.Ordinal);
        Assert.Contains("exit 130", text, StringComparison.Ordinal);
        Assert.Contains("open-forge route inspect memory/working/checkpoints", text, StringComparison.Ordinal);
        Assert.Contains("route inspect --help", text, StringComparison.Ordinal);
        Assert.Contains("route inspect --version", text, StringComparison.Ordinal);
        Assert.Contains("--view", text, StringComparison.Ordinal);
        Assert.Contains("JSON", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("route list", text, StringComparison.Ordinal);
        Assert.Contains("context — use", text, StringComparison.Ordinal);
        Assert.Contains("open-forge context [source-reference...]", text, StringComparison.Ordinal);
        Assert.DoesNotContain("context — unavailable", text, StringComparison.Ordinal);
        Assert.Contains("doctor — diagnose workspace conditions without changing them.", text, StringComparison.Ordinal);
        Assert.DoesNotContain("doctor — unavailable", text, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Route Inspect expanded output renders each observation or condition message once")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    [InlineData("observation", "The automatic source ID is not unique.")]
    [InlineData("condition", "route inspect requires one known source reference.")]
    public void ExpandedOutputDoesNotDuplicateMessages(string scenario, string expectedMessage)
    {
        var result = scenario == "observation"
            ? RouteInspectPresentationTestData.ExactPathAttentionResult()
            : RouteInspectPresentationTestData.InvalidResult();
        var output = RouteInspectHumanRenderer.Render(
            RouteInspectPresentationTestData.Presentation(result, CliView.Expanded));

        Assert.Equal(1, Count(output, expectedMessage));
    }

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

        Assert.Equal(expected, RouteInspectHumanAutomaticReading.Explanation(reading));
    }

    private static int Count(string value, string expected)
    {
        var count = 0;
        var index = 0;
        while ((index = value.IndexOf(expected, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += expected.Length;
        }

        return count;
    }
}
