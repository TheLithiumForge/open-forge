using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.List.Models.Result;

/// <summary>
/// The command-owned, Framework-free facts consumed by route-list presentation. The operation
/// result keeps its validated command and Framework model; this projection is the boundary that
/// lets Presentation render only the values that the route-list contract exposes.
/// </summary>
internal sealed record RouteListPresentationFacts
{
    internal required string Command { get; init; }

    internal required CliSemanticStatus Status { get; init; }

    internal string? WorkspacePath { get; init; }

    internal bool WorkspaceExplicit { get; init; }

    internal required RouteListPresentationSelection Selection { get; init; }

    internal required RouteListPresentationCoverage Coverage { get; init; }

    internal RouteListPresentationDepth? RequestedDepth => Coverage.RequestedDepth;

    internal RouteListPresentationDepth? EffectiveDepth => Coverage.EffectiveDepth;

    internal required IReadOnlyList<RouteListPresentationRow> Rows { get; init; }

    internal required IReadOnlyList<RouteListPresentationFinding> Findings { get; init; }

    internal CliNextAction? Next { get; init; }

    internal static RouteListPresentationFacts From(RouteListResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new RouteListPresentationFacts
        {
            Command = result.Command,
            Status = result.Status,
            WorkspacePath = result.WorkspacePath,
            WorkspaceExplicit = result.WorkspaceExplicit,
            Selection = ProjectSelection(result.Selection),
            Coverage = ProjectCoverage(result.Coverage),
            Rows = result.Rows.Select(Row).ToArray(),
            Findings = result.Findings.Select(Finding).ToArray(),
            Next = result.Next,
        };
    }

    private static RouteListPresentationSelection ProjectSelection(RouteListSelection selection)
        => new()
        {
            Kind = selection.Kind switch
            {
                RouteListSelectionKind.LoaderRoots => RouteListPresentationSelectionKind.LoaderRoots,
                RouteListSelectionKind.SourceId => RouteListPresentationSelectionKind.SourceId,
                RouteListSelectionKind.SourcePath => RouteListPresentationSelectionKind.SourcePath,
                _ => throw new ArgumentOutOfRangeException(nameof(selection), selection.Kind, "The route-list selection kind is not defined."),
            },
            ResolvedId = selection.ResolvedId,
            ResolvedPath = selection.ResolvedPath,
        };

    private static RouteListPresentationCoverage ProjectCoverage(RouteListCoverage coverage)
        => new()
        {
            RequestedDepth = Depth(coverage.RequestedDepth),
            EffectiveDepth = Depth(coverage.EffectiveDepth),
            SelectedRootCount = coverage.SelectedRootCount,
            ConfirmedRowCount = coverage.ConfirmedRowCount,
        };

    private static RouteListPresentationDepth? Depth(RouteListDepth? depth)
        => depth is null
            ? null
            : new RouteListPresentationDepth
            {
                Kind = depth.Kind switch
                {
                    RouteListDepthKind.Finite => RouteListPresentationDepthKind.Finite,
                    RouteListDepthKind.All => RouteListPresentationDepthKind.All,
                    _ => throw new ArgumentOutOfRangeException(nameof(depth), depth.Kind, "The route-list depth kind is not defined."),
                },
                Value = depth.Value,
            };

    private static RouteListPresentationRow Row(RouteListRow row)
        => new()
        {
            Id = row.Id,
            Path = row.Path,
            ParentId = row.ParentId,
            ParentPath = row.ParentPath,
            AbsoluteDepth = row.AbsoluteDepth,
            RelativeDepth = row.RelativeDepth,
            Kind = row.Kind switch
            {
                RouteListRowKind.Entrypoint => RouteListPresentationRowKind.Entrypoint,
                RouteListRowKind.RoutedLeaf => RouteListPresentationRowKind.RoutedLeaf,
                _ => throw new ArgumentOutOfRangeException(nameof(row), row.Kind, "The route-list row kind is not defined."),
            },
            Description = row.Description,
            Tags = row.Tags.ToArray(),
            DirectChildCount = row.DirectChildCount,
            Selection = row.Provenance.Selection switch
            {
                RouteListSelectionProvenance.LoaderRoot => RouteListPresentationSelectionProvenance.LoaderRoot,
                RouteListSelectionProvenance.ExplicitRoot => RouteListPresentationSelectionProvenance.ExplicitRoot,
                RouteListSelectionProvenance.DetachedRoot => RouteListPresentationSelectionProvenance.DetachedRoot,
                RouteListSelectionProvenance.Descendant => RouteListPresentationSelectionProvenance.Descendant,
                _ => throw new ArgumentOutOfRangeException(nameof(row), row.Provenance.Selection, "The route-list selection provenance is not defined."),
            },
            HasOverwrite = row.Provenance.HasOverwrite,
        };

