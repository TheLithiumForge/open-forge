using System.Text;
using OpenForge.Cli.Core.Framework.Permissions;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Serialization;

namespace OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;

internal static class ExtensionPermissionPresentation
{
    internal static void Append(StringBuilder builder, WorkspacePermissionResult result)
    {
        builder.AppendLine($"""
            Permissions: {WorkspacePermissionJsonProjection.ReadName(result.Decision)}
              Document: {WorkspacePermissionDefinitions.RelativePath}
              Action: {WorkspacePermissionJsonProjection.ReadName(result.Action)}
              Outcome: {WorkspacePermissionJsonProjection.ReadName(result.Outcome)}
            """);
        foreach (var requirement in result.Required)
        {
            builder.AppendLine($"  Required: {requirement.Subject.Id}: {requirement.Path}");
        }
        foreach (var missing in result.Missing)
        {
            builder.AppendLine($"  Missing when reviewed: {missing.Subject.Id}: {missing.Path}");
        }
    }
}
