using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Commands.Route.Create.Models.Binding;

internal sealed record RouteCreateBindingInput
{
    public string? Target { get; init; }

    public string? Description { get; init; }

    public required IReadOnlyList<string> Tags { get; init; }

    public string? Responsibility { get; init; }

    public string? Template { get; init; }

    public required CliOptionResultFacts DescriptionFacts { get; init; }

    public required CliOptionResultFacts TagFacts { get; init; }

    public required CliOptionResultFacts ResponsibilityFacts { get; init; }

    public required CliOptionResultFacts TemplateFacts { get; init; }

    public required IReadOnlyList<string> ParserErrors { get; init; }
}

internal sealed record RouteCreateBindingFailure(
    RouteCreateFindingCode Code,
    string Cause);

internal sealed record RouteCreateBindingFacts
{
    public required string Target { get; init; }

    public required string? Description { get; init; }

    public required IReadOnlyList<string> Tags { get; init; }

    public string? Responsibility { get; init; }

    public string? Template { get; init; }

    public required RouteCreateMode Mode { get; init; }
}

internal sealed record RouteCreateBindingValidation
{
    private RouteCreateBindingValidation(
        RouteCreateBindingFacts? facts,
        RouteCreateBindingFailure? failure)
    {
        if ((facts is null) == (failure is null))
        {
            throw new ArgumentException(
                "Binding validation must contain exactly one valid facts or failure value.");
        }

        Facts = facts;
        Failure = failure;
    }

    public RouteCreateBindingFacts? Facts { get; }

    public RouteCreateBindingFailure? Failure { get; }

    internal static RouteCreateBindingValidation Valid(RouteCreateBindingFacts facts)
        => new(facts: facts, failure: null);

    internal static RouteCreateBindingValidation Invalid(RouteCreateBindingFailure failure)
        => new(facts: null, failure: failure);
}
