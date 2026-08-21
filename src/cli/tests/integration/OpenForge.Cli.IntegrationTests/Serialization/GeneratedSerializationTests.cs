using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class GeneratedSerializationTests
{
    [Fact(DisplayName = "Source-generated JSON serializes registered concrete shape")]
    [Trait("Feature", "cli-serialization"), Trait("Evidence", "Integration")]
    public void SourceGeneratedJsonSerializesRegisteredConcreteShape()
    {
        var completion = new CliProcessCompletion(
            CliSemanticStatus.Complete,
            0,
            CliOutputTarget.StandardOutput);

        var json = JsonSerializer.Serialize(completion, CliJsonContext.Default.CliProcessCompletion);

        Assert.Contains("\"status\": 0", json, StringComparison.Ordinal);
        Assert.Contains("\"exitCode\": 0", json, StringComparison.Ordinal);
        Assert.False(JsonSerializer.IsReflectionEnabledByDefault);
    }

    [Fact(DisplayName = "Static YAML context parses every registered metadata shape")]
    [Trait("Feature", "cli-serialization"), Trait("Evidence", "Integration")]
    public void StaticYamlContextParsesEveryRegisteredMetadataShape()
    {
        var deserializer = new StaticDeserializerBuilder(new CliYamlContext()).Build();

        var openForge = deserializer.Deserialize<CliOpenForgeMetadata>(
            "description: Routed source\ntags: [Memory, CurrentTruth]\n");
        var authored = deserializer.Deserialize<CliAuthoredMetadata>(
            "open-forge:\n  description: Authored route\n  tags: [Authored]\nname: route-name\ndescription: Top-level\n");
        var skill = deserializer.Deserialize<CliSkillMetadata>(
            "name: native-skill\ndescription: Native metadata.\n");

        Assert.Equal("Routed source", openForge.Description);
        Assert.Equal(["Memory", "CurrentTruth"], Assert.IsType<string[]>(openForge.Tags));
        Assert.Equal("route-name", authored.Name);
        Assert.Equal("Authored route", authored.OpenForge?.Description);
        Assert.Equal("native-skill", skill.Name);
        Assert.Throws<YamlException>(() =>
            deserializer.Deserialize<CliAuthoredMetadata>("open-forge: [\n"));
    }
}
