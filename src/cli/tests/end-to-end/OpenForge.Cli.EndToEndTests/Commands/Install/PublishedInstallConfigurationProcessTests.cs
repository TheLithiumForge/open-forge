using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests.Commands.Install;

public sealed class PublishedInstallConfigurationProcessTests
{
    private static readonly string TranscriptRunId = Guid.NewGuid().ToString("N");

    [Fact(DisplayName = "Published preset preview, apply and no-op preserve complete configuration receipts"), Trait("Feature", "install-configuration"), Trait("Evidence", "EndToEnd")]
    public async Task PresetJsonPreviewApplyRepeat()
    {
        using var workspace = Create("e2e-install-configuration");
        var before = workspace.SnapshotState();
        var preview = await workspace.RunAsync("install", "--preset", "essentials", "--dry-run", "--format", "json");
        Assert.Equal(0, preview.ExitCode);
        Assert.Empty(preview.StandardError);
        Assert.Equal(before, workspace.SnapshotState());
        using var planned = JsonDocument.Parse(preview.StandardOutput);
        AssertConfiguration(planned.RootElement, "essentials", false);
        var apply = await workspace.RunAsync("install", "--preset", "essentials", "--automatic", "--format", "json");
        Assert.Equal(0, apply.ExitCode);
        Assert.Empty(apply.StandardError);
        using var applied = JsonDocument.Parse(apply.StandardOutput);
        AssertConfiguration(applied.RootElement, "essentials", false);
        Assert.Equal(planned.RootElement.GetProperty("effects").EnumerateArray().Select(effect => effect.GetProperty("path").GetString()),
            applied.RootElement.GetProperty("effects").EnumerateArray().Select(effect => effect.GetProperty("path").GetString()));
        Assert.Contains(applied.RootElement.GetProperty("effects").EnumerateArray(), effect => effect.GetProperty("path").GetString() == ".agents/open-forge.json"
            && effect.GetProperty("kind").GetString() == "setting");
        using var ownership = JsonDocument.Parse(File.ReadAllText(workspace.Combine(".agents/open-forge.lock.json")));
        var sharing = Assert.Single(ownership.RootElement.GetProperty("framework").GetProperty("gitIgnoredRoutes").EnumerateArray());
        Assert.Equal(".agents/memory/working", sharing.GetProperty("directory").GetString());
        Assert.Equal(".agents/memory/working/_working.md", sharing.GetProperty("entrypoint").GetString());
        Assert.Contains("!/.agents/memory/working/_working.md", File.ReadAllText(workspace.Combine(".gitignore")), StringComparison.Ordinal);
        var after = workspace.SnapshotState();
        var repeat = await workspace.RunAsync("install", "--configure", "--preset", "essentials", "--automatic", "--format", "json");
        Assert.Equal(0, repeat.ExitCode);
        using var repeated = JsonDocument.Parse(repeat.StandardOutput);
        AssertConfiguration(repeated.RootElement, "essentials", true);
        Assert.Empty(repeated.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Equal(after, workspace.SnapshotState());
        var ordinary = await workspace.RunAsync("install", "--automatic", "--format", "json");
        Assert.Equal(0, ordinary.ExitCode);
        using var ordinaryDocument = JsonDocument.Parse(ordinary.StandardOutput);
        Assert.False(ordinaryDocument.RootElement.GetProperty("data").TryGetProperty("configuration", out _));
        var doctor = await workspace.RunAsync("doctor", "--format", "json");
        Assert.Equal(0, doctor.ExitCode);
        var context = await workspace.RunAsync("context", "--format", "json");
        Assert.Equal(0, context.ExitCode);
    }

    [Theory(DisplayName = "Published invalid setup input leaves the workspace unchanged and never prompts")]
    [InlineData("configure-without-preset"), InlineData("unknown-preset"), InlineData("conflicting-route"), Trait("Feature", "install-configuration"), Trait("Evidence", "EndToEnd")]
    public async Task InvalidInput(string scenario)
    {
        using var workspace = Create("e2e-install-invalid-configuration");
        string[] arguments = scenario switch
        {
            "configure-without-preset" => ["install", "--configure", "--automatic", "--format", "json"],
            "unknown-preset" => ["install", "--preset", "unknown", "--automatic", "--format", "json"],
            _ => ["install", "--preset", "custom", "--route", "skills=add", "--route", "skills=remove", "--automatic", "--format", "json"],
        };
        var before = workspace.SnapshotState();
        var result = await workspace.RunAsync(arguments);
        Assert.Equal(4, result.ExitCode);
        Assert.Empty(result.StandardError);
        Assert.Equal(before, workspace.SnapshotState());
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("invalid-input", document.RootElement.GetProperty("status").GetString());
    }

    [Theory(DisplayName = "Real TTY first Install offers Essentials before one reviewed application confirmation"), InlineData(80, 24, true), InlineData(40, 12, true), InlineData(80, 24, false)]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "EndToEnd")]
    public async Task FirstPresetTerminal(short width, short height, bool apply)
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This confirmation journey requires the repository's Windows ConPTY harness.");
        using var workspace = Create("e2e-install-preset-terminal");
        var before = workspace.SnapshotState();
        var terminal = await PublishedWindowsTerminal.RunAsync(workspace.Target, workspace.Path, ["install"], workspace.ProcessEnvironment,
            new PublishedTerminalScenario
            {
                Size = new(width, height),
                TerminalName = "xterm",
                Steps = [new("Choose your Open Forge setup", "\r"), new("Apply these changes? [y/N]", apply ? "y" : "n")],
            });
        Capture($"essentials-{width}x{height}-{apply}", terminal.Transcript);
        Assert.Equal(apply ? 0 : 130, terminal.ExitCode);
        Assert.Contains("Essentials", terminal.Transcript, StringComparison.Ordinal);
        var plan = terminal.Transcript.IndexOf("Would install", StringComparison.Ordinal);
        var confirmation = terminal.Transcript.IndexOf("Apply these changes? [y/N]", StringComparison.Ordinal);
        Assert.True(plan >= 0 && confirmation > plan, terminal.Transcript);
        Assert.DoesNotContain("\u001b[2J", terminal.Transcript[plan..], StringComparison.Ordinal);
        if (apply)
        {
            Assert.True(File.Exists(workspace.Combine(".agents/memory/working/_working.md")));
            Assert.False(File.Exists(workspace.Combine(".agents/guidance/_guidance.md")));
            Assert.Contains("/.agents/memory/working/", File.ReadAllText(workspace.Combine(".gitignore")), StringComparison.Ordinal);
        }
        else Assert.Equal(before, workspace.SnapshotState());
    }

    [Fact(DisplayName = "Real TTY configure changes one Custom row and finishes without ten sequential questions"), Trait("Feature", "install-configuration"), Trait("Evidence", "EndToEnd")]
    public async Task CustomTerminalEditsOneRoute()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This confirmation journey requires the repository's Windows ConPTY harness.");
        using var workspace = Create("e2e-install-custom-terminal");
        Assert.Equal(0, (await workspace.RunAsync("install", "--preset", "essentials", "--automatic")).ExitCode);
        var terminal = await PublishedWindowsTerminal.RunAsync(workspace.Target, workspace.Path, ["install", "--configure"], workspace.ProcessEnvironment,
            new PublishedTerminalScenario
            {
                TerminalName = "xterm",
                Size = new(80, 24),
                Steps = [new("Choose your Open Forge setup", "\r"), new("Choose a route to change", "3"),
                    new("Choose how to configure guidance", "2"), new("Choose a route to change", "\r"),
                    new("[y/N]", "y")],
            });
        Capture("custom-guidance-80x24", terminal.Transcript);
        Assert.Equal(0, terminal.ExitCode);
        Assert.Contains("Finish selection", terminal.Transcript, StringComparison.Ordinal);
        Assert.Contains("Existing files, notes and companions", terminal.Transcript, StringComparison.Ordinal);
        Assert.True(File.Exists(workspace.Combine(".agents/guidance/_guidance.md")));
        Assert.False(File.Exists(workspace.Combine(".agents/templates/_templates.md")));
    }

    private static PublishedJourneyWorkspace Create(string purpose)
    {
        var workspace = PublishedJourneyWorkspace.Create(purpose);
        workspace.ExpectCoreInstall();
        workspace.ExpectFiles(".agents/open-forge.json", ".gitignore");
        return workspace;
    }

    private static void AssertConfiguration(JsonElement root, string preset, bool configure)
    {
        var configuration = root.GetProperty("data").GetProperty("configuration");
        Assert.Equal(["configure", "preset", "routes"], configuration.EnumerateObject().Select(property => property.Name));
        Assert.Equal(configure, configuration.GetProperty("configure").GetBoolean());
        Assert.Equal(preset, configuration.GetProperty("preset").GetString());
        Assert.Equal(10, configuration.GetProperty("routes").GetArrayLength());
        Assert.Contains(configuration.GetProperty("routes").EnumerateArray(), route => route.GetProperty("id").GetString() == "memory/working"
            && route.GetProperty("action").GetString() == "git-ignore");
    }

    private static void Capture(string name, string transcript)
    {
        var directory = Path.Combine(PublishedExecutableTarget.FindRepositoryRoot(), "artifacts", "test-evidence", "install-configuration-transcripts", TranscriptRunId);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, name + ".txt"), transcript);
    }
}
