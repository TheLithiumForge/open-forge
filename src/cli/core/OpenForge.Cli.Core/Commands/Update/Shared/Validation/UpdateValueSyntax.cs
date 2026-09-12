namespace OpenForge.Cli.Core.Commands.Update.Shared.Validation;

internal static class UpdateValueSyntax
{
    internal static bool IsCanonicalRelative(string value)
        => !value.StartsWith('/')
            && !IsDriveQualified(value)
            && !value.Contains('\\')
            && value.Split('/').All(segment => segment.Length != 0
                && segment != "."
                && segment != ".."
                && segment.All(character => !char.IsControl(character)));

    internal static bool IsSha256(string value)
        => value.Length == 64
            && value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static bool IsDriveQualified(string value)
        => value.Length >= 2
            && char.IsAsciiLetter(value[0])
            && value[1] == ':';
}
