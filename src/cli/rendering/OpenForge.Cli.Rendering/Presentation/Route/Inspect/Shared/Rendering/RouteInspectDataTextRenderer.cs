using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Models;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Rendering;

internal static class RouteInspectDataTextRenderer
{
    internal static CliTextDocument Render(RouteInspectData data, CliSelection selection, CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);
        var builder = new StringBuilder();
        if (data.Profile is null)
        {
            AddFullContext(builder, data, selection);
            return new CliTextDocument(builder.Length == 0 ? [] : [new CliTextSpan(builder.ToString())]);
        }

        var standard = selection.Detail >= CliDetail.Standard;
        var full = selection.Detail >= CliDetail.Full;
        var profile = data.Profile;
        var identity = data.Identity;

        builder.Append('\n').Append(RouteInspectWording.BelongsHeading).Append('\n');
        AddTopology(builder, profile.Topology);
        if (identity is not null)
        {
            AddUnusualIdentity(builder, identity);
            if (selection.Detail >= CliDetail.Standard && identity.Tags.Count > 0)
            {
                Line(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleTags(), string.Join(", ", identity.Tags));
            }
        }

        builder.Append('\n').Append(RouteInspectWording.ReadingHeading).Append('\n');
        AddReading(builder, profile.Reading);
        builder.Append('\n').Append(RouteInspectWording.ContextSizeHeading).Append('\n');
        AddMeasurements(builder, profile.Measurements, full);

        if (standard)
        {
            AddAxioms(builder, profile.Axioms);
        }

        if (full)
        {
            AddFullContext(builder, data, selection);
        }

