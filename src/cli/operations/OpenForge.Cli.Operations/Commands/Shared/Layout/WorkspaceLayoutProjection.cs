using OpenForge.Cli.Core.Commands.Shared.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Shared.Layout;

// Task 85 defines the optional layout facts shared by Status and Install.
internal static class WorkspaceLayoutProjection
{
    internal static WorkspaceLayoutInfo? Project(WorkspaceLayout? layout)
        => layout is { IsDefault: false } ? new(layout.MainRoot, layout.LoaderPath) : null;
}
