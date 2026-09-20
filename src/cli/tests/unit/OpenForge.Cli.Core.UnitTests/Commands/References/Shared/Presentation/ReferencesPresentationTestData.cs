using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Selection;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.References.Shared.Presentation;

internal static class ReferencesPresentationTestData
{
    internal static IReadOnlyList<CliSemanticStatus> Statuses =>
    [
        CliSemanticStatus.Complete,
        CliSemanticStatus.Attention,
        CliSemanticStatus.Incomplete,
        CliSemanticStatus.Invalid,
        CliSemanticStatus.Blocked,
        CliSemanticStatus.Failed,
        CliSemanticStatus.Interrupted,
    ];

    internal static ReferencesResult ForStatus(CliSemanticStatus status)
        => status switch
        {
            CliSemanticStatus.Complete => CompleteResult(),
            CliSemanticStatus.Attention => AttentionResult(),
            CliSemanticStatus.Incomplete => IncompleteResult(),
            CliSemanticStatus.Invalid => InvalidResult(),
            CliSemanticStatus.Blocked => BlockedResult(),
            CliSemanticStatus.Failed => FailedResult(),
            CliSemanticStatus.Interrupted => InterruptedResult(),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The References status is not defined."),
        };

    internal static ReferencesResult CompleteResult()
        => Build(
            CliSemanticStatus.Complete,
            ReferencesDirection.Both,
            Source(),
            Selection(filtered: true),
            IncomingSection(CliSemanticStatus.Complete),
            OutgoingSection(CliSemanticStatus.Complete),
            [],
            null);

    internal static ReferencesResult AttentionResult()
    {
        var source = Source();
        var location = Location(3, 1, 24, 22);
        var finding = new ReferencesFinding(
            ReferencesFindingCode.DestinationUnsupported,
            ReferencesDirection.Out,
            "mailto:docs@example.invalid",
            "The destination scheme is unsupported.",
            null,
            null,
            source.Identity,
            SourceLayerKind.Base,
            source.Layers[0].Path,
            location,
            null,
            []);
        return Build(
            CliSemanticStatus.Attention,
            ReferencesDirection.Out,
            source,
            null,
            null,
            OutgoingSection(CliSemanticStatus.Attention),
            [finding],
            ReferencesDefinitions.ReadNextAction(CliSemanticStatus.Attention, [finding]));
    }

    internal static ReferencesResult IncompleteResult()
    {
        var source = Source();
        var finding = new ReferencesFinding(
            ReferencesFindingCode.InspectionUnavailable,
            ReferencesDirection.In,
            source.Id,
            "The incoming source layer could not be inspected.",
            null,
            null,
            source.Identity,
            SourceLayerKind.Base,
            source.Layers[0].Path,
            null,
            null,
            []);
        return Build(
            CliSemanticStatus.Incomplete,
            ReferencesDirection.In,
            source,
            Selection(filtered: false),
            IncomingSection(CliSemanticStatus.Incomplete),
            null,
            [finding],
            ReferencesDefinitions.ReadNextAction(CliSemanticStatus.Incomplete, [finding]));
    }

    internal static ReferencesResult InvalidResult()
    {
        var finding = new ReferencesFinding(
            ReferencesFindingCode.InvalidDirection,
            null,
            "incoming",
            "The direction value is not one of in, out, or both.",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            []);
        return Build(
            CliSemanticStatus.Invalid,
            null,
            null,
            null,
            null,
            null,
            [finding],
            ReferencesDefinitions.ReadNextAction(CliSemanticStatus.Invalid, [finding]));
    }

