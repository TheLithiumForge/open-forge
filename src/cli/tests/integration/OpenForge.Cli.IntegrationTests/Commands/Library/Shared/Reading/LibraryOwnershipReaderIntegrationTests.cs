using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Detach;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Inspect;
using OpenForge.Cli.Core.Commands.Library.List;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Commands.Library.Sync;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Reading;

[Trait("Feature", "ownership-operational-readers"), Trait("Evidence", "Integration")]
public sealed class LibraryOwnershipReaderIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library ownership boundaries never fall back to legacy claims or delete matching links")]
    [InlineData("invalid", false), InlineData("invalid", true)]
    [InlineData("unavailable", false), InlineData("unavailable", true)]
    [InlineData("absent", false)]
    public async Task MutationsDoNotFallBackToLegacyClaimsWhenOwnershipIsNotUsable(string condition, bool apply)
    {
        using var workspace = LegacyFixture(condition);
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
        var expectedMutationStatus = condition == "absent"
            ? CliSemanticStatus.Complete
            : CliSemanticStatus.Blocked;

        Assert.Equal(expectedMutationStatus, detach.Status);
        Assert.Equal(expectedMutationStatus, sync.Status);
        Assert.Equal(expectedAttachStatus, attach.Status);
        Assert.Equal(expectedListStatus, list.Status);
        Assert.Equal(expectedRecordStatus, inspect.Status);
        Assert.Empty(detach.Result.Plan.Links);
        Assert.Empty(sync.Result.Plan.Links);
        Assert.Contains(sync.Result.Findings, finding => finding.Status == expectedMutationStatus);
        Assert.Empty(list.Result.Libraries);
        Assert.All(detach.Result.Findings, finding => Assert.Equal(expectedMutationStatus, finding.Status));
        Assert.NotEmpty(detach.Result.Findings);
        Assert.NotEmpty(sync.Result.Findings);
        Assert.NotEmpty(list.Result.Findings);
        Assert.NotEmpty(inspect.Result.Findings);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Detach records exclusion without ownership and later Sync honors it"), Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task DetachPersistsAbsentOwnershipExclusionAndLaterSyncHonorsIt()
    {
        using var workspace = LegacyFixture("absent");
        var sourcePath = workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{LibraryMutationWorkspace.Leaf}");
        var destinationPath = workspace.Absolute(LibraryMutationWorkspace.Leaf);
        var legacyPath = workspace.Absolute(".agents/open-forge.libraries.json");
        var sourceBytes = File.ReadAllBytes(sourcePath);
        var legacyBytes = File.ReadAllBytes(legacyPath);
        var linkTarget = new FileInfo(destinationPath).LinkTarget;
        Assert.NotNull(linkTarget);
        var before = workspace.Snapshot();

        var listBefore = await new LibraryListOperation().ExecuteAsync(
            new() { Workspace = workspace.Workspace }, TestContext.Current.CancellationToken);
        var inspectBefore = await new LibraryInspectOperation().ExecuteAsync(new()
        {
            Workspace = workspace.Workspace,
            LibraryId = LibraryId.Create("team-knowledge"),
        }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, listBefore.Status);
        Assert.Equal(CliSemanticStatus.Complete, inspectBefore.Status);
        Assert.Equal(before, workspace.Snapshot());

        var detached = await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(
            workspace.Detach(LibraryMode.Apply), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, detached.Status);
        Assert.Equal("create", detached.Result.Plan.SettingsChange?.Action);
        Assert.Equal(LibraryRecordEffect.None, detached.Result.Plan.RecordEffect);
        Assert.Empty(detached.Result.Plan.Links);
        Assert.False(File.Exists(workspace.Absolute(LibraryMutationWorkspace.RecordPath)));
        Assert.Equal(linkTarget, new FileInfo(destinationPath).LinkTarget);
        Assert.Equal(sourceBytes, File.ReadAllBytes(sourcePath));
        Assert.Equal(legacyBytes, File.ReadAllBytes(legacyPath));
        using (var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Absolute(".agents/open-forge.json"))))
        {
            Assert.Equal("team-knowledge", Assert.Single(settings.RootElement.GetProperty("removedLibraries").EnumerateArray()).GetString());
        }

        var afterDetach = workspace.Snapshot();
        Assert.Equal(before.Count + 1, afterDetach.Count);
        Assert.All(before, entry => Assert.Equal(entry.Value, afterDetach[entry.Key]));
        Assert.Contains(".agents/open-forge.json", afterDetach.Keys);

        var sync = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(
            workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, sync.Status);
        Assert.Contains(sync.Result.Findings, finding => finding.Code == LibrarySyncFindingCode.LibraryRemoved);
        Assert.Empty(sync.Result.Plan.Links);
        Assert.Equal(afterDetach, workspace.Snapshot());
        Assert.Equal(linkTarget, new FileInfo(destinationPath).LinkTarget);
        Assert.Equal(legacyBytes, File.ReadAllBytes(legacyPath));

        var attach = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(
            workspace.Attach(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, attach.Status);
        Assert.Contains(attach.Result.Findings, finding => finding.Code == LibraryAttachFindingCode.LibraryRemoved);
        Assert.Equal(afterDetach, workspace.Snapshot());

        var listAfter = await new LibraryListOperation().ExecuteAsync(
            new() { Workspace = workspace.Workspace }, TestContext.Current.CancellationToken);
        var inspectAfter = await new LibraryInspectOperation().ExecuteAsync(new()
        {
            Workspace = workspace.Workspace,
            LibraryId = LibraryId.Create("team-knowledge"),
        }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, listAfter.Status);
        Assert.Equal(CliSemanticStatus.Complete, inspectAfter.Status);
        Assert.Equal(afterDetach, workspace.Snapshot());
    }

    private static LibraryMutationWorkspace LegacyFixture(string ownershipState)
    {
        var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Link();
        workspace.Write(".agents/open-forge.libraries.json", """
            {"schemaVersion":1,"libraries":[{"id":"team-knowledge","sourceRoot":"shared/team-knowledge","destinationRoot":".","paths":[".agents/directives/review.md"]}]}
            """);
        if (ownershipState == "invalid")
        {
            workspace.Write(LibraryMutationWorkspace.OwnershipPath, "{");
        }
        else if (ownershipState == "unavailable")
        {
            workspace.Directory(LibraryMutationWorkspace.OwnershipPath);
        }

        return workspace;
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
