namespace OpenForge.Cli.Core.Commands.References.Models.Result;

/// <summary>
/// The command's own vocabulary for facts it observes through Framework types. Rendering may not
/// import the Framework layer, so References restates the few values its output needs, the way
/// the other command families already do for their own observations.
/// </summary>
internal enum ReferencesLayer
{
    Base,
    Overwrite,
}

internal enum ReferencesSelectorRole
{
    Include,
    Exclude,
}
