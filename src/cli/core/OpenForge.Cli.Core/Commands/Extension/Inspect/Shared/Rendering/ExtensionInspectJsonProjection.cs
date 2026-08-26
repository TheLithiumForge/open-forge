using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Presentation;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;

internal static class ExtensionInspectJsonProjection
{
    internal static ExtensionInspectJsonDocument Create(ExtensionInspectResult result)
        => new()
        {
            SchemaVersion = ExtensionInspectDefinitions.SchemaVersion,
            Command = result.Command,
            Status = CliStatusDefinitions.Read(result.Status).MachineName,
            Workspace = result.Workspace is null ? null : Workspace(result.Workspace),
            Result = new ExtensionInspectJsonResult
            {
                Subject = Subject(result.Subject),
                Source = Source(result.Source),
                Lifecycle = Lifecycle(result.Lifecycle),
                Installed = Installed(result.Installed),
                Available = Available(result.Available),
                Dependencies = Dependencies(result.Dependencies),
                PathFacts = PathFacts(result.PathFacts),
                Comparison = Comparison(result.Comparison),
                Generated = Generated(result.Generated),
                Findings = result.Findings.Select(Finding).ToArray(),
                Counts = Counts(result.Counts),
            },
            Next = result.Next is null
                ? null
                : new ExtensionInspectJsonNext
                {
                    Command = result.Next.Command,
                    Reason = result.Next.Reason,
                },
        };

    private static ExtensionInspectJsonWorkspace Workspace(CliWorkspace workspace)
        => new()
        {
            Path = workspace.LexicalRoot,
            SelectedBy = workspace.SelectedBy switch
            {
                CliWorkspaceSelectionMethod.CurrentDirectory => "current-directory",
                CliWorkspaceSelectionMethod.ExplicitWorkspace => "explicit-workspace",
                _ => throw new ArgumentOutOfRangeException(nameof(workspace), workspace.SelectedBy, "The workspace selection method is not defined."),
            },
        };

    private static ExtensionInspectJsonSubject Subject(ExtensionInspectSubject value)
        => new()
        {
            Supplied = value.Supplied,
            Form = value.Form is null ? null : SubjectForm(value.Form.Value),
            Id = value.Id,
            State = SubjectState(value.State),
            Candidates = value.Candidates.Select(Candidate).ToArray(),
        };

    private static ExtensionInspectJsonCandidate Candidate(ExtensionInspectCandidate value)
        => new() { Id = value.Id, Path = value.Path };

    private static ExtensionInspectJsonSource Source(ExtensionInspectSource value)
        => new()
        {
            Supplied = value.Supplied,
            Explicit = value.Explicit,
            Identity = value.Identity,
            Kind = value.Kind is null ? null : SourceKind(value.Kind.Value),
            State = SourceState(value.State),
        };

    private static ExtensionInspectJsonLifecycle Lifecycle(ExtensionInspectLifecycle value)
        => new()
        {
            DocumentPath = value.DocumentPath,
            ReadState = LifecycleReadState(value.ReadState),
            Trust = LifecycleTrust(value.Trust),
            Coverage = Coverage(value.Coverage),
            WorkspaceBinding = WorkspaceBinding(value.WorkspaceBinding),
            FingerprintPolicy = value.FingerprintPolicy,
        };

    private static ExtensionInspectJsonInstalled Installed(ExtensionInspectInstalled value)
        => new()
        {
            State = InstalledState(value.State),
            Package = value.Package is null ? null : new ExtensionInspectJsonInstalledPackage
            {
                Id = value.Package.Id,
                Version = value.Package.Version,
                Source = value.Package.Source,
                Dependencies = value.Package.Dependencies.ToArray(),
                Paths = value.Package.Paths.ToArray(),
            },
        };

    private static ExtensionInspectJsonAvailable Available(ExtensionInspectAvailable value)
        => new()
        {
            State = AvailableState(value.State),
            Package = value.Package is null ? null : new ExtensionInspectJsonAvailablePackage
            {
                Id = value.Package.Id,
                Name = value.Package.Name,
                Description = value.Package.Description,
                Version = value.Package.Version,
                ManifestPath = value.Package.ManifestPath,
                Dependencies = value.Package.Dependencies.ToArray(),
                Payload = value.Package.Payload.Select(file => new ExtensionInspectJsonPackageFile
                {
                    Path = file.Path,
                    TargetPath = file.TargetPath,
                    State = PackageFileState(file.State),
                    ByteLength = file.ByteLength,
                    Sha256 = file.Sha256,
                }).ToArray(),
            },
        };

