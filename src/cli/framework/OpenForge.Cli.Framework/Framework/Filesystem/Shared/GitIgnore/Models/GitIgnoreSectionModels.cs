namespace OpenForge.Cli.Core.Framework.Filesystem.Shared.GitIgnore.Models;

internal sealed record GitIgnoreSectionMarkers(string Begin, string End, string Label);
internal sealed record GitIgnoreSectionSpan(int Start, int EndExclusive, int BodyStart, int BodyEnd, string LineEnding);
