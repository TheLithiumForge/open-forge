using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Services;

internal enum ToolChangeDisposition { Add, NoChange, Migrate, Conflict, Remove, Keep }

internal sealed record ToolConfigRequest
{
    public required string ToolId { get; init; }
    public required string WorkspacePath { get; init; }
    public required string MainRoot { get; init; }
    public required string ProfileDigest { get; init; }
    public ToolCommandTarget? Invocation { get; init; }
    public required IReadOnlyList<ToolConfigChange> Changes { get; init; }
    public required IReadOnlyList<ToolAppliedChange> AppliedChanges { get; init; }
    public required IReadOnlyList<ToolNativeSettingRecord> NativeSettings { get; init; }
}

internal sealed record ToolConfigObservation
{
    public required ToolConfigChange Change { get; init; }
    public required ToolRecordedValue Current { get; init; }
    public required ToolRecordedValue Desired { get; init; }
    public required ToolChangeDisposition Disposition { get; init; }
    public string? Conflict { get; init; }
}

internal sealed record ToolConfigPlan(ToolConfigRequest Request, IReadOnlyList<ToolConfigObservation> Changes);

internal sealed record ToolConfigApplyResult
{
    public required IReadOnlyList<ToolAppliedChange> Additions { get; init; }
    public required IReadOnlyList<ToolNativeSettingRecord> NativeSettings { get; init; }
    public required IReadOnlyList<string> Unfinished { get; init; }
    public string? Failure { get; init; }
}

internal sealed record ToolConfigRemoveRequest
{
    public required string ToolId { get; init; }
    public required string WorkspacePath { get; init; }
    public required ToolLockRecord Lock { get; init; }
    public required IReadOnlyList<ToolNativeSettingRecord> NativeSettings { get; init; }
}

internal sealed record ToolConfigRemoveResult
{
    public required IReadOnlyList<string> RemovedChanges { get; init; }
    public required IReadOnlyList<ToolAppliedChange> KeptAdditions { get; init; }
    public required IReadOnlyList<ToolNativeSettingRecord> KeptNativeSettings { get; init; }
    public string? Failure { get; init; }
}
