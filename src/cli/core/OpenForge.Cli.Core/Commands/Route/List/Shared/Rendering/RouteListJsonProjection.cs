using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;

internal sealed record RouteListJsonDocument(
    int SchemaVersion,
    string Command,
    string Status,
    RouteListJsonWorkspace? Workspace,
    RouteListJsonResult Result,
    RouteListJsonNext? Next);

internal sealed record RouteListJsonWorkspace(
    string Path,
    string SelectedBy);

internal sealed record RouteListJsonResult(
    RouteListJsonSelection Selection,
    RouteListJsonDepth? RequestedDepth,
    RouteListJsonDepth? EffectiveDepth,
    RouteListJsonCoverage Coverage,
    RouteListJsonFinding[] Findings,
    RouteListJsonRow[] Rows);

internal sealed record RouteListJsonSelection(
    string Kind,
    string? AttemptedId,
    string? AttemptedPath,
    string? ResolvedId,
    string? ResolvedPath);

internal sealed record RouteListJsonCoverage(
    string State,
    int SelectedRootCount,
    int ConfirmedRowCount,
    string[] Confirmations,
    string[] UnresolvedBoundaries);

internal sealed record RouteListJsonRow(
    string Id,
    string Path,
    string? ParentId,
    string? ParentPath,
    int? AbsoluteDepth,
    int RelativeDepth,
    string Kind,
    string Description,
    string[] Tags,
    int? DirectChildCount,
    RouteListJsonProvenance Provenance);

internal sealed record RouteListJsonProvenance(
    string Selection,
    string Source,
    bool HasOverwrite);

internal sealed record RouteListJsonFinding(
    string Code,
    string Status,
    string? Subject,
    string Cause,
    string[] CandidatePaths);

internal sealed record RouteListJsonNext(
    string Command,
    string Reason);

[JsonConverter(typeof(RouteListJsonDepthConverter))]
internal sealed class RouteListJsonDepth
{
    private RouteListJsonDepth(RouteListDepth value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Value = value;
    }

    internal RouteListDepth Value { get; }

    internal static RouteListJsonDepth? From(RouteListDepth? value)
    {
        return value is null ? null : new RouteListJsonDepth(value);
    }
}

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

        writer.WriteNumberValue(value.Value.Value!.Value);
    }
}

internal static class RouteListJsonProjection
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
            workspace.SelectedBy switch
            {
                CliWorkspaceSelectionMethod.CurrentDirectory => "current-directory",
                CliWorkspaceSelectionMethod.ExplicitWorkspace => "explicit-workspace",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(workspace),
                    workspace.SelectedBy,
                    "The workspace selection method is not defined."),
            });
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
