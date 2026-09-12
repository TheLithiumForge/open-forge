using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Presentation;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Presentation.Envelope;
using OpenForge.Cli.Core.Commands.Library.Shared.Rendering.Coordinates;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Shared.Rendering;

internal static class LibraryDetachPresentation
{
    internal static CliHelpContent CreateHelp() => new(
    [
        new CliHelpSection("Syntax", """
              open-forge library detach <library-id> [--dry-run] [global options]
            """),
        new CliHelpSection("Write policy", """
              Remove the registered destination links while preserving source files. Use --dry-run to preview all changes without writing.
            """),
        new CliHelpSection("Examples", """
              open-forge library detach shared --dry-run
              open-forge library detach shared
            """),
    ]);
    internal static string RenderHuman(CliPresentationRequest<LibraryDetachResult> presentation)
        => Render(presentation, "detach");
    internal static string RenderJson(CliPresentationRequest<LibraryDetachResult> presentation)
    {
        Validate(presentation);
        var result = presentation.Result;
        return JsonSerializer.Serialize(new LibraryDetachJsonDocument
        {
            SchemaVersion = 1,
            Command = result.Command,
            Status = result.Status,
            Workspace = Workspace(result.Workspace),
            Result = result.Result,
            Next = result.Next is null ? null : new LibraryJsonNext
            {
                Command = result.Next.Command,
                Reason = result.Next.Reason,
            },
        }, LibraryDetachJsonContext.Default.LibraryDetachJsonDocument);
    }

    private static string Render(CliPresentationRequest<LibraryDetachResult> presentation, string leaf)
    {
        Validate(presentation);
        var result = presentation.Result;
        var builder = new StringBuilder();
        builder.Append("Library ").Append(leaf).Append(": id=")
            .Append(Escape(result.Result.Identity.LibraryId ?? "unavailable"))
            .Append("; mode=").Append(LibraryModeConverter.ReadWireValue(result.Result.Identity.Mode))
            .Append("; plan=").Append(LibraryPlanStateConverter.ReadWireValue(result.Result.Plan.State))
            .Append("; status=").AppendLine(CliStatusDefinitions.Read(result.Status).MachineName);
        foreach (var finding in result.Result.Findings)
        {
            builder.Append("Finding: ").Append(LibraryDetachDefinitions.ReadFindingCode(finding.Code))
                .Append("; path=").Append(Escape(finding.Path ?? "unavailable"))
                .Append("; ").AppendLine(Escape(finding.Cause));
        }

        builder.Append("Status: ").Append(CliStatusDefinitions.Read(result.Status).MachineName);
        return builder.ToString();
    }

    private static LibraryJsonWorkspace? Workspace(CliWorkspace? workspace)
        => workspace is null ? null : new LibraryJsonWorkspace
        {
            Path = workspace.LexicalRoot,
            SelectedBy = workspace.SelectedBy switch
            {
                CliWorkspaceSelectionMethod.CurrentDirectory => "current-directory",
                CliWorkspaceSelectionMethod.ExplicitWorkspace => "explicit-workspace",
                _ => throw new ArgumentOutOfRangeException(nameof(workspace), workspace.SelectedBy, "The workspace selection is not defined."),
            },
        };

    private static void Validate(CliPresentationRequest<LibraryDetachResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
    }

    private static string Escape(string value) => value.Replace('\r', ' ').Replace('\n', ' ');
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    Converters = new[]
    {
        typeof(LibraryApplicationStateConverter),
        typeof(LibraryCollisionKindConverter),
        typeof(LibraryComparisonRelationConverter),
        typeof(LibraryDetachFindingCodeConverter),
        typeof(LibraryExpectedStateKindConverter),
        typeof(LibraryLinkEffectKindConverter),
        typeof(LibraryLinkViewStateConverter),
        typeof(LibraryModeConverter),
        typeof(LibraryMutationRecordStateConverter),
        typeof(LibraryOwnershipKindConverter),
        typeof(LibraryPlanStateConverter),
        typeof(LibraryRecordEffectConverter),
        typeof(LibraryRecordPublicationStateConverter),
        typeof(LibraryRecoveryStateConverter),
        typeof(LibraryResidualKindConverter),
        typeof(LibraryResidualStateConverter),
        typeof(LibrarySemanticStatusConverter),
        typeof(LibraryVerificationStateConverter),
    })]
[JsonSerializable(typeof(LibraryDetachJsonDocument))]
internal sealed partial class LibraryDetachJsonContext : JsonSerializerContext;
