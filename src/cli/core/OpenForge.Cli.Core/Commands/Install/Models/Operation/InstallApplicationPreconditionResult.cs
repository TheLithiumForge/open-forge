using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Install.Models.Operation;

internal sealed record InstallApplicationPreconditionResult
{
    private InstallApplicationPreconditionResult(
        MutationValidationResult validation,
        InstallFindingCode? findingCode)
    {
        Validation = validation;
        FindingCode = findingCode;
    }

    internal MutationValidationResult Validation { get; }

    internal InstallFindingCode? FindingCode { get; }

    internal bool IsValid => FindingCode is null;

    internal static InstallApplicationPreconditionResult Valid(
        MutationValidationResult validation)
    {
        if (validation.State != MutationValidationState.Valid)
        {
            throw new ArgumentException(
                "A valid Install application precondition requires valid mutation checks.",
                nameof(validation));
        }

        return new InstallApplicationPreconditionResult(validation, findingCode: null);
    }

    internal static InstallApplicationPreconditionResult Boundary(
        MutationValidationResult validation,
        InstallFindingCode findingCode)
    {
        if (validation.State == MutationValidationState.Valid)
        {
            throw new ArgumentException(
                "An Install application precondition boundary cannot carry valid mutation checks.",
                nameof(validation));
        }

        return new InstallApplicationPreconditionResult(validation, findingCode);
    }
}
