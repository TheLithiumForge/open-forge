using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;

internal sealed record RouteMovePostMoveObservation(
    string Meaning,
    FileExpectation Expectation);
