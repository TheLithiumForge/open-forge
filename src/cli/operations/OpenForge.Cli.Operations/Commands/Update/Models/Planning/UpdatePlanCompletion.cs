using OpenForge.Cli.Core.Framework.Ownership.Models;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;

namespace OpenForge.Cli.Core.Commands.Update.Models.Planning;

internal sealed record UpdatePlanCompletion
{
    public required UpdateRequest Request { get; init; }

    public required FrameworkPayload Payload { get; init; }

    public required UpdateIntendedStateBuild Intended { get; init; }

    public required UpdatePlanningPlan Plan { get; init; }

    public required IReadOnlyList<UpdatePlannedEffect> Effects { get; init; }

    public required IReadOnlyList<PlannedDirectoryCreation> DirectoryCreations { get; init; }

    public required OwnershipWritePlanState OwnershipState { get; init; }

    public required PlannedFileChange? OwnershipChange { get; init; }

    public required IReadOnlyList<UpdateFinding> Findings { get; init; }
}
