using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands.Models;

namespace OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Request;

internal sealed record StripRequest
{
    public required CliWorkspace Workspace { get; init; }

    public required StripCommandKind Command { get; init; }

    public ImmutableArray<StripPart> Parts { get; init; } = [];

    public string? Flatten { get; init; }

    public ImmutableArray<string> RemoveEdited { get; init; } = [];

    public bool DryRun { get; init; }

    public bool Automatic { get; init; }

    public bool AllowInteractiveConfirmation { get; init; }

    public CliRetryOptions? RetryOptions { get; init; }

    internal bool Includes(StripPart part) => Parts.Contains(part);
}
