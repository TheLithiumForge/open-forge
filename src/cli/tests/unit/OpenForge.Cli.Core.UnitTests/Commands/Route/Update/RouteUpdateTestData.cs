using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Update;

internal static partial class RouteUpdateTestData
{
    internal const string ParentId = "memory";
    internal const string ParentPath = ".agents/memory/_memory.md";
    internal const string TargetId = "memory/topic";
    internal const string TargetPath = ".agents/memory/topic.md";
    internal const string TemplateId = "templates/topic";
    internal const string TemplatePath = ".agents/templates/topic.md";

    internal const string TargetText = """
        ---
        open-forge:
          description: Before
          responsibility: Before responsibility
          tags: [Memory, Before]
          custom: preserve-me
        ---

        # Authored body

        Keep this body.
        """;

    internal static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(
            Path.Combine(Path.GetTempPath(), "open-forge-route-update-contract"));
        return new CliWorkspace(
            root,
            root,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    internal static CliInvocation Invocation(CliWorkspace? workspace = null)
    {
        var selectedWorkspace = workspace ?? Workspace();
        return new CliInvocation(
            Process: new CliProcessIdentity("open-forge", "test"),
            Presentation: new CliPresentation(
                CliOutputFormat.Json,
                CliView.Expanded,
                CliVerbosity.Normal),
            TerminalMode: CliTerminalMode.None,
            WorkspaceRequest: new CliWorkspaceRequest(
                selectedWorkspace.LexicalRoot,
                selectedWorkspace.LexicalRoot),
            Workspace: selectedWorkspace);
    }

    internal static RouteUpdateRequest Request(
        RouteUpdatePatchRequest? patch = null,
        string? templateReference = null,
        RouteUpdateMode mode = RouteUpdateMode.Apply,
        CliWorkspace? workspace = null)
        => new(
            workspace: workspace ?? Workspace(),
            sourceReference: TargetId,
            patch: patch ?? DescriptionPatch("After"),
            templateReference: templateReference,
            mode: mode);

    internal static RouteUpdatePatchRequest DescriptionPatch(string value)
        => Patch(
            description: new RouteUpdateDescriptionRequest
            {
                Requested = true,
                Value = value,
            });

    internal static RouteUpdatePatchRequest ResponsibilityPatch(string? value)
        => Patch(
            responsibility: new RouteUpdateResponsibilityRequest
            {
                Operation = value is null
                    ? RouteUpdateResponsibilityOperation.Remove
                    : RouteUpdateResponsibilityOperation.Set,
                Value = value,
            });

    internal static RouteUpdatePatchRequest TagsPatch(params string[] values)
        => Patch(
            tags: new RouteUpdateTagsRequest
            {
                Requested = true,
                Values = ImmutableArray.CreateRange(values),
            });

    internal static RouteUpdatePatchRequest Patch(
        RouteUpdateDescriptionRequest? description = null,
        RouteUpdateResponsibilityRequest? responsibility = null,
        RouteUpdateTagsRequest? tags = null)
        => new()
        {
            Description = description ?? new RouteUpdateDescriptionRequest
            {
                Requested = false,
                Value = null,
            },
            Responsibility = responsibility ?? new RouteUpdateResponsibilityRequest
            {
                Operation = RouteUpdateResponsibilityOperation.NotRequested,
                Value = null,
            },
            Tags = tags ?? new RouteUpdateTagsRequest
            {
                Requested = false,
                Values = [],
            },
        };

    internal static RouteUpdateMetadataPatcher MetadataPatcher()
    {
        var yamlParser = new YamlDocumentParser();
        return new RouteUpdateMetadataPatcher(
            new RouteUpdateMetadataLayoutReader(),
            new RouteUpdateMetadataEditPlanner(
                new FrameworkDocumentMetadataEmitter(),
                yamlParser),
            new RouteUpdateMetadataByteEditor());
    }
}
