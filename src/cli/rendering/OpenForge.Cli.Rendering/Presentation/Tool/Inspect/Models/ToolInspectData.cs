using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Tool.Models.Inventory;
using OpenForge.Cli.Core.Commands.Tool.Models.Provider;

namespace OpenForge.Cli.Core.Presentation.Tool.Inspect.Models;

internal sealed record ToolInspectData(string? Id, [property: JsonIgnore] string? Version, JsonElement? Definition,
    string Target, ToolInstalledFact? Installed, bool? Telemetry, string[] Requirements, string[] Limitations, ToolInstruction Instructions);
