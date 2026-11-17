namespace OpenForge.Cli.Core.Framework.Tools.Shared.Versions;

internal static class ToolRuntimeRange
{
    internal static bool Satisfies(string? version, string range)
    {
        if (ToolVersionService.ReadStable(version) is not { } actual) return false;
        if (ToolVersionService.ReadStable(range) is { } exact) return actual == exact;
        var bounds = range.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (bounds.Length is < 1 or > 2 || !bounds[0].StartsWith(">=", StringComparison.Ordinal)
            || ReadBound(bounds[0][2..]) is not { } minimum || actual < minimum) return false;
        if (bounds.Length == 1) return true;
        return bounds[1].StartsWith('<') && ReadBound(bounds[1][1..]) is { } ceiling && ceiling > minimum && actual < ceiling;
    }

    private static Version? ReadBound(string value)
    {
        var components = value.Split('.');
        if (components.Length is < 1 or > 3) return null;
        return ToolVersionService.ReadStable(string.Join('.', components.Concat(Enumerable.Repeat("0", 3 - components.Length))));
    }
}
