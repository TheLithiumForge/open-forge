using System.CommandLine;
using OpenForge.Cli.Core.Shell.Parsing;

namespace OpenForge.Cli.Core.Commands.Route.Create.Models.Binding;

internal sealed record RouteCreateSymbols
{
    public required Command RouteGroup { get; init; }

    public required Command CreateCommand { get; init; }

    public required Argument<string?> FileTarget { get; init; }

    public required Option<string?> Description { get; init; }

    public required Option<string[]> Tag { get; init; }

    public required Option<string?> Responsibility { get; init; }

    public required Option<string?> Template { get; init; }

    public required Option<bool> DryRun { get; init; }

    public required IReadOnlyList<CliDelimiterPolicy> DelimiterPolicies { get; init; }
}
