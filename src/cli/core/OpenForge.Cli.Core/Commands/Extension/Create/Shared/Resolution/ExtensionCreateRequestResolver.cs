using System.Diagnostics.CodeAnalysis;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Resolution;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Manifest;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Result;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Shared.Resolution;

internal sealed class ExtensionCreateRequestResolver(CliInteractiveSession interactiveSession)
{
    private const string StableIdPrompt = "Stable ID (lowercase ASCII letters or digits separated by single hyphens):";
    private const string StableIdRetryPrompt = "That stable ID is invalid. Enter lowercase ASCII letters or digits separated by single hyphens:";
    private const string CataloguePrompt = "Catalogue path (existing ordinary directory; aliases are allowed):";
    private const string CatalogueRetryPrompt = "That path is not an existing ordinary directory. Enter another catalogue path:";

    private readonly CliInteractiveSession _interactiveSession = interactiveSession;
    private readonly PhysicalPathResolver _physicalPathResolver = new();

    internal async ValueTask<ExtensionCreateRequestResolution> ResolveAsync(
        ExtensionCreateRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var optionalFailure = ValidateOptionalInput(request);
        if (optionalFailure is not null)
        {
            return optionalFailure;
        }

        var stableId = request.StableId;
        if (stableId is null)
        {
            if (!CanPrompt(request))
            {
                return ExtensionCreateRequestResolution.Invalid(request, null, "A stable ID is required.");
            }

            stableId = await AskStableIdAsync(cancellationToken).ConfigureAwait(false);
            if (stableId is null)
            {
                return ExtensionCreateRequestResolution.Invalid(
                    request,
                    null,
                    "Input ended before a valid stable ID was supplied.");
            }

            request = request with { StableId = stableId };
        }
        else if (!ExtensionIdentity.IsValidStableId(stableId))
        {
            return ExtensionCreateRequestResolution.Invalid(request, stableId, "The stable ID is invalid.");
        }

        if (request.Dependencies.Contains(stableId, StringComparer.Ordinal))
        {
            return ExtensionCreateRequestResolution.Invalid(
                request,
                stableId,
                "An Extension cannot depend on itself.");
        }

        var cataloguePath = request.CataloguePath;
        if (cataloguePath is null)
        {
            if (!CanPrompt(request))
            {
                return ExtensionCreateRequestResolution.Invalid(request, null, "A catalogue path is required.");
            }

            ExtensionCreateCataloguePromptOutcome promptedCatalogue;
            try
            {
                promptedCatalogue = await AskCataloguePathAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return ExtensionCreateRequestResolution.Failed(
                    request,
                    ExtensionCreateResultFactory.Finding(
                        code: ExtensionCreateFindingCode.Interrupted,
                        status: CliSemanticStatus.Interrupted,
                        subject: stableId,
                        cause: "Extension Create was cancelled before all required input was supplied."));
            }

            if (promptedCatalogue.Finding is not null)
            {
                return ExtensionCreateRequestResolution.Failed(request, promptedCatalogue.Finding);
            }

            cataloguePath = promptedCatalogue.Path
                ?? throw new InvalidOperationException("A successful prompted catalogue outcome requires a path.");
        }
        else if (!IsValidPath(cataloguePath))
        {
            return ExtensionCreateRequestResolution.Invalid(
                request,
                cataloguePath,
                "The catalogue path is invalid.");
        }

        var payload = ExtensionCreateManifestBuilder.Build(request, stableId);
        return ExtensionCreateRequestResolution.Resolved(
            new ExtensionCreateResolvedRequest
            {
                StableId = stableId,
                CataloguePath = cataloguePath,
                Manifest = payload.Manifest,
                ManifestBytes = payload.Bytes,
                Mode = request.Mode,
            });
    }

    private async ValueTask<string?> AskStableIdAsync(CancellationToken cancellationToken)
    {
        var prompt = StableIdPrompt;
        while (true)
        {
            var response = await _interactiveSession
                .AskAsync($"{prompt}{Environment.NewLine}", cancellationToken)
                .ConfigureAwait(false);
            if (response.IsEndOfInput)
            {
                return null;
            }

            if (ExtensionIdentity.IsValidStableId(response.Answer))
            {
                return response.Answer;
            }

            prompt = StableIdRetryPrompt;
        }
    }

    private async ValueTask<ExtensionCreateCataloguePromptOutcome> AskCataloguePathAsync(
        CancellationToken cancellationToken)
    {
        var prompt = CataloguePrompt;
        while (true)
        {
            var response = await _interactiveSession
                .AskAsync($"{prompt}{Environment.NewLine}", cancellationToken)
                .ConfigureAwait(false);
            if (response.IsEndOfInput)
            {
                return ExtensionCreateCataloguePromptOutcome.Failed(
                    Finding(
                        code: ExtensionCreateFindingCode.InvalidInput,
                        status: CliSemanticStatus.Invalid,
                        subject: null,
                        cause: "Input ended before a valid catalogue path was supplied."));
            }

            var resolution = ResolvePromptedCatalogue(response.Answer);
            if (resolution is not null)
            {
                return resolution;
            }

            prompt = CatalogueRetryPrompt;
        }
    }

