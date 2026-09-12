using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Permissions.Models.Document;
using OpenForge.Cli.Core.Framework.Permissions.Models.Observation;
using OpenForge.Cli.Core.Framework.Permissions.Models.Planning;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Planning;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Permissions.Shared.Planning;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Unit")]
public sealed class WorkspacePermissionChangePlannerTests
{
    [Theory(DisplayName = "Approved permission grants with canonical unchanged bytes remain no-ops")]
    [InlineData(false), InlineData(true)]
    public void CanonicalUnchangedApprovalReturnsNoChange(bool library)
    {
        var document = library
            ? new WorkspacePermissionDocument([], [new("team", "shared/team", [".apm/a.md"], [])])
            : new WorkspacePermissionDocument([new("team", [".apm/a.md"])], []);
        var path = Path.GetFullPath("permission-unit/permissions.json");
        var snapshot = FileStateSnapshot.File(path, path, WorkspacePermissionCodec.Write(document));
        var observation = new WorkspacePermissionRead(WorkspacePermissionReadState.Complete, document, snapshot, Cause: null);

        Assert.Null(PlanApproved(library, observation));
    }

    [Theory(DisplayName = "Permission planning preserves safe observation and decoded document guard precedence")]
    [InlineData(false, (int)WorkspacePermissionReadState.Invalid, true, "Permission approval requires a complete safe observation.")]
    [InlineData(true, (int)WorkspacePermissionReadState.Invalid, true, "Permission approval requires a complete safe observation.")]
    [InlineData(false, (int)WorkspacePermissionReadState.Complete, false, "Permission approval requires a complete safe observation.")]
    [InlineData(true, (int)WorkspacePermissionReadState.Complete, false, "Permission approval requires a complete safe observation.")]
    [InlineData(false, (int)WorkspacePermissionReadState.Complete, true, "A complete permission observation requires its decoded document.")]
    [InlineData(true, (int)WorkspacePermissionReadState.Complete, true, "A complete permission observation requires its decoded document.")]
    public void InvalidObservationRetainsExactFailure(bool library, int state, bool hasSnapshot, string message)
    {
        var snapshot = hasSnapshot ? FileStateSnapshot.Missing(Path.GetFullPath("permission-unit/permissions.json")) : null;
        var observation = new WorkspacePermissionRead((WorkspacePermissionReadState)state, Document: null, snapshot, Cause: "observed");

        var exception = Assert.Throws<InvalidOperationException>(() => PlanApproved(library, observation));

        Assert.Equal(message, exception.Message);
    }

    [Fact(DisplayName = "No-work permission approvals return before observing unsafe documents")]
    public void NoWorkSkipsObservationButPriorLibraryRootRequiresIt()
    {
        var observation = new WorkspacePermissionRead(WorkspacePermissionReadState.Invalid, Document: null, Snapshot: null, Cause: "unsafe");
        var requirement = new WorkspacePermissionRequirement(new ExtensionPermissionSubject("team"), ".apm/a.md");
        var subject = new LibraryPermissionSubject("team", "shared/team");

        Assert.Null(WorkspacePermissionChangePlanner.Plan(new(observation, new([requirement], [], WorkspacePermissionDecision.Approved))));
        Assert.Null(WorkspacePermissionChangePlanner.Plan(new(observation, new([requirement], [requirement], WorkspacePermissionDecision.Required))));
        Assert.Null(WorkspacePermissionChangePlanner.PlanLibrary(new(observation, new()
        {
            Subject = subject,
            PreviousSourceRoot = null,
            ApprovedScopes = [],
        })));
        var exception = Assert.Throws<InvalidOperationException>(() => WorkspacePermissionChangePlanner.PlanLibrary(new(observation, new()
        {
            Subject = subject,
            PreviousSourceRoot = "shared/old",
            ApprovedScopes = [],
        })));
        Assert.Equal("Permission approval requires a complete safe observation.", exception.Message);
    }

