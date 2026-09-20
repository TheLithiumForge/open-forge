using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Binding;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Binding;

internal static class RouteCreateRequestBinder
{
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
        var description = CliOptionResultFactsReader.ReadValue(parseResult, symbols.Description);
        var tags = CliOptionResultFactsReader.ReadValues(parseResult, symbols.Tag);
        var responsibility = CliOptionResultFactsReader.ReadValue(parseResult, symbols.Responsibility);
        var template = CliOptionResultFactsReader.ReadValue(parseResult, symbols.Template);
        var mode = parseResult.GetValue(symbols.DryRun)
            ? RouteCreateMode.DryRun
            : RouteCreateMode.Apply;

        var parserErrors = parseResult.Errors.Select(error => error.Message).ToArray();
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
            ParserErrors = parserErrors,
        };
        var validation = ValidateGrammar(bindingInput, mode);
        if (validation.Failure is { } failure)
        {
            return CliBindResult<RouteCreateRequest, RouteCreateResult>.Invalid(
                RouteCreateInvalidResultFactory.CreateInvalidResult(
                    workspace: invocation.Workspace,
                    requested: target,
                    mode: mode,
                    code: failure.Code,
                    cause: failure.Cause,
                    findingTarget: failure.Code == RouteCreateFindingCode.InvalidTemplate
                        ? TemplateFindingTarget(template)
                        : null));
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

        var metadataProblems = new List<string>();
        if (input.DescriptionFacts.IsExplicitWithoutValue
            || (input.DescriptionFacts.IsExplicit
                && string.IsNullOrWhiteSpace(input.Description)))
        {
            metadataProblems.Add("--description is missing");
        }
        else if (input.DescriptionFacts.IsExplicit && input.DescriptionFacts.IdentifierCount != 1)
        {
            metadataProblems.Add("--description accepts exactly one value");
        }

        if (input.TagFacts.IsExplicitWithoutValue
            || (input.TagFacts.IsExplicit
                && (input.TagFacts.ValueCount == 0 || input.Tags.Count == 0)))
        {
            metadataProblems.Add("at least one --tag is required");
        }
        else if (input.TagFacts.IsExplicit)
        {
            metadataProblems.AddRange(input.Tags
                .Where(tag => !FrameworkDocumentMetadataTagGrammar.IsValid(tag))
                .Distinct(StringComparer.Ordinal)
                .Select(tag => $"--tag {tag} is not a valid tag"));
            metadataProblems.AddRange(input.Tags
                .GroupBy(tag => tag, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => $"--tag {group.Key} is repeated"));
        }

        var responsibilityFailure = RouteOptionValidation.ValidateSingleton(
            optionName: RouteCreateDefinitions.Responsibility.Name,
            facts: input.ResponsibilityFacts,
            value: input.Responsibility,
            allowExactEmpty: true);
        if (responsibilityFailure is not null)
        {
            metadataProblems.Add(responsibilityFailure.TrimEnd('.'));
        }

        if (input.Responsibility is { Length: > 0 }
            && string.IsNullOrWhiteSpace(input.Responsibility))
        {
            metadataProblems.Add("--responsibility accepts a nonblank value or an exact empty value");
        }

        if (metadataProblems.Count > 0)
        {
            return RouteCreateBindingValidation.Invalid(
                new RouteCreateBindingFailure(
                    Code: RouteCreateFindingCode.InvalidMetadata,
                    Cause: string.Join(" and ", metadataProblems) + "."));
        }

        var templateFailure = RouteOptionValidation.ValidateSingleton(
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


    private static string? NormalizeOptionalValue(string? value)
        => value is { Length: 0 } ? null : value;

    private static string TemplateFindingTarget(string? template)
        => string.IsNullOrWhiteSpace(template)
            ? RouteCreateDefinitions.TemplateReferenceValueName
            : template;


}
