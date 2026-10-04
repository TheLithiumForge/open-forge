using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;

namespace OpenForge.Cli.EndToEndTests;

[Trait("Feature", "library-git-ignore"), Trait("Evidence", "EndToEnd")]
public sealed class PublishedLibraryGitIgnoreProcessTests
{
    private const string OwnershipPath = ".agents/open-forge.lock.json";
    private const string SourceBody = "# Source-owned bytes\n";
    private const string Section = "# BEGIN OPEN FORGE LIBRARIES\n/docs/a.md\n# END OPEN FORGE LIBRARIES\n";

    [Fact]
    public static async Task ExplicitTrueLifecycleIncludesOrdinaryFileReportsAndRootLibraryRemove()
    {
        using var workspace = Create("library-ignore-process");
        workspace.WriteText(".gitignore", "# authored\n");
        using var attached = Result(await workspace.RunAsync("library", "attach", "team", "source", "--to", "docs", "--git-ignore", "true", "--allow-path", "docs", "--allow-path", ".gitignore", "--automatic", "--format=json"));
        Assert.Equal(".gitignore", attached.RootElement.GetProperty("data").GetProperty("gitIgnore").GetProperty("path").GetString());
        AssertIgnoreEffect(attached.RootElement, "rewritten");
        Assert.Equal("# authored\n" + Section, File.ReadAllText(workspace.Combine(".gitignore")));
        Assert.True(ReadLibrary(workspace).GetProperty("gitIgnore").GetBoolean());
        workspace.WriteText("source/b.md", SourceBody);
        using var synced = Result(await workspace.RunAsync("library", "sync", "team", "--automatic", "--format=json"));
        AssertIgnoreEffect(synced.RootElement, "rewritten");
        Assert.Equal("# authored\n# BEGIN OPEN FORGE LIBRARIES\n/docs/a.md\n/docs/b.md\n# END OPEN FORGE LIBRARIES\n", File.ReadAllText(workspace.Combine(".gitignore")));
        using var removed = Result(await workspace.RunAsync("remove", "team", "--kind", "library", "--automatic", "--format=json"));
        Assert.Equal("remove", removed.RootElement.GetProperty("command").GetString());
        AssertIgnoreEffect(removed.RootElement, "rewritten");
        Assert.Equal("# authored\n", File.ReadAllText(workspace.Combine(".gitignore")));
        Assert.False(File.Exists(workspace.Combine("docs/a.md")));
        Assert.False(File.Exists(workspace.Combine("docs/b.md")));
        Assert.Equal(SourceBody, File.ReadAllText(workspace.Combine("source/a.md")));
        Assert.Equal(SourceBody, File.ReadAllText(workspace.Combine("source/b.md")));
    }

    [Theory]
    [InlineData("false"), InlineData(null)]
    public static async Task ExplicitFalseAndUnattendedOmissionKeepIgnoreAndOmitPersistedIntent(string? argument)
    {
        using var workspace = Create("library-ignore-process-no");
        workspace.WriteText(".gitignore", "# BEGIN OPEN FORGE LIBRARIES\n/unrelated\n");
        var before = File.ReadAllBytes(workspace.Combine(".gitignore"));
        var arguments = new List<string> { "library", "attach", "team", "source", "--to", "docs", "--allow-path", "docs", "--automatic", "--format=json" };
        if (argument is not null) arguments.AddRange(["--git-ignore", argument]);
        using var result = Result(await workspace.RunAsync(arguments.ToArray()));
        Assert.False(result.RootElement.GetProperty("data").TryGetProperty("gitIgnore", out _));
        Assert.False(ReadLibrary(workspace).TryGetProperty("gitIgnore", out _));
        Assert.Equal(before, File.ReadAllBytes(workspace.Combine(".gitignore")));
        Assert.False(result.RootElement.GetProperty("counts").TryGetProperty("gitIgnoreFilesUpdated", out _));
    }

