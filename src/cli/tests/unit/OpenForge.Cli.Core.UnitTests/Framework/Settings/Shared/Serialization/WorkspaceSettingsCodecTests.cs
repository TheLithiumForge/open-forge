using System.Text;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Serialization;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit")]
public sealed class WorkspaceSettingsCodecTests
{
    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Settings arrays are read in authored order")]
    public void ReadsSettingsArrays()
    {
        const string json = """
            {"allowInstallPaths":["docs/**","tools/*.md"],"removedCategories":["templates","skills"],"removedFiles":[".agents/memory/_memory.md","AGENTS.md"],"removedDirectories":["docs/obsolete"],"removedExtensions":["planning"],"removedLibraries":["team-knowledge"]}
            """;

        var result = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(json));

        var document = Assert.IsType<WorkspaceSettingsDocument>(result.Document);
        Assert.Null(result.Cause);
        Assert.Equal(["docs/**", "tools/*.md"], document.AllowInstallPaths);
        Assert.Equal(["templates", "skills"], document.RemovedCategories);
        Assert.Equal([".agents/memory/_memory.md", "AGENTS.md"], document.RemovedFiles);
        Assert.Equal(["docs/obsolete"], document.RemovedDirectories);
        Assert.Equal(["planning"], document.RemovedExtensions);
        Assert.Equal(["team-knowledge"], document.RemovedLibraries);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "An absent, null or empty array reads as no entries")]
    [InlineData("{}")]
    [InlineData("""{"allowInstallPaths":[],"removedCategories":[],"removedFiles":[],"removedDirectories":[],"removedExtensions":[],"removedLibraries":[]}""")]
    [InlineData("""{"allowInstallPaths":null,"removedCategories":null}""")]
    public void ReadsDefaults(string json)
    {
        var result = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(json));

        var document = Assert.IsType<WorkspaceSettingsDocument>(result.Document);
        Assert.Empty(document.AllowInstallPaths);
        Assert.Empty(document.RemovedCategories);
        Assert.Empty(document.RemovedFiles);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Removed files are an optional array and default to empty")]
    public void RemovedFilesDefaultToEmpty()
    {
        var result = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes("{}"));

        Assert.Empty(Assert.IsType<WorkspaceSettingsDocument>(result.Document).RemovedFiles);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "The four-argument settings constructor initializes additive removal lists")]
    public void PreservesSettingsConstructorAndInitializesNewLists()
    {
        var document = new WorkspaceSettingsDocument(1, ["docs"], ["skills"], ["README.md"]);

        Assert.Empty(document.RemovedDirectories);
        Assert.Empty(document.RemovedExtensions);
        Assert.Empty(document.RemovedLibraries);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Removed files reject malformed and noncanonical paths")]
    [InlineData("null")]
    [InlineData("\".agents/loader.md\"")]
    [InlineData("[1]")]
    [InlineData("[\"\"]")]
    [InlineData("[\"/AGENTS.md\"]")]
    [InlineData("[\"C:/AGENTS.md\"]")]
    [InlineData("[\".agents\\\\loader.md\"]")]
    [InlineData("[\".agents/../AGENTS.md\"]")]
    [InlineData("[\".agents/./loader.md\"]")]
    [InlineData("[\".agents//loader.md\"]")]
    [InlineData("[\".agents/templates/\"]")]
    [InlineData("[\".agents/templates/*.md\"]")]
    [InlineData("[\".agents/loader.md\",\".agents/loader.md\"]")]
    [InlineData("[\".agents/open-forge.json\"]")]
    [InlineData("[\".agents/open-forge.lock.json\"]")]
    [InlineData("[\".git/config\"]")]
    public void RejectsMalformedRemovedFiles(string array)
    {
        var result = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes($"{{\"removedFiles\":{array}}}"));

        Assert.Null(result.Document);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Removed directories reject noncanonical and reserved paths")]
    [InlineData("null")]
    [InlineData("[1]")]
    [InlineData("[\"/docs\"]")]
    [InlineData("[\"C:/docs\"]")]
    [InlineData("[\"docs/../other\"]")]
    [InlineData("[\"docs/archive/\"]")]
    [InlineData("[\"docs/*.md\"]")]
    [InlineData("[\".\"]")]
    [InlineData("[\".agents\"]")]
    [InlineData("[\".agents/open-forge.json\"]")]
    [InlineData("[\".agents/open-forge.lock.json\"]")]
    [InlineData("[\".git\"]")]
    [InlineData("[\"docs/.git/config\"]")]
    public void RejectsMalformedRemovedDirectories(string array)
        => RejectsRemovalArray("removedDirectories", array);

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Removed categories are unique canonical root names")]
    [InlineData("[\"templates\",\"templates\"]")]
    [InlineData("[\"templates/nested\"]")]
    [InlineData("[\".git\"]")]
    [InlineData("[\"open-forge.json\"]")]
    public void RejectsMalformedRemovedCategories(string array)
        => RejectsRemovalArray("removedCategories", array);

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Removed IDs use existing stable identity validation")]
    [InlineData("removedExtensions", "Planning")]
    [InlineData("removedExtensions", "planning_")]
    [InlineData("removedLibraries", "Team-Knowledge")]
    [InlineData("removedLibraries", "team--knowledge")]
    public void RejectsMalformedRemovedIds(string property, string id)
        => RejectsRemovalArray(property, System.Text.Json.JsonSerializer.Serialize(new[] { id }));

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Every removal list rejects ordinal duplicates")]
    [InlineData("removedCategories", "templates")]
    [InlineData("removedFiles", "README.md")]
    [InlineData("removedDirectories", "docs/archive")]
    [InlineData("removedExtensions", "planning")]
    [InlineData("removedLibraries", "team-knowledge")]
    public void RejectsDuplicateRemovalEntries(string property, string item)
    {
        var encoded = System.Text.Json.JsonSerializer.Serialize(item);
        RejectsRemovalArray(property, $"[{encoded},{encoded}]");
    }

    private static void RejectsRemovalArray(string property, string array)
    {
        var json = $"{{\"{property}\":{array}}}";
        var result = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(json));

        Assert.Null(result.Document);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    /// <summary>
    /// The authored file gains keys over releases and a person may add one this
    /// CLI has not heard of. Refusing it would make a newer file unusable by an
    /// older CLI and buy no safety, so unknown keys are read past.
    /// </summary>
    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "An unknown key is accepted and ignored")]
    [InlineData("""{"rules":{"reference.cycle":"off"},"allowInstallPaths":["docs/**"]}""")]
    [InlineData("""{"$schema":"https://example.invalid/open-forge.json","allowInstallPaths":["docs/**"]}""")]
    [InlineData("""{"allowInstallPaths":["docs/**"],"thresholds":{"outputLines":{"warn":200}}}""")]
    public void AcceptsUnknownKeys(string json)
    {
        var result = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(json));

        var document = Assert.IsType<WorkspaceSettingsDocument>(result.Document);
        Assert.Null(result.Cause);
        Assert.Equal(["docs/**"], document.AllowInstallPaths);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Comments and a trailing comma are tolerated in an authored file")]
    [InlineData("""{"allowInstallPaths":["docs/**"],}""")]
    [InlineData("""
        {
          // the docs tree is ours to write
          "allowInstallPaths": ["docs/**"]
        }
        """)]
    public void ToleratesAuthoringConveniences(string json)
    {
        var result = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(json));

        var document = Assert.IsType<WorkspaceSettingsDocument>(result.Document);
        Assert.Equal(["docs/**"], document.AllowInstallPaths);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "A malformed value is reported with a cause rather than skipped")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("\"text\"")]
    [InlineData("{")]
    [InlineData("""{"allowInstallPaths":"docs/**"}""")]
    [InlineData("""{"allowInstallPaths":[1]}""")]
    [InlineData("""{"allowInstallPaths":[""]}""")]
    [InlineData("""{"allowInstallPaths":["  "]}""")]
    [InlineData("""{"removedCategories":{"templates":true}}""")]
    [InlineData("""{"removedFiles":[".agents/../loader.md"]}""")]
    public void ReportsMalformedInput(string json)
    {
        var result = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(json));

        Assert.Null(result.Document);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Invalid UTF-8 is reported rather than silently replaced")]
    public void ReportsInvalidUtf8()
    {
        var result = WorkspaceSettingsCodec.Read(new byte[] { 0x7B, 0xFF, 0xFE, 0x7D });

        Assert.Null(result.Document);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }
}