        return new CliTextDocument([new CliTextSpan(builder.ToString())]);
    }

    private static void AddTopology(
        StringBuilder builder,
        RouteInspectFact<RouteInspectTopology> fact)
    {
        if (fact.State == RouteInspectFactState.Value)
        {
            var topology = fact.ReadValue();
            Line(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleRouteChain(), string.Join(" -> ", topology.RouteChain));
            Line(builder, global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleParent(), topology.ParentId ?? global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelNoneLoaderRoot());
            AddCounts(builder, topology.Counts);
            return;
        }

        if (fact.State == RouteInspectFactState.Unavailable)
        {
            UnavailableLine(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleRouteChain(), fact.ReadReason());
            UnavailableLine(builder, global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleParent(), fact.ReadReason());
            UnavailableLine(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleDirectChildren(), fact.ReadReason());
            UnavailableLine(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleDescendants(), fact.ReadReason());
        }
    }

    private static void AddCounts(
        StringBuilder builder,
        RouteInspectFact<RouteInspectTopologyCounts> fact)
    {
        if (fact.State == RouteInspectFactState.Value)
        {
            var counts = fact.ReadValue();
            CountLine(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleDirectChildren(), counts.DirectRoutedFileCount, counts.DirectEntrypointCount);
            CountLine(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleDescendants(), counts.DescendantRoutedFileCount, counts.DescendantEntrypointCount);
        }
        else if (fact.State == RouteInspectFactState.Unavailable)
        {
            UnavailableLine(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleDirectChildren(), fact.ReadReason());
            UnavailableLine(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleDescendants(), fact.ReadReason());
        }
    }

    private static void CountLine(StringBuilder builder, string label, int files, int entrypoints)
    {
        var values = new List<string>();
        if (files > 0)
        {
            values.Add($"{files.ToString(CultureInfo.InvariantCulture)} {CliText.Plural(files, "file")}");
        }

        if (entrypoints > 0)
        {
            values.Add($"{entrypoints.ToString(CultureInfo.InvariantCulture)} {CliText.Plural(entrypoints, "entrypoint")}");
        }

        if (values.Count > 0)
        {
            Line(builder, label, string.Join(", ", values));
        }
    }

    private static void AddUnusualIdentity(
        StringBuilder builder,
        Commands.Route.Inspect.Models.Resolution.RouteInspectIdentity identity)
    {
        if (identity.Form == Commands.Route.Inspect.Models.Resolution.RouteInspectSourceForm.CompatibilityEntrypoint)
        {
            var name = RouteInspectWording.EntryPointName(identity.CanonicalWorkspaceRelativePath);
            var folder = RouteInspectWording.EntryPointFolder(identity.CanonicalWorkspaceRelativePath);
            Line(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleEntrypointName(), global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectPhrases.FormatCompatibilityNameTheCanonicalNameIsMd($"{name}", $"{folder}"));
        }

        if (identity.PhysicalLayers.Count > 1)
        {
            Line(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleOverwriteFile(), identity.PhysicalLayers[1].WorkspaceRelativePath);
        }
    }

    private static void AddReading(
        StringBuilder builder,
        RouteInspectReadingProfile reading)
    {
        AddBoolean(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleAtTaskStartOrResume(), reading.TaskStart);
        if (reading.Automatic.State == RouteInspectFactState.Value)
        {
            foreach (var reason in reading.Automatic.ReadValue().Reasons)
            {
                if (reason.Kind == RouteInspectAutomaticReadingKind.OnDemand)
                {
                    Line(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleReadAutomaticallyAfterAnotherRoute(), global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelNo());
                    Line(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleReadWhenThisRouteIsSelected(), global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelYes());
                }
                else
                {
                    builder.Append(("  " + global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleReadAutomaticallyWhen() + " "))
                        .Append(CliText.Escape(RouteInspectWording.AutomaticExplanation(reason)))
                        .Append('\n');
                }
            }
        }
        else if (reading.Automatic.State == RouteInspectFactState.Unavailable)
        {
            UnavailableLine(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleReadAutomatically(), reading.Automatic.ReadReason());
        }

        if (reading.Later.State == RouteInspectFactState.Value)
        {
            var later = reading.Later.ReadValue();
            Line(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleMayBeReadAgain(), later.MayBeReadAgain ? global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelYes() : global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelNo());
            if (later.MayBeReadAgain)
            {
                Line(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleWhen(), RouteInspectWording.LaterDescription(later.Occasions));
            }
        }
        else if (reading.Later.State == RouteInspectFactState.Unavailable)
        {
            UnavailableLine(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleMayBeReadAgain(), reading.Later.ReadReason());
        }
    }

    private static void AddBoolean(
        StringBuilder builder,
        string label,
        RouteInspectFact<bool> fact)
    {
        if (fact.State == RouteInspectFactState.Value)
        {
            Line(builder, label, fact.ReadValue() ? global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelYes() : global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelNo());
        }
        else if (fact.State == RouteInspectFactState.Unavailable)
        {
            UnavailableLine(builder, label, fact.ReadReason());
        }
    }

    private static void AddMeasurements(
        StringBuilder builder,
        RouteInspectMeasurements measurements,
        bool full)
    {
        if (measurements.OwnSource.State == RouteInspectFactState.Value)
        {
            Line(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleThisFile(), Measurement(measurements.OwnSource.ReadValue(), omitSingleFile: true));
        }
        else if (measurements.OwnSource.State == RouteInspectFactState.Unavailable)
        {
            UnavailableLine(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleThisFile(), measurements.OwnSource.ReadReason());
        }

        if (measurements.SelectionAddition.State == RouteInspectFactState.Value)
        {
            var addition = measurements.SelectionAddition.ReadValue();
            Line(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleSelectingThisRouteAdds(), addition.PhysicalFileCount == 0
                ? global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelNothingAlreadyInStartupContext()
                : Measurement(addition, omitSingleFile: false));
        }
        else if (measurements.SelectionAddition.State == RouteInspectFactState.Unavailable)
        {
            UnavailableLine(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleSelectingThisRouteAdds(), measurements.SelectionAddition.ReadReason());
        }

        if (measurements.LoadNowDescendants.State == RouteInspectFactState.Value)
        {
            var loadNow = measurements.LoadNowDescendants.ReadValue();
            if (loadNow.PhysicalFileCount > 0)
            {
                Line(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleReadAutomaticallyBelowItThroughLoadNow(), Measurement(loadNow, omitSingleFile: false));
            }
        }
        else if (measurements.LoadNowDescendants.State == RouteInspectFactState.Unavailable)
        {
            UnavailableLine(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleReadAutomaticallyBelowItThroughLoadNow(), measurements.LoadNowDescendants.ReadReason());
        }

        if (full)
        {
            AddFullMeasurement(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleSelectedContext(), measurements.SelectedClosure);
            AddFullMeasurement(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleAlreadyInStartupContext(), measurements.TaskStartOverlap);
        }
    }

    private static void AddFullMeasurement(
        StringBuilder builder,
        string label,
        RouteInspectFact<RouteInspectMeasurement> fact)
    {
        if (fact.State == RouteInspectFactState.Value)
        {
            Line(builder, label, Measurement(fact.ReadValue(), omitSingleFile: false));
        }
        else if (fact.State == RouteInspectFactState.Unavailable)
        {
            UnavailableLine(builder, label, fact.ReadReason());
        }
    }

    private static void AddAxioms(
        StringBuilder builder,
        RouteInspectFact<RouteInspectAxiomsProfile> fact)
    {
        builder.Append('\n').Append(RouteInspectWording.AxiomsHeading).Append('\n');
        if (fact.State == RouteInspectFactState.Value)
        {
            var axioms = fact.ReadValue();
            if (axioms.Inherited.State == RouteInspectFactState.Value)
            {
                var inherited = axioms.Inherited.ReadValue().SourceIds.Count == 0
                    ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNone()
                    : string.Join(", ", axioms.Inherited.ReadValue().SourceIds);
                Line(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleInheritedRulesFrom(), inherited);
            }
            else if (axioms.Inherited.State == RouteInspectFactState.Unavailable)
            {
                UnavailableLine(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleInheritedRulesFrom(), axioms.Inherited.ReadReason());
            }

            if (axioms.Local.State == RouteInspectFactState.Value)
            {
                Line(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleLocalRules(), axioms.Local.ReadValue() == RouteInspectAxiomsLocalState.Substantive ? global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelYes() : global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelNo());
            }
            else if (axioms.Local.State == RouteInspectFactState.Unavailable)
            {
                UnavailableLine(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleLocalRules(), axioms.Local.ReadReason());
            }
        }
        else if (fact.State == RouteInspectFactState.Unavailable)
        {
            UnavailableLine(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleAxioms(), fact.ReadReason());
        }
    }

    private static void AddFullContext(
        StringBuilder builder,
        RouteInspectData data,
        CliSelection selection)
    {
        if (selection.Detail < CliDetail.Full)
        {
            return;
        }

        if (data.StatusReason is { } reason)
        {
            Line(builder, global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleWhy(), reason);
        }

        if (data.Selection is { } selected)
        {
            var requested = selected.Requested is null ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNone() : $"\"{selected.Requested}\"";
            Line(builder, global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSelection(), $"{HumanReferenceKind(selected.Kind)}; {HumanSelectionMethod(selected.Method)}; requested {requested}");
        }

        if (data.Layers is { Count: > 0 } layers)
        {
            builder.Append('\n').Append(RouteInspectWording.PhysicalLayersHeading).Append('\n');
            foreach (var layer in layers)
            {
                builder.Append("  ")
                    .Append(CliText.Escape(layer.Kind))
                    .Append(" ")
                    .Append(CliText.Escape(layer.Path))
                    .Append('\n');
            }
        }
    }

    private static string Measurement(RouteInspectMeasurement measurement, bool omitSingleFile)
    {
        var parts = new List<string>();
        if (!omitSingleFile || measurement.PhysicalFileCount != 1)
        {
            parts.Add(string.Create(
                CultureInfo.InvariantCulture,
                $"{measurement.PhysicalFileCount} {CliText.Plural(measurement.PhysicalFileCount, "file")}"));
        }

        parts.Add(RouteInspectWording.Bytes(measurement.Utf8ByteCount));
        parts.Add(global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectPhrases.FormatAboutTokens(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{measurement.EstimatedTokens}")));
        return string.Join(", ", parts);
    }

    private static string HumanReferenceKind(string value)
        => value switch
        {
            "source-id" => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelSourceId(),
            "source-path" => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelSourcePath(),
            "missing" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelMissing(),
            "invalid" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelInvalid(),
            _ => value,
        };

    private static string HumanSelectionMethod(string value)
        => value switch
        {
            "automatic-id" => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelAutomaticId(),
            "exact-path" => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelExactPath(),
            "interactive" => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelInteractiveSelection(),
            "unresolved" => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelUnresolved(),
            _ => value,
        };

    private static void Line(StringBuilder builder, string label, string value)
        => builder.Append("  ")
            .Append(label)
            .Append(": ")
            .Append(CliText.Escape(value))
            .Append('\n');

    private static void UnavailableLine(StringBuilder builder, string fact, string reason)
        => builder.Append("  ")
            .Append(CliText.Escape(RouteInspectWording.UnavailableFact(fact, reason)))
            .Append('\n');
}
