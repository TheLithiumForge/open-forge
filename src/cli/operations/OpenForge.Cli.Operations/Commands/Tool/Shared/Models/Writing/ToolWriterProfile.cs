using OpenForge.Cli.Core.Commands.Tool.Models.Provider;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Execution;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;

internal sealed record ToolWriterProfileContext(
    CliWorkspace Workspace, ToolDefinition Definition, WorkspaceToolSetting Setting, ToolStorePaths StorePaths, string InstallRoot, string OpenForge);

internal sealed record ToolWriterSkill(string Path, byte[] Bytes);

internal sealed record ToolWriterProfile
{
    public required ToolCommandValues Values { get; init; }
    public required Dictionary<string, string> Environment { get; init; }
    public ToolWriterSkill? Skill { get; init; }
    public ToolInstruction[] Instructions { get; init; } = [];
}

internal enum ToolWriterProfileStage { Preflight, Artifacts, Verify }
internal delegate ValueTask<ToolVerification[]> ToolObserveWriterProfile(ToolWriterProfileContext context, ToolWriterProfileStage stage, CancellationToken token);

internal sealed record ToolWriterAdapter(Func<ToolWriterProfileContext, ToolWriterProfile> Describe, ToolObserveWriterProfile Observe);
