using OpenForge.Cli.Core.Commands.Extension.Create.Models.Manifest;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Models.Resolution;

internal sealed record ExtensionCreateResolvedRequest
{
    private ReadOnlyMemory<byte> _manifestBytes;

    public required string StableId { get; init; }

    public required string CataloguePath { get; init; }

    public required ExtensionCreateManifest Manifest { get; init; }

    public required ReadOnlyMemory<byte> ManifestBytes
    {
        get => _manifestBytes;
        init => _manifestBytes = value.ToArray();
    }

    public required ExtensionCreateMode Mode { get; init; }

    public bool Automatic { get; init; }
}

internal sealed record ExtensionCreateRequestResolution
{
    private ExtensionCreateRequestResolution(
        ExtensionCreateResolvedRequest? request,
        ExtensionCreateRequest? partialRequest,
        ExtensionCreateFinding? finding)
    {
        if (request is not null && (partialRequest is not null || finding is not null))
        {
            throw new ArgumentException("A resolved request cannot retain terminal request facts or a finding.");
        }

        if (request is null && (partialRequest is null || finding is null))
        {
            throw new ArgumentException("A terminal request resolution requires partial request facts and a finding.");
        }

        Request = request;
        PartialRequest = partialRequest;
        Finding = finding;
    }

    internal ExtensionCreateResolvedRequest? Request { get; }

    internal ExtensionCreateRequest? PartialRequest { get; }

    internal ExtensionCreateFinding? Finding { get; }

    internal static ExtensionCreateRequestResolution Resolved(ExtensionCreateResolvedRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new ExtensionCreateRequestResolution(
            request: request,
            partialRequest: null,
            finding: null);
    }

    internal static ExtensionCreateRequestResolution Invalid(
        ExtensionCreateRequest partialRequest,
        string? subject,
        string cause)
    {
        ArgumentNullException.ThrowIfNull(partialRequest);
        return new(
            request: null,
            partialRequest: partialRequest,
            finding: new ExtensionCreateFinding
            {
                Code = ExtensionCreateFindingCode.InvalidInput,
                Status = Shell.Definitions.CliSemanticStatus.Invalid,
                Subject = subject,
                Cause = cause,
            });
    }

    internal static ExtensionCreateRequestResolution Failed(
        ExtensionCreateRequest partialRequest,
        ExtensionCreateFinding finding)
    {
        ArgumentNullException.ThrowIfNull(partialRequest);
        ArgumentNullException.ThrowIfNull(finding);
        return new(
            request: null,
            partialRequest: partialRequest,
            finding: finding);
    }
}

internal sealed record ExtensionCreateCataloguePromptOutcome
{
    private ExtensionCreateCataloguePromptOutcome(
        string? path,
        ExtensionCreateFinding? finding)
    {
        if ((path is null) == (finding is null))
        {
            throw new ArgumentException("A prompted catalogue outcome requires exactly one path or finding.");
        }

        Path = path;
        Finding = finding;
    }

    internal string? Path { get; }

    internal ExtensionCreateFinding? Finding { get; }

    internal static ExtensionCreateCataloguePromptOutcome Accepted(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return new ExtensionCreateCataloguePromptOutcome(path, null);
    }

    internal static ExtensionCreateCataloguePromptOutcome Failed(ExtensionCreateFinding finding)
    {
        ArgumentNullException.ThrowIfNull(finding);
        return new ExtensionCreateCataloguePromptOutcome(null, finding);
    }
}
