using System.Globalization;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Presentation;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Cleanup.Shared.Rendering;

internal static class CleanupPresentation
{
    internal static string Json(CliPresentationRequest<CleanupResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return JsonSerializer.Serialize(CleanupJsonProjection.Create(presentation.Result), CleanupJsonContext.Default.CleanupJsonDocument);
    }

    internal static string Human(CliPresentationRequest<CleanupResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var workspace = result.Workspace?.LexicalRoot ?? "not established";
        var selectedBy = result.Workspace is { } selectedWorkspace
            ? CleanupWireVocabulary.WorkspaceSelection(selectedWorkspace.SelectedBy)
            : "not established";
        var builder = new StringBuilder();
        builder.AppendLine($"""
            Open Forge cleanup
            Workspace: {Escape(workspace)}
            Selected by: {selectedBy}
            Mode: {CleanupWireVocabulary.Mode(result.Mode)}
            Result: {CleanupWireVocabulary.Status(result.Status)}
            Catalogue: {CleanupWireVocabulary.Coverage(result.Catalogue.Coverage)}
            Plan: {CleanupWireVocabulary.Safety(result.Plan.Safety)}
            Preflight: {CleanupWireVocabulary.Preflight(result.Preflight.State)}
            Lease: {CleanupWireVocabulary.Lease(result.Lease.State)}
            Revalidation: {CleanupWireVocabulary.Comparison(result.Revalidation.State)}
            Verification: {CleanupWireVocabulary.Verification(result.Verification.State)}
            """);
        if (result.Mode == CleanupMode.DryRun && !result.Plan.DeletionEntries.IsEmpty)
        {
            builder.AppendLine("Dry run: planned deletions require an application lease and final validation before removal.");
        }

        foreach (var effect in result.Effects)
        {
            var outcome = effect.Outcome == CleanupEffectOutcome.Verified
                ? "Removed and verified"
                : CleanupWireVocabulary.EffectOutcome(effect.Outcome);
            builder.AppendLine($"  {Escape(effect.Path)}: {outcome}; residual {CleanupWireVocabulary.EffectResidual(effect.Residual)}");
        }

        AppendObservedCandidates(builder, result);

        if (presentation.Presentation.View == CliView.Expanded)
        {
            foreach (var candidate in result.Catalogue.Candidates)
            {
                var kind = CleanupWireVocabulary.CandidateKind(candidate.Kind);
                var integrity = CleanupWireVocabulary.Integrity(candidate.Integrity);
                var eligibility = CleanupWireVocabulary.Eligibility(candidate.Eligibility);
                builder.AppendLine($"  {Escape(candidate.Path)}: {kind}, {integrity}, {eligibility}");
            }
        }

        foreach (var finding in result.Findings)
        {
            builder.AppendLine($"  {CleanupWireVocabulary.FindingCode(finding.Code)}: {Escape(finding.Cause)}");
            if (finding.Subject is { } subject)
            {
                builder.AppendLine($"    {Escape(subject)}");
            }
        }

        if (result.Next is { } next)
        {
            builder.AppendLine($"Next: {next.Command} ({Escape(next.Reason)})");
        }

        return builder.ToString().TrimEnd();
    }

    internal static string Diagnostic(CliPresentationRequest<CleanupResult> presentation)
        => string.Create(CultureInfo.InvariantCulture,
            $"cleanup: candidates={presentation.Result.Catalogue.Candidates.Length}; effects={presentation.Result.Effects.Length}; findings={presentation.Result.Findings.Length}");

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
