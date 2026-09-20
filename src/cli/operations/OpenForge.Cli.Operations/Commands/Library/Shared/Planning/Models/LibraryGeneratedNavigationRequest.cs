using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Planning.Models;

internal sealed record LibraryGeneratedNavigationRequest
{
    public required CliWorkspace Workspace { get; init; }
    public required LibraryRegistration SelectedLibrary { get; init; }
    public required LibraryRegistrationSet? CurrentRecord { get; init; }
    public required ImmutableArray<EligibleSourceFile> IntendedEntries { get; init; }
}
