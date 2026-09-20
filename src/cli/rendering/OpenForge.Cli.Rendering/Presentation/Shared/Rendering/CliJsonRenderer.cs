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
            writer.WriteNumber("schemaVersion", 3);
            writer.WriteString("command", report.Command);
            writer.WriteString("status", CliStatusDefinitions.Read(report.Status).MachineName);
            writer.WriteString("detail", CliReportVocabulary.Name(detail));
            writer.WritePropertyName("filter");
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

            writer.WritePropertyName("workspace");
            if (report.Workspace is { } workspace)
            {
                writer.WriteStartObject();
                writer.WriteString("path", workspace.Path);
                writer.WriteString("selectedBy", workspace.Explicit ? "explicit-workspace" : "current-directory");
                writer.WriteEndObject();
            }
            else
            {
                writer.WriteNullValue();
            }

            writer.WriteStartObject("summary");
            writer.WriteString("headline", report.Headline.Sentence);
            writer.WriteString("kind", CliReportVocabulary.Name(report.Headline.Kind));
            writer.WriteEndObject();
            writer.WriteStartArray("findings");
            foreach (var finding in report.Findings)
            {
                WriteFinding(writer, finding, detail);
            }

            writer.WriteEndArray();
            writer.WriteStartArray("effects");
            foreach (var effect in report.Effects)
            {
                writer.WriteStartObject();
                writer.WriteString("path", effect.Path);
                writer.WriteString("kind", CliReportVocabulary.Name(effect.Kind));
                writer.WriteString("action", CliReportVocabulary.Name(effect.Action));
                writer.WriteString("outcome", CliReportVocabulary.Name(effect.Outcome));
                writer.WriteString("reason", effect.Reason);
                writer.WriteString("owner", effect.Owner);
                if (detail >= CliDetail.Full)
                {
                    writer.WriteString("before", effect.Before);
                    writer.WriteString("after", effect.After);
                }

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteStartObject("counts");
            foreach (var count in report.Counts)
            {
                if (count.Value is { } value) writer.WriteNumber(count.Name, value);
                else writer.WriteNull(count.Name);
            }

            writer.WriteEndObject();
            writer.WriteStartArray("limitations");
            foreach (var limitation in report.Limitations)
            {
                writer.WriteStartObject();
                writer.WriteString("what", limitation.What);
                writer.WriteString("why", limitation.Why);
                writer.WritePropertyName("subject");
                WriteSubject(writer, limitation.Subject);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WritePropertyName("data");
            JsonSerializer.Serialize(writer, report.Data, dataTypeInfo);
            writer.WritePropertyName("recovery");
            if (report.Recovery is { } recovery)
            {
                writer.WriteStartObject();
                writer.WriteString("path", recovery.Path);
                writer.WriteString("disposition", CliReportVocabulary.Name(recovery.Disposition));
                writer.WriteEndObject();
            }
            else writer.WriteNullValue();
            writer.WritePropertyName("next");
            WriteAction(writer, report.Next);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteFinding(Utf8JsonWriter writer, CliFinding finding, CliDetail detail)
    {
        writer.WriteStartObject();
        writer.WriteString("severity", CliReportVocabulary.Name(finding.Severity));
        writer.WriteString("code", finding.Code);
        writer.WriteString("title", finding.Title);
        writer.WriteString("message", finding.Message);
        writer.WritePropertyName("subject");
        WriteSubject(writer, finding.Subject);
        writer.WriteString("category", finding.Category);
        if (detail >= CliDetail.Standard)
        {
            writer.WriteString("resolution", finding.Resolution is { } resolution ? CliReportVocabulary.Name(resolution) : null);
        }

        writer.WriteStartArray("actions");
        foreach (var action in finding.Actions) WriteAction(writer, action);
        writer.WriteEndArray();
        if (detail >= CliDetail.Full)
        {
            writer.WriteStartArray("candidates");
            foreach (var candidate in finding.Candidates)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("subject");
                WriteSubject(writer, candidate.Subject);
                writer.WriteStartArray("reasons");
                foreach (var reason in candidate.Reasons) writer.WriteStringValue(reason);
                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteStartArray("evidence");
            foreach (var evidence in finding.Evidence)
            {
                writer.WriteStartObject();
                writer.WriteString("label", evidence.Label);
                writer.WriteString("value", evidence.Value);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WritePropertyName("provenance");
            if (finding.Provenance is { } provenance)
            {
                writer.WriteStartObject();
                writer.WriteString("source", provenance.Source);
                writer.WriteString("path", provenance.Path);
                writer.WritePropertyName("location");
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
        writer.WriteString("kind", CliReportVocabulary.Name(subject.Kind));
        writer.WriteString("path", subject.Path);
        writer.WriteString("id", subject.Id);
        writer.WritePropertyName("location");
        WriteLocation(writer, subject.Location);
        writer.WriteEndObject();
    }

    private static void WriteLocation(Utf8JsonWriter writer, CliSourceLocation? location)
    {
        if (location is null) { writer.WriteNullValue(); return; }
        writer.WriteStartObject();
        writer.WriteNumber("line", location.Line);
        writer.WriteNumber("column", location.Column);
        writer.WriteEndObject();
    }

    private static void WriteAction(Utf8JsonWriter writer, CliNextAction? action)
    {
        if (action is null) { writer.WriteNullValue(); return; }
        writer.WriteStartObject();
        writer.WriteString("kind", CliReportVocabulary.Name(action.Kind));
        writer.WriteString("command", action.Command);
        writer.WriteString("reason", action.Reason);
        writer.WriteEndObject();
    }
}
