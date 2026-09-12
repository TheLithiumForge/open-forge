using System.Text;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Rendering;

internal static partial class RouteMoveHumanRenderer
{
    private static void AppendSubject(StringBuilder builder, RouteMoveSubject subject)
    {
        builder.AppendLine("Subject layers:");
        foreach (var layer in subject.Layers)
        {
            builder.AppendLine(
                $"  {RouteMoveDefinitions.ReadMachineName(layer.Layer)}: {Value(layer.SourcePath)} -> {Value(layer.DestinationPath)}");
        }

        if (!subject.Items.IsEmpty)
        {
            builder.AppendLine("Category items:");
        }

        foreach (var item in subject.Items)
        {
            builder.AppendLine(
                $"  {RouteMoveDefinitions.ReadMachineName(item.Kind)}: {Value(item.SourcePath)} -> {Value(item.DestinationPath)} / layer={Optional(item.Layer, RouteMoveDefinitions.ReadMachineName)} / id={Value(item.SourceId)}");
        }
    }

    private static void AppendOwnership(
        StringBuilder builder,
        RouteMoveOwnership ownership,
        bool showClaims)
    {
        builder.AppendLine(
            $"Ownership: {RouteMoveDefinitions.ReadMachineName(ownership.State)} / framework={RouteMoveDefinitions.ReadMachineName(ownership.Framework)} / extensions={RouteMoveDefinitions.ReadMachineName(ownership.Extensions)}");
        if (!showClaims)
        {
            return;
        }

        foreach (var claim in ownership.Claims)
        {
            builder.AppendLine(
                $"  Claim: {Value(claim.Path)} / {RouteMoveDefinitions.ReadMachineName(claim.Manager)} / {Value(claim.Owner)}");
        }
    }

    private static void AppendReferences(StringBuilder builder, RouteMoveReferences references)
    {
        builder.AppendLine(
            $"References: coverage={RouteMoveDefinitions.ReadMachineName(references.Coverage)}, scanned={references.ScannedSourceCount}, inspected={references.InspectedSourceCount}, occurrences={references.OccurrenceCount}");
        foreach (var rewrite in references.Rewrites)
        {
            builder.AppendLine(
                $"  {Value(rewrite.SourcePath)}:{rewrite.Location.Line}:{rewrite.Location.Column} -> {Value(rewrite.DestinationSourcePath)}: {Value(rewrite.Before)} -> {Value(rewrite.Expected)}");
            builder.AppendLine(
                $"    Target: {Value(rewrite.OldTarget.Path)} -> {Value(rewrite.ExpectedTarget.Path)}");
        }
    }

    private static void AppendGeneratedNavigation(
        StringBuilder builder,
        RouteMoveGeneratedNavigation navigation)
    {
        builder.AppendLine(
            $"Generated navigation: {RouteMoveDefinitions.ReadMachineName(navigation.Coverage)}");
        foreach (var region in navigation.Regions)
        {
            var reasons = string.Join(", ", region.Reasons.Select(RouteMoveDefinitions.ReadMachineName));
            builder.AppendLine(
                $"  {Value(region.Path)}: {RouteMoveDefinitions.ReadMachineName(region.State)} / reasons={reasons}");
        }
    }

    private static void AppendEffects(StringBuilder builder, IReadOnlyList<RouteMoveEffect> effects)
    {
        builder.AppendLine("Effects:");
        foreach (var effect in effects)
        {
            builder.AppendLine(
                $"  {Value(effect.Path)}: {RouteMoveDefinitions.ReadMachineName(effect.Action)} {RouteMoveDefinitions.ReadMachineName(effect.Kind)} / {PathState(effect.Before)} -> {PathState(effect.Expected)} / {RouteMoveDefinitions.ReadMachineName(effect.Outcome)} / residual={RouteMoveDefinitions.ReadMachineName(effect.Residual)}");
        }
    }

    private static string PathState(RouteMovePathState state)
    {
        var kind = RouteMoveDefinitions.ReadMachineName(state.Kind);
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
        RouteMoveRecovery recovery)
    {
        builder.AppendLine(
            $"Recovery: {RouteMoveDefinitions.ReadMachineName(recovery.State)} / residual={Value(recovery.ResidualPath)}");
        foreach (var path in recovery.ProtectedPaths)
        {
            builder.AppendLine($"  Protected: {Value(path)}");
        }
    }

    private static void AppendFindings(
        StringBuilder builder,
        IReadOnlyList<RouteMoveFinding> findings)
    {
        foreach (var finding in findings)
        {
            builder.AppendLine(
                $"{CliHumanText.Status(finding.Status).ToUpperInvariant()}: {Value(finding.Cause)} [{RouteMoveDefinitions.ReadMachineName(finding.Code)}]");
            if (finding.Target is { } target)
            {
                builder.AppendLine($"  {Value(target)}");
            }
        }
    }
}
