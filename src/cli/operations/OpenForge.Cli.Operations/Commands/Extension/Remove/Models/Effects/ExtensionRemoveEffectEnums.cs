namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;

internal enum ExtensionRemoveEffectKind
{
    PackageFile,
    GeneratedRegion,
    Lifecycle,
}

internal enum ExtensionRemoveEffectAction
{
    ReleaseOwnership,
    Delete,
    Retain,
}

internal enum ExtensionRemoveEffectOutcome
{
    Planned,
    NotStarted,
    Verified,
    VerificationFailed,
    CompletionUnknown,
}

internal enum ExtensionRemoveEffectResidual
{
    None,
    Retained,
    Unknown,
}
