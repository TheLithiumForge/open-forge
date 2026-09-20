using System.Collections.Frozen;
using OpenForge.Cli.Core.Commands.References.Models.Result;

namespace OpenForge.Cli.Core.Presentation.References.Shared.Selection;

/// <summary>
/// The machine names References publishes for its finding codes. Rendering owns this mapping so
/// it does not import the command's behaviour-carrying definitions.
/// </summary>
internal static class ReferencesWireVocabulary
{
    internal static string Name(ReferencesFindingCode code) => code switch
    {
        ReferencesFindingCode.InvalidInput => "references.invalid-input",
        ReferencesFindingCode.InvalidSource => "references.invalid-source",
        ReferencesFindingCode.InvalidDirection => "references.invalid-direction",
        ReferencesFindingCode.InvalidFilter => "references.invalid-filter",
        ReferencesFindingCode.WorkspaceUnavailable => "references.workspace-unavailable",
        ReferencesFindingCode.WorkspaceUnsafe => "references.workspace-unsafe",
        ReferencesFindingCode.SourceAmbiguous => "references.source-ambiguous",
        ReferencesFindingCode.SourceUnsafe => "references.source-unsafe",
        ReferencesFindingCode.SelectorAmbiguous => "references.selector-ambiguous",
        ReferencesFindingCode.SelectorUnsafe => "references.selector-unsafe",
        ReferencesFindingCode.IdentityCollision => "references.identity-collision",
        ReferencesFindingCode.PhysicalAlias => "references.physical-alias",
        ReferencesFindingCode.IdentityUnavailable => "references.identity-unavailable",
        ReferencesFindingCode.CandidateUnsafe => "references.candidate-unsafe",
        ReferencesFindingCode.LayerUnresolved => "references.layer-unresolved",
        ReferencesFindingCode.InspectionUnavailable => "references.inspection-unavailable",
        ReferencesFindingCode.InvalidEncoding => "references.invalid-encoding",
        ReferencesFindingCode.LinkEncodingInvalid => "references.link-encoding-invalid",
        ReferencesFindingCode.GeneratedRegionUnavailable => "references.generated-region-unavailable",
        ReferencesFindingCode.DestinationMalformed => "references.destination-malformed",
        ReferencesFindingCode.DestinationUnsupported => "references.destination-unsupported",
        ReferencesFindingCode.TargetMissing => "references.target-missing",
        ReferencesFindingCode.FragmentMissing => "references.fragment-missing",
        ReferencesFindingCode.TargetUnsafe => "references.target-unsafe",
        ReferencesFindingCode.TargetAmbiguous => "references.target-ambiguous",
        ReferencesFindingCode.TargetUnreadable => "references.target-unreadable",
        ReferencesFindingCode.OperationFailed => "references.operation-failed",
        ReferencesFindingCode.Interrupted => "references.interrupted",
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The References finding code is not defined."),
    };

    /// <summary>
    /// A row-state finding repeats a fact the `out` row already prints inline, so text lists it
    /// only at `full` and above. JSON keeps every finding at every level, which is why this
    /// selects text rather than trimming the report.
    /// </summary>
    internal static bool IsRowState(ReferencesFindingCode code) => code switch
    {
        ReferencesFindingCode.DestinationMalformed
            or ReferencesFindingCode.DestinationUnsupported
            or ReferencesFindingCode.TargetMissing
            or ReferencesFindingCode.FragmentMissing
            or ReferencesFindingCode.TargetUnreadable => true,
        _ => false,
    };

    internal static bool IsRowState(string code) => RowStateNames.Contains(code);

    private static readonly FrozenSet<string> RowStateNames = Enum.GetValues<ReferencesFindingCode>()
        .Where(IsRowState)
        .Select(Name)
        .ToFrozenSet(StringComparer.Ordinal);
}
