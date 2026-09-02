using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Shell.Parsing.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Binding;

internal sealed class RouteUpdateBindingValidator
{
    internal RouteUpdateBindingValidation Validate(
        RouteUpdateBindingInput input,
        RouteUpdateMode mode)
    {
        if (string.IsNullOrWhiteSpace(input.Target))
        {
            return Invalid(
                RouteUpdateFindingCode.InvalidTarget,
                "Route Update requires one source reference.");
        }

        var descriptionFailure = ValidateSingleton(
            RouteUpdateDefinitions.Description.Name,
            input.DescriptionFacts,
            input.Description,
            allowExactEmpty: false);
        if (descriptionFailure is not null
            || input.DescriptionFacts.IsExplicit && string.IsNullOrWhiteSpace(input.Description))
        {
            return Invalid(
                RouteUpdateFindingCode.InvalidPatch,
                descriptionFailure ?? "--description requires one nonblank value.");
        }

        if (input.TagFacts.IsExplicit
            && (input.TagFacts.ValueCount == 0
                || input.Tags.Count == 0
                || input.Tags.Any(tag => !FrameworkDocumentMetadataTagGrammar.IsValid(tag))
                || input.Tags.Distinct(StringComparer.Ordinal).Count() != input.Tags.Count))
        {
            return Invalid(
                RouteUpdateFindingCode.InvalidPatch,
                "Supplied --tag values must form one nonempty ordered list of unique canonical tags.");
        }

        var responsibilityFailure = ValidateSingleton(
            RouteUpdateDefinitions.Responsibility.Name,
            input.ResponsibilityFacts,
            input.Responsibility,
            allowExactEmpty: true);
        if (responsibilityFailure is not null
            || input.Responsibility is { Length: > 0 }
                && string.IsNullOrWhiteSpace(input.Responsibility))
        {
            return Invalid(
                RouteUpdateFindingCode.InvalidPatch,
                responsibilityFailure
                    ?? "--responsibility accepts a nonblank value or an exact empty value.");
        }

        var templateFailure = ValidateSingleton(
            RouteUpdateDefinitions.Template.Name,
            input.TemplateFacts,
            input.Template,
            allowExactEmpty: false);
        if (templateFailure is not null
            || input.TemplateFacts.IsExplicit && string.IsNullOrWhiteSpace(input.Template))
        {
            return Invalid(
                RouteUpdateFindingCode.InvalidTemplate,
                templateFailure ?? "--template requires one nonblank route reference.");
        }

        if (!input.DescriptionFacts.IsExplicit
            && !input.TagFacts.IsExplicit
            && !input.ResponsibilityFacts.IsExplicit
            && !input.TemplateFacts.IsExplicit)
        {
            return Invalid(
                RouteUpdateFindingCode.InvalidInput,
                "Route Update requires at least one metadata or Template operation.");
        }

        if (input.ParserErrors.Count != 0)
        {
            return Invalid(
                RouteUpdateFindingCode.InvalidInput,
                string.Join(" ", input.ParserErrors));
        }

        return RouteUpdateBindingValidation.Valid(
            new RouteUpdateBindingFacts
            {
                Target = input.Target,
                Patch = CreatePatch(input),
                Template = input.Template,
                Mode = mode,
            });
    }

    internal static RouteUpdatePatchRequest CreatePatch(
        RouteUpdateBindingInput input)
        => new()
        {
            Description = new RouteUpdateDescriptionRequest
            {
                Requested = input.DescriptionFacts.IsExplicit,
                Value = input.Description,
            },
            Responsibility = new RouteUpdateResponsibilityRequest
            {
                Operation = ReadResponsibilityOperation(input),
                Value = input.Responsibility is { Length: > 0 }
                    ? input.Responsibility
                    : null,
            },
            Tags = new RouteUpdateTagsRequest
            {
                Requested = input.TagFacts.IsExplicit,
                Values = ImmutableArray.CreateRange(input.Tags),
            },
        };

    private static RouteUpdateResponsibilityOperation ReadResponsibilityOperation(
        RouteUpdateBindingInput input)
    {
        if (!input.ResponsibilityFacts.IsExplicit)
        {
            return RouteUpdateResponsibilityOperation.NotRequested;
        }

        return input.Responsibility is { Length: 0 }
            ? RouteUpdateResponsibilityOperation.Remove
            : RouteUpdateResponsibilityOperation.Set;
    }

    private static RouteUpdateBindingValidation Invalid(
        RouteUpdateFindingCode code,
        string cause)
        => RouteUpdateBindingValidation.Invalid(
            new RouteUpdateBindingFailure(code, cause));

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
}
