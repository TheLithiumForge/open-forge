using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;

internal enum RouteInclusionTargetKind { File, Folder }

internal sealed record RouteInclusionTarget
{
    public required string Path { get; init; }
    public required RouteInclusionTargetKind Kind { get; init; }
    public string? CataloguePath { get; init; }
    public WorkspaceOutsideRouteRead? DirectoryRead { get; init; }
    public ImmutableArray<FileExpectation> Expectations { get; init; } = [];
    internal string LoaderPath => CataloguePath ?? Path;
}
