using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Install.Models.Configuration;

/// <summary>
/// The optional first-installation question that registers one existing folder of the user's own docs as an
/// outside route. An empty answer skips it. Validation reuses the outside-route path rules so the folder never
/// overlaps the chosen main root, the fixed controls or the runtime folder.
/// </summary>
internal sealed record InstallOutsideRouteQuestion(string MainRoot)
{
    internal const string ControlsDirectory = WorkspaceLayout.DefaultMainRoot;

    internal static bool IsSkip(string value) => value.Length == 0;

    internal bool IsValidAnswer(string value)
    {
        if (IsSkip(value)) return true;
        try
        {
            return WorkspaceOutsideRoutePath.Validate(MainRoot, [value]) is [{ DirectoryPath: var canonical }]
                && string.Equals(canonical, value, StringComparison.Ordinal);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
