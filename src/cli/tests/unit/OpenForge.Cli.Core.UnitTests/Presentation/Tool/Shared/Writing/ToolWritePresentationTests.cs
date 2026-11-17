using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Consent;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Presentation.Tool.Install;
using OpenForge.Cli.Core.Presentation.Tool.Update;
using OpenForge.Cli.Core.Presentation.Tool.Remove;
using OpenForge.Cli.Core.Presentation.Tool.Shared.Writing;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Tool.Shared.Writing;

[Trait("Feature", "tool"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
public sealed class ToolWritePresentationTests
{
    [Theory(DisplayName = "Tool writer text and JSON keep the concrete plan outcomes and account scope")]
    [InlineData((int)ToolWriteKind.Install)]
    [InlineData((int)ToolWriteKind.Update)]
    [InlineData((int)ToolWriteKind.Remove)]
    public void WriterReports(int value)
    {
        var kind = (ToolWriteKind)value;
        var result = Result(kind);
        var rendering = kind switch
        {
            ToolWriteKind.Install => ToolInstallPresentation.Rendering,
            ToolWriteKind.Update => ToolUpdatePresentation.Rendering,
            ToolWriteKind.Remove => ToolRemovePresentation.Rendering,
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
        var selection = new CliSelection(CliDetail.Full, null);
        var selected = CliReportTrimmer.Trim(rendering.Selector(result, selection), selection, rendering.Shape);
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, rendering.DataTextRenderer).Content;
        var json = CliJsonRenderer.Render(selected, rendering.DataJsonTypeInfo);

        CommandOutputSnapshot.MatchDetailSnapshot(new Dictionary<string, string>
        {
            [kind + ".text"] = text,
            [kind + ".json"] = json,
        }, testName: nameof(WriterReports) + kind);
        Assert.Same(result.Data, selected.Report.Data);
    }

    [Fact(DisplayName = "Tool account uninstall confirmation names account effects and keeps decline as an answered choice")]
    public void AccountUninstallQuestion()
    {
        var question = ToolWriteInteractionPresentation.OptionalQuestion(new()
        {
            Kind = ToolWriteConsentKind.AccountUninstall,
            ToolId = "synthetic-tool",
            Effects = [],
        });

        Assert.Equal([false, true], question.Choices.Select(choice => choice.Value));
        CommandOutputSnapshot.MatchSnapshot(question.Question + "\n" + string.Join("\n", question.Choices.Select(choice => choice.Label)), "account-uninstall");
    }

    [Fact(DisplayName = "Saved grants and effective native settings stay visible when tool planning blocks")]
    public void SavedGrantsAndNativeSettings()
    {
        var original = Result(ToolWriteKind.Install);
        var result = original with
        {
            Status = CliSemanticStatus.Blocked,
            Findings = [new(ToolWriteFindingCode.OpenForgeVersionUnsupported, CliSemanticStatus.Blocked, "synthetic-tool")],
            Data = original.Data with
            {
                Preview = false,
                Grants = [new(["synthetic/project/**"], ToolWriteStages.Completed, ["synthetic/settings.json"], [])],
                Tools = [original.Data.Tools[0] with
                {
                    Changes = [new("synthetic-native", ToolRecordDefinitions.NativeCommandKind, null, null, "NoChange", "Synthetic native setting.")
                    {
                        StoredNativeValue = ToolNativeConfigDefinitions.DisabledValue,
                        NativeEnvironmentOverrides = new Dictionary<string, string>
                        {
                            [ToolNativeConfigDefinitions.CodeGraphTelemetryVariable] = ToolNativeConfigDefinitions.EnabledValue,
                        },
                    }],
                }],
                Plan = original.Data.Plan with
                {
                    Backups = [new("synthetic/settings.json", "synthetic/settings.json.bak", new string('c', 64), true)],
                },
            },
        };
        var selection = new CliSelection(CliDetail.Full, null);
        var rendering = ToolWritePresentation.Rendering;
        var report = CliReportTrimmer.Trim(rendering.Selector(result, selection), selection, rendering.Shape);
        CommandOutputSnapshot.MatchDetailSnapshot(new Dictionary<string, string>
        {
            ["grants.text"] = CliTextRenderer.Render(report, CliTextStyle.Plain, rendering.DataTextRenderer).Content,
            ["grants.json"] = CliJsonRenderer.Render(report, rendering.DataJsonTypeInfo),
        });
    }

    [Fact(DisplayName = "Tool writer finding projection covers every defined code and rejects undefined values")]
    public void FindingCodesAreClosed()
    {
        var codes = Enum.GetValues<ToolWriteFindingCode>();
        Assert.Equal(codes.Length, codes.Select(ToolWriteReportSelector.Suffix).Distinct(StringComparer.Ordinal).Count());
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolWriteReportSelector.Suffix((ToolWriteFindingCode)int.MaxValue));
        foreach (var code in codes)
        {
            var result = Result(ToolWriteKind.Install) with
            {
                Status = CliSemanticStatus.Blocked,
                Findings = [new(code, CliSemanticStatus.Blocked, "synthetic-tool")],
            };
            Assert.Single(ToolWritePresentation.Rendering.Selector(result, new(CliDetail.Full, null)).Findings);
        }
    }

    private static ToolWriteResult Result(ToolWriteKind kind)
        => new()
        {
            Kind = kind,
            Status = CliSemanticStatus.Complete,
            WorkspacePath = "<workspace>",
            WorkspaceExplicit = true,
            Findings = [],
            Data = new()
            {
                Preview = true,
                Plan = new()
                {
                    UsedNetwork = kind != ToolWriteKind.Remove,
                    BinPath = "<home>/bin",
                    PathReadiness = "missing",
                    PathInstructions = ["synthetic manual PATH instruction"],
                },
                Outcomes = [new("synthetic-tool", ToolWriteStages.Native,
                    kind == ToolWriteKind.Remove ? ToolWriteStages.Skipped : ToolWriteStages.NotStarted, [], [])],
                Tools = [new()
                {
                    Id = "synthetic-tool", OldVersion = "1.2.3", Version = kind == ToolWriteKind.Remove ? "1.2.3" : "1.2.4", Channel = "synthetic-channel",
                    AccountReplacement = kind != ToolWriteKind.Remove, NativeHome = "<native-home>", Profile = "synthetic-profile",
                    MinimumOpenForgeVersion = "1.0.0", MinimumToolVersion = "1.2.3",
                    ProfileDigest = new string('a', 64), NativePreview = "Description",
                    Commands = kind == ToolWriteKind.Remove
                        ? [new("manual-native-uninstall", "<tool>", ["uninstall", "--synthetic"], "<workspace>", new Dictionary<string, string?>(), 30, 1024),
                            new("manual-native-setting", "<tool>", ["synthetic-restore", "on"], "<workspace>", new Dictionary<string, string?>(), 0, 0)]
                        : [new("native", "<tool>", ["install", "--synthetic"], "<workspace>",
                            new Dictionary<string, string?> { ["SYNTHETIC_OVERLAY"] = null }, 30, 1024)],
                    Effects = [new("synthetic-native", "Account", "Configuration",
                        kind == ToolWriteKind.Remove ? "Synthetic account uninstall." : "Synthetic account setup.", "<native-home>/agents.json")],
                    Changes = [new("synthetic-ignore", ToolRecordDefinitions.MarkedBlockKind, "synthetic.ignore", null,
                        kind == ToolWriteKind.Remove ? "Remove" : "Add", "Synthetic project exclusion.")],
                    Downloads = kind == ToolWriteKind.Remove ? [] : [new("https://example.invalid/synthetic.zip", "<home>/synthetic.zip", new string('b', 64))],
                    Paths = ["<home>/bin/synthetic-tool"], Backups = ["<native-home>/agents.json"],
                    Requirements = ["synthetic-runtime >=1"],
                    KeptNativeSettings = kind == ToolWriteKind.Remove
                        ? [new("synthetic-setting", "<native-home>", "off", ["synthetic-restore", "on"])] : [],
                }],
            },
        };
}
