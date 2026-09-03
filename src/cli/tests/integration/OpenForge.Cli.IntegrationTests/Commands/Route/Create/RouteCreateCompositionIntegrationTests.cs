using System.Runtime.CompilerServices;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Create;

public sealed class RouteCreateCompositionIntegrationTests
{
    [Fact(DisplayName = "Composed root owns one exact Route Create leaf and stable Route delimiters"), Trait("Feature", "route-create"), Trait("Evidence", "Integration")]
    public void ComposedRootOwnsExactRouteCreateLeafAndDelimiters()
    {
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "test"));
        var parser = CliCoreApplicationAccess.Parser(application);
        var tree = CliParserAccess.Tree(parser);
        var route = Assert.Single(
            tree.Root.Subcommands,
            command => command.Name == "route");

        Assert.Equal(
            ["list", "inspect", "init", "create", "update", "move"],
            route.Subcommands.Select(command => command.Name));
        var create = route.Subcommands[3];
        var parse = tree.Parse(["route", "create"]);
        var selection = CliBindingSelector.Select(parse);

        Assert.Same(create, selection.Command);
        Assert.Equal(CliBindingSelectionState.Leaf, selection.State);
        var binding = Assert.IsAssignableFrom<ICliCommandBinding>(selection.Binding);
        Assert.Same(create, binding.Command);
        Assert.Same(binding, tree.FindBinding(create));
        Assert.Equal(
            ["--depth", "--tag"],
            parse.DelimiterPolicies.Select(policy => policy.OptionName));
        Assert.Single(
            parse.DelimiterPolicies,
            policy => policy.OptionName == "--tag");
    }

    [Fact(DisplayName = "Composed Route group and Create leaf help are direct no-write terminal modes"), Trait("Feature", "route-create"), Trait("Evidence", "Integration")]
    public async Task ComposedRouteHelpIsTruthfulAndWriteFree()
    {
        using var workspace = TemporaryWorkspace.Create("route-create-composed-help");
        var before = workspace.SnapshotHashes();
        var missingWorkspace = workspace.Combine("missing-workspace");
        var group = await RunAsync(["route"], workspace.Path);
        var leaf = await RunAsync(
            ["route", "create", "--help", "--workspace", missingWorkspace],
            workspace.Path);

        Assert.Equal(0, group.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, group.Status);
        Assert.Equal(string.Empty, group.StandardError);
        Assert.Contains("Commands:", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("list <source-reference>", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("inspect <source-reference>", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("init <route-target>", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("create <file-target>", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("update <source-reference>", group.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("list     available", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("The route group performs no operation.", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Planned but unavailable operation: remove.", group.StandardOutput, StringComparison.Ordinal);

        Assert.Equal(0, leaf.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, leaf.Status);
        Assert.Equal(string.Empty, leaf.StandardError);
        Assert.Contains("open-forge route create <file-target>", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Metadata", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Template", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Write policy", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(Directory.Exists(missingWorkspace));
        Assert.False(group.LockInfrastructureExists);
        Assert.False(leaf.LockInfrastructureExists);
    }

    private static async Task<RouteCreateCompositionRun> RunAsync(
        string[] arguments,
        string currentDirectory)
    {
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        using var lockStore = WorkspaceLockTestStore.Create(
            "route-create-composition-lock-store");
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "test"),
            new CliCompositionInputs
            {
                StandardInput = TextReader.Null,
                PromptOutput = TextWriter.Null,
                StandardInputRedirected = true,
                PromptOutputRedirected = true,
                LockStoreRoot = lockStore.StoreRoot,
            });
        var completion = await application.RunAsync(
            arguments,
            new CliProcessEnvironment(currentDirectory),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);
        return new RouteCreateCompositionRun(
            completion.ExitCode,
            completion.Status,
            standardOutput.ToString(),
            standardError.ToString(),
            lockStore.InfrastructureExists);
    }

    private static class CliCoreApplicationAccess
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_parser")]
        internal static extern ref CliParser Parser(CliCoreApplication application);
    }

    private static class CliParserAccess
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_tree")]
        internal static extern ref CliCommandTree Tree(CliParser parser);
    }

    private sealed record RouteCreateCompositionRun(
        int ExitCode,
        CliSemanticStatus Status,
        string StandardOutput,
        string StandardError,
        bool LockInfrastructureExists);
}
