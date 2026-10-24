using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Install.Models.Configuration;

internal sealed record InstallMainRootQuestion(string Initial)
{
    internal const string DefaultRoot = WorkspaceLayout.DefaultMainRoot;
    internal const string SettingsDirectory = WorkspaceSettingsDefinitions.DirectoryName;

    internal bool IsValidAnswer(string value) => WorkspaceMainRootPath.IsCanonical(value);
}
