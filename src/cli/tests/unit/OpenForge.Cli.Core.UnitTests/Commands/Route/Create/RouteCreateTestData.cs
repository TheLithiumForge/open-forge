using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Create;

internal static class RouteCreateTestData
{
    internal const string ParentId = "memory/project-alpha";
    internal const string ParentPath = ".agents/memory/project-alpha/_project-alpha.md";
    internal const string TargetId = "memory/project-alpha/overview";
    internal const string TargetPath = ".agents/memory/project-alpha/overview.md";

    internal static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(
            Path.Combine(Path.GetTempPath(), "open-forge-route-create-contract"));
        return new CliWorkspace(
            root,
            root,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    internal static CliInvocation Invocation(
        CliWorkspace? workspace = null,
        CliOutputFormat format = CliOutputFormat.Json)
    {
        var selectedWorkspace = workspace ?? Workspace();
        return new CliInvocation(
            Process: new CliProcessIdentity("open-forge", "test"),
            Presentation: new CliPresentation(format, CliView.Expanded, CliVerbosity.Normal),
            TerminalMode: CliTerminalMode.None,
            WorkspaceRequest: new CliWorkspaceRequest(
                selectedWorkspace.LexicalRoot,
                selectedWorkspace.LexicalRoot),
            Workspace: selectedWorkspace);
    }

    internal static RouteCreateRequest Request(
        CliWorkspace? workspace = null,
        string fileTarget = TargetId,
        RouteCreateMode mode = RouteCreateMode.Apply,
        string? templateReference = null)
        => new(
            workspace: workspace ?? Workspace(),
            fileTarget: fileTarget,
            metadata: new RouteCreateMetadataInput(
                description: "Project overview",
                tags: ["Docs", "Overview"],
                responsibility: "Explains the project"),
            templateReference: templateReference,
            mode: mode);

    internal static RouteCreateFinding Finding(
        RouteCreateFindingCode code,
        string? target = TargetPath,
        string cause = "A bounded Route Create finding.")
        => new(code, cause, target);

    internal static RouteCreateResultFormation PreviewFormation(
        CliWorkspace? workspace = null,
        RouteCreateMode mode = RouteCreateMode.Apply)
    {
        var selectedWorkspace = workspace ?? Workspace();
        return new RouteCreateResultFormation
        {
            Workspace = selectedWorkspace,
            Mode = mode,
            Target = new RouteCreateTarget
            {
                Requested = TargetId,
                Id = TargetId,
                Path = TargetPath,
            },
            Parent = new RouteCreateParent
            {
                Id = ParentId,
                Path = ParentPath,
                Form = RouteCreateParentForm.Canonical,
            },
            Metadata = new RouteCreateMetadata
            {
                Description = "Project overview",
                Responsibility = "Explains the project",
                Tags = ["Docs", "Overview"],
            },
            Template = null,
            Plan = new RouteCreatePlanFacts
            {
                Completeness = RouteCreatePlanCompleteness.Complete,
                Safety = RouteCreatePlanSafety.Safe,
            },
            Effects = [CreateEffect()],
            UnchangedPaths = [".agents/loader.md"],
            Recovery = new RouteCreateRecovery
            {
                State = RouteCreateRecoveryState.NotRequired,
                ResidualPath = null,
            },
            Verification = RouteCreateVerificationState.NotRequested,
            Findings = [],
        };
    }

    internal static RouteCreateResultFormation VerifiedFormation(
        CliWorkspace? workspace = null)
    {
        var preview = PreviewFormation(workspace);
        return preview with
        {
            Effects =
            [
                CreateEffect() with
                {
                    Outcome = RouteCreateEffectOutcome.Verified,
                },
            ],
            Verification = RouteCreateVerificationState.Verified,
        };
    }

    internal static RouteCreateResultFormation BoundaryFormation()
    {
        var preview = PreviewFormation();
        return preview with
        {
            Parent = null,
            Plan = new RouteCreatePlanFacts
            {
                Completeness = RouteCreatePlanCompleteness.NotEstablished,
                Safety = RouteCreatePlanSafety.NotEstablished,
            },
            Effects = [],
            UnchangedPaths = [],
        };
    }

    internal static RouteCreateResult Result(
        RouteCreateResultFormation? formation = null)
        => new RouteCreateResultBuilder().Build(formation ?? VerifiedFormation());

    internal static RouteCreatePlan Plan()
    {
        var workspace = Workspace();
        var parent = Source(workspace, ParentId, ParentPath, SourceDocumentForm.CanonicalEntrypoint);
        var target = Source(workspace, TargetId, TargetPath, SourceDocumentForm.Markdown);
        var catalogue = new SourceCatalogue(
            workspace: workspace,
            candidates: [Candidate(parent)],
            sources: [parent],
            issues: [],
            isCancelled: false);
        var intendedBytes = Encoding.UTF8.GetBytes("# Project overview\n");
        var change = PlannedFileChange.Create(
            FileExpectation.Missing(target.Base.PhysicalPath),
            intendedBytes);
        return new RouteCreatePlan
        {
            Request = Request(workspace),
            Preview = PreviewFormation(workspace),
            NavigationFormation = new GeneratedNavigationFormationBuilder().Build(
                catalogue,
                [parent, target]),
            TargetSnapshot = FileStateSnapshot.Missing(target.Base.PhysicalPath),
            TargetSource = target,
            ParentSource = parent,
            TemplateSource = null,
            IntendedTargetBytes = ImmutableArray.CreateRange(intendedBytes),
            FileChanges = [change],
            RecoveryTargets = [],
        };
    }

    internal static RouteCreateApplicationProgress ApplicationProgress(
        RouteCreatePlan plan)
    {
        var change = plan.FileChanges.Single();
        var after = FileStateSnapshot.File(
            change.LogicalPath,
            change.LogicalPath,
            change.IntendedBytes.AsSpan());
        return new RouteCreateApplicationProgress
        {
            Receipts =
            [
                FileChangeReceipt.Verified(
                    change,
                    plan.TargetSnapshot,
                    after),
            ],
            UncertainAttempt = null,
            Recovery = new RouteCreateRecovery
            {
                State = RouteCreateRecoveryState.NotRequired,
                ResidualPath = null,
            },
            Verification = RouteCreateVerificationState.Verified,
            Findings = [],
        };
    }

    internal static RouteCreateEffect CreateEffect()
        => new()
        {
            Path = TargetPath,
            Kind = RouteCreateEffectKind.RoutedFile,
            Action = RouteCreateEffectAction.Create,
            Change = new RouteCreateEffectChange
            {
                Before = null,
                Expected = "target-content-hash",
            },
            Outcome = RouteCreateEffectOutcome.Planned,
            Residual = RouteCreateEffectResidual.None,
        };

    internal static RouteCreateEffect ParentEffect()
        => new()
        {
            Path = ParentPath,
            Kind = RouteCreateEffectKind.GeneratedRegion,
            Action = RouteCreateEffectAction.Replace,
            Change = new RouteCreateEffectChange
            {
                Before = "parent-before-hash",
                Expected = "parent-expected-hash",
            },
            Outcome = RouteCreateEffectOutcome.Planned,
            Residual = RouteCreateEffectResidual.None,
        };

    private static SourceLogicalSource Source(
        CliWorkspace workspace,
        string id,
        string path,
        SourceDocumentForm form)
    {
        var physicalPath = Path.GetFullPath(Path.Combine(
            workspace.LexicalRoot,
            path.Replace('/', Path.DirectorySeparatorChar)));
        return new SourceLogicalSource(
            new SourceLogicalIdentity(id, path),
            new SourceLayer(
                canonicalPath: path,
                physicalPath: physicalPath,
                form: form,
                kind: SourceLayerKind.Base));
    }

    private static SourceCandidate Candidate(SourceLogicalSource source)
        => new(
            canonicalPath: source.Base.CanonicalPath,
            form: source.Base.Form,
            automaticId: source.Identity.AutomaticId,
            physicalState: PhysicalPathState.Contained,
            physicalPath: source.Base.PhysicalPath,
            physicalParentPath: Path.GetDirectoryName(source.Base.PhysicalPath)
                ?? throw new InvalidOperationException(
                    "A Route Create source fixture requires a physical parent."));
}
