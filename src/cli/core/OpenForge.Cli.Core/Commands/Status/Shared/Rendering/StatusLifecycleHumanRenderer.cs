using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Rendering;

internal static class StatusLifecycleHumanRenderer
{
    internal static void Append(StringBuilder builder, StatusLifecycle lifecycle, CliView view)
    {
        builder.AppendLine();
        builder.AppendLine($"""
            Framework
              Installation record: {StatusWireVocabulary.LifecycleState(lifecycle.Framework.State)}
              Source: {StatusWireVocabulary.SourceAvailability(lifecycle.Framework.SourceAvailability)}
            """);
        foreach (var target in lifecycle.Framework.Targets)
        {
            if (view == CliView.Expanded || target.State != OperationalTargetState.Current)
            {
                builder.AppendLine($"  {StatusHumanRenderer.Text(target.Path)}: {StatusWireVocabulary.TargetState(target.State)}");
            }
        }

        builder.AppendLine();
        builder.AppendLine($"""
            Extensions: {ExtensionCount(lifecycle.Extensions)}
              Installation record: {StatusWireVocabulary.LifecycleState(lifecycle.Extensions.State)}
              Source: {StatusWireVocabulary.SourceAvailability(lifecycle.Extensions.SourceAvailability)}
            """);
        if (view == CliView.Expanded)
        {
            foreach (var extension in lifecycle.Extensions.Installed)
            {
                builder.AppendLine($"  {StatusHumanRenderer.Text(extension.Id)} {StatusHumanRenderer.Text(extension.Version ?? "version unavailable")}");
            }
        }

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

        builder.AppendLine($"""
            Managed files:
              {Value(counts.Current)} current, {Value(counts.Changed)} changed, {Value(counts.Missing)} missing
              {Value(counts.Unavailable)} unavailable, {Value(counts.Blocked)} blocked
            """);
        foreach (var target in lifecycle.Extensions.ManagedFiles.Targets)
        {
            if (view == CliView.Expanded || target.State != OperationalTargetState.Current)
            {
                builder.AppendLine($"  {StatusHumanRenderer.Text(target.Path)}: {StatusWireVocabulary.TargetState(target.State)}");
            }
        }
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
