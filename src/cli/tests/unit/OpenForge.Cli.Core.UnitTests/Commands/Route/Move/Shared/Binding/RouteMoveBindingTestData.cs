using OpenForge.Cli.Core.Commands.Route.Move.Shared.Binding;
using OpenForge.Cli.Core.Commands.Route.Move;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Move;

internal static class RouteMoveBindingTestData
{
    internal static RouteMoveBinding CreateBinding()
        => new(
            new RouteMoveBindingValidator(),
            new RouteMoveInvalidResultFactory());
}
