using System.Reflection;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Install;
using OpenForge.Cli.Core.Presentation.Install.Models;
using OpenForge.Cli.Core.Presentation.Install.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install;

public sealed class InstallPresentationContractTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install native data preserves the frozen property order and nullable boundaries"), Trait("Feature", "install-presentation"), Trait("Evidence", "Unit")]
    public void NativeDataPreservesPropertyOrderAndNullableBoundaries()
    {
        AssertProperties<InstallData>(
            "Mode", "Force", "Automatic", "Classification", "Footprint", "LockPath", "Effects", "Source", "Lifecycle", "Verification");
        AssertProperties<InstallDataFootprint>("Files", "Directories", "Sections");
        AssertProperties<InstallDataSource>("InventoryFingerprint", "AssetCount");
        AssertProperties<InstallDataEffect>("Path", "Kind", "Action", "SourceAssetPath", "Outcome", "Residual");
        AssertProperties<InstallDataLifecycle>("Action", "Outcome");

        AssertNotNullable<InstallData>(nameof(InstallData.Mode));
        AssertNotNullable<InstallData>(nameof(InstallData.Force));
        AssertNotNullable<InstallData>(nameof(InstallData.Automatic));
        AssertNullable<InstallData>(nameof(InstallData.Classification));
        AssertNullable<InstallData>(nameof(InstallData.Footprint));
        AssertNotNullable<InstallData>(nameof(InstallData.LockPath));
        AssertNullable<InstallData>(nameof(InstallData.Effects));
        AssertNullable<InstallData>(nameof(InstallData.Source));
        AssertNullable<InstallData>(nameof(InstallData.Lifecycle));
        AssertNullable<InstallData>(nameof(InstallData.Verification));
        AssertNullable<InstallDataFootprint>(nameof(InstallDataFootprint.Directories));
        AssertNullable<InstallDataEffect>(nameof(InstallDataEffect.SourceAssetPath));

        var effects = Property<InstallData>(nameof(InstallData.Effects));
        Assert.Equal(typeof(IReadOnlyList<InstallDataEffect>), effects.PropertyType);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install native rendering keeps JSON generated and text output sourced from one typed report"), Trait("Feature", "install-presentation"), Trait("Evidence", "Unit")]
    public void NativeRenderingUsesOneTypedReportForJsonAndText()
    {
        var result = Result(
            InstallMode.DryRun,
            force: true,
            automatic: false,
            effects:
            [
                Effect(".agents/loader.md", InstallEffectKind.File, InstallEffectAction.Replace, InstallEffectOutcome.Planned, "framework/loader.md"),
                Effect(InstallDefinitions.OwnershipRecordPath, InstallEffectKind.File, InstallEffectAction.Create, InstallEffectOutcome.Planned, null),
            ],
            classification: InstallManagementClassification.EligibleInitialOccupant,
            lifecycle: new InstallLifecycle(InstallLifecycleAction.Publish, InstallLifecycleOutcome.Planned),
            verification: InstallResultVerificationState.NotRequested,
            footprint: new InstallFootprint(payloadFiles: 1, managedRegions: 0, generatedRegions: 0));

        var minimal = Select(result, CliDetail.Minimal);
        var text = CliTextRenderer.Render(minimal, CliTextStyle.Plain, InstallPresentation.Rendering.DataTextRenderer).Content;
        Assert.StartsWith("Would install the Open Forge Framework into ", text, StringComparison.Ordinal);
        Assert.Contains("replacing 1 existing file", text, StringComparison.Ordinal);
        Assert.Contains("No files were changed.", text, StringComparison.Ordinal);
        Assert.DoesNotContain("framework/loader.md", text, StringComparison.Ordinal);

        using var json = JsonDocument.Parse(CliJsonRenderer.Render(minimal, InstallPresentation.Rendering.DataJsonTypeInfo));
        var root = json.RootElement;
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("install", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        Assert.Equal("dry-run", root.GetProperty("data").GetProperty("mode").GetString());
        Assert.True(root.GetProperty("data").GetProperty("force").GetBoolean());
        Assert.Equal(1, root.GetProperty("counts").GetProperty("filesReplaced").GetInt32());
        Assert.Equal(2, root.GetProperty("effects").GetArrayLength());
        Assert.False(root.GetProperty("data").TryGetProperty("effects", out _));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install preview keeps the ownership record visible without claiming an applied write"), Trait("Feature", "install-presentation"), Trait("Evidence", "Unit")]
    public void PreviewMinimalRowsKeepOwnershipRecordWithoutCompletedWriteClaim()
    {
        var result = Result(
            InstallMode.DryRun,
            force: false,
            automatic: true,
            effects:
            [
                Effect(InstallDefinitions.OwnershipRecordPath, InstallEffectKind.File, InstallEffectAction.Create, InstallEffectOutcome.Planned, null),
            ],
            classification: InstallManagementClassification.SafeAbsence,
            lifecycle: new InstallLifecycle(InstallLifecycleAction.Publish, InstallLifecycleOutcome.Planned),
            verification: InstallResultVerificationState.NotRequested,
            footprint: new InstallFootprint(payloadFiles: 0, managedRegions: 0, generatedRegions: 0));

        var selected = Select(result, CliDetail.Minimal);
        var row = Assert.Single(selected.Report.Data.TextRows);
        Assert.Equal(InstallDefinitions.OwnershipRecordPath, row.Path);
        Assert.Equal("would be created", row.Wording);

        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, InstallPresentation.Rendering.DataTextRenderer).Content;

        Assert.Contains(InstallDefinitions.OwnershipRecordPath, text, StringComparison.Ordinal);
        Assert.DoesNotContain("created; records the files above", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Install presentation excludes the agents root from directory counts while retaining its effect"), Trait("Feature", "install-presentation"), Trait("Evidence", "Unit")]
    [InlineData((int)InstallMode.Apply, (int)InstallEffectOutcome.Verified, (int)InstallLifecycleOutcome.Verified, "Created 1 file and 2 directories under .agents.")]
    [InlineData((int)InstallMode.DryRun, (int)InstallEffectOutcome.Planned, (int)InstallLifecycleOutcome.Planned, "Would create 1 file and 2 directories under .agents.")]
    public void DirectoryCountsExcludeAgentsRootButDetailedEffectsRetainIt(
        int modeValue,
        int outcomeValue,
        int lifecycleOutcomeValue,
        string expectedSummary)
    {
        var mode = (InstallMode)modeValue;
        var outcome = (InstallEffectOutcome)outcomeValue;
        var lifecycleOutcome = (InstallLifecycleOutcome)lifecycleOutcomeValue;
        var result = Result(
            mode,
            force: false,
            automatic: true,
            effects:
            [
                Effect(".agents", InstallEffectKind.Directory, InstallEffectAction.Create, outcome, null),
                Effect(".agents/directives", InstallEffectKind.Directory, InstallEffectAction.Create, outcome, null),
                Effect(".agents/memory", InstallEffectKind.Directory, InstallEffectAction.Create, outcome, null),
                Effect(".agents/loader.md", InstallEffectKind.File, InstallEffectAction.Create, outcome, "framework/loader.md"),
            ],
            classification: InstallManagementClassification.SafeAbsence,
            lifecycle: new InstallLifecycle(InstallLifecycleAction.Publish, lifecycleOutcome),
            verification: mode == InstallMode.Apply
                ? InstallResultVerificationState.Verified
                : InstallResultVerificationState.NotRequested,
            footprint: new InstallFootprint(payloadFiles: 1, managedRegions: 0, generatedRegions: 0));

        var selected = Select(result, CliDetail.Full);
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, InstallPresentation.Rendering.DataTextRenderer).Content;
        Assert.Contains(expectedSummary, text, StringComparison.Ordinal);

        using var json = JsonDocument.Parse(CliJsonRenderer.Render(selected, InstallPresentation.Rendering.DataJsonTypeInfo));
        var root = json.RootElement;
        Assert.Equal(2, root.GetProperty("counts").GetProperty("directoriesCreated").GetInt32());
        Assert.Equal(2, root.GetProperty("data").GetProperty("footprint").GetProperty("directories").GetInt32());

        var effects = root.GetProperty("effects");
        Assert.Equal(4, effects.GetArrayLength());
        Assert.Equal(".agents", effects[0].GetProperty("path").GetString());
        Assert.Equal("directory", effects[0].GetProperty("kind").GetString());

        var detailedEffects = root.GetProperty("data").GetProperty("effects");
        Assert.Equal(4, detailedEffects.GetArrayLength());
        Assert.Equal(".agents", detailedEffects[0].GetProperty("path").GetString());
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Install native confirmation boundary does not render the checked plan"), Trait("Feature", "install-presentation"), Trait("Evidence", "Unit")]
    [InlineData(0, "Apply these changes? [y/N]")]
    [InlineData(1, "Replace the 1 existing file listed above? [y/N]")]
    [InlineData(2, "Replace the 2 existing files listed above? [y/N]")]
    public void ConfirmationWordingPreservesExactCountGrammar(int replacements, string expected)
        => Assert.Equal(expected, InstallWordingForTest(replacements));

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install native invalid confirmation output keeps the refusal bounded"), Trait("Feature", "install-presentation"), Trait("Evidence", "Unit")]
    public void ConfirmationUnavailableIsBounded()
    {
        var result = InstallResult.Invalid(
            new InstallBindingInput(Force: false, Automatic: false, Mode: InstallMode.Apply),
            Workspace(),
            [new InstallFinding(
                InstallFindingCode.ConfirmationRequired,
                "Interactive confirmation is unavailable.")]);
        var text = Text(result, CliDetail.Minimal);
        Assert.Contains("Install needs confirmation, and this session cannot ask.", text, StringComparison.Ordinal);
        Assert.DoesNotContain(".agents/", text, StringComparison.Ordinal);
        Assert.DoesNotContain("create", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Next: open-forge install --automatic", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install native minimal blocked rows preserve unrelated findings"), Trait("Feature", "install-presentation"), Trait("Evidence", "Unit")]
    public void MinimalBlockedRowsPreserveUnrelatedFindings()
    {
        var result = InstallResult.Invalid(
            new InstallBindingInput(Force: false, Automatic: false, Mode: InstallMode.Apply),
            Workspace(),
            [
                new InstallFinding(
                    InstallFindingCode.TargetOccupied,
                    "A target already exists.",
                    ".agents/loader.md"),
                new InstallFinding(
                    InstallFindingCode.TargetUnsafe,
                    "The filesystem access is denied.",
                    ".agents/other.md"),
            ]);

        var text = Text(result, CliDetail.Minimal);

        Assert.Contains(".agents/loader.md", text, StringComparison.Ordinal);
        Assert.Contains(
            ".agents/other.md cannot be written safely: The filesystem access is denied.",
            text,
            StringComparison.Ordinal);
        Assert.DoesNotContain("A target already exists.", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install native help and debug diagnostics keep the accepted public vocabulary"), Trait("Feature", "install-presentation"), Trait("Evidence", "Unit")]
    public void HelpAndDiagnosticsUseNativePresentation()
    {
        var help = InstallHelpSections.Create();
        var syntax = Assert.Single(help.Sections, section => section.Heading == "Syntax");
        Assert.Contains("open-forge install [--force] [--automatic] [--dry-run] [global options]", syntax.Body, StringComparison.Ordinal);
        Assert.Contains("Redirected text requests that would write require --automatic", Assert.Single(help.Sections, section => section.Heading == "Confirmation").Body, StringComparison.Ordinal);

        var result = InstallResult.Invalid(
            new InstallBindingInput(Force: false, Automatic: false, Mode: InstallMode.Apply),
            Workspace(),
            [new InstallFinding(InstallFindingCode.InvalidInput, "The representative Install input was invalid.")]);
        var selected = Select(result, CliDetail.Debug);
        Assert.Contains("status=invalid-input", selected.Report.Diagnostics);
        Assert.Contains("mode=apply", selected.Report.Diagnostics);
        Assert.Contains("classification=none", selected.Report.Diagnostics);
        Assert.Contains("recovery=not-required", selected.Report.Diagnostics);
        Assert.Contains("next=present", selected.Report.Diagnostics);
    }

    private static string InstallWordingForTest(int replacements)
        => OpenForge.Cli.Core.Presentation.Install.Shared.Wording.InstallWording.Confirmation(new InstallConfirmationFacts(replacements));

    private static CliSelectedReport<InstallData> Select(InstallResult result, CliDetail detail)
    {
        var selection = new CliSelection(detail, null);
        var rendering = InstallPresentation.Rendering;
        var selected = CliReportTrimmer.Trim(rendering.Selector(result, selection), selection, rendering.Shape);
        return rendering.SelectText!(selected);
    }

    private static string Text(InstallResult result, CliDetail detail)
        => CliTextRenderer.Render(Select(result, detail), CliTextStyle.Plain, InstallPresentation.Rendering.DataTextRenderer).Content;

    private static InstallResult Result(
        InstallMode mode,
        bool force,
        bool automatic,
        IReadOnlyList<InstallEffect> effects,
        InstallManagementClassification classification,
        InstallLifecycle lifecycle,
        InstallResultVerificationState verification,
        InstallFootprint footprint)
        => new(
            Workspace(),
            new InstallBindingInput(force, automatic, mode),
            [],
            facts: new InstallResultFacts(new InstallResultFactsInput
            {
                Source = new InstallSource(new string('a', 64), 1),
                Classification = classification,
                Footprint = footprint,
                Effects = effects,
                Lifecycle = lifecycle,
                Recovery = new InstallRecovery(InstallResultRecoveryState.NotRequired, null),
                Verification = new InstallVerification(verification),
            }));

    private static InstallEffect Effect(
        string path,
        InstallEffectKind kind,
        InstallEffectAction action,
        InstallEffectOutcome outcome,
        string? sourceAssetPath)
        => new(new InstallEffectInput
        {
            Path = path,
            Kind = kind,
            Action = action,
            SourceAssetPath = sourceAssetPath,
            Outcome = outcome,
            Residual = InstallEffectResidual.None,
        });

    private static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "install-native-presentation-contract"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.CurrentDirectory);
    }

    private static PropertyInfo Property<T>(string name)
        => typeof(T).GetProperty(name, BindingFlags.Instance | BindingFlags.Public)
            ?? throw new InvalidOperationException($"Missing property {typeof(T).Name}.{name}.");

    private static void AssertProperties<T>(params string[] expected)
        => Assert.Equal(
            expected,
            typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => property.Name));

    private static void AssertNullable<T>(string name)
        => Assert.Equal(
            NullabilityState.Nullable,
            new NullabilityInfoContext().Create(Property<T>(name)).ReadState);

    private static void AssertNotNullable<T>(string name)
        => Assert.Equal(
            NullabilityState.NotNull,
            new NullabilityInfoContext().Create(Property<T>(name)).ReadState);
}
