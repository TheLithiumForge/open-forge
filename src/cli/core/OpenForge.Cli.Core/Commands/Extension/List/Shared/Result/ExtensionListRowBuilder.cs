using OpenForge.Cli.Core.Commands.Extension.List.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.List.Shared.Result;

internal static class ExtensionListRowBuilder
{
    internal static IReadOnlyList<ExtensionListInstalledRow> CreateInstalledRows(
        ExtensionListSelection selection,
        ExtensionSourceReadResult source,
        LifecycleReadResult? lifecycle,
        ICollection<ExtensionListFinding> findings)
    {
        if (!selection.Installed || lifecycle is null)
        {
            return [];
        }

        var availableIds = source.State == ExtensionSourceReadState.Complete
            ? source.Packages.Select(package => package.Id).ToHashSet(StringComparer.Ordinal)
            : [];
        var rows = lifecycle.Packages
            .Select(package => new ExtensionListInstalledRow
            {
                Id = package.Id,
                Version = package.Version,
                Trust = lifecycle.Trust,
                ManagedPathCount = package.Paths.Count,
                SourceAvailable = availableIds.Contains(package.Id),
            })
            .OrderBy(row => row.Id, StringComparer.Ordinal)
            .ToArray();
        foreach (var row in rows.Where(row => source.State == ExtensionSourceReadState.Complete && !row.SourceAvailable))
        {
            findings.Add(new ExtensionListFinding(
                code: ExtensionListFindingCode.SourceUnavailable,
                status: CliSemanticStatus.Attention,
                subject: row.Id,
                cause: "The installed package ID is not available from the selected package source."));
        }

        return rows;
    }

    internal static ExtensionListAvailableRow[] CreateAvailableRows(IReadOnlyList<ExtensionPackageFact> packages)
    {
        var byId = packages.ToDictionary(package => package.Id, StringComparer.Ordinal);
        return packages
            .OrderBy(package => package.Id, StringComparer.Ordinal)
            .Select(package =>
            {
                var dependencies = ReadDependencyClosure(package.Id, byId);
                return new ExtensionListAvailableRow
                {
                    Id = package.Id,
                    Name = package.Name,
                    Description = package.Description,
                    Version = package.Version,
                    PackageCount = dependencies.Count + 1,
                    DependencyCount = dependencies.Count,
                };
            })
            .ToArray();
    }

    private static HashSet<string> ReadDependencyClosure(
        string id,
        IReadOnlyDictionary<string, ExtensionPackageFact> packages)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>(packages[id].Dependencies.Reverse());
        while (pending.TryPop(out var dependency))
        {
            if (!result.Add(dependency))
            {
                continue;
            }

            foreach (var nested in packages[dependency].Dependencies.Reverse())
            {
                pending.Push(nested);
            }
        }

        return result;
    }
}
