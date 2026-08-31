using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models;

namespace OpenForge.Cli.Core.Commands.Route.Create;

internal static class RouteCreateBinding
{
    internal static RouteCreateSymbols CreateSymbols(Command routeGroup)
    {
        var fileTarget = new Argument<string?>(RouteCreateDefinitions.FileTarget.Name)
        {
            Description = RouteCreateDefinitions.FileTarget.Description,
            Arity = ArgumentArity.ZeroOrOne,
        };
        var description = CreateSingleton(RouteCreateDefinitions.Description);
        var tag = new Option<string[]>(RouteCreateDefinitions.Tag.Name)
        {
            Description = RouteCreateDefinitions.Tag.Description,
            HelpName = RouteCreateDefinitions.Tag.ValueName,
            Arity = ArgumentArity.ZeroOrMore,
            AllowMultipleArgumentsPerToken = false,
        };
        var responsibility = CreateSingleton(RouteCreateDefinitions.Responsibility);
        var template = CreateSingleton(RouteCreateDefinitions.Template);
        var dryRun = new Option<bool>(RouteCreateDefinitions.DryRun.Name)
        {
            Description = RouteCreateDefinitions.DryRun.Description,
            Arity = ArgumentArity.Zero,
        };
        var command = new Command(
            RouteCreateDefinitions.CreateCommand.Name,
            RouteCreateDefinitions.CreateCommand.Description);
        command.Arguments.Add(fileTarget);
        command.Options.Add(description);
        command.Options.Add(tag);
        command.Options.Add(responsibility);
        command.Options.Add(template);
        command.Options.Add(dryRun);
        routeGroup.Subcommands.Add(command);
        return new RouteCreateSymbols
        {
            RouteGroup = routeGroup,
            CreateCommand = command,
            FileTarget = fileTarget,
            Description = description,
            Tag = tag,
            Responsibility = responsibility,
            Template = template,
            DryRun = dryRun,
            DelimiterPolicies = Array.AsReadOnly(
            [
                new CliDelimiterPolicy(
                    RouteCreateDefinitions.Tag.Name,
                    CliDelimiterShape.Equals),
            ]),
        };
    }

