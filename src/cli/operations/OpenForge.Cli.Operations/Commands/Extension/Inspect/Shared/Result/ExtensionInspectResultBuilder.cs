using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal sealed class ExtensionInspectResultBuilder(ExtensionInspectComparisonBuilder comparisonBuilder)
{
    private readonly ExtensionInspectComparisonBuilder _comparisonBuilder = comparisonBuilder;

    internal ExtensionInspectResult Build(
        ExtensionInspectRequest request,
        ExtensionSourceReadResult source,
        WorkspaceOwnershipRead lifecycle,
        IReadOnlyList<ExtensionInspectCurrentPath> currentSnapshot)
    {
        lifecycle = ExtensionInspectInstalledClosureReader.ReadOwnership(lifecycle);
        var subjectId = request.StableId;
        var findings = new List<ExtensionInspectFinding>();
        var subjectPackageFacts = ExtensionInspectSubjectPackageBuilder.Build(
            new ExtensionInspectSubjectPackageInput
            {
                Request = request,
                Source = source,
                Ownership = lifecycle,
                Findings = findings,
            });
        var lifecyclePackage = subjectPackageFacts.Selection.InstalledPackage;
        var availablePackage = subjectPackageFacts.Selection.AvailablePackage;

        var dependencyPathFacts = ExtensionInspectDependencyPathBuilder.Build(
            new ExtensionInspectDependencyPathInput
            {
                Source = source,
                AvailablePackage = availablePackage,
                InstalledPackages = lifecycle.Document.Extensions,
                InstalledPackage = lifecyclePackage,
                SubjectId = subjectId,
                CurrentSnapshot = currentSnapshot,
                Findings = findings,
            });
        var dependencies = dependencyPathFacts.Dependencies;
        var availableClosure = dependencyPathFacts.AvailableClosure;
        var sourcePathProjections = dependencyPathFacts.SourcePathProjections;
        var pathFacts = dependencyPathFacts.PathFacts;

        var comparisonFacts = _comparisonBuilder.Build(
            new ExtensionInspectComparisonBuildInput
            {
                Ownership = lifecycle,
                InstalledPackage = lifecyclePackage,
                AvailablePackage = availablePackage,
                AvailableClosure = availableClosure,
                Dependencies = dependencies,
                PathFacts = pathFacts,
                SourcePathProjections = sourcePathProjections,
                Findings = findings,
            });
        var generated = comparisonFacts.Generated;
        var comparison = comparisonFacts.Comparison;

        ExtensionInspectFindingPolicy.Sort(findings);
        var status = ExtensionInspectFindingPolicy.ReadStatus(findings);
        var counts = ExtensionInspectCountBuilder.Build(
            new ExtensionInspectCountsInput
            {
                Ownership = lifecycle,
                Source = source,
                InstalledPackage = lifecyclePackage,
                AvailablePackage = availablePackage,
                Dependencies = dependencies,
                PathFacts = pathFacts,
                Comparison = comparison,
                Generated = generated,
                FindingCount = findings.Count,
            });
        var next = ExtensionInspectDefinitions.ReadNext(
            status,
            ExtensionInspectActionabilityPolicy.IsActionable(
                new ExtensionInspectActionabilityInput
                {
                    Comparison = comparison,
                    Generated = generated,
                    Ownership = lifecycle,
                    Source = source,
                    Findings = findings,
                }),
            subjectPackageFacts.Subject.Id);

        return new ExtensionInspectResult
        {
            Status = status,
            Workspace = request.Workspace,
            Subject = subjectPackageFacts.Subject,
            Source = subjectPackageFacts.Source,
            Lifecycle = subjectPackageFacts.Lifecycle,
            Installed = subjectPackageFacts.Installed,
            Available = subjectPackageFacts.Available,
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
        => ExtensionInspectBoundaryResultFactory.Invalid(
            workspace,
            supplied,
            ExtensionInspectFindingCode.InvalidInput,
            cause);

    internal static ExtensionInspectResult InvalidStableId(
        CliWorkspace? workspace,
        string? supplied,
        string cause)
        => ExtensionInspectBoundaryResultFactory.Invalid(
            workspace,
            supplied,
            ExtensionInspectFindingCode.InvalidStableId,
            cause);

    internal static ExtensionInspectResult WorkspaceBlocked(
        string? supplied,
        string cause)
        => ExtensionInspectBoundaryResultFactory.WorkspaceBlocked(supplied, cause);

    internal ExtensionInspectResult Event(ExtensionInspectEventInput input)
    {
        var finding = ExtensionInspectFindingPolicy.Create(new ExtensionInspectFindingInput
        {
            Code = input.Code,
            Subject = input.Request.StableId,
            Cause = input.Cause,
        });
        var subjectPackageFacts = ExtensionInspectSubjectPackageBuilder.BuildEvent(
            new ExtensionInspectEventSubjectPackageInput
            {
                Request = input.Request,
                Source = input.Source,
                Ownership = input.Ownership,
            });
        var result = ExtensionInspectBoundaryResultFactory.Empty(
            new ExtensionInspectEmptyResultInput(
                input.Status,
                input.Request.Workspace,
                subjectPackageFacts.Subject,
                [finding],
                ExtensionInspectDefinitions.ReadNext(
                    input.Status,
                    actionable: false,
                    subjectId: input.Request.StableId)));
        return result with
        {
            Source = subjectPackageFacts.Source ?? result.Source,
            Lifecycle = subjectPackageFacts.Lifecycle ?? result.Lifecycle,
            Installed = subjectPackageFacts.Installed ?? result.Installed,
            Available = subjectPackageFacts.Available ?? result.Available,
            PathFacts = ExtensionInspectDependencyPathBuilder.ReadEventPathFacts(input.CurrentPaths),
            Counts = result.Counts with
            {
                InstalledPackages = input.Ownership is { IsTrustworthy: true } observed ? observed.Document.Extensions.Length : null,
                AvailablePackages = input.Source?.Packages.Count,
                CurrentPaths = input.CurrentPaths?.Count,
            },
        };
    }
}
