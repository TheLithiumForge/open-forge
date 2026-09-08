using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Permissions.Models.Observation;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;

namespace OpenForge.Cli.Core.Framework.Permissions.Models.Planning;

internal sealed record WorkspacePermissionEvaluation(
    ImmutableArray<WorkspacePermissionRequirement> Required,
    ImmutableArray<WorkspacePermissionRequirement> Missing,
    WorkspacePermissionDecision Decision);

internal sealed record WorkspacePermissionChangeRequest(
    WorkspacePermissionRead Observation,
    WorkspacePermissionEvaluation Approval);
