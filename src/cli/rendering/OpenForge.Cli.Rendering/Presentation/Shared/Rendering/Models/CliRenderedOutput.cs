using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;

namespace OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

internal sealed record CliRenderedOutput(
    CliSemanticStatus Status,
    CliFormat Format,
    CliOutputTarget PrimaryTarget,
    string PrimaryContent,
    string? DiagnosticContent)
{
    internal CliTextDocument? TextDocument { get; init; }
}
