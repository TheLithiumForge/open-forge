using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Create.Models;
using OpenForge.Cli.Core.Presentation.Route.Create.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Route.Create.Shared.Selection;

internal static class RouteCreateReportSelector
{
    internal static CliReport<RouteCreateData> Select(
        RouteCreateResult result,
        CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        var data = Data(result, selection.Detail);
        return new CliReport<RouteCreateData>
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
                new CliCount("filesCreated", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesCreated(), CountProgressed(result, RouteCreateEffectKind.RoutedFile, RouteCreateEffectKind.Entrypoint)),
                new CliCount("sectionsUpdated", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelSectionsUpdated(), CountProgressed(result, RouteCreateEffectKind.GeneratedRegion)),
            ],
            Data = data,
            Recovery = Recovery(result),
            Next = Next(result),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? Diagnostics(result)
                : [],
        };
    }

    private static RouteCreateData Data(RouteCreateResult result, CliDetail detail)
    {
        var includeStandard = detail >= CliDetail.Standard;
        var includeFull = detail >= CliDetail.Full;
        var sections = result.Sections.IsDefault
            ? Array.Empty<RouteCreateDataSection>()
            : result.Sections.Select(section => new RouteCreateDataSection
            {
                Path = section.Path,
                Before = section.Before,
                After = section.After,
                Verification = Verification(result, section.Path),
            }).ToArray();
        return new RouteCreateData
        {
            Mode = RouteCreateWireVocabulary.Name(result.Mode),
            Target = new RouteCreateDataTarget
            {
                Id = result.Target.Id,
                Path = result.Target.Path,
            },
            ListedIn = result.Parent?.Path,
            Template = result.Template is { } template
                ? new RouteCreateDataTemplate
                {
                    Id = template.Id,
                    Path = template.Path,
                }
                : null,
            Metadata = includeStandard
                ? new RouteCreateDataMetadata
                {
                    Description = result.Metadata.Description,
                    Responsibility = result.Metadata.Responsibility,
                    Tags = result.Metadata.Tags,
                }
                : null,
            Content = includeFull ? result.Content : null,
            Sections = includeFull ? sections : null,
            TextMetadata = TextMetadata(result, includeStandard),
            TextRows = TextRows(result),
            TextNextLines = TextNextLines(result),
            TextSections = includeFull ? sections : [],
        };
    }

    private static IReadOnlyList<string> TextMetadata(
        RouteCreateResult result,
        bool includeStandard)
    {
        if (!includeStandard)
        {
            return [];
        }

        var lines = new List<string>();
        if (result.Metadata.Description is { Length: > 0 } description)
        {
            lines.Add(RouteCreateWording.MetadataDescription(description));
        }

        if (!result.Metadata.Tags.IsDefaultOrEmpty)
        {
            lines.Add(RouteCreateWording.MetadataTags(result.Metadata.Tags));
        }

        if (result.Metadata.Responsibility is { } responsibility)
        {
            lines.Add(RouteCreateWording.MetadataResponsibility(responsibility));
        }

        var targetEffect = result.Effects.FirstOrDefault(effect =>
            effect.Kind == RouteCreateEffectKind.RoutedFile);
        if (result.Template is { } template && targetEffect is not null)
        {
            lines.Add(RouteCreateWording.TemplatePath(template.Path));
        }

        return lines;
    }

    private static IReadOnlyList<string> TextRows(RouteCreateResult result)
    {
        var rows = new List<string>();
        foreach (var effect in result.Effects.Where(effect =>
                     effect.Kind is RouteCreateEffectKind.Directory
                         or RouteCreateEffectKind.Entrypoint))
        {
            rows.Add(EffectRow(effect, target: true));
        }

        var targetEffect = result.Effects.FirstOrDefault(effect =>
            effect.Kind == RouteCreateEffectKind.RoutedFile);
        var parentEffect = result.Effects.FirstOrDefault(effect =>
            effect.Kind == RouteCreateEffectKind.GeneratedRegion);
        var targetIsHeadline = targetEffect is not null
            && result.Status is (CliSemanticStatus.Complete or CliSemanticStatus.Attention)
            && IsProgressed(targetEffect, result.Mode);

        if (targetEffect is not null && !targetIsHeadline)
        {
            rows.Add(EffectRow(targetEffect, target: true));
        }

        if (result.Template is { } template && targetEffect is not null
            && IsProgressed(targetEffect, result.Mode))
        {
            rows.Add(RouteCreateWording.BodyCopied(template.Id));
        }

        if (parentEffect is not null)
        {
            rows.Add(EffectRow(parentEffect, target: false));
        }

        return rows;
    }

    private static IReadOnlyList<string> TextNextLines(RouteCreateResult result)
    {
        if (result.Findings.Length != 1
            || result.Findings[0].Code != RouteCreateFindingCode.OptionalMetadata
            || Next(result) is not { } next)
        {
            return [];
        }

        return
        [
            global::OpenForge.Cli.OutputText.Route.Create.RouteCreateWording.OptionalMetadataNext(next.Command),
        ];
    }

    private static string EffectRow(
        RouteCreateEffect effect,
        bool target)
        => effect.Outcome switch
        {
            RouteCreateEffectOutcome.Planned when target
                => RouteCreateWording.WouldCreateEffect(effect.Path),
            RouteCreateEffectOutcome.Verified when target
                => RouteCreateWording.CreatedEffect(effect.Path),
            RouteCreateEffectOutcome.Planned
                => RouteCreateWording.WouldList(effect.Path),
            RouteCreateEffectOutcome.Verified
                => RouteCreateWording.Listed(effect.Path),
            RouteCreateEffectOutcome.NotStarted
                => RouteCreateWording.NotStarted(effect.Path),
            RouteCreateEffectOutcome.CompletionUnknown
                => RouteCreateWording.Unknown(effect.Path),
            RouteCreateEffectOutcome.VerificationFailed
                => RouteCreateWording.FailedEffect(effect.Path),
            _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Outcome, "The Route Create effect outcome is not defined."),
        };

    private static CliHeadline Headline(RouteCreateResult result)
    {
        var path = result.Target.Path ?? result.Target.Requested;
        var id = result.Target.Id ?? result.Target.Requested;
        var targetEffect = result.Effects.FirstOrDefault(effect =>
            effect.Kind == RouteCreateEffectKind.RoutedFile);
        var first = result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Findings.FirstOrDefault();
        return result.Status switch
        {
            CliSemanticStatus.Complete when targetEffect is null
                => new(RouteCreateWording.AlreadyMatching(path), CliHeadlineKind.NothingToDo),
            CliSemanticStatus.Complete when result.Mode == RouteCreateMode.DryRun
                => new(RouteCreateWording.WouldCreate(path, id), CliHeadlineKind.Preview),
            CliSemanticStatus.Complete
                => new(RouteCreateWording.Created(path, id), CliHeadlineKind.Done),
            CliSemanticStatus.Attention when result.Mode == RouteCreateMode.DryRun
                => new(RouteCreateWording.WouldCreate(path, id), CliHeadlineKind.Warnings),
            CliSemanticStatus.Attention
                when result.Effects.IsEmpty
                && result.Findings.Any(finding =>
                    finding.Code == RouteCreateFindingCode.OptionalMetadata)
                && !result.Findings.Any(finding =>
                    finding.Code == RouteCreateFindingCode.RecoveryArtifactRetained)
                => new(RouteCreateWording.AlreadyMatching(path), CliHeadlineKind.Warnings),
            CliSemanticStatus.Attention
                => new(RouteCreateWording.Created(path, id), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete
                => new(RouteCreateWording.Incomplete(Message(result, first)), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid
                => new(InvalidHeadline(result, first), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked
                => new(RouteCreateWording.Blocked(path, BlockedReason(result, first)), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed
                => new(RouteCreateWording.Failed(ProgressedCount(result), result.Effects.Length), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted
                => new(RouteCreateWording.Cancelled(), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Route Create status is not defined."),
        };
    }

    private static string InvalidHeadline(RouteCreateResult result, RouteCreateFinding? finding)
    {
        var message = Message(result, finding);
        return finding?.Code == RouteCreateFindingCode.InvalidInput
            ? message
            : RouteCreateWording.Invalid(message);
    }

    private static string BlockedReason(RouteCreateResult result, RouteCreateFinding? finding)
    {
        if (finding is null)
        {
            return global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.LabelTheBlockingConditionIsNotDefined();
        }

        return finding.Code switch
        {
            RouteCreateFindingCode.TargetContentDiffers
                or RouteCreateFindingCode.ParentMissing
                or RouteCreateFindingCode.TemplateUnsafe
                or RouteCreateFindingCode.TemplateUnavailable
                or RouteCreateFindingCode.IdentityCollision
                or RouteCreateFindingCode.WorkspaceLockUnavailable => Message(result, finding),
            _ => TrimSentence(finding.Cause),
        };
    }

    private static string? HeadlineFindingCode(RouteCreateResult result)
    {
        if (result.Findings.Length != 1)
        {
            return null;
        }

        return result.Status is CliSemanticStatus.Invalid
            or CliSemanticStatus.Blocked
            or CliSemanticStatus.Incomplete
            ? RouteCreateWireVocabulary.Name(result.Findings[0].Code)
            : null;
    }

    private static CliFinding Finding(RouteCreateResult result, RouteCreateFinding finding)
    {
        var code = RouteCreateWireVocabulary.Name(finding.Code);
        var subject = Subject(result, finding);
        var action = Action(result, finding);
        return new CliFinding
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = code,
            Title = RouteCreateWording.FindingTitle(finding.Code),
            Message = Message(result, finding),
            Subject = subject,
            Resolution = finding.Code == RouteCreateFindingCode.TargetContentDiffers
                ? CliResolution.TargetedOperation
                : null,
            Actions = action is null ? [] : [action],
            Evidence = finding.Code == RouteCreateFindingCode.WorkspaceLockUnavailable
                ? [new CliEvidence("cause", finding.Cause)]
                : [],
        };
    }

    private static CliSubject Subject(RouteCreateResult result, RouteCreateFinding finding)
    {
        if (finding.Code is RouteCreateFindingCode.WorkspaceUnavailable
            or RouteCreateFindingCode.WorkspaceUnsafe)
        {
            return new CliSubject(
                CliSubjectKind.Workspace,
                Id: result.WorkspacePath ?? finding.Target ?? "workspace");
        }

        var value = finding.Target
            ?? result.Target.Path
            ?? result.Target.Id
            ?? result.Target.Requested;
        var kind = finding.Code switch
        {
            RouteCreateFindingCode.InvalidInput
                or RouteCreateFindingCode.InvalidTarget
                or RouteCreateFindingCode.InvalidMetadata
                or RouteCreateFindingCode.InvalidTemplate
                or RouteCreateFindingCode.RouteAmbiguous
                or RouteCreateFindingCode.IdentityCollision => CliSubjectKind.Identifier,
            RouteCreateFindingCode.ParentMissing => CliSubjectKind.Directory,
            RouteCreateFindingCode.TemplateUnsafe
                or RouteCreateFindingCode.TemplateUnavailable => CliSubjectKind.Source,
            _ => CliSubjectKind.File,
        };
        return kind == CliSubjectKind.Identifier
            ? new CliSubject(kind, Id: value)
            : kind == CliSubjectKind.Directory
                ? new CliSubject(kind, Path: ParentFolder(result))
                : new CliSubject(kind, Path: value);
    }

    private static string Message(RouteCreateResult result, RouteCreateFinding? finding)
    {
        if (finding is null)
        {
            return global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.MessageTheRouteCreateResultDidNotContainAFinding();
        }

        var path = finding.Target
            ?? result.Target.Path
            ?? result.Target.Requested;
        var workspace = result.WorkspacePath ?? path;
        var template = finding.Target
            ?? result.Template?.Requested
            ?? RouteCreateWireVocabulary.TemplateReferenceValueName;
        return finding.Code switch
        {
            RouteCreateFindingCode.InvalidInput
                => CliFindingWording.InvalidInput(global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.LabelCreateTheRoutedFile(), TrimSentence(finding.Cause)),
            RouteCreateFindingCode.InvalidTarget
                => RouteCreateWording.InvalidTarget(path, IsSpecialTarget(path)),
            RouteCreateFindingCode.InvalidMetadata
                => RouteCreateWording.InvalidMetadata(TrimSentence(finding.Cause)),
            RouteCreateFindingCode.InvalidTemplate
                => RouteCreateWording.InvalidTemplate(template),
            RouteCreateFindingCode.WorkspaceUnavailable
                => CliFindingWording.WorkspaceUnavailable(workspace),
            RouteCreateFindingCode.WorkspaceUnsafe
                => CliFindingWording.WorkspaceUnsafe(workspace, TrimSentence(finding.Cause)),
            RouteCreateFindingCode.TargetUnsafe
                => CliFindingWording.TargetUnsafe(path, TrimSentence(finding.Cause)),
            RouteCreateFindingCode.TargetContentDiffers
                => RouteCreateWording.TargetContentDiffers(path),
            RouteCreateFindingCode.ParentMissing
                => RouteCreateWording.ParentMissing(ParentFolder(result)),
            RouteCreateFindingCode.RouteAmbiguous
                => CliFindingWording.RouteAmbiguous(result.Target.Id ?? result.Target.Requested),
            RouteCreateFindingCode.IdentityCollision
                => RouteCreateWording.IdentityCollision(result.Target.Id ?? result.Target.Requested),
            RouteCreateFindingCode.MetadataUnsafe
                => CliFindingWording.MetadataUnsafe(path, TrimSentence(finding.Cause)),
            RouteCreateFindingCode.TemplateUnsafe
                => RouteCreateWording.TemplateUnsafe(template),
            RouteCreateFindingCode.GeneratedRegionUnsafe
                => CliFindingWording.GeneratedRegionUnsafe(path, TrimSentence(finding.Cause)),
            RouteCreateFindingCode.WorkspaceLockUnavailable
                => CliFindingWording.WorkspaceLockUnavailable(),
            RouteCreateFindingCode.TargetChanged
                => CliFindingWording.TargetChanged(path),
            RouteCreateFindingCode.RecoveryConflict
                => CliFindingWording.RecoveryConflict(path),
            RouteCreateFindingCode.InspectionIncomplete
                => CliFindingWording.InspectionIncomplete(path),
            RouteCreateFindingCode.MetadataIncomplete
                => CliFindingWording.MetadataIncomplete(path),
            RouteCreateFindingCode.ProjectionIncomplete
                => CliFindingWording.ProjectionUnavailable(path, TrimSentence(finding.Cause)),
            RouteCreateFindingCode.TemplateUnavailable
                => RouteCreateWording.TemplateUnavailable(template),
            RouteCreateFindingCode.RecoveryUnavailable
                => CliFindingWording.RecoveryUnavailable(result.WorkspacePath ?? path),
            RouteCreateFindingCode.RecoveryArtifactRetained
                => CliFindingWording.RecoveryRetained(result.Recovery.ResidualPath ?? path),
            RouteCreateFindingCode.OptionalMetadata
                => OptionalMetadataMessage(result, path),
            RouteCreateFindingCode.TargetChangedDuringApply
                => CliFindingWording.TargetChangedDuringApply(path, ProgressedCount(result), result.Effects.Length),
            RouteCreateFindingCode.WriteFailed
                => CliFindingWording.WriteFailed(path, ProgressedCount(result), result.Effects.Length, result.Recovery.ResidualPath ?? "unknown"),
            RouteCreateFindingCode.VerificationFailed
                => CliFindingWording.VerificationFailed(path, result.Recovery.ResidualPath ?? "unknown"),
            RouteCreateFindingCode.RecoveryFailed
                => CliFindingWording.RecoveryFailed(),
            RouteCreateFindingCode.OperationFailed
                => CliFindingWording.OperationFailed(global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.TitleRouteCreate(), TrimSentence(finding.Cause)),
            RouteCreateFindingCode.Interrupted
                => ProgressedCount(result) == 0
                    ? RouteCreateWording.Cancelled()
                    : CliFindingWording.Interrupted(global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.TitleRouteCreate(), ProgressedCount(result), result.Effects.Length),
            _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code, "The Route Create finding code is not defined."),
        };
    }

    private static CliNextAction? Action(RouteCreateResult result, RouteCreateFinding finding)
        => finding.Code switch
        {
            RouteCreateFindingCode.InvalidTarget
                => new CliNextAction(RouteCreateWireVocabulary.RouteCreateHelpCommand, RouteCreateWording.CorrectInputReason()),
            RouteCreateFindingCode.InvalidMetadata
                => new CliNextAction(
                    RouteCreateWording.CorrectedCreateCommand(result.Target.Requested),
                    RouteCreateWording.CorrectInputReason()),
            RouteCreateFindingCode.InvalidTemplate
                => new CliNextAction("open-forge find --tag Template", RouteCreateWording.FindTemplateReason()),
            RouteCreateFindingCode.ParentMissing
                => new CliNextAction(
                    $"open-forge route init {ParentId(result)}",
                    RouteCreateWording.InitParentReason()),
            RouteCreateFindingCode.TargetContentDiffers
                => new CliNextAction(
                    $"open-forge route update {TargetId(result)}",
                    RouteCreateWording.UpdateReason()),
            RouteCreateFindingCode.RecoveryArtifactRetained
                => new CliNextAction("open-forge cleanup", RouteCreateWording.CleanupReason()),
            RouteCreateFindingCode.OptionalMetadata
                => new CliNextAction(
                    $"open-forge route update {TargetId(result)}",
                    RouteCreateWording.AddOptionalDescriptionOrTag()),
            RouteCreateFindingCode.Interrupted
                => new CliNextAction("open-forge route create", RouteCreateWording.RerunReason()),
            _ => null,
        };

    private static CliNextAction? Next(RouteCreateResult result)
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
                => new CliNextAction("open-forge cleanup", RouteCreateWording.CleanupReason()),
            CliSemanticStatus.Incomplete
                => null,
            CliSemanticStatus.Blocked when first?.Code is RouteCreateFindingCode.WorkspaceLockUnavailable
                or RouteCreateFindingCode.TargetChanged
                => new CliNextAction("open-forge route create", RouteCreateWording.RetryReason()),
            CliSemanticStatus.Blocked
                => new CliNextAction("open-forge doctor", global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.MessageInspectTheBlockedRouteCreateBoundaryBeforeRerunningTheRequest()),
            CliSemanticStatus.Invalid
                => new CliNextAction(RouteCreateWireVocabulary.RouteCreateHelpCommand, RouteCreateWording.CorrectInputReason()),
            CliSemanticStatus.Failed
                => new CliNextAction(RouteCreateWireVocabulary.VerboseRouteCreateCommand, RouteCreateWording.DebugReason()),
            CliSemanticStatus.Interrupted
                => new CliNextAction("open-forge route create", RouteCreateWording.RerunReason()),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Route Create status is not defined."),
        };
    }

    private static string OptionalMetadataMessage(
        RouteCreateResult result,
        string path)
    {
        if (result.Status != CliSemanticStatus.Attention)
        {
            return RouteCreateWording.OptionalMetadataMissing(path);
        }

        if (result.Mode == RouteCreateMode.DryRun)
        {
            return RouteCreateWording.WouldCreateWithoutOptionalMetadata(path);
        }

        return result.Effects.Any(effect =>
                effect.Kind == RouteCreateEffectKind.RoutedFile
                && effect.Outcome == RouteCreateEffectOutcome.Verified)
            ? RouteCreateWording.CreatedWithoutOptionalMetadata(path)
            : RouteCreateWording.CurrentWithoutOptionalMetadata(path);
    }

    private static CliEffect Effect(RouteCreateEffect effect, CliDetail detail)
        => new()
        {
            Path = effect.Path,
            Kind = effect.Kind switch
            {
                RouteCreateEffectKind.Directory => CliEffectKind.Directory,
                RouteCreateEffectKind.GeneratedRegion => CliEffectKind.Section,
                RouteCreateEffectKind.Entrypoint
                    or RouteCreateEffectKind.RoutedFile => CliEffectKind.File,
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Kind, "The Route Create effect kind is not defined."),
            },
            Action = effect.Action == RouteCreateEffectAction.Create
                ? CliEffectAction.Created
                : CliEffectAction.Rewritten,
            Outcome = effect.Outcome switch
            {
                RouteCreateEffectOutcome.Planned => CliEffectOutcome.Planned,
                RouteCreateEffectOutcome.Verified => CliEffectOutcome.Done,
                RouteCreateEffectOutcome.NotStarted => CliEffectOutcome.NotStarted,
                RouteCreateEffectOutcome.CompletionUnknown => CliEffectOutcome.Unknown,
                RouteCreateEffectOutcome.VerificationFailed => CliEffectOutcome.Failed,
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Outcome, "The Route Create effect outcome is not defined."),
            },
            Before = detail >= CliDetail.Full ? effect.Change?.Before : null,
            After = detail >= CliDetail.Full ? effect.Change?.Expected : null,
        };

    private static CliRecovery? Recovery(RouteCreateResult result)
        => new(
            result.Recovery.ResidualPath,
            result.Recovery.State switch
            {
                RouteCreateRecoveryState.NotRequired or RouteCreateRecoveryState.NotCreated
                    => CliRecoveryDisposition.NotRequired,
                RouteCreateRecoveryState.Removed => CliRecoveryDisposition.Removed,
                RouteCreateRecoveryState.Retained => CliRecoveryDisposition.Retained,
                RouteCreateRecoveryState.Unknown => CliRecoveryDisposition.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(result), result.Recovery.State, "The Route Create recovery state is not defined."),
            });

    private static IReadOnlyList<string> Diagnostics(RouteCreateResult result)
        => new[]
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={RouteCreateWireVocabulary.Name(result.Mode)}",
            $"target={result.Target.Id ?? result.Target.Requested}",
            $"completeness={RouteCreateWireVocabulary.Name(result.Plan.Completeness)}",
            $"safety={RouteCreateWireVocabulary.Name(result.Plan.Safety)}",
            $"effects={result.Effects.Length.ToString(CultureInfo.InvariantCulture)}",
            $"recovery={HumanState(RouteCreateWireVocabulary.Name(result.Recovery.State))}",
            $"verification={HumanState(RouteCreateWireVocabulary.Name(result.Verification))}",
            $"findings={result.Findings.Length.ToString(CultureInfo.InvariantCulture)}",
        }.Concat(result.Findings.Select(finding => finding.Cause)).ToArray();

    private static string Verification(RouteCreateResult result, string path)
    {
        var effect = result.Effects.FirstOrDefault(candidate =>
            candidate.Kind == RouteCreateEffectKind.GeneratedRegion
            && string.Equals(candidate.Path, path, StringComparison.Ordinal));
        return effect is null
            ? RouteCreateWireVocabulary.Name(result.Verification)
            : RouteCreateWireVocabulary.Name(effect.Outcome);
    }

    private static string HumanState(string value)
        => value switch
        {
            "not-requested" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotRun(),
            "not-started" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            _ => value.Replace('-', ' '),
        };

    private static int CountProgressed(
        RouteCreateResult result,
        params RouteCreateEffectKind[] kinds)
        => result.Effects.Count(effect => kinds.Contains(effect.Kind)
            && IsProgressed(effect, result.Mode));

    private static int ProgressedCount(RouteCreateResult result)
        => result.Effects.Count(effect => IsProgressed(effect, result.Mode));

    private static bool IsProgressed(RouteCreateEffect effect, RouteCreateMode mode)
        => mode == RouteCreateMode.DryRun
            ? effect.Outcome == RouteCreateEffectOutcome.Planned
            : effect.Outcome == RouteCreateEffectOutcome.Verified;

    private static string ParentFolder(RouteCreateResult result)
    {
        var path = result.Target.Path ?? result.Target.Requested;
        var normalized = path.Replace('\\', '/');
        var end = normalized.EndsWith(".md", StringComparison.Ordinal)
            ? normalized[..^".md".Length]
            : normalized;
        var slash = end.LastIndexOf('/');
        return slash > 0 ? end[..slash] : global::OpenForge.Cli.OutputText.Route.Create.RouteCreateText.LabelTheTargetFolder();
    }

    private static string ParentId(RouteCreateResult result)
    {
        var folder = ParentFolder(result);
        return folder.StartsWith(".agents/", StringComparison.Ordinal)
            ? folder[".agents/".Length..]
            : folder;
    }

    private static string TargetId(RouteCreateResult result)
    {
        if (result.Target.Id is { } id)
        {
            return id;
        }

        var path = (result.Target.Path ?? result.Target.Requested).Replace('\\', '/');
        if (path.StartsWith(".agents/", StringComparison.Ordinal))
        {
            path = path[".agents/".Length..];
        }

        return path.EndsWith(".md", StringComparison.Ordinal)
            ? path[..^".md".Length]
            : path;
    }

    private static bool IsSpecialTarget(string target)
    {
        var normalized = target.Replace('\\', '/');
        var slash = normalized.LastIndexOf('/');
        var fileName = slash >= 0 ? normalized[(slash + 1)..] : normalized;
        return fileName.EndsWith(".overwrite.md", StringComparison.Ordinal)
            || fileName is "index.md" or "_index.md" or "references.md" or "_references.md"
            || fileName.StartsWith('_') && fileName.EndsWith(".md", StringComparison.Ordinal);
    }

    private static string TrimSentence(string value) => value.Trim().TrimEnd('.');
}
