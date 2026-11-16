using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;

namespace OpenForge.Cli.Core.Commands.Stripping;

/// <summary>Reads strip parts from operands or --strip values. Each value is one typed comma list.</summary>
internal static class StripPartGrammar
{
    internal static bool TryRead(IEnumerable<string> values, out ImmutableArray<StripPart> parts)
        => TryRead(values, out parts, out _);

    // The rejected member is reported back so the invalid-input message can name it.
    internal static bool TryRead(IEnumerable<string> values, out ImmutableArray<StripPart> parts, out string? rejected)
    {
        var selected = new HashSet<StripPart>();
        parts = [];
        rejected = null;
        foreach (var value in values)
        {
            foreach (var member in value.Split(StrippingDefinitions.PartSeparator))
            {
                if (!TryReadMember(member, out var part))
                {
                    rejected = member;
                    return false;
                }

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
