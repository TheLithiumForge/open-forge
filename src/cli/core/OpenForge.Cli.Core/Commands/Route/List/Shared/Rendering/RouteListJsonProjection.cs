using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

using OpenForge.Cli.Core.Commands.Route.List.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;

internal sealed class RouteListJsonDepthConverter : JsonConverter<RouteListJsonDepth>
{
    public override bool HandleNull => true;

    public override RouteListJsonDepth? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        throw new NotSupportedException("Route-list depth JSON is a serialization-only projection.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        RouteListJsonDepth? value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        if (value.Value.Kind == RouteListDepthKind.All)
        {
            writer.WriteStringValue(RouteListDefinitions.AllDepth);
            return;
        }

        writer.WriteNumberValue(value.Value.FiniteValue);
    }
}

internal static partial class RouteListJsonProjection
{
    internal static RouteListJsonDocument Create(RouteListResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new RouteListJsonDocument(
            result.SchemaVersion,
            result.Command,
            Status(result.Status),
            result.Workspace is null ? null : Workspace(result.Workspace),
            new RouteListJsonResult(
                Selection(result.Selection),
                RouteListJsonDepth.From(result.RequestedDepth),
                RouteListJsonDepth.From(result.EffectiveDepth),
                Coverage(result.Coverage),
                result.Findings.Select(Finding).ToArray(),
                result.Rows.Select(Row).ToArray()),
            result.Next is null ? null : new RouteListJsonNext(result.Next.Command, result.Next.Reason));
    }

    private static RouteListJsonWorkspace Workspace(CliWorkspace workspace)
    {
        return new RouteListJsonWorkspace(
            workspace.LexicalRoot,
            WorkspaceSelectionWireVocabulary.Read(workspace.SelectedBy));
    }

    private static RouteListJsonSelection Selection(RouteListSelection selection)
    {
        return new RouteListJsonSelection(
            selection.Kind switch
            {
                RouteListSelectionKind.LoaderRoots => "loader-roots",
                RouteListSelectionKind.SourceId => "source-id",
                RouteListSelectionKind.SourcePath => "source-path",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(selection),
                    selection.Kind,
                    "The route-list selection kind is not defined."),
            },
            selection.AttemptedId,
            selection.AttemptedPath,
            selection.ResolvedId,
            selection.ResolvedPath);
    }

    private static RouteListJsonCoverage Coverage(RouteListCoverage coverage)
    {
        return new RouteListJsonCoverage(
            coverage.State switch
            {
                RouteListCoverageState.NotStarted => "not-started",
                RouteListCoverageState.Complete => "complete",
                RouteListCoverageState.Incomplete => "incomplete",
                RouteListCoverageState.Blocked => "blocked",
                RouteListCoverageState.Failed => "failed",
                RouteListCoverageState.Interrupted => "interrupted",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(coverage),
                    coverage.State,
                    "The route-list coverage state is not defined."),
            },
            coverage.SelectedRootCount,
            coverage.ConfirmedRowCount,
            coverage.Evidence.ToArray(),
            coverage.UnresolvedBoundaries.ToArray());
    }

    private static RouteListJsonRow Row(RouteListRow row)
    {
        return new RouteListJsonRow(
            row.Id,
            row.Path,
            row.ParentId,
            row.ParentPath,
            row.AbsoluteDepth,
            row.RelativeDepth,
            row.Kind switch
            {
                RouteListRowKind.Entrypoint => "entrypoint",
                RouteListRowKind.RoutedLeaf => "routed-leaf",
                _ => throw new ArgumentOutOfRangeException(nameof(row), row.Kind, "The route-list row kind is not defined."),
            },
            row.Description,
            row.Tags.ToArray(),
            row.DirectChildCount,
            new RouteListJsonProvenance(
                row.Provenance.Selection switch
                {
                    RouteListSelectionProvenance.LoaderRoot => "loader-root",
                    RouteListSelectionProvenance.ExplicitRoot => "explicit-root",
                    RouteListSelectionProvenance.DetachedRoot => "detached-root",
                    RouteListSelectionProvenance.Descendant => "descendant",
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(row),
                        row.Provenance.Selection,
                        "The route-list selection provenance is not defined."),
                },
                row.Provenance.Source switch
                {
                    RouteListSourceProvenance.AuthoredEntrypoint => "authored-entrypoint",
                    RouteListSourceProvenance.AuthoredLeaf => "authored-leaf",
                    RouteListSourceProvenance.RoutedNative => "routed-native",
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(row),
                        row.Provenance.Source,
                        "The route-list source provenance is not defined."),
                },
                row.Provenance.HasOverwrite));
    }

    private static RouteListJsonFinding Finding(RouteListFinding finding)
    {
        return new RouteListJsonFinding(
            finding.MachineCode,
            Status(finding.Status),
            finding.Subject,
            finding.Cause,
            finding.CandidatePaths.ToArray());
    }

    private static string Status(CliSemanticStatus status)
    {
        return CliStatusDefinitions.Read(status).MachineName;
    }
}