    private static ExtensionInspectJsonDependencyClosure Dependencies(ExtensionInspectDependencyClosure value)
        => new()
        {
            State = DependencyState(value.State),
            Declared = value.Declared.Select(edge => new ExtensionInspectJsonDependencyEdge
            {
                From = edge.From,
                To = edge.To,
                Position = edge.Position,
            }).ToArray(),
            Resolved = value.Resolved.Select(package => new ExtensionInspectJsonDependencyPackage
            {
                Id = package.Id,
                Version = package.Version,
                Source = package.Source,
                State = DependencyPackageState(package.State),
            }).ToArray(),
            Order = value.Order.ToArray(),
        };

    private static ExtensionInspectJsonPathFacts PathFacts(ExtensionInspectPathFacts value)
        => new()
        {
            State = PathState(value.State),
            Declared = value.Declared.Select(path => new ExtensionInspectJsonDeclaredPath
            {
                Path = path.Path,
                SourcePath = path.SourcePath,
                State = DeclaredPathState(path.State),
            }).ToArray(),
            Current = value.Current.Select(path => new ExtensionInspectJsonCurrentPath
            {
                Path = path.Path,
                State = CurrentPathState(path.State),
                PhysicalIdentity = path.PhysicalIdentity,
                ByteLength = path.ByteLength,
                ExactSha256 = path.ExactSha256,
            }).ToArray(),
        };

    private static ExtensionInspectJsonComparison Comparison(ExtensionInspectComparison value)
        => new()
        {
            State = ComparisonState(value.State),
            Mode = ComparisonMode(value.Mode),
            Baseline = Side(value.Baseline),
            Current = Side(value.Current),
            Intended = Side(value.Intended),
            Paths = value.Paths.Select(PathComparison).ToArray(),
            Dependencies = DependencyComparison(value.Dependencies),
        };

    private static ExtensionInspectJsonComparisonSide Side(ExtensionInspectComparisonSide value)
        => new()
        {
            State = ComparisonSideState(value.State),
            Fingerprints = value.Fingerprints.Select(FingerprintFact).ToArray(),
        };

    private static ExtensionInspectJsonFingerprintFact FingerprintFact(ExtensionInspectFingerprintFact value)
        => new()
        {
            Path = value.Path,
            Fingerprint = value.Fingerprint is null ? null : Fingerprint(value.Fingerprint),
        };

    private static ExtensionInspectJsonFingerprint Fingerprint(ExtensionInspectFingerprint value)
        => new()
        {
            Kind = FingerprintKind(value.Kind),
            Policy = value.Policy,
            Sha256 = value.Sha256,
            Origin = FingerprintOrigin(value.Origin),
        };

    private static ExtensionInspectJsonPathComparison PathComparison(ExtensionInspectPathComparison value)
        => new()
        {
            Path = value.Path,
            Baseline = value.Baseline is null ? null : Fingerprint(value.Baseline),
            Current = value.Current is null ? null : Fingerprint(value.Current),
            Intended = value.Intended is null ? null : Fingerprint(value.Intended),
            Relation = PathRelation(value.Relation),
            BaselineOwners = value.BaselineOwners.ToArray(),
            CurrentOwners = value.CurrentOwners.ToArray(),
            IntendedOwners = value.IntendedOwners.ToArray(),
        };

    private static ExtensionInspectJsonDependencyComparison DependencyComparison(ExtensionInspectDependencyComparison value)
        => new()
        {
            State = DependencyComparisonState(value.State),
            Baseline = value.Baseline.ToArray(),
            Current = value.Current.ToArray(),
            Intended = value.Intended.ToArray(),
            Relation = DependencyRelation(value.Relation),
        };

