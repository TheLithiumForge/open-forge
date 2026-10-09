using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Models.Interaction;

internal sealed record ExtensionInstallConfirmationFacts(int ReplacementCount)
{
    internal static ExtensionInstallConfirmationFacts From(ExtensionInstallPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var replacementCount = plan.Effects
            .Where(effect => effect.Result.Kind == ExtensionInstallEffectKind.PackageFile
                && effect.Result.Action == ExtensionInstallEffectAction.Replace)
            .Select(effect => effect.Result.Path)
            .Distinct(StringComparer.Ordinal)
            .Count();
        return new(replacementCount);
    }
}
