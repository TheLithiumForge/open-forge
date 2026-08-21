using YamlDotNet.Serialization;

namespace OpenForge.Cli.Core.Shell.Serialization;

internal sealed class CliOpenForgeMetadata
{
    [YamlMember(Alias = "description")]
    public string? Description { get; set; }

    [YamlMember(Alias = "tags")]
    public string[]? Tags { get; set; }
}

internal sealed class CliAuthoredMetadata
{
    [YamlMember(Alias = "open-forge")]
    public CliOpenForgeMetadata? OpenForge { get; set; }

    [YamlMember(Alias = "name")]
    public string? Name { get; set; }

    [YamlMember(Alias = "description")]
    public string? Description { get; set; }
}

internal sealed class CliSkillMetadata
{
    [YamlMember(Alias = "name")]
    public string? Name { get; set; }

    [YamlMember(Alias = "description")]
    public string? Description { get; set; }
}

[YamlStaticContext]
[YamlSerializable(typeof(CliOpenForgeMetadata))]
[YamlSerializable(typeof(CliAuthoredMetadata))]
[YamlSerializable(typeof(CliSkillMetadata))]
public partial class CliYamlContext : StaticContext;
