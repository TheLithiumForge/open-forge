using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Status.Models;

internal sealed record ToolStatusRequest(CliWorkspace Workspace, string? Id);
