using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.PackageManagers;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;
using OpenForge.Cli.Core.Framework.Tools.Shared.Runtimes;
using OpenForge.Cli.Core.Framework.Tools.Shared.Path;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Supply;

[Trait("Feature", "tool-npm-channels"), Trait("Evidence", "Integration")]
public sealed class ToolNpmChannelTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "An exact npm plan uses local entry facts without a registry lookup")]
    public async Task PlansExactWithoutNetwork()
    {
        using var fixture = new ToolChannelFixture();
        var channel = fixture.Npm();
        var modules = System.IO.Path.Combine(fixture.Native, ToolNpmPackageReader.ModulesDirectory);
        fixture.Processes.Run = request => ToolChannelFixture.Success(modules);
        var result = await fixture.Service.PlanAsync(fixture.Request(fixture.Entry(channel)) with { AvailableManagers = [fixture.Manager()] }, Token);
        Assert.NotNull(result.Resolution);
        Assert.False(result.UsedNetwork);
        Assert.False(Directory.Exists(modules));
        Assert.Empty(fixture.Downloads.MetadataRequests);
        Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Home));
        Assert.All(fixture.Processes.Requests, request => Assert.Equal("root", request.Arguments[0]));
        Assert.Equal($"{channel.Package}@{ToolChannelFixture.Version}", result.Resolution.Execution.Commands[0].Arguments[^1]);
        Assert.Empty(result.Resolution.Execution.Commands[0].Environment);
        Assert.Equal(System.IO.Path.Combine(modules, channel.Package.Replace('/', System.IO.Path.DirectorySeparatorChar), channel.EntryRelativePath),
            Assert.Single(result.Resolution.Execution.Invocation.PrefixArguments));
    }

    [Fact(DisplayName = "Latest npm selection records the actual newer supported patch and discloses its lookup")]
    public async Task ResolvesLatestSupported()
    {
        using var fixture = new ToolChannelFixture();
        fixture.Processes.Run = request => Metadata(fixture, request);
        var result = await fixture.Service.PlanAsync(fixture.Request(fixture.Entry(fixture.Npm())) with
        {
            AvailableManagers = [fixture.Manager()],
            Selection = new() { Kind = ToolVersionSelectionKind.LatestSupported },
            AllowLatestLookup = true,
        }, Token);
        Assert.NotNull(result.Resolution);
        Assert.Equal("2.4.5", result.Resolution.Version);
        Assert.True(result.UsedNetwork);
        Assert.DoesNotContain(fixture.Processes.Requests, request => request.Arguments.Contains("install"));
        Assert.Equal(0, fixture.Downloads.Calls);
    }

    [Theory(DisplayName = "Project lock preference precedes unattended fallback while live ambiguity stays unresolved")]
    [InlineData("preferred")]
    [InlineData("default")]
    [InlineData("ask")]
    public async Task ChoosesManager(string mode)
    {
        using var fixture = new ToolChannelFixture();
        var channel = fixture.Npm();
        var modules = System.IO.Path.Combine(fixture.Native, ToolNpmPackageReader.ModulesDirectory);
        WritePackage(modules, channel.Package);
        fixture.Processes.Run = _ => ToolChannelFixture.Success(modules);
        var request = fixture.Request(fixture.Entry(channel)) with
        {
            AvailableManagers = [fixture.Manager(), fixture.Manager(ToolNpmClient.Pnpm)],
            ProjectManagers = mode == "preferred" ? [ToolNpmClient.Pnpm] : [],
            ManagerChoice = mode == "default" ? ToolManagerChoiceMode.Default : ToolManagerChoiceMode.Ask
        };
        var result = await fixture.Service.PlanAsync(request, Token);
        if (mode == "ask")
        {
            Assert.Null(result.Resolution);
            Assert.Equal(2, result.ManagerChoices.Count);
            Assert.Empty(result.Problems);
        }
        else
        {
            Assert.NotNull(result.Resolution);
            Assert.Equal(mode == "preferred" ? ToolNpmClient.Pnpm : ToolNpmClient.Npm, result.Resolution.PackageManager?.Client);
        }
    }

    [Fact(DisplayName = "Private Node uses its own npm and child PATH for native addon installation")]
    public async Task InstallsWithPrivateNode()
    {
        using var fixture = new ToolChannelFixture();
        var archive = fixture.Archive("node.exe", "node_modules/npm/bin/npm-cli.js");
        var node = new ToolPrivateNodeSpec
        {
            Version = ToolChannelFixture.RuntimeVersion,
            NpmRelativePath = "node_modules/npm/bin/npm-cli.js",
            SignedChecksums = new($"https://{ToolArtifactSource.NodeHost}/dist/v{ToolChannelFixture.RuntimeVersion}/SHASUMS256.txt.asc"),
            VerifiedSigner = "synthetic-signer",
            Archive = archive with { Version = ToolChannelFixture.RuntimeVersion, Url = new($"https://{ToolArtifactSource.NodeHost}/dist/v{ToolChannelFixture.RuntimeVersion}/node.zip"), EntryRelativePath = "node.exe" }
        };
        var manager = fixture.Manager(ToolNpmClient.Bun) with { RuntimePath = null, RuntimeVersion = null };
        fixture.Processes.Run = request => Metadata(fixture, request);
        var request = fixture.Request(fixture.Entry(fixture.Npm())) with
        {
            AvailableManagers = [manager],
            AvailableNodeRuntimes = [node],
            Selection = new() { Kind = ToolVersionSelectionKind.MinorRange, Range = new(2, 4) },
            AllowLatestLookup = true
        };
        var planned = await fixture.Service.PlanAsync(request, Token);
        Assert.NotNull(planned.Resolution);
        Assert.Same(node, planned.Resolution.PrivateNode);
        var execution = planned.Resolution.Execution;
        fixture.Processes.Run = command =>
        {
            if (command.Arguments.Contains("install"))
            {
                var entry = execution.Invocation.PrefixArguments[0];
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(entry) ?? throw new InvalidOperationException());
                File.WriteAllText(entry, "synthetic package entry");
                return ToolChannelFixture.Success("");
            }
            if (command.Arguments[0] == "--version") return ToolChannelFixture.Success(node.Version);
            if (command.Arguments[^1] == "--version" && command.Arguments[0].EndsWith("npm-cli.js", StringComparison.Ordinal)) return ToolChannelFixture.Success("10.0.0");
            return ToolChannelFixture.Success(planned.Resolution.Version);
        };
        var result = await fixture.Service.InstallAsync(new(planned.Resolution), Token);
        Assert.Null(result.Failure);
        Assert.NotNull(result.State);
        Assert.Equal(node.Version, result.State.PackageManager?.RuntimeVersion);
        Assert.Equal(ToolVerificationKind.SignedNodeChecksums, Assert.Single(result.Runtimes).Integrity.Kind);
        var install = Assert.Single(execution.Commands, command => command.Arguments.Contains("install"));
        Assert.Equal(execution.RuntimePath, install.ExecutablePath);
        Assert.Equal(System.IO.Path.GetDirectoryName(execution.RuntimePath), install.Environment[ToolPathPolicy.PathVariable]?.Split(System.IO.Path.PathSeparator)[0]);
        Assert.True(File.Exists(result.State.CommandPath));
        Assert.False(File.Exists(System.IO.Path.Combine(execution.BinPath, ToolPrivateNodeInstaller.WindowsNodeName)));
        Assert.Equal(0, fixture.Backups.Completed);
    }

    [Fact(DisplayName = "Latest resolution stays blocked when lookup permission isn't part of the preview")]
    public async Task BlocksUndisclosedLatestLookup()
    {
        using var fixture = new ToolChannelFixture();
        var result = await fixture.Service.PlanAsync(fixture.Request(fixture.Entry(fixture.Npm())) with
        { AvailableManagers = [fixture.Manager()], Selection = new() { Kind = ToolVersionSelectionKind.LatestSupported } }, Token);
        Assert.Null(result.Resolution);
        Assert.False(result.UsedNetwork);
        Assert.Empty(fixture.Processes.Requests);
        Assert.Empty(fixture.Downloads.MetadataRequests);
    }

    [Fact(DisplayName = "A fresh exact private npm plan is offline even without Node or any manager")]
    public async Task PlansExactPrivateWithoutClient()
    {
        using var fixture = new ToolChannelFixture();
        var result = await fixture.Service.PlanAsync(fixture.Request(fixture.Entry(fixture.Npm())) with
        { AvailableNodeRuntimes = [Node(fixture, ToolChannelFixture.RuntimeVersion)] }, Token);
        Assert.NotNull(result.Resolution);
        Assert.NotNull(result.Resolution.PrivateNode);
        Assert.False(result.UsedNetwork);
        Assert.Empty(fixture.Downloads.MetadataRequests);
        Assert.Empty(fixture.Processes.Requests);
        Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Home));
    }

    [Fact(DisplayName = "Updating a managed private package keeps private Node even when a global npm client is available")]
    public async Task KeepsPrivateNpmDuringUpdate()
    {
        using var fixture = new ToolChannelFixture();
        var node = Node(fixture, ToolChannelFixture.RuntimeVersion);
        var root = ToolSupplyPaths.Runtime(fixture.Home, node.Version);
        var runtime = ToolSupplyPaths.Under(root, node.Archive.EntryRelativePath);
        var npmEntry = ToolSupplyPaths.Under(root, node.NpmRelativePath);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(npmEntry) ?? throw new InvalidOperationException());
        File.WriteAllText(runtime, "synthetic");
        File.WriteAllText(npmEntry, "synthetic");
        var oldPrefix = ToolSupplyPaths.Tool(fixture.Home, ToolChannelFixture.Id, ToolChannelFixture.Version);
        Directory.CreateDirectory(oldPrefix);
        var manager = fixture.Manager() with
        {
            RuntimePath = runtime,
            GlobalBinPath = oldPrefix,
            Invocation = new() { ExecutablePath = runtime, PrefixArguments = [npmEntry] }
        };
        var state = fixture.State(System.IO.Path.Combine(ToolSupplyPaths.Bin(fixture.Home), "sample.cmd"), runtime) with
        { ChannelId = fixture.Npm().Id, ToolPath = oldPrefix, PackageManager = manager };
        fixture.Records.Value = fixture.Records.Value with { Runtimes = new Dictionary<string, ToolNodeRuntimeRecord> { [node.Version] = ToolPrivateNodeInstaller.Record(node, fixture.Home) } };
        var result = await fixture.Service.PlanAsync(fixture.Request(fixture.Entry(fixture.Npm())) with
        {
            Existing = new()
            {
                ToolId = state.ToolId,
                Version = state.Version,
                ChannelId = state.ChannelId,
                CommandPath = state.CommandPath,
                Invocation = state.Invocation,
                RuntimePath = runtime,
                RecordedState = state,
                PackageManager = manager
            },
            AvailableManagers = [fixture.Manager()],
            AvailableNodeRuntimes = [node],
            Selection = new() { Kind = ToolVersionSelectionKind.Exact, ExactVersion = "2.4.5" },
        }, Token);
        Assert.NotNull(result.Resolution);
        Assert.NotNull(result.Resolution.PrivateNode);
        Assert.Null(result.Resolution.PackageManager);
        Assert.Equal(ToolSupplyPaths.Tool(fixture.Home, ToolChannelFixture.Id, "2.4.5"), result.Resolution.Execution.ToolPath);
        Assert.Empty(fixture.Processes.Requests);
        Assert.Empty(result.Resolution.Execution.Downloads);
        Assert.Equal(runtime, Assert.Single(result.Resolution.Execution.Commands, command => command.Arguments.Contains("install")).ExecutablePath);
    }

    [Theory(DisplayName = "An unknown responsible npm client or changed global prefix blocks a second installation")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BlocksUnknownResponsibleManager(bool recordedManager)
    {
        using var fixture = new ToolChannelFixture();
        var manager = fixture.Manager();
        var state = fixture.State(System.IO.Path.Combine(fixture.Native, "sample.cmd")) with { ChannelId = fixture.Npm().Id };
        var result = await fixture.Service.PlanAsync(fixture.Request(fixture.Entry(fixture.Npm())) with
        {
            Existing = new()
            {
                ToolId = state.ToolId,
                Version = state.Version,
                ChannelId = state.ChannelId,
                CommandPath = state.CommandPath,
                Invocation = state.Invocation,
                PackageManager = recordedManager ? manager : null
            },
            AvailableManagers = [manager with { GlobalBinPath = fixture.Temporary.CreateDirectory("other-prefix") }],
            Selection = new() { Kind = ToolVersionSelectionKind.Exact, ExactVersion = "2.4.5" },
        }, Token);
        Assert.Null(result.Resolution);
        Assert.NotEmpty(result.Problems);
        Assert.Empty(fixture.Processes.Requests);
        Assert.Empty(fixture.Downloads.MetadataRequests);
    }

    [Fact(DisplayName = "No-client latest metadata selects the exact supported patch and an approved runtime within both bounds")]
    public async Task ResolvesLatestWithoutClient()
    {
        using var fixture = new ToolChannelFixture();
        var lower = Node(fixture, "22.1.0");
        var higher = Node(fixture, ToolChannelFixture.RuntimeVersion);
        var channel = fixture.Npm();
        fixture.Downloads.Metadata = request => new(
            [new("2.4.2", channel.EntryRelativePath, null), new("2.4.5", channel.EntryRelativePath, ">=20 <24"),
                new("2.4.6-rc.1", channel.EntryRelativePath, null), new("2.5.0", channel.EntryRelativePath, null)],
            new("https://registry.example.test/"), null);
        var result = await fixture.Service.PlanAsync(fixture.Request(fixture.Entry(channel)) with
        { AvailableNodeRuntimes = [lower, higher], Selection = new() { Kind = ToolVersionSelectionKind.LatestSupported }, AllowLatestLookup = true }, Token);
        Assert.NotNull(result.Resolution);
        Assert.Equal("2.4.5", result.Resolution.Version);
        Assert.Same(lower, result.Resolution.PrivateNode);
        Assert.True(result.UsedNetwork);
        Assert.Equal(channel, Assert.Single(fixture.Downloads.MetadataRequests).Channel);
        Assert.Empty(fixture.Processes.Requests);
        Assert.Equal(0, fixture.Downloads.Calls);
        Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Home));
    }

    [Fact(DisplayName = "A failed no-client metadata lookup stops without a GitHub fallback")]
    public async Task BlocksFailedMetadata()
    {
        using var fixture = new ToolChannelFixture();
        var asset = fixture.Archive("sample.exe");
        var release = new ToolGitHubReleaseChannel { Id = "release", Kind = ToolChannelKind.GitHubRelease, Repository = "example/sample", Requirements = new() { Targets = [] }, Assets = [asset] };
        fixture.Downloads.Metadata = _ => new([], null, "synthetic lookup failure");
        var result = await fixture.Service.PlanAsync(fixture.Request(fixture.Entry(fixture.Npm(), release)) with
        { AvailableNodeRuntimes = [Node(fixture, ToolChannelFixture.RuntimeVersion)], Selection = new() { Kind = ToolVersionSelectionKind.LatestSupported }, AllowLatestLookup = true }, Token);
        Assert.Null(result.Resolution);
        Assert.NotEmpty(result.Problems);
        Assert.True(result.UsedNetwork);
        Assert.Single(fixture.Downloads.MetadataRequests);
        Assert.Equal(0, fixture.Downloads.Calls);
        Assert.Empty(fixture.Processes.Requests);
    }

    [Theory(DisplayName = "Shared runtime reuse requires the same fresh signed receipt before installation")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RechecksSharedRuntime(bool changed)
    {
        using var fixture = new ToolChannelFixture();
        var node = Node(fixture, ToolChannelFixture.RuntimeVersion);
        var root = ToolSupplyPaths.Runtime(fixture.Home, node.Version);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(ToolSupplyPaths.Under(root, node.NpmRelativePath))!);
        File.WriteAllText(ToolSupplyPaths.Under(root, node.Archive.EntryRelativePath), "synthetic");
        File.WriteAllText(ToolSupplyPaths.Under(root, node.NpmRelativePath), "synthetic");
        fixture.Records.Value = fixture.Records.Value with { Runtimes = new Dictionary<string, ToolNodeRuntimeRecord> { [node.Version] = ToolPrivateNodeInstaller.Record(node, fixture.Home) } };
        fixture.Processes.Run = request => Metadata(fixture, request);
        var planned = await fixture.Service.PlanAsync(PrivateRequest(fixture, fixture.Entry(fixture.Npm()), node), Token);
        Assert.NotNull(planned.Resolution);
        Assert.Empty(planned.Resolution.Execution.Downloads);
        Assert.Empty(planned.Resolution.Execution.Extractions);
        if (changed) fixture.Records.Value = fixture.Records.Value with { Runtimes = new Dictionary<string, ToolNodeRuntimeRecord>() };
        fixture.Processes.Requests.Clear();
        fixture.Processes.Run = request => ExecutePrivate(planned.Resolution, request);
        var result = await fixture.Service.InstallAsync(new(planned.Resolution), Token);
        Assert.True(fixture.Records.Reads >= 2);
        Assert.Equal(0, fixture.Downloads.Calls);
        if (changed)
        {
            Assert.NotNull(result.Failure);
            Assert.Empty(fixture.Processes.Requests);
        }
        else
        {
            Assert.Null(result.Failure);
            Assert.Equal(node.Version, Assert.Single(result.Runtimes).Version);
        }
    }

    [Fact(DisplayName = "Two private Node versions coexist and each tool keeps its own npm and runtime")]
    public async Task InstallsMultipleRuntimeVersions()
    {
        using var fixture = new ToolChannelFixture();
        var states = new List<ToolMachineState>();
        foreach (var (id, version) in new[] { (ToolChannelFixture.Id, "22.1.0"), ("second", ToolChannelFixture.RuntimeVersion) })
        {
            var node = Node(fixture, version);
            var entry = fixture.Entry(fixture.Npm());
            entry = entry with { Identity = entry.Identity with { Id = id }, Detection = entry.Detection with { Command = id } };
            fixture.Processes.Run = request => request.Arguments.Contains("bin") ? ToolChannelFixture.Success($$$"""{"{{{id}}}":"entry.js"}""") : Metadata(fixture, request);
            var planned = await fixture.Service.PlanAsync(PrivateRequest(fixture, entry, node) with { Existing = new() { ToolId = id } }, Token);
            Assert.NotNull(planned.Resolution);
            fixture.Processes.Run = request => ExecutePrivate(planned.Resolution, request);
            var result = await fixture.Service.InstallAsync(new(planned.Resolution), Token);
            Assert.Null(result.Failure);
            Assert.NotNull(result.State);
            states.Add(result.State);
            var receipt = Assert.Single(result.Runtimes);
            fixture.Records.Value = fixture.Records.Value with { Runtimes = fixture.Records.Value.Runtimes.Append(new KeyValuePair<string, ToolNodeRuntimeRecord>(version, receipt)).ToDictionary() };
        }
        Assert.Equal(2, fixture.Records.Value.Runtimes.Count);
        Assert.NotEqual(states[0].RuntimePath, states[1].RuntimePath);
        Assert.All(states, state => Assert.True(File.Exists(state.Invocation.ExecutablePath)));
        Assert.All(states, state => Assert.Equal(state.RuntimePath, state.PackageManager?.Invocation.ExecutablePath));
    }

    private static ToolChannelRequest PrivateRequest(ToolChannelFixture fixture, ToolCatalogueEntry entry, ToolPrivateNodeSpec node)
        => fixture.Request(entry) with
        {
            AvailableManagers = [fixture.Manager(ToolNpmClient.Bun) with { RuntimePath = null, RuntimeVersion = null }],
            AvailableNodeRuntimes = [node],
            Selection = new() { Kind = ToolVersionSelectionKind.LatestSupported },
            AllowLatestLookup = true,
        };

    private static ToolPrivateNodeSpec Node(ToolChannelFixture fixture, string version) => new()
    {
        Version = version,
        NpmRelativePath = "node_modules/npm/bin/npm-cli.js",
        SignedChecksums = new($"https://{ToolArtifactSource.NodeHost}/dist/v{version}/SHASUMS256.txt.asc"),
        VerifiedSigner = "synthetic-signer",
        Archive = fixture.Archive("node.exe", "node_modules/npm/bin/npm-cli.js") with
        { Version = version, Url = new($"https://{ToolArtifactSource.NodeHost}/dist/v{version}/node.zip"), EntryRelativePath = "node.exe" },
    };

    private static ToolProcessResult ExecutePrivate(ToolChannelResolution plan, ToolProcessRequest request)
    {
        if (request.Arguments.Contains("install"))
        {
            var entry = plan.Execution.Invocation.PrefixArguments[0];
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(entry)!);
            File.WriteAllText(entry, "synthetic");
            return ToolChannelFixture.Success("");
        }
        if (request.Arguments[0] == "--version") return ToolChannelFixture.Success(plan.PrivateNode!.Version);
        if (request.Arguments[0].EndsWith("npm-cli.js", StringComparison.Ordinal)) return ToolChannelFixture.Success("10.0.0");
        return ToolChannelFixture.Success(plan.Version);
    }

    private static ToolProcessResult Metadata(ToolChannelFixture fixture, ToolProcessRequest request)
    {
        if (request.Arguments.Contains("versions")) return ToolChannelFixture.Success("[\"2.4.2\",\"2.4.5\",\"2.5.0\",\"2.4.6-rc.1\"]");
        if (request.Arguments.Contains("bin")) return ToolChannelFixture.Success("{\"sample\":\"entry.js\"}");
        return ToolChannelFixture.Success(System.IO.Path.Combine(fixture.Native, ToolNpmPackageReader.ModulesDirectory));
    }

    private static void WritePackage(string modules, string package)
    {
        var path = System.IO.Path.Combine(modules, package.Replace('/', System.IO.Path.DirectorySeparatorChar));
        Directory.CreateDirectory(path);
        File.WriteAllText(System.IO.Path.Combine(path, ToolNpmPackageReader.ManifestName), $$$"""{"name":"{{{package}}}","version":"{{{ToolChannelFixture.Version}}}","bin":{"sample":"entry.js"}}""");
        File.WriteAllText(System.IO.Path.Combine(path, "entry.js"), "synthetic");
    }
}
