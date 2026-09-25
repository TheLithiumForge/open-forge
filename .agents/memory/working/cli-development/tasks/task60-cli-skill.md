---
open-forge:
  description: Task 60 shipped the open-forge-cli Skill in Core and keeps open whether the loader's CLI section should later move into it
  tags: [Memory, Working, Task, Core, Skill, CLI, Loader, Contextual, Active]
---

# Task 60 — CLI Skill in Core

## Outcome

Recorded at the maintainer's request on 2026-09-25. Core ships a native Skill
that teaches an agent when and how to use the `open-forge` CLI: which command
helps with which job, the everyday flows, and the habits of previewing with
`--dry-run` and reading the result status. The loader's own CLI section probably
moves into that Skill, so the loader stays about routing and loading.

**Direction:** the maintainer asked for a task, not implementation. Changing
the loader and the Core payload are Framework changes and follow the
[deliberate Framework change](../../../../directives/open-forge/framework/_framework.md)
Directive once accepted.

**Why:**

- The loader's `CLI` section loads at startup in every workspace, including
  those without the CLI. As a Skill, the guidance loads when it's needed.
- The loader lists commands but not how to use them well. The site's
  [Working with the CLI](../../../../../src/docusaurus/docs/cli/index.md) and
  [Everyday flows](../../../../../src/docusaurus/docs/cli/flows.md) pages are a
  ready starting point for the Skill's content.
- It matches how [Task 43](task43-workflows-as-skill.md) expressed Workflows:
  a capability selected when relevant, not text every task carries.

**Settled on 2026-09-25 by the maintainer:**

- The Skill ships in Core as `.agents/skills/open-forge-cli/SKILL.md`.
- The loader keeps its CLI section and command list for now. Not every harness
  activates native Skills, so the commands stay visible in harnesses that read
  only `AGENTS.md` and the loader.
- The [Skills maintenance contract](../../../../memory/crystallized/documents/maintenance/payload/agents/skills.md)
  keeps the Skill aligned with the command reference when commands change.

**Still open:** whether the loader's CLI section should later shrink or move
into the Skill. Revisit it with [Task 53](task53-loading-and-scoping-audit.md),
once there's evidence about how harnesses activate native Skills.

**Done when:**

- [x] The Skill exists in the Core payload and this workspace, and both Skills
      entrypoints list it.
- [x] The startup context is measured before and after, using the
      [development guide's method](../../../../../docs/development.md#measure-context-size):
      startup went from about 5.7k to about 5.8k tokens (the new entry line),
      and the whole base from 14 files and about 6.4k tokens to 15 files and
      about 7.9k.
- [x] The README, the site, the Framework Architecture, and the Skills
      maintenance contract describe the new Core Skill.
- [ ] The CLI's tests match the changed payload. The hard-coded payload lists
      and install counts are updated, the unit suite passes, and the
      end-to-end suite passes except one lock-timing test on a slow run. The
      integration suite could not run locally: Smart App Control blocked the
      rebuilt test DLL. Its Install and Status snapshots record exact file,
      token, and character counts, so they need
      `OPENFORGE_SNAPSHOT_UPDATE=1` on a machine or in CI where the suite runs.
- [ ] The loader question above is decided.

## Current State

**Now:** the Skill shipped with the documentation work. The loader question
stays open by the maintainer's direction.

**Related:** [Task 53](task53-loading-and-scoping-audit.md) audits what loads at
startup, and moving the CLI section is one of its likely findings.
[Task 46](task46-routed-skill-resources.md) covers routing into a Skill's
resources, which the CLI Skill could use for a command reference.
