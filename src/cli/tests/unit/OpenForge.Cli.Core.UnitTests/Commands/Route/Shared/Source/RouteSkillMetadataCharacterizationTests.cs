using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Source;

public sealed class RouteSkillMetadataCharacterizationTests
{
    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Route native Skill metadata reads name and description and ignores every other member")]
    [InlineData("---\nname: &identity native-skill\ndescription: *identity\n---\n", nameof(RouteSourceMetadataState.Complete), "native-skill")]
    // A Skill keeps the metadata its own runtime requires. `license`, `allowed-tools` and
    // anything else are read past, not rejected: rejecting them blocked the whole workspace.
    [InlineData("---\nname: native-skill\ndescription: Native description.\nunmatched: value\n---\n", nameof(RouteSourceMetadataState.Complete), "Native description.")]
    [InlineData("---\nname: native-skill\ndescription: Native description.\nlicense: Apache-2.0\nallowed-tools: Read, Bash\n---\n", nameof(RouteSourceMetadataState.Complete), "Native description.")]
    [InlineData("---\nname: native-skill\ndescription: Native description.\nmetadata:\n  nested: true\n---\n", nameof(RouteSourceMetadataState.Complete), "Native description.")]
    [InlineData("---\nname: [native-skill]\ndescription: Native description.\n---\n", nameof(RouteSourceMetadataState.Malformed), null)]
    [InlineData("---\nname: native-skill\ndescription:\n  value: Native description.\n---\n", nameof(RouteSourceMetadataState.Malformed), null)]
    [InlineData("---\nnull\n---\n", nameof(RouteSourceMetadataState.Missing), null)]
    [InlineData("---\n~\n---\n", nameof(RouteSourceMetadataState.Missing), null)]
    [InlineData("---\nname: null\ndescription: Native description.\n---\n", nameof(RouteSourceMetadataState.Missing), null)]
    [InlineData("---\nname: native-skill\ndescription: ~\n---\n", nameof(RouteSourceMetadataState.Missing), null)]
    [InlineData("---\nname: \"null\"\ndescription: \"null\"\n---\n", nameof(RouteSourceMetadataState.Complete), "null")]
    [Trait("Feature", "route-metadata"), Trait("Evidence", "Unit")]
    public void CurrentNativeSkillGrammarIsCharacterized(
        string source,
        string expectedState,
        string? expectedDescription)
    {
        var document = new MarkdownDocumentParser().Parse(source);
        var facts = new SourceAuthoredMetadataParser().Parse(document, SourceDocumentForm.Skill);
        var metadata = new RouteMetadataParser().Parse(
            facts,
            isCompatibilityEntrypoint: false,
            isOverwritePresent: false);

        Assert.Equal(Enum.Parse<RouteSourceMetadataState>(expectedState), metadata.State);
        Assert.Equal(expectedDescription, metadata.Description);
        Assert.Empty(metadata.Tags);
    }
}
