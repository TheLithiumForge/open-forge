using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor;

public sealed class DoctorOperationEventTests
{
    [Fact(DisplayName = "Doctor contributor failures retain six failed domains without actions")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public async Task ContributorFailureRetainsSixFailedDomainsWithoutActions()
    {
        var support = new DoctorOperationTestSupport();
        support.WorkspaceEntry.Failure = new IOException("Injected contributor failure.");

        var result = await new DoctorOperation(support.Catalogue)
            .ExecuteAsync(
                new DoctorRequest(DoctorOperationTestSupport.Workspace()),
                CancellationToken.None);

        Assert.Equal(["workspace-entry"], support.Calls);
        AssertEventResult(result, CliSemanticStatus.Failed);
    }

    [Fact(DisplayName = "Doctor pre-cancellation retains six interrupted domains without reading contributors")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public async Task PreCancellationRetainsSixInterruptedDomainsWithoutActions()
    {
        var support = new DoctorOperationTestSupport();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await new DoctorOperation(support.Catalogue)
            .ExecuteAsync(
                new DoctorRequest(DoctorOperationTestSupport.Workspace()),
                cancellation.Token);

        Assert.Empty(support.Calls);
        AssertEventResult(result, CliSemanticStatus.Interrupted);
    }

    private static void AssertEventResult(
        DoctorResult result,
        CliSemanticStatus status)
    {
        Assert.Equal(status, result.Status);
        Assert.Equal(DoctorCoverageState.Incomplete, result.Diagnosis.Coverage);
        Assert.Equal(
            Enum.GetValues<DoctorDomainKind>(),
            result.Diagnosis.Domains.Select(domain => domain.Domain));
        Assert.All(result.Diagnosis.Domains, domain =>
        {
            Assert.Equal(DoctorCoverageState.Incomplete, domain.Coverage);
            Assert.Single(domain.Limitations);
            Assert.Empty(domain.Findings);
            Assert.Empty(domain.Actions);
        });
        Assert.Empty(result.Diagnosis.Actions);
    }
}
