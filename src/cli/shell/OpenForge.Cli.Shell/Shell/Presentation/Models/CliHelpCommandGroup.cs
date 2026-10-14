using System.CommandLine;

namespace OpenForge.Cli.Core.Shell.Presentation.Models;

internal sealed record CliHelpCommandGroup(string Heading, IReadOnlyList<Command> Commands);
