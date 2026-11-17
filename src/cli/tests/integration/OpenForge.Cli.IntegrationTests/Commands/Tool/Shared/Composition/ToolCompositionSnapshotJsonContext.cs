using System.Text.Json.Serialization;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition;

[JsonSerializable(typeof(string[]))]
[JsonSourceGenerationOptions(WriteIndented = true, GenerationMode = JsonSourceGenerationMode.Serialization)]
internal sealed partial class ToolCompositionSnapshotJsonContext : JsonSerializerContext;
