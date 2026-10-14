---
title: Core Templates
description: One starter file for each Core category, plus a general Memory record.
---

# Core Templates

Start your own workspace instructions, advice, reusable shapes, capabilities, maps, and memory without inventing each file from scratch. This is a good first Extension if you plan to write your own content.

- **[Package ID](../glossary.md#package-id):** `core-templates`
- **Depends on:** Nothing
- **Startup visibility:** This package adds no entries to startup indexes. The Templates root [entrypoint](../glossary.md#entrypoint) stays on demand, so these starters do too.

These counts describe this package's own entries when their parent routes expose them in `Entries` and are read. Required packages add their own entries. Linked files stay on demand unless selected or required.

## Install

Package files follow your installation choices. Essentials leaves out Templates, so this package skips its starters and copies no package files. The CLI still records it as installed and names the excluded category in the result. If you want the starters, restore the Templates category before installing the package. Preview the restoration, review the plan, then apply:

```sh
open-forge route init templates --framework --dry-run
open-forge route init templates --framework
```

From your installed Open Forge workspace, preview the package and its required packages:

```sh
open-forge extension install core-templates --dry-run
```

Review the plan. Then apply it:

```sh
open-forge extension install core-templates
```

The CLI installs required packages from the same [catalogue](../glossary.md#catalogue). See the [Extension guide](/guides/extensions#install-an-extension) for scripts using `--automatic`, updates, removal, or a local source.

## Why it exists

Each Core category has a distinct role. A Directive states obligations under `Instructions`, Guidance explains a recommendation, and a Pattern shows a reusable shape. One starter per category makes that distinction clear before you write.

It's separate from the Development Toolkit because writing your own workspace content isn't specific to software development.

## What it installs

```text
.agents/templates/core/
  _core.md          <- how to choose a starter
  directive.md      <- required behavior
  guidance.md       <- recommended approach
  pattern.md        <- reusable shape
  skill.md          <- native SKILL.md capability
  template.md       <- another Template
  map.md            <- pointers to sources
  memory.md         <- a general Memory record
```

## What each file is for

| File           | Starts a...                                            | Sections you fill in                                                                                                                                                                      |
| -------------- | ------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `_core.md`     | (entrypoint)                                           | Explains how to pick the right role before copying: a Directive requires, Guidance recommends, a Pattern shapes, a Skill performs, a Template starts, a Map points, and Memory remembers. |
| `directive.md` | [Directive](../concepts/core-categories.md#directives) | `Instructions`: required actions, conditions, and meaningful exceptions. An optional `Example` illustrates the instructions.                                                              |
| `guidance.md`  | [Guidance](../concepts/core-categories.md#guidance)    | `Situation`, `Recommended Approach`, `Reasons And Tradeoffs`, `Example`.                                                                                                                  |
| `pattern.md`   | [Pattern](../concepts/core-categories.md#patterns)     | A brief `Applies To`, then the concrete `Shape`. Keep `Variations` and `Related Requirements` only when useful.                                                                           |
| `skill.md`     | [Skill](../concepts/core-categories.md#skills)         | A fenced native `SKILL.md` with `name`, `description`, `Purpose`, `Required Inputs And Resources`, `Instructions`, and `Result`.                                                          |
| `template.md`  | [Template](../concepts/core-categories.md#templates)   | Guidance for the future reader, then the sections you name for a recurring artifact. Add a section only when it helps create the artifact.                                                |
| `map.md`       | [Map](../concepts/core-categories.md#maps)             | `Scope` and `Sources`: where each source is and when to read it.                                                                                                                          |
| `memory.md`    | Memory record                                          | `Summary And Scope`, `Details And Sources`, `Limits And Next Use`. Use it when no specialized record type fits.                                                                           |

## How to use it

Keep the smallest complete result. Combine overlapping answers and remove unused prompts and optional sections. Preserve required structure and enough context for a readable progression.

> Use the relevant Core Template to write this workspace rule. Keep it in the narrowest useful scope and follow the destination's loading rules.

With the CLI, `route create --template` copies a Template's body into a new routed file. You supply the destination's description and tags, then replace the prompts. If a scope in the path doesn't exist yet, such as `frontend/` here, the command creates its folder and entrypoint too:

```sh
open-forge route create directives/frontend/components \
  --template=templates/core/directive \
  --description="Keep components presentational" \
  --tag=LoadNow --tag=Directive --tag=Frontend \
  --dry-run
```

## Good to know

- **The Skill starter is different.** Copy only the fenced block inside `skill.md` into `{skill-name}/SKILL.md` under your Skills scope. The wrapper is a routed Template, not a capability, and `route create` doesn't create native Skill packages.
- Choose a specialized Template from another Extension when one already fits, such as a Task from [Planning](planning.md) or a Decision record.
- Copies are independent. Package updates never touch the files you created from these starters.

Next: [Install this package](#install), or [update and remove Extensions](/guides/extensions#update-and-remove).
