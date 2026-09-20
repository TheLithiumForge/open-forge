using OpenForge.Cli.Core.Commands.Extension.List.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.List.Shared.Result;

internal static class ExtensionListRowBuilder
{
    internal static IReadOnlyList<ExtensionListInstalledRow> CreateInstalledRows(
        ExtensionListSelection selection,
        ExtensionSourceReadResult source,
        WorkspaceOwnershipRead? lifecycle,
        ICollection<ExtensionListFinding> findings,
        ExtensionLifecycleDoctorView? doctor = null)
    {
        if (!selection.Installed || lifecycle is null || !lifecycle.IsTrustworthy)
        {
            return [];
        }

        var rows = lifecycle.Document.Extensions
            .Select(package => CreateInstalledRow(package, source, doctor, findings))
            .OrderBy(row => row.Id, StringComparer.Ordinal)
            .ToArray();
        return rows;
    }

    internal static ExtensionListAvailableRow[] CreateAvailableRows(
        IReadOnlyList<ExtensionPackageFact> packages,
        IReadOnlyList<ExtensionListInstalledRow>? installed = null)
    {
        var byId = packages.ToDictionary(package => package.Id, StringComparer.Ordinal);
        var installedVersions = (installed ?? [])
            .Where(row => row.Version is not null)
            .ToDictionary(row => row.Id, row => row.Version!, StringComparer.Ordinal);
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
                    Dependencies = package.Dependencies.ToArray(),
                    InstalledVersion = installedVersions.GetValueOrDefault(package.Id),
                };
            })
            .ToArray();
    }

    private static ExtensionListInstalledRow CreateInstalledRow(
        ExtensionOwnership package,
        ExtensionSourceReadResult selectedSource,
        ExtensionLifecycleDoctorView? doctor,
        ICollection<ExtensionListFinding> findings)
    {
        var source = ReadRecordedSource(package.Source, selectedSource, doctor);
        var comparison = doctor?.Packages.FirstOrDefault(value =>
            string.Equals(value.Installed.Id, package.Id, StringComparison.Ordinal));
        var files = doctor is null ? [] : ReadFiles(package, doctor.Targets);
        AddSourceFindings(package, source, findings);
        AddTargetFindings(package, source, comparison, files, findings);

        var packageState = comparison?.State switch
        {
            ExtensionInstalledPackageComparisonState.Current => ExtensionListInstalledPackageState.Current,
            ExtensionInstalledPackageComparisonState.Missing => ExtensionListInstalledPackageState.Missing,
            ExtensionInstalledPackageComparisonState.VersionMismatch => ExtensionListInstalledPackageState.VersionMismatch,
            ExtensionInstalledPackageComparisonState.DependencyMismatch => ExtensionListInstalledPackageState.DependencyMismatch,
            ExtensionInstalledPackageComparisonState.Ambiguous => ExtensionListInstalledPackageState.Ambiguous,
            ExtensionInstalledPackageComparisonState.SourceUnavailable => ExtensionListInstalledPackageState.SourceUnavailable,
            null => ExtensionListInstalledPackageState.Current,
            _ => throw new ArgumentOutOfRangeException(nameof(comparison), comparison?.State, "The Extension package comparison state is not defined."),
        };
        return new ExtensionListInstalledRow
        {
            Id = package.Id,
            Version = package.Version,
            Trust = ExtensionListOwnershipTrust.Trusted,
            ManagedPathCount = package.Paths.Length,
            SourceAvailable = source?.Read.State == ExtensionSourceReadState.Complete,
            SourceState = source?.Read.State switch
            {
                null => null,
                ExtensionSourceReadState.Complete => ExtensionListSourceState.Complete,
                ExtensionSourceReadState.Missing => ExtensionListSourceState.Missing,
                ExtensionSourceReadState.Invalid => ExtensionListSourceState.Invalid,
                ExtensionSourceReadState.Blocked => ExtensionListSourceState.Blocked,
                ExtensionSourceReadState.Unavailable => ExtensionListSourceState.Unavailable,
                ExtensionSourceReadState.Cancelled => ExtensionListSourceState.Interrupted,
                _ => throw new ArgumentOutOfRangeException(nameof(source), source?.Read.State, "The Extension source read state is not defined."),
            },
            Dependencies = package.Dependencies.ToArray(),
            RecordedSource = package.Source,
            PackageState = packageState,
            Coverage = ReadCoverage(files),
            Files = files,
        };
    }

    private static ExtensionSourceObservation? ReadRecordedSource(
        string? recordedSource,
        ExtensionSourceReadResult selectedSource,
        ExtensionLifecycleDoctorView? doctor)
    {
        if (doctor is not null)
        {
            return doctor.Sources.FirstOrDefault(source => string.Equals(
                source.RecordedSource,
                recordedSource,
                StringComparison.Ordinal));
        }

        if (recordedSource is null && selectedSource.Kind == ExtensionSourceKind.EmbeddedCatalogue)
        {
            return new ExtensionSourceObservation(RecordedSource: null, selectedSource);
        }

        return string.Equals(selectedSource.Identity, recordedSource, StringComparison.Ordinal)
            ? new ExtensionSourceObservation(recordedSource, selectedSource)
            : null;
    }

    private static IReadOnlyList<ExtensionListInstalledFile> ReadFiles(
        ExtensionOwnership package,
        IReadOnlyList<ExtensionManagedTargetDoctorObservation> targets)
        => package.Paths
            .Select(path => targets.FirstOrDefault(target => string.Equals(
                target.Target.Path,
                path,
                StringComparison.Ordinal)))
            .Where(target => target is not null)
            .Select(target =>
            {
                var value = target!;
                return new ExtensionListInstalledFile
                {
                    Path = value.Target.Path,
                    State = value.Target.State switch
                    {
                        OperationalTargetState.Current => ExtensionListInstalledFileState.Current,
                        OperationalTargetState.Changed => ExtensionListInstalledFileState.Changed,
                        OperationalTargetState.Missing => ExtensionListInstalledFileState.Missing,
                        OperationalTargetState.Unavailable => ExtensionListInstalledFileState.Unavailable,
                        OperationalTargetState.Blocked => ExtensionListInstalledFileState.Blocked,
                        _ => throw new ArgumentOutOfRangeException(nameof(target), value.Target.State, "The Extension target state is not defined."),
                    },
                    Cause = value.Cause,
                    UnavailableReason = value.UnavailableReason switch
                    {
                        null => null,
                        ExtensionManagedTargetUnavailableReason.IntendedComparisonUnavailable => ExtensionListInstalledUnavailableReason.IntendedComparisonUnavailable,
                        ExtensionManagedTargetUnavailableReason.TargetReadUnavailable => ExtensionListInstalledUnavailableReason.TargetReadUnavailable,
                        _ => throw new ArgumentOutOfRangeException(nameof(target), value.UnavailableReason, "The Extension target unavailable reason is not defined."),
                    },
                    Owners = value.Target.Owners.ToArray(),
                };
            })
            .OrderBy(file => file.Path, StringComparer.Ordinal)
            .ToArray();

    private static void AddSourceFindings(
        ExtensionOwnership package,
        ExtensionSourceObservation? source,
        ICollection<ExtensionListFinding> findings)
    {
        if (source is null)
        {
            return;
        }

        var read = source.Read;
        var finding = read.State switch
        {
            ExtensionSourceReadState.Missing => new ExtensionListFinding(
                ExtensionListFindingCode.InstalledSourceMissing,
                CliSemanticStatus.Attention,
                package.Id,
                read.Cause ?? "The recorded Extension source is missing.")
            {
                Path = read.Identity,
                Owner = package.Id,
            },
            ExtensionSourceReadState.Unavailable => new ExtensionListFinding(
                ExtensionListFindingCode.InstalledSourceUnavailable,
                CliSemanticStatus.Incomplete,
                package.Id,
                read.Cause ?? "The recorded Extension source is unavailable.")
            {
                Path = read.Identity,
                Owner = package.Id,
            },
            ExtensionSourceReadState.Invalid => new ExtensionListFinding(
                ExtensionListFindingCode.InstalledSourceInvalid,
                CliSemanticStatus.Incomplete,
                package.Id,
                read.Cause ?? "The recorded Extension source is invalid.")
            {
                Path = read.Identity,
                Owner = package.Id,
            },
            ExtensionSourceReadState.Blocked => new ExtensionListFinding(
                ExtensionListFindingCode.InstalledSourceBlocked,
                CliSemanticStatus.Blocked,
                package.Id,
                read.Cause ?? "The recorded Extension source is blocked.")
            {
                Path = read.Identity,
                Owner = package.Id,
            },
            _ => null,
        };
        if (finding is not null)
        {
            findings.Add(finding);
        }
    }

    private static void AddTargetFindings(
        ExtensionOwnership package,
        ExtensionSourceObservation? source,
        ExtensionInstalledPackageComparison? comparison,
        IReadOnlyList<ExtensionListInstalledFile> files,
        ICollection<ExtensionListFinding> findings)
    {
        var changed = files.Count(file => file.State == ExtensionListInstalledFileState.Changed);
        if (changed > 0)
        {
            findings.Add(new ExtensionListFinding(
                ExtensionListFindingCode.InstalledFilesChanged,
                CliSemanticStatus.Attention,
                package.Id,
                "The installed Extension files differ from their recorded source.")
            {
                Count = changed,
                Owner = package.Id,
                Path = files.First(file => file.State == ExtensionListInstalledFileState.Changed).Path,
            });
        }

        var missing = files.Count(file => file.State == ExtensionListInstalledFileState.Missing);
        if (missing > 0)
        {
            findings.Add(new ExtensionListFinding(
                ExtensionListFindingCode.InstalledFilesMissing,
                CliSemanticStatus.Attention,
                package.Id,
                "The installed Extension files are missing.")
            {
                Count = missing,
                Owner = package.Id,
                Path = files.First(file => file.State == ExtensionListInstalledFileState.Missing).Path,
            });
        }

        foreach (var file in files.Where(file => file.State == ExtensionListInstalledFileState.Unavailable
            && file.UnavailableReason == ExtensionListInstalledUnavailableReason.TargetReadUnavailable))
        {
            findings.Add(new ExtensionListFinding(
                ExtensionListFindingCode.InstalledTargetUnavailable,
                CliSemanticStatus.Incomplete,
                package.Id,
                file.Cause ?? "The installed Extension target could not be read completely.")
            {
                Path = file.Path,
                Owner = package.Id,
            });
        }

        foreach (var file in files.Where(file => file.State == ExtensionListInstalledFileState.Blocked))
        {
            findings.Add(new ExtensionListFinding(
                ExtensionListFindingCode.InstalledTargetBlocked,
                CliSemanticStatus.Blocked,
                package.Id,
                file.Cause ?? "The installed Extension target could not be checked safely.")
            {
                Path = file.Path,
                Owner = package.Id,
            });
        }

        var comparisonUnavailable = files.FirstOrDefault(file => file.State == ExtensionListInstalledFileState.Unavailable
            && file.UnavailableReason == ExtensionListInstalledUnavailableReason.IntendedComparisonUnavailable);
        if (comparisonUnavailable is not null && source?.Read.State == ExtensionSourceReadState.Complete)
        {
            findings.Add(new ExtensionListFinding(
                ExtensionListFindingCode.InstalledFilesUnavailable,
                CliSemanticStatus.Incomplete,
                package.Id,
                comparisonUnavailable.Cause ?? "The installed Extension files could not be compared.")
            {
                Path = comparisonUnavailable.Path,
                Owner = package.Id,
            });
        }

        if (comparison is { State: ExtensionInstalledPackageComparisonState.Missing or ExtensionInstalledPackageComparisonState.Ambiguous }
            && source?.Read.State == ExtensionSourceReadState.Complete
            && comparisonUnavailable is null)
        {
            findings.Add(new ExtensionListFinding(
                ExtensionListFindingCode.InstalledFilesUnavailable,
                CliSemanticStatus.Incomplete,
                package.Id,
                comparison.Cause ?? "The installed Extension files could not be compared.")
            {
                Owner = package.Id,
            });
        }
    }

    private static ExtensionListInstalledCoverage ReadCoverage(
        IReadOnlyList<ExtensionListInstalledFile> files)
    {
        if (files.Any(file => file.State == ExtensionListInstalledFileState.Blocked))
        {
            return ExtensionListInstalledCoverage.Blocked;
        }

        if (files.Any(file => file.State == ExtensionListInstalledFileState.Unavailable))
        {
            return ExtensionListInstalledCoverage.Incomplete;
        }

        if (files.Any(file => file.State == ExtensionListInstalledFileState.Changed))
        {
            return ExtensionListInstalledCoverage.Changed;
        }

        if (files.Any(file => file.State == ExtensionListInstalledFileState.Missing))
        {
            return ExtensionListInstalledCoverage.Missing;
        }

        return ExtensionListInstalledCoverage.Complete;
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
