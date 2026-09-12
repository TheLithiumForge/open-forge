using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Shared.Templates.Models;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Update;

internal static partial class RouteUpdateTestData
{
    internal static RouteUpdateObservation Observation(
        RouteUpdatePatchRequest? patch = null,
        string text = TargetText,
        string? templateReference = null)
    {
        var workspace = Workspace();
        var source = Source(workspace, TargetId, TargetPath);
        var markdown = new MarkdownDocumentParser().Parse(text);
        var yamlSpan = markdown.Frontmatter.YamlSpan
            ?? throw new InvalidOperationException(
                "A Route Update Unit observation requires frontmatter YAML.");
        var frontmatter = new YamlDocumentParser().Parse(
            text[yamlSpan.Start..yamlSpan.End]);
        return new RouteUpdateObservation
        {
            Request = Request(
                patch,
                templateReference,
                workspace: workspace),
            Target = Target(),
            Catalogue = Catalogue(workspace, source),
            TargetSource = source,
            TargetSnapshot = FileStateSnapshot.File(
                source.Base.PhysicalPath,
                source.Base.PhysicalPath,
                Encoding.UTF8.GetBytes(text)),
            OverwriteSnapshot = null,
            TargetText = text,
            Markdown = markdown,
            Frontmatter = frontmatter,
        };
    }

    internal static RouteUpdateMetadataPatch MetadataPlan(
        RouteUpdateObservation? observation = null,
        string intendedText = TargetText)
    {
        var observed = observation ?? Observation();
        return new RouteUpdateMetadataPatch
        {
            Observation = observed,
            Patch = PatchFacts(
                expectedDescription: "After",
                descriptionState: RouteUpdatePatchState.Changed),
            IntendedTargetBytes = ImmutableArray.CreateRange(
                Encoding.UTF8.GetBytes(intendedText)),
            Preview =
            [
                new RouteUpdatePreviewHunk
                {
                    Kind = RouteUpdatePreviewKind.MetadataField,
                    Before = "description: Before",
                    Expected = "description: After",
                },
            ],
        };
    }

    internal static RouteUpdateBodyPlan BodyPlan(
        RouteUpdateObservation? observation = null,
        RouteUpdateTemplate? template = null,
        RouteUpdateBodyState state = RouteUpdateBodyState.Preserved,
        string intendedText = TargetText)
        => new()
        {
            Metadata = MetadataPlan(observation, intendedText),
            Template = template,
            State = state,
            IntendedTargetBytes = ImmutableArray.CreateRange(
                Encoding.UTF8.GetBytes(intendedText)),
            Preview = [],
        };

    internal static RouteTemplateResolution ResolvedTemplate(
        CliWorkspace? workspace = null,
        string body = "# Template body\n")
    {
        var selectedWorkspace = workspace ?? Workspace();
        var source = Source(selectedWorkspace, TemplateId, TemplatePath);
        var bytes = ImmutableArray.CreateRange(Encoding.UTF8.GetBytes(body));
        return new RouteTemplateResolution
        {
            State = RouteTemplateResolutionState.Resolved,
            Template = new RouteTemplateSelection
            {
                Requested = TemplateId,
                Id = TemplateId,
                Path = TemplatePath,
                BodyByteLength = bytes.Length,
            },
            Source = source,
            BodyBytes = bytes,
            Issue = null,
            Cause = null,
            Target = null,
        };
    }

    internal static RouteUpdateDestinationPlan DestinationPlan()
    {
        var body = BodyPlan();
        return new RouteUpdateDestinationPlan
        {
            Body = body,
            IntendedTargetBytes = body.IntendedTargetBytes,
            IntendedMetadata = SourceAuthoredMetadataFacts.Complete(
                "After",
                ["Memory", "Before"]),
        };
    }

    internal static RouteUpdateNavigationPlan NavigationPlan(
        RouteUpdateObservation? observation = null)
    {
        var observed = observation ?? Observation();
        return new RouteUpdateNavigationPlan
        {
            Formation = new GeneratedNavigationFormationBuilder().Build(
                observed.Catalogue),
            Regions = [],
        };
    }

    private static SourceCatalogue Catalogue(
        CliWorkspace workspace,
        SourceLogicalSource source)
        => new(
            workspace: workspace,
            candidates: [Candidate(source)],
            sources: [source],
            issues: [],
            isCancelled: false);

    private static SourceLogicalSource Source(
        CliWorkspace workspace,
        string id,
        string path)
    {
        var physicalPath = Path.GetFullPath(Path.Combine(
            workspace.LexicalRoot,
            path.Replace('/', Path.DirectorySeparatorChar)));
        return new SourceLogicalSource(
            new SourceLogicalIdentity(id, path),
            new SourceLayer(
                canonicalPath: path,
                physicalPath: physicalPath,
                form: SourceDocumentForm.Markdown,
                kind: SourceLayerKind.Base));
    }

    private static SourceCandidate Candidate(SourceLogicalSource source)
        => new(
            canonicalPath: source.Base.CanonicalPath,
            form: source.Base.Form,
            automaticId: source.Identity.AutomaticId,
            physicalState: PhysicalPathState.Contained,
            physicalPath: source.Base.PhysicalPath,
            physicalParentPath: Path.GetDirectoryName(source.Base.PhysicalPath)
                ?? throw new InvalidOperationException(
                    "A Route Update source fixture requires a physical parent."));
}
