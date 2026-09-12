using OpenForge.Cli.TestSupport;
using System.Text.Json;
using OpenForge.Cli.Hosting;
using OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Shared.Profile;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect;

public sealed class RouteInspectApplicationIntegrationTests
{
    [Fact(DisplayName = "Composed Route Inspect preserves a filesystem-safe Unicode and space source argument"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task FilesystemSafeSourceArgumentRemainsExact()
    {
        using var workspace = CompleteWorkspace();
        const string path = ".agents/root/space value-東京-😀.md";
        workspace.WriteRoutedMarkdown(path, "Exact source", ["Route"], "# Exact source\n");
        var before = workspace.Snapshot();
        var response = await CliHostCapture.RunAsync(["route", "inspect", path, "--workspace", workspace.Path, "--json"], workspace.Path);

        Assert.Equal(0, response.ExitCode);
        Assert.Equal(string.Empty, response.Error);
        using var document = JsonDocument.Parse(response.Output);
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        var result = document.RootElement.GetProperty("result");
        Assert.Equal(path, result.GetProperty("selection").GetProperty("requestedReference").GetString());
        Assert.Equal(path, result.GetProperty("identity").GetProperty("path").GetString());
        var after = workspace.Snapshot();
        Assert.Equal(before.FileHashes, after.FileHashes);
        Assert.Equal(before.Entries, after.Entries);
    }

    [Fact(DisplayName = "CLI Route Inspect help exposes the composed Inspect leaf"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task InspectHelpExposesComposedLeaf()
    {
        using var workspace = RouteInspectProfileIntegrationWorkspace.Create();

        var leaf = await CliHostCapture.RunAsync(["route", "inspect", "--help"], workspace.Path);

        Assert.Equal(0, leaf.ExitCode);
        Assert.Equal(string.Empty, leaf.Error);
        Assert.Contains(
            "open-forge route inspect <source-reference>",
            leaf.Output,
            StringComparison.Ordinal);
        Assert.Contains("--workspace <path>", leaf.Output, StringComparison.Ordinal);
        Assert.Contains("--json", leaf.Output, StringComparison.Ordinal);
        Assert.Contains("route list", leaf.Output, StringComparison.Ordinal);
        Assert.Contains("Related commands", leaf.Output, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "CLI Route Inspect renders complete real workspace human views without writes"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task CompleteWorkspaceRendersHumanViewsWithoutWriting()
    {
        using var workspace = CompleteWorkspace();
        var before = workspace.Snapshot();

        var compact = await CliHostCapture.RunAsync(
            ["route", "inspect", "root", "--workspace", workspace.Path, "--view=compact"],
            workspace.Path);
        var expanded = await CliHostCapture.RunAsync(
            ["route", "inspect", "root", "--workspace", workspace.Path, "--view=expanded"],
            workspace.Path);

        Assert.Equal(0, compact.ExitCode);
        Assert.Equal(string.Empty, compact.Error);
        Assert.Contains("Route: root", compact.Output, StringComparison.Ordinal);
        Assert.Contains(".agents/root/_root.md", compact.Output, StringComparison.Ordinal);
        Assert.Contains("Status: complete", compact.Output, StringComparison.Ordinal);
        Assert.Contains("Own source:", compact.Output, StringComparison.Ordinal);
        Assert.Equal(0, expanded.ExitCode);
        Assert.Equal(string.Empty, expanded.Error);
        Assert.Contains("Axioms", expanded.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("loader", expanded.Output, StringComparison.Ordinal);

        var after = workspace.Snapshot();
        Assert.Equal(before.FileHashes, after.FileHashes);
        Assert.Equal(before.Entries, after.Entries);
    }

    [Fact(DisplayName = "Route Inspect expanded human verbosity preserves primary bytes and bounds diagnostics without writes"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task ExpandedHumanVerbosePreservesPrimaryResult()
    {
        using var workspace = CompleteWorkspace();
        var before = workspace.Snapshot();
        string[] arguments = ["route", "inspect", "root", "--workspace", workspace.Path, "--view=expanded"];

        var plain = await CliHostCapture.RunAsync(arguments, workspace.Path);
        var afterPlain = workspace.Snapshot();
        Assert.Equal(before.FileHashes, afterPlain.FileHashes);
        Assert.Equal(before.Entries, afterPlain.Entries);
        var verbose = await CliHostCapture.RunAsync([.. arguments, "--verbose"], workspace.Path);

        Assert.Equal(0, plain.ExitCode);
        Assert.Equal(plain.ExitCode, verbose.ExitCode);
        Assert.Equal(plain.Output, verbose.Output);
        Assert.Equal(string.Empty, plain.Error);
        Assert.InRange(verbose.Error.Length, 1, 4096);
        var afterVerbose = workspace.Snapshot();
        Assert.Equal(before.FileHashes, afterVerbose.FileHashes);
        Assert.Equal(before.Entries, afterVerbose.Entries);
    }

    [Fact(DisplayName = "CLI Route Inspect JSON view is stable across view selection and keeps diagnostics on stderr"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task JsonViewsRetainCoreAndVerboseDiagnosticsStaySeparate()
    {
        using var workspace = CompleteWorkspace();
        var before = workspace.Snapshot();

        var json = await CliHostCapture.RunAsync(
            ["route", "inspect", "root", "--workspace", workspace.Path, "--json", "--view=compact"],
            workspace.Path);
        var expandedJson = await CliHostCapture.RunAsync(
            ["route", "inspect", "root", "--workspace", workspace.Path, "--json", "--view=expanded"],
            workspace.Path);
        var verboseJson = await CliHostCapture.RunAsync(
            ["route", "inspect", "root", "--workspace", workspace.Path, "--json", "--verbose"],
            workspace.Path);

        Assert.Equal(0, json.ExitCode);
        Assert.Equal(string.Empty, json.Error);
        Assert.Equal(0, expandedJson.ExitCode);
        Assert.Equal(string.Empty, expandedJson.Error);
        Assert.Equal(0, verboseJson.ExitCode);
        Assert.True(JsonViewComparison.RetainsResult(json.Output, expandedJson.Output));
        Assert.Equal(expandedJson.Output, verboseJson.Output);
        Assert.Contains("status=complete", verboseJson.Error, StringComparison.Ordinal);
        Assert.DoesNotContain("Open Forge route inspect", json.Output, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(json.Output);
        Assert.Equal("route inspect", document.RootElement.GetProperty("command").GetString());
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            "root",
            document.RootElement.GetProperty("result").GetProperty("selection")
                .GetProperty("requestedReference").GetString());
        Assert.Equal(
            ".agents/root/_root.md",
            document.RootElement.GetProperty("result").GetProperty("identity")
                .GetProperty("path").GetString());

        var after = workspace.Snapshot();
        Assert.Equal(before.FileHashes, after.FileHashes);
        Assert.Equal(before.Entries, after.Entries);
    }

    [Theory(DisplayName = "CLI Route Inspect preserves native workspace and view delimiter parity"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration"),
     InlineData("spaced", "spaced"),
     InlineData("spaced", "equals"),
     InlineData("spaced", "colon"),
     InlineData("equals", "spaced"),
     InlineData("equals", "equals"),
     InlineData("equals", "colon"),
     InlineData("colon", "spaced"),
     InlineData("colon", "equals"),
     InlineData("colon", "colon")]
    public static async Task NativeGlobalFormsProduceIdenticalCompleteResults(
        string workspaceForm,
        string viewForm)
    {
        using var workspace = CompleteWorkspace();
        var before = workspace.Snapshot();
        var baseline = await CliHostCapture.RunAsync(
            ["route", "inspect", "root", "--workspace", workspace.Path, "--view=compact"],
            workspace.Path);
        Assert.Equal(0, baseline.ExitCode);
        Assert.Equal(string.Empty, baseline.Error);

        var arguments = new List<string> { "route", "inspect", "root" };
        AddScalar(arguments, "--workspace", workspace.Path, workspaceForm);
        AddScalar(arguments, "--view", "compact", viewForm);
        var result = await CliHostCapture.RunAsync([.. arguments], workspace.Path);

        Assert.Equal(baseline.ExitCode, result.ExitCode);
        Assert.Equal(baseline.Output, result.Output);
        Assert.Equal(baseline.Error, result.Error);

        var after = workspace.Snapshot();
        Assert.Equal(before.FileHashes, after.FileHashes);
        Assert.Equal(before.Entries, after.Entries);
    }

    [Fact(DisplayName = "CLI Route Inspect missing domain operand produces a typed invalid JSON result"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task MissingDomainOperandProducesTypedInvalidJson()
    {
        using var workspace = CompleteWorkspace();

        var missing = await CliHostCapture.RunAsync(
            ["route", "inspect", "--workspace", workspace.Path, "--json"],
            workspace.Path);

        Assert.Equal(4, missing.ExitCode);
        Assert.Equal(string.Empty, missing.Error);
        using var missingDocument = JsonDocument.Parse(missing.Output);
        AssertInvalidJson(missingDocument, "route-inspect.missing-source");
    }

    [Fact(DisplayName = "CLI Route Inspect multiple domain operands produce a typed invalid JSON result"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task MultipleDomainOperandsProduceTypedInvalidJson()
    {
        using var workspace = CompleteWorkspace();

        var multiple = await CliHostCapture.RunAsync(
            ["route", "inspect", "first", "second", "--workspace", workspace.Path, "--json"],
            workspace.Path);

        Assert.Equal(4, multiple.ExitCode);
        Assert.Equal(string.Empty, multiple.Error);
        using var multipleDocument = JsonDocument.Parse(multiple.Output);
        AssertInvalidJson(multipleDocument, "route-inspect.multiple-sources");
    }

    [Fact(DisplayName = "CLI Route Inspect preserves an option-like source operand after the option terminator"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task OptionLikeOperandAfterTerminatorRemainsSourceReference()
    {
        using var workspace = CompleteWorkspace();

        var optionLike = await CliHostCapture.RunAsync(
            ["route", "inspect", "--workspace", workspace.Path, "--json", "--", "--view"],
            workspace.Path);

        Assert.Equal(4, optionLike.ExitCode);
        Assert.Equal(string.Empty, optionLike.Error);
        using var optionLikeDocument = JsonDocument.Parse(optionLike.Output);
        AssertInvalidJson(optionLikeDocument, "route-inspect.unknown-source");
        Assert.Equal(
            "--view",
            optionLikeDocument.RootElement.GetProperty("result").GetProperty("selection")
                .GetProperty("requestedReference").GetString());
    }

    [Fact(DisplayName = "CLI Route Inspect contextual workspace failure retains the requested source in JSON"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task InvalidWorkspaceRetainsTypedRequestedSource()
    {
        var missing = Path.Combine(
            Path.GetTempPath(),
            $"open-forge-route-inspect-missing-{Guid.NewGuid():N}");
        var result = await CliHostCapture.RunAsync(
            ["route", "inspect", "requested-source", "--workspace", missing, "--json"],
            missing);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("workspace").ValueKind);
        Assert.Equal(
            JsonValueKind.Null,
            document.RootElement.GetProperty("result").GetProperty("identity").ValueKind);
        Assert.Equal(
            JsonValueKind.Null,
            document.RootElement.GetProperty("result").GetProperty("profile").ValueKind);
        Assert.Equal(
            "requested-source",
            document.RootElement.GetProperty("result").GetProperty("selection")
                .GetProperty("requestedReference").GetString());
        Assert.Equal(
            "route-inspect.invalid-workspace",
            document.RootElement.GetProperty("result").GetProperty("conditions")[0]
                .GetProperty("code").GetString());
        Assert.False(Directory.Exists(missing));
    }

    [Fact(DisplayName = "CLI Route Inspect help bypasses workspace selection"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task InspectHelpBypassesWorkspaceSelection()
    {
        var missing = Path.Combine(
            Path.GetTempPath(),
            $"open-forge-route-inspect-terminal-{Guid.NewGuid():N}");
        var help = await CliHostCapture.RunAsync(
            [
                "route",
                "inspect",
                "--workspace",
                missing,
                "--json",
                "--view=compact",
                "--verbose",
                "--help",
            ],
            missing);

        Assert.Equal(0, help.ExitCode);
        Assert.Equal(string.Empty, help.Error);
        Assert.Contains("open-forge route inspect <source-reference>", help.Output, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missing));
    }

    [Fact(DisplayName = "CLI Route Inspect version bypasses workspace selection"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task InspectVersionBypassesWorkspaceSelection()
    {
        var missing = Path.Combine(
            Path.GetTempPath(),
            $"open-forge-route-inspect-version-{Guid.NewGuid():N}");
        var version = await CliHostCapture.RunAsync(
            [
                "route",
                "inspect",
                "--workspace",
                missing,
                "--json",
                "--view=compact",
                "--verbose",
                "--version",
            ],
            missing);

        Assert.Equal(0, version.ExitCode);
        Assert.Equal($"{CliBuildVersion.InformationalVersion}{Environment.NewLine}", version.Output);
        Assert.Equal(string.Empty, version.Error);
        Assert.False(Directory.Exists(missing));
    }

    [Theory(DisplayName = "CLI Route Inspect terminal modes reject source input before workspace selection"),
     InlineData("--help", "root"),
     InlineData("--version", "root"),
     InlineData("--help", "option-like"),
     InlineData("--version", "option-like"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public static async Task TerminalModesRejectSourceInputBeforeWorkspaceSelection(
        string terminalMode,
        string sourceKind)
    {
        var missingWorkspace = Path.Combine(
            Path.GetTempPath(),
            $"open-forge-route-inspect-terminal-conflict-{Guid.NewGuid():N}");
        var arguments = new List<string>
        {
            "route",
            "inspect",
            "--workspace",
            missingWorkspace,
            "--json",
            "--view=compact",
            "--verbose",
            terminalMode,
        };
        switch (sourceKind)
        {
            case "root":
                arguments.Add("root");
                break;
            case "option-like":
                arguments.Add("--");
                arguments.Add("--view");
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(sourceKind),
                    sourceKind,
                    "The terminal source case is not defined.");
        }

        var result = await CliHostCapture.RunAsync([.. arguments], missingWorkspace);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Output);
        var diagnostic = Assert.Single(
            result.Error.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
        Assert.InRange(diagnostic.Length, 1, 4096);
        Assert.False(Directory.Exists(missingWorkspace));
    }

    [Fact(DisplayName = "CLI Route Inspect exact-path identity collision reports attention without an invented next action"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task ExactPathCollisionReportsAttentionWithoutNextAction()
    {
        using var workspace = CollisionWorkspace();

        var exactPath = await CliHostCapture.RunAsync(
            ["route", "inspect", ".agents/root/collision.md", "--workspace", workspace.Path, "--view=compact"],
            workspace.Path);

        Assert.Equal(2, exactPath.ExitCode);
        Assert.Equal(string.Empty, exactPath.Error);
        Assert.Contains("Status: requires attention", exactPath.Output, StringComparison.Ordinal);
        Assert.Contains("Note:", exactPath.Output, StringComparison.Ordinal);
        Assert.Contains("not unique", exactPath.Output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Next:", exactPath.Output, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "CLI Route Inspect unresolved identity collision retains candidates and its exact-path next action"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task UnresolvedCollisionReturnsBlockedJsonWithCandidatesAndNext()
    {
        using var workspace = CollisionWorkspace();

        var blocked = await CliHostCapture.RunAsync(
            ["route", "inspect", "root/collision", "--workspace", workspace.Path, "--json"],
            workspace.Path);

        Assert.Equal(5, blocked.ExitCode);
        Assert.Equal(string.Empty, blocked.Error);
        using var blockedDocument = JsonDocument.Parse(blocked.Output);
        Assert.Equal("blocked", blockedDocument.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            [".agents/root/collision.md", ".agents/root/collision/_collision.md"],
            blockedDocument.RootElement.GetProperty("result").GetProperty("selection")
                .GetProperty("candidatePaths").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(
            "open-forge route inspect \".agents/root/collision.md\"",
            blockedDocument.RootElement.GetProperty("next").GetProperty("command").GetString());
        Assert.Equal(
            "Rerun with one listed exact path to resolve the source collision.",
            blockedDocument.RootElement.GetProperty("next").GetProperty("reason").GetString());
    }

    [Fact(DisplayName = "CLI Route Inspect unavailable real fact reports incomplete status and its doctor next action"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task UnavailableFactReturnsIncompleteWithDoctorNextAction()
    {
        using var workspace = IncompleteWorkspace();

        var incompleteResult = await CliHostCapture.RunAsync(
            ["route", "inspect", "root", "--workspace", workspace.Path, "--view=compact"],
            workspace.Path);

        Assert.Equal(3, incompleteResult.ExitCode);
        Assert.Equal(string.Empty, incompleteResult.Error);
        Assert.Contains("Status: incomplete", incompleteResult.Output, StringComparison.Ordinal);
        Assert.Contains("unavailable", incompleteResult.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Next: open-forge doctor", incompleteResult.Output, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "CLI Route Inspect unknown source reports invalid status on stderr with its correction next action"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task UnknownSourceReturnsInvalidOnStderrWithNextAction()
    {
        using var workspace = CollisionWorkspace();

        var invalid = await CliHostCapture.RunAsync(
            ["route", "inspect", "unknown", "--workspace", workspace.Path, "--view=compact"],
            workspace.Path);

        Assert.Equal(4, invalid.ExitCode);
        Assert.Empty(invalid.Output);
        Assert.Contains("Next: open-forge route inspect --help", invalid.Error, StringComparison.Ordinal);
    }

    private static RouteInspectProfileIntegrationWorkspace CompleteWorkspace(
        bool includeMalformedLoadNowSource = false)
    {
        var workspace = RouteInspectProfileIntegrationWorkspace.Create();
        workspace.WriteLoader(
            [RouteInspectProfileIntegrationWorkspace.Entry("Root", "root/_root.md", "LoadNow", "Root")],
            "- Loader rules are inherited.");
        workspace.WriteEntrypoint(new RouteInspectProfileIntegrationEntrypoint
        {
            RelativePath = ".agents/root/_root.md",
            Description = "Root",
            Tags = ["Root", "LoadNow"],
            Axioms = "- Root rules are local.",
            Entries = includeMalformedLoadNowSource
                ?
                [
                    RouteInspectProfileIntegrationWorkspace.Entry("Child", "child.md", "Route"),
                    RouteInspectProfileIntegrationWorkspace.Entry("Malformed", "malformed.md", "LoadNow"),
                ]
                : [RouteInspectProfileIntegrationWorkspace.Entry("Child", "child.md", "Route")],
        });
        workspace.WriteRoutedMarkdown(
            ".agents/root/child.md",
            "Child",
            ["Route"],
            "# Child\n\nA routed child.\n");
        return workspace;
    }

    private static void AddScalar(
        List<string> arguments,
        string option,
        string value,
        string form)
    {
        switch (form)
        {
            case "spaced":
                arguments.Add(option);
                arguments.Add(value);
                break;
            case "equals":
                arguments.Add($"{option}={value}");
                break;
            case "colon":
                arguments.Add($"{option}:{value}");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(form), form, "The native scalar form is not defined.");
        }
    }

    private static RouteInspectProfileIntegrationWorkspace CollisionWorkspace()
    {
        var workspace = RouteInspectProfileIntegrationWorkspace.Create();
        workspace.WriteLoader(
            [RouteInspectProfileIntegrationWorkspace.Entry("Root", "root/_root.md", "Root")]);
        workspace.WriteEntrypoint(new RouteInspectProfileIntegrationEntrypoint
        {
            RelativePath = ".agents/root/_root.md",
            Description = "Root",
            Tags = ["Root"],
        });
        workspace.WriteRoutedMarkdown(
            ".agents/root/collision.md",
            "Collision leaf",
            ["Route"],
            "# Collision leaf\n");
        workspace.WriteEntrypoint(new RouteInspectProfileIntegrationEntrypoint
        {
            RelativePath = ".agents/root/collision/_collision.md",
            Description = "Collision entrypoint",
            Tags = ["Route"],
        });
        return workspace;
    }

    private static RouteInspectProfileIntegrationWorkspace IncompleteWorkspace()
    {
        var workspace = CompleteWorkspace(includeMalformedLoadNowSource: true);
        workspace.Write(
            ".agents/root/malformed.md",
            "---\nopen-forge: [\n---\n\n# Malformed\n");
        return workspace;
    }

    private static void AssertInvalidJson(
        JsonDocument document,
        string conditionCode)
    {
        var result = document.RootElement.GetProperty("result");
        Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, result.GetProperty("identity").ValueKind);
        Assert.Equal(JsonValueKind.Null, result.GetProperty("profile").ValueKind);
        Assert.Equal(
            conditionCode,
            result.GetProperty("conditions")[0].GetProperty("code").GetString());
    }
}
