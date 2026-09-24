---
title: Reading installed files
description: The conventions inside Extension files, so you can tell a Template from a Memory category from a workflow recipe at a glance.
---

# Reading installed files

Extension files follow a few conventions. Once you know them, you can open any installed file and tell what it's for, even if the CLI output didn't make it obvious.

## Start with the path

The folder a file lands in tells you its role:

```text
.agents/
  guidance/adaptive-collaboration.md                        <- Guidance: advice
  patterns/work-records.md                                  <- Pattern: a reusable shape
  templates/planning/task.md                                <- Template: copy this, don't follow it
  memory/crystallized/decisions/_decisions.md               <- Memory category: where Decisions go
  skills/use-workflow/SKILL.md                              <- native Skill
  skills/use-workflow/references/development/review.md      <- workflow recipe
```

## Then check the tags

Every Extension file carries `#Extension`, plus tags for its role:

```yaml
tags: [Extension, Template, Planning, Task, Memory]
```

Look for the role tag: `Template`, `Guidance`, `Pattern`, `Workflow`, `Memory`, `Decision`, and so on. If you see `LoadNow`, the file loads whenever its parent does. First-party Extension files don't use it. They all load on demand.

## Templates: `{prompts}` and source guidance

A Template is a starter you copy, never an instruction to follow. You'll recognize one by its placeholders:

```md
# {Task Outcome}

{
Use when the task needs a durable record and no existing task source already serves it.
Replace {prompts}; remove this source guidance and sections that add no value.
Set metadata for the destination, not this Template. Rebase links after copying.
}

## Outcome

{State the result to deliver, not merely the activity to perform.}
```

- **`{Something}`** is a prompt. Replace it with your content.
- **A `{ ... }` block right after the title** is source guidance: when to use the Template and how to adapt it. Delete it from your copy.
- **The frontmatter describes the Template**, not your record. Give the copy its own description and tags, and drop `Template` and `Extension`.
- **Relative links** point from the Template's location. Fix them for the copy's location.

Your copy is yours. Updating the Template never updates copies made from it, and removing a Template never removes them.

## Memory categories: empty on purpose

A Memory category is an entrypoint that defines a kind of record. It arrives empty:

```md
## Entries

- none - No entries - #Empty
```

That's not a broken install. The category defines what a Decision, a Handoff, or an Observation is and the rules for writing one. Records appear as you and your agents create them, usually by copying the matching Template.

## Workflow recipes: Goal, Steps, Completion

A recipe always has three sections, in order:

- **Goal** says when the recipe fits and the outcome it serves.
- **Steps** is the method, including conditional work.
- **Completion** says what establishes the result, or an honest blocked boundary.

Recipes are reached through the `use-workflow` Skill, not the loader. Ask your agent to "use the review workflow", and the Skill finds the recipe in its catalogue. Finishing a recipe never grants permission to commit, merge, or publish.

## Entrypoints: the `inherited` line

Many Extension entrypoints add no rules of their own. They say so explicitly:

```md
## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.
```

It means the folder only organizes its children. Every rule from the folders above it still applies.

## Package READMEs

Each package in the source catalogue has a `README.md` and an `extension.json` manifest. Those stay in the catalogue and aren't installed into your workspace. The installed files carry everything an agent needs to understand them.
