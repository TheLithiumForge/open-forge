using OpenForge.Cli.Core.Commands.Help;
using OpenForge.Cli.Core.Commands.Tool;
using OpenForge.Cli.Core.Commands.Tool.Inspect;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Binding;
using OpenForge.Cli.Core.Commands.Tool.Status;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodeGraph;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool;

public sealed class ToolCompositionIsolationIntegrationTests
{
    private const string UnknownId = "unknown-tool";
    private const string MissingChannel = "missing";

    // One row per distinct inert path through the full Root: the bare group, global version and help,
    // unknown IDs for a read and a write leaf, a rejected selector and both missing-channel forms.
    // Leaf help pages take the help path that ToolHelpIntegrationTests covers for every leaf.
    public static TheoryData<string[]> Cases
    {
        get
        {
            var group = ToolDefinitions.Group.Name;
            var install = ToolWriteBinding.Name(ToolWriteKind.Install);
            return new()
            {
                new[] { group },
                new[] { CliSyntaxDefinitions.Version.Name },
                new[] { HelpDefinitions.CommandIdentity },
                new[] { group, ToolInspectDefinitions.CommandName, UnknownId },
                new[] { group, ToolStatusDefinitions.CommandName, UnknownId },
                new[] { group, install, UnknownId, ToolWriteBinding.YesOption },
                new[] { group, install, CodeGraphDefinitions.Id + "@1.x", ToolWriteBinding.YesOption },
                new[] { group, install, ToolWriteBinding.ChannelOption, MissingChannel },
                new[] { group, install, CodeGraphDefinitions.Id, ToolWriteBinding.ChannelOption, MissingChannel },
            };
        }
    }

    [Theory(DisplayName = "Inert composed invocations never resolve invalid homes or call any Tool host"), MemberData(nameof(Cases)), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task InertInvocations(string[] arguments)
    {
        using var fixture = new ToolCompositionFixture { ThrowHosts = true, InvalidHome = true };
        var before = fixture.Snapshot();
        var run = await fixture.RunAsync(arguments, fullRoot: true);
        Assert.Contains(run.ExitCode, new[] { ToolCompositionFixture.Exit(CliSemanticStatus.Complete), ToolCompositionFixture.Exit(CliSemanticStatus.Invalid) });
        Assert.Equal(before, fixture.Snapshot());
        Assert.Equal(0, fixture.ResolveCalls);
        Assert.Equal(0, fixture.PathObservations);
        Assert.Equal(0, fixture.PathEffects);
        Assert.Equal(0, fixture.DownloadCalls);
        Assert.Empty(fixture.Events);
        Assert.False(Directory.Exists(fixture.Home));
    }

    [Theory(DisplayName = "Unrelated read operations never construct the Tool graph"), InlineData("find --tag Synthetic"), InlineData("context"), InlineData("route list root"), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task UnrelatedOperations(string command)
    {
        using var fixture = new ToolCompositionFixture { ThrowHosts = true, InvalidHome = true };
        var before = fixture.Snapshot();
        var run = await fixture.RunAsync(command.Split(' '), fullRoot: true);
        Assert.Contains(run.ExitCode, new[] { CliSemanticStatus.Complete, CliSemanticStatus.Incomplete, CliSemanticStatus.Invalid, CliSemanticStatus.Blocked }.Select(ToolCompositionFixture.Exit));
        Assert.Equal(before, fixture.Snapshot());
        Assert.Equal(0, fixture.ResolveCalls);
        Assert.Equal(0, fixture.PathObservations);
        Assert.False(Directory.Exists(fixture.Home));
    }
}
