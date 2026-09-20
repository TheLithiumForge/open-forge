using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Update.Models;
using OpenForge.Cli.Core.Presentation.Route.Update.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Route.Update.Shared.Selection;

internal static class RouteUpdateReportSelector
{
    internal static CliReport<RouteUpdateData> Select(
        RouteUpdateResult result,
        CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        return new CliReport<RouteUpdateData>
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
                new CliCount("fieldsChanged", global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.LabelFieldsChanged(), ChangedFields(result)),
                new CliCount("sectionsUpdated", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelSectionsUpdated(), CountProgressed(result, RouteUpdateEffectKind.GeneratedRegion)),
            ],
            Data = Data(result, selection.Detail),
            Recovery = Recovery(result),
            Next = Next(result),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? Diagnostics(result)
                : [],
        };
    }

    private static RouteUpdateData Data(RouteUpdateResult result, CliDetail detail)
    {
        var standard = detail >= CliDetail.Standard;
        var full = detail >= CliDetail.Full;
        var targetEffect = result.Effects.FirstOrDefault(effect =>
            effect.Kind == RouteUpdateEffectKind.RoutedFile);
        var parentEffect = result.Effects.FirstOrDefault(effect =>
            effect.Kind == RouteUpdateEffectKind.GeneratedRegion);
        var changes = Changes(result.Patch);
        var targetDataEffect = targetEffect is null
            ? null
            : new RouteUpdateDataEffect
            {
                Path = targetEffect.Path,
                Action = RouteUpdateWording.FrontmatterRewritten(targetEffect.Path),
                Outcome = RouteUpdateWireVocabulary.Name(targetEffect.Outcome),
                Before = full ? targetEffect.Change.Before : null,
                After = full ? targetEffect.Change.Expected : null,
            };
        var template = result.Template is { Id: not null, Path: not null } value
            ? new RouteUpdateDataTemplate
            {
                Id = value.Id,
                Path = value.Path,
                Applied = value.Decision == RouteUpdateTemplateDecision.Copied,
            }
            : null;
        var textRows = TextRows(result, changes, parentEffect);
        return new RouteUpdateData
        {
            Mode = RouteUpdateWireVocabulary.Name(result.Mode),
            Target = new RouteUpdateDataTarget
            {
                Id = result.Target.Id,
                Path = result.Target.Path,
            },
            Changes = changes,
            Template = template,
            ListedIn = ParentPath(result),
            FrontmatterBefore = full ? Frontmatter(targetEffect, before: true) : null,
            FrontmatterAfter = full ? Frontmatter(targetEffect, before: false) : null,
            TextMetadata = TextMetadata(result, standard, template),
            TextRows = textRows,
            TextEffects = standard && targetDataEffect is not null
                ? [targetDataEffect]
                : [],
            ShowNoChanges = result.Mode == RouteUpdateMode.DryRun
                && result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention,
        };
    }

    private static IReadOnlyList<RouteUpdateDataChange> Changes(RouteUpdatePatch patch)
    {
        var changes = new List<RouteUpdateDataChange>();
        if (patch.Description.State == RouteUpdatePatchState.Changed)
        {
            changes.Add(new RouteUpdateDataChange
            {
                Field = "description",
                Before = patch.Description.Before,
                After = patch.Description.Expected,
            });
        }

        if (patch.Responsibility.State == RouteUpdatePatchState.Changed)
        {
            changes.Add(new RouteUpdateDataChange
            {
                Field = "responsibility",
                Before = patch.Responsibility.Before,
                After = patch.Responsibility.Operation == RouteUpdateResponsibilityOperation.Remove
                    ? null
                    : patch.Responsibility.Expected,
            });
        }

        if (patch.Tags.State == RouteUpdatePatchState.Changed)
        {
            changes.Add(new RouteUpdateDataChange
            {
                Field = "tags",
                Before = Tags(patch.Tags.Before),
                After = Tags(patch.Tags.Expected),
            });
        }

        return changes;
    }

    private static IReadOnlyList<string> TextMetadata(
        RouteUpdateResult result,
        bool standard,
        RouteUpdateDataTemplate? template)
    {
        if (!standard)
        {
            return [];
        }

        var lines = new List<string>();
        if (result.Target.Path is { } path)
        {
            lines.Add(RouteUpdateWording.Path(path));
        }

        if (template is not null)
        {
            lines.Add(RouteUpdateWording.TemplatePath(template.Path));
        }

        return lines;
    }

    private static IReadOnlyList<string> TextRows(
        RouteUpdateResult result,
        IReadOnlyList<RouteUpdateDataChange> changes,
        RouteUpdateEffect? parentEffect)
    {
        var rows = changes.Select(ChangeRow).ToList();
        if (result.Template is { Decision: RouteUpdateTemplateDecision.Copied } template)
        {
            rows.Add(RouteUpdateWording.BodyCopied(template.Id ?? template.Requested));
        }

        if (result.Template is { Decision: RouteUpdateTemplateDecision.AuthoredBodyProtected } protectedTemplate)
        {
            rows.Add(RouteUpdateWording.ProtectedBody(protectedTemplate.Id ?? protectedTemplate.Requested));
        }

        if (parentEffect is not null && IsProgressed(parentEffect, result.Mode))
        {
            rows.Add(RouteUpdateWording.EntryUpdated(parentEffect.Path));
        }

        return rows;
    }

    private static string ChangeRow(RouteUpdateDataChange change)
    {
        var before = change.Field == "tags"
            ? change.Before ?? global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.PlaceholderNone()
            : change.Before is null ? global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.PlaceholderNone() : $"\"{change.Before}\"";
        var after = change.Field == "tags"
            ? change.After ?? global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.PlaceholderNone()
            : change.After is null
                ? change.Field == "responsibility" ? global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.PlaceholderRemoved() : global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.PlaceholderNone()
                : $"\"{change.After}\"";
        return $"{change.Field}: {before} -> {after}";
    }

    private static string? Tags(IReadOnlyList<string>? values)
        => values is null
            ? null
            : string.Join(" ", values.Select(value => value.StartsWith('#') ? value : $"#{value}"));

    private static string? Frontmatter(RouteUpdateEffect? effect, bool before)
    {
        if (effect is null)
        {
            return null;
        }

        var values = effect.Preview
            .Where(preview => preview.Kind == RouteUpdatePreviewKind.MetadataField)
            .Select(preview => before ? preview.Before : preview.Expected)
            .ToArray();
        return values.Length == 0 ? null : string.Join("\n", values);
    }

    private static CliHeadline Headline(RouteUpdateResult result)
    {
        var id = result.Target.Id ?? result.Target.Requested;
        var first = result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Findings.FirstOrDefault();
        return result.Status switch
        {
            CliSemanticStatus.Complete when result.Effects.IsEmpty
                => new(RouteUpdateWording.AlreadyMatching(id), CliHeadlineKind.NothingToDo),
            CliSemanticStatus.Complete when result.Mode == RouteUpdateMode.DryRun
                => new(RouteUpdateWording.WouldUpdate(id), CliHeadlineKind.Preview),
            CliSemanticStatus.Complete
                => new(RouteUpdateWording.Updated(id), CliHeadlineKind.Done),
            CliSemanticStatus.Attention when HasFinding(result, RouteUpdateFindingCode.TemplateBodyProtected)
                => new(RouteUpdateWording.UpdatedProtected(id), CliHeadlineKind.Warnings),
            CliSemanticStatus.Attention
                => new(RouteUpdateWording.Updated(id), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete
                => new(RouteUpdateWording.Incomplete(id, Message(result, first)), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid
                => new(RouteUpdateWording.Invalid(id, Message(result, first)), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked
                => new(RouteUpdateWording.Blocked(id, Message(result, first)), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed
                => new(RouteUpdateWording.Failed(ProgressedCount(result), result.Effects.Length), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted
                => new(RouteUpdateWording.Cancelled(), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Route Update status is not defined."),
        };
    }

    private static string? HeadlineFindingCode(RouteUpdateResult result)
    {
        if (result.Findings.Length != 1)
        {
            return null;
        }

        return result.Status is CliSemanticStatus.Invalid
            or CliSemanticStatus.Blocked
            or CliSemanticStatus.Incomplete
            or CliSemanticStatus.Failed
            or CliSemanticStatus.Interrupted
            or CliSemanticStatus.Attention
            ? RouteUpdateWireVocabulary.Name(result.Findings[0].Code)
            : null;
    }

    private static CliFinding Finding(RouteUpdateResult result, RouteUpdateFinding finding)
    {
        var code = RouteUpdateWireVocabulary.Name(finding.Code);
        var action = Action(result, finding);
        return new CliFinding
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = code,
            Title = RouteUpdateWording.FindingTitle(finding.Code),
            Message = Message(result, finding),
            Subject = Subject(result, finding),
            Resolution = finding.Code is RouteUpdateFindingCode.FrontmatterUnsafe
                or RouteUpdateFindingCode.MetadataPreservationUnsafe
                ? CliResolution.ManualDecision
                : null,
            Actions = action is null ? [] : [action],
        };
    }

    private static CliSubject Subject(RouteUpdateResult result, RouteUpdateFinding finding)
    {
        if (finding.Code is RouteUpdateFindingCode.WorkspaceUnavailable
            or RouteUpdateFindingCode.WorkspaceUnsafe)
        {
            return new CliSubject(
                CliSubjectKind.Workspace,
                Id: result.WorkspacePath ?? finding.Target ?? "workspace");
        }

        var target = finding.Target
            ?? result.Target.Path
            ?? result.Target.Id
            ?? result.Target.Requested;
        var kind = finding.Code switch
        {
            RouteUpdateFindingCode.InvalidInput
                or RouteUpdateFindingCode.InvalidTarget
                or RouteUpdateFindingCode.InvalidPatch
                or RouteUpdateFindingCode.InvalidTemplate
                or RouteUpdateFindingCode.RouteAmbiguous
                or RouteUpdateFindingCode.IdentityCollision => CliSubjectKind.Identifier,
            RouteUpdateFindingCode.TemplateUnsafe
                or RouteUpdateFindingCode.TemplateUnavailable
                or RouteUpdateFindingCode.TemplateBodyProtected => CliSubjectKind.Source,
            _ => CliSubjectKind.File,
        };
        return kind switch
        {
            CliSubjectKind.Identifier => new CliSubject(kind, Id: target),
            CliSubjectKind.Source => new CliSubject(kind, Path: result.Template?.Path ?? target),
            _ => new CliSubject(kind, Path: result.Target.Path ?? target),
        };
    }

    private static string Message(RouteUpdateResult result, RouteUpdateFinding? finding)
    {
        if (finding is null)
        {
            return global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.MessageTheRouteUpdateResultDidNotContainAFinding();
        }

        var target = result.Target.Id ?? result.Target.Requested;
        var path = result.Target.Path ?? result.Target.Requested;
        var template = result.Template?.Requested
            ?? finding.Target
            ?? RouteUpdateWireVocabulary.TemplateReferenceValueName;
        var cause = TrimSentence(finding.Cause);
        return finding.Code switch
        {
            RouteUpdateFindingCode.InvalidInput => cause.Contains("at least one metadata", StringComparison.OrdinalIgnoreCase)
                || cause.Contains("requires one source", StringComparison.OrdinalIgnoreCase)
                ? RouteUpdateWording.NothingToUpdate()
                : CliFindingWording.InvalidInput(global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUpdate(), cause),
            RouteUpdateFindingCode.InvalidTarget => RouteUpdateWording.InvalidTarget(finding.Target ?? target),
            RouteUpdateFindingCode.InvalidPatch => InvalidPatchMessage(cause),
            RouteUpdateFindingCode.InvalidTemplate => RouteUpdateWording.InvalidTemplate(template),
            RouteUpdateFindingCode.WorkspaceUnavailable => CliFindingWording.WorkspaceUnavailable(result.WorkspacePath ?? path),
            RouteUpdateFindingCode.WorkspaceUnsafe => CliFindingWording.WorkspaceUnsafe(result.WorkspacePath ?? path, cause),
            RouteUpdateFindingCode.TargetUnsafe => CliFindingWording.TargetUnsafe(path, cause),
            RouteUpdateFindingCode.RouteAmbiguous => CliFindingWording.RouteAmbiguous(target),
            RouteUpdateFindingCode.IdentityCollision => CliFindingWording.IdentityCollision(target),
            RouteUpdateFindingCode.FrontmatterUnsafe => RouteUpdateWording.FrontmatterUnsafe(path, cause),
            RouteUpdateFindingCode.MetadataPreservationUnsafe => RouteUpdateWording.MetadataPreservationUnsafe(path),
            RouteUpdateFindingCode.TemplateUnsafe => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdatePhrases.FormatTheTemplateCouldNotBeVerifiedSafely($"{template}"),
            RouteUpdateFindingCode.GeneratedRegionUnsafe => CliFindingWording.GeneratedRegionUnsafe(path, cause),
            RouteUpdateFindingCode.WorkspaceLockUnavailable => CliFindingWording.WorkspaceLockUnavailable(),
            RouteUpdateFindingCode.TargetChanged => CliFindingWording.TargetChanged(path),
            RouteUpdateFindingCode.RecoveryConflict => CliFindingWording.RecoveryConflict(path),
            RouteUpdateFindingCode.InspectionIncomplete => CliFindingWording.InspectionIncomplete(path),
            RouteUpdateFindingCode.ProjectionIncomplete => CliFindingWording.ProjectionUnavailable(path, cause),
            RouteUpdateFindingCode.TemplateUnavailable => global::OpenForge.Cli.OutputText.Route.Update.RouteUpdatePhrases.FormatTheTemplateCouldNotBeRead($"{template}"),
            RouteUpdateFindingCode.RecoveryUnavailable => CliFindingWording.RecoveryUnavailable(result.WorkspacePath ?? path),
            RouteUpdateFindingCode.TemplateBodyProtected => RouteUpdateWording.ProtectedBody(template),
            RouteUpdateFindingCode.RecoveryArtifactRetained => CliFindingWording.RecoveryRetained(result.Recovery.ResidualPath ?? path),
            RouteUpdateFindingCode.TargetChangedDuringApply => CliFindingWording.TargetChangedDuringApply(path, ProgressedCount(result), result.Effects.Length),
            RouteUpdateFindingCode.WriteFailed => CliFindingWording.WriteFailed(path, ProgressedCount(result), result.Effects.Length, result.Recovery.ResidualPath ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnknown()),
            RouteUpdateFindingCode.VerificationFailed => CliFindingWording.VerificationFailed(path, result.Recovery.ResidualPath ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUnknown()),
            RouteUpdateFindingCode.RecoveryFailed => CliFindingWording.RecoveryFailed(),
            RouteUpdateFindingCode.OperationFailed => CliFindingWording.OperationFailed(global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.TitleRouteUpdate(), cause),
            RouteUpdateFindingCode.Interrupted => RouteUpdateWording.Cancelled(),
            _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code, "The Route Update finding code is not defined."),
        };
    }

    private static string InvalidPatchMessage(string cause)
    {
        if (cause.Contains("repeated", StringComparison.OrdinalIgnoreCase))
        {
            return global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.MessageTagValueIsRepeated();
        }

        if (cause.Contains("description", StringComparison.OrdinalIgnoreCase)
            && cause.Contains("blank", StringComparison.OrdinalIgnoreCase))
        {
            return global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.MessageDescriptionMustNotBeBlank();
        }

        return cause;
    }

    private static CliNextAction? Action(RouteUpdateResult result, RouteUpdateFinding finding)
        => finding.Code switch
        {
            RouteUpdateFindingCode.InvalidTarget => new CliNextAction(
                "open-forge route list --depth=all",
                global::OpenForge.Cli.OutputText.Route.Update.RouteUpdateText.MessageChooseARoutedSourceThenRerunRouteUpdate()),
            RouteUpdateFindingCode.InvalidInput
                or RouteUpdateFindingCode.InvalidPatch => new CliNextAction(
                RouteUpdateCommand(result),
                RouteUpdateWording.CorrectInputReason()),
            RouteUpdateFindingCode.InvalidTemplate => new CliNextAction(
                "open-forge find --tag Template",
                RouteUpdateWording.FindTemplateReason()),
            RouteUpdateFindingCode.RouteAmbiguous => new CliNextAction(
                "open-forge route list --depth=all",
                RouteUpdateWording.ChooseSourceReason()),
            RouteUpdateFindingCode.RecoveryArtifactRetained => new CliNextAction(
                "open-forge cleanup",
                RouteUpdateWording.CleanupReason()),
            RouteUpdateFindingCode.WorkspaceLockUnavailable
                or RouteUpdateFindingCode.TargetChanged => new CliNextAction(
                "open-forge route update",
                RouteUpdateWording.RetryReason()),
            RouteUpdateFindingCode.TemplateBodyProtected => null,
            RouteUpdateFindingCode.Interrupted => new CliNextAction(
                "open-forge route update",
                RouteUpdateWording.RerunReason()),
            _ => null,
        };

    private static CliNextAction? Next(RouteUpdateResult result)
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
            CliSemanticStatus.Attention => result.Recovery.State == RouteUpdateRecoveryState.Retained
                ? new CliNextAction("open-forge cleanup", RouteUpdateWording.CleanupReason())
                : null,
            CliSemanticStatus.Incomplete => null,
            CliSemanticStatus.Failed => new CliNextAction(
                RouteUpdateWireVocabulary.VerboseRouteUpdateCommand,
                RouteUpdateWording.DebugReason()),
            CliSemanticStatus.Interrupted => new CliNextAction(
                "open-forge route update",
                RouteUpdateWording.RerunReason()),
            _ => null,
        };
    }

    private static CliEffect Effect(RouteUpdateEffect effect, CliDetail detail)
        => new()
        {
            Path = effect.Path,
            Kind = effect.Kind == RouteUpdateEffectKind.RoutedFile
                ? CliEffectKind.File
                : CliEffectKind.Section,
            Action = CliEffectAction.Replaced,
            Outcome = effect.Outcome switch
            {
                RouteUpdateEffectOutcome.Planned => CliEffectOutcome.Planned,
                RouteUpdateEffectOutcome.Verified => CliEffectOutcome.Done,
                RouteUpdateEffectOutcome.NotStarted => CliEffectOutcome.NotStarted,
                RouteUpdateEffectOutcome.CompletionUnknown => CliEffectOutcome.Unknown,
                RouteUpdateEffectOutcome.VerificationFailed => CliEffectOutcome.Failed,
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect.Outcome, "The Route Update effect outcome is not defined."),
            },
            Before = detail >= CliDetail.Full ? effect.Change.Before : null,
            After = detail >= CliDetail.Full ? effect.Change.Expected : null,
        };

    private static CliRecovery Recovery(RouteUpdateResult result)
        => new(
            result.Recovery.ResidualPath,
            result.Recovery.State switch
            {
                RouteUpdateRecoveryState.NotRequired or RouteUpdateRecoveryState.NotCreated
                    => CliRecoveryDisposition.NotRequired,
                RouteUpdateRecoveryState.Removed => CliRecoveryDisposition.Removed,
                RouteUpdateRecoveryState.Retained => CliRecoveryDisposition.Retained,
                RouteUpdateRecoveryState.Unknown => CliRecoveryDisposition.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(result), result.Recovery.State, "The Route Update recovery state is not defined."),
            });

    private static IReadOnlyList<string> Diagnostics(RouteUpdateResult result)
        => new[]
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={RouteUpdateWireVocabulary.Name(result.Mode)}",
            $"target={result.Target.Id ?? result.Target.Requested}",
            $"completeness={RouteUpdateWireVocabulary.Name(result.Plan.Completeness)}",
            $"safety={RouteUpdateWireVocabulary.Name(result.Plan.Safety)}",
            $"body={RouteUpdateWireVocabulary.Name(result.Plan.Body)}",
            $"effects={result.Effects.Length.ToString(CultureInfo.InvariantCulture)}",
            $"recovery={HumanState(RouteUpdateWireVocabulary.Name(result.Recovery.State))}",
            $"verification={HumanState(RouteUpdateWireVocabulary.Name(result.Verification))}",
            $"findings={result.Findings.Length.ToString(CultureInfo.InvariantCulture)}",
        }.Concat(result.Findings.Select(finding => finding.Cause)).ToArray();

    private static string HumanState(string value)
        => value switch
        {
            "not-requested" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotRun(),
            "not-started" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
            _ => value.Replace('-', ' '),
        };

    private static string? ParentPath(RouteUpdateResult result)
    {
        var target = result.Target.Path;
        var parent = result.Effects.FirstOrDefault(effect =>
                effect.Kind == RouteUpdateEffectKind.GeneratedRegion)?.Path
            ?? result.UnchangedPaths.FirstOrDefault(path =>
                !string.Equals(path, target, StringComparison.Ordinal));
        return parent;
    }

    private static int ChangedFields(RouteUpdateResult result)
        => Changes(result.Patch).Count;

    private static int CountProgressed(RouteUpdateResult result, RouteUpdateEffectKind kind)
        => result.Effects.Count(effect => effect.Kind == kind && IsProgressed(effect, result.Mode));

    private static int ProgressedCount(RouteUpdateResult result)
        => result.Effects.Count(effect => IsProgressed(effect, result.Mode));

    private static bool IsProgressed(RouteUpdateEffect effect, RouteUpdateMode mode)
        => mode == RouteUpdateMode.DryRun
            ? effect.Outcome == RouteUpdateEffectOutcome.Planned
            : effect.Outcome == RouteUpdateEffectOutcome.Verified;

    private static bool HasFinding(RouteUpdateResult result, RouteUpdateFindingCode code)
        => result.Findings.Any(finding => finding.Code == code);

    private static string RouteUpdateCommand(RouteUpdateResult result)
        => result.Target.Requested is { Length: > 0 } target
            ? $"open-forge route update {target} --description \"<one sentence>\""
            : RouteUpdateWireVocabulary.RouteUpdateHelpCommand;

    private static string TrimSentence(string value) => value.Trim().TrimEnd('.');
}
