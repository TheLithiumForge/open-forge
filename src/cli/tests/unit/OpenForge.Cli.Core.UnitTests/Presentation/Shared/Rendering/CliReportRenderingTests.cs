using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Rendering;

public sealed class CliReportRenderingTests
{
    [Trait("Boundary", "Output")]
    [Theory, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    [InlineData(0, 0, 2)]
    [InlineData(0, 1, 1)]
    [InlineData(1, 1, 2)]
    [InlineData(2, 1, 3)]
    [InlineData(3, 1, 3)]
    public void ShapeLadderAndCountsAreEstablishedBeforeFiltering(int detail, int shape, int listed)
    {
        var selected = CliReportTrimmer.Trim(Report(), new CliSelection((CliDetail)detail), (CliCommandShape)shape);
        Assert.Equal(listed, selected.Report.Findings.Count);
        Assert.Equal(1, Assert.Single(selected.Report.Counts, count => count.Name == "errors").Value);
        Assert.Equal(1, Assert.Single(selected.Report.Counts, count => count.Name == "warnings").Value);
        Assert.Equal(1, Assert.Single(selected.Report.Counts, count => count.Name == "infos").Value);
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void OptedInMinimalDiagnosisRetainsWarningSourceAndFirstActionUntilFullInfoDetail()
    {
        var original = CliReportTrimmer.Trim(Report(), new CliSelection(CliDetail.Minimal), CliCommandShape.Diagnosis);
        Assert.Equal(new[] { "error" }, original.TextFindings.Select(finding => finding.Code));
        var minimal = CliReportTrimmer.Trim(
            Report() with { ShowMinimalTextWarnings = true },
            new CliSelection(CliDetail.Minimal),
            CliCommandShape.Diagnosis);
        Assert.Equal(new[] { "error", "warning" }, minimal.TextFindings.Select(finding => finding.Code));
        Assert.Equal(new[] { "error" }, minimal.Report.Findings.Select(finding => finding.Code));
        var warning = Assert.Single(minimal.TextFindings, finding => finding.Severity == CliSeverity.Warning);
        Assert.Equal("b.md", warning.Subject.Path);
        var firstAction = Assert.Single(warning.Actions);
        Assert.Equal("first", firstAction.Command);
        Assert.Equal("first reason", firstAction.Reason);

        var errorsOnly = CliReportTrimmer.Trim(
            Report() with { ShowMinimalTextWarnings = true },
            new CliSelection(CliDetail.Minimal, new HashSet<CliSeverity> { CliSeverity.Error }),
            CliCommandShape.Diagnosis);
        Assert.Equal(new[] { "error" }, errorsOnly.Report.Findings.Select(finding => finding.Code));

        var standard = CliReportTrimmer.Trim(
            Report(),
            new CliSelection(CliDetail.Standard),
            CliCommandShape.Diagnosis);
        Assert.DoesNotContain(standard.Report.Findings, finding => finding.Severity == CliSeverity.Info);

        var full = CliReportTrimmer.Trim(
            Report(),
            new CliSelection(CliDetail.Full),
            CliCommandShape.Diagnosis);
        Assert.Contains(full.Report.Findings, finding => finding.Severity == CliSeverity.Info);
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void ExplicitFilterOverridesDiagnosisLadderWithoutDuplicatingEstablishedTotals()
    {
        var report = Report() with { Counts = [new CliCount("warnings", "warnings", 17), new CliCount("warnings", "warnings", 17)] };
        var selected = CliReportTrimmer.Trim(report, new CliSelection(CliDetail.Minimal, new HashSet<CliSeverity> { CliSeverity.Warning }), CliCommandShape.Diagnosis);
        Assert.Equal(CliSeverity.Warning, Assert.Single(selected.Report.Findings).Severity);
        Assert.Equal(17, Assert.Single(selected.Report.Counts, count => count.Name == "warnings").Value);
        Assert.Equal(3, selected.Report.Counts.Count);
    }

    [Trait("Boundary", "Output")]
    [Theory, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void DepthIsAppliedOnceAndBothFormatsRetainOrderedIdentities(int detailValue)
    {
        var detail = (CliDetail)detailValue;
        var selected = CliReportTrimmer.Trim(Report(), new CliSelection(detail, new HashSet<CliSeverity>(Enum.GetValues<CliSeverity>())), CliCommandShape.ChangeReport);
        Assert.Equal(new[] { "error", "warning", "info" }, selected.Report.Findings.Select(finding => finding.Code));
        var first = selected.Report.Findings[0];
        Assert.Equal(detail >= CliDetail.Full ? 1 : 0, first.Candidates.Count);
        Assert.Equal(detail >= CliDetail.Full ? 1 : 0, first.Evidence.Count);
        Assert.Equal(detail >= CliDetail.Full, first.Provenance is not null);
        Assert.Equal(detail >= CliDetail.Standard, first.Resolution is not null);
        Assert.Equal(detail >= CliDetail.Standard ? 2 : 1, first.Actions.Count);
        Assert.Equal(detail >= CliDetail.Full, selected.Report.Effects[0].Before is not null);
        Assert.Equal(detail == CliDetail.Debug ? 1 : 0, selected.Report.Diagnostics.Count);
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, static (_, _, _) => new CliTextDocument([])).Content;
        using var json = JsonDocument.Parse(CliJsonRenderer.Render(selected, Context.Data));
        var findings = json.RootElement.GetProperty("findings").EnumerateArray().ToArray();
        Assert.Equal(new[] { "error", "warning", "info" }, findings.Select(finding => finding.GetProperty("code").GetString()));
        Assert.True(text.IndexOf("a.md", StringComparison.Ordinal) < text.IndexOf("b.md", StringComparison.Ordinal));
        Assert.True(text.IndexOf("b.md", StringComparison.Ordinal) < text.IndexOf("c.md", StringComparison.Ordinal));
        Assert.Contains("changed.md", text);
        Assert.Equal("changed.md", json.RootElement.GetProperty("effects")[0].GetProperty("path").GetString());
        Assert.Equal(detail >= CliDetail.Full, findings[0].TryGetProperty("candidates", out _));
        Assert.Equal(detail >= CliDetail.Standard, findings[0].TryGetProperty("resolution", out _));
        Assert.Equal(detail >= CliDetail.Full, json.RootElement.GetProperty("effects")[0].TryGetProperty("before", out _));
        if (detail >= CliDetail.Full)
        {
            Assert.Contains("  changed.md  replaced\n    Before: before\n    After: after\n", text, StringComparison.Ordinal);
        }
        else
        {
            Assert.DoesNotContain("Before:", text, StringComparison.Ordinal);
            Assert.DoesNotContain("After:", text, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("\u001b", CliJsonRenderer.Render(selected, Context.Data), StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Theory, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    [InlineData(0, "can be fixed automatically")]
    [InlineData(1, "needs a choice")]
    [InlineData(2, "use open-forge repair")]
    [InlineData(3, "fix by hand")]
    [InlineData(4, "cannot be fixed automatically")]
    [InlineData(5, null)]
    public void ResolutionPhrasesFollowTheMessageOnlyAtFullAndDebug(int resolutionValue, string? phrase)
    {
        var report = Report() with
        {
            Workspace = null,
            Effects = [],
            Findings = [Finding(CliSeverity.Error, "error", "a.md") with
            {
                Resolution = (CliResolution)resolutionValue,
                Actions = [new CliNextAction("Read the guide.", "Understand the choice.") { Kind = CliNextActionKind.Sentence },
                    new CliNextAction("open-forge repair", "Apply the repair."),
                    new CliNextAction("open-forge doctor", "Inspect again.")],
                Candidates = [], Evidence = [], Provenance = null,
            }],
        };
        foreach (var detail in Enum.GetValues<CliDetail>())
        {
            var selected = CliReportTrimmer.Trim(report, new CliSelection(detail), CliCommandShape.Diagnosis);
            var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, static (_, _, _) => new CliTextDocument([])).Content;
            if (detail >= CliDetail.Full && phrase is not null)
            {
                Assert.Contains($"         Message.\n         {phrase}\n         Read the guide.\n", text, StringComparison.Ordinal);
            }
            else
            {
                Assert.Contains("         Message.\n         Read the guide.\n", text, StringComparison.Ordinal);
                if (phrase is not null) Assert.DoesNotContain($"         {phrase}\n", text, StringComparison.Ordinal);
            }

            using var json = JsonDocument.Parse(CliJsonRenderer.Render(selected, Context.Data));
            Assert.Equal(detail >= CliDetail.Standard, json.RootElement.GetProperty("findings")[0].TryGetProperty("resolution", out _));
        }
    }

    [Trait("Boundary", "Output")]
    [Theory, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void TargetedOperationRequiresAnEstablishedCommandBeforeDetailSelection(int detailValue)
    {
        var report = Report() with
        {
            Findings = [Finding(CliSeverity.Error, "error", "a.md") with
            {
                Resolution = CliResolution.TargetedOperation,
                Actions = [new CliNextAction("Read the guide.", "Understand the choice.") { Kind = CliNextActionKind.Sentence }],
            }],
        };
        var exception = Assert.Throws<InvalidOperationException>(() =>
            CliReportTrimmer.Trim(report, new CliSelection((CliDetail)detailValue), CliCommandShape.Diagnosis));
        Assert.Equal("A targeted-operation finding requires a command action.", exception.Message);
    }

    [Trait("Boundary", "Output")]
    [Theory, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void OptionalHashesKeepTheirIdentityAndEscapeOnlyAtTextOutput(int detailValue)
    {
        var detail = (CliDetail)detailValue;
        var report = Report() with
        {
            Workspace = null,
            Findings = [],
            Effects = [new CliEffect { Path = "new.md", Kind = CliEffectKind.File, Action = CliEffectAction.Created,
                Outcome = CliEffectOutcome.Done, After = "sha\n256\\literal" }],
        };
        var selected = CliReportTrimmer.Trim(report, new CliSelection(detail), CliCommandShape.ChangeReport);
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, static (_, _, _) => new CliTextDocument([])).Content;
        Assert.Equal(detail >= CliDetail.Full
            ? "Done.\n  new.md  created\n    After: sha\\n256\\literal\n"
            : "Done.\n  new.md  created\n", text);
        using var json = JsonDocument.Parse(CliJsonRenderer.Render(selected, Context.Data));
        var effect = json.RootElement.GetProperty("effects")[0];
        if (detail >= CliDetail.Full)
        {
            Assert.Equal(JsonValueKind.Null, effect.GetProperty("before").ValueKind);
            Assert.Equal("sha\n256\\literal", effect.GetProperty("after").GetString());
        }
        else
        {
            Assert.False(effect.TryGetProperty("before", out _));
            Assert.False(effect.TryGetProperty("after", out _));
        }
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void WorkspaceTextVisibilityDoesNotEraseJsonFacts()
    {
        var selected = CliReportTrimmer.Trim(Report(), new CliSelection(CliDetail.Minimal), CliCommandShape.Summary);
        Assert.False(selected.ShowWorkspace);
        Assert.NotNull(selected.Report.Workspace);
        Assert.True(CliReportTrimmer.Trim(Report() with { Status = CliSemanticStatus.Blocked }, new CliSelection(CliDetail.Minimal), CliCommandShape.Summary).ShowWorkspace);
        Assert.True(CliReportTrimmer.Trim(Report() with { Workspace = new CliWorkspaceEcho("explicit", true) }, new CliSelection(CliDetail.Minimal), CliCommandShape.Summary).ShowWorkspace);
    }

    [Trait("Boundary", "Output")]
    [Theory, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void NextCommandIsTheFinalLineAndItsReasonPrecedesIt(int detailValue)
    {
        var detail = (CliDetail)detailValue;
        var report = Report() with
        {
            Workspace = null,
            Findings = [],
            Effects = [],
            Next = new CliNextAction("open-forge index", "Review the preview."),
        };
        var selected = CliReportTrimmer.Trim(report, new CliSelection(detail), CliCommandShape.Summary);
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, static (_, _, _) => new CliTextDocument([])).Content;
        Assert.Equal(detail >= CliDetail.Standard
            ? "Done.\n  Review the preview.\nNext: open-forge index\n"
            : "Done.\nNext: open-forge index\n", text);
        var suppressed = CliTextRenderer.Render(selected with { ShowNext = false }, CliTextStyle.Plain,
            static (_, _, _) => new CliTextDocument([])).Content;
        Assert.Equal("Done.\n", suppressed);
        using var json = JsonDocument.Parse(CliJsonRenderer.Render(selected, Context.Data));
        Assert.Equal("open-forge index", json.RootElement.GetProperty("next").GetProperty("command").GetString());
        Assert.Equal("Review the preview.", json.RootElement.GetProperty("next").GetProperty("reason").GetString());
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void RelaxedJsonEscapesControlsAndPreservesPrintableCharacters()
    {
        var selected = CliReportTrimmer.Trim(Report() with { Data = new Data("<>&\n😀") }, new CliSelection(CliDetail.Minimal), CliCommandShape.Summary);
        var json = CliJsonRenderer.Render(selected, Context.Data);
        Assert.Contains("<>&\\n", json);
        Assert.DoesNotContain("\\u003C", json);
        Assert.DoesNotContain("\\u0026", json);
        using var parsed = JsonDocument.Parse(json);
        Assert.Equal("<>&\n😀", parsed.RootElement.GetProperty("data").GetProperty("value").GetString());
        Assert.DoesNotContain('\n', json);
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void DiagnosticsHavePerValueAndTotalBoundsAndOnlyAppearAtDebug()
    {
        Assert.Null(CliDiagnosticRenderer.Render(["detail"], CliDetail.Full));
        var rendered = CliDiagnosticRenderer.Render([new string('x', 241), "next\nvalue", new string('y', 241)], CliDetail.Debug)!;
        Assert.Equal(new string('x', 237) + "...", rendered.Split('\n')[0]);
        Assert.Contains("next\\nvalue", rendered);
        var bounded = CliDiagnosticRenderer.Render(Enumerable.Repeat(new string('z', 240), 30).ToArray(), CliDetail.Debug)!;
        Assert.True((CliText.PlatformLineEndings(bounded) + Environment.NewLine).Length <= 4096);
    }

    [Trait("Boundary", "Output")]
    [Theory, InlineData(false), InlineData(true), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void UnavailableCountReasonIsOneSharedLimitationInBothFormats(bool hasExplicitLimitation)
    {
        var report = Report() with
        {
            Findings = [],
            Effects = [],
            Counts = [new CliCount("files", "files checked", null, "The directory could not be read.")],
            Limitations = hasExplicitLimitation ? [new CliLimitation("files checked", "The directory could not be read.")] : [],
        };
        var selected = CliReportTrimmer.Trim(report, new CliSelection(CliDetail.Minimal), CliCommandShape.Summary);
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, static (_, _, _) => new CliTextDocument([])).Content;
        Assert.Equal("Done.\n  files checked: The directory could not be read.\n", text);
        using var json = JsonDocument.Parse(CliJsonRenderer.Render(selected, Context.Data));
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("counts").GetProperty("files").ValueKind);
        var limitation = Assert.Single(json.RootElement.GetProperty("limitations").EnumerateArray());
        Assert.Equal("files checked", limitation.GetProperty("what").GetString());
        Assert.Equal("The directory could not be read.", limitation.GetProperty("why").GetString());
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void StyleUsesTargetStreamCapabilityAndLeavesAuthoredDataUntouched()
    {
        var colors = new CliOutputColors(false, true);
        Assert.Equal("path", CliTextStyle.For(CliSemanticStatus.Complete, colors).Subject("path"));
        Assert.Equal("\u001b[1mpath\u001b[22m", CliTextStyle.For(CliSemanticStatus.Blocked, colors).Subject("path"));
        var selected = CliReportTrimmer.Trim(Report(), new CliSelection(CliDetail.Full), CliCommandShape.Summary);
        var document = CliTextRenderer.Render(selected, CliTextStyle.Color,
            static (_, _, _) => new CliTextDocument([CliTextSpan.FromAuthored(new CliAuthoredSpan("raw\r\n\u001b\n"))]));
        Assert.Equal("raw\r\n\u001b\n", Assert.Single(document.Spans, span => span.Authored).Content);
    }

    private static readonly ReportDataContext Context = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    });

    private static CliReport<Data> Report() => new()
    {
        Command = "test",
        Status = CliSemanticStatus.Complete,
        Headline = new CliHeadline("Done.", CliHeadlineKind.Done),
        Workspace = new CliWorkspaceEcho("workspace", false),
        Data = new Data("value"),
        Diagnostics = ["diagnostic"],
        Findings = [Finding(CliSeverity.Info, "info", "c.md"), Finding(CliSeverity.Warning, "warning", "b.md"), Finding(CliSeverity.Error, "error", "a.md")],
        Effects = [new CliEffect { Path = "changed.md", Kind = CliEffectKind.File, Action = CliEffectAction.Replaced, Outcome = CliEffectOutcome.Done, Before = "before", After = "after" }],
    };

    private static CliFinding Finding(CliSeverity severity, string code, string path) => new()
    {
        Severity = severity,
        Code = code,
        Title = "Title",
        Message = "Message.",
        Subject = new CliSubject(CliSubjectKind.File, path),
        Resolution = CliResolution.ManualDecision,
        Actions = [new CliNextAction("first", "first reason"), new CliNextAction("second", "second reason")],
        Candidates = [new CliCandidate(new CliSubject(CliSubjectKind.File, "candidate.md"), [])],
        Evidence = [new CliEvidence("label", "value")],
        Provenance = new CliProvenance("source", path),
    };
}

internal sealed record Data(string Value);
[JsonSerializable(typeof(Data))]
internal sealed partial class ReportDataContext : JsonSerializerContext;
