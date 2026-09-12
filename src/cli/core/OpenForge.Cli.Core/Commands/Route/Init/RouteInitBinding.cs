using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;

namespace OpenForge.Cli.Core.Commands.Route.Init;

internal static class RouteInitBinding
{
    internal static RouteInitSymbols CreateSymbols(Command routeGroup)
    {
        ArgumentNullException.ThrowIfNull(routeGroup);
        var routeTarget = new Argument<string?>(RouteInitDefinitions.RouteTarget.Name)
        {
            Description = RouteInitDefinitions.RouteTarget.Description,
            Arity = ArgumentArity.ZeroOrOne,
        };
        var framework = CreateBoolean(RouteInitDefinitions.Framework);
        var description = CreateSingleton(RouteInitDefinitions.Description);
        var responsibility = CreateSingleton(RouteInitDefinitions.Responsibility);
        var tag = new Option<string[]>(RouteInitDefinitions.Tag.Name)
        {
            Description = RouteInitDefinitions.Tag.Description,
            HelpName = RouteInitDefinitions.Tag.ValueName,
            Arity = ArgumentArity.ZeroOrMore,
            AllowMultipleArgumentsPerToken = false,
        };
        var dryRun = CreateBoolean(RouteInitDefinitions.DryRun);
        var command = new Command(
            RouteInitDefinitions.InitCommand.Name,
            RouteInitDefinitions.InitCommand.Description);
        command.Arguments.Add(routeTarget);
        command.Options.Add(framework);
        command.Options.Add(description);
        command.Options.Add(responsibility);
        command.Options.Add(tag);
        command.Options.Add(dryRun);
        routeGroup.Subcommands.Add(command);
        return new RouteInitSymbols(
            routeGroup,
            command,
            routeTarget,
            framework,
            description,
            responsibility,
            tag,
            dryRun,
            Array.AsReadOnly(
            [
                new CliDelimiterPolicy(
                    RouteInitDefinitions.Tag.Name,
                    CliDelimiterShape.Equals),
            ]));
    }

    internal static CliCommandBinding<RouteInitRequest, RouteInitResult> Close(
        RouteInitSymbols symbols,
        RouteInitBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        ArgumentNullException.ThrowIfNull(components.Operation);
        return new CliCommandBinding<RouteInitRequest, RouteInitResult>(
            symbols.InitCommand,
            new CliCommandBindingComponents<RouteInitRequest, RouteInitResult>
            {
                Help = components.Help,
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = (parse, invocation) => Bind(parse.Result, invocation, symbols),
                InvalidResultFactory = CreateContextualInvalidResult,
                Operation = components.Operation.ExecuteAsync,
                Renderers = components.Renderers,
                DiagnosticRenderer = components.DiagnosticRenderer,
            });
    }

    internal static CliBindResult<RouteInitRequest, RouteInitResult> Bind(
        ParseResult parseResult,
        CliInvocation invocation,
        RouteInitSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(invocation);
        ArgumentNullException.ThrowIfNull(symbols);
        var routeTarget = parseResult.GetValue(symbols.RouteTarget);
        if (routeTarget is not { } requestedTarget || !IsValidTarget(requestedTarget))
        {
            return Invalid(
                invocation.Workspace,
                routeTarget,
                RouteInitFindingCode.InvalidTarget,
                "Route Init requires one safe route ID or exact canonical .agents entrypoint path.");
        }

        var scaffold = parseResult.GetValue(symbols.Framework)
            ? RouteInitScaffold.Framework
            : RouteInitScaffold.Generic;
        var mode = parseResult.GetValue(symbols.DryRun)
            ? RouteInitMode.DryRun
            : RouteInitMode.Apply;
        var descriptionFacts = CliOptionResultFactsReader.Read(parseResult, symbols.Description);
        var responsibilityFacts = CliOptionResultFactsReader.Read(parseResult, symbols.Responsibility);
        var tagFacts = CliOptionResultFactsReader.Read(parseResult, symbols.Tag);
        var description = parseResult.GetValue(symbols.Description);
        var responsibility = parseResult.GetValue(symbols.Responsibility);
        var tags = parseResult.GetValue(symbols.Tag) ?? [];
        if (!IsValidMetadata(
                scaffold,
                descriptionFacts.IsExplicit,
                description,
                responsibilityFacts.IsExplicit,
                responsibility,
                tagFacts.IsExplicit,
                tags))
        {
            return Invalid(
                invocation.Workspace,
                routeTarget,
                RouteInitFindingCode.InvalidMetadata,
                "Route Init metadata must follow the generic scaffold metadata grammar and cannot be combined with --framework.");
        }

        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException("The route-init binding requires a selected workspace.");
        return CliBindResult<RouteInitRequest, RouteInitResult>.Bound(
            new RouteInitRequest(
                workspace,
                requestedTarget,
                scaffold,
                mode,
                new RouteInitMetadataInput(
                    description,
                    responsibilityFacts.IsExplicit,
                    responsibility,
                    tags)));
    }

