using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Library.Detach;
using OpenForge.Cli.Core.Commands.Library.Inspect;
using OpenForge.Cli.Core.Commands.Library.List;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Sync;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Reading;

[Trait("Feature", "ownership-operational-readers"), Trait("Evidence", "Integration")]
public sealed class LibraryOwnershipReaderIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library ownership boundaries never fall back to legacy claims or delete matching links")]
    [InlineData("absent", false), InlineData("absent", true)]
    [InlineData("invalid", false), InlineData("invalid", true)]
    [InlineData("unavailable", false), InlineData("unavailable", true)]
    public async Task MissingOwnershipIsInformational(string condition, bool apply)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Link();
        workspace.Write(".agents/open-forge.libraries.json", """
            {"schemaVersion":1,"libraries":[{"id":"team-knowledge","sourceRoot":"shared/team-knowledge","destinationRoot":".","paths":[".agents/directives/review.md"]}]}
            """);
        if (condition == "invalid")
        {
            workspace.Write(LibraryMutationWorkspace.OwnershipPath, "{");
        }
        else if (condition == "unavailable")
        {
            workspace.Directory(LibraryMutationWorkspace.OwnershipPath);
        }
        var before = workspace.Snapshot();
        var mode = apply ? LibraryMode.Apply : LibraryMode.DryRun;

        var detach = await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(
            workspace.Detach(mode), TestContext.Current.CancellationToken);
        var sync = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(
            workspace.Sync(mode), TestContext.Current.CancellationToken);
        var attach = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(
            workspace.Attach(mode), TestContext.Current.CancellationToken);
        var list = await new LibraryListOperation().ExecuteAsync(new() { Workspace = workspace.Workspace }, TestContext.Current.CancellationToken);
        var inspect = await new LibraryInspectOperation().ExecuteAsync(new()
        {
            Workspace = workspace.Workspace,
            LibraryId = LibraryId.Create("team-knowledge"),
        }, TestContext.Current.CancellationToken);

        var expectedRecordStatus = condition == "invalid"
            ? CliSemanticStatus.Incomplete
            : CliSemanticStatus.Complete;
        var expectedListStatus = condition switch
        {
            "invalid" or "unavailable" => CliSemanticStatus.Incomplete,
            _ => CliSemanticStatus.Complete,
        };
        var expectedAttachStatus = CliSemanticStatus.Blocked;

        Assert.Equal(expectedRecordStatus, detach.Status);
        Assert.Equal(expectedRecordStatus, sync.Status);
        Assert.Equal(expectedAttachStatus, attach.Status);
        Assert.Equal(expectedListStatus, list.Status);
        Assert.Equal(expectedRecordStatus, inspect.Status);
        Assert.Empty(detach.Result.Plan.Links);
        Assert.Empty(sync.Result.Plan.Links);
        Assert.Empty(list.Result.Libraries);
        Assert.All(detach.Result.Findings, finding => Assert.Equal(expectedRecordStatus, finding.Status));
        Assert.NotEmpty(detach.Result.Findings);
        Assert.NotEmpty(sync.Result.Findings);
        Assert.NotEmpty(list.Result.Findings);
        Assert.NotEmpty(inspect.Result.Findings);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Library List and Inspect select lock registrations when the legacy record disagrees")]
    public async Task LockWinsOverLegacyRecord()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Link();
        workspace.OwnershipAt(".", LibraryMutationWorkspace.Leaf);
        workspace.Write(".agents/open-forge.libraries.json", """
            {"schemaVersion":1,"libraries":[{"id":"legacy-only","sourceRoot":"shared/absent","destinationRoot":".","paths":["legacy.md"]}]}
            """);
        var before = workspace.Snapshot();

        var list = await new LibraryListOperation().ExecuteAsync(new() { Workspace = workspace.Workspace }, TestContext.Current.CancellationToken);
        var inspect = await new LibraryInspectOperation().ExecuteAsync(new()
        {
            Workspace = workspace.Workspace,
            LibraryId = LibraryId.Create("team-knowledge"),
        }, TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, list.Status);
        Assert.Equal("team-knowledge", Assert.Single(list.Result.Libraries).Id);
        Assert.Equal(CliSemanticStatus.Complete, inspect.Status);
        Assert.Equal("team-knowledge", inspect.Result.Record.Id);
        Assert.Equal(LibraryMutationWorkspace.OwnershipPath, inspect.Result.Record.Path);
        Assert.Equal(before, workspace.Snapshot());
    }
}
