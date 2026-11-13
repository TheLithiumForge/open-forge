using System.Text.RegularExpressions;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Content;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Registration;

internal static partial class ToolRoleGuidanceRenderer
{
    internal const string MarkerPrefix = "<!-- open-forge:tool-role ";
    internal const string MarkerEnd = " -->";

    internal static string Line(ToolRoleGuidanceSelection selection)
        => $"{selection.Guidance.Line} {MarkerPrefix}{selection.Guidance.Role} {selection.ToolId}{MarkerEnd}";

    internal static ToolRoleGuidanceSelection[] Select(ToolRoleGuidanceRequest request)
    {
        foreach (var selection in request.Active)
        {
            if (!ToolIdentity.IsToolId(selection.ToolId) || !ToolIdentity.IsId(selection.Guidance.Role))
                throw new InvalidDataException("Role guidance requires an exact role and tool ID.");
            ToolTextEntries.RequireSingleLine(selection.Guidance.Line);
            if (selection.Guidance.Line.Contains(MarkerPrefix, StringComparison.Ordinal)) throw new InvalidDataException("A role line contains a reserved marker.");
        }
        if (request.Active.Select(selection => selection.Guidance.Role).Distinct(StringComparer.Ordinal).Count() != request.Active.Length)
            throw new InvalidDataException("Each role requires at most one guidance line.");
        return [.. request.Active.OrderBy(selection => selection.Guidance.Role, StringComparer.Ordinal)];
    }

    internal static (byte[] After, ToolRoleLineCheck[] Checks) Render(
        ToolRoleGuidanceRequest request, string file, byte[]? before, FrameworkPayload? payload, bool observe)
    {
        var active = Select(request);
        var source = before is null ? string.Empty : ToolRegistrationBytes.Utf8.GetString(before);
        var document = new MarkdownDocumentParser().Parse(source);
        var managed = document.ManagedHost;
        if (managed.State == MarkdownManagedHostState.Invalid) throw new InvalidDataException(managed.Cause);
        if (managed.State == MarkdownManagedHostState.Absent)
        {
            var checks = active.Select(selection => new ToolRoleLineCheck(selection.Guidance.Role, selection.ToolId, file, ToolRoleLineState.Missing, null)).ToArray();
            if (observe || active.Length == 0) return (before ?? [], checks);
            var available = payload ?? EmbeddedFrameworkPayloadReader.Read().Payload ?? throw new InvalidDataException("The Framework agent blocks are unavailable.");
            var asset = available.Find(file) ?? throw new InvalidDataException("The Framework agent block is missing.");
            var rendered = WorkspaceLayoutPayloadRenderer.Render(request.Layout, file, asset.Bytes.AsMemory());
            if (rendered.Bytes is not { } bytes) throw new InvalidDataException("The Framework agent block cannot use this layout.");
            var newline = ToolRegistrationBytes.Newline(source);
            var block = ToolRegistrationBytes.Utf8.GetString(bytes.Span).ReplaceLineEndings(newline);
            var separator = string.Empty;
            if (source.Length > 0 && !source.EndsWith(newline + newline, StringComparison.Ordinal))
                separator = source.EndsWith(newline, StringComparison.Ordinal) ? newline : newline + newline;
            var result = Render(request, file, ToolRegistrationBytes.Utf8.GetBytes(source + separator + block), payload, observe: false);
            return (result.After, checks);
        }
        var span = managed.Span ?? throw new InvalidDataException("The managed agent block has no source span.");
        var lines = ToolRegistrationBytes.Lines(source);
        var owned = lines.Where(line => line.Start >= span.Start && line.End <= span.End && line.Content.Contains(MarkerPrefix, StringComparison.Ordinal)
            && !document.OpaqueSpans.Any(opaque => opaque.IsCode && opaque.Span.Start < line.ContentEnd && opaque.Span.End > line.Start)).ToArray();
        var facts = owned.Select(line => (Line: line, Match: RoleMarker().Match(line.Content))).ToArray();
        if (facts.Any(fact => !fact.Match.Success)) throw new InvalidDataException("A conditional role marker is malformed.");
        var desired = active.Select(Line).ToArray();
        var matching = owned.Select(line => line.Content).SequenceEqual(desired, StringComparer.Ordinal);
        var checksPresent = active.Select(selection => new ToolRoleLineCheck(selection.Guidance.Role, selection.ToolId, file,
            State(selection.Guidance.Role), null)).ToList();
        ToolRoleLineState State(string role)
        {
            if (matching) return ToolRoleLineState.Matching;
            return facts.Any(fact => fact.Match.Groups[1].Value == role) ? ToolRoleLineState.Drifted : ToolRoleLineState.Missing;
        }
        foreach (var fact in facts.Where(fact => !active.Any(selection => selection.Guidance.Role == fact.Match.Groups[1].Value)))
            checksPresent.Add(new(fact.Match.Groups[1].Value, fact.Match.Groups[2].Value, file, ToolRoleLineState.Drifted, null));
        if (observe || matching) return (before ?? [], [.. checksPresent]);
        var withoutLines = ToolRegistrationBytes.Replace(before ?? [], owned.Select(line => ToolRegistrationBytes.Span(source, line.Start, line.End, string.Empty)));
        if (desired.Length == 0) return (withoutLines, [.. checksPresent]);
        var clean = ToolRegistrationBytes.Utf8.GetString(withoutLines);
        var insertion = clean.IndexOf('\n', span.Start);
        if (insertion < 0) throw new InvalidDataException("The managed block opening is incomplete.");
        insertion++;
        var newlineValue = ToolRegistrationBytes.Newline(clean);
        if (clean.AsSpan(insertion).StartsWith(newlineValue, StringComparison.Ordinal)) insertion += newlineValue.Length;
        var addition = string.Join(newlineValue, desired) + newlineValue;
        var after = ToolRegistrationBytes.Replace(withoutLines, [ToolRegistrationBytes.Span(clean, insertion, insertion, addition)]);
        return (after, [.. checksPresent]);
    }

    [GeneratedRegex(" " + MarkerPrefix + @"([a-z0-9][a-z0-9-]{0,127}) ([a-z0-9][a-z0-9-]{0,127})" + MarkerEnd + @"\z", RegexOptions.CultureInvariant)]
    private static partial Regex RoleMarker();
}
