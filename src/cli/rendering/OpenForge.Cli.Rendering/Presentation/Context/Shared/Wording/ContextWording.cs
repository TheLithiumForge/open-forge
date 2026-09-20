using System.Globalization;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Presentation.Context.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Context.Shared.Wording;

internal static class ContextWording
{
    internal const string MetadataPartName = "metadata";

    internal static string HelpSyntax()
        => global::OpenForge.Cli.OutputText.Context.ContextText.HelpSyntax();

    internal static string HelpSelection()
        => global::OpenForge.Cli.OutputText.Context.ContextText.HelpSelection();

    internal static string HelpContent()
        => global::OpenForge.Cli.OutputText.Context.ContextText.HelpContent();

    internal static string HelpLinks()
        => global::OpenForge.Cli.OutputText.Context.ContextText.HelpLinks();

    internal static string HelpGlobalOptions()
        => global::OpenForge.Cli.OutputText.Shared.SharedText.HelpGlobalOptionsWithDetailSelection();

    internal static string HelpExamples()
        => global::OpenForge.Cli.OutputText.Context.ContextText.HelpExamples();

    internal static string HelpRelatedCommands()
        => "open-forge route list --depth=all — list source IDs and exact paths.\nopen-forge doctor — inspect unavailable source or link facts.";

    internal static string HelpNotes()
        => global::OpenForge.Cli.OutputText.Context.ContextText.HelpNotes();

    internal static string Completed() => global::OpenForge.Cli.OutputText.Context.ContextText.MessageContextIsComplete();

    internal static string CompletedWithWarnings() => global::OpenForge.Cli.OutputText.Context.ContextText.MessageContextCompletedWithWarnings();

    internal static string Incomplete() => global::OpenForge.Cli.OutputText.Context.ContextText.MessageContextCouldNotBeReadCompletely();

    internal static string CannotRead(string problem) => global::OpenForge.Cli.OutputText.Context.ContextPhrases.FormatCannotReadContext($"{problem.TrimEnd('.')}");

    internal static string Failed(string reason) => global::OpenForge.Cli.OutputText.Context.ContextPhrases.FormatContextStoppedBecauseOfAnUnexpectedError($"{reason.TrimEnd('.')}");

    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Context.ContextText.MessageContextWasCancelled();

    internal static string NoAdditional() => global::OpenForge.Cli.OutputText.Context.ContextText.MessageNoAdditionalContextTheSelectedSourcesAreAlreadyReadAtStartup();

