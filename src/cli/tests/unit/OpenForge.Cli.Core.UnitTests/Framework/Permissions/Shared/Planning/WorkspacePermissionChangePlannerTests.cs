using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Permissions.Models.Document;
using OpenForge.Cli.Core.Framework.Permissions.Models.Observation;
using OpenForge.Cli.Core.Framework.Permissions.Models.Planning;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Planning;

namespace OpenForge.Cli.Core.UnitTests.Framework.Permissions.Shared.Planning;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Unit")]
public sealed class WorkspacePermissionChangePlannerTests
{
    [Theory]
    [InlineData(false), InlineData(true)]
    public void ApprovedGrantPreservesExactPriorExpectationAndUnrelatedPermission(bool present)
    {
        var path = Path.GetFullPath(Path.Combine("permission-unit", ".agents", "open-forge.permissions.json"));
        const string original = "{ \"schemaVersion\":1,\"extensions\":[{\"id\":\"keep\",\"paths\":[\"keep.txt\"]}],\"libraries\":[] }\r\n";
        var document = present
            ? new WorkspacePermissionDocument([new("keep", ["keep.txt"])], [])
            : WorkspacePermissionDocument.Empty;
        var snapshot = present
            ? FileStateSnapshot.File(path, path, Encoding.UTF8.GetBytes(original))
            : FileStateSnapshot.Missing(path);
        var observation = new WorkspacePermissionRead(
            present ? WorkspacePermissionReadState.Complete : WorkspacePermissionReadState.Missing,
            document, snapshot, Cause: null);
        var requirement = new WorkspacePermissionRequirement(new ExtensionPermissionSubject("team"), ".apm/a.md");
        var approval = new WorkspacePermissionEvaluation([requirement], [requirement], WorkspacePermissionDecision.Approved);

        var change = Assert.IsType<PlannedFileChange>(WorkspacePermissionChangePlanner.Plan(new(observation, approval)));

        Assert.Equal(present ? PlannedFileChangeKind.Replace : PlannedFileChangeKind.Create, change.Kind);
        Assert.Equal(snapshot.Expectation, change.Expectation);
        Assert.Equal(path, change.LogicalPath);
        using var json = JsonDocument.Parse(change.IntendedBytes.AsMemory());
        var grants = json.RootElement.GetProperty("extensions").EnumerateArray().ToArray();
        var added = Assert.Single(grants, value => value.GetProperty("id").GetString() == "team");
        Assert.Equal(".apm/a.md", Assert.Single(added.GetProperty("paths").EnumerateArray()).GetString());
        if (present)
        {
            var kept = Assert.Single(grants, value => value.GetProperty("id").GetString() == "keep");
            Assert.Equal("keep.txt", Assert.Single(kept.GetProperty("paths").EnumerateArray()).GetString());
            Assert.Equal(original, Encoding.UTF8.GetString(snapshot.Bytes.AsSpan()));
        }
    }

    [Theory]
    [InlineData((int)WorkspacePermissionDecision.Required)]
    [InlineData((int)WorkspacePermissionDecision.Declined)]
    public void MissingPermissionWithoutApprovalProducesNoChange(int decision)
    {
        var path = Path.GetFullPath(Path.Combine("permission-unit", ".agents", "open-forge.permissions.json"));
        var observation = new WorkspacePermissionRead(
            WorkspacePermissionReadState.Missing, WorkspacePermissionDocument.Empty, FileStateSnapshot.Missing(path), Cause: null);
        var requirement = new WorkspacePermissionRequirement(new ExtensionPermissionSubject("team"), ".apm/a.md");
        var evaluation = new WorkspacePermissionEvaluation([requirement], [requirement], (WorkspacePermissionDecision)decision);

        Assert.Null(WorkspacePermissionChangePlanner.Plan(new(observation, evaluation)));
    }
}
