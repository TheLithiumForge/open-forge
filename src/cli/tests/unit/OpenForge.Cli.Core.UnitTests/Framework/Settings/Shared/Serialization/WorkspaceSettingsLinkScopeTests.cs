using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Serialization;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
public sealed class WorkspaceSettingsLinkScopeTests
{
    private const string OldFile = "docs/old.md";
    private const string OldDirectory = "docs/old";
    private const string OldDirectoryCasing = "Docs/old";
    private const string OldNote = "docs/old/note.md";
    private const string OldChild = "docs/old/file.md";
    private const string NewDirectory = "docs/new";
    private const string NewNote = "docs/new/note.md";
    private const string NewChild = "docs/new/file.md";
    private const string OlderDirectory = "docs/older";
    private const string Outside = "outside";
    private const string ExistingSchema = "custom";
    private const string Future = "future";
    private const string Keep = "keep";

    [Theory(DisplayName = "Link scope reads known fields across lower higher and absent settings versions")]
    [InlineData("")]
    [InlineData($"\"{WorkspaceSettingsDefinitions.SchemaVersionProperty}\":1,")]
    [InlineData($"\"{WorkspaceSettingsDefinitions.SchemaVersionProperty}\":2,")]
    [InlineData($"\"{WorkspaceSettingsDefinitions.SchemaVersionProperty}\":99,")]
    public void ReadsAuthoredLists(string version)
    {
        var decoded = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes($$$"""{ {{{version}}} "{{{WorkspaceSettingsDefinitions.IgnoreLinksFromProperty}}}":["{{{WorkspaceSettingsDefinitions.MemoryArchivePath}}}"],"{{{WorkspaceSettingsDefinitions.IgnoreLinksToProperty}}}":["{{{OldFile}}}"],"{{{Future}}}":{"x":true} }"""));
        var settings = Assert.IsType<WorkspaceSettingsDocument>(decoded.Document);
        Assert.True(settings.HasIgnoreLinksFrom);
        Assert.Equal([WorkspaceSettingsDefinitions.MemoryArchivePath], settings.IgnoreLinksFrom);
        Assert.Equal([OldFile], settings.IgnoreLinksTo);
        Assert.False(settings.IsKnownSchemaVersion && settings.SchemaVersion == 99);
    }

    [Theory(DisplayName = "Link scope validates array entries exactly like install path entries")]
    [InlineData("42")]
    [InlineData("[false]")]
    [InlineData("[\"\"]")]
    [InlineData("[\"  \"]")]
    public void ReportsInvalidEntries(string value)
    {
        var allow = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes($$$"""{"{{{WorkspaceSettingsDefinitions.AllowInstallPathsProperty}}}":{{{value}}}}"""));
        foreach (var key in new[] { WorkspaceSettingsDefinitions.IgnoreLinksFromProperty, WorkspaceSettingsDefinitions.IgnoreLinksToProperty })
        {
            var result = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes($$$"""{"{{{key}}}":{{{value}}}}"""));
            Assert.Null(result.Document);
            Assert.NotNull(allow.Cause);
            Assert.NotNull(result.Cause);
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
        var original = Encoding.UTF8.GetBytes($$$"""{"{{{WorkspaceSettingsDefinitions.SchemaProperty}}}":"{{{ExistingSchema}}}","{{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}}":1,"{{{Future}}}":{"{{{Keep}}}":7},"{{{WorkspaceSettingsDefinitions.IgnoreLinksFromProperty}}}":["{{{OldDirectoryCasing}}}","{{{OldNote}}}","{{{OlderDirectory}}}"],"{{{WorkspaceSettingsDefinitions.IgnoreLinksToProperty}}}":["{{{OldChild}}}","{{{Outside}}}"]}""");
        var moved = Assert.IsType<byte[]>(WorkspaceSettingsCodec.MoveLinkScope(original, OldDirectory, NewDirectory));
        var settings = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(moved).Document);
        Assert.Equal([NewDirectory, NewNote, OlderDirectory], settings.IgnoreLinksFrom);
        Assert.Equal([NewChild, Outside], settings.IgnoreLinksTo);
        var removed = Assert.IsType<byte[]>(WorkspaceSettingsCodec.RemoveLinkScope(moved, [NewDirectory]));
        using var json = JsonDocument.Parse(removed);
        Assert.Equal(ExistingSchema, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaProperty).GetString());
        Assert.Equal(1, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaVersionProperty).GetInt32());
        Assert.Equal(7, json.RootElement.GetProperty(Future).GetProperty(Keep).GetInt32());
        var after = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(removed).Document);
        Assert.Equal([OlderDirectory], after.IgnoreLinksFrom);
        Assert.Equal([Outside], after.IgnoreLinksTo);
    }

    [Theory(DisplayName = "Archive seeding honors the presence of an authored key even when empty or null")]
    [InlineData("[]")]
    [InlineData("null")]
    [InlineData("[\"my/archive\"]")]
    public void PreservesExistingChoice(string value)
        => Assert.Null(WorkspaceSettingsCodec.SeedArchiveLinkScope(Encoding.UTF8.GetBytes($$$"""{"{{{WorkspaceSettingsDefinitions.IgnoreLinksFromProperty}}}":{{{value}}}}"""), WorkspaceSettingsDefinitions.MemoryArchivePath));

    [Fact(DisplayName = "Archive seeding creates the versioned shape only when the key is absent")]
    public void SeedsAbsentChoice()
    {
        var bytes = Assert.IsType<byte[]>(WorkspaceSettingsCodec.SeedArchiveLinkScope(ReadOnlyMemory<byte>.Empty, WorkspaceSettingsDefinitions.MemoryArchivePath));
        var settings = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(bytes).Document);
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaVersion, settings.SchemaVersion);
        Assert.Equal([WorkspaceSettingsDefinitions.MemoryArchivePath], settings.IgnoreLinksFrom);
    }
}