    [Theory(DisplayName = "Missing permission observations ignore an attached decoded document")]
    [InlineData(false), InlineData(true)]
    public void MissingObservationUsesEmptyDocument(bool library)
    {
        var document = new WorkspacePermissionDocument([new("keep", ["keep.txt"])], []);
        var observation = new WorkspacePermissionRead(WorkspacePermissionReadState.Missing, document,
            FileStateSnapshot.Missing(Path.GetFullPath("permission-unit/permissions.json")), Cause: null);

        var change = Assert.IsType<PlannedFileChange>(PlanApproved(library, observation));

        Assert.Equal(PlannedFileChangeKind.Create, change.Kind);
        using var json = JsonDocument.Parse(change.IntendedBytes.AsMemory());
        Assert.DoesNotContain(json.RootElement.GetProperty("extensions").EnumerateArray(), value => value.GetProperty("id").GetString() == "keep");
    }

    private static PlannedFileChange? PlanApproved(bool library, WorkspacePermissionRead observation)
    {
        if (library)
        {
            var subject = new LibraryPermissionSubject("team", "shared/team");
            return WorkspacePermissionChangePlanner.PlanLibrary(new(observation, new()
            {
                Subject = subject,
                PreviousSourceRoot = null,
                ApprovedScopes = [new(subject, LibraryPermissionScopeKind.File, ".apm/a.md")],
            }));
        }
        var requirement = new WorkspacePermissionRequirement(new ExtensionPermissionSubject("team"), ".apm/a.md");
        return WorkspacePermissionChangePlanner.Plan(new(observation, new([requirement], [requirement], WorkspacePermissionDecision.Approved)));
    }

    [Theory]
    [InlineData(false), InlineData(true)]
    public void ExplicitLibraryScopesPlanOneOrdinaryFileChangeWithExactPriorState(bool present)
    {
        var path = Path.GetFullPath(Path.Combine("permission-unit", ".agents", "open-forge.permissions.json"));
        const string original = "{ \"schemaVersion\":1,\"extensions\":[{\"id\":\"keep\",\"paths\":[\"keep.txt\"]}],\"libraries\":[] }\r\n";
        var document = present ? new WorkspacePermissionDocument([new("keep", ["keep.txt"])], []) : WorkspacePermissionDocument.Empty;
        var snapshot = present ? FileStateSnapshot.File(path, path, Encoding.UTF8.GetBytes(original)) : FileStateSnapshot.Missing(path);
        var observation = new WorkspacePermissionRead(present ? WorkspacePermissionReadState.Complete : WorkspacePermissionReadState.Missing,
            document, snapshot, Cause: null);
        var subject = new LibraryPermissionSubject("team", "shared/team");
        var approval = new LibraryPermissionGrantChange
        {
            Subject = subject,
            PreviousSourceRoot = null,
            ApprovedScopes = [new(subject, LibraryPermissionScopeKind.Directory, "docs"), new(subject, LibraryPermissionScopeKind.File, "README.md")],
        };

        var change = Assert.IsType<PlannedFileChange>(WorkspacePermissionChangePlanner.PlanLibrary(new(observation, approval)));

        Assert.Equal(present ? PlannedFileChangeKind.Replace : PlannedFileChangeKind.Create, change.Kind);
        Assert.Equal(snapshot.Expectation, change.Expectation);
        using var json = JsonDocument.Parse(change.IntendedBytes.AsMemory());
        var library = Assert.Single(json.RootElement.GetProperty("libraries").EnumerateArray());
        Assert.Equal("team", library.GetProperty("id").GetString());
        Assert.Equal("shared/team", library.GetProperty("sourceRoot").GetString());
        Assert.Equal("README.md", Assert.Single(library.GetProperty("paths").EnumerateArray()).GetString());
        Assert.Equal("docs", Assert.Single(library.GetProperty("directories").EnumerateArray()).GetString());
        Assert.Equal(present ? 1 : 0, json.RootElement.GetProperty("extensions").GetArrayLength());
        if (present)
        {
            Assert.Equal(original, Encoding.UTF8.GetString(snapshot.Bytes.AsSpan()));
        }
    }

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
