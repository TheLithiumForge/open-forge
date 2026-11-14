using OpenForge.Cli.Composition.Shared.ToolHomes;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using static OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory.CodebaseMemoryDefinitions;

namespace OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes;

public sealed class ToolHomeProviderTests
{
    [Fact(DisplayName = "Home provider construction is inert and resolves one captured Open Forge home"), Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void DeferredHome()
    {
        using var fixture = new ToolHomeFixture();
        var before = Directory.GetFileSystemEntries(fixture.Owned.Path, "*", SearchOption.AllDirectories).Order().ToArray();
        var provider = fixture.Provider();
        Assert.Equal(0, fixture.FolderCalls);
        Assert.Equal(Path.Combine(fixture.User, ToolRecordDefinitions.HomeDirectoryName), provider.HomePath);
        Assert.Equal(provider.HomePath, provider.HomePath);
        Assert.Equal(1, fixture.FolderCalls);
        Assert.False(Directory.Exists(provider.HomePath));
        Assert.Equal(before, Directory.GetFileSystemEntries(fixture.Owned.Path, "*", SearchOption.AllDirectories).Order().ToArray());
    }

    [Theory(DisplayName = "Native home policies keep Node platform selection distinct from HOME-first CBM"),
     InlineData((int)ToolOperatingSystem.Windows), InlineData((int)ToolOperatingSystem.MacOS), InlineData((int)ToolOperatingSystem.Linux),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void HomePolicies(int platform)
    {
        var os = (ToolOperatingSystem)platform;
        using var fixture = new ToolHomeFixture();
        fixture.Environment[ToolHomeEnvironment.UserProfile] = fixture.User;
        var provider = fixture.Provider(os);
        var graph = provider.Resolve(ToolHomeFixture.Profile(OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodeGraph.CodeGraphDefinitions.Id));
        var memory = provider.Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(os == ToolOperatingSystem.Windows ? fixture.User : fixture.Native, graph.NativeHomePath);
        Assert.Equal(fixture.Native, memory.NativeHomePath);
        Assert.Equal(1, fixture.FolderCalls);
        Assert.Equal(fixture.User, fixture.Environment[ToolHomeEnvironment.UserProfile]);
        Assert.Equal(fixture.Native, fixture.Environment[ToolHomeEnvironment.Home]);
    }

    [Fact(DisplayName = "An absent Node variable uses the injected OS-user home while empty stays invalid"), Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void NodeFallback()
    {
        using var fixture = new ToolHomeFixture();
        var graph = ToolHomeFixture.Profile(OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodeGraph.CodeGraphDefinitions.Id);
        fixture.Environment.Remove(ToolHomeEnvironment.UserProfile);
        Assert.Equal(fixture.User, fixture.Provider().Resolve(graph).NativeHomePath);
        fixture.Environment[ToolHomeEnvironment.UserProfile] = string.Empty;
        Assert.Throws<IOException>(() => fixture.Provider().Resolve(graph));
    }

    [Fact(DisplayName = "CBM falls back from empty HOME to USERPROFILE and never borrows an OS-user fallback"), Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void CbmFallback()
    {
        using var fixture = new ToolHomeFixture();
        fixture.Environment[ToolHomeEnvironment.Home] = string.Empty;
        Assert.Equal(fixture.Native, fixture.Provider().Resolve(ToolHomeFixture.Profile(Id)).NativeHomePath);
        fixture.Environment.Remove(ToolHomeEnvironment.UserProfile);
        Assert.Throws<IOException>(() => fixture.Provider().Resolve(ToolHomeFixture.Profile(Id)));
    }

    [Fact(DisplayName = "Fully injected paths avoid every known-folder and privileged host call"), Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void CompleteInjection()
    {
        using var fixture = new ToolHomeFixture();
        var roots = ToolHomePathOracle.RequiredKeys().ToDictionary(key => key, key => fixture.NativePath("injected/" + key), StringComparer.Ordinal);
        var inputs = fixture.Inputs(roots);
        var provider = new ToolHomeProvider(new Dictionary<string, string>(), _ => throw new InvalidOperationException(), ToolOperatingSystem.Windows, inputs);
        fixture.Environment.Clear();
        roots.Clear();
        var facts = provider.Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(inputs.HomePath, facts.HomePath);
        Assert.Equal(fixture.Native, facts.NativeHomePath);
        Assert.Equal(ToolHomePathOracle.RequiredKeys(), facts.Clients.Bindings.Keys.Order(StringComparer.Ordinal));
        Assert.Empty(facts.Clients.Unavailable);
        Assert.All(facts.Clients.Files.Keys, key => Assert.True(ToolClientRootResolver.IsFileBinding(key)));
        Assert.All(facts.Clients.Directories.Keys, key => Assert.False(ToolClientRootResolver.IsFileBinding(key)));
        Assert.False(Directory.Exists(inputs.HomePath));
        Assert.False(Directory.Exists(fixture.NativePath("injected")));
        Assert.Equal(0, fixture.FolderCalls);
    }

    [Fact(DisplayName = "Raw declared native environment facts are captured separately without interpretation"), Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void NarrowEnvironment()
    {
        using var fixture = new ToolHomeFixture();
        fixture.Environment[ToolNativeConfigDefinitions.DoNotTrackVariable] = "synthetic raw value";
        fixture.Environment[ToolNativeConfigDefinitions.CodeGraphTelemetryVariable] = string.Empty;
        fixture.Environment[ToolNativeConfigDefinitions.CodeGraphNoDownloadVariable] = "synthetic unrelated value";
        var before = fixture.Environment.ToArray();
        var provider = fixture.Provider();
        fixture.Environment[ToolNativeConfigDefinitions.DoNotTrackVariable] = "changed after capture";
        var graph = provider.Resolve(ToolHomeFixture.Profile(OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodeGraph.CodeGraphDefinitions.Id));
        Assert.Equal(before.Single(pair => pair.Key == ToolNativeConfigDefinitions.DoNotTrackVariable).Value, graph.NativeEnvironment[ToolNativeConfigDefinitions.DoNotTrackVariable]);
        Assert.Equal(string.Empty, graph.NativeEnvironment[ToolNativeConfigDefinitions.CodeGraphTelemetryVariable]);
        Assert.Equal(2, graph.NativeEnvironment.Count);
        Assert.Empty(provider.Resolve(ToolHomeFixture.Profile(Id)).NativeEnvironment);
    }

    [Fact(DisplayName = "Every declared native home policy is handled and unnamed values are rejected"), Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void PolicyCoverage()
    {
        using var fixture = new ToolHomeFixture();
        ToolNativeHomePolicy[] policies = [ToolNativeHomePolicy.Node, ToolNativeHomePolicy.HomeThenUserProfile];
        Assert.Equal(policies.Order(), Enum.GetValues<ToolNativeHomePolicy>().Order());
        ToolOperatingSystem[] platforms = [ToolOperatingSystem.Windows, ToolOperatingSystem.MacOS, ToolOperatingSystem.Linux];
        Assert.Equal(platforms.Order(), Enum.GetValues<ToolOperatingSystem>().Order());
        var profile = ToolHomeFixture.Profile(Id);
        Assert.Throws<ArgumentOutOfRangeException>(() => fixture.Provider().Resolve(profile with { NativeHomePolicy = (ToolNativeHomePolicy)(-1) }));
        Assert.Throws<ArgumentOutOfRangeException>(() => fixture.Provider((ToolOperatingSystem)(-1)));
    }

    [Fact(DisplayName = "Injected native and Open Forge homes resolve existing physical ancestors without creating descendants"), Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void MissingInjectedHomes()
    {
        using var fixture = new ToolHomeFixture();
        var inputs = fixture.Inputs(new Dictionary<string, string>()) with { NativeHomePath = fixture.NativePath("missing/home") };
        var facts = fixture.Provider(inputs: inputs).Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(inputs.NativeHomePath, facts.NativeHomePath);
        Assert.Equal(Path.Combine(inputs.HomePath, ToolRecordDefinitions.BinDirectoryName), facts.BinPath);
        Assert.False(Directory.Exists(inputs.NativeHomePath));
        Assert.False(Directory.Exists(inputs.HomePath));
    }

    [Fact(DisplayName = "Production home and runtime known folders are each resolved once through the injected capability"),
     Trait("Feature", "tool-homes"), Trait("Evidence", "Integration")]
    public void KnownFoldersOnce()
    {
        using var fixture = new ToolHomeFixture();
        fixture.Environment.Remove(ToolHomeEnvironment.RuntimeDirectory);
        var calls = new Dictionary<Environment.SpecialFolder, int>();
        string Folder(Environment.SpecialFolder folder)
        {
            calls[folder] = calls.GetValueOrDefault(folder) + 1;
            return folder switch
            {
                Environment.SpecialFolder.UserProfile => fixture.User,
                Environment.SpecialFolder.LocalApplicationData => fixture.Runtime,
                _ => throw new ArgumentOutOfRangeException(nameof(folder)),
            };
        }
        var provider = new ToolHomeProvider(fixture.Environment, Folder, ToolOperatingSystem.Windows);
        Assert.Empty(calls);
        var first = provider.Resolve(ToolHomeFixture.Profile(Id));
        var second = provider.Resolve(ToolHomeFixture.Profile(Id));
        Assert.Equal(fixture.Runtime, first.Clients.Directories[RuntimeRoot]);
        Assert.Equal(first.HomePath, second.HomePath);
        Assert.Equal(2, calls.Count);
        Assert.All(calls.Values, count => Assert.Equal(1, count));
    }
}
