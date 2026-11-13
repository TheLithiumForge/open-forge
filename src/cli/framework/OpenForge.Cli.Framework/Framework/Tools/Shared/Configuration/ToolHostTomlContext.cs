using Tomlyn.Model;
using Tomlyn.Serialization;
using Tomlyn;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Configuration;

[TomlSerializable(typeof(TomlTable))]
[TomlSourceGenerationOptions(DuplicateKeyHandling = TomlDuplicateKeyHandling.Error)]
internal sealed partial class ToolHostTomlContext : TomlSerializerContext;
