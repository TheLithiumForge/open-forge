namespace OpenForge.Cli.Core.Commands.Stripping.Models.Request;

// The strip Interface contract fixes this order: frontmatter or metadata, then entrypoints, then links.
internal enum StripPart
{
    Metadata,
    Frontmatter,
    Entrypoints,
    Links,
}

internal enum StripCommandKind
{
    Strip,
    Uninstall,
}
