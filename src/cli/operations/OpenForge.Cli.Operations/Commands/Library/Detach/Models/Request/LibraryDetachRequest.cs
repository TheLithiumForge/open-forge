using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Models.Request;

internal sealed record LibraryDetachRequest
{
    public required CliWorkspace Workspace { get; init; }

    public required LibraryId LibraryId { get; init; }

    public required bool AllowPrompt { get; init; }

    public bool Automatic { get; init; }

    public required LibraryMode Mode { get; init; }

    public ImmutableArray<string> Allow { get; init; } = [];
}
