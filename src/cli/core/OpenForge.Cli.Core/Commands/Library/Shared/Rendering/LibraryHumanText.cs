using System.Text;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering.Coordinates;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Rendering;

internal static class LibraryHumanText
{
    internal static string Value(string? value) => CliHumanText.Text(value ?? "unavailable");

    internal static string Known(bool? value) => value switch
    {
        true => "yes",
        false => "no",
        null => "unavailable",
    };

    internal static string Heading(string operation, CliSemanticStatus status, LibraryMode mode)
    {
        var label = mode switch
        {
            LibraryMode.DryRun => $"{operation} preview",
            LibraryMode.Apply => operation,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "The mode is not defined."),
        };
        return status switch
        {
            CliSemanticStatus.Complete => $"{label} completed.",
            CliSemanticStatus.Attention => $"{label} requires attention.",
            CliSemanticStatus.Incomplete => $"{label} could not finish.",
            CliSemanticStatus.Invalid => $"{label} could not start because the input is invalid.",
            CliSemanticStatus.Blocked => $"{label} is blocked.",
            CliSemanticStatus.Failed => $"{label} failed.",
            CliSemanticStatus.Interrupted => $"{label} was interrupted.",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The status is not defined."),
        };
    }

    internal static void AppendFinding(StringBuilder builder, CliSemanticStatus status, string code, string cause, string? path)
    {
        builder.AppendLine($"{CliHumanText.Status(status).ToUpperInvariant()}: {Value(cause)} [{code}]");
        if (path is not null)
        {
            builder.AppendLine($"  {Value(path)}");
        }
    }

    internal static string State(LibraryRecordViewState value)
        => value == LibraryRecordViewState.NotStarted ? "not checked" : LibraryRecordViewStateConverter.ReadWireValue(value);
    internal static string State(LibraryMutationRecordState value)
        => value == LibraryMutationRecordState.NotStarted ? "not checked" : LibraryMutationRecordStateConverter.ReadWireValue(value);
    internal static string State(LibrarySourceRootViewState value)
        => value == LibrarySourceRootViewState.NotStarted ? "not checked" : LibrarySourceRootViewStateConverter.ReadWireValue(value);
    internal static string State(LibraryMutationInventoryState value)
        => value == LibraryMutationInventoryState.NotStarted ? "not scanned" : LibraryMutationInventoryStateConverter.ReadWireValue(value);
    internal static string State(LibraryLinkViewState value)
        => value == LibraryLinkViewState.NotStarted ? "not checked" : LibraryLinkViewStateConverter.ReadWireValue(value);
    internal static string State(LibraryCoverage value)
        => value == LibraryCoverage.NotStarted ? "not checked" : LibraryCoverageConverter.ReadWireValue(value);
    internal static string State(LibraryPlanState value)
        => value == LibraryPlanState.NotStarted ? "not started" : LibraryPlanStateConverter.ReadWireValue(value);
    internal static string Relation(LibraryComparisonRelation value) => value switch
    {
        LibraryComparisonRelation.NotStarted => "not compared",
        LibraryComparisonRelation.Added => "new to the intended Library",
        LibraryComparisonRelation.Retired => "not in the intended Library",
        _ => LibraryComparisonRelationConverter.ReadWireValue(value),
    };
}
