using System.Text;
using OpenForge.Cli.Core.Presentation.Extension.Inspect.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Extension.Inspect.Shared.Rendering;

internal static class ExtensionInspectDataTextRenderer
{
    internal static CliTextDocument Render(
        ExtensionInspectData data,
        CliSelection selection,
        CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);
        var builder = new StringBuilder();
        if (selection.Detail == CliDetail.Minimal)
        {
            if (data.TextSummaryLine is { Length: > 0 } summary)
            {
                builder.Append(CliText.Escape(summary)).Append('\n');
            }

            foreach (var file in data.TextFiles.Where(file => IsMinimalFile(file.Relation)))
            {
                AppendMinimalFile(builder, file, style);
            }

            return new CliTextDocument([new CliTextSpan(builder.ToString())]);
        }

        if (data.TextSourceLine is { } sourceLine)
        {
            builder.Append(CliText.Escape(sourceLine)).Append('\n');
        }

        builder.Append((global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleDependencies() + "\n"));
        if (data.TextDependencies.Count == 0)
        {
            builder.Append(("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNone() + "\n"));
        }
        else
        {
            builder.Append(CliTable.Render(
                data.TextDependencies,
                (column, cell) => column == 0 ? style.Subject(cell) : cell));
        }

        builder.Append('\n').Append((global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleFiles() + "\n"));
        if (data.TextFiles.Count == 0)
        {
            builder.Append(("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNone() + "\n"));
        }
        else
        {
            var rows = data.TextFiles
                .Select(file => (IReadOnlyList<string>)[file.Path, RelationText(file.Relation)])
                .ToArray();
            builder.Append(CliTable.Render(
                rows,
                (column, cell) => column == 0 ? style.Subject(cell) : cell));
        }

        if (selection.Detail >= CliDetail.Full)
        {
            foreach (var file in data.TextFiles)
            {
                builder.Append("    ")
                    .Append(CliText.Escape(global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatInstalledSha256($"{file.InstalledSha256 ?? "unavailable"}")))
                    .Append('\n');
                builder.Append("    ")
                    .Append(CliText.Escape(global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectPhrases.FormatPackageSha256($"{file.PackageSha256 ?? "unavailable"}")))
                    .Append('\n');
            }

            if (data.TextManifestLine is { } manifest)
            {
                builder.Append(CliText.Escape(manifest)).Append('\n');
            }

            if (data.TextResolutionLine is { } resolution)
            {
                builder.Append(CliText.Escape(resolution)).Append('\n');
            }

            builder.Append((global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.TitleRegisteredIn() + "\n"));
            if (data.TextRegisteredIn.Count == 0)
            {
                builder.Append(("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNone() + "\n"));
            }
            else
            {
                foreach (var path in data.TextRegisteredIn)
                {
                    builder.Append("  ").Append(CliText.Escape(path)).Append('\n');
                }
            }

            if (data.TextCoverageLine is { } coverage)
            {
                builder.Append(CliText.Escape(coverage)).Append('\n');
            }
        }

        return new CliTextDocument([new CliTextSpan(builder.ToString())]);
    }

    private static void AppendMinimalFile(
        StringBuilder builder,
        ExtensionInspectDataFile file,
        CliTextStyle style)
    {
        var severity = file.Text.Severity switch
        {
            "Error" => CliSeverity.Error,
            "Warning" => CliSeverity.Warning,
            "Info" => CliSeverity.Info,
            _ => throw new ArgumentOutOfRangeException(nameof(file), file.Text.Severity, "The Extension Inspect file severity is not defined."),
        };
        builder.Append("  ")
            .Append(style.Severity(file.Text.Severity, severity))
            .Append("  ")
            .Append(style.Subject(CliText.Escape(file.Path)))
            .Append("  ")
            .Append(CliText.Escape(file.Text.Message))
            .Append('\n');
    }

    private static bool IsMinimalFile(string relation)
        => relation is "changed" or "missing" or "new" or "retired";

    private static string RelationText(string relation)
        => relation switch
        {
            "unchanged" => "unchanged",
            "changed" => "changed",
            "missing" => "missing",
            "new" => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelNewInThePackage(),
            "retired" => global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelNoLongerPartOfThePackage(),
            "not-applicable" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotApplicable(),
            "unknown" => global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.LabelCouldNotBeCompared(),
            "unavailable" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCouldNotBeRead(),
            "invalid" => "invalid",
            "blocked" => "blocked",
            "not-started" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            _ => relation,
        };
}
