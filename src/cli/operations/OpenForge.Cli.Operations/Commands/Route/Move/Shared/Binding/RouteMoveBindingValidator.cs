using OpenForge.Cli.Core.Commands.Route.Move.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Binding;

internal sealed class RouteMoveBindingValidator
{
    internal RouteMoveBindingValidation Validate(
        RouteMoveBindingInput input,
        RouteMoveMode mode)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "The Route Move mode is not defined.");
        }

        if (string.IsNullOrWhiteSpace(input.SourceReference))
        {
            return Invalid("Route Move requires one nonblank source reference.");
        }

        if (string.IsNullOrWhiteSpace(input.DestinationTarget))
        {
            return Invalid("Route Move requires one nonblank destination target.");
        }

        if (input.ParserErrors.Count != 0)
        {
            return Invalid(string.Join(" ", input.ParserErrors));
        }

        return RouteMoveBindingValidation.Valid(
            new RouteMoveBindingFacts
            {
                SourceReference = input.SourceReference,
                DestinationTarget = input.DestinationTarget,
                Mode = mode,
            });
    }

    private static RouteMoveBindingValidation Invalid(string cause)
        => RouteMoveBindingValidation.Invalid(
            new RouteMoveBindingFailure(RouteMoveFindingCode.InvalidInput, cause));
}
