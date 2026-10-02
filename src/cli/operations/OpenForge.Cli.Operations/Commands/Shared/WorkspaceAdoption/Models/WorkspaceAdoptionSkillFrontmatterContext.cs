using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

namespace OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;

internal sealed record WorkspaceAdoptionSkillFrontmatterContext
{
    internal WorkspaceAdoptionSkillFrontmatterContext(
        string yaml,
        int yamlStart,
        YamlNode? root,
        SourceOpenForgeMetadataFacts openForge)
    {
        ArgumentNullException.ThrowIfNull(yaml);
        if (yamlStart < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(yamlStart));
        }

        ArgumentNullException.ThrowIfNull(openForge);

        Yaml = yaml;
        YamlStart = yamlStart;
        Root = root;
        OpenForge = openForge;
    }

    internal string Yaml { get; }

    internal int YamlStart { get; }

    internal YamlNode? Root { get; }

    internal SourceOpenForgeMetadataFacts OpenForge { get; }
}
