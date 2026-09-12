using System.CommandLine;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Binding;

internal sealed record RouteUpdateSymbols
{
    public required Command RouteGroup { get; init; }

    public required Command UpdateCommand { get; init; }

    public required Argument<string?> SourceReference { get; init; }

    public required Option<string?> Description { get; init; }

    public required Option<string[]> Tag { get; init; }

    public required Option<string?> Responsibility { get; init; }

    public required Option<string?> Template { get; init; }

    public required Option<bool> DryRun { get; init; }

    public required IReadOnlyList<CliDelimiterPolicy> DelimiterPolicies { get; init; }
}
