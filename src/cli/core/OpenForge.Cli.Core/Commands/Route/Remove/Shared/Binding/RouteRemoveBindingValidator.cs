using OpenForge.Cli.Core.Commands.Route.Remove.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Binding;

internal sealed class RouteRemoveBindingValidator
{
    internal RouteRemoveBindingValidation Validate(
        RouteRemoveBindingInput input,
        RouteRemoveMode mode)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "The Route Remove mode is not defined.");
        }

        if (string.IsNullOrWhiteSpace(input.SourceReference))
        {
            return Invalid("Route Remove requires one nonblank source reference.");
        }

        if (input.ParserErrors.Count != 0)
        {
            return Invalid(string.Join(" ", input.ParserErrors));
        }

        return RouteRemoveBindingValidation.Valid(
            new RouteRemoveBindingFacts
            {
                SourceReference = input.SourceReference,
                Mode = mode,
            });
    }

    private static RouteRemoveBindingValidation Invalid(string cause)
        => RouteRemoveBindingValidation.Invalid(
            new RouteRemoveBindingFailure(RouteRemoveFindingCode.InvalidInput, cause));
}
