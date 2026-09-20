using System.Text;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Presentation.Status.Models;
using OpenForge.Cli.Core.Presentation.Status.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Status.Shared.Rendering;

internal static class StatusDataTextRenderer
{
    internal static CliTextDocument Render(
        StatusData data,
        CliSelection selection,
        CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);

        var builder = new StringBuilder();
        if (!data.TextFacts.ShowOperationalSections)
        {
            return new CliTextDocument([new CliTextSpan(string.Empty)]);
        }

        if (!data.TextFacts.ShowStandard)
        {
            AppendMinimal(builder, data);
        }
        else
        {
            AppendStandard(builder, data, style);
        }

        if (data.TextFacts.ShowFull)
        {
            AppendFull(builder, data, style);
        }

        return new CliTextDocument([new CliTextSpan(builder.ToString())]);
    }

    private static void AppendMinimal(StringBuilder builder, StatusData data)
    {
        var startup = data.Context.Startup.Current;
        var routed = data.Context.AllRouted;
        if (startup.FilesState == StatusValueState.Available
            && startup.Files is { } files
            && routed.FilesState == StatusValueState.Available
            && routed.Files is { } routedFiles
            && startup.TokensState == StatusValueState.Available
            && startup.Tokens is { } tokens)
        {
            AppendLine(builder, StatusWording.StartupReads(files, routedFiles, tokens));
        }

        if (data.Extensions.Count > 0)
        {
            AppendLine(builder, StatusWording.ExtensionList(
                string.Join(", ", data.Extensions.Select(StatusWording.ExtensionName))));
        }

        if (data.Libraries.Count > 0)
        {
            AppendLine(builder, StatusWording.LibraryList(
                string.Join(", ", data.Libraries.Select(library => StatusWording.LibraryName(library, minimal: true)))));
        }
    }

    private static void AppendStandard(StringBuilder builder, StatusData data, CliTextStyle style)
    {
        AppendLine(builder, StatusWording.StartupContextHeading());
        var measurementRows = new List<IReadOnlyList<string>>
        {
            new[] { StatusWording.ShippedByCliLabel(), StatusWording.MeasurementSummary(data.Context.Startup.Shipped) },
            new[] { StatusWording.WorkspaceMeasurementLabel(), StatusWording.MeasurementSummary(data.Context.Startup.Current) },
        };
        if (data.TextFacts.ShowMayLoadAgain)
        {
            measurementRows.Add([
                StatusWording.MayLoadAgainLabel(),
                StatusWording.MeasurementSummary(data.Context.Startup.MayLoadAgain),
            ]);
        }

        AppendTable(builder, measurementRows, style);
        if (data.TextFacts.ShowDifference)
        {
            AppendLine(builder, StatusWording.AddedSinceShipped(data.Context.Startup.Difference));
        }

        AppendLine(builder, StatusWording.AllRouted(data.Context.AllRouted, new StatusDecimalValue(
            data.Context.StartupShareState,
            data.Context.StartupShare)));

        if (data.Structure is { } structure)
        {
            var rootCategories = new StatusIntegerValue(
                structure.RootCategories.CountState,
                structure.RootCategories.Count);
            AppendLine(builder, StatusWording.RoutesSummary(
                rootCategories,
                data.TextFacts.EntriesCurrent,
                structure.RootCategories.Added,
                structure.RootCategories.Removed));
        }

        if (StatusWording.FrameworkSummary(
                data.TextFacts.FrameworkCurrent,
                data.TextFacts.FrameworkChanged,
                data.TextFacts.FrameworkMissing) is { } framework)
        {
            AppendLine(builder, framework);
        }

        foreach (var extension in data.Extensions)
        {
            AppendLine(builder, StatusWording.ExtensionSummary(extension));
        }

        foreach (var library in data.Libraries)
        {
            AppendLine(builder, StatusWording.LibrarySummary(library));
        }
    }

    private static void AppendFull(StringBuilder builder, StatusData data, CliTextStyle style)
    {
        if (data.Context.MayLoadAgainSources is { Count: > 0 } sources)
        {
            AppendLine(builder, StatusWording.LargestMayLoadAgainSourcesHeading());
            AppendTable(builder,
                sources.Select(source => (IReadOnlyList<string>)[
                    source.SourceId,
                    CliText.Bytes(source.Bytes),
                ]).ToArray(),
                style);
        }

        if (data.FrameworkFiles is { Count: > 0 } framework)
        {
            AppendLine(builder, StatusWording.FrameworkFilesHeading());
            AppendTable(builder,
                framework.Select(file => (IReadOnlyList<string>)[
                    file.Path,
                    StatusWording.ManagedTargetRow(StatusWording.HumanState(file.TargetState)),
                ]).ToArray(),
                style);
        }

        if (data.EntriesSections is { Count: > 0 } entries)
        {
            AppendLine(builder, StatusWording.EntriesSectionsHeading());
            AppendTable(builder,
                entries.Select(entry => (IReadOnlyList<string>)[
                    entry.Path,
                    StatusWording.HumanState(entry.NavigationState),
                ]).ToArray(),
                style);
        }

        foreach (var extension in data.Extensions)
        {
            if (extension.FileRows is not { } rows || rows.Count == 0)
            {
                continue;
            }

            AppendLine(builder, StatusWording.ExtensionFilesHeading(extension.Id));
            AppendTable(builder,
                rows.Select(row => (IReadOnlyList<string>)[
                    row.Path,
                    StatusWording.HumanState(row.TargetState),
                ]).ToArray(),
                style);
        }

        foreach (var library in data.Libraries)
        {
            if (library.LinkRows is not { } rows || rows.Count == 0)
            {
                continue;
            }

            AppendLine(builder, StatusWording.LibraryLinksHeading(library.Id));
            AppendTable(builder,
                rows.Select(row => (IReadOnlyList<string>)[
                    row.Path,
                    StatusWording.LibraryLinkRow(
                        StatusWording.HumanState(row.TargetState),
                        row.ExpectedTarget,
                        row.ObservedTarget),
                ]).ToArray(),
                style);
        }

        if (data.Recovery is { } recovery)
        {
            if (recovery.Candidates.Count == 0)
            {
                AppendLine(builder, $"{StatusWording.RecoveryDataHeading()}: {StatusWording.NoRecovery()}");
                return;
            }

            AppendLine(builder, $"{StatusWording.RecoveryDataHeading()}:");
            AppendTable(builder,
                recovery.Candidates.Select(candidate => (IReadOnlyList<string>)[
                    candidate.Path,
                    StatusWording.RecoveryRow(candidate),
                ]).ToArray(),
                style);
        }
    }

    private static void AppendTable(
        StringBuilder builder,
        IEnumerable<IReadOnlyList<string>> rows,
        CliTextStyle style)
        => builder.Append(CliTable.Render(
            rows.ToArray(),
            (column, cell) => column == 0 ? style.Subject(cell) : cell));

    private static void AppendLine(StringBuilder builder, string value)
        => builder.Append(CliText.Escape(value)).Append('\n');
}
