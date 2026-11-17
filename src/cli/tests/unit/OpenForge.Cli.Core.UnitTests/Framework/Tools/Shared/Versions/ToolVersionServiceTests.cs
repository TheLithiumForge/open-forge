using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Shared.Versions;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Versions;

[Trait("Feature", "tool-versions"), Trait("Evidence", "Unit")]
public sealed class ToolVersionServiceTests
{
    [Theory(DisplayName = "Tool specs distinguish latest, minor and exact stable versions")]
    [InlineData("sample", ToolVersionSelectionKind.LatestSupported)]
    [InlineData("sample@2.4", ToolVersionSelectionKind.MinorRange)]
    [InlineData("sample@2.4.3", ToolVersionSelectionKind.Exact)]
    [InlineData("sample@2.4.3+build.7", ToolVersionSelectionKind.Exact)]
    public void ParsesSpecs(string input, object kind)
    {
        var result = new ToolVersionService().ReadSpec(input);
        Assert.NotNull(result);
        Assert.Equal(kind, result.Version.Kind);
    }

    [Theory(DisplayName = "Tool CLI specs reject unsupported selectors and malformed versions")]
    [InlineData("")]
    [InlineData("sample@")]
    [InlineData("sample@2.4.x")]
    [InlineData("sample@latest")]
    [InlineData("sample@^2.4.0")]
    [InlineData("sample@2.4.0-rc.1")]
    [InlineData("sample@02.4.0")]
    [InlineData("sample@2.4.0+")]
    [InlineData("sample@2.4.0+build..x")]
    [InlineData("sample@2.4.0@other")]
    [InlineData("./sample")]
    public void RejectsSpecs(string input) => Assert.Null(new ToolVersionService().ReadSpec(input));

    [Theory(DisplayName = "Settings ranges use minor.x independently of CLI minor selectors")]
    [InlineData("2.4.x", true)]
    [InlineData("2.4", false)]
    [InlineData("2.x", false)]
    [InlineData("2.4.0", false)]
    [InlineData("2.04.x", false)]
    public void ReadsSettingsRange(string input, bool valid) => Assert.Equal(valid, new ToolVersionService().ReadRange(input) is not null);

    [Theory(DisplayName = "Stable version matching respects minor selection and exact pins")]
    [InlineData("sample@2.4", "2.4.9", true)]
    [InlineData("sample@2.4", "2.5.0", false)]
    [InlineData("sample@2.4.3", "2.4.4", false)]
    [InlineData("sample@2.4.3", "2.4.3", true)]
    [InlineData("sample", "2.4.3-rc.1", false)]
    public void MatchesSelection(string spec, string version, bool matches)
    {
        var service = new ToolVersionService();
        var parsed = service.ReadSpec(spec);
        Assert.NotNull(parsed);
        Assert.Equal(matches, service.Satisfies(version, parsed.Version));
    }

    [Theory(DisplayName = "Profiles reject versions below their reviewed floor")]
    [InlineData("2.4.2", false)]
    [InlineData("2.4.3", true)]
    [InlineData("2.4.9", true)]
    [InlineData("2.5.0", false)]
    [InlineData("2.4.3-rc.1", false)]
    public void MatchesProfile(string version, bool matches)
    {
        var setup = new ToolSetupSpec { Arguments = [], Environment = new Dictionary<string, string?>(), Effects = [], Timeout = TimeSpan.FromSeconds(1), MaximumOutputBytes = 4096, PossibleOutputs = [] };
        var entry = new ToolCatalogueEntry
        {
            Identity = new() { Id = "sample", Name = "Sample", Licence = "MIT", Origin = ToolOrigin.ThirdParty, Upstream = new("https://example.com") },
            Channels = [],
            Detection = new()
            {
                Command = "sample",
                Arguments = [],
                Environment = new Dictionary<string, string?>(),
                Parser = ToolVersionParser.StableSemVer,
                Timeout = TimeSpan.FromSeconds(1),
                MaximumOutputBytes = 100
            },
            Profiles = [new() { Id = "supported", Range = new(2, 4), MinimumToolVersion = "2.4.3", MinimumOpenForgeVersion = "1.0.0", TestedVersions = ["2.4.3"],
                NativeHomePolicy = ToolNativeHomePolicy.Node, Setup = setup, Uninstall = setup, Changes = [] }],
        };
        Assert.Equal(matches, ToolVersionService.MatchProfile(entry, version) is not null);
    }

    [Theory(DisplayName = "Version probes accept one unambiguous stable version")]
    [InlineData("sample v2.4.3\n", "2.4.3")]
    [InlineData("2.4.3-rc.1", null)]
    [InlineData("2.4.3 2.5.0", null)]
    public void ReadsProbe(string output, string? expected) => Assert.Equal(expected, ToolVersionService.ReadProbe(output));

    [Fact(DisplayName = "Unknown version selection kinds are rejected")]
    public void RejectsUndefinedKind() => Assert.Throws<ArgumentOutOfRangeException>(() => new ToolVersionService().Satisfies("1.0.0", new() { Kind = (ToolVersionSelectionKind)int.MaxValue }));
}
