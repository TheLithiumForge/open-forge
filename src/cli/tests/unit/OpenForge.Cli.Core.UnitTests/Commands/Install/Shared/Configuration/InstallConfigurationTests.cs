using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Shared.Binding;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install.Shared.Configuration;

public sealed class InstallConfigurationTests
{
    [Fact(DisplayName = "Essentials resolves ordinary Working as Git-ignored and omits four defaults")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void EssentialsHasExactConcreteChoices()
    {
        var rows = InstallConfigurationChoices.Defaults(InstallPreset.Essentials);
        Assert.Equal(10, rows.Length);
        Assert.Equal(InstallRouteAction.GitIgnore, rows.Single(row => row.Id == "memory/working").Action);
        Assert.Equal(["guidance", "maps", "templates", "memory/archived"], rows.Where(row => row.Action == InstallRouteAction.Remove).Select(row => row.Id));
        Assert.All(InstallConfigurationChoices.Defaults(InstallPreset.FullCore), row => Assert.Equal(InstallRouteAction.Add, row.Action));
    }

    [Fact(DisplayName = "Every named preset and action maps and undefined values are rejected"), Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void FiniteMappingsRejectUndefinedValues()
    {
        foreach (var preset in Enum.GetValues<InstallPreset>())
        {
            Assert.Equal(preset, InstallConfigurationChoices.ReadPreset(InstallConfigurationChoices.Name(preset)));
            Assert.Equal(10, InstallConfigurationChoices.Defaults(preset).Length);
        }
        foreach (var action in Enum.GetValues<InstallRouteAction>())
            Assert.Equal(action, InstallConfigurationChoices.ReadAction(InstallConfigurationChoices.Name(action)));
        Assert.Throws<ArgumentOutOfRangeException>(() => InstallConfigurationChoices.Defaults((InstallPreset)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => InstallConfigurationChoices.Name((InstallPreset)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => InstallConfigurationChoices.Name((InstallRouteAction)99));
    }

    [Theory(DisplayName = "Setup parsing rejects unknown syntax and conflicting route repeats"), InlineData("--preset unknown"), InlineData("--preset custom --route skills=unknown")]
    [InlineData("--preset custom --route wrong=add"), InlineData("--preset essentials --route skills=add"), InlineData("--preset custom --route skills=add --route skills=remove")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void InvalidSetup(string arguments)
    {
        var symbols = InstallBinding.CreateSymbols();
        var parse = symbols.InstallCommand.Parse(arguments.Split(' '));
        Assert.NotNull(InstallBindingInputReader.Read(parse, symbols).SetupError);
    }

    [Fact(DisplayName = "Identical route repeats produce one typed override"), Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void RepeatedSameRouteIsIdempotent()
    {
        var symbols = InstallBinding.CreateSymbols();
        var parse = symbols.InstallCommand.Parse(["--configure", "--preset", "custom", "--route", "skills=add", "--route", "skills=add"]);
        Assert.Empty(parse.Errors);
        var input = InstallBindingInputReader.Read(parse, symbols);
        Assert.Null(input.SetupError);
        Assert.Equal(new InstallRouteSelection("skills", InstallRouteAction.Add), Assert.Single(Assert.IsType<InstallSetupInput>(input.Setup).Overrides));
    }

    [Fact(DisplayName = "Install ignore changes preserve outside bytes and user rules"), Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void IgnoreSectionPreservesOutsideBytes()
    {
        const string outside = "# authored\r\n/.agents/memory/working/\r\n*.local\r\n";
        var before = Encoding.UTF8.GetBytes(outside);
        var added = InstallIgnoreSection.Rewrite(before, InstallConfigurationChoices.Defaults(InstallPreset.Essentials));
        Assert.StartsWith(outside, Encoding.UTF8.GetString(added), StringComparison.Ordinal);
        Assert.Equal(["memory/working"], InstallIgnoreSection.Read(added));
        Assert.Equal(added, InstallIgnoreSection.Rewrite(added, InstallConfigurationChoices.Defaults(InstallPreset.Essentials)));
        Assert.Equal(before, InstallIgnoreSection.Rewrite(added, InstallConfigurationChoices.Defaults(InstallPreset.FullCore)));
    }

    [Theory(DisplayName = "Malformed or duplicate Install ignore markers block"), InlineData("# BEGIN OPEN FORGE INSTALL\n"), InlineData("# END OPEN FORGE INSTALL\n")]
    [InlineData("# BEGIN OPEN FORGE INSTALL\n# END OPEN FORGE INSTALL\n# BEGIN OPEN FORGE INSTALL\n# END OPEN FORGE INSTALL\n")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void AmbiguousIgnoreSection(string text)
        => Assert.Throws<InvalidDataException>(() => InstallIgnoreSection.Read(Encoding.UTF8.GetBytes(text)));
}
