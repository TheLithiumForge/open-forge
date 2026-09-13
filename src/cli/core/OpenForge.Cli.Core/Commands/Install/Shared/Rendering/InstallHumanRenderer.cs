using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Rendering;

internal static class InstallHumanRenderer
{
    internal static string Render(CliPresentationRequest<InstallResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var facts = result.Facts;
        var style = CliHumanStyle.For(presentation);
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, "Open Forge install");
        builder.AppendLine($"Mode: {InstallDefinitions.ReadMachineName(result.Mode)}; force: {MachineBoolean(result.Force)}; automatic: {MachineBoolean(result.Automatic)}");
        builder.AppendLine(facts.Source is null
            ? "Source: unavailable"
            : string.Create(
                CultureInfo.InvariantCulture,
                $"Source: embedded Framework; {facts.Source.AssetCount} assets"));
        if (expanded && facts.Source is { } source)
        {
            builder.AppendLine($"  Inventory fingerprint: {Value(source.InventoryFingerprint)}");
        }

        builder.AppendLine(
            $"Installation state: {Classification(facts.Classification)}");
        builder.AppendLine(facts.Footprint is null
            ? "Footprint: unavailable"
            : string.Create(
                CultureInfo.InvariantCulture,
                $"Footprint: {facts.Footprint.PayloadFiles} payload files, {facts.Footprint.ManagedRegions} managed regions, {facts.Footprint.GeneratedRegions} generated regions"));
        builder.AppendLine(string.Create(
            CultureInfo.InvariantCulture,
            $"Effects: {facts.Effects.Count}"));
        foreach (var effect in facts.Effects)
        {
            AppendEffect(builder, effect, expanded);
        }

        builder.AppendLine(string.Create(
            CultureInfo.InvariantCulture,
            $"Findings: {result.Findings.Count}"));
        foreach (var finding in result.Findings)
        {
            builder.AppendLine(
                $"{style.Finding(finding.Status)}: {CommandTextEscaping.Escape(finding.Cause)} [{InstallDefinitions.ReadMachineName(finding.Code)}]");
            if (finding.Subject is not null)
            {
                builder.AppendLine($"    Target: {CommandTextEscaping.Escape(finding.Subject)}");
            }
        }

        builder.AppendLine($"""
            Lifecycle: {InstallDefinitions.ReadMachineName(facts.Lifecycle.Action)} / {InstallDefinitions.ReadMachineName(facts.Lifecycle.Outcome)}
            Recovery: {InstallDefinitions.ReadMachineName(facts.Recovery.State)}{PathSuffix(facts.Recovery.ResidualPath)}
            Verification: {InstallDefinitions.ReadMachineName(facts.Verification.State)}
            """.Replace("\n", Environment.NewLine, StringComparison.Ordinal));
        CliHumanText.AppendNext(builder, presentation);

        return builder.ToString().TrimEnd();
    }

    private static void AppendEffect(
        StringBuilder builder,
        InstallEffect effect,
        bool expanded)
    {
        builder.Append(
            $"  {CommandTextEscaping.Escape(effect.Path)}: {InstallDefinitions.ReadMachineName(effect.Action)} {InstallDefinitions.ReadMachineName(effect.Kind)}; {InstallDefinitions.ReadMachineName(effect.Outcome)}; residual: {InstallDefinitions.ReadMachineName(effect.Residual)}");
        if (expanded && effect.SourceAssetPath is not null)
        {
            builder.Append($"; source: {CommandTextEscaping.Escape(effect.SourceAssetPath)}");
        }

        builder.AppendLine();
    }

    private static string Classification(InstallManagementClassification? value) => value switch
    {
        null => "unavailable",
        InstallManagementClassification.SafeAbsence => "no existing installation content",
        InstallManagementClassification.TrustedExact => "matches the installed Framework",
        InstallManagementClassification.ManagedDivergence => "local changes to managed content",
        InstallManagementClassification.EligibleInitialOccupant => "existing content at installation paths",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The installation state is not defined."),
    };

    private static string Value(string? value)
        => value is null ? "unavailable" : CommandTextEscaping.Escape(value);

    private static string MachineBoolean(bool value) => value ? "true" : "false";

    private static string PathSuffix(string? path)
        => path is null ? string.Empty : $" / {CommandTextEscaping.Escape(path)}";
}
