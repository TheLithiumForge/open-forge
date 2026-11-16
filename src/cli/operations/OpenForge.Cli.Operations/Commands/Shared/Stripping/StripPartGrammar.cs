using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Request;

namespace OpenForge.Cli.Core.Commands.Shared.Stripping;

/// <summary>Reads strip parts from operands or --strip values. Each value is one typed comma list.</summary>
internal static class StripPartGrammar
{
    internal static bool TryRead(IEnumerable<string> values, out ImmutableArray<StripPart> parts)
    {
        var selected = new HashSet<StripPart>();
        parts = [];
        foreach (var value in values)
        {
            foreach (var member in value.Split(StrippingDefinitions.PartSeparator))
            {
                if (!TryReadMember(member, out var part)) return false;
                selected.Add(part);
            }
        }

        parts = [.. Enum.GetValues<StripPart>().Where(selected.Contains)];
        return true;
    }

    private static bool TryReadMember(string member, out StripPart part)
    {
        foreach (var candidate in Enum.GetValues<StripPart>())
        {
            if (string.Equals(StrippingDefinitions.PartName(candidate), member, StringComparison.Ordinal))
            {
                part = candidate;
                return true;
            }
        }

        part = default;
        return false;
    }
}
