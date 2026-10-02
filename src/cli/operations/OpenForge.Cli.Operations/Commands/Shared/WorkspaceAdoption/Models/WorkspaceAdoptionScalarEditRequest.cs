using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;

internal sealed record WorkspaceAdoptionScalarEditRequest
{
    internal WorkspaceAdoptionScalarEditRequest(
        WorkspaceAdoptionSkillFrontmatterContext frontmatter,
        YamlNode node,
        string field,
        string value)
    {
        ArgumentNullException.ThrowIfNull(frontmatter);
        ArgumentNullException.ThrowIfNull(node);
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        ArgumentNullException.ThrowIfNull(value);

        Frontmatter = frontmatter;
        Node = node;
        Field = field;
        Value = value;
    }

    internal WorkspaceAdoptionSkillFrontmatterContext Frontmatter { get; }

    internal YamlNode Node { get; }

    internal string Field { get; }

    internal string Value { get; }
}
