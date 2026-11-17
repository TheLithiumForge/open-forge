using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Shared.PackageManagers;
using OpenForge.Cli.Core.Framework.Tools.Shared.Paths;
using OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Supply;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.PackageManagers;

[Trait("Feature", "tool-package-manager-detection"), Trait("Evidence", "Integration")]
public sealed class ToolPackageManagerDetectorTests
{
    [Theory(DisplayName = "Corepack read probes preserve the project and never bootstrap a missing cached client")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task KeepsCorepackReadsOffline(bool cached)
    {
        using var fixture = new ToolChannelFixture();
        var sentinel = System.IO.Path.Combine(fixture.Workspace, ToolNpmPackageReader.ManifestName);
        var before = """{"name":"synthetic-project"}"""u8.ToArray();
        File.WriteAllBytes(sentinel, before);
        var shim = System.IO.Path.Combine(fixture.Native, "corepack-like-shim");
        fixture.Processes.Commands["pnpm"] = new(shim, new() { ExecutablePath = shim, PrefixArguments = [] });
        var inherited = ToolPackageManagerPolicy.ReadEnvironment.Keys.ToDictionary(key => key, _ => (string?)"1");
        var mutated = false;
        fixture.Processes.Run = request =>
        {
            if (!ToolPackageManagerPolicy.ReadEnvironment.All(pair => request.Environment.GetValueOrDefault(pair.Key) == pair.Value))
            { mutated = true; File.WriteAllBytes(sentinel, []); }
            if (!cached) return ToolChannelFixture.Success() with { ExitCode = 1 };
            return ToolChannelFixture.Success(request.Arguments.SequenceEqual(ToolPackageManagerPolicy.VersionArguments) ? "10.0.0" : fixture.Native);
        };
        var result = await new ToolPackageManagerDetector(fixture.Processes, inherited).DetectAsync(fixture.Workspace, TestContext.Current.CancellationToken);
        Assert.False(mutated);
        Assert.Equal(before, File.ReadAllBytes(sentinel));
        Assert.All(inherited.Values, value => Assert.Equal("1", value));
        Assert.Equal(cached ? 1 : 0, result.Count);
        Assert.Equal(cached ? 2 : 1, fixture.Processes.Requests.Count);
        Assert.All(fixture.Processes.Requests, request => Assert.All(ToolPackageManagerPolicy.ReadEnvironment,
            pair => Assert.Equal(pair.Value, request.Environment[pair.Key])));
    }
    [Fact(DisplayName = "Four npm-family clients have separate bounded version and global-bin probes")]
    public async Task ProbesEachClient()
    {
        using var fixture = new ToolChannelFixture();
        var names = new[] { "node", "npm", "pnpm", "yarn", "bun" };
        foreach (var name in names)
        {
            var executable = System.IO.Path.Combine(fixture.Native, name);
            fixture.Processes.Commands[name] = new(executable, new() { ExecutablePath = executable, PrefixArguments = [] });
        }
        fixture.Processes.Run = request =>
        {
            var name = System.IO.Path.GetFileName(request.ExecutablePath);
            if (request.Arguments.SequenceEqual(ToolPackageManagerPolicy.VersionArguments))
                return ToolChannelFixture.Success(name switch { "node" => ToolChannelFixture.RuntimeVersion, "yarn" => "1.22.22", _ => "10.0.0" });
            return ToolChannelFixture.Success(fixture.Native);
        };
        var environment = new Dictionary<string, string?>
        {
            [ToolPathPolicy.PathVariable] = string.Join(System.IO.Path.PathSeparator,
            fixture.Native, System.IO.Path.Combine(fixture.Native, OpenForge.Cli.Core.Framework.Tools.Models.Records.ToolRecordDefinitions.BinDirectoryName))
        };
        var result = await new ToolPackageManagerDetector(fixture.Processes, environment).DetectAsync(fixture.Workspace, TestContext.Current.CancellationToken);
        Assert.Equal(4, result.Count);
        Assert.Equal(ToolPackageManagerPolicy.ClientOrder, result.Select(manager => manager.Client));
        Assert.All(result, manager => { Assert.True(manager.SupportsGlobalInstall); Assert.Equal(ToolPathReadiness.Ready, manager.GlobalBinReadiness); });
        foreach (var name in names) Assert.Single(fixture.Processes.Requests, request => System.IO.Path.GetFileName(request.ExecutablePath) == name
            && request.Arguments.SequenceEqual(ToolPackageManagerPolicy.VersionArguments));
        Assert.All(fixture.Processes.Requests, request => { Assert.True(request.Timeout > TimeSpan.Zero); Assert.True(request.MaximumOutputBytes > 0); });
    }

    [Fact(DisplayName = "Yarn Modern is detected but never probed as a global-install client")]
    public async Task ExcludesModernYarnGlobals()
    {
        using var fixture = new ToolChannelFixture();
        var executable = System.IO.Path.Combine(fixture.Native, "yarn");
        fixture.Processes.Commands["yarn"] = new(executable, new() { ExecutablePath = executable, PrefixArguments = [] });
        fixture.Processes.Run = _ => ToolChannelFixture.Success("4.1.0");
        var result = await new ToolPackageManagerDetector(fixture.Processes, new Dictionary<string, string?>()).DetectAsync(fixture.Workspace, TestContext.Current.CancellationToken);
        var yarn = Assert.Single(result);
        Assert.Equal(ToolNpmClient.YarnModern, yarn.Client);
        Assert.False(yarn.SupportsGlobalInstall);
        Assert.Equal(ToolPathReadiness.Unavailable, yarn.GlobalBinReadiness);
        Assert.Single(fixture.Processes.Requests);
    }

    [Fact(DisplayName = "Each client records its actual interpreter when a shim uses a different Node from PATH")]
    public async Task ProbesClientInterpreter()
    {
        using var fixture = new ToolChannelFixture();
        var globalNode = System.IO.Path.Combine(fixture.Native, "node.exe");
        var clientNode = System.IO.Path.Combine(fixture.Native, "client-node.exe");
        var script = System.IO.Path.Combine(fixture.Native, "npm-cli.js");
        File.WriteAllText(script, "synthetic");
        File.WriteAllText(System.IO.Path.Combine(fixture.Native, ToolNpmPackageReader.ManifestName), """{"engines":{"node":">=22 <23"}}""");
        fixture.Processes.Commands["node"] = new(globalNode, new() { ExecutablePath = globalNode, PrefixArguments = [] });
        fixture.Processes.Commands["npm"] = new(System.IO.Path.Combine(fixture.Native, "npm.cmd"), new() { ExecutablePath = clientNode, PrefixArguments = [script] });
        fixture.Processes.Run = request => request.Arguments[^1] == "--version"
            ? ToolChannelFixture.Success(request.Arguments.Count > 1 ? "10.0.0" : request.ExecutablePath == clientNode ? "22.1.0" : ToolChannelFixture.RuntimeVersion)
            : ToolChannelFixture.Success(fixture.Native);
        var result = await new ToolPackageManagerDetector(fixture.Processes, new Dictionary<string, string?>()).DetectAsync(fixture.Workspace, TestContext.Current.CancellationToken);
        var npm = Assert.Single(result);
        Assert.Equal(clientNode, npm.RuntimePath);
        Assert.Equal("22.1.0", npm.RuntimeVersion);
        Assert.True(npm.SupportsGlobalInstall);
    }
}
