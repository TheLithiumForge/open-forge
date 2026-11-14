using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Ownership.Shared.Serialization;

public sealed class WorkspaceOwnershipPreservationTests
{
    [Theory(DisplayName = "Ownership writes clear every omitted optional value while keeping unknown siblings"),
     InlineData(0), InlineData(1), InlineData(2), InlineData(3), InlineData(4),
     Trait("Feature", "workspace-ownership"), Trait("Evidence", "Unit")]
    public void ClearsKnownOptionalValues(int shape)
    {
        var root = Root(Seed());
        var target = OptionalTarget(root, shape);
        target["future"] = new JsonObject { ["sentinel"] = 31 };
        var document = Read(root);
        var framework = Assert.IsType<FrameworkOwnership>(document.Framework);
        var extension = Assert.Single(document.Extensions);
        var library = Assert.Single(document.Libraries);
        document = shape switch
        {
            0 => document with { Libraries = [library with { GitIgnore = false }] },
            1 => document with { Framework = framework with { GitIgnoredRoutes = [] } },
            2 => document with { Extensions = [extension with { Version = null }] },
            3 => document with { Extensions = [extension with { Source = null }] },
            4 => document with { Framework = framework with { Source = framework.Source with { Version = null } } },
            _ => throw new ArgumentOutOfRangeException(nameof(shape)),
        };
        var after = Root(document);
        var written = OptionalTarget(after, shape);
        var omitted = shape switch
        {
            0 => Property(nameof(LibraryOwnershipEntry.GitIgnore)),
            1 => Property(nameof(FrameworkOwnershipEntry.GitIgnoredRoutes)),
            2 or 4 => Property(nameof(OwnedSourceEntry.Version)),
            3 => Property(nameof(ExtensionOwnershipEntry.Source)),
            _ => throw new ArgumentOutOfRangeException(nameof(shape)),
        };
        Assert.False(written.ContainsKey(omitted));
        Assert.True(JsonNode.DeepEquals(target["future"], written["future"]));
        var reread = Read(after);
        if (shape == 0) Assert.False(Assert.Single(reread.Libraries).GitIgnore);
        if (shape == 1) Assert.Empty(Assert.IsType<FrameworkOwnership>(reread.Framework).GitIgnoredRoutes);
        if (shape == 2) Assert.Null(Assert.Single(reread.Extensions).Version);
        if (shape == 3) Assert.Null(Assert.Single(reread.Extensions).Source);
        if (shape == 4) Assert.Null(Assert.IsType<FrameworkOwnership>(reread.Framework).Source.Version);
    }

    [Theory(DisplayName = "Owned regions keep unknown fields by path and region through reorder and replacement"), InlineData(false), InlineData(true),
     Trait("Feature", "workspace-ownership"), Trait("Evidence", "Unit")]
    public void RegionIdentity(bool extension)
    {
        ImmutableArray<OwnedRegion> regions = [new("same.md", "first"), new("same.md", "second"), new("other.md", "first")];
        var document = Seed();
        document = SetRegions(document, extension, regions);
        var root = Root(document);
        var owner = Owner(root, extension);
        var key = Property(nameof(FrameworkOwnershipEntry.Regions));
        var original = Assert.IsType<JsonArray>(owner[key]);
        foreach (var row in original.OfType<JsonObject>()) row["future"] = row.ToJsonString();
        owner[key] = new JsonArray(original.Reverse().Select(row => row?.DeepClone()).ToArray());
        document = SetRegions(Read(root), extension, [regions[2], regions[0], new("same.md", "new")]);
        var after = Assert.IsType<JsonArray>(Owner(Root(document), extension)[key]);
        Assert.Equal(3, after.Count);
        foreach (var region in new[] { regions[0], regions[2] })
        {
            var beforeRow = FindRegion(original, region);
            Assert.True(JsonNode.DeepEquals(beforeRow["future"], FindRegion(after, region)["future"]));
        }
        Assert.False(FindRegion(after, new("same.md", "new")).ContainsKey("future"));
        Assert.DoesNotContain(after.OfType<JsonObject>(), row => row[Property(nameof(OwnedRegionEntry.Region))]?.GetValue<string>() == regions[1].Region);
    }

