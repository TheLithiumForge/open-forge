namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;

internal enum ExtensionRemoveEffectKind
{
    PackageFile,
    GeneratedRegion,
    Lifecycle,
    Settings,
    Directory,
}

internal enum ExtensionRemoveEffectAction
{
    ReleaseOwnership,
    Delete,
    Retain,
    RecordExclusion,
    Create,
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
