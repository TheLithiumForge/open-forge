using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;

internal static class ExtensionInspectPathsHumanRenderer
{
    internal static void Append(StringBuilder builder, CliPresentationRequest<ExtensionInspectResult> presentation)
    {
        var result = presentation.Result;
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var comparisons = result.Comparison.Paths.ToLookup(item => item.Path, StringComparer.Ordinal);
        var findings = result.Findings.Where(item => item.Path is not null).ToLookup(item => item.Path, StringComparer.Ordinal);
        var declared = result.PathFacts.Declared.ToLookup(item => item.Path, StringComparer.Ordinal);
        var current = result.PathFacts.Current.ToLookup(item => item.Path, StringComparer.Ordinal);
        var payload = (result.Available.Package?.Payload ?? []).Where(item => item.TargetPath is not null).ToLookup(item => item.TargetPath, StringComparer.Ordinal);
        var baseline = result.Comparison.Baseline.Fingerprints.ToLookup(item => item.Path, StringComparer.Ordinal);
        var currentFingerprints = result.Comparison.Current.Fingerprints.ToLookup(item => item.Path, StringComparer.Ordinal);
        var intended = result.Comparison.Intended.Fingerprints.ToLookup(item => item.Path, StringComparer.Ordinal);
        var installed = (result.Installed.Package?.Paths ?? []).ToHashSet(StringComparer.Ordinal);
        var paths = comparisons.Select(group => group.Key)
            .Concat(findings.Select(group => group.Key).OfType<string>())
            .Concat(declared.Select(group => group.Key)).Concat(current.Select(group => group.Key))
            .Concat(payload.Select(group => group.Key).OfType<string>()).Concat(installed)
            .Concat(baseline.Select(group => group.Key)).Concat(currentFingerprints.Select(group => group.Key))
            .Concat(intended.Select(group => group.Key))
            .Distinct(StringComparer.Ordinal);
        builder.AppendLine($"Path checks: {ExtensionInspectWireVocabulary.PathState(result.PathFacts.State)}");
        var summarized = 0;
        foreach (var path in paths)
        {
            if (!expanded && comparisons[path].Any()
                && comparisons[path].All(item => item.Relation == ExtensionInspectPathRelation.Unchanged
                    || (item.Relation == ExtensionInspectPathRelation.NotApplicable && result.Comparison.Mode == ExtensionInspectComparisonMode.AvailableOnly))
                && !findings[path].Any()
                && declared[path].All(item => item.State == ExtensionInspectDeclaredPathState.Available)
                && current[path].All(item => item.State == ExtensionInspectCurrentPathState.Present)
                && payload[path].All(item => item.State == ExtensionInspectPackageFileState.Available))
            {
                summarized++;
                continue;
            }

            builder.AppendLine($"  {Value(path)}");
            ExtensionInspectFindingHumanRenderer.Append(builder, findings[path]);
            foreach (var comparison in comparisons[path])
            {
                builder.AppendLine($"    Comparison: {Relation(comparison.Relation)}");
                if (expanded || comparison.Relation == ExtensionInspectPathRelation.Shared)
                {
                    builder.AppendLine($"""
                        Owners at installation: {ExtensionHumanText.Values(comparison.BaselineOwners)}
                        Current owners: {ExtensionHumanText.Values(comparison.CurrentOwners)}
                        Intended owners: {ExtensionHumanText.Values(comparison.IntendedOwners)}
                    """);
                }

                if (expanded)
                {
                    AppendFingerprint(builder, "Installed baseline", comparison.Baseline);
                    AppendFingerprint(builder, "Current workspace", comparison.Current);
                    AppendFingerprint(builder, "Selected package", comparison.Intended);
                }
            }

            if (installed.Contains(path) && (expanded || !comparisons[path].Any()))
            {
                builder.AppendLine("    Recorded in the installed package.");
            }

            if (expanded)
            {
                AppendUnmatchedFingerprints(builder, "Installed baseline", baseline[path], comparisons[path].Select(item => item.Baseline));
                AppendUnmatchedFingerprints(builder, "Current workspace", currentFingerprints[path], comparisons[path].Select(item => item.Current));
                AppendUnmatchedFingerprints(builder, "Selected package", intended[path], comparisons[path].Select(item => item.Intended));
            }

            foreach (var observation in declared[path])
            {
                var sharedSource = payload[path].Any(file => file.Path == observation.SourcePath);
                var source = sharedSource ? string.Empty : $"; source {Value(observation.SourcePath)}";
                builder.AppendLine($"    Declared path: {ExtensionInspectWireVocabulary.DeclaredPathState(observation.State)}{source}");
            }

            foreach (var observation in current[path])
            {
                builder.AppendLine($"    Current path: {ExtensionInspectWireVocabulary.CurrentPathState(observation.State)}");
                if (expanded)
                {
                    builder.AppendLine(CultureInfo.InvariantCulture, $"      Physical path: {Value(observation.PhysicalIdentity)}; bytes: {observation.ByteLength?.ToString(CultureInfo.InvariantCulture) ?? "unknown"}; SHA-256: {Value(observation.ExactSha256)}");
                }
            }

            foreach (var file in payload[path])
            {
                builder.AppendLine($"    Selected package file: {Value(file.Path)}; {ExtensionInspectWireVocabulary.PackageFileState(file.State)}");
                if (expanded)
                {
                    builder.AppendLine($"      SHA-256: {Value(file.Sha256)}");
                }
            }
        }

        if (summarized > 0)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"  Paths summarized: {summarized}. Use --view expanded for their observations.");
        }

        foreach (var file in (result.Available.Package?.Payload ?? []).Where(item => item.TargetPath is null))
        {
            builder.AppendLine($"  Package source file {Value(file.Path)}: {ExtensionInspectWireVocabulary.PackageFileState(file.State)}; destination unavailable");
        }
    }

    private static void AppendUnmatchedFingerprints(StringBuilder builder, string label,
        IEnumerable<ExtensionInspectFingerprintFact> facts, IEnumerable<ExtensionInspectFingerprint?> comparisons)
    {
        var compared = comparisons.ToHashSet();
        foreach (var fact in facts)
        {
            if (!compared.Contains(fact.Fingerprint))
            {
                AppendFingerprint(builder, label, fact.Fingerprint);
            }
        }
    }

    private static void AppendFingerprint(StringBuilder builder, string label, ExtensionInspectFingerprint? fingerprint)
    {
        if (fingerprint is null)
        {
            builder.AppendLine($"    {label}: fingerprint unavailable");
            return;
        }

        builder.AppendLine($"    {label}: {ExtensionInspectWireVocabulary.FingerprintKind(fingerprint.Kind)}; SHA-256 {Value(fingerprint.Sha256)}; policy {Value(fingerprint.Policy)}; {ExtensionInspectWireVocabulary.FingerprintOrigin(fingerprint.Origin)}");
    }

    internal static string Relation(ExtensionInspectPathRelation value) => value switch
    {
        ExtensionInspectPathRelation.NotStarted => "not compared",
        ExtensionInspectPathRelation.NotApplicable => "not applicable",
        ExtensionInspectPathRelation.Changed => "selected package differs; workspace still matches the installed baseline",
        ExtensionInspectPathRelation.CurrentDiverged => "workspace differs from the installed baseline",
        ExtensionInspectPathRelation.Retired => "not in the selected package",
        ExtensionInspectPathRelation.New => "new in the selected package",
        ExtensionInspectPathRelation.Shared => "shared by compatible package owners",
        _ => ExtensionInspectWireVocabulary.PathRelation(value),
    };

    private static string Value(string? value) => ExtensionHumanText.Value(value);
}
