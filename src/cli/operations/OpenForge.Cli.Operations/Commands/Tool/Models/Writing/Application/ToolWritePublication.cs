using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;

internal sealed record ToolWritePublication
{
    public required ToolWritePlan Plan { get; init; }
    public required ToolWriteToolPlan Tool { get; init; }
    public ToolLockRecord? Lock { get; init; }
    public ToolMachineState? Machine { get; init; }
    public IReadOnlyList<ToolNodeRuntimeRecord> Runtimes { get; init; } = [];
    public IReadOnlyList<ToolNativeSettingRecord>? NativeSettings { get; init; }
    public bool RemoveDeclaration { get; init; }
}

internal static class ToolWriteStages
{
    internal const string Channel = "channel";
    internal const string Native = "native";
    internal const string Config = "config";
    internal const string Records = "records";
    internal const string SharedFiles = "shared-files";
    internal const string Path = "path";
    internal const string Completed = "completed";
    internal const string Planned = "planned";
    internal const string Failed = "failed";
    internal const string NotStarted = "not-started";
    internal const string Skipped = "skipped";
    internal const string Kept = "kept";
}
