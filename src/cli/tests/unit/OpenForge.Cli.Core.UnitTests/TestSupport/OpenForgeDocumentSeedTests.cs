using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.Core.UnitTests.TestSupport;

public sealed class OpenForgeDocumentSeedTests
{
    [Fact(DisplayName = "Routed Skill entrypoint seed orders names and emits canonical Skill entries")]
    [Trait("Feature", "test-support"), Trait("Evidence", "Unit")]
    public void SkillEntrypointOrdersNamesAndUsesCanonicalEntries()
    {
        var document = OpenForgeDocumentSeed.SkillEntrypoint(
            ["experience-design", "accessibility"]);

        Assert.Equal(
            """
            ---
            open-forge:
              description: Skills
              tags: [Skill]
            ---

            # Skills

            ## Entries

            <!-- open-forge:generated-index:start -->
            - [accessibility](accessibility/SKILL.md) - #Skill
            - [experience-design](experience-design/SKILL.md) - #Skill
            <!-- open-forge:generated-index:end -->

            """,
            document);
    }

    [Theory(DisplayName = "Routed Skill entrypoint seed rejects empty duplicate or blank names")]
    [Trait("Feature", "test-support"), Trait("Evidence", "Unit")]
    [InlineData("empty")]
    [InlineData("duplicate")]
    [InlineData("blank")]
    public void SkillEntrypointRejectsInvalidNameSets(string scenario)
    {
        string[] names = scenario switch
        {
            "empty" => [],
            "duplicate" => ["design", "design"],
            "blank" => [" "],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The Skill name scenario is not defined."),
        };

        Assert.ThrowsAny<ArgumentException>(() => OpenForgeDocumentSeed.SkillEntrypoint(names));
    }
}
