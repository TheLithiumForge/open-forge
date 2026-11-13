namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Inventory;

internal sealed record ToolAvailableRow(string Id, string Version, string[] Capabilities,
    string Target, string TargetSupport, string Admission, bool Reviewed, string? Blocker);
