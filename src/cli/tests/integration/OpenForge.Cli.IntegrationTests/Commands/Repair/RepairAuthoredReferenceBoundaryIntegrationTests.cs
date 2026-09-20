using System.Text;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.IntegrationTests.Commands.Repair.Shared.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

public sealed class RepairAuthoredReferenceBoundaryIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Automatic Repair skips a generated entry and repairs an authored link after Entries")]
    [Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task AutomaticRepairSkipsGeneratedEntryAndRepairsAuthoredLinkAfterEntries()
    {
        using var workspace = RepairIntegrationWorkspace.Create(
            "repair-authored-boundary-complete",
            includeSafeExact: false,
            includeGuided: false);
        var source = WriteSource(
            workspace,
            "\n# Source\n\n"
                + "😀 before the boundary.\n\n"
                + "## Entries\n\n"
                + "- [Generated](generated-missing.md)\n\n"
                + "## Notes\n\n"
                + "[Authored](./guide.md)\n");

        var result = await new RepairOperation().ExecuteAsync(
            workspace.Request(mode: RepairMode.Apply, automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(RepairCoverageState.Complete, result.Diagnosis.SelectedScope);
        Assert.Equal(RepairPostDiagnosisState.Complete, result.PostDiagnosis.State);
        Assert.NotNull(result.Plan);
        var effect = Assert.Single(result.Plan!.Effects);
        var change = Assert.Single(effect.Changes);
        var destinationStart = source.IndexOf("./guide.md", StringComparison.Ordinal);
        Assert.True(destinationStart >= 0);
        Assert.Equal(
            Encoding.UTF8.GetByteCount(source[..destinationStart]),
            change.Occurrence.ByteOffset);
        Assert.Equal(
            Encoding.UTF8.GetByteCount("./guide.md"),
            change.Occurrence.ByteLength);
        Assert.Equal("./guide.md", change.ExpectedDestination);
        Assert.Equal("guide.md", change.IntendedDestination);
        Assert.Contains(
            "[Generated](generated-missing.md)",
            workspace.ReadText(RepairIntegrationWorkspace.SourcePath),
            StringComparison.Ordinal);
        Assert.Contains(
            "[Authored](guide.md)",
            workspace.ReadText(RepairIntegrationWorkspace.SourcePath),
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            result.Findings.Concat(result.PostDiagnosis.Findings),
            finding => finding.SourceCanonicalPath == RepairIntegrationWorkspace.SourcePath
                && (finding.Code is RepairFindingCode.GuidedFindingRemaining
                    or RepairFindingCode.ManualFindingRemaining));
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Malformed or unavailable Entries boundary refuses without silent skip")]
    [InlineData("invalid")]
    [InlineData("unavailable")]
    [Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task MalformedOrUnavailableEntriesBoundaryRefusesWithoutSilentSkip(string scenario)
    {
        using var workspace = RepairIntegrationWorkspace.Create(
            $"repair-authored-boundary-{scenario}",
            includeSafeExact: false,
            includeGuided: false);
        if (scenario == "invalid")
        {
            WriteSource(
                workspace,
                "\n# Source\n\n"
                    + "## Entries\n\n"
                    + "- [Generated](generated-missing.md)\n\n"
                    + "## Entries\n\n"
                    + "[Authored](./guide.md)\n");
        }
        else
        {
            WriteSource(
                workspace,
                "\n# Source\n\n"
                    + "[Authored](./guide.md)\n");
        }

        var before = workspace.ReadBytes(RepairIntegrationWorkspace.SourcePath);
        RepairResult result;
        if (scenario == "unavailable")
        {
            using var output = new StringWriter();
            using var input = new UnavailableSourceReader(() =>
            {
                var sourcePath = workspace.Combine(RepairIntegrationWorkspace.SourcePath);
                File.Delete(sourcePath);
                Directory.CreateDirectory(sourcePath);
            });
            var components = RepairOperationFactory.CreateDefaultComponents() with
            {
                Interaction = RepairInteractionTestFactory.Create(input, output),
            };
            try
            {
                result = await new RepairOperation(components).ExecuteAsync(
                    workspace.Request(mode: RepairMode.Apply, allowInteraction: true),
                    TestContext.Current.CancellationToken);
            }
            finally
            {
                var sourcePath = workspace.Combine(RepairIntegrationWorkspace.SourcePath);
                if (Directory.Exists(sourcePath))
                {
                    Directory.Delete(sourcePath);
                }

                File.WriteAllBytes(sourcePath, before);
            }
        }
        else
        {
            result = await new RepairOperation().ExecuteAsync(
                workspace.Request(mode: RepairMode.Apply, automatic: true),
                TestContext.Current.CancellationToken);
        }

        Assert.NotEqual(CliSemanticStatus.Complete, result.Status);
        Assert.Contains(
            result.Findings.Concat(result.PostDiagnosis.Findings),
            finding => finding.Code is RepairFindingCode.DiagnosisIncomplete
                or RepairFindingCode.DiagnosisBlocked
                || scenario == "unavailable" && finding.Code is RepairFindingCode.TargetChanged or RepairFindingCode.TargetUnsafe);
        Assert.Equal(before, workspace.ReadBytes(RepairIntegrationWorkspace.SourcePath));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Explicit relink to a generated entry is rejected")]
    [Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task ExplicitRelinkToGeneratedEntryIsRejected()
    {
        using var workspace = RepairIntegrationWorkspace.Create(
            "repair-authored-boundary-generated-relink",
            includeSafeExact: false,
            includeGuided: false);
        var source = WriteSource(
            workspace,
            "\n# Source\n\n"
                + "## Entries\n\n"
                + "- [Generated](generated-missing.md)\n");
        const string expectedDestination = "generated-missing.md";
        var destinationStart = source.IndexOf(expectedDestination, StringComparison.Ordinal);
        Assert.True(destinationStart >= 0);
        var line = 1 + source[..destinationStart].Count(character => character == '\n');
        var lineStart = source.LastIndexOf('\n', destinationStart == 0 ? 0 : destinationStart - 1);
        lineStart = lineStart < 0 ? 0 : lineStart + 1;
        var column = destinationStart - lineStart + 1;
        var before = workspace.ReadBytes(RepairIntegrationWorkspace.SourcePath);

        var result = await new RepairOperation().ExecuteAsync(
            workspace.Request(
                mode: RepairMode.Apply,
                relinks:
                [
                    new RepairRelinkRequest(
                        RepairIntegrationWorkspace.SourcePath,
                        line,
                        column,
                        expectedDestination,
                        RepairIntegrationWorkspace.SafeTargetPath,
                        selectedTargetFragment: null),
                ]),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.NotNull(result.Plan);
        Assert.Contains(
            result.Plan!.Conflicts,
            conflict => conflict.Kind == RepairConflictKind.SelectionUnmatched
                && conflict.SourceCanonicalPath == RepairIntegrationWorkspace.SourcePath);
        Assert.Empty(result.Plan.Effects);
        Assert.Equal(RepairApplicationState.NotRequested, result.Application.State);
        Assert.Equal(before, workspace.ReadBytes(RepairIntegrationWorkspace.SourcePath));
    }

    private static string WriteSource(
        RepairIntegrationWorkspace workspace,
        string body)
    {
        var source = OpenForgeDocumentSeed.Metadata(
            description: "Source",
            tags: ["Docs"],
            body: body);
        File.WriteAllText(
            workspace.Combine(RepairIntegrationWorkspace.SourcePath),
            source,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return source;
    }

    private sealed class UnavailableSourceReader(Action makeUnavailable) : StringReader("y\ny\n")
    {
        private int _reads;

        public override ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
        {
            if (++_reads == 2)
            {
                makeUnavailable();
            }

            return base.ReadLineAsync(cancellationToken);
        }
    }
}
