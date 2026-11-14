using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Services;

internal enum ToolChangeDisposition { Add, NoChange, Migrate, Conflict, Remove, Keep }

internal sealed record ToolConfigRequest
{
    public required string ToolId { get; init; }
    public required ToolRecipeContext Context { get; init; }
    public required string ProfileDigest { get; init; }
    public ToolCommandTarget? Invocation { get; init; }
    public ToolCommandTarget? InstalledInvocation { get; init; }
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
    public IReadOnlyDictionary<string, string>? NativeEnvironmentOverrides { get; init; }
}

internal sealed record ToolConfigRemovalObservation
{
    public required ToolAppliedChange Addition { get; init; }
    public required ToolRecordedValue Current { get; init; }
    public required ToolRecordedValue Desired { get; init; }
    public required ToolChangeDisposition Disposition { get; init; }
    public string? Conflict { get; init; }
}

internal sealed record ToolConfigPlan(
    ToolConfigRequest Request,
    IReadOnlyList<ToolConfigObservation> Changes,
    IReadOnlyList<ToolConfigRemovalObservation> Removals);

internal sealed record ToolConfigApplyResult
{
    public required IReadOnlyList<ToolAppliedChange> RemainingAdditions { get; init; }
    public required IReadOnlyList<string> CompletedChanges { get; init; }
    public required IReadOnlyList<string> RemovedChanges { get; init; }
    public required IReadOnlyList<ToolBackupCompletionRequest> PendingBackups { get; init; }
    public required IReadOnlyList<ToolNativeSettingRecord> NativeSettings { get; init; }
    public required IReadOnlyList<string> Unfinished { get; init; }
    public string? Failure { get; init; }
}

internal sealed record ToolConfigRemoveRequest
{
    public required string ToolId { get; init; }
    public required ToolRecipeContext Context { get; init; }
    public required ToolLockRecord Lock { get; init; }
    public required IReadOnlyList<ToolNativeSettingRecord> NativeSettings { get; init; }
}

internal sealed record ToolConfigRemovePlan(
    ToolConfigRemoveRequest Request,
    IReadOnlyList<ToolConfigRemovalObservation> Changes);

internal sealed record ToolConfigRemoveResult
{
    public required IReadOnlyList<string> RemovedChanges { get; init; }
    public required IReadOnlyList<ToolAppliedChange> RemainingAdditions { get; init; }
    public required IReadOnlyList<ToolBackupCompletionRequest> PendingBackups { get; init; }
    public required IReadOnlyList<string> Unfinished { get; init; }
    public required IReadOnlyList<ToolNativeSettingRecord> KeptNativeSettings { get; init; }
    public string? Failure { get; init; }
}