    private static ExtensionInspectJsonGenerated Generated(ExtensionInspectGenerated value)
        => new()
        {
            State = GeneratedState(value.State),
            Ownership = value.Ownership,
            Regions = value.Regions.Select(region => new ExtensionInspectJsonGeneratedRegion
            {
                Path = region.Path,
                State = GeneratedRegionState(region.State),
                StartMarker = region.StartMarker,
                EndMarker = region.EndMarker,
                StartByteOffset = region.StartByteOffset,
                EndByteOffset = region.EndByteOffset,
                ExcludedInteriorByteLength = region.ExcludedInteriorByteLength,
                MarkerLinesRetained = region.MarkerLinesRetained,
            }).ToArray(),
        };

    private static ExtensionInspectJsonFinding Finding(ExtensionInspectFinding value)
        => new()
        {
            Code = ExtensionInspectDefinitions.ReadFindingCode(value.Code),
            Status = CliStatusDefinitions.Read(value.Status).MachineName,
            Subject = value.Subject,
            PackageId = value.PackageId,
            Dependency = value.Dependency,
            Path = value.Path,
            Cause = value.Cause,
            Location = value.Location is null ? null : Location(value.Location),
            Candidates = value.Candidates.Select(Candidate).ToArray(),
        };

    private static ExtensionInspectJsonLocation Location(SourceLocation value)
        => new()
        {
            Line = value.Line,
            Column = value.Column,
            ByteOffset = value.ByteOffset,
            ByteLength = value.ByteLength,
        };

    private static ExtensionInspectJsonCounts Counts(ExtensionInspectCounts value)
        => new()
        {
            InstalledPackages = value.InstalledPackages,
            AvailablePackages = value.AvailablePackages,
            DeclaredPaths = value.DeclaredPaths,
            CurrentPaths = value.CurrentPaths,
            BaselinePaths = value.BaselinePaths,
            IntendedPaths = value.IntendedPaths,
            Dependencies = value.Dependencies,
            UnchangedPaths = value.UnchangedPaths,
            ChangedPaths = value.ChangedPaths,
            CurrentDivergedPaths = value.CurrentDivergedPaths,
            MissingPaths = value.MissingPaths,
            NewPaths = value.NewPaths,
            RetiredPaths = value.RetiredPaths,
            SharedPaths = value.SharedPaths,
            GeneratedRegions = value.GeneratedRegions,
            ExcludedGeneratedBytes = value.ExcludedGeneratedBytes,
            Findings = value.Findings,
        };

    internal static string SubjectForm(ExtensionInspectSubjectForm value)
        => value switch
        {
            ExtensionInspectSubjectForm.StableId => "stable-id",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The subject form is not defined."),
        };

    internal static string SubjectState(ExtensionInspectSubjectState value)
        => value switch
        {
            ExtensionInspectSubjectState.NotStarted => "not-started",
            ExtensionInspectSubjectState.Resolved => "resolved",
            ExtensionInspectSubjectState.Invalid => "invalid",
            ExtensionInspectSubjectState.Unknown => "unknown",
            ExtensionInspectSubjectState.Ambiguous => "ambiguous",
            ExtensionInspectSubjectState.Unsafe => "unsafe",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The subject state is not defined."),
        };

    internal static string SourceKind(ExtensionInspectSourceKind value)
        => value switch
        {
            ExtensionInspectSourceKind.EmbeddedCatalogue => "embedded-catalogue",
            ExtensionInspectSourceKind.Package => "package",
            ExtensionInspectSourceKind.Catalogue => "catalogue",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The source kind is not defined."),
        };

    internal static string SourceState(ExtensionInspectSourceState value)
        => value switch
        {
            ExtensionInspectSourceState.NotStarted => "not-started",
            ExtensionInspectSourceState.Available => "available",
            ExtensionInspectSourceState.Missing => "missing",
            ExtensionInspectSourceState.Invalid => "invalid",
            ExtensionInspectSourceState.Blocked => "blocked",
            ExtensionInspectSourceState.Unavailable => "unavailable",
            ExtensionInspectSourceState.Failed => "failed",
            ExtensionInspectSourceState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The source state is not defined."),
        };

    internal static string LifecycleReadState(ExtensionInspectLifecycleReadState value)
        => value switch
        {
            ExtensionInspectLifecycleReadState.NotStarted => "not-started",
            ExtensionInspectLifecycleReadState.Complete => "complete",
            ExtensionInspectLifecycleReadState.Missing => "missing",
            ExtensionInspectLifecycleReadState.Invalid => "invalid",
            ExtensionInspectLifecycleReadState.Unavailable => "unavailable",
            ExtensionInspectLifecycleReadState.Blocked => "blocked",
            ExtensionInspectLifecycleReadState.Failed => "failed",
            ExtensionInspectLifecycleReadState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The lifecycle read state is not defined."),
        };

