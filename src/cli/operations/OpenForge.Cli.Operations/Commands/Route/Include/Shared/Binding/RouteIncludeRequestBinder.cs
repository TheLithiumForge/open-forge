using System.CommandLine;
using System.Collections.Immutable;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;

namespace OpenForge.Cli.Core.Commands.Route.Include.Shared.Binding;

internal static class RouteIncludeRequestBinder
{
    internal static CliBindResult<RouteIncludeRequest, RouteIncludeResult> Bind(ParseResult parse, CliInvocation invocation, RouteIncludeSymbols symbols)
    {
        var file = parse.GetValue(symbols.File) ?? "workspace-file";
        CliBindResult<RouteIncludeRequest, RouteIncludeResult> Invalid(string cause)
            => CliBindResult<RouteIncludeRequest, RouteIncludeResult>.Invalid(RouteIncludeInvalidResultFactory.Invalid(invocation.Workspace, file, cause));
        if (parse.Errors.Count > 0) return Invalid(string.Join(" ", parse.Errors.Select(error => error.Message)));
        if (string.IsNullOrWhiteSpace(parse.GetValue(symbols.File))) return Invalid("Supply one workspace-relative file.");
        var descriptionFacts = CliOptionResultFactsReader.Read(parse, symbols.Description);
        var description = CliOptionResultFactsReader.ReadValue(parse, symbols.Description);
        if (descriptionFacts.IsExplicit && (descriptionFacts.IdentifierCount != 1 || string.IsNullOrWhiteSpace(description)
            || description.Any(c => char.IsControl(c) || c is '[' or ']')))
            return Invalid("Supply one non-empty --description without brackets or control characters.");
        var tagFacts = CliOptionResultFactsReader.Read(parse, symbols.Tag);
        var tags = CliOptionResultFactsReader.ReadValues(parse, symbols.Tag).Select(tag => tag.StartsWith('#') ? tag[1..] : tag).ToImmutableArray();
        if (tagFacts.IsExplicit && (tagFacts.IsExplicitWithoutValue || tags.IsEmpty || tags.Any(tag => !FrameworkDocumentMetadataTagGrammar.IsValid(tag))
            || tags.Distinct(StringComparer.Ordinal).Count() != tags.Length))
            return Invalid("Supply unique non-empty --tag names, with or without #.");
        var applyFacts = CliOptionResultFactsReader.Read(parse, symbols.ApplyTo);
        var patterns = ImmutableArray.CreateBuilder<ApplyToPattern>();
        if (applyFacts.IsExplicitWithoutValue) return Invalid("Supply a valid glob after --apply-to.");
        foreach (var value in CliOptionResultFactsReader.ReadValues(parse, symbols.ApplyTo))
        {
            var parsed = ApplyToPatternExpressionParser.Parse(value);
            if (parsed.Failure is not null) return Invalid("Supply valid workspace-relative --apply-to globs.");
            patterns.AddRange(parsed.Patterns);
        }
        var workspace = invocation.Workspace ?? throw new InvalidOperationException("A selected workspace is required.");
        return CliBindResult<RouteIncludeRequest, RouteIncludeResult>.Bound(new RouteIncludeRequest
        {
            Workspace = workspace, File = file, DryRun = parse.GetValue(symbols.DryRun),
            Description = descriptionFacts.IsExplicit ? description : null,
            Tags = tagFacts.IsExplicit ? tags : null,
            ApplyTo = applyFacts.IsExplicit ? patterns.DistinctBy(pattern => pattern.Text, StringComparer.Ordinal).OrderBy(pattern => pattern.Text, StringComparer.Ordinal).ToImmutableArray() : null,
        });
    }
}
