using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Tool.List.Models;

internal sealed record ToolListRequest(CliWorkspace Workspace, string? Source, string? Role);
