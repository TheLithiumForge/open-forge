using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Init.Models;
using OpenForge.Cli.Core.Presentation.Route.Init.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Route.Init.Shared.Selection;

internal static class RouteInitReportSelector
{
    internal static CliReport<RouteInitData> Select(
        RouteInitResult result,
        CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        return new CliReport<RouteInitData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result),
            HeadlineFindingCode = HeadlineFindingCode(result),
            Workspace = result.WorkspacePath is { } workspace
                ? new CliWorkspaceEcho(workspace, result.WorkspaceExplicit)
                : null,
            Findings = result.Findings.Select(finding => Finding(result, finding)).ToArray(),
            Effects = result.Effects.Select(effect => Effect(effect, selection.Detail)).ToArray(),
            Counts =
            [
                new CliCount("entrypointsCreated", global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.LabelEntrypointsCreated(), CountProgressedEntrypoints(result)),
                new CliCount("entrypointsPresent", global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.LabelEntrypointsPresent(), CountPresentEntrypoints(result)),
                new CliCount("sectionsUpdated", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelSectionsUpdated(), CountProgressedSections(result)),
            ],
            Data = Data(result, selection.Detail),
            Recovery = Recovery(result),
            Next = Next(result),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? Diagnostics(result)
                : [],
        };
    }

    private static RouteInitData Data(RouteInitResult result, CliDetail detail)
    {
        var includeStandard = detail >= CliDetail.Standard;
        var includeFull = detail >= CliDetail.Full;
        var metadata = TargetEntrypoint(result)?.Metadata;
        var sections = result.Effects
            .Where(effect => effect.Kind == RouteInitEffectKind.GeneratedRegion)
            .Select(effect => new RouteInitDataSection
            {
                Path = effect.Path,
                Before = effect.Change?.Before ?? "unavailable",
                After = effect.Change?.Expected ?? "unavailable",
                Verification = Verification(effect),
                TextBefore = effect.Change?.Before is { } before
                    ? Hash(before)
                    : "unavailable",
                TextAfter = effect.Change?.Expected is { } after
                    ? Hash(after)
                    : "unavailable",
            })
            .ToArray();
        var entrypoints = result.Entrypoints
            .Select(entrypoint => new RouteInitDataEntrypoint
            {
                Path = entrypoint.Path,
                Outcome = RouteInitWireVocabulary.Name(entrypoint.Outcome),
                NeedsAuthoring = NeedsAuthoring(entrypoint),
                Content = includeFull ? EntrypointContent(result, entrypoint) : null,
            })
            .ToArray();
        return new RouteInitData
        {
            Mode = RouteInitWireVocabulary.Name(result.Mode),
            Target = new RouteInitDataTarget
            {
                Id = result.Target.Id,
                Path = result.Target.Path,
            },
            Scaffold = RouteInitWireVocabulary.Name(result.Scaffold),
            Entrypoints = entrypoints,
            ListedIn = result.Effects
                .Where(effect => effect.Kind == RouteInitEffectKind.GeneratedRegion)
                .Select(effect => effect.Path)
                .ToArray(),
            Metadata = includeStandard && metadata is { } value
                ? new RouteInitDataMetadata
                {
                    Description = value.Description,
                    Responsibility = value.Responsibility,
                    Tags = value.Tags,
                    Sources = new RouteInitDataMetadataSources
                    {
                        Description = RouteInitWireVocabulary.Name(value.DescriptionSource),
                        Responsibility = RouteInitWireVocabulary.Name(value.ResponsibilitySource),
                        Tags = RouteInitWireVocabulary.Name(value.TagsSource),
                    },
                }
                : null,
            LockPath = includeStandard && HasRecordedFrameworkOwnership(result)
                ? ".agents/open-forge.lock.json"
                : null,
            Sections = includeFull ? sections : null,
            FrameworkFingerprint = includeFull ? result.Framework?.InventoryFingerprint : null,
            Verification = includeFull
                ? RouteInitWireVocabulary.Name(result.Verification)
                : null,
            TextMetadata = TextMetadata(result, includeStandard, metadata),
            TextRows = TextRows(result, detail),
            TextEntrypoints = includeFull
                ? entrypoints.Where(entrypoint =>
                    entrypoint.Content is not null
                    && (entrypoint.Outcome == "created"
                        || (result.Mode == RouteInitMode.DryRun && entrypoint.Outcome == "planned")))
                    .ToArray()
                : [],
            TextSections = includeFull ? sections : [],
            TextAdvisory = result.Entrypoints.Any(NeedsAuthoring)
                ? RouteInitWording.Advisory()
                : null,
            TextRecovery = includeFull
                ? RouteInitWording.Recovery(result.Recovery)
                : null,
        };
    }

    private static IReadOnlyList<string> TextMetadata(
        RouteInitResult result,
        bool includeStandard,
        RouteInitMetadata? metadata)
    {
        if (!includeStandard)
        {
            return [];
        }

        var lines = new List<string>
        {
            RouteInitWording.Scaffold(RouteInitWireVocabulary.Name(result.Scaffold)),
        };
        if (metadata is { } value)
        {
            lines.Add(RouteInitWording.MetadataDescription(value.Description));
            if (value.Responsibility is { } responsibility)
            {
                lines.Add(RouteInitWording.MetadataResponsibility(responsibility));
            }

            if (!value.Tags.IsDefaultOrEmpty)
            {
                lines.Add(RouteInitWording.MetadataTags(value.Tags));
            }
        }

        if (HasRecordedFrameworkOwnership(result)
            && result.Lifecycle.Action != RouteInitLifecycleAction.None)
        {
            lines.Add(RouteInitWording.LockUpdated(
                result.Lifecycle.Outcome == RouteInitLifecycleOutcome.Planned));
        }

        return lines;
    }

    private static IReadOnlyList<string> TextRows(
        RouteInitResult result,
        CliDetail detail)
    {
        var rows = new List<string>();
        if (result.Status is CliSemanticStatus.Failed or CliSemanticStatus.Interrupted)
        {
            rows.AddRange(result.Effects.Select(EffectRow));
            return rows;
        }

        if (detail == CliDetail.Minimal)
        {
            var changingEntrypoints = result.Entrypoints
                .Where(entrypoint => entrypoint.Outcome != RouteInitEntrypointOutcome.Unchanged)
                .ToArray();
            if (changingEntrypoints.Length > 1)
            {
                rows.AddRange(changingEntrypoints.Select(entrypoint => entrypoint.Path));
            }
        }
        else
        {
            rows.AddRange(result.Entrypoints.Select(entrypoint =>
                RouteInitWording.EntryPointState(
                    entrypoint.Path,
                    EntrypointOutcome(entrypoint))));
        }

        foreach (var effect in result.Effects.Where(effect =>
            effect.Kind == RouteInitEffectKind.GeneratedRegion))
        {
            rows.Add(EffectRow(effect));
        }

        return rows;
    }

    private static string EffectRow(RouteInitEffect effect)
        => effect.Outcome switch
        {
            RouteInitEffectOutcome.Planned when effect.Kind == RouteInitEffectKind.GeneratedRegion
                => RouteInitWording.WouldList(effect.Path),
            RouteInitEffectOutcome.Verified when effect.Kind == RouteInitEffectKind.GeneratedRegion
                => RouteInitWording.Listed(effect.Path),
            RouteInitEffectOutcome.Planned
                => global::OpenForge.Cli.OutputText.Route.Init.RouteInitPhrases.FormatWouldCreate($"{effect.Path}"),
            RouteInitEffectOutcome.Verified
                => RouteInitWording.Created(effect.Path),
            RouteInitEffectOutcome.NotStarted
                => global::OpenForge.Cli.OutputText.Route.Init.RouteInitPhrases.FormatNotStarted($"{effect.Path}"),
            RouteInitEffectOutcome.CompletionUnknown
                => global::OpenForge.Cli.OutputText.Route.Init.RouteInitPhrases.FormatFinalStateUnknown($"{effect.Path}"),
            RouteInitEffectOutcome.VerificationFailed
                => global::OpenForge.Cli.OutputText.Route.Init.RouteInitPhrases.FormatFailed($"{effect.Path}"),
            _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Outcome,
                "The Route Init effect outcome is not defined."),
        };

    private static string EntrypointOutcome(RouteInitEntrypoint entrypoint)
        => entrypoint.Outcome switch
        {
            RouteInitEntrypointOutcome.Unchanged => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.LabelAlreadyPresent(),
            RouteInitEntrypointOutcome.Planned => global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.LabelWouldCreate(),
            RouteInitEntrypointOutcome.Created => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCreated(),
            RouteInitEntrypointOutcome.NotStarted => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            RouteInitEntrypointOutcome.CompletionUnknown => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown(),
            RouteInitEntrypointOutcome.VerificationFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFailed(),
            _ => throw new ArgumentOutOfRangeException(nameof(entrypoint), entrypoint.Outcome,
                "The Route Init entrypoint outcome is not defined."),
        };

    private static CliHeadline Headline(RouteInitResult result)
    {
        var target = result.Target.Path ?? result.Target.Requested;
        var id = result.Target.Id ?? result.Target.Requested;
        var missingEntrypoints = result.Entrypoints
            .Where(entrypoint => entrypoint.Outcome != RouteInitEntrypointOutcome.Unchanged)
            .ToArray();
        var first = result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Findings.FirstOrDefault();
        return result.Status switch
        {
            CliSemanticStatus.Complete when missingEntrypoints.Length == 0
                => new(RouteInitWording.AlreadyInitialized(id), CliHeadlineKind.NothingToDo),
            CliSemanticStatus.Complete when missingEntrypoints.Length == 1
                && result.Mode == RouteInitMode.DryRun
                => new(RouteInitWording.WouldCreate(missingEntrypoints[0].Path), CliHeadlineKind.Preview),
            CliSemanticStatus.Complete when missingEntrypoints.Length == 1
                => new(RouteInitWording.Created(missingEntrypoints[0].Path), CliHeadlineKind.Done),
            CliSemanticStatus.Complete when result.Mode == RouteInitMode.DryRun
                => new(RouteInitWording.WouldCreateMany(missingEntrypoints.Length, id), CliHeadlineKind.Preview),
            CliSemanticStatus.Complete
                => new(RouteInitWording.CreatedMany(missingEntrypoints.Length, id), CliHeadlineKind.Done),
            CliSemanticStatus.Attention when missingEntrypoints.Length == 1
                && result.Mode == RouteInitMode.DryRun
                => new(RouteInitWording.WouldCreate(missingEntrypoints[0].Path), CliHeadlineKind.Warnings),
            CliSemanticStatus.Attention when result.Mode == RouteInitMode.DryRun
                => new(RouteInitWording.WouldCreateMany(missingEntrypoints.Length, id), CliHeadlineKind.Warnings),
            CliSemanticStatus.Attention when missingEntrypoints.Length == 1
                => new(RouteInitWording.Created(missingEntrypoints[0].Path), CliHeadlineKind.Warnings),
            CliSemanticStatus.Attention
                => new(RouteInitWording.CreatedMany(missingEntrypoints.Length, id), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete
                => new(RouteInitWording.Incomplete(Message(result, first)), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid
                => new(InvalidHeadline(result, first), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked
                => new(RouteInitWording.Blocked(target, Message(result, first)), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed
                => new(RouteInitWording.Failed(ProgressedCount(result), result.Effects.Length), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted
                => new(RouteInitWording.Cancelled(), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status,
                "The Route Init status is not defined."),
        };
    }

    private static string InvalidHeadline(
        RouteInitResult result,
        RouteInitFinding? finding)
    {
        if (finding is null)
        {
            return RouteInitWording.Invalid(result.Target.Requested, global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheRequestIsInvalid());
        }

        if (finding.Code == RouteInitFindingCode.InvalidTarget
            && string.Equals(result.Target.Requested, "loader", StringComparison.Ordinal))
        {
            return RouteInitWording.InvalidTarget(result.Target.Requested);
        }

        return RouteInitWording.Invalid(
            result.Target.Requested,
            finding.Code == RouteInitFindingCode.InvalidTarget
                ? global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.LabelNotARouteIdOrAnEntrypointPathUnderAgents()
                : Message(result, finding));
    }

    private static string Message(
        RouteInitResult result,
        RouteInitFinding? finding)
    {
        if (finding is null)
        {
            return global::OpenForge.Cli.OutputText.Route.Init.RouteInitText.LabelTheRouteInitResultDidNotContainAFinding();
        }

        return RouteInitWording.Message(
            finding.Code,
            finding.Target ?? result.Target.Requested,
            result.WorkspacePath ?? finding.Target ?? result.Target.Requested,
            finding.Cause,
            ProgressedCount(result),
            result.Effects.Length,
            result.Recovery.ResidualPath);
    }

    private static string? HeadlineFindingCode(RouteInitResult result)
    {
        if (result.Findings.Length != 1)
        {
            return null;
        }

        return result.Status is CliSemanticStatus.Invalid
            or CliSemanticStatus.Blocked
            or CliSemanticStatus.Incomplete
            ? RouteInitWireVocabulary.Name(result.Findings[0].Code)
            : null;
    }

    private static CliFinding Finding(
        RouteInitResult result,
        RouteInitFinding finding)
    {
        var target = finding.Target
            ?? result.Target.Path
            ?? result.Target.Id
            ?? result.Target.Requested;
        var action = Action(result, finding);
        return new CliFinding
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = RouteInitWireVocabulary.Name(finding.Code),
            Title = RouteInitWording.FindingTitle(finding.Code),
            Message = RouteInitWording.Message(
                finding.Code,
                target,
                result.WorkspacePath ?? target,
                finding.Cause,
                ProgressedCount(result),
                result.Effects.Length,
                result.Recovery.ResidualPath),
            Subject = Subject(result, finding, target),
            Resolution = finding.Code == RouteInitFindingCode.NeedsAuthoring
                ? CliResolution.Informational
                : null,
            Actions = action is null ? [] : [action],
        };
    }

    private static CliSubject Subject(
        RouteInitResult result,
        RouteInitFinding finding,
        string target)
    {
        if (finding.Code is RouteInitFindingCode.WorkspaceUnavailable
            or RouteInitFindingCode.WorkspaceUnsafe)
        {
            return new CliSubject(
                CliSubjectKind.Workspace,
                Id: result.WorkspacePath ?? target);
        }

        var kind = finding.Code switch
        {
            RouteInitFindingCode.InvalidInput
                or RouteInitFindingCode.InvalidTarget
                or RouteInitFindingCode.InvalidMetadata
                or RouteInitFindingCode.RouteAmbiguous
                or RouteInitFindingCode.IdentityCollision => CliSubjectKind.Identifier,
            RouteInitFindingCode.LoaderUnsafe
                or RouteInitFindingCode.FrameworkPayloadInvalid
                or RouteInitFindingCode.FrameworkInstallRequired
                or RouteInitFindingCode.FrameworkUpdateRequired
                or RouteInitFindingCode.FrameworkAlignmentBlocked
                or RouteInitFindingCode.FrameworkPayloadUnavailable => CliSubjectKind.Source,
            RouteInitFindingCode.GeneratedRegionUnsafe
                or RouteInitFindingCode.MetadataUnsafe
                or RouteInitFindingCode.TargetUnsafe
                or RouteInitFindingCode.TargetChanged
                or RouteInitFindingCode.TargetChangedDuringApply
                or RouteInitFindingCode.WriteFailed
                or RouteInitFindingCode.VerificationFailed => CliSubjectKind.File,
            _ => CliSubjectKind.Identifier,
        };
        return kind == CliSubjectKind.Identifier
            ? new CliSubject(kind, Id: target)
            : new CliSubject(kind, Path: target);
    }

    private static CliNextAction? Action(
        RouteInitResult result,
        RouteInitFinding finding)
        => finding.Code switch
        {
            RouteInitFindingCode.InvalidTarget
                => new CliNextAction("open-forge route init --help", RouteInitWording.CorrectInputReason()),
            RouteInitFindingCode.InvalidMetadata
                => new CliNextAction(
                    RouteInitWording.CorrectedCommand(result.Target.Requested),
                    RouteInitWording.CorrectInputReason()),
            RouteInitFindingCode.FrameworkInstallRequired
                => new CliNextAction("open-forge install --dry-run", RouteInitWording.InstallReason()),
            RouteInitFindingCode.FrameworkUpdateRequired
                => new CliNextAction("open-forge update", RouteInitWording.UpdateReason()),
            RouteInitFindingCode.WorkspaceLockUnavailable
                or RouteInitFindingCode.TargetChanged
                => new CliNextAction("open-forge route init", RouteInitWording.RetryReason()),
            RouteInitFindingCode.RecoveryArtifactRetained
                => new CliNextAction("open-forge cleanup", RouteInitWording.CleanupReason()),
            RouteInitFindingCode.Interrupted
                => new CliNextAction("open-forge route init", RouteInitWording.RerunReason()),
            _ => null,
        };

    private static CliNextAction? Next(RouteInitResult result)
    {
        if (result.Status == CliSemanticStatus.Complete)
        {
            return null;
        }

        var first = result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Findings.FirstOrDefault();
        if (first is not null && Action(result, first) is { } action)
        {
            return action;
        }

        return result.Status switch
        {
            CliSemanticStatus.Attention
                => new CliNextAction("open-forge cleanup", RouteInitWording.CleanupReason()),
            CliSemanticStatus.Incomplete
                => new CliNextAction("open-forge doctor", RouteInitWording.IncompleteReason()),
            CliSemanticStatus.Invalid
                => new CliNextAction("open-forge route init --help", RouteInitWording.CorrectInputReason()),
            CliSemanticStatus.Blocked
                => new CliNextAction("open-forge doctor", RouteInitWording.DoctorReason()),
            CliSemanticStatus.Failed
                => new CliNextAction("open-forge route init --detail debug", RouteInitWording.DebugReason()),
            CliSemanticStatus.Interrupted
                => new CliNextAction("open-forge route init", RouteInitWording.RerunReason()),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status,
                "The Route Init status is not defined."),
        };
    }

    private static CliEffect Effect(
        RouteInitEffect effect,
        CliDetail detail)
        => new()
        {
            Path = effect.Path,
            Kind = effect.Kind switch
            {
                RouteInitEffectKind.Directory => CliEffectKind.Directory,
                RouteInitEffectKind.Entrypoint => CliEffectKind.File,
                RouteInitEffectKind.GeneratedRegion => CliEffectKind.Section,
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Kind,
                    "The Route Init effect kind is not defined."),
            },
            Action = effect.Action == RouteInitEffectAction.Create
                ? CliEffectAction.Created
                : CliEffectAction.Rewritten,
            Outcome = effect.Outcome switch
            {
                RouteInitEffectOutcome.Planned => CliEffectOutcome.Planned,
                RouteInitEffectOutcome.Verified => CliEffectOutcome.Done,
                RouteInitEffectOutcome.NotStarted => CliEffectOutcome.NotStarted,
                RouteInitEffectOutcome.CompletionUnknown => CliEffectOutcome.Unknown,
                RouteInitEffectOutcome.VerificationFailed => CliEffectOutcome.Failed,
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Outcome,
                    "The Route Init effect outcome is not defined."),
            },
            Before = detail >= CliDetail.Full ? effect.Change?.Before : null,
            After = detail >= CliDetail.Full ? effect.Change?.Expected : null,
        };

    private static CliRecovery Recovery(RouteInitResult result)
        => new(
            result.Recovery.ResidualPath,
            result.Recovery.State switch
            {
                RouteInitRecoveryState.NotRequired or RouteInitRecoveryState.NotCreated
                    => CliRecoveryDisposition.NotRequired,
                RouteInitRecoveryState.Removed => CliRecoveryDisposition.Removed,
                RouteInitRecoveryState.Retained => CliRecoveryDisposition.Retained,
                RouteInitRecoveryState.Unknown => CliRecoveryDisposition.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(result), result.Recovery.State,
                    "The Route Init recovery state is not defined."),
            });

    private static IReadOnlyList<string> Diagnostics(RouteInitResult result)
        =>
        [
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={RouteInitWireVocabulary.Name(result.Mode)}",
            $"target={result.Target.Id ?? result.Target.Requested}",
            $"scaffold={RouteInitWireVocabulary.Name(result.Scaffold)}",
            $"completeness={RouteInitWireVocabulary.Name(result.Plan.Completeness)}",
            $"safety={RouteInitWireVocabulary.Name(result.Plan.Safety)}",
            $"effects={result.Effects.Length.ToString(CultureInfo.InvariantCulture)}",
            $"recovery={RouteInitWireVocabulary.Name(result.Recovery.State)}",
            $"verification={RouteInitWireVocabulary.Name(result.Verification)}",
            $"findings={result.Findings.Length.ToString(CultureInfo.InvariantCulture)}",
            .. result.Findings.Select(finding => finding.Cause),
        ];

    private static RouteInitEntrypoint? TargetEntrypoint(RouteInitResult result)
        => result.Entrypoints.FirstOrDefault(entrypoint =>
            string.Equals(entrypoint.Id, result.Target.Id, StringComparison.Ordinal))
            ?? result.Entrypoints.LastOrDefault(entrypoint => entrypoint.Metadata is not null);

    private static string? EntrypointContent(
        RouteInitResult result,
        RouteInitEntrypoint entrypoint)
        => result.Effects.FirstOrDefault(effect =>
            effect.Kind == RouteInitEffectKind.Entrypoint
            && string.Equals(effect.Path, entrypoint.Path, StringComparison.Ordinal))
            ?.Change?.Expected;

    private static string Verification(RouteInitEffect effect)
        => RouteInitWireVocabulary.Name(effect.Outcome);

    private static bool HasRecordedFrameworkOwnership(RouteInitResult result)
        => result.Scaffold == RouteInitScaffold.Framework
            && result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention;

    private static string Hash(string value)
        => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static bool NeedsAuthoring(RouteInitEntrypoint entrypoint)
        => entrypoint.Metadata?.Tags.Contains("NeedsAuthoring", StringComparer.Ordinal) == true;

    private static long CountProgressedEntrypoints(RouteInitResult result)
        => result.Entrypoints.LongCount(entrypoint =>
            entrypoint.Outcome is RouteInitEntrypointOutcome.Created
                or RouteInitEntrypointOutcome.Planned);

    private static long CountPresentEntrypoints(RouteInitResult result)
        => result.Entrypoints.LongCount(entrypoint =>
            entrypoint.Outcome == RouteInitEntrypointOutcome.Unchanged);

    private static long CountProgressedSections(RouteInitResult result)
        => result.Effects.LongCount(effect =>
            effect.Kind == RouteInitEffectKind.GeneratedRegion
            && (result.Mode == RouteInitMode.DryRun
                ? effect.Outcome == RouteInitEffectOutcome.Planned
                : effect.Outcome == RouteInitEffectOutcome.Verified));

    private static int ProgressedCount(RouteInitResult result)
        => result.Effects.Count(effect =>
            result.Mode == RouteInitMode.DryRun
                ? effect.Outcome == RouteInitEffectOutcome.Planned
                : effect.Outcome == RouteInitEffectOutcome.Verified);
}
