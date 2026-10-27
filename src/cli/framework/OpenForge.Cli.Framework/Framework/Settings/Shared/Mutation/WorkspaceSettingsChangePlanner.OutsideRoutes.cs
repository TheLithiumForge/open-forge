using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;

internal static partial class WorkspaceSettingsChangePlanner
{
    internal static PlannedFileChange? PlanOutsideRouteRegistration(WorkspaceSettingsRead observation, string directoryPath)
        => PlanRootBytes(observation, bytes => WorkspaceSettingsCodec.AddOutsideRoute(bytes, directoryPath));

    internal static PlannedFileChange? PlanOutsideRouteDeregistration(WorkspaceSettingsRead observation, string directoryPath)
        => PlanRootBytes(observation, bytes => WorkspaceSettingsCodec.RemoveOutsideRoute(bytes, directoryPath));
}
