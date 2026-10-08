using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;

namespace OpenForge.Cli.EndToEndTests.Commands.Install;

public sealed class PublishedInstallFrontmatterProcessTests
{
    [Fact(DisplayName = "Published first Install asks for frontmatter after the preset and cancellation writes nothing")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "EndToEnd")]
    public async Task FirstInstallAsksFormAfterPreset()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This selection journey requires the repository's Windows ConPTY harness.");
        using var workspace = Create("install-frontmatter-questions");
        var before = workspace.SnapshotState();
        var terminal = await PublishedWindowsTerminal.RunAsync(workspace.Target, workspace.Path, ["install"], workspace.ProcessEnvironment,
            new PublishedTerminalScenario
            {
                Size = new(80, 24),
                TerminalName = "xterm",
                Steps = [new("Choose your Open Forge setup", "\r"), new("How should Open Forge write file metadata?", "\u001b")],
            });
        Assert.Equal(130, terminal.ExitCode);
        var preset = terminal.Transcript.IndexOf("Choose your Open Forge setup", StringComparison.Ordinal);
        var form = terminal.Transcript.IndexOf("How should Open Forge write file metadata?", StringComparison.Ordinal);
        Assert.True(preset >= 0 && form > preset, terminal.Transcript);
        Assert.Equal(before, workspace.SnapshotState());
    }

    [Fact(DisplayName = "Published explicit frontmatter skips the form question before confirmation")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "EndToEnd")]
    public async Task ExplicitFormSkipsQuestion()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This selection journey requires the repository's Windows ConPTY harness.");
        using var workspace = Create("install-frontmatter-explicit");
        var before = workspace.SnapshotState();
        var terminal = await PublishedWindowsTerminal.RunAsync(workspace.Target, workspace.Path,
            ["install", "--preset", "full-core", "--frontmatter", "scoped"], workspace.ProcessEnvironment,
            new PublishedTerminalScenario
            {
                Size = new(80, 24),
                TerminalName = "xterm",
                Steps = [new("Apply these changes? [y/N]", "n")],
            });
        Assert.Equal(130, terminal.ExitCode);
        Assert.DoesNotContain("How should Open Forge write file metadata?", terminal.Transcript, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotState());
    }

    [Fact(DisplayName = "Published JSON preview shows the resolved form without prompts or writes")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "EndToEnd")]
    public async Task JsonPreviewShowsForm()
    {
        using var workspace = Create("install-frontmatter-json");
        var before = workspace.SnapshotState();
        var preview = await workspace.RunAsync("install", "--frontmatter", "root", "--dry-run", "--format", "json");
        Assert.Equal(0, preview.ExitCode);
        Assert.Empty(preview.StandardError);
        using var document = JsonDocument.Parse(preview.StandardOutput);
        var form = document.RootElement.GetProperty("data").GetProperty("frontmatter");
        Assert.Equal("root", form.GetProperty("form").GetString());
        Assert.False(form.TryGetProperty("previousForm", out _));
        Assert.Equal(before, workspace.SnapshotState());
    }

    private static PublishedJourneyWorkspace Create(string purpose)
    {
        var workspace = PublishedJourneyWorkspace.Create(purpose);
        workspace.ExpectCoreInstall();
        workspace.ExpectFiles(".agents/open-forge.json", ".gitignore");
        return workspace;
    }
}
