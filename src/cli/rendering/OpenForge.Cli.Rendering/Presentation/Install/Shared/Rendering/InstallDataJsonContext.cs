using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Shared.Models.Layout;

namespace OpenForge.Cli.Core.Presentation.Install.Shared.Rendering;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(OpenForge.Cli.Core.Presentation.Install.Models.InstallData))]
[JsonSerializable(typeof(WorkspaceLayoutInfo))]
[JsonSerializable(typeof(OpenForge.Cli.Core.Presentation.Install.Models.InstallDataFrontmatter))]
internal sealed partial class InstallDataJsonContext : JsonSerializerContext
{
}
