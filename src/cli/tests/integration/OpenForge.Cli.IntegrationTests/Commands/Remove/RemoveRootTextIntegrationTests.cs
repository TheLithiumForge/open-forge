using System.Text.Json;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;

namespace OpenForge.Cli.IntegrationTests.Commands.Remove;

public sealed class RemoveRootTextIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "Root Remove preview text describes planned effects truthfully at every detail level")]
    [InlineData("minimal")]
    [InlineData("standard")]
    [InlineData("full")]
    [InlineData("debug")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task PreviewTextReflectsActionAndDetail(string detail)
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create($"remove-root-preview-text-{detail}");
        workspace.WriteText("old.txt", "Keep preview write-free.\n");
        var before = workspace.SnapshotHashes();
        var textOutput = new StringWriter();
        var textError = new StringWriter();

        var textCompletion = await workspace.RunAsync(
            ["remove", "old.txt", "--dry-run", "--detail", detail, "--format", "text"],
            textOutput,
            textError);

        Assert.Equal(0, textCompletion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, textCompletion.Status);
        Assert.Equal(string.Empty, textError.ToString());
        Assert.Contains("Would remove old.txt", textOutput.ToString(), StringComparison.Ordinal);
        if (detail == "minimal")
        {
            Assert.DoesNotContain("Would create", textOutput.ToString(), StringComparison.Ordinal);
            Assert.DoesNotContain("Would remove file old.txt", textOutput.ToString(), StringComparison.Ordinal);
        }
        else
        {
            Assert.Contains("Would create directory .agents", textOutput.ToString(), StringComparison.Ordinal);
            Assert.Contains("Would create setting .agents/open-forge.json", textOutput.ToString(), StringComparison.Ordinal);
            Assert.Contains("Would remove file old.txt", textOutput.ToString(), StringComparison.Ordinal);
        }
        Assert.Equal(before, workspace.SnapshotHashes());

        var jsonOutput = new StringWriter();
        var jsonError = new StringWriter();
        var jsonCompletion = await workspace.RunAsync(
            ["remove", "old.txt", "--dry-run", "--detail", detail, "--format", "json"],
            jsonOutput,
            jsonError);

        Assert.Equal(0, jsonCompletion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, jsonCompletion.Status);
        Assert.Equal(string.Empty, jsonError.ToString());
        using var report = JsonDocument.Parse(jsonOutput.ToString());
        var effects = report.RootElement.GetProperty("data").GetProperty("effects").EnumerateArray().ToArray();
        AssertEffect(effects, ".agents", "directory", "create", "planned");
        AssertEffect(effects, ".agents/open-forge.json", "setting", "create", "planned");
        AssertEffect(effects, "old.txt", "file", "delete", "planned");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root Remove minimal text prints its headline finding only once")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task MinimalTextDoesNotRepeatHeadlineFinding()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-minimal-finding");
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["remove", ".agents/open-forge.json", "--detail", "minimal", "--format", "text"],
            output,
            error);

        Assert.Equal(5, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, completion.Status);
        const string message = "This path contains workspace settings, ownership, Git metadata, or other protected workspace state.";
        var text = output.ToString() + error.ToString();
        Assert.Equal(1, text.Split(message, StringSplitOptions.None).Length - 1);
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Root Remove partial-failure text preserves completed and failed paths at every detail level")]
    [InlineData("minimal")]
    [InlineData("standard")]
    [InlineData("full")]
    [InlineData("debug")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task PartialFailureTextReflectsReceiptOutcomes(string detail)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        using var textWorkspace = PartialWorkspace($"remove-root-partial-text-{detail}");
        var textOutput = new StringWriter();
        var textError = new StringWriter();
        var textCompletion = await RunWithLockedFile(
            textWorkspace,
            ["remove", "batch", "--automatic", "--detail", detail, "--format", "text"],
            textOutput,
            textError);

        Assert.Equal(CliSemanticStatus.Incomplete, textCompletion.Status);
        Assert.Equal(string.Empty, textError.ToString());
        Assert.Contains("Created directory .agents", textOutput.ToString(), StringComparison.Ordinal);
        Assert.Contains("Created setting .agents/open-forge.json", textOutput.ToString(), StringComparison.Ordinal);
        Assert.Contains("Removed file batch/a-first.bin", textOutput.ToString(), StringComparison.Ordinal);

        using var jsonWorkspace = PartialWorkspace($"remove-root-partial-json-{detail}");
        var jsonOutput = new StringWriter();
        var jsonError = new StringWriter();
        var jsonCompletion = await RunWithLockedFile(
            jsonWorkspace,
            ["remove", "batch", "--automatic", "--detail", detail, "--format", "json"],
            jsonOutput,
            jsonError);

        Assert.Equal(CliSemanticStatus.Incomplete, jsonCompletion.Status);
        Assert.Equal(string.Empty, jsonError.ToString());
        using var report = JsonDocument.Parse(jsonOutput.ToString());
        var effects = report.RootElement.GetProperty("data").GetProperty("effects").EnumerateArray().ToArray();
        AssertEffect(effects, ".agents", "directory", "create", "done");
        AssertEffect(effects, ".agents/open-forge.json", "setting", "create", "done");
        AssertEffect(effects, "batch/a-first.bin", "file", "delete", "done");
        var blocked = Assert.Single(effects, effect => effect.GetProperty("path").GetString() == "batch/z-locked.bin");
        Assert.Equal("file", blocked.GetProperty("kind").GetString());
        Assert.Equal("delete", blocked.GetProperty("action").GetString());
        var outcome = blocked.GetProperty("outcome").GetString();
        Assert.NotEqual("done", outcome);
        Assert.Contains(PartialEffectText(outcome), textOutput.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("Removed file batch/z-locked.bin", textOutput.ToString(), StringComparison.Ordinal);
    }

    private static RemoveRootIntegrationWorkspace PartialWorkspace(string purpose)
    {
        var workspace = RemoveRootIntegrationWorkspace.Create(purpose);
        workspace.WriteText("batch/a-first.bin", "Remove this target first.\n");
        workspace.WriteText("batch/z-locked.bin", "Retain this locked target.\n");
        return workspace;
    }

    private static async Task<CliProcessCompletion> RunWithLockedFile(
        RemoveRootIntegrationWorkspace workspace,
        IReadOnlyList<string> arguments,
        StringWriter output,
        StringWriter error)
    {
        using var held = new FileStream(
            workspace.Combine("batch/z-locked.bin"),
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read | FileShare.Write);
        return await workspace.RunAsync(arguments, output, error);
    }

    private static void AssertEffect(
        IEnumerable<JsonElement> effects,
        string path,
        string kind,
        string action,
        string outcome)
    {
        var effect = Assert.Single(effects, candidate => candidate.GetProperty("path").GetString() == path);
        Assert.Equal(kind, effect.GetProperty("kind").GetString());
        Assert.Equal(action, effect.GetProperty("action").GetString());
        Assert.Equal(outcome, effect.GetProperty("outcome").GetString());
    }

    private static string PartialEffectText(string? outcome)
        => outcome switch
        {
            "failed" => "Could not remove file batch/z-locked.bin",
            "unknown" => "Could not confirm whether file batch/z-locked.bin was removed",
            "not-started" => "Did not remove file batch/z-locked.bin",
            _ => throw new InvalidOperationException($"Unexpected partial deletion outcome '{outcome}'."),
        };
}
