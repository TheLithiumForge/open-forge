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
    internal static PlannedFileChange? Plan(WorkspaceSettingsRead settings, InstallConfiguration? configuration, InstallFrontmatterSelection? frontmatter)
    {
        var change = PlanChoices(settings, configuration, frontmatter);
        ReadOnlyMemory<byte> bytes = change?.IntendedBytes.ToArray() ?? settings.Snapshot?.Bytes.ToArray()
            ?? throw new InvalidOperationException("Install settings require their observed bytes.");
        var intended = WorkspaceSettingsCodec.Read(bytes.IsEmpty ? "{}"u8.ToArray() : bytes).Document
            ?? throw new InvalidOperationException("Planned Install settings must decode.");
        if (!FrameworkPayloadSelection.IncludesPath(WorkspaceSettingsDefinitions.MemoryArchivePath + "/_archived.md", intended)) return change;
        var seeded = WorkspaceSettingsCodec.SeedArchiveLinkScope(bytes);
        if (seeded is null) return change;
        var snapshot = settings.Snapshot ?? throw new InvalidOperationException("Install settings require their observed snapshot.");
        return snapshot.Kind == FileExpectationKind.Missing
            ? PlannedFileChange.Create(snapshot.Expectation, seeded)
            : PlannedFileChange.Replace(snapshot.Expectation, seeded);
    }

    private const string WorkingRoute = "memory/working";
    private const string SavingPolicyPath = ".agents/memory/saving.md";

    private static PlannedFileChange? PlanChoices(WorkspaceSettingsRead settings, InstallConfiguration? configuration, InstallFrontmatterSelection? frontmatter)
    {
        if (configuration is null)
            return WorkspaceSettingsChangePlanner.PlanConfiguration(settings, new(), new(), frontmatter?.Persist == true ? frontmatter.Form : null);
        var included = configuration.Routes.Where(row => row.Action != InstallRouteAction.Remove).ToArray();
        var removed = configuration.Routes.Where(row => row.Action == InstallRouteAction.Remove).ToArray();
        var includesMemory = included.Any(row => row.Id.StartsWith("memory/", StringComparison.Ordinal));
        var categories = included.Where(row => !row.Id.Contains('/')).Select(row => row.Id).ToList();
        var directories = included.Select(row => InstallConfigurationChoices.Directory(row.Id)).ToList();
        var files = included.Select(row => InstallConfigurationChoices.Entrypoint(row.Id))
            .Concat(directories).ToList();
        if (included.Length > 0) files.Add(".agents/loader.md");
        if (includesMemory)
        {
            categories.Add("memory");
            directories.Add(".agents/memory");
            files.Add(".agents/memory/_memory.md");
            files.Add(".agents/memory");
        }
        // Every Memory state needs the Memory catalogue, and the catalogue is only useful with a state.
        var removedCategories = removed.Where(row => !row.Id.Contains('/')).Select(row => row.Id).ToList();
        if (!includesMemory) removedCategories.Add("memory");
        // The saving policy sits in the shared Memory catalogue so it stays in Git while Working records are
        // private. It only has a job when Working Memory is installed. Adding Working back brings the policy
        // with it, while a policy the user removed on its own stays removed.
        var removedFiles = new List<string>();
        var workingWasRemoved = WorkspaceRemovals.IsPathRemoved(InstallConfigurationChoices.Entrypoint(WorkingRoute), settings.Document);
        if (!included.Any(row => row.Id == WorkingRoute)) removedFiles.Add(SavingPolicyPath);
        else if (workingWasRemoved) files.Add(SavingPolicyPath);
        var additions = new WorkspaceRemovalSelection
        {
            Categories = removedCategories.ToImmutableArray(),
            Directories = removed.Where(row => row.Id.Contains('/')).Select(row => InstallConfigurationChoices.Directory(row.Id)).ToImmutableArray(),
            Files = includesMemory ? removedFiles.ToImmutableArray() : [],
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
