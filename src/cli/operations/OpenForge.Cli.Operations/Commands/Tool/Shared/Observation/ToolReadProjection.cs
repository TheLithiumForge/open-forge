using System.Globalization;
using OpenForge.Cli.Core.Commands.Tool.Models.Read;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Observation;

internal static class ToolReadProjection
{
    internal static ToolReadTool Catalogue(ToolCatalogueEntry entry, ToolSetting? setting) => new()
    {
        Id = entry.Identity.Id,
        Name = entry.Identity.Name,
        Origin = Name(entry.Identity.Origin),
        Upstream = entry.Identity.Upstream.AbsoluteUri,
        Licence = entry.Identity.Licence,
        DeclaredRange = setting is null ? null : Range(setting.Version),
        Channels = entry.Channels.Select(Channel).ToArray(),
        Profiles = entry.Profiles.Select(Profile).ToArray(),
    };

    internal static ToolReadTool Observed(ToolObservation facts)
    {
        var manager = facts.Detection.PackageManager ?? facts.Machine?.PackageManager;
        var currentManager = manager is null ? null : facts.Managers.FirstOrDefault(value => value.Client == manager.Client);
        return Catalogue(facts.Entry, facts.Setting) with
        {
            LockedVersion = facts.Locked?.Version,
            DetectedVersion = facts.Detection.Version,
            DetectionFailure = facts.Detection.Failure,
            Channel = facts.Detection.ChannelId ?? facts.Machine?.ChannelId ?? facts.Locked?.Channel,
            State = Name(facts.State),
            MachineState = Name(facts.MachineRecordState),
            NativeRecordState = Name(facts.NativeRecordState),
            NativeSettings = facts.NativeSettings.Select(value => new ToolReadNativeSetting(value.ChangeId, value.NativeHomePath, Value(value.Previous), Value(value.Applied), value.ManualRestoreArguments)).ToArray(),
            CommandPath = facts.Detection.CommandPath,
            ToolPath = facts.Machine?.ToolPath,
            RuntimePath = facts.Detection.RuntimePath ?? facts.Machine?.RuntimePath,
            BinPath = facts.Path.BinPath,
            BinReadiness = Name(facts.Path.Readiness),
            ManagerGlobalBinPath = currentManager?.GlobalBinPath ?? manager?.GlobalBinPath,
            ManagerGlobalBinReadiness = currentManager is null ? "unavailable" : Name(currentManager.GlobalBinReadiness),
            Managers = facts.Managers.Select(value => new ToolReadManager(Name(value.Client), value.Version, value.RuntimePath,
                value.RuntimeVersion, value.GlobalBinPath, Name(value.GlobalBinReadiness), value.SupportsGlobalInstall)).ToArray(),
            Changes = facts.Configuration?.Changes.Select(value => new ToolReadChange(value.Change.Id, Name(value.Disposition),
                Value(value.Current), Value(value.Desired), value.Conflict)
            { NativeEnvironmentOverrides = value.NativeEnvironmentOverrides }).ToArray() ?? [],
            Removals = facts.Configuration?.Removals.Select(value => new ToolReadChange(value.Addition.Id, Name(value.Disposition),
                Value(value.Current), Value(value.Desired), value.Conflict)).ToArray() ?? [],
            Issues = facts.Issues.Select(value => Name(value.Kind)).ToArray(),
        };
    }

    internal static string Range(ToolVersionRange value) => string.Create(CultureInfo.InvariantCulture, $"{value.Major}.{value.Minor}.x");
    internal static string Name<T>(T value) where T : struct, Enum
    {
        if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
        return System.Text.Json.JsonNamingPolicy.KebabCaseLower.ConvertName(value.ToString());
    }

    private static ToolReadChannel Channel(ToolChannel channel) => new()
    {
        Id = channel.Id,
        Kind = Name(channel.Kind),
        Package = (channel as ToolNpmChannel)?.Package,
        Repository = (channel as ToolGitHubReleaseChannel)?.Repository,
        RequiredClient = channel is ToolNpmChannel { RequiredClient: { } client } ? Name(client) : null,
        MinimumClientVersions = channel is ToolNpmChannel npm ? npm.MinimumClientVersions.ToDictionary(pair => Name(pair.Key), pair => pair.Value) : new Dictionary<string, string>(),
        Runtime = channel.Requirements.Runtime?.Runtime,
        RuntimeRange = channel.Requirements.Runtime?.VersionRange,
        Targets = channel.Requirements.Targets.Select(Target).ToArray(),
        Assets = channel is ToolGitHubReleaseChannel release ? release.Assets.Select(Asset).ToArray() : [],
    };

