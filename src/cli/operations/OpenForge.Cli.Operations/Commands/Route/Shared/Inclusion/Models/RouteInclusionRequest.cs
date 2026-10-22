using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;

internal enum RouteInclusionAction { Include, Exclude }

internal sealed record RouteInclusionRequest
{
    public required CliWorkspace Workspace { get; init; }
    public required string File { get; init; }
    public required RouteInclusionAction Action { get; init; }
    public bool DryRun { get; init; }
    public string? Description { get; init; }
    public ImmutableArray<string>? Tags { get; init; }
    public ImmutableArray<ApplyToPattern>? ApplyTo { get; init; }
    internal string Command => Action == RouteInclusionAction.Include ? "route include" : "route exclude";
}
