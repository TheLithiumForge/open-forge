using System.Text;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Rendering;

internal static class LibraryPermissionHumanRenderer
{
    internal static void Append(StringBuilder builder, LibraryPermissionView permissions, CliView view)
    {
        builder.AppendLine();
        builder.AppendLine($"Permissions: {LibraryHumanText.Value(permissions.Decision)}; action {LibraryHumanText.Value(permissions.Action)}; outcome {LibraryHumanText.Value(permissions.Outcome)}");
        if (view == CliView.Expanded)
        {
            builder.AppendLine($"  Record: {LibraryHumanText.Value(permissions.Path)}");
            foreach (var required in permissions.Required)
            {
                builder.AppendLine($"  Required: {LibraryHumanText.Value(required.Path)}; Library {LibraryHumanText.Value(required.Id)}; source {LibraryHumanText.Value(required.SourceRoot)}");
            }
        }
        foreach (var missing in permissions.Missing)
        {
            builder.AppendLine($"  MISSING: {LibraryHumanText.Value(missing.Path)}; Library {LibraryHumanText.Value(missing.Id)}; source {LibraryHumanText.Value(missing.SourceRoot)}");
        }
        AppendScopes(builder, permissions.ProposedScopes, "Proposed");
        AppendScopes(builder, permissions.ApprovedScopes, "Approved");
        if (permissions.Rebinding is { } rebinding)
        {
            builder.AppendLine($"  Source permission change: {LibraryHumanText.Value(rebinding.PreviousSourceRoot)} -> {LibraryHumanText.Value(rebinding.SourceRoot)}");
        }
    }

    private static void AppendScopes(StringBuilder builder, LibraryPermissionScopeView[] scopes, string label)
    {
        foreach (var scope in scopes)
        {
            builder.AppendLine($"  {label}: {LibraryHumanText.Value(scope.Kind)} {LibraryHumanText.Value(scope.Path)}; Library {LibraryHumanText.Value(scope.Id)}; source {LibraryHumanText.Value(scope.SourceRoot)}");
        }
    }
}
