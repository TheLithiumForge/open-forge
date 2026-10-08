using System.Text;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Update;

public sealed class RouteUpdateMetadataPreservationTests
{
    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Route Update replaces applyTo declarations in their authored locations and preserves comments"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    [InlineData(
        "---\napplyTo: [\"old/*.cs\"] # root comment\nopen-forge:\n  description: Before\n  tags: [Memory]\n---\n\n# Body\n",
        "---\napplyTo: [\"**/*.cs\", \"docs/*.md\"] # root comment\nopen-forge:\n  description: Before\n  tags: [Memory]\n---\n\n# Body\n")]
    [InlineData(
        "---\nopen-forge:\n  description: Before\n  tags: [Memory]\n  applyTo: [\"old/*.cs\"] # scoped comment\n---\n\n# Body\n",
        "---\nopen-forge:\n  description: Before\n  tags: [Memory]\n  applyTo: [\"**/*.cs\", \"docs/*.md\"] # scoped comment\n---\n\n# Body\n")]
    public void ReplacesApplyToAtAuthoredLocationAndPreservesInlineComments(
        string source,
        string expected)
    {
        var build = RouteUpdateTestData.MetadataPatcher().Build(
            RouteUpdateTestData.Observation(
                RouteUpdateTestData.ApplyToPatch("**/*.cs", "docs/*.md"),
                source));
        Assert.True(
            build.Patch is not null,
            build.Boundary?.Formation.Findings[0].Cause);
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), patch.IntendedTargetBytes);
        Assert.Equal(RouteUpdatePatchState.Changed, patch.Patch.ApplyTo.State);
        Assert.Equal(["old/*.cs"], patch.Patch.ApplyTo.Before);
        Assert.Equal(["**/*.cs", "docs/*.md"], patch.Patch.ApplyTo.Expected);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update replaces a block-style applyTo list without changing its field placement")]
    [Trait("Feature", "route-update")]
    [Trait("Evidence", "UnitBehavior")]
    public void ReplacesBlockStyleApplyToList()
    {
        const string source = "---\napplyTo:\n  - \"old/*.cs\"\n  - \"old/*.md\"\nopen-forge:\n  description: Before\n  tags: [Memory]\n---\n\n# Body\n";
        const string expected = "---\napplyTo:\n  [\"**/*.cs\", \"docs/*.md\"]\nopen-forge:\n  description: Before\n  tags: [Memory]\n---\n\n# Body\n";

        var build = RouteUpdateTestData.MetadataPatcher().Build(
            RouteUpdateTestData.Observation(
                RouteUpdateTestData.ApplyToPatch("**/*.cs", "docs/*.md"),
                source));
        Assert.True(build.Patch is not null, build.Boundary?.Formation.Findings[0].Cause);
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), patch.IntendedTargetBytes);
        Assert.Equal(RouteUpdatePatchState.Changed, patch.Patch.ApplyTo.State);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update updates equivalent root and scoped applyTo declarations together")]
    [Trait("Feature", "route-update")]
    [Trait("Evidence", "UnitBehavior")]
    public void UpdatesEquivalentDualApplyToDeclarationsTogether()
    {
        const string source = "---\napplyTo: [\"old/*.cs\", \"old/*.md\"]\nopen-forge:\n  description: Before\n  tags: [Memory]\n  applyTo: [\"old/*.md\", \"old/*.cs\"] # scoped comment\n---\n\n# Body\n";
        const string expected = "---\napplyTo: [\"**/*.cs\", \"docs/*.md\"]\nopen-forge:\n  description: Before\n  tags: [Memory]\n  applyTo: [\"**/*.cs\", \"docs/*.md\"] # scoped comment\n---\n\n# Body\n";

        var build = RouteUpdateTestData.MetadataPatcher().Build(
            RouteUpdateTestData.Observation(
                RouteUpdateTestData.ApplyToPatch("**/*.cs", "docs/*.md"),
                source));
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), patch.IntendedTargetBytes);
        Assert.Equal(RouteUpdatePatchState.Changed, patch.Patch.ApplyTo.State);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update inserts a missing applyTo list using canonical quoted YAML")]
    [Trait("Feature", "route-update")]
    [Trait("Evidence", "UnitBehavior")]
    public void InsertsMissingApplyToAsCanonicalQuotedList()
    {
        const string source = "---\nopen-forge:\n  description: Before\n  tags: [Memory]\n---\n\n# Body\n";
        const string expected = "---\nopen-forge:\n  description: Before\n  tags: [Memory]\n  applyTo: [\"**/*.cs\", \"docs/*.md\"]\n---\n\n# Body\n";

        var build = RouteUpdateTestData.MetadataPatcher().Build(
            RouteUpdateTestData.Observation(
                RouteUpdateTestData.ApplyToPatch("**/*.cs", "docs/*.md"),
                source));
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), patch.IntendedTargetBytes);
        Assert.Equal(RouteUpdatePatchState.Changed, patch.Patch.ApplyTo.State);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Route Update clears every applyTo declaration and preserves inline comments")]
    [InlineData(
        "---\napplyTo: [\"**/*.cs\"] # root comment\nopen-forge:\n  description: Before\n  tags: [Memory]\n  applyTo: [\"**/*.cs\"] # scoped comment\n---\n\n# Body\n",
        "---\n# root comment\nopen-forge:\n  description: Before\n  tags: [Memory]\n  # scoped comment\n---\n\n# Body\n")]
    [InlineData(
        "---\nopen-forge:\n  description: Before\n  tags: [Memory]\n  applyTo: [\"**/*.cs\"]\n---\n\n# Body\n",
        "---\nopen-forge:\n  description: Before\n  tags: [Memory]\n---\n\n# Body\n")]
    public void ClearsApplyToWithoutRemovingCommentsOrOtherMetadata(string source, string expected)
    {
        var build = RouteUpdateTestData.MetadataPatcher().Build(
            RouteUpdateTestData.Observation(
                RouteUpdateTestData.ClearApplyToPatch(),
                source));
        Assert.True(build.Patch is not null, build.Boundary?.Formation.Findings[0].Cause);
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), patch.IntendedTargetBytes);
        Assert.Equal(RouteUpdatePatchState.Changed, patch.Patch.ApplyTo.State);
        Assert.Equal(["**/*.cs"], patch.Patch.ApplyTo.Before);
        Assert.Null(patch.Patch.ApplyTo.Expected);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update clears a block-style applyTo list")]
    [Trait("Feature", "route-update")]
    [Trait("Evidence", "UnitBehavior")]
    public void ClearsBlockStyleApplyToList()
    {
        const string source = "---\nopen-forge:\n  description: Before\n  tags: [Memory]\n  applyTo:\n    - \"old/*.cs\"\n    - \"old/*.md\"\n---\n\n# Body\n";
        const string expected = "---\nopen-forge:\n  description: Before\n  tags: [Memory]\n---\n\n# Body\n";

        var build = RouteUpdateTestData.MetadataPatcher().Build(
            RouteUpdateTestData.Observation(
                RouteUpdateTestData.ClearApplyToPatch(),
                source));
        Assert.True(build.Patch is not null, build.Boundary?.Formation.Findings[0].Cause);
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), patch.IntendedTargetBytes);
        Assert.Equal(RouteUpdatePatchState.Changed, patch.Patch.ApplyTo.State);
        Assert.Equal(["old/*.cs", "old/*.md"], patch.Patch.ApplyTo.Before);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update preserves trailing comments when replacing a block-style applyTo list")]
    [Trait("Feature", "route-update")]
    [Trait("Evidence", "UnitBehavior")]
    public void ReplacesBlockStyleApplyToListAndPreservesTrailingComment()
    {
        const string source = "---\nopen-forge:\n  description: Before\n  tags: [Memory]\n  applyTo:\n    - \"old/*.cs\"\n    - \"old/*.md\" # keep this note\n---\n\n# Body\n";
        const string expected = "---\nopen-forge:\n  description: Before\n  tags: [Memory]\n  applyTo:\n    [\"**/*.cs\", \"docs/*.md\"] # keep this note\n---\n\n# Body\n";

        var build = RouteUpdateTestData.MetadataPatcher().Build(
            RouteUpdateTestData.Observation(
                RouteUpdateTestData.ApplyToPatch("**/*.cs", "docs/*.md"),
                source));
        Assert.True(build.Patch is not null, build.Boundary?.Formation.Findings[0].Cause);
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), patch.IntendedTargetBytes);
        Assert.Equal(RouteUpdatePatchState.Changed, patch.Patch.ApplyTo.State);

        var clear = RouteUpdateTestData.MetadataPatcher().Build(
            RouteUpdateTestData.Observation(
                RouteUpdateTestData.ClearApplyToPatch(),
                source));
        Assert.Null(clear.Patch);
        var boundary = Assert.IsType<RouteUpdatePlanningBoundary>(clear.Boundary);
        Assert.Contains(
            boundary.Formation.Findings,
            finding => finding.Code == RouteUpdateFindingCode.MetadataPreservationUnsafe);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update blocks applyTo edits when an interior sequence comment cannot be retained")]
    [Trait("Feature", "route-update")]
    [Trait("Evidence", "UnitBehavior")]
    public void BlocksApplyToEditsWithInteriorSequenceComments()
    {
        var sources = new[]
        {
            "---\nopen-forge:\n  description: Before\n  tags: [Memory]\n  applyTo:\n    - \"old/*.cs\"\n    # preserve the reason for this pattern\n    - \"old/*.md\"\n---\n\n# Body\n",
            "---\nopen-forge:\n  description: Before\n  tags: [Memory]\n  applyTo: [\"old/*.cs\", # preserve the reason for this pattern\n    \"old/*.md\"]\n---\n\n# Body\n",
        };
        var requests = new[]
        {
            RouteUpdateTestData.ApplyToPatch("**/*.cs"),
            RouteUpdateTestData.ClearApplyToPatch(),
        };

        foreach (var source in sources)
        {
            foreach (var request in requests)
            {
                var observation = RouteUpdateTestData.Observation(request, source);
                Assert.Equal(
                    OpenForge.Cli.Core.Framework.Documents.Yaml.Models.YamlDocumentState.Complete,
                    observation.Frontmatter?.State);
                var build = RouteUpdateTestData.MetadataPatcher().Build(
                    observation);

                Assert.Null(build.Patch);
                var boundary = Assert.IsType<RouteUpdatePlanningBoundary>(build.Boundary);
                Assert.Contains(
                    boundary.Formation.Findings,
                    finding => finding.Code == RouteUpdateFindingCode.MetadataPreservationUnsafe);
            }
        }
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update clearing absent applyTo is a byte-exact no-op")]
    [Trait("Feature", "route-update")]
    [Trait("Evidence", "UnitBehavior")]
    public void ClearingAbsentApplyToIsNoOp()
    {
        const string source = "---\nopen-forge:\n  description: Before\n  tags: [Memory]\n---\n\n# Body\n";
        var build = RouteUpdateTestData.MetadataPatcher().Build(
            RouteUpdateTestData.Observation(
                RouteUpdateTestData.ClearApplyToPatch(),
                source));
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(Encoding.UTF8.GetBytes(source), patch.IntendedTargetBytes);
        Assert.Equal(RouteUpdatePatchState.Unchanged, patch.Patch.ApplyTo.State);
        Assert.Null(patch.Patch.ApplyTo.Before);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Route Update expands an empty scoped mapping without relocating root applyTo")]
    [InlineData(false)]
    [InlineData(true)]
    public void ExpandsEmptyMappingAndPreservesRootApplyTo(bool setNewPatterns)
    {
        const string source = "---\napplyTo: [\"old/*.cs\"] # root comment\nopen-forge: {}\n---\n\n# Body\n";
        var expected = setNewPatterns
            ? "---\napplyTo: [\"**/*.cs\", \"docs/*.md\"] # root comment\nopen-forge:\n  description: After\n  tags: [Memory]\n---\n\n# Body\n"
            : "---\napplyTo: [\"old/*.cs\"] # root comment\nopen-forge:\n  description: After\n  tags: [Memory]\n---\n\n# Body\n";
        var patchRequest = RouteUpdateTestData.Patch(
            description: new RouteUpdateDescriptionRequest
            {
                Requested = true,
                Value = "After",
            },
            tags: new RouteUpdateTagsRequest
            {
                Requested = true,
                Values = ["Memory"],
            },
            applyTo: setNewPatterns
                ? new RouteUpdateApplyToRequest
                {
                    Operation = RouteUpdateApplyToOperation.Set,
                    Values = ["**/*.cs", "docs/*.md"],
                }
                : null);
        var build = RouteUpdateTestData.MetadataPatcher().Build(
            RouteUpdateTestData.Observation(patchRequest, source));
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), patch.IntendedTargetBytes);
        Assert.Equal(1, Encoding.UTF8.GetString(patch.IntendedTargetBytes.AsSpan())
            .Split("applyTo:", StringSplitOptions.None).Length - 1);
        Assert.Equal(
            setNewPatterns ? RouteUpdatePatchState.Changed : RouteUpdatePatchState.NotRequested,
            patch.Patch.ApplyTo.State);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update clears root applyTo while expanding an empty scoped mapping")]
    [Trait("Feature", "route-update")]
    [Trait("Evidence", "UnitBehavior")]
    public void ClearsRootApplyToWhileExpandingEmptyMapping()
    {
        const string source = "---\napplyTo: [\"old/*.cs\"]\nopen-forge: {}\n---\n\n# Body\n";
        const string expected = "---\nopen-forge:\n  description: After\n  tags: [Memory]\n---\n\n# Body\n";
        var patchRequest = RouteUpdateTestData.Patch(
            description: new RouteUpdateDescriptionRequest
            {
                Requested = true,
                Value = "After",
            },
            tags: new RouteUpdateTagsRequest
            {
                Requested = true,
                Values = ["Memory"],
            },
            applyTo: new RouteUpdateApplyToRequest
            {
                Operation = RouteUpdateApplyToOperation.Clear,
                Values = [],
            });
        var build = RouteUpdateTestData.MetadataPatcher().Build(
            RouteUpdateTestData.Observation(patchRequest, source));
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), patch.IntendedTargetBytes);
        Assert.Equal(RouteUpdatePatchState.Changed, patch.Patch.ApplyTo.State);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Route Update refuses invalid or conflicting applyTo metadata")]
    [InlineData("---\nopen-forge:\n  description: Before\n  tags: [Memory]\n  applyTo: [\"**/*.cs\", plain]\n---\n\n# Body\n")]
    [InlineData("---\napplyTo: [\"**/*.cs\"]\nopen-forge:\n  description: Before\n  tags: [Memory]\n  applyTo: [\"docs/*.md\"]\n---\n\n# Body\n")]
    public void RefusesInvalidOrConflictingApplyToMetadata(string source)
    {
        var build = RouteUpdateTestData.MetadataPatcher().Build(
            RouteUpdateTestData.Observation(
                RouteUpdateTestData.ClearApplyToPatch(),
                source));

        Assert.Null(build.Patch);
        var boundary = Assert.IsType<RouteUpdatePlanningBoundary>(build.Boundary);
        Assert.Contains(
            boundary.Formation.Findings,
            finding => finding.Code == RouteUpdateFindingCode.MetadataPreservationUnsafe);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Route Update metadata patcher expands an empty mapping with exact line endings"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void ExpandsEmptyMappingWithExactLineEndings(string lineEnding)
    {
        var source = string.Join(
            lineEnding,
            "---",
            "title: \"café 🙂\" # preserve this top-level comment",
            "open-forge: {}",
            "# preserve this top-level comment too",
            "---",
            string.Empty,
            "# Body",
            "Keep the authored body.",
            string.Empty);
        var expected = string.Join(
            lineEnding,
            "---",
            "title: \"café 🙂\" # preserve this top-level comment",
            "open-forge:",
            "  description: After",
            "  tags: [Memory]",
            "# preserve this top-level comment too",
            "---",
            string.Empty,
            "# Body",
            "Keep the authored body.",
            string.Empty);

        var build = RouteUpdateTestData.MetadataPatcher().Build(
            RouteUpdateTestData.Observation(
                RouteUpdateTestData.Patch(
                    description: new RouteUpdateDescriptionRequest
                    {
                        Requested = true,
                        Value = "After",
                    },
                    tags: new RouteUpdateTagsRequest
                    {
                        Requested = true,
                        Values = ["Memory"],
                    }),
                source));
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), patch.IntendedTargetBytes);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Route Update metadata patcher preserves the established line ending"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    [InlineData("\r")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void PreservesEstablishedLineEnding(string lineEnding)
    {
        var source = string.Join(
            lineEnding,
            "---",
            "open-forge:",
            "  description: Before",
            "  tags: [Memory]",
            "---",
            string.Empty,
            "# Body",
            string.Empty);
        var expected = source.Replace(
            "description: Before",
            "description: After",
            StringComparison.Ordinal);

        AssertExactPatch(source, expected);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update metadata patcher preserves mixed line endings outside the edit"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void PreservesMixedLineEndingsOutsideEdit()
    {
        const string source = "---\r\nopen-forge:\n  description: Before\r  tags: [Memory]\r\n  custom: exact\n---\r\n\r\n# Body\n";
        const string expected = "---\r\nopen-forge:\n  description: After\r  tags: [Memory]\r\n  custom: exact\n---\r\n\r\n# Body\n";

        AssertExactPatch(source, expected);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update metadata patcher converts Unicode offsets before a quoted scalar"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void ConvertsUnicodeOffsetsBeforeQuotedScalar()
    {
        const string source = "---\nopen-forge:\n  custom: \"café 🙂\"\n  description: \"Before\"\n  tags: [Memory]\n---\n\n# Body\n";
        const string expected = "---\nopen-forge:\n  custom: \"café 🙂\"\n  description: After\n  tags: [Memory]\n---\n\n# Body\n";

        AssertExactPatch(source, expected);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update metadata patcher preserves unknown and comment bytes exactly"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void PreservesUnknownAndCommentBytesExactly()
    {
        const string source = "---\nopen-forge:\n  # keep this comment byte-for-byte\n  custom: \"opaque # value\" # keep inline comment\n  description: Before\n  tags: [Memory]\n---\n\n# Body\n";
        const string expected = "---\nopen-forge:\n  # keep this comment byte-for-byte\n  custom: \"opaque # value\" # keep inline comment\n  description: After\n  tags: [Memory]\n---\n\n# Body\n";

        AssertExactPatch(source, expected);
    }

    private static void AssertExactPatch(string source, string expected)
    {
        var build = RouteUpdateTestData.MetadataPatcher().Build(
            RouteUpdateTestData.Observation(
                    RouteUpdateTestData.DescriptionPatch("After"),
                    source));
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(
            Encoding.UTF8.GetBytes(expected),
            patch.IntendedTargetBytes);
    }
}