    internal static ReferencesResult BlockedResult()
    {
        var finding = new ReferencesFinding(
            ReferencesFindingCode.WorkspaceUnavailable,
            null,
            "/missing/workspace",
            "The selected workspace is unavailable.",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            []);
        return new ReferencesResult(
            null,
            null,
            ReferencesDirection.Both,
            Selection(filtered: false),
            new ReferencesSection(ReferencesCoverage.Blocked, CliSemanticStatus.Blocked, []),
            new ReferencesSection(ReferencesCoverage.Blocked, CliSemanticStatus.Blocked, []),
            [finding],
            CliSemanticStatus.Blocked,
            ReferencesDefinitions.ReadNextAction(CliSemanticStatus.Blocked, [finding]));
    }

    internal static ReferencesResult FailedResult()
        => TerminalResult(
            CliSemanticStatus.Failed,
            ReferencesFindingCode.OperationFailed,
            "The References operation failed at its bounded operating boundary.",
            "open-forge references --detail debug",
            "Report the failure and retry the same References request with bounded diagnostics.");

    internal static ReferencesResult InterruptedResult()
        => TerminalResult(
            CliSemanticStatus.Interrupted,
            ReferencesFindingCode.Interrupted,
            "The References operation was interrupted at its bounded operating boundary.",
            "open-forge references",
            "Rerun the same References request.");

    internal static ReferencesResult ControlCharacterResult()
    {
        var source = Source(withOverwrite: false);
        var occurrence = new ReferencesOccurrence(
            ReferencesDirection.Out,
            new ReferencesOccurrenceSource("docs", ".agents/docs.md", SourceLayerKind.Base),
            Location(4, 1, 34, 18),
            null,
            "line\nbreak\t<unicode-λ>",
            null,
            new ReferencesTarget(
                ReferencesTargetKind.Local,
                null,
                ".agents/missing.md",
                null,
                ReferencesTargetResolution.Missing,
                null),
            ReferencesProvenance.SelectedSource);
        var finding = new ReferencesFinding(
            ReferencesFindingCode.TargetMissing,
            ReferencesDirection.Out,
            "line\nbreak",
            "control\tcause",
            null,
            null,
            source.Identity,
            SourceLayerKind.Base,
            source.Layers[0].Path,
            occurrence.Location,
            null,
            []);
        return Build(
            CliSemanticStatus.Attention,
            ReferencesDirection.Out,
            source,
            null,
            null,
            new ReferencesSection(ReferencesCoverage.Complete, CliSemanticStatus.Attention, [occurrence]),
            [finding],
            ReferencesDefinitions.ReadNextAction(CliSemanticStatus.Attention, [finding]));
    }

    internal static CliPresentationRequest<ReferencesResult> Presentation(
        ReferencesResult result,
        CliFormat format = CliFormat.Text,
        CliDetail view = CliDetail.Standard,
        CliDetail? diagnosticDetail = null)
        => CliPresentationStage.Create(result, new CliPresentation(format, diagnosticDetail ?? view, null));

    internal static ReferencesSource Source(bool withOverwrite = true)
        => new(
            "docs",
            ".agents/docs.md",
            withOverwrite
                ? [
                    new ReferencesSourceLayer(SourceLayerKind.Base, ".agents/docs.md"),
                    new ReferencesSourceLayer(SourceLayerKind.Overwrite, ".agents/docs.overwrite.md"),
                ]
                : [new ReferencesSourceLayer(SourceLayerKind.Base, ".agents/docs.md")]);

    internal static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "references-presentation-red"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    private static ReferencesResult Build(
        CliSemanticStatus status,
        ReferencesDirection? direction,
        ReferencesSource? source,
        ReferencesIncomingSelection? selection,
        ReferencesSection? incoming,
        ReferencesSection? outgoing,
        IReadOnlyList<ReferencesFinding> findings,
        CliNextAction? next)
        => new(
            Workspace(),
            source,
            direction,
            selection,
            incoming,
            outgoing,
            findings,
            status,
            next);

