namespace OpenForge.Cli.Core.Commands.Shared.Permissions.Models;

internal sealed class WorkspacePermissionJson
{
    public required string Path { get; init; }
    public required string[] Required { get; init; }
    public required string[] Missing { get; init; }
    public required string Decision { get; init; }
    public required string Action { get; init; }
    public required string Outcome { get; init; }
}
