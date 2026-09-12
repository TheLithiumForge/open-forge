using System.Text;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering.Coordinates;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Rendering;

internal static class LibraryApplicationHumanRenderer
{
    internal static void Append(StringBuilder builder, LibraryMutationApplication application, CliView view)
    {
        var state = application.State switch
        {
            LibraryApplicationState.NotStarted => "not started",
            LibraryApplicationState.NoOp => "no changes needed",
            _ => LibraryApplicationStateConverter.ReadWireValue(application.State),
        };
        var verification = application.Verification == LibraryVerificationState.NotStarted
            ? "not started" : LibraryVerificationStateConverter.ReadWireValue(application.Verification);
        var recovery = application.Recovery.State == LibraryRecoveryState.NotRequested
            ? "not requested" : LibraryRecoveryStateConverter.ReadWireValue(application.Recovery.State);
        builder.AppendLine($"""
            Application: {state}; verification {verification}
            Recovery: {recovery}
            """);
        if (application.Recovery.Path is { } path)
        {
            builder.AppendLine($"  {LibraryHumanText.Value(path)}");
        }
        var publication = application.RecordPublication.State == LibraryRecordPublicationState.NotStarted
            ? "not started" : LibraryRecordPublicationStateConverter.ReadWireValue(application.RecordPublication.State);
        builder.AppendLine($"Record publication: {publication}");
        if (view == CliView.Expanded)
        {
            builder.AppendLine($"  Published after all other changes: {LibraryHumanText.Known(application.RecordPublication.PublishedLast)}");
        }
        foreach (var residual in application.Residuals)
        {
            var kind = residual.Kind == LibraryResidualKind.GeneratedRegion
                ? "generated navigation" : LibraryResidualKindConverter.ReadWireValue(residual.Kind);
            builder.AppendLine($"  Remaining {kind}: {LibraryHumanText.Value(residual.Path)} ({LibraryResidualStateConverter.ReadWireValue(residual.State)})");
        }
    }
}
