using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Observation.Models;

internal enum ToolObservationState { Current, Missing, Unavailable, Unsupported, Changed }
internal enum ToolObservationIssueKind { Missing, Unavailable, Unsupported, VersionChanged, ConfigurationDrift, MachineRecordMissing, NativeRecordMissing, LockMissing, ProfileChanged, PathNotReady }
internal sealed record ToolObservationIssue(ToolObservationIssueKind Kind, string? ChangeId = null);
internal sealed record ToolObservationRequest(ToolCatalogueEntry Entry, ToolRecordsRequest Locations, ToolRecordsResult Records, string MainRoot);
internal sealed record ToolObservation
{
    internal required ToolCatalogueEntry Entry { get; init; }
    internal ToolSetting? Setting { get; init; }
    internal ToolLockRecord? Locked { get; init; }
    internal ToolMachineState? Machine { get; init; }
    internal required ToolRecordReadState MachineRecordState { get; init; }
    internal ToolRecordReadState NativeRecordState { get; init; } = ToolRecordReadState.Missing;
    internal IReadOnlyList<ToolNativeSettingRecord> NativeSettings { get; init; } = [];
    internal required ToolDetectionResult Detection { get; init; }
    internal ToolVersionProfile? Profile { get; init; }
    internal ToolConfigPlan? Configuration { get; init; }
    internal required ToolPathFacts Path { get; init; }
    internal required IReadOnlyList<ToolPackageManagerFacts> Managers { get; init; }
    internal required ToolObservationState State { get; init; }
    internal required IReadOnlyList<ToolObservationIssue> Issues { get; init; }
}
