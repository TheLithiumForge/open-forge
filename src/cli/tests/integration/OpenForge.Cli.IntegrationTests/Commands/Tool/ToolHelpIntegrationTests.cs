using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Commands.Tool;
using OpenForge.Cli.Core.Commands.Tool.List;
using OpenForge.Cli.Core.Commands.Tool.Inspect;
using OpenForge.Cli.Core.Commands.Tool.Status;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Binding;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool;

public sealed class ToolHelpIntegrationTests
{
    public static TheoryData<string> Pages => new()
    {
        string.Empty, ToolListDefinitions.CommandName, ToolInspectDefinitions.CommandName, ToolStatusDefinitions.CommandName,
        ToolWriteBinding.Name(ToolWriteKind.Install), ToolWriteBinding.Name(ToolWriteKind.Update), ToolWriteBinding.Name(ToolWriteKind.Remove),
    };

    [Theory(DisplayName = "Tool help uses the actual Root graph without resolving homes or calling hosts"), MemberData(nameof(Pages))]
    [Trait("Feature", "tool-help"), Trait("Evidence", "Integration")]
    public async Task HelpNeedsNoWorkspaceOrHosts(string leaf)
    {
        using var fixture = new ToolCompositionFixture { ThrowHosts = true, InvalidHome = true };
        var before = fixture.Snapshot();
        var missing = Path.Combine(fixture.Workspace, "missing");
        string[] operand = leaf.Length == 0 ? [] : [leaf];
        var result = await fixture.RunAsync([ToolDefinitions.Group.Name, .. operand, CliSyntaxDefinitions.Help.Name, CliSyntaxDefinitions.Workspace.Name, missing], fullRoot: true);
        Assert.Equal(0, result.ExitCode);
        Assert.NotEmpty(result.Output);
        Assert.Empty(result.Error);
        Assert.Equal(before, fixture.Snapshot());
        Assert.False(Directory.Exists(missing));
        Assert.Equal(0, fixture.ResolveCalls);
        Assert.Equal(0, fixture.DownloadCalls);
        Assert.Equal(0, fixture.PathEffects);
    }

    [Fact(DisplayName = "All six Tool leaves provide structured examples behavior and global help"), Trait("Feature", "tool-help"), Trait("Evidence", "Integration")]
    public void LeavesHaveStructuredHelp()
    {
        using var fixture = new ToolCompositionFixture { ThrowHosts = true, InvalidHome = true };
        var tool = fixture.Compose(new CliCompositionInputs
        {
            StandardInput = TextReader.Null,
            PromptOutput = TextWriter.Null,
            StandardInputRedirected = true,
            PromptOutputRedirected = true,
            Tools = fixture.Inputs,
        });
        var leaves = new[] { tool.ListBinding, tool.InspectBinding, tool.StatusBinding, tool.InstallBinding, tool.UpdateBinding, tool.RemoveBinding };
        Assert.All(leaves, leaf =>
        {
            Assert.False(string.IsNullOrEmpty(leaf.Help.Usage));
            Assert.True(leaf.Help.SummarizeGlobalOptions);
            Assert.Equal(2, leaf.Help.Introduction.Count);
        });
        Assert.Equal(0, fixture.ResolveCalls);
    }

    [Theory(DisplayName = "Inspect and Remove still require one ID during normal execution")]
    [InlineData("inspect"), InlineData("remove"), Trait("Feature", "tool-help"), Trait("Evidence", "Integration")]
    public async Task ExecutionRequiresId(string leaf)
    {
        using var fixture = new ToolCompositionFixture { ThrowHosts = true, InvalidHome = true };
        var before = fixture.Snapshot();
        var run = await fixture.RunAsync([ToolDefinitions.Group.Name, leaf], fullRoot: true);
        Assert.Equal(CliStatusDefinitions.Read(CliSemanticStatus.Invalid).Disposition.ExitCode, run.ExitCode);
        Assert.Equal(before, fixture.Snapshot());
        Assert.False(Directory.Exists(fixture.Home));
        Assert.Equal(0, fixture.ResolveCalls);
        Assert.Equal(0, fixture.DownloadCalls);
        Assert.Equal(0, fixture.PathEffects);
        Assert.Empty(fixture.Events);
    }
}
