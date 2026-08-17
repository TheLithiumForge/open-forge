using YamlDotNet.Serialization;

namespace OpenForge.Cli.Foundation;

[YamlSerializable]
internal sealed class FoundationYamlDocument
{
    public string Name { get; set; } = string.Empty;
}

[YamlStaticContext]
[YamlSerializable(typeof(FoundationYamlDocument))]
public partial class FoundationYamlContext : YamlDotNet.Serialization.StaticContext;