    private static ToolReadAsset Asset(ToolReleaseAsset asset)
    {
        var runtime = asset.BundledRuntime;
        if (runtime is null) return new(asset.Version, Target(asset.Target), null);
        return new(asset.Version, Target(asset.Target), new(runtime.Runtime, runtime.Version, runtime.RuntimeRelativePath, runtime.EntryRelativePath, runtime.Arguments));
    }

    private static ToolReadTarget Target(ToolTarget target) => new(Name(target.OperatingSystem), Name(target.Cpu),
        target.LinuxLibc is { } libc ? Name(libc) : null, target.MinimumOperatingSystemVersion, target.MinimumLibcVersion,
        target.MinimumLinuxKernelVersion, target.MinimumLibStdCppVersion);

    private static ToolReadProfile Profile(ToolVersionProfile profile) => new()
    {
        Id = profile.Id,
        Range = Range(profile.Range),
        MinimumToolVersion = profile.MinimumToolVersion,
        MinimumOpenForgeVersion = profile.MinimumOpenForgeVersion,
        TestedVersions = profile.TestedVersions,
        NativeHomePolicy = Name(profile.NativeHomePolicy),
        Setup = Command(profile.Setup),
        Uninstall = Command(profile.Uninstall),
        Additions = profile.Changes.Select(Config).ToArray(),
    };

    private static ToolReadCommand Command(ToolSetupSpec spec) => new(spec.Arguments, spec.DryRunArguments, spec.Environment, spec.Timeout.TotalSeconds,
        spec.Effects.Select(effect => new ToolReadEffect(effect.Id, Name(effect.Scope), Name(effect.Kind), effect.Description, effect.PathTemplate, effect.Selector)).ToArray())
    {
        MaximumOutputBytes = spec.MaximumOutputBytes,
        NativePlan = spec.NativePlan is { } plan ? new(plan.Arguments, Name(plan.Parser), plan.Timeout.TotalSeconds, plan.MaximumOutputBytes) : null,
        PossibleOutputs = spec.PossibleOutputs.Select(output => new ToolReadNativeOutput(output.PathTemplate, Name(output.Scope), Name(output.Match),
            Name(output.Purpose), output.OperatingSystem is { } os ? Name(os) : null, output.ClientId)).ToArray(),
    };

    private static ToolReadConfig Config(ToolConfigChange change)
    {
        var result = new ToolReadConfig { Id = change.Id, Kind = string.Empty, InitialState = Name(change.InitialState), Scope = Name(change.Scope), Description = change.Description };
        return change switch
        {
            ToolJsonValueChange json => result with { Kind = Name(json.Operation), PathTemplate = json.PathTemplate, Pointer = json.Pointer, Value = json.Value },
            ToolMarkedBlockChange block => result with
            {
                Kind = ToolRecordDefinitions.MarkedBlockKind,
                PathTemplate = block.PathTemplate,
                StartMarker = block.StartMarker,
                EndMarker = block.EndMarker,
                WhenNotIgnored = block.WhenNotIgnored?.WorkspaceRelativePath,
                ContentTemplate = block.ContentTemplate
            },
            ToolManagedFileChange file => result with { Kind = ToolRecordDefinitions.ManagedFileKind, PathTemplate = file.PathTemplate, TemplateResource = file.TemplateResource },
            ToolNativeConfigChange native => result with
            {
                Kind = ToolRecordDefinitions.NativeCommandKind,
                AllowListId = native.AllowListId,
                ApplyArguments = native.ApplyArguments,
                ReadArguments = native.ReadArguments,
                ReadEnvironment = native.ReadEnvironment,
                ValueParserId = native.ValueParserId,
                ReadTimeoutSeconds = native.ReadTimeout?.TotalSeconds,
                ReadMaximumOutputBytes = native.ReadMaximumOutputBytes,
                StoredValue = native.StoredValue is { } stored ? new(stored.PathTemplate, Name(stored.Parser), stored.MissingValue, stored.MaximumBytes) : null,
                DesiredValue = native.DesiredValue,
                ManualRestoreArguments = native.ManualRestoreArgumentsTemplate
            },
            _ => throw new ArgumentOutOfRangeException(nameof(change)),
        };
    }

    private static string? Value(ToolRecordedValue value)
    {
        if (!value.Present) return null;
        return value.Kind switch
        {
            ToolRecordedValueKind.Native => value.Native,
            ToolRecordedValueKind.Json => value.Json?.GetRawText(),
            ToolRecordedValueKind.Bytes => value.Bytes is null ? null : Convert.ToBase64String(value.Bytes),
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
    }
}