    private ExtensionCreateCataloguePromptOutcome? ResolvePromptedCatalogue(string? answer)
    {
        if (!IsValidPath(answer))
        {
            return null;
        }

        var resolution = _physicalPathResolver.ResolveRoot(answer);
        switch (resolution.State)
        {
            case PhysicalPathState.Contained:
                try
                {
                    return IsOrdinaryDirectory(resolution.GetContainedPhysicalPath())
                        ? ExtensionCreateCataloguePromptOutcome.Accepted(answer)
                        : null;
                }
                catch (Exception exception) when (IsUnavailable(exception))
                {
                    return ExtensionCreateCataloguePromptOutcome.Failed(
                        Finding(
                            code: ExtensionCreateFindingCode.CatalogueUnavailable,
                            status: CliSemanticStatus.Incomplete,
                            subject: answer,
                            cause: exception.Message));
                }
            case PhysicalPathState.Missing:
            case PhysicalPathState.Invalid:
                return null;
            case PhysicalPathState.Inaccessible:
            case PhysicalPathState.InputOutputFailure:
                return ExtensionCreateCataloguePromptOutcome.Failed(
                    Finding(
                        code: ExtensionCreateFindingCode.CatalogueUnavailable,
                        status: CliSemanticStatus.Incomplete,
                        subject: answer,
                        cause: Describe(resolution)));
            case PhysicalPathState.Dangling:
            case PhysicalPathState.External:
            case PhysicalPathState.Cycle:
            case PhysicalPathState.Unsupported:
                return ExtensionCreateCataloguePromptOutcome.Failed(
                    Finding(
                        code: ExtensionCreateFindingCode.CatalogueUnsafe,
                        status: CliSemanticStatus.Blocked,
                        subject: answer,
                        cause: Describe(resolution)));
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(resolution),
                    resolution.State,
                    "The prompted catalogue physical state is not defined.");
        }
    }

    private bool CanPrompt(ExtensionCreateRequest request)
        => request.AllowInteraction && _interactiveSession.CanPrompt;

    private static ExtensionCreateRequestResolution? ValidateOptionalInput(ExtensionCreateRequest request)
    {
        if (request.Name is not null && string.IsNullOrWhiteSpace(request.Name))
        {
            return ExtensionCreateRequestResolution.Invalid(
                request,
                request.Name,
                "--name requires a nonblank value.");
        }

        if (request.Description is not null && string.IsNullOrWhiteSpace(request.Description))
        {
            return ExtensionCreateRequestResolution.Invalid(
                request,
                request.Description,
                "--description requires a nonblank value.");
        }

        if (request.PackageVersion is not null && string.IsNullOrWhiteSpace(request.PackageVersion))
        {
            return ExtensionCreateRequestResolution.Invalid(
                request,
                request.PackageVersion,
                "--package-version requires a nonblank value.");
        }

        var dependencies = request.Dependencies;
        if (dependencies.Any(dependency => !ExtensionIdentity.IsValidStableId(dependency)))
        {
            return ExtensionCreateRequestResolution.Invalid(
                request,
                null,
                "Every dependency must be a valid stable ID.");
        }

        if (dependencies.Distinct(StringComparer.Ordinal).Count() != dependencies.Count)
        {
            return ExtensionCreateRequestResolution.Invalid(request, null, "Dependencies must be distinct.");
        }

        if (request.StableId is not null && dependencies.Contains(request.StableId, StringComparer.Ordinal))
        {
            return ExtensionCreateRequestResolution.Invalid(
                request,
                request.StableId,
                "An Extension cannot depend on itself.");
        }

        return null;
    }

    private static bool IsValidPath([NotNullWhen(true)] string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            _ = Path.GetFullPath(path);
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or PathTooLongException or NotSupportedException)
        {
            return false;
        }
    }

    private static bool IsOrdinaryDirectory(string path)
    {
        var attributes = File.GetAttributes(path);
        return (attributes & FileAttributes.Directory) != 0
            && (attributes & (FileAttributes.Device | FileAttributes.ReparsePoint)) == 0;
    }

    private static bool IsUnavailable(Exception exception)
        => exception is IOException or UnauthorizedAccessException or NotSupportedException;

    private static string Describe(PhysicalPathResolution resolution)
        => resolution.Failure?.DirectCause ?? $"The path resolved as {resolution.State}.";

    private static ExtensionCreateFinding Finding(
        ExtensionCreateFindingCode code,
        CliSemanticStatus status,
        string? subject,
        string cause)
        => ExtensionCreateResultFactory.Finding(
            code: code,
            status: status,
            subject: subject,
            cause: cause);
}
