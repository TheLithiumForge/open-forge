using System.Text;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Rendering;

internal static partial class RouteUpdateHumanRenderer
{
    private static void AppendChanged(
        StringBuilder builder,
        RouteUpdateResult result)
    {
        var changed = new List<string>(capacity: 4);
        if (result.Patch.Description.State == RouteUpdatePatchState.Changed)
        {
            changed.Add("description");
        }

        if (result.Patch.Responsibility.State == RouteUpdatePatchState.Changed)
        {
            changed.Add("responsibility");
        }

        if (result.Patch.Tags.State == RouteUpdatePatchState.Changed)
        {
            changed.Add("tags");
        }

        if (result.Template?.Decision == RouteUpdateTemplateDecision.Copied)
        {
            changed.Add("Template body");
        }

        if (changed.Count > 0)
        {
            builder.AppendLine($"Changed: {string.Join(", ", changed)}");
        }
    }

    private static void AppendEffects(
        StringBuilder builder,
        RouteUpdateResult result,
        bool showPreview)
    {
        if (result.Effects.IsEmpty)
        {
            builder.AppendLine("No files changed.");
            return;
        }

        builder.AppendLine("Effects:");
        foreach (var effect in result.Effects)
        {
            builder.AppendLine(
                $"  {Value(effect.Path)}: {RouteUpdateDefinitions.ReadMachineName(effect.Action)} {RouteUpdateDefinitions.ReadMachineName(effect.Kind)} / {RouteUpdateDefinitions.ReadMachineName(effect.Outcome)} / residual={RouteUpdateDefinitions.ReadMachineName(effect.Residual)}");
            if (!showPreview)
            {
                continue;
            }

            builder.AppendLine($"    Before: {Value(effect.Change.Before)}");
            builder.AppendLine($"    Expected: {Value(effect.Change.Expected)}");
            foreach (var preview in effect.Preview)
            {
                builder.AppendLine(
                    $"    {RouteUpdateDefinitions.ReadMachineName(preview.Kind)}: {Value(preview.Before)} -> {Value(preview.Expected)}");
            }
        }
    }

    private static void AppendUnchanged(
        StringBuilder builder,
        IReadOnlyList<string> unchangedPaths)
    {
        if (unchangedPaths.Count == 0)
        {
            return;
        }

        builder.AppendLine("Unchanged:");
        foreach (var path in unchangedPaths)
        {
            builder.AppendLine($"  {Value(path)}");
        }
    }
}
