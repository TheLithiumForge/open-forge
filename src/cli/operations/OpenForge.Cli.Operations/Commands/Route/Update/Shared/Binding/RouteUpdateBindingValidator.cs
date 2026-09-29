using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Binding;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

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

        var descriptionFailure = RouteOptionValidation.ValidateSingleton(
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

        var responsibilityFailure = RouteOptionValidation.ValidateSingleton(
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

        var templateFailure = RouteOptionValidation.ValidateSingleton(
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

        if (input.ApplyToFacts.IsExplicit && input.ClearApplyToFacts.IsExplicit)
        {
            return Invalid(
                RouteUpdateFindingCode.InvalidPatch,
                "--apply-to and --clear-apply-to cannot be used together.");
        }

        if (input.ApplyToFacts.IsExplicit
            && (input.ApplyToFacts.ValueCount == 0
                || input.ApplyTo.Count == 0
                || input.ApplyTo.Any(value => string.IsNullOrWhiteSpace(value)
                    || ApplyToPatternMatcher.Parse(value).Pattern is null)))
        {
            return Invalid(
                RouteUpdateFindingCode.InvalidPatch,
                "Supplied --apply-to values must be valid nonempty workspace-relative patterns.");
        }

        if (!input.DescriptionFacts.IsExplicit
            && !input.TagFacts.IsExplicit
            && !input.ResponsibilityFacts.IsExplicit
            && !input.TemplateFacts.IsExplicit
            && !input.ApplyToFacts.IsExplicit
            && !input.ClearApplyToFacts.IsExplicit)
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
            ApplyTo = new RouteUpdateApplyToRequest
            {
                Operation = ReadApplyToOperation(input),
                Values = input.ApplyToFacts.IsExplicit
                    ? input.ApplyTo.Distinct(StringComparer.Ordinal).ToImmutableArray()
                    : [],
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

    private static RouteUpdateApplyToOperation ReadApplyToOperation(
        RouteUpdateBindingInput input)
    {
        if (input.ClearApplyToFacts.IsExplicit)
        {
            return RouteUpdateApplyToOperation.Clear;
        }

        if (input.ApplyToFacts.IsExplicit)
        {
            return RouteUpdateApplyToOperation.Set;
        }

        return RouteUpdateApplyToOperation.NotRequested;
    }

    private static RouteUpdateBindingValidation Invalid(
        RouteUpdateFindingCode code,
        string cause)
        => RouteUpdateBindingValidation.Invalid(
            new RouteUpdateBindingFailure(code, cause));

}
