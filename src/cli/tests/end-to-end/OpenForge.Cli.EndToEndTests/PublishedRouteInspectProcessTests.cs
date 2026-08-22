using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRouteInspectHelpProcessTests
{
    [Fact(DisplayName = "Published Route Inspect leaf help exposes exact grammar and related commands")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task InspectHelpExposesPublicGrammar()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = TemporaryWorkspace.Create("e2e-route-inspect-help");
        var missingWorkspace = working.Combine("missing-workspace");
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            [
                "route",
                "inspect",
                "--workspace",
                missingWorkspace,
                "--json",
                "--view=compact",
                "--verbose",
                "--help",
            ]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("open-forge route inspect <source-reference>", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--workspace <path>", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--json", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--view", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--verbose", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Related commands", result.StandardOutput, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missingWorkspace));
    }

    [Fact(DisplayName = "Published Route Inspect version bypasses a missing workspace in terminal mode")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task InspectVersionBypassesWorkspaceAndOperation()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = TemporaryWorkspace.Create("e2e-route-inspect-version");
        var missingWorkspace = working.Combine("missing-workspace");
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            [
                "route",
                "inspect",
                "--workspace",
                missingWorkspace,
                "--json",
                "--view=compact",
                "--verbose",
                "--version",
            ]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(environment.ExpectedVersion + Environment.NewLine, result.StandardOutput);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.False(Directory.Exists(missingWorkspace));
    }
}

public sealed class PublishedRouteInspectPresentationProcessTests
{
    [Fact(DisplayName = "Published Route Inspect compact human result uses stdout and exit zero")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task CompactHumanResultUsesSuccessStream()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteInspectWorkspace.CreateComplete();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "root", "--workspace", working.Path, "--view=compact"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("ID: root", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Status: complete", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(".agents/root/_root.md", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Next:", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Route Inspect expanded human result uses stdout and exit zero")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task ExpandedHumanResultUsesSuccessStream()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteInspectWorkspace.CreateComplete();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "root", "--workspace", working.Path, "--view=expanded"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("Status: complete", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Axioms", result.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(".agents/root/_root.md", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Next:", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Route Inspect JSON exposes the complete typed identity and profile graph")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task JsonResultExposesCompleteTypedGraph()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteInspectWorkspace.CreateComplete();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "root", "--workspace", working.Path, "--json"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;

        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("route inspect", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal(
            "root",
            root.GetProperty("result").GetProperty("selection").GetProperty("requestedReference").GetString());

        var identity = root.GetProperty("result").GetProperty("identity");
        Assert.Equal(".agents/root/_root.md", identity.GetProperty("path").GetString());
        var layers = identity.GetProperty("physicalLayers").EnumerateArray().ToArray();
        Assert.Equal(
            [".agents/root/_root.md", ".agents/root/_root.overwrite.md"],
            layers.Select(layer => layer.GetProperty("workspaceRelativePath").GetString()));
        Assert.Equal(["base", "overwrite"], layers.Select(layer => layer.GetProperty("role").GetString()));

        var profile = root.GetProperty("result").GetProperty("profile");
        Assert.Equal(
            ["reading", "measurements", "topology", "axioms", "completeness", "safety"],
            profile.EnumerateObject().Select(property => property.Name));
        Assert.Equal(
            ["ownSource", "selectedClosure", "taskStartOverlap", "selectionAddition", "loadNowDescendants"],
            profile.GetProperty("measurements").EnumerateObject().Select(property => property.Name));
        Assert.Equal(
            "value",
            profile.GetProperty("measurements").GetProperty("ownSource").GetProperty("state").GetString());
        Assert.True(profile.GetProperty("reading").TryGetProperty("automatic", out _));
        Assert.True(profile.GetProperty("topology").TryGetProperty("value", out _));
        Assert.True(profile.GetProperty("axioms").TryGetProperty("value", out _));
        Assert.Equal(JsonValueKind.Array, root.GetProperty("result").GetProperty("observations").ValueKind);
        Assert.Equal(JsonValueKind.Array, root.GetProperty("result").GetProperty("conditions").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
    }

    [Fact(DisplayName = "Published Route Inspect JSON view is a no-op across compact and expanded requests")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task JsonViewDoesNotChangeTheTypedDocument()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteInspectWorkspace.CreateComplete();
        var compact = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "root", "--workspace", working.Path, "--json", "--view=compact"]);
        var expanded = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "root", "--workspace", working.Path, "--json", "--view=expanded"]);

        Assert.Equal(0, compact.ExitCode);
        Assert.Equal(0, expanded.ExitCode);
        Assert.Equal(string.Empty, compact.StandardError);
        Assert.Equal(string.Empty, expanded.StandardError);
        Assert.Equal(compact.StandardOutput, expanded.StandardOutput);
    }

    [Fact(DisplayName = "Published Route Inspect verbose human output preserves the primary result and adds bounded diagnostics")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task VerboseHumanOutputPreservesPrimaryResult()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteInspectWorkspace.CreateComplete();
        var plain = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "root", "--workspace", working.Path, "--view=expanded"]);
        var verbose = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "root", "--workspace", working.Path, "--view=expanded", "--verbose"]);

        Assert.Equal(plain.ExitCode, verbose.ExitCode);
        Assert.Equal(plain.StandardOutput, verbose.StandardOutput);
        Assert.Equal(string.Empty, plain.StandardError);
        Assert.InRange(verbose.StandardError.Length, 1, 4096);
    }

    [Fact(DisplayName = "Published Route Inspect verbose JSON preserves one stdout document and separates diagnostics")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task VerboseJsonPreservesPrimaryDocument()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteInspectWorkspace.CreateComplete();
        var plain = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "root", "--workspace", working.Path, "--json"]);
        var verbose = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "root", "--workspace", working.Path, "--json", "--verbose"]);

        Assert.Equal(plain.ExitCode, verbose.ExitCode);
        Assert.Equal(plain.StandardOutput, verbose.StandardOutput);
        Assert.Equal(string.Empty, plain.StandardError);
        Assert.InRange(verbose.StandardError.Length, 1, 4096);
        using var document = JsonDocument.Parse(verbose.StandardOutput);
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
    }

    [Theory(DisplayName = "Published Route Inspect accepts native scalar workspace and view forms")]
    [InlineData("spaced", "spaced")]
    [InlineData("spaced", "equals")]
    [InlineData("spaced", "colon")]
    [InlineData("equals", "spaced")]
    [InlineData("equals", "equals")]
    [InlineData("equals", "colon")]
    [InlineData("colon", "spaced")]
    [InlineData("colon", "equals")]
    [InlineData("colon", "colon")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task NativeScalarFormsRemainAccepted(string workspaceForm, string viewForm)
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteInspectWorkspace.CreateComplete();
        var arguments = new List<string> { "route", "inspect", "root" };
        AddScalar(arguments, "--workspace", working.Path, workspaceForm);
        AddScalar(arguments, "--view", "compact", viewForm);

        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            arguments);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("ID: root", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Status: complete", result.StandardOutput, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Published Route Inspect preserves filesystem-safe hostile source values")]
    [InlineData(PublishedRouteInspectWorkspace.HostileSourcePath)]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task HostileFilesystemSafeSourceValuesRemainExact(string sourcePath)
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteInspectWorkspace.CreateHostileValue();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", sourcePath, "--workspace", working.Path, "--json"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            sourcePath,
            document.RootElement.GetProperty("result").GetProperty("selection")
                .GetProperty("requestedReference").GetString());
        Assert.Equal(
            sourcePath,
            document.RootElement.GetProperty("result").GetProperty("identity")
                .GetProperty("path").GetString());
    }

    [Fact(DisplayName = "Published Route Inspect preserves an option-like operand after the process terminator")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task OptionLikeOperandAfterTerminatorRemainsDomainInput()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteInspectWorkspace.CreateComplete();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "--workspace", working.Path, "--json", "--", "--view"]);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            "--view",
            document.RootElement.GetProperty("result").GetProperty("selection")
                .GetProperty("requestedReference").GetString());
    }

    [Theory(DisplayName = "Published Route Inspect terminal modes reject source input before workspace and operation")]
    [InlineData("--help", "root")]
    [InlineData("--version", "root")]
    [InlineData("--help", "option-like")]
    [InlineData("--version", "option-like")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task TerminalModesRejectSourceInputBeforeWorkspaceAndOperation(
        string terminalMode,
        string sourceKind)
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = TemporaryWorkspace.Create("e2e-route-inspect-terminal-conflict");
        var missingWorkspace = working.Combine("missing-workspace");
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

        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            arguments);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        var diagnostic = Assert.Single(
            result.StandardError.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
        Assert.InRange(diagnostic.Length, 1, 4096);
        Assert.False(Directory.Exists(missingWorkspace));
    }

    private static void AddScalar(
        ICollection<string> arguments,
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
}

