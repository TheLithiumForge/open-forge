using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Globalization;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Presentation;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Cleanup.Shared.Rendering;

internal static class CleanupPresentation
{
    internal static string Json(CliPresentationRequest<CleanupResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var document = CleanupJsonProjection.Create(presentation.Result);
        if (presentation.Presentation.View == CliView.Compact)
        {
            return JsonSerializer.Serialize(
                CliCompactJsonProjection.Create(presentation.Result, document.Result),
                CleanupJsonContext.Compact.CompactDocument);
        }

        return JsonSerializer.Serialize(document, CleanupJsonContext.Default.CleanupJsonDocument);
    }

    internal static string Human(CliPresentationRequest<CleanupResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var style = CliHumanStyle.For(presentation);
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, result.Mode == CleanupMode.DryRun
            ? "Preview of recovery-data cleanup"
            : "Recovery-data cleanup");
        builder.AppendLine($"Mode: {CleanupWireVocabulary.Mode(result.Mode)}");
        builder.AppendLine($"Candidate check: {CleanupWireVocabulary.Coverage(result.Catalogue.Coverage)}; plan: {CleanupWireVocabulary.Safety(result.Plan.Safety)}");
        builder.AppendLine($"Preflight: {CleanupWireVocabulary.Preflight(result.Preflight.State)}");
        if (presentation.Presentation.View == CliView.Expanded || result.Mode != CleanupMode.DryRun)
        {
            builder.AppendLine($"Workspace lock: {CleanupWireVocabulary.Lease(result.Lease.State)}");
            builder.AppendLine($"Final workspace check: {CleanupWireVocabulary.Comparison(result.Revalidation.State)}");
        }

        builder.AppendLine($"Verification: {CleanupWireVocabulary.Verification(result.Verification.State)}");
        if (result.Mode == CleanupMode.DryRun && !result.Plan.DeletionEntries.IsEmpty)
        {
            builder.AppendLine("Dry run: planned removals require an application lease (workspace lock) and final validation before removal.");
        }

        var candidates = result.Catalogue.Candidates.ToLookup(candidate => candidate.Path, PhysicalIdentityTracker.PathComparer);
        var representedPaths = new HashSet<string>(PhysicalIdentityTracker.PathComparer);
        foreach (var effect in result.Effects)
        {
            var outcome = effect.Outcome == CleanupEffectOutcome.Verified
                ? "Removed and verified"
                : CleanupWireVocabulary.EffectOutcome(effect.Outcome);
            builder.AppendLine($"  {Escape(effect.Path)}: {outcome}; residual: {CleanupWireVocabulary.EffectResidual(effect.Residual)}");
            if (representedPaths.Add(effect.Path))
            {
                foreach (var candidate in candidates[effect.Path])
                {
                    AppendCandidateFacts(builder, candidate);
                }
            }

            if (effect.Cause is { } cause)
            {
                builder.AppendLine($"    {Escape(cause)}");
            }
        }

        foreach (var candidate in result.Catalogue.Candidates.Where(candidate => !representedPaths.Contains(candidate.Path)))
        {
            builder.AppendLine($"  {Escape(candidate.Path)}");
            AppendCandidateFacts(builder, candidate);
        }

        AppendObservedCandidates(builder, result);
        foreach (var finding in result.Findings)
        {
            builder.AppendLine($"{style.Finding(finding.Status)}: {Escape(finding.Cause)} [{CleanupWireVocabulary.FindingCode(finding.Code)}]");
            if (finding.Subject is { } subject)
            {
                builder.AppendLine($"    {Escape(subject)}");
            }
        }

        if (result.Mode == CleanupMode.DryRun)
        {
            builder.AppendLine("No files changed (--dry-run).");
        }

        CliHumanText.AppendNext(builder, presentation);

        return builder.ToString().TrimEnd();
    }

    internal static string Diagnostic(CliPresentationRequest<CleanupResult> presentation)
        => string.Create(CultureInfo.InvariantCulture,
            $"cleanup: candidates={presentation.Result.Catalogue.Candidates.Length}; effects={presentation.Result.Effects.Length}; findings={presentation.Result.Findings.Length}");

    private static void AppendCandidateFacts(StringBuilder builder, CleanupCandidate candidate)
    {
        builder.AppendLine($"    {CleanupWireVocabulary.CandidateKind(candidate.Kind)}; integrity: {CleanupWireVocabulary.Integrity(candidate.Integrity)}; removal: {CleanupWireVocabulary.Eligibility(candidate.Eligibility)}");
        if (candidate.Cause is { } cause)
        {
            builder.AppendLine($"    {Escape(cause)}");
        }
    }

    private static void AppendObservedCandidates(StringBuilder builder, CleanupResult result)
    {
        if (result.Revalidation.Observed is not { } observed)
        {
            return;
        }

        var representedPaths = result.Effects.Select(effect => effect.Path).ToHashSet(PhysicalIdentityTracker.PathComparer);
        foreach (var candidate in observed.Candidates)
        {
            if (representedPaths.Contains(candidate.Path))
            {
                continue;
            }

            var preservationEstablished = candidate.FileKind == CleanupArtifactFileKind.Ordinary
                && candidate.Integrity is RecoveryBundleIntegrity.Verified or RecoveryBundleIntegrity.Malformed
                    or RecoveryBundleIntegrity.Unsupported or RecoveryBundleIntegrity.Incomplete;
            var disposition = preservationEstablished ? "preserved" : "disposition unknown";
            builder.AppendLine($"  Observed {disposition}: {Escape(candidate.Path)}");
        }
    }

    private static string Escape(string value)
    {
        var builder = new StringBuilder();
        foreach (var character in value)
        {
            if (char.IsControl(character))
            {
                builder.Append(CultureInfo.InvariantCulture, $"\\u{(int)character:x4}");
            }
            else
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }
}
