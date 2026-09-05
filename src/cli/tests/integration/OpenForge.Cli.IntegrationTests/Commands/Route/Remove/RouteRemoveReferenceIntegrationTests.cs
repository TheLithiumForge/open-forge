using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

public sealed class RouteRemoveReferenceIntegrationTests
{
    [Fact(DisplayName = "Route Remove records every external detachment with an independent visible-label oracle"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task ExternalDetachmentRetainsLocationAndSurroundingProse()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-reference-detachment");
        workspace.WriteText(
            "outside.md",
            "Before [Readable guide](.agents/guidance/old%20guide.md#part), after.\n");
        var before = workspace.ReadText("outside.md");
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--dry-run", "--json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        using var document = JsonDocument.Parse(output.ToString());
        var detachments = document.RootElement
            .GetProperty("result")
            .GetProperty("references")
            .GetProperty("detachments")
            .EnumerateArray()
            .ToArray();
        var detachment = Assert.Single(
            detachments,
            candidate => candidate.GetProperty("sourcePath").GetString() == "outside.md");
        Assert.Equal("Readable guide", detachment.GetProperty("visibleLabel").GetString());
        Assert.Equal(
            "Before [Readable guide](.agents/guidance/old%20guide.md#part), after.",
            detachment.GetProperty("before").GetString());
        Assert.Equal("Before Readable guide, after.", detachment.GetProperty("expected").GetString());
        Assert.Equal(
            ".agents/guidance/old%20guide.md#part",
            detachment.GetProperty("originalDestination").GetString());
        Assert.Equal(before, workspace.ReadText("outside.md"));
    }

    [Fact(DisplayName = "Route Remove leaves external URLs and unrelated targets untouched"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task UnrelatedReferencesRemainByteIdentical()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-reference-preservation");
        const string outside = "Before https://example.invalid/a?b=c#part and [Topics](.agents/guidance/topics/_topics.md) after.\n";
        workspace.WriteText("outside.md", outside);
        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(outside, workspace.ReadText("outside.md"));
        Assert.Equal(before[".agents/open-forge.lifecycle.json"],
            workspace.SnapshotHashes()[".agents/open-forge.lifecycle.json"]);
    }

    [Fact(DisplayName = "Route Remove blocks an unsupported incoming transformation before any write"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task UnsupportedIncomingTransformationIsWriteFree()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-reference-unsafe");
        workspace.WriteText(
            "outside.md",
            "Before [Readable guide][target], after.\n\n[target]: .agents/guidance/old%20guide.md\n");
        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId],
            output,
            error);

        Assert.Equal(5, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, completion.Status);
        Assert.Contains("route-remove.reference-unsafe", error.ToString(), StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
        workspace.AssertNoLockInfrastructure();
    }
}