    internal static string LifecycleTrust(ExtensionInspectLifecycleTrust value)
        => value switch
        {
            ExtensionInspectLifecycleTrust.NotStarted => "not-started",
            ExtensionInspectLifecycleTrust.Trusted => "trusted",
            ExtensionInspectLifecycleTrust.Untrusted => "untrusted",
            ExtensionInspectLifecycleTrust.Incomplete => "incomplete",
            ExtensionInspectLifecycleTrust.Blocked => "blocked",
            ExtensionInspectLifecycleTrust.Absent => "absent",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The lifecycle trust is not defined."),
        };

    internal static string Coverage(ExtensionInspectCoverageState value)
        => value switch
        {
            ExtensionInspectCoverageState.NotStarted => "not-started",
            ExtensionInspectCoverageState.Complete => "complete",
            ExtensionInspectCoverageState.Incomplete => "incomplete",
            ExtensionInspectCoverageState.Invalid => "invalid",
            ExtensionInspectCoverageState.Blocked => "blocked",
            ExtensionInspectCoverageState.Failed => "failed",
            ExtensionInspectCoverageState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The coverage state is not defined."),
        };

    internal static string WorkspaceBinding(ExtensionInspectWorkspaceBinding value)
        => value switch
        {
            ExtensionInspectWorkspaceBinding.NotChecked => "not-checked",
            ExtensionInspectWorkspaceBinding.Matched => "matched",
            ExtensionInspectWorkspaceBinding.Mismatched => "mismatched",
            ExtensionInspectWorkspaceBinding.Unavailable => "unavailable",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The workspace binding is not defined."),
        };

    internal static string InstalledState(ExtensionInspectInstalledState value)
        => value switch
        {
            ExtensionInspectInstalledState.NotStarted => "not-started",
            ExtensionInspectInstalledState.Present => "present",
            ExtensionInspectInstalledState.Absent => "absent",
            ExtensionInspectInstalledState.Unavailable => "unavailable",
            ExtensionInspectInstalledState.Invalid => "invalid",
            ExtensionInspectInstalledState.Blocked => "blocked",
            ExtensionInspectInstalledState.Failed => "failed",
            ExtensionInspectInstalledState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The installed state is not defined."),
        };

    internal static string AvailableState(ExtensionInspectAvailableState value)
        => value switch
        {
            ExtensionInspectAvailableState.NotStarted => "not-started",
            ExtensionInspectAvailableState.Present => "present",
            ExtensionInspectAvailableState.Absent => "absent",
            ExtensionInspectAvailableState.Unavailable => "unavailable",
            ExtensionInspectAvailableState.Invalid => "invalid",
            ExtensionInspectAvailableState.Blocked => "blocked",
            ExtensionInspectAvailableState.Failed => "failed",
            ExtensionInspectAvailableState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The available state is not defined."),
        };

    internal static string PackageFileState(ExtensionInspectPackageFileState value)
        => value switch
        {
            ExtensionInspectPackageFileState.NotStarted => "not-started",
            ExtensionInspectPackageFileState.Available => "available",
            ExtensionInspectPackageFileState.Missing => "missing",
            ExtensionInspectPackageFileState.Invalid => "invalid",
            ExtensionInspectPackageFileState.Blocked => "blocked",
            ExtensionInspectPackageFileState.Unavailable => "unavailable",
            ExtensionInspectPackageFileState.Failed => "failed",
            ExtensionInspectPackageFileState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The package file state is not defined."),
        };

    internal static string DependencyState(ExtensionInspectDependencyState value)
        => value switch
        {
            ExtensionInspectDependencyState.NotStarted => "not-started",
            ExtensionInspectDependencyState.Complete => "complete",
            ExtensionInspectDependencyState.Incomplete => "incomplete",
            ExtensionInspectDependencyState.Invalid => "invalid",
            ExtensionInspectDependencyState.Blocked => "blocked",
            ExtensionInspectDependencyState.Failed => "failed",
            ExtensionInspectDependencyState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The dependency state is not defined."),
        };

