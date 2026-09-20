using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Shell;

public sealed class DefinitionsTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "CLI status policy is exhaustive and stable")]
    [Trait("Feature", "cli-shell"), Trait("Evidence", "Unit")]
    public void StatusPolicyIsExhaustiveAndStable()
    {
        var expected = new Dictionary<CliSemanticStatus, (string Name, int Exit, CliOutputTarget Target)>
        {
            [CliSemanticStatus.Complete] = ("completed", 0, CliOutputTarget.StandardOutput),
            [CliSemanticStatus.Failed] = ("failed", 1, CliOutputTarget.StandardError),
            [CliSemanticStatus.Attention] = ("completed-with-warnings", 2, CliOutputTarget.StandardOutput),
            [CliSemanticStatus.Incomplete] = ("incomplete", 3, CliOutputTarget.StandardOutput),
            [CliSemanticStatus.Invalid] = ("invalid-input", 4, CliOutputTarget.StandardError),
            [CliSemanticStatus.Blocked] = ("blocked", 5, CliOutputTarget.StandardError),
            [CliSemanticStatus.Interrupted] = ("cancelled", 130, CliOutputTarget.StandardError),
        };

        Assert.Equal(Enum.GetValues<CliSemanticStatus>().Length, expected.Count);
        foreach (var pair in expected)
        {
            var definition = CliStatusDefinitions.Read(pair.Key);
            Assert.Equal(pair.Value.Name, definition.MachineName);
            Assert.Equal(pair.Value.Exit, definition.Disposition.ExitCode);
            Assert.Equal(pair.Value.Target, definition.Disposition.HumanOutputTarget);
            Assert.True(CliStatusDefinitions.TryParse(pair.Value.Name, out var parsed));
            Assert.Equal(pair.Key, parsed);
        }
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "CLI finite definitions reject unknown values")]
    [Trait("Feature", "cli-shell"), Trait("Evidence", "Unit")]
    public void UnknownFiniteValuesFailClosed()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CliStatusDefinitions.Read((CliSemanticStatus)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CliPresentationDefinitions.Validate(
                new CliPresentation((CliFormat)int.MaxValue, CliDetail.Standard, null)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CliTerminalPolicy.Validate((CliTerminalMode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CliPresentationDefinitions.ValidateTarget((CliOutputTarget)int.MaxValue));
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "CLI typed syntax owns arity defaults and spellings")]
    [Trait("Feature", "cli-shell"), Trait("Evidence", "Unit")]
    public void TypedSyntaxDefinitionsOwnArityDefaultsAndSpellings()
    {
        Assert.Equal("open-forge", CliSyntaxDefinitions.Root.Name);
        Assert.Equal(CliOptionArity.ExactlyOne, CliSyntaxDefinitions.Workspace.Arity);
        Assert.Null(CliSyntaxDefinitions.Workspace.DefaultValue);
        Assert.Equal(CliOptionArity.ExactlyOne, CliSyntaxDefinitions.Format.Arity);
        Assert.Equal(CliFormat.Text, CliSyntaxDefinitions.Format.DefaultValue);
        Assert.Equal(CliDetail.Minimal, CliSyntaxDefinitions.Detail.DefaultValue);
        Assert.True(CliSyntaxDefinitions.Detail.TryReadFinite("minimal", out var compact));
        Assert.Equal(CliDetail.Minimal, compact);
        Assert.False(CliSyntaxDefinitions.Detail.TryReadFinite("wide", out _));
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "CLI terminal policy rejects conflicting flags")]
    [Trait("Feature", "cli-shell"), Trait("Evidence", "Unit")]
    public void TerminalModesRejectConflictingFlags()
    {
        Assert.Equal(CliTerminalMode.Help, CliTerminalPolicy.Resolve(help: true, version: false));
        Assert.Equal(CliTerminalMode.Version, CliTerminalPolicy.Resolve(help: false, version: true));
        Assert.Throws<ArgumentException>(() => CliTerminalPolicy.Resolve(help: true, version: true));
    }
}
