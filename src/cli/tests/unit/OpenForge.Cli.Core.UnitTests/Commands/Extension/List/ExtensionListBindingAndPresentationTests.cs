using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.List.Models;
using OpenForge.Cli.Core.Commands.Extension.List.Shared.Result;
using OpenForge.Cli.Core.Commands.Extension.List;
using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Extension.List;
using OpenForge.Cli.Core.Presentation.Extension.List.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.List;

public sealed class ExtensionListBindingAndPresentationTests
{
    private static string WorkspaceRoot { get; } = Path.GetFullPath(
        Path.Combine(Path.GetTempPath(), "open-forge-extension-list-workspace"));

    [Fact(DisplayName = "Extension List symbols compose one group leaf and exact local options"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void SymbolsComposeExactGrammar()
    {
        var group = ExtensionBinding.CreateGroup();
        var symbols = ExtensionListBinding.CreateSymbols(group);
        var parse = group.Parse("list --installed --installed --available --source catalogue");

        Assert.Empty(parse.Errors);
        Assert.Same(symbols.ListCommand, parse.CommandResult.Command);
        Assert.True(parse.GetValue(symbols.Installed));
        Assert.True(parse.GetValue(symbols.Available));
        Assert.Equal("catalogue", parse.GetValue(symbols.Source));
        Assert.Equal(3, symbols.ListCommand.Options.Count);
    }

    [Fact(DisplayName = "Extension List JSON preserves the schema envelope and complete command graph"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void JsonPreservesSchemaEnvelopeAndGraph()
    {
        var result = CreateResult();
        var selected = CliReportSelection.Select(
            result,
            new CliSelection(CliDetail.Standard),
            ExtensionListPresentation.Rendering);
        var json = CliJsonRenderer.Render(selected, ExtensionListPresentation.Rendering.DataJsonTypeInfo);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("extension list", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        var commandResult = root.GetProperty("data");
        Assert.Single(commandResult.GetProperty("installed").EnumerateArray());
        Assert.Single(commandResult.GetProperty("available").EnumerateArray());
        Assert.Equal("embedded-catalogue", commandResult.GetProperty("source").GetProperty("kind").GetString());
        Assert.False(Assert.Single(commandResult.GetProperty("available").EnumerateArray()).TryGetProperty("installed", out _));
        Assert.Equal("open-forge extension install <id>", root.GetProperty("next").GetProperty("command").GetString());
    }

    [Fact(DisplayName = "Extension List JSON retains nullable full installed fields at their selected detail"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void JsonDataPresenceFollowsDetail()
    {
        var result = CreateResult();

        using var minimalDocument = JsonDocument.Parse(RenderJson(result, CliDetail.Minimal));
        var minimalRow = Assert.Single(minimalDocument.RootElement.GetProperty("data").GetProperty("installed").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, minimalRow.GetProperty("note").ValueKind);
        Assert.False(minimalRow.TryGetProperty("files", out _));
        Assert.False(minimalRow.TryGetProperty("recordedSource", out _));
        Assert.False(minimalRow.TryGetProperty("coverage", out _));

        using var fullDocument = JsonDocument.Parse(RenderJson(result, CliDetail.Full));
        var fullRow = Assert.Single(fullDocument.RootElement.GetProperty("data").GetProperty("installed").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, fullRow.GetProperty("note").ValueKind);
        Assert.Equal(JsonValueKind.Null, fullRow.GetProperty("recordedSource").ValueKind);
        Assert.Equal(2, fullRow.GetProperty("files").GetInt32());
        Assert.Equal("complete", fullRow.GetProperty("coverage").GetString());
    }

    [Fact(DisplayName = "Extension List human views retain section identity status and safe rows"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void HumanViewsRetainSectionsAndStatus()
    {
        var result = CreateResult();
        var compact = RenderText(result, CliDetail.Minimal);
        var expanded = RenderText(result, CliDetail.Standard);

        Assert.Contains("Installed", compact, StringComparison.Ordinal);
        Assert.Contains("toolkit", compact, StringComparison.Ordinal);
        Assert.Contains("A toolkit.", compact, StringComparison.Ordinal);
        Assert.Contains("Source: embedded catalogue", expanded, StringComparison.Ordinal);
        Assert.DoesNotContain("embedded-catalogue", expanded, StringComparison.Ordinal);
        Assert.Contains("Next: open-forge extension install <id>", expanded, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Extension List maps cancelled source discovery to interrupted"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void CancelledDiscoveryStateIsInterrupted()
    {
        var request = new ExtensionListRequest
        {
            Workspace = new CliWorkspace(
                lexicalRoot: WorkspaceRoot,
                physicalRoot: WorkspaceRoot,
                selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace),
            Selection = ExtensionListSelection.Create(installedFlag: true, availableFlag: true),
            ExplicitSource = null,
        };
        var source = new ExtensionSourceReadResult(
            state: ExtensionSourceReadState.Cancelled,
            kind: ExtensionSourceKind.EmbeddedCatalogue,
            identity: "embedded catalogue",
            packages: [],
            cause: "Source interrupted.");
        var lifecycle = WorkspaceOwnershipRead.Absent(Path.Combine(WorkspaceRoot, ".agents", "open-forge.lock.json"));

        var result = ExtensionListResultBuilder.Build(request, source, lifecycle);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionListFindingCode.Interrupted);
    }

    [Theory(DisplayName = "Extension List distinguishes a known empty installation from unavailable inventory"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    [InlineData(ExtensionListCoverage.Complete, "none")]
    [InlineData(ExtensionListCoverage.Incomplete, "unavailable")]
    public void EmptyCoverageIsHonest(object coverageValue, string expected)
    {
        var coverage = (ExtensionListCoverage)coverageValue;
        var result = new ExtensionListResult(
            status: coverage == ExtensionListCoverage.Complete ? CliSemanticStatus.Complete : CliSemanticStatus.Incomplete,
            workspace: null,
            selection: ExtensionListSelection.Create(installedFlag: true, availableFlag: false),
            source: null,
            lifecycleTrust: coverage == ExtensionListCoverage.Complete ? ExtensionListOwnershipTrust.Trusted : ExtensionListOwnershipTrust.Incomplete,
            installedCoverage: coverage,
            availableCoverage: ExtensionListCoverage.NotRequested,
            installed: [], available: [], findings: [], next: null);
        foreach (var view in new[] { CliDetail.Minimal, CliDetail.Standard })
        {
            var text = RenderText(result, view);
            Assert.Contains(expected, text, StringComparison.Ordinal);
            Assert.DoesNotContain("Available", text, StringComparison.Ordinal);
            if (coverage == ExtensionListCoverage.Incomplete)
            {
                Assert.DoesNotContain("none", text, StringComparison.Ordinal);
            }
        }
    }

    [Theory(DisplayName = "Extension List projects typed selected-source failures consistently across detail levels"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    [InlineData((int)ExtensionSourceFailureDetailKind.InvalidManifest, (int)ExtensionSourceReadState.Invalid, (int)CliSemanticStatus.Invalid, (int)ExtensionListFindingCode.SourceInvalid, "is not a valid Extension manifest.", "Check the manifest's required fields and values, then retry.")]
    [InlineData((int)ExtensionSourceFailureDetailKind.InvalidEncoding, (int)ExtensionSourceReadState.Invalid, (int)CliSemanticStatus.Invalid, (int)ExtensionListFindingCode.SourceInvalid, "is not valid UTF-8.", "Save the manifest as UTF-8, then retry.")]
    [InlineData((int)ExtensionSourceFailureDetailKind.AccessDenied, (int)ExtensionSourceReadState.Unavailable, (int)CliSemanticStatus.Incomplete, (int)ExtensionListFindingCode.SourceUnavailable, "could not be read: permission was denied.", "Check read access to the named file, then retry.")]
    [InlineData((int)ExtensionSourceFailureDetailKind.FileInUse, (int)ExtensionSourceReadState.Unavailable, (int)CliSemanticStatus.Incomplete, (int)ExtensionListFindingCode.SourceUnavailable, "could not be read because it is in use.", "Close the program holding the file, then retry.")]
    [InlineData((int)ExtensionSourceFailureDetailKind.InputOutput, (int)ExtensionSourceReadState.Unavailable, (int)CliSemanticStatus.Incomplete, (int)ExtensionListFindingCode.SourceUnavailable, "could not be read because a filesystem operation failed.", "Check that the file is accessible, then retry.")]
    public void TypedSelectedSourceFailureKeepsFactsAndEvidenceStable(
        int detailKindValue,
        int sourceStateValue,
        int expectedStatusValue,
        int expectedFindingCodeValue,
        string expectedMessageSuffix,
        string expectedAdvice)
    {
        var detailKind = (ExtensionSourceFailureDetailKind)detailKindValue;
        var sourceState = (ExtensionSourceReadState)sourceStateValue;
        var expectedStatus = (CliSemanticStatus)expectedStatusValue;
        var expectedFindingCode = (ExtensionListFindingCode)expectedFindingCodeValue;
        var sourceIdentity = Path.Combine(WorkspaceRoot, "catalogue");
        var manifestPath = Path.Combine(sourceIdentity, "toolkit", "extension.json");
        var rawCause = $"The reader reported a raw cause for {detailKind}.";
        var request = new ExtensionListRequest
        {
            Workspace = new CliWorkspace(
                lexicalRoot: WorkspaceRoot,
                physicalRoot: WorkspaceRoot,
                selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace),
            Selection = ExtensionListSelection.Create(installedFlag: true, availableFlag: true),
            ExplicitSource = sourceIdentity,
        };
        var detail = new ExtensionSourceFailureDetail(manifestPath, detailKind);
        var source = new ExtensionSourceReadResult(
            state: sourceState,
            kind: ExtensionSourceKind.Catalogue,
            identity: sourceIdentity,
            packages: [],
            cause: rawCause,
            failureKind: sourceState == ExtensionSourceReadState.Invalid
                ? ExtensionSourceFailureKind.PackageInvalid
                : ExtensionSourceFailureKind.Unavailable)
        {
            FailureDetail = detail,
        };
        var lifecycle = WorkspaceOwnershipRead.Absent(Path.Combine(WorkspaceRoot, ".agents", "open-forge.lock.json"));
        var result = ExtensionListResultBuilder.Build(request, source, lifecycle);

        Assert.Equal(sourceIdentity, result.Source?.Identity);
        var finding = Assert.Single(result.Findings, value => value.Code == expectedFindingCode);
        Assert.Equal(sourceIdentity, finding.Subject);
        Assert.Null(finding.Path);
        Assert.Equal(manifestPath, finding.FailureDetail?.Path);

        var expectedMessage = $"{manifestPath} {expectedMessageSuffix}";
        var expectedHeadline = $"Available Extensions could not be listed from {manifestPath}.";
        var expectedStatusName = CliStatusDefinitions.Read(expectedStatus).MachineName;
        var expectedFindingMachineCode = ExtensionListDefinitions.ReadFindingCode(expectedFindingCode);
        foreach (var detailLevel in new[] { CliDetail.Minimal, CliDetail.Standard, CliDetail.Full, CliDetail.Debug })
        {
            var selected = CliReportSelection.Select(
                result,
                new CliSelection(detailLevel),
                ExtensionListPresentation.Rendering);
            var report = selected.Report;
            Assert.Equal(expectedStatus, report.Status);
            Assert.Equal(expectedHeadline, report.Headline.Sentence);
            Assert.Equal(
                expectedStatus == CliSemanticStatus.Invalid ? CliHeadlineKind.CannotStart : CliHeadlineKind.Incomplete,
                report.Headline.Kind);
            var projectedFinding = Assert.Single(report.Findings, value => value.Code == expectedFindingMachineCode);
            Assert.Equal(expectedMessage, projectedFinding.Message);
            var availableCount = Assert.Single(report.Counts, value => value.Name == "available");
            Assert.Null(availableCount.Value);
            Assert.Equal(expectedMessage, availableCount.UnavailableReason);
            Assert.NotNull(report.Next);
            Assert.Equal(CliNextActionKind.Sentence, report.Next!.Kind);
            Assert.Equal(expectedAdvice, report.Next.Command);
            Assert.Equal(expectedAdvice, report.Next.Reason);

            var text = RenderText(result, detailLevel);
            Assert.Contains(expectedHeadline, text, StringComparison.Ordinal);
            Assert.Contains(expectedMessage, text, StringComparison.Ordinal);
            Assert.Contains(expectedAdvice, text, StringComparison.Ordinal);
            if (detailLevel >= CliDetail.Full)
            {
                Assert.Contains(rawCause, text, StringComparison.Ordinal);
            }
            else
            {
                Assert.DoesNotContain(rawCause, text, StringComparison.Ordinal);
            }

            var json = RenderJson(result, detailLevel);
            Assert.DoesNotContain("failureDetail", json, StringComparison.Ordinal);
            Assert.DoesNotContain("failure-detail", json, StringComparison.Ordinal);
            if (detailLevel >= CliDetail.Full)
            {
                Assert.Contains(rawCause, json, StringComparison.Ordinal);
            }
            else
            {
                Assert.DoesNotContain(rawCause, json, StringComparison.Ordinal);
            }

            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            Assert.Equal(expectedStatusName, root.GetProperty("status").GetString());
            Assert.Equal(expectedHeadline, root.GetProperty("summary").GetProperty("headline").GetString());
            var jsonFinding = Assert.Single(root.GetProperty("findings").EnumerateArray(), value => value.GetProperty("code").GetString() == expectedFindingMachineCode);
            Assert.Equal(expectedMessage, jsonFinding.GetProperty("message").GetString());
            var next = root.GetProperty("next");
            Assert.Equal("sentence", next.GetProperty("kind").GetString());
            Assert.Equal(expectedAdvice, next.GetProperty("command").GetString());
            Assert.Equal(expectedAdvice, next.GetProperty("reason").GetString());
        }
    }

    [Fact(DisplayName = "Extension List retains semantic source causes without typed detail"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void UntypedSemanticSourceFailureRetainsExistingProjection()
    {
        var request = new ExtensionListRequest
        {
            Workspace = new CliWorkspace(
                lexicalRoot: WorkspaceRoot,
                physicalRoot: WorkspaceRoot,
                selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace),
            Selection = ExtensionListSelection.Create(installedFlag: true, availableFlag: true),
            ExplicitSource = "catalogue",
        };
        var rawCause = "The selected source has a dependency conflict.";
        var result = ExtensionListResultBuilder.Build(
            request,
            new ExtensionSourceReadResult(
                state: ExtensionSourceReadState.Invalid,
                kind: ExtensionSourceKind.Catalogue,
                identity: "catalogue",
                packages: [],
                cause: rawCause,
                failureKind: ExtensionSourceFailureKind.DependencyConflict),
            WorkspaceOwnershipRead.Absent(Path.Combine(WorkspaceRoot, ".agents", "open-forge.lock.json")));

        var selected = CliReportSelection.Select(
            result,
            new CliSelection(CliDetail.Standard),
            ExtensionListPresentation.Rendering);
        var finding = Assert.Single(result.Findings, value => value.Code == ExtensionListFindingCode.SourceInvalid);
        Assert.Null(finding.FailureDetail);
        var projected = Assert.Single(selected.Report.Findings, value => value.Code == finding.MachineCode);
        Assert.Contains(rawCause, projected.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("is not a valid Extension manifest.", projected.Message, StringComparison.Ordinal);
        Assert.Null(selected.Report.Next);
    }

    [Theory(DisplayName = "Extension List selects manifest advice before installed inspection only when typed detail applies"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    [InlineData(false)]
    [InlineData(true)]
    public void SelectedSourceDetailControlsAdviceBeforeInstalledInspection(bool typedFailure)
    {
        var result = new ExtensionListResult(
            status: CliSemanticStatus.Invalid,
            workspace: null,
            selection: ExtensionListSelection.Create(installedFlag: true, availableFlag: true),
            source: null,
            lifecycleTrust: ExtensionListOwnershipTrust.Incomplete,
            installedCoverage: ExtensionListCoverage.Incomplete,
            availableCoverage: ExtensionListCoverage.Incomplete,
            installed: [],
            available: [],
            findings:
            [
                new ExtensionListFinding(
                    ExtensionListFindingCode.SourceInvalid,
                    CliSemanticStatus.Invalid,
                    "catalogue",
                    "The selected source could not be read.")
                {
                    FailureDetail = typedFailure
                        ? new("catalogue/extension.json", ExtensionListSourceFailureDetailKind.InvalidManifest)
                        : null,
                },
                new ExtensionListFinding(
                    ExtensionListFindingCode.InstalledSourceMissing,
                    CliSemanticStatus.Attention,
                    "toolkit",
                    "The recorded source is missing.")
                {
                    Owner = "toolkit",
                    Path = "missing-source",
                },
            ],
            next: null);

        var selected = CliReportSelection.Select(
            result,
            new CliSelection(CliDetail.Standard),
            ExtensionListPresentation.Rendering);

        Assert.NotNull(selected.Report.Next);
        Assert.Equal(typedFailure
            ? "Check the manifest's required fields and values, then retry."
            : "open-forge extension inspect toolkit", selected.Report.Next!.Command);
        Assert.Equal(typedFailure ? CliNextActionKind.Sentence : CliNextActionKind.Command, selected.Report.Next.Kind);
    }

    [Theory(DisplayName = "Installed-only failures do not claim an unselected Available section"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    [InlineData((int)ExtensionSourceReadState.Invalid, (int)ExtensionSourceFailureDetailKind.InvalidManifest, (int)CliSemanticStatus.Invalid)]
    [InlineData((int)ExtensionSourceReadState.Unavailable, (int)ExtensionSourceFailureDetailKind.FileInUse, (int)CliSemanticStatus.Attention)]
    public void InstalledOnlyTypedFailuresPreserveSelection(int stateValue, int kindValue, int statusValue)
    {
        var sourcePath = Path.Combine(WorkspaceRoot, "catalogue");
        var manifestPath = Path.Combine(sourcePath, "extension.json");
        var request = new ExtensionListRequest
        {
            Workspace = new CliWorkspace(WorkspaceRoot, WorkspaceRoot, CliWorkspaceSelectionMethod.ExplicitWorkspace),
            Selection = ExtensionListSelection.Create(installedFlag: true, availableFlag: false),
            ExplicitSource = sourcePath,
        };
        var source = new ExtensionSourceReadResult(
            (ExtensionSourceReadState)stateValue,
            ExtensionSourceKind.Catalogue,
            sourcePath,
            [],
            "The selected manifest could not be read.")
        {
            FailureDetail = new(manifestPath, (ExtensionSourceFailureDetailKind)kindValue),
        };
        var lifecycle = WorkspaceOwnershipRead.Absent(Path.Combine(WorkspaceRoot, ".agents", "open-forge.lock.json"));
        var result = ExtensionListResultBuilder.Build(request, source, lifecycle);

        foreach (var detail in new[] { CliDetail.Minimal, CliDetail.Standard, CliDetail.Full, CliDetail.Debug })
        {
            var selected = CliReportSelection.Select(result, new(detail), ExtensionListPresentation.Rendering);
            Assert.Equal((CliSemanticStatus)statusValue, selected.Report.Status);
            Assert.DoesNotContain("Available", RenderText(result, detail), StringComparison.Ordinal);
            using var document = JsonDocument.Parse(RenderJson(result, detail));
            Assert.DoesNotContain("Available", document.RootElement.GetProperty("summary").GetProperty("headline").GetString(), StringComparison.Ordinal);
            Assert.Contains(selected.Report.Findings, finding => finding.Message.Contains(manifestPath, StringComparison.Ordinal));
            Assert.Equal(CliNextActionKind.Sentence, selected.Report.Next?.Kind);
        }
    }

    private static ExtensionListResult CreateResult()
        => new(
            status: CliSemanticStatus.Complete,
            workspace: new CliWorkspace(
                lexicalRoot: WorkspaceRoot,
                physicalRoot: WorkspaceRoot,
                selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace),
            selection: ExtensionListSelection.Create(installedFlag: true, availableFlag: true),
            source: new ExtensionListSource
            {
                Identity = "embedded catalogue",
                Kind = ExtensionListSourceKind.EmbeddedCatalogue,
                State = ExtensionListSourceState.Complete,
            },
            lifecycleTrust: ExtensionListOwnershipTrust.Trusted,
            installedCoverage: ExtensionListCoverage.Complete,
            availableCoverage: ExtensionListCoverage.Complete,
            installed:
            [
                new ExtensionListInstalledRow
                {
                    Id = "toolkit",
                    Version = "1.0.0",
                    Trust = ExtensionListOwnershipTrust.Trusted,
                    ManagedPathCount = 2,
                    SourceAvailable = true,
                },
            ],
            available:
            [
                new ExtensionListAvailableRow
                {
                    Id = "toolkit",
                    Name = "Toolkit",
                    Description = "A toolkit.",
                    Version = "1.0.0",
                    PackageCount = 1,
                    DependencyCount = 0,
                    InstalledVersion = "1.0.0",
                },
            ],
            findings: [],
            next: null);

    private static string RenderText(ExtensionListResult result, CliDetail detail)
    {
        var selected = CliReportSelection.Select(result, new CliSelection(detail), ExtensionListPresentation.Rendering);
        return CliTextRenderer.Render(selected, CliTextStyle.Plain, ExtensionListPresentation.Rendering.DataTextRenderer).Content.TrimEnd();
    }

    private static string RenderJson(ExtensionListResult result, CliDetail detail)
    {
        var selected = CliReportSelection.Select(result, new CliSelection(detail), ExtensionListPresentation.Rendering);
        return CliJsonRenderer.Render(selected, ExtensionListPresentation.Rendering.DataJsonTypeInfo);
    }
}
