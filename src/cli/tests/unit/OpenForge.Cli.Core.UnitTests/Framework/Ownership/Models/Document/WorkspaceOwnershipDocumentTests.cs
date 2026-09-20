using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;

namespace OpenForge.Cli.Core.UnitTests.Framework.Ownership.Models.Document;

[Trait("Feature", "workspace-ownership"), Trait("Evidence", "Unit")]
public sealed class WorkspaceOwnershipDocumentTests
{
    /// <summary>
    /// Nothing recorded means nothing owned, which means nothing deleted. This is
    /// the direction the lock is allowed to be wrong in: a missed deletion, never
    /// an over-deletion.
    /// </summary>
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "An empty document owns nothing and claims the current schema")]
    public void EmptyOwnsNothing()
    {
        var empty = WorkspaceOwnershipDocument.Empty;

        Assert.Null(empty.Framework);
        Assert.Empty(empty.Extensions);
        Assert.Empty(empty.Libraries);
        Assert.Empty(empty.OwnersOf("anything.md"));
        Assert.True(empty.IsKnownSchemaVersion);
        Assert.Equal(WorkspaceOwnershipDefinitions.SchemaVersion, empty.SchemaVersion);
    }

    /// <summary>
    /// Shared ownership is derived rather than stored, so an install only appends
    /// its own receipt instead of merging into a table every other owner shares.
    /// </summary>
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Shared ownership is derived from the per-extension receipts")]
    public void DerivesSharedOwnership()
    {
        var document = new WorkspaceOwnershipDocument(
            WorkspaceOwnershipDefinitions.SchemaVersion,
            Framework: null,
            [
                new ExtensionOwnership("a", null, null, [], ["shared.md", "only-a.md"], []),
                new ExtensionOwnership("b", null, null, [], ["shared.md"], []),
            ],
            []);

        Assert.Equal(["a", "b"], document.OwnersOf("shared.md"));
        Assert.Equal(["a"], document.OwnersOf("only-a.md"));
        Assert.Empty(document.OwnersOf("owned-by-nobody.md"));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Framework ownership is not extension ownership")]
    public void FrameworkPathsAreNotExtensionOwners()
    {
        var document = new WorkspaceOwnershipDocument(
            WorkspaceOwnershipDefinitions.SchemaVersion,
            new FrameworkOwnership(new OwnedSource("open-forge", "0.1.0"), ["loader.md"], []),
            [],
            []);

        Assert.Empty(document.OwnersOf("loader.md"));
        Assert.Equal(["loader.md"], document.Framework?.Paths);
    }
}
