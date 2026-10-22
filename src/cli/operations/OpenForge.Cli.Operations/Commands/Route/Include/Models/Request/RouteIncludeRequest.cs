using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Route.Include.Models.Request;

internal sealed record RouteIncludeRequest
{
    public required CliWorkspace Workspace { get; init; }
    public required string File { get; init; }
    public bool DryRun { get; init; }
    public string? Description { get; init; }
    public ImmutableArray<string>? Tags { get; init; }
    public ImmutableArray<ApplyToPattern>? ApplyTo { get; init; }

    internal RouteInclusionRequest ToInclusion() => new()
    {
        Workspace = Workspace, File = File, DryRun = DryRun, Action = RouteInclusionAction.Include,
        Description = Description, Tags = Tags, ApplyTo = ApplyTo,
    };
}
