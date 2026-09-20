using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Pipeline.Models.Output;

internal sealed record CliOutputReceipt(
    CliSemanticStatus Status,
    CliFormat Format,
    CliOutputTarget PrimaryTarget,
    bool DiagnosticWritten);
