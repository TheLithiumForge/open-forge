using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal static class InstallEffectOrdering
{
    internal static IEnumerable<InstallFileEffect> Files(IReadOnlyList<InstallFileEffect> targets, InstallFileEffect? ownership)
        => targets.Where(effect => effect.RelativePath != InstallIgnoreSection.Path)
            .Concat(ownership is null ? [] : [ownership])
            .Concat(targets.Where(effect => effect.RelativePath == InstallIgnoreSection.Path));
}
