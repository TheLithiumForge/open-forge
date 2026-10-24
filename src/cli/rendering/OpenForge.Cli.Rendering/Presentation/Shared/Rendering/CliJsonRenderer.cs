using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Shared.Rendering;

internal static class CliJsonRenderer
{
    internal static string Render<TData>(CliSelectedReport<TData> selected, JsonTypeInfo<TData> dataTypeInfo) where TData : class
    {
        ArgumentNullException.ThrowIfNull(selected);
        ArgumentNullException.ThrowIfNull(dataTypeInfo);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
        {
            var report = selected.Report;
            var detail = selected.Selection.Detail;
            writer.WriteStartObject();
            writer.WriteNumber(CliJsonFields.SchemaVersion, 3);
            writer.WriteString(CliJsonFields.Command, report.Command);
            writer.WriteString(CliJsonFields.Status, CliStatusDefinitions.Read(report.Status).MachineName);
            writer.WriteString(CliJsonFields.Detail, CliReportVocabulary.Name(detail));
            writer.WritePropertyName(CliJsonFields.Filter);
            if (selected.Selection.Filter is { } filter)
            {
                writer.WriteStartArray();
                foreach (var severity in filter.Order())
                {
                    writer.WriteStringValue(CliReportVocabulary.Name(severity));
                }

                writer.WriteEndArray();
            }
            else
            {
                writer.WriteNullValue();
            }

            writer.WritePropertyName(CliJsonFields.Workspace);
            if (report.Workspace is { } workspace)
            {
                writer.WriteStartObject();
                writer.WriteString(CliJsonFields.Path, workspace.Path);
                writer.WriteString(CliJsonFields.SelectedBy, workspace.Explicit ? "explicit-workspace" : "current-directory");
                writer.WriteEndObject();
            }
            else
            {
                writer.WriteNullValue();
            }

            writer.WriteStartObject(CliJsonFields.Summary);
            writer.WriteString(CliJsonFields.Headline, report.Headline.Sentence);
            writer.WriteString(CliJsonFields.Kind, CliReportVocabulary.Name(report.Headline.Kind));
            writer.WriteEndObject();
            writer.WriteStartArray(CliJsonFields.Findings);
            foreach (var finding in report.Findings)
            {
                WriteFinding(writer, finding, detail);
            }

            writer.WriteEndArray();
            writer.WriteStartArray(CliJsonFields.Effects);
            foreach (var effect in report.Effects)
            {
                writer.WriteStartObject();
                writer.WriteString(CliJsonFields.Path, effect.Path);
                writer.WriteString(CliJsonFields.Kind, CliReportVocabulary.Name(effect.Kind));
                writer.WriteString(CliJsonFields.Action, CliReportVocabulary.Name(effect.Action));
                writer.WriteString(CliJsonFields.Outcome, CliReportVocabulary.Name(effect.Outcome));
                writer.WriteString(CliJsonFields.Reason, effect.Reason);
                writer.WriteString(CliJsonFields.Owner, effect.Owner);
                if (detail >= CliDetail.Full)
                {
                    writer.WriteString(CliJsonFields.Before, effect.Before);
                    writer.WriteString(CliJsonFields.After, effect.After);
                }

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteStartObject(CliJsonFields.Counts);
            foreach (var count in report.Counts)
            {
                if (count.Value is { } value) writer.WriteNumber(count.Name, value);
                else writer.WriteNull(count.Name);
            }

            writer.WriteEndObject();
            writer.WriteStartArray(CliJsonFields.Limitations);
            foreach (var limitation in report.Limitations)
            {
                writer.WriteStartObject();
                writer.WriteString(CliJsonFields.What, limitation.What);
                writer.WriteString(CliJsonFields.Why, limitation.Why);
                writer.WritePropertyName(CliJsonFields.Subject);
                WriteSubject(writer, limitation.Subject);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WritePropertyName(CliJsonFields.Data);
            JsonSerializer.Serialize(writer, report.Data, dataTypeInfo);
            writer.WritePropertyName(CliJsonFields.Recovery);
            if (report.Recovery is { } recovery)
            {
                writer.WriteStartObject();
                writer.WriteString(CliJsonFields.Path, recovery.Path);
                writer.WriteString(CliJsonFields.Disposition, CliReportVocabulary.Name(recovery.Disposition));
                writer.WriteEndObject();
            }
            else writer.WriteNullValue();
            writer.WritePropertyName(CliJsonFields.Next);
            WriteAction(writer, report.Next);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteFinding(Utf8JsonWriter writer, CliFinding finding, CliDetail detail)
    {
        writer.WriteStartObject();
        writer.WriteString(CliJsonFields.Severity, CliReportVocabulary.Name(finding.Severity));
        writer.WriteString(CliJsonFields.Code, finding.Code);
        writer.WriteString(CliJsonFields.Title, finding.Title);
        writer.WriteString(CliJsonFields.Message, finding.Message);
        writer.WritePropertyName(CliJsonFields.Subject);
        WriteSubject(writer, finding.Subject);
        writer.WriteString(CliJsonFields.Category, finding.Category);
        if (detail >= CliDetail.Standard)
        {
            writer.WriteString(CliJsonFields.Resolution, finding.Resolution is { } resolution ? CliReportVocabulary.Name(resolution) : null);
        }

        writer.WriteStartArray(CliJsonFields.Actions);
        foreach (var action in finding.Actions) WriteAction(writer, action);
        writer.WriteEndArray();
        if (detail >= CliDetail.Full)
        {
            writer.WriteStartArray(CliJsonFields.Candidates);
            foreach (var candidate in finding.Candidates)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(CliJsonFields.Subject);
                WriteSubject(writer, candidate.Subject);
                writer.WriteStartArray(CliJsonFields.Reasons);
                foreach (var reason in candidate.Reasons) writer.WriteStringValue(reason);
                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteStartArray(CliJsonFields.Evidence);
            foreach (var evidence in finding.Evidence)
            {
                writer.WriteStartObject();
                writer.WriteString(CliJsonFields.Label, evidence.Label);
                writer.WriteString(CliJsonFields.Value, evidence.Value);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WritePropertyName(CliJsonFields.Provenance);
            if (finding.Provenance is { } provenance)
            {
                writer.WriteStartObject();
                writer.WriteString(CliJsonFields.Source, provenance.Source);
                writer.WriteString(CliJsonFields.Path, provenance.Path);
                writer.WritePropertyName(CliJsonFields.Location);
                WriteLocation(writer, provenance.Location);
                writer.WriteEndObject();
            }
            else writer.WriteNullValue();
        }

        writer.WriteEndObject();
    }

    private static void WriteSubject(Utf8JsonWriter writer, CliSubject? subject)
    {
        if (subject is null) { writer.WriteNullValue(); return; }
        writer.WriteStartObject();
        writer.WriteString(CliJsonFields.Kind, CliReportVocabulary.Name(subject.Kind));
        writer.WriteString(CliJsonFields.Path, subject.Path);
        writer.WriteString(CliJsonFields.Id, subject.Id);
        writer.WritePropertyName(CliJsonFields.Location);
        WriteLocation(writer, subject.Location);
        writer.WriteEndObject();
    }

    private static void WriteLocation(Utf8JsonWriter writer, CliSourceLocation? location)
    {
        if (location is null) { writer.WriteNullValue(); return; }
        writer.WriteStartObject();
        writer.WriteNumber(CliJsonFields.Line, location.Line);
        writer.WriteNumber(CliJsonFields.Column, location.Column);
        writer.WriteEndObject();
    }

    private static void WriteAction(Utf8JsonWriter writer, CliNextAction? action)
    {
        if (action is null) { writer.WriteNullValue(); return; }
        writer.WriteStartObject();
        writer.WriteString(CliJsonFields.Kind, CliReportVocabulary.Name(action.Kind));
        writer.WriteString(CliJsonFields.Command, action.Command);
        writer.WriteString(CliJsonFields.Reason, action.Reason);
        writer.WriteEndObject();
    }
}