    private static RouteListPresentationFinding Finding(RouteListFinding finding)
        => new()
        {
            Code = finding.Code switch
            {
                RouteListFindingCode.InvalidDepth => RouteListPresentationFindingCode.InvalidDepth,
                RouteListFindingCode.InvalidSourceReference => RouteListPresentationFindingCode.InvalidSourceReference,
                RouteListFindingCode.InvalidWorkspace => RouteListPresentationFindingCode.InvalidWorkspace,
                RouteListFindingCode.WorkspaceUnavailable => RouteListPresentationFindingCode.WorkspaceUnavailable,
                RouteListFindingCode.UnknownSource => RouteListPresentationFindingCode.UnknownSource,
                RouteListFindingCode.UnsupportedSource => RouteListPresentationFindingCode.UnsupportedSource,
                RouteListFindingCode.LoaderSubject => RouteListPresentationFindingCode.LoaderSubject,
                RouteListFindingCode.AmbiguousSource => RouteListPresentationFindingCode.AmbiguousSource,
                RouteListFindingCode.UnsafeSource => RouteListPresentationFindingCode.UnsafeSource,
                RouteListFindingCode.LoaderUnavailable => RouteListPresentationFindingCode.LoaderUnavailable,
                RouteListFindingCode.LoaderMalformed => RouteListPresentationFindingCode.LoaderMalformed,
                RouteListFindingCode.RouteAmbiguous => RouteListPresentationFindingCode.RouteAmbiguous,
                RouteListFindingCode.MetadataMissing => RouteListPresentationFindingCode.MetadataMissing,
                RouteListFindingCode.MetadataMalformed => RouteListPresentationFindingCode.MetadataMalformed,
                RouteListFindingCode.AuthoredForm => RouteListPresentationFindingCode.AuthoredForm,
                RouteListFindingCode.IdentityCollision => RouteListPresentationFindingCode.IdentityCollision,
                RouteListFindingCode.ReadUnavailable => RouteListPresentationFindingCode.ReadUnavailable,
                RouteListFindingCode.PhysicalBoundary => RouteListPresentationFindingCode.PhysicalBoundary,
                RouteListFindingCode.OperationFailed => RouteListPresentationFindingCode.OperationFailed,
                RouteListFindingCode.Interrupted => RouteListPresentationFindingCode.Interrupted,
                _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code, "The route-list finding code is not defined."),
            },
            MachineCode = finding.MachineCode,
            Status = finding.Status,
            Subject = finding.Subject,
            Cause = finding.Cause,
            CandidatePaths = finding.CandidatePaths.ToArray(),
        };
}

internal enum RouteListPresentationDepthKind
{
    Finite,
    All,
}

internal sealed record RouteListPresentationDepth
{
    internal required RouteListPresentationDepthKind Kind { get; init; }

    internal int? Value { get; init; }

    internal int FiniteValue
        => Kind == RouteListPresentationDepthKind.Finite && Value is { } finiteValue
            ? finiteValue
            : throw new InvalidOperationException("A finite route-list depth requires a finite value.");

    internal string MachineValue => Kind switch
    {
        RouteListPresentationDepthKind.Finite => FiniteValue.ToString(CultureInfo.InvariantCulture),
        RouteListPresentationDepthKind.All => "all",
        _ => throw new ArgumentOutOfRangeException(nameof(Kind), Kind, "The route-list depth kind is not defined."),
    };
}

internal enum RouteListPresentationSelectionKind
{
    LoaderRoots,
    SourceId,
    SourcePath,
}

internal sealed record RouteListPresentationSelection
{
    internal required RouteListPresentationSelectionKind Kind { get; init; }

    internal string? ResolvedId { get; init; }

    internal string? ResolvedPath { get; init; }
}

internal sealed record RouteListPresentationCoverage
{
    internal RouteListPresentationDepth? RequestedDepth { get; init; }

    internal RouteListPresentationDepth? EffectiveDepth { get; init; }

    internal required int SelectedRootCount { get; init; }

    internal required int ConfirmedRowCount { get; init; }
}

internal enum RouteListPresentationRowKind
{
    Entrypoint,
    RoutedLeaf,
}

internal enum RouteListPresentationSelectionProvenance
{
    LoaderRoot,
    ExplicitRoot,
    DetachedRoot,
    Descendant,
}

internal sealed record RouteListPresentationRow
{
    internal required string Id { get; init; }

    internal required string Path { get; init; }

    internal string? ParentId { get; init; }

    internal string? ParentPath { get; init; }

    internal int? AbsoluteDepth { get; init; }

    internal required int RelativeDepth { get; init; }

    internal required RouteListPresentationRowKind Kind { get; init; }

    internal required string Description { get; init; }

    internal required IReadOnlyList<string> Tags { get; init; }

    internal int? DirectChildCount { get; init; }

    internal required RouteListPresentationSelectionProvenance Selection { get; init; }

    internal required bool HasOverwrite { get; init; }
}

internal enum RouteListPresentationFindingCode
{
    InvalidDepth,
    InvalidSourceReference,
    InvalidWorkspace,
    WorkspaceUnavailable,
    UnknownSource,
    UnsupportedSource,
    LoaderSubject,
    AmbiguousSource,
    UnsafeSource,
    LoaderUnavailable,
    LoaderMalformed,
    RouteAmbiguous,
    MetadataMissing,
    MetadataMalformed,
    AuthoredForm,
    IdentityCollision,
    ReadUnavailable,
    PhysicalBoundary,
    OperationFailed,
    Interrupted,
}

internal sealed record RouteListPresentationFinding
{
    internal required RouteListPresentationFindingCode Code { get; init; }

    internal required string MachineCode { get; init; }

    internal required CliSemanticStatus Status { get; init; }

    internal string? Subject { get; init; }

    internal required string Cause { get; init; }

    internal required IReadOnlyList<string> CandidatePaths { get; init; }
}
