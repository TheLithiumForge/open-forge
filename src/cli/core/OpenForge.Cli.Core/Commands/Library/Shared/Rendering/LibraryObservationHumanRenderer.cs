using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering.Coordinates;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Rendering;

internal static class LibraryObservationHumanRenderer
{
    internal static void AppendIdentity(StringBuilder builder, LibraryMutationIdentity identity)
    {
        builder.AppendLine($"""
            Library: {LibraryHumanText.Value(identity.LibraryId)}
            Source root: {LibraryHumanText.Value(identity.SourceRoot)}
            Destination root: {LibraryHumanText.Value(identity.DestinationRoot)}
            """);
        if (identity.Mode == LibraryMode.DryRun)
        {
            builder.AppendLine("No files changed (--dry-run).");
        }
        if (identity.SourceIndependent)
        {
            builder.AppendLine("Source files are not scanned for this operation.");
        }
    }

    internal static void AppendRecord(StringBuilder builder, LibraryMutationRecord record, CliView view)
    {
        builder.AppendLine($"Record before change: {LibraryHumanText.State(record.State)} ({LibraryHumanText.Value(record.Path)})");
        if (view == CliView.Expanded)
        {
            foreach (var path in record.RegisteredPaths)
            {
                builder.AppendLine($"  Registered: {LibraryHumanText.Value(path)}");
            }
        }
    }

    internal static void AppendSource(StringBuilder builder, LibraryMutationSource source, CliView view)
    {
        builder.AppendLine($"Source: {LibraryHumanText.State(source.RootState)}; scan {LibraryHumanText.State(source.InventoryState)}");
        if (view == CliView.Expanded)
        {
            builder.AppendLine($"""
                Source location: {LibraryHumanText.Value(source.LexicalRoot)}
                Resolved source: {LibraryHumanText.Value(source.PhysicalRoot)}
                Inside workspace: path {LibraryHumanText.Known(source.LexicallyContained)}; resolved path {LibraryHumanText.Known(source.PhysicallyContained)}
                """);
            foreach (var path in source.EligiblePaths)
            {
                builder.AppendLine($"  Eligible: {LibraryHumanText.Value(path.SourcePath)} -> {LibraryHumanText.Value(path.DestinationPath)}; ID {LibraryHumanText.Value(path.SourceId)}");
            }
            foreach (var path in source.ExcludedPaths)
            {
                builder.AppendLine($"  Excluded: {LibraryHumanText.Value(path.Path)} ({LibraryExclusionKindConverter.ReadWireValue(path.Reason)})");
            }
        }
        else
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"  Observed files: {source.EligiblePaths.Length} eligible, {source.ExcludedPaths.Length} excluded");
        }
        foreach (var path in source.UnavailablePaths)
        {
            builder.AppendLine($"  Unavailable: {LibraryHumanText.Value(path.Path)} — {LibraryHumanText.Value(path.Cause)}");
        }
    }

    internal static void AppendProjection(StringBuilder builder, LibraryMutationProjection projection, CliView view)
    {
        builder.AppendLine();
        builder.AppendLine($"Links before change: checks {LibraryHumanText.State(projection.State)}");
        foreach (var mapping in projection.Mappings)
        {
            builder.AppendLine($"  {LibraryHumanText.Value(mapping.SourcePath)} -> {LibraryHumanText.Value(mapping.DestinationPath)}");
            builder.AppendLine($"    Link: {LibraryHumanText.State(mapping.State)}; comparison: {LibraryHumanText.Relation(mapping.Relation)}; ID {LibraryHumanText.Value(mapping.SourceId)}");
            if (view == CliView.Expanded)
            {
                builder.AppendLine($"""
                        Expected target: {LibraryHumanText.Value(mapping.ExpectedRelativeLink)}
                        Observed target: {LibraryHumanText.Value(mapping.ObservedRelativeLink)}
                    """);
            }
        }
        foreach (var collision in projection.Collisions)
        {
            builder.AppendLine($"  BLOCKED: {LibraryHumanText.Value(collision.Path)} — {LibraryHumanText.Value(collision.Cause)} [{LibraryCollisionKindConverter.ReadWireValue(collision.Kind)}]");
        }
        foreach (var owner in projection.Ownership)
        {
            if (view == CliView.Compact && owner.Kind is not (LibraryOwnershipKind.Blocked or LibraryOwnershipKind.Unavailable))
            {
                continue;
            }
            builder.AppendLine($"  Ownership: {LibraryHumanText.Value(owner.Path)}; {LibraryOwnershipKindConverter.ReadWireValue(owner.Kind)}; owner {LibraryHumanText.Value(owner.OwnerId)}");
            if (owner.Cause is not null)
            {
                builder.AppendLine($"    {LibraryHumanText.Value(owner.Cause)}");
            }
        }
    }
}
