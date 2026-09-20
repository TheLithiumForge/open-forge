using System.Text.Json;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;

namespace OpenForge.Cli.IntegrationTests.Framework.Libraries.Operational.Shared.Attribution;

internal static class LibraryResidualManifest
{
    internal static void Write(Stream stream, RecoveryBundleVerifiedRead candidate)
    {
        // Independent schema-v1 fixture bytes; no production codec or recovery preparation arranges this observation.
        using var writer = new Utf8JsonWriter(stream);
        writer.WriteStartObject();
        writer.WriteNumber("schemaVersion", 1);
        writer.WriteString("command", "library detach");
        writer.WriteString("operationId", candidate.OperationId.ToString("N"));
        writer.WriteString("workspacePath", candidate.WorkspacePhysicalPath);
        writer.WriteString("workspaceKey", candidate.WorkspaceKey);
        writer.WriteStartObject("attribution");
        writer.WriteString("producer", "library");
        writer.WriteString("operation", "detach");
        writer.WriteStartObject("subject");
        writer.WriteString("kind", "workspace");
        writer.WriteString("identity", candidate.WorkspaceKey);
        writer.WriteEndObject();
        writer.WriteEndObject();
        writer.WriteStartArray("entries");
        foreach (var entry in candidate.Entries)
        {
            writer.WriteStartObject();
            writer.WriteNumber("ordinal", entry.Ordinal);
            writer.WriteString("logicalPath", entry.TargetPath);
            writer.WriteString("kind", entry.Kind switch { RecoveryEntryKind.OrdinaryDelete => "ordinary-delete", RecoveryEntryKind.OrdinaryReplace => "ordinary-replace", _ => "relative-file-link-delete" });
            writer.WriteStartObject("prior");
            if (entry.Prior.OrdinaryFile is { } identity)
            {
                writer.WriteString("kind", "ordinary-file");
                writer.WriteNumber("length", identity.Length);
                writer.WriteString("sha256", identity.Sha256);
                writer.WriteNull("linkKind");
                writer.WriteNull("rawRelativeTarget");
            }
            else
            {
                var link = entry.Prior.RelativeFileLink ?? throw new InvalidOperationException("The fixture entry requires prior link identity.");
                writer.WriteString("kind", "relative-file-link");
                writer.WriteNull("length");
                writer.WriteNull("sha256");
                writer.WriteString("linkKind", "relative-file-symbolic-link");
                writer.WriteString("rawRelativeTarget", link.RawRelativeTarget);
            }
            writer.WriteEndObject();
            writer.WriteStartObject("intended");
            if (entry.Intended.OrdinaryFile is { } intended)
            {
                writer.WriteString("kind", "ordinary-file");
                writer.WriteNumber("length", intended.Length);
                writer.WriteString("sha256", intended.Sha256);
            }
            else
            {
                writer.WriteString("kind", "missing");
                writer.WriteNull("length");
                writer.WriteNull("sha256");
            }
            writer.WriteNull("linkKind");
            writer.WriteNull("rawRelativeTarget");
            writer.WriteEndObject();
            writer.WriteString("priorPayload", entry.PriorPayload);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}
