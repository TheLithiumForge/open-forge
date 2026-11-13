using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Models.Evidence;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Provider;

internal sealed record ToolStatusRow(string Id, string Version, string? Target, string[] Roles,
    ToolObservation Installation, ToolObservation Configuration, ToolObservation Index,
    ToolObservation Guidance, JsonElement? ProcessPolicy, string[] Limits);
