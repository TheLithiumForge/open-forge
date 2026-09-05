using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Rendering;

internal static class StatusLifecycleHumanRenderer
{
    internal static void Append(StringBuilder builder, StatusLifecycle lifecycle)
    {
        builder.AppendLine(
            $"Framework lifecycle: {StatusWireVocabulary.LifecycleState(lifecycle.Framework.State)}");
        builder.AppendLine(
            $"Framework source: {StatusWireVocabulary.SourceAvailability(lifecycle.Framework.SourceAvailability)}");
        builder.AppendLine(
            $"Extension lifecycle: {StatusWireVocabulary.LifecycleState(lifecycle.Extensions.State)}");
        builder.AppendLine($"Extensions: {ExtensionCount(lifecycle.Extensions)}");
        var counts = lifecycle.Extensions.ManagedFiles.Counts;
        if (ManagedFilesUnavailable(counts))
        {
            builder.AppendLine("Managed files: unavailable");
            return;
        }

        if (ManagedFilesEmpty(counts))
        {
            builder.AppendLine("Managed files: none recorded");
            return;
        }

        builder.AppendLine("Managed files:");
        builder.AppendLine($"  Current: {Value(counts.Current)}");
        builder.AppendLine($"  Changed: {Value(counts.Changed)}");
        builder.AppendLine($"  Missing: {Value(counts.Missing)}");
        builder.AppendLine($"  Unavailable: {Value(counts.Unavailable)}");
        builder.AppendLine($"  Blocked: {Value(counts.Blocked)}");
    }

    private static string ExtensionCount(StatusExtensionLifecycle extensions)
    {
        if (extensions.State == OperationalLifecycleState.Absent)
        {
            return "0 recorded";
        }

        if (extensions.State == OperationalLifecycleState.Trusted)
        {
            return extensions.Installed.Count.ToString(CultureInfo.InvariantCulture);
        }

        return extensions.Installed.Count == 0
            ? "unavailable"
            : $"{extensions.Installed.Count.ToString(CultureInfo.InvariantCulture)} recorded ({StatusWireVocabulary.LifecycleState(extensions.State)})";
    }

    private static bool ManagedFilesUnavailable(StatusManagedTargetCounts counts)
        => counts.Current.State == OperationalValueState.Unavailable
            && counts.Changed.State == OperationalValueState.Unavailable
            && counts.Missing.State == OperationalValueState.Unavailable
            && counts.Unavailable.State == OperationalValueState.Unavailable
            && counts.Blocked.State == OperationalValueState.Unavailable;

    private static bool ManagedFilesEmpty(StatusManagedTargetCounts counts)
        => counts.Current is { State: OperationalValueState.Available, Value: 0L }
            && counts.Changed is { State: OperationalValueState.Available, Value: 0L }
            && counts.Missing is { State: OperationalValueState.Available, Value: 0L }
            && counts.Unavailable is { State: OperationalValueState.Available, Value: 0L }
            && counts.Blocked is { State: OperationalValueState.Available, Value: 0L };

    private static string Value(StatusIntegerValue value)
        => value.State == OperationalValueState.Available
            ? value.Value?.ToString(CultureInfo.InvariantCulture) ?? "unavailable"
            : StatusWireVocabulary.ValueState(value.State);
}
