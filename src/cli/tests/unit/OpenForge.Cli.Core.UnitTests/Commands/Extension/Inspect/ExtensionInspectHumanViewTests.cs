using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;
using OpenForge.Cli.Core.Presentation.Extension.Inspect;
using OpenForge.Cli.Core.Presentation.Extension.Inspect.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Inspect;

public sealed class ExtensionInspectHumanViewTests
{
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Extension Inspect preserves long finding paths in every selected text view"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Unit")]
    [InlineData((int)CliDetail.Minimal)]
    [InlineData((int)CliDetail.Standard)]
    public void FindingPathsRemainVisible(int detail)
    {
        var path = $".agents/{new string('x', 600)}.md";
        var result = ExtensionInspectResultBuilder.InvalidStableId(null, "Toolkit", "The ID is not lowercase.") with
        {
            Findings =
            [
                new()
                {
                    Code = ExtensionInspectFindingCode.PathUnavailable,
                    Status = CliSemanticStatus.Incomplete,
                    Subject = "selected-source",
                    PackageId = "toolkit",
                    Dependency = "dependency",
                    Path = path,
                    Cause = "The file could not be read.",
                    Location = null,
                    Candidates = [],
                },
            ],
        };

        var selected = CliReportSelection.Select(
            result,
            new CliSelection((CliDetail)detail),
            ExtensionInspectPresentation.Rendering);
        var text = CliTextRenderer.Render(
            selected,
            CliTextStyle.Plain,
            ExtensionInspectPresentation.Rendering.DataTextRenderer).Content;
        Assert.Contains(path, text, StringComparison.Ordinal);
        Assert.Contains("could not be read", text, StringComparison.Ordinal);
        Assert.DoesNotContain("600+10", text, StringComparison.Ordinal);

        using var json = JsonDocument.Parse(CliJsonRenderer.Render(
            selected,
            ExtensionInspectPresentation.Rendering.DataJsonTypeInfo));
        Assert.Equal(
            path,
            json.RootElement.GetProperty("findings")[0].GetProperty("subject").GetProperty("path").GetString());
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Extension Inspect bounds manifest parser causes at compact detail levels")]
    [InlineData((int)CliDetail.Minimal)]
    [InlineData((int)CliDetail.Standard)]
    [Trait("Feature", "extension-inspect"), Trait("Evidence", "Unit")]
    public void ManifestParserCausesAreBounded(int detail)
    {
        const string rawCause = "The Extension manifest is invalid: 'm' is an invalid start of a property name. LineNumber: 0 | BytePositionInLine: 2.";
        var result = ExtensionInspectResultBuilder.InvalidStableId(null, "Toolkit", "Unused seed finding.") with
        {
            Status = CliSemanticStatus.Invalid,
            Findings =
            [
                new()
                {
                    Code = ExtensionInspectFindingCode.PackageInvalid,
                    Status = CliSemanticStatus.Invalid,
                    Subject = "selected-source",
                    PackageId = "toolkit",
                    Dependency = null,
                    Path = "extension.json",
                    Cause = rawCause,
                    Location = null,
                    Candidates = [],
                },
            ],
        };
        var rendering = ExtensionInspectPresentation.Rendering;
        var selected = CliReportSelection.Select(result, new CliSelection((CliDetail)detail), rendering);
        var text = CliTextRenderer.Render(
            selected,
            CliTextStyle.Plain,
            rendering.DataTextRenderer).Content;
        Assert.Contains("the content is not valid JSON", text, StringComparison.Ordinal);
        Assert.DoesNotContain("LineNumber:", text, StringComparison.Ordinal);

        using var json = JsonDocument.Parse(CliJsonRenderer.Render(selected, rendering.DataJsonTypeInfo));
        var message = json.RootElement.GetProperty("findings")[0].GetProperty("message").GetString();
        Assert.Contains("the content is not valid JSON", message, StringComparison.Ordinal);
        Assert.DoesNotContain("LineNumber:", message, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Inspect keeps differing files in minimal text and gates hashes behind full detail"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Unit")]
    public void DifferenceRowsAndHashesFollowDetailPolicy()
    {
        var current = Fingerprint('a');
        var intended = Fingerprint('b');
        var path = new ExtensionInspectPathComparison
        {
            Path = ".agents/changed.md",
            Current = current,
            Intended = intended,
            Relation = ExtensionInspectPathRelation.Changed,
            CurrentOwners = ["toolkit"],
            IntendedOwners = ["toolkit"],
        };
        var seed = ExtensionInspectResultBuilder.InvalidStableId(null, "Toolkit", "Unused seed finding.");
        var result = seed with
        {
            Status = CliSemanticStatus.Attention,
            Subject = seed.Subject with { State = ExtensionInspectSubjectState.Resolved, Id = "toolkit", Form = ExtensionInspectSubjectForm.StableId },
            Installed = new()
            {
                State = ExtensionInspectInstalledState.Present,
                Package = new() { Id = "toolkit", Version = "1.0.0", Source = "embedded catalogue", Dependencies = [], Paths = [path.Path] },
            },
            Available = new()
            {
                State = ExtensionInspectAvailableState.Present,
                Package = new()
                {
                    Id = "toolkit",
                    Name = "Toolkit",
                    Description = "Review helpers.",
                    Version = "1.0.0",
                    ManifestPath = "toolkit/extension.json",
                    Dependencies = [],
                    Payload = [new ExtensionInspectPackageFile
                    {
                        Path = path.Path,
                        TargetPath = path.Path,
                        State = ExtensionInspectPackageFileState.Available,
                        ByteLength = 1,
                        Sha256 = intended.Sha256,
                    }],
                },
            },
            Dependencies = new()
            {
                State = ExtensionInspectDependencyState.Complete,
                Declared = [],
                Resolved = [new() { Id = "toolkit", Version = "1.0.0", Source = "embedded catalogue", State = ExtensionInspectDependencyPackageState.Available }],
                Order = ["toolkit"],
            },
            Comparison = new()
            {
                State = ExtensionInspectComparisonState.Complete,
                Mode = ExtensionInspectComparisonMode.InstalledAndAvailable,
                Current = new() { State = ExtensionInspectComparisonSideState.Available, Fingerprints = [new() { Path = path.Path, Fingerprint = current }] },
                Intended = new() { State = ExtensionInspectComparisonSideState.Available, Fingerprints = [new() { Path = path.Path, Fingerprint = intended }] },
                Paths = [path],
                Dependencies = new() { State = ExtensionInspectDependencyComparisonState.Available, Current = [], Intended = [], Relation = ExtensionInspectDependencyRelation.Equal },
            },
            Findings =
            [
                new()
                {
                    Code = ExtensionInspectFindingCode.PathChanged,
                    Status = CliSemanticStatus.Attention,
                    Subject = null,
                    PackageId = "toolkit",
                    Dependency = null,
                    Path = path.Path,
                    Cause = "The current workspace differs from the selected package.",
                    Location = null,
                    Candidates = [],
                },
            ],
            Counts = seed.Counts with { InstalledPackages = 1, AvailablePackages = 1, IntendedPaths = 1, CurrentPaths = 1, ChangedPaths = 1, Dependencies = 1, Findings = 1 },
        };

        var rendering = ExtensionInspectPresentation.Rendering;
        var minimal = CliReportSelection.Select(result, new CliSelection(CliDetail.Minimal), rendering);
        var minimalText = CliTextRenderer.Render(minimal, CliTextStyle.Plain, rendering.DataTextRenderer).Content;
        Assert.Contains(".agents/changed.md", minimalText, StringComparison.Ordinal);
        Assert.Contains("changed since it was installed", minimalText, StringComparison.Ordinal);
        Assert.DoesNotContain("SHA-256", minimalText, StringComparison.Ordinal);
        using var minimalJson = JsonDocument.Parse(CliJsonRenderer.Render(minimal, rendering.DataJsonTypeInfo));
        Assert.False(minimalJson.RootElement.GetProperty("data").GetProperty("files")[0].TryGetProperty("installedSha256", out _));

        var full = CliReportSelection.Select(result, new CliSelection(CliDetail.Full), rendering);
        var fullText = CliTextRenderer.Render(full, CliTextStyle.Plain, rendering.DataTextRenderer).Content;
        Assert.Contains("Installed SHA-256", fullText, StringComparison.Ordinal);
        Assert.Contains("Package SHA-256", fullText, StringComparison.Ordinal);
        using var fullJson = JsonDocument.Parse(CliJsonRenderer.Render(full, rendering.DataJsonTypeInfo));
        Assert.Equal(current.Sha256, fullJson.RootElement.GetProperty("data").GetProperty("files")[0].GetProperty("installedSha256").GetString());
        Assert.Equal(intended.Sha256, fullJson.RootElement.GetProperty("data").GetProperty("files")[0].GetProperty("packageSha256").GetString());
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Extension Inspect relation wording follows the catalogue"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Unit")]
    [InlineData(ExtensionInspectPathRelation.Retired, "no longer part of the package")]
    [InlineData(ExtensionInspectPathRelation.Changed, "changed")]
    [InlineData(ExtensionInspectPathRelation.New, "new in the package")]
    public void RelationWording(object value, string expected)
        => Assert.Equal(expected, ExtensionInspectWording.Relation((ExtensionInspectPathRelation)value));

    private static ExtensionInspectFingerprint Fingerprint(char value)
        => new()
        {
            Kind = ExtensionInspectFingerprintKind.Semantic,
            Policy = "open-forge-markdown-v1",
            Sha256 = new string(value, 64),
            Origin = ExtensionInspectFingerprintOrigin.OperationTimeCurrent,
        };
}
