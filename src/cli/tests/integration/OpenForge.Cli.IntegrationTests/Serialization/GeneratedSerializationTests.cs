using System.Text.Json;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class GeneratedSerializationTests
{
    [Trait("Boundary", "Output")]
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

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Static YAML context parses every registered metadata shape")]
    [Trait("Feature", "cli-serialization"), Trait("Evidence", "Integration")]
    public void StaticYamlContextParsesEveryRegisteredMetadataShape()
    {
        var deserializer = new StaticDeserializerBuilder(new FrameworkMetadataYamlContext()).Build();

        var openForge = deserializer.Deserialize<FrameworkOpenForgeMetadataYamlModel>(
            "description: Routed source\ntags: [Memory, CurrentTruth]\n");
        var authored = deserializer.Deserialize<FrameworkAuthoredMetadataYamlDocument>(
            "open-forge:\n  description: Authored route\n  tags: [Authored]\nname: route-name\ndescription: Top-level\n");
        var skill = deserializer.Deserialize<FrameworkSkillMetadataYamlDocument>(
            "name: native-skill\ndescription: Native metadata.\n");

        Assert.Equal("Routed source", openForge.Description);
        Assert.Equal(["Memory", "CurrentTruth"], Assert.IsType<string[]>(openForge.Tags));
        Assert.Equal("route-name", authored.Name);
        Assert.Equal("Authored route", authored.OpenForge?.Description);
        Assert.Equal("native-skill", skill.Name);
        Assert.Throws<YamlException>(() =>
            deserializer.Deserialize<FrameworkAuthoredMetadataYamlDocument>("open-forge: [\n"));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Static YAML context returns null for an empty document")]
    [Trait("Feature", "cli-serialization"), Trait("Evidence", "Integration")]
    public void StaticYamlContextReturnsNullForEmptyDocument()
    {
        var deserializer = new StaticDeserializerBuilder(new FrameworkMetadataYamlContext()).Build();

        var metadata = deserializer.Deserialize<FrameworkSkillMetadataYamlDocument>(string.Empty);

        Assert.Null(metadata);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Static YAML context serializes canonical Framework metadata")]
    [Trait("Feature", "cli-serialization"), Trait("Evidence", "Integration")]
    public void StaticYamlContextSerializesCanonicalFrameworkMetadata()
    {
        var yaml = new FrameworkDocumentMetadataEmitter().Emit(
            new FrameworkDocumentMetadata(
                "AOT-safe route metadata",
                ["CurrentTruth"],
                "Owns static serialization."));

        Assert.Equal(
            "open-forge:\n"
                + "  description: AOT-safe route metadata\n"
                + "  tags: [CurrentTruth]\n"
                + "  responsibility: Owns static serialization.\n",
            yaml);
    }
}
