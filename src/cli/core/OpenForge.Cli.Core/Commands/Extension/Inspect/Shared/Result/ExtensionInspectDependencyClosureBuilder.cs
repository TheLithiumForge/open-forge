using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectDependencyClosureBuilder
{
    internal static ExtensionInspectDependencyClosure ReadDependencies(
        ExtensionInspectDependencyPathInput input)
    {
        var source = input.Source;
        var package = input.AvailablePackage;
        var installedPackage = input.InstalledPackage;
        var subjectId = input.SubjectId;
        var findings = input.Findings;
        if (package is not null)
        {
            var sourceIdentity = ExtensionInspectSubjectPackageBuilder.ReadSourceIdentity(source);
            var sourceNodes = source.Packages
                .Select(value => new ExtensionInspectDependencyNode(
                    Id: value.Id,
                    Dependencies: value.Dependencies,
                    Version: value.Version,
                    Source: sourceIdentity))
                .ToArray();
            return ReadDependencyClosure(
                sourceNodes,
                package.Id,
                sourceIdentity,
                source.FailureKind,
                findings);
        }

        if (installedPackage is not null)
        {
            var lifecycleNodes = input.InstalledPackages
                .Select(value => new ExtensionInspectDependencyNode(
                    Id: value.Id,
                    Dependencies: value.Dependencies,
                    Version: value.Version,
                    Source: value.Source))
                .ToArray();
            return ReadDependencyClosure(
                lifecycleNodes,
                installedPackage.Id,
                installedPackage.Source,
                ExtensionSourceFailureKind.None,
                findings);
        }

        return new ExtensionInspectDependencyClosure
        {
            State = ExtensionInspectDependencyState.NotStarted,
            Declared = [],
            Resolved = [],
            Order = [],
        };
    }

    internal static IReadOnlyList<ExtensionPackageFact> ReadAvailableClosure(
        ExtensionSourceReadResult source,
        ExtensionPackageFact? availablePackage,
        ExtensionInspectDependencyClosure dependencies)
    {
        if (availablePackage is null)
        {
            return [];
        }

        var ids = dependencies.Resolved
            .Where(value => value.State == ExtensionInspectDependencyPackageState.Available)
            .Select(value => value.Id)
            .ToHashSet(StringComparer.Ordinal);
        ids.Add(availablePackage.Id);
        return source.Packages
            .Where(value => ids.Contains(value.Id))
            .OrderBy(value => value.Id, StringComparer.Ordinal)
            .ToArray();
    }

    private static ExtensionInspectDependencyClosure ReadDependencyClosure(
        IReadOnlyList<ExtensionInspectDependencyNode> nodes,
        string subjectId,
        string? sourceIdentity,
        ExtensionSourceFailureKind reportedFailure,
        ICollection<ExtensionInspectFinding> findings)
    {
        var byId = nodes.ToDictionary(value => value.Id, StringComparer.Ordinal);
        if (!byId.TryGetValue(subjectId, out var package))
        {
            return new ExtensionInspectDependencyClosure
            {
                State = ExtensionInspectDependencyState.NotStarted,
                Declared = [],
                Resolved = [],
                Order = [],
            };
        }

        var declared = package.Dependencies
            .Select((dependency, index) => new ExtensionInspectDependencyEdge
            {
                From = subjectId,
                To = dependency,
                Position = index + 1,
            })
            .ToArray();
        var resolved = new List<ExtensionInspectDependencyPackage>();
        var order = new List<string>();
        var active = new HashSet<string>(StringComparer.Ordinal);
        var complete = true;
        var cycle = false;
        var visiting = new HashSet<string>(StringComparer.Ordinal);

        void Visit(string id)
        {
            if (active.Contains(id))
            {
                return;
            }

            if (!byId.TryGetValue(id, out var node))
            {
                complete = false;
                if (resolved.All(item => item.Id != id))
                {
                    resolved.Add(new ExtensionInspectDependencyPackage
                    {
                        Id = id,
                        Version = null,
                        Source = sourceIdentity,
                        State = ExtensionInspectDependencyPackageState.Missing,
                    });
                }

                if (reportedFailure != ExtensionSourceFailureKind.DependencyIncomplete)
                {
                    ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
                    {
                        Code = ExtensionInspectFindingCode.DependencyIncomplete,
                        Subject = subjectId,
                        PackageId = subjectId,
                        Dependency = id,
                        Cause = "The dependency is not present in the selected source universe.",
                    });
                }
                return;
            }

            if (!visiting.Add(id))
            {
                cycle = true;
                return;
            }

            foreach (var dependency in node.Dependencies.Order(StringComparer.Ordinal))
            {
                Visit(dependency);
            }

            visiting.Remove(id);
            active.Add(id);
            if (resolved.All(item => item.Id != id))
            {
                resolved.Add(new ExtensionInspectDependencyPackage
                {
                    Id = node.Id,
                    Version = node.Version,
                    Source = node.Source,
                    State = ExtensionInspectDependencyPackageState.Available,
                });
                order.Add(id);
            }
        }

        Visit(package.Id);
        if (cycle && reportedFailure != ExtensionSourceFailureKind.DependencyCycle)
        {
            ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.DependencyCycle,
                Subject = subjectId,
                PackageId = subjectId,
                Cause = "The selected dependency closure contains a cycle.",
            });
        }

        var state = ReadDependencyState(cycle, complete);
        return new ExtensionInspectDependencyClosure
        {
            State = state,
            Declared = declared,
            Resolved = resolved.OrderBy(item => item.Id, StringComparer.Ordinal).ToArray(),
            Order = order,
        };
    }

    private static ExtensionInspectDependencyState ReadDependencyState(
        bool cycle,
        bool complete)
    {
        if (cycle)
        {
            return ExtensionInspectDependencyState.Blocked;
        }

        return complete
            ? ExtensionInspectDependencyState.Complete
            : ExtensionInspectDependencyState.Incomplete;
    }
}
