namespace OpenForge.Cli.Core.Shell.Parsing;

internal enum CliDelimiterShape
{
    Separate,
    Equals,
}

internal sealed record CliDelimiterPolicy(string OptionName, CliDelimiterShape RequiredShape);

internal sealed record CliDelimiterViolation(
    string OptionName,
    CliDelimiterShape RequiredShape)
{
    internal string Describe()
    {
        return RequiredShape switch
        {
            CliDelimiterShape.Separate => $"{OptionName} requires a separate value token.",
            CliDelimiterShape.Equals => $"{OptionName} requires the {OptionName}=<value> form.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(RequiredShape),
                RequiredShape,
                "The delimiter shape is not defined."),
        };
    }
}

internal static class CliDelimiterGuard
{
    internal static CliDelimiterViolation? Validate(
        IReadOnlyList<string> arguments,
        IReadOnlyList<CliDelimiterPolicy> policies)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(policies);

        foreach (var argument in arguments)
        {
            foreach (var policy in policies)
            {
                ArgumentNullException.ThrowIfNull(policy);
                if (Violates(argument, policy))
                {
                    return new CliDelimiterViolation(policy.OptionName, policy.RequiredShape);
                }
            }
        }

        return null;
    }

    private static bool Violates(string argument, CliDelimiterPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(argument);
        ArgumentException.ThrowIfNullOrWhiteSpace(policy.OptionName);
        if (argument.Equals(policy.OptionName, StringComparison.Ordinal))
        {
            return policy.RequiredShape == CliDelimiterShape.Equals;
        }

        if (!argument.StartsWith(policy.OptionName, StringComparison.Ordinal)
            || argument.Length == policy.OptionName.Length)
        {
            return false;
        }

        var delimiter = argument[policy.OptionName.Length];
        if (delimiter is not ('=' or ':'))
        {
            return false;
        }

        return policy.RequiredShape == CliDelimiterShape.Separate || delimiter != '=';
    }
}
