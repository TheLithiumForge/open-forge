using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Binding;

internal sealed record RouteUpdateBindingInput
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

internal sealed record RouteUpdateBindingFailure(
    RouteUpdateFindingCode Code,
    string Cause);

internal sealed record RouteUpdateBindingFacts
{
    public required string Target { get; init; }

    public required RouteUpdatePatchRequest Patch { get; init; }

    public string? Template { get; init; }

    public required RouteUpdateMode Mode { get; init; }
}

internal sealed record RouteUpdateBindingValidation
{
    private RouteUpdateBindingValidation(
        RouteUpdateBindingFacts? facts,
        RouteUpdateBindingFailure? failure)
    {
        if ((facts is null) == (failure is null))
        {
            throw new ArgumentException(
                "Binding validation must contain exactly one valid facts or failure value.");
        }

        Facts = facts;
        Failure = failure;
    }

    public RouteUpdateBindingFacts? Facts { get; }

    public RouteUpdateBindingFailure? Failure { get; }

    internal static RouteUpdateBindingValidation Valid(RouteUpdateBindingFacts facts)
        => new(facts: facts, failure: null);

    internal static RouteUpdateBindingValidation Invalid(RouteUpdateBindingFailure failure)
        => new(facts: null, failure: failure);
}
