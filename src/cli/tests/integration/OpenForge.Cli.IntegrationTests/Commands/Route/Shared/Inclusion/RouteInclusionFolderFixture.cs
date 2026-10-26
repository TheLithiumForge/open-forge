using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Shared.Destinations;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;

internal static class RouteInclusionFolderFixture
{
    internal const string Folder = "project/team notes";
    internal const string OtherFolder = "project/other";
    internal const string MainRoot = "docs/context";
    internal const string AuthoredCatalogue = "\uFEFF---\r\nopen-forge:\r\n  description: Authored policy\r\n  tags: [LoadNow, Authored]\r\n  applyTo: [private/**]\r\nforeign: keep\r\n---\r\n\r\n# Team\r\n\r\n## Axioms\r\n\r\n- Keep this rule.\r\n\r\n## Entries\r\n\r\n- [Guide](../../guide.md) - #Docs\r\n";
    internal static string Catalogue => WorkspaceRoutePath.CanonicalEntrypointPath(Folder);
    internal static CancellationToken Token => TestContext.Current.CancellationToken;

    internal static RouteInclusionWorkspace Create(bool custom, bool anchor = false)
    {
        var workspace = new RouteInclusionWorkspace();
        if (custom)
        {
            File.Delete(workspace.Files.Combine(SourceLogicalPath.LoaderPath));
            workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath,
                $$"""{"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{MainRoot}}","unrelated":true}""");
            workspace.Files.WriteText(Loader(workspace), GeneratedLoaderDocumentBuilder.Build(MarkdownEntriesSectionReader.EmptyEntry));
        }
        workspace.Files.CreateDirectory(Folder);
        if (anchor) workspace.Files.WriteText(Catalogue, AuthoredCatalogue);
        else workspace.ExpectCreate(Catalogue);
        return workspace;
    }

    internal static string Loader(RouteInclusionWorkspace workspace)
        => WorkspaceLayout.Create(workspace.Workspace, Settings(workspace).MainRoot).LoaderPath;

    internal static WorkspaceSettingsDocument Settings(RouteInclusionWorkspace workspace)
        => WorkspaceSettingsCodec.Read(workspace.Bytes(WorkspaceSettingsDefinitions.RelativePath)).Document
            ?? throw new InvalidOperationException("The fixture requires valid settings.");

    internal static RouteInclusionRequest Request(RouteInclusionWorkspace workspace, bool exclude = false)
        => new() { Workspace = workspace.Workspace, File = Folder, Action = exclude ? RouteInclusionAction.Exclude : RouteInclusionAction.Include };

    internal static void Rows(RouteInclusionWorkspace workspace, string rows)
        => workspace.Files.ReplaceText(Loader(workspace), GeneratedLoaderDocumentBuilder.Build(rows));

    internal static string Destination(RouteInclusionWorkspace workspace)
        => SourceDestinationEncoder.EncodePath(Path.GetRelativePath(Path.GetDirectoryName(workspace.Files.Combine(Loader(workspace)))!, workspace.Files.Combine(Catalogue)).Replace('\\', '/'));
}
