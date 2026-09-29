using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;

internal sealed record ApplyToPattern(string Text, ImmutableArray<string> Segments)
{
    internal ImmutableArray<ImmutableArray<string>> Alternatives { get; init; } = [];
}
