namespace OpenForge.Cli.Core.Commands.Install.Models.Result;

internal sealed record InstallInvalidOption(InstallOptionKind Kind, string Value);

internal enum InstallOptionKind
{
    Preset,
    Frontmatter,
}
