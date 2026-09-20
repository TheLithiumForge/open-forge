using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Shell.Pipeline.Models.Output;

internal sealed record CliOutputWriters
{
    internal CliOutputWriters(TextWriter standardOutput, TextWriter standardError)
    {
        ArgumentNullException.ThrowIfNull(standardOutput);
        ArgumentNullException.ThrowIfNull(standardError);
        StandardOutput = standardOutput;
        StandardError = standardError;
    }

    internal CliOutputColors Colors { get; init; } = CliOutputColors.Plain;

    internal TextWriter StandardOutput { get; }

    internal TextWriter StandardError { get; }
}
