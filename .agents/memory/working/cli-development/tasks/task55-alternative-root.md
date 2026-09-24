---
open-forge:
  description: Open Task 55 to investigate running Open Forge under another root such as .apm, so users can also use APM frontmatter, packaging, and compilation
  tags: [Memory, Working, Task, Framework, Root, APM, Investigation, Release, Contextual, Active]
---

# Task 55 — Alternative workspace root such as `.apm`

## Outcome

Recorded at the maintainer's request on 2026-09-25. Decide whether Open Forge
can live under a root other than `.agents/`, for example `.apm/` with routed
content inside an APM folder such as `instructions/`, so a user can combine
Open Forge routing with APM's frontmatter, packaging, and compilation to
harness targets.

The result is a decision-ready analysis with a small prototype, not an
implementation. A root change is breaking for installed workspaces and
Extension packages, so it should be settled before 1.0 fixes the layout.

**Direction:** the maintainer asked for the task. This record authorizes no
Framework, CLI, or payload change.

**In scope:**

- How deeply `.agents` is assumed today, and what a configurable or alternative
  root would cost.
- Whether one file can carry both APM's frontmatter and the `open-forge:` block,
  or whether Open Forge should read the APM fields natively.
- How Open Forge categories would map onto APM's content types.
- Whether APM compilation preserves on-demand loading or flattens routed
  content into always-loaded context.

**Preserve / out of scope:** existing `.agents/` workspaces keep working
throughout. This repository's own APM use for agent projections
(`apm.yml`, `.apm/agents/`, `npm run apm:install`) is evidence, not something
this task changes.

**Done when:**

- [ ] An inventory lists every place that assumes `.agents`: loader and
      `AGENTS.md` text, CLI source-reference grammar and containment, the lock
      and settings paths, Extension `content/.agents/` layout, route IDs, tests,
      and documentation.
- [ ] APM's current layout, file naming, frontmatter fields, and compile
      behavior are confirmed against APM's own documentation, not assumed.
- [ ] A mapping table covers each Core category, Memory state, Template,
      Skill, and workflow recipe, and marks where APM has no counterpart.
- [ ] At least these options are compared, with a recommendation:
  - a configurable root, set in the settings file, with the same routing inside
  - an APM export or adapter that generates APM content from `.agents/`
  - keeping `.agents/` and reading APM frontmatter fields as native metadata
- [ ] A throwaway prototype shows one workspace working under the recommended
      shape, including what `apm compile` produces from it.
- [ ] The maintainer accepts or rejects the recommendation.

## Plan

1. **Confirm APM.** Read APM's current documentation for the `.apm/` layout,
   `.instructions.md` and related file types, their frontmatter, and what each
   compile target writes.
2. **Inventory `.agents`.** Search the CLI, payload, Extensions, tests, and
   docs for the hard-coded root, and group the hits by kind.
3. **Map and compare.** Build the category mapping and the options comparison,
   including migration cost for existing workspaces and Extension packages.
4. **Prototype** the leading option in a scratch workspace outside this
   repository.
5. **Recommend** one option with its compatibility and migration plan.

## Current State

**Now:** recorded, not started.

**Related:** [Task 46](task46-routed-skill-resources.md) already reads route
metadata from native `SKILL.md` frontmatter instead of requiring an Open Forge
block. Reading APM frontmatter would extend the same idea. Tasks
[53](task53-loading-and-scoping-audit.md) and [54](task54-tag-trimming.md) edit
the same frontmatter, so agree on the order before any of them changes files.
