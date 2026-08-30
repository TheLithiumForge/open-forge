using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Result;
using OpenForge.Cli.Core.Framework.Extensions.Identity;

namespace OpenForge.Cli.Core.Commands.Extension.Create;

internal sealed class ExtensionCreateRequestBinder(ExtensionCreateSymbols symbols)
{
    private readonly ExtensionCreateSymbols _symbols = symbols;

    internal CliBindResult<ExtensionCreateRequest, ExtensionCreateResult> Bind(
        CliBindingParse parse,
        CliInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(invocation);
        var result = parse.Result;
        var stableId = ReadArgument(result, _symbols.StableId);
        var path = ReadSingleton(result, _symbols.Path);
        var name = ReadSingleton(result, _symbols.Name);
        var description = ReadSingleton(result, _symbols.Description);
        var packageVersion = ReadSingleton(result, _symbols.PackageVersion);
        var dependencies = ReadMany(result, _symbols.Dependency);
        var automatic = result.GetValue(_symbols.Automatic);
        var request = new ExtensionCreateRequest
        {
            StableId = stableId,
            CataloguePath = path,
            Name = name,
            Description = description,
            PackageVersion = packageVersion,
            Dependencies = dependencies,
            AllowInteraction = invocation.Presentation.Format == CliOutputFormat.Human && !automatic,
            Mode = result.GetValue(_symbols.DryRun) ? ExtensionCreateMode.DryRun : ExtensionCreateMode.Apply,
        };
        var cause = ValidateGrammar(result, _symbols, request);
        if (cause is not null)
        {
            return CliBindResult<ExtensionCreateRequest, ExtensionCreateResult>.Invalid(
                CreateInvalidResult(request, cause));
        }

        return CliBindResult<ExtensionCreateRequest, ExtensionCreateResult>.Bound(
            request);
    }

    internal static ExtensionCreateResult CreateInvalidResult(
        ExtensionCreateRequest request,
        string cause)
    {
        ArgumentNullException.ThrowIfNull(request);
        var finding = ExtensionCreateResultFactory.Finding(
            code: ExtensionCreateFindingCode.InvalidInput,
            status: CliSemanticStatus.Invalid,
            subject: request.StableId ?? request.CataloguePath,
            cause);
        return ExtensionCreateResultFactory.Create(
            request,
            new ExtensionCreateResultOutcome
            {
                Status = CliSemanticStatus.Invalid,
                Verification = new ExtensionCreateVerification
                {
                    Catalogue = ExtensionCreateVerificationState.NotStarted,
                    Destination = ExtensionCreateVerificationState.NotStarted,
                    Manifest = ExtensionCreateVerificationState.NotStarted,
                    Payload = ExtensionCreateVerificationState.NotStarted,
                    Cause = cause,
                },
                Finding = finding,
            });
    }

    private static string? ValidateGrammar(
        ParseResult result,
        ExtensionCreateSymbols symbols,
        ExtensionCreateRequest request)
    {
        if (ReadArgumentCount(result, symbols.StableId) > 1)
        {
            return "Extension Create accepts at most one stable-ID operand.";
        }

        if (request.StableId is not null && !ExtensionIdentity.IsValidStableId(request.StableId))
        {
            return "The stable-ID operand is invalid.";
        }

        var singletonFailure = ValidateSingleton(result, symbols.Path, request.CataloguePath)
            ?? ValidateSingleton(result, symbols.Name, request.Name)
            ?? ValidateSingleton(result, symbols.Description, request.Description)
            ?? ValidateSingleton(result, symbols.PackageVersion, request.PackageVersion);
        if (singletonFailure is not null)
        {
            return singletonFailure;
        }

        var dependencyFacts = CliOptionResultFactsReader.Read(result, symbols.Dependency);
        if (dependencyFacts.IsExplicit
            && (dependencyFacts.ValueCount == 0
                || dependencyFacts.IdentifierCount != dependencyFacts.ValueCount))
        {
            return "Each --dependency occurrence requires exactly one stable ID.";
        }

        return result.Errors.Count == 0
            ? null
            : string.Join(" ", result.Errors.Select(error => error.Message));
    }

    private static string? ValidateSingleton(
        ParseResult result,
        Option<string?> option,
        string? value)
    {
        var facts = CliOptionResultFactsReader.Read(result, option);
        if (facts.IdentifierCount > 1 || facts.IsExplicitWithoutValue)
        {
            return $"{option.Name} accepts exactly one nonblank value.";
        }

        return facts.IsExplicit && string.IsNullOrWhiteSpace(value)
            ? $"{option.Name} accepts exactly one nonblank value."
            : null;
    }

    private static int ReadArgumentCount(ParseResult result, Argument<string?> argument)
        => result.GetResult(argument) is ArgumentResult argumentResult
            ? argumentResult.Tokens.Count(token => token.Type == TokenType.Argument)
            : 0;

    private static string? ReadArgument(ParseResult result, Argument<string?> argument)
    {
        try
        {
            return result.GetValue(argument);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static string? ReadSingleton(ParseResult result, Option<string?> option)
    {
        try
        {
            return result.GetValue(option);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static string[] ReadMany(ParseResult result, Option<string[]> option)
    {
        try
        {
            return result.GetValue(option) ?? [];
        }
        catch (InvalidOperationException)
        {
            return [];
        }
    }
}
