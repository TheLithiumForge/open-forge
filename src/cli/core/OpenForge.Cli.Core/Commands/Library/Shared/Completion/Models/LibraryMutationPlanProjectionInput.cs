using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Completion.Models;

internal sealed record LibraryMutationPlanProjectionInput
{
    public required CliWorkspace Workspace { get; init; }
    public required LibraryPlanState State { get; init; }
    public required LibraryMutationEffects? Effects { get; init; }
}
