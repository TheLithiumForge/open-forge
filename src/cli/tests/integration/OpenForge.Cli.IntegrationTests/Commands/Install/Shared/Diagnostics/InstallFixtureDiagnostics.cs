using System.Text;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Diagnostics;

internal static class InstallFixtureDiagnostics
{
    internal static string? Failure(InstallResult result)
    {
        if (result.Status == CliSemanticStatus.Complete && result.Findings.Count == 0) return null;
        var message = new StringBuilder();
        message.AppendLine($"Install fixture: {result.Status}; workspace: {result.WorkspacePath}; root exists: {Directory.Exists(result.WorkspacePath)}");
        foreach (var finding in result.Findings)
        {
            message.AppendLine($"{finding.Code}: target {finding.Subject ?? "(unspecified)"}: {finding.Cause}");
        }
        foreach (var effect in result.Facts.Effects)
        {
            message.AppendLine($"Effect: {effect.Path}; {effect.Action}; {effect.Outcome}");
            if (result.WorkspacePath is { } root)
            {
                var parent = Path.GetDirectoryName(Path.Combine(root, effect.Path));
                message.AppendLine($"Parent: {parent}; exists: {Directory.Exists(parent)}");
            }
        }
        message.AppendLine($"Recovery: {result.Facts.Recovery.State}; residual: {result.Facts.Recovery.ResidualPath}; exists: {File.Exists(result.Facts.Recovery.ResidualPath)}");
        return message.ToString();
    }
}
