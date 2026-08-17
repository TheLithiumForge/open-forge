---
open-forge:
  description: Current maintenance contract for the installable Skills Core category entrypoint
  responsibility: Preserve native SKILL.md routing, runtime boundaries, source alignment, and deterministic skill discovery
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Core, Skill]
---

# Skills Category Maintenance Contract

## Source

[`src/open-forge/.agents/skills/_skills.md`](../../../../../../../src/open-forge/.agents/skills/_skills.md) is the canonical installed Skills entrypoint. The repository [Skills entrypoint](../../../../../../skills/_skills.md) dogfoods the same authored contract and may add local generated entries.

The [current Skills document](../../../framework/primitives/skills.md) defines the capability role, native runtime boundary, resource ownership, scope, and composition. The [Core primitive model](../../../framework/primitives/model.md#roles) owns the comparative taxonomy.

## Contract

- Frontmatter uses #LoadNow, #Core, and #Skill so the category and its selection rule enter baseline context
- The standard `route` exposes ordinary `.agents/skills/{skill-name}/SKILL.md` packages without rewriting their files
- Routed scopes beneath the Skills `root route` may expose direct native Skill packages through the same contract for generated `entries`
- The selected `SKILL.md` defines its metadata, use, instructions, resource organization, and on-demand loading
- The active agent runtime controls activation, invocation, installation, and execution
- A `route` elsewhere remains generically routable, but a familiar name or #Skill tag does not grant native Skill-package indexing outside the Skills `root route`
- Open Forge does not impose an internal `Entries`, `References`, scripts, or assets schema on a Skill package
- Loose Markdown files directly under the Skills `root route` or one of its scopes do not become native Skills
- The installable source begins with no bundled Skill packages

## Verification

- Skill routing tests verify package discovery at the Skills `root route` and within routed scopes beneath it, nested resources, generated `entries`, cross-root rejection, and loader-to-resource inheritance
- Core installation tests verify that the category installs, indexes, and remains baseline-loaded
