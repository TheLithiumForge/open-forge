using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Presentation.Models;

internal sealed record CliPresentation(
    CliOutputFormat Format,
    CliView View,
    CliVerbosity Verbosity);
