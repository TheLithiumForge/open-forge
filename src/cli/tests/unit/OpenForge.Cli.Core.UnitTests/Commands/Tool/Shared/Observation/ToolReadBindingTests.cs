using System.CommandLine;
using OpenForge.Cli.Core.Commands.Tool.Inspect;
using OpenForge.Cli.Core.Commands.Tool.List;
using OpenForge.Cli.Core.Commands.Tool.Status;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Observation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Observation;

public sealed class ToolReadBindingTests
{
    [Fact(DisplayName = "Read bindings and invalid results don't construct home-dependent operations"), Trait("Feature", "tool-read"), Trait("Evidence", "Unit")]
    public void DeferredConstruction()
    {
        var parent = new Command("synthetic-tools");
        var seed = ToolObservationSeeds.Create(OpenForge.Cli.Core.Framework.Tools.Shared.Observation.Models.ToolObservationState.Current).Entry;
        var catalogue = new ToolCatalogue { Entries = new Dictionary<string, ToolCatalogueEntry> { [seed.Identity.Id] = seed }, NodeRuntimes = [] };
        var calls = 0;
        var list = ToolListBinding.CreateRequestBinding(ToolListBinding.CreateSymbols(parent), () => { calls++; throw new InvalidOperationException(); }, CliHelpContent.Empty, catalogue);
        var inspect = ToolInspectBinding.CreateRequestBinding(ToolInspectBinding.CreateSymbols(parent), () => { calls++; throw new InvalidOperationException(); }, CliHelpContent.Empty, catalogue);
        var status = ToolStatusBinding.CreateRequestBinding(ToolStatusBinding.CreateSymbols(parent), () => { calls++; throw new InvalidOperationException(); }, CliHelpContent.Empty, catalogue);
        var path = Path.Combine(Path.GetTempPath(), "synthetic-workspace");
        var invalid = new CliInvalidBindingInput(new("synthetic", CliInvalidInputSource.Parser, ["synthetic diagnostic"]),
            new(null, 0, CliFormat.Text, 0, CliDetail.Standard, 0, null, 0, false, 0, false, 0), new(path), new(list.Command.Parse([]), []));
        var listed = list.InvalidResultFactory(invalid);
        Assert.Equal(seed.Identity.Id, Assert.Single(listed.Data.Tools).Id);
        Assert.Equal(CliSemanticStatus.Invalid, listed.Status);
        Assert.Equal(CliSemanticStatus.Invalid, inspect.InvalidResultFactory(invalid).Status);
        Assert.Equal(CliSemanticStatus.Invalid, status.InvalidResultFactory(invalid).Status);
        Assert.Equal(0, calls);
        Assert.Equal(CliSemanticStatus.Blocked, list.InvalidResultFactory(invalid with
        {
            InvalidInput = new("synthetic", CliInvalidInputSource.Workspace, ["synthetic diagnostic"]),
            WorkspaceSelectionState = CliWorkspaceSelectionState.Unsafe,
        }).Status);
        Assert.Equal(0, calls);
    }

    [Theory(DisplayName = "Tool read symbols keep each leaf's operand count and accept no writer flags"), InlineData(0), InlineData(1), InlineData(2), Trait("Feature", "tool-read"), Trait("Evidence", "Unit")]
    public void LeafSymbolsAreBounded(int leaf)
    {
        var parent = new Command("synthetic-tools");
        var path = Path.Combine(Path.GetTempPath(), "synthetic-workspace");
        var workspace = new CliWorkspace(path, path, CliWorkspaceSelectionMethod.CurrentDirectory);
        var invocation = new CliInvocation(new("open-forge", "0.0.0"), new(CliFormat.Text, CliDetail.Standard, null), CliTerminalMode.None, new(null, path), workspace);
        var catalogue = new ToolCatalogue { Entries = new Dictionary<string, ToolCatalogueEntry>(), NodeRuntimes = [] };
        Func<CliBindingParse, CliSemanticStatus?>? bind = null;
        Command command;
        switch (leaf)
        {
            case 0: command = ToolListBinding.CreateSymbols(parent); break;
            case 1:
                var inspect = ToolInspectBinding.CreateSymbols(parent);
                command = inspect.Command;
                bind = parse => ToolInspectBinding.Bind(parse, invocation, inspect, catalogue).InvalidResult?.Status;
                break;
            case 2:
                var status = ToolStatusBinding.CreateSymbols(parent);
                command = status.Command;
                bind = parse => ToolStatusBinding.Bind(parse, invocation, status, catalogue).InvalidResult?.Status;
                break;
            default: throw new ArgumentOutOfRangeException(nameof(leaf));
        }
        Assert.Same(command, Assert.Single(parent.Subcommands));
        Assert.Empty(command.Options);
        Assert.NotEmpty(command.Parse(["first", "second"]).Errors);
        foreach (var flag in new[] { "--yes", "--dry-run" })
        {
            string[] arguments = [flag];
            var parsed = command.Parse(arguments);
            if (parsed.Errors.Count == 0) Assert.Equal(CliSemanticStatus.Invalid, bind?.Invoke(new CliBindingParse(parsed, arguments)));
            else Assert.NotEmpty(parsed.Errors);
        }
        Assert.Equal(leaf != 1, command.Parse(Array.Empty<string>()).Errors.Count == 0);
    }
}
