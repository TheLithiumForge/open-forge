using OpenForge.Cli.Core.Commands.Tool.Models;
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
        using var result = await ToolOutputCapture.MatchAsync(new("dry-run", ToolCommandFixture.Install(CodeGraphEntry.Id, DryRun)));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), result.Run.ExitCode);
        Assert.Equal(result.Before, result.Fixture.Files());
        Assert.Empty(result.Fixture.Processes.Effects);
    }

    [Fact(DisplayName = "Install with --yes runs install then setup and adds the archive exclusion and the settings entry")]
    public async Task InstallsAndConfigures()
    {
        using var result = await ToolOutputCapture.MatchAsync(new("installed", ToolCommandFixture.Install(CodeGraphEntry.Id, Yes)));
        var fixture = result.Fixture;

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), result.Run.ExitCode);
        Assert.Equal([.. fixture.CodeGraph.Install.Commands.Select(command => command.Text), .. fixture.CodeGraph.Setup!.Commands.Select(command => command.Text)],
            fixture.Processes.Effects);
        AssertIntegrated(fixture, fixture.CodeGraph, ToolIntegrationState.Present);
        Assert.Equal([fixture.CodeGraph.Id], fixture.ConfiguredTools());
    }

    [Fact(DisplayName = "An installed tool skips its install command, still runs setup and names the detected version")]
    public async Task AlreadyInstalled()
    {
        static Task Installed(ToolCommandFixture fixture)
        {
            fixture.Processes.Installed(fixture.CodeGraph);
            return Task.CompletedTask;
        }

        using var preview = await ToolOutputCapture.MatchAsync(new("already-installed-dry-run", ToolCommandFixture.Install(CodeGraphEntry.Id, DryRun))
        {
            Arrange = Installed,
        });
        using var result = await ToolOutputCapture.MatchAsync(new("already-installed", ToolCommandFixture.Install(CodeGraphEntry.Id, Yes))
        {
            Arrange = Installed,
        });

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), result.Run.ExitCode);
        Assert.Equal(result.Fixture.CodeGraph.Setup!.Commands.Select(command => command.Text), result.Fixture.Processes.Effects);
    }

    [Fact(DisplayName = "A missing package manager blocks before anything runs and says what to install")]
    public async Task MissingPackageManager()
    {
        using var result = await ToolOutputCapture.MatchAsync(new("missing-npm", ToolCommandFixture.Install(CodeGraphEntry.Id, Yes))
        {
            Arrange = static fixture =>
            {
                fixture.Processes.Missing(ToolDefinitions.Probe(ToolRequirement.Npm));
                return Task.CompletedTask;
            },
        });

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Blocked), result.Run.ExitCode);
        Assert.Equal(result.Before, result.Fixture.Files());
        Assert.Empty(result.Fixture.Processes.Effects);
    }

    [Fact(DisplayName = "A failed setup command reports its exit code, output and next step and leaves the files unchanged")]
    public async Task FailedCommand()
    {
        using var result = await ToolOutputCapture.MatchAsync(new("failed-setup", ToolCommandFixture.Install(CodeGraphEntry.Id, Yes))
        {
            Arrange = static fixture =>
            {
                fixture.Processes.Fail(fixture.CodeGraph.Setup!.Commands[0]);
                return Task.CompletedTask;
            },
        });

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Failed), result.Run.ExitCode);
        Assert.Equal(result.Before, result.Fixture.Files());
    }

    [Fact(DisplayName = "Without --yes and a terminal Install runs nothing and asks for confirmation")]
    public async Task ConfirmationRequired()
    {
        using var result = await ToolOutputCapture.MatchAsync(new("confirmation-required", ToolCommandFixture.Install(CodeGraphEntry.Id)));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Invalid), result.Run.ExitCode);
        Assert.Empty(result.Fixture.Processes.Effects);
    }

    [Fact(DisplayName = "An interactive plan shows the commands and changes, then runs only on yes")]
    public async Task InteractivePlan()
    {
        using var declined = await ToolOutputCapture.MatchAsync(new("plan-declined", ToolCommandFixture.Install(CodeGraphEntry.Id))
        {
            Answers = Environment.NewLine,
        });
        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Interrupted), declined.Run.ExitCode);
        Assert.Equal(declined.Before, declined.Fixture.Files());
        Assert.Empty(declined.Fixture.Processes.Effects);
        ToolOutputCapture.MatchPrompt(declined.Run.Prompts, "plan-prompt", nameof(ToolInstallOutputSnapshotTests));

        using var accepted = await ToolOutputCapture.MatchAsync(new("plan-accepted", ToolCommandFixture.Install(CodeGraphEntry.Id))
        {
            Answers = "y" + Environment.NewLine,
        });

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), accepted.Run.ExitCode);
        AssertIntegrated(accepted.Fixture, accepted.Fixture.CodeGraph, ToolIntegrationState.Present);
    }

    [Fact(DisplayName = "Codebase Memory's plan shows its own installer for each platform and its marked block")]
    public async Task ScriptInstaller()
    {
        foreach (var platform in Enum.GetValues<ToolPlatform>())
        {
            using var result = await ToolOutputCapture.MatchAsync(
                new($"script-{platform.ToString().ToLowerInvariant()}", ToolCommandFixture.Install(CodebaseMemoryEntry.Id, DryRun))
                {
                    Create = () => new ToolCommandFixture(platform),
                });
            Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), result.Run.ExitCode);
        }
    }

    [Fact(DisplayName = "Codebase Memory installs through its script and adds its marked block")]
    public async Task ScriptInstallRuns()
    {
        using var result = await ToolOutputCapture.MatchAsync(new("script-installed", ToolCommandFixture.Install(CodebaseMemoryEntry.Id, Yes)));
        var fixture = result.Fixture;

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), result.Run.ExitCode);
        Assert.Equal(fixture.CodebaseMemory.Install.Commands.Select(command => command.Text), fixture.Processes.Effects);
        AssertIntegrated(fixture, fixture.CodebaseMemory, ToolIntegrationState.Present);
    }

    [Fact(DisplayName = "Bare install reports when the project has no tools and runs nothing")]
    public async Task BareWithoutTools()
    {
        using var result = await ToolOutputCapture.MatchAsync(new("bare-none", ToolCommandFixture.Install(null, Yes)));

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), result.Run.ExitCode);
        Assert.Empty(result.Fixture.Processes.Effects);
    }

    [Fact(DisplayName = "Bare install installs only the configured tools that are missing and never reruns setup")]
    public async Task BareInstallsMissing()
    {
        static async Task Configured(ToolCommandFixture fixture)
            => Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete),
                (await fixture.RunAsync(ToolCommandFixture.Install(CodeGraphEntry.Id, Yes))).ExitCode);

        using var preview = await ToolOutputCapture.MatchAsync(new("bare-dry-run", ToolCommandFixture.Install(null, DryRun)) { Arrange = Configured });
        using var result = await ToolOutputCapture.MatchAsync(new("bare-installed", ToolCommandFixture.Install(null, Yes)) { Arrange = Configured });
        using var nothing = await ToolOutputCapture.MatchAsync(new("bare-nothing-missing", ToolCommandFixture.Install(null, Yes))
        {
            Arrange = static async fixture =>
            {
                await Configured(fixture);
                fixture.Processes.Installed(fixture.CodeGraph);
            },
        });

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), result.Run.ExitCode);
        Assert.Equal(result.Fixture.CodeGraph.Install.Commands.Select(command => command.Text), result.Fixture.Processes.Effects);
        Assert.Equal(result.Before, result.Fixture.Files());
        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Complete), nothing.Run.ExitCode);
        Assert.Empty(nothing.Fixture.Processes.Effects);
    }

    [Fact(DisplayName = "A malformed integration file blocks Install and stays unchanged")]
    public async Task IntegrationConflict()
    {
        using var result = await ToolOutputCapture.MatchAsync(new("conflict", ToolCommandFixture.Install(CodeGraphEntry.Id, Yes))
        {
            Arrange = static fixture =>
            {
                fixture.Write(CodeGraphEntry.ConfigurationPath, "[");
                return Task.CompletedTask;
            },
        });

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Blocked), result.Run.ExitCode);
        Assert.Equal(result.Before, result.Fixture.Files());
        Assert.Empty(result.Fixture.Processes.Effects);
        Assert.Empty(result.Fixture.ConfiguredTools());
    }

    [Fact(DisplayName = "A folder without Open Forge blocks Install before anything runs")]
    public async Task WithoutOpenForge()
    {
        using var result = await ToolOutputCapture.MatchAsync(new("without-open-forge", ToolCommandFixture.Install(CodeGraphEntry.Id, Yes))
        {
            Create = static () => new ToolCommandFixture(openForge: false),
        });

        Assert.Equal(ToolCommandFixture.Exit(CliSemanticStatus.Blocked), result.Run.ExitCode);
        Assert.Empty(result.Fixture.Processes.Effects);
        Assert.Empty(result.Fixture.Files());
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