public sealed class PublishedRouteInspectStatusProcessTests
{
    [Fact(DisplayName = "Published Route Inspect complete status uses stdout exit zero and no next action")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task CompleteStatusUsesSuccessStreamWithoutNext()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteInspectWorkspace.CreateComplete();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "root", "--view=compact"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("Status: complete", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Next:", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Route Inspect exact-path attention status uses stdout without an invented next action")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task ExactPathAttentionStatusHasNoInventedNext()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteInspectWorkspace.CreateAttention();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", ".agents/root/collision.md", "--view=compact"]);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("Status: attention", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Next:", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Route Inspect blocked collision uses stderr and the exact public next wording")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task BlockedCollisionUsesExactNextWording()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteInspectWorkspace.CreateAttention();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "root/collision", "--view=compact"]);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains("Status: blocked", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("Next: rerun with one of the listed exact paths.", result.StandardError, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Route Inspect blocked JSON retains every collision candidate and exit")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task BlockedJsonRetainsCollisionCandidates()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteInspectWorkspace.CreateAttention();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "root/collision", "--json"]);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("blocked", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            [".agents/root/collision.md", ".agents/root/collision/_collision.md"],
            document.RootElement.GetProperty("result").GetProperty("selection")
                .GetProperty("candidatePaths").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(
            "open-forge route inspect \".agents/root/collision.md\"",
            document.RootElement.GetProperty("next").GetProperty("command").GetString());
        Assert.Equal(
            "Rerun with one listed exact path to resolve the source collision.",
            document.RootElement.GetProperty("next").GetProperty("reason").GetString());
    }

    [Fact(DisplayName = "Published Route Inspect incomplete status uses stdout and the doctor next action")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task IncompleteStatusUsesDoctorNextAction()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteInspectWorkspace.CreateIncomplete();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "root", "--view=compact"]);

        Assert.Equal(3, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("Status: incomplete", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Next: open-forge doctor", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Route Inspect invalid source status uses stderr and the exact public next wording")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task InvalidSourceUsesExactNextWording()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteInspectWorkspace.CreateComplete();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "unknown", "--view=compact"]);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains("Status: invalid", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("Next: correct the named source or input.", result.StandardError, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Published Route Inspect invalid cardinality remains a JSON result without operation execution")]
    [InlineData("missing", "route-inspect.missing-source")]
    [InlineData("multiple", "route-inspect.multiple-sources")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task InvalidCardinalityUsesTypedJson(string caseName, string expectedCondition)
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteInspectWorkspace.CreateComplete();
        IReadOnlyList<string> arguments = caseName == "missing"
            ? ["route", "inspect", "--workspace", working.Path, "--json"]
            : ["route", "inspect", "first", "second", "--workspace", working.Path, "--json"];
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            arguments);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            expectedCondition,
            document.RootElement.GetProperty("result").GetProperty("conditions")[0]
                .GetProperty("code").GetString());
    }
}
