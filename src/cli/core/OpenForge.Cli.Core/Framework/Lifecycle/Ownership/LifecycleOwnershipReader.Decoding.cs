using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Ownership;

internal sealed partial class LifecycleOwnershipReader
{
    private static LifecycleOwnershipReadResult Decode(
        CliWorkspace workspace,
        FileStateSnapshot snapshot)
    {
        try
        {
            _ = StrictUtf8.GetString(snapshot.Bytes.AsSpan());
            LifecycleJsonSyntaxValidator.ValidateNoDuplicateProperties(
                snapshot.Bytes.AsSpan(),
                LifecycleSection.Framework);
            LifecycleJsonSyntaxValidator.ValidateNoDuplicateProperties(
                snapshot.Bytes.AsSpan(),
                LifecycleSection.Extensions);
            var envelope = JsonSerializer.Deserialize(
                snapshot.Bytes.AsSpan(),
                LifecycleJsonContext.Default.LifecycleEnvelopeV1)
                ?? throw new JsonException("The lifecycle document cannot be null.");
            var common = LifecycleDocumentValidator.ValidateCommon(workspace, envelope);
            if (common.State != LifecycleCommonValidationState.Valid)
            {
                return Blocked(
                    LifecycleOwnershipFindingCode.LifecycleInvalid,
                    common.Cause ?? "The lifecycle common envelope is not trusted.");
            }

            if (envelope.Framework is not { } frameworkElement
                || frameworkElement.ValueKind == JsonValueKind.Null
                || envelope.Extensions is not { } extensionsElement
                || extensionsElement.ValueKind == JsonValueKind.Null)
            {
                return Blocked(
                    LifecycleOwnershipFindingCode.LifecycleInvalid,
                    "Trusted ownership requires both lifecycle sections.");
            }

            var framework = frameworkElement.Deserialize(
                LifecycleJsonContext.Default.FrameworkLifecycleState)
                ?? throw new JsonException("The lifecycle Framework section cannot be null.");
            var extensions = extensionsElement.Deserialize(
                LifecycleJsonContext.Default.ExtensionLifecycleState)
                ?? throw new JsonException("The lifecycle Extension section cannot be null.");
            var frameworkValidation = LifecycleFrameworkValidator.Validate(framework);
            if (frameworkValidation.State != LifecycleSectionValidationState.Valid)
            {
                return Blocked(
                    LifecycleOwnershipFindingCode.FrameworkBlocked,
                    frameworkValidation.Cause
                        ?? "The lifecycle Framework section is not trusted.",
                    LifecycleOwnershipSection.Framework);
            }

            var extensionsValidation = LifecycleDocumentValidator.ValidateExtensions(
                workspace,
                envelope,
                extensions);
            if (extensionsValidation.State != LifecycleReadState.Complete)
            {
                return Blocked(
                    LifecycleOwnershipFindingCode.ExtensionsBlocked,
                    extensionsValidation.Cause
                        ?? "The lifecycle Extension section is not trusted.",
                    LifecycleOwnershipSection.Extensions);
            }

            var collision = LifecycleFrameworkValidator.ValidateNoCrossSectionCollisions(
                framework,
                extensions);
            if (collision.State != LifecycleSectionValidationState.Valid)
            {
                return Blocked(
                    LifecycleOwnershipFindingCode.CrossSectionCollision,
                    collision.Cause
                        ?? "The lifecycle sections contain a cross-section ownership collision.");
            }

            var claims = framework.Targets
                .Select(target => new LifecycleOwnershipClaim(
                    target.Path,
                    LifecycleOwnershipManager.Framework,
                    framework.Source.Id))
                .Concat(extensions.Paths.SelectMany(path => path.Owners.Select(
                    owner => new LifecycleOwnershipClaim(
                        path.Path,
                        LifecycleOwnershipManager.Extension,
                        owner))));
            return new LifecycleOwnershipReadResult(
                TrustedSection(LifecycleOwnershipSection.Framework),
                TrustedSection(LifecycleOwnershipSection.Extensions),
                claims,
                snapshot.Expectation,
                []);
        }
        catch (Exception exception) when (exception is JsonException
            or DecoderFallbackException)
        {
            return Blocked(
                LifecycleOwnershipFindingCode.LifecycleInvalid,
                $"The lifecycle document is invalid: {exception.Message}");
        }
    }

    private static LifecycleOwnershipSectionResult TrustedSection(
        LifecycleOwnershipSection section)
        => new(section, LifecycleOwnershipReadState.Trusted, cause: null);
}
