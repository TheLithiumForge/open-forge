using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Completion.Models;

internal sealed record LibraryMutationPlanProjectionInput
{
    public required WorkspaceLayout? Layout { get; init; }
    public required CliWorkspace Workspace { get; init; }
    public required LibraryPlanState State { get; init; }
    public required LibraryMutationEffects? Effects { get; init; }
    public required PlannedFileChange? SettingsChange { get; init; }
}
