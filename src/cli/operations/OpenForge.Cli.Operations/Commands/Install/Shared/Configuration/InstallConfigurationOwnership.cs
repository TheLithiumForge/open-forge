using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Configuration;

internal static class InstallConfigurationOwnership
{
    internal static FrameworkOwnership Build(InstallEstablishmentPlanInput input, IReadOnlyList<InstallFileEffect> effects)
    {
        var context = input.Context;
        var configuration = context.Request.Configuration ?? throw new InvalidOperationException("Configuration is required.");
        var omitted = context.Payload.Assets.Where(asset => configuration.Routes.Any(row => row.Action == InstallRouteAction.Remove
            && InstallConfigurationChoices.Contains(row.Id, asset.Path)))
            .Select(asset => asset.Path).ToHashSet(StringComparer.Ordinal);
        var initialEstablishment = context.IntendedState.Configuration?.UsesInitialAdoption == true;
        var userOwnedPaths = context.IntendedState.UserOwnedPaths
            .Select(PortableWorkspacePath.CreatePortableKey).ToHashSet(StringComparer.Ordinal);
        var paths = (input.Ownership.Document.Framework?.Paths ?? [])
            .Concat(effects.Where(effect => effect.Identity.Kind == InstallEffectKind.File
                && (initialEstablishment || effect.Change.Kind == PlannedFileChangeKind.Create)
                && context.Payload.Find(effect.RelativePath) is not null
                && effect.RelativePath is not (FrameworkPayloadAsset.RootAgentPath or FrameworkPayloadAsset.RootClaudePath))
                .Select(effect => effect.RelativePath))
            .Concat(initialEstablishment ? context.IntendedState.PreservedEntrypointPaths : [])
            .Where(path => !initialEstablishment || !userOwnedPaths.Contains(PortableWorkspacePath.CreatePortableKey(path)))
            .Where(path => !omitted.Contains(path)).Distinct(StringComparer.Ordinal).ToImmutableArray();
        var writtenRegions = initialEstablishment
            ? context.IntendedState.GeneratedRegionPaths.Select(path => new OwnedRegion(path, "entries"))
                .Concat(context.IntendedState.ManagedBlockBytes.Keys.Select(path =>
                    new OwnedRegion(path, WorkspaceOwnershipDefinitions.ManagedBlockRegion)))
            : effects.Where(effect => context.IntendedState.GeneratedRegionPaths.Contains(effect.RelativePath))
            .Select(effect => new OwnedRegion(effect.RelativePath, "entries"))
            .Concat(effects.Where(effect => context.IntendedState.ManagedBlockBytes.ContainsKey(effect.RelativePath))
                .Select(effect => new OwnedRegion(effect.RelativePath, WorkspaceOwnershipDefinitions.ManagedBlockRegion)));
        var regions = (input.Ownership.Document.Framework?.Regions ?? []).Concat(writtenRegions)
            .Where(region => !omitted.Contains(region.Path)).Distinct().ToImmutableArray();
        return new(new("embedded-framework", null), paths, regions)
        { GitIgnoredRoutes = context.IntendedState.Configuration?.GitIgnoredRoutes ?? [] };
    }
}
