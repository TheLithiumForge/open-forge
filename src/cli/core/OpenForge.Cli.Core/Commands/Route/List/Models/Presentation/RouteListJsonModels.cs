using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Route.List.Models.Presentation;

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
        Value = value;
    }

    internal RouteListDepth Value { get; }

    internal static RouteListJsonDepth? From(RouteListDepth? value)
    {
        return value is null ? null : new RouteListJsonDepth(value);
    }
}