    private static RouteInitResult CreateContextualInvalidResult(
        CliInvalidBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var cause = input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
        var finding = new RouteInitFinding(
            RouteInitFindingCode.InvalidInput,
            string.IsNullOrWhiteSpace(cause)
                ? "The Route Init command input is invalid."
                : cause);
        return CreateInvalidResult(
            workspace: null,
            requested: "route-target",
            finding);
    }

    private static bool IsValidTarget(string? target)
    {
        if (string.IsNullOrWhiteSpace(target)
            || string.Equals(target, "loader", StringComparison.Ordinal))
        {
            return false;
        }

        var normalized = target.Replace('\\', '/');
        var isExactPath = normalized.StartsWith(".agents/", StringComparison.Ordinal)
            || normalized.StartsWith("./.agents/", StringComparison.Ordinal);
        var segments = normalized.Split('/', StringSplitOptions.None);
        if (normalized.StartsWith("./.agents/", StringComparison.Ordinal))
        {
            var exactSegments = segments[1..];
            return exactSegments.All(IsSafeSegment)
                && IsValidExactPath(exactSegments);
        }

        if (segments.Any(segment => !IsSafeSegment(segment)))
        {
            return false;
        }

        return isExactPath
            ? IsValidExactPath(segments)
            : segments.All(segment => !segment.EndsWith(".md", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsValidExactPath(IReadOnlyList<string> segments)
    {
        if (segments.Count < 3
            || !string.Equals(segments[0], ".agents", StringComparison.Ordinal))
        {
            return false;
        }

        var folder = segments[^2];
        return string.Equals(
                segments[^1],
                $"_{folder}.md",
                StringComparison.Ordinal)
            || SourceFormClassifier.IsCompatibilityFileName(segments[^1]);
    }

    private static bool IsSafeSegment(string segment)
        => segment.Length > 0 && segment is not ("." or "..");

    private static bool IsValidMetadata(
        RouteInitScaffold scaffold,
        bool descriptionSpecified,
        string? description,
        bool responsibilitySpecified,
        string? responsibility,
        bool tagsSpecified,
        IReadOnlyList<string> tags)
    {
        if (scaffold == RouteInitScaffold.Framework
            && (descriptionSpecified || responsibilitySpecified || tagsSpecified))
        {
            return false;
        }

        if (descriptionSpecified && string.IsNullOrWhiteSpace(description))
        {
            return false;
        }

        if (responsibilitySpecified
            && (responsibility is null
                || (responsibility.Length > 0
                    && string.IsNullOrWhiteSpace(responsibility))))
        {
            return false;
        }

        return (!tagsSpecified || tags.Count > 0)
            && tags.All(SourceOpenForgeMetadataParser.IsValidTag)
            && tags.Distinct(StringComparer.Ordinal).Count() == tags.Count;
    }

    private static CliBindResult<RouteInitRequest, RouteInitResult> Invalid(
        OpenForge.Cli.Core.Framework.Workspace.Models.CliWorkspace? workspace,
        string? requested,
        RouteInitFindingCode code,
        string cause)
        => CliBindResult<RouteInitRequest, RouteInitResult>.Invalid(
            CreateInvalidResult(
                workspace,
                requested ?? "route-target",
                new RouteInitFinding(
                    code,
                    cause,
                    string.IsNullOrEmpty(requested) ? null : requested)));

    private static RouteInitResult CreateInvalidResult(
        OpenForge.Cli.Core.Framework.Workspace.Models.CliWorkspace? workspace,
        string requested,
        RouteInitFinding finding)
    {
        var formation = new RouteInitResultFormation(
            workspace,
            RouteInitMode.Apply,
            RouteInitScaffold.Generic,
            new RouteInitTarget(requested, null, null),
            new RouteInitPlanFacts(
                RouteInitPlanCompleteness.NotEstablished,
                RouteInitPlanSafety.NotEstablished),
            framework: null,
            entrypoints: [],
            effects: [],
            unchangedPaths: [],
            new RouteInitLifecycle(
                RouteInitLifecycleAction.None,
                RouteInitLifecycleOutcome.NotRequested),
            new RouteInitRecovery(RouteInitRecoveryState.NotRequired, null),
            RouteInitVerificationState.NotRequested,
            [finding]);
        return new Shared.Result.RouteInitResultBuilder().Build(formation);
    }

    private static Option<bool> CreateBoolean(CliOptionDefinition<bool> definition)
        => new(definition.Name)
        {
            Description = definition.Description,
            Arity = ArgumentArity.Zero,
        };

    private static Option<string?> CreateSingleton(CliOptionDefinition<string?> definition)
        => new(definition.Name)
        {
            Description = definition.Description,
            HelpName = definition.ValueName,
            Arity = ArgumentArity.ZeroOrOne,
        };
}
