using System.CommandLine;
using OpenForge.Cli.Core.Commands.Tool.Install;
using OpenForge.Cli.Core.Commands.Tool.Update;
using OpenForge.Cli.Core.Commands.Tool.Remove;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Binding;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Binding;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Binding;

[Trait("Feature", "tool"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
public sealed class ToolWriteBindingTests
{
    [Theory(DisplayName = "Tool writer binding keeps parsed specs and all supported write flags")]
    [InlineData(false)]
    [InlineData(true)]
    public void BindsSupportedFlags(bool update)
    {
        var fixture = new ToolWriteOperationFixture();
        var symbols = Symbols(update ? ToolWriteKind.Update : ToolWriteKind.Install);
        string[] args =
        [
            ToolWriteOperationFixture.Id + "@1.2", ToolWriteBinding.ChannelOption, ToolWriteOperationFixture.ChannelId,
            ToolWriteBinding.YesOption, ToolWriteBinding.YesOption, ToolWriteBinding.DryRunOption,
            Core.Commands.Shared.PermissionOptions.AllowPath.Name, "synthetic-first",
            Core.Commands.Shared.PermissionOptions.AllowPath.Name, "synthetic-second",
        ];
        var result = ToolWriteRequestBinder.Bind(symbols, fixture.Versions, new(symbols.Command.Parse(args), args), Invocation(fixture.Root));
        var request = Assert.IsType<ToolWriteRequest>(result.Request);

        Assert.Null(result.InvalidResult);
        Assert.Equal(fixture.Versions.ReadSpec(args[0]), request.Spec);
        Assert.Equal(ToolWriteOperationFixture.ChannelId, request.Channel);
        Assert.True(request.Yes);
        Assert.True(request.Preview);
        Assert.Equal(2, request.AllowPaths.Length);
    }

    [Theory(DisplayName = "Tool writer bindings reject extra operands legacy flags repeated channels and unsupported selectors")]
    [InlineData("extra")]
    [InlineData("automatic")]
    [InlineData("download")]
    [InlineData("channel-repeat")]
    [InlineData("bare-channel")]
    [InlineData("empty-selector")]
    [InlineData("minor-x")]
    [InlineData("tag")]
    [InlineData("npm-range")]
    public void RejectsInvalidInput(string scenario)
    {
        var fixture = new ToolWriteOperationFixture();
        var symbols = Symbols(ToolWriteKind.Install);
        string[] args = scenario switch
        {
            "extra" => [ToolWriteOperationFixture.Id, ToolWriteOperationFixture.OtherId],
            "automatic" => ["--automatic"],
            "download" => ["--allow-download"],
            "channel-repeat" => [ToolWriteOperationFixture.Id, ToolWriteBinding.ChannelOption, ToolWriteOperationFixture.ChannelId, ToolWriteBinding.ChannelOption, ToolWriteOperationFixture.ChannelId],
            "bare-channel" => [ToolWriteBinding.ChannelOption, ToolWriteOperationFixture.ChannelId],
            "empty-selector" => [ToolWriteOperationFixture.Id + "@"],
            "minor-x" => [ToolWriteOperationFixture.Id + "@1.2.x"],
            "tag" => [ToolWriteOperationFixture.Id + "@latest"],
            "npm-range" => [ToolWriteOperationFixture.Id + "@^1.2"],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        var result = ToolWriteRequestBinder.Bind(symbols, fixture.Versions, new(symbols.Command.Parse(args), args), Invocation(fixture.Root));

        Assert.Null(result.Request);
        Assert.Equal(CliSemanticStatus.Invalid, Assert.IsType<Core.Commands.Tool.Models.Writing.Result.ToolWriteResult>(result.InvalidResult).Status);
    }

    [Theory(DisplayName = "Tool Remove requires one bare ID and accepts its write flags")]
    [InlineData(false)]
    [InlineData(true)]
    public void RemoveRequiresBareId(bool selector)
    {
        var fixture = new ToolWriteOperationFixture();
        var symbols = Symbols(ToolWriteKind.Remove);
        string[] args = [selector ? ToolWriteOperationFixture.Id + "@1.2" : ToolWriteOperationFixture.Id, ToolWriteBinding.YesOption];
        var result = ToolWriteRequestBinder.Bind(symbols, fixture.Versions, new(symbols.Command.Parse(args), args), Invocation(fixture.Root));

        if (selector) Assert.NotNull(result.InvalidResult);
        else Assert.Equal(ToolWriteKind.Remove, Assert.IsType<ToolWriteRequest>(result.Request).Kind);
        Assert.Null(symbols.Channel);
    }

    [Fact(DisplayName = "Tool JSON binding disables interactive request policy")]
    public void JsonNeverAllowsPrompts()
    {
        var fixture = new ToolWriteOperationFixture();
        var symbols = Symbols(ToolWriteKind.Install);
        var result = ToolWriteRequestBinder.Bind(symbols, fixture.Versions,
            new(symbols.Command.Parse([]), []), Invocation(fixture.Root, CliFormat.Json));

        Assert.False(Assert.IsType<ToolWriteRequest>(result.Request).CanPrompt);
    }

    private static ToolWriteSymbols Symbols(ToolWriteKind kind)
    {
        var parent = new Command("synthetic-tool-group");
        return kind switch
        {
            ToolWriteKind.Install => ToolInstallBinding.CreateSymbols(parent),
            ToolWriteKind.Update => ToolUpdateBinding.CreateSymbols(parent),
            ToolWriteKind.Remove => ToolRemoveBinding.CreateSymbols(parent),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }

    private static CliInvocation Invocation(string root, CliFormat format = CliFormat.Text)
        => new(new CliProcessIdentity("synthetic-cli", "1.0.0"), new CliPresentation(format, CliDetail.Minimal, null),
            CliTerminalMode.None, new CliWorkspaceRequest(null, root),
            new CliWorkspace(root, root, CliWorkspaceSelectionMethod.CurrentDirectory));
}
