using System.Text;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Update.Shared.Planning.Metadata;

[Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
public sealed class RouteUpdateRootMetadataTests
{
    [Theory(DisplayName = "Route Update edits each root field in place"), Trait("Boundary", "Processing")]
    [InlineData("description", "Before", "After")]
    [InlineData("responsibility", "Define before", "Define after")]
    [InlineData("tags", "[Memory]", "[Docs, Guide]")]
    public void EditsEachRootFieldInPlace(string field, string before, string after)
    {
        const string source = "---\ndescription: Before\nresponsibility: Define before\ntags: [Memory]\ncustom: exact\n---\n# Body\n";
        var request = field switch
        {
            "description" => RouteUpdateTestData.DescriptionPatch(after),
            "responsibility" => RouteUpdateTestData.Patch(responsibility: new RouteUpdateResponsibilityRequest
            {
                Operation = RouteUpdateResponsibilityOperation.Set,
                Value = after,
            }),
            "tags" => RouteUpdateTestData.Patch(tags: new RouteUpdateTagsRequest { Requested = true, Values = ["Docs", "Guide"] }),
            _ => throw new ArgumentOutOfRangeException(nameof(field)),
        };
        AssertExact(source, source.Replace($"{field}: {before}", $"{field}: {after}", StringComparison.Ordinal), request);
    }

    [Fact(DisplayName = "Route Update preserves quoted root keys scalar styles separators and comments"), Trait("Boundary", "Processing")]
    public void PreservesQuotedKeysScalarStylesAndComments()
    {
        const string source = "---\n'description':   'Before' # reason\n\"responsibility\": \"Define before\" # scope\ntags: [Memory]\n---\n# Body\n";
        var expected = source.Replace("'Before'", "'After''s choice'", StringComparison.Ordinal)
            .Replace("\"Define before\"", "\"Define after\"", StringComparison.Ordinal);
        AssertExact(source, expected, RouteUpdateTestData.Patch(
            description: new RouteUpdateDescriptionRequest { Requested = true, Value = "After's choice" },
            responsibility: new RouteUpdateResponsibilityRequest { Operation = RouteUpdateResponsibilityOperation.Set, Value = "Define after" }));
    }

    [Theory(DisplayName = "Route Update adds missing root fields at stable anchors"), Trait("Boundary", "Processing")]
    [InlineData("tags: [Memory]\n", "description: After\nresponsibility: Define after\ntags: [Memory]\n")]
    [InlineData("description: Before\n", "description: After\nresponsibility: Define after\ntags: [Memory]\n")]
    [InlineData("responsibility: Before\n", "description: After\nresponsibility: Define after\ntags: [Memory]\n")]
    public void AddsMissingFieldsAtStableAnchors(string members, string expected)
        => AssertExact($"---\n{members}custom: kept\n---\n# Body\n", $"---\n{expected}custom: kept\n---\n# Body\n",
            RouteUpdateTestData.Patch(
                description: new RouteUpdateDescriptionRequest { Requested = true, Value = "After" },
                tags: new RouteUpdateTagsRequest { Requested = true, Values = ["Memory"] },
                responsibility: new RouteUpdateResponsibilityRequest { Operation = RouteUpdateResponsibilityOperation.Set, Value = "Define after" }));

    [Fact(DisplayName = "Route Update preserves BOM Unicode and mixed endings outside root edits"), Trait("Boundary", "Processing")]
    public void PreservesBomUnicodeAndMixedEndings()
    {
        const string source = "\uFEFF---\r\ncustom: café 🙂\ndescription: Before\rtags: [Memory]\r\n---\r\n# Body 🙂\n";
        AssertExact(source, source.Replace("Before", "After", StringComparison.Ordinal), RouteUpdateTestData.DescriptionPatch("After"));
    }

    [Fact(DisplayName = "Route Update semantic root no-op retains every byte"), Trait("Boundary", "Processing")]
    public void SemanticNoOpPreservesBytes()
    {
        const string source = "---\n'description': 'Before' # retained\ntags: [Memory]\n---\n# Body\n";
        AssertExact(source, source, RouteUpdateTestData.DescriptionPatch("Before"));
    }

    [Fact(DisplayName = "Scoped metadata owns its fields beside duplicate foreign root members"), Trait("Boundary", "Processing")]
    public void ScopedMappingPreservesForeignRootMembers()
    {
        const string source = "---\ndescription: foreign\ndescription: foreign again\ntags: [Other]\nopen-forge:\n  description: Before\n  tags: [Memory]\n---\n# Body\n";
        AssertExact(source, source.Replace("  description: Before", "  description: After", StringComparison.Ordinal), RouteUpdateTestData.DescriptionPatch("After"));
    }

    [Theory(DisplayName = "Route Update rejects unsafe selected metadata layouts before effects"), Trait("Boundary", "Processing")]
    [InlineData("description: Before\ndescription: duplicate\ntags: [Memory]")]
    [InlineData("open-forge: null\ndescription: foreign\ntags: [Other]")]
    [InlineData("open-forge: []\ndescription: foreign\ntags: [Other]")]
    [InlineData("{description: Before, tags: [Memory]}")]
    public void UnsupportedLayoutsBlockBeforeEffects(string yaml)
    {
        var build = RouteUpdateTestData.MetadataPatcher().Build(RouteUpdateTestData.Observation(
            RouteUpdateTestData.DescriptionPatch("After"), $"---\n{yaml}\n---\n# Body\n"));
        Assert.Null(build.Patch);
        Assert.NotNull(build.Boundary);
    }

    private static void AssertExact(string source, string expected, RouteUpdatePatchRequest request)
    {
        var build = RouteUpdateTestData.MetadataPatcher().Build(RouteUpdateTestData.Observation(request, source));
        Assert.True(build.Patch is not null, build.Boundary?.Formation.Findings[0].Cause);
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), patch.IntendedTargetBytes);
    }
}