    internal static string FindingCode(ContextFindingCode code)
        => code switch
        {
            ContextFindingCode.InvalidInput => "context.invalid-input",
            ContextFindingCode.InvalidSource => "context.invalid-source",
            ContextFindingCode.InvalidContent => "context.invalid-content",
            ContextFindingCode.InvalidLinkDepth => "context.invalid-link-depth",
            ContextFindingCode.WorkspaceUnavailable => "context.workspace-unavailable",
            ContextFindingCode.WorkspaceUnsafe => "context.workspace-unsafe",
            ContextFindingCode.SourceAmbiguous => "context.source-ambiguous",
            ContextFindingCode.SourceUnsafe => "context.source-unsafe",
            ContextFindingCode.OverwriteAmbiguous => "context.overwrite-ambiguous",
            ContextFindingCode.TargetAmbiguous => "context.target-ambiguous",
            ContextFindingCode.TargetUnsafe => "context.target-unsafe",
            ContextFindingCode.ClosureUnavailable => "context.closure-unavailable",
            ContextFindingCode.LayerUnavailable => "context.layer-unavailable",
            ContextFindingCode.InvalidEncoding => "context.invalid-encoding",
            ContextFindingCode.MarkdownUnavailable => "context.markdown-unavailable",
            ContextFindingCode.TargetMissing => "context.target-missing",
            ContextFindingCode.FragmentMissing => "context.fragment-missing",
            ContextFindingCode.LinkEncodingInvalid => "context.link-encoding-invalid",
            ContextFindingCode.TargetUnreadable => "context.target-unreadable",
            ContextFindingCode.SectionAmbiguous => "context.section-ambiguous",
            ContextFindingCode.ProjectionUnavailable => "context.projection-unavailable",
            ContextFindingCode.IdentityCollision => "context.identity-collision",
            ContextFindingCode.TargetCaseMismatch => "context.target-case-mismatch",
            ContextFindingCode.FrontmatterMissing => "context.frontmatter-missing",
            ContextFindingCode.SectionMissing => "context.section-missing",
            ContextFindingCode.OperationFailed => "context.operation-failed",
            ContextFindingCode.Interrupted => "context.interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Context finding code is not defined."),
        };

    internal static string Summary(long sources, long tokens)
        => FormattableString.Invariant($"{sources} {CliText.Plural(sources, "source")}, {CliText.Tokens(tokens)}.");

    internal static string ChooseSourceNext()
        => global::OpenForge.Cli.OutputText.Context.ContextText.MessageListSourceIdsAndExactPathsThenRerunContext();

    internal static string InspectUnavailableNext()
        => global::OpenForge.Cli.OutputText.Context.ContextText.MessageInspectTheUnavailableClosureSourceLinkOrProjectionFactsBeforeRelyingOnThisContextResult();

    internal static string RepairCaseMismatchNext()
        => global::OpenForge.Cli.OutputText.Context.ContextText.MessageRunRepairWithAutomaticToCorrectTheLinkTargetCasingThenRerunContext();

    internal static string Reason(ContextInclusionReason reason)
    {
        var value = reason.Kind switch
        {
            ContextInclusionReasonKind.WorkspaceEntry => global::OpenForge.Cli.OutputText.Context.ContextText.LabelWorkspaceEntry(),
            ContextInclusionReasonKind.Loader => global::OpenForge.Cli.OutputText.Context.ContextText.TitleLoader(),
            ContextInclusionReasonKind.LoadNow => global::OpenForge.Cli.OutputText.Context.ContextText.LabelLoadNow(),
            ContextInclusionReasonKind.KeepInMind => global::OpenForge.Cli.OutputText.Context.ContextText.LabelKeepInMind(),
            ContextInclusionReasonKind.AncestorRequired => global::OpenForge.Cli.OutputText.Context.ContextText.LabelAncestorRequired(),
            ContextInclusionReasonKind.SelectedSource => global::OpenForge.Cli.OutputText.Context.ContextText.LabelSelectedSource(),
            ContextInclusionReasonKind.ScopeLocal => global::OpenForge.Cli.OutputText.Context.ContextText.LabelScopeLocalLoading(),
            ContextInclusionReasonKind.LinkedSource => global::OpenForge.Cli.OutputText.Context.ContextText.LabelLinkedSource(),
            ContextInclusionReasonKind.OverwriteCompanion => global::OpenForge.Cli.OutputText.Context.ContextText.LabelOverwriteCompanion(),
            _ => throw new ArgumentOutOfRangeException(nameof(reason), reason.Kind, "The inclusion reason is not defined."),
        };
        if (reason.Source is not null)
        {
            value += global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatFrom($"{reason.Source.Path}");
        }

        if (reason.Reference is not null)
        {
            value += global::OpenForge.Cli.OutputText.Context.ContextPhrases.FormatFor($"{reason.Reference}");
        }

        if (reason.Depth is { } depth)
        {
            value = global::OpenForge.Cli.OutputText.Context.ContextPhrases.FormatAtDepth(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{value}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{depth}"));
        }

        return value;
    }

    internal static string FindingTitle(ContextFindingCode code)
        => code switch
        {
            ContextFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
            ContextFindingCode.InvalidSource => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidSource(),
            ContextFindingCode.InvalidContent => global::OpenForge.Cli.OutputText.Context.ContextText.TitleInvalidContent(),
            ContextFindingCode.InvalidLinkDepth => global::OpenForge.Cli.OutputText.Context.ContextText.TitleInvalidLinkDepth(),
            ContextFindingCode.WorkspaceUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnavailable(),
            ContextFindingCode.WorkspaceUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnsafe(),
            ContextFindingCode.SourceAmbiguous => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceIsAmbiguous(),
            ContextFindingCode.SourceUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceIsUnsafe(),
            ContextFindingCode.OverwriteAmbiguous => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOverwriteIsAmbiguous(),
            ContextFindingCode.TargetAmbiguous => global::OpenForge.Cli.OutputText.Context.ContextText.TitleLinkTargetIsAmbiguous(),
            ContextFindingCode.TargetUnsafe => global::OpenForge.Cli.OutputText.Context.ContextText.TitleLinkTargetIsUnsafe(),
            ContextFindingCode.ClosureUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleStartupContextIsUnavailable(),
            ContextFindingCode.LayerUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceCouldNotBeRead(),
            ContextFindingCode.InvalidEncoding => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceEncodingIsInvalid(),
            ContextFindingCode.MarkdownUnavailable => global::OpenForge.Cli.OutputText.Context.ContextText.TitleSourceMarkdownIsUnavailable(),
            ContextFindingCode.TargetMissing => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLinkTargetIsMissing(),
            ContextFindingCode.FragmentMissing => global::OpenForge.Cli.OutputText.Context.ContextText.TitleLinkHeadingIsMissing(),
            ContextFindingCode.LinkEncodingInvalid => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLinkEncodingIsInvalid(),
            ContextFindingCode.TargetUnreadable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLinkTargetCouldNotBeRead(),
            ContextFindingCode.SectionAmbiguous => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSectionIsAmbiguous(),
            ContextFindingCode.ProjectionUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleContentPartIsUnavailable(),
            ContextFindingCode.IdentityCollision => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceIdentityCollides(),
            ContextFindingCode.TargetCaseMismatch => global::OpenForge.Cli.OutputText.Context.ContextText.TitleLinkTargetCasingDiffers(),
            ContextFindingCode.FrontmatterMissing => global::OpenForge.Cli.OutputText.Context.ContextText.TitleFrontmatterIsMissing(),
            ContextFindingCode.SectionMissing => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSectionIsMissing(),
            ContextFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Context.ContextText.TitleContextFailed(),
            ContextFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Context.ContextText.TitleContextWasCancelled(),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Context finding code is not defined."),
        };

    internal static string FindingMessage(ContextFinding finding)
    {
        var problem = FindingProblem(finding);
        return finding.Code == ContextFindingCode.InvalidInput
            || finding.Code == ContextFindingCode.InvalidSource
                && finding.InvalidSourceKind == ContextInvalidSourceKind.Other
            ? CannotRead(problem)
            : problem;
    }

    internal static string FindingProblem(ContextFinding finding)
    {
        var path = finding.Path ?? finding.Source?.Path ?? finding.Subject ?? global::OpenForge.Cli.OutputText.Context.ContextText.LabelTheRequestedSource();
        var reason = finding.Cause.TrimEnd('.');
        return finding.Code switch
        {
            ContextFindingCode.InvalidInput => reason,
            ContextFindingCode.InvalidSource
                when finding.InvalidSourceKind == ContextInvalidSourceKind.UnknownId
                => CliFindingWording.UnknownSource(
                    finding.Subject ?? finding.Reference ?? global::OpenForge.Cli.OutputText.Context.ContextText.LabelTheRequestedSource()),
            ContextFindingCode.InvalidSource => reason,
            ContextFindingCode.InvalidContent => global::OpenForge.Cli.OutputText.Context.ContextPhrases.FormatContentIsNotAKnownPartUseMetadataPathsFrontmatterHeadingsBodyOrSectionName($"{finding.Subject ?? "value"}"),
            ContextFindingCode.InvalidLinkDepth => global::OpenForge.Cli.OutputText.Context.ContextText.MessageFollowLinksMustBeAPositiveNumberOrAll(),
            ContextFindingCode.WorkspaceUnavailable => CliFindingWording.WorkspaceUnavailable(path),
            ContextFindingCode.WorkspaceUnsafe => CliFindingWording.WorkspaceUnsafe(path, reason),
            ContextFindingCode.SourceAmbiguous => CliFindingWording.SourceAmbiguous(path),
            ContextFindingCode.SourceUnsafe => CliFindingWording.SourceUnsafe(path),
            ContextFindingCode.OverwriteAmbiguous => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatCouldBelongToMoreThanOneBaseFile($"{path}"),
            ContextFindingCode.TargetAmbiguous => global::OpenForge.Cli.OutputText.Context.ContextPhrases.FormatTheLinkAtCouldPointToMoreThanOneFileItWasNotFollowed($"{Location(finding)}"),
            ContextFindingCode.TargetUnsafe => global::OpenForge.Cli.OutputText.Context.ContextPhrases.FormatTheLinkAtPointsOutsideTheWorkspaceItWasNotFollowed($"{Location(finding)}"),
            ContextFindingCode.ClosureUnavailable => global::OpenForge.Cli.OutputText.Context.ContextPhrases.FormatTheStartupFilesCouldNotBeResolved($"{reason}"),
            ContextFindingCode.LayerUnavailable => global::OpenForge.Cli.OutputText.Context.ContextPhrases.FormatCouldNotBeReadSoItWasNotIncluded($"{path}"),
            ContextFindingCode.InvalidEncoding => global::OpenForge.Cli.OutputText.Context.ContextPhrases.FormatIsNotValidUtf8SoItWasNotIncluded($"{path}"),
            ContextFindingCode.MarkdownUnavailable => global::OpenForge.Cli.OutputText.Context.ContextPhrases.FormatCouldNotBeParsedAsMarkdownSoItsWasNotProduced($"{path}", $"{finding.Part?.CanonicalValue ?? "content"}"),
            ContextFindingCode.TargetMissing => global::OpenForge.Cli.OutputText.Context.ContextPhrases.FormatTheLinkAtPointsToWhichDoesNotExistItWasNotFollowed($"{Location(finding)}", $"{finding.Subject ?? "a missing file"}"),
            ContextFindingCode.FragmentMissing => global::OpenForge.Cli.OutputText.Context.ContextPhrases.FormatTheLinkAtPointsToWhichHasNoHeadingItWasNotFollowed($"{Location(finding)}", $"{path}", $"{finding.Subject ?? "the requested fragment"}"),
            ContextFindingCode.LinkEncodingInvalid => global::OpenForge.Cli.OutputText.Context.ContextPhrases.FormatTheLinkAtHasAnEncodingThatCannotBeResolvedItWasNotFollowed($"{Location(finding)}"),
            ContextFindingCode.TargetUnreadable => global::OpenForge.Cli.OutputText.Context.ContextPhrases.FormatTheLinkAtPointsToWhichCouldNotBeReadItWasNotFollowed($"{Location(finding)}", $"{path}"),
            ContextFindingCode.SectionAmbiguous => global::OpenForge.Cli.OutputText.Context.ContextPhrases.FormatHasMoreThanOneSectionNamedNoneWasIncluded($"{path}", $"{finding.Part?.Name ?? finding.Subject ?? "the requested section"}"),
            ContextFindingCode.ProjectionUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatTheOfCouldNotBeProduced($"{finding.Part?.CanonicalValue ?? "content"}", $"{path}", $"{reason}"),
            ContextFindingCode.IdentityCollision => CliFindingWording.IdentityCollision(finding.Subject ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheRequestedId()),
            ContextFindingCode.TargetCaseMismatch => global::OpenForge.Cli.OutputText.Context.ContextPhrases.FormatTheLinkAtIsWrittenButTheFileIsNamed($"{Location(finding)}", $"{finding.Subject ?? "the destination"}", $"{path}"),
            ContextFindingCode.FrontmatterMissing => global::OpenForge.Cli.OutputText.Context.ContextPhrases.FormatHasNoFrontmatter($"{path}"),
            ContextFindingCode.SectionMissing => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatHasNoSectionNamed($"{path}", $"{finding.Part?.Name ?? finding.Subject ?? "the requested section"}"),
            ContextFindingCode.OperationFailed => CliFindingWording.OperationFailed(global::OpenForge.Cli.OutputText.Context.ContextText.TitleContext(), reason),
            ContextFindingCode.Interrupted => CliFindingWording.Interrupted(global::OpenForge.Cli.OutputText.Context.ContextText.TitleContext()),
            _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code, "The Context finding code is not defined."),
        };
    }

    internal static string LinksHeading() => global::OpenForge.Cli.OutputText.Context.ContextText.HeadingLinks();

    internal static string LinkArrow() => " -> ";

    internal static string LinkDetailSeparator() => "; ";

    internal static ContextLinkWording LinkRow(
        string location,
        string destination,
        string resolution,
        bool followed)
        => new(location, destination, resolution, LinkState(followed));

    internal static string LinkState(bool followed)
        => followed ? global::OpenForge.Cli.OutputText.Context.ContextText.LabelFollowed() : global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotFollowed();

    internal static string Layer(ContextSourceLayerKind layer)
        => layer switch
        {
            ContextSourceLayerKind.Base => "base",
            ContextSourceLayerKind.Overwrite => "overwrite",
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "The Context source layer is not defined."),
        };

    internal static string Resolution(ContextLinkResolution resolution)
        => resolution switch
        {
            ContextLinkResolution.Complete => "complete",
            ContextLinkResolution.Missing => "missing",
            ContextLinkResolution.FragmentMissing => "fragment-missing",
            ContextLinkResolution.CaseMismatch => "case-mismatch",
            ContextLinkResolution.Malformed => "malformed",
            ContextLinkResolution.Absolute => "absolute",
            ContextLinkResolution.Query => "query",
            ContextLinkResolution.EncodingUnsupported => "encoding-unsupported",
            ContextLinkResolution.OutsideWorkspace => "outside-workspace",
            ContextLinkResolution.PhysicalEscape => "physical-escape",
            ContextLinkResolution.Ambiguous => "ambiguous",
            ContextLinkResolution.Unreadable => "unreadable",
            ContextLinkResolution.Unsupported => "unsupported",
            ContextLinkResolution.ExternalUnchecked => "external-unchecked",
            _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "The Context link resolution is not defined."),
        };

    internal static string Disposition(ContextLinkDisposition disposition)
        => disposition switch
        {
            ContextLinkDisposition.Selected => "selected",
            ContextLinkDisposition.AlreadySelected => "already-selected",
            ContextLinkDisposition.Cycle => "cycle",
            ContextLinkDisposition.ExternalUnchecked => "external-unchecked",
            ContextLinkDisposition.Unresolved => "unresolved",
            _ => throw new ArgumentOutOfRangeException(nameof(disposition), disposition, "The Context link disposition is not defined."),
        };

    private static string Location(ContextFinding finding)
        => finding.LocationView is { } location
            ? string.Create(
                CultureInfo.InvariantCulture,
                $"{finding.Source?.Path ?? finding.Path ?? "the source"}:{location.Line}:{location.Column}")
            : finding.Source?.Path ?? finding.Path ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheSource();
}

internal sealed record ContextLinkWording(
    string Location,
    string Destination,
    string Resolution,
    string State);
