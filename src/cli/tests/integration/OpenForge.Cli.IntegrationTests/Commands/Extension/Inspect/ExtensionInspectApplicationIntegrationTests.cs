using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Inspect;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Reading;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Inspect;

public sealed class ExtensionInspectApplicationIntegrationTests
{
    private const string ToolkitOwnership = """
        {"schemaVersion":1,"extensions":[{"id":"toolkit","version":"1.0.0","source":"embedded catalogue","dependencies":[],"paths":[".agents/toolkit.md"],"regions":[]}]}
        """;

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Extension Inspect retains relations, paths and fingerprints across detail levels")]
    [Trait("Feature", "compact-json"), Trait("Evidence", "Integration")]
    public async Task CompactComparisonRetainsObservedChangesWithoutWrites()
    {
        using var fixture = InspectFixture.Create(intendedContent: "beta\n");
        var beforeWorkspace = fixture.Workspace.SnapshotHashes();
        var beforeSource = fixture.Source.SnapshotHashes();
        string[] arguments = ["extension", "inspect", "toolkit", "--workspace", fixture.Workspace.Path, "--source", fixture.Source.Path, "--format", "json"];
        var expanded = await CliHostCapture.RunAsync([.. arguments, "--detail=standard"], fixture.Workspace.Path);
        var compact = await CliHostCapture.RunAsync([.. arguments, "--detail=minimal"], fixture.Workspace.Path);

        Assert.Equal(2, expanded.ExitCode);
        Assert.Equal(expanded.ExitCode, compact.ExitCode);
        Assert.Equal(string.Empty, compact.Error);
        Assert.Equal(string.Empty, expanded.Error);
        using var expandedDocument = JsonDocument.Parse(expanded.Output);
        using var compactDocument = JsonDocument.Parse(compact.Output);
        var expandedData = expandedDocument.RootElement.GetProperty("data");
        var compactData = compactDocument.RootElement.GetProperty("data");
        Assert.NotEmpty(expandedData.GetProperty("files").EnumerateArray());
        Assert.Equal(compactData.GetProperty("id").GetString(), expandedData.GetProperty("id").GetString());
        Assert.Equal(compactData.GetProperty("installed").GetRawText(), expandedData.GetProperty("installed").GetRawText());
        Assert.Equal(compactData.GetProperty("available").GetRawText(), expandedData.GetProperty("available").GetRawText());
        Assert.Equal(compactData.GetProperty("source").GetRawText(), expandedData.GetProperty("source").GetRawText());
        Assert.Equal(compactData.GetProperty("matches").GetBoolean(), expandedData.GetProperty("matches").GetBoolean());
        Assert.Equal(compactData.GetProperty("files").GetRawText(), expandedData.GetProperty("files").GetRawText());
        Assert.Equal(beforeWorkspace, fixture.Workspace.SnapshotHashes());
        Assert.Equal(beforeSource, fixture.Source.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Extension Inspect help exposes exact grammar and the read-only boundary"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task InspectHelpExposesExactGrammarAndReadOnlyBoundary()
    {
        using var workspace = TemporaryWorkspace.Create("extension-inspect-help");
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            ["extension", "inspect", "--help"],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains(
            "open-forge extension inspect <stable-id>",
            result.Output,
            StringComparison.Ordinal);
        Assert.Contains("--source <package-or-catalogue-path>", result.Output, StringComparison.Ordinal);
        Assert.Contains("--workspace <path>", result.Output, StringComparison.Ordinal);
        Assert.Contains("Results and streams", result.Output, StringComparison.Ordinal);
        Assert.Matches(@"never\s+writes", result.Output);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Extension Inspect ignores leftover integrity records when comparing current and intended content"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task LeftoverIntegrityRecordDoesNotChangeComparison()
    {
        using var fixture = InspectFixture.Create(
            intendedContent: "beta\n");
        fixture.Workspace.WriteText(".agents/open-forge.lifecycle.json", "{ obsolete and malformed }");
        var beforeWorkspace = fixture.Workspace.SnapshotHashes();
        var beforeSource = fixture.Source.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            [
                "extension", "inspect", "toolkit",
                "--workspace", fixture.Workspace.Path,
                "--source", fixture.Source.Path,
                "--format", "json",
            ],
            fixture.Workspace.Path);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("completed-with-warnings", root.GetProperty("status").GetString());
        Assert.Equal("open-forge extension update toolkit --dry-run", root.GetProperty("next").GetProperty("command").GetString());

        var commandResult = root.GetProperty("data");
        Assert.Equal("toolkit", commandResult.GetProperty("id").GetString());
        Assert.Equal("package", commandResult.GetProperty("source").GetProperty("kind").GetString());
        Assert.Equal(fixture.Source.Path, commandResult.GetProperty("source").GetProperty("path").GetString());

        var comparisonPath = Assert.Single(commandResult.GetProperty("files").EnumerateArray());
        Assert.Equal("changed", comparisonPath.GetProperty("relation").GetString());
        Assert.Contains(
            root.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "extension-inspect.path-changed");
        Assert.Equal(beforeWorkspace, fixture.Workspace.SnapshotHashes());
        Assert.Equal(beforeSource, fixture.Source.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Extension Inspect recommends update from complete current and intended semantic facts"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task CurrentAndIntendedSemanticFactsEnableRecommendation()
    {
        using var fixture = InspectFixture.Create(
            intendedContent: "beta\n");
        var beforeWorkspace = fixture.Workspace.SnapshotHashes();
        var beforeSource = fixture.Source.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            [
                "extension", "inspect", "toolkit",
                "--workspace", fixture.Workspace.Path,
                "--source", fixture.Source.Path,
                "--format", "json",
            ],
            fixture.Workspace.Path);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("completed-with-warnings", root.GetProperty("status").GetString());
        Assert.Equal(
            "open-forge extension update toolkit --dry-run",
            root.GetProperty("next").GetProperty("command").GetString());
        Assert.Equal(
            "extension-inspect.path-changed",
            Assert.Single(root.GetProperty("findings").EnumerateArray()).GetProperty("code").GetString());
        Assert.Equal(
            "changed",
            Assert.Single(root.GetProperty("data").GetProperty("files").EnumerateArray())
                .GetProperty("relation")
                .GetString());
        Assert.Equal(beforeWorkspace, fixture.Workspace.SnapshotHashes());
        Assert.Equal(beforeSource, fixture.Source.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Extension Inspect does not fall back to embedded facts when an explicit source is missing"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task MissingExplicitSourceRemainsTheOnlySource()
    {
        using var fixture = InspectFixture.Create(
            intendedContent: "alpha\n");
        var missingSource = fixture.Source.Combine("missing-source");
        var beforeWorkspace = fixture.Workspace.SnapshotHashes();
        var beforeSource = fixture.Source.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            [
                "extension", "inspect", "toolkit",
                "--workspace", fixture.Workspace.Path,
                "--source", missingSource,
                "--format", "json",
            ],
            fixture.Workspace.Path);

        Assert.Equal(3, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("incomplete", root.GetProperty("status").GetString());
        var source = root.GetProperty("data").GetProperty("source");
        Assert.Equal(JsonValueKind.Null, source.GetProperty("kind").ValueKind);
        Assert.Equal(missingSource, source.GetProperty("path").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("data").GetProperty("available").ValueKind);
        Assert.Contains(
            root.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "extension-inspect.source-unavailable");
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        Assert.Equal(beforeWorkspace, fixture.Workspace.SnapshotHashes());
        Assert.Equal(beforeSource, fixture.Source.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Extension Inspect invalid stable IDs keep the native report data shape"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task InvalidStableIdProducesTypedResult()
    {
        using var workspace = TemporaryWorkspace.Create("extension-inspect-invalid-id");
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            ["extension", "inspect", "Toolkit", "--workspace", workspace.Path, "--format", "json"],
            workspace.Path);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("invalid-input", root.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("workspace").ValueKind);
        var commandResult = root.GetProperty("data");
        Assert.Equal("Toolkit", commandResult.GetProperty("id").GetString());
        Assert.Equal(JsonValueKind.Null, commandResult.GetProperty("installed").ValueKind);
        Assert.Equal(JsonValueKind.Null, commandResult.GetProperty("available").ValueKind);
        Assert.Empty(commandResult.GetProperty("files").EnumerateArray());
        Assert.Equal(
            "extension-inspect.invalid-stable-id",
            Assert.Single(root.GetProperty("findings").EnumerateArray()).GetProperty("code").GetString());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Inspect cancellation produces an interrupted typed event without writes"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task CancellationProducesInterruptedEvent()
    {
        using var workspace = TemporaryWorkspace.Create("extension-inspect-cancel-workspace");
        using var source = TemporaryWorkspace.Create("extension-inspect-cancel-source");
        var request = new ExtensionInspectRequest(
            new CliWorkspace(
                lexicalRoot: workspace.Path,
                physicalRoot: workspace.Path,
                selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace),
            "toolkit",
            source.Path);
        var beforeWorkspace = workspace.SnapshotHashes();
        var beforeSource = source.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await ExtensionInspectOperationFactory.Create().ExecuteAsync(request, cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(
            ExtensionInspectFindingCode.Interrupted,
            Assert.Single(result.Findings).Code);
        Assert.Equal(beforeWorkspace, workspace.SnapshotHashes());
        Assert.Equal(beforeSource, source.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Extension Inspect blocks an explicit source overlapping the workspace"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task OverlappingSourceIsBlockedBeforeSelection()
    {
        using var workspace = TemporaryWorkspace.Create("extension-inspect-overlap");
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            [
                "extension", "inspect", "toolkit",
                "--workspace", workspace.Path,
                "--source", workspace.Path,
                "--format", "json",
            ],
            workspace.Path);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("blocked", root.GetProperty("status").GetString());
        Assert.Contains(
            root.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "extension-inspect.source-overlap");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Inspect blocks ambiguous source shape and duplicate active identity without selecting a candidate"),
     Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    [InlineData("source-shape", (int)ExtensionInspectFindingCode.SourceAmbiguous, 0)]
    [InlineData("duplicate-identity", (int)ExtensionInspectFindingCode.IdentityAmbiguous, 2)]
    public static async Task AmbiguousSourceAndIdentityRemainUnselected(
        string scenario,
        int expectedFinding,
        int expectedCandidates)
    {
        using var fixture = InspectScenario.Create(scenario);
        if (scenario == "source-shape")
        {
            fixture.WritePackage(string.Empty, "toolkit", []);
            fixture.WritePackage("catalogue", "helper", []);
        }
        else
        {
            fixture.WritePackage("a", "toolkit", []);
            fixture.WritePackage("b", "toolkit", []);
        }

        var beforeWorkspace = fixture.Workspace.SnapshotHashes();
        var beforeSource = fixture.Source.SnapshotHashes();

        var result = await fixture.InspectAsync();

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == (ExtensionInspectFindingCode)expectedFinding);
        Assert.Equal(expectedCandidates, result.Subject.Candidates.Count);
        Assert.Null(result.Available.Package);
        Assert.Equal(beforeWorkspace, fixture.Workspace.SnapshotHashes());
        Assert.Equal(beforeSource, fixture.Source.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Inspect distinguishes incomplete cyclic and conflicting dependency closure"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    [InlineData("incomplete", (int)ExtensionInspectFindingCode.DependencyIncomplete, (int)CliSemanticStatus.Incomplete, (int)ExtensionInspectDependencyState.Incomplete)]
    [InlineData("cycle", (int)ExtensionInspectFindingCode.DependencyCycle, (int)CliSemanticStatus.Blocked, (int)ExtensionInspectDependencyState.Blocked)]
    [InlineData("conflict", (int)ExtensionInspectFindingCode.DependencyConflict, (int)CliSemanticStatus.Blocked, (int)ExtensionInspectDependencyState.NotStarted)]
    public static async Task DependencyFailuresRetainOnlySafeClosureFacts(
        string scenario,
        int expectedFinding,
        int expectedStatus,
        int expectedDependencyState)
    {
        using var fixture = InspectScenario.Create($"dependency-{scenario}");
        switch (scenario)
        {
            case "incomplete":
                fixture.WritePackage("toolkit", "toolkit", ["missing"]);
                break;
            case "cycle":
                fixture.WritePackage("helper", "helper", ["toolkit"]);
                fixture.WritePackage("toolkit", "toolkit", ["helper"]);
                break;
            case "conflict":
                fixture.WritePackage(string.Empty, "toolkit", ["helper", "helper"]);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The dependency scenario is not defined.");
        }

        var beforeWorkspace = fixture.Workspace.SnapshotHashes();
        var beforeSource = fixture.Source.SnapshotHashes();

        var result = await fixture.InspectAsync();

        Assert.Equal((CliSemanticStatus)expectedStatus, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == (ExtensionInspectFindingCode)expectedFinding);
        Assert.Equal((ExtensionInspectDependencyState)expectedDependencyState, result.Dependencies.State);
        if (scenario is "incomplete" or "cycle")
        {
            Assert.NotNull(result.Available.Package);
            Assert.Contains(result.Dependencies.Resolved, dependency => dependency.Id == "toolkit");
        }
        else
        {
            Assert.Null(result.Available.Package);
        }

        Assert.Equal(beforeWorkspace, fixture.Workspace.SnapshotHashes());
        Assert.Equal(beforeSource, fixture.Source.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Inspect classifies a malformed selected package without inventing package facts"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task MalformedSelectedPackageIsPackageInvalid()
    {
        using var fixture = InspectScenario.Create("package-invalid");
        fixture.Source.WriteText(
            "extension.json",
            "{ \"id\": \"toolkit\", \"id\": \"duplicate\" }");
        var beforeWorkspace = fixture.Workspace.SnapshotHashes();
        var beforeSource = fixture.Source.SnapshotHashes();

        var result = await fixture.InspectAsync();

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionInspectFindingCode.PackageInvalid);
        Assert.Null(result.Available.Package);
        Assert.Equal(beforeWorkspace, fixture.Workspace.SnapshotHashes());
        Assert.Equal(beforeSource, fixture.Source.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Inspect blocks unsafe targets and conflicting closure ownership"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    [InlineData("unsafe-target", (int)ExtensionInspectFindingCode.PathInvalid)]
    [InlineData("ownership", (int)ExtensionInspectFindingCode.OwnershipConflict)]
    public static async Task UnsafeTargetsAndOwnershipAreBlocked(
        string scenario,
        int expectedFinding)
    {
        using var fixture = InspectScenario.Create(scenario);
        if (scenario == "unsafe-target")
        {
            fixture.WritePackage(string.Empty, "toolkit", [], (".agents/e\u0301.md", "alpha\n"));
        }
        else
        {
            fixture.WritePackage("helper", "helper", [], (".agents/shared.md", "alpha\n"));
            fixture.WritePackage("toolkit", "toolkit", ["helper"], (".agents/shared.md", "beta\n"));
        }

        var beforeWorkspace = fixture.Workspace.SnapshotHashes();
        var beforeSource = fixture.Source.SnapshotHashes();

        var result = await fixture.InspectAsync();

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == (ExtensionInspectFindingCode)expectedFinding);
        Assert.NotEmpty(result.PathFacts.Declared);
        Assert.Equal(beforeWorkspace, fixture.Workspace.SnapshotHashes());
        Assert.Equal(beforeSource, fixture.Source.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Inspect retains installed facts when the selected source has no requested package"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task PackageUnavailableRetainsInstalledFacts()
    {
        using var fixture = InspectScenario.Create("package-unavailable");
        fixture.WriteInstalledToolkit();
        fixture.WritePackage(string.Empty, "nearby", []);
        var beforeWorkspace = fixture.Workspace.SnapshotHashes();
        var beforeSource = fixture.Source.SnapshotHashes();

        var result = await fixture.InspectAsync();

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(ExtensionInspectInstalledState.Present, result.Installed.State);
        Assert.Equal("toolkit", result.Installed.Package?.Id);
        Assert.Equal(ExtensionInspectAvailableState.Absent, result.Available.State);
        Assert.Null(result.Available.Package);
        Assert.Single(result.PathFacts.Current);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionInspectFindingCode.PackageUnavailable);
        Assert.Equal(beforeWorkspace, fixture.Workspace.SnapshotHashes());
        Assert.Equal(beforeSource, fixture.Source.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Inspect makes a dependency-only trusted three-way divergence actionable"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task DependencyOnlyDivergenceIsActionable()
    {
        using var fixture = InspectScenario.Create("dependency-only-change");
        fixture.WriteInstalledToolkit();
        fixture.WritePackage("helper", "helper", []);
        fixture.WritePackage("toolkit", "toolkit", ["helper"], (".agents/toolkit.md", "alpha\n"));
        var beforeWorkspace = fixture.Workspace.SnapshotHashes();
        var beforeSource = fixture.Source.SnapshotHashes();

        var result = await fixture.InspectAsync();

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Equal(ExtensionInspectDependencyRelation.Changed, result.Comparison.Dependencies.Relation);
        Assert.Equal(
            ExtensionInspectPathRelation.Unchanged,
            Assert.Single(result.Comparison.Paths).Relation);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionInspectFindingCode.DependencyChanged);
        Assert.DoesNotContain(result.Findings, finding => finding.Code == ExtensionInspectFindingCode.PathChanged);
        Assert.Equal("open-forge extension update toolkit", result.Next?.Command);
        Assert.Equal(beforeWorkspace, fixture.Workspace.SnapshotHashes());
        Assert.Equal(beforeSource, fixture.Source.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Inspect preserves an invalid intended generated boundary when current Markdown is valid"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task IntendedGeneratedBoundaryCannotBeHiddenByCurrentFacts()
    {
        using var fixture = InspectScenario.Create("intended-generated-invalid");
        fixture.WriteInstalledToolkit();
        fixture.WritePackage(
            string.Empty,
            "toolkit",
            [],
            (".agents/toolkit.md", "## Entries\n\n## Entries\n"));
        var beforeWorkspace = fixture.Workspace.SnapshotHashes();
        var beforeSource = fixture.Source.SnapshotHashes();

        var result = await fixture.InspectAsync();

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(
            ExtensionInspectGeneratedRegionState.Invalid,
            Assert.Single(result.Generated.Regions).State);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionInspectFindingCode.GeneratedBoundaryInvalid);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionInspectFindingCode.FingerprintFallback);
        Assert.Equal("open-forge doctor", result.Next?.Command);
        Assert.Equal(beforeWorkspace, fixture.Workspace.SnapshotHashes());
        Assert.Equal(beforeSource, fixture.Source.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Inspect keeps unavailable generated boundaries distinct, non-invalid, and incomplete")]
    [Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task UnavailableGeneratedBoundaryRemainsDistinctAndIncomplete()
    {
        using var fixture = InspectScenario.Create("intended-generated-unavailable");
        fixture.WriteInstalledToolkit();
        fixture.WritePackage(
            string.Empty,
            "toolkit",
            [],
            (".agents/toolkit.md", "---\nopen-forge:\n  tags: [One]\n# Missing terminator\n"));

        var result = await fixture.InspectAsync();

        Assert.Equal(
            ExtensionInspectGeneratedRegionState.Unavailable,
            Assert.Single(result.Generated.Regions).State);
        Assert.DoesNotContain(
            result.Findings,
            finding => finding.Code == ExtensionInspectFindingCode.GeneratedBoundaryInvalid);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == ExtensionInspectFindingCode.FingerprintFallback);
        Assert.Equal(ExtensionInspectGeneratedState.Incomplete, result.Generated.State);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Inspect reports opaque payloads as exact bytes without a Markdown fallback"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task OpaquePayloadUsesExactBytesWithoutFallback()
    {
        using var fixture = InspectScenario.Create("unsupported-fallback");
        fixture.WritePackageBytes(
            string.Empty,
            "toolkit",
            [],
            (".agents/toolkit.bin", new byte[] { 0x00, 0xFF, 0x41 }));
        var beforeWorkspace = fixture.Workspace.SnapshotHashes();
        var beforeSource = fixture.Source.SnapshotHashes();

        var result = await fixture.InspectAsync();

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var intended = Assert.Single(result.Comparison.Intended.Fingerprints).Fingerprint;
        Assert.NotNull(intended);
        Assert.Equal(ExtensionInspectFingerprintKind.ExactBytes, intended.Kind);
        Assert.Equal(ExtensionInspectDefinitions.FingerprintPolicy, intended.Policy);
        Assert.DoesNotContain(result.Findings, finding => finding.Code == ExtensionInspectFindingCode.FingerprintFallback);
        Assert.Equal(beforeWorkspace, fixture.Workspace.SnapshotHashes());
        Assert.Equal(beforeSource, fixture.Source.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Inspect fails closed when current and intended fallback findings have the same key"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task DuplicateFallbackFindingFailsClosedWithoutErasingPathFacts()
    {
        using var fixture = InspectScenario.Create("duplicate-fallback");
        fixture.WriteInstalledToolkit(currentContents: [0xC3, 0x28]);
        fixture.WritePackageBytes(
            string.Empty,
            "toolkit",
            [],
            (".agents/toolkit.md", new byte[] { 0xC3, 0x28 }));
        var beforeWorkspace = fixture.Workspace.SnapshotHashes();
        var beforeSource = fixture.Source.SnapshotHashes();

        var result = await fixture.InspectAsync();

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal(
            ExtensionInspectFindingCode.OperationFailed,
            Assert.Single(result.Findings).Code);
        Assert.Equal(ExtensionInspectCurrentPathState.Present, Assert.Single(result.PathFacts.Current).State);
        var comparison = Assert.Single(result.Comparison.Paths);
        Assert.Equal(ExtensionInspectDefinitions.FingerprintPolicy, comparison.Current?.Policy);
        Assert.Equal(ExtensionInspectDefinitions.FingerprintPolicy, comparison.Intended?.Policy);
        Assert.Equal(beforeWorkspace, fixture.Workspace.SnapshotHashes());
        Assert.Equal(beforeSource, fixture.Source.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Inspect events retain each completed read stage and leave later stages not started"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task EventFormationRetainsCompletedStageFacts()
    {
        using var fixture = InspectScenario.Create("staged-event");
        fixture.WriteInstalledToolkit();
        fixture.WritePackage(string.Empty, "toolkit", [], (".agents/toolkit.md", "alpha\n"));
        var workspace = new CliWorkspace(
            lexicalRoot: fixture.Workspace.Path,
            physicalRoot: fixture.Workspace.Path,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var request = new ExtensionInspectRequest(workspace, "toolkit", fixture.Source.Path);
        var physicalPathResolver = new PhysicalPathResolver();
        var source = await new ExtensionSourceReader(physicalPathResolver)
            .ReadAsync(workspace, fixture.Source.Path, CancellationToken.None);
        var lifecycle = await WorkspaceOwnershipReader.ReadAsync(physicalPathResolver, workspace, CancellationToken.None);
        var current = await new ExtensionInspectCurrentPathReader(physicalPathResolver)
            .ReadAsync(workspace, lifecycle.Document.Extensions, "toolkit", CancellationToken.None);
        var builder = new ExtensionInspectResultBuilder(
            new ExtensionInspectComparisonBuilder(new MarkdownFingerprintReader()));

        var afterSource = builder.Event(new ExtensionInspectEventInput
        {
            Request = request,
            Source = source,
            Ownership = null,
            CurrentPaths = null,
            Status = CliSemanticStatus.Interrupted,
            Code = ExtensionInspectFindingCode.Interrupted,
            Cause = "Interrupted after source inspection.",
        });
        var afterLifecycle = builder.Event(new ExtensionInspectEventInput
        {
            Request = request,
            Source = source,
            Ownership = lifecycle,
            CurrentPaths = null,
            Status = CliSemanticStatus.Interrupted,
            Code = ExtensionInspectFindingCode.Interrupted,
            Cause = "Interrupted after lifecycle inspection.",
        });
        var afterCurrent = builder.Event(new ExtensionInspectEventInput
        {
            Request = request,
            Source = source,
            Ownership = lifecycle,
            CurrentPaths = current,
            Status = CliSemanticStatus.Failed,
            Code = ExtensionInspectFindingCode.OperationFailed,
            Cause = "Result formation failed after current-path inspection.",
        });

        Assert.Equal(ExtensionInspectSourceState.Available, afterSource.Source.State);
        Assert.Equal(ExtensionInspectAvailableState.Present, afterSource.Available.State);
        Assert.Equal(ExtensionInspectLifecycleReadState.NotStarted, afterSource.Lifecycle.ReadState);
        Assert.Equal(ExtensionInspectInstalledState.NotStarted, afterSource.Installed.State);
        Assert.Equal(ExtensionInspectLifecycleReadState.Complete, afterLifecycle.Lifecycle.ReadState);
        Assert.Equal(ExtensionInspectInstalledState.Present, afterLifecycle.Installed.State);
        Assert.Equal(ExtensionInspectPathState.NotStarted, afterLifecycle.PathFacts.State);
        Assert.Equal(ExtensionInspectPathState.Complete, afterCurrent.PathFacts.State);
        Assert.Equal(ExtensionInspectCurrentPathState.Present, Assert.Single(afterCurrent.PathFacts.Current).State);
        Assert.Equal(ExtensionInspectComparisonState.NotStarted, afterCurrent.Comparison.State);
        Assert.Equal(ExtensionInspectGeneratedState.NotStarted, afterCurrent.Generated.State);
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Inspect compares current bytes with intended bytes without a stored baseline"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    [InlineData("beta\n", (int)ExtensionInspectPathRelation.Unchanged)]
    [InlineData("beta\r\n", (int)ExtensionInspectPathRelation.Unchanged)]
    [InlineData("local edit\n", (int)ExtensionInspectPathRelation.Changed)]
    public async Task CurrentContentDeterminesTwoWayRelation(string current, int relation)
    {
        using var fixture = InspectScenario.Create("two-way");
        fixture.WriteInstalledToolkit(System.Text.Encoding.UTF8.GetBytes(current));
        fixture.WritePackage(string.Empty, "toolkit", [], (".agents/toolkit.md", "beta\n"));
        fixture.Workspace.WriteText(".agents/open-forge.lifecycle.json", "An unrelated file that must survive.");
        var before = fixture.Workspace.SnapshotHashes();
        var result = await fixture.InspectAsync();
        var comparison = Assert.Single(result.Comparison.Paths);
        Assert.Equal((ExtensionInspectPathRelation)relation, comparison.Relation);
        Assert.NotNull(comparison.Current);
        Assert.NotNull(comparison.Intended);
        Assert.Equal(relation == (int)ExtensionInspectPathRelation.Unchanged,
            comparison.Current.Sha256 == comparison.Intended.Sha256);
        Assert.Equal(before, fixture.Workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Inspect treats uninterpretable ownership as information without adopting matching files"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    [InlineData("missing")]
    [InlineData("invalid")]
    [InlineData("nonordinary")]
    [InlineData("duplicate")]
    [InlineData("cycle")]
    [InlineData("missing-dependency")]
    [InlineData("unsafe-path")]
    public async Task UnknownOwnershipDoesNotGateOrAdopt(string state)
    {
        using var fixture = InspectScenario.Create("unknown-ownership");
        fixture.Workspace.WriteText(".agents/toolkit.md", "alpha\n");
        fixture.WritePackage(string.Empty, "toolkit", [], (".agents/toolkit.md", "alpha\n"));
        var lockPath = ".agents/open-forge.lock.json";
        if (state == "invalid") fixture.Workspace.WriteText(lockPath, "{");
        if (state == "nonordinary") fixture.Workspace.CreateDirectory(lockPath);
        if (state == "duplicate") fixture.Workspace.WriteText(lockPath, """
            {"extensions":[{"id":"toolkit"},{"id":"toolkit"}]}
            """);
        if (state == "cycle") fixture.Workspace.WriteText(lockPath, ToolkitOwnership.Replace("\"dependencies\":[]", "\"dependencies\":[\"toolkit\"]", StringComparison.Ordinal));
        if (state == "missing-dependency") fixture.Workspace.WriteText(lockPath, ToolkitOwnership.Replace("\"dependencies\":[]", "\"dependencies\":[\"missing\"]", StringComparison.Ordinal));
        if (state == "unsafe-path") fixture.Workspace.WriteText(lockPath, ToolkitOwnership.Replace(".agents/toolkit.md", "../outside.md", StringComparison.Ordinal));
        var before = fixture.Workspace.SnapshotHashes();
        var result = await fixture.InspectAsync();
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(ExtensionInspectInstalledState.Unavailable, result.Installed.State);
        Assert.Null(result.Installed.Package);
        Assert.Null(result.Counts.InstalledPackages);
        Assert.Equal(ExtensionInspectComparisonMode.AvailableOnly, result.Comparison.Mode);
        Assert.Empty(result.PathFacts.Current);
        var finding = Assert.Single(result.Findings);
        Assert.Equal(ExtensionInspectFindingCode.OwnershipObservation, finding.Code);
        Assert.Equal(CliSemanticStatus.Complete, finding.Status);
        Assert.Equal(before, fixture.Workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Inspect derives retirement from receipt membership and preserves missing and unavailable path facts"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task RetirementAndMissingContentUseObservedMembership()
    {
        using var retired = InspectScenario.Create("retired-receipt");
        retired.WriteInstalledToolkit();
        retired.WritePackage(string.Empty, "toolkit", []);
        var retirement = await retired.InspectAsync();
        Assert.Equal(ExtensionInspectPathRelation.Retired, Assert.Single(retirement.Comparison.Paths).Relation);

        using var missing = InspectScenario.Create("missing-receipt");
        missing.Workspace.WriteText(".agents/open-forge.lock.json", ToolkitOwnership);
        missing.WritePackage(string.Empty, "toolkit", [], (".agents/toolkit.md", "alpha\n"));
        var missingResult = await missing.InspectAsync();
        Assert.Equal(ExtensionInspectCurrentPathState.Missing, Assert.Single(missingResult.PathFacts.Current).State);
        Assert.Equal(ExtensionInspectPathRelation.Missing, Assert.Single(missingResult.Comparison.Paths).Relation);

        using var unavailable = InspectScenario.Create("unavailable-receipt");
        unavailable.Workspace.WriteText(".agents/open-forge.lock.json", ToolkitOwnership);
        unavailable.Workspace.CreateDirectory(".agents/toolkit.md");
        unavailable.WritePackage(string.Empty, "toolkit", [], (".agents/toolkit.md", "alpha\n"));
        var unavailableResult = await unavailable.InspectAsync();
        Assert.NotEqual(ExtensionInspectPathRelation.Missing, Assert.Single(unavailableResult.Comparison.Paths).Relation);
        Assert.NotEqual(ExtensionInspectPathRelation.Retired, Assert.Single(unavailableResult.Comparison.Paths).Relation);
    }

    private sealed class InspectFixture : IDisposable
    {
        private InspectFixture(
            TemporaryWorkspace workspace,
            TemporaryWorkspace source)
        {
            Workspace = workspace;
            Source = source;
        }

        internal TemporaryWorkspace Workspace { get; }

        internal TemporaryWorkspace Source { get; }

        internal static InspectFixture Create(string intendedContent)
        {
            var workspace = TemporaryWorkspace.Create("extension-inspect-package-workspace");
            var source = TemporaryWorkspace.Create("extension-inspect-package-source");
            try
            {
                workspace.WriteText(".agents/toolkit.md", "alpha\n");
                workspace.WriteText(
                    ".agents/open-forge.lock.json",
                    ToolkitOwnership);
                source.WriteText(
                    "extension.json",
                    """
                    {
                      "id": "toolkit",
                      "name": "Toolkit",
                      "description": "A test Extension package.",
                      "version": "1.0.0",
                      "dependencies": []
                    }
                    """);
                source.WriteText("content/.agents/toolkit.md", intendedContent);
                return new InspectFixture(workspace, source);
            }
            catch
            {
                source.Dispose();
                workspace.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            Source.Dispose();
            Workspace.Dispose();
        }


    }

    private sealed class InspectScenario : IDisposable
    {
        private InspectScenario(TemporaryWorkspace workspace, TemporaryWorkspace source)
        {
            Workspace = workspace;
            Source = source;
        }

        internal TemporaryWorkspace Workspace { get; }

        internal TemporaryWorkspace Source { get; }

        internal static InspectScenario Create(string name)
            => new(
                TemporaryWorkspace.Create($"extension-inspect-{name}-workspace"),
                TemporaryWorkspace.Create($"extension-inspect-{name}-source"));

        internal void WritePackage(
            string directory,
            string id,
            IReadOnlyList<string> dependencies,
            params (string Path, string Content)[] payload)
        {
            var prefix = string.IsNullOrEmpty(directory) ? string.Empty : $"{directory}/";
            var dependencyJson = string.Join(", ", dependencies.Select(dependency => $"\"{dependency}\""));
            Source.WriteText(
                $"{prefix}extension.json",
                $$"""
                {
                  "id": "{{id}}",
                  "name": "{{id}}",
                  "description": "An Extension Inspect scenario package.",
                  "version": "1.0.0",
                  "dependencies": [{{dependencyJson}}]
                }
                """);
            foreach (var (path, content) in payload)
            {
                Source.WriteText($"{prefix}content/{path}", content);
            }
        }

        internal void WritePackageBytes(
            string directory,
            string id,
            IReadOnlyList<string> dependencies,
            params (string Path, byte[] Content)[] payload)
        {
            WritePackage(directory, id, dependencies);
            var prefix = string.IsNullOrEmpty(directory) ? string.Empty : $"{directory}/";
            foreach (var (path, content) in payload)
            {
                Source.WriteBytes($"{prefix}content/{path}", content);
            }
        }

        internal void WriteInstalledToolkit(byte[]? currentContents = null)
        {
            if (currentContents is null)
            {
                Workspace.WriteText(".agents/toolkit.md", "alpha\n");
            }
            else
            {
                Workspace.WriteBytes(".agents/toolkit.md", currentContents);
            }

            Workspace.WriteText(".agents/open-forge.lock.json", ToolkitOwnership);
        }

        internal ValueTask<ExtensionInspectResult> InspectAsync()
        {
            var request = new ExtensionInspectRequest(
                new CliWorkspace(
                    lexicalRoot: Workspace.Path,
                    physicalRoot: Workspace.Path,
                    selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace),
                "toolkit",
                Source.Path);
            return ExtensionInspectOperationFactory.Create().ExecuteAsync(request, CancellationToken.None);
        }

        public void Dispose()
        {
            Source.Dispose();
            Workspace.Dispose();
        }
    }
}
