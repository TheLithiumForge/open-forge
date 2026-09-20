using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Init;

public sealed class RouteInitDefinitionsAndBindingTests
{
    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route Init definitions expose the frozen command grammar and result identity"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void DefinitionsExposeFrozenCommandGrammarAndResultIdentity()
    {
        Assert.Equal("route init", RouteInitDefinitions.CommandIdentity);
        Assert.Equal(1, RouteInitDefinitions.SchemaVersion);
        Assert.Equal("init", RouteInitDefinitions.InitCommand.Name);
        Assert.Equal("route-target", RouteInitDefinitions.RouteTarget.Name);
        Assert.Equal("--framework", RouteInitDefinitions.Framework.Name);
        Assert.Equal("--description", RouteInitDefinitions.Description.Name);
        Assert.Equal("--responsibility", RouteInitDefinitions.Responsibility.Name);
        Assert.Equal("--tag", RouteInitDefinitions.Tag.Name);
        Assert.Equal("--dry-run", RouteInitDefinitions.DryRun.Name);
        Assert.Equal(CliOptionArity.ExactlyOne, RouteInitDefinitions.Description.Arity);
        Assert.Equal(CliOptionArity.ExactlyOne, RouteInitDefinitions.Responsibility.Arity);
        Assert.Equal(CliOptionArity.ExactlyOne, RouteInitDefinitions.Tag.Arity);
        Assert.Equal(
            Enum.GetValues<RouteInitFindingCode>().Length,
            RouteInitDefinitions.FindingCodes.Count);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route Init symbols own one optional parser target and exact option arities"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void SymbolsOwnOneOptionalTargetAndExactOptionArities()
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteInitBinding.CreateSymbols(route);

        Assert.Same(route, symbols.RouteGroup);
        Assert.Same(symbols.InitCommand, route.Subcommands.Single());
        Assert.Empty(symbols.InitCommand.Aliases);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.RouteTarget.Arity);
        Assert.Equal(typeof(string), symbols.RouteTarget.ValueType);
        Assert.Equal(ArgumentArity.Zero, symbols.Framework.Arity);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.Description.Arity);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.Responsibility.Arity);
        Assert.Equal(ArgumentArity.ZeroOrMore, symbols.Tag.Arity);
        Assert.False(symbols.Tag.AllowMultipleArgumentsPerToken);
        Assert.Equal(ArgumentArity.Zero, symbols.DryRun.Arity);

        var parse = route.Parse(["init", "memory/project-alpha/documents", "--tag=alpha"]);
        Assert.Empty(parse.Errors);
        Assert.Equal("memory/project-alpha/documents", parse.GetValue(symbols.RouteTarget));
        var tags = parse.GetValue(symbols.Tag);
        Assert.NotNull(tags);
        Assert.Equal(["alpha"], tags);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route Init binding forms a generic request with ordered metadata and write policy"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void BindingFormsGenericRequestWithOrderedMetadataAndWritePolicy()
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteInitBinding.CreateSymbols(route);
        var workspace = RouteInitRedTestData.Workspace();
        var parse = route.Parse(
        [
            "init",
            "memory/project-alpha/documents",
            "--description=Project documents",
            "--responsibility=Owns project documents",
            "--tag=Docs",
            "--tag=Public",
            "--dry-run",
        ]);

        Assert.Empty(parse.Errors);
        var bound = RouteInitBinding.Bind(
            parse,
            RouteInitRedTestData.Invocation(workspace),
            symbols);
        var request = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Init.Models.Request.RouteInitRequest>(bound.Request);

        Assert.Null(bound.InvalidResult);
        Assert.Equal("memory/project-alpha/documents", request.RouteTarget);
        Assert.Equal(RouteInitScaffold.Generic, request.Scaffold);
        Assert.Equal(RouteInitMode.DryRun, request.Mode);
        Assert.Equal("Project documents", request.Metadata.Description);
        Assert.Equal("Owns project documents", request.Metadata.Responsibility);
        Assert.True(request.Metadata.ResponsibilitySpecified);
        Assert.Equal(["Docs", "Public"], request.Metadata.Tags);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route Init binding rejects missing targets and unsafe target forms before planning"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void BindingRejectsMissingAndUnsafeTargetsBeforePlanning()
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteInitBinding.CreateSymbols(route);
        var invocation = RouteInitRedTestData.Invocation(RouteInitRedTestData.Workspace());

        foreach (var arguments in new[]
        {
            new[] { "init" },
            new[] { "init", "" },
            new[] { "init", "loader" },
            new[] { "init", "memory/../documents" },
            new[] { "init", ".agents/memory/documents/readme.md" },
        })
        {
            var parse = route.Parse(arguments);
            var bound = RouteInitBinding.Bind(parse, invocation, symbols);
            var invalid = Assert.IsType<RouteInitResult>(bound.InvalidResult);
            Assert.Null(bound.Request);
            Assert.Equal(CliSemanticStatus.Invalid, invalid.Status);
            Assert.Contains(
                invalid.Findings,
                finding => finding.Code == RouteInitFindingCode.InvalidTarget);
        }
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route Init binding rejects Framework metadata combinations and invalid metadata values"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void BindingRejectsFrameworkMetadataAndInvalidMetadataValues()
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteInitBinding.CreateSymbols(route);
        var invocation = RouteInitRedTestData.Invocation(RouteInitRedTestData.Workspace());

        foreach (var arguments in new[]
        {
            new[] { "init", "memory/crystallized/documents", "--framework", "--description=managed" },
            new[] { "init", "memory/crystallized/documents", "--framework", "--responsibility=managed" },
            new[] { "init", "memory/crystallized/documents", "--framework", "--tag=managed" },
            new[] { "init", "memory/project-alpha/documents", "--description=" },
            new[] { "init", "memory/project-alpha/documents", "--responsibility=   " },
            new[] { "init", "memory/project-alpha/documents", "--tag=" },
            new[] { "init", "memory/project-alpha/documents", "--tag=#already-prefixed" },
            new[] { "init", "memory/project-alpha/documents", "--tag=duplicate", "--tag=duplicate" },
        })
        {
            var parse = route.Parse(arguments);
            var bound = RouteInitBinding.Bind(parse, invocation, symbols);
            var invalid = Assert.IsType<RouteInitResult>(bound.InvalidResult);
            Assert.Null(bound.Request);
            Assert.Equal(CliSemanticStatus.Invalid, invalid.Status);
            Assert.Contains(
                invalid.Findings,
                finding => finding.Code == RouteInitFindingCode.InvalidMetadata);
        }
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route Init binding accepts idempotent booleans and preserves ordered repeated tags"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void BindingAcceptsIdempotentBooleansAndPreservesOrderedTags()
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteInitBinding.CreateSymbols(route);
        var parse = route.Parse(
        [
            "init",
            "memory/Mobile App/crystallized/documents",
            "--framework",
            "--framework",
            "--dry-run",
            "--dry-run",
        ]);

        Assert.Empty(parse.Errors);
        var bound = RouteInitBinding.Bind(
            parse,
            RouteInitRedTestData.Invocation(RouteInitRedTestData.Workspace()),
            symbols);
        var request = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Init.Models.Request.RouteInitRequest>(bound.Request);

        Assert.Null(bound.InvalidResult);
        Assert.Equal(RouteInitScaffold.Framework, request.Scaffold);
        Assert.Equal(RouteInitMode.DryRun, request.Mode);
        Assert.Empty(request.Metadata.Tags);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route Init singleton options reject equal repetition at the parser boundary"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void SingletonOptionsRejectEqualRepetitionAtParserBoundary()
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteInitBinding.CreateSymbols(route);

        var description = route.Parse(
        ["init", "memory/project-alpha/documents", "--description=docs", "--description=docs"]);
        var responsibility = route.Parse(
        ["init", "memory/project-alpha/documents", "--responsibility=docs", "--responsibility=docs"]);

        Assert.NotEmpty(description.Errors);
        Assert.NotEmpty(responsibility.Errors);
        Assert.Equal(2, Assert.IsType<OptionResult>(description.GetResult(symbols.Description)).IdentifierTokenCount);
        Assert.Equal(2, Assert.IsType<OptionResult>(responsibility.GetResult(symbols.Responsibility)).IdentifierTokenCount);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Init machine mappings cover every named finite value and reject undefined values"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void MachineMappingsCoverNamedValuesAndRejectUndefinedValues()
    {
        Assert.Equal(["apply", "dry-run"], Enum.GetValues<RouteInitMode>().Select(RouteInitDefinitions.ReadMachineName));
        Assert.Equal(["generic", "framework"], Enum.GetValues<RouteInitScaffold>().Select(RouteInitDefinitions.ReadMachineName));
        Assert.Equal(["not-established", "incomplete", "complete"], Enum.GetValues<RouteInitPlanCompleteness>().Select(RouteInitDefinitions.ReadMachineName));
        Assert.Equal(["not-established", "safe", "blocked"], Enum.GetValues<RouteInitPlanSafety>().Select(RouteInitDefinitions.ReadMachineName));
        Assert.Equal(["installed-root", "managed", "scope"], Enum.GetValues<RouteInitFrameworkSegmentRole>().Select(RouteInitDefinitions.ReadMachineName));
        Assert.Equal(["canonical", "compatibility"], Enum.GetValues<RouteInitEntrypointForm>().Select(RouteInitDefinitions.ReadMachineName));
        Assert.Equal(["existing", "missing"], Enum.GetValues<RouteInitEntrypointCurrent>().Select(RouteInitDefinitions.ReadMachineName));
        Assert.Equal(["user", "framework"], Enum.GetValues<RouteInitEntrypointOwnership>().Select(RouteInitDefinitions.ReadMachineName));
        Assert.Equal(["draft", "explicit", "embedded"], Enum.GetValues<RouteInitDescriptionSource>().Select(RouteInitDefinitions.ReadMachineName));
        Assert.Equal(["default-omitted", "explicit-omitted", "explicit", "embedded"], Enum.GetValues<RouteInitResponsibilitySource>().Select(RouteInitDefinitions.ReadMachineName));
        Assert.Equal(["draft", "explicit", "mixed", "embedded"], Enum.GetValues<RouteInitTagsSource>().Select(RouteInitDefinitions.ReadMachineName));
        Assert.Equal(["unchanged", "planned", "not-started", "created", "verification-failed", "completion-unknown"], Enum.GetValues<RouteInitEntrypointOutcome>().Select(RouteInitDefinitions.ReadMachineName));
        Assert.Equal(["directory", "entrypoint", "generated-region"], Enum.GetValues<RouteInitEffectKind>().Select(RouteInitDefinitions.ReadMachineName));
        Assert.Equal(["create", "replace"], Enum.GetValues<RouteInitEffectAction>().Select(RouteInitDefinitions.ReadMachineName));
        Assert.Equal(["planned", "not-started", "verified", "verification-failed", "completion-unknown"], Enum.GetValues<RouteInitEffectOutcome>().Select(RouteInitDefinitions.ReadMachineName));
        Assert.Equal(["none", "retained", "unknown"], Enum.GetValues<RouteInitEffectResidual>().Select(RouteInitDefinitions.ReadMachineName));
        Assert.Equal(["none", "preserve", "publish"], Enum.GetValues<RouteInitLifecycleAction>().Select(RouteInitDefinitions.ReadMachineName));
        Assert.Equal(["not-requested", "planned", "already-current", "not-started", "verified", "verification-failed", "completion-unknown"], Enum.GetValues<RouteInitLifecycleOutcome>().Select(RouteInitDefinitions.ReadMachineName));
        Assert.Equal(["not-required", "not-created", "removed", "retained", "unknown"], Enum.GetValues<RouteInitRecoveryState>().Select(RouteInitDefinitions.ReadMachineName));
        Assert.Equal(["not-requested", "verified", "failed", "unknown"], Enum.GetValues<RouteInitVerificationState>().Select(RouteInitDefinitions.ReadMachineName));

        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitMode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitScaffold)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitPlanCompleteness)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitPlanSafety)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitFrameworkSegmentRole)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitEntrypointForm)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitEntrypointCurrent)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitEntrypointOwnership)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitDescriptionSource)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitResponsibilitySource)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitTagsSource)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitEntrypointOutcome)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitEffectKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitEffectAction)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitEffectOutcome)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitEffectResidual)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitLifecycleAction)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitLifecycleOutcome)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitRecoveryState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInitDefinitions.ReadMachineName((RouteInitVerificationState)int.MaxValue));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Init finding mappings preserve fixed status order and next-action precedence"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void FindingMappingsPreserveStatusOrderAndNextActionPrecedence()
    {
        var expectedCodes = new[]
        {
            "route-init.invalid-input", "route-init.invalid-target", "route-init.invalid-metadata",
            "route-init.workspace-unavailable", "route-init.workspace-unsafe", "route-init.target-unsafe",
            "route-init.route-ambiguous", "route-init.identity-collision", "route-init.loader-unsafe",
            "route-init.framework-payload-invalid", "route-init.framework-install-required",
            "route-init.framework-update-required", "route-init.framework-alignment-blocked",
            "route-init.metadata-unsafe", "route-init.generated-region-unsafe", "route-init.lifecycle-blocked",
            "route-init.workspace-lock-unavailable", "route-init.target-changed", "route-init.recovery-conflict",
            "route-init.framework-payload-unavailable", "route-init.inspection-incomplete",
            "route-init.metadata-incomplete", "route-init.projection-incomplete", "route-init.lifecycle-unavailable",
            "route-init.recovery-unavailable", "route-init.needs-authoring", "route-init.recovery-artifact-retained",
            "route-init.target-changed-during-apply", "route-init.write-failed", "route-init.verification-failed",
            "route-init.lifecycle-publication-failed", "route-init.recovery-failed", "route-init.operation-failed",
            "route-init.interrupted",
        };
        Assert.Equal(
            expectedCodes,
            RouteInitDefinitions.FindingCodes.Select(RouteInitDefinitions.ReadMachineName));

        Assert.Equal(
            Enum.GetValues<RouteInitFindingCode>().Select(ReadExpectedStatus),
            RouteInitDefinitions.FindingCodes.Select(RouteInitDefinitions.ReadStatus));

        Assert.Null(RouteInitDefinitions.ReadNextAction(CliSemanticStatus.Complete, []));
        AssertNext(
            RouteInitDefinitions.ReadNextAction(
                CliSemanticStatus.Invalid,
                [RouteInitRedTestData.Finding(RouteInitFindingCode.InvalidInput)]),
            "open-forge route init --help",
            "Correct the named Route Init input, then rerun the request.");
        AssertNext(
            RouteInitDefinitions.ReadNextAction(
                CliSemanticStatus.Blocked,
                [RouteInitRedTestData.Finding(RouteInitFindingCode.FrameworkInstallRequired)]),
            "open-forge install --dry-run",
            "Establish a trusted current Framework installation before rerunning Route Init in Framework mode.");
        AssertNext(
            RouteInitDefinitions.ReadNextAction(
                CliSemanticStatus.Blocked,
                [RouteInitRedTestData.Finding(RouteInitFindingCode.FrameworkUpdateRequired)]),
            "open-forge update",
            "Update the installed Framework state to the running CLI's embedded inventory before rerunning Route Init.");
        AssertNext(
            RouteInitDefinitions.ReadNextAction(
                CliSemanticStatus.Blocked,
                [RouteInitRedTestData.Finding(RouteInitFindingCode.WorkspaceLockUnavailable)]),
            "open-forge route init",
            "Wait for the blocking condition or inspect the changed target, then rerun Route Init from a fresh plan.");
        AssertNext(
            RouteInitDefinitions.ReadNextAction(
                CliSemanticStatus.Blocked,
                [RouteInitRedTestData.Finding(RouteInitFindingCode.GeneratedRegionUnsafe)]),
            "open-forge doctor",
            "Inspect the blocked workspace, route, identity, lifecycle, generated-region, or recovery boundary before rerunning Route Init.");
        AssertNext(
            RouteInitDefinitions.ReadNextAction(
                CliSemanticStatus.Incomplete,
                [RouteInitRedTestData.Finding(RouteInitFindingCode.ProjectionIncomplete)]),
            "open-forge doctor",
            "Inspect the unavailable route, metadata, projection, lifecycle, or recovery facts before relying on this Route Init result.");
        AssertNext(
            RouteInitDefinitions.ReadNextAction(
                CliSemanticStatus.Attention,
                [RouteInitRedTestData.Finding(RouteInitFindingCode.RecoveryArtifactRetained)]),
            "open-forge cleanup",
            "Review and remove the reported recovery artifact after confirming the verified Route Init result.");
        Assert.Null(
            RouteInitDefinitions.ReadNextAction(
                CliSemanticStatus.Complete,
                [RouteInitRedTestData.Finding(RouteInitFindingCode.NeedsAuthoring)]));
        AssertNext(
            RouteInitDefinitions.ReadNextAction(
                CliSemanticStatus.Failed,
                [RouteInitRedTestData.Finding(RouteInitFindingCode.WriteFailed)]),
            "open-forge route init --detail debug",
            "Report the failure and retry the same Route Init request with bounded diagnostics.");
        AssertNext(
            RouteInitDefinitions.ReadNextAction(
                CliSemanticStatus.Interrupted,
                [RouteInitRedTestData.Finding(RouteInitFindingCode.Interrupted)]),
            "open-forge route init",
            "Rerun the same Route Init request.");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RouteInitDefinitions.ReadStatus((RouteInitFindingCode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RouteInitDefinitions.ReadNextAction((CliSemanticStatus)int.MaxValue, []));
    }

    private static CliSemanticStatus ReadExpectedStatus(RouteInitFindingCode code)
    {
        var value = (int)code;
        return value switch
        {
            <= 2 => CliSemanticStatus.Invalid,
            <= 18 => CliSemanticStatus.Blocked,
            <= 24 => CliSemanticStatus.Incomplete,
            25 => CliSemanticStatus.Complete,
            26 => CliSemanticStatus.Attention,
            <= 32 => CliSemanticStatus.Failed,
            _ => CliSemanticStatus.Interrupted,
        };
    }

    private static void AssertNext(
        CliNextAction? action,
        string command,
        string reason)
    {
        var next = Assert.IsType<CliNextAction>(action);
        Assert.Equal(command, next.Command);
        Assert.Equal(reason, next.Reason);
    }
}
