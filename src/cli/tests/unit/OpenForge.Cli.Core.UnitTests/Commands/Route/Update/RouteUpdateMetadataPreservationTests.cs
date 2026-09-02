using System.Text;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Update;

public sealed class RouteUpdateMetadataPreservationTests
{
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

    [Fact(DisplayName = "Route Update metadata patcher preserves mixed line endings outside the edit"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void PreservesMixedLineEndingsOutsideEdit()
    {
        const string source = "---\r\nopen-forge:\n  description: Before\r  tags: [Memory]\r\n  custom: exact\n---\r\n\r\n# Body\n";
        const string expected = "---\r\nopen-forge:\n  description: After\r  tags: [Memory]\r\n  custom: exact\n---\r\n\r\n# Body\n";

        AssertExactPatch(source, expected);
    }

    [Fact(DisplayName = "Route Update metadata patcher converts Unicode offsets before a quoted scalar"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void ConvertsUnicodeOffsetsBeforeQuotedScalar()
    {
        const string source = "---\nopen-forge:\n  custom: \"café 🙂\"\n  description: \"Before\"\n  tags: [Memory]\n---\n\n# Body\n";
        const string expected = "---\nopen-forge:\n  custom: \"café 🙂\"\n  description: After\n  tags: [Memory]\n---\n\n# Body\n";

        AssertExactPatch(source, expected);
    }

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
            new RouteUpdateMetadataPatchInput
            {
                Observation = RouteUpdateTestData.Observation(
                    RouteUpdateTestData.DescriptionPatch("After"),
                    source),
            });
        var patch = Assert.IsType<RouteUpdateMetadataPatch>(build.Patch);

        Assert.Null(build.Boundary);
        Assert.Equal(
            Encoding.UTF8.GetBytes(expected),
            patch.IntendedTargetBytes);
    }
}
