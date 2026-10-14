using System.Text.Json;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

public sealed class ExtensionInstallIncrementalToolkitIntegrationTests
{
    [Theory(DisplayName = "Toolkit installation applies its preview after Planning and preserves existing package content")]
    [InlineData("full-core")]
    [InlineData("essentials")]
    [Trait("Boundary", "Host"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task ToolkitAfterPlanningAppliesReviewedEffects(string preset)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create($"extension-install-planning-toolkit-{preset}");
        Assert.Equal(0, (await workspace.RunAsync(["install", "--preset", preset, "--automatic"])).ExitCode);
        Assert.Equal(0, (await workspace.RunAsync(["extension", "install", "planning", "--automatic"])).ExitCode);
        const string host = ".agents/skills/use-workflow/references/_references.md";
        var authored = workspace.ReadText(host).ReplaceLineEndings("\r\n");
        workspace.ReplaceText(host, authored);
        var before = workspace.Snapshot();

        var preview = await workspace.RunAsync(["extension", "install", "development-toolkit", "--dry-run", "--format", "json"]);

        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(before, workspace.Snapshot());
        using var previewDocument = JsonDocument.Parse(preview.StandardOutput);
        var reviewedPaths = previewDocument.RootElement.GetProperty("effects").EnumerateArray()
            .Select(effect => effect.GetProperty("path").GetString()).ToHashSet(StringComparer.Ordinal);

        var applied = await workspace.RunAsync(["extension", "install", "development-toolkit", "--automatic", "--format", "json"]);

        Assert.True(applied.ExitCode == 0, applied.StandardOutput);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Contains(host, reviewedPaths);
        using var appliedDocument = JsonDocument.Parse(applied.StandardOutput);
        Assert.All(appliedDocument.RootElement.GetProperty("effects").EnumerateArray(), effect =>
        {
            Assert.Contains(effect.GetProperty("path").GetString(), reviewedPaths);
            Assert.Equal("done", effect.GetProperty("outcome").GetString());
        });
        var after = workspace.Snapshot();
        Assert.All(before.Where(pair => pair.Key.StartsWith("file:", StringComparison.Ordinal)
            && pair.Key != $"file:{ExtensionInstallIntegrationWorkspace.OwnershipPath}"
            && !reviewedPaths.Contains(pair.Key[5..])), pair => Assert.Equal(pair.Value, after[pair.Key]));
        var parser = new MarkdownDocumentParser();
        var original = parser.Parse(authored);
        var current = parser.Parse(workspace.ReadText(host));
        var originalBlock = original.GeneratedRegion.EntriesBlock
            ?? throw new InvalidOperationException("The fixture requires an Entries block.");
        var currentBlock = current.GeneratedRegion.EntriesBlock
            ?? throw new InvalidOperationException("The installed host requires an Entries block.");
        Assert.Equal(original.Source[..originalBlock.Span.Start], current.Source[..currentBlock.Span.Start]);
        Assert.Equal(original.Source[originalBlock.Span.End..], current.Source[currentBlock.Span.End..]);
        Assert.Contains("\r\n", workspace.ReadText(host));
        var installed = workspace.ReadExtensionOwnership().EnumerateArray().Select(package => package.GetProperty("id").GetString());
        Assert.Equal(["development", "development-toolkit", "planning", "project-documents", "scenarios", "workflows"], installed);
        if (preset == "essentials")
        {
            Assert.False(Directory.Exists(workspace.Combine(".agents/templates")));
        }
    }
}
