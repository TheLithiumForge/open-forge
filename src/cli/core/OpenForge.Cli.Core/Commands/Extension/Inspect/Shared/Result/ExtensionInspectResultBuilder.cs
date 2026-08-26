using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal sealed partial class ExtensionInspectResultBuilder(ExtensionInspectComparisonBuilder comparisonBuilder)
{
    private const string EmbeddedIdentity = "embedded catalogue";
    private const string GeneratedOwnership = "derived-navigation-only";

    private readonly ExtensionInspectComparisonBuilder _comparisonBuilder = comparisonBuilder;

    internal ExtensionInspectResult Build(
        ExtensionInspectRequest request,
        ExtensionSourceReadResult source,
        LifecycleReadResult lifecycle,
        IReadOnlyList<ExtensionInspectCurrentPath> currentSnapshot)
    {
        var subjectId = request.StableId;
        var findings = new List<ExtensionInspectFinding>();
        var inspectSource = ReadSource(request, source);
        var inspectLifecycle = ReadLifecycle(lifecycle);
        var sourcePackages = source.Packages;
        var sourceCandidates = ReadCandidates(sourcePackages, subjectId);
        var lifecyclePackage = lifecycle.Packages.FirstOrDefault(package => package.Id == subjectId);
        var sourcePackage = CanSelectSourcePackage(source)
            && sourcePackages.Count(package => package.Id == subjectId) == 1
            ? sourcePackages.First(package => package.Id == subjectId)
            : null;
        var sourceMatches = sourcePackages.Count(package => package.Id == subjectId);
        var subject = ReadSubject(subjectId, sourceCandidates, lifecyclePackage, sourceMatches, source.State);
        var installed = ReadInstalled(lifecycle, lifecyclePackage);
        var available = ReadAvailable(source, sourcePackage, sourceMatches);
        AddBoundaryFindings(subjectId, source, lifecycle, lifecyclePackage, sourcePackage, sourceMatches, findings);

        var dependencies = ReadDependencies(new ExtensionInspectDependencyInput
        {
            Source = source,
            SourcePackage = sourcePackage,
            InstalledPackages = lifecycle.Packages,
            InstalledPackage = lifecyclePackage,
            SubjectId = subjectId,
            Findings = findings,
        });
        var sourceClosure = ReadSourceClosure(source, sourcePackage, dependencies);
        var sourcePathProjections = ReadSourcePathProjections(sourceClosure, findings);
        var declaredPaths = ReadDeclaredPaths(sourcePathProjections);
        var currentPaths = ReadCurrentPaths(currentSnapshot, subjectId, findings);
        var pathFacts = new ExtensionInspectPathFacts
        {
            State = ReadPathState(sourcePackage, lifecyclePackage, currentPaths, declaredPaths),
            Declared = declaredPaths,
            Current = currentPaths,
        };

        var baselineRecords = ExtensionInspectComparisonBuilder.ReadBaselineRecords(lifecycle, lifecyclePackage);
        var baselineFingerprints = ExtensionInspectComparisonBuilder.ReadBaselineFingerprints(baselineRecords);
        var currentMarkdownFacts = new Dictionary<string, MarkdownFingerprintFacts>(StringComparer.Ordinal);
        var intendedMarkdownFacts = new Dictionary<string, MarkdownFingerprintFacts>(StringComparer.Ordinal);
        var currentFingerprints = _comparisonBuilder.ReadCurrentFingerprints(currentPaths, findings, currentMarkdownFacts);
        var intendedFingerprints = _comparisonBuilder.ReadIntendedFingerprints(sourcePathProjections, findings, intendedMarkdownFacts);
        var generated = ExtensionInspectComparisonBuilder.ReadGenerated(currentMarkdownFacts, intendedMarkdownFacts, pathFacts.State, findings);
        var comparison = ExtensionInspectComparisonBuilder.ReadComparison(
            new ExtensionInspectComparisonInput
            {
                Lifecycle = lifecycle,
                InstalledPackage = lifecyclePackage,
                AvailablePackage = sourcePackage,
                AvailableClosure = sourceClosure,
                Dependencies = dependencies,
                BaselineRecords = baselineRecords,
                Baseline = baselineFingerprints,
                Current = currentFingerprints,
                Intended = intendedFingerprints,
                Findings = findings,
            });

        SortFindings(findings);
        var status = ReadStatus(findings);
        var counts = ExtensionInspectComparisonBuilder.ReadCounts(
            new ExtensionInspectCountsInput
            {
                Lifecycle = lifecycle,
                Source = source,
                InstalledPackage = lifecyclePackage,
                AvailablePackage = sourcePackage,
                Dependencies = dependencies,
                PathFacts = pathFacts,
                Comparison = comparison,
                Generated = generated,
                FindingCount = findings.Count,
            });
        var next = ExtensionInspectDefinitions.ReadNext(
            status,
            ExtensionInspectComparisonBuilder.IsActionable(comparison, generated, lifecycle, source, findings),
            subject.Id);

        return new ExtensionInspectResult
        {
            Status = status,
            Workspace = request.Workspace,
            Subject = subject,
            Source = inspectSource,
            Lifecycle = inspectLifecycle,
            Installed = installed,
            Available = available,
            Dependencies = dependencies,
            PathFacts = pathFacts,
            Comparison = comparison,
            Generated = generated,
            Findings = new ReadOnlyCollection<ExtensionInspectFinding>(findings),
            Counts = counts,
            Next = next,
        };
    }

    internal static ExtensionInspectResult Invalid(
        CliWorkspace? workspace,
        string? supplied,
        string cause)
        => Invalid(
            workspace,
            supplied,
            ExtensionInspectFindingCode.InvalidInput,
            cause);

    internal static ExtensionInspectResult InvalidStableId(
        CliWorkspace? workspace,
        string? supplied,
        string cause)
        => Invalid(
            workspace,
            supplied,
            ExtensionInspectFindingCode.InvalidStableId,
            cause);

    private static ExtensionInspectResult Invalid(
        CliWorkspace? workspace,
        string? supplied,
        ExtensionInspectFindingCode code,
        string cause)
    {
        var finding = Finding(new ExtensionInspectFindingInput
        {
            Code = code,
            Subject = supplied,
            Cause = cause,
        });
        return Empty(
            CliSemanticStatus.Invalid,
            workspace,
            new ExtensionInspectSubject
            {
                Supplied = supplied,
                Form = supplied is null ? null : ExtensionInspectSubjectForm.StableId,
                Id = null,
                State = ExtensionInspectSubjectState.Invalid,
                Candidates = [],
            },
            [finding],
            ExtensionInspectDefinitions.InvalidNext);
    }

    internal static ExtensionInspectResult WorkspaceBlocked(
        string? supplied,
        string cause)
    {
        var finding = Finding(new ExtensionInspectFindingInput
        {
            Code = ExtensionInspectFindingCode.WorkspaceUnavailable,
            Subject = supplied,
            Cause = cause,
        });
        return Empty(
            CliSemanticStatus.Blocked,
            null,
            new ExtensionInspectSubject
            {
                Supplied = supplied,
                Form = supplied is null ? null : ExtensionInspectSubjectForm.StableId,
                Id = supplied,
                State = ExtensionInspectSubjectState.NotStarted,
                Candidates = [],
            },
            [finding],
            ExtensionInspectDefinitions.BlockedNext);
    }

    internal ExtensionInspectResult Event(ExtensionInspectEventInput input)
    {
        var finding = Finding(new ExtensionInspectFindingInput
        {
            Code = input.Code,
            Subject = input.Request.StableId,
            Cause = input.Cause,
        });

        var sourcePackage = ReadEventSourcePackage(input);
        var lifecyclePackage = input.Lifecycle?.Packages
            .FirstOrDefault(package => package.Id == input.Request.StableId);
        var sourceMatches = input.Source?.Packages
            .Count(package => package.Id == input.Request.StableId) ?? 0;
        var subject = input.Source is null && input.Lifecycle is null
            ? new ExtensionInspectSubject
            {
                Supplied = input.Request.StableId,
                Form = ExtensionInspectSubjectForm.StableId,
                Id = input.Request.StableId,
                State = ExtensionInspectSubjectState.NotStarted,
                Candidates = [],
            }
            : ReadSubject(
                input.Request.StableId,
                ReadCandidates(input.Source?.Packages ?? [], input.Request.StableId),
                lifecyclePackage,
                sourceMatches,
                input.Source?.State ?? ExtensionSourceReadState.Cancelled);
        var result = Empty(
            input.Status,
            input.Request.Workspace,
            subject,
            [finding],
            ExtensionInspectDefinitions.ReadNext(
                input.Status,
                actionable: false,
                subjectId: input.Request.StableId));
        var currentPaths = input.CurrentPaths ?? [];
        var currentPathState = ReadEventCurrentPathState(input.CurrentPaths);
        return result with
        {
            Source = input.Source is null
                ? result.Source
                : ReadSource(input.Request, input.Source),
            Lifecycle = input.Lifecycle is null
                ? result.Lifecycle
                : ReadLifecycle(input.Lifecycle),
            Installed = input.Lifecycle is null
                ? result.Installed
                : ReadInstalled(input.Lifecycle, lifecyclePackage),
            Available = input.Source is null
                ? result.Available
                : ReadAvailable(input.Source, sourcePackage, sourceMatches),
            PathFacts = new ExtensionInspectPathFacts
            {
                State = currentPathState,
                Declared = [],
                Current = currentPaths,
            },
            Counts = result.Counts with
            {
                InstalledPackages = input.Lifecycle?.Packages.Count,
                AvailablePackages = input.Source?.Packages.Count,
                CurrentPaths = input.CurrentPaths?.Count,
            },
        };
    }

    private static ExtensionInspectPathState ReadEventCurrentPathState(
        IReadOnlyList<ExtensionInspectCurrentPath>? currentPaths)
    {
        if (currentPaths is null)
        {
            return ExtensionInspectPathState.NotStarted;
        }

        return currentPaths.All(path => path.State == ExtensionInspectCurrentPathState.Present)
            ? ExtensionInspectPathState.Complete
            : ExtensionInspectPathState.Incomplete;
    }

    private static ExtensionPackageFact? ReadEventSourcePackage(ExtensionInspectEventInput input)
    {
        if (input.Source is not { } source || !CanSelectSourcePackage(source))
        {
            return null;
        }

        var matches = source.Packages
            .Where(package => package.Id == input.Request.StableId)
            .Take(2)
            .ToArray();
        return matches.Length == 1 ? matches[0] : null;
    }

    private static ExtensionInspectResult Empty(
        CliSemanticStatus status,
        CliWorkspace? workspace,
        ExtensionInspectSubject subject,
        IReadOnlyList<ExtensionInspectFinding> findings,
        CliNextAction? next)
        => new()
        {
            Status = status,
            Workspace = workspace,
            Subject = subject,
            Source = new ExtensionInspectSource
            {
                Supplied = null,
                Explicit = false,
                Identity = null,
                Kind = null,
                State = ExtensionInspectSourceState.NotStarted,
            },
            Lifecycle = new ExtensionInspectLifecycle
            {
                DocumentPath = LifecycleDocumentReader.RelativePath,
                ReadState = ExtensionInspectLifecycleReadState.NotStarted,
                Trust = ExtensionInspectLifecycleTrust.NotStarted,
                Coverage = ExtensionInspectCoverageState.NotStarted,
                WorkspaceBinding = ExtensionInspectWorkspaceBinding.NotChecked,
                FingerprintPolicy = null,
            },
            Installed = new ExtensionInspectInstalled
            {
                State = ExtensionInspectInstalledState.NotStarted,
                Package = null,
            },
            Available = new ExtensionInspectAvailable
            {
                State = ExtensionInspectAvailableState.NotStarted,
                Package = null,
            },
            Dependencies = new ExtensionInspectDependencyClosure
            {
                State = ExtensionInspectDependencyState.NotStarted,
                Declared = [],
                Resolved = [],
                Order = [],
            },
            PathFacts = new ExtensionInspectPathFacts
            {
                State = ExtensionInspectPathState.NotStarted,
                Declared = [],
                Current = [],
            },
            Comparison = new ExtensionInspectComparison
            {
                State = ExtensionInspectComparisonState.NotStarted,
                Mode = ExtensionInspectComparisonMode.None,
                Baseline = Side(ExtensionInspectComparisonSideState.NotStarted, []),
                Current = Side(ExtensionInspectComparisonSideState.NotStarted, []),
                Intended = Side(ExtensionInspectComparisonSideState.NotStarted, []),
                Paths = [],
                Dependencies = new ExtensionInspectDependencyComparison
                {
                    State = ExtensionInspectDependencyComparisonState.NotStarted,
                    Baseline = [],
                    Current = [],
                    Intended = [],
                    Relation = ExtensionInspectDependencyRelation.NotStarted,
                },
            },
            Generated = new ExtensionInspectGenerated
            {
                State = ExtensionInspectGeneratedState.NotStarted,
                Ownership = GeneratedOwnership,
                Regions = [],
            },
            Findings = findings,
            Counts = new ExtensionInspectCounts
            {
                InstalledPackages = null,
                AvailablePackages = null,
                DeclaredPaths = null,
                CurrentPaths = null,
                BaselinePaths = null,
                IntendedPaths = null,
                Dependencies = null,
                UnchangedPaths = null,
                ChangedPaths = null,
                CurrentDivergedPaths = null,
                MissingPaths = null,
                NewPaths = null,
                RetiredPaths = null,
                SharedPaths = null,
                GeneratedRegions = null,
                ExcludedGeneratedBytes = null,
                Findings = findings.Count,
            },
            Next = next,
        };

    private static ExtensionInspectSubject ReadSubject(
        string subjectId,
        IReadOnlyList<ExtensionInspectCandidate> sourceCandidates,
        LifecycleInstalledPackage? lifecyclePackage,
        int sourceMatches,
        ExtensionSourceReadState sourceState)
    {
        var candidates = sourceCandidates;
        var ambiguous = sourceMatches > 1;
        var resolved = lifecyclePackage is not null || sourceMatches == 1;
        var state = ReadSubjectState(ambiguous, resolved, sourceState);
        return new ExtensionInspectSubject
        {
            Supplied = subjectId,
            Form = ExtensionInspectSubjectForm.StableId,
            Id = subjectId,
            State = state,
            Candidates = candidates,
        };
    }

    private static IReadOnlyList<ExtensionInspectCandidate> ReadCandidates(
        IEnumerable<ExtensionPackageFact> packages,
        string subjectId)
        => packages
            .Where(package => package.Id == subjectId)
            .Select(package => new ExtensionInspectCandidate
            {
                Id = package.Id,
                Path = package.ManifestPath,
            })
            .OrderBy(candidate => candidate.Id, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Path, StringComparer.Ordinal)
            .ToArray();

    private static ExtensionInspectSource ReadSource(
        ExtensionInspectRequest request,
        ExtensionSourceReadResult source)
        => new()
        {
            Supplied = request.ExplicitSource,
            Explicit = request.ExplicitSource is not null,
            Identity = ReadSourceIdentity(source),
            Kind = ReadSourceKind(source.Kind),
            State = ReadSourceState(source.State),
        };

    private static ExtensionInspectSubjectState ReadSubjectState(
        bool ambiguous,
        bool resolved,
        ExtensionSourceReadState sourceState)
    {
        if (ambiguous)
        {
            return ExtensionInspectSubjectState.Ambiguous;
        }

        if (resolved)
        {
            return ExtensionInspectSubjectState.Resolved;
        }

        return sourceState == ExtensionSourceReadState.Blocked
            ? ExtensionInspectSubjectState.Unsafe
            : ExtensionInspectSubjectState.Unknown;
    }

    private static string ReadSourceIdentity(ExtensionSourceReadResult source)
    {
        if (source.State == ExtensionSourceReadState.Complete
            && source.Kind == ExtensionSourceKind.EmbeddedCatalogue)
        {
            return EmbeddedIdentity;
        }

        return source.Identity;
    }

    private static ExtensionInspectSourceKind? ReadSourceKind(ExtensionSourceKind? kind)
        => kind is { } value ? ReadSourceKind(value) : null;

    private static ExtensionInspectLifecycle ReadLifecycle(LifecycleReadResult lifecycle)
        => new()
        {
            DocumentPath = LifecycleDocumentReader.RelativePath,
            ReadState = ReadLifecycleState(lifecycle),
            Trust = ReadLifecycleTrust(lifecycle.Trust),
            Coverage = ReadCoverage(lifecycle.Coverage),
            WorkspaceBinding = ReadWorkspaceBinding(lifecycle.WorkspaceBinding),
            FingerprintPolicy = lifecycle.FingerprintPolicy == ExtensionInspectDefinitions.FingerprintPolicy
                ? lifecycle.FingerprintPolicy
                : null,
        };

    private static ExtensionInspectInstalled ReadInstalled(
        LifecycleReadResult lifecycle,
        LifecycleInstalledPackage? package)
    {
        var state = ReadInstalledState(lifecycle, package);
        return new ExtensionInspectInstalled
        {
            State = state,
            Package = ReadInstalledPackage(package),
        };
    }

    private static ExtensionInspectInstalledState ReadInstalledState(
        LifecycleReadResult lifecycle,
        LifecycleInstalledPackage? package)
    {
        if (package is not null)
        {
            return ExtensionInspectInstalledState.Present;
        }

        if (lifecycle.Trust is LifecycleExtensionTrust.Absent
            && lifecycle.Coverage == LifecycleCoverageState.Complete)
        {
            return ExtensionInspectInstalledState.Absent;
        }

        if (lifecycle.State == LifecycleReadState.Cancelled)
        {
            return ExtensionInspectInstalledState.Interrupted;
        }

        if (lifecycle.Trust == LifecycleExtensionTrust.Blocked)
        {
            return ExtensionInspectInstalledState.Blocked;
        }

        if (lifecycle.State == LifecycleReadState.Invalid)
        {
            return ExtensionInspectInstalledState.Invalid;
        }

        return ExtensionInspectInstalledState.Unavailable;
    }

    private static ExtensionInspectInstalledPackage? ReadInstalledPackage(
        LifecycleInstalledPackage? package)
    {
        if (package is null)
        {
            return null;
        }

        return new ExtensionInspectInstalledPackage
        {
            Id = package.Id,
            Version = package.Version,
            Source = package.Source,
            Dependencies = package.Dependencies.ToArray(),
            Paths = package.Paths.Order(StringComparer.Ordinal).ToArray(),
        };
    }

    private static ExtensionInspectAvailable ReadAvailable(
        ExtensionSourceReadResult source,
        ExtensionPackageFact? package,
        int sourceMatches)
    {
        var state = ReadAvailableState(source, package, sourceMatches);
        return new ExtensionInspectAvailable
        {
            State = state,
            Package = ReadAvailablePackage(package),
        };
    }

    private static ExtensionInspectAvailableState ReadAvailableState(
        ExtensionSourceReadResult source,
        ExtensionPackageFact? package,
        int sourceMatches)
    {
        if (source.State != ExtensionSourceReadState.Complete)
        {
            return ReadAvailableState(source.State);
        }

        if (sourceMatches > 1)
        {
            return ExtensionInspectAvailableState.Blocked;
        }

        return package is null
            ? ExtensionInspectAvailableState.Absent
            : ExtensionInspectAvailableState.Present;
    }

    private static ExtensionInspectAvailablePackage? ReadAvailablePackage(
        ExtensionPackageFact? package)
    {
        if (package is null)
        {
            return null;
        }

        return new ExtensionInspectAvailablePackage
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Version = package.Version,
            ManifestPath = package.ManifestPath,
            Dependencies = package.Dependencies.ToArray(),
            Payload = package.Payload
                .Select(file => new ExtensionInspectPackageFile
                {
                    Path = file.Path,
                    TargetPath = file.TargetPath,
                    State = ReadPackageFileState(file.State),
                    ByteLength = file.ByteLength,
                    Sha256 = file.Sha256,
                    Bytes = file.Bytes,
                })
                .ToArray(),
        };
    }

    private static void AddBoundaryFindings(
        string subjectId,
        ExtensionSourceReadResult source,
        LifecycleReadResult lifecycle,
        LifecycleInstalledPackage? lifecyclePackage,
        ExtensionPackageFact? sourcePackage,
        int sourceMatches,
        ICollection<ExtensionInspectFinding> findings)
    {
        if (source.State != ExtensionSourceReadState.Complete)
        {
            var code = source.FailureKind switch
            {
                ExtensionSourceFailureKind.Overlap => ExtensionInspectFindingCode.SourceOverlap,
                ExtensionSourceFailureKind.Ambiguous => ExtensionInspectFindingCode.SourceAmbiguous,
                ExtensionSourceFailureKind.IdentityAmbiguous => ExtensionInspectFindingCode.IdentityAmbiguous,
                ExtensionSourceFailureKind.DependencyCycle => ExtensionInspectFindingCode.DependencyCycle,
                ExtensionSourceFailureKind.DependencyConflict => ExtensionInspectFindingCode.DependencyConflict,
                ExtensionSourceFailureKind.DependencyIncomplete => ExtensionInspectFindingCode.DependencyIncomplete,
                ExtensionSourceFailureKind.PackageInvalid => ExtensionInspectFindingCode.PackageInvalid,
                _ when source.State is ExtensionSourceReadState.Missing or ExtensionSourceReadState.Unavailable
                    => ExtensionInspectFindingCode.SourceUnavailable,
                _ => ExtensionInspectFindingCode.SourceInvalid,
            };
            AddFinding(findings, new ExtensionInspectFindingInput
            {
                Code = code,
                Subject = source.Identity,
                Cause = source.Cause ?? "The selected Extension source is unavailable.",
            });
        }

        if (lifecycle.State == LifecycleReadState.Cancelled)
        {
            AddFinding(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.Interrupted,
                Subject = LifecycleDocumentReader.RelativePath,
                Cause = lifecycle.Cause ?? "Lifecycle inspection was interrupted.",
            });
        }
        else if (lifecycle.Trust == LifecycleExtensionTrust.Blocked)
        {
            AddFinding(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.LifecycleBlocked,
                Subject = LifecycleDocumentReader.RelativePath,
                Cause = lifecycle.Cause ?? "Extension lifecycle evidence is blocked.",
            });
        }
        else if (lifecycle.State is LifecycleReadState.Missing or LifecycleReadState.Unavailable)
        {
            AddFinding(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.LifecycleUnavailable,
                Subject = LifecycleDocumentReader.RelativePath,
                Cause = lifecycle.Cause ?? "Extension lifecycle evidence is unavailable.",
            });
        }
        else if (lifecycle.State == LifecycleReadState.Invalid)
        {
            AddFinding(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.LifecycleInvalid,
                Subject = LifecycleDocumentReader.RelativePath,
                Cause = lifecycle.Cause ?? "Extension lifecycle evidence is invalid.",
            });
        }

        if (source.State == ExtensionSourceReadState.Complete && sourceMatches > 1)
        {
            AddFinding(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.IdentityAmbiguous,
                Subject = subjectId,
                PackageId = subjectId,
                Cause = "The selected source contains more than one active package identity for the supplied stable ID.",
                Candidates = ReadCandidates(source.Packages, subjectId),
            });
        }

        if (source.State == ExtensionSourceReadState.Complete && sourcePackage is null)
        {
            AddFinding(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.PackageUnavailable,
                Subject = subjectId,
                PackageId = subjectId,
                Cause = "The requested package is not available from the selected source.",
            });
        }
    }

    private static ExtensionInspectDependencyClosure ReadDependencies(
        ExtensionInspectDependencyInput input)
    {
        var source = input.Source;
        var package = input.SourcePackage;
        var installedPackage = input.InstalledPackage;
        var subjectId = input.SubjectId;
        var findings = input.Findings;
        if (package is not null)
        {
            var sourceNodes = source.Packages
                .Select(value => new ExtensionInspectDependencyNode(
                    Id: value.Id,
                    Dependencies: value.Dependencies,
                    Version: value.Version,
                    Source: ReadSourceIdentity(source)))
                .ToArray();
            return ReadDependencyClosure(
                sourceNodes,
                package.Id,
                ReadSourceIdentity(source),
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
                    AddFinding(findings, new ExtensionInspectFindingInput
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
            AddFinding(findings, new ExtensionInspectFindingInput
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

    private static bool CanSelectSourcePackage(ExtensionSourceReadResult source)
        => source.State == ExtensionSourceReadState.Complete
            || source.FailureKind is ExtensionSourceFailureKind.DependencyIncomplete
                or ExtensionSourceFailureKind.DependencyCycle;

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

    private static IReadOnlyList<ExtensionPackageFact> ReadSourceClosure(
        ExtensionSourceReadResult source,
        ExtensionPackageFact? package,
        ExtensionInspectDependencyClosure dependencies)
    {
        if (package is null)
        {
            return [];
        }

        var ids = dependencies.Resolved
            .Where(value => value.State == ExtensionInspectDependencyPackageState.Available)
            .Select(value => value.Id)
            .ToHashSet(StringComparer.Ordinal);
        ids.Add(package.Id);
        return source.Packages
            .Where(value => ids.Contains(value.Id))
            .OrderBy(value => value.Id, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<ExtensionInspectDeclaredPathProjection> ReadSourcePathProjections(
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
                AddFinding(findings, new ExtensionInspectFindingInput
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
        var state = ReadDeclaredPathState(file.State);
        if (state is ExtensionInspectDeclaredPathState.Invalid or ExtensionInspectDeclaredPathState.Blocked)
        {
            AddFinding(findings, new ExtensionInspectFindingInput
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
            AddFinding(findings, new ExtensionInspectFindingInput
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
        if (files.Any(value => ReadDeclaredPathState(value.File.State) == ExtensionInspectDeclaredPathState.Invalid))
        {
            return ExtensionInspectDeclaredPathState.Invalid;
        }

        if (files.Any(value => ReadDeclaredPathState(value.File.State) == ExtensionInspectDeclaredPathState.Blocked))
        {
            return ExtensionInspectDeclaredPathState.Blocked;
        }

        if (files.Any(value => ReadDeclaredPathState(value.File.State) == ExtensionInspectDeclaredPathState.Unavailable))
        {
            return ExtensionInspectDeclaredPathState.Unavailable;
        }

        if (files.Any(value => ReadDeclaredPathState(value.File.State) == ExtensionInspectDeclaredPathState.Missing))
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
                    AddFinding(findings, new ExtensionInspectFindingInput
                    {
                        Code = ExtensionInspectFindingCode.PathMissing,
                        Subject = subjectId,
                        PackageId = subjectId,
                        Path = path.Path,
                        Cause = "A trusted managed path is missing from the current workspace.",
                    });
                    break;
                case ExtensionInspectCurrentPathState.Blocked:
                    AddFinding(findings, new ExtensionInspectFindingInput
                    {
                        Code = ExtensionInspectFindingCode.PathInvalid,
                        Subject = subjectId,
                        PackageId = subjectId,
                        Path = path.Path,
                        Cause = "The current managed path is outside the safe workspace boundary or unavailable.",
                    });
                    break;
                case ExtensionInspectCurrentPathState.Unavailable:
                    AddFinding(findings, new ExtensionInspectFindingInput
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
        ExtensionPackageFact? sourcePackage,
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

        return sourcePackage is not null || lifecyclePackage is not null
            ? ExtensionInspectPathState.Complete
            : ExtensionInspectPathState.NotStarted;
    }

    private static ExtensionInspectSourceKind ReadSourceKind(ExtensionSourceKind kind)
        => kind switch
        {
            ExtensionSourceKind.EmbeddedCatalogue => ExtensionInspectSourceKind.EmbeddedCatalogue,
            ExtensionSourceKind.Package => ExtensionInspectSourceKind.Package,
            ExtensionSourceKind.Catalogue => ExtensionInspectSourceKind.Catalogue,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Extension source kind is not defined."),
        };

    private static ExtensionInspectSourceState ReadSourceState(ExtensionSourceReadState state)
        => state switch
        {
            ExtensionSourceReadState.Complete => ExtensionInspectSourceState.Available,
            ExtensionSourceReadState.Missing => ExtensionInspectSourceState.Missing,
            ExtensionSourceReadState.Invalid => ExtensionInspectSourceState.Invalid,
            ExtensionSourceReadState.Blocked => ExtensionInspectSourceState.Blocked,
            ExtensionSourceReadState.Unavailable => ExtensionInspectSourceState.Unavailable,
            ExtensionSourceReadState.Cancelled => ExtensionInspectSourceState.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Extension source state is not defined."),
        };

    private static ExtensionInspectLifecycleReadState ReadLifecycleState(LifecycleReadResult lifecycle)
    {
        if (lifecycle.Trust == LifecycleExtensionTrust.Blocked
            || lifecycle.Coverage == LifecycleCoverageState.Blocked)
        {
            return ExtensionInspectLifecycleReadState.Blocked;
        }

        return lifecycle.State switch
        {
            LifecycleReadState.Complete => ExtensionInspectLifecycleReadState.Complete,
            LifecycleReadState.Missing => ExtensionInspectLifecycleReadState.Missing,
            LifecycleReadState.Invalid => ExtensionInspectLifecycleReadState.Invalid,
            LifecycleReadState.Unavailable => ExtensionInspectLifecycleReadState.Unavailable,
            LifecycleReadState.Cancelled => ExtensionInspectLifecycleReadState.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle.State, "The lifecycle read state is not defined."),
        };
    }

    private static ExtensionInspectLifecycleTrust ReadLifecycleTrust(LifecycleExtensionTrust trust)
        => trust switch
        {
            LifecycleExtensionTrust.Trusted => ExtensionInspectLifecycleTrust.Trusted,
            LifecycleExtensionTrust.Untrusted => ExtensionInspectLifecycleTrust.Untrusted,
            LifecycleExtensionTrust.Incomplete => ExtensionInspectLifecycleTrust.Incomplete,
            LifecycleExtensionTrust.Blocked => ExtensionInspectLifecycleTrust.Blocked,
            LifecycleExtensionTrust.Absent => ExtensionInspectLifecycleTrust.Absent,
            _ => throw new ArgumentOutOfRangeException(nameof(trust), trust, "The lifecycle trust is not defined."),
        };

    private static ExtensionInspectCoverageState ReadCoverage(LifecycleCoverageState coverage)
        => coverage switch
        {
            LifecycleCoverageState.NotStarted => ExtensionInspectCoverageState.NotStarted,
            LifecycleCoverageState.Complete => ExtensionInspectCoverageState.Complete,
            LifecycleCoverageState.Incomplete => ExtensionInspectCoverageState.Incomplete,
            LifecycleCoverageState.Blocked => ExtensionInspectCoverageState.Blocked,
            LifecycleCoverageState.Failed => ExtensionInspectCoverageState.Failed,
            LifecycleCoverageState.Interrupted => ExtensionInspectCoverageState.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(coverage), coverage, "The lifecycle coverage is not defined."),
        };

    private static ExtensionInspectWorkspaceBinding ReadWorkspaceBinding(LifecycleWorkspaceBinding binding)
        => binding switch
        {
            LifecycleWorkspaceBinding.NotChecked => ExtensionInspectWorkspaceBinding.NotChecked,
            LifecycleWorkspaceBinding.Matched => ExtensionInspectWorkspaceBinding.Matched,
            LifecycleWorkspaceBinding.Mismatched => ExtensionInspectWorkspaceBinding.Mismatched,
            LifecycleWorkspaceBinding.Unavailable => ExtensionInspectWorkspaceBinding.Unavailable,
            _ => throw new ArgumentOutOfRangeException(nameof(binding), binding, "The lifecycle workspace binding is not defined."),
        };

    private static ExtensionInspectAvailableState ReadAvailableState(ExtensionSourceReadState state)
        => state switch
        {
            ExtensionSourceReadState.Complete => ExtensionInspectAvailableState.Present,
            ExtensionSourceReadState.Missing => ExtensionInspectAvailableState.Unavailable,
            ExtensionSourceReadState.Invalid => ExtensionInspectAvailableState.Invalid,
            ExtensionSourceReadState.Blocked => ExtensionInspectAvailableState.Blocked,
            ExtensionSourceReadState.Unavailable => ExtensionInspectAvailableState.Unavailable,
            ExtensionSourceReadState.Cancelled => ExtensionInspectAvailableState.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Extension source state is not defined."),
        };

    private static ExtensionInspectPackageFileState ReadPackageFileState(ExtensionPackageFileReadState state)
        => state switch
        {
            ExtensionPackageFileReadState.Available => ExtensionInspectPackageFileState.Available,
            ExtensionPackageFileReadState.Missing => ExtensionInspectPackageFileState.Missing,
            ExtensionPackageFileReadState.Invalid => ExtensionInspectPackageFileState.Invalid,
            ExtensionPackageFileReadState.Blocked => ExtensionInspectPackageFileState.Blocked,
            ExtensionPackageFileReadState.Unavailable => ExtensionInspectPackageFileState.Unavailable,
            ExtensionPackageFileReadState.Cancelled => ExtensionInspectPackageFileState.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Extension package-file state is not defined."),
        };

    private static ExtensionInspectDeclaredPathState ReadDeclaredPathState(ExtensionPackageFileReadState state)
        => state switch
        {
            ExtensionPackageFileReadState.Available => ExtensionInspectDeclaredPathState.Available,
            ExtensionPackageFileReadState.Missing => ExtensionInspectDeclaredPathState.Missing,
            ExtensionPackageFileReadState.Invalid => ExtensionInspectDeclaredPathState.Invalid,
            ExtensionPackageFileReadState.Blocked => ExtensionInspectDeclaredPathState.Blocked,
            ExtensionPackageFileReadState.Unavailable => ExtensionInspectDeclaredPathState.Unavailable,
            ExtensionPackageFileReadState.Cancelled => ExtensionInspectDeclaredPathState.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Extension package-file state is not defined."),
        };

    private static ExtensionInspectComparisonSide Side(
        ExtensionInspectComparisonSideState state,
        IReadOnlyList<ExtensionInspectFingerprintFact> facts)
        => new()
        {
            State = state,
            Fingerprints = facts,
        };
}
