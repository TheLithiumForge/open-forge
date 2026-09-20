using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Selection;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Presentation.References.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.References.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.References.Shared.Rendering;

public sealed class ReferencesFindingPresentationTests
{
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "References identity findings publish distinct codes severities and messages"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    [InlineData(
        "IdentityCollision",
        "references.identity-collision",
        "warning",
        "The ID docs/guide matches more than one file. Use the exact path.")]
    [InlineData(
        "PhysicalAlias",
        "references.physical-alias",
        "error",
        ".agents/alias.md and .agents/target.md resolve to the same physical file.")]
    [InlineData(
        "IdentityUnavailable",
        "references.identity-unavailable",
        "warning",
        "The identity of .agents/.md could not be determined.")]
    public void IdentityFindingsPublishDistinctPresentation(
        string codeName,
        string machineName,
        string severityName,
        string message)
    {
        var code = Enum.Parse<ReferencesFindingCode>(codeName);
        var report = ReferencesReportSelector.Select(
            CreateResult(code),
            new CliSelection(CliDetail.Full));
        var finding = Assert.Single(report.Findings);

        Assert.Equal(machineName, finding.Code);
        Assert.Equal(severityName, finding.Severity.ToString().ToLowerInvariant());
        Assert.Equal(message, finding.Message);
        Assert.Equal(code == ReferencesFindingCode.IdentityUnavailable ? [] : ExpectedPaths(code), finding.Candidates.Select(candidate => candidate.Subject.Path));
        if (code == ReferencesFindingCode.PhysicalAlias)
        {
            Assert.Equal(
                "Cannot list references: .agents/alias.md and .agents/target.md resolve to the same physical file.",
                report.Headline.Sentence);
        }
    }

    private static ReferencesResult CreateResult(ReferencesFindingCode code)
    {
        var status = ReferencesDefinitions.ReadStatus(code);
        var source = ReferencesPresentationTestData.Source(withOverwrite: false);
        var finding = new ReferencesFinding(
            code,
            ReferencesDirection.In,
            null,
            "The source catalogue retained an unresolved boundary.",
            null,
            null,
            source.Identity,
            SourceLayerKind.Base,
            Path(code),
            null,
            null,
            Candidates(code));
        var findings = new[] { finding };
        return new ReferencesResult(
            ReferencesPresentationTestData.Workspace(),
            source,
            ReferencesDirection.In,
            new ReferencesIncomingSelection(ReferencesSelectionMode.Default, [], [], [], []),
            new ReferencesSection(Coverage(status), status, []),
            null,
            findings,
            status,
            ReferencesDefinitions.ReadNextAction(status, findings));
    }

    private static ReferencesCoverage Coverage(CliSemanticStatus status)
        => status switch
        {
            CliSemanticStatus.Attention => ReferencesCoverage.Complete,
            CliSemanticStatus.Incomplete => ReferencesCoverage.Incomplete,
            CliSemanticStatus.Blocked => ReferencesCoverage.Blocked,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The identity finding status is not defined."),
        };

    private static string Path(ReferencesFindingCode code)
        => code switch
        {
            ReferencesFindingCode.IdentityCollision => ".agents/docs/guide.md",
            ReferencesFindingCode.PhysicalAlias => ".agents/target.md",
            ReferencesFindingCode.IdentityUnavailable => ".agents/.md",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The identity finding code is not defined."),
        };

    private static IReadOnlyList<ReferencesSourceIdentity> Candidates(ReferencesFindingCode code)
        => code switch
        {
            ReferencesFindingCode.IdentityCollision =>
            [
                new ReferencesSourceIdentity("docs/guide", ".agents/docs/guide.md"),
                new ReferencesSourceIdentity("docs/guide", ".agents/docs/guide/_guide.md"),
            ],
            ReferencesFindingCode.PhysicalAlias =>
            [
                new ReferencesSourceIdentity("alias", ".agents/alias.md"),
                new ReferencesSourceIdentity("target", ".agents/target.md"),
            ],
            ReferencesFindingCode.IdentityUnavailable => [],
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The identity finding code is not defined."),
        };

    private static IReadOnlyList<string> ExpectedPaths(ReferencesFindingCode code)
        => Candidates(code).Select(candidate => candidate.Path).ToArray();
}
