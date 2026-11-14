using OpenForge.Cli.Core.Commands.Tool.Models.Evidence;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Inventory;

internal sealed record ToolInstalledRow(string Id, string Version, string[] Roles,
    string[]? Capabilities, ToolObservation Verification);
