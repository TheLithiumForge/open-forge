using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Route.Init.Models.Binding;

internal sealed record RouteInitSymbols(
    Command RouteGroup,
    Command InitCommand,
    Argument<string?> RouteTarget,
    Option<bool> Framework,
    Option<string?> Description,
    Option<string?> Responsibility,
    Option<string[]> Tag,
    Option<bool> DryRun);
