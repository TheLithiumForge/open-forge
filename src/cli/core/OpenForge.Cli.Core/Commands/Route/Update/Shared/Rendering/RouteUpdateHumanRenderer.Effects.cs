using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Rendering;

internal static partial class RouteUpdateHumanRenderer
{
    private static void AppendPatch(
        StringBuilder builder,
        RouteUpdateResult result)
    {
        var patch = result.Patch;
        if (patch.Description.Requested)
        {
            builder.AppendLine($"Description ({RouteUpdateDefinitions.ReadMachineName(patch.Description.State)}): {MetadataValue(patch.Description.Before, patch.Description.State)} -> {MetadataValue(patch.Description.Expected, patch.Description.State)}");
        }

        if (patch.Responsibility.Requested)
        {
            var expected = patch.Responsibility.Operation == RouteUpdateResponsibilityOperation.Remove
                ? "absent"
                : MetadataValue(patch.Responsibility.Expected, patch.Responsibility.State);
            builder.AppendLine($"Responsibility ({RouteUpdateDefinitions.ReadMachineName(patch.Responsibility.State)}): {MetadataValue(patch.Responsibility.Before, patch.Responsibility.State)} -> {expected}");
        }

        if (patch.Tags.Requested)
        {
            builder.AppendLine($"Tags ({RouteUpdateDefinitions.ReadMachineName(patch.Tags.State)}): {Tags(patch.Tags.Before)} -> {Tags(patch.Tags.Expected)}");
        }

        if (result.Template?.Decision == RouteUpdateTemplateDecision.Copied)
        {
            builder.AppendLine("Template body: selected for copying");
        }
    }

    private static string MetadataValue(string? value, RouteUpdatePatchState state)
        => value is not null ? $"\"{Value(value)}\""
            : state == RouteUpdatePatchState.Unresolved ? "unavailable" : "absent";

    private static string Tags(ImmutableArray<string>? values)
        => values is { } tags
            ? $"[{string.Join(", ", tags.Select(tag => $"\"{Value(tag)}\""))}]"
            : "unavailable";

    private static void AppendEffects(
        StringBuilder builder,
        RouteUpdateResult result)
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
