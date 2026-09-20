using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using YamlDotNet.Serialization;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata;

[YamlStaticContext]
[YamlSerializable(typeof(FrameworkOpenForgeMetadataYamlModel))]
[YamlSerializable(typeof(FrameworkAuthoredMetadataYamlDocument))]
[YamlSerializable(typeof(FrameworkSkillMetadataYamlDocument))]
public partial class FrameworkMetadataYamlContext : StaticContext;
