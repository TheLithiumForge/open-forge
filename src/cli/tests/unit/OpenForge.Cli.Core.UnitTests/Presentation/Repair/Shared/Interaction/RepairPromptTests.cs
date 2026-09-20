using OpenForge.Cli.Core.Commands.Repair.Models.Interaction;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Presentation.Repair;
using OpenForge.Cli.Core.Presentation.Repair.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Repair;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Repair.Shared.Interaction;

public sealed class RepairPromptTests
{
    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Repair prompt selects a bounded guided candidate"),
        Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public async Task SelectsGuidedCandidate()
    {
        var scripted = ScriptedCliTerminal.Lines(["1"]);
        var interaction = RepairPromptAdapters.Create(
            new CliPrompts(scripted.Terminal),
            RepairPresentation.Rendering);
        var reply = await interaction.SelectReference(
            new RepairReferencePromptQuestion(Guided()),
            new CliPromptPolicy(true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliPromptState.Answered, reply.State);
        var answer = reply.Value;
        var relink = Assert.IsType<RepairRelinkRequest>(answer.Relink);
        Assert.Equal(RepairTestData.TargetPath, relink.SelectedTargetPath);
        Assert.Contains(".agents/docs/missing.md:1:1", scripted.Output.ToString(), StringComparison.Ordinal);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Repair prompt keeps Skip as the last guided choice"),
        Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public async Task SkipsGuidedCandidate()
    {
        var scripted = ScriptedCliTerminal.Lines(["2"]);
        var interaction = RepairPromptAdapters.Create(
            new CliPrompts(scripted.Terminal),
            RepairPresentation.Rendering);
        var reply = await interaction.SelectReference(
            new RepairReferencePromptQuestion(Guided()),
            new CliPromptPolicy(true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliPromptState.Answered, reply.State);
        var answer = reply.Value;
        Assert.True(answer.Skipped);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Repair Library prompt keeps Select before the final Skip row"),
        InlineData("1", true), InlineData("2", false), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public async Task SelectsOrSkipsLibraryResidual(string line, bool expectedSelected)
    {
        var scripted = ScriptedCliTerminal.Lines([line]);
        var interaction = RepairPromptAdapters.Create(
            new CliPrompts(scripted.Terminal),
            RepairPresentation.Rendering);
        var reply = await interaction.SelectLibrary(
            new RepairLibraryPromptQuestion(new RepairLibraryRecoveryProposal(LibraryRepairData.Evidence())),
            new CliPromptPolicy(true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliPromptState.Answered, reply.State);
        Assert.Equal(expectedSelected, reply.Value.Selected);
        var output = scripted.Output.ToString();
        Assert.Contains("1. Select", output, StringComparison.Ordinal);
        Assert.Contains("2. skip", output, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Repair prompt cancellation and EOF discard selection authority"),
        InlineData(""), InlineData(null), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public async Task CancellationDiscardsSelection(string? line)
    {
        var scripted = ScriptedCliTerminal.Lines([line]);
        var interaction = RepairPromptAdapters.Create(
            new CliPrompts(scripted.Terminal),
            RepairPresentation.Rendering);
        var reply = await interaction.SelectReference(
            new RepairReferencePromptQuestion(Guided()),
            new CliPromptPolicy(true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliPromptState.Cancelled, reply.State);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Repair prompt reports unavailable input without terminal I/O"),
        Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public async Task UnavailableDoesNotWrite()
    {
        var scripted = ScriptedCliTerminal.Lines(["1"], canPrompt: false);
        var interaction = RepairPromptAdapters.Create(
            new CliPrompts(scripted.Terminal),
            RepairPresentation.Rendering);
        var reply = await interaction.SelectReference(
            new RepairReferencePromptQuestion(Guided()),
            new CliPromptPolicy(true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliPromptState.Unavailable, reply.State);
        Assert.Equal(0, scripted.WriteCalls);
        Assert.Equal(0, scripted.LineReadCalls);
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Repair confirmation wording preserves the safe count"),
        InlineData(1, "Apply the 1 repair that is safe? [y/N]"),
        InlineData(6, "Apply the 6 repairs that are safe? [y/N]"),
        Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void SafeConfirmationUsesDynamicCount(int count, string expected)
    {
        var wording = RepairWording.Confirmation(
            new RepairConfirmationQuestion(RepairConfirmationKind.Safe, count));

        Assert.Equal(expected, wording);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Repair final confirmation keeps the shared apply wording"),
        Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void FinalConfirmationUsesSharedApplyWording()
    {
        var wording = RepairWording.Confirmation(
            new RepairConfirmationQuestion(RepairConfirmationKind.Final, 6));

        Assert.Equal("Apply these changes? [y/N]", wording);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Repair prompt escapes raw evidence once at the terminal boundary"),
        Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public async Task RawEvidenceIsEscapedOnce()
    {
        var candidate = new RepairCandidate(
            RepairTestData.Target(),
            [new RepairCandidateEvidence(RepairCandidateEvidenceKind.Title, "title\nvalue", location: null)]);
        var proposal = new RepairProposal(
            RepairCatalogueMember.MissingTargetRelink,
            ".agents/docs/missing.md",
            RepairTestData.Occurrence(),
            "old",
            new RepairCandidateSet([candidate]));
        var scripted = ScriptedCliTerminal.Lines(["2"]);
        var interaction = RepairPromptAdapters.Create(
            new CliPrompts(scripted.Terminal),
            RepairPresentation.Rendering);
        await interaction.SelectReference(
            new RepairReferencePromptQuestion(proposal),
            new CliPromptPolicy(true),
            TestContext.Current.CancellationToken);

        Assert.Contains("title match: title\\nvalue", scripted.Output.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("title\nvalue", scripted.Output.ToString(), StringComparison.Ordinal);
    }

    private static RepairProposal Guided()
        => new(
            RepairCatalogueMember.MissingTargetRelink,
            ".agents/docs/missing.md",
            RepairTestData.Occurrence(),
            "old",
            new RepairCandidateSet([
                new RepairCandidate(
                    RepairTestData.Target(),
                    [new RepairCandidateEvidence(RepairCandidateEvidenceKind.Title, "New", location: null)],
                    recommendedForReview: true),
            ]));
}
