using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Permissions;
using OpenForge.Cli.Core.Framework.Permissions.Models.Planning;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Serialization;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Permissions;

internal static class LibraryPermissionPresentation
{
    internal static LibraryPermissionView Project(LibraryPermissionStage stage)
        => new()
        {
            Path = WorkspacePermissionDefinitions.RelativePath,
            Required = [.. stage.Result.Required.Select(ProjectLeaf)],
            Missing = [.. stage.Result.Missing.Select(ProjectLeaf)],
            ProposedScopes = [.. (stage.Approval?.ProposedScopes ?? []).Select(ProjectScope)],
            ApprovedScopes = [.. (stage.Approval?.ApprovedScopes ?? []).Select(ProjectScope)],
            Rebinding = stage.Approval?.Rebinding,
            Decision = WorkspacePermissionJsonProjection.ReadName(stage.Result.Decision),
            Action = WorkspacePermissionJsonProjection.ReadName(stage.Result.Action),
            Outcome = WorkspacePermissionJsonProjection.ReadName(stage.Result.Outcome),
        };

    internal static string RenderPrompt(LibraryPermissionRequest request, LibraryPermissionApproval approval)
    {
        var leaves = string.Join('\n', approval.Leaves.Missing.Select(leaf =>
            $"  {leaf.Path} ({ReadEffectName(request.Targets.Single(target => target.DestinationPath == leaf.Path).Effect)})"));
        var scopes = string.Join('\n', approval.ProposedScopes.Select(scope =>
            $"  {scope.Path} ({ReadScopeKind(scope.Kind)})"));
        var rebinding = approval.Rebinding is { } replacement
            ? $"Replace Library permissions for source '{replacement.PreviousSourceRoot}' with permissions for '{replacement.SourceRoot}'.\n"
            : string.Empty;
        return $"""
            Library: {request.Library.Id.Value}
            Source: {request.Library.SourceRoot.Value}
            {rebinding}Allow these destinations and remember this permission in this workspace?
            Destinations requiring permission:
            {leaves}
            Remembered scopes:
            Directory grants include all future descendants; file grants cover only the listed file.
            {scopes}
            [y/N]
            """ + " ";
    }

    internal static LibraryPermissionEffect ReadEffect(RelativeFileLinkEffectKind? kind)
        => kind switch
        {
            RelativeFileLinkEffectKind.Create => LibraryPermissionEffect.CreateLink,
            RelativeFileLinkEffectKind.Delete => LibraryPermissionEffect.RemoveLink,
            null => LibraryPermissionEffect.RetainLink,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Library link effect kind is not defined."),
        };

    internal static string ReadEffectName(LibraryPermissionEffect effect)
        => effect switch
        {
            LibraryPermissionEffect.CreateLink => "create link",
            LibraryPermissionEffect.RetainLink => "retain link",
            LibraryPermissionEffect.RemoveLink => "remove link",
            _ => throw new ArgumentOutOfRangeException(nameof(effect), effect, "The Library permission effect is not defined."),
        };

    private static LibraryPermissionLeafView ProjectLeaf(WorkspacePermissionRequirement requirement)
        => requirement.Subject is LibraryPermissionSubject subject
            ? new(subject.Id, subject.SourceRoot, requirement.Path)
            : throw new InvalidOperationException("A Library permission result requires a Library subject.");

    private static LibraryPermissionScopeView ProjectScope(LibraryPermissionScope scope)
        => new(scope.Subject.Id, scope.Subject.SourceRoot, ReadScopeKind(scope.Kind), scope.Path);

    private static string ReadScopeKind(LibraryPermissionScopeKind kind)
        => kind switch
        {
            LibraryPermissionScopeKind.File => "file",
            LibraryPermissionScopeKind.Directory => "directory",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Library permission scope kind is not defined."),
        };
}
