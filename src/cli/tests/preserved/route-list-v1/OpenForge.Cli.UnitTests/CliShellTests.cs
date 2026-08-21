using System.Reflection;
using OpenForge.Cli.Commands.Route.List;
using OpenForge.Cli.Definitions;
using OpenForge.Cli.Invocation;
using OpenForge.Cli.Parsing;
using OpenForge.Cli.Pipeline;
using OpenForge.Cli.Presentation;

namespace OpenForge.Cli.UnitTests;

public sealed class CliShellTests
{
    [Fact(DisplayName = "CLI shell rejects help and version together before terminal output"), Trait("Feature", "cli-shell"), Trait("Evidence", "Unit")]
    public async Task ShellRejectsTerminalConflict()
    {
        var result = await RunAsync("--help", "--version");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains("mutually exclusive", result.StandardError, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "CLI help treats other well-formed global flags as no-ops without workspace resolution"), Trait("Feature", "cli-shell"), Trait("Evidence", "Unit")]
    public async Task HelpSkipsWorkspaceAndOperationStages()
    {
        var result = await RunAsync(
            "--workspace",
            "\0invalid-workspace",
            "--json",
            "--view=compact",
            "--verbose",
            "--help");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Usage:", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("schemaVersion", result.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.StandardError);
    }

    [Fact(DisplayName = "CLI version treats other well-formed global flags as no-ops without workspace resolution"), Trait("Feature", "cli-shell"), Trait("Evidence", "Unit")]
    public async Task VersionSkipsWorkspaceAndOperationStages()
    {
        var result = await RunAsync(
            "--workspace",
            "missing-workspace",
            "--json",
            "--view=expanded",
            "--verbose",
            "--version");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(CliDefinitions.Process.Executable.Version, result.StandardOutput.Trim());
        Assert.Equal(string.Empty, result.StandardError);
    }

