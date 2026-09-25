---
title: Core Templates
description: One starter file for each Core category, plus a general Memory record.
---

# Core Templates

Start your own workspace instructions, advice, reusable shapes, capabilities, maps, and memory without inventing each file from scratch. This is a good first Extension if you plan to write your own content.

- **Package ID:** `core-templates`
- **Depends on:** Nothing
- **Loads at startup:** Nothing. Templates are always on demand.

## Why it exists

The first Directive or Pattern you write sets the tone for the rest, and guessing its shape is where inconsistency starts. One starter per category makes each role concrete: a Directive has an `Instructions` section, Guidance explains its tradeoffs, a Pattern shows an example.

It's separate from the Development Toolkit because writing your own workspace content has nothing to do with software in particular.

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
| `directive.md` | [Directive](../concepts/core-categories.md#directives) | `Instructions`: the required behavior for one clearly selected scope.                                                                                                                     |
| `guidance.md`  | [Guidance](../concepts/core-categories.md#guidance)    | `Situation`, `Recommended Approach`, `Reasons And Tradeoffs`, `Example`.                                                                                                                  |
| `pattern.md`   | [Pattern](../concepts/core-categories.md#patterns)     | `Applies To`, `Shape`, `Example`, `Variations And Tradeoffs`.                                                                                                                             |
| `skill.md`     | [Skill](../concepts/core-categories.md#skills)         | A fenced native `SKILL.md` with `name`, `description`, `Purpose`, and required inputs.                                                                                                    |
| `template.md`  | [Template](../concepts/core-categories.md#templates)   | Named sections for a recurring artifact, kept only if they earn their place.                                                                                                              |
| `map.md`       | [Map](../concepts/core-categories.md#maps)             | `Scope` and `Sources`: where each source is and when to read it.                                                                                                                          |
| `memory.md`    | Memory record                                          | `Summary And Scope`, `Details And Sources`, `Limits And Next Use`. Use it when no specialized record type fits.                                                                           |

## How to use it

> Use the relevant Core Template to write this workspace rule. Keep it in the narrowest useful scope and follow the destination's loading rules.

With the CLI, `route create --template` copies a Template's body into a new routed file. You supply the destination's description and tags, then replace the prompts:

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
