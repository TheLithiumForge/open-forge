using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Parsing;

internal sealed record CliGlobalInput(
    string? WorkspaceValue,
    int WorkspaceOccurrences,
    CliOutputFormat OutputFormat,
    int JsonOccurrences,
    CliView View,
    int ViewOccurrences,
    CliVerbosity Verbosity,
    int VerboseOccurrences,
    bool Help,
    int HelpOccurrences,
    bool Version,
    int VersionOccurrences)
{
    internal CliPresentation Presentation => new(OutputFormat, View, Verbosity);

    internal static CliGlobalInput Read(CliParseOutcome parse)
    {
        ArgumentNullException.ThrowIfNull(parse);
        var result = parse.Result;
        var options = parse.Options;
        var arguments = parse.OriginalArguments;
        var workspaceOccurrences = CountSeparate(arguments, CliSyntaxDefinitions.Workspace.Name);
        var jsonOccurrences = CountExact(arguments, CliSyntaxDefinitions.Json.Name);
        var viewOccurrences = CountEquals(arguments, CliSyntaxDefinitions.View.Name);
        var verboseOccurrences = CountExact(arguments, CliSyntaxDefinitions.Verbose.Name);
        var helpOccurrences = CountExact(arguments, CliSyntaxDefinitions.Help.Name);
        var versionOccurrences = CountExact(arguments, CliSyntaxDefinitions.Version.Name);
        return new CliGlobalInput(
            ReadSeparateValue(arguments, CliSyntaxDefinitions.Workspace.Name)
                ?? result.GetValue(options.Workspace),
            workspaceOccurrences,
            jsonOccurrences > 0 ? CliOutputFormat.Json : CliOutputFormat.Human,
            jsonOccurrences,
            ReadView(arguments, result.GetValue(options.View)),
            viewOccurrences,
            verboseOccurrences > 0 ? CliVerbosity.Verbose : CliVerbosity.Normal,
            verboseOccurrences,
            helpOccurrences > 0,
            helpOccurrences,
            versionOccurrences > 0,
            versionOccurrences);
    }

    internal static CliGlobalInput ReadAvailable(CliParseOutcome parse)
    {
        ArgumentNullException.ThrowIfNull(parse);
        var arguments = parse.OriginalArguments;
        var workspaceOccurrences = CountSeparate(arguments, CliSyntaxDefinitions.Workspace.Name);
        var jsonOccurrences = CountExact(arguments, CliSyntaxDefinitions.Json.Name);
        var viewOccurrences = CountEquals(arguments, CliSyntaxDefinitions.View.Name);
        var verboseOccurrences = CountExact(arguments, CliSyntaxDefinitions.Verbose.Name);
        var helpOccurrences = CountExact(arguments, CliSyntaxDefinitions.Help.Name);
        var versionOccurrences = CountExact(arguments, CliSyntaxDefinitions.Version.Name);

        return new CliGlobalInput(
            ReadSeparateValue(arguments, CliSyntaxDefinitions.Workspace.Name),
            workspaceOccurrences,
            jsonOccurrences > 0 ? CliOutputFormat.Json : CliOutputFormat.Human,
            jsonOccurrences,
            ReadView(arguments, CliView.Expanded),
            viewOccurrences,
            verboseOccurrences > 0 ? CliVerbosity.Verbose : CliVerbosity.Normal,
            verboseOccurrences,
            helpOccurrences > 0,
            helpOccurrences,
            versionOccurrences > 0,
            versionOccurrences);
    }

    private static string? ReadSeparateValue(
        IReadOnlyList<string> arguments,
        string optionName)
    {
        for (var index = 0; index < arguments.Count - 1; index++)
        {
            if (string.Equals(arguments[index], "--", StringComparison.Ordinal))
            {
                break;
            }

            if (string.Equals(arguments[index], optionName, StringComparison.Ordinal))
            {
                return arguments[index + 1];
            }
        }

        return null;
    }

    private static CliView ReadView(
        IReadOnlyList<string> arguments,
        CliView fallback)
    {
        var prefix = $"{CliSyntaxDefinitions.View.Name}=";
        foreach (var argument in arguments)
        {
            if (string.Equals(argument, "--", StringComparison.Ordinal))
            {
                break;
            }

            if (!argument.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            try
            {
                return CliPresentationDefinitions.ParseView(argument[prefix.Length..]);
            }
            catch (ArgumentOutOfRangeException)
            {
                return fallback;
            }
        }

        return fallback;
    }

    private static int CountExact(
        IReadOnlyList<string> arguments,
        string optionName)
    {
        return arguments
            .TakeWhile(argument => !string.Equals(argument, "--", StringComparison.Ordinal))
            .Count(argument => string.Equals(argument, optionName, StringComparison.Ordinal));
    }

    private static int CountSeparate(
        IReadOnlyList<string> arguments,
        string optionName)
    {
        return CountExact(arguments, optionName);
    }

    private static int CountEquals(
        IReadOnlyList<string> arguments,
        string optionName)
    {
        var prefix = $"{optionName}=";
        return arguments
            .TakeWhile(argument => !string.Equals(argument, "--", StringComparison.Ordinal))
            .Count(argument => argument.StartsWith(prefix, StringComparison.Ordinal));
    }
}

internal enum CliInvalidInputSource
{
    Parser,
    Delimiter,
    Semantic,
    Workspace,
}

internal sealed class CliInvalidInput
{
    internal CliInvalidInput(
        string code,
        CliInvalidInputSource source,
        IEnumerable<string> diagnostics)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentNullException.ThrowIfNull(diagnostics);
        var materialized = diagnostics.ToArray();
        if (materialized.Length == 0 || materialized.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("At least one non-empty diagnostic is required.", nameof(diagnostics));
        }

        Code = code;
        Source = source;
        Diagnostics = new ReadOnlyCollection<string>(materialized);
    }

    internal string Code { get; }

    internal CliInvalidInputSource Source { get; }

    internal IReadOnlyList<string> Diagnostics { get; }
}

internal sealed record CliGlobalInputResolution(
    CliGlobalInput? Input,
    CliTerminalMode TerminalMode,
    CliInvalidInput? InvalidInput);

internal static class CliTerminalValidator
{
    internal static CliGlobalInputResolution Validate(CliParseOutcome parse)
    {
        ArgumentNullException.ThrowIfNull(parse);
        if (parse.Result.Errors.Count > 0)
        {
            return Invalid(
                "cli.parser.invalid",
                CliInvalidInputSource.Parser,
                parse.Result.Errors.Select(error => error.Message));
        }

        var delimiterViolation = CliDelimiterGuard.Validate(parse.OriginalArguments, parse.DelimiterPolicies);
        if (delimiterViolation is not null)
        {
            return Invalid(
                "cli.delimiter.invalid",
                CliInvalidInputSource.Delimiter,
                [delimiterViolation.Describe()]);
        }

        var input = CliGlobalInputReader.Read(parse);
        try
        {
            var terminalMode = CliTerminalPolicy.Resolve(input.Help, input.Version);
            return new CliGlobalInputResolution(input, terminalMode, null);
        }
        catch (ArgumentException exception)
        {
            return Invalid(
                "cli.terminal.conflict",
                CliInvalidInputSource.Semantic,
                [exception.Message]);
        }
    }

    private static CliGlobalInputResolution Invalid(
        string code,
        CliInvalidInputSource source,
        IEnumerable<string> diagnostics)
    {
        return new CliGlobalInputResolution(
            null,
            CliTerminalMode.None,
            new CliInvalidInput(code, source, diagnostics));
    }
}
