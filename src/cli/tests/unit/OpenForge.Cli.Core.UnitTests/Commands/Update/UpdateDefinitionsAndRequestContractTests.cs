using System.CommandLine;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Binding;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Commands.Update;

public sealed class UpdateDefinitionsAndRequestContractTests
{
    [Fact(DisplayName = "Update accepts the direct root command without operands"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void AcceptsDirectRootUpdateWithoutOperands()
    {
        var symbols = UpdateSymbols.Create();
        var parse = symbols.UpdateCommand.Parse(Array.Empty<string>());

        Assert.Empty(parse.Errors);
        Assert.Empty(symbols.UpdateCommand.Arguments);
        Assert.Equal(4, symbols.UpdateCommand.Options.Count);
    }

    [Fact(DisplayName = "Update binds four Boolean options and collapses repeated occurrences"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void BindsFourBooleanOptionsAndCollapsesRepeatedOccurrences()
    {
        var symbols = UpdateSymbols.Create();
        var parse = symbols.UpdateCommand.Parse(
        [
            "--force",
            "--force",
            "--prune",
            "--automatic",
            "--dry-run",
            "--dry-run",
        ]);

        Assert.Empty(parse.Errors);
        Assert.True(parse.GetValue(symbols.Force));
        Assert.True(parse.GetValue(symbols.Prune));
        Assert.True(parse.GetValue(symbols.Automatic));
        Assert.True(parse.GetValue(symbols.DryRun));
    }

    [Fact(DisplayName = "Update rejects operands, unknown options, and extra tokens"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void RejectsOperandsUnknownOptionsAndExtraTokens()
    {
        var symbols = UpdateSymbols.Create();
        var cases = new[]
        {
            new[] { "source" },
            new[] { "--unknown" },
            new[] { "first", "second" },
        };

        foreach (var arguments in cases)
        {
            Assert.NotEmpty(symbols.UpdateCommand.Parse(arguments).Errors);
        }
    }

    [Fact(DisplayName = "Update validates mode and prompt-capability combinations"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void ValidatesModeAndPromptCapabilityMatrix()
    {
        var workspace = Workspace();

        var dryRun = new UpdateRequest(workspace, UpdateMode.DryRun, false, false, false, false);
        var automatic = new UpdateRequest(workspace, UpdateMode.Apply, false, false, true, false);
        var humanApply = new UpdateRequest(workspace, UpdateMode.Apply, false, false, false, true);

        Assert.True(dryRun.IsDryRun);
        Assert.True(automatic.Automatic);
        Assert.True(humanApply.AllowsInteractiveConfirmation);
        Assert.Throws<ArgumentException>(() => new UpdateRequest(
            workspace,
            UpdateMode.DryRun,
            false,
            false,
            false,
            true));
        Assert.Throws<ArgumentException>(() => new UpdateRequest(
            workspace,
            UpdateMode.Apply,
            false,
            false,
            true,
            true));
    }

    [Fact(DisplayName = "Update preserves independent force, prune, automatic, and immutable request facts"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void PreservesForcePruneAutomaticAndRequestImmutability()
    {
        var workspace = Workspace();
        var request = new UpdateRequest(workspace, UpdateMode.Apply, true, true, true, false);

        Assert.Same(workspace, request.Workspace);
        Assert.True(request.Force);
        Assert.True(request.Prune);
        Assert.True(request.Automatic);
        Assert.False(request.IsDryRun);
        Assert.Equal(
            typeof(UpdateRequest).GetProperties().Where(property => property.CanWrite),
            []);
    }

    private static CliWorkspace Workspace()
        => new("update-test-workspace", "update-test-workspace", CliWorkspaceSelectionMethod.CurrentDirectory);
}
