using System.CommandLine;
using OpenForge.Cli.Core.Commands.Tool.Install;
using OpenForge.Cli.Core.Commands.Tool.Update;
using OpenForge.Cli.Core.Commands.Tool.Remove;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Binding;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Binding;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
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
        var result = ToolWriteRequestBinder.Bind(symbols, fixture.Catalogue, fixture.Versions, new(symbols.Command.Parse(args), args), Invocation(fixture.Root));
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
        var result = ToolWriteRequestBinder.Bind(symbols, fixture.Catalogue, fixture.Versions, new(symbols.Command.Parse(args), args), Invocation(fixture.Root));

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
        var result = ToolWriteRequestBinder.Bind(symbols, fixture.Catalogue, fixture.Versions, new(symbols.Command.Parse(args), args), Invocation(fixture.Root));

        if (selector) Assert.NotNull(result.InvalidResult);
        else Assert.Equal(ToolWriteKind.Remove, Assert.IsType<ToolWriteRequest>(result.Request).Kind);
        Assert.Null(symbols.Channel);
    }

    [Fact(DisplayName = "Tool JSON binding disables interactive request policy")]
    public void JsonNeverAllowsPrompts()
    {
        var fixture = new ToolWriteOperationFixture();
        var symbols = Symbols(ToolWriteKind.Install);
        var result = ToolWriteRequestBinder.Bind(symbols, fixture.Catalogue, fixture.Versions,
            new(symbols.Command.Parse([]), []), Invocation(fixture.Root, CliFormat.Json));

        Assert.False(Assert.IsType<ToolWriteRequest>(result.Request).CanPrompt);
    }

    [Theory(DisplayName = "Tool leaf request binding construction keeps operation factories deferred")]
    [InlineData((int)ToolWriteKind.Install)]
    [InlineData((int)ToolWriteKind.Update)]
    [InlineData((int)ToolWriteKind.Remove)]
    public void ConstructionDoesNotCreateOperation(int value)
    {
        var fixture = new ToolWriteOperationFixture();
        var symbols = Symbols((ToolWriteKind)value);
        var calls = 0;
        var binding = RequestBinding(symbols, fixture, () => calls++);

        Assert.Same(symbols.Command, binding.Command);
        Assert.Equal(CliWorkspaceRequirement.Required, binding.WorkspaceRequirement);
        Assert.Equal(0, calls);
        Assert.Empty(fixture.Calls);
    }

    [Theory(DisplayName = "Tool leaf help selection and help content do not create operations")]
    [InlineData((int)ToolWriteKind.Install)]
    [InlineData((int)ToolWriteKind.Update)]
    [InlineData((int)ToolWriteKind.Remove)]
    public void HelpDoesNotCreateOperation(int value)
    {
        var fixture = new ToolWriteOperationFixture();
        var parent = new Command("synthetic-tool-group");
        var symbols = Symbols((ToolWriteKind)value, parent);
        var calls = 0;
        var binding = RequestBinding(symbols, fixture, () => calls++);
        var tree = CliCommandTree.Create(CliHelpContent.Empty, [new(parent, CliHelpContent.Empty)], [],
            [new(symbols.Command, binding.Help)]);
        var parse = tree.Parse([parent.Name, symbols.Command.Name, tree.Options.Help.Name]);
        var input = CliGlobalInputReader.Read(parse);

        Assert.True(input.Help);
        Assert.Same(symbols.Command, parse.Result.CommandResult.Command);
        _ = CliTerminalInputValidator.Validate(parse, input);
        Assert.Same(binding.Help, tree.ReadHelp(symbols.Command));
        Assert.Equal(0, calls);
        Assert.Empty(fixture.Calls);
    }

    [Theory(DisplayName = "Unrelated command selection leaves Tool leaf operation factories unused")]
    [InlineData((int)ToolWriteKind.Install)]
    [InlineData((int)ToolWriteKind.Update)]
    [InlineData((int)ToolWriteKind.Remove)]
    public void UnrelatedSelectionDoesNotCreateOperation(int value)
    {
        var fixture = new ToolWriteOperationFixture();
        var parent = new Command("synthetic-tool-group");
        var symbols = Symbols((ToolWriteKind)value, parent);
        var unrelated = new Command("synthetic-other-command");
        parent.Subcommands.Add(unrelated);
        var calls = 0;
        _ = RequestBinding(symbols, fixture, () => calls++);
        var tree = CliCommandTree.Create(CliHelpContent.Empty, [new(parent, CliHelpContent.Empty)], []);
        var parse = tree.Parse([parent.Name, unrelated.Name]);

        Assert.Empty(parse.Result.Errors);
        Assert.Same(unrelated, parse.Result.CommandResult.Command);
        Assert.Equal(0, calls);
        Assert.Empty(fixture.Calls);
    }

    [Theory(DisplayName = "Tool parser and semantic binding failures keep operation factories unused")]
    [InlineData((int)ToolWriteKind.Install, false)]
    [InlineData((int)ToolWriteKind.Update, false)]
    [InlineData((int)ToolWriteKind.Remove, false)]
    [InlineData((int)ToolWriteKind.Install, true)]
    [InlineData((int)ToolWriteKind.Update, true)]
    [InlineData((int)ToolWriteKind.Remove, true)]
    public void InvalidInputDoesNotCreateOperation(int value, bool semantic)
    {
        var fixture = new ToolWriteOperationFixture();
        var symbols = Symbols((ToolWriteKind)value);
        var calls = 0;
        var binding = RequestBinding(symbols, fixture, () => calls++);
        string[] args = semantic ? [ToolWriteOperationFixture.Id + "@1.2.x"] : [ToolWriteOperationFixture.Id, "--synthetic-invalid"];
        var parse = symbols.Command.Parse(args);
        var result = binding.Binder(new(parse, args), Invocation(fixture.Root));

        Assert.Equal(semantic, parse.Errors.Count == 0);
        Assert.Null(result.Request);
        Assert.Equal(CliSemanticStatus.Invalid, Assert.IsType<ToolWriteResult>(result.InvalidResult).Status);
        Assert.Equal(0, calls);
        Assert.Empty(fixture.Calls);
    }

    [Theory(DisplayName = "Tool unknown named IDs fail binding before operation factories")]
    [InlineData((int)ToolWriteKind.Install)]
    [InlineData((int)ToolWriteKind.Update)]
    [InlineData((int)ToolWriteKind.Remove)]
    public void UnknownIdDoesNotCreateOperation(int value)
    {
        var fixture = new ToolWriteOperationFixture();
        var symbols = Symbols((ToolWriteKind)value);
        var catalogue = fixture.Catalogue with
        {
            Entries = new Dictionary<string, ToolCatalogueEntry> { [ToolWriteOperationFixture.Id] = fixture.Catalogue.Entries[ToolWriteOperationFixture.Id] },
        };
        var calls = 0;
        var binding = RequestBinding(symbols, fixture, () => calls++, catalogue);
        string[] args = [ToolWriteOperationFixture.OtherId];
        var parse = symbols.Command.Parse(args);
        var result = binding.Binder(new(parse, args), Invocation(fixture.Root));

        Assert.Empty(parse.Errors);
        Assert.NotNull(fixture.Versions.ReadSpec(args[0]));
        Assert.Null(result.Request);
        var invalid = Assert.IsType<ToolWriteResult>(result.InvalidResult);
        Assert.Equal(CliSemanticStatus.Invalid, invalid.Status);
        var finding = Assert.Single(invalid.Findings);
        Assert.Equal(ToolWriteFindingCode.UnknownTool, finding.Code);
        Assert.Equal(ToolWriteOperationFixture.OtherId, finding.ToolId);
        Assert.Equal(0, calls);
        Assert.Empty(fixture.Calls);
    }

    [Theory(DisplayName = "Tool unadvertised named channels fail binding before operation factories")]
    [InlineData((int)ToolWriteKind.Install)]
    [InlineData((int)ToolWriteKind.Update)]
    public void UnadvertisedChannelDoesNotCreateOperation(int value)
    {
        var fixture = new ToolWriteOperationFixture();
        var symbols = Symbols((ToolWriteKind)value);
        var calls = 0;
        var binding = RequestBinding(symbols, fixture, () => calls++);
        string[] args = [ToolWriteOperationFixture.Id, ToolWriteBinding.ChannelOption, "synthetic-unadvertised-channel"];
        var parse = symbols.Command.Parse(args);
        var result = binding.Binder(new(parse, args), Invocation(fixture.Root));

        Assert.Empty(parse.Errors);
        Assert.Null(result.Request);
        var invalid = Assert.IsType<ToolWriteResult>(result.InvalidResult);
        Assert.Equal(CliSemanticStatus.Invalid, invalid.Status);
        var finding = Assert.Single(invalid.Findings);
        Assert.Equal(ToolWriteFindingCode.ChannelInvalid, finding.Code);
        Assert.Equal(ToolWriteOperationFixture.Id, finding.ToolId);
        Assert.Equal(0, calls);
        Assert.Empty(fixture.Calls);
    }

    [Theory(DisplayName = "Tool workspace binding failure does not resolve the operation graph")]
    [InlineData((int)ToolWriteKind.Install)]
    [InlineData((int)ToolWriteKind.Update)]
    [InlineData((int)ToolWriteKind.Remove)]
    public void MissingWorkspaceDoesNotCreateOperation(int value)
    {
        var fixture = new ToolWriteOperationFixture();
        var symbols = Symbols((ToolWriteKind)value);
        var calls = 0;
        var binding = RequestBinding(symbols, fixture, () => calls++);
        string[] args = [ToolWriteOperationFixture.Id];
        var invocation = Invocation(fixture.Root) with { Workspace = null };
        var result = binding.Binder(new(symbols.Command.Parse(args), args), invocation);

        Assert.Null(result.Request);
        Assert.Equal(CliSemanticStatus.Blocked, Assert.IsType<ToolWriteResult>(result.InvalidResult).Status);
        Assert.Equal(0, calls);
        Assert.Empty(fixture.Calls);
    }

    [Theory(DisplayName = "Tool contextual invalid result factory stays independent of operation factories")]
    [InlineData((int)ToolWriteKind.Install)]
    [InlineData((int)ToolWriteKind.Update)]
    [InlineData((int)ToolWriteKind.Remove)]
    public void InvalidResultDoesNotCreateOperation(int value)
    {
        var fixture = new ToolWriteOperationFixture();
        var symbols = Symbols((ToolWriteKind)value);
        var calls = 0;
        var binding = RequestBinding(symbols, fixture, () => calls++);
        var invocation = Invocation(fixture.Root);
        var invalid = binding.InvalidResultFactory(new(
            new("synthetic-workspace-unavailable", CliInvalidInputSource.Workspace, ["Synthetic workspace unavailable."]),
            new(null, 0, invocation.Presentation.Format, 0, invocation.Presentation.Detail, 0, null, 0, false, 0, false, 0),
            new(fixture.Root), new(symbols.Command.Parse([]), [])));

        Assert.Equal(symbols.Kind, invalid.Kind);
        Assert.Equal(CliSemanticStatus.Blocked, invalid.Status);
        Assert.Equal(ToolWriteFindingCode.InvalidInput, Assert.Single(invalid.Findings).Code);
        Assert.Equal(0, calls);
        Assert.Empty(fixture.Calls);
    }

    [Theory(DisplayName = "Tool valid bound execution creates its leaf operation exactly once")]
    [InlineData((int)ToolWriteKind.Install)]
    [InlineData((int)ToolWriteKind.Update)]
    [InlineData((int)ToolWriteKind.Remove)]
    public async Task ValidExecutionCreatesOperationOnce(int value)
    {
        var fixture = new ToolWriteOperationFixture();
        var symbols = Symbols((ToolWriteKind)value);
        var calls = 0;
        var binding = RequestBinding(symbols, fixture, () => calls++);
        string[] args = [ToolWriteOperationFixture.Id, ToolWriteBinding.YesOption];
        var bound = binding.Binder(new(symbols.Command.Parse(args), args), Invocation(fixture.Root));
        var request = Assert.IsType<ToolWriteRequest>(bound.Request);
        Assert.Null(bound.InvalidResult);
        Assert.Equal(0, calls);
        Assert.Empty(fixture.Calls);

        var result = await CliOperationStage.InvokeAsync(new CliOperationRequest<ToolWriteRequest>(request), binding.Operation, CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Result.Status);
        Assert.Equal(symbols.Kind, result.Result.Kind);
        Assert.Equal(1, calls);
        Assert.NotEmpty(fixture.Calls);
    }

    [Theory(DisplayName = "Tool bare requests keep declared batch selection in execution")]
    [InlineData((int)ToolWriteKind.Install)]
    [InlineData((int)ToolWriteKind.Update)]
    public async Task BareExecutionResolvesDeclaredRecords(int value)
    {
        var fixture = new ToolWriteOperationFixture(secondTool: true);
        var symbols = Symbols((ToolWriteKind)value);
        var calls = 0;
        var binding = RequestBinding(symbols, fixture, () => calls++);
        string[] args = [ToolWriteBinding.YesOption];
        var bound = binding.Binder(new(symbols.Command.Parse(args), args), Invocation(fixture.Root));
        var request = Assert.IsType<ToolWriteRequest>(bound.Request);

        Assert.Null(bound.InvalidResult);
        Assert.Null(request.Spec);
        Assert.Null(request.Channel);
        Assert.Equal(0, calls);
        Assert.Empty(fixture.Calls);

        var result = await CliOperationStage.InvokeAsync(new CliOperationRequest<ToolWriteRequest>(request), binding.Operation, CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Result.Status);
        Assert.Equal(1, calls);
        Assert.Equal(new[] { ToolWriteOperationFixture.Id, ToolWriteOperationFixture.OtherId },
            fixture.ChannelRequests.Select(request => request.Entry.Identity.Id));
    }

    private static CliRequestBinding<ToolWriteRequest, ToolWriteResult> RequestBinding(
        ToolWriteSymbols symbols, ToolWriteOperationFixture fixture, Action onCreate, ToolCatalogue? catalogue = null)
    {
        catalogue ??= fixture.Catalogue;
        return symbols.Kind switch
        {
            ToolWriteKind.Install => ToolInstallBinding.CreateRequestBinding(symbols, () => new(Create()), catalogue, fixture.Versions, CliHelpContent.Empty),
            ToolWriteKind.Update => ToolUpdateBinding.CreateRequestBinding(symbols, () => new(Create()), catalogue, fixture.Versions, CliHelpContent.Empty),
            ToolWriteKind.Remove => ToolRemoveBinding.CreateRequestBinding(symbols, () => new(Create()), catalogue, fixture.Versions, CliHelpContent.Empty),
            _ => throw new ArgumentOutOfRangeException(nameof(symbols)),
        };

        ToolWriteOperation Create()
        {
            onCreate();
            return fixture.Create();
        }
    }

    private static ToolWriteSymbols Symbols(ToolWriteKind kind, Command? parent = null)
    {
        parent ??= new Command("synthetic-tool-group");
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
