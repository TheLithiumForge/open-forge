namespace OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

internal sealed record WorkspaceMaintenanceBoundary(
    WorkspaceMaintenanceBoundaryKind Kind,
    string DirectoryPath,
    string AnchorPath);
