using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Rendering;

internal static class FindNextHumanRenderer
{
    internal static string? Line(FindResult result)
    {
        if (result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention)
        {
            if (result.Next is not null)
            {
                throw new InvalidOperationException("A complete Find result cannot carry a next action.");
            }
            return null;
        }
        if (result.Next is null)
        {
            throw new InvalidOperationException("A non-terminal Find status requires a typed next action.");
        }
        return $"Next: {FindTextEscaping.Escape(result.Next.Command)}";
    }
}
