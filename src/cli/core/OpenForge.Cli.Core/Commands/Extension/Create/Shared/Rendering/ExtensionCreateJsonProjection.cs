using OpenForge.Cli.Core.Commands.Extension.Create.Models.Presentation;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Shared.Rendering;

internal static class ExtensionCreateJsonProjection
{
    internal static ExtensionCreateJsonDocument Create(ExtensionCreateResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new ExtensionCreateJsonDocument
        {
            SchemaVersion = ExtensionCreateDefinitions.SchemaVersion,
            Command = result.Command,
            Status = CliStatusDefinitions.Read(result.Status).MachineName,
            Workspace = null,
            Result = new ExtensionCreateJsonResult
            {
                Catalogue = result.Catalogue,
                Destination = result.Destination,
                Id = result.StableId,
                Manifest = result.Manifest is null
                    ? null
                    : new ExtensionCreateJsonManifest
                    {
                        Name = result.Manifest.Name,
                        Description = result.Manifest.Description,
                        Version = result.Manifest.Version,
                        Dependencies = result.Manifest.Dependencies.ToArray(),
                    },
                Mode = ExtensionCreateDefinitions.ReadMachineName(result.Mode),
                IntendedEffects = result.IntendedEffects.Select(Effect).ToArray(),
                AppliedEffects = result.AppliedEffects.Select(Effect).ToArray(),
                Verification = new ExtensionCreateJsonVerification
                {
                    Catalogue = ExtensionCreateDefinitions.ReadVerificationState(result.Verification.Catalogue),
                    Destination = ExtensionCreateDefinitions.ReadVerificationState(result.Verification.Destination),
                    Manifest = ExtensionCreateDefinitions.ReadVerificationState(result.Verification.Manifest),
                    Payload = ExtensionCreateDefinitions.ReadVerificationState(result.Verification.Payload),
                    Cause = result.Verification.Cause,
                },
                WorkspaceLifecycleChanged = false,
            },
            Next = result.Next is null
                ? null
                : new ExtensionCreateJsonNext
                {
                    Command = result.Next.Command,
                    Reason = result.Next.Reason,
                },
        };
    }

    private static ExtensionCreateJsonEffect Effect(ExtensionCreateEffect effect)
        => new()
        {
            Kind = ExtensionCreateDefinitions.ReadEffectKind(effect.Kind),
            Path = effect.Path,
        };
}
