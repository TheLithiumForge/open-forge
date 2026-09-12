using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Rendering;

internal static class InstallHumanRenderer
{
    internal static string Render(CliPresentationRequest<InstallResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return Render(
            presentation.Result,
            presentation.Presentation.View == CliView.Expanded);
    }

    private static string Render(InstallResult result, bool expanded)
    {
        var facts = result.Facts;
        var builder = new StringBuilder();
        builder.AppendLine($"""
            Open Forge install
            Workspace: {Value(result.Workspace?.LexicalRoot)}
            Flags: mode={InstallDefinitions.ReadMachineName(result.Mode)}, force={MachineBoolean(result.Force)}, automatic={MachineBoolean(result.Automatic)}
            """.Replace("\n", Environment.NewLine, StringComparison.Ordinal));
        builder.AppendLine(facts.Source is null
            ? "Source: unavailable"
            : string.Create(
                CultureInfo.InvariantCulture,
                $"Source: {CommandTextEscaping.Escape(facts.Source.InventoryFingerprint)} / {facts.Source.AssetCount} assets"));
        builder.AppendLine(
            $"Classification: {(facts.Classification is { } classification ? InstallDefinitions.ReadMachineName(classification) : "unavailable")}");
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
                $"  {InstallDefinitions.ReadMachineName(finding.Code)}: {CommandTextEscaping.Escape(finding.Cause)}");
            if (finding.Subject is not null)
            {
                builder.AppendLine($"    Target: {CommandTextEscaping.Escape(finding.Subject)}");
            }
        }

        builder.AppendLine($"""
            Lifecycle: {InstallDefinitions.ReadMachineName(facts.Lifecycle.Action)} / {InstallDefinitions.ReadMachineName(facts.Lifecycle.Outcome)}
            Recovery: {InstallDefinitions.ReadMachineName(facts.Recovery.State)}{PathSuffix(facts.Recovery.ResidualPath)}
            Verification: {InstallDefinitions.ReadMachineName(facts.Verification.State)}
            Status: {Status(result.Status)}
            """.Replace("\n", Environment.NewLine, StringComparison.Ordinal));
        if (result.Next is { } next)
        {
            builder.AppendLine($"""
                Next: {CommandTextEscaping.Escape(next.Command)}
                  {CommandTextEscaping.Escape(next.Reason)}
                """.Replace("\n", Environment.NewLine, StringComparison.Ordinal));
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendEffect(
        StringBuilder builder,
        InstallEffect effect,
        bool expanded)
    {
        builder.Append(
            $"  {CommandTextEscaping.Escape(effect.Path)}: {InstallDefinitions.ReadMachineName(effect.Kind)} / {InstallDefinitions.ReadMachineName(effect.Action)} / {InstallDefinitions.ReadMachineName(effect.Outcome)} / {InstallDefinitions.ReadMachineName(effect.Residual)}");
        if (effect.SourceAssetPath is not null)
        {
            builder.Append(expanded
                ? $" / source={CommandTextEscaping.Escape(effect.SourceAssetPath)}"
                : $" / <- {CommandTextEscaping.Escape(effect.SourceAssetPath)}");
        }

        builder.AppendLine();
    }

    private static string Value(string? value)
        => value is null ? "unavailable" : CommandTextEscaping.Escape(value);

    private static string MachineBoolean(bool value) => value ? "true" : "false";

    private static string Status(CliSemanticStatus status)
        => status == CliSemanticStatus.Attention
            ? "requires attention"
            : CliStatusDefinitions.Read(status).MachineName;

    private static string PathSuffix(string? path)
        => path is null ? string.Empty : $" / {CommandTextEscaping.Escape(path)}";
}
