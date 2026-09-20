namespace OpenForge.Cli.Core.Shell.Definitions.Models;

internal sealed record CliStatusDefinition(
    CliSemanticStatus Status,
    string MachineName,
    CliProcessDisposition Disposition);
