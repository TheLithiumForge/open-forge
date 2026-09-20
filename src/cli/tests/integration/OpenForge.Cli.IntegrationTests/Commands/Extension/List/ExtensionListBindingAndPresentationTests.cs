using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using System.Globalization;
using OpenForge.Cli.Core.Commands.Extension.List.Models;
using OpenForge.Cli.Core.Commands.Extension.List.Shared.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Extension.List;
using OpenForge.Cli.Core.Presentation.Extension.List.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.List;

public sealed class ExtensionListBindingAndPresentationTests
{
    private static string WorkspaceRoot { get; } = Path.GetFullPath(
        Path.Combine(Path.GetTempPath(), "open-forge-extension-list-workspace"));

    [Theory(DisplayName = "Extension List human presentation executes every status stream and exit policy"), Trait("Feature", "extension-list"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    [InlineData((int)CliSemanticStatus.Complete, "complete", 0, (int)CliOutputTarget.StandardOutput)]
    [InlineData((int)CliSemanticStatus.Failed, "failed", 1, (int)CliOutputTarget.StandardError)]
    [InlineData((int)CliSemanticStatus.Attention, "requires attention", 2, (int)CliOutputTarget.StandardOutput)]
    [InlineData((int)CliSemanticStatus.Incomplete, "incomplete", 3, (int)CliOutputTarget.StandardOutput)]
    [InlineData((int)CliSemanticStatus.Invalid, "invalid", 4, (int)CliOutputTarget.StandardError)]
    [InlineData((int)CliSemanticStatus.Blocked, "blocked", 5, (int)CliOutputTarget.StandardError)]
    [InlineData((int)CliSemanticStatus.Interrupted, "interrupted", 130, (int)CliOutputTarget.StandardError)]
    public async Task HumanPresentationExecutesCompleteStatusMatrix(
        int statusValue,
        string humanStatus,
        int expectedExitCode,
        int targetValue)
    {
        var status = (CliSemanticStatus)statusValue;
        var target = (CliOutputTarget)targetValue;
        var result = status == CliSemanticStatus.Complete
            ? CreateResult()
            : ExtensionListResultBuilder.Event(
                CreateRequest(),
                status,
                ReadFindingCode(status),
                $"The {humanStatus} presentation path was selected.");
        using var standardOutput = new StringWriter(CultureInfo.InvariantCulture);
        using var standardError = new StringWriter(CultureInfo.InvariantCulture);
        var pipeline = new CliReportPipeline<ExtensionListRequest, ExtensionListResult, ExtensionListData>(
            (_, _) => ValueTask.FromResult(result),
            ExtensionListPresentation.Rendering);
        var presentation = new CliPresentation(
            CliFormat.Text,
            CliDetail.Standard, null);

        var completion = await pipeline.ExecuteAsync(
            CreateRequest(),
            presentation,
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);

        var selected = CliReportSelection.Select(result, new CliSelection(CliDetail.Standard), ExtensionListPresentation.Rendering);
        var rendered = CliTextRenderer.Render(selected, CliTextStyle.Plain, ExtensionListPresentation.Rendering.DataTextRenderer).Content;
        Assert.Equal(status, completion.Status);
        Assert.Equal(expectedExitCode, completion.ExitCode);
        Assert.Equal(target, completion.PrimaryOutputTarget);
        var platformRendered = rendered.Replace("\n", Environment.NewLine, StringComparison.Ordinal);
        Assert.Equal(target == CliOutputTarget.StandardOutput ? platformRendered : string.Empty, standardOutput.ToString());
        Assert.Equal(target == CliOutputTarget.StandardError ? platformRendered : string.Empty, standardError.ToString());
        _ = humanStatus;
    }

    private static ExtensionListFindingCode ReadFindingCode(CliSemanticStatus status)
        => status switch
        {
            CliSemanticStatus.Failed => ExtensionListFindingCode.OperationFailed,
            CliSemanticStatus.Attention => ExtensionListFindingCode.SourceUnavailable,
            CliSemanticStatus.Incomplete => ExtensionListFindingCode.SourceUnavailable,
            CliSemanticStatus.Invalid => ExtensionListFindingCode.InvalidInput,
            CliSemanticStatus.Blocked => ExtensionListFindingCode.SourceBlocked,
            CliSemanticStatus.Interrupted => ExtensionListFindingCode.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The event finding status is not defined."),
        };

    private static ExtensionListRequest CreateRequest()
        => new()
        {
            Workspace = new CliWorkspace(
                lexicalRoot: WorkspaceRoot,
                physicalRoot: WorkspaceRoot,
                selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace),
            Selection = ExtensionListSelection.Create(installedFlag: true, availableFlag: true),
            ExplicitSource = null,
        };

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
}
