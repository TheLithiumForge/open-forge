using OpenForge.Cli.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class CliYamlContextTests
{
    [Fact(DisplayName = "CLI YAML context directly parses Open Forge scoped metadata"), Trait("Feature", "cli-yaml"), Trait("Evidence", "Integration")]
    public void ParsesOpenForgeMetadata()
    {
        var context = new CliYamlContext();
        var deserializer = new StaticDeserializerBuilder(context).Build();

        var metadata = deserializer.Deserialize<CliOpenForgeMetadata>(
            "description: Routed source\ntags: [Memory, CurrentTruth]\n");

        Assert.Equal("Routed source", metadata.Description);
        Assert.Equal(["Memory", "CurrentTruth"], metadata.Tags!);
    }

    [Fact(DisplayName = "CLI YAML context directly parses authored metadata with exact aliases"), Trait("Feature", "cli-yaml"), Trait("Evidence", "Integration")]
    public void ParsesAuthoredMetadata()
    {
        var deserializer = new StaticDeserializerBuilder(new CliYamlContext()).Build();

        var metadata = deserializer.Deserialize<CliAuthoredMetadata>(
            "open-forge:\n  description: Routed source\n  tags: [Memory]\nname: Authored name\ndescription: Top-level description\n");

        Assert.Equal("Authored name", metadata.Name);
        Assert.Equal("Top-level description", metadata.Description);
        Assert.Equal("Routed source", metadata.OpenForge?.Description);
        Assert.Equal(["Memory"], metadata.OpenForge?.Tags!);
    }

    [Fact(DisplayName = "CLI YAML context directly parses native Skill metadata"), Trait("Feature", "cli-yaml"), Trait("Evidence", "Integration")]
    public void ParsesSkillMetadata()
    {
        var deserializer = new StaticDeserializerBuilder(new CliYamlContext()).Build();

        var metadata = deserializer.Deserialize<CliSkillMetadata>(
            "name: experience-design\ndescription: Analyze user journeys.\n");

        Assert.Equal("experience-design", metadata.Name);
        Assert.Equal("Analyze user journeys.", metadata.Description);
    }

    [Fact(DisplayName = "CLI YAML context reports malformed YAML"), Trait("Feature", "cli-yaml"), Trait("Evidence", "Integration")]
    public void ReportsMalformedYaml()
    {
        var deserializer = new StaticDeserializerBuilder(new CliYamlContext()).Build();

        Assert.Throws<YamlException>(() => deserializer.Deserialize<CliAuthoredMetadata>("open-forge: [\n"));
    }

    [Fact(DisplayName = "CLI YAML context returns no model for an empty document"), Trait("Feature", "cli-yaml"), Trait("Evidence", "Integration")]
    public void EmptyYamlReturnsNoModel()
    {
        var deserializer = new StaticDeserializerBuilder(new CliYamlContext()).Build();

        var metadata = deserializer.Deserialize<CliSkillMetadata>(string.Empty);

        Assert.Null(metadata);
    }
}