    internal static CliCommandBinding<RouteCreateRequest, RouteCreateResult> Close(
        RouteCreateSymbols symbols,
        RouteCreateBindingComponents components)
    {
        return new CliCommandBinding<RouteCreateRequest, RouteCreateResult>(
            symbols.CreateCommand,
            new CliCommandBindingComponents<RouteCreateRequest, RouteCreateResult>
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

    internal static CliBindResult<RouteCreateRequest, RouteCreateResult> Bind(
        ParseResult parseResult,
        CliInvocation invocation,
        RouteCreateSymbols symbols)
    {
        var target = parseResult.GetValue(symbols.FileTarget);
        var descriptionFacts = CliOptionResultFactsReader.Read(parseResult, symbols.Description);
        var tagFacts = CliOptionResultFactsReader.Read(parseResult, symbols.Tag);
        var responsibilityFacts = CliOptionResultFactsReader.Read(parseResult, symbols.Responsibility);
        var templateFacts = CliOptionResultFactsReader.Read(parseResult, symbols.Template);
        var description = ReadSingleton(parseResult, symbols.Description);
        var tags = ReadMany(parseResult, symbols.Tag);
        var responsibility = ReadSingleton(parseResult, symbols.Responsibility);
        var template = ReadSingleton(parseResult, symbols.Template);
        var mode = parseResult.GetValue(symbols.DryRun)
            ? RouteCreateMode.DryRun
            : RouteCreateMode.Apply;

        var bindingInput = new RouteCreateBindingInput
        {
            Target = target,
            Description = description,
            Tags = tags,
            Responsibility = responsibility,
            Template = template,
            DescriptionFacts = descriptionFacts,
            TagFacts = tagFacts,
            ResponsibilityFacts = responsibilityFacts,
            TemplateFacts = templateFacts,
            ParserErrors = parseResult.Errors.Select(error => error.Message).ToArray(),
        };
        var validation = ValidateGrammar(bindingInput, mode);
        if (validation.Failure is { } failure)
        {
            return CliBindResult<RouteCreateRequest, RouteCreateResult>.Invalid(
                CreateInvalidResult(
                    workspace: invocation.Workspace,
                    requested: target,
                    mode: mode,
                    code: failure.Code,
                    cause: failure.Cause));
        }

        var facts = validation.Facts
            ?? throw new InvalidOperationException(
                "Valid Route Create binding validation requires bound facts.");
        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException("The route-create binding requires a selected workspace.");
        return CliBindResult<RouteCreateRequest, RouteCreateResult>.Bound(
            new RouteCreateRequest(
                workspace: workspace,
                fileTarget: facts.Target,
                metadata: new RouteCreateMetadataInput(
                    description: facts.Description,
                    tags: facts.Tags,
                    responsibility: facts.Responsibility),
                templateReference: facts.Template,
                mode: facts.Mode));
    }

    private static RouteCreateResult CreateContextualInvalidResult(
        CliInvalidBindingInput input)
    {
        var cause = input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
        return CreateInvalidResult(
            workspace: null,
            requested: RouteCreateDefinitions.FileTargetValueName,
            mode: RouteCreateMode.Apply,
            code: RouteCreateFindingCode.InvalidInput,
            cause: string.IsNullOrWhiteSpace(cause)
                ? "The Route Create command input is invalid."
                : cause);
    }

    private static RouteCreateBindingValidation ValidateGrammar(
        RouteCreateBindingInput input,
        RouteCreateMode mode)
    {
        if (string.IsNullOrWhiteSpace(input.Target))
        {
            return RouteCreateBindingValidation.Invalid(
                new RouteCreateBindingFailure(
                    Code: RouteCreateFindingCode.InvalidTarget,
                    Cause: "Route Create requires one file target."));
        }

        if (!input.DescriptionFacts.IsExplicit
            || input.DescriptionFacts.IdentifierCount != 1
            || input.DescriptionFacts.IsExplicitWithoutValue
            || string.IsNullOrWhiteSpace(input.Description))
        {
            return RouteCreateBindingValidation.Invalid(
                new RouteCreateBindingFailure(
                    Code: RouteCreateFindingCode.InvalidMetadata,
                    Cause: "Route Create requires one nonblank --description value."));
        }

        if (!input.TagFacts.IsExplicit
            || input.TagFacts.ValueCount == 0
            || input.Tags.Count == 0
            || input.Tags.Any(tag => !FrameworkDocumentMetadataTagGrammar.IsValid(tag))
            || input.Tags.Distinct(StringComparer.Ordinal).Count() != input.Tags.Count)
        {
            return RouteCreateBindingValidation.Invalid(
                new RouteCreateBindingFailure(
                    Code: RouteCreateFindingCode.InvalidMetadata,
                    Cause: "Route Create requires one or more unique canonical --tag values."));
        }

        var responsibilityFailure = ValidateSingleton(
            optionName: RouteCreateDefinitions.Responsibility.Name,
            facts: input.ResponsibilityFacts,
            value: input.Responsibility,
            allowExactEmpty: true);
        if (responsibilityFailure is not null)
        {
            return RouteCreateBindingValidation.Invalid(
                new RouteCreateBindingFailure(
                    Code: RouteCreateFindingCode.InvalidMetadata,
                    Cause: responsibilityFailure));
        }

        var templateFailure = ValidateSingleton(
            optionName: RouteCreateDefinitions.Template.Name,
            facts: input.TemplateFacts,
            value: input.Template,
            allowExactEmpty: false);
        if (templateFailure is not null)
        {
            return RouteCreateBindingValidation.Invalid(
                new RouteCreateBindingFailure(
                    Code: RouteCreateFindingCode.InvalidTemplate,
                    Cause: templateFailure));
        }

        if (input.Template is not null && string.IsNullOrWhiteSpace(input.Template))
        {
            return RouteCreateBindingValidation.Invalid(
                new RouteCreateBindingFailure(
                    Code: RouteCreateFindingCode.InvalidTemplate,
                    Cause: "--template requires a nonblank route reference."));
        }

        if (input.Responsibility is { Length: > 0 }
            && string.IsNullOrWhiteSpace(input.Responsibility))
        {
            return RouteCreateBindingValidation.Invalid(
                new RouteCreateBindingFailure(
                    Code: RouteCreateFindingCode.InvalidMetadata,
                    Cause: "--responsibility accepts a nonblank value or an exact empty value."));
        }

        if (input.ParserErrors.Count != 0)
        {
            return RouteCreateBindingValidation.Invalid(
                new RouteCreateBindingFailure(
                    Code: RouteCreateFindingCode.InvalidInput,
                    Cause: string.Join(" ", input.ParserErrors)));
        }

        return RouteCreateBindingValidation.Valid(
            new RouteCreateBindingFacts
            {
                Target = input.Target,
                Description = input.Description,
                Tags = input.Tags,
                Responsibility = NormalizeOptionalValue(input.Responsibility),
                Template = input.Template,
                Mode = mode,
            });
    }

    private static string? ValidateSingleton(
        string optionName,
        CliOptionResultFacts facts,
        string? value,
        bool allowExactEmpty)
    {
        if (facts.IdentifierCount > 1)
        {
            return $"{optionName} accepts exactly one value.";
        }

        if (facts.IsExplicitWithoutValue)
        {
            if (allowExactEmpty && value is { Length: 0 })
            {
                return null;
            }

            return $"{optionName} accepts exactly one value.";
        }

        if (facts.IsExplicit
            && (value is null || !allowExactEmpty && value.Length == 0))
        {
            return $"{optionName} accepts exactly one value.";
        }

        return null;
    }

    private static string? NormalizeOptionalValue(string? value)
        => value is { Length: 0 } ? null : value;

    private static RouteCreateResult CreateInvalidResult(
        OpenForge.Cli.Core.Framework.Workspace.CliWorkspace? workspace,
        string? requested,
        RouteCreateMode mode,
        RouteCreateFindingCode code,
        string cause)
    {
        var attemptedTarget = string.IsNullOrWhiteSpace(requested)
            ? RouteCreateDefinitions.FileTargetValueName
            : requested;
        var formation = new RouteCreateResultFormation
        {
            Workspace = workspace,
            Mode = mode,
            Target = new RouteCreateTarget
            {
                Requested = attemptedTarget,
            },
            Parent = null,
            Metadata = new RouteCreateMetadata
            {
                Description = string.Empty,
                Responsibility = null,
                Tags = [],
            },
            Template = null,
            Plan = new RouteCreatePlanFacts
            {
                Completeness = RouteCreatePlanCompleteness.NotEstablished,
                Safety = RouteCreatePlanSafety.NotEstablished,
            },
            Effects = [],
            UnchangedPaths = [],
            Recovery = new RouteCreateRecovery
            {
                State = RouteCreateRecoveryState.NotRequired,
                ResidualPath = null,
            },
            Verification = RouteCreateVerificationState.NotRequested,
            Findings =
            [
                new RouteCreateFinding(code, cause, attemptedTarget),
            ],
        };
        return new Shared.Result.RouteCreateResultBuilder().Build(formation);
    }

    private static Option<string?> CreateSingleton(
        CliOptionDefinition<string?> definition)
        => new(definition.Name)
        {
            Description = definition.Description,
            HelpName = definition.ValueName,
            Arity = ArgumentArity.ZeroOrOne,
        };

    private static string? ReadSingleton(
        ParseResult parseResult,
        Option<string?> option)
    {
        try
        {
            return parseResult.GetValue(option);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static string[] ReadMany(
        ParseResult parseResult,
        Option<string[]> option)
    {
        try
        {
            return parseResult.GetValue(option) ?? [];
        }
        catch (InvalidOperationException)
        {
            return [];
        }
    }
}
