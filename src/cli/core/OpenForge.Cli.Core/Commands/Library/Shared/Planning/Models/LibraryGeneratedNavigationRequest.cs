using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Planning.Models;

internal sealed record LibraryGeneratedNavigationRequest
{
    public required CliWorkspace Workspace { get; init; }
    public required LibraryRecord SelectedLibrary { get; init; }
    public required LibrariesRecord? CurrentRecord { get; init; }
    public required ImmutableArray<EligibleSourceFile> IntendedEntries { get; init; }
}
