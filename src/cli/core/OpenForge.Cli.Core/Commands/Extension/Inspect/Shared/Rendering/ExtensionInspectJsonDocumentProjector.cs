using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Presentation;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;

internal static class ExtensionInspectJsonDocumentProjector
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
            SelectedBy = ExtensionInspectWireVocabulary.WorkspaceSelection(workspace.SelectedBy),
        };

    private static ExtensionInspectJsonSubject Subject(ExtensionInspectSubject value)
        => new()
        {
            Supplied = value.Supplied,
            Form = value.Form is null ? null : ExtensionInspectWireVocabulary.SubjectForm(value.Form.Value),
            Id = value.Id,
            State = ExtensionInspectWireVocabulary.SubjectState(value.State),
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
            Kind = value.Kind is null ? null : ExtensionInspectWireVocabulary.SourceKind(value.Kind.Value),
            State = ExtensionInspectWireVocabulary.SourceState(value.State),
        };

    private static ExtensionInspectJsonLifecycle Lifecycle(ExtensionInspectLifecycle value)
        => new()
        {
            DocumentPath = value.DocumentPath,
            ReadState = ExtensionInspectWireVocabulary.LifecycleReadState(value.ReadState),
            Trust = ExtensionInspectWireVocabulary.LifecycleTrust(value.Trust),
            Coverage = ExtensionInspectWireVocabulary.Coverage(value.Coverage),
            WorkspaceBinding = ExtensionInspectWireVocabulary.WorkspaceBinding(value.WorkspaceBinding),
            FingerprintPolicy = value.FingerprintPolicy,
        };

    private static ExtensionInspectJsonInstalled Installed(ExtensionInspectInstalled value)
        => new()
        {
            State = ExtensionInspectWireVocabulary.InstalledState(value.State),
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
            State = ExtensionInspectWireVocabulary.AvailableState(value.State),
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
                    State = ExtensionInspectWireVocabulary.PackageFileState(file.State),
                    ByteLength = file.ByteLength,
                    Sha256 = file.Sha256,
                }).ToArray(),
            },
        };

    private static ExtensionInspectJsonDependencyClosure Dependencies(ExtensionInspectDependencyClosure value)
        => new()
        {
            State = ExtensionInspectWireVocabulary.DependencyState(value.State),
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
                State = ExtensionInspectWireVocabulary.DependencyPackageState(package.State),
            }).ToArray(),
            Order = value.Order.ToArray(),
        };

    private static ExtensionInspectJsonPathFacts PathFacts(ExtensionInspectPathFacts value)
        => new()
        {
            State = ExtensionInspectWireVocabulary.PathState(value.State),
            Declared = value.Declared.Select(path => new ExtensionInspectJsonDeclaredPath
            {
                Path = path.Path,
                SourcePath = path.SourcePath,
                State = ExtensionInspectWireVocabulary.DeclaredPathState(path.State),
            }).ToArray(),
            Current = value.Current.Select(path => new ExtensionInspectJsonCurrentPath
            {
                Path = path.Path,
                State = ExtensionInspectWireVocabulary.CurrentPathState(path.State),
                PhysicalIdentity = path.PhysicalIdentity,
                ByteLength = path.ByteLength,
                ExactSha256 = path.ExactSha256,
            }).ToArray(),
        };

    private static ExtensionInspectJsonComparison Comparison(ExtensionInspectComparison value)
        => new()
        {
            State = ExtensionInspectWireVocabulary.ComparisonState(value.State),
            Mode = ExtensionInspectWireVocabulary.ComparisonMode(value.Mode),
            Baseline = Side(value.Baseline),
            Current = Side(value.Current),
            Intended = Side(value.Intended),
            Paths = value.Paths.Select(PathComparison).ToArray(),
            Dependencies = DependencyComparison(value.Dependencies),
        };

    private static ExtensionInspectJsonComparisonSide Side(ExtensionInspectComparisonSide value)
        => new()
        {
            State = ExtensionInspectWireVocabulary.ComparisonSideState(value.State),
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
            Kind = ExtensionInspectWireVocabulary.FingerprintKind(value.Kind),
            Policy = value.Policy,
            Sha256 = value.Sha256,
            Origin = ExtensionInspectWireVocabulary.FingerprintOrigin(value.Origin),
        };

    private static ExtensionInspectJsonPathComparison PathComparison(ExtensionInspectPathComparison value)
        => new()
        {
            Path = value.Path,
            Baseline = value.Baseline is null ? null : Fingerprint(value.Baseline),
            Current = value.Current is null ? null : Fingerprint(value.Current),
            Intended = value.Intended is null ? null : Fingerprint(value.Intended),
            Relation = ExtensionInspectWireVocabulary.PathRelation(value.Relation),
            BaselineOwners = value.BaselineOwners.ToArray(),
            CurrentOwners = value.CurrentOwners.ToArray(),
            IntendedOwners = value.IntendedOwners.ToArray(),
        };

    private static ExtensionInspectJsonDependencyComparison DependencyComparison(ExtensionInspectDependencyComparison value)
        => new()
        {
            State = ExtensionInspectWireVocabulary.DependencyComparisonState(value.State),
            Baseline = value.Baseline.ToArray(),
            Current = value.Current.ToArray(),
            Intended = value.Intended.ToArray(),
            Relation = ExtensionInspectWireVocabulary.DependencyRelation(value.Relation),
        };

    private static ExtensionInspectJsonGenerated Generated(ExtensionInspectGenerated value)
        => new()
        {
            State = ExtensionInspectWireVocabulary.GeneratedState(value.State),
            Ownership = value.Ownership,
            Regions = value.Regions.Select(region => new ExtensionInspectJsonGeneratedRegion
            {
                Path = region.Path,
                State = ExtensionInspectWireVocabulary.GeneratedRegionState(region.State),
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
}
