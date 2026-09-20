using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Binding;

/// <summary>
/// Validation shared by the Route binders. Route Create and Route Update accept
/// the same single-valued options and refuse them in the same words, so the rule
/// is stated once.
/// </summary>
internal static class RouteOptionValidation
{
    /// <summary>
    /// The refusal for an option that must appear at most once with one value, or
    /// null when it is acceptable. With <paramref name="allowExactEmpty"/> an
    /// explicitly empty value is accepted, which is how a Route option is cleared.
    /// </summary>
    internal static string? ValidateSingleton(
        string optionName,
        CliOptionResultFacts facts,
        string? value,
        bool allowExactEmpty)
    {
        if (facts.IdentifierCount > 1)
        {
            return Refusal(optionName);
        }

        if (facts.IsExplicitWithoutValue)
        {
            return allowExactEmpty && value is { Length: 0 }
                ? null
                : Refusal(optionName);
        }

        return facts.IsExplicit
            && (value is null || !allowExactEmpty && value.Length == 0)
                ? Refusal(optionName)
                : null;
    }

    private static string Refusal(string optionName)
        => $"{optionName} accepts exactly one value.";
}
