using System.Text;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

using OpenForge.Cli.IntegrationTests.Commands.Repair.Shared.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

public sealed class RepairBoundaryDiagnosisIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Repair final No freshly diagnoses an independently added missing reference")]
    [Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task FinalNoObservesNewFindingWithoutApplyingPlan()
    {
        using var workspace = RepairIntegrationWorkspace.Create("repair-final-no-diagnosis", includeGuided: false);
        const string destination = "new-missing-target.md";
        var changed = workspace.ReadText(RepairIntegrationWorkspace.SourcePath) + $"\n[New]({destination})\n";
        var prefix = changed[..changed.LastIndexOf(destination, StringComparison.Ordinal)];
        using var output = new StringWriter();
        using var input = new FinalNoEditReader(() =>
            File.WriteAllText(workspace.Combine(RepairIntegrationWorkspace.SourcePath), changed));
        var components = RepairOperationFactory.CreateDefaultComponents() with
        {
            Interaction = RepairInteractionTestFactory.Create(input, output),
        };
        var result = await new RepairOperation(components).ExecuteAsync(
            workspace.Request(allowInteraction: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(changed, workspace.ReadText(RepairIntegrationWorkspace.SourcePath));
        Assert.Equal(RepairPostDiagnosisState.Complete, result.PostDiagnosis.State);
        Assert.Contains(result.PostDiagnosis.Findings, finding =>
            finding.Code == RepairFindingCode.GuidedFindingRemaining
            && finding.SourceCanonicalPath == RepairIntegrationWorkspace.SourcePath
            && finding.Occurrence?.Line == prefix.Count(character => character == '\n') + 1
            && finding.Occurrence.ByteOffset == Encoding.UTF8.GetByteCount(prefix));
        workspace.AssertNoRecoveryArtifacts();
    }

    private sealed class FinalNoEditReader(Action edit) : StringReader("y\nn\n")
    {
        private int _reads;

        public override ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
        {
            if (++_reads == 2)
            {
                edit();
            }

            return base.ReadLineAsync(cancellationToken);
        }
    }
}
