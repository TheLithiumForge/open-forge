---
title: Reading installed files
description: The conventions inside Extension files, so you can tell a Template from a Memory category from a workflow recipe at a glance.
---

import Tabs from "@theme/Tabs"
import TabItem from "@theme/TabItem"

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

Every Extension file except a native `SKILL.md` carries the `Extension` tag in its frontmatter, plus tags for its role:

Choose root fields or an `open-forge:` mapping for your workspace. Both are readable in every workspace. New files follow `.agents/open-forge.json`. A missing `frontmatter` setting means scoped.

<Tabs groupId="frontmatter-form">
<TabItem value="root" label="Root" default>

```yaml
---
description: Start a task record
tags: [Extension, Template, Planning, Task, Memory]
---
```

</TabItem>
<TabItem value="scoped" label="Scoped">

```yaml
---
open-forge:
  description: Start a task record
  tags: [Extension, Template, Planning, Task, Memory]
---
```

</TabItem>
</Tabs>

Framework and Extension delivery uses the workspace's form for leading Open Forge metadata. Repository sources and fenced body examples stay unchanged. Native `SKILL.md` keeps its own metadata contract. [Installation](../getting-started/installation.md#configure-an-existing-workspace) explains how Configure converts unedited owned files while keeping and reporting edited files.

When an `open-forge` mapping exists, root descriptions and tags belong to another tool. Open Forge does not merge them. `applyTo` is read at both locations, with equivalent sets counting once and conflicting sets rejected.

Look for the role tag: `Template`, `Guidance`, `Pattern`, `Workflow`, `Memory`, `Decision`, and so on. A `SKILL.md` keeps its harness's own frontmatter instead, with no Open Forge tags.

A `LoadNow` or `KeepInMind` tag would make a file load as soon as its parent entrypoint (the `_{folder}.md` file that lists it under `Entries`) is read. First-party Extension files carry neither, so all of them are on demand. At startup, an agent sees at most a file's one-line entry in a parent entrypoint that's already loaded. It opens the file only when a task needs it.

## Templates: `{prompts}` and source guidance

A Template is a starter you copy, never an instruction to follow. You'll recognize one by its placeholders. Here's the start of the Planning `task.md` Template:

```md
# {Task Outcome}

{
Use when the task needs a durable record and no existing task source already serves it. A plan does not grant permission.
Replace {prompts}. Remove this source guidance and optional sections that add no value.
}

## Outcome

{State the result to deliver, not merely the activity to perform.}
```

- **`{Something}`** is a prompt. Replace it with your content.
- **A `{ ... }` block right after the title** is source guidance: when to use the Template and how to adapt it. Delete it from your copy.
- **The frontmatter describes the Template**, not your record. Give the copy its own description and tags in the workspace's chosen form. Drop `Extension`, and drop `Template` unless the copy is itself a new Template.
- **Relative links** point from the Template's location. Fix them for the copy's location.

Your copy is yours. Updating the Template never updates copies made from it, and removing a Template never removes them.

## Memory categories: empty on purpose

A Memory category is an entrypoint that defines a kind of record. It arrives empty:

```md
## Entries

- none - No entries - #Empty
```

That's not a broken install. The category defines what a Decision, a Handoff, or an Observation is and the rules for writing one. Records appear as you and your agents create them, usually by copying the matching Template.

A category has no loading tag. At startup, an agent sees only its one-line entry in the parent state's `Entries`, such as the Decisions entry in Crystallized. The category entrypoint and its records open on demand.

## Workflow recipes: Goal, Steps, Completion

Every recipe has these three sections, in this order, and may add others:

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

It means the entrypoint adds no rules of its own. It can still explain its folder and list its children. Every rule from the entrypoints above it still applies.

## Package READMEs

Each package in the source catalogue has a `README.md` and an `extension.json` manifest. Those stay in the catalogue and aren't installed into your workspace. The installed files carry everything an agent needs to understand them.
