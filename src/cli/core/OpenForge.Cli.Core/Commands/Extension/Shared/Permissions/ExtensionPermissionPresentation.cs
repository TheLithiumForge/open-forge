using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Permissions;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;

internal static class ExtensionPermissionPresentation
{
    internal static void Append(StringBuilder builder, WorkspacePermissionResult result, CliView view)
    {
        builder.AppendLine($"Permissions: {WorkspacePermissionJsonProjection.ReadName(result.Decision)}; record {WorkspacePermissionJsonProjection.ReadName(result.Action)}; {WorkspacePermissionJsonProjection.ReadName(result.Outcome)}");
        foreach (var missing in result.Missing)
        {
            builder.AppendLine($"  Missing when reviewed: {ExtensionHumanText.Value(missing.Subject.Id)}: {ExtensionHumanText.Value(missing.Path)}");
        }

        if (view == CliView.Expanded)
        {
            builder.AppendLine($"  Document: {WorkspacePermissionDefinitions.RelativePath}");
            foreach (var requirement in result.Required)
            {
                builder.AppendLine($"  Required: {ExtensionHumanText.Value(requirement.Subject.Id)}: {ExtensionHumanText.Value(requirement.Path)}");
            }
        }
    }
}
