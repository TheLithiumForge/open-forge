using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install.Shared.Configuration;

public sealed class InstallFrontmatterResolutionTests
{
    [Theory(DisplayName = "Fresh explicit Install selects and persists the requested form without conversion")]
    [InlineData((int)FrontmatterForm.Root), InlineData((int)FrontmatterForm.Scoped)]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void FreshFlagWins(int form)
        => AssertSelection(Resolve(false, FrontmatterForm.Root, Setup(form: (FrontmatterForm)form)), (FrontmatterForm)form, null, true, false);

    [Theory(DisplayName = "Fresh Install retains an authored preference and does not ask again")]
    [InlineData((int)FrontmatterForm.Root), InlineData((int)FrontmatterForm.Scoped)]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void FreshDeclaredPreferenceWins(int form)
        => AssertSelection(Resolve(false, (FrontmatterForm)form, canPrompt: true), (FrontmatterForm)form, null, true, false);

    [Fact(DisplayName = "Fresh interactive Install asks with root first")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void FreshInteractiveAsks()
    {
        var resolution = Resolve(false, canPrompt: true);
        Assert.Null(resolution.Selection);
        Assert.Equal("root", Assert.IsType<InstallFrontmatterQuestion>(resolution.Question).InitialForm);
        AssertSelection(Resolve(false, setup: Setup(form: FrontmatterForm.Scoped)), FrontmatterForm.Scoped, null, true, false);
    }

    [Fact(DisplayName = "Fresh unattended Install persists root by default")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void FreshUnattendedUsesRoot()
        => AssertSelection(Resolve(false), FrontmatterForm.Root, null, true, false);

    [Theory(DisplayName = "Ordinary installed Install retains the effective form without prompting or persisting")]
    [InlineData(null, (int)FrontmatterForm.Scoped), InlineData((int)FrontmatterForm.Root, (int)FrontmatterForm.Root)]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void InstalledKeepsEffective(int? declared, int expected)
        => AssertSelection(Resolve(true, (FrontmatterForm?)declared, canPrompt: true), (FrontmatterForm)expected, (FrontmatterForm)expected, false, false);

    [Fact(DisplayName = "An installed workspace requires Configure for an explicit frontmatter flag")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void InstalledFlagRequiresConfigure()
    {
        var resolution = Resolve(true, setup: Setup(form: FrontmatterForm.Scoped));
        Assert.Equal(InstallFindingCode.InvalidInput, Assert.IsType<InstallSetupBoundary>(resolution.Boundary).Code);
        Assert.Contains("--configure", resolution.Boundary.Cause, StringComparison.Ordinal);
        Assert.Null(resolution.Selection);
    }

    [Theory(DisplayName = "Explicit form-only Configure persists only a differing declaration and requests conversion")]
    [InlineData(null, (int)FrontmatterForm.Scoped, true), InlineData((int)FrontmatterForm.Scoped, (int)FrontmatterForm.Scoped, false)]
    [InlineData((int)FrontmatterForm.Scoped, (int)FrontmatterForm.Root, true), InlineData((int)FrontmatterForm.Root, (int)FrontmatterForm.Root, false)]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void InstalledExplicitConfigure(int? declared, int form, bool persist)
        => AssertSelection(Resolve(true, (FrontmatterForm?)declared, Setup(configure: true, form: (FrontmatterForm)form)), (FrontmatterForm)form, (FrontmatterForm?)declared ?? FrontmatterForm.Scoped, persist, true);

    [Theory(DisplayName = "Interactive Configure asks with the current form first")]
    [InlineData(null, "scoped"), InlineData((int)FrontmatterForm.Root, "root")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void InstalledInteractiveConfigure(int? declared, string initial)
    {
        var resolution = Resolve(true, (FrontmatterForm?)declared, Setup(configure: true), canPrompt: true);
        Assert.Equal(initial, Assert.IsType<InstallFrontmatterQuestion>(resolution.Question).InitialForm);
        Assert.Null(resolution.Selection);
        AssertSelection(Resolve(true, (FrontmatterForm?)declared, Setup(configure: true, form: FrontmatterForm.Scoped)),
            FrontmatterForm.Scoped, (FrontmatterForm?)declared ?? FrontmatterForm.Scoped, (FrontmatterForm?)declared != FrontmatterForm.Scoped, true);
    }

    [Fact(DisplayName = "Unattended preset Configure retains the effective form and requests conversion")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void InstalledPresetConfigureKeepsForm()
        => AssertSelection(Resolve(true, setup: Setup(configure: true, preset: InstallPreset.FullCore)), FrontmatterForm.Scoped, FrontmatterForm.Scoped, false, true);

    [Fact(DisplayName = "Unattended Configure without a preset or form stays invalid")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void NoninteractiveConfigureWithoutChoiceStaysInvalid()
    {
        var resolution = Resolve(true, setup: Setup(configure: true));
        Assert.Equal(InstallFindingCode.InvalidInput, Assert.IsType<InstallSetupBoundary>(resolution.Boundary).Code);
        Assert.Null(resolution.Selection);
    }

    private static InstallSetupInput Setup(bool configure = false, InstallPreset? preset = null, FrontmatterForm? form = null)
        => new(configure, preset, []) { Frontmatter = form };

    private static InstallFrontmatterResolution Resolve(bool installed, FrontmatterForm? declared = null, InstallSetupInput? setup = null, bool canPrompt = false)
    {
        var settings = new WorkspaceSettingsRead(WorkspaceSettingsReadState.Complete,
            WorkspaceSettingsDocument.Empty with { DeclaredFrontmatter = declared }, WorkspaceSettingsDefinitions.RelativePath, null);
        var resolution = InstallFrontmatterResolver.Resolve(setup, settings, installed, canPrompt);
        if (resolution.Selection is { } selection) Assert.Same(settings, selection.Settings);
        return resolution;
    }

    private static void AssertSelection(InstallFrontmatterResolution resolution, FrontmatterForm form, FrontmatterForm? previous, bool persist, bool convert)
    {
        Assert.Null(resolution.Boundary);
        Assert.Null(resolution.Question);
        var selection = Assert.IsType<InstallFrontmatterSelection>(resolution.Selection);
        Assert.Equal(form, selection.Form);
        Assert.Equal(previous, selection.PreviousForm);
        Assert.Equal(persist, selection.Persist);
        Assert.Equal(convert, selection.ConvertOwnedFiles);
    }
}
