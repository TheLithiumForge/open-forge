using System.Text;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Sources.Sharing;

namespace OpenForge.Cli.Core.UnitTests.Framework.Ownership.Shared.Serialization;

[Trait("Feature", "workspace-route-sharing"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class SourceSharingCodecTests
{
    [Fact(DisplayName = "Sharing records roundtrip without granting whole-file ownership")]
    public void RoundtripIsSeparateFromDeletionAuthority()
    {
        var framework = new FrameworkOwnership(new("framework", null), [], [])
        { GitIgnoredRoutes = [new(".agents/memory/working", ".agents/memory/working/index.md")] };
        var document = WorkspaceOwnershipDocument.Empty with { Framework = framework };
        var decoded = Assert.IsType<WorkspaceOwnershipDocument>(WorkspaceOwnershipCodec.Read(WorkspaceOwnershipCodec.Write(document)).Document);
        Assert.Equal(framework.GitIgnoredRoutes, Assert.IsType<FrameworkOwnership>(decoded.Framework).GitIgnoredRoutes);
        Assert.Empty(decoded.ManagedPaths());
        var sharing = new SourceSharing(framework.GitIgnoredRoutes);
        Assert.True(sharing.Includes(".agents/memory/working/index.md"));
        Assert.True(sharing.Includes(".agents/memory/_memory.md"));
        Assert.False(sharing.Includes(".agents/memory/working/note.md"));
        Assert.False(sharing.Includes(".agents/memory/working/index.overwrite.md"));
        Assert.False(sharing.Includes(".agents/memory/working/child/_child.md"));
    }

    [Fact(DisplayName = "Legacy ownership without sharing records keeps empty policy")]
    public void LegacyLockHasEmptyPolicy()
    {
        var decoded = WorkspaceOwnershipCodec.Read(Encoding.UTF8.GetBytes("""{"schemaVersion":1,"framework":{"source":{"id":"framework"},"paths":[],"regions":[]}}"""));
        Assert.Empty(Assert.IsType<FrameworkOwnership>(Assert.IsType<WorkspaceOwnershipDocument>(decoded.Document).Framework).GitIgnoredRoutes);
    }

    [Theory(DisplayName = "Malformed sharing facts make the lock invalid")]
    [InlineData("[null]")]
    [InlineData("null")]
    [InlineData("[{}]")]
    [InlineData("[{\"directory\":\"../private\",\"entrypoint\":\"../private/index.md\"}]")]
    [InlineData("[{\"directory\":\".agents/memory/working\",\"entrypoint\":\".agents/memory/working/child/index.md\"}]")]
    [InlineData("[{\"directory\":\".agents/memory/working\",\"entrypoint\":\".agents/memory/working/note.md\"}]")]
    [InlineData("[{\"directory\":\".agents/memory/working\",\"entrypoint\":\".agents/memory/working/index.md\"},{\"directory\":\".agents/memory/working\",\"entrypoint\":\".agents/memory/working/index.md\"}]")]
    [InlineData("[{\"directory\":\".agents/memory\",\"entrypoint\":\".agents/memory/_memory.md\"},{\"directory\":\".agents/memory/working\",\"entrypoint\":\".agents/memory/working/index.md\"}]")]
    public void RejectsMalformedRecords(string routes)
    {
        var json = $$$"""
            {"framework": {
                "source": {"id":"framework"},
                "gitIgnoredRoutes": {{{routes}}}
            }}
            """;
        var decoded = WorkspaceOwnershipCodec.Read(Encoding.UTF8.GetBytes(json));
        Assert.Null(decoded.Document);
        Assert.NotNull(decoded.Cause);
    }
}
