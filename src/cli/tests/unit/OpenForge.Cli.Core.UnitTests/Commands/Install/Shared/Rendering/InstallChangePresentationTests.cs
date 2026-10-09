using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;
using OpenForge.Cli.Core.Presentation.Install;
using OpenForge.Cli.Core.Presentation.Install.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Install.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install.Shared.Rendering;

public sealed class InstallChangePresentationTests
{
    [Theory(DisplayName = "Install retains precise labels for every existing-file change at every detail")]
    [InlineData(true), InlineData(false)]
    [Trait("Feature", "install-presentation"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ExistingChangesKeepTheirLabels(bool preview)
    {
        foreach (var detail in new[] { CliDetail.Minimal, CliDetail.Standard, CliDetail.Full, CliDetail.Debug })
        {
            var data = InstallReportSelector.Select(Result(preview), new(detail, null)).Data;
            Assert.Equal(1, data.ReplacedFiles);
            Assert.Equal(9, data.TextRows.Count);
            CommandOutputSnapshot.MatchSnapshot(string.Join("\n", data.TextRows.Select(row => $"{row.Path}  {row.Wording}")), preview ? "preview-labels" : "done-labels");
            Assert.DoesNotContain("Nothing that already exists would be changed.", data.TextSummaryLines);
            Assert.Contains(preview ? "Would update 8 existing files and replace 1 existing file."
                : "Updated 8 existing files and replaced 1 existing file.", data.TextSummaryLines);
        }
    }

    [Theory(DisplayName = "Install plan confirmations use identical whole-plan wording in key and line modes")]
    [InlineData(true, 0), InlineData(false, 0), InlineData(true, 1), InlineData(false, 1)]
    [Trait("Feature", "install-presentation"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task ConfirmationKeepsWholePlanInBothModes(bool keys, int replacements)
    {
        var scripted = keys
            ? ScriptedCliTerminal.Keys([new(CliKey.Character, 'y')])
            : ScriptedCliTerminal.Lines(["yes"]);
        var prompts = new CliPrompts(scripted.Terminal);
        var confirm = prompts.PlanConfirmation(InstallPresentation.Rendering,
            static (InstallConfirmationFacts facts) => new CliConfirmQuestion(InstallWording.Confirmation(facts)));
        var reply = await confirm(Result(true), new(replacements), new(true), TestContext.Current.CancellationToken);
        Assert.Equal(CliPromptState.Answered, reply.State);
        var expected = replacements == 0 ? "Apply these changes? [y/N]" : "Apply these changes, including replacing 1 existing file? [y/N]";
        var output = scripted.Output.ToString();
        Assert.EndsWith(expected + Environment.NewLine, output, StringComparison.Ordinal);
        Assert.Contains("ownership record would be updated", output, StringComparison.Ordinal);
        Assert.Equal(0, scripted.ClearCalls);
    }

    [Fact(DisplayName = "Install rejects an undefined content-change fact")]
    [Trait("Feature", "install-presentation"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void UndefinedContentChangeIsRejected()
        => Assert.Throws<ArgumentOutOfRangeException>(() => Effect(".agents/file.md", InstallEffectKind.File,
            InstallEffectAction.Replace, InstallEffectOutcome.Planned, (InstallEffectContentChange)99));

    [Theory(DisplayName = "Install replacement rows promise recovery only for an observed retained bundle")]
    [InlineData(true, (int)InstallResultRecoveryState.NotCreated, "would be replaced")]
    [InlineData(false, (int)InstallResultRecoveryState.Removed, "replaced")]
    [InlineData(false, (int)InstallResultRecoveryState.Unknown, "replaced")]
    [InlineData(false, (int)InstallResultRecoveryState.Retained, "replaced (your previous file is in the recovery bundle)")]
    [Trait("Feature", "install-presentation"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ReplacementRecoveryLabels(bool preview, int recoveryValue, string expected)
    {
        var data = InstallReportSelector.Select(Result(preview, (InstallResultRecoveryState)recoveryValue), new(CliDetail.Minimal, null)).Data;
        Assert.Equal(expected, Assert.Single(data.TextRows, row => row.Path == ".agents/occupant.md").Wording);
    }

    [Theory(DisplayName = "Install preserved-content edits without metadata completion use updated wording")]
    [InlineData(true, "would be updated"), InlineData(false, "updated")]
    [Trait("Feature", "install-presentation"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void PreservedContentUsesUpdateLabel(bool preview, string expected)
    {
        var data = InstallReportSelector.Select(Result(preview, contentChange: InstallEffectContentChange.PreservedContent), new(CliDetail.Minimal, null)).Data;
        Assert.Equal(0, data.ReplacedFiles);
        Assert.Equal(expected, Assert.Single(data.TextRows, row => row.Path == ".agents/occupant.md").Wording);
    }

    [Theory(DisplayName = "Install metadata completion rows and whole-plan confirmation agree in both input modes")]
    [InlineData(true), InlineData(false)]
    [Trait("Feature", "install-presentation"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task MetadataCompletionScreens(bool keys)
    {
        var migration = new InstallMigration(".agents/occupant.md", [WorkspaceAdoptionAction.MetadataCompleted], ["description"], [], InstallMigrationOutcome.Planned);
        var preview = Result(true, contentChange: InstallEffectContentChange.PreservedContent, migrations: [migration]);
        var data = InstallReportSelector.Select(preview, new(CliDetail.Minimal, null)).Data;
        Assert.Equal(0, data.ReplacedFiles);
        Assert.Contains(data.TextRows, row => row.Path == migration.Path
            && row.Wording == "metadata would be completed, your content would be kept");
        var terminal = keys
            ? ScriptedCliTerminal.Keys([new(CliKey.Character, 'y')])
            : ScriptedCliTerminal.Lines(["yes"]);
        var confirmation = new CliPrompts(terminal.Terminal).PlanConfirmation(InstallPresentation.Rendering,
            static (InstallConfirmationFacts facts) => new CliConfirmQuestion(InstallWording.Confirmation(facts)));
        var reply = await confirmation(preview, new(0), new(true), TestContext.Current.CancellationToken);
        Assert.Equal(CliPromptState.Answered, reply.State);
        CommandOutputSnapshot.MatchSnapshot(terminal.Output.ToString(), keys ? "key-80x24" : "line");
        var appliedMigration = new InstallMigration(migration.Path, migration.Actions, migration.Fields, migration.Derivation, InstallMigrationOutcome.Applied);
        var applied = InstallReportSelector.Select(Result(false, contentChange: InstallEffectContentChange.PreservedContent, migrations: [appliedMigration]), new(CliDetail.Minimal, null)).Data;
        Assert.Equal(0, applied.ReplacedFiles);
        Assert.Contains(applied.TextRows, row => row.Path == migration.Path && row.Wording == "metadata completed, your content was kept");
    }

    private static InstallResult Result(bool preview, InstallResultRecoveryState recoveryState = InstallResultRecoveryState.NotCreated,
        InstallEffectContentChange contentChange = InstallEffectContentChange.WholeFile, IReadOnlyList<InstallMigration>? migrations = null)
    {
        var outcome = preview ? InstallEffectOutcome.Planned : InstallEffectOutcome.Verified;
        return new(null, new(Force: false, Automatic: true, Mode: preview ? InstallMode.DryRun : InstallMode.Apply), [], facts: new(new InstallResultFactsInput
        {
            Source = null,
            Classification = InstallManagementClassification.TrustedExact,
            Footprint = null,
            Migrations = migrations ?? [],
            Effects =
            [
                Effect(".agents/loader.md", InstallEffectKind.GeneratedRegion, InstallEffectAction.Replace, outcome),
                Effect("AGENTS.md", InstallEffectKind.ManagedRegion, InstallEffectAction.Replace, outcome),
                Effect("CLAUDE.md", InstallEffectKind.ManagedRegion, InstallEffectAction.Append, outcome),
                Effect(".agents/open-forge.json", InstallEffectKind.File, InstallEffectAction.Replace, outcome, InstallEffectContentChange.PreservedContent),
                Effect(".agents/open-forge.lock.json", InstallEffectKind.File, InstallEffectAction.Replace, outcome, InstallEffectContentChange.PreservedContent),
                Effect(".agents/metadata.md", InstallEffectKind.File, InstallEffectAction.Replace, outcome, InstallEffectContentChange.FrontmatterConversion),
                Effect(".gitignore", InstallEffectKind.File, InstallEffectAction.Replace, outcome, InstallEffectContentChange.PreservedContent),
                Effect(".agents/occupant.md", InstallEffectKind.File, InstallEffectAction.Replace, outcome, contentChange),
                Effect(".agents/memory/_memory.md", InstallEffectKind.GeneratedRegion, InstallEffectAction.Replace, outcome),
            ],
            Lifecycle = new(InstallLifecycleAction.Publish, preview ? InstallLifecycleOutcome.Planned : InstallLifecycleOutcome.Verified),
            Recovery = new(recoveryState, recoveryState == InstallResultRecoveryState.Retained
                ? Path.Combine(Path.GetTempPath(), "install-recovery-fixture.zip") : null),
            Verification = new(preview ? InstallResultVerificationState.NotRequested : InstallResultVerificationState.Verified),
        }))
        { Frontmatter = new("root", "scoped", []) { WasInstalled = true } };
    }

    private static InstallEffect Effect(string path, InstallEffectKind kind, InstallEffectAction action, InstallEffectOutcome outcome,
        InstallEffectContentChange contentChange = InstallEffectContentChange.WholeFile)
        => new(new InstallEffectInput
        {
            Path = path,
            Kind = kind,
            Action = action,
            SourceAssetPath = null,
            Outcome = outcome,
            Residual = InstallEffectResidual.None,
            ContentChange = contentChange,
        });
}
