using OpenForge.Cli.Core.Commands.Status.Models.Presentation;
using OpenForge.Cli.Core.Commands.Status.Models.Result;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Rendering;

internal static class StatusJsonLifecycleProjection
{
    internal static StatusJsonLifecycle Create(StatusLifecycle lifecycle)
        => new()
        {
            Framework = new StatusJsonFrameworkLifecycle
            {
                State = StatusWireVocabulary.LifecycleState(lifecycle.Framework.State),
                SourceAvailability = StatusWireVocabulary.SourceAvailability(lifecycle.Framework.SourceAvailability),
                Targets = lifecycle.Framework.Targets.Select(target => new StatusJsonFrameworkTarget
                {
                    Path = target.Path,
                    Kind = StatusWireVocabulary.FrameworkTargetKind(target.Kind),
                    SourceAssetPath = target.SourceAssetPath,
                    Region = target.Region,
                    BaselineFingerprint = target.BaselineFingerprint,
                    FingerprintKind = target.FingerprintKind,
                    State = StatusWireVocabulary.TargetState(target.State),
                }).ToArray(),
            },
            Extensions = Extension(lifecycle.Extensions),
        };

    private static StatusJsonExtensionLifecycle Extension(StatusExtensionLifecycle extensions)
        => new()
        {
            State = StatusWireVocabulary.LifecycleState(extensions.State),
            SourceAvailability = StatusWireVocabulary.SourceAvailability(extensions.SourceAvailability),
            Installed = extensions.Installed.Select(item => new StatusJsonInstalledExtension
            {
                Id = item.Id,
                Version = item.Version,
                Source = item.Source,
                SourceAvailability = StatusWireVocabulary.SourceAvailability(item.SourceAvailability),
                Dependencies = item.Dependencies.ToArray(),
                Paths = item.Paths.ToArray(),
            }).ToArray(),
            ManagedFiles = new StatusJsonManagedExtensionFiles
            {
                Counts = Counts(extensions.ManagedFiles.Counts),
                Targets = extensions.ManagedFiles.Targets.Select(target => new StatusJsonExtensionTarget
                {
                    Path = target.Path,
                    Owners = target.Owners.ToArray(),
                    BaselineFingerprint = target.BaselineFingerprint,
                    FingerprintKind = target.FingerprintKind,
                    State = StatusWireVocabulary.TargetState(target.State),
                }).ToArray(),
            },
        };

    private static StatusJsonManagedTargetCounts Counts(StatusManagedTargetCounts counts)
        => new()
        {
            Current = StatusJsonContextProjection.Value(counts.Current),
            Changed = StatusJsonContextProjection.Value(counts.Changed),
            Missing = StatusJsonContextProjection.Value(counts.Missing),
            Unavailable = StatusJsonContextProjection.Value(counts.Unavailable),
            Blocked = StatusJsonContextProjection.Value(counts.Blocked),
        };
}
