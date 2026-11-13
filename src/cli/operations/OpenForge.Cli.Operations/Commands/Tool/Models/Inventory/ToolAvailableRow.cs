namespace OpenForge.Cli.Core.Commands.Tool.Models.Inventory;

internal sealed record ToolAvailableRow(string Id, string Version, string Origin, string[] Capabilities,
    string Target, string TargetSupport, string Admission, bool Reviewed, string? Blocker);
