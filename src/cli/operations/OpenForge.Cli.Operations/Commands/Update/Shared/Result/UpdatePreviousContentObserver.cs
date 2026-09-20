using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Result;

internal static class UpdatePreviousContentObserver
{
    internal static bool Read(CliWorkspace? workspace, IReadOnlyList<UpdatePhysicalEffect> effects)
    {
        if (workspace is null || !effects.Any(effect => effect.Action == UpdatePhysicalEffectAction.Replace))
        {
            return false;
        }

        try
        {
            return Directory.Exists(Path.Combine(workspace.PhysicalRoot, ".git"));
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
