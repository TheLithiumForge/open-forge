using OpenForge.Cli.Composition.Shared.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Shared.PackageManagers;
using OpenForge.Cli.Core.Framework.Tools.Shared.Paths;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Composition.Shared.Tools;

[Trait("Feature", "tool-command-resolution"), Trait("Evidence", "Integration")]
public sealed class ToolCommandResolverTests
{
    [Fact(DisplayName = "Command resolution uses the supplied PATH and returns absolute native invocation paths")]
    public void ResolvesNativeCommand()
    {
        using var temporary = TemporaryWorkspace.Create("tool-resolver");
        var name = OperatingSystem.IsWindows() ? "sample.exe" : "sample";
        var executable = temporary.CreateFile(name, "synthetic");
        if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(executable, UnixFileMode.UserRead | UnixFileMode.UserExecute);
        var result = new ToolCommandResolver(new Dictionary<string, string?> { [ToolPathPolicy.PathVariable] = temporary.Path }).Resolve("sample", temporary.Path);
        Assert.NotNull(result);
        Assert.Equal(executable, result.CommandPath);
        Assert.Equal(executable, result.Invocation.ExecutablePath);
        Assert.Empty(result.Invocation.PrefixArguments);
    }

    [Fact(DisplayName = "Npm cmd shims resolve through package bin metadata and a real Node executable")]
    public void ResolvesNpmShim()
    {
        using var temporary = TemporaryWorkspace.Create("tool-resolver");
        var node = temporary.CreateFile("node.exe", "synthetic");
        var command = temporary.CreateFile("npm.cmd", "synthetic");
        var package = temporary.CreateDirectory(System.IO.Path.Combine(ToolNpmPackageReader.ModulesDirectory, "npm"));
        temporary.CreateFile(System.IO.Path.GetRelativePath(temporary.Path, System.IO.Path.Combine(package, ToolNpmPackageReader.ManifestName)), """{"name":"npm","version":"10.0.0","bin":{"npm":"bin/npm-cli.js"}}""");
        var entry = temporary.CreateFile(System.IO.Path.GetRelativePath(temporary.Path, System.IO.Path.Combine(package, "bin", "npm-cli.js")), "synthetic");
        var result = new ToolCommandResolver(new Dictionary<string, string?>()).Resolve(command, temporary.Path);
        Assert.NotNull(result);
        Assert.Equal(node, result.Invocation.ExecutablePath);
        Assert.Equal(entry, Assert.Single(result.Invocation.PrefixArguments));
    }

    [Fact(DisplayName = "Unrecognized cmd files are reported as unsupported instead of disappearing from detection")]
    public void RejectsUnknownShellShim()
    {
        using var temporary = TemporaryWorkspace.Create("tool-resolver");
        temporary.CreateFile("node.exe", "synthetic");
        var command = temporary.CreateFile("sample.cmd", "synthetic");
        Assert.Throws<NotSupportedException>(() => new ToolCommandResolver(new Dictionary<string, string?>()).Resolve(command, temporary.Path));
    }

    [Fact(DisplayName = "Declared bundled runtime flags precede its real application entry during resolution")]
    public void ResolvesDeclaredBundle()
    {
        using var temporary = TemporaryWorkspace.Create("tool-resolver");
        var command = temporary.CreateFile(System.IO.Path.Combine("bundle", "bin", "sample.cmd"), "synthetic");
        var node = temporary.CreateFile(System.IO.Path.Combine("bundle", "node.exe"), "synthetic");
        var script = temporary.CreateFile(System.IO.Path.Combine("bundle", "lib", "app.js"), "synthetic");
        var entry = new ToolCatalogueEntry
        {
            Identity = new() { Id = "sample", Name = "Sample", Origin = ToolOrigin.ThirdParty, Upstream = new("https://github.com/example/sample"), Licence = "MIT" },
            Profiles = [],
            Detection = new() { Command = "sample", Arguments = [], Environment = new Dictionary<string, string?>(), Parser = ToolVersionParser.StableSemVer, Timeout = TimeSpan.FromSeconds(1), MaximumOutputBytes = 4096 },
            Channels = [new ToolGitHubReleaseChannel { Id = "release", Kind = ToolChannelKind.GitHubRelease, Repository = "example/sample", Requirements = new() { Targets = [] },
                Assets = [new() { Version = "2.4.3", Target = new() { OperatingSystem = ToolOperatingSystem.Windows, Cpu = ToolCpu.X64 }, Url = new("https://github.com/example/sample/releases/download/v2.4.3/sample.zip"),
                    Archive = ToolArchiveKind.Zip, StripComponents = 0, MaximumBytes = 100, EntryRelativePath = "bin/sample.cmd",
                    BundledRuntime = new() { Runtime = "node", Version = "24.16.0", RuntimeRelativePath = "node.exe", EntryRelativePath = "lib/app.js", Arguments = ["--liftoff-only"] } }] }],
        };
        var catalogue = new ToolCatalogue { Entries = new Dictionary<string, ToolCatalogueEntry> { [entry.Identity.Id] = entry }, NodeRuntimes = [] };
        var result = new ToolCommandResolver(new Dictionary<string, string?>(), catalogue).Resolve(command, temporary.Path);
        Assert.NotNull(result);
        Assert.Equal(node, result.Invocation.ExecutablePath);
        Assert.Equal(new[] { "--liftoff-only", script }, result.Invocation.PrefixArguments);
        entry = entry with { Identity = entry.Identity with { Id = ToolCommandResolver.CodeGraphId } };
        catalogue = catalogue with { Entries = new Dictionary<string, ToolCatalogueEntry> { [entry.Identity.Id] = entry } };
        Assert.True(new ToolCommandResolver(new Dictionary<string, string?>(), catalogue).IsCodeGraphBundle(new()
        {
            ExecutablePath = node,
            Arguments = ["--liftoff-only", script, "--version"],
            WorkingDirectory = temporary.Path,
            Environment = new Dictionary<string, string?>(),
            Timeout = TimeSpan.FromSeconds(1),
            MaximumOutputBytes = 4096,
        }));
    }
}
