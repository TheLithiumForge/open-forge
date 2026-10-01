using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.List.Models;
using OpenForge.Cli.Core.Presentation.Extension.List;
using OpenForge.Cli.Core.Presentation.Extension.List.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Extension.List;

[Trait("Feature", "task32-presentation"), Trait("Evidence", "Unit")]
public sealed class Task32ExtensionListNextPolicyTests
{
    private static readonly CliDetail[] Details =
    [CliDetail.Minimal, CliDetail.Standard, CliDetail.Full, CliDetail.Debug];

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension List healthy available hint is hidden only from minimal text")]
    public void CompleteAvailableHintIsTextOnlyAtMinimal()
    {
        var result = CompleteAvailableResult();
        foreach (var detail in Details)
        {
            var rendered = Render(result, detail);
            using var json = JsonDocument.Parse(rendered.Json);
            var next = json.RootElement.GetProperty("next");

            Assert.True(rendered.Selected.Report.Data.IsHealthyAvailableHint);
            Assert.Equal("open-forge extension install <id>", next.GetProperty("command").GetString());
            Assert.Equal("Install one of the available Extensions.", next.GetProperty("reason").GetString());
            Assert.False(
                json.RootElement.GetProperty("data").TryGetProperty("isHealthyAvailableHint", out _));
            if (detail == CliDetail.Minimal)
            {
                Assert.DoesNotContain("Next:", rendered.Text, StringComparison.Ordinal);
            }
            else
            {
                Assert.Contains("Next: open-forge extension install <id>", rendered.Text, StringComparison.Ordinal);
            }
        }
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension List installed-owner warning keeps its minimal next action when filtered")]
    public void InstalledOwnerNextSurvivesAnErrorsOnlyFilter()
    {
        var result = InstalledOwnerWarningResult();
        var rendered = Render(
            result,
            CliDetail.Minimal,
            new HashSet<CliSeverity> { CliSeverity.Error });

        Assert.False(rendered.Selected.Report.Data.IsHealthyAvailableHint);
        Assert.Empty(rendered.Selected.Report.Findings);
        Assert.Equal("open-forge extension inspect legacy", rendered.Selected.Report.Next?.Command);
        Assert.Contains("Next: open-forge extension inspect legacy", rendered.Text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension List source failure retains typed remediation at minimal detail")]
    public void SourceFailureDetailRemediationRemainsVisible()
    {
        var rendered = Render(SourceFailureResult(), CliDetail.Minimal);

        Assert.False(rendered.Selected.Report.Data.IsHealthyAvailableHint);
        Assert.Equal("Check read access to the named file, then retry.", rendered.Selected.Report.Next?.Command);
        Assert.Contains("Next: Check read access to the named file, then retry.", rendered.Text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension List blocked source does not fall through to its generic available hint")]
    public void SourceBlockedKeepsItsTypedNoNextResult()
    {
        var rendered = Render(SourceBlockedResult(), CliDetail.Minimal);

        Assert.False(rendered.Selected.Report.Data.IsHealthyAvailableHint);
        Assert.Null(rendered.Selected.Report.Next);
        Assert.DoesNotContain("Next:", rendered.Text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension List attention result retains its generic available hint")]
    public void AttentionAvailableHintRemainsVisibleAtMinimal()
    {
        var rendered = Render(AttentionAvailableResult(), CliDetail.Minimal);

        Assert.False(rendered.Selected.Report.Data.IsHealthyAvailableHint);
        Assert.Equal("open-forge extension install <id>", rendered.Selected.Report.Next?.Command);
        Assert.Contains("Next: open-forge extension install <id>", rendered.Text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension List incomplete result preserves its typed next action")]
    public void IncompleteResultKeepsItsExistingNextAction()
    {
        var rendered = Render(IncompleteResult(), CliDetail.Minimal);

        Assert.False(rendered.Selected.Report.Data.IsHealthyAvailableHint);
        Assert.Equal("open-forge doctor", rendered.Selected.Report.Next?.Command);
        Assert.Equal("Review the incomplete Extension List result.", rendered.Selected.Report.Next?.Reason);
        Assert.Contains("Next: open-forge doctor", rendered.Text, StringComparison.Ordinal);
    }

    private static ExtensionListResult CompleteAvailableResult()
        => new(
            status: CliSemanticStatus.Complete,
            workspace: null,
            selection: ExtensionListSelection.Create(installedFlag: false, availableFlag: true),
            source: AvailableSource(),
            lifecycleTrust: null,
            installedCoverage: ExtensionListCoverage.NotRequested,
            availableCoverage: ExtensionListCoverage.Complete,
            installed: [],
            available: [AvailableRow()],
            findings: [],
            next: null);

    private static ExtensionListResult InstalledOwnerWarningResult()
        => new(
            status: CliSemanticStatus.Attention,
            workspace: null,
            selection: ExtensionListSelection.Create(installedFlag: true, availableFlag: true),
            source: AvailableSource(),
            lifecycleTrust: ExtensionListOwnershipTrust.Trusted,
            installedCoverage: ExtensionListCoverage.Complete,
            availableCoverage: ExtensionListCoverage.Complete,
            installed:
            [
                new ExtensionListInstalledRow
                {
                    Id = "legacy",
                    Version = "1.0.0",
                    Trust = ExtensionListOwnershipTrust.Trusted,
                    ManagedPathCount = 1,
                    SourceAvailable = false,
                    SourceState = ExtensionListSourceState.Missing,
                    PackageState = ExtensionListInstalledPackageState.Missing,
                },
            ],
            available: [AvailableRow()],
            findings:
            [
                new ExtensionListFinding(
                    ExtensionListFindingCode.InstalledSourceMissing,
                    CliSemanticStatus.Attention,
                    "legacy",
                    "The installed Extension source is missing.")
                {
                    Owner = "legacy",
                    Path = ".agents/extensions/legacy/extension.json",
                },
            ],
            next: new CliNextAction("fallback", "Use the fallback action."));

    private static ExtensionListResult SourceFailureResult()
        => new(
            status: CliSemanticStatus.Incomplete,
            workspace: null,
            selection: ExtensionListSelection.Create(installedFlag: false, availableFlag: true),
            source: new ExtensionListSource
            {
                Identity = "catalogue.json",
                Kind = ExtensionListSourceKind.Catalogue,
                State = ExtensionListSourceState.Unavailable,
            },
            lifecycleTrust: null,
            installedCoverage: ExtensionListCoverage.NotRequested,
            availableCoverage: ExtensionListCoverage.Incomplete,
            installed: [],
            available: [],
            findings:
            [
                new ExtensionListFinding(
                    ExtensionListFindingCode.SourceUnavailable,
                    CliSemanticStatus.Incomplete,
                    "catalogue.json",
                    "The named catalogue cannot be read.")
                {
                    FailureDetail = new ExtensionListSourceFailureDetail(
                        "catalogue.json",
                        ExtensionListSourceFailureDetailKind.AccessDenied),
                },
            ],
            next: new CliNextAction("fallback", "Use the fallback action."));

    private static ExtensionListResult SourceBlockedResult()
        => new(
            status: CliSemanticStatus.Blocked,
            workspace: null,
            selection: ExtensionListSelection.Create(installedFlag: false, availableFlag: true),
            source: new ExtensionListSource
            {
                Identity = "catalogue.json",
                Kind = ExtensionListSourceKind.Catalogue,
                State = ExtensionListSourceState.Blocked,
            },
            lifecycleTrust: null,
            installedCoverage: ExtensionListCoverage.NotRequested,
            availableCoverage: ExtensionListCoverage.Blocked,
            installed: [],
            available: [AvailableRow()],
            findings:
            [
                new ExtensionListFinding(
                    ExtensionListFindingCode.SourceBlocked,
                    CliSemanticStatus.Blocked,
                    "catalogue.json",
                    "The selected catalogue is blocked by workspace policy."),
            ],
            next: new CliNextAction("fallback", "Use the fallback action."));

    private static ExtensionListResult AttentionAvailableResult()
        => new(
            status: CliSemanticStatus.Attention,
            workspace: null,
            selection: ExtensionListSelection.Create(installedFlag: false, availableFlag: true),
            source: AvailableSource(),
            lifecycleTrust: null,
            installedCoverage: ExtensionListCoverage.NotRequested,
            availableCoverage: ExtensionListCoverage.Complete,
            installed: [],
            available: [AvailableRow()],
            findings:
            [
                new ExtensionListFinding(
                    ExtensionListFindingCode.OwnershipObservation,
                    CliSemanticStatus.Attention,
                    ".agents/open-forge.lock.json",
                    "No Extension ownership is recorded."),
            ],
            next: new CliNextAction("fallback", "Use the fallback action."));

    private static ExtensionListResult IncompleteResult()
        => new(
            status: CliSemanticStatus.Incomplete,
            workspace: null,
            selection: ExtensionListSelection.Create(installedFlag: true, availableFlag: false),
            source: null,
            lifecycleTrust: null,
            installedCoverage: ExtensionListCoverage.Incomplete,
            availableCoverage: ExtensionListCoverage.NotRequested,
            installed: [],
            available: [],
            findings:
            [
                new ExtensionListFinding(
                    ExtensionListFindingCode.WorkspaceUnavailable,
                    CliSemanticStatus.Incomplete,
                    "workspace",
                    "The workspace could not be read."),
            ],
            next: new CliNextAction("open-forge doctor", "Review the incomplete Extension List result."));

    private static ExtensionListSource AvailableSource()
        => new()
        {
            Identity = "embedded catalogue",
            Kind = ExtensionListSourceKind.EmbeddedCatalogue,
            State = ExtensionListSourceState.Complete,
        };

    private static ExtensionListAvailableRow AvailableRow()
        => new()
        {
            Id = "toolkit",
            Name = "Toolkit",
            Description = "Review helpers.",
            Version = "2.0.0",
            PackageCount = 1,
            DependencyCount = 0,
        };

    private static RenderedResult Render(
        ExtensionListResult result,
        CliDetail detail,
        IReadOnlySet<CliSeverity>? filter = null)
    {
        var rendering = ExtensionListPresentation.Rendering;
        var selected = CliReportSelection.Select(result, new CliSelection(detail, filter), rendering);
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, rendering.DataTextRenderer).Content;
        var json = CliJsonRenderer.Render(selected, rendering.DataJsonTypeInfo);
        return new RenderedResult(selected, text, json);
    }

    private sealed record RenderedResult(
        CliSelectedReport<ExtensionListData> Selected,
        string Text,
        string Json);
}
