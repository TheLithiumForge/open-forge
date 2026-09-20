using System.Text;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Serialization;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit")]
public sealed class WorkspaceSettingsCodecTests
{
    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Both settings arrays are read in authored order")]
    public void ReadsBothArrays()
    {
        const string json = """
            {"allowInstallPaths":["docs/**","tools/*.md"],"removedCategories":["templates","skills"]}
            """;

        var result = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(json));

        var document = Assert.IsType<WorkspaceSettingsDocument>(result.Document);
        Assert.Null(result.Cause);
        Assert.Equal(["docs/**", "tools/*.md"], document.AllowInstallPaths);
        Assert.Equal(["templates", "skills"], document.RemovedCategories);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "An absent, null or empty array reads as no entries")]
    [InlineData("{}")]
    [InlineData("""{"allowInstallPaths":[],"removedCategories":[]}""")]
    [InlineData("""{"allowInstallPaths":null,"removedCategories":null}""")]
    public void ReadsDefaults(string json)
    {
        var result = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(json));

        var document = Assert.IsType<WorkspaceSettingsDocument>(result.Document);
        Assert.Empty(document.AllowInstallPaths);
        Assert.Empty(document.RemovedCategories);
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
