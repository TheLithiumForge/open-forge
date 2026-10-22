using System.CommandLine;
namespace OpenForge.Cli.Core.Commands.Route.Exclude.Models.Binding;

internal sealed record RouteExcludeSymbols
{
    public required Command Command { get; init; }
    public required Argument<string?> File { get; init; }
    public required Option<bool> DryRun { get; init; }
}
