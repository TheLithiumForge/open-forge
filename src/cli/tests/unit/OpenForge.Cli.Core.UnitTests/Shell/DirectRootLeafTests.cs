using System.CommandLine;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Parsing.Models.Results;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Shell;

public sealed class DirectRootLeafTests
{
    [Fact(DisplayName = "A direct root leaf preserves its command identity and delimiter policies"), Trait("Feature", "shell-root-leaf"), Trait("Evidence", "Unit")]
    public void RootLeafDefinitionPreservesItsShape()
    {
        var command = new Command("find");
        var policies = new List<CliDelimiterPolicy>();

        var rootLeaf = new CliRootLeaf(command, policies);

        Assert.Same(command, rootLeaf.Command);
        Assert.Equal(policies, rootLeaf.DelimiterPolicies);
    }

    [Fact(DisplayName = "A direct root leaf is classified as a leaf and selects its exact binding"), Trait("Feature", "shell-root-leaf"), Trait("Evidence", "Unit")]
    public void DirectRootLeafIsNotAGroupAndSelectsItsBinding()
    {
        var command = new Command("find");
        var binding = new RecordingBinding(command, new CliHelpContent([
            new CliHelpSection("Syntax", "find")
        ]));
        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [],
            [binding],
            rootLeaves: [new CliRootLeaf(command, [])]);

        var selection = CliBindingSelector.Select(tree.Parse(["find"]));

        Assert.False(tree.IsGroup(command));
        Assert.Same(binding, tree.FindBinding(command));
        Assert.Equal(CliBindingSelectionState.Leaf, selection.State);
        Assert.Same(command, selection.Command);
        Assert.Same(binding, selection.Binding);
    }

    [Fact(DisplayName = "A direct root leaf requires one exact binding"), Trait("Feature", "shell-root-leaf"), Trait("Evidence", "Unit")]
    public void RootLeafWithoutBindingIsRejected()
    {
        var command = new Command("find");

        var exception = Assert.Throws<ArgumentException>(() => CliCommandTree.Create(
            CliHelpContent.Empty,
            [],
            [],
            rootLeaves: [new CliRootLeaf(command, [])]));

        Assert.Contains("binding", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "A null direct root leaf is rejected before root composition"), Trait("Feature", "shell-root-leaf"), Trait("Evidence", "Unit")]
    public void NullRootLeafIsRejected()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => CliCommandTree.Create(
            CliHelpContent.Empty,
            [],
            [],
            rootLeaves: [null!]));

        Assert.Contains("root leaf", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "A direct root leaf requires a reference-equal binding command"), Trait("Feature", "shell-root-leaf"), Trait("Evidence", "Unit")]
    public void RootLeafRequiresReferenceEqualBindingCommand()
    {
        var leaf = new Command("find");
        var differentCommand = new Command("find");
        var binding = new RecordingBinding(differentCommand, CliHelpContent.Empty);

        var exception = Assert.Throws<ArgumentException>(() => CliCommandTree.Create(
            CliHelpContent.Empty,
            [],
            [binding],
            rootLeaves: [new CliRootLeaf(leaf, [])]));

        Assert.Contains("same", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Duplicate direct root leaves are rejected deterministically"), Trait("Feature", "shell-root-leaf"), Trait("Evidence", "Unit")]
    public void DuplicateRootLeavesAreRejected()
    {
        var command = new Command("find");
        var binding = new RecordingBinding(command, CliHelpContent.Empty);

        var exception = Assert.Throws<ArgumentException>(() => CliCommandTree.Create(
            CliHelpContent.Empty,
            [],
            [binding],
            rootLeaves: [new CliRootLeaf(command, []), new CliRootLeaf(command, [])]));

        Assert.Contains("once", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "A direct root leaf cannot also be a root branch"), Trait("Feature", "shell-root-leaf"), Trait("Evidence", "Unit")]
    public void RootLeafBranchCollisionIsRejected()
    {
        var command = new Command("find");
        var binding = new RecordingBinding(command, CliHelpContent.Empty);

        var exception = Assert.Throws<ArgumentException>(() => CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(command, CliHelpContent.Empty, [])],
            [binding],
            rootLeaves: [new CliRootLeaf(command, [])]));

        Assert.Contains("branch", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "A same-name direct root leaf cannot coexist with a distinct root branch command"), Trait("Feature", "shell-root-leaf"), Trait("Evidence", "Unit")]
    public void SameNameRootLeafAndBranchCollisionIsRejected()
    {
        var branch = new Command("find");
        var leaf = new Command("find");
        var binding = new RecordingBinding(leaf, CliHelpContent.Empty);

        var exception = Assert.Throws<ArgumentException>(() => CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(branch, CliHelpContent.Empty, [])],
            [binding],
            rootLeaves: [new CliRootLeaf(leaf, [])]));

        Assert.Contains("collision", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "A direct root leaf keeps binding-owned help authoritative over additional root help"), Trait("Feature", "shell-root-leaf"), Trait("Evidence", "Unit")]
    public void BindingOwnedHelpWinsForRootLeaf()
    {
        var command = new Command("find");
        var bindingHelp = new CliHelpContent([
            new CliHelpSection("Binding", "Find binding help")
        ]);
        var additionalHelp = new CliCommandHelp(
            command,
            new CliHelpContent([
                new CliHelpSection("Additional", "Additional help must not replace binding help")
            ]));
        var binding = new RecordingBinding(command, bindingHelp);

        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [],
            [binding],
            [additionalHelp],
            [new CliRootLeaf(command, [])]);

        Assert.Same(bindingHelp, tree.ReadHelp(command));
    }

    [Fact(DisplayName = "Existing Route group commands remain groups while their leaves still select exact bindings"), Trait("Feature", "shell-root-leaf"), Trait("Evidence", "Unit")]
    public void ExistingRouteGroupCompositionRemainsUnchanged()
    {
        var route = new Command("route");
        var list = new Command("list");
        route.Add(list);
        var binding = new RecordingBinding(list, CliHelpContent.Empty);
        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(route, CliHelpContent.Empty, [])],
            [binding]);

        var selection = CliBindingSelector.Select(tree.Parse(["route", "list"]));

        Assert.True(tree.IsGroup(route));
        Assert.False(tree.IsGroup(list));
        Assert.Equal(CliBindingSelectionState.Leaf, selection.State);
        Assert.Same(binding, selection.Binding);
        Assert.Equal(["list"], route.Subcommands.Select(command => command.Name));
    }

    private sealed class RecordingBinding(Command command, CliHelpContent help) : ICliCommandBinding
    {
        public Command Command { get; } = command;

        public CliHelpContent Help { get; } = help;

        public CliWorkspaceRequirement WorkspaceRequirement => CliWorkspaceRequirement.Absent;

        public ValueTask<CliProcessCompletion> InvokeAsync(
            CliBindingParse parse,
            CliInvocation invocation,
            CliOutputWriters writers,
            CancellationToken cancellationToken)
            => throw new InvalidOperationException("The root-leaf composition evidence does not invoke the recording binding.");

        public ValueTask<CliProcessCompletion> PresentInvalidAsync(
            CliInvalidBindingInput input,
            CliOutputWriters writers,
            CancellationToken cancellationToken)
            => throw new InvalidOperationException("The root-leaf composition evidence does not present an invalid binding.");
    }
}
