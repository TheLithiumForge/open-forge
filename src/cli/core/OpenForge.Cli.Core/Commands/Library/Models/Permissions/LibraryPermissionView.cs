using OpenForge.Cli.Core.Framework.Permissions;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Serialization;

namespace OpenForge.Cli.Core.Commands.Library.Models.Permissions;

internal sealed record LibraryPermissionView
{
    public required string Path { get; init; }
    public required LibraryPermissionLeafView[] Required { get; init; }
    public required LibraryPermissionLeafView[] Missing { get; init; }
    public required LibraryPermissionScopeView[] ProposedScopes { get; init; }
    public required LibraryPermissionScopeView[] ApprovedScopes { get; init; }
    public required LibraryPermissionRebinding? Rebinding { get; init; }
    public required string Decision { get; init; }
    public required string Action { get; init; }
    public required string Outcome { get; init; }

    internal static LibraryPermissionView NotEvaluated() => new()
    {
        Path = WorkspacePermissionDefinitions.RelativePath,
        Required = [],
        Missing = [],
        ProposedScopes = [],
        ApprovedScopes = [],
        Rebinding = null,
        Decision = WorkspacePermissionJsonProjection.ReadName(WorkspacePermissionDecision.NotEvaluated),
        Action = WorkspacePermissionJsonProjection.ReadName(WorkspacePermissionAction.None),
        Outcome = WorkspacePermissionJsonProjection.ReadName(WorkspacePermissionOutcome.NotRequested),
    };
}

internal sealed record LibraryPermissionLeafView(string Id, string SourceRoot, string Path);

internal sealed record LibraryPermissionScopeView(string Id, string SourceRoot, string Kind, string Path);
