using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRouteMoveProcessTests
{
    [Fact(DisplayName = "Published Route Move help exposes only the exact two-operand mutation grammar")]
    [Trait("Feature", "route-move"), Trait("Evidence", "EndToEnd")]
    public async Task HelpIsTruthfulAndBypassesWorkspaceEffects()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await PublishedRouteMoveWorkspace.CreateAsync(target);
        var missingWorkspace = workspace.Combine("missing-workspace");
        var group = await RunWithoutWritesAsync(target, workspace, ["route", "--help"]);
        var leaf = await RunWithoutWritesAsync(
            target,
            workspace,
            ["route", "move", "--help", "--workspace", missingWorkspace]);
        var version = await RunWithoutWritesAsync(
            target,
            workspace,
            ["route", "move", "--version", "--workspace", missingWorkspace]);

        Assert.All([group, leaf, version], result =>
        {
            Assert.Equal(0, result.ExitCode);
            Assert.Equal(string.Empty, result.StandardError);
        });
        Assert.Equal(target.ExpectedVersion + Environment.NewLine, version.StandardOutput);
        Assert.Contains("move <source-reference> <destination-target>", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            "open-forge route move <source-reference> <destination-target>",
            leaf.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains(
            "Name the intended destination by exact path.",
            leaf.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains("--dry-run", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("--force", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("--automatic", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("--recursive", leaf.StandardOutput, StringComparison.Ordinal);
        AssertExitMapping(leaf.StandardOutput, "complete", 0, "stdout");
        AssertExitMapping(leaf.StandardOutput, "attention", 2, "stdout");
        AssertExitMapping(leaf.StandardOutput, "incomplete", 3, "stdout");
        AssertExitMapping(leaf.StandardOutput, "invalid", 4, "stderr");
        AssertExitMapping(leaf.StandardOutput, "blocked", 5, "stderr");
        AssertExitMapping(leaf.StandardOutput, "failed", 1, "stderr");
        AssertExitMapping(leaf.StandardOutput, "interrupted", 130, "stderr");
        Assert.False(Directory.Exists(missingWorkspace));
        workspace.AssertNoLockInfrastructure();
    }

    [Theory(DisplayName = "Published Route Move rejects missing and consumed operands on invalid stderr"),
        InlineData("missing-source"),
        InlineData("missing-destination"),
        InlineData("consumed-source")]
    [Trait("Feature", "route-move"), Trait("Evidence", "EndToEnd")]
    public async Task InvalidOperandBoundariesAreWriteFree(string scenario)
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await PublishedRouteMoveWorkspace.CreateAsync(target);
        string[] arguments = scenario switch
        {
            "missing-source" => ["route", "move"],
            "missing-destination" => ["route", "move", PublishedRouteMoveWorkspace.SourceId],
            "consumed-source" => ["route", "move", ".agents/guidance/consumed.md", PublishedRouteMoveWorkspace.DestinationPath],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown grammar scenario."),
        };

        var result = await RunWithoutWritesAsync(target, workspace, arguments);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains("Status: invalid", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("route-move.", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("Next: open-forge route move --help", result.StandardError, StringComparison.Ordinal);
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published Route Move rejects a third positional operand at the shell boundary without writes")]
    [Trait("Feature", "route-move"), Trait("Evidence", "EndToEnd")]
    public async Task ThirdPositionalOperandIsShellInvalidWithoutDomainEffects()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await PublishedRouteMoveWorkspace.CreateAsync(target);
        var result = await RunWithoutWritesAsync(
            target,
            workspace,
            [
                "route", "move", PublishedRouteMoveWorkspace.SourceId,
                PublishedRouteMoveWorkspace.DestinationPath, "extra",
            ]);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.False(string.IsNullOrWhiteSpace(result.StandardError));
        Assert.DoesNotContain("Status:", result.StandardError, StringComparison.Ordinal);
        Assert.DoesNotContain("route-move.", result.StandardError, StringComparison.Ordinal);
        Assert.DoesNotContain("Next:", result.StandardError, StringComparison.Ordinal);
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published Route Move repeated dry-run is idempotent exact JSON and verbose-stable")]
    [Trait("Feature", "route-move"), Trait("Evidence", "EndToEnd")]
    public async Task JsonDryRunFreezesTheCompleteSchemaAndNoWriteBoundary()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await PublishedRouteMoveWorkspace.CreateAsync(target);
        string[] arguments =
        [
            "route", "move", PublishedRouteMoveWorkspace.SourceId,
            PublishedRouteMoveWorkspace.DestinationPath,
            "--dry-run", "--dry-run", "--json",
        ];
        var preview = await RunWithoutWritesAsync(target, workspace, arguments);
        var verbose = await RunWithoutWritesAsync(target, workspace, [.. arguments, "--verbose"]);

        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        Assert.Equal(preview.ExitCode, verbose.ExitCode);
        Assert.Equal(preview.StandardOutput, verbose.StandardOutput);
        Assert.InRange(verbose.StandardError.Length, 1, 4096);

        using var document = JsonDocument.Parse(preview.StandardOutput);
        var root = document.RootElement;
        AssertOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("route move", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        var result = root.GetProperty("result");
        AssertOrder(
            result,
            "mode", "source", "destination", "subject", "ownership", "references",
            "generatedNavigation", "plan", "effects", "unchangedPaths", "recovery",
            "verification", "findings");
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        AssertOrder(root.GetProperty("workspace"), "path", "selectedBy");
        AssertOrder(result.GetProperty("source"), "requested", "selectedBy", "id", "path", "form");
        AssertOrder(result.GetProperty("destination"), "requested", "id", "path", "parentId", "parentPath");
        var subject = result.GetProperty("subject");
        AssertOrder(subject, "kind", "layers", "items");
        Assert.NotEmpty(subject.GetProperty("layers").EnumerateArray());
        Assert.All(subject.GetProperty("layers").EnumerateArray(), layer =>
            AssertOrder(layer, "layer", "sourcePath", "destinationPath"));
        Assert.Empty(subject.GetProperty("items").EnumerateArray());
        var ownership = result.GetProperty("ownership");
        AssertOrder(ownership, "state", "framework", "extensions", "claims");
        Assert.Equal("unmanaged", ownership.GetProperty("state").GetString());
        Assert.Empty(ownership.GetProperty("claims").EnumerateArray());
        var references = result.GetProperty("references");
        AssertOrder(
            references,
            "coverage", "scannedSourceCount", "inspectedSourceCount", "occurrenceCount", "rewrites");
        Assert.Equal("complete", references.GetProperty("coverage").GetString());
        Assert.NotEmpty(references.GetProperty("rewrites").EnumerateArray());
        Assert.All(references.GetProperty("rewrites").EnumerateArray(), rewrite =>
        {
            AssertOrder(
                rewrite,
                "sourcePath", "destinationSourcePath", "layer", "location", "before", "expected",
                "oldTarget", "expectedTarget");
            AssertOrder(rewrite.GetProperty("location"), "line", "column", "byteOffset", "byteLength");
            AssertOrder(rewrite.GetProperty("oldTarget"), "id", "path");
            AssertOrder(rewrite.GetProperty("expectedTarget"), "id", "path");
        });
        var generated = result.GetProperty("generatedNavigation");
        AssertOrder(generated, "coverage", "regions");
        Assert.Equal("complete", generated.GetProperty("coverage").GetString());
        Assert.NotEmpty(generated.GetProperty("regions").EnumerateArray());
        Assert.All(generated.GetProperty("regions").EnumerateArray(), region =>
            AssertOrder(region, "path", "reasons", "state"));
        AssertOrder(result.GetProperty("plan"), "completeness", "safety");
        Assert.NotEmpty(result.GetProperty("effects").EnumerateArray());
        Assert.All(result.GetProperty("effects").EnumerateArray(), effect =>
        {
            AssertOrder(effect, "path", "kind", "action", "before", "expected", "outcome", "residual");
            AssertOrder(effect.GetProperty("before"), "kind", "contentSha256");
            AssertOrder(effect.GetProperty("expected"), "kind", "contentSha256");
            Assert.Equal("planned", effect.GetProperty("outcome").GetString());
            Assert.Equal("none", effect.GetProperty("residual").GetString());
        });
        Assert.NotEmpty(result.GetProperty("unchangedPaths").EnumerateArray());
        var recovery = result.GetProperty("recovery");
        AssertOrder(recovery, "state", "protectedPaths", "residualPath");
        Assert.Equal("not-created", recovery.GetProperty("state").GetString());
        Assert.NotEmpty(recovery.GetProperty("protectedPaths").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, recovery.GetProperty("residualPath").ValueKind);
        Assert.Equal("not-requested", result.GetProperty("verification").GetString());
        Assert.Empty(result.GetProperty("findings").EnumerateArray());
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published Route Move invalid JSON retains the exact null empty and next graph")]
    [Trait("Feature", "route-move"), Trait("Evidence", "EndToEnd")]
    public async Task InvalidJsonRetainsUnresolvedFactsWithoutOmittingTheGraph()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await PublishedRouteMoveWorkspace.CreateAsync(target);

        var response = await RunWithoutWritesAsync(
            target,
            workspace,
            [
                "route", "move", ".agents/guidance/consumed.md",
                PublishedRouteMoveWorkspace.DestinationPath,
                "--json",
            ]);

        Assert.Equal(4, response.ExitCode);
        Assert.Equal(string.Empty, response.StandardError);
        using var document = JsonDocument.Parse(response.StandardOutput);
        var root = document.RootElement;
        AssertOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");
        Assert.Equal("invalid", root.GetProperty("status").GetString());
        var result = root.GetProperty("result");
        var source = result.GetProperty("source");
        AssertOrder(source, "requested", "selectedBy", "id", "path", "form");
        Assert.Equal(".agents/guidance/consumed.md", source.GetProperty("requested").GetString());
        Assert.Equal("base-path", source.GetProperty("selectedBy").GetString());
        Assert.All(
            ["id", "path", "form"],
            name => Assert.Equal(JsonValueKind.Null, source.GetProperty(name).ValueKind));
        Assert.Empty(result.GetProperty("subject").GetProperty("layers").EnumerateArray());
        Assert.Empty(result.GetProperty("subject").GetProperty("items").EnumerateArray());
        Assert.Empty(result.GetProperty("ownership").GetProperty("claims").EnumerateArray());
        Assert.Empty(result.GetProperty("references").GetProperty("rewrites").EnumerateArray());
        Assert.Empty(result.GetProperty("generatedNavigation").GetProperty("regions").EnumerateArray());
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Empty(result.GetProperty("unchangedPaths").EnumerateArray());
        var finding = Assert.Single(result.GetProperty("findings").EnumerateArray());
        AssertOrder(finding, "code", "status", "target", "cause");
        Assert.Equal("route-move.source-not-found", finding.GetProperty("code").GetString());
        AssertOrder(root.GetProperty("next"), "command", "reason");
        Assert.Equal("open-forge route move --help", root.GetProperty("next").GetProperty("command").GetString());
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published Route Move compact dry-run keeps every affected path and bounded change")]
    [Trait("Feature", "route-move"), Trait("Evidence", "EndToEnd")]
    public async Task CompactDryRunKeepsTheCompleteEffectInventory()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await PublishedRouteMoveWorkspace.CreateAsync(target);

        var result = await RunWithoutWritesAsync(
            target,
            workspace,
            [
                "route", "move", PublishedRouteMoveWorkspace.SourceId,
                PublishedRouteMoveWorkspace.DestinationPath,
                "--dry-run", "--view=compact",
            ]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains(PublishedRouteMoveWorkspace.SourcePath, result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(PublishedRouteMoveWorkspace.SourceOverwritePath, result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(PublishedRouteMoveWorkspace.DestinationPath, result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(PublishedRouteMoveWorkspace.DestinationOverwritePath, result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("README.md", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("definitions.md", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(".agents/guidance/_guidance.md", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(".agents/archive/_archive.md", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(".agents/guidance/old%20guide.md#section", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(".agents/archive/new%20guide.md#section", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("No files changed (--dry-run).", result.StandardOutput, StringComparison.Ordinal);
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published Route Move applies a leaf overwrite references and navigation then old-source repeat is invalid")]
    [Trait("Feature", "route-move"), Trait("Evidence", "EndToEnd")]
    public async Task LeafApplyPreservesBytesAndConsumesTheOldIdentity()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await PublishedRouteMoveWorkspace.CreateAsync(target);
        var lifecycleBefore = workspace.ReadBytes(".agents/open-forge.lifecycle.json");
        string[] arguments =
        [
            "route", "move", PublishedRouteMoveWorkspace.SourceId,
            PublishedRouteMoveWorkspace.DestinationPath,
        ];

        var applied = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            arguments,
            workspace.ProcessEnvironment);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        Assert.Contains("Status: complete", applied.StandardOutput, StringComparison.Ordinal);
        Assert.False(File.Exists(workspace.Combine(PublishedRouteMoveWorkspace.SourcePath)));
        Assert.False(File.Exists(workspace.Combine(PublishedRouteMoveWorkspace.SourceOverwritePath)));
        Assert.Equal(
            PublishedRouteMoveWorkspace.SourceText,
            workspace.ReadText(PublishedRouteMoveWorkspace.DestinationPath));
        Assert.Equal(
            PublishedRouteMoveWorkspace.OverwriteText,
            workspace.ReadText(PublishedRouteMoveWorkspace.DestinationOverwritePath));
        Assert.Contains(
            ".agents/archive/new%20guide.md#section",
            workspace.ReadText("README.md"),
            StringComparison.Ordinal);
        Assert.Equal(lifecycleBefore, workspace.ReadBytes(".agents/open-forge.lifecycle.json"));
        workspace.AssertPersistentExternalLock();

        var repeated = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            arguments,
            workspace.ProcessEnvironment);

        Assert.Equal(4, repeated.ExitCode);
        Assert.Equal(string.Empty, repeated.StandardOutput);
        Assert.Contains("route-move.source-not-found", repeated.StandardError, StringComparison.Ordinal);
        Assert.Contains("Status: invalid", repeated.StandardError, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Route Move moves one complete category layout and outside references")]
    [Trait("Feature", "route-move"), Trait("Evidence", "EndToEnd")]
    public async Task CategoryApplyPreservesEveryContainedItemAndRelativeLayout()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await PublishedRouteMoveWorkspace.CreateAsync(target);
        var binaryBefore = workspace.ReadBytes(".agents/guidance/topics/image.bin");

        var result = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            [
                "route", "move", PublishedRouteMoveWorkspace.CategoryPath,
                PublishedRouteMoveWorkspace.CategoryDestination,
            ],
            workspace.ProcessEnvironment);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("Status: complete", result.StandardOutput, StringComparison.Ordinal);
        Assert.False(Directory.Exists(workspace.Combine(".agents/guidance/topics")));
        Assert.True(File.Exists(workspace.Combine(PublishedRouteMoveWorkspace.CategoryDestination)));
        Assert.True(File.Exists(workspace.Combine(".agents/archive/topics/child.md")));
        Assert.True(File.Exists(workspace.Combine(".agents/archive/topics/notes.md")));
        Assert.Equal(binaryBefore, workspace.ReadBytes(".agents/archive/topics/image.bin"));
        Assert.Contains(
            "../../guidance/old%20guide.md#section",
            workspace.ReadText(".agents/archive/topics/child.md"),
            StringComparison.Ordinal);
        Assert.Contains(
            "notes.md#detail",
            workspace.ReadText(".agents/archive/topics/child.md"),
            StringComparison.Ordinal);
        workspace.AssertPersistentExternalLock();
    }

    [Theory(DisplayName = "Published Route Move maps deterministic blocked and incomplete conditions to exact streams"),
        InlineData("occupied", 5, "blocked", "route-move.destination-occupied"),
        InlineData("ownership-missing", 5, "blocked", "route-move.ownership-unavailable"),
        InlineData("invalid-utf8", 3, "incomplete", "route-move.reference-coverage-incomplete")]
    [Trait("Feature", "route-move"), Trait("Evidence", "EndToEnd")]
    public async Task SafetyAndCoverageConditionsRemainWriteFree(
        string scenario,
        int exit,
        string status,
        string code)
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await PublishedRouteMoveWorkspace.CreateAsync(target);
        if (scenario == "occupied")
        {
            workspace.SeedOccupiedDestination();
        }
        else if (scenario == "ownership-missing")
        {
            workspace.RemoveLifecycle();
        }
        else if (scenario == "invalid-utf8")
        {
            workspace.SeedInvalidUtf8();
        }

        var result = await RunWithoutWritesAsync(
            target,
            workspace,
            [
                "route", "move", PublishedRouteMoveWorkspace.SourceId,
                PublishedRouteMoveWorkspace.DestinationPath,
            ]);

        Assert.Equal(exit, result.ExitCode);
        var primary = status == "incomplete" ? result.StandardOutput : result.StandardError;
        Assert.Contains($"Status: {status}", primary, StringComparison.Ordinal);
        Assert.Contains(code, primary, StringComparison.Ordinal);
        workspace.AssertNoLockInfrastructure();
    }

    private static Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableTarget target,
        PublishedRouteMoveWorkspace workspace,
        string[] arguments)
        => PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            arguments,
            workspace.ProcessEnvironment);

    private static void AssertOrder(JsonElement element, params string[] names)
        => Assert.Equal(names, element.EnumerateObject().Select(property => property.Name));

    private static void AssertExitMapping(string help, string status, int exitCode, string stream)
        => Assert.Contains(
            $"{status}: exit {exitCode} and human {stream}.",
            help,
            StringComparison.Ordinal);
}
