using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Binding;

internal sealed record RouteInspectSymbols(
    Command RouteGroup,
    Command InspectCommand,
    Argument<string[]> SourceReferences);
