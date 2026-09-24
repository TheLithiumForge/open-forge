using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Effects;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.Sync.Models;
using OpenForge.Cli.Core.Presentation.Library.Sync.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Library.Sync.Shared.Selection;

internal static class LibrarySyncReportSelector
{
    internal static CliReport<LibrarySyncData> Select(
        LibrarySyncResult result,
        CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        var payload = result.Result;
        var id = payload.Identity.LibraryId
            ?? payload.Findings.FirstOrDefault(finding => finding.LibraryId is not null)?.LibraryId
            ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSuppliedId();
        var effects = ProjectEffects(result, selection.Detail);
        var unchanged = payload.Projection.Mappings
            .Where(mapping => mapping.Relation == LibraryComparisonRelation.Current)
            .Select(mapping => mapping.DestinationPath)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var counts = Counts(effects, unchanged.Length);
        var data = new LibrarySyncData
        {
            Mode = LibrarySyncWording.Wire(payload.Identity.Mode),
            Id = payload.Identity.LibraryId,
            SourceFolder = payload.Identity.SourceRoot,
            DestinationFolder = payload.Identity.DestinationRoot,
            Permissions = Permissions(payload.Permissions),
            Effects = effects,
            Unchanged = selection.Detail >= CliDetail.Standard ? unchanged : null,
            Inventory = selection.Detail >= CliDetail.Full ? Inventory(payload.Source) : null,
            ExpectedStates = selection.Detail >= CliDetail.Full ? ExpectedStates(payload.Plan) : null,
            Verification = selection.Detail >= CliDetail.Full ? Verification(payload.Application) : null,
            Recovery = selection.Detail >= CliDetail.Full ? Recovery(payload.Application) : null,
            TextRows = TextRows(result, effects, selection.Detail),
            TextSummaryLines = TextSummaryLines(result, counts, selection.Detail),
            TextDetailLines = TextDetailLines(payload, selection.Detail),
        };

        return new CliReport<LibrarySyncData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result, data, counts, id),
            HeadlineFindingCode = HeadlineFindingCode(result),
            Workspace = result.WorkspacePath is { } path
                ? new CliWorkspaceEcho(path, result.WorkspaceExplicit)
                : null,
            Findings = result.Result.Findings.Select(finding => Finding(result, finding, id)).ToArray(),
            Effects = ReportEffects(result),
            Counts = counts,
            Limitations = [],
            Data = data,
            Recovery = EnvelopeRecovery(payload.Application),
            Next = Next(result, id),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? Diagnostics(result, data, counts)
                : [],
        };
    }

    private static IReadOnlyList<LibrarySyncDataEffect> ProjectEffects(
        LibrarySyncResult result,
        CliDetail detail)
    {
        var payload = result.Result;
        var includeTargets = detail >= CliDetail.Standard;
        var effects = new List<LibrarySyncDataEffect>();

        if (payload.Plan.SettingsChange is { } settingsChange)
        {
            effects.Add(new LibrarySyncDataEffect
            {
                Path = settingsChange.Path,
                Action = settingsChange.Action,
                Outcome = SettingsOutcome(payload.Permissions.Outcome),
                IsSettings = true,
            });
        }

        foreach (var directory in payload.Plan.Directories.OrderBy(effect => effect.Path, StringComparer.Ordinal))
        {
            effects.Add(new LibrarySyncDataEffect
            {
                Path = directory.Path,
                Action = "add",
                Outcome = Outcome(result, directory.Path),
            });
        }

        foreach (var link in payload.Plan.Links.OrderBy(effect => effect.Path, StringComparer.Ordinal))
        {
            effects.Add(new LibrarySyncDataEffect
            {
                Path = link.Path,
                Action = link.Kind == LibraryLinkEffectKind.Create ? "add" : "remove",
                Outcome = Outcome(result, link.Path),
                Target = includeTargets ? link.RawRelativeTarget : null,
                IsLink = true,
                Suffix = link.Kind == LibraryLinkEffectKind.Delete ? global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.LabelItsSourceFileIsGone() : null,
            });
        }

        foreach (var region in payload.Plan.GeneratedRegions.OrderBy(effect => effect.Path, StringComparer.Ordinal))
        {
            effects.Add(new LibrarySyncDataEffect
            {
                Path = region.Path,
                Action = "update",
                Outcome = Outcome(result, region.Path),
                IsSection = true,
            });
        }

        if (payload.Plan.RecordEffect != LibraryRecordEffect.None)
        {
            effects.Add(new LibrarySyncDataEffect
            {
                Path = payload.Record.Path,
                Action = "update",
                Outcome = Outcome(result, payload.Record.Path),
                IsRecord = true,
            });
        }

        return effects;
    }

    private static IReadOnlyList<CliEffect> ReportEffects(LibrarySyncResult result)
        => ProjectEffects(result, CliDetail.Full)
            .Select(effect => new CliEffect
            {
                Path = effect.Path,
                Kind = effect.IsRecord
                    ? CliEffectKind.Record
                    : effect.IsSection
                        ? CliEffectKind.Section
                        : effect.IsLink
                            ? CliEffectKind.Link
                            : effect.IsSettings
                                ? CliEffectKind.Setting
                                : CliEffectKind.Directory,
                Action = effect.Action switch
                {
                    "add" => CliEffectAction.Created,
                    "remove" => CliEffectAction.Deleted,
                    "create" => CliEffectAction.Created,
                    "replace" => CliEffectAction.Rewritten,
                    "update" when effect.IsRecord => RecordAction(result.Result.Plan.RecordEffect),
                    "update" => CliEffectAction.Rewritten,
                    _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Action, "The Library Sync effect action is not defined."),
                },
                Outcome = effect.Outcome switch
                {
                    "planned" => CliEffectOutcome.Planned,
                    "done" => CliEffectOutcome.Done,
                    "not-started" => CliEffectOutcome.NotStarted,
                    "unknown" => CliEffectOutcome.Unknown,
                    "failed" => CliEffectOutcome.Failed,
                    _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Outcome, "The Library Sync effect outcome is not defined."),
                },
                Reason = effect.Suffix,
            })
            .ToArray();

    private static CliEffectAction RecordAction(LibraryRecordEffect effect)
        => effect switch
        {
            LibraryRecordEffect.Create => CliEffectAction.Created,
            LibraryRecordEffect.Replace => CliEffectAction.Replaced,
            LibraryRecordEffect.Delete => CliEffectAction.Deleted,
            _ => throw new ArgumentOutOfRangeException(nameof(effect), effect, "The Library Sync record effect is not defined."),
        };

    private static string Outcome(LibrarySyncResult result, string path)
    {
        var payload = result.Result;
        if (string.Equals(path, ".agents/open-forge.json", StringComparison.Ordinal))
        {
            return payload.Identity.Mode == LibraryMode.DryRun
                ? "planned"
                : SettingsOutcome(payload.Permissions.Outcome);
        }
        if (payload.Identity.Mode == LibraryMode.DryRun && payload.Plan.State == LibraryPlanState.Complete)
        {
            return "planned";
        }

        if (payload.Application.State == LibraryApplicationState.Applied
            || payload.Application.State == LibraryApplicationState.NoOp)
        {
            return "done";
        }

        if (payload.Application.State is LibraryApplicationState.Failed or LibraryApplicationState.Interrupted)
        {
            var residual = payload.Application.Residuals.FirstOrDefault(residual =>
                string.Equals(residual.Path, path, StringComparison.Ordinal));
            return residual?.State switch
            {
                LibraryResidualState.Unknown => "unknown",
                LibraryResidualState.Retained => "done",
                _ => "not-started",
            };
        }

        return "not-started";
    }

    private static string SettingsOutcome(string outcome)
        => outcome switch
        {
            "planned" => "planned",
            "verified" => "done",
            "not-started" or "not-requested" => "not-started",
            "verification-failed" => "failed",
            "completion-unknown" => "unknown",
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The Library settings outcome is not defined."),
        };

    private static LibrarySyncDataPermissions Permissions(LibraryPermissionView permissions)
        => new()
        {
            Required = [.. permissions.Required],
            Missing = [.. permissions.Missing],
            ProposedScopes = permissions.ProposedScopes.Select(scope => new LibrarySyncDataPermissionScope
            {
                Kind = scope.Kind,
                Path = scope.Path,
            }).ToArray(),
            ApprovedScopes = permissions.ApprovedScopes.Select(scope => new LibrarySyncDataPermissionScope
            {
                Kind = scope.Kind,
                Path = scope.Path,
            }).ToArray(),
            Decision = permissions.Decision,
            Action = permissions.Action,
            Outcome = permissions.Outcome,
        };

    private static LibrarySyncDataInventory Inventory(LibraryMutationSource source)
        => new()
        {
            RootState = LibrarySyncWording.Wire(source.RootState),
            InventoryState = LibrarySyncWording.Wire(source.InventoryState),
            EligiblePaths = source.EligiblePaths.Select(path => new LibrarySyncDataInventoryPath
            {
                SourcePath = path.SourcePath,
                DestinationPath = path.DestinationPath,
                SourceId = path.SourceId,
            }).ToArray(),
            ExcludedPaths = source.ExcludedPaths.Select(path => new LibrarySyncDataExcludedPath
            {
                Path = path.Path,
                Reason = LibrarySyncWording.Wire(path.Reason),
            }).ToArray(),
            UnavailablePaths = source.UnavailablePaths.Select(path => new LibrarySyncDataUnavailablePath
            {
                Path = path.Path,
                Cause = path.Cause,
            }).ToArray(),
            LexicalRoot = source.LexicalRoot,
            PhysicalRoot = source.PhysicalRoot,
            LexicallyContained = source.LexicallyContained,
            PhysicallyContained = source.PhysicallyContained,
        };

    private static IReadOnlyList<LibrarySyncDataExpectedState> ExpectedStates(LibraryMutationPlanView plan)
    {
        var states = new List<LibrarySyncDataExpectedState>();
        states.AddRange(plan.Directories.Select(directory => Expected(directory.Path, directory.Expected)));
        states.AddRange(plan.Links.Select(link => Expected(link.Path, link.Expected)));
        states.AddRange(plan.GeneratedRegions.Select(region => Expected(region.Path, region.Expected)));
        if (plan.SettingsChange is { } settingsChange)
        {
            states.Add(Expected(settingsChange.Path, settingsChange.Expected));
        }
        if (plan.RecordExpected is { } record)
        {
            states.Add(Expected(".agents/open-forge.lock.json", record));
        }

        return states.OrderBy(state => state.Path, StringComparer.Ordinal).ToArray();
    }

    private static LibrarySyncDataExpectedState Expected(string path, LibraryExpectedState expected)
        => new()
        {
            Path = path,
            Kind = LibrarySyncWording.Wire(expected.Kind),
            Length = expected.Length,
            Sha256 = expected.Sha256,
            Target = expected.RawRelativeTarget,
        };

    private static LibrarySyncDataVerification Verification(LibraryMutationApplication application)
        => new()
        {
            State = LibrarySyncWording.Wire(application.Verification),
            RecordPublication = LibrarySyncWording.Wire(application.RecordPublication.State),
            PublishedLast = application.RecordPublication.PublishedLast,
        };

    private static LibrarySyncDataRecovery Recovery(LibraryMutationApplication application)
        => new()
        {
            State = LibrarySyncWording.Wire(application.Recovery.State),
            Path = application.Recovery.Path,
            Residuals = application.Residuals.OrderBy(residual => residual.Path, StringComparer.Ordinal)
                .ThenBy(residual => residual.Kind)
                .Select(residual => new LibrarySyncDataResidual
                {
                    Path = residual.Path,
                    Kind = LibrarySyncWording.Wire(residual.Kind),
                    State = LibrarySyncWording.Wire(residual.State),
                }).ToArray(),
        };

    private static CliCount[] Counts(
        IReadOnlyList<LibrarySyncDataEffect> effects,
        int unchanged)
        =>
        [
            new CliCount("linksAdded", global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.LabelLinksAdded(), effects.Count(effect => effect.IsLink && effect.Action == "add" && IsCounted(effect.Outcome))),
            new CliCount("linksRemoved", global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelLinksRemoved(), effects.Count(effect => effect.IsLink && effect.Action == "remove" && IsCounted(effect.Outcome))),
            new CliCount("linksUnchanged", global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.LabelLinksUnchanged(), unchanged),
            new CliCount("sectionsUpdated", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelSectionsUpdated(), effects.Count(effect => effect.IsSection && IsCounted(effect.Outcome))),
        ];

    private static bool IsCounted(string outcome)
        => outcome is "planned" or "done";

    private static IReadOnlyList<LibrarySyncDataTextRow> TextRows(
        LibrarySyncResult result,
        IReadOnlyList<LibrarySyncDataEffect> effects,
        CliDetail detail)
    {
        if (result.Status == CliSemanticStatus.Invalid
            || result.Status == CliSemanticStatus.Incomplete && !HasSafeEffects(effects)
            || result.Status == CliSemanticStatus.Blocked
                && !result.Result.Findings.Any(finding => finding.Code == LibrarySyncFindingCode.LockUnavailable))
        {
            return BlockedRows(result, detail);
        }

        var preview = result.Result.Identity.Mode == LibraryMode.DryRun;
        return effects
            .Where(effect => effect.IsLink || effect.IsSection)
            .Where(effect => detail >= CliDetail.Standard || !effect.IsRecord)
            .Select(effect => new LibrarySyncDataTextRow
            {
                Path = effect.Path,
                Label = effect.IsSection
                    ? LibrarySyncWording.SectionLabel(effect.Outcome, preview)
                    : LibrarySyncWording.EffectLabel(
                        effect.Action == "add" ? LibraryLinkEffectKind.Create : LibraryLinkEffectKind.Delete,
                        effect.Outcome,
                        preview),
                Suffix = effect.Suffix,
                Target = detail >= CliDetail.Standard && effect.IsLink ? effect.Target : null,
                IsSection = effect.IsSection,
            }).ToArray();
    }

    private static bool HasSafeEffects(IReadOnlyList<LibrarySyncDataEffect> effects)
        => effects.Any(effect => IsCounted(effect.Outcome));

    private static IReadOnlyList<LibrarySyncDataTextRow> BlockedRows(
        LibrarySyncResult result,
        CliDetail detail)
    {
        if (detail != CliDetail.Minimal || result.Status != CliSemanticStatus.Blocked)
        {
            return [];
        }

        var finding = result.Result.Findings.FirstOrDefault(finding => finding.Status == result.Status);
        if (finding is null || finding.Path is null)
        {
            return [];
        }

        var mapping = result.Result.Projection.Mappings.FirstOrDefault(mapping =>
            string.Equals(mapping.DestinationPath, finding.Path, StringComparison.Ordinal));
        if (finding.Code is LibrarySyncFindingCode.DestinationCollision or LibrarySyncFindingCode.MappingBlocked
            && mapping is not null)
        {
            var occupant = mapping.ObservedRelativeLink is null ? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelAnOrdinaryFile() : global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelADifferentLink();
            return
            [
                new LibrarySyncDataTextRow
                {
                    Path = finding.Path,
                    Label = LibrarySyncWording.ChangedDestinationResolution(occupant),
                    IsStandalone = true,
                },
            ];
        }

        return [];
    }

    private static IReadOnlyList<string> TextSummaryLines(
        LibrarySyncResult result,
        IReadOnlyList<CliCount> counts,
        CliDetail detail)
    {
        var lines = new List<string>();
        if (detail >= CliDetail.Standard)
        {
            if (result.Result.Plan.SettingsChange is { } settingsChange)
            {
                lines.Add(LibrarySyncWording.SettingsChange(
                    settingsChange.Action,
                    result.Result.Permissions.Outcome,
                    result.Result.Identity.Mode == LibraryMode.DryRun));
            }
            var unchanged = counts.First(count => count.Name == "linksUnchanged").Value;
            if (unchanged is > 0)
            {
                lines.Add(LibrarySyncWording.Unchanged((int)unchanged.Value));
            }

            var record = result.Result.Plan.RecordEffect != LibraryRecordEffect.None;
            if (record)
            {
                var effect = result.Result.Application;
                var action = result.Result.Identity.Mode == LibraryMode.DryRun
                    ? global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.LabelWouldBeUpdated()
                    : effect.State == LibraryApplicationState.Applied
                    ? "updated"
                    : effect.State == LibraryApplicationState.NotStarted
                        ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted()
                        : effect.State == LibraryApplicationState.Failed
                            ? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown()
                            : "updated";
                lines.Add(LibrarySyncWording.Lock(action));
            }
        }

        return lines;
    }

    private static IReadOnlyList<string> TextDetailLines(
        LibrarySyncPayload payload,
        CliDetail detail)
    {
        if (detail < CliDetail.Full)
        {
            return [];
        }

        var lines = new List<string>
        {
            LibrarySyncWording.Inventory(
                payload.Source.EligiblePaths.Length,
                payload.Source.ExcludedPaths.Length,
                payload.Source.UnavailablePaths.Length),
        };
        foreach (var state in ExpectedStates(payload.Plan))
        {
            lines.Add(LibrarySyncWording.ExpectedState(state.Path, state.Kind, state.Target));
        }

        lines.Add(LibrarySyncWording.Verification(
            LibrarySyncWording.Wire(payload.Application.Verification),
            LibrarySyncWording.Wire(payload.Application.RecordPublication.State)));
        lines.Add(LibrarySyncWording.Recovery(
            LibrarySyncWording.Wire(payload.Application.Recovery.State),
            payload.Application.Recovery.Path));
        return lines;
    }

    private static CliHeadline Headline(
        LibrarySyncResult result,
        LibrarySyncData data,
        IReadOnlyList<CliCount> counts,
        string id)
    {
        var finding = result.Result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Result.Findings.FirstOrDefault();
        var added = Count(counts, "linksAdded");
        var removed = Count(counts, "linksRemoved");
        var unchanged = Count(counts, "linksUnchanged");
        var hasChanges = data.Effects.Any(effect => effect.IsLink || effect.IsSection || effect.IsRecord || effect.IsSettings);
        return result.Status switch
        {
            CliSemanticStatus.Complete when !hasChanges
                && result.Result.Findings.Any(finding => finding.Code == LibrarySyncFindingCode.PathExcluded)
                => new(LibrarySyncWording.ExcludedDestinationsUntouched(id), CliHeadlineKind.NothingToDo),
            CliSemanticStatus.Complete when result.Result.Findings.Any(finding => finding.Code == LibrarySyncFindingCode.OwnershipObservation)
                => new(LibrarySyncWording.NoOwnership(id), CliHeadlineKind.NothingToDo),
            CliSemanticStatus.Complete when !hasChanges
                => new(LibrarySyncWording.UpToDate(id), CliHeadlineKind.NothingToDo),
            CliSemanticStatus.Complete when result.Result.Identity.Mode == LibraryMode.DryRun
                => new(LibrarySyncWording.WouldSynchronize(id, added, removed), CliHeadlineKind.Preview),
            CliSemanticStatus.Complete
                => new(LibrarySyncWording.Synchronized(id, added, removed, unchanged), CliHeadlineKind.Done),
            CliSemanticStatus.Attention
                => new(LibrarySyncWording.Synchronized(id, added, removed, unchanged), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete when HasSafeEffects(data.Effects)
                => new(LibrarySyncWording.IncompleteWithEffects(id, IncompleteReason(result, finding, id)), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Incomplete
                => new(LibrarySyncWording.Incomplete(id, IncompleteReason(result, finding, id)), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid
                => new(LibrarySyncWording.CannotSynchronize(id, Message(result, finding, id)), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked
                => new(LibrarySyncWording.Blocked(id, BlockedReason(result, finding, id)), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed when CountCompleted(data.Effects) == 0
                => new(LibrarySyncWording.Failed(0, data.Effects.Count), CliHeadlineKind.Failed),
            CliSemanticStatus.Failed
                => new(LibrarySyncWording.Failed(CountCompleted(data.Effects), data.Effects.Count), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted when CountCompleted(data.Effects) == 0
                => new(LibrarySyncWording.Cancelled(), CliHeadlineKind.Cancelled),
            CliSemanticStatus.Interrupted
                => new(LibrarySyncWording.CancelledAfter(CountCompleted(data.Effects), data.Effects.Count), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Library Sync status is not defined."),
        };
    }

    private static string? HeadlineFindingCode(LibrarySyncResult result)
    {
        if (result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention)
        {
            return null;
        }

        var findings = result.Result.Findings.Where(finding => finding.Status == result.Status).ToArray();
        return findings.Length == 1 ? LibrarySyncWording.MachineCode(findings[0].Code) : null;
    }

    private static CliFinding Finding(
        LibrarySyncResult result,
        LibrarySyncFinding finding,
        string id)
    {
        var kind = finding.Code is LibrarySyncFindingCode.InvalidInput
            or LibrarySyncFindingCode.InvalidId
            or LibrarySyncFindingCode.UnknownId
            or LibrarySyncFindingCode.LibraryRemoved
            ? CliSubjectKind.Identifier
            : finding.Code is LibrarySyncFindingCode.SourceRootInvalid
                or LibrarySyncFindingCode.SourceRootUnavailable
                or LibrarySyncFindingCode.SourceRootBlocked
                or LibrarySyncFindingCode.InventoryIncomplete
                ? CliSubjectKind.Directory
                : CliSubjectKind.File;
        var subject = kind == CliSubjectKind.Identifier
            ? finding.LibraryId ?? id
            : finding.Path ?? finding.LibraryId ?? id;
        var action = Action(result, finding, id);
        return new CliFinding
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = LibrarySyncWording.MachineCode(finding.Code),
            Title = LibrarySyncWording.FindingTitle(finding.Code),
            Message = Message(result, finding, id),
            Subject = kind == CliSubjectKind.Identifier
                ? new CliSubject(kind, null, subject)
                : new CliSubject(kind, subject),
            Category = LibrarySyncWording.Family(finding.Code),
            Resolution = action is null ? null : CliResolution.TargetedOperation,
            Actions = action is null ? [] : [action],
        };
    }

    private static string Message(LibrarySyncResult result, LibrarySyncFinding? finding, string id)
    {
        if (finding is null)
        {
            return global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheResultDidNotContainAFinding();
        }

        var path = finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSourceFolder();
        return finding.Code switch
        {
            LibrarySyncFindingCode.InvalidInput => TrimSentence(finding.Cause),
            LibrarySyncFindingCode.InvalidId => LibrarySyncWording.InvalidId(finding.LibraryId ?? finding.Path),
            LibrarySyncFindingCode.UnknownId => TrimSentence(finding.Cause),
            LibrarySyncFindingCode.OwnershipObservation => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.MessageNoOwnershipRecordExists(),
            LibrarySyncFindingCode.RecordInvalid => LibrarySyncWording.RecordInvalid(finding.Cause),
            LibrarySyncFindingCode.SourceRootInvalid => LibrarySyncWording.SourceRootInvalid(path),
            LibrarySyncFindingCode.SourceRootUnavailable => LibrarySyncWording.SourceRootUnavailable(path),
            LibrarySyncFindingCode.SourceRootBlocked => LibrarySyncWording.SourceRootBlocked(path),
            LibrarySyncFindingCode.InventoryIncomplete => LibrarySyncWording.InventoryIncomplete(path),
            LibrarySyncFindingCode.MappingUnavailable => LibrarySyncWording.MappingUnavailable(finding.Cause),
            LibrarySyncFindingCode.MappingBlocked => LibrarySyncWording.MappingBlocked(path, Occupant(result, path, finding.Cause)),
            LibrarySyncFindingCode.DestinationCollision => DestinationMessage(result, path, finding.Cause),
            LibrarySyncFindingCode.RetiredLinkMissing => LibrarySyncWording.RetiredLinkMissing(path),
            LibrarySyncFindingCode.RegisteredLinkRestored => LibrarySyncWording.RegisteredLinkRestored(path),
            LibrarySyncFindingCode.LinkCapabilityUnavailable => LibrarySyncWording.LinkCapabilityUnavailable(),
            LibrarySyncFindingCode.ConsumerBlocked => LibrarySyncWording.ConsumerBlocked(path),
            LibrarySyncFindingCode.LockUnavailable => LibrarySyncWording.LockUnavailable(),
            LibrarySyncFindingCode.Interrupted when finding.Cause.Contains("Nothing was changed", StringComparison.Ordinal)
                => LibrarySyncWording.Cancelled(),
            LibrarySyncFindingCode.RecoveryRetained when finding.Path is { } recovery
                => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatTheRecoveryBundleWasRetainedAt($"{recovery}"),
            _ => TrimSentence(finding.Cause),
        };
    }

    private static string DestinationMessage(LibrarySyncResult result, string path, string cause)
    {
        var mapping = result.Result.Projection.Mappings.FirstOrDefault(mapping =>
            string.Equals(mapping.DestinationPath, path, StringComparison.Ordinal));
        return mapping?.State == LibraryLinkViewState.Changed
            ? LibrarySyncWording.ChangedDestination(path)
            : LibrarySyncWording.DestinationCollision(path);
    }

    private static string IncompleteReason(LibrarySyncResult result, LibrarySyncFinding? finding, string id)
    {
        if (finding is null)
        {
            return global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.LabelRequiredLibraryFactsAreUnavailable();
        }

        var path = finding.Path ?? result.Result.Identity.SourceRoot ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSourceFolder();
        return finding.Code switch
        {
            LibrarySyncFindingCode.SourceRootInvalid => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatTheSourceFolderIsNotAFolderInsideTheWorkspace($"{path}"),
            LibrarySyncFindingCode.SourceRootUnavailable => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatTheSourceFolderCannotBeRead($"{path}"),
            LibrarySyncFindingCode.SourceRootBlocked => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatTheSourceFolderResolvesToAnUnsafeLocation($"{path}"),
            LibrarySyncFindingCode.InventoryIncomplete => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatSomeFilesUnderCouldNotBeListed($"{path}"),
            LibrarySyncFindingCode.MappingUnavailable => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.PlanFailureReason($"{TrimSentence(finding.Cause)}"),
            LibrarySyncFindingCode.MappingBlocked when finding.Status == CliSemanticStatus.Incomplete
                => Message(result, finding, id),
            _ => TrimSentence(finding.Cause),
        };
    }

    private static string BlockedReason(LibrarySyncResult result, LibrarySyncFinding? finding, string id)
    {
        if (finding is null)
        {
            return global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.LabelTheSynchronizationIsBlocked();
        }

        var path = finding.Path ?? id;
        return finding.Code switch
        {
            LibrarySyncFindingCode.LockUnavailable => LibrarySyncWording.LockUnavailableReason(),
            LibrarySyncFindingCode.MappingBlocked => LibrarySyncWording.MappingBlocked(path, Occupant(result, path, finding.Cause)),
            LibrarySyncFindingCode.DestinationCollision => result.Result.Projection.Mappings.Any(mapping =>
                mapping.DestinationPath == path && mapping.State == LibraryLinkViewState.Changed)
                    ? LibrarySyncWording.ChangedDestination(path)
                    : LibrarySyncWording.DestinationCollision(path),
            LibrarySyncFindingCode.RetiredLinkMissing => LibrarySyncWording.RetiredLinkMissing(path),
            LibrarySyncFindingCode.ConsumerBlocked => LibrarySyncWording.ConsumerBlocked(path),
            _ => TrimSentence(finding.Cause),
        };
    }

    private static string Occupant(LibrarySyncResult result, string path, string cause)
    {
        var mapping = result.Result.Projection.Mappings.FirstOrDefault(mapping =>
            string.Equals(mapping.DestinationPath, path, StringComparison.Ordinal));
        return mapping?.ObservedRelativeLink is null ? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelAnOrdinaryFile() : global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelADifferentLink();
    }

    private static CliNextAction? Action(LibrarySyncResult result, LibrarySyncFinding finding, string id)
        => finding.Code switch
        {
            LibrarySyncFindingCode.InvalidId or LibrarySyncFindingCode.UnknownId
                => new CliNextAction("open-forge library list", LibrarySyncWording.NextListReason()),
            LibrarySyncFindingCode.RecordInvalid
                or LibrarySyncFindingCode.RecordUnavailable
                or LibrarySyncFindingCode.MappingUnavailable
                => new CliNextAction("open-forge doctor", LibrarySyncWording.NextDoctorReason()),
            LibrarySyncFindingCode.MappingBlocked
                or LibrarySyncFindingCode.DestinationCollision
                or LibrarySyncFindingCode.RetiredLinkMissing when finding.LibraryId is not null
                => new CliNextAction($"open-forge library inspect {finding.LibraryId ?? id}", LibrarySyncWording.NextInspectReason()),
            _ => null,
        };

    private static CliNextAction? Next(LibrarySyncResult result, string id)
    {
        if (result.Next is not null)
        {
            return result.Next;
        }

        var finding = result.Result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Result.Findings.FirstOrDefault();
        if (finding?.Code == LibrarySyncFindingCode.LibraryRemoved)
        {
            return new CliNextAction(
                LibrarySyncWording.RemoveExcludedLibraryNext(finding.LibraryId ?? id),
                LibrarySyncWording.WorkspaceSettingsNextReason())
            {
                Kind = CliNextActionKind.Sentence,
            };
        }
        return finding is null ? null : Action(result, finding, id);
    }

    private static CliRecovery? EnvelopeRecovery(LibraryMutationApplication application)
        => application.Recovery.Path is not null
            ? new CliRecovery(
                application.Recovery.Path,
                application.Recovery.State switch
                {
                    LibraryRecoveryState.Removed => CliRecoveryDisposition.Removed,
                    LibraryRecoveryState.Prepared or LibraryRecoveryState.Retained => CliRecoveryDisposition.Retained,
                    LibraryRecoveryState.Unknown => CliRecoveryDisposition.Unknown,
                    _ => CliRecoveryDisposition.NotRequired,
                })
            : null;

    private static IReadOnlyList<string> Diagnostics(
        LibrarySyncResult result,
        LibrarySyncData data,
        IReadOnlyList<CliCount> counts)
        =>
        [
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={data.Mode}",
            $"library={data.Id ?? "unavailable"}",
            $"effects={data.Effects.Count}",
            $"links-added={Count(counts, "linksAdded")}",
            $"links-removed={Count(counts, "linksRemoved")}",
            $"links-unchanged={Count(counts, "linksUnchanged")}",
            $"findings={result.Result.Findings.Length}",
        ];

    private static int Count(IReadOnlyList<CliCount> counts, string name)
        => (int)(counts.First(count => count.Name == name).Value ?? 0);

    private static int CountCompleted(IReadOnlyList<LibrarySyncDataEffect> effects)
        => effects.Count(effect => effect.Outcome == "done");

    private static string TrimSentence(string value)
        => CliFindingWording.PlainCause(value);
}
