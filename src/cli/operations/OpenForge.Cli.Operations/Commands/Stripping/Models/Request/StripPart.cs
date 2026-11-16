namespace OpenForge.Cli.Core.Commands.Stripping.Models.Request;

// The strip Interface contract fixes this order: metadata, entries, links.
internal enum StripPart
{
    Metadata,
    Entries,
    Links,
}

internal enum StripCommandKind
{
    Strip,
    Uninstall,
}
