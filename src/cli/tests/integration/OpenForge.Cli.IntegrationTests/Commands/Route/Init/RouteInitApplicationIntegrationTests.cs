using System.Text.Json;
using System.CommandLine;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.Results;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Composition;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init;

public sealed class RouteInitApplicationIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Route Init help retains exact shared exit and stream policy"),
     Trait("Feature", "route-init"), Trait("Evidence", "Integration")]
    public async Task HelpRetainsExactSharedExitAndStreamPolicy()
    {
        using var workspace = TemporaryWorkspace.Create("init-help-policy");
        var before = workspace.SnapshotHashes();
        var missing = workspace.Combine("missing");
        var result = await CliHostCapture.RunAsync(["route", "init", "--help", "--workspace", missing], workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains("open-forge route init", result.Output, StringComparison.Ordinal);
        Assert.Contains("--dry-run", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("--automatic", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("--force", result.Output, StringComparison.Ordinal);
        Assert.Contains("[--framework]", result.Output, StringComparison.Ordinal);
        Assert.Contains("Omit --dry-run to apply the complete checked plan.", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("--template", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("--yes", result.Output, StringComparison.Ordinal);
        Assert.Contains("completed: exit 0 and text stdout.", result.Output, StringComparison.Ordinal);
        Assert.Contains("completed-with-warnings: exit 2 and text stdout.", result.Output, StringComparison.Ordinal);
        Assert.Contains("incomplete: exit 3 and text stdout.", result.Output, StringComparison.Ordinal);
        Assert.Contains("invalid-input: exit 4 and text stderr.", result.Output, StringComparison.Ordinal);
        Assert.Contains("blocked: exit 5 and text stderr.", result.Output, StringComparison.Ordinal);
        Assert.Contains("failed: exit 1 and text stderr.", result.Output, StringComparison.Ordinal);
        Assert.Contains("cancelled: exit 130 and text stderr.", result.Output, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missing));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Route Init JSON dry-run keeps bounded diagnostics separate"),
     Trait("Feature", "route-init"), Trait("Evidence", "Integration")]
    public async Task JsonDryRunPreservesPrimaryDocumentWithVerboseDiagnostics()
    {
        using var workspace = TemporaryWorkspace.Create("init-json-diagnostics");
        var before = workspace.SnapshotHashes();
        string[] arguments =
        [
            "route", "init", "docs", "--description", "Project documents",
            "--tag=Documentation", "--dry-run", "--format", "json",
        ];

        var plain = await CliHostCapture.RunAsync(arguments, workspace.Path);
        var verbose = await CliHostCapture.RunAsync([.. arguments, "--detail", "debug"], workspace.Path);

        Assert.Equal(0, plain.ExitCode);
        Assert.Equal(string.Empty, plain.Error);
        Assert.Equal(plain.ExitCode, verbose.ExitCode);
        using var plainDocument = JsonDocument.Parse(plain.Output);
        using var verboseDocument = JsonDocument.Parse(verbose.Output);
        Assert.Equal(
            plainDocument.RootElement.GetProperty("schemaVersion").GetInt32(),
            verboseDocument.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(
            plainDocument.RootElement.GetProperty("command").GetString(),
            verboseDocument.RootElement.GetProperty("command").GetString());
        Assert.Equal(
            plainDocument.RootElement.GetProperty("status").GetString(),
            verboseDocument.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            plainDocument.RootElement.GetProperty("data").GetProperty("mode").GetString(),
            verboseDocument.RootElement.GetProperty("data").GetProperty("mode").GetString());
        Assert.Equal(
            plainDocument.RootElement.GetProperty("data").GetProperty("target").GetRawText(),
            verboseDocument.RootElement.GetProperty("data").GetProperty("target").GetRawText());
        Assert.Equal(
            plainDocument.RootElement.GetProperty("data").GetProperty("scaffold").GetString(),
            verboseDocument.RootElement.GetProperty("data").GetProperty("scaffold").GetString());
        var plainEntrypoints = plainDocument.RootElement.GetProperty("data").GetProperty("entrypoints").EnumerateArray().ToArray();
        var verboseEntrypoints = verboseDocument.RootElement.GetProperty("data").GetProperty("entrypoints").EnumerateArray().ToArray();
        Assert.Equal(plainEntrypoints.Length, verboseEntrypoints.Length);
        for (var index = 0; index < plainEntrypoints.Length; index++)
        {
            Assert.Equal(
                plainEntrypoints[index].GetProperty("path").GetString(),
                verboseEntrypoints[index].GetProperty("path").GetString());
            Assert.Equal(
                plainEntrypoints[index].GetProperty("outcome").GetString(),
                verboseEntrypoints[index].GetProperty("outcome").GetString());
            Assert.Equal(
                plainEntrypoints[index].GetProperty("needsAuthoring").GetBoolean(),
                verboseEntrypoints[index].GetProperty("needsAuthoring").GetBoolean());
        }
        Assert.Equal(
            plainDocument.RootElement.GetProperty("data").GetProperty("listedIn").GetRawText(),
            verboseDocument.RootElement.GetProperty("data").GetProperty("listedIn").GetRawText());
        var diagnostics = verbose.Error.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(10, diagnostics.Length);
        Assert.InRange(verbose.Error.Length, 1, 4096);
        Assert.All(diagnostics, diagnostic =>
        {
            Assert.InRange(diagnostic.Length, 1, 240);
            Assert.DoesNotContain('\r', diagnostic);
            Assert.DoesNotContain('\n', diagnostic);
        });
        Assert.EndsWith(Environment.NewLine, verbose.Error, StringComparison.Ordinal);
        Assert.Equal("status=completed", diagnostics[0]);
        Assert.Equal("mode=dry-run", diagnostics[1]);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed root registers Route Init as one nested route leaf"), Trait("Feature", "route-init-presentation"), Trait("Evidence", "Integration")]
    public void ComposedRootRegistersOneNestedRouteInitLeaf()
    {
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "test"));
        var tree = CliCoreApplicationAccess.Tree(application);
        var parse = tree.Parse(["route", "init"]);
        var selection = CliBindingSelector.Select(parse);

        Assert.Equal("init", selection.Command.Name);
        Assert.Equal(CliBindingSelectionState.Leaf, selection.State);
        Assert.False(tree.IsGroup(selection.Command));
        var binding = selection.Binding;
        Assert.NotNull(binding);
        Assert.Same(selection.Command, binding.Command);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Route parser accepts equivalent equals and separated tag values"), Trait("Feature", "route-init-presentation"), Trait("Evidence", "Integration")]
    public void ComposedRouteParserUsesNativeTagDelimiters()
    {
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "test"));
        var tree = CliCoreApplicationAccess.Tree(application);
        var equals = tree.Parse(
            ["route", "init", "memory/project-alpha/documents", "--tag=Memory"]);
        var equalsSelection = CliBindingSelector.Select(equals);

        Assert.Equal(CliBindingSelectionState.Leaf, equalsSelection.State);
        Assert.NotNull(equalsSelection.Binding);
        Assert.Null(CliTerminalValidator.Validate(equals).InvalidInput);
        var tag = Assert.Single(equalsSelection.Command.Options.OfType<Option<string[]>>());
        var tagValues = Assert.IsType<string[]>(equals.Result.GetValue(tag));
        Assert.Equal(["Memory"], tagValues);

        var separated = tree.Parse(
            ["route", "init", "memory/project-alpha/documents", "--tag", "Memory"]);
        var separatedResolution = CliTerminalValidator.Validate(separated);

        Assert.Null(separatedResolution.InvalidInput);
        Assert.Equal(tagValues, separated.Result.GetValue(tag));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed route help exposes Route Init exactly once in Commands"), Trait("Feature", "route-init-presentation"), Trait("Evidence", "Integration")]
    public async Task ComposedRouteHelpExposesRouteInitOnceInCommands()
    {
        using var workspace = TemporaryWorkspace.Create("route-init-composed-help");

        var result = await CliHostCapture.RunAsync(
            ["route", "--help"],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        var discovery = result.Output
            .Split(Environment.NewLine, StringSplitOptions.None)
            .SkipWhile(line => !line.Equals("Commands:", StringComparison.Ordinal))
            .Skip(1)
            .TakeWhile(line => line.StartsWith("  ", StringComparison.Ordinal))
            .Where(line => line.TrimStart().StartsWith("init ", StringComparison.Ordinal))
            .ToArray();
        Assert.Single(discovery);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Route Init help comes from its registered binding without workspace effects"), Trait("Feature", "route-init-presentation"), Trait("Evidence", "Integration")]
    public async Task ComposedRouteInitHelpComesFromRegisteredBinding()
    {
        using var workspace = TemporaryWorkspace.Create("route-init-leaf-help");

        var result = await CliHostCapture.RunAsync(
            ["route", "init", "--help"],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains("open-forge route init", result.Output, StringComparison.Ordinal);
        Assert.Contains("Scaffold mode", result.Output, StringComparison.Ordinal);
        Assert.Contains("Results and streams", result.Output, StringComparison.Ordinal);
    }
}
