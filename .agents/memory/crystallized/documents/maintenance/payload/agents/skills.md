---
open-forge:
  description: Current maintenance contract for the installable Skills Core category entrypoint
  responsibility: Preserve native SKILL.md routing, runtime boundaries, source alignment, and deterministic skill discovery
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Core, Skill]
---

# Skills Category Maintenance Contract

## Source

[`src/open-forge/.agents/skills/_skills.md`](../../../../../../../src/open-forge/.agents/skills/_skills.md) is the canonical installed Skills entrypoint. The repository [Skills entrypoint](../../../../../../skills/_skills.md) dogfoods the same authored contract and may add local generated entries.

[`src/open-forge/.agents/skills/open-forge-cli/SKILL.md`](../../../../../../../src/open-forge/.agents/skills/open-forge-cli/SKILL.md) is the one Skill Core ships. It identifies the most important commands and links on-demand native resources for [discovery](../../../../../../../src/open-forge/.agents/skills/open-forge-cli/references/discovery.md), [route maintenance](../../../../../../../src/open-forge/.agents/skills/open-forge-cli/references/routes.md), [Framework and packages](../../../../../../../src/open-forge/.agents/skills/open-forge-cli/references/packages.md), and [shared options and results](../../../../../../../src/open-forge/.agents/skills/open-forge-cli/references/common.md). Its repository counterpart is [`.agents/skills/open-forge-cli/SKILL.md`](../../../../../../skills/open-forge-cli/SKILL.md). The [CLI command reference](../../../../../../../docs/cli.md) defines the public commands and options, while the running executable's help defines its exact installed interface.

The [current Skills document](../../../framework/primitives/skills.md) defines the capability role, native runtime boundary, resource ownership, scope, and composition. The [Core primitive model](../../../framework/primitives/model.md#roles) owns the comparative taxonomy.

## Contract

- The title is followed by `## Which specialized capability would help with this work?` and a short paragraph describing the folder.
- Frontmatter uses #Core and #Skill without a loading tag. Check the category when work may benefit from a capability.
- The standard `route` exposes ordinary `.agents/skills/{skill-name}/SKILL.md` packages. A package the user owns is maintained like other user-owned Markdown: Index fills a missing native `name` and `description` in `SKILL.md` and the usual description and tags in other package Markdown, and Repair maintains its links. A managed package stays with its managing Update
- Routed scopes beneath the Skills `root route` may expose direct native Skill packages through the same contract for generated `entries`
- The selected `SKILL.md` defines its metadata, use, instructions, resource organization, and on-demand loading
- The active agent runtime controls activation, invocation, installation, and execution
- A `route` elsewhere remains generically routable, but a familiar name or #Skill tag does not grant native Skill-package indexing outside the Skills `root route`
- Open Forge does not impose an internal `Entries`, `References`, scripts, or assets schema on a Skill package
- Loose Markdown files directly under the Skills `root route` or one of its scopes do not become native Skills
- The installable source ships one Skill package, `open-forge-cli`, and other Skills arrive through Extensions or the workspace itself
- When present, `open-forge-cli/SKILL.md` must be read before CLI commands other than `context`. It is not a startup requirement. Intentional omission is respected, and unavailable CLI commands fall back to plain files.
- The loader keeps the Context entry path and names Include. The CLI Skill and its task-specific resources carry command discovery, navigation, mutation and lifecycle details. Shared flags are explained once and exact installed syntax comes from command help. The file-format section of the [workspace guide](README.md) defines workspace metadata formats, and the Skill points to it or to `open-forge help concepts`.
- Keep the CLI Skill and all its resources byte-aligned between the installable source and repository counterpart. Reference pages carry concise Open Forge metadata for retrieval and health checks. They need no entrypoint or blanket loading rule.

## Verification

- [IndexSkillPackageMetadataIntegrationTests](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Index/IndexSkillPackageMetadataIntegrationTests.cs) cover user-owned native and resource metadata. [IndexSkillCatalogueIntegrationTests](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Index/IndexSkillCatalogueIntegrationTests.cs) cover bounded resource catalogue maintenance.
- [GeneratedNavigationSkillProjectionTests](../../../../../../../src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/GeneratedNavigation/GeneratedNavigationSkillProjectionTests.cs) cover native package navigation. These checks do not activate a Skill or execute its resources.
- Use the [payload integrity checks](../_payload.md#verification) for the category and packaged Skill. Compare both complete Skill trees directly where shared text is intended.
- Check plain startup omits Skills, selecting the category exposes its packages and CLI use requires the guide only before commands other than Context. Deliberate omission leaves no unconditional required read.
- When commands, flags, defaults or lifecycle behavior change, review the matching Skill resources against command definitions and help. Check examples, relative links and source-to-repository parity.
