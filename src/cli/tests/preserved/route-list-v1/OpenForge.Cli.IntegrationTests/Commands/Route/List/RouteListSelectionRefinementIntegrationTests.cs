using System.Text.Json;
using OpenForge.Cli.Commands.Route.List;
using OpenForge.Cli.Pipeline;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

public sealed class RouteListSelectionRefinementIntegrationTests
{
    [Fact(DisplayName = "Route-list JSON preserves an unknown attempted ID without inventing a source path"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task UnknownIdRetainsOnlyAttemptedId()
    {
        using var workspace = CreateRootedWorkspace();

        var result = await RunJsonAsync(workspace, "unknown/source");

        Assert.Equal(4, result.ExitCode);
        AssertSelection(result.Root, "unknown/source", expectedPath: null);
        Assert.Equal("invalid", result.Root.GetProperty("status").GetString());
    }

    [Fact(DisplayName = "Route-list JSON preserves an invalid attempted exact path without inventing a source ID"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task InvalidExactPathRetainsOnlyAttemptedPath()
    {
        const string attemptedPath = ".agents/\0invalid.md";
        using var workspace = CreateRootedWorkspace();

        var result = await RunJsonAsync(workspace, attemptedPath);

        Assert.Equal(4, result.ExitCode);
        AssertSelection(result.Root, expectedId: null, attemptedPath);
        Assert.Equal(RouteListFindingCodes.SourcePathInvalid, SingleFinding(result.Root).GetProperty("code").GetString());
        Assert.DoesNotContain("\0", result.RawJson, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route-list JSON ambiguity preserves the attempted source ID and candidate locations"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task AmbiguousIdRetainsAttemptedIdentity()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.WriteRoute(".agents/root/collision.md", "Collision leaf", "Collision");
        workspace.WriteRoute(".agents/root/collision/_collision.md", "Collision entrypoint", "Collision");

        var result = await RunJsonAsync(workspace, "root/collision");

        Assert.Equal(5, result.ExitCode);
        Assert.Equal("blocked", result.Root.GetProperty("status").GetString());
        AssertSelection(result.Root, "root/collision", expectedPath: null);
        var findings = result.Root.GetProperty("result").GetProperty("findings").EnumerateArray().ToArray();
        Assert.Equal(2, findings.Length);
        Assert.All(findings, finding => Assert.Equal(RouteListFindingCodes.IdCollision, finding.GetProperty("code").GetString()));
        Assert.Contains(findings, finding => finding.GetProperty("path").GetString() == ".agents/root/collision.md");
        Assert.Contains(findings, finding => finding.GetProperty("path").GetString() == ".agents/root/collision/_collision.md");
    }

    [Theory(DisplayName = "Route-list resolved ID and exact-path selections expose both canonical identities"),
     InlineData("root"),
     InlineData("./.agents/root/_root.md"),
     Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task ResolvedSelectionCarriesBothCanonicalIdentities(string sourceReference)
    {
        using var workspace = CreateRootedWorkspace();

        var result = await RunJsonAsync(workspace, sourceReference);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("complete", result.Root.GetProperty("status").GetString());
        AssertSelection(result.Root, "root", ".agents/root/_root.md");
    }

    [Fact(DisplayName = "Route-list incomplete JSON retains the resolved explicit source and invalid-UTF8 finding location"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task IncompleteResultRetainsResolvedSelection()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.WriteBytes(".agents/root/broken.md", [0xFF, 0xFE]);

        var result = await RunJsonAsync(workspace, "root");

        Assert.Equal(3, result.ExitCode);
        Assert.Equal("incomplete", result.Root.GetProperty("status").GetString());
        AssertSelection(result.Root, "root", ".agents/root/_root.md");
        Assert.Contains(
            result.Root.GetProperty("result").GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == RouteListFindingCodes.SourceReadFailed
                && finding.GetProperty("path").GetString() == ".agents/root/broken.md");
    }

    [Theory(DisplayName = "Route-list rejects malformed source IDs without normalization or an accidental source path"),
     InlineData("/root"),
     InlineData("root/"),
     InlineData("root//child"),
     InlineData("root/./child"),
     InlineData("root/../child"),
     InlineData("root\\child"),
     InlineData("root/\u0001child"),
     Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Integration")]
    public async Task InvalidIdGrammarIsRejectedWithoutNormalization(string attemptedId)
    {
        using var workspace = CreateRootedWorkspace();

        var result = await RunJsonAsync(workspace, attemptedId);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal("invalid", result.Root.GetProperty("status").GetString());
        AssertSelection(result.Root, attemptedId, expectedPath: null);
        var finding = SingleFinding(result.Root);
        Assert.Equal(RouteListFindingCodes.SourceIdInvalid, finding.GetProperty("code").GetString());
        Assert.InRange(finding.GetProperty("message").GetString()?.Length ?? 0, 1, 512);
        Assert.DoesNotContain("System.", finding.GetProperty("message").GetString(), StringComparison.Ordinal);
        Assert.DoesNotContain("\u0001", result.RawJson, StringComparison.Ordinal);
    }

    private static RouteListTestWorkspace CreateRootedWorkspace()
    {
        var workspace = RouteListTestWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.WriteRoute(".agents/root/_root.md", "Root", "Root");
        return workspace;
    }

    private static async Task<JsonApplicationResult> RunJsonAsync(
        RouteListTestWorkspace workspace,
        string sourceReference)
    {
        var before = workspace.SnapshotHashes();
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        var exitCode = await CliApplication.RunAsync(
            ["route", "list", sourceReference, "--workspace", workspace.Path, "--json"],
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);
        var rawJson = standardOutput.ToString();
        using var document = JsonDocument.Parse(rawJson);

        Assert.Equal(string.Empty, standardError.ToString());
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(RouteListDefinitions.SchemaVersion, document.RootElement.GetProperty("schemaVersion").GetInt32());
        return new JsonApplicationResult(exitCode, rawJson, document.RootElement.Clone());
    }

    private static void AssertSelection(JsonElement root, string? expectedId, string? expectedPath)
    {
        var selection = root.GetProperty("result").GetProperty("selection");
        Assert.Equal("explicitSource", selection.GetProperty("kind").GetString());
        AssertJsonNullableString(selection.GetProperty("sourceId"), expectedId);
        AssertJsonNullableString(selection.GetProperty("sourcePath"), expectedPath);
    }

    private static void AssertJsonNullableString(JsonElement value, string? expected)
    {
        if (expected is null)
        {
            Assert.Equal(JsonValueKind.Null, value.ValueKind);
            return;
        }

        Assert.Equal(expected, value.GetString());
    }

    private static JsonElement SingleFinding(JsonElement root)
    {
        return Assert.Single(root.GetProperty("result").GetProperty("findings").EnumerateArray());
    }

    private sealed record JsonApplicationResult(int ExitCode, string RawJson, JsonElement Root);
}
