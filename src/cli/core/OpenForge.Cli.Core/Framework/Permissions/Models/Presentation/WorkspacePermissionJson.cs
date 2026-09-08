namespace OpenForge.Cli.Core.Framework.Permissions.Models.Presentation;

internal sealed class WorkspacePermissionJson
{
    public required string Path { get; init; }
    public required WorkspacePermissionJsonRequirement[] Required { get; init; }
    public required WorkspacePermissionJsonRequirement[] Missing { get; init; }
    public required string Decision { get; init; }
    public required string Action { get; init; }
    public required string Outcome { get; init; }
}

internal sealed class WorkspacePermissionJsonRequirement
{
    public required string Id { get; init; }
    public required string Path { get; init; }
}
