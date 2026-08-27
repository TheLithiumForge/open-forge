using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Inspect;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Reading;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Inspect;

public sealed class ExtensionInspectApplicationIntegrationTests
{
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
        Assert.Contains("never writes", result.Output, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Extension Inspect keeps exact-byte lifecycle evidence out of semantic comparison and update advice"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task ExactByteBaselineIsReportedButNeverBecomesSemanticUpdateAdvice()
    {
        using var fixture = InspectFixture.Create(
            fingerprintKind: "exact-bytes",
            intendedContent: "beta\n");
        var beforeWorkspace = fixture.Workspace.SnapshotHashes();
        var beforeSource = fixture.Source.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            [
                "extension", "inspect", "toolkit",
                "--workspace", fixture.Workspace.Path,
                "--source", fixture.Source.Path,
                "--json",
            ],
            fixture.Workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);

        var commandResult = root.GetProperty("result");
        Assert.Equal("toolkit", commandResult.GetProperty("subject").GetProperty("id").GetString());
        Assert.Equal("package", commandResult.GetProperty("source").GetProperty("kind").GetString());
        Assert.Equal(fixture.Source.Path, commandResult.GetProperty("source").GetProperty("identity").GetString());
        Assert.Equal("trusted", commandResult.GetProperty("lifecycle").GetProperty("trust").GetString());
        Assert.Equal("open-forge-markdown-v1", commandResult.GetProperty("lifecycle").GetProperty("fingerprintPolicy").GetString());
        Assert.Equal("three-way", commandResult.GetProperty("comparison").GetProperty("mode").GetString());

        var comparisonPath = Assert.Single(commandResult.GetProperty("comparison").GetProperty("paths").EnumerateArray());
        Assert.Equal("exact-bytes", comparisonPath.GetProperty("baseline").GetProperty("kind").GetString());
        Assert.Equal("persisted-baseline", comparisonPath.GetProperty("baseline").GetProperty("origin").GetString());
        Assert.Equal("semantic", comparisonPath.GetProperty("current").GetProperty("kind").GetString());
        Assert.Equal("semantic", comparisonPath.GetProperty("intended").GetProperty("kind").GetString());
        Assert.Equal("unknown", comparisonPath.GetProperty("relation").GetString());
        Assert.DoesNotContain(
            commandResult.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "extension-inspect.path-changed");
        Assert.Equal(beforeWorkspace, fixture.Workspace.SnapshotHashes());
        Assert.Equal(beforeSource, fixture.Source.SnapshotHashes());
    }

