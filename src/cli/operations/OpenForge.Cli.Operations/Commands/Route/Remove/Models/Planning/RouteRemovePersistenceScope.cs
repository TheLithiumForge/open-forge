using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;

internal sealed record RouteRemovePersistenceScope(
    RouteRemoveSubjectKind Kind,
    ImmutableArray<string> Paths);
