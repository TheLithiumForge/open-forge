using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Serialization;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class WorkspaceSettingsMainRootTests
{
    private const string CustomRoot = "docs/forge";
    private const string OtherRoot = "forge";
    private const string Note = "notes.md";
    private const string First = "first";
    private const string Last = "last";
    private const string Keep = "keep";
    private const string Unknown = "unknown";
    private const string ExistingSchema = "custom";
    private const int ExistingVersion = 1;

    public static TheoryData<string> InvalidValues => new()
    {
        "null", "1", "true", "[]", "{}",
        JsonValue.Create(WorkspaceLayout.DefaultMainRoot + "/child")!.ToJsonString(),
        JsonValue.Create(WorkspaceLayout.DefaultMainRoot.ToUpperInvariant())!.ToJsonString(),
        JsonValue.Create("../forge")!.ToJsonString(),
        JsonValue.Create(WorkspaceRuntimePaths.DirectoryName + "/forge")!.ToJsonString(),
    };

    [Theory(DisplayName = "Main root rejects null, wrong types and noncanonical values")]
    [MemberData(nameof(InvalidValues))]
    public void RejectsInvalidValues(string value)
        => Assert.Null(WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes($$$"""{"{{{WorkspaceSettingsDefinitions.MainRootProperty}}}":{{{value}}}}""")).Document);

    [Fact(DisplayName = "Absent default is never inserted and unchanged roots preserve exact bytes")]
    public void DefaultAndNoOp()
    {
        var document = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(Bytes(new())).Document);
        Assert.Null(document.DeclaredMainRoot);
        Assert.Equal(WorkspaceLayout.DefaultMainRoot, document.MainRoot);
        Assert.Null(WorkspaceSettingsCodec.SetMainRoot(ReadOnlyMemory<byte>.Empty, WorkspaceLayout.DefaultMainRoot));
        Assert.Null(WorkspaceSettingsCodec.SetMainRoot(Bytes(new() { [Unknown] = 7 }), WorkspaceLayout.DefaultMainRoot));
        Assert.Null(WorkspaceSettingsCodec.SetMainRoot(Bytes(Settings(CustomRoot)), CustomRoot));
    }

    [Fact(DisplayName = "Selecting a custom root declares v4 and retains unknown values and order")]
    public void SetsCustomRoot()
    {
        var bytes = Assert.IsType<byte[]>(WorkspaceSettingsCodec.SetMainRoot(Bytes(new()
        {
            [First] = new JsonArray(1, null),
            [WorkspaceSettingsDefinitions.SchemaVersionProperty] = ExistingVersion,
            [Last] = new JsonObject { [Keep] = true },
        }), CustomRoot));
        using var json = JsonDocument.Parse(bytes);
        Assert.Equal([First, WorkspaceSettingsDefinitions.SchemaVersionProperty, Last, WorkspaceSettingsDefinitions.MainRootProperty, WorkspaceSettingsDefinitions.SchemaProperty], json.RootElement.EnumerateObject().Select(value => value.Name));
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaVersion, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaVersionProperty).GetInt32());
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaUrl, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaProperty).GetString());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty(First)[1].ValueKind);
        Assert.True(json.RootElement.GetProperty(Last).GetProperty(Keep).GetBoolean());
        Assert.Equal(CustomRoot, WorkspaceSettingsCodec.Read(bytes).Document?.MainRoot);
        var reset = Assert.IsType<byte[]>(WorkspaceSettingsCodec.SetMainRoot(bytes, WorkspaceLayout.DefaultMainRoot));
        using var resetJson = JsonDocument.Parse(reset);
        Assert.False(resetJson.RootElement.TryGetProperty(WorkspaceSettingsDefinitions.MainRootProperty, out _));
    }

    [Fact(DisplayName = "New custom settings declare v4 and unrelated edits retain old declarations")]
    public void PreservesExistingDeclarations()
    {
        var created = Assert.IsType<byte[]>(WorkspaceSettingsCodec.SetMainRoot(ReadOnlyMemory<byte>.Empty, OtherRoot));
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaVersion, WorkspaceSettingsCodec.Read(created).Document?.SchemaVersion);
        var original = Settings(OtherRoot);
        original[WorkspaceSettingsDefinitions.SchemaProperty] = ExistingSchema;
        original[WorkspaceSettingsDefinitions.SchemaVersionProperty] = ExistingVersion;
        original[Unknown] = false;
        var bytes = Assert.IsType<byte[]>(WorkspaceSettingsCodec.AddRoot(Bytes(original), Note));
        using var json = JsonDocument.Parse(bytes);
        Assert.Equal(ExistingSchema, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaProperty).GetString());
        Assert.Equal(ExistingVersion, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaVersionProperty).GetInt32());
        Assert.False(json.RootElement.GetProperty(Unknown).GetBoolean());
        Assert.Equal(OtherRoot, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.MainRootProperty).GetString());
    }

    public static TheoryData<string> ContentRegistrations => new()
    {
        CustomRoot + "/" + Note, CustomRoot.ToUpperInvariant() + "/" + Note, CustomRoot.Split('/')[0],
        WorkspaceLayout.NativeSkillsPath + "/" + Note, WorkspaceLayout.NativeSkillsPath.ToUpperInvariant() + "/" + Note,
        WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.SettingsFileName,
        WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.OwnershipFileName,
    };

    [Theory(DisplayName = "Registrations reject either content location and fixed controls")]
    [MemberData(nameof(ContentRegistrations))]
    public void RejectsContentRegistrations(string path)
    {
        var existing = Bytes(Settings(CustomRoot));
        Assert.False(WorkspaceSettingsCodec.IsValidRootPath(path, CustomRoot));
        Assert.Throws<ArgumentException>(() => WorkspaceSettingsCodec.AddRoot(existing, path));
        Assert.Throws<ArgumentException>(() => WorkspaceSettingsCodec.RemoveRoot(existing, path));
        var invalid = Settings(CustomRoot);
        invalid[WorkspaceSettingsDefinitions.RootsProperty] = new JsonArray(path);
        Assert.Null(WorkspaceSettingsCodec.Read(Bytes(invalid)).Document);
    }

    [Fact(DisplayName = "Registration validation uses settings bytes and root changes cannot absorb registered files")]
    public void UsesSelectedLayout()
    {
        var outside = WorkspaceLayout.DefaultMainRoot + "/" + Note;
        var bytes = Assert.IsType<byte[]>(WorkspaceSettingsCodec.AddRoot(Bytes(Settings(OtherRoot)), outside));
        Assert.Equal([outside], WorkspaceSettingsCodec.Read(bytes).Document?.Roots);
        Assert.Throws<ArgumentException>(() => WorkspaceSettingsCodec.SetMainRoot(bytes, WorkspaceLayout.DefaultMainRoot));
        Assert.NotNull(WorkspaceSettingsCodec.RemoveRoot(bytes, outside));
        var destination = OtherRoot + WorkspaceSettingsDefinitions.MemoryArchivePath[WorkspaceLayout.DefaultMainRoot.Length..];
        var archive = Assert.IsType<byte[]>(WorkspaceSettingsCodec.SeedArchiveLinkScope(bytes, destination));
        Assert.Equal([destination], WorkspaceSettingsCodec.Read(archive).Document?.IgnoreLinksFrom);
    }

    [Fact(DisplayName = "Registered source membership and install grants do not alter primary layout mappings")]
    public void PrimaryLayoutDoesNotChooseRegisteredRoots()
    {
        var outside = OtherRoot + "/" + Note;
        var settings = Settings(CustomRoot);
        settings[WorkspaceSettingsDefinitions.RootsProperty] = new JsonArray(outside);
        settings[WorkspaceSettingsDefinitions.AllowInstallPathsProperty] = new JsonArray(OtherRoot);
        var document = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(Bytes(settings)).Document);
        var workspace = new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory);
        var layout = WorkspaceLayout.Create(workspace, document.MainRoot);

        Assert.Contains(outside, document.Roots);
        Assert.True(WorkspaceAllowList.Admits(document.AllowInstallPaths, outside));
        Assert.DoesNotContain(OtherRoot, layout.SourceRoots);
        Assert.False(layout.IsImplicitContentDestination(outside));
        Assert.Equal(CustomRoot + "/" + OtherRoot, layout.CategoryDirectory(OtherRoot));
        Assert.Equal(CustomRoot + "/" + outside, layout.SourcePath(outside));
        Assert.Equal(CustomRoot + "/" + outside, layout.MapPayloadPath(WorkspaceLayout.DefaultMainRoot + "/" + outside));
        Assert.Equal(outside, layout.MapPayloadPath(outside));
    }

    private static JsonObject Settings(string root) => new() { [WorkspaceSettingsDefinitions.MainRootProperty] = root };
    private static byte[] Bytes(JsonObject document) => Encoding.UTF8.GetBytes(document.ToJsonString());
}
