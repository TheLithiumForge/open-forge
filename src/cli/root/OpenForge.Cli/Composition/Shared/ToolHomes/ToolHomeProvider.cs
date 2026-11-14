using System.Collections.Immutable;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Composition.Shared.ToolHomes.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;

namespace OpenForge.Cli.Composition.Shared.ToolHomes;

// Execution-time Root input resolution. Construction captures inputs and performs no host activity.
// Contract: .agents/memory/crystallized/documents/cli/technical-designs/tool-setup.md#root-hosts
internal sealed class ToolHomeProvider
{
    private readonly ToolHomeEnvironment _environment;
    private readonly ToolOperatingSystem _operatingSystem;
    private readonly Func<Environment.SpecialFolder, string> _knownFolder;
    private readonly string? _injectedHome;
    private readonly string? _injectedNativeHome;
    private readonly ImmutableDictionary<string, string> _injectedRoots;
    private readonly Lazy<string> _userHome;
    private readonly Lazy<string> _home;
    private readonly Lazy<string> _runtimeParent;

    internal ToolHomeProvider(IReadOnlyDictionary<string, string> environment,
        Func<Environment.SpecialFolder, string> knownFolder, ToolOperatingSystem operatingSystem, CliToolHostInputs? inputs = null)
    {
        if (!Enum.IsDefined(operatingSystem)) throw new ArgumentOutOfRangeException(nameof(operatingSystem));
        _environment = new(inputs?.ProcessEnvironment ?? environment, operatingSystem);
        _knownFolder = knownFolder;
        _operatingSystem = operatingSystem;
        _injectedHome = inputs?.HomePath;
        _injectedNativeHome = inputs?.NativeHomePath;
        _injectedRoots = inputs?.ClientRoots.ToImmutableDictionary(StringComparer.Ordinal) ?? ImmutableDictionary<string, string>.Empty;
        _userHome = new(ResolveUserHome);
        _home = new(ResolveOpenForgeHome);
        _runtimeParent = new(ResolveRuntimeParent);
    }

    internal string HomePath => _home.Value;

    internal ToolHomeFacts Resolve(ToolVersionProfile profile)
    {
        var nativeHomeInput = NativeHome(profile.NativeHomePolicy);
        var nativeHome = ToolHomePaths.Resolve(nativeHomeInput, directory: true);
        var clients = profile.NativeHomePolicy == ToolNativeHomePolicy.HomeThenUserProfile
            ? new ToolClientRootResolver(environment: _environment, home: Path.GetFullPath(nativeHomeInput), operatingSystem: _operatingSystem,
                injected: _injectedRoots, runtimeParent: ReadRuntimeParent).Resolve()
            : InjectedClients();
        return new()
        {
            HomePath = HomePath,
            BinPath = Path.Combine(HomePath, ToolRecordDefinitions.BinDirectoryName),
            NativeHomePath = nativeHome,
            Clients = clients,
            NativeEnvironment = NativeEnvironment(profile),
        };
    }

    private string ResolveUserHome() => ToolHomePaths.Resolve(_knownFolder(Environment.SpecialFolder.UserProfile), directory: true);
    private string ResolveOpenForgeHome() => ToolHomePaths.Resolve(
        _injectedHome ?? Path.Combine(_userHome.Value, ToolRecordDefinitions.HomeDirectoryName), directory: true);

    private string NativeHome(ToolNativeHomePolicy policy)
    {
        if (!Enum.IsDefined(policy)) throw new ArgumentOutOfRangeException(nameof(policy));
        if (_injectedNativeHome is not null) return _injectedNativeHome;
        return policy switch
        {
            ToolNativeHomePolicy.Node => NodeHome(),
            ToolNativeHomePolicy.HomeThenUserProfile => _environment.Nonempty(ToolHomeEnvironment.Home)
                ?? _environment.Nonempty(ToolHomeEnvironment.UserProfile)
                ?? throw new IOException("The native HOME and USERPROFILE are unavailable."),
            _ => throw new ArgumentOutOfRangeException(nameof(policy)),
        };
    }

    private string NodeHome()
    {
        var key = _operatingSystem == ToolOperatingSystem.Windows ? ToolHomeEnvironment.UserProfile : ToolHomeEnvironment.Home;
        return _environment.Values.TryGetValue(key, out var value) ? value : _userHome.Value;
    }

    private string ReadRuntimeParent() => _runtimeParent.Value;

    private string ResolveRuntimeParent()
    {
        if (_environment.Nonempty(ToolHomeEnvironment.RuntimeDirectory) is { } value) return value;
        return _operatingSystem switch
        {
            ToolOperatingSystem.Windows => _knownFolder(Environment.SpecialFolder.LocalApplicationData),
            ToolOperatingSystem.MacOS => "/private/tmp",
            ToolOperatingSystem.Linux => "/tmp",
            _ => throw new ArgumentOutOfRangeException(nameof(_operatingSystem)),
        };
    }

    private ToolClientRoots InjectedClients()
    {
        var directories = _injectedRoots.Where(pair => !ToolClientRootResolver.IsFileBinding(pair.Key))
            .ToImmutableDictionary(pair => pair.Key, pair => ToolHomePaths.Resolve(pair.Value, directory: true), StringComparer.Ordinal);
        var files = _injectedRoots.Where(pair => ToolClientRootResolver.IsFileBinding(pair.Key))
            .ToImmutableDictionary(pair => pair.Key, pair => ToolHomePaths.Resolve(pair.Value, directory: false), StringComparer.Ordinal);
        return new(directories: directories, files: files, unavailable: ImmutableDictionary<string, string>.Empty);
    }

    private ImmutableDictionary<string, string> NativeEnvironment(ToolVersionProfile profile)
    {
        if (!profile.Changes.OfType<ToolNativeConfigChange>().Any(change => change.AllowListId == ToolNativeConfigDefinitions.CodeGraphTelemetryAllowListId))
            return ImmutableDictionary<string, string>.Empty;
        string[] keys = [ToolNativeConfigDefinitions.DoNotTrackVariable, ToolNativeConfigDefinitions.CodeGraphTelemetryVariable];
        var values = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        foreach (var key in keys)
            if (_environment.Values.TryGetValue(key, out var value)) values.Add(key, value);
        return values.ToImmutable();
    }
}
