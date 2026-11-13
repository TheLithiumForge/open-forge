using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Inventory;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Provider;

namespace OpenForge.Cli.Core.Presentation.Tool.Inspect.Models;

internal sealed record ToolInspectData(string? Id, [property: JsonIgnore] string? Version, JsonElement? Definition,
    string Target, ToolInstalledFact? Installed, string[] Requirements, string[] Limitations, ToolInstruction Instructions);
