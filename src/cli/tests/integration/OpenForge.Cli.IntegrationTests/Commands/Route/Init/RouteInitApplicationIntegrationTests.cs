using System.CommandLine;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Parsing.Models.Results;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Composition;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init;

public sealed class RouteInitApplicationIntegrationTests
{
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
        Assert.Contains("Omit --dry-run to apply the complete preflighted plan.", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("--template", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("--yes", result.Output, StringComparison.Ordinal);
        Assert.Contains("complete: exit 0 and human stdout.", result.Output, StringComparison.Ordinal);
        Assert.Contains("attention: exit 2 and human stdout.", result.Output, StringComparison.Ordinal);
        Assert.Contains("incomplete: exit 3 and human stdout.", result.Output, StringComparison.Ordinal);
        Assert.Contains("invalid: exit 4 and human stderr.", result.Output, StringComparison.Ordinal);
        Assert.Contains("blocked: exit 5 and human stderr.", result.Output, StringComparison.Ordinal);
        Assert.Contains("failed: exit 1 and human stderr.", result.Output, StringComparison.Ordinal);
        Assert.Contains("interrupted: exit 130 and human stderr.", result.Output, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missing));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Composed Route Init JSON dry-run keeps bounded diagnostics separate"),
     Trait("Feature", "route-init"), Trait("Evidence", "Integration")]
    public async Task JsonDryRunPreservesPrimaryDocumentWithVerboseDiagnostics()
    {
        using var workspace = TemporaryWorkspace.Create("init-json-diagnostics");
        var before = workspace.SnapshotHashes();
        string[] arguments =
        [
            "route", "init", "docs", "--description", "Project documents",
            "--tag=Documentation", "--dry-run", "--json",
        ];

        var plain = await CliHostCapture.RunAsync(arguments, workspace.Path);
        var verbose = await CliHostCapture.RunAsync([.. arguments, "--verbose"], workspace.Path);

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
        Assert.Equal(before, workspace.SnapshotHashes());
    }

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

    [Fact(DisplayName = "Composed Route parser accepts equals tag values and rejects separated tag values"), Trait("Feature", "route-init-presentation"), Trait("Evidence", "Integration")]
    public void ComposedRouteParserUsesTheFrozenTagDelimiter()
    {
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "test"));
        var tree = CliCoreApplicationAccess.Tree(application);
        var equals = tree.Parse(
            ["route", "init", "memory/project-alpha/documents", "--tag=Memory"]);
        var equalsSelection = CliBindingSelector.Select(equals);

        Assert.Equal(
            ["--depth", "--tag"],
            equals.DelimiterPolicies.Select(policy => policy.OptionName));
        Assert.Equal(
            1,
            equals.DelimiterPolicies.Count(policy => policy.OptionName == "--tag"));
        Assert.Equal(CliBindingSelectionState.Leaf, equalsSelection.State);
        Assert.NotNull(equalsSelection.Binding);
        Assert.Null(CliTerminalValidator.Validate(equals).InvalidInput);
        var tag = Assert.Single(equalsSelection.Command.Options.OfType<Option<string[]>>());
        var tagValues = Assert.IsType<string[]>(equals.Result.GetValue(tag));
        Assert.Equal(["Memory"], tagValues);

        var separated = tree.Parse(
            ["route", "init", "memory/project-alpha/documents", "--tag", "Memory"]);
        var separatedResolution = CliTerminalValidator.Validate(separated);

        var invalid = Assert.IsType<CliInvalidInput>(separatedResolution.InvalidInput);
        Assert.Equal("cli.delimiter.invalid", invalid.Code);
        Assert.Equal(CliInvalidInputSource.Delimiter, invalid.Source);
    }

    [Fact(DisplayName = "Composed root help exposes Route Init exactly once in Discovery"), Trait("Feature", "route-init-presentation"), Trait("Evidence", "Integration")]
    public async Task ComposedRootHelpExposesRouteInitOnceInDiscovery()
    {
        using var workspace = TemporaryWorkspace.Create("route-init-composed-help");

        var result = await CliHostCapture.RunAsync(
            ["--help"],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        var discovery = result.Output
            .Split(Environment.NewLine, StringSplitOptions.None)
            .SkipWhile(line => !line.Equals("Discovery:", StringComparison.Ordinal))
            .Skip(1)
            .TakeWhile(line => line.StartsWith("  ", StringComparison.Ordinal))
            .Where(line => line.TrimStart().StartsWith("route init", StringComparison.Ordinal))
            .ToArray();
        Assert.Single(discovery);
    }

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