    [Fact(DisplayName = "Extension Inspect permits an actionable recommendation only for a semantic baseline"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task SemanticBaselineEnablesOnlyTrustedThreeWayRecommendation()
    {
        using var fixture = InspectFixture.Create(
            fingerprintKind: "semantic",
            intendedContent: "beta\n");
        var beforeWorkspace = fixture.Workspace.SnapshotHashes();
        var beforeSource = fixture.Source.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            [
                "extension", "inspect", "toolkit",
                "--workspace", fixture.Workspace.Path,
                "--source", fixture.Source.Path,
                "--json",
            ],
            fixture.Workspace.Path);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("attention", root.GetProperty("status").GetString());
        Assert.Equal(
            "open-forge extension update toolkit",
            root.GetProperty("next").GetProperty("command").GetString());
        Assert.Equal(
            "extension-inspect.path-changed",
            Assert.Single(root.GetProperty("result").GetProperty("findings").EnumerateArray()).GetProperty("code").GetString());
        Assert.Equal(
            "changed",
            Assert.Single(root.GetProperty("result").GetProperty("comparison").GetProperty("paths").EnumerateArray())
                .GetProperty("relation")
                .GetString());
        Assert.Equal(beforeWorkspace, fixture.Workspace.SnapshotHashes());
        Assert.Equal(beforeSource, fixture.Source.SnapshotHashes());
    }

    [Fact(DisplayName = "Extension Inspect does not fall back to embedded facts when an explicit source is missing"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task MissingExplicitSourceRemainsTheOnlySource()
    {
        using var fixture = InspectFixture.Create(
            fingerprintKind: "semantic",
            intendedContent: "alpha\n");
        var missingSource = fixture.Source.Combine("missing-source");
        var beforeWorkspace = fixture.Workspace.SnapshotHashes();
        var beforeSource = fixture.Source.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            [
                "extension", "inspect", "toolkit",
                "--workspace", fixture.Workspace.Path,
                "--source", missingSource,
                "--json",
            ],
            fixture.Workspace.Path);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("attention", root.GetProperty("status").GetString());
        var source = root.GetProperty("result").GetProperty("source");
        Assert.True(source.GetProperty("explicit").GetBoolean());
        Assert.Equal("missing", source.GetProperty("state").GetString());
        Assert.Equal(missingSource, source.GetProperty("identity").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("result").GetProperty("available").GetProperty("package").ValueKind);
        Assert.Contains(
            root.GetProperty("result").GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "extension-inspect.source-unavailable");
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        Assert.Equal(beforeWorkspace, fixture.Workspace.SnapshotHashes());
        Assert.Equal(beforeSource, fixture.Source.SnapshotHashes());
    }

    [Fact(DisplayName = "Extension Inspect invalid stable IDs keep typed JSON states and empty downstream arrays"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task InvalidStableIdProducesTypedResult()
    {
        using var workspace = TemporaryWorkspace.Create("extension-inspect-invalid-id");
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            ["extension", "inspect", "Toolkit", "--workspace", workspace.Path, "--json"],
            workspace.Path);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("invalid", root.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("workspace").ValueKind);
        var commandResult = root.GetProperty("result");
        Assert.Equal("Toolkit", commandResult.GetProperty("subject").GetProperty("supplied").GetString());
        Assert.Equal("invalid", commandResult.GetProperty("subject").GetProperty("state").GetString());
        Assert.Empty(commandResult.GetProperty("dependencies").GetProperty("declared").EnumerateArray());
        Assert.Empty(commandResult.GetProperty("pathFacts").GetProperty("current").EnumerateArray());
        Assert.Equal(
            "extension-inspect.invalid-stable-id",
            Assert.Single(commandResult.GetProperty("findings").EnumerateArray()).GetProperty("code").GetString());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

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
                "--json",
            ],
            workspace.Path);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("blocked", root.GetProperty("status").GetString());
        Assert.Contains(
            root.GetProperty("result").GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "extension-inspect.source-overlap");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Extension Inspect blocks ambiguous source shape and duplicate active identity without selecting a candidate"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    [InlineData("source-shape", (int)ExtensionInspectFindingCode.SourceAmbiguous, 0)]
    [InlineData("duplicate-identity", (int)ExtensionInspectFindingCode.IdentityAmbiguous, 2)]
    public async Task AmbiguousSourceAndIdentityRemainUnselected(
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

    [Theory(DisplayName = "Extension Inspect distinguishes incomplete cyclic and conflicting dependency closure"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    [InlineData("incomplete", (int)ExtensionInspectFindingCode.DependencyIncomplete, (int)CliSemanticStatus.Incomplete, (int)ExtensionInspectDependencyState.Incomplete)]
    [InlineData("cycle", (int)ExtensionInspectFindingCode.DependencyCycle, (int)CliSemanticStatus.Blocked, (int)ExtensionInspectDependencyState.Blocked)]
    [InlineData("conflict", (int)ExtensionInspectFindingCode.DependencyConflict, (int)CliSemanticStatus.Blocked, (int)ExtensionInspectDependencyState.NotStarted)]
    public async Task DependencyFailuresRetainOnlySafeClosureFacts(
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

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionInspectFindingCode.PackageInvalid);
        Assert.Null(result.Available.Package);
        Assert.Equal(beforeWorkspace, fixture.Workspace.SnapshotHashes());
        Assert.Equal(beforeSource, fixture.Source.SnapshotHashes());
    }

    [Theory(DisplayName = "Extension Inspect blocks unsafe targets and conflicting closure ownership"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    [InlineData("unsafe-target", (int)ExtensionInspectFindingCode.PathInvalid)]
    [InlineData("ownership", (int)ExtensionInspectFindingCode.OwnershipConflict)]
    public async Task UnsafeTargetsAndOwnershipAreBlocked(
        string scenario,
        int expectedFinding)
    {
        using var fixture = InspectScenario.Create(scenario);
        if (scenario == "unsafe-target")
        {
            fixture.WritePackage(string.Empty, "toolkit", [], (".agents/unsafe?.md", "alpha\n"));
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

    [Fact(DisplayName = "Extension Inspect retains installed facts when the selected source has no requested package"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task PackageUnavailableRetainsInstalledFacts()
    {
        using var fixture = InspectScenario.Create("package-unavailable");
        fixture.WriteInstalledToolkit();
        fixture.WritePackage(string.Empty, "nearby", []);
        var beforeWorkspace = fixture.Workspace.SnapshotHashes();
        var beforeSource = fixture.Source.SnapshotHashes();

        var result = await fixture.InspectAsync();

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(ExtensionInspectInstalledState.Present, result.Installed.State);
        Assert.Equal("toolkit", result.Installed.Package?.Id);
        Assert.Equal(ExtensionInspectAvailableState.Absent, result.Available.State);
        Assert.Null(result.Available.Package);
        Assert.Single(result.PathFacts.Current);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionInspectFindingCode.PackageUnavailable);
        Assert.Equal(beforeWorkspace, fixture.Workspace.SnapshotHashes());
        Assert.Equal(beforeSource, fixture.Source.SnapshotHashes());
    }

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

    [Fact(DisplayName = "Extension Inspect preserves an invalid intended generated boundary when current Markdown is valid"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task IntendedGeneratedBoundaryCannotBeHiddenByCurrentFacts()
    {
        using var fixture = InspectScenario.Create("intended-generated-invalid");
        fixture.WriteInstalledToolkit();
        fixture.WritePackage(
            string.Empty,
            "toolkit",
            [],
            (".agents/toolkit.md", "## Entries\n<!-- open-forge:generated-index:bogus -->\n"));
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

    [Fact(DisplayName = "Extension Inspect reports unsupported payloads through the v1 exact-byte fallback"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration")]
    public async Task UnsupportedPayloadUsesSelectedPolicyFallback()
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

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        var intended = Assert.Single(result.Comparison.Intended.Fingerprints).Fingerprint;
        Assert.NotNull(intended);
        Assert.Equal(ExtensionInspectFingerprintKind.ExactBytes, intended.Kind);
        Assert.Equal(ExtensionInspectDefinitions.FingerprintPolicy, intended.Policy);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionInspectFindingCode.FingerprintFallback);
        Assert.Equal(beforeWorkspace, fixture.Workspace.SnapshotHashes());
        Assert.Equal(beforeSource, fixture.Source.SnapshotHashes());
    }

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
        var lifecycle = await new LifecycleDocumentReader(physicalPathResolver)
            .ReadExtensionsAsync(workspace, CancellationToken.None);
        var current = await new ExtensionInspectCurrentPathReader(physicalPathResolver)
            .ReadAsync(workspace, lifecycle.Packages, "toolkit", CancellationToken.None);
        var builder = new ExtensionInspectResultBuilder(
            new ExtensionInspectComparisonBuilder(new MarkdownFingerprintReader()));

        var afterSource = builder.Event(new ExtensionInspectEventInput
        {
            Request = request,
            Source = source,
            Lifecycle = null,
            CurrentPaths = null,
            Status = CliSemanticStatus.Interrupted,
            Code = ExtensionInspectFindingCode.Interrupted,
            Cause = "Interrupted after source inspection.",
        });
        var afterLifecycle = builder.Event(new ExtensionInspectEventInput
        {
            Request = request,
            Source = source,
            Lifecycle = lifecycle,
            CurrentPaths = null,
            Status = CliSemanticStatus.Interrupted,
            Code = ExtensionInspectFindingCode.Interrupted,
            Cause = "Interrupted after lifecycle inspection.",
        });
        var afterCurrent = builder.Event(new ExtensionInspectEventInput
        {
            Request = request,
            Source = source,
            Lifecycle = lifecycle,
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

        internal static InspectFixture Create(string fingerprintKind, string intendedContent)
        {
            var workspace = TemporaryWorkspace.Create("extension-inspect-package-workspace");
            var source = TemporaryWorkspace.Create("extension-inspect-package-source");
            try
            {
                workspace.WriteText(".agents/toolkit.md", "alpha\n");
                workspace.WriteText(
                    ".agents/open-forge.lifecycle.json",
                    Lifecycle(workspace.Path, fingerprintKind));
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
                source.WriteText("payload/.agents/toolkit.md", intendedContent);
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

        private static string Lifecycle(string workspacePath, string fingerprintKind)
            => $$"""
                {
                  "schemaVersion": 1,
                  "fingerprintPolicy": "open-forge-markdown-v1",
                  "workspacePath": "{{JsonEncodedText.Encode(System.IO.Path.GetFullPath(workspacePath))}}",
                  "framework": null,
                  "extensions": {
                    "coverage": "complete",
                    "packages": [{
                      "id": "toolkit",
                      "version": "1.0.0",
                      "source": "embedded catalogue",
                      "dependencies": [],
                      "paths": [".agents/toolkit.md"]
                    }],
                    "paths": [{
                      "path": ".agents/toolkit.md",
                      "owners": ["toolkit"],
                      "baselineFingerprint": "b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060",
                      "fingerprintKind": "{{fingerprintKind}}"
                    }]
                  }
                }
                """;
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
                Source.WriteText($"{prefix}payload/{path}", content);
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
                Source.WriteBytes($"{prefix}payload/{path}", content);
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

            Workspace.WriteText(
                ".agents/open-forge.lifecycle.json",
                $$"""
                {
                  "schemaVersion": 1,
                  "fingerprintPolicy": "open-forge-markdown-v1",
                  "workspacePath": "{{JsonEncodedText.Encode(System.IO.Path.GetFullPath(Workspace.Path))}}",
                  "framework": null,
                  "extensions": {
                    "coverage": "complete",
                    "packages": [{
                      "id": "toolkit",
                      "version": "1.0.0",
                      "source": "embedded catalogue",
                      "dependencies": [],
                      "paths": [".agents/toolkit.md"]
                    }],
                    "paths": [{
                      "path": ".agents/toolkit.md",
                      "owners": ["toolkit"],
                      "baselineFingerprint": "b6a98d9ce9a2d9149288fa3df42d377c3e42737afdcdaf714e33c0a100b51060",
                      "fingerprintKind": "semantic"
                    }]
                  }
                }
                """);
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
