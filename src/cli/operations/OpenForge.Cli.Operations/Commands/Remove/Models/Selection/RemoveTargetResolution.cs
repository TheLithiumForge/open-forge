using OpenForge.Cli.Core.Commands.Remove.Models.Request;

namespace OpenForge.Cli.Core.Commands.Remove.Models.Selection;

internal sealed record RemoveTargetResolution(
    RemoveKind Kind,
    string Target,
    string? Cause,
    bool IsEntrypointFile = false);
