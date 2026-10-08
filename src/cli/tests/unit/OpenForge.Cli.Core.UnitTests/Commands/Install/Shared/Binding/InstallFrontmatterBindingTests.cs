using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Binding;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install.Shared.Binding;

public sealed class InstallFrontmatterBindingTests
{
    [Theory(DisplayName = "Install binds both frontmatter forms through every native option delimiter")]
    [InlineData("--frontmatter", "root", (int)FrontmatterForm.Root), InlineData("--frontmatter", "scoped", (int)FrontmatterForm.Scoped)]
    [InlineData("--frontmatter=root", null, (int)FrontmatterForm.Root), InlineData("--frontmatter:scoped", null, (int)FrontmatterForm.Scoped)]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void BindsForms(string option, string? value, int expected)
    {
        var input = Read(value is null ? [option] : [option, value]);
        Assert.Null(input.SetupError);
        Assert.Equal((FrontmatterForm)expected, Assert.IsType<InstallSetupInput>(input.Setup).Frontmatter);
    }

    [Theory(DisplayName = "Identical frontmatter repetitions resolve one form"), InlineData("root"), InlineData("scoped")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void IdenticalRepeatsAgree(string form)
    {
        var input = Read(["--frontmatter", form, "--frontmatter", form]);
        Assert.Null(input.SetupError);
        Assert.NotNull(Assert.IsType<InstallSetupInput>(input.Setup).Frontmatter);
    }

    [Fact(DisplayName = "Conflicting frontmatter repetitions are invalid input")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void ConflictingRepeatsAreInvalid()
        => Assert.NotNull(Read(["--frontmatter", "root", "--frontmatter", "scoped"]).SetupError);

    [Theory(DisplayName = "Frontmatter rejects empty, unknown and differently cased values")]
    [InlineData(""), InlineData("ROOT"), InlineData("Scoped"), InlineData("unknown")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void UnknownOrCasedValueIsInvalid(string value)
        => Assert.NotNull(Read(["--frontmatter", value]).SetupError);

    [Fact(DisplayName = "A missing frontmatter value is invalid input")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void MissingValueIsInvalid()
        => Assert.NotNull(Read(["--frontmatter"]).SetupError);

    [Theory(DisplayName = "The automatic retry retains the explicit frontmatter form"), InlineData("root"), InlineData("scoped")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void RetryPreservesFrontmatter(string form)
    {
        var input = Read(["--configure", "--frontmatter", form]);
        var next = InstallDefinitions.ReadNextAction(CliSemanticStatus.Invalid,
            [new InstallFinding(InstallFindingCode.ConfirmationRequired, "Confirmation is unavailable.")], input);
        Assert.Equal($"open-forge install --automatic --configure --frontmatter {form}", next?.Command);
    }

    private static InstallBindingInput Read(string[] arguments)
    {
        var symbols = InstallBinding.CreateSymbols();
        return InstallBindingInputReader.Read(symbols.InstallCommand.Parse(arguments), symbols);
    }
}
