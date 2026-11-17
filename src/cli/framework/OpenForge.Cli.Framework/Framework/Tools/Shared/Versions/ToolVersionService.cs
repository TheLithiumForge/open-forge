using System.Globalization;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Versions;

internal sealed class ToolVersionService : IToolVersionService
{
    public ToolSpec? ReadSpec(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var parts = value.Split('@');
        if (parts.Length > 2 || !IsId(parts[0])) return null;
        if (parts.Length == 1) return new(parts[0], new() { Kind = ToolVersionSelectionKind.LatestSupported });
        if (ReadMinor(parts[1]) is { } range)
            return new(parts[0], new() { Kind = ToolVersionSelectionKind.MinorRange, Range = range });
        return ReadStable(parts[1]) is null ? null : new(parts[0], new() { Kind = ToolVersionSelectionKind.Exact, ExactVersion = parts[1] });
    }

    public ToolVersionRange? ReadRange(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.EndsWith(".x", StringComparison.Ordinal) ? ReadMinor(value[..^2]) : null;
    }

    public bool Satisfies(string version, ToolVersionSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        if (ReadStable(version) is not { } parsed) return false;
        return selection.Kind switch
        {
            ToolVersionSelectionKind.LatestSupported => true,
            ToolVersionSelectionKind.MinorRange => selection.Range == new ToolVersionRange(parsed.Major, parsed.Minor),
            ToolVersionSelectionKind.Exact => ReadStable(selection.ExactVersion) is { } exact && parsed == exact,
            _ => throw new ArgumentOutOfRangeException(nameof(selection)),
        };
    }

    internal static ToolVersionProfile? MatchProfile(ToolCatalogueEntry entry, string version)
    {
        if (ReadStable(version) is not { } parsed) return null;
        return entry.Profiles.SingleOrDefault(profile => profile.Range == new ToolVersionRange(parsed.Major, parsed.Minor)
            && ReadStable(profile.MinimumToolVersion) is { } minimum && parsed >= minimum);
    }

    internal static Version? ReadStable(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        var build = value.IndexOf('+');
        var core = value;
        if (build >= 0)
        {
            if (!value[(build + 1)..].Split('.').All(IsBuildIdentifier)) return null;
            core = value[..build];
        }
        var parts = core.Split('.');
        if (parts.Length != 3 || !parts.All(IsNumber)) return null;
        return Version.TryParse(core, out var version) ? version : null;
    }

    internal static string? ReadProbe(string output)
    {
        var versions = output.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(token => token.StartsWith('v') ? token[1..] : token)
            .Where(token => ReadStable(token) is not null).Distinct(StringComparer.Ordinal).Take(2).ToArray();
        return versions.Length == 1 ? versions[0] : null;
    }

    private static ToolVersionRange? ReadMinor(string value)
    {
        var parts = value.Split('.');
        return parts.Length == 2 && parts.All(IsNumber)
            ? new(int.Parse(parts[0], CultureInfo.InvariantCulture), int.Parse(parts[1], CultureInfo.InvariantCulture)) : null;
    }

    private static bool IsNumber(string value)
        => value.Length > 0 && (value.Length == 1 || value[0] != '0') && value.All(char.IsAsciiDigit)
            && int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out _);

    private static bool IsBuildIdentifier(string value)
        => value.Length > 0 && value.All(character => char.IsAsciiLetterOrDigit(character) || character == '-');

    private static bool IsId(string value)
        => value.Length > 0 && char.IsAsciiLetterOrDigit(value[0])
            && value.All(character => char.IsAsciiLetterOrDigit(character) || character == '-');
}
