using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Commands.Shared.Permissions;

namespace OpenForge.Cli.Core.Commands.Library.Models.Permissions;

internal sealed record LibraryPermissionView
{
    public required string Path { get; init; }
    public required string[] Required { get; init; }
    public required string[] Missing { get; init; }
    public required LibraryPermissionScopeView[] ProposedScopes { get; init; }
    public required LibraryPermissionScopeView[] ApprovedScopes { get; init; }
    public required string Decision { get; init; }
    public required string Action { get; init; }
    public required string Outcome { get; init; }

    internal static LibraryPermissionView NotEvaluated() => new()
    {
        Path = WorkspaceSettingsDefinitions.RelativePath,
        Required = [],
        Missing = [],
        ProposedScopes = [],
        ApprovedScopes = [],
        Decision = WorkspacePermissionJsonProjection.ReadName(WorkspacePermissionDecision.NotEvaluated),
        Action = WorkspacePermissionJsonProjection.ReadName(WorkspacePermissionAction.None),
        Outcome = WorkspacePermissionJsonProjection.ReadName(WorkspacePermissionOutcome.NotRequested),
    };
}

internal sealed record LibraryPermissionScopeView(string Kind, string Path);
