using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectPathFactsBuilder
{
    internal static ExtensionInspectPathFacts Build(
        ExtensionInspectDependencyPathInput input,
        IReadOnlyList<ExtensionInspectDeclaredPathProjection> sourcePathProjections)
    {
        var declaredPaths = ReadDeclaredPaths(sourcePathProjections);
        var currentPaths = ReadCurrentPaths(input.CurrentSnapshot, input.SubjectId, input.Findings);
        return new ExtensionInspectPathFacts
        {
            State = ReadPathState(
                input.AvailablePackage,
                input.InstalledPackage,
                currentPaths,
                declaredPaths),
            Declared = declaredPaths,
            Current = currentPaths,
        };
    }

    internal static ExtensionInspectPathFacts ReadEventPathFacts(
        IReadOnlyList<ExtensionInspectCurrentPath>? currentPaths)
    {
        var paths = currentPaths ?? [];
        var inspectionStarted = currentPaths is not null;
        var allPathsPresent = currentPaths?.All(
            path => path.State == ExtensionInspectCurrentPathState.Present) == true;
        ExtensionInspectPathState state;
        if (!inspectionStarted)
        {
            state = ExtensionInspectPathState.NotStarted;
        }
        else if (allPathsPresent)
        {
            state = ExtensionInspectPathState.Complete;
        }
        else
        {
            state = ExtensionInspectPathState.Incomplete;
        }

        return new ExtensionInspectPathFacts
        {
            State = state,
            Declared = [],
            Current = paths,
        };
    }

    internal static IReadOnlyList<ExtensionInspectDeclaredPathProjection> ReadSourcePathProjections(
        IReadOnlyList<ExtensionPackageFact> packages,
        ICollection<ExtensionInspectFinding> findings)
    {
        var byPath = new Dictionary<string, List<ExtensionInspectPathFileInput>>(StringComparer.Ordinal);
        foreach (var package in packages)
        {
            foreach (var file in package.Payload)
            {
                var path = file.TargetPath ?? file.Path;
                if (!byPath.TryGetValue(path, out var files))
                {
                    files = [];
                    byPath.Add(path, files);
                }

                files.Add(new ExtensionInspectPathFileInput
                {
                    PackageId = package.Id,
                    File = file,
                });
            }
        }

        var projections = new List<ExtensionInspectDeclaredPathProjection>(byPath.Count);
        foreach (var pair in byPath.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            var files = pair.Value;
            foreach (var value in files)
            {
                AddDeclaredPathFinding(value.PackageId, pair.Key, value.File, findings);
            }

            var owners = files
                .Select(value => value.PackageId)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();
            var hasConflict = HasConflictingFiles(files);
            if (hasConflict)
            {
                ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
                {
                    Code = ExtensionInspectFindingCode.OwnershipConflict,
                    Subject = owners[0],
                    PackageId = owners[0],
                    Path = pair.Key,
                    Cause = "Dependency packages declare the same target path with conflicting bytes.",
                });
            }

            var sourcePaths = files
                .Select(value => value.File.Path)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();
            projections.Add(new ExtensionInspectDeclaredPathProjection
            {
                Path = pair.Key,
                SourcePath = sourcePaths.Length == 1 ? sourcePaths[0] : null,
                State = ReadProjectionState(files),
                Owners = owners,
                Files = files.Select(value => value.File).ToArray(),
                HasConflict = hasConflict,
            });
        }

        return projections;
    }

    private static void AddDeclaredPathFinding(
        string packageId,
        string path,
        ExtensionPackageFileFact file,
        ICollection<ExtensionInspectFinding> findings)
    {
        var state = ExtensionInspectResultMappings.ReadDeclaredPathState(file.State);
        if (state is ExtensionInspectDeclaredPathState.Invalid or ExtensionInspectDeclaredPathState.Blocked)
        {
            ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.PathInvalid,
                Subject = packageId,
                PackageId = packageId,
                Path = path,
                Cause = "The package payload target path is invalid.",
            });
        }
        else if (state is ExtensionInspectDeclaredPathState.Unavailable
            or ExtensionInspectDeclaredPathState.Missing
            or ExtensionInspectDeclaredPathState.Interrupted)
        {
            ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.PathUnavailable,
                Subject = packageId,
                PackageId = packageId,
                Path = path,
                Cause = "The package payload path cannot be read completely.",
            });
        }
    }

    private static ExtensionInspectDeclaredPathState ReadProjectionState(
        IReadOnlyList<ExtensionInspectPathFileInput> files)
    {
        if (files.Any(value => ExtensionInspectResultMappings.ReadDeclaredPathState(value.File.State) == ExtensionInspectDeclaredPathState.Invalid))
        {
            return ExtensionInspectDeclaredPathState.Invalid;
        }

        if (files.Any(value => ExtensionInspectResultMappings.ReadDeclaredPathState(value.File.State) == ExtensionInspectDeclaredPathState.Blocked))
        {
            return ExtensionInspectDeclaredPathState.Blocked;
        }

        if (files.Any(value => ExtensionInspectResultMappings.ReadDeclaredPathState(value.File.State) == ExtensionInspectDeclaredPathState.Unavailable))
        {
            return ExtensionInspectDeclaredPathState.Unavailable;
        }

        if (files.Any(value => ExtensionInspectResultMappings.ReadDeclaredPathState(value.File.State) == ExtensionInspectDeclaredPathState.Missing))
        {
            return ExtensionInspectDeclaredPathState.Missing;
        }

        return ExtensionInspectDeclaredPathState.Available;
    }

    private static bool HasConflictingFiles(
        IReadOnlyList<ExtensionInspectPathFileInput> files)
    {
        var readable = files
            .Where(value => value.File.State == ExtensionPackageFileReadState.Available
                && value.File.Sha256 is not null)
            .Select(value => value.File.Sha256)
            .OfType<string>()
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return readable.Length > 1;
    }

    private static IReadOnlyList<ExtensionInspectDeclaredPath> ReadDeclaredPaths(
        IReadOnlyList<ExtensionInspectDeclaredPathProjection> projections)
        => projections
            .Select(path => new ExtensionInspectDeclaredPath
            {
                Path = path.Path,
                SourcePath = path.SourcePath,
                State = path.State,
            })
            .ToArray();

    private static IReadOnlyList<ExtensionInspectCurrentPath> ReadCurrentPaths(
        IReadOnlyList<ExtensionInspectCurrentPath> snapshot,
        string subjectId,
        ICollection<ExtensionInspectFinding> findings)
    {
        foreach (var path in snapshot)
        {
            switch (path.State)
            {
                case ExtensionInspectCurrentPathState.Missing:
                    ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
                    {
                        Code = ExtensionInspectFindingCode.PathMissing,
                        Subject = subjectId,
                        PackageId = subjectId,
                        Path = path.Path,
                        Cause = "A trusted managed path is missing from the current workspace.",
                    });
                    break;
                case ExtensionInspectCurrentPathState.Blocked:
                    ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
                    {
                        Code = ExtensionInspectFindingCode.PathInvalid,
                        Subject = subjectId,
                        PackageId = subjectId,
                        Path = path.Path,
                        Cause = "The current managed path is outside the safe workspace boundary or unavailable.",
                    });
                    break;
                case ExtensionInspectCurrentPathState.Unavailable:
                    ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
                    {
                        Code = ExtensionInspectFindingCode.PathUnavailable,
                        Subject = subjectId,
                        PackageId = subjectId,
                        Path = path.Path,
                        Cause = "The current managed path could not be read.",
                    });
                    break;
            }
        }

        return snapshot.OrderBy(path => path.Path, StringComparer.Ordinal).ToArray();
    }

    private static ExtensionInspectPathState ReadPathState(
        ExtensionPackageFact? availablePackage,
        LifecycleInstalledPackage? lifecyclePackage,
        IReadOnlyList<ExtensionInspectCurrentPath> current,
        IReadOnlyList<ExtensionInspectDeclaredPath> declared)
    {
        if (current.Any(path => path.State is ExtensionInspectCurrentPathState.Blocked)
            || declared.Any(path => path.State == ExtensionInspectDeclaredPathState.Invalid))
        {
            return ExtensionInspectPathState.Blocked;
        }

        if (current.Any(path => path.State is ExtensionInspectCurrentPathState.Unavailable)
            || declared.Any(path => path.State is ExtensionInspectDeclaredPathState.Unavailable or ExtensionInspectDeclaredPathState.Missing))
        {
            return ExtensionInspectPathState.Incomplete;
        }

        return availablePackage is not null || lifecyclePackage is not null
            ? ExtensionInspectPathState.Complete
            : ExtensionInspectPathState.NotStarted;
    }
}
