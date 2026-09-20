using OpenForge.Cli.Core.Commands.Library.Detach;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Detach;

public sealed class LibraryDetachPreparationIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Library Detach retains incomplete preparation when its owned recovery bucket is a file"), Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task RecoveryBucketFileRetainsIncompleteWithoutTargetEffects()
    {
        const string jsonPath = ".agents/resources/data.json";
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source(jsonPath);
        workspace.Directory(".agents/resources");
        workspace.Link(jsonPath);
        workspace.Record(jsonPath);
        using var bucket = new LibraryRecoveryBucketFile(workspace);
        var before = workspace.Snapshot();

        var result = await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(
            workspace.Detach(LibraryMode.Apply),
            TestContext.Current.CancellationToken);

        Assert.Equal(before, workspace.Snapshot());
        bucket.AssertUnchanged();
        Assert.Equal(LibraryPlanState.Complete, result.Result.Plan.State);
        Assert.Single(result.Result.Plan.Links);
        Assert.Empty(result.Result.Plan.GeneratedRegions);
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Contains(result.Result.Findings, finding => finding.Code == LibraryDetachFindingCode.RecoveryUnavailable);
        var application = result.Result.Application;
        Assert.Equal(LibraryApplicationState.NotStarted, application.State);
        Assert.Equal(LibraryVerificationState.NotStarted, application.Verification);
        Assert.Equal(LibraryRecordPublicationState.NotStarted, application.RecordPublication.State);
        Assert.Null(application.RecordPublication.PublishedLast);
        Assert.Equal(LibraryRecoveryState.Unknown, application.Recovery.State);
        Assert.Null(application.Recovery.Path);
        Assert.Empty(application.Residuals);
    }
}
