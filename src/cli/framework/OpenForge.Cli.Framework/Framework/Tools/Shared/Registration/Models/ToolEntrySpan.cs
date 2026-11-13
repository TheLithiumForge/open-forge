namespace OpenForge.Cli.Core.Framework.Tools.Shared.Registration.Models;

internal sealed record ToolEntrySpan(int Start, int End, byte[] Replacement);
internal sealed record ToolJsonSelection(string Pointer, int Start, int ValueStart, int End);
internal sealed record ToolTextLine(int Start, int ContentEnd, int End, string Content);
