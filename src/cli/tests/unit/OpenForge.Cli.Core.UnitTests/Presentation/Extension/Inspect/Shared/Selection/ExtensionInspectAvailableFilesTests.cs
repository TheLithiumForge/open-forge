using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;
using OpenForge.Cli.Core.Presentation.Extension.Inspect;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Extension.Inspect.Shared.Selection;

[Trait("Feature", "extension-inspect"), Trait("Evidence", "Unit")]
public sealed class ExtensionInspectAvailableFilesTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Inspect standard text lists available-only files without an installed claim"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Unit")]
    public void StandardTextListsAvailableOnlyFilesWithoutInstalledClaim()
    {
        const string path = ".agents/guidance/toolkit.md";
        var intended = new ExtensionInspectFingerprint
        {
            Kind = ExtensionInspectFingerprintKind.Semantic,
            Policy = "open-forge-markdown-v1",
            Sha256 = new string('a', 64),
            Origin = ExtensionInspectFingerprintOrigin.OperationTimeIntended,
        };
        var comparisonPath = new ExtensionInspectPathComparison
        {
            Path = path,
            Current = null,
            Intended = intended,
            Relation = ExtensionInspectPathRelation.NotApplicable,
            CurrentOwners = [],
            IntendedOwners = ["toolkit"],
        };
        var seed = ExtensionInspectResultBuilder.InvalidStableId(null, "toolkit", "unused seed finding");
        var result = seed with
        {
            Status = CliSemanticStatus.Complete,
            Findings = [],
            Next = null,
            Subject = seed.Subject with
            {
                Form = ExtensionInspectSubjectForm.StableId,
                Id = "toolkit",
                State = ExtensionInspectSubjectState.Resolved,
            },
            Source = seed.Source with
            {
                Explicit = true,
                Identity = "catalogue",
                Kind = ExtensionInspectSourceKind.Catalogue,
                State = ExtensionInspectSourceState.Available,
            },
            Installed = seed.Installed with
            {
                State = ExtensionInspectInstalledState.Absent,
                Package = null,
            },
            Available = seed.Available with
            {
                State = ExtensionInspectAvailableState.Present,
                Package = new ExtensionInspectAvailablePackage
                {
                    Id = "toolkit",
                    Name = "Toolkit",
                    Description = "Guidance tools.",
                    Version = "1.0.0",
                    ManifestPath = "toolkit/extension.json",
                    Dependencies = [],
                    Payload = [],
                },
            },
            Comparison = seed.Comparison with
            {
                State = ExtensionInspectComparisonState.Complete,
                Mode = ExtensionInspectComparisonMode.AvailableOnly,
                Current = seed.Comparison.Current with
                {
                    State = ExtensionInspectComparisonSideState.NotApplicable,
                    Fingerprints = [],
                },
                Intended = seed.Comparison.Intended with
                {
                    State = ExtensionInspectComparisonSideState.Available,
                    Fingerprints =
                    [
                        new ExtensionInspectFingerprintFact
                        {
                            Path = path,
                            Fingerprint = intended,
                        },
                    ],
                },
                Paths = [comparisonPath],
            },
        };

        var rendering = ExtensionInspectPresentation.Rendering;
        var selected = CliReportSelection.Select(result, new CliSelection(CliDetail.Standard), rendering);
        var file = Assert.Single(selected.Report.Data.Files);
        Assert.Equal(path, file.Path);
        Assert.Equal("not-applicable", file.Relation);
        Assert.Null(file.InstalledSha256);
        Assert.Null(selected.Report.Data.Installed);
        Assert.Equal(path, Assert.Single(selected.Report.Data.TextFiles).Path);

        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, rendering.DataTextRenderer).Content;
        Assert.Contains(path, text, StringComparison.Ordinal);
        Assert.Contains("not applicable", text, StringComparison.OrdinalIgnoreCase);
    }
}
