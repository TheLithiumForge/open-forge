using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Commands.Shared.Permissions;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Permissions;

internal static class LibraryPermissionPresentation
{
    internal static LibraryPermissionView Project(LibraryPermissionStage stage)
        => new()
        {
            Path = WorkspaceSettingsDefinitions.RelativePath,
            Required = [.. stage.Result.Required],
            Missing = [.. stage.Result.Missing],
            ProposedScopes = [.. (stage.Approval?.ProposedScopes ?? []).Select(ProjectScope)],
            ApprovedScopes = [.. (stage.Approval?.ApprovedScopes ?? []).Select(ProjectScope)],
            Decision = WorkspacePermissionJsonProjection.ReadName(stage.Result.Decision),
            Action = WorkspacePermissionJsonProjection.ReadName(stage.Result.Action),
            Outcome = WorkspacePermissionJsonProjection.ReadName(stage.Result.Outcome),
        };

    internal static LibraryPermissionEffect ReadEffect(RelativeFileLinkEffectKind? kind)
        => kind switch
        {
            RelativeFileLinkEffectKind.Create => LibraryPermissionEffect.CreateLink,
            RelativeFileLinkEffectKind.Delete => LibraryPermissionEffect.RemoveLink,
            null => LibraryPermissionEffect.RetainLink,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Library link effect kind is not defined."),
        };

    private static LibraryPermissionScopeView ProjectScope(LibraryPermissionScope scope)
        => new(ReadScopeKind(scope.Kind), scope.Path);

    private static string ReadScopeKind(LibraryPermissionScopeKind kind)
        => kind switch
        {
            LibraryPermissionScopeKind.File => "file",
            LibraryPermissionScopeKind.Directory => "directory",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Library permission scope kind is not defined."),
        };
}
