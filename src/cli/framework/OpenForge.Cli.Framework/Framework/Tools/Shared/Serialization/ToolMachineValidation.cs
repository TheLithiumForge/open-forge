using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;

internal static class ToolMachineValidation
{
    internal static void Validate(object value)
    {
        switch (value)
        {
            case ToolMachineState machine:
                ToolPortableCodec.ValidateId(machine.ToolId);
                ArgumentException.ThrowIfNullOrWhiteSpace(machine.ChannelId);
                Absolute(machine.CommandPath);
                ArgumentNullException.ThrowIfNull(machine.Invocation);
                Absolute(machine.Invocation.ExecutablePath);
                ArgumentNullException.ThrowIfNull(machine.Invocation.PrefixArguments);
                ArgumentNullException.ThrowIfNull(machine.Launchers);
                ArgumentNullException.ThrowIfNull(machine.Integrity);
                if (machine.ToolPath is not null) Absolute(machine.ToolPath);
                if (machine.RuntimePath is not null) Absolute(machine.RuntimePath);
                foreach (var launcher in machine.Launchers)
                {
                    ArgumentNullException.ThrowIfNull(launcher, JsonProperty(nameof(machine.Launchers)));
                    if (launcher.ToolId != machine.ToolId || !Enum.IsDefined(launcher.Kind) || !Enum.IsDefined(launcher.TargetKind))
                        throw new ArgumentException("The launcher requires a matching tool ID and recognized kinds.");
                    Absolute(launcher.LauncherPath); Absolute(launcher.EntryPath);
                    if (launcher.RuntimePath is not null) Absolute(launcher.RuntimePath);
                    ArgumentNullException.ThrowIfNull(launcher.Arguments);
                }
                foreach (var integrity in machine.Integrity) Integrity(integrity);
                break;
            case ToolWorkspaceMachineState workspace:
                Absolute(workspace.WorkspacePath);
                ArgumentNullException.ThrowIfNull(workspace.NativeSettings);
                foreach (var setting in workspace.NativeSettings)
                {
                    ArgumentNullException.ThrowIfNull(setting, JsonProperty(nameof(workspace.NativeSettings)));
                    ToolPortableCodec.ValidateId(setting.ToolId);
                    ArgumentException.ThrowIfNullOrWhiteSpace(setting.ChangeId);
                    Absolute(setting.NativeHomePath);
                    if (!PhysicalIdentityTracker.PathComparer.Equals(setting.NativeHomePath, OpenForge.Cli.Core.Framework.Tools.Shared.Records.ToolRecordPaths.ResolveDirectory(setting.NativeHomePath)))
                        throw new ArgumentException("Native-setting homes must name the resolved physical directory.");
                    ArgumentNullException.ThrowIfNull(setting.Previous);
                    ArgumentNullException.ThrowIfNull(setting.Applied);
                    ArgumentNullException.ThrowIfNull(setting.ManualRestoreArguments);
                    if (!setting.Previous.Present || !setting.Applied.Present)
                        throw new ArgumentException("Native settings require readable previous and applied values.");
                    Recorded(setting.Previous); Recorded(setting.Applied);
                }
                break;
            case ToolNodeRuntimeRecord runtime:
                Absolute(runtime.RuntimePath); Integrity(runtime.Integrity);
                break;
            case ToolPathState path:
                Absolute(path.BinPath);
                ArgumentNullException.ThrowIfNull(path.Changes);
                foreach (var change in path.Changes)
                {
                    ArgumentNullException.ThrowIfNull(change, JsonProperty(nameof(path.Changes)));
                    if (!Enum.IsDefined(change.Kind)) throw new ArgumentException("The PATH change kind is unrecognized.");
                    ArgumentException.ThrowIfNullOrWhiteSpace(change.Target);
                    ArgumentNullException.ThrowIfNull(change.Prior); ArgumentNullException.ThrowIfNull(change.Applied);
                    Recorded(change.Prior); Recorded(change.Applied);
                }
                break;
            default: throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

    private static string JsonProperty(string name) => System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(name);

    private static void Absolute(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path)) throw new ArgumentException("Machine paths must be absolute.");
    }

    private static void Integrity(ToolIntegrityEvidence value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (!Enum.IsDefined(value.Kind)) throw new ArgumentException("The integrity kind is unrecognized.");
        ArgumentException.ThrowIfNullOrWhiteSpace(value.Source);
        if (value.Sha256 is not null) ToolPortableCodec.ValidateDigest(value.Sha256);
    }

    private static void Recorded(ToolRecordedValue value)
    {
        if (!Enum.IsDefined(value.Kind)) throw new ArgumentException("The recorded value kind is unrecognized.");
        if (!value.Present) return;
        var complete = value.Kind switch
        {
            ToolRecordedValueKind.Native => value.Native is not null,
            ToolRecordedValueKind.Json => value.Json is not null,
            ToolRecordedValueKind.Bytes => value.Bytes is not null,
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
        if (!complete) throw new ArgumentException("The recorded value is present but its content is missing.");
    }
}
