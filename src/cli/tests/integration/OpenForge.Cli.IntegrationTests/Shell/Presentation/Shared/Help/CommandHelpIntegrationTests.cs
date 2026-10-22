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
        "", "route", "extension", "library",
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
            .Concat(["--help", "--workspace", missing]).ToArray();
        var application = CliCompositionRoot.Create(new CliProcessIdentity("open-forge", "test"));

        var completion = await application.RunAsync(arguments, new CliProcessEnvironment(workspace.Path, helpWidth: 80),
            new CliOutputWriters(stdout, stderr), TestContext.Current.CancellationToken);

        var text = stdout.ToString();
        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(string.Empty, stderr.ToString());
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(Directory.Exists(missing));
        Assert.Single(text.Split(Environment.NewLine), line => line == "Usage:");
        Assert.InRange(text.Split(Environment.NewLine).Count(line => line == "Options:"), 0, 1);
        Assert.All(text.Split(Environment.NewLine), line =>
            Assert.True(line.Length <= 80 || line.TrimStart().StartsWith("open-forge ", StringComparison.Ordinal)
                || line.Split(' ').Any(word => word.Length > 78), $"Line exceeds 80 columns: {line}"));

        if (command.Length > 0 && command is not "route" and not "extension" and not "library")
        {
            var headings = new[] { "Usage:", "Examples:", "Behavior:", "Global options:", "Automation:", "Related commands:" };
            var positions = headings.Select(heading => text.IndexOf(heading, StringComparison.Ordinal)).ToArray();
            Assert.All(positions, position => Assert.True(position >= 0));
            Assert.Equal(positions.Order(), positions);
            var examples = text[(positions[1] + "Examples:".Length)..positions[2]]
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.InRange(examples.Length, 2, 3);
            Assert.All(examples, line => Assert.StartsWith("  open-forge ", line, StringComparison.Ordinal));
            Assert.DoesNotContain("--workspace <", text, StringComparison.Ordinal);
            Assert.DoesNotContain("--detail <", text, StringComparison.Ordinal);
            Assert.DoesNotContain("|", text[positions[^2]..positions[^1]], StringComparison.Ordinal);
            Assert.Contains("See open-forge --help.", text, StringComparison.Ordinal);
            Assert.InRange(text[positions[^2]..positions[^1]].TrimEnd().Split(Environment.NewLine).Length, 4, 5);
        }
        else if (command.Length == 0)
        {
            var headings = new[] { "Usage:", "Get started in your project folder:", "Set up and manage:", "Read and discover:", "Check and maintain:", "Global options:", "Use open-forge <command> --help" };
            var positions = headings.Select(heading => text.IndexOf(heading, StringComparison.Ordinal)).ToArray();
            Assert.All(positions, position => Assert.True(position >= 0));
            Assert.Equal(positions.Order(), positions);
            Assert.DoesNotContain("Managed content:", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Command help:", text, StringComparison.Ordinal);
            Assert.Single(text.Split(Environment.NewLine), line => line.Contains("--workspace <path>", StringComparison.Ordinal));
            Assert.Contains("  open-forge install --preset minimal --dry-run  Preview Minimal, no edits", text, StringComparison.Ordinal);
        }

        CommandOutputSnapshot.MatchSnapshot(text, command.Length == 0 ? "root" : command.Replace(' ', '-'),
            testName: nameof(CommandHelpIntegrationTests));
    }
}
