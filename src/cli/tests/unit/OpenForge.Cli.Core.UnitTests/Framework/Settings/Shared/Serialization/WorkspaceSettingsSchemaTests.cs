using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Serialization;

/// <summary>
/// The declared schema is documentation, not a gate. These prove the property
/// that makes adding a version safe: no value of it, including one from a release
/// that does not exist yet, can stop an older CLI from reading the file.
/// </summary>
[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit")]
public sealed class WorkspaceSettingsSchemaTests
{
    private const string Docs = "docs";
    private const string Tools = "tools";
    private const string ExistingSchema = "https://example.invalid/v1.json";

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "A declared schema version is read back")]
    public void ReadsDeclaredVersion()
    {
        var read = WorkspaceSettingsCodec.Read(
            Encoding.UTF8.GetBytes($$$"""{"{{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}}":1,"{{{WorkspaceSettingsDefinitions.AllowInstallPathsProperty}}}":["{{{Docs}}}"]}"""));

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
    [InlineData($$$"""{"{{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}}":null}""")]
    [InlineData($$$"""{"{{{WorkspaceSettingsDefinitions.AllowInstallPathsProperty}}}":["{{{Docs}}}"]}""")]
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
    [InlineData(WorkspaceSettingsDefinitions.SchemaVersion + 1)]
    [InlineData(4096)]
    public void ReadsUnrecognisedVersion(int version)
    {
        var read = WorkspaceSettingsCodec.Read(
            Encoding.UTF8.GetBytes($$$"""{"{{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}}":{{{version}}},"{{{WorkspaceSettingsDefinitions.AllowInstallPathsProperty}}}":["{{{Docs}}}"]}"""));

        var document = Assert.IsType<WorkspaceSettingsDocument>(read.Document);
        Assert.Null(read.Cause);
        Assert.Equal(version, document.SchemaVersion);
        Assert.False(document.IsKnownSchemaVersion);
        Assert.Equal([Docs], document.AllowInstallPaths);
    }

    /// <summary>
    /// A version that is not a number is a typing mistake in a key the CLI acts
    /// on, which is the one case worth telling the author about.
    /// </summary>
    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "A schema version that is not a whole number is reported")]
    [InlineData($$$"""{"{{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}}":"1"}""")]
    [InlineData($$$"""{"{{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}}":1.5}""")]
    [InlineData($$$"""{"{{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}}":true}""")]
    public void ReportsNonIntegerVersion(string json)
    {
        var read = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(json));

        Assert.Null(read.Document);
        Assert.NotNull(read.Cause);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "A file the CLI creates declares its schema and version")]
    public void CreatedFileDeclaresItself()
    {
        var written = WorkspaceSettingsCodec.AddAllowInstallPath(ReadOnlyMemory<byte>.Empty, Docs);

        using var json = JsonDocument.Parse(Assert.IsType<byte[]>(written));
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaUrl, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaProperty).GetString());
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaVersion, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaVersionProperty).GetInt32());

        // The declaration must not cost the file its readability.
        var read = WorkspaceSettingsCodec.Read(Assert.IsType<byte[]>(written));
        Assert.Equal([Docs], read.Document!.AllowInstallPaths);
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
            Encoding.UTF8.GetBytes($$$"""{"{{{WorkspaceSettingsDefinitions.AllowInstallPathsProperty}}}":["{{{Docs}}}"]}"""),
            Tools);

        using var json = JsonDocument.Parse(Assert.IsType<byte[]>(written));
        Assert.False(json.RootElement.TryGetProperty(WorkspaceSettingsDefinitions.SchemaProperty, out _));
        Assert.False(json.RootElement.TryGetProperty(WorkspaceSettingsDefinitions.SchemaVersionProperty, out _));
        Assert.Equal([Docs, Tools], WorkspaceSettingsCodec.Read(Assert.IsType<byte[]>(written)).Document!.AllowInstallPaths);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "An existing schema and version survive a grant")]
    public void DeclarationSurvivesAGrant()
    {
        var written = WorkspaceSettingsCodec.AddAllowInstallPath(
            Encoding.UTF8.GetBytes(
                $$$"""{"{{{WorkspaceSettingsDefinitions.SchemaProperty}}}":"{{{ExistingSchema}}}","{{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}}":1,"{{{WorkspaceSettingsDefinitions.AllowInstallPathsProperty}}}":["{{{Docs}}}"]}"""),
            Tools);

        using var json = JsonDocument.Parse(Assert.IsType<byte[]>(written));
        Assert.Equal(ExistingSchema, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaProperty).GetString());
        Assert.Equal(1, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaVersionProperty).GetInt32());
    }
}
