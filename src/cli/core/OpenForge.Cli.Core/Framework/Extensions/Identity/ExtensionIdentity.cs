namespace OpenForge.Cli.Core.Framework.Extensions.Identity;

internal static class ExtensionIdentity
{
    internal static bool IsValidStableId(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > 128)
        {
            return false;
        }

        var segmentStart = true;
        foreach (var character in value)
        {
            if (character == '-')
            {
                if (segmentStart)
                {
                    return false;
                }

                segmentStart = true;
                continue;
            }

            if (character is not (>= 'a' and <= 'z')
                && character is not (>= '0' and <= '9'))
            {
                return false;
            }

            segmentStart = false;
        }

        return !segmentStart;
    }
}
