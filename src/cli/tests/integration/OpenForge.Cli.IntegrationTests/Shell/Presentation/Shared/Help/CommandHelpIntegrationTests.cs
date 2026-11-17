using OpenForge.Cli.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Shell.Presentation.Shared.Help;

public sealed class CommandHelpIntegrationTests
{
    public static TheoryData<string> Pages => new()
    {
        "", "route", "extension", "library", "tool",
        "tool list", "tool inspect", "tool status", "tool install", "tool update", "tool remove",
        "install", "update", "remove", "status", "context", "find", "references", "index", "doctor", "repair", "cleanup", "help",
        "route list", "route inspect", "route init", "route create", "route include", "route exclude", "route update", "route move", "route remove",
        "extension list", "extension inspect", "extension create", "extension install", "extension update", "extension remove",
        "library list", "library inspect", "library attach", "library sync", "library detach",
    };

    [Theory(DisplayName = "Every composed help page fits 80 columns and preserves the public help contract"), MemberData(nameof(Pages))]
    [Trait("Feature", "command-help"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task HelpPagesAt80Columns(string command)
    {
        using var workspace = TemporaryWorkspace.Create("command-help");
        var before = workspace.SnapshotHashes();
        var missing = workspace.Combine("missing");
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var arguments = command.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Concat([CliSyntaxDefinitions.Help.Name, CliSyntaxDefinitions.Workspace.Name, missing]).ToArray();
        var application = CliCompositionRoot.Create(new CliProcessIdentity(CliSyntaxDefinitions.ExecutableName, "test"));

        var completion = await application.RunAsync(arguments, new CliProcessEnvironment(workspace.Path, helpWidth: 80),
            new CliOutputWriters(stdout, stderr), TestContext.Current.CancellationToken);

        var text = stdout.ToString();
        Assert.Equal(0, completion.ExitCode);
        Assert.Empty(stderr.ToString());
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(Directory.Exists(missing));
        Assert.All(text.Split(Environment.NewLine), line =>
            Assert.True(line.Length <= 80 || line.TrimStart().StartsWith($"{CliSyntaxDefinitions.ExecutableName} ", StringComparison.Ordinal)
                || line.Split(' ').Any(word => word.Length > 78), $"Line exceeds 80 columns: {line}"));

        CommandOutputSnapshot.MatchSnapshot(text, command.Length == 0 ? "root" : command.Replace(' ', '-'),
            testName: nameof(CommandHelpIntegrationTests));
    }
}
