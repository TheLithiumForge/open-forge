using System.Globalization;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Manifest;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Presentation.Extension.Create.Models;
using OpenForge.Cli.Core.Presentation.Extension.Create.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Extension.Create.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Extension.Create.Shared.Selection;

internal static class ExtensionCreateReportSelector
{
    internal static CliReport<ExtensionCreateData> Select(
        ExtensionCreateResult result,
        CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        return new CliReport<ExtensionCreateData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result),
            HeadlineFindingCode = HeadlineFindingCode(result),
            Workspace = null,
            Findings = result.Findings.Select(finding => Finding(result, finding)).ToArray(),
            Effects = Effects(result),
            Counts =
            [
                new CliCount("filesCreated", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesCreated(), CountProgressed(result, ExtensionCreateEffectKind.ManifestFile)),
                new CliCount("directoriesCreated", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelDirectoriesCreated(), CountProgressed(result, ExtensionCreateEffectKind.PayloadAgentsDirectory)),
            ],
            Data = Data(result, selection.Detail),
            Recovery = null,
            Next = result.Next,
            Diagnostics = selection.Detail == CliDetail.Debug
                ? Diagnostics(result)
                : [],
        };
    }

    private static ExtensionCreateData Data(
        ExtensionCreateResult result,
        CliDetail detail)
    {
        var includeStandard = detail >= CliDetail.Standard;
        var includeFull = detail >= CliDetail.Full;
        var manifestPath = EffectPath(result, ExtensionCreateEffectKind.ManifestFile);
        var contentPath = EffectPath(result, ExtensionCreateEffectKind.PayloadAgentsDirectory);
        ExtensionCreateDataManifest? manifest = null;
        if (includeStandard && result.Manifest is { } value)
        {
            manifest = new ExtensionCreateDataManifest
            {
                Name = value.Name,
                Description = value.Description,
                Version = value.Version,
                Dependencies = value.Dependencies,
            };
        }

        IReadOnlyList<string> textManifest = [];
        if (includeStandard && result.Manifest is { } textValue)
        {
            textManifest =
            [
                ExtensionCreateWording.ManifestName(textValue.Name),
                ExtensionCreateWording.ManifestDescription(textValue.Description),
                ExtensionCreateWording.ManifestVersion(textValue.Version),
                ExtensionCreateWording.ManifestDependencies(textValue.Dependencies),
            ];
        }

        var textEditInstruction = result.Status == CliSemanticStatus.Complete
            && manifestPath is not null
            && contentPath is not null
            ? ExtensionCreateWording.EditNext()
            : null;
        return new ExtensionCreateData
        {
            Mode = ExtensionCreateWireVocabulary.Name(result.Mode),
            Id = result.StableId,
            Folder = result.Catalogue,
            PackagePath = result.Destination,
            ManifestPath = manifestPath,
            ContentPath = contentPath,
            Manifest = manifest,
            ManifestContent = includeFull && result.Manifest is { } fullValue
                ? ManifestContent(fullValue)
                : null,
            TextRows = TextRows(result),
            TextManifest = textManifest,
            TextEditInstruction = textEditInstruction,
            ShowNoChanges = result.Mode == ExtensionCreateMode.DryRun
                && result.Status == CliSemanticStatus.Complete
                && !IsNoOp(result),
        };
    }

    private static IReadOnlyList<ExtensionCreateDataTextRow> TextRows(
        ExtensionCreateResult result)
    {
        var effects = result.IntendedEffects
            .Concat(result.AppliedEffects)
            .Distinct()
            .ToArray();
        var rows = new List<ExtensionCreateDataTextRow>(effects.Length);
        foreach (var effect in effects)
        {
            var wording = string.Empty;
            if (result.Mode != ExtensionCreateMode.DryRun
                && result.Status != CliSemanticStatus.Complete)
            {
                wording = IsApplied(result, effect)
                    ? ExtensionCreateWording.CreatedEffect()
                    : ExtensionCreateWording.NotStartedEffect();
            }

            var path = result.Mode == ExtensionCreateMode.DryRun && !IsNoOp(result)
                ? ExtensionCreateWording.WouldCreateEffect(effect.Path)
                : effect.Path;
            rows.Add(new ExtensionCreateDataTextRow(path, wording));
        }

        return rows;
    }

    private static CliHeadline Headline(ExtensionCreateResult result)
    {
        var id = result.StableId ?? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelTheExtension();
        var folder = result.Destination ?? result.Catalogue ?? global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.LabelThePackageFolder();
        var finding = result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Findings.FirstOrDefault();
        return result.Status switch
        {
            CliSemanticStatus.Complete when IsNoOp(result)
                => new(ExtensionCreateWording.AlreadyMatches(id, folder), CliHeadlineKind.NothingToDo),
            CliSemanticStatus.Complete when result.Mode == ExtensionCreateMode.DryRun
                => new(ExtensionCreateWording.WouldCreate(id, folder), CliHeadlineKind.Preview),
            CliSemanticStatus.Complete
                => new(ExtensionCreateWording.Created(id, folder), CliHeadlineKind.Done),
            CliSemanticStatus.Attention
                => new(ExtensionCreateWording.Created(id, folder), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete
                => new(ExtensionCreateWording.Incomplete(Message(result, finding)), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid
                => new(ExtensionCreateWording.Invalid(Message(result, finding)), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked
                => new(ExtensionCreateWording.Blocked(id, folder, BlockedReason(result, finding)), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed
                => new(ExtensionCreateWording.Failed(ProgressedCount(result), result.IntendedEffects.Count), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted
                => new(ExtensionCreateWording.Cancelled(), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Extension Create status is not defined."),
        };
    }

    private static string BlockedReason(
        ExtensionCreateResult result,
        ExtensionCreateFinding? finding)
        => finding?.Code switch
        {
            ExtensionCreateFindingCode.CatalogueUnsafe => CatalogueUnsafeReason(result, finding),
            ExtensionCreateFindingCode.DestinationCollision => TrimSentence(
                ExtensionCreateWording.DestinationCollision(
                    finding.Subject ?? result.Destination ?? result.Catalogue ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheDestination())),
            ExtensionCreateFindingCode.DestinationChanged => global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.LabelTheDestinationChangedAfterThePlanWasMade(),
            _ => TrimSentence(finding?.Cause ?? "the blocking condition is not defined"),
        };

    private static string? HeadlineFindingCode(ExtensionCreateResult result)
    {
        if (result.Findings.Count != 1)
        {
            return null;
        }

        return result.Status is CliSemanticStatus.Invalid
            or CliSemanticStatus.Blocked
            or CliSemanticStatus.Incomplete
            ? ExtensionCreateWording.FindingCode(result.Findings[0].Code)
            : null;
    }

    private static CliFinding Finding(
        ExtensionCreateResult result,
        ExtensionCreateFinding finding)
        => new()
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = ExtensionCreateWording.FindingCode(finding.Code),
            Title = ExtensionCreateWording.FindingTitle(finding.Code),
            Message = Message(result, finding),
            Subject = Subject(result, finding),
            Resolution = null,
            Actions = [],
        };

    private static CliSubject Subject(
        ExtensionCreateResult result,
        ExtensionCreateFinding finding)
    {
        var value = finding.Subject
            ?? result.Destination
            ?? result.Catalogue
            ?? result.StableId
            ?? result.Command;
        var kind = finding.Code switch
        {
            ExtensionCreateFindingCode.InvalidInput
                or ExtensionCreateFindingCode.ConfirmationRequired => CliSubjectKind.Identifier,
            ExtensionCreateFindingCode.CatalogueUnavailable
                or ExtensionCreateFindingCode.CatalogueUnsafe
                or ExtensionCreateFindingCode.DestinationCollision
                or ExtensionCreateFindingCode.DestinationChanged
                or ExtensionCreateFindingCode.ApplicationFailed
                or ExtensionCreateFindingCode.VerificationFailed
                or ExtensionCreateFindingCode.Interrupted when finding.Subject is not null => CliSubjectKind.Directory,
            ExtensionCreateFindingCode.Interrupted => CliSubjectKind.Identifier,
            _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code, "The Extension Create finding code is not defined."),
        };
        return kind == CliSubjectKind.Identifier
            ? new CliSubject(kind, Id: value)
            : new CliSubject(kind, Path: value);
    }

    private static string Message(
        ExtensionCreateResult result,
        ExtensionCreateFinding? finding)
    {
        if (finding is null)
        {
            return global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.MessageTheExtensionCreateResultDidNotContainAFinding();
        }

        var subject = finding.Subject
            ?? result.Destination
            ?? result.Catalogue
            ?? result.StableId
            ?? result.Command;
        return finding.Code switch
        {
            ExtensionCreateFindingCode.InvalidInput => InvalidInputMessage(finding),
            ExtensionCreateFindingCode.CatalogueUnavailable => ExtensionCreateWording.CatalogueUnavailable(subject),
            ExtensionCreateFindingCode.CatalogueUnsafe => ExtensionCreateWording.CatalogueUnsafe(
                subject,
                CatalogueUnsafeReason(result, finding)),
            ExtensionCreateFindingCode.DestinationCollision => ExtensionCreateWording.DestinationCollision(subject),
            ExtensionCreateFindingCode.DestinationChanged => ExtensionCreateWording.DestinationChanged(subject),
            ExtensionCreateFindingCode.ApplicationFailed => ExtensionCreateWording.ApplicationFailed(
                ProgressedCount(result),
                result.IntendedEffects.Count),
            ExtensionCreateFindingCode.VerificationFailed => ExtensionCreateWording.VerificationFailed(subject),
            ExtensionCreateFindingCode.ConfirmationRequired => CliFindingWording.ConfirmationRequired(global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.TitleExtensionCreate()),
            ExtensionCreateFindingCode.Interrupted => ExtensionCreateWording.Cancelled(),
            _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code, "The Extension Create finding code is not defined."),
        };
    }

    private static string InvalidInputMessage(ExtensionCreateFinding finding)
    {
        if (finding.Cause is "A stable ID is required." or "The stable ID is invalid.")
        {
            return finding.Subject is { Length: > 0 } value
                ? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedPhrases.FormatIsNotAValidExtensionIdUseLowercaseLettersDigitsAndHyphens($"{value}")
                : global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.MessageNoIdWasGivenAndThisSessionCannotAsk();
        }

        if (finding.Cause == "A catalogue path is required.")
        {
            return global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.MessageNoPathWasGivenAndThisSessionCannotAsk();
        }

        if (finding.Cause == "The stable-ID operand is invalid.")
        {
            return finding.Subject is { Length: > 0 } value
                ? global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedPhrases.FormatIsNotAValidExtensionIdUseLowercaseLettersDigitsAndHyphens($"{value}")
                : finding.Cause;
        }

        return finding.Cause;
    }

    private static string CatalogueUnsafeReason(
        ExtensionCreateResult result,
        ExtensionCreateFinding finding)
    {
        var path = finding.Subject ?? result.Catalogue ?? string.Empty;
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/.agents/", StringComparison.Ordinal)
            || normalized.EndsWith("/.agents", StringComparison.Ordinal)
            ? global::OpenForge.Cli.OutputText.Extension.Create.ExtensionCreateText.LabelItIsInsideTheWorkspaceSAgents()
            : global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.LabelItResolvesToAnUnsafeLocation();
    }

    private static IReadOnlyList<CliEffect> Effects(ExtensionCreateResult result)
    {
        var effects = result.IntendedEffects
            .Concat(result.AppliedEffects)
            .Distinct()
            .ToArray();
        return effects.Select(effect => new CliEffect
        {
            Path = effect.Path,
            Kind = effect.Kind == ExtensionCreateEffectKind.ManifestFile
                ? CliEffectKind.File
                : CliEffectKind.Directory,
            Action = CliEffectAction.Created,
            Outcome = EffectOutcome(result, effect),
        }).ToArray();
    }

    private static CliEffectOutcome EffectOutcome(
        ExtensionCreateResult result,
        ExtensionCreateEffect effect)
    {
        if (result.Mode == ExtensionCreateMode.DryRun && !IsNoOp(result))
        {
            return CliEffectOutcome.Planned;
        }

        return IsApplied(result, effect)
            ? CliEffectOutcome.Done
            : CliEffectOutcome.NotStarted;
    }

    private static IReadOnlyList<string> Diagnostics(ExtensionCreateResult result)
        => new[]
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={ExtensionCreateWireVocabulary.Name(result.Mode)}",
            $"id={Value(result.StableId)}",
            $"folder={Value(result.Catalogue)}",
            $"package={Value(result.Destination)}",
            string.Create(CultureInfo.InvariantCulture, $"intended={result.IntendedEffects.Count}"),
            string.Create(CultureInfo.InvariantCulture, $"applied={result.AppliedEffects.Count}"),
            string.Create(CultureInfo.InvariantCulture, $"findings={result.Findings.Count}"),
        }.Concat(result.Findings.Select(finding =>
            $"finding={ExtensionCreateWording.FindingCode(finding.Code)}:subject={Value(finding.Subject)}:cause={Value(finding.Cause)}"))
        .ToArray();

    private static string ManifestContent(ExtensionCreateManifest manifest)
        => JsonSerializer.Serialize(
            new ExtensionCreateDataManifestContent
            {
                Id = manifest.Id,
                Name = manifest.Name,
                Description = manifest.Description,
                Version = manifest.Version,
                Dependencies = manifest.Dependencies,
            },
            ExtensionCreateManifestContentJsonContext.Default.ExtensionCreateDataManifestContent);

    private static string? EffectPath(
        ExtensionCreateResult result,
        ExtensionCreateEffectKind kind)
        => result.IntendedEffects
            .Concat(result.AppliedEffects)
            .FirstOrDefault(effect => effect.Kind == kind)?.Path;

    private static bool IsApplied(
        ExtensionCreateResult result,
        ExtensionCreateEffect effect)
        => result.AppliedEffects.Contains(effect);

    private static bool IsNoOp(ExtensionCreateResult result)
        => result.AppliedEffects.Count == 0
            && result.Verification.Destination == ExtensionCreateVerificationState.Verified;

    private static int CountProgressed(
        ExtensionCreateResult result,
        ExtensionCreateEffectKind kind)
        => result.IntendedEffects.Count(effect => effect.Kind == kind && IsProgressed(result, effect));

    private static int ProgressedCount(ExtensionCreateResult result)
        => result.IntendedEffects.Count(effect => IsProgressed(result, effect));

    private static bool IsProgressed(
        ExtensionCreateResult result,
        ExtensionCreateEffect effect)
    {
        if (result.Mode == ExtensionCreateMode.DryRun)
        {
            return !IsNoOp(result);
        }

        return IsApplied(result, effect);
    }

    private static string Value(string? value) => value ?? "none";

    private static string TrimSentence(string value) => value.Trim().TrimEnd('.');
}
