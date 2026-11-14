using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Composition.Shared.ToolHomes;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Tools.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes;

internal sealed class ToolHomeFixture : IDisposable
{
    internal TemporaryWorkspace Owned { get; } = TemporaryWorkspace.Create("tool-home-roots");
    internal string Native { get; }
    internal string User { get; }
    internal string Runtime { get; }
    internal Dictionary<string, string> Environment { get; }
    internal int FolderCalls { get; private set; }
    internal static ToolCatalogue Catalogue { get; } = ToolCatalogueFactory.Create([]);

    internal ToolHomeFixture()
    {
        Native = Physical(Owned.CreateDirectory("native"));
        User = Physical(Owned.CreateDirectory("user"));
        Runtime = Physical(Owned.CreateDirectory("runtime"));
        Environment = new(StringComparer.Ordinal)
        {
            [ToolHomeEnvironment.Home] = Native,
            [ToolHomeEnvironment.UserProfile] = Native,
            [ToolHomeEnvironment.RuntimeDirectory] = Runtime,
        };
    }

    internal ToolHomeProvider Provider(ToolOperatingSystem os = ToolOperatingSystem.Windows, CliToolHostInputs? inputs = null)
        => new(Environment, KnownFolder, os, inputs);

    internal string KnownFolder(System.Environment.SpecialFolder folder)
    {
        FolderCalls++;
        Assert.Equal(System.Environment.SpecialFolder.UserProfile, folder);
        return User;
    }

    internal static ToolVersionProfile Profile(string id) => Assert.Single(Catalogue.Entries[id].Profiles);
    internal string NativePath(string suffix) => ToolHomePaths.Join(Native, suffix);
    internal string File(string suffix, string contents = "{}") => Physical(Owned.CreateFile("native/" + suffix, contents));
    internal string Directory(string suffix) => Physical(Owned.CreateDirectory("native/" + suffix));

    internal CliToolHostInputs Inputs(IReadOnlyDictionary<string, string> roots)
    {
        var hosts = new UnusedHosts();
        return new()
        {
            HomePath = Path.Combine(User, "injected-open-forge"),
            NativeHomePath = Native,
            ClientRoots = roots,
            ProcessEnvironment = Environment,
            Processes = hosts,
            Downloads = hosts,
            Path = hosts,
        };
    }

    internal static string Physical(string path) => new PhysicalPathResolver().ResolveRoot(path).GetContainedPhysicalPath();
    public void Dispose() => Owned.Dispose();

    private sealed class UnusedHosts : IToolProcessHost, IToolDownloadHost, IToolPathHost
    {
        public Task<ToolResolvedCommand?> ResolveAsync(string command, string workingDirectory, CancellationToken cancellationToken) => throw new InvalidOperationException();
        public Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken) => throw new InvalidOperationException();
        public Task<ToolNpmMetadataResult> ReadNpmMetadataAsync(ToolNpmMetadataRequest request, CancellationToken cancellationToken) => throw new InvalidOperationException();
        public Task<ToolDownloadResult> DownloadAsync(ToolDownloadRequest request, CancellationToken cancellationToken) => throw new InvalidOperationException();
        public Task<ToolPathFacts> ObserveAsync(string homePath, CancellationToken cancellationToken) => throw new InvalidOperationException();
        public Task<ToolPathSetupResult> AddAsync(ToolPathSetupRequest request, CancellationToken cancellationToken) => throw new InvalidOperationException();
        public Task<ToolPathRemoveResult> RemoveAsync(ToolPathRemoveRequest request, CancellationToken cancellationToken) => throw new InvalidOperationException();
    }
}
