using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Serialization;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
public sealed class WorkspaceSettingsLinkScopeTests
{
    [Theory(DisplayName = "Link scope reads known fields across lower higher and absent settings versions")]
    [InlineData("")]
    [InlineData("\"schemaVersion\":1,")]
    [InlineData("\"schemaVersion\":2,")]
    [InlineData("\"schemaVersion\":99,")]
    public void ReadsAuthoredLists(string version)
    {
        var decoded = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes($$"""{ {{version}} "ignoreLinksFrom":[".agents/memory/archived"],"ignoreLinksTo":["docs/old.md"],"future":{"x":true} }"""));
        var settings = Assert.IsType<WorkspaceSettingsDocument>(decoded.Document);
        Assert.True(settings.HasIgnoreLinksFrom);
        Assert.Equal([".agents/memory/archived"], settings.IgnoreLinksFrom);
        Assert.Equal(["docs/old.md"], settings.IgnoreLinksTo);
        Assert.False(settings.IsKnownSchemaVersion && settings.SchemaVersion == 99);
    }

    [Theory(DisplayName = "Link scope validates array entries exactly like install path entries")]
    [InlineData("42")]
    [InlineData("[false]")]
    [InlineData("[\"\"]")]
    [InlineData("[\"  \"]")]
    public void ReportsInvalidEntries(string value)
    {
        var allow = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes($$"""{"allowInstallPaths":{{value}}}"""));
        foreach (var key in new[] { "ignoreLinksFrom", "ignoreLinksTo" })
        {
            var result = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes($$"""{"{{key}}":{{value}}}"""));
            Assert.Null(result.Document);
            Assert.Equal(allow.Cause?.Replace("allowInstallPaths", key, StringComparison.Ordinal), result.Cause);
        }
    }

    [Theory(DisplayName = "Link scope uses literal case-insensitive segment boundaries without filesystem work")]
    [InlineData("docs/archive", "DOCS/archive/old.md", true)]
    [InlineData("docs/archive", "docs/archived/old.md", false)]
    [InlineData("docs/note.md", "DOCS/note.md", true)]
    [InlineData("docs/note.md", "docs/other.md", false)]
    [InlineData("docs/*.md", "docs/note.md", false)]
    public void MatchesInstallPaths(string entry, string path, bool expected)
    {
        var settings = WorkspaceSettingsDocument.Empty with { IgnoreLinksFrom = [entry], IgnoreLinksTo = [entry] };
        Assert.Equal(WorkspaceAllowList.Admits([entry], path), WorkspaceLinkScope.IgnoresSource(settings, path));
        Assert.Equal(expected, WorkspaceLinkScope.IgnoresTarget(settings, path));
    }

    [Fact(DisplayName = "Move and removal change only matching link entries and preserve authored unknown settings")]
    public void RewritesAndPreservesSettings()
    {
        var original = Encoding.UTF8.GetBytes("""{"$schema":"custom","schemaVersion":1,"future":{"keep":7},"ignoreLinksFrom":["Docs/old","docs/old/note.md","docs/older"],"ignoreLinksTo":["docs/old/file.md","outside"]}""");
        var moved = Assert.IsType<byte[]>(WorkspaceSettingsCodec.MoveLinkScope(original, "docs/old", "docs/new"));
        var settings = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(moved).Document);
        Assert.Equal(["docs/new", "docs/new/note.md", "docs/older"], settings.IgnoreLinksFrom);
        Assert.Equal(["docs/new/file.md", "outside"], settings.IgnoreLinksTo);
        var removed = Assert.IsType<byte[]>(WorkspaceSettingsCodec.RemoveLinkScope(moved, ["docs/new"]));
        using var json = JsonDocument.Parse(removed);
        Assert.Equal("custom", json.RootElement.GetProperty("$schema").GetString());
        Assert.Equal(1, json.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(7, json.RootElement.GetProperty("future").GetProperty("keep").GetInt32());
        var after = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(removed).Document);
        Assert.Equal(["docs/older"], after.IgnoreLinksFrom);
        Assert.Equal(["outside"], after.IgnoreLinksTo);
    }

    [Theory(DisplayName = "Archive seeding honors the presence of an authored key even when empty or null")]
    [InlineData("[]")]
    [InlineData("null")]
    [InlineData("[\"my/archive\"]")]
    public void PreservesExistingChoice(string value)
        => Assert.Null(WorkspaceSettingsCodec.SeedArchiveLinkScope(Encoding.UTF8.GetBytes($$"""{"ignoreLinksFrom":{{value}}}""")));

    [Fact(DisplayName = "Archive seeding creates the versioned shape only when the key is absent")]
    public void SeedsAbsentChoice()
    {
        var bytes = Assert.IsType<byte[]>(WorkspaceSettingsCodec.SeedArchiveLinkScope(ReadOnlyMemory<byte>.Empty));
        var settings = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(bytes).Document);
        Assert.Equal(2, settings.SchemaVersion);
        Assert.Equal([".agents/memory/archived"], settings.IgnoreLinksFrom);
    }
}
