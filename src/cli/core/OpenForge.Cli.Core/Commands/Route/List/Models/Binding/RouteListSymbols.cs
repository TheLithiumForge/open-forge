using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Route.List.Models.Binding;

internal sealed record RouteListSymbols(
    Command RouteGroup,
    Command ListCommand,
    Argument<string?> SourceReference,
    Option<string?> Depth);
