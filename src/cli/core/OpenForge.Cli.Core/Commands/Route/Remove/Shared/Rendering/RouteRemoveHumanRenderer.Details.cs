using System.Text;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Rendering;

internal static partial class RouteRemoveHumanRenderer
{
    private static void AppendSubject(StringBuilder builder, RouteRemoveSubject subject)
    {
        builder.AppendLine("Subject layers:");
        foreach (var layer in subject.Layers)
        {
            builder.AppendLine(
                $"  {RouteRemoveDefinitions.ReadMachineName(layer.Layer)}: {Value(layer.SourcePath)}");
        }

        if (!subject.Items.IsEmpty)
        {
            builder.AppendLine("Category items:");
        }

        foreach (var item in subject.Items)
        {
            var kind = RouteRemoveDefinitions.ReadMachineName(item.Kind);
            var sourcePath = Value(item.SourcePath);
            var relativePath = Value(item.RelativePath);
            var layer = Optional(item.Layer, RouteRemoveDefinitions.ReadMachineName);
            var sourceId = Value(item.SourceId);
            builder.AppendLine($"  {kind}: {sourcePath} / relative={relativePath} / layer={layer} / id={sourceId}");
        }
    }

    private static void AppendOwnership(
        StringBuilder builder,
        RouteRemoveOwnership ownership,
        bool showClaims)
    {
        var state = RouteRemoveDefinitions.ReadMachineName(ownership.State);
        var framework = RouteRemoveDefinitions.ReadMachineName(ownership.Framework);
        var extensions = RouteRemoveDefinitions.ReadMachineName(ownership.Extensions);
        builder.AppendLine($"Ownership: {state} / framework={framework} / extensions={extensions}");
        if (!showClaims)
        {
            return;
        }

        foreach (var claim in ownership.Claims)
        {
            builder.AppendLine(
                $"  Claim: {Value(claim.Path)} / {RouteRemoveDefinitions.ReadMachineName(claim.Manager)} / {Value(claim.Owner)}");
        }
    }

    private static void AppendReferences(StringBuilder builder, RouteRemoveReferences references)
    {
        var coverage = RouteRemoveDefinitions.ReadMachineName(references.Coverage);
        builder.AppendLine($"References: coverage={coverage}, scanned={references.ScannedSourceCount}, inspected={references.InspectedSourceCount}, occurrences={references.OccurrenceCount}");
        foreach (var detachment in references.Detachments)
        {
            builder.AppendLine(
                $"  {Value(detachment.SourcePath)}: {Value(detachment.Before)} -> {Value(detachment.Expected)}");
            builder.AppendLine(
                $"    Detached: {Value(detachment.OriginalDestination)} / label={Value(detachment.VisibleLabel)}");
        }
    }

    private static void AppendGeneratedNavigation(
        StringBuilder builder,
        RouteRemoveGeneratedNavigation navigation)
    {
        builder.AppendLine(
            $"Generated navigation: {RouteRemoveDefinitions.ReadMachineName(navigation.Coverage)}");
        foreach (var region in navigation.Regions)
        {
            var reasons = string.Join(", ", region.Reasons.Select(RouteRemoveDefinitions.ReadMachineName));
            builder.AppendLine(
                $"  {Value(region.Path)}: {RouteRemoveDefinitions.ReadMachineName(region.State)} / reasons={reasons}");
        }
    }

    private static void AppendEffects(StringBuilder builder, IReadOnlyList<RouteRemoveEffect> effects)
    {
        builder.AppendLine("Effects:");
        foreach (var effect in effects)
        {
            var path = Value(effect.Path);
            var action = RouteRemoveDefinitions.ReadMachineName(effect.Action);
            var kind = RouteRemoveDefinitions.ReadMachineName(effect.Kind);
            var before = PathState(effect.Before);
            var expected = PathState(effect.Expected);
            var outcome = RouteRemoveDefinitions.ReadMachineName(effect.Outcome);
            var residual = RouteRemoveDefinitions.ReadMachineName(effect.Residual);
            builder.AppendLine($"  {path}: {action} {kind} / {before} -> {expected} / {outcome} / residual={residual}");
        }
    }

    private static string PathState(RouteRemovePathState state)
    {
        var kind = RouteRemoveDefinitions.ReadMachineName(state.Kind);
        return state.ContentSha256 is null
            ? kind
            : $"{kind}:{state.ContentSha256}";
    }

    private static void AppendUnchanged(StringBuilder builder, IReadOnlyList<string> paths)
    {
        if (paths.Count == 0)
        {
            return;
        }

        builder.AppendLine("Unchanged:");
        foreach (var path in paths)
        {
            builder.AppendLine($"  {Value(path)}");
        }
    }

    private static void AppendRecovery(
        StringBuilder builder,
        RouteRemoveRecovery recovery,
        bool showProtectedPaths)
    {
        builder.AppendLine(
            $"Recovery: {RouteRemoveDefinitions.ReadMachineName(recovery.State)} / residual={Value(recovery.ResidualPath)}");
        if (!showProtectedPaths)
        {
            return;
        }

        foreach (var path in recovery.ProtectedPaths)
        {
            builder.AppendLine($"  Protected: {Value(path)}");
        }
    }

    private static void AppendFindings(
        StringBuilder builder,
        IReadOnlyList<RouteRemoveFinding> findings,
        bool showFindings)
    {
        if (!showFindings)
        {
            return;
        }

        foreach (var finding in findings)
        {
            builder.AppendLine(
                $"Finding: {RouteRemoveDefinitions.ReadMachineName(finding.Code)} / target={Value(finding.Target)} / {Value(finding.Cause)}");
        }
    }
}
