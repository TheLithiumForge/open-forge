using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.PackageManagers;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.PackageManagers;

[Trait("Feature", "tool-package-managers"), Trait("Evidence", "Unit")]
public sealed class ToolPackageManagerPolicyTests
{
    [Theory(DisplayName = "Global manager candidates enforce each client's minimum and Node floor")]
    [InlineData("valid", true)]
    [InlineData("old-client", false)]
    [InlineData("old-node", false)]
    [InlineData("no-node", false)]
    [InlineData("required-client", false)]
    [InlineData("modern-yarn", false)]
    [InlineData("no-globals", false)]
    public void ChecksRequirements(string scenario, bool usable)
    {
        var channel = Channel();
        var manager = Manager(ToolNpmClient.Npm);
        manager = scenario switch
        {
            "valid" or "required-client" => manager,
            "old-client" => manager with { Version = "1.9.9" },
            "old-node" => manager with { RuntimeVersion = "19.9.0" },
            "no-node" => manager with { RuntimePath = null },
            "modern-yarn" => manager with { Client = ToolNpmClient.YarnModern },
            "no-globals" => manager with { SupportsGlobalInstall = false },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        if (scenario == "required-client") channel = channel with { RequiredClient = ToolNpmClient.Pnpm };
        Assert.Equal(usable, ToolPackageManagerPolicy.IsUsable(channel, manager));
    }

    [Theory(DisplayName = "Manager choice uses lockfile preference, live choice and unattended ordering")]
    [InlineData("unattended", ToolNpmClient.Npm)]
    [InlineData("preferred", ToolNpmClient.Pnpm)]
    [InlineData("ambiguous", null)]
    [InlineData("chosen", ToolNpmClient.Bun)]
    [InlineData("multiple-locks", null)]
    public void ChoosesManager(string scenario, object? expected)
    {
        var candidates = ToolPackageManagerPolicy.Candidates(Channel(), [Manager(ToolNpmClient.Bun), Manager(ToolNpmClient.Pnpm), Manager(ToolNpmClient.Npm)]);
        var request = Request() with
        {
            ManagerChoice = scenario == "unattended" ? ToolManagerChoiceMode.Default : ToolManagerChoiceMode.Ask,
            ProjectManagers = scenario switch { "preferred" => [ToolNpmClient.Pnpm], "multiple-locks" => [ToolNpmClient.Npm, ToolNpmClient.Pnpm], _ => [] },
            ChosenManager = scenario == "chosen" ? ToolNpmClient.Bun : null,
        };
        Assert.Equal(expected, ToolPackageManagerPolicy.Default(request, candidates.Reverse().ToArray()));
    }

    [Fact(DisplayName = "Global bin readiness stays separate from a manager's installation capability")]
    public void KeepsBinReadiness()
    {
        var candidates = ToolPackageManagerPolicy.Candidates(Channel(), [Manager(ToolNpmClient.Npm) with { GlobalBinReadiness = ToolPathReadiness.Missing }]);
        Assert.Equal(ToolPathReadiness.Missing, Assert.Single(candidates).GlobalBinReadiness);
    }

    [Theory(DisplayName = "Absent client minimums are unrestricted while runtime ceilings are enforced")]
    [InlineData("23.9.9", true)]
    [InlineData("24.0.0", false)]
    public void EnforcesCeilingWithoutInventingClientMinimum(string node, bool expected)
    {
        var channel = Channel() with { MinimumClientVersions = new Dictionary<ToolNpmClient, string>(), Requirements = new() { Targets = [], Runtime = new("node", ">=20 <24") } };
        Assert.Equal(expected, ToolPackageManagerPolicy.IsUsable(channel, Manager(ToolNpmClient.Npm) with { Version = "1.0.0", RuntimeVersion = node }));
    }

    [Fact(DisplayName = "Npm-family command mappings cover every supported client and reject unknown kinds")]
    public void CoversClients()
    {
        Assert.Equal(Enum.GetValues<ToolNpmClient>().Where(client => client != ToolNpmClient.YarnModern), ToolPackageManagerPolicy.ClientOrder.Order());
        foreach (var client in ToolPackageManagerPolicy.ClientOrder)
        {
            Assert.NotEmpty(ToolPackageManagerPolicy.GlobalBinArguments(client));
            var arguments = ToolPackageManagerPolicy.InstallArguments(client, "@example/sample", "2.4.3");
            Assert.Equal("@example/sample@2.4.3", arguments[^1]);
        }
        Assert.Throws<ArgumentException>(() => ToolPackageManagerPolicy.InstallArguments(ToolNpmClient.YarnModern, "sample", "2.4.3"));
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolPackageManagerPolicy.GlobalBinArguments((ToolNpmClient)int.MaxValue));
    }

    private static ToolPackageManagerFacts Manager(ToolNpmClient client) => new()
    {
        Client = client,
        Version = "2.0.0",
        RuntimePath = System.IO.Path.Combine(AppContext.BaseDirectory, "node"),
        RuntimeVersion = "20.0.0",
        Invocation = new() { ExecutablePath = System.IO.Path.Combine(AppContext.BaseDirectory, "manager"), PrefixArguments = [] },
        SupportsGlobalInstall = true,
        GlobalBinReadiness = ToolPathReadiness.Ready,
    };

    private static ToolNpmChannel Channel() => new()
    {
        Id = "npm",
        Kind = ToolChannelKind.Npm,
        Package = "@example/sample",
        EntryRelativePath = "entry.js",
        MinimumClientVersions = ToolPackageManagerPolicy.ClientOrder.ToDictionary(client => client, _ => "2.0.0"),
        Requirements = new() { Targets = [], Runtime = new("node", ">=20") },
    };

    private static ToolChannelRequest Request() => new()
    {
        Entry = new()
        {
            Identity = new() { Id = "sample", Name = "Sample", Licence = "MIT", Origin = ToolOrigin.ThirdParty, Upstream = new("https://example.com") },
            Channels = [Channel()],
            Profiles = [],
            Detection = new()
            {
                Command = "sample",
                Arguments = [],
                Environment = new Dictionary<string, string?>(),
                Parser = ToolVersionParser.StableSemVer,
                Timeout = TimeSpan.FromSeconds(1),
                MaximumOutputBytes = 100
            },
        },
        WorkspacePath = AppContext.BaseDirectory,
        HomePath = AppContext.BaseDirectory,
        Selection = new() { Kind = ToolVersionSelectionKind.LatestSupported },
        Existing = new() { ToolId = "sample" },
        Target = new() { OperatingSystem = ToolOperatingSystem.Windows, Cpu = ToolCpu.X64 },
        ProjectManagers = [],
        AvailableManagers = [],
        AvailableNodeRuntimes = [],
        ManagerChoice = ToolManagerChoiceMode.Ask,
        AllowLatestLookup = false,
    };
}