    private static ReferencesResult TerminalResult(
        CliSemanticStatus status,
        ReferencesFindingCode code,
        string cause,
        string nextCommand,
        string nextReason)
    {
        var finding = new ReferencesFinding(
            code,
            null,
            null,
            cause,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            []);
        return Build(
            status,
            ReferencesDirection.Both,
            Source(),
            Selection(filtered: false),
            new ReferencesSection(ReferencesCoverage.Incomplete, status, []),
            new ReferencesSection(ReferencesCoverage.Incomplete, status, []),
            [finding],
            new CliNextAction(nextCommand, nextReason));
    }

    private static ReferencesIncomingSelection Selection(bool filtered)
    {
        if (!filtered)
        {
            return new ReferencesIncomingSelection(
                ReferencesSelectionMode.Default,
                [],
                [],
                [new ReferencesSourceIdentity("alpha", ".agents/alpha.md")],
                [
                    new ReferencesSourceLayerEvidence(
                        new ReferencesSourceIdentity("alpha", ".agents/alpha.md"),
                        SourceLayerKind.Base,
                        ".agents/alpha.md"),
                ]);
        }

        var source = new ReferencesSourceIdentity("alpha", ".agents/alpha.md");
        var supplied = new ReferencesSelectorOccurrence(SourceUniverseSelectorRole.Include, "alpha");
        var resolved = new ReferencesSelectorResolution(
            SourceUniverseSelectorRole.Include,
            1,
            "alpha",
            SourceReferenceKind.SourceId,
            SourceReferenceResolutionState.Resolved,
            source,
            SourceUniverseSelectorExpansion.Source,
            []);
        return new ReferencesIncomingSelection(
            ReferencesSelectionMode.Filtered,
            [supplied],
            [resolved],
            [source],
            [new ReferencesSourceLayerEvidence(source, SourceLayerKind.Base, ".agents/alpha.md")]);
    }

    private static ReferencesSection IncomingSection(CliSemanticStatus status)
        => new(
            status == CliSemanticStatus.Incomplete ? ReferencesCoverage.Incomplete : ReferencesCoverage.Complete,
            status,
            [
                new ReferencesOccurrence(
                    ReferencesDirection.In,
                    new ReferencesOccurrenceSource("alpha", ".agents/alpha.md", SourceLayerKind.Base),
                    Location(7, 3, 96, 18),
                    Location(7, 12, 105, 8),
                    "docs.md#Overview",
                    "Overview",
                    new ReferencesTarget(
                        ReferencesTargetKind.Local,
                        "docs",
                        ".agents/docs.md",
                        SourceLayerKind.Base,
                        ReferencesTargetResolution.Complete,
                        null),
                    ReferencesProvenance.FilteredIncomingScan),
            ]);

    private static ReferencesSection OutgoingSection(CliSemanticStatus status)
        => new(
            ReferencesCoverage.Complete,
            status,
            [
                new ReferencesOccurrence(
                    ReferencesDirection.Out,
                    new ReferencesOccurrenceSource("docs", ".agents/docs.md", SourceLayerKind.Base),
                    Location(4, 1, 34, 18),
                    Location(4, 9, 42, 10),
                    "guide.md#Start",
                    "Start",
                    new ReferencesTarget(
                        ReferencesTargetKind.Local,
                        "guide",
                        ".agents/guide.md",
                        SourceLayerKind.Base,
                        ReferencesTargetResolution.Complete,
                        null),
                    ReferencesProvenance.SelectedSource),
                new ReferencesOccurrence(
                    ReferencesDirection.Out,
                    new ReferencesOccurrenceSource("docs", ".agents/docs.overwrite.md", SourceLayerKind.Overwrite),
                    Location(6, 1, 72, 33),
                    null,
                    "https://example.invalid/Unicode%20target",
                    null,
                    new ReferencesTarget(
                        ReferencesTargetKind.External,
                        null,
                        null,
                        null,
                        ReferencesTargetResolution.ExternalUnchecked,
                        ReferencesNetworkState.NetworkNotAttempted),
                    ReferencesProvenance.SelectedSource),
            ]);

    private static SourceLocation Location(int line, int column, long offset, long length)
        => new(line, column, offset, length);
}
