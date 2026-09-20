using System.Globalization;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.References.Shared.Wording;

internal static class ReferencesWording
{
    private const string EntriesNotCounted = "Entries links are not counted.";

    internal static string NoLinks(ReferencesDirection direction, string path) => direction switch
    {
        ReferencesDirection.Both => global::OpenForge.Cli.OutputText.References.ReferencesPhrases.FormatHasNoAuthoredLinksInOrOut($"{path}", $"{EntriesNotCounted}"),
        ReferencesDirection.In => global::OpenForge.Cli.OutputText.References.ReferencesPhrases.FormatHasNoAuthoredLinksIn($"{path}", $"{EntriesNotCounted}"),
        ReferencesDirection.Out => global::OpenForge.Cli.OutputText.References.ReferencesPhrases.FormatHasNoAuthoredLinksOut($"{path}", $"{EntriesNotCounted}"),
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "The References direction is not defined."),
    };

    internal static string Incomplete() => global::OpenForge.Cli.OutputText.References.ReferencesText.MessageTheScanIsIncomplete();

    internal static string CannotList(string cause) => global::OpenForge.Cli.OutputText.References.ReferencesPhrases.FormatCannotListReferences($"{Reason(cause)}");

    internal static string InvalidInput(string cause)
        => CliFindingWording.InvalidInput(global::OpenForge.Cli.OutputText.References.ReferencesText.LabelListReferences(), Reason(cause));

    internal static string Failed(string cause)
        => CliFindingWording.OperationFailed(global::OpenForge.Cli.OutputText.References.ReferencesText.TitleReferences(), Reason(cause));

    internal static string Interrupted() => CliFindingWording.Interrupted(global::OpenForge.Cli.OutputText.References.ReferencesText.TitleReferences());

    internal static string UnknownSourceId(string reference) => global::OpenForge.Cli.OutputText.References.ReferencesWording.UnknownSourceId(reference);

    internal static string SourceOutsideAgents(string path) => global::OpenForge.Cli.OutputText.References.ReferencesWording.SourceOutsideAgents(path);

    internal static string InvalidDirection() => global::OpenForge.Cli.OutputText.References.ReferencesText.MessageDirectionMustBeInOutOrBoth();

    internal static string FilterNeedsIncoming()
        => global::OpenForge.Cli.OutputText.References.ReferencesText.MessageIncludeAndExcludeApplyToIncomingLinksUseDirectionInOrBoth();

    internal static string FilterNotASource(string value)
        => global::OpenForge.Cli.OutputText.References.ReferencesWording.FilterNotASource(value);

    internal static string CandidateUnsafe(string path)
        => global::OpenForge.Cli.OutputText.References.ReferencesWording.CandidateUnsafe(path);

    internal static string LayerUnresolved(string name)
        => global::OpenForge.Cli.OutputText.References.ReferencesWording.LayerUnresolved(name);

    internal static string InspectionUnavailable(string path)
        => global::OpenForge.Cli.OutputText.References.ReferencesWording.InspectionUnavailable(path);

    internal static string InvalidEncoding(string path)
        => global::OpenForge.Cli.OutputText.References.ReferencesWording.InvalidEncoding(path);

    internal static string LinkEncodingInvalid(string location)
        => global::OpenForge.Cli.OutputText.References.ReferencesWording.LinkEncodingInvalid(location);

    internal static string GeneratedRegionUnavailable(string path)
        => global::OpenForge.Cli.OutputText.References.ReferencesWording.GeneratedRegionUnavailable(path);

    internal static string TargetUnsafe(string location)
        => global::OpenForge.Cli.OutputText.References.ReferencesWording.TargetUnsafe(location);

    internal static string TargetAmbiguous(string location)
        => global::OpenForge.Cli.OutputText.References.ReferencesWording.TargetAmbiguous(location);

    internal static string IdentityCollision(string? id)
        => id is { } identity
            ? CliFindingWording.IdentityCollision(identity)
            : global::OpenForge.Cli.OutputText.References.ReferencesText.MessageTheSourceIdentityCollisionCouldNotBeDescribedBecauseItsIdIsUnavailable();

    internal static string PhysicalAlias(IReadOnlyList<string> paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        if (paths.Count == 0)
        {
            return global::OpenForge.Cli.OutputText.References.ReferencesText.MessageThePhysicalAliasCouldNotBeDescribedBecauseItsPathsAreUnavailable();
        }

        if (paths.Count == 1)
        {
            return global::OpenForge.Cli.OutputText.References.ReferencesPhrases.FormatThePhysicalAliasInvolvingCouldNotBeDescribedBecauseOneOfItsPathsIsUnavailable($"{paths[0]}");
        }

        if (paths.Count == 2)
        {
            return global::OpenForge.Cli.OutputText.References.ReferencesPhrases.FormatAndResolveToTheSamePhysicalFile($"{paths[0]}", $"{paths[1]}");
        }

        return global::OpenForge.Cli.OutputText.References.ReferencesPhrases.FormatAndResolveToTheSamePhysicalFile($"{string.Join(", ", paths.Take(paths.Count - 1))}", $"{paths[^1]}");
    }

    internal static string IdentityUnavailable(string? path)
        => path is { } sourcePath
            ? global::OpenForge.Cli.OutputText.References.ReferencesPhrases.FormatTheIdentityOfCouldNotBeDetermined($"{sourcePath}")
            : global::OpenForge.Cli.OutputText.References.ReferencesText.MessageTheIdentityOfTheSourceCouldNotBeDeterminedBecauseItsPathIsUnavailable();

    /// <summary>
    /// The state word an outgoing row carries when the link is not fine. A fine link prints no
    /// word at all, so this returns null for a complete local resolution.
    /// </summary>
    internal static string? RowState(ReferencesTargetResolution resolution) => resolution switch
    {
        ReferencesTargetResolution.Complete => null,
        ReferencesTargetResolution.Missing => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelMissing(),
        ReferencesTargetResolution.FragmentMissing => global::OpenForge.Cli.OutputText.References.ReferencesText.LabelHeadingNotFound(),
        ReferencesTargetResolution.ExternalUnchecked => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotChecked(),
        ReferencesTargetResolution.Unsupported => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotFollowed(),
        ReferencesTargetResolution.Malformed
            or ReferencesTargetResolution.Absolute
            or ReferencesTargetResolution.Query
            or ReferencesTargetResolution.EncodingUnsupported => global::OpenForge.Cli.OutputText.References.ReferencesText.LabelNotAResolvableLink(),
        ReferencesTargetResolution.OutsideWorkspace
            or ReferencesTargetResolution.PhysicalEscape => global::OpenForge.Cli.OutputText.References.ReferencesText.LabelPointsOutsideTheWorkspace(),
        ReferencesTargetResolution.Ambiguous => global::OpenForge.Cli.OutputText.References.ReferencesText.LabelCouldPointToMoreThanOneFile(),
        ReferencesTargetResolution.Unreadable => global::OpenForge.Cli.OutputText.References.ReferencesText.LabelTargetCouldNotBeRead(),
        _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "The References target resolution is not defined."),
    };

    internal static string WireResolution(ReferencesTargetResolution resolution) => resolution switch
    {
        ReferencesTargetResolution.Complete => "complete",
        ReferencesTargetResolution.Missing => "missing",
        ReferencesTargetResolution.FragmentMissing => "fragment-missing",
        ReferencesTargetResolution.Malformed => "malformed",
        ReferencesTargetResolution.Absolute => "absolute",
        ReferencesTargetResolution.Query => "query",
        ReferencesTargetResolution.EncodingUnsupported => "encoding-unsupported",
        ReferencesTargetResolution.OutsideWorkspace => "outside-workspace",
        ReferencesTargetResolution.PhysicalEscape => "physical-escape",
        ReferencesTargetResolution.Ambiguous => "ambiguous",
        ReferencesTargetResolution.Unreadable => "unreadable",
        ReferencesTargetResolution.Unsupported => "unsupported",
        ReferencesTargetResolution.ExternalUnchecked => "external-unchecked",
        _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "The References target resolution is not defined."),
    };

    internal static string WireDirection(ReferencesDirection direction) => direction switch
    {
        ReferencesDirection.In => "in",
        ReferencesDirection.Out => "out",
        ReferencesDirection.Both => "both",
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "The References direction is not defined."),
    };

    internal static string WireLayer(ReferencesLayer layer) => layer switch
    {
        ReferencesLayer.Base => "base",
        ReferencesLayer.Overwrite => "overwrite",
        _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "The References source layer is not defined."),
    };

    internal static string FindingTitle(ReferencesFindingCode code) => code switch
    {
        ReferencesFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
        ReferencesFindingCode.InvalidSource => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceIsUnknown(),
        ReferencesFindingCode.InvalidDirection => global::OpenForge.Cli.OutputText.References.ReferencesText.TitleDirectionIsInvalid(),
        ReferencesFindingCode.InvalidFilter => global::OpenForge.Cli.OutputText.References.ReferencesText.TitleFilterIsInvalid(),
        ReferencesFindingCode.WorkspaceUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnavailable(),
        ReferencesFindingCode.WorkspaceUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnsafe(),
        ReferencesFindingCode.SourceAmbiguous => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceIsAmbiguous(),
        ReferencesFindingCode.SourceUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceIsUnsafe(),
        ReferencesFindingCode.SelectorAmbiguous => global::OpenForge.Cli.OutputText.References.ReferencesText.TitleFilterIsAmbiguous(),
        ReferencesFindingCode.SelectorUnsafe => global::OpenForge.Cli.OutputText.References.ReferencesText.TitleFilterIsUnsafe(),
        ReferencesFindingCode.IdentityCollision => global::OpenForge.Cli.OutputText.References.ReferencesText.TitleTwoSourcesShareAnIdentity(),
        ReferencesFindingCode.PhysicalAlias => global::OpenForge.Cli.OutputText.References.ReferencesText.TitleSourcesShareAPhysicalFile(),
        ReferencesFindingCode.IdentityUnavailable => global::OpenForge.Cli.OutputText.References.ReferencesText.TitleSourceIdentityIsUnavailable(),
        ReferencesFindingCode.CandidateUnsafe => global::OpenForge.Cli.OutputText.References.ReferencesText.TitleSourceCouldNotBeScannedSafely(),
        ReferencesFindingCode.LayerUnresolved => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOverwriteHasNoBaseFile(),
        ReferencesFindingCode.InspectionUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceCouldNotBeRead(),
        ReferencesFindingCode.InvalidEncoding => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceEncodingIsInvalid(),
        ReferencesFindingCode.LinkEncodingInvalid => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLinkEncodingIsInvalid(),
        ReferencesFindingCode.GeneratedRegionUnavailable => global::OpenForge.Cli.OutputText.References.ReferencesText.TitleEntriesSectionCouldNotBeIdentified(),
        ReferencesFindingCode.DestinationMalformed => global::OpenForge.Cli.OutputText.References.ReferencesText.TitleLinkIsNotResolvable(),
        ReferencesFindingCode.DestinationUnsupported => global::OpenForge.Cli.OutputText.References.ReferencesText.TitleLinkWasNotFollowed(),
        ReferencesFindingCode.TargetMissing => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLinkTargetIsMissing(),
        ReferencesFindingCode.FragmentMissing => global::OpenForge.Cli.OutputText.References.ReferencesText.TitleHeadingWasNotFound(),
        ReferencesFindingCode.TargetUnsafe => global::OpenForge.Cli.OutputText.References.ReferencesText.TitleLinkPointsOutsideTheWorkspace(),
        ReferencesFindingCode.TargetAmbiguous => global::OpenForge.Cli.OutputText.References.ReferencesText.TitleLinkCouldPointToMoreThanOneFile(),
        ReferencesFindingCode.TargetUnreadable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLinkTargetCouldNotBeRead(),
        ReferencesFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.References.ReferencesText.TitleReferencesFailed(),
        ReferencesFindingCode.Interrupted => global::OpenForge.Cli.OutputText.References.ReferencesText.TitleReferencesWasCancelled(),
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The References finding code is not defined."),
    };

    internal static string Counts(int incoming, int outgoing)
        => global::OpenForge.Cli.OutputText.References.ReferencesWording.Counts(incoming, outgoing);

    internal static string FilterSummary(IReadOnlyList<string> include, IReadOnlyList<string> exclude)
    {
        if (include.Count == 0 && exclude.Count == 0)
        {
            return global::OpenForge.Cli.OutputText.References.ReferencesText.MessageTheIncomingScanUsedEverySource();
        }

        if (exclude.Count == 0)
        {
            return global::OpenForge.Cli.OutputText.References.ReferencesPhrases.FormatTheIncomingScanUsedOnly($"{string.Join(", ", include)}");
        }

        return include.Count == 0
            ? global::OpenForge.Cli.OutputText.References.ReferencesPhrases.FormatTheIncomingScanSkipped($"{string.Join(", ", exclude)}")
            : global::OpenForge.Cli.OutputText.References.ReferencesPhrases.FormatTheIncomingScanUsedOnlyAndSkipped($"{string.Join(", ", include)}", $"{string.Join(", ", exclude)}");
    }

    internal static string ScannedHeading(int count)
        => global::OpenForge.Cli.OutputText.References.ReferencesPhrases.FormatScannedForIncomingLinks(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(count, "source", "sources")}"));

    internal static string DoctorNextReason() => global::OpenForge.Cli.OutputText.References.ReferencesText.LabelCheckTheWorkspaceForBrokenLinks();
    internal static string RouteListNextReason() => global::OpenForge.Cli.OutputText.References.ReferencesText.LabelListTheSourcesThatExist();
    internal static string HelpNextReason() => global::OpenForge.Cli.OutputText.References.ReferencesText.HelpNextReason();
    internal static string FailedNextReason() => global::OpenForge.Cli.OutputText.References.ReferencesText.LabelSeeTheDiagnosticsForThisFailure();
    internal static string RetryNextReason() => global::OpenForge.Cli.OutputText.References.ReferencesText.LabelRunItAgain();

    private static string Reason(string cause) => cause.Trim().TrimEnd('.');
}
