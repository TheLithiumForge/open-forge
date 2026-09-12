using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.Results;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Composition;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Create;

public sealed class RouteCreateCompositionIntegrationTests
{
    [Fact(DisplayName = "Composed Route Create help retains exact grammar without workspace effects"),
     Trait("Feature", "route-create"), Trait("Evidence", "Integration")]
    public async Task HelpRetainsExactGrammarWithoutWorkspaceEffects()
    {
        using var workspace = TemporaryWorkspace.Create("create-help-policy");
        var before = workspace.SnapshotHashes();
        var missing = workspace.Combine("missing");
        var result = await CliHostCapture.RunAsync(["route", "create", "--help", "--workspace", missing], workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains("open-forge route create", result.Output, StringComparison.Ordinal);
        Assert.Contains("--dry-run", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("--automatic", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("--force", result.Output, StringComparison.Ordinal);
        Assert.Contains("open-forge route create <file-target> --description <text> --tag <tag>...", string.Join(" ", result.Output.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)), StringComparison.Ordinal);
        Assert.Contains("--description <text>", result.Output, StringComparison.Ordinal);
        Assert.Contains("--tag <tag>", result.Output, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missing));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Composed Route Create JSON dry-run keeps bounded diagnostics separate"),
     Trait("Feature", "route-create"), Trait("Evidence", "Integration")]
    public async Task JsonDryRunPreservesPrimaryDocumentWithVerboseDiagnostics()
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create("create-json-diagnostics");
        workspace.SeedBase();
        var before = workspace.SnapshotHashes();
        string[] arguments =
        [
            "route", "create", RouteCreateIntegrationWorkspace.TargetId,
            "--description", "Project overview", "--tag=Docs", "--tag=Overview", "--dry-run", "--json",
        ];

        var plain = await CliHostCapture.RunAsync(arguments, workspace.Workspace.LexicalRoot);
        var verbose = await CliHostCapture.RunAsync([.. arguments, "--verbose"], workspace.Workspace.LexicalRoot);

        Assert.Equal(0, plain.ExitCode);
        Assert.Equal(string.Empty, plain.Error);
        Assert.Equal(plain.ExitCode, verbose.ExitCode);
        Assert.Equal(plain.Output, verbose.Output);
        var diagnostic = Assert.Single(verbose.Error.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
        Assert.InRange(diagnostic.Length, 1, 4095);
        Assert.DoesNotContain('\r', diagnostic);
        Assert.DoesNotContain('\n', diagnostic);
        Assert.EndsWith(Environment.NewLine, verbose.Error, StringComparison.Ordinal);
        Assert.Contains("status=complete; mode=dry-run", diagnostic, StringComparison.Ordinal);
        Assert.Contains($"target={RouteCreateIntegrationWorkspace.TargetId}", diagnostic, StringComparison.Ordinal);
        Assert.Contains("effects=2", diagnostic, StringComparison.Ordinal);
        using var document = System.Text.Json.JsonDocument.Parse(plain.Output);
        var root = document.RootElement;
        Assert.Equal("current-directory", root.GetProperty("workspace").GetProperty("selectedBy").GetString());
        Assert.Equal([".agents/loader.md", ".agents/templates/_templates.md"],
            root.GetProperty("result").GetProperty("unchangedPaths").EnumerateArray().Select(path => path.GetString()));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Composed root owns one exact Route Create leaf and binding"), Trait("Feature", "route-create"), Trait("Evidence", "Integration")]
    public void ComposedRootOwnsExactRouteCreateLeafAndBinding()
    {
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "test"));
        var tree = CliCoreApplicationAccess.Tree(application);
        var route = Assert.Single(
            tree.Root.Subcommands,
            command => command.Name == "route");

        Assert.Equal(
            ["list", "inspect", "init", "create", "update", "move", "remove"],
            route.Subcommands.Select(command => command.Name));
        var create = route.Subcommands[3];
        var parse = tree.Parse(["route", "create"]);
        var selection = CliBindingSelector.Select(parse);

        Assert.Same(create, selection.Command);
        Assert.Equal(CliBindingSelectionState.Leaf, selection.State);
        var binding = Assert.IsType<ICliCommandBinding>(selection.Binding, exactMatch: false);
        Assert.Same(create, binding.Command);
        Assert.Same(binding, tree.FindBinding(create));
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
        Assert.Contains("Command help:", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("remove <source-reference>", group.StandardOutput, StringComparison.Ordinal);

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

    private sealed record RouteCreateCompositionRun(
        int ExitCode,
        CliSemanticStatus Status,
        string StandardOutput,
        string StandardError,
        bool LockInfrastructureExists);
}
