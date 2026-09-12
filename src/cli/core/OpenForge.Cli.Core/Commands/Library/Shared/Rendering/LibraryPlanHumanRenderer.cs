using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Effects;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering.Coordinates;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Rendering;

internal static class LibraryPlanHumanRenderer
{
    internal static void Append(StringBuilder builder, LibraryMutationPlanView plan, string recordPath, CliView view)
    {
        builder.AppendLine();
        builder.AppendLine($"Plan: {LibraryHumanText.State(plan.State)}");
        foreach (var directory in plan.Directories)
        {
            builder.AppendLine($"  Create directory: {LibraryHumanText.Value(directory.Path)}");
            if (view == CliView.Expanded)
            {
                AppendExpected(builder, directory.Expected);
            }
        }
        foreach (var link in plan.Links)
        {
            var action = link.Kind switch
            {
                LibraryLinkEffectKind.Create => "Create link",
                LibraryLinkEffectKind.Delete => "Remove link",
                _ => throw new ArgumentOutOfRangeException(nameof(plan), link.Kind, "The link effect is not defined."),
            };
            builder.AppendLine($"  {action}: {LibraryHumanText.Value(link.Path)} -> {LibraryHumanText.Value(link.RawRelativeTarget)}");
            if (view == CliView.Expanded)
            {
                AppendExpected(builder, link.Expected);
            }
        }
        foreach (var region in plan.GeneratedRegions)
        {
            builder.AppendLine($"  Update generated navigation: {LibraryHumanText.Value(region.Path)}");
            if (view == CliView.Expanded)
            {
                builder.AppendLine($"    SHA-256: {LibraryHumanText.Value(region.ExpectedSha256)} -> {LibraryHumanText.Value(region.IntendedSha256)}");
                AppendExpected(builder, region.Expected);
            }
        }
        builder.AppendLine($"  Record: {LibraryRecordEffectConverter.ReadWireValue(plan.RecordEffect)} ({LibraryHumanText.Value(recordPath)})");
        if (view == CliView.Expanded && plan.RecordExpected is { } expected)
        {
            AppendExpected(builder, expected);
        }
    }

    private static void AppendExpected(StringBuilder builder, LibraryExpectedState expected)
    {
        var kind = expected.Kind switch
        {
            LibraryExpectedStateKind.Missing => "absent",
            LibraryExpectedStateKind.OrdinaryFile => "ordinary file",
            LibraryExpectedStateKind.RelativeFileLink => "relative file link",
            _ => throw new ArgumentOutOfRangeException(nameof(expected), expected.Kind, "The expected state is not defined."),
        };
        builder.AppendLine($"    Expected before change: {kind}");
        if (expected.Length is { } length)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"      {length} bytes");
        }
        if (expected.Sha256 is { } hash)
        {
            builder.AppendLine($"      SHA-256: {LibraryHumanText.Value(hash)}");
        }
        if (expected.RawRelativeTarget is { } target)
        {
            builder.AppendLine($"      Link target: {LibraryHumanText.Value(target)}");
        }
    }
}