    internal static string DependencyPackageState(ExtensionInspectDependencyPackageState value)
        => value switch
        {
            ExtensionInspectDependencyPackageState.NotStarted => "not-started",
            ExtensionInspectDependencyPackageState.Available => "available",
            ExtensionInspectDependencyPackageState.Missing => "missing",
            ExtensionInspectDependencyPackageState.Invalid => "invalid",
            ExtensionInspectDependencyPackageState.Blocked => "blocked",
            ExtensionInspectDependencyPackageState.Unavailable => "unavailable",
            ExtensionInspectDependencyPackageState.Failed => "failed",
            ExtensionInspectDependencyPackageState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The dependency package state is not defined."),
        };

    internal static string PathState(ExtensionInspectPathState value)
        => value switch
        {
            ExtensionInspectPathState.NotStarted => "not-started",
            ExtensionInspectPathState.Complete => "complete",
            ExtensionInspectPathState.Incomplete => "incomplete",
            ExtensionInspectPathState.Invalid => "invalid",
            ExtensionInspectPathState.Blocked => "blocked",
            ExtensionInspectPathState.Failed => "failed",
            ExtensionInspectPathState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The path state is not defined."),
        };

    internal static string DeclaredPathState(ExtensionInspectDeclaredPathState value)
        => value switch
        {
            ExtensionInspectDeclaredPathState.NotStarted => "not-started",
            ExtensionInspectDeclaredPathState.Available => "available",
            ExtensionInspectDeclaredPathState.Missing => "missing",
            ExtensionInspectDeclaredPathState.Invalid => "invalid",
            ExtensionInspectDeclaredPathState.Blocked => "blocked",
            ExtensionInspectDeclaredPathState.Unavailable => "unavailable",
            ExtensionInspectDeclaredPathState.Failed => "failed",
            ExtensionInspectDeclaredPathState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The declared path state is not defined."),
        };

    internal static string CurrentPathState(ExtensionInspectCurrentPathState value)
        => value switch
        {
            ExtensionInspectCurrentPathState.NotStarted => "not-started",
            ExtensionInspectCurrentPathState.Present => "present",
            ExtensionInspectCurrentPathState.Missing => "missing",
            ExtensionInspectCurrentPathState.Invalid => "invalid",
            ExtensionInspectCurrentPathState.Blocked => "blocked",
            ExtensionInspectCurrentPathState.Unavailable => "unavailable",
            ExtensionInspectCurrentPathState.Failed => "failed",
            ExtensionInspectCurrentPathState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The current path state is not defined."),
        };

    internal static string ComparisonState(ExtensionInspectComparisonState value)
        => value switch
        {
            ExtensionInspectComparisonState.NotStarted => "not-started",
            ExtensionInspectComparisonState.Complete => "complete",
            ExtensionInspectComparisonState.Incomplete => "incomplete",
            ExtensionInspectComparisonState.Invalid => "invalid",
            ExtensionInspectComparisonState.Blocked => "blocked",
            ExtensionInspectComparisonState.Failed => "failed",
            ExtensionInspectComparisonState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The comparison state is not defined."),
        };

    internal static string ComparisonMode(ExtensionInspectComparisonMode value)
        => value switch
        {
            ExtensionInspectComparisonMode.None => "none",
            ExtensionInspectComparisonMode.InstalledOnly => "installed-only",
            ExtensionInspectComparisonMode.AvailableOnly => "available-only",
            ExtensionInspectComparisonMode.ThreeWay => "three-way",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The comparison mode is not defined."),
        };

    internal static string ComparisonSideState(ExtensionInspectComparisonSideState value)
        => value switch
        {
            ExtensionInspectComparisonSideState.NotStarted => "not-started",
            ExtensionInspectComparisonSideState.NotApplicable => "not-applicable",
            ExtensionInspectComparisonSideState.Available => "available",
            ExtensionInspectComparisonSideState.Unavailable => "unavailable",
            ExtensionInspectComparisonSideState.Invalid => "invalid",
            ExtensionInspectComparisonSideState.Blocked => "blocked",
            ExtensionInspectComparisonSideState.Failed => "failed",
            ExtensionInspectComparisonSideState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The comparison side state is not defined."),
        };

