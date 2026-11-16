using System.Text;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.OutputText.Tool.Shared.Writing;

namespace OpenForge.Cli.Core.Presentation.Tool.Shared.Writing;

internal static class ToolWriteDataTextRenderer
{
    internal static CliTextDocument Render(ToolWriteData data, CliSelection selection, CliTextStyle style)
    {
        var text = new StringBuilder();
        foreach (var grant in data.Grants)
        {
            foreach (var path in grant.Requested) Line(ToolWriteText.Grant(path, grant.State));
            foreach (var path in grant.WrittenPaths) Line(ToolWriteText.Row("Saved grant settings", path));
            foreach (var path in grant.Unfinished) Line(ToolWriteText.Row("Unfinished grant", path));
        }
        if (data.Plan.UsedNetwork) Line(ToolWriteText.Network());
        foreach (var tool in data.Tools)
        {
            Line(ToolWriteText.Tool(tool.Id, tool.Version, tool.Channel));
            if (tool.RequestedSpec is not null) Line(ToolWriteText.Row("Requested", tool.RequestedSpec));
            Line(ToolWriteText.Row("Previous version", tool.OldVersion));
            if (tool.OldProfile is not null) Line(ToolWriteText.Row("Previous profile", tool.OldProfile));
            Line(ToolWriteText.Row("Native home", tool.NativeHome));
            if (tool.AccountReplacement) Line(ToolWriteText.Replacement());
            if (tool.Manager is not null)
            {
                Line(ToolWriteText.Row("Manager", tool.Manager));
                Line(ToolWriteText.Row("Manager global bin", tool.ManagerGlobalBin));
                Line(ToolWriteText.Row("Manager PATH", tool.ManagerPathReadiness));
            }
            foreach (var effect in tool.Effects) Line(ToolWriteText.Row(effect.Scope, string.Join(" ", new[] { effect.Description, effect.Path }.OfType<string>())));
            foreach (var requirement in tool.Requirements) Line(ToolWriteText.Row("Requirement", requirement));
            Line(ToolWriteText.Row("Minimum tool version", tool.MinimumToolVersion));
            Line(ToolWriteText.Row("Minimum Open Forge version", tool.MinimumOpenForgeVersion));
            foreach (var target in tool.Targets)
            {
                List<string> requirements = [target.OperatingSystem, target.Cpu];
                if (target.Libc is not null) requirements.Add(target.Libc);
                if (target.MinimumOperatingSystemVersion is not null) requirements.Add($"OS >= {target.MinimumOperatingSystemVersion}");
                if (target.MinimumLibcVersion is not null) requirements.Add($"libc >= {target.MinimumLibcVersion}");
                if (target.MinimumLinuxKernelVersion is not null) requirements.Add($"kernel >= {target.MinimumLinuxKernelVersion}");
                if (target.MinimumLibStdCppVersion is not null) requirements.Add($"libstdc++ >= {target.MinimumLibStdCppVersion}");
                Line(ToolWriteText.Row("Target", string.Join(", ", requirements)));
            }
            foreach (var output in tool.PossibleOutputs) Line(ToolWriteText.Row("Native output",
                $"{output.Scope} {output.Match} {output.Path} {output.Purpose} conditional={output.Conditional}"));
            foreach (var download in tool.Downloads)
                Line(ToolWriteText.Row("Download", $"{download.Url} -> {download.Destination} SHA-256 {download.Sha256}"));
            foreach (var path in tool.Paths) Line(ToolWriteText.Row("Path", path));
            foreach (var backup in tool.Backups) Line(ToolWriteText.Row("Backup target", backup));
            foreach (var change in tool.Changes)
            {
                Line(ToolWriteText.Row(change.Id, $"{change.Kind} {change.Path} {change.Pointer} {change.Disposition} {change.Description}"));
                if (change.Items is { } items)
                    foreach (var item in items) Line(ToolWriteText.Row("Union item", item.GetRawText()));
                if (change.Value is { } value) Line(ToolWriteText.Row("Set value", value.GetRawText()));
                if (change.StartMarker is not null) Line(ToolWriteText.Row("Start marker", change.StartMarker));
                if (change.EndMarker is not null) Line(ToolWriteText.Row("End marker", change.EndMarker));
                if (change.Content is not null) Line(ToolWriteText.Row("Managed content", change.Content));
                if (change.ContentSha256 is not null) Line(ToolWriteText.Row("Content SHA-256", change.ContentSha256));
                if (change.TemplateResource is not null) Line(ToolWriteText.Row("Template", change.TemplateResource));
                if (change.NativeValue is not null) Line(ToolWriteText.Row("Desired native setting", change.NativeValue));
                if (change.StoredNativeValue is not null) Line(ToolWriteText.Row("Stored native setting", change.StoredNativeValue));
                if (change.NativeEnvironmentOverrides is { } environment)
                    foreach (var variable in environment) Line(ToolWriteText.EffectiveNative(variable.Key, variable.Value));
            }
            if (tool.Permissions is { } permissions)
            {
                Line(ToolWriteText.Row("Destination permission", permissions.Decision));
                foreach (var path in permissions.Required) Line(ToolWriteText.Row("Required path", path));
                foreach (var path in permissions.Missing) Line(ToolWriteText.MissingPermission(path));
            }
            foreach (var setting in tool.KeptNativeSettings) Line(ToolWriteText.KeptSetting(setting.ChangeId, setting.NativeHome, setting.Value));
            foreach (var command in tool.Commands)
            {
                Line(ToolWriteText.Row(command.Purpose, string.Join(" ", new[] { command.ExecutablePath }.Concat(command.Arguments).Select(CliCommandText.Quote))));
                Line(ToolWriteText.Row("Working directory", command.WorkingDirectory));
                foreach (var variable in command.Environment) Line(ToolWriteText.EnvironmentChange(variable.Key, variable.Value));
            }
            if (selection.Detail >= Core.Shell.Definitions.CliDetail.Full)
            {
                Line(ToolWriteText.Row("Profile", tool.Profile));
                Line(ToolWriteText.Row("Profile digest", tool.ProfileDigest));
            }
        }
        foreach (var outcome in data.Outcomes)
        {
            Line(ToolWriteText.Outcome(outcome.ToolId, outcome.Stage, outcome.State));
            foreach (var unfinished in outcome.Unfinished) Line(ToolWriteText.Row("Unfinished", unfinished));
            if (outcome.Output is { } output)
            {
                if (output.StandardOutput.Length != 0) Line(output.StandardOutput);
                if (output.StandardError.Length != 0) Line(output.StandardError);
            }
        }
        Line(ToolWriteText.Row("Open Forge bin", data.Plan.BinPath));
        Line(ToolWriteText.Row("PATH", data.Plan.PathReadiness));
        foreach (var path in data.Plan.ManualDeletions ?? []) Line(ToolWriteText.ManualDeletion(path));
        foreach (var instruction in data.Plan.PathInstructions) Line(instruction);
        foreach (var backup in data.Plan.Backups.Where(backup => backup.Backup is not null))
            Line(ToolWriteText.GuardedBackup(backup.Target, backup.Backup ?? string.Empty, backup.Verified ? backup.ExpectedSha256 : null));
        foreach (var backup in data.Plan.Backups.Where(backup => backup.Backup is null && backup.GitObjectId is not null))
            Line(ToolWriteText.GuardedGit(backup.Target, backup.GitObjectId ?? string.Empty));
        // The renderer owns the one restart reminder after manual PATH steps or a completed PATH change.
        if (data.Plan.PathInstructions.Count != 0
            || data.Outcomes.Any(outcome => outcome.Stage == ToolWriteStages.Path && outcome.State == ToolWriteStages.Completed))
            Line(ToolWriteText.Restart());
        return new([new(text.ToString())]);

        void Line(string value) => text.AppendLine(CliText.Escape(value));
    }
}
