using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Framework.Settings.Models.Permissions;

internal sealed record WorkspacePermissionEvaluation(
    ImmutableArray<string> Required,
    ImmutableArray<string> Missing,
    WorkspacePermissionDecision Decision);
