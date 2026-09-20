using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Commands.References.Shared.Binding;

internal static class ReferencesSelectorOccurrenceReader
{
    internal static IReadOnlyList<SourceUniverseSelectorOccurrence> Read(
        ParseResult result,
        IReadOnlyList<string> includes,
        IReadOnlyList<string> excludes,
        CliOptionResultFacts includeFacts,
        CliOptionResultFacts excludeFacts,
        out string? cause)
    {
        var includeQueue = new Queue<string>(includes);
        var excludeQueue = new Queue<string>(excludes);
        var values = new List<SourceUniverseSelectorOccurrence>();
        var position = 0;
        var missing = (includeFacts.IsExplicit && includeFacts.ValueCount == 0)
            || (excludeFacts.IsExplicit && excludeFacts.ValueCount == 0);

        for (var index = 0; index < result.Tokens.Count; index++)
        {
            var token = result.Tokens[index];
            if (token.Type != TokenType.Option
                || !TryReadRole(token.Value, out var role, out var attachedValue))
            {
                continue;
            }

            var value = attachedValue;
            if (value is null
                && index + 1 < result.Tokens.Count
                && result.Tokens[index + 1].Type == TokenType.Argument)
            {
                value = result.Tokens[++index].Value;
            }

            var queue = role == SourceUniverseSelectorRole.Include
                ? includeQueue
                : excludeQueue;
            if (value is null)
            {
                missing = true;
                continue;
            }

            if (queue.Count != 0)
            {
                var typedValue = queue.Dequeue();
                if (!string.Equals(value, typedValue, StringComparison.Ordinal))
                {
                    missing = true;
                }
            }

            values.Add(new SourceUniverseSelectorOccurrence(role, value, ++position));
        }

        if (includeQueue.Count != 0 || excludeQueue.Count != 0)
        {
            missing = true;
        }

        var hasEmptyValue = values.Any(value => string.IsNullOrWhiteSpace(value.Value));
        if (missing)
        {
            cause = "Each include and exclude option requires one scalar source reference.";
        }
        else if (hasEmptyValue)
        {
            cause = "Include and exclude values cannot be empty.";
        }
        else
        {
            cause = null;
        }

        return values;
    }

    private static bool TryReadRole(
        string token,
        out SourceUniverseSelectorRole role,
        out string? attachedValue)
    {
        ArgumentNullException.ThrowIfNull(token);
        attachedValue = null;
        if (token.Equals(ReferencesDefinitions.Include.Name, StringComparison.Ordinal))
        {
            role = SourceUniverseSelectorRole.Include;
            return true;
        }

        if (token.Equals(ReferencesDefinitions.Exclude.Name, StringComparison.Ordinal))
        {
            role = SourceUniverseSelectorRole.Exclude;
            return true;
        }

        foreach (var separator in new[] { '=', ':' })
        {
            var includePrefix = ReferencesDefinitions.Include.Name + separator;
            if (token.StartsWith(includePrefix, StringComparison.Ordinal))
            {
                role = SourceUniverseSelectorRole.Include;
                attachedValue = token[includePrefix.Length..];
                return true;
            }

            var excludePrefix = ReferencesDefinitions.Exclude.Name + separator;
            if (token.StartsWith(excludePrefix, StringComparison.Ordinal))
            {
                role = SourceUniverseSelectorRole.Exclude;
                attachedValue = token[excludePrefix.Length..];
                return true;
            }
        }

        role = default;
        return false;
    }
}
