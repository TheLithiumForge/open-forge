using System.Globalization;
using System.Text.Json;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Rendering;

public sealed class CliCountNumericTests
{
    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void FractionalPercentageRemainsExactNumericInJsonAndText()
    {
        var selected = Select([new CliCount("startupShare", "startup share", 80.87m)]);

        Assert.Equal("Done.\n80.87 startup share.\n", RenderText(selected));

        using var json = JsonDocument.Parse(RenderJson(selected));
        var value = json.RootElement.GetProperty("counts").GetProperty("startupShare");
        Assert.Equal(JsonValueKind.Number, value.ValueKind);
        Assert.Equal(80.87m, value.GetDecimal());
        Assert.Equal("80.87", value.GetRawText());
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void IntegralValuesIncludingLongMaxRemainExact()
    {
        const string maximumText = "9223372036854775807";
        var maximum = (decimal)long.MaxValue;
        var selected = Select([
            new CliCount("files", "files", 42m),
            new CliCount("maximum", "maximum", maximum),
        ]);

        var text = RenderText(selected);
        Assert.Contains("42 files", text, StringComparison.Ordinal);
        Assert.Contains($"{maximumText} maximum", text, StringComparison.Ordinal);

        using var json = JsonDocument.Parse(RenderJson(selected));
        var counts = json.RootElement.GetProperty("counts");
        Assert.Equal("42", counts.GetProperty("files").GetRawText());
        Assert.Equal(maximumText, counts.GetProperty("maximum").GetRawText());
        Assert.Equal(maximum, counts.GetProperty("maximum").GetDecimal());
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void UnavailableCountRemainsNullAndKeepsItsSharedLimitation()
    {
        var selected = Select([
            new CliCount("startupShare", "startup share", null, "Startup share was unavailable."),
        ]);

        Assert.Null(selected.Report.Counts.Single().Value);
        Assert.Equal("startup share", Assert.Single(selected.Report.Limitations).What);
        Assert.Equal("Startup share was unavailable.", selected.Report.Limitations[0].Why);
        Assert.Equal("Done.\n  startup share: Startup share was unavailable.\n", RenderText(selected));

        using var json = JsonDocument.Parse(RenderJson(selected));
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("counts").GetProperty("startupShare").ValueKind);
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void DuplicateFractionalCountsUseMaximumAndAnyNullMakesUnavailable()
    {
        var maximum = Select([
            new CliCount("startupShare", "startup share", 80.87m),
            new CliCount("startupShare", "startup share", 99.12m),
        ]);
        Assert.Equal(99.12m, Assert.Single(maximum.Report.Counts).Value);
        Assert.Contains("99.12 startup share", RenderText(maximum), StringComparison.Ordinal);

        using (var json = JsonDocument.Parse(RenderJson(maximum)))
        {
            Assert.Equal("99.12", json.RootElement.GetProperty("counts").GetProperty("startupShare").GetRawText());
        }

        var unavailable = Select([
            new CliCount("startupShare", "startup share", 99.12m),
            new CliCount("startupShare", "startup share", null, "One startup source was unavailable."),
        ]);
        Assert.Null(Assert.Single(unavailable.Report.Counts).Value);
        Assert.Contains("startup share: One startup source was unavailable.", RenderText(unavailable), StringComparison.Ordinal);

        using var unavailableJson = JsonDocument.Parse(RenderJson(unavailable));
        Assert.Equal(JsonValueKind.Null, unavailableJson.RootElement.GetProperty("counts").GetProperty("startupShare").ValueKind);
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void NonEnglishCurrentCultureDoesNotChangeDecimalSeparators()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            var selected = Select([new CliCount("startupShare", "startup share", 80.87m)]);

            Assert.Contains("80.87 startup share.", RenderText(selected), StringComparison.Ordinal);
            using var json = JsonDocument.Parse(RenderJson(selected));
            Assert.Equal("80.87", json.RootElement.GetProperty("counts").GetProperty("startupShare").GetRawText());
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    private static CliSelectedReport<Data> Select(IReadOnlyList<CliCount> counts)
        => CliReportTrimmer.Trim(new CliReport<Data>
        {
            Command = "test",
            Status = CliSemanticStatus.Complete,
            Headline = new CliHeadline("Done.", CliHeadlineKind.Done),
            Counts = counts,
            Data = new Data("payload"),
        }, new CliSelection(CliDetail.Minimal), CliCommandShape.Summary);

    private static string RenderText(CliSelectedReport<Data> selected)
        => CliTextRenderer.Render(selected, CliTextStyle.Plain, static (_, _, _) => new CliTextDocument([])).Content;

    private static string RenderJson(CliSelectedReport<Data> selected)
        => CliJsonRenderer.Render(selected, ReportDataContext.Default.Data);
}
