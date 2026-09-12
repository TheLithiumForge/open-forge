using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor.Shared.Rendering;

public sealed class DoctorHumanVocabularyTests
{
    [Fact(DisplayName = "Empty Doctor counts collapse only when every count is known to be zero"), Trait("Feature", "doctor-presentation"), Trait("Evidence", "Unit")]
    public void UnknownCountsDoNotBecomeNoFindings()
    {
        var zero = DoctorFindingAggregation.Count([]);
        var unknown = zero with { Severity = zero.Severity with { Warning = new DoctorCount { State = OperationalValueState.Unavailable, Value = null } } };
        var complete = new System.Text.StringBuilder();
        var incomplete = new System.Text.StringBuilder();

        DoctorCountsHumanRenderer.Append(complete, zero, string.Empty);
        DoctorCountsHumanRenderer.Append(incomplete, unknown, string.Empty);

        Assert.Equal("Findings: none" + Environment.NewLine, complete.ToString());
        Assert.Contains("warning count unavailable", incomplete.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("Findings: none", incomplete.ToString(), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Doctor human vocabulary covers every supported value and rejects undefined values"), Trait("Feature", "doctor-presentation"), Trait("Evidence", "Unit")]
    public void HumanMappingsAreClosed()
    {
        Check(DoctorHumanVocabulary.Outcome, (CliSemanticStatus)int.MaxValue);
        Check(DoctorHumanVocabulary.Status, (CliSemanticStatus)int.MaxValue);
        Check(DoctorHumanVocabulary.Selection, (CliWorkspaceSelectionMethod)int.MaxValue);
        Check(DoctorHumanVocabulary.Domain, (DoctorDomainKind)int.MaxValue);
        Check(DoctorHumanVocabulary.Severity, (DoctorFindingSeverity)int.MaxValue);
        Check(DoctorHumanVocabulary.Resolution, (DoctorResolutionLane)int.MaxValue);
        Check(DoctorHumanVocabulary.Subject, (DoctorSubjectKind)int.MaxValue);
        Check(DoctorHumanVocabulary.CandidateBasis, (DoctorCandidateBasisKind)int.MaxValue);
        Check(DoctorHumanVocabulary.Provenance, (DoctorProvenanceSource)int.MaxValue);
    }

    private static void Check<T>(Func<T, string> render, T undefined) where T : struct, Enum
    {
        foreach (var value in Enum.GetValues<T>())
        {
            Assert.False(string.IsNullOrWhiteSpace(render(value)));
        }

        Assert.Throws<ArgumentOutOfRangeException>(() => render(undefined));
    }
}
