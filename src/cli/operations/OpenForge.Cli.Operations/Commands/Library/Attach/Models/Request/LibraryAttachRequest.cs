using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Library.Attach.Models.Request;

internal sealed record LibraryAttachRequest
{
    public required CliWorkspace Workspace { get; init; }

    public required LibraryId LibraryId { get; init; }

    public required WorkspaceRelativeDirectory SourceRoot { get; init; }

    public required LibraryDestinationRoot DestinationRoot { get; init; }

    public required bool AllowPrompt { get; init; }

    public bool Automatic { get; init; }

    public required LibraryMode Mode { get; init; }

    /// <summary>Paths named by <c>--allow-path</c>, staged with the permission plan.</summary>
    public ImmutableArray<string> Allow { get; init; } = [];
}
