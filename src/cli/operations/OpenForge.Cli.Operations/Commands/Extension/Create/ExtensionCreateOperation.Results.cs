using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Create;

internal sealed partial class ExtensionCreateOperation
{
    private static ExtensionCreateResult Complete(
        ExtensionCreatePlan plan,
        IEnumerable<ExtensionCreateEffect> appliedEffects,
        bool verified,
        ExtensionCreateMode? mode = null)
        => ExtensionCreateResultFactory.Create(
            plan,
            new ExtensionCreateResultOutcome
            {
                Status = CliSemanticStatus.Complete,
                AppliedEffects = appliedEffects.ToArray(),
                Verification = new ExtensionCreateVerification
                {
                    Catalogue = ExtensionCreateVerificationState.Verified,
                    Destination = verified ? ExtensionCreateVerificationState.Verified : ExtensionCreateVerificationState.Planned,
                    Manifest = verified ? ExtensionCreateVerificationState.Verified : ExtensionCreateVerificationState.Planned,
                    Payload = verified ? ExtensionCreateVerificationState.Verified : ExtensionCreateVerificationState.Planned,
                    Cause = null,
                },
            },
            mode);

    private static ExtensionCreateResult Failure(
        ExtensionCreatePlan plan,
        IReadOnlyList<ExtensionCreateEffect> appliedEffects,
        ExtensionCreateFinding finding)
    {
        var manifestApplied = appliedEffects.Any(effect => effect.Kind == ExtensionCreateEffectKind.ManifestFile);
        var payloadApplied = appliedEffects.Any(effect => effect.Kind == ExtensionCreateEffectKind.PayloadAgentsDirectory);
        return ExtensionCreateResultFactory.Create(
            plan,
            new ExtensionCreateResultOutcome
            {
                Status = finding.Status,
                AppliedEffects = appliedEffects,
                Verification = new ExtensionCreateVerification
                {
                    Catalogue = finding.Code == ExtensionCreateFindingCode.CatalogueUnavailable
                        ? ExtensionCreateVerificationState.Unavailable
                        : ExtensionCreateVerificationState.Verified,
                    Destination = ExtensionCreateVerificationState.Failed,
                    Manifest = manifestApplied ? ExtensionCreateVerificationState.Verified : ExtensionCreateVerificationState.NotStarted,
                    Payload = payloadApplied ? ExtensionCreateVerificationState.Verified : ExtensionCreateVerificationState.NotStarted,
                    Cause = finding.Cause,
                },
                Finding = finding,
            });
    }

    private static ExtensionCreateResult CreateTerminalRequestResult(
        ExtensionCreateRequest request,
        ExtensionCreateFinding finding)
        => ExtensionCreateResultFactory.Create(
            request,
            new ExtensionCreateResultOutcome
            {
                Status = finding.Status,
                Verification = new ExtensionCreateVerification
                {
                    Catalogue = ExtensionCreateVerificationState.NotStarted,
                    Destination = ExtensionCreateVerificationState.NotStarted,
                    Manifest = ExtensionCreateVerificationState.NotStarted,
                    Payload = ExtensionCreateVerificationState.NotStarted,
                    Cause = finding.Cause,
                },
                Finding = finding,
            });

    private static ExtensionCreateResult Interrupted(ExtensionCreateRequest request)
    {
        var finding = ExtensionCreateResultFactory.Finding(
            code: ExtensionCreateFindingCode.Interrupted,
            status: CliSemanticStatus.Interrupted,
            subject: request.CataloguePath,
            cause: "Extension create was cancelled. Nothing was changed.");
        return CreateTerminalRequestResult(request, finding);
    }

    private static ExtensionCreateResult Interrupted(ExtensionCreatePlan plan)
    {
        var finding = ExtensionCreateResultFactory.Finding(
            code: ExtensionCreateFindingCode.Interrupted,
            status: CliSemanticStatus.Interrupted,
            subject: plan.Destination,
            cause: "Extension create was cancelled. Nothing was changed.");
        return ExtensionCreateResultFactory.Create(
            plan,
            new ExtensionCreateResultOutcome
            {
                Status = CliSemanticStatus.Interrupted,
                Verification = new ExtensionCreateVerification
                {
                    Catalogue = ExtensionCreateVerificationState.Verified,
                    Destination = ExtensionCreateVerificationState.Planned,
                    Manifest = ExtensionCreateVerificationState.NotStarted,
                    Payload = ExtensionCreateVerificationState.NotStarted,
                    Cause = finding.Cause,
                },
                Finding = finding,
            });
    }

    private static ExtensionCreateResult ConfirmationRequired(ExtensionCreatePlan plan)
    {
        var finding = ExtensionCreateResultFactory.Finding(
            code: ExtensionCreateFindingCode.ConfirmationRequired,
            status: CliSemanticStatus.Invalid,
            subject: plan.Destination,
            cause: "Extension create needs confirmation, and this session cannot ask.");
        return ExtensionCreateResultFactory.Create(
            plan,
            new ExtensionCreateResultOutcome
            {
                Status = CliSemanticStatus.Invalid,
                Verification = new ExtensionCreateVerification
                {
                    Catalogue = ExtensionCreateVerificationState.Verified,
                    Destination = ExtensionCreateVerificationState.Planned,
                    Manifest = ExtensionCreateVerificationState.NotStarted,
                    Payload = ExtensionCreateVerificationState.NotStarted,
                    Cause = finding.Cause,
                },
                Finding = finding,
            });
    }
}
