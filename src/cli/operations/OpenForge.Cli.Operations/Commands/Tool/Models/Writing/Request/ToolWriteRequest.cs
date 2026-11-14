using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;

internal enum ToolWriteKind { Install, Update, Remove }

internal sealed record ToolWriteRequest
{
    public required ToolWriteKind Kind { get; init; }
    public required CliWorkspace Workspace { get; init; }
    public ToolSpec? Spec { get; init; }
    public string? Channel { get; init; }
    public required bool Yes { get; init; }
    public required bool CanPrompt { get; init; }
    public required CliFormat Format { get; init; }
    public required bool Preview { get; init; }
    public ImmutableArray<string> AllowPaths { get; init; } = [];
}
