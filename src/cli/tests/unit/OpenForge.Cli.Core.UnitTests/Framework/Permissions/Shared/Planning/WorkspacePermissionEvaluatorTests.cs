using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Permissions.Models.Document;
using OpenForge.Cli.Core.Framework.Permissions.Models.Planning;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Planning;

namespace OpenForge.Cli.Core.UnitTests.Framework.Permissions.Shared.Planning;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Unit")]
public sealed class WorkspacePermissionEvaluatorTests
{
    [Fact]
    public void ComputesExactMissingSetWithoutSharingAnotherSubjectsGrant()
    {
        var document = new WorkspacePermissionDocument(
            Extensions: [new("team", [".apm/a.md"])],
            Libraries: [new("team", "shared/original", [".apm/a.md"], Directories: [])]);
        ImmutableArray<WorkspacePermissionRequirement> required =
        [
            new(new ExtensionPermissionSubject("team"), ".apm/b.md"),
            new(new ExtensionPermissionSubject("team"), ".apm/a.md"),
            new(new ExtensionPermissionSubject("other"), ".apm/a.md"),
            new(new LibraryPermissionSubject("team", "shared/other"), ".apm/a.md"),
            new(new ExtensionPermissionSubject("team"), ".apm/b.md"),
        ];

        var result = WorkspacePermissionEvaluator.Evaluate(document, required);

        Assert.Equal(WorkspacePermissionDecision.Required, result.Decision);
        Assert.Equal(4, result.Required.Length);
        Assert.Equal(3, result.Missing.Length);
        Assert.Contains(result.Missing, value => value.Subject is ExtensionPermissionSubject { Id: "other" });
        Assert.Contains(result.Missing, value => value.Subject is LibraryPermissionSubject { SourceRoot: "shared/other" });
        Assert.Contains(result.Missing, value => value.Subject is ExtensionPermissionSubject { Id: "team" } && value.Path == ".apm/b.md");
        Assert.DoesNotContain(result.Missing, value => value.Subject is ExtensionPermissionSubject { Id: "team" } && value.Path == ".apm/a.md");
    }

    [Fact]
    public void DistinguishesNoRequirementsFromExistingApproval()
    {
        var document = new WorkspacePermissionDocument([new("team", [".apm/a.md"])], []);

        var none = WorkspacePermissionEvaluator.Evaluate(document, []);
        var granted = WorkspacePermissionEvaluator.Evaluate(document, [new(new ExtensionPermissionSubject("team"), ".apm/a.md")]);

        Assert.Equal(WorkspacePermissionDecision.NotRequired, none.Decision);
        Assert.Empty(none.Missing);
        Assert.Equal(WorkspacePermissionDecision.Granted, granted.Decision);
        Assert.Empty(granted.Missing);
    }

    [Fact]
    public void GrantPreservesUnrelatedEntriesAndDoesNotMutateObservedDocument()
    {
        var document = new WorkspacePermissionDocument(
            Extensions: [new("team", ["a.txt"]), new("other", ["keep.txt"])],
            Libraries: [new("team", "shared/team", ["linked.txt"], Directories: [])]);

        var result = WorkspacePermissionEvaluator.Grant(
            document,
            [new(new ExtensionPermissionSubject("team"), "b.txt")]);

        Assert.Equal(["a.txt", "b.txt"], result.Extensions.Single(value => value.Id == "team").Paths);
        Assert.Equal(["keep.txt"], result.Extensions.Single(value => value.Id == "other").Paths);
        var library = Assert.Single(result.Libraries);
        Assert.Equal("team", library.Id);
        Assert.Equal("shared/team", library.SourceRoot);
        Assert.Equal(["linked.txt"], library.Paths);
        Assert.Equal(["a.txt"], document.Extensions.Single(value => value.Id == "team").Paths);
    }

    [Fact]
    public void EvaluatesPortableFileIdentityWithinTheExactSubject()
    {
        var document = new WorkspacePermissionDocument([new("team", [".apm/A.md"])], []);

        var result = WorkspacePermissionEvaluator.Evaluate(document, [new(new ExtensionPermissionSubject("team"), ".apm/a.md")]);

        Assert.Equal(WorkspacePermissionDecision.Granted, result.Decision);
        Assert.Empty(result.Missing);
        Assert.Equal(".apm/a.md", Assert.Single(result.Required).Path);
    }

    [Fact]
    public void DistinctPortableKeysDoNotInheritOrDiscardAnotherPathsGrant()
    {
        var document = new WorkspacePermissionDocument([new("team", [".apm/σ.md"])], []);
        ImmutableArray<WorkspacePermissionRequirement> requirements = [new(new ExtensionPermissionSubject("team"), ".apm/ς.md")];

        var result = WorkspacePermissionEvaluator.Evaluate(document, requirements);
        var granted = WorkspacePermissionEvaluator.Grant(document, requirements);

        Assert.Equal(WorkspacePermissionDecision.Required, result.Decision);
        Assert.Equal(".apm/ς.md", Assert.Single(result.Missing).Path);
        Assert.Equal([".apm/ς.md", ".apm/σ.md"], Assert.Single(granted.Extensions).Paths);
    }

