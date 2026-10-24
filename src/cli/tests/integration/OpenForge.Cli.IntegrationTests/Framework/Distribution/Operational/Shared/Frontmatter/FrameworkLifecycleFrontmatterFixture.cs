using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Content;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.IntegrationTests.Framework.Distribution.Operational.Shared.Frontmatter;

internal static class FrameworkLifecycleFrontmatterFixture
{
    internal const string TargetPath = ".agents/memory/_memory.md";

    internal static void Write(TemporaryWorkspace workspace, FrontmatterForm form)
    {
        var payload = EmbeddedFrameworkPayloadReader.Read().Payload
            ?? throw new InvalidOperationException("The embedded Framework payload is unavailable.");
        var asset = payload.Assets.Single(asset => asset.Path == TargetPath);
        var rendered = WorkspacePayloadRenderer.Render(DefaultWorkspaceLayout, TargetPath, asset.Bytes.AsMemory(), form);
        var bytes = rendered.Bytes ?? throw new InvalidOperationException(rendered.Cause);
        workspace.WriteBytes(TargetPath, bytes.ToArray());
        workspace.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath,
            WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with
            {
                Framework = new FrameworkOwnership(new("test-framework", "1.0.0"), [TargetPath], []),
            }));
    }

    private static readonly WorkspaceLayout DefaultWorkspaceLayout = WorkspaceLayout.Create(
            new CliWorkspace(
                Path.GetFullPath(nameof(FrameworkLifecycleFrontmatterFixture)),
                Path.GetFullPath(nameof(FrameworkLifecycleFrontmatterFixture)),
                CliWorkspaceSelectionMethod.ExplicitWorkspace),
            WorkspaceLayout.DefaultMainRoot);
}
