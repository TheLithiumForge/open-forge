using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Shared.Rendering;

internal static class ExtensionCreateHumanRenderer
{
    internal static string Render(CliPresentationRequest<ExtensionCreateResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var operation = result.Mode == ExtensionCreateMode.DryRun ? "Extension creation preview" : "Extension creation";
        var builder = new StringBuilder();
        builder.AppendLine($"""
            {CliHumanText.Outcome(operation, result.Status)}
            Status: {CliHumanText.Status(result.Status)}
            Package: {Text(result.StableId)}
            Catalogue: {Text(result.Catalogue)}
            Destination: {Text(result.Destination)}
            Mode: {ExtensionCreateDefinitions.ReadMachineName(result.Mode)}
            """);
        foreach (var finding in result.Findings)
        {
            builder.AppendLine($"{CliHumanText.Status(finding.Status).ToUpperInvariant()}: {Text(finding.Cause)} [{ExtensionCreateDefinitions.ReadFindingCode(finding.Code)}]");
            if (finding.Subject is { } subject)
            {
                builder.AppendLine($"  {Text(subject)}");
            }
        }

        AppendManifest(builder, result);
        builder.AppendLine(CultureInfo.InvariantCulture, $"Scaffold: {result.IntendedEffects.Count} intended; {result.AppliedEffects.Count} applied");
        var applied = result.AppliedEffects.ToHashSet();
        foreach (var effect in result.IntendedEffects.Concat(result.AppliedEffects).Distinct())
        {
            builder.AppendLine($"  {Text(effect.Path)}: {(applied.Contains(effect) ? "applied" : "intended; not applied")}");
            if (expanded)
            {
                builder.AppendLine($"    Creates: {ExtensionCreateDefinitions.ReadEffectKind(effect.Kind)}");
            }
        }

        var verification = result.Verification;
        builder.AppendLine($"Verification: catalogue {State(verification.Catalogue)}; destination {State(verification.Destination)}; manifest {State(verification.Manifest)}; content {State(verification.Payload)}");
        if (expanded && verification.Cause is { } cause)
        {
            builder.AppendLine(Text(cause));
        }

        builder.AppendLine("Workspace installation: unchanged");
        CliHumanText.AppendNext(builder, presentation);
        return builder.ToString().TrimEnd();
    }

    private static void AppendManifest(StringBuilder builder, ExtensionCreateResult result)
    {
        if (result.Manifest is not { } manifest)
        {
            builder.AppendLine("Manifest: unavailable");
            return;
        }

        builder.AppendLine($"""
            Manifest:
              Name: {Text(manifest.Name)}
              Description: {Text(manifest.Description)}
              Version: {Text(manifest.Version)}
              Dependencies: {(manifest.Dependencies.Count == 0 ? "none" : string.Join(", ", manifest.Dependencies.Select(Text)))}
            """);
    }

    private static string State(ExtensionCreateVerificationState value)
        => value == ExtensionCreateVerificationState.NotStarted ? "not checked" : ExtensionCreateDefinitions.ReadVerificationState(value);

    private static string Text(string? value) => CliHumanText.Text(value ?? "unavailable");
}