    [Theory]
    [InlineData("missing"), InlineData("invalid"), InlineData("repeated")]
    public static async Task InvalidOrRepeatedBooleanIsRejectedWithoutEffects(string scenario)
    {
        using var workspace = Create("library-ignore-process-invalid");
        var option = scenario switch
        {
            "missing" => new[] { "--git-ignore" },
            "invalid" => new[] { "--git-ignore", "yes" },
            "repeated" => new[] { "--git-ignore", "true", "--git-ignore", "false" },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        var before = workspace.SnapshotState();
        var result = await workspace.RunAsync(new[] { "library", "attach", "team", "source", "--automatic", "--format=json" }.Concat(option).ToArray());
        Assert.Equal(4, result.ExitCode);
        Assert.Equal(before, workspace.SnapshotState());
        Assert.False(File.Exists(workspace.Combine(OwnershipPath)));
    }

    [Fact]
    public static async Task UnattendedHelpExampleUsesImplicitDestinationPermissionAndTheExactIgnoreGrant()
    {
        using var workspace = Create("library-ignore-help-example");
        workspace.WriteText("vendor/shared/a.md", SourceBody);
        workspace.ExpectFiles(".agents/libraries/shared/a.md");
        using var result = Result(await workspace.RunAsync("library", "attach", "shared", "vendor/shared", "--to", ".agents/libraries/shared",
            "--git-ignore", "true", "--allow-path", ".gitignore", "--automatic", "--format=json"));
        AssertIgnoreEffect(result.RootElement, "created");
        Assert.Equal("# BEGIN OPEN FORGE LIBRARIES\n/.agents/libraries/shared/a.md\n# END OPEN FORGE LIBRARIES\n", File.ReadAllText(workspace.Combine(".gitignore")));
        Assert.Equal(SourceBody, File.ReadAllText(workspace.Combine("vendor/shared/a.md")));
        Assert.True(ReadLibrary(workspace).GetProperty("gitIgnore").GetBoolean());
    }

    [Fact]
    public static async Task HelpAndDryRunExposeTrueFalseAndTheExactIgnorePlanWithoutEffects()
    {
        using var workspace = Create("library-ignore-process-preview");
        var help = await workspace.RunAsync("library", "attach", "--help");
        Assert.Equal(0, help.ExitCode);
        Assert.Contains("--git-ignore <true|false>", help.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--to .agents/libraries/shared", help.StandardOutput, StringComparison.Ordinal);
        var before = workspace.SnapshotState();
        using var result = Result(await workspace.RunAsync("library", "attach", "team", "source", "--to", "docs", "--git-ignore", "true", "--allow-path", "docs", "--allow-path", ".gitignore", "--dry-run", "--detail=full", "--format=json"));
        Assert.Equal("create", result.RootElement.GetProperty("data").GetProperty("gitIgnore").GetProperty("action").GetString());
        var effect = Assert.Single(result.RootElement.GetProperty("effects").EnumerateArray(), effect => effect.GetProperty("path").GetString() == ".gitignore");
        Assert.Equal("planned", effect.GetProperty("outcome").GetString());
        Assert.Equal(before, workspace.SnapshotState());
    }

    [Theory]
    [InlineData("yes"), InlineData("no"), InlineData("cancel")]
    public static async Task BoundTerminalChoiceUsesDefaultNoAndDistinguishesEscape(string answer)
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("Library terminal choice evidence requires Windows ConPTY.");
        using var workspace = Create("library-ignore-terminal");
        var before = workspace.SnapshotState();
        var steps = new List<PublishedTerminalStep>
        {
            new("enter choose  esc cancel", answer switch { "yes" => "\u001b[B\r", "no" => "\r", "cancel" => "\u001b", _ => throw new ArgumentOutOfRangeException(nameof(answer)) }),
        };
        if (answer != "cancel")
        {
            steps.Add(new("team writes outside .agents:", "2"));
            steps.Add(new("Apply these changes? [y/N]", "y"));
        }
        var terminal = await PublishedWindowsTerminal.RunAsync(workspace.Target, workspace.Path,
            ["library", "attach", "team", "source", "--to", "docs"], workspace.ProcessEnvironment,
            new PublishedTerminalScenario { TerminalName = "xterm", Steps = steps });
        Assert.Equal(answer == "cancel" ? 130 : 0, terminal.ExitCode);
        Assert.Contains("Add this Library's projected links to .gitignore?", terminal.Transcript, StringComparison.Ordinal);
        Assert.Contains("Choice 1/2", terminal.Transcript, StringComparison.Ordinal);
        var choice = terminal.Transcript.IndexOf("Add this Library's projected links to .gitignore?", StringComparison.Ordinal);
        if (answer == "cancel")
        {
            Assert.Equal(before, workspace.SnapshotState());
            Assert.DoesNotContain("team writes outside .agents:", terminal.Transcript, StringComparison.Ordinal);
        }
        else
        {
            var permission = terminal.Transcript.IndexOf("team writes outside .agents:", StringComparison.Ordinal);
            var preview = terminal.Transcript.IndexOf("Would register the team Library from source.", StringComparison.Ordinal);
            var confirmation = terminal.Transcript.IndexOf("Apply these changes? [y/N]", StringComparison.Ordinal);
            Assert.True(choice >= 0 && permission > choice && preview > permission && confirmation > preview, terminal.Transcript);
            Assert.Equal(answer == "yes", File.Exists(workspace.Combine(".gitignore")));
            Assert.Equal(answer == "yes", ReadLibrary(workspace).TryGetProperty("gitIgnore", out _));
            Assert.Equal("../source/a.md", new FileInfo(workspace.Combine("docs/a.md")).LinkTarget);
            if (answer == "yes") Assert.Equal(Section, File.ReadAllText(workspace.Combine(".gitignore")));
        }
        Assert.Equal(SourceBody, File.ReadAllText(workspace.Combine("source/a.md")));
    }

    private static PublishedJourneyWorkspace Create(string purpose)
    {
        var workspace = PublishedJourneyWorkspace.Create(purpose);
        workspace.WriteText(".agents/placeholder.md", "# Workspace\n");
        workspace.WriteText("source/a.md", SourceBody);
        workspace.ExpectFiles("docs/a.md", "docs/b.md", ".gitignore", OwnershipPath, ".agents/open-forge.json");
        return workspace;
    }

    private static JsonElement ReadLibrary(PublishedJourneyWorkspace workspace)
    {
        using var document = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(OwnershipPath)));
        return Assert.Single(document.RootElement.GetProperty("libraries").EnumerateArray()).Clone();
    }

    private static JsonDocument Result(Shared.PublishedProcess.ProcessRunResult result)
    {
        Assert.True(result.ExitCode == 0, $"{result.ExitCode}: {result.StandardError}\n{result.StandardOutput}");
        Assert.Equal(string.Empty, result.StandardError);
        var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
        return document;
    }

    private static void AssertIgnoreEffect(JsonElement result, string action)
    {
        var effect = Assert.Single(result.GetProperty("effects").EnumerateArray(), effect => effect.GetProperty("path").GetString() == ".gitignore");
        Assert.Equal("file", effect.GetProperty("kind").GetString());
        Assert.Equal(action, effect.GetProperty("action").GetString());
        Assert.Equal("done", effect.GetProperty("outcome").GetString());
        Assert.Equal(1, result.GetProperty("counts").GetProperty("gitIgnoreFilesUpdated").GetInt32());
    }
}
