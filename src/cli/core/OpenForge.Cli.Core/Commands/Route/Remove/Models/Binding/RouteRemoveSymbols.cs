using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Binding;

internal sealed record RouteRemoveSymbols(
    Command RouteGroup,
    Command RemoveCommand,
    Argument<string?> SourceReference,
    Option<bool> DryRun);