    [Fact(DisplayName = "CLI bare root shows root help with the retained route group"), Trait("Feature", "cli-shell"), Trait("Evidence", "Unit")]
    public async Task BareRootShowsHelp()
    {
        var result = await RunAsync();

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("open-forge <command> [options]", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("route", result.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.StandardError);
    }

    [Fact(DisplayName = "CLI root help contains every canonical global option and view spelling"), Trait("Feature", "cli-shell"), Trait("Evidence", "Unit")]
    public void RootHelpContainsCanonicalGlobalSyntax()
    {
        var help = CliRootHelp.Render([RouteListDefinitions.RouteCommand]);
        var options = CliDefinitions.Options;

        Assert.Contains(options.Workspace.Name, help, StringComparison.Ordinal);
        Assert.Contains(options.Json.Name, help, StringComparison.Ordinal);
        Assert.Contains(options.View.Name, help, StringComparison.Ordinal);
        Assert.Contains(options.Verbose.Name, help, StringComparison.Ordinal);
        Assert.Contains(options.Help.Name, help, StringComparison.Ordinal);
        Assert.Contains(options.Version.Name, help, StringComparison.Ordinal);
        Assert.Contains($"{options.View.Name}=<{options.View.ValueName}>", help, StringComparison.Ordinal);
        Assert.Contains(CliDefinitions.Views.Compact.Name, help, StringComparison.Ordinal);
        Assert.Contains(CliDefinitions.Views.Expanded.Name, help, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "CLI generated version matches assembly informational-version metadata"), Trait("Feature", "cli-shell"), Trait("Evidence", "Unit")]
    public void GeneratedVersionMatchesAssemblyMetadata()
    {
        var assembly = typeof(CliDefinitions).Assembly;
        var metadata = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        Assert.NotNull(metadata);
        Assert.Equal(metadata.InformationalVersion, CliDefinitions.Process.Executable.Version);
    }

    [Fact(DisplayName = "CLI global binding produces typed presentation and explicit option facts"), Trait("Feature", "cli-shell"), Trait("Evidence", "Unit")]
    public void GlobalBindingProducesTypedInput()
    {
        var parse = CliRootTree.Create().Parse(
            ["--workspace", "relative", "--json", "--view=compact", "--verbose", "--verbose"]);

        var resolution = CliGlobalInputResolver.Resolve(parse);

        var input = Assert.IsType<CliGlobalInput>(resolution.Input);
        Assert.Equal("relative", input.WorkspaceValue);
        Assert.Equal(1, input.WorkspaceOccurrences);
        Assert.Equal(CliOutputFormat.Json, input.Presentation.Format);
        Assert.Equal(CliView.Compact, input.Presentation.View);
        Assert.Equal(CliVerbosity.Verbose, input.Presentation.Verbosity);
        Assert.Equal(2, input.VerboseOccurrences);
    }

    [Fact(DisplayName = "CLI workspace-aware resolution normalizes explicit and current-directory selections"), Trait("Feature", "cli-shell"), Trait("Evidence", "Unit")]
    public void WorkspaceAwareResolutionNormalizesBothSelections()
    {
        var currentDirectory = Path.GetFullPath(Path.GetTempPath());
        var explicitInput = ReadGlobalInput("--workspace", "child");
        var defaultInput = ReadGlobalInput();

        var explicitResolution = CliInvocationResolver.Resolve(explicitInput, CliWorkspaceMode.Aware, currentDirectory);
        var defaultResolution = CliInvocationResolver.Resolve(defaultInput, CliWorkspaceMode.Aware, currentDirectory);

        var explicitWorkspace = Assert.IsType<CliWorkspace>(explicitResolution.Invocation?.Workspace);
        Assert.Equal(Path.GetFullPath("child", currentDirectory), explicitWorkspace.Path);
        Assert.Equal(CliWorkspaceSelection.ExplicitWorkspace, explicitWorkspace.SelectedBy);
        var defaultWorkspace = Assert.IsType<CliWorkspace>(defaultResolution.Invocation?.Workspace);
        Assert.Equal(currentDirectory, defaultWorkspace.Path);
        Assert.Equal(CliWorkspaceSelection.CurrentDirectory, defaultWorkspace.SelectedBy);
    }

    [Fact(DisplayName = "CLI no-workspace resolution carries genuine absence and ignores workspace text"), Trait("Feature", "cli-shell"), Trait("Evidence", "Unit")]
    public void NoWorkspaceResolutionCarriesAbsence()
    {
        var input = ReadGlobalInput("--workspace", "\0invalid-workspace");

        var resolution = CliInvocationResolver.Resolve(input, CliWorkspaceMode.None, "\0unused-current-directory");

        var invocation = Assert.IsType<CliInvocation>(resolution.Invocation);
        Assert.Null(invocation.Workspace);
        Assert.Null(resolution.InvalidInput);
    }

    [Fact(DisplayName = "CLI command paths and next actions are formed from typed definitions"), Trait("Feature", "cli-shell"), Trait("Evidence", "Unit")]
    public void CommandPathsUseDefinitions()
    {
        var path = CliCommandPath.From(CliDefinitions.Process.Executable);
        var next = new CliNextAction(path, "Show root help.");

        Assert.Equal(CliDefinitions.Root.Name, next.Command.Value);
        Assert.Equal("Show root help.", next.Reason);
    }

    [Theory(DisplayName = "CLI maps every semantic status to its fixed exit and human target"),
     InlineData((int)CliSemanticStatus.Complete, "complete", 0, (int)CliOutputTarget.StandardOutput),
     InlineData((int)CliSemanticStatus.Failed, "failed", 1, (int)CliOutputTarget.StandardError),
     InlineData((int)CliSemanticStatus.Attention, "attention", 2, (int)CliOutputTarget.StandardOutput),
     InlineData((int)CliSemanticStatus.Incomplete, "incomplete", 3, (int)CliOutputTarget.StandardOutput),
     InlineData((int)CliSemanticStatus.Invalid, "invalid", 4, (int)CliOutputTarget.StandardError),
     InlineData((int)CliSemanticStatus.Blocked, "blocked", 5, (int)CliOutputTarget.StandardError),
     InlineData((int)CliSemanticStatus.Interrupted, "interrupted", 130, (int)CliOutputTarget.StandardError),
     Trait("Feature", "cli-shell"), Trait("Evidence", "Unit")]
    public void StatusDefinitionsMapFixedDisposition(
        int statusValue,
        string machineName,
        int exitCode,
        int targetValue)
    {
        var status = (CliSemanticStatus)statusValue;
        var target = (CliOutputTarget)targetValue;
        var definition = CliStatusDefinitions.Read(status);

        Assert.Equal(machineName, definition.MachineName);
        Assert.Equal(exitCode, definition.Disposition.ExitCode);
        Assert.Equal(target, definition.Disposition.HumanOutputTarget);
    }

    [Fact(DisplayName = "CLI status definitions fail closed for unknown values"), Trait("Feature", "cli-shell"), Trait("Evidence", "Unit")]
    public void StatusDefinitionsRejectUnknownValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CliStatusDefinitions.Read((CliSemanticStatus)int.MaxValue));
    }

    private static CliGlobalInput ReadGlobalInput(params string[] arguments)
    {
        var parse = CliRootTree.Create().Parse(arguments);
        var resolution = CliGlobalInputResolver.Resolve(parse);
        return Assert.IsType<CliGlobalInput>(resolution.Input);
    }

    private static async Task<ApplicationResult> RunAsync(params string[] arguments)
    {
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        var writers = new CliOutputWriters(standardOutput, standardError);

        var exitCode = await CliApplication.RunAsync(arguments, writers, TestContext.Current.CancellationToken);

        return new ApplicationResult(exitCode, standardOutput.ToString(), standardError.ToString());
    }

    private sealed record ApplicationResult(int ExitCode, string StandardOutput, string StandardError);
}
