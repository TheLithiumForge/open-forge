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

- Frontmatter uses #LoadNow, #Core, and #Skill so the category and its selection rule enter baseline context
- The standard `route` exposes ordinary `.agents/skills/{skill-name}/SKILL.md` packages without rewriting their files
- Routed scopes beneath the Skills `root route` may expose direct native Skill packages through the same contract for generated `entries`
- The selected `SKILL.md` defines its metadata, use, instructions, resource organization, and on-demand loading
- The active agent runtime controls activation, invocation, installation, and execution
- A `route` elsewhere remains generically routable, but a familiar name or #Skill tag does not grant native Skill-package indexing outside the Skills `root route`
- Open Forge does not impose an internal `Entries`, `References`, scripts, or assets schema on a Skill package
- Loose Markdown files directly under the Skills `root route` or one of its scopes do not become native Skills
- The installable source ships one Skill package, `open-forge-cli`, and other Skills arrive through Extensions or the workspace itself
- The baseline-loaded Skills entrypoint explicitly requires reading `open-forge-cli/SKILL.md` when present. This makes usage instructions available at startup without changing native Skill metadata, generated #Skill entries, or runtime invocation policy. Intentional omissions remain respected; the loader's CLI summary stays available. Other Skills remain selected on demand.
- The loader keeps its short CLI command list so harnesses that never activate native Skills still see the commands. The `open-forge-cli` Skill keeps startup instructions short and teaches every command and flag in task-specific resources loaded only when relevant. Shared flags are explained once. The guide defers to `open-forge <command> --help` for the installed version.
- Keep the CLI Skill and all its resources byte-aligned between the installable source and repository counterpart. Reference pages carry concise Open Forge metadata for retrieval and health checks. They need no entrypoint or blanket loading rule.

## Verification

- Verify native package discovery at the Skills `root route` and within routed scopes beneath it, plus package-local resource structure, generated `entries`, and cross-root rejection.
- Verify Index selection separately: it discovers recognized entrypoints in each eligible Skill package's immediate child directories and maintains their generated `Entries`. This selection does not automatically load resources or activate the Skill.
- Core installation tests verify that the category installs, indexes, and remains baseline-loaded
- Inspect the installed Skills entrypoint's conditional required-read path and its target. Verify default installation loads the usage Skill and a deliberately omitted Skill leaves no dangling required-read link. Verify `context skills/open-forge-cli` batches the tag-derived startup closure and this explicitly selected Skill without duplicate sources; plain `context` reports the tag-derived closure and does not interpret prose instructions.
- When a command, flag, default, status meaning, or lifecycle behavior changes, review the CLI Skill and relevant resources against the command reference and current local help. Verify complete command and flag coverage, valid examples and links, and byte alignment of both package trees.
