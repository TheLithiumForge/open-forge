using System.CommandLine;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;

internal sealed record CliCommandHelp(Command Command, CliHelpContent Content);
