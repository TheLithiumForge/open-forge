using System.Text;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Serialization;

/// <summary>
/// The declared schema is documentation, not a gate. These prove the property
/// that makes adding a version safe: no value of it, including one from a release
/// that does not exist yet, can stop an older CLI from reading the file.
/// </summary>
[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit")]
public sealed class WorkspaceSettingsSchemaTests
{
    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "A declared schema version is read back")]
    public void ReadsDeclaredVersion()
    {
        var read = WorkspaceSettingsCodec.Read(
            Encoding.UTF8.GetBytes("""{"schemaVersion":1,"allowInstallPaths":["docs"]}"""));

        var document = Assert.IsType<WorkspaceSettingsDocument>(read.Document);
        Assert.Equal(1, document.SchemaVersion);
        Assert.True(document.IsKnownSchemaVersion);
    }

    /// <summary>
    /// A file written before the key existed, or by hand, is not making a mistake.
    /// </summary>
    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "An absent or null schema version reads as this release's own")]
    [InlineData("{}")]
    [InlineData("""{"schemaVersion":null}""")]
    [InlineData("""{"allowInstallPaths":["docs"]}""")]
    public void DefaultsAbsentVersion(string json)
    {
        var read = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(json));

        var document = Assert.IsType<WorkspaceSettingsDocument>(read.Document);
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaVersion, document.SchemaVersion);
        Assert.True(document.IsKnownSchemaVersion);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "An unrecognised schema version is still read")]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(4096)]
    public void ReadsUnrecognisedVersion(int version)
    {
        var read = WorkspaceSettingsCodec.Read(
            Encoding.UTF8.GetBytes($$"""{"schemaVersion":{{version}},"allowInstallPaths":["docs"]}"""));

        var document = Assert.IsType<WorkspaceSettingsDocument>(read.Document);
        Assert.Null(read.Cause);
        Assert.Equal(version, document.SchemaVersion);
        Assert.False(document.IsKnownSchemaVersion);
        Assert.Equal(["docs"], document.AllowInstallPaths);
    }

    /// <summary>
    /// A version that is not a number is a typing mistake in a key the CLI acts
    /// on, which is the one case worth telling the author about.
    /// </summary>
    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "A schema version that is not a whole number is reported")]
    [InlineData("""{"schemaVersion":"1"}""")]
    [InlineData("""{"schemaVersion":1.5}""")]
    [InlineData("""{"schemaVersion":true}""")]
    public void ReportsNonIntegerVersion(string json)
    {
        var read = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(json));

        Assert.Null(read.Document);
        Assert.Contains("whole number", read.Cause, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "A file the CLI creates declares its schema and version")]
    public void CreatedFileDeclaresItself()
    {
        var written = WorkspaceSettingsCodec.AddAllowInstallPath(ReadOnlyMemory<byte>.Empty, "docs");

        var json = Encoding.UTF8.GetString(Assert.IsType<byte[]>(written));
        Assert.Contains(WorkspaceSettingsDefinitions.SchemaUrl, json, StringComparison.Ordinal);
        Assert.Contains("\"schemaVersion\": 1", json, StringComparison.Ordinal);

        // The declaration must not cost the file its readability.
        var read = WorkspaceSettingsCodec.Read(Assert.IsType<byte[]>(written));
        Assert.Equal(["docs"], read.Document!.AllowInstallPaths);
    }

    /// <summary>
    /// Adding keys the author did not ask for to a document they own is not this
    /// command's business. A grant adds the grant, and nothing else.
    /// </summary>
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "A file the author already wrote is not given a schema or version")]
    public void ExistingFileIsLeftAsWritten()
    {
        var written = WorkspaceSettingsCodec.AddAllowInstallPath(
            Encoding.UTF8.GetBytes("""{"allowInstallPaths":["docs"]}"""),
            "tools");

        var json = Encoding.UTF8.GetString(Assert.IsType<byte[]>(written));
        Assert.DoesNotContain("$schema", json, StringComparison.Ordinal);
        Assert.DoesNotContain("schemaVersion", json, StringComparison.Ordinal);
        Assert.Equal(["docs", "tools"], WorkspaceSettingsCodec.Read(Assert.IsType<byte[]>(written)).Document!.AllowInstallPaths);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "An existing schema and version survive a grant")]
    public void DeclarationSurvivesAGrant()
    {
        var written = WorkspaceSettingsCodec.AddAllowInstallPath(
            Encoding.UTF8.GetBytes(
                """{"$schema":"https://example.invalid/v1.json","schemaVersion":1,"allowInstallPaths":["docs"]}"""),
            "tools");

        var json = Encoding.UTF8.GetString(Assert.IsType<byte[]>(written));
        Assert.Contains("https://example.invalid/v1.json", json, StringComparison.Ordinal);
        Assert.Contains("\"schemaVersion\": 1", json, StringComparison.Ordinal);
    }
}
