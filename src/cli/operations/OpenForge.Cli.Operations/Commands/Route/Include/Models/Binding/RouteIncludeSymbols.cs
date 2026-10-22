using System.CommandLine;
namespace OpenForge.Cli.Core.Commands.Route.Include.Models.Binding;

internal sealed record RouteIncludeSymbols
{
    public required Command Command { get; init; }
    public required Argument<string?> File { get; init; }
    public required Option<bool> DryRun { get; init; }
    public required Option<string?> Description { get; init; }
    public required Option<string[]> Tag { get; init; }
    public required Option<string[]> ApplyTo { get; init; }
}
