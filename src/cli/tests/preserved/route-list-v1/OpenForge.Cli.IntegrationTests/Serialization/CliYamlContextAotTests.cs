using OpenForge.Cli.Serialization;
using YamlDotNet.Serialization;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class CliYamlContextAotTests
{
    [Fact(DisplayName = "Native AOT directly executes all concrete CLI YAML shapes"), Trait("Feature", "cli-yaml"), Trait("Evidence", "Integration")]
    public void ExecutesAllConcreteYamlShapes()
    {
        var context = new CliYamlContext();
        var deserializer = new StaticDeserializerBuilder(context).Build();

        var openForge = deserializer.Deserialize<CliOpenForgeMetadata>(
            "description: Native route\ntags: [NativeAOT, Route]\n");
        var authored = deserializer.Deserialize<CliAuthoredMetadata>(
            "open-forge:\n  description: Authored route\n  tags: [Authored]\nname: route-name\ndescription: Top-level description\n");
        var skill = deserializer.Deserialize<CliSkillMetadata>(
            "name: native-skill\ndescription: Native Skill metadata.\n");

        Assert.Equal("Native route", openForge.Description);
        Assert.Equal(["NativeAOT", "Route"], openForge.Tags!);
        Assert.Equal("route-name", authored.Name);
        Assert.Equal("Top-level description", authored.Description);
        Assert.Equal("Authored route", authored.OpenForge?.Description);
        Assert.Equal(["Authored"], authored.OpenForge?.Tags!);
        Assert.Equal("native-skill", skill.Name);
        Assert.Equal("Native Skill metadata.", skill.Description);
    }
}
