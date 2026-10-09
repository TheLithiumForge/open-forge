using OpenForge.Cli.Core.Commands.Repair.Models.Interaction;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Presentation.Repair;
using OpenForge.Cli.Core.Presentation.Repair.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Repair;
using OpenForge.Cli.TestSupport.Interaction;
using OpenForge.Cli.TestSupport.Snapshots;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;

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
    [Fact(DisplayName = "Repair prompt keeps leaving a link unchanged as the last guided choice"),
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
    [Theory(DisplayName = "Repair Library prompt keeps Include before Leave out"),
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
        Assert.Contains("1. Include", output, StringComparison.Ordinal);
        Assert.Contains("2. Leave out", output, StringComparison.Ordinal);
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
        InlineData(1, "Include this 1 link repair in the plan? [y/N]"),
        InlineData(6, "Include these 6 link repairs in the plan? [y/N]"),
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

    [Theory(DisplayName = "Safe repair inclusion is a plan question in key and line modes")]
    [InlineData(1), InlineData(6)]
    [Trait("Feature", "repair"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task SafeConfirmationScreens(int count)
    {
        var question = new CliConfirmQuestion(RepairWording.Confirmation(new(RepairConfirmationKind.Safe, count)));
        var keys = ScriptedCliTerminal.Keys([new(CliKey.Character, 'y')]);
        var lines = ScriptedCliTerminal.Lines(["yes"]);
        Assert.True((await new CliPrompts(keys.Terminal).ConfirmAsync(question, new(true), CancellationToken.None)).Value);
        Assert.True((await new CliPrompts(lines.Terminal).ConfirmAsync(question, new(true), CancellationToken.None)).Value);
        CommandOutputSnapshot.MatchSnapshot(keys.Output.ToString(), $"safe.{count}.key.80x24");
        CommandOutputSnapshot.MatchSnapshot(lines.Output.ToString(), $"safe.{count}.line");
    }

    [Fact(DisplayName = "Guided link selection identifies the location evidence suggestion and unchanged choice in both modes")]
    [Trait("Feature", "repair"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task GuidedScreens()
    {
        var keys = ScriptedCliTerminal.Keys([new(CliKey.Down), new(CliKey.Enter)]);
        var lines = ScriptedCliTerminal.Lines(["2"]);
        foreach (var script in new[] { keys, lines })
        {
            var interaction = RepairPromptAdapters.Create(new(script.Terminal), RepairPresentation.Rendering);
            var reply = await interaction.SelectReference(new(Guided()), new(true), CancellationToken.None);
            Assert.True(reply.Value.Skipped);
        }
        CommandOutputSnapshot.MatchSnapshot(keys.Frames[0], "guided.key.80x24");
        CommandOutputSnapshot.MatchSnapshot(keys.Frames[^1], "guided.unchanged.key.80x24");
        CommandOutputSnapshot.MatchSnapshot(lines.Output.ToString(), "guided.line");
    }

    [Theory(DisplayName = "Library recovery descriptions name the proposed reverse change or previous-state check")]
    [InlineData("OrdinaryCreate", false), InlineData("OrdinaryReplace", false), InlineData("OrdinaryReplaceGeneratedRegion", false)]
    [InlineData("OrdinaryDelete", false), InlineData("RelativeFileLinkCreate", false), InlineData("RelativeFileLinkDelete", false)]
    [InlineData("OrdinaryReplace", true)]
    [Trait("Feature", "repair"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task LibraryRecoveryScreens(string kind, bool prior)
    {
        var evidence = LibraryRepairData.Evidence(Enum.Parse<RecoveryEntryKind>(kind),
            prior ? RecoveryBundleTargetComparisonState.Prior : RecoveryBundleTargetComparisonState.Intended);
        var question = new RepairLibraryPromptQuestion(new(evidence));
        Assert.Equal(prior, question.AlreadyMatchesPriorState);
        var keys = ScriptedCliTerminal.Keys([new(CliKey.Down), new(CliKey.Enter)]);
        var lines = ScriptedCliTerminal.Lines(["1"]);
        foreach (var script in new[] { keys, lines })
        {
            var interaction = RepairPromptAdapters.Create(new(script.Terminal), RepairPresentation.Rendering);
            var reply = await interaction.SelectLibrary(question, new(true), CancellationToken.None);
            Assert.Equal(script == lines, reply.Value.Selected);
        }
        CommandOutputSnapshot.MatchSnapshot(keys.Frames[0], $"library.{kind}.{prior}.key.80x24");
        CommandOutputSnapshot.MatchSnapshot(lines.Output.ToString(), $"library.{kind}.{prior}.line");
        if (kind == "RelativeFileLinkCreate")
            CommandOutputSnapshot.MatchSnapshot(keys.Frames[^1], "library.leave-out.key.80x24");
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
                    [new RepairCandidateEvidence(RepairCandidateEvidenceKind.Title, "New", location: null),
                        new RepairCandidateEvidence(RepairCandidateEvidenceKind.RouteNeighborhood, ".agents/docs", location: null)],
                    recommendedForReview: true),
            ]));
}