    [Fact(DisplayName = "Git-ignored routes keep unknown fields by directory and entrypoint through reorder and replacement"),
     Trait("Feature", "workspace-ownership"), Trait("Evidence", "Unit")]
    public void RouteIdentity()
    {
        var document = Seed();
        var framework = Assert.IsType<FrameworkOwnership>(document.Framework) with
        {
            GitIgnoredRoutes = [new("notes", "notes/index.md"), new("team", "team/index.md"), new("private", "private/index.md")],
        };
        var root = Root(document with { Framework = framework });
        var owner = Owner(root, extension: false);
        var key = Property(nameof(FrameworkOwnershipEntry.GitIgnoredRoutes));
        var original = Assert.IsType<JsonArray>(owner[key]);
        foreach (var row in original.OfType<JsonObject>()) row["future"] = row.ToJsonString();
        owner[key] = new JsonArray(original.Reverse().Select(row => row?.DeepClone()).ToArray());
        document = Read(root);
        framework = Assert.IsType<FrameworkOwnership>(document.Framework) with
        {
            GitIgnoredRoutes = [new("team", "team/index.md"), new("notes", "notes/index.md"), new("private", "private/_private.md")],
        };
        var after = Assert.IsType<JsonArray>(Owner(Root(document with { Framework = framework }), extension: false)[key]);
        Assert.Equal(3, after.Count);
        var entrypoint = Property(nameof(GitIgnoredRouteEntry.Entrypoint));
        foreach (var path in new[] { "notes/index.md", "team/index.md" })
        {
            var before = original.OfType<JsonObject>().Single(row => row[entrypoint]?.GetValue<string>() == path);
            var written = after.OfType<JsonObject>().Single(row => row[entrypoint]?.GetValue<string>() == path);
            Assert.True(JsonNode.DeepEquals(before["future"], written["future"]));
        }
        Assert.False(after.OfType<JsonObject>().Single(row => row[entrypoint]?.GetValue<string>() == "private/_private.md").ContainsKey("future"));
    }

    private static WorkspaceOwnershipDocument Seed() => new(WorkspaceOwnershipDefinitions.SchemaVersion,
        new(new("synthetic-core", "1.0.0"), [], []) { GitIgnoredRoutes = [new("notes", "notes/index.md")] },
        [new("synthetic-extension", "1.0.0", "synthetic-source", [], [], [])],
        [new("synthetic-library", "source", "destination", [], GitIgnore: true)]);

    private static WorkspaceOwnershipDocument SetRegions(WorkspaceOwnershipDocument document, bool extension, ImmutableArray<OwnedRegion> regions)
    {
        if (extension) return document with { Extensions = [Assert.Single(document.Extensions) with { Regions = regions }] };
        return document with { Framework = Assert.IsType<FrameworkOwnership>(document.Framework) with { Regions = regions } };
    }

    private static JsonObject FindRegion(JsonArray rows, OwnedRegion region) => rows.OfType<JsonObject>().Single(row =>
        row[Property(nameof(OwnedRegionEntry.Path))]?.GetValue<string>() == region.Path && row[Property(nameof(OwnedRegionEntry.Region))]?.GetValue<string>() == region.Region);

    private static JsonObject OptionalTarget(JsonObject root, int shape) => shape switch
    {
        0 => Assert.IsType<JsonObject>(root[WorkspaceOwnershipDefinitions.LibrariesProperty]?[0]),
        1 => Owner(root, extension: false),
        2 or 3 => Owner(root, extension: true),
        4 => Assert.IsType<JsonObject>(Owner(root, extension: false)[Property(nameof(FrameworkOwnershipEntry.Source))]),
        _ => throw new ArgumentOutOfRangeException(nameof(shape)),
    };

    private static JsonObject Owner(JsonObject root, bool extension)
    {
        if (extension) return Assert.IsType<JsonObject>(root[WorkspaceOwnershipDefinitions.ExtensionsProperty]?[0]);
        return Assert.IsType<JsonObject>(root[WorkspaceOwnershipDefinitions.FrameworkProperty]);
    }

    private static WorkspaceOwnershipDocument Read(JsonObject root)
        => Assert.IsType<WorkspaceOwnershipDocument>(WorkspaceOwnershipCodec.Read(System.Text.Encoding.UTF8.GetBytes(root.ToJsonString())).Document);
    private static JsonObject Root(WorkspaceOwnershipDocument document) => Assert.IsType<JsonObject>(JsonNode.Parse(WorkspaceOwnershipCodec.Write(document)));
    private static string Property(string name) => JsonNamingPolicy.CamelCase.ConvertName(name);
}
