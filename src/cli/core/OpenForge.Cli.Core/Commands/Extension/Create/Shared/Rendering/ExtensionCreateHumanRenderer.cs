using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Shared.Rendering;

internal static class ExtensionCreateHumanRenderer
{
    internal static string Render(CliPresentationRequest<ExtensionCreateResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        return presentation.Presentation.View switch
        {
            CliView.Compact => Compact(presentation.Result),
            CliView.Expanded => Expanded(presentation.Result),
            _ => throw new ArgumentOutOfRangeException(
                nameof(presentation),
                presentation.Presentation.View,
                "The Extension Create view is not defined."),
        };
    }

    private static string Compact(ExtensionCreateResult result)
    {
        var builder = new StringBuilder();
        var catalogue = Escape(result.Catalogue ?? "unresolved");
        var destination = Escape(result.Destination ?? "unresolved");
        var stableId = Escape(result.StableId ?? "unresolved");
        var mode = ExtensionCreateDefinitions.ReadMachineName(result.Mode);
        var status = CliStatusDefinitions.Read(result.Status).MachineName;
        builder.AppendLine(
            $"extension create: catalogue={catalogue}; destination={destination}; id={stableId}");
        builder.AppendLine($"Mode: {mode}; verification={Verification(result.Verification)}");
        builder.Append(
            $"Effects: intended={result.IntendedEffects.Count}; applied={result.AppliedEffects.Count}; workspace=unchanged; status={status}");
        AppendFindingsAndNext(builder, result);
        return builder.ToString();
    }

    private static string Expanded(ExtensionCreateResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Open Forge extension create");
        builder.AppendLine($"Catalogue: {Escape(result.Catalogue ?? "unresolved")}");
        builder.AppendLine($"Destination: {Escape(result.Destination ?? "unresolved")}");
        builder.AppendLine($"ID: {Escape(result.StableId ?? "unresolved")}");
        builder.AppendLine($"Mode: {ExtensionCreateDefinitions.ReadMachineName(result.Mode)}");
        if (result.Manifest is not null)
        {
            builder.AppendLine(
                $"Manifest: {Escape(result.Manifest.Name)}; {Escape(result.Manifest.Description)}; {Escape(result.Manifest.Version)}");
            builder.AppendLine(
                result.Manifest.Dependencies.Count == 0
                    ? "Dependencies: none"
                    : $"Dependencies: {string.Join(", ", result.Manifest.Dependencies.Select(Escape))}");
        }
        else
        {
            builder.AppendLine("Manifest: unresolved");
            builder.AppendLine("Dependencies: unresolved");
        }

        foreach (var effect in result.IntendedEffects)
        {
            builder.AppendLine($"Intended: {Escape(effect.Path)}");
        }

        foreach (var effect in result.AppliedEffects)
        {
            builder.AppendLine($"Applied: {Escape(effect.Path)}");
        }

        builder.AppendLine($"Verification: {Verification(result.Verification)}");
        builder.AppendLine("Workspace lifecycle: unchanged");
        builder.AppendLine($"Status: {CliStatusDefinitions.Read(result.Status).MachineName}");
        AppendFindingsAndNext(builder, result);
        return builder.ToString().TrimEnd();
    }

    private static void AppendFindingsAndNext(
        StringBuilder builder,
        ExtensionCreateResult result)
    {
        foreach (var finding in result.Findings)
        {
            EnsureNewLine(builder);
            var code = ExtensionCreateDefinitions.ReadFindingCode(finding.Code);
            var subject = Escape(finding.Subject ?? "unresolved");
            var cause = Escape(finding.Cause);
            builder.Append(
                $"Finding: {code}; subject={subject}; cause={cause}");
        }

        if (result.Next is not null)
        {
            EnsureNewLine(builder);
            builder.Append($"Next: {Escape(result.Next.Command)}; {Escape(result.Next.Reason)}");
        }
    }

    private static string Verification(ExtensionCreateVerification verification)
    {
        var catalogue = ExtensionCreateDefinitions.ReadVerificationState(verification.Catalogue);
        var destination = ExtensionCreateDefinitions.ReadVerificationState(verification.Destination);
        var manifest = ExtensionCreateDefinitions.ReadVerificationState(verification.Manifest);
        var payload = ExtensionCreateDefinitions.ReadVerificationState(verification.Payload);
        return $"catalogue={catalogue}, destination={destination}, manifest={manifest}, payload={payload}";
    }

    private static void EnsureNewLine(StringBuilder builder)
    {
        if (builder.Length > 0 && builder[^1] != '\n')
        {
            builder.AppendLine();
        }
    }

    private static string Escape(string value) => ExtensionCreateTextEscaping.Escape(value);
}