    [Fact]
    public void GrantsLibraryPathsOnlyAtTheirBoundSourceRoot()
    {
        var document = new WorkspacePermissionDocument(
            Extensions: [new("team", ["owned.txt"])],
            Libraries: [new("team", "shared/team", ["a.txt"], Directories: [])]);

        var granted = WorkspacePermissionEvaluator.Grant(document,
            [new(new LibraryPermissionSubject("team", "shared/team"), "b.txt"),
             new(new LibraryPermissionSubject("other", "shared/other"), "c.txt")]);

        Assert.Equal(["a.txt", "b.txt"], granted.Libraries.Single(value => value.Id == "team").Paths);
        Assert.Equal("shared/other", granted.Libraries.Single(value => value.Id == "other").SourceRoot);
        Assert.Equal(["owned.txt"], Assert.Single(granted.Extensions).Paths);
        Assert.Throws<InvalidOperationException>(() => WorkspacePermissionEvaluator.Grant(document,
            [new(new LibraryPermissionSubject("team", "shared/rebound"), "b.txt")]));
        Assert.Equal(["a.txt"], Assert.Single(document.Libraries).Paths);
    }

    [Theory]
    [InlineData("docs/a.md", true), InlineData("docs/future/nested.md", true), InlineData("DOCS/A.md", true)]
    [InlineData("docs", false), InlineData("docs-other/a.md", false), InlineData("other/a.md", false)]
    public void LibraryDirectoryGrantUsesPortableDescendantSegments(string path, bool covered)
    {
        var document = new WorkspacePermissionDocument([], [new("team", "shared/team", [], ["docs"])]);

        var result = WorkspacePermissionEvaluator.Evaluate(document,
            [new(new LibraryPermissionSubject("team", "shared/team"), path)]);

        Assert.Equal(covered ? WorkspacePermissionDecision.Granted : WorkspacePermissionDecision.Required, result.Decision);
        Assert.Equal(covered ? 0 : 1, result.Missing.Length);
    }

    [Fact]
    public void DirectoryScopeNeverCrossesSubjectOrSourceIdentity()
    {
        var document = new WorkspacePermissionDocument([], [new("team", "shared/team", [], ["docs"])]);
        ImmutableArray<WorkspacePermissionRequirement> required =
        [
            new(new ExtensionPermissionSubject("team"), "docs/a.md"),
            new(new LibraryPermissionSubject("other", "shared/team"), "docs/a.md"),
            new(new LibraryPermissionSubject("team", "shared/new"), "docs/a.md"),
        ];

        var result = WorkspacePermissionEvaluator.Evaluate(document, required);

        Assert.Equal(3, result.Missing.Length);
        Assert.Equal(WorkspacePermissionDecision.Required, result.Decision);
    }

    [Theory]
    [InlineData(false), InlineData(true)]
    public void ExplicitLibraryGrantPreservesSameSourceAndReplacesOnlyReboundSubject(bool rebind)
    {
        var document = new WorkspacePermissionDocument([new("team", ["extension.txt"])],
            [new("other", "shared/other", ["other.txt"], ["other"]), new("team", "shared/old", ["old.txt"], ["old"])]);
        var subject = new LibraryPermissionSubject("team", rebind ? "shared/new" : "shared/old");
        var change = new LibraryPermissionGrantChange
        {
            Subject = subject,
            PreviousSourceRoot = rebind ? "shared/old" : null,
            ApprovedScopes = [new(subject, LibraryPermissionScopeKind.Directory, "docs")],
        };

        var result = WorkspacePermissionEvaluator.GrantLibrary(document, change);

        var selected = Assert.Single(result.Libraries, value => value.Id == "team");
        string[] expectedPaths = rebind ? [] : ["old.txt"];
        string[] expectedDirectories = rebind ? ["docs"] : ["docs", "old"];
        Assert.Equal(subject.SourceRoot, selected.SourceRoot);
        Assert.Equal(expectedPaths, selected.Paths);
        Assert.Equal(expectedDirectories, selected.Directories);
        Assert.Equal(document.Extensions, result.Extensions);
        Assert.Equal(document.Libraries[0], result.Libraries[0]);
        Assert.Equal(["old.txt"], document.Libraries[1].Paths);
        Assert.Equal(["old"], document.Libraries[1].Directories);
    }

    [Fact]
    public void LibraryGrantRequiresExplicitAccurateRebindingAuthority()
    {
        var document = new WorkspacePermissionDocument([], [new("team", "shared/old", [], ["old"])]);
        var subject = new LibraryPermissionSubject("team", "shared/new");
        var change = new LibraryPermissionGrantChange
        {
            Subject = subject,
            PreviousSourceRoot = null,
            ApprovedScopes = [new(subject, LibraryPermissionScopeKind.File, "README.md")],
        };

        Assert.Throws<InvalidOperationException>(() => WorkspacePermissionEvaluator.GrantLibrary(document, change));
        Assert.Throws<InvalidOperationException>(() => WorkspacePermissionEvaluator.GrantLibrary(document,
            change with { PreviousSourceRoot = "shared/unrelated" }));
    }

    [Fact]
    public void UndefinedScopeCannotBeRememberedAsAFileOrDirectoryGrant()
    {
        var subject = new LibraryPermissionSubject("team", "shared/team");
        var change = new LibraryPermissionGrantChange
        {
            Subject = subject,
            PreviousSourceRoot = null,
            ApprovedScopes = [new(subject, (LibraryPermissionScopeKind)int.MaxValue, "docs")],
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => WorkspacePermissionEvaluator.GrantLibrary(
            new WorkspacePermissionDocument([], []), change));
    }
}
