using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Shell.Parsing.Models.Input;

internal enum CliInvalidInputSource
{
    Parser,
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
