using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedLibrarySyncProcessTests
{
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task UnchangedInventoryIsAnEffectFreeNoOp()
    {
        using var workspace = new PublishedLibraryWorkspace();
        workspace.Source();
        workspace.Link();
        workspace.Record(PublishedLibraryWorkspace.ReviewPath);
        using var document = PublishedLibraryWorkspace.Result(
            await workspace.ReadOnlyAsync(PublishedExecutableTarget.Discover(), "library", "sync", "team-knowledge", "--format=json"), "completed");
        var result = document.RootElement.GetProperty("data");
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        workspace.AssertSource();
        workspace.AssertNoInfrastructure();
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task DryRunThenApplyReconcilesOneAdditionAndRetirement()
    {
        using var workspace = new PublishedLibraryWorkspace();
        workspace.Source("future/new.md");
        workspace.MappedLink("docs/old.md", "../shared/team-knowledge/old.md");
        workspace.RecordAt("docs", "old.md");
        workspace.GrantDocs();
        var target = PublishedExecutableTarget.Discover();
        using var preview = PublishedLibraryWorkspace.Result(
            await workspace.ReadOnlyAsync(target, "library", "sync", "team-knowledge", "--dry-run", "--format=json"), "completed");
        workspace.AssertNoInfrastructure();
        try
        {
            using var applied = PublishedLibraryWorkspace.Result(
                await workspace.RunAsync(target, "library", "sync", "team-knowledge", "--automatic", "--format=json"), "completed");
            Assert.Equal(EffectShapes(preview.RootElement), EffectShapes(applied.RootElement));
            Assert.Null(new FileInfo(workspace.Combine("docs/old.md")).LinkTarget);
            Assert.Equal("../../shared/team-knowledge/future/new.md", new FileInfo(workspace.Combine("docs/future/new.md")).LinkTarget);
            using var record = JsonDocument.Parse(File.ReadAllText(workspace.Combine(PublishedLibraryWorkspace.RecordPath)));
            Assert.Equal("future/new.md",
                Assert.Single(Assert.Single(record.RootElement.GetProperty("libraries").EnumerateArray()).GetProperty("paths").EnumerateArray()).GetString());
            Assert.Equal(PublishedLibraryWorkspace.SourceBody, File.ReadAllText(workspace.Combine("shared/team-knowledge/future/new.md")));
            workspace.AssertAppliedInfrastructure();
        }
        finally
        {
            if (new FileInfo(workspace.Combine("docs/future/new.md")).LinkTarget == "../../shared/team-knowledge/future/new.md")
            {
                File.Delete(workspace.Combine("docs/future/new.md"));
            }
            var parent = workspace.Combine("docs/future");
            if (Directory.Exists(parent) && !Directory.EnumerateFileSystemEntries(parent).Any())
            {
                Directory.Delete(parent);
            }
        }
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task ChangedOccupantWithIndependentWorkStillRequiresConfirmation()
    {
        using var workspace = new PublishedLibraryWorkspace();
        workspace.Source();
        workspace.Source(".agents/directives/new.md");
        workspace.Record(PublishedLibraryWorkspace.ReviewPath);
        workspace.Write(PublishedLibraryWorkspace.ReviewPath, "Changed consumer occupant.");
        using var document = PublishedLibraryWorkspace.Result(
            await workspace.ReadOnlyAsync(PublishedExecutableTarget.Discover(), "library", "sync", "team-knowledge", "--format=json"), "invalid-input", 4);
        Assert.Contains(document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "library-sync.confirmation-required");
        Assert.False(File.Exists(workspace.Combine(".agents/directives/new.md")));
        workspace.AssertSource();
        workspace.AssertNoInfrastructure();
    }

    private static string[] EffectShapes(JsonElement document)
        => document.GetProperty("data").GetProperty("effects").EnumerateArray()
            .Select(effect => string.Join(
                "|",
                effect.GetProperty("path").GetString(),
                effect.GetProperty("action").GetString(),
                effect.TryGetProperty("target", out var target) ? target.GetString() : null))
            .ToArray();
}
