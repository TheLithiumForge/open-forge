using OpenForge.Cli.Core.Commands.Tool;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models;
using OpenForge.Cli.Core.Framework.Tools.Shared.Integration;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Tool.Shared;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool;

[Trait("Feature", "tool-install"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
public sealed class ToolInstallOutputSnapshotTests
{
    private const string Yes = ToolCommandDefinitions.YesOption;
    private const string DryRun = ToolCommandDefinitions.DryRunOption;

    [Fact(DisplayName = "Dry run shows the exact commands and Open Forge's changes and runs or changes nothing")]
    public async Task Preview()
    {
        using var fixture = new ToolCommandFixture();
        var before = fixture.Files();

        var text = await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodeGraph.Id, DryRun));
        var json = await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodeGraph.Id, [DryRun, .. ToolCommandFixture.Json]));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), text.ExitCode);
        Assert.Equal(before, fixture.Files());
        Assert.Empty(fixture.Processes.Effects);
        ToolCommandFixture.Match(text.Output, "dry-run");
        ToolCommandFixture.Match(json.Output, "dry-run.json");
    }

    [Fact(DisplayName = "Install with --yes runs install then setup and adds the archive exclusion and the settings entry")]
    public async Task InstallsAndConfigures()
    {
        using var fixture = new ToolCommandFixture();

        var run = await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodeGraph.Id, Yes));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), run.ExitCode);
        Assert.Equal([.. fixture.CodeGraph.Install.Commands.Select(command => command.Text), .. fixture.CodeGraph.Setup!.Commands.Select(command => command.Text)],
            fixture.Processes.Effects);
        AssertIntegrated(fixture, fixture.CodeGraph, ToolIntegrationState.Present);
        Assert.Equal([fixture.CodeGraph.Id], fixture.ConfiguredTools());
        ToolCommandFixture.Match(run.Output, "installed");
    }

    [Fact(DisplayName = "An installed tool skips its install command, still runs setup and names the detected version")]
    public async Task AlreadyInstalled()
    {
        using var fixture = new ToolCommandFixture();
        fixture.Processes.Installed(fixture.CodeGraph);

        var preview = await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodeGraph.Id, DryRun));
        var run = await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodeGraph.Id, Yes));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), run.ExitCode);
        Assert.Equal(fixture.CodeGraph.Setup!.Commands.Select(command => command.Text), fixture.Processes.Effects);
        ToolCommandFixture.Match(preview.Output, "already-installed.dry-run");
        ToolCommandFixture.Match(run.Output, "already-installed");
    }

    [Fact(DisplayName = "A missing package manager blocks before anything runs and says what to install")]
    public async Task MissingPackageManager()
    {
        using var fixture = new ToolCommandFixture();
        fixture.Processes.Missing(ToolDefinitions.Probe(ToolRequirement.Npm));
        var before = fixture.Files();

        var text = await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodeGraph.Id, Yes));
        var json = await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodeGraph.Id, [Yes, .. ToolCommandFixture.Json]));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Blocked), text.ExitCode);
        Assert.Equal(before, fixture.Files());
        Assert.Empty(fixture.Processes.Effects);
        ToolCommandFixture.Match(text.Output, "missing-npm");
        ToolCommandFixture.Match(json.Output, "missing-npm.json");
    }

    [Fact(DisplayName = "A failed setup command reports its exit code, output and next step and leaves the files unchanged")]
    public async Task FailedCommand()
    {
        using var fixture = new ToolCommandFixture();
        fixture.Processes.Fail(fixture.CodeGraph.Setup!.Commands[0]);
        var before = fixture.Files();

        var text = await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodeGraph.Id, Yes));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Failed), text.ExitCode);
        Assert.Equal(before, fixture.Files());
        ToolCommandFixture.Match(text.Output, "failed-setup");
    }

    [Fact(DisplayName = "Without --yes and a terminal Install runs nothing and asks for confirmation")]
    public async Task ConfirmationRequired()
    {
        using var fixture = new ToolCommandFixture();

        var text = await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodeGraph.Id));
        var json = await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodeGraph.Id, ToolCommandFixture.Json));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Invalid), text.ExitCode);
        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Invalid), json.ExitCode);
        Assert.Empty(fixture.Processes.Effects);
        ToolCommandFixture.Match(text.Output, "confirmation-required");
    }

    [Fact(DisplayName = "An interactive plan shows the commands and changes, then runs only on yes")]
    public async Task InteractivePlan()
    {
        using var fixture = new ToolCommandFixture();
        var before = fixture.Files();

        var declined = await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodeGraph.Id), Environment.NewLine);
        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Interrupted), declined.ExitCode);
        Assert.Equal(before, fixture.Files());
        Assert.Empty(fixture.Processes.Effects);

        var accepted = await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodeGraph.Id), "y" + Environment.NewLine);

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), accepted.ExitCode);
        AssertIntegrated(fixture, fixture.CodeGraph, ToolIntegrationState.Present);
        ToolCommandFixture.Match(declined.Prompts, "plan.prompt");
        ToolCommandFixture.Match(accepted.Output, "plan.accepted");
    }

    [Fact(DisplayName = "Codebase Memory's plan shows its own installer for each platform and its marked block")]
    public async Task ScriptInstaller()
    {
        foreach (var platform in Enum.GetValues<ToolPlatform>())
        {
            using var fixture = new ToolCommandFixture(platform);
            var text = await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodebaseMemory.Id, DryRun));
            Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), text.ExitCode);
            ToolCommandFixture.Match(text.Output, $"script.{platform.ToString().ToLowerInvariant()}");
        }
    }

    [Fact(DisplayName = "Codebase Memory installs through its script and adds its marked block")]
    public async Task ScriptInstallRuns()
    {
        using var fixture = new ToolCommandFixture();

        var run = await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodebaseMemory.Id, Yes));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), run.ExitCode);
        Assert.Equal(fixture.CodebaseMemory.Install.Commands.Select(command => command.Text), fixture.Processes.Effects);
        AssertIntegrated(fixture, fixture.CodebaseMemory, ToolIntegrationState.Present);
        ToolCommandFixture.Match(run.Output, "script-installed");
    }

    [Fact(DisplayName = "Bare install reports when the project has no tools and runs nothing")]
    public async Task BareWithoutTools()
    {
        using var fixture = new ToolCommandFixture();

        var run = await fixture.RunAsync(ToolCommandFixture.Install(null, Yes));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), run.ExitCode);
        Assert.Empty(fixture.Processes.Effects);
        ToolCommandFixture.Match(run.Output, "bare.none");
    }

    [Fact(DisplayName = "Bare install installs only the configured tools that are missing and never reruns setup")]
    public async Task BareInstallsMissing()
    {
        using var fixture = new ToolCommandFixture();
        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete),
            (await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodeGraph.Id, Yes))).ExitCode);
        var configured = fixture.Files();
        fixture.Processes.Commands.Clear();

        var preview = await fixture.RunAsync(ToolCommandFixture.Install(null, DryRun));
        var run = await fixture.RunAsync(ToolCommandFixture.Install(null, Yes));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), run.ExitCode);
        Assert.Equal(fixture.CodeGraph.Install.Commands.Select(command => command.Text), fixture.Processes.Effects);
        Assert.Equal(configured, fixture.Files());
        ToolCommandFixture.Match(preview.Output, "bare.dry-run");
        ToolCommandFixture.Match(run.Output, "bare.installed");

        fixture.Processes.Installed(fixture.CodeGraph);
        var nothing = await fixture.RunAsync(ToolCommandFixture.Install(null, Yes));
        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), nothing.ExitCode);
        ToolCommandFixture.Match(nothing.Output, "bare.nothing-missing");
    }

    [Fact(DisplayName = "A malformed integration file blocks Install and stays unchanged")]
    public async Task IntegrationConflict()
    {
        using var fixture = new ToolCommandFixture();
        fixture.Write(CodeGraphEntry.ConfigurationPath, "[");
        var before = fixture.Files();

        var run = await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodeGraph.Id, Yes));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Blocked), run.ExitCode);
        Assert.Equal(before, fixture.Files());
        Assert.Empty(fixture.Processes.Effects);
        Assert.Empty(fixture.ConfiguredTools());
        ToolCommandFixture.Match(run.Output, "conflict");
    }

    [Fact(DisplayName = "A folder without Open Forge blocks Install before anything runs")]
    public async Task WithoutOpenForge()
    {
        using var fixture = new ToolCommandFixture(openForge: false);

        var run = await fixture.RunAsync(ToolCommandFixture.Install(fixture.CodeGraph.Id, Yes));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Blocked), run.ExitCode);
        Assert.Empty(fixture.Processes.Effects);
        Assert.Empty(fixture.Files());
        ToolCommandFixture.Match(run.Output, "without-open-forge");
    }

    internal static void AssertIntegrated(ToolCommandFixture fixture, ToolEntry entry, ToolIntegrationState expected)
    {
        foreach (var integration in entry.Integrations)
        {
            var text = fixture.Read(integration.Path);
            var bytes = text is null ? null : System.Text.Encoding.UTF8.GetBytes(text);
            Assert.Equal(expected, ToolIntegrationEditor.Read(integration, bytes, WorkspaceSettingsDefinitions.MemoryArchivePath).State);
        }
    }
}
