using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Binding;

internal sealed record RouteMoveSymbols(
    Command RouteGroup,
    Command MoveCommand,
    Argument<string?> SourceReference,
    Argument<string?> DestinationTarget,
    Option<bool> DryRun);
