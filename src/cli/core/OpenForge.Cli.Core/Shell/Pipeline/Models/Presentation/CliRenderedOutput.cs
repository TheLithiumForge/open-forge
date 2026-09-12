using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

internal sealed record CliRenderedOutput(
    CliSemanticStatus Status,
    CliOutputFormat Format,
    CliOutputTarget PrimaryTarget,
    string PrimaryContent,
    string? DiagnosticContent);
