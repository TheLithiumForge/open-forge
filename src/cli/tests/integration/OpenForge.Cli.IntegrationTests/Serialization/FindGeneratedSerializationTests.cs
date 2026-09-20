using System.Text.Json;
using OpenForge.Cli.Core.Presentation.Find;
using OpenForge.Cli.Core.Presentation.Find.Models;

namespace OpenForge.Cli.IntegrationTests.Serialization;

[Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
public sealed class FindGeneratedSerializationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Find generated JSON serializes the minimal match shape and omits unselected data")]
    public void MinimalShapeIsStable()
    {
        var json = Serialize(new FindData
        {
            Matches =
            [
                new FindDataMatch
                {
                    Id = "docs",
                    Path = ".agents/docs.md",
                    Description = null,
                },
            ],
        });

        using var document = JsonDocument.Parse(json);
        var match = Assert.Single(document.RootElement.GetProperty("matches").EnumerateArray());
        Assert.Equal(
            ["id", "path", "description"],
            match.EnumerateObject().Select(property => property.Name));
        Assert.Equal(JsonValueKind.Null, match.GetProperty("description").ValueKind);
        Assert.DoesNotContain("contentBlocks", json, StringComparison.Ordinal);
        Assert.DoesNotContain("prepandBlankLine", json, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Find generated JSON preserves standard evidence and query order")]
    public void StandardShapeIsStable()
    {
        var json = Serialize(new FindData
        {
            Matches =
            [
                new FindDataMatch
                {
                    Id = "docs",
                    Path = ".agents/docs.md",
                    Description = "Docs",
                    Evidence =
                    [
                        new FindDataEvidence
                        {
                            Kind = "tag",
                            Value = "Architecture",
                            Region = "frontmatter",
                            Layer = "base",
                        },
                    ],
                },
            ],
            Query = new FindDataQuery
            {
                Tags = ["Architecture"],
                Headings = [],
                Require = "all",
                Within = ["frontmatter"],
            },
        });

        using var document = JsonDocument.Parse(json);
        Assert.Equal(
            ["matches", "query"],
            document.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal("tag", document.RootElement.GetProperty("matches")[0].GetProperty("evidence")[0].GetProperty("kind").GetString());
        Assert.Equal("Architecture", document.RootElement.GetProperty("query").GetProperty("tags")[0].GetString());
        Assert.False(document.RootElement.TryGetProperty("sourceSet", out _));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Find generated JSON emits full source set and content parts")]
    public void FullShapeIsStable()
    {
        var json = Serialize(new FindData
        {
            Matches =
            [
                new FindDataMatch
                {
                    Id = "docs",
                    Path = ".agents/docs.md",
                    Description = "Docs",
                    Parts =
                    [
                        new FindDataPart
                        {
                            Part = "body",
                            Layer = "base",
                            Path = ".agents/docs.md",
                            Text = "# Docs\n",
                        },
                    ],
                },
            ],
            SourceSet = new FindDataSourceSet
            {
                Mode = "default",
                Include = [],
                Exclude = [],
                Inspected = 21,
                Candidates = 21,
            },
        });

        using var document = JsonDocument.Parse(json);
        var sourceSet = document.RootElement.GetProperty("sourceSet");
        Assert.Equal("default", sourceSet.GetProperty("mode").GetString());
        Assert.Equal(21, sourceSet.GetProperty("inspected").GetInt32());
        Assert.Equal(21, sourceSet.GetProperty("candidates").GetInt32());
        var part = document.RootElement.GetProperty("matches")[0].GetProperty("parts")[0];
        Assert.Equal("body", part.GetProperty("part").GetString());
        Assert.Equal("# Docs\n", part.GetProperty("text").GetString());
    }

    private static string Serialize(FindData data)
        => JsonSerializer.Serialize(data, FindPresentation.Rendering.DataJsonTypeInfo);
}
