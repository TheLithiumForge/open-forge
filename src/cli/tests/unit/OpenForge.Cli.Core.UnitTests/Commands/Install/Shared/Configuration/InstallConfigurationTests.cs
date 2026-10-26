using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.TestSupport.Snapshots;
using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Shared.Binding;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install.Shared.Configuration;

public sealed class InstallConfigurationTests
{
    private const string WorkingDirectory = WorkspaceLayout.DefaultMainRoot + "/" + InstallConfigurationChoices.MemoryWorkingRouteId;
    private const string AuthoredLine = "# authored";
    private const string LocalPattern = "*.local";
    private const string LegacySnapshot = "legacy-ignore-upgrade";

    [Fact(DisplayName = "Essentials resolves ordinary Working as Git-ignored and omits four defaults")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void EssentialsHasExactConcreteChoices()
    {
        var rows = InstallConfigurationChoices.Defaults(InstallPreset.Essentials);
        Assert.Equal(10, rows.Length);
        Assert.Equal(InstallRouteAction.GitIgnore, rows.Single(row => row.Id == InstallConfigurationChoices.MemoryWorkingRouteId).Action);
        Assert.Equal([WorkspaceCategoryNames.Guidance, WorkspaceCategoryNames.Maps, WorkspaceCategoryNames.Templates, InstallConfigurationChoices.MemoryArchivedRouteId], rows.Where(row => row.Action == InstallRouteAction.Remove).Select(row => row.Id));
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

    [Fact(DisplayName = "Minimal adds Directives and Crystallized Memory, and every other preset builds on it"), Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void MinimalStartsEmptyAndTakesOverrides()
    {
        var minimal = InstallConfigurationChoices.Defaults(InstallPreset.Minimal);
        Assert.Equal([WorkspaceCategoryNames.Directives, InstallConfigurationChoices.MemoryCrystallizedRouteId], minimal.Where(row => row.Action == InstallRouteAction.Add).Select(row => row.Id));
        Assert.All(minimal.Where(row => row.Id is not (WorkspaceCategoryNames.Directives or InstallConfigurationChoices.MemoryCrystallizedRouteId)), row => Assert.Equal(InstallRouteAction.Remove, row.Action));
        foreach (var preset in new[] { InstallPreset.Essentials, InstallPreset.FullCore })
            Assert.All(minimal.Where(row => row.Action != InstallRouteAction.Remove), row => Assert.NotEqual(InstallRouteAction.Remove, InstallConfigurationChoices.Defaults(preset).Single(other => other.Id == row.Id).Action));
        var symbols = InstallBinding.CreateSymbols();
        var parse = symbols.InstallCommand.Parse(["--preset", "minimal", "--route", "guidance=add", "--route", "memory/working=git-ignore"]);
        Assert.Empty(parse.Errors);
        var input = InstallBindingInputReader.Read(parse, symbols);
        Assert.Null(input.SetupError);
        var setup = Assert.IsType<InstallSetupInput>(input.Setup);
        Assert.Equal(InstallPreset.Minimal, setup.Preset);
        Assert.Equal([WorkspaceCategoryNames.Guidance, InstallConfigurationChoices.MemoryWorkingRouteId], setup.Overrides.Select(row => row.Id));
    }

    [Fact(DisplayName = "Each route switches on through its top-level Loader line"), Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void RoutesMapToLoaderLines()
    {
        Assert.Equal($"{WorkspaceLayout.DefaultMainRoot}/{WorkspaceCategoryNames.Directives}/{SourceFormClassifier.CanonicalEntrypointFileName(WorkspaceCategoryNames.Directives)}", InstallConfigurationChoices.LoaderRoot(FixtureLayout, WorkspaceCategoryNames.Directives));
        Assert.Equal($"{WorkspaceLayout.DefaultMainRoot}/{WorkspaceCategoryNames.Memory}/{SourceFormClassifier.CanonicalEntrypointFileName(WorkspaceCategoryNames.Memory)}", InstallConfigurationChoices.LoaderRoot(FixtureLayout, InstallConfigurationChoices.MemoryCrystallizedRouteId));
        Assert.Equal($"{WorkspaceLayout.DefaultMainRoot}/{WorkspaceCategoryNames.Memory}/{SourceFormClassifier.CanonicalEntrypointFileName(WorkspaceCategoryNames.Memory)}", InstallConfigurationChoices.LoaderRoot(FixtureLayout, InstallConfigurationChoices.MemoryWorkingRouteId));
    }

    [Fact(DisplayName = "Identical route repeats produce one typed override"), Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void RepeatedSameRouteIsIdempotent()
    {
        var symbols = InstallBinding.CreateSymbols();
        var parse = symbols.InstallCommand.Parse(["--configure", "--preset", "custom", "--route", "skills=add", "--route", "skills=add"]);
        Assert.Empty(parse.Errors);
        var input = InstallBindingInputReader.Read(parse, symbols);
        Assert.Null(input.SetupError);
        Assert.Equal(new InstallRouteSelection(WorkspaceLayoutNames.SkillsDirectoryName, InstallRouteAction.Add), Assert.Single(Assert.IsType<InstallSetupInput>(input.Setup).Overrides));
    }

    [Fact(DisplayName = "Install ignore changes preserve outside bytes and user rules"), Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void IgnoreSectionPreservesOutsideBytes()
    {
        var outside = $"{AuthoredLine}\r\n/{WorkingDirectory}/\r\n{LocalPattern}\r\n";
        var before = Encoding.UTF8.GetBytes(outside);
        var added = InstallIgnoreSection.Rewrite(before, InstallConfigurationChoices.Defaults(InstallPreset.Essentials), null, FixtureLayout);
        Assert.Equal(before, added.AsSpan(0, before.Length).ToArray());
        Assert.Equal([InstallConfigurationChoices.MemoryWorkingRouteId], InstallIgnoreSection.Read(added, FixtureLayout));
        Assert.Equal(added, InstallIgnoreSection.Rewrite(added, InstallConfigurationChoices.Defaults(InstallPreset.Essentials), null, FixtureLayout));
        Assert.Equal(before, InstallIgnoreSection.Rewrite(added, InstallConfigurationChoices.Defaults(InstallPreset.FullCore), null, FixtureLayout));
    }

    [Fact(DisplayName = "Legacy whole-directory ignore upgrades to private contents and a shared compatibility host")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void LegacyIgnoreUpgradesWithCompatibilityHost()
    {
        var before = Encoding.UTF8.GetBytes($"{AuthoredLine}\r\n{InstallIgnoreSection.Begin}\r\n/{WorkingDirectory}/\r\n{InstallIgnoreSection.End}\r\n{LocalPattern}\r\n");
        Assert.Equal([InstallConfigurationChoices.MemoryWorkingRouteId], InstallIgnoreSection.Read(before, FixtureLayout));
        var rows = InstallConfigurationChoices.Defaults(InstallPreset.Essentials);
        var written = InstallIgnoreSection.Rewrite(before, rows,
            [new(WorkingDirectory, WorkingDirectory + "/" + SourceFormClassifier.IndexFileName)], FixtureLayout);
        CommandOutputSnapshot.MatchSnapshot(Encoding.UTF8.GetString(written), LegacySnapshot);
        Assert.Equal([InstallConfigurationChoices.MemoryWorkingRouteId], InstallIgnoreSection.Read(written, FixtureLayout));
        Assert.Equal(written, InstallIgnoreSection.Rewrite(written, rows,
            [new(WorkingDirectory, WorkingDirectory + "/" + SourceFormClassifier.IndexFileName)], FixtureLayout));
    }

    [Theory(DisplayName = "Malformed or duplicate Install ignore markers block"), InlineData(InstallIgnoreSection.Begin + "\n"), InlineData(InstallIgnoreSection.End + "\n")]
    [InlineData(InstallIgnoreSection.Begin + "\n" + InstallIgnoreSection.End + "\n" + InstallIgnoreSection.Begin + "\n" + InstallIgnoreSection.End + "\n")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void AmbiguousIgnoreSection(string text)
        => Assert.Throws<InvalidDataException>(() => InstallIgnoreSection.Read(Encoding.UTF8.GetBytes(text), FixtureLayout));

    private static readonly WorkspaceLayout FixtureLayout = WorkspaceLayout.Create(
            new CliWorkspace(
                System.IO.Path.GetFullPath(nameof(InstallConfigurationTests)),
                System.IO.Path.GetFullPath(nameof(InstallConfigurationTests)),
                CliWorkspaceSelectionMethod.ExplicitWorkspace),
            WorkspaceLayout.DefaultMainRoot);
}
