using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;

namespace OpenForge.Cli.Core.Commands.Update.Models.Planning;

internal sealed record UpdateMigrationPlan
{
    public required string Path { get; init; }

    public required IReadOnlyList<WorkspaceAdoptionAction> Actions { get; init; }

    public required IReadOnlyList<string> Fields { get; init; }

    public required IReadOnlyList<WorkspaceAdoptionDerivation> Derivation { get; init; }

    public bool IsUserOwnedSource { get; init; }
}
