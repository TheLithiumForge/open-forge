using YamlDotNet.Serialization;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Models;

internal sealed class FrameworkOpenForgeMetadataYamlModel
{
    [YamlMember(Alias = "description", Order = 0)]
    public string? Description { get; set; }

    [YamlMember(Alias = "tags", Order = 1)]
    public string[]? Tags { get; set; }

    [YamlMember(
        Alias = "responsibility",
        Order = 2,
        DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    public string? Responsibility { get; set; }
}

internal sealed class FrameworkAuthoredMetadataYamlDocument
{
    [YamlMember(Alias = "open-forge", Order = 0)]
    public FrameworkOpenForgeMetadataYamlModel? OpenForge { get; set; }

    [YamlMember(Alias = "name", Order = 1)]
    public string? Name { get; set; }

    [YamlMember(Alias = "description", Order = 2)]
    public string? Description { get; set; }
}

internal sealed class FrameworkSkillMetadataYamlDocument
{
    [YamlMember(Alias = "name", Order = 0)]
    public string? Name { get; set; }

    [YamlMember(Alias = "description", Order = 1)]
    public string? Description { get; set; }
}