    internal static string FingerprintKind(ExtensionInspectFingerprintKind value)
        => value switch
        {
            ExtensionInspectFingerprintKind.Semantic => "semantic",
            ExtensionInspectFingerprintKind.ExactBytes => "exact-bytes",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The fingerprint kind is not defined."),
        };

    internal static string FingerprintOrigin(ExtensionInspectFingerprintOrigin value)
        => value switch
        {
            ExtensionInspectFingerprintOrigin.PersistedBaseline => "persisted-baseline",
            ExtensionInspectFingerprintOrigin.OperationTimeCurrent => "operation-time-current",
            ExtensionInspectFingerprintOrigin.OperationTimeIntended => "operation-time-intended",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The fingerprint origin is not defined."),
        };

    internal static string PathRelation(ExtensionInspectPathRelation value)
        => value switch
        {
            ExtensionInspectPathRelation.NotStarted => "not-started",
            ExtensionInspectPathRelation.NotApplicable => "not-applicable",
            ExtensionInspectPathRelation.Unchanged => "unchanged",
            ExtensionInspectPathRelation.Changed => "changed",
            ExtensionInspectPathRelation.CurrentDiverged => "current-diverged",
            ExtensionInspectPathRelation.Missing => "missing",
            ExtensionInspectPathRelation.New => "new",
            ExtensionInspectPathRelation.Retired => "retired",
            ExtensionInspectPathRelation.Shared => "shared",
            ExtensionInspectPathRelation.Unknown => "unknown",
            ExtensionInspectPathRelation.Unavailable => "unavailable",
            ExtensionInspectPathRelation.Invalid => "invalid",
            ExtensionInspectPathRelation.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The path relation is not defined."),
        };

    internal static string DependencyComparisonState(ExtensionInspectDependencyComparisonState value)
        => value switch
        {
            ExtensionInspectDependencyComparisonState.NotStarted => "not-started",
            ExtensionInspectDependencyComparisonState.NotApplicable => "not-applicable",
            ExtensionInspectDependencyComparisonState.Available => "available",
            ExtensionInspectDependencyComparisonState.Unavailable => "unavailable",
            ExtensionInspectDependencyComparisonState.Invalid => "invalid",
            ExtensionInspectDependencyComparisonState.Blocked => "blocked",
            ExtensionInspectDependencyComparisonState.Failed => "failed",
            ExtensionInspectDependencyComparisonState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The dependency comparison state is not defined."),
        };

    internal static string DependencyRelation(ExtensionInspectDependencyRelation value)
        => value switch
        {
            ExtensionInspectDependencyRelation.NotStarted => "not-started",
            ExtensionInspectDependencyRelation.NotApplicable => "not-applicable",
            ExtensionInspectDependencyRelation.Equal => "equal",
            ExtensionInspectDependencyRelation.Changed => "changed",
            ExtensionInspectDependencyRelation.Unavailable => "unavailable",
            ExtensionInspectDependencyRelation.Invalid => "invalid",
            ExtensionInspectDependencyRelation.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The dependency relation is not defined."),
        };

    internal static string GeneratedState(ExtensionInspectGeneratedState value)
        => value switch
        {
            ExtensionInspectGeneratedState.NotStarted => "not-started",
            ExtensionInspectGeneratedState.Complete => "complete",
            ExtensionInspectGeneratedState.Incomplete => "incomplete",
            ExtensionInspectGeneratedState.Invalid => "invalid",
            ExtensionInspectGeneratedState.Blocked => "blocked",
            ExtensionInspectGeneratedState.Failed => "failed",
            ExtensionInspectGeneratedState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The generated state is not defined."),
        };

    internal static string GeneratedRegionState(ExtensionInspectGeneratedRegionState value)
        => value switch
        {
            ExtensionInspectGeneratedRegionState.NotStarted => "not-started",
            ExtensionInspectGeneratedRegionState.Valid => "valid",
            ExtensionInspectGeneratedRegionState.Absent => "absent",
            ExtensionInspectGeneratedRegionState.Invalid => "invalid",
            ExtensionInspectGeneratedRegionState.Ambiguous => "ambiguous",
            ExtensionInspectGeneratedRegionState.Unavailable => "unavailable",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The generated region state is not defined."),
        };
}
