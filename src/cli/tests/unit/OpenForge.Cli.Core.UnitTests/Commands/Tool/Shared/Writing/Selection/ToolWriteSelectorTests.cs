using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Selection;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Selection;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Selection;

[Trait("Feature", "tool"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class ToolWriteSelectorTests
{
    private const string FirstId = "alpha-tool";
    private const string SecondId = "zeta-tool";
    private const string ChannelId = "fixture-npm";
    private const string LockedVersion = "2.4.7";
    private static readonly ToolVersionRange DeclaredRange = new(2, 4);

    [Fact(DisplayName = "Bare Tool Install restores the exact locked version without forcing its recorded channel")]
    public void InstallRestoresExactLock()
    {
        var records = Records(locked: true);
        var result = Select(ToolWriteSelectionKind.Install, records);

        var tool = Assert.Single(result.Tools);
        Assert.Empty(result.Issues);
        Assert.Equal(ToolVersionSelectionKind.Exact, tool.Version.Kind);
        Assert.Equal(records.Lock[FirstId].Version, tool.Version.ExactVersion);
        Assert.Null(tool.RequiredChannel);
        Assert.Same(records.Lock[FirstId], tool.Lock);
        Assert.Same(records.Settings[FirstId], tool.Declaration);
    }

    [Fact(DisplayName = "Bare Tool Install without a lock resolves latest within the declaration")]
    public void InstallUsesDeclaredRangeWithoutLock()
    {
        var tool = Assert.Single(Select(ToolWriteSelectionKind.Install, Records()).Tools);

        Assert.Equal(ToolVersionSelectionKind.MinorRange, tool.Version.Kind);
        Assert.Equal(DeclaredRange, tool.Version.Range);
        Assert.Null(tool.RequiredChannel);
        Assert.Null(tool.Lock);
    }

    [Fact(DisplayName = "Named bare Tool Install selects latest despite an older lock")]
    public void NamedInstallSelectsLatest()
    {
        var version = new ToolVersionSelection { Kind = ToolVersionSelectionKind.LatestSupported };
        var tool = Assert.Single(Select(ToolWriteSelectionKind.Install, Records(locked: true), new(FirstId, version)).Tools);

        Assert.Same(version, tool.Version);
        Assert.Null(tool.RequiredChannel);
    }

    [Theory(DisplayName = "Named Tool writers keep the version selection parsed by the version service")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void NamedWriterKeepsParsedSelection(bool exact, bool update)
    {
        var selection = exact ? ToolVersionSelectionKind.Exact : ToolVersionSelectionKind.MinorRange;
        var version = new ToolVersionSelection
        {
            Kind = selection,
            Range = selection == ToolVersionSelectionKind.MinorRange ? DeclaredRange : null,
            ExactVersion = selection == ToolVersionSelectionKind.Exact ? LockedVersion : null,
        };
        var result = Select(update ? ToolWriteSelectionKind.Update : ToolWriteSelectionKind.Install,
            Records(locked: true), new(FirstId, version));

        Assert.Same(version, Assert.Single(result.Tools).Version);
        Assert.Empty(result.Issues);
    }

    [Fact(DisplayName = "Bare Tool Update selects newest support beyond the old declaration")]
    public void UpdateSelectsLatest()
    {
        var tool = Assert.Single(Select(ToolWriteSelectionKind.Update, Records(locked: true)).Tools);

        Assert.Equal(ToolVersionSelectionKind.LatestSupported, tool.Version.Kind);
        Assert.Null(tool.Version.Range);
        Assert.Null(tool.Version.ExactVersion);
        Assert.Null(tool.RequiredChannel);
        Assert.NotNull(tool.Lock);
    }

    [Fact(DisplayName = "Bare Tool writers select declared IDs in ordinal order")]
    public void BareWritersOrderDeclarations()
    {
        var records = new ToolPortableRecords
        {
            Settings = new Dictionary<string, ToolSetting>
            {
                [SecondId] = new(DeclaredRange),
                [FirstId] = new(DeclaredRange),
            },
            Lock = new Dictionary<string, ToolLockRecord>(),
        };

        foreach (var kind in Enum.GetValues<ToolWriteSelectionKind>())
        {
            Assert.Equal([FirstId, SecondId], Select(kind, records).Tools.Select(tool => tool.Entry.Identity.Id));
        }
    }

    [Fact(DisplayName = "Bare Tool writers ignore lock-only entries and empty declarations are a no-op")]
    public void BareWritersIgnoreLockOnlyEntries()
    {
        var records = Records(locked: true) with { Settings = new Dictionary<string, ToolSetting>() };

        foreach (var kind in Enum.GetValues<ToolWriteSelectionKind>())
        {
            var result = Select(kind, records);
            Assert.Empty(result.Tools);
            Assert.Empty(result.Issues);
        }
    }

    [Fact(DisplayName = "Named Tool Update accepts a lock-only tool")]
    public void NamedUpdateAcceptsLockOnly()
    {
        var records = Records(locked: true) with { Settings = new Dictionary<string, ToolSetting>() };
        var result = Select(ToolWriteSelectionKind.Update, records, Latest());

        Assert.Single(result.Tools);
        Assert.Empty(result.Issues);
    }

    [Fact(DisplayName = "Named Tool Install accepts a catalogue tool without existing records")]
    public void NamedInstallAcceptsFreshTool()
    {
        var records = Records() with { Settings = new Dictionary<string, ToolSetting>() };
        var result = Select(ToolWriteSelectionKind.Install, records, Latest());

        Assert.Single(result.Tools);
        Assert.Empty(result.Issues);
    }

    [Fact(DisplayName = "Named Tool Update requires a declaration or lock")]
    public void NamedUpdateRequiresRecord()
    {
        var records = Records() with { Settings = new Dictionary<string, ToolSetting>() };
        var result = Select(ToolWriteSelectionKind.Update, records, Latest());

        Assert.Empty(result.Tools);
        Assert.Equal(ToolWriteSelectionIssueCode.UpdateRequiresRecord, Assert.Single(result.Issues).Code);
    }

    [Fact(DisplayName = "One unknown declaration prevents selecting any tools in a batch")]
    public void UnknownDeclarationPreventsPartialSelection()
    {
        var records = Records() with
        {
            Settings = new Dictionary<string, ToolSetting>
            {
                [FirstId] = new(DeclaredRange),
                ["unknown-fixture"] = new(DeclaredRange),
            },
        };
        var result = Select(ToolWriteSelectionKind.Install, records);

        Assert.Empty(result.Tools);
        Assert.Equal(ToolWriteSelectionIssueCode.UnknownId, Assert.Single(result.Issues).Code);
    }

    [Fact(DisplayName = "Tool channel override requires a named spec")]
    public void ChannelRequiresName()
    {
        var result = Select(ToolWriteSelectionKind.Install, Records(), channel: ChannelId);

        Assert.Empty(result.Tools);
        Assert.Equal(ToolWriteSelectionIssueCode.NamedSpecRequired, Assert.Single(result.Issues).Code);
    }

    [Fact(DisplayName = "Named Tool writers accept only advertised channel IDs")]
    public void NamedChannelValidation()
    {
        var valid = Select(ToolWriteSelectionKind.Install, Records(), Latest(), ChannelId);
        var invalid = Select(ToolWriteSelectionKind.Install, Records(), Latest(), "unpublished-fixture");

        Assert.Equal(ChannelId, Assert.Single(valid.Tools).RequiredChannel);
        Assert.Empty(invalid.Tools);
        Assert.Equal(ToolWriteSelectionIssueCode.UnknownChannel, Assert.Single(invalid.Issues).Code);
    }

    [Fact(DisplayName = "Tool selection rejects an undefined writer kind")]
    public void RejectsUndefinedKind()
        => Assert.Throws<ArgumentOutOfRangeException>(() => Select((ToolWriteSelectionKind)int.MaxValue, Records()));

    private static ToolSpec Latest()
        => new(FirstId, new ToolVersionSelection { Kind = ToolVersionSelectionKind.LatestSupported });

    private static ToolWriteSelectionResult Select(
        ToolWriteSelectionKind kind,
        ToolPortableRecords records,
        ToolSpec? spec = null,
        string? channel = null)
        => new ToolWriteSelector(new ToolCatalogue
        {
            Entries = new Dictionary<string, ToolCatalogueEntry>
            {
                [FirstId] = Entry(FirstId),
                [SecondId] = Entry(SecondId),
            },
            NodeRuntimes = [],
        }).Select(new() { Kind = kind, Spec = spec, ChannelOverride = channel, Records = records });

    private static ToolPortableRecords Records(bool locked = false)
        => new()
        {
            Settings = new Dictionary<string, ToolSetting> { [FirstId] = new(DeclaredRange) },
            Lock = locked
                ? new Dictionary<string, ToolLockRecord>
                {
                    [FirstId] = new() { Version = LockedVersion, Channel = ChannelId, Changes = [] },
                }
                : new Dictionary<string, ToolLockRecord>(),
        };

    private static ToolCatalogueEntry Entry(string id)
        => new()
        {
            Identity = new()
            {
                Id = id,
                Name = id,
                Origin = ToolOrigin.ThirdParty,
                Upstream = new("https://example.invalid/fixture"),
                Licence = "fixture",
            },
            Channels =
            [
                new ToolNpmChannel
                {
                    EntryRelativePath = "synthetic-entry.js",
                    Id = ChannelId,
                    Kind = ToolChannelKind.Npm,
                    Requirements = new() { Targets = [] },
                    Package = id,
                    MinimumClientVersions = new Dictionary<ToolNpmClient, string>(),
                },
            ],
            Detection = new()
            {
                Command = id,
                Arguments = [],
                Timeout = TimeSpan.FromSeconds(1),
                MaximumOutputBytes = 1024,
                Environment = new Dictionary<string, string?>(),
                Parser = ToolVersionParser.StableSemVer,
            },
            Profiles = [],
        };
}
