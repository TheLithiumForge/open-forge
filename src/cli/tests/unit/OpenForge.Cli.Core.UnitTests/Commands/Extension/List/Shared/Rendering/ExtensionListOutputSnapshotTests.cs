using OpenForge.Cli.Core.Commands.Extension.List.Models;
using OpenForge.Cli.Core.Presentation.Extension.List;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using TheLithium.Imprint;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.List.Shared.Rendering;

[Trait("Feature", "state-retirement-output"), Trait("Evidence", "Unit")]
public sealed class ExtensionListOutputSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension List minimal text missing state matches its reviewed snapshot")]
    public void MinimalText() => Render(CliFormat.Text, CliDetail.Minimal).AssertSnapshot();

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension List standard text missing state matches its reviewed snapshot")]
    public void StandardText() => Render(CliFormat.Text, CliDetail.Standard).AssertSnapshot();

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension List minimal json missing state matches its reviewed snapshot")]
    public void MinimalJson() => Render(CliFormat.Json, CliDetail.Minimal).AssertSnapshot();

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension List standard json missing state matches its reviewed snapshot")]
    public void StandardJson() => Render(CliFormat.Json, CliDetail.Standard).AssertSnapshot();

    private static string Render(CliFormat format, CliDetail detail)
    {
        var result = new ExtensionListResult(
            status: CliSemanticStatus.Complete,
            workspace: null,
            selection: ExtensionListSelection.Create(true, true),
            source: new() { Identity = "embedded catalogue", Kind = ExtensionListSourceKind.EmbeddedCatalogue, State = ExtensionListSourceState.Complete },
            lifecycleTrust: ExtensionListOwnershipTrust.Absent,
            installedCoverage: ExtensionListCoverage.Complete,
            availableCoverage: ExtensionListCoverage.Complete,
            installed: [],
            available: [new() { Id = "toolkit", Name = "Toolkit", Description = "Review helpers.", Version = "2.0.0", PackageCount = 1, DependencyCount = 0 }],
            findings: [new(ExtensionListFindingCode.OwnershipObservation, CliSemanticStatus.Complete,
                ".agents/open-forge.lock.json", "No Extension ownership is recorded; installed packages cannot be established.")],
            next: null);
        var rendering = ExtensionListPresentation.Rendering;
        var selected = CliReportSelection.Select(result, new CliSelection(detail), rendering);
        return format == CliFormat.Json
            ? CliJsonRenderer.Render(selected, rendering.DataJsonTypeInfo)
            : CliTextRenderer.Render(selected, CliTextStyle.Plain, rendering.DataTextRenderer).Content;
    }
}
