using System.Globalization;

namespace OpenForge.Cli.OutputText.Library.Shared;

internal static class LibrarySharedPhrases
{
    // @OpenForgeText library.shared.phrase.is-not-a-valid-library-id-use-lowercase-letters-digits-and-hyphens
    internal static string FormatIsNotAValidLibraryIdUseLowercaseLettersDigitsAndHyphens(string suppliedText)
        => $"{suppliedText} is not a valid Library ID. Use lowercase letters, digits and hyphens.";

    // @OpenForgeText library.shared.phrase.could-not-be-checked-safely
    internal static string FormatCouldNotBeCheckedSafely(string pathText, string reasonText)
        => $"{pathText}  could not be checked safely: {reasonText}.";

    // @OpenForgeText library.shared.phrase.expected-target
    internal static string FormatExpectedTarget(string valueText)
        => $"Expected target: {valueText}";

    // @OpenForgeText library.shared.phrase.observed-target
    internal static string FormatObservedTarget(string valueText)
        => $"Observed target: {valueText}";
}
