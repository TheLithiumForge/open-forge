using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Commands.Repair.Shared.Interaction;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair.Shared.Interaction;

public sealed class RepairWizardTests
{
    [Fact(DisplayName = "Repair wizard proposes exact selection and leaves a guided candidate unselected"),
        Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public async Task ExactDefaultDoesNotSelectGuidedCandidate()
    {
        using var output = new StringWriter();
        var result = await Wizard("\n\n", output).SelectAsync(
            [RepairTestData.SafeProposal(), Guided()], TestContext.Current.CancellationToken);
        Assert.False(result.Cancelled);
        var selected = Assert.Single(result.Relinks);
        Assert.Equal(RepairTestData.SourcePath, selected.SourceCanonicalPath);
        Assert.Equal("old", selected.ExpectedDestination);
        Assert.Contains("No candidate selected", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("title", output.ToString(), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Repair wizard can skip, go back, and explicitly select a guided candidate"),
        Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public async Task BackReplacesEarlierSelection()
    {
        using var output = new StringWriter();
        var result = await Wizard("select\nback\nskip\n1\n", output).SelectAsync(
            [RepairTestData.SafeProposal(), Guided()], TestContext.Current.CancellationToken);
        Assert.False(result.Cancelled);
        var selected = Assert.Single(result.Relinks);
        Assert.Equal(".agents/docs/missing.md", selected.SourceCanonicalPath);
        Assert.Equal(RepairTestData.TargetPath, selected.SelectedTargetPath);
    }

    [Theory(DisplayName = "Repair wizard cancellation and end of input discard application authority"),
        InlineData("cancel\n"), InlineData(""), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public async Task CancellationDiscardsSelection(string answers)
    {
        using var output = new StringWriter();
        var result = await Wizard(answers, output).SelectAsync(
            [RepairTestData.SafeProposal()], TestContext.Current.CancellationToken);
        Assert.True(result.Cancelled);
        Assert.Empty(result.Relinks);
    }

    [Theory(DisplayName = "Repair wizard final application confirmation defaults to No"),
        InlineData("\n", false), InlineData("back\n", false), InlineData("cancel\n", false),
        InlineData("yes\n", true), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public async Task FinalConfirmationRequiresYes(string answers, bool expected)
    {
        using var output = new StringWriter();
        var plan = RepairPlanner.Build(
            RepairTestData.Request(mode: RepairMode.Apply), [RepairTestData.Reference()]);
        var actual = await Wizard(answers, output).ConfirmAsync(plan, TestContext.Current.CancellationToken);
        Assert.Equal(expected, actual);
        Assert.Contains("No", output.ToString(), StringComparison.Ordinal);
        Assert.Contains(RepairTestData.SourcePath, output.ToString(), StringComparison.Ordinal);
        Assert.Contains("old", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("new", output.ToString(), StringComparison.Ordinal);
        Assert.Contains(Assert.IsType<string>(plan.Effects[0].ExpectedState.Expectation.ContentHash), output.ToString(), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Repair wizard dry-run cannot request application confirmation"),
        Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public async Task DryRunHasNoApplicationConfirmation()
    {
        using var output = new StringWriter();
        var plan = RepairPlanner.Build(RepairTestData.Request(), [RepairTestData.Reference()]);
        var confirmed = await Wizard("yes\n", output).ConfirmAsync(plan, TestContext.Current.CancellationToken);
        Assert.False(confirmed);
        Assert.Equal(string.Empty, output.ToString());
    }

    private static RepairWizard Wizard(string answers, TextWriter output)
        => new(new CliInteractiveSession(new StringReader(answers), output, canPrompt: true));

    private static RepairProposal Guided()
    {
        var evidence = new RepairCandidateEvidence(RepairCandidateEvidenceKind.Title, "New", location: null);
        return new RepairProposal(
            RepairCatalogueMember.MissingTargetRelink,
            ".agents/docs/missing.md",
            RepairTestData.Occurrence(),
            "old",
            new RepairCandidateSet([new RepairCandidate(RepairTestData.Target(), [evidence], recommendedForReview: true)]));
    }
}
