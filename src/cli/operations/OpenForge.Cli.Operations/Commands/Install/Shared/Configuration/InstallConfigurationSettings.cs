using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Configuration;

internal static class InstallConfigurationSettings
{
    internal static PlannedFileChange? Plan(WorkspaceLayout layout, WorkspaceSettingsRead settings,
        InstallConfiguration? configuration, InstallFrontmatterSelection? frontmatter, string? keptGuide = null)
    {
        var snapshot = settings.Snapshot ?? throw new InvalidOperationException("Install settings require their observed snapshot.");
        var change = PlanChoices(layout, settings, configuration, frontmatter, keptGuide);
        ReadOnlyMemory<byte> bytes = change?.IntendedBytes.ToArray() ?? snapshot.Bytes.ToArray();
        bytes = WorkspaceSettingsCodec.SetMainRoot(bytes, layout.MainRoot) ?? bytes;
        var intended = bytes.IsEmpty ? WorkspaceSettingsDocument.Empty : WorkspaceSettingsCodec.Read(bytes).Document
            ?? throw new InvalidOperationException("Planned Install settings must decode.");
        var archive = layout.MapPayloadPath(WorkspaceSettingsDefinitions.MemoryArchivePath);
        var entrypoint = $"{archive}/{SourceFormClassifier.CanonicalEntrypointFileName(SourceLogicalPath.ReadFileName(archive))}";
        if (FrameworkPayloadSelection.IncludesPath(layout, entrypoint, intended))
            bytes = WorkspaceSettingsCodec.SeedArchiveLinkScope(bytes, archive) ?? bytes;
        if (snapshot.Bytes.AsSpan().SequenceEqual(bytes.Span)) return null;
        return snapshot.Kind == FileExpectationKind.Missing
            ? PlannedFileChange.Create(snapshot.Expectation, bytes.ToArray())
            : PlannedFileChange.Replace(snapshot.Expectation, bytes.ToArray());
    }

    private static PlannedFileChange? PlanChoices(WorkspaceLayout layout, WorkspaceSettingsRead settings, InstallConfiguration? configuration, InstallFrontmatterSelection? frontmatter, string? keptGuide)
    {
        // A kept guide is recorded as an exclusion, so later Updates leave the user's file and the missing guide alone.
        ImmutableArray<string> keptFiles = keptGuide is null ? [] : [keptGuide];
        if (configuration is null)
            return WorkspaceSettingsChangePlanner.PlanConfiguration(settings, new() { Files = keptFiles }, new(), frontmatter?.Persist == true ? frontmatter.Form : null);
        var included = configuration.Routes.Where(row => row.Action != InstallRouteAction.Remove).ToArray();
        var removed = configuration.Routes.Where(row => row.Action == InstallRouteAction.Remove).ToArray();
        var includesMemory = included.Any(row => row.Id.StartsWith(WorkspaceCategoryNames.Memory + "/", StringComparison.Ordinal));
        var categories = included.Where(row => !row.Id.Contains('/')).Select(row => row.Id).ToList();
        var directories = included.Select(row => InstallConfigurationChoices.Directory(layout, row.Id)).ToList();
        var files = included.Select(row => InstallConfigurationChoices.Entrypoint(layout, row.Id))
            .Concat(directories).ToList();
        if (included.Length > 0) files.Add(layout.LoaderPath);
        if (includesMemory)
        {
            categories.Add(WorkspaceCategoryNames.Memory);
            directories.Add(layout.CategoryDirectory(WorkspaceCategoryNames.Memory));
            files.Add(InstallConfigurationChoices.Entrypoint(layout, WorkspaceCategoryNames.Memory));
            files.Add(layout.CategoryDirectory(WorkspaceCategoryNames.Memory));
        }
        // Every Memory state needs the Memory catalogue, and the catalogue is only useful with a state.
        var removedCategories = removed.Where(row => !row.Id.Contains('/')).Select(row => row.Id).ToList();
        if (!includesMemory) removedCategories.Add(WorkspaceCategoryNames.Memory);
        var additions = new WorkspaceRemovalSelection
        {
            Categories = removedCategories.ToImmutableArray(),
            Directories = removed.Where(row => row.Id.Contains('/')).Select(row => InstallConfigurationChoices.Directory(layout, row.Id)).ToImmutableArray(),
            Files = keptFiles,
        };
        var clear = new WorkspaceRemovalSelection
        {
            Categories = categories.ToImmutableArray(),
            Directories = directories.ToImmutableArray(),
            Files = files.ToImmutableArray(),
        };
        return WorkspaceSettingsChangePlanner.PlanConfiguration(settings, additions, clear, frontmatter?.Persist == true ? frontmatter.Form : null);
    }
}
