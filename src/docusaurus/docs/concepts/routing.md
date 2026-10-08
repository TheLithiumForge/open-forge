---
title: Routing
description: The loader, entrypoints, and Entries, and how an agent uses them to find context without reading everything.
---

import Tabs from "@theme/Tabs"
import TabItem from "@theme/TabItem"

# Routing

Routing is how an agent finds the right context. Instead of reading every file, it starts at the loader and follows short links whose descriptions tell it whether a branch is worth opening.

## The loader

An agent enters through `AGENTS.md`, which tells it to read `.agents/loader.md` before starting a task. In Claude Code, `CLAUDE.md` imports both files. The loader defines:

- the terms used everywhere else, such as `entrypoint`, `route`, and `scope`
- the rules (called **Axioms**) that apply to the whole workspace
- what each defined tag means, including the two loading tags
- the frontmatter fields: `description`, `responsibility`, `tags`, and `applyTo`
- the **root routes**: Directives, Guidance, Maps, Memory, Patterns, Skills, and Templates

A root route exists only because the loader lists it. A folder called `patterns/` somewhere deeper in the tree is an ordinary scope with that name. It doesn't gain the Patterns rules.

## Entrypoints

Every routed folder has exactly one **entrypoint**, named after the folder with a leading underscore: `directives/_directives.md`, `memory/working/_working.md`, `directives/frontend/_frontend.md`.

An entrypoint has a predictable shape:

Choose root fields or an `open-forge:` mapping for your workspace. Both are readable in every workspace. New files follow `.agents/open-forge.json`. A missing `frontmatter` setting means scoped.

<Tabs groupId="frontmatter-form">
<TabItem value="root" label="Root" default>

```md title=".agents/patterns/_patterns.md (shortened, with an example entry)"
---
description: Reusable default shapes for code, files, APIs, documents, and other work
tags: [LoadNow, Core, Pattern]
---

# Patterns

## What reusable shape makes related work easy to create and inspect?

Patterns define reusable default shapes that make related work consistent...

## Axioms

- Check `Entries` when the work creates, changes, or reviews something with a visible structure.
- ...

## Entries

- [Keep API responses in one envelope shape](api-envelope.md) - #Pattern #API
```

</TabItem>
<TabItem value="scoped" label="Scoped">

```md title=".agents/patterns/_patterns.md (shortened, with an example entry)"
---
open-forge:
  description: Reusable default shapes for code, files, APIs, documents, and other work
  tags: [LoadNow, Core, Pattern]
---

# Patterns

## What reusable shape makes related work easy to create and inspect?

Patterns define reusable default shapes that make related work consistent...

## Axioms

- Check `Entries` when the work creates, changes, or reviews something with a visible structure.
- ...

## Entries

- [Keep API responses in one envelope shape](api-envelope.md) - #Pattern #API
```

</TabItem>
</Tabs>

When an `open-forge` mapping exists, root descriptions and tags belong to another tool. Open Forge does not merge them. `applyTo` is read at both locations, with equivalent sets counting once and conflicting sets rejected.

The same precedence applies to `responsibility`. Frontmatter alone does not create a route. Native `SKILL.md` files keep their own metadata contract. [Loading and tags](loading-and-tags.md#frontmatter) explains the fields and how the workspace chooses its output form.

| Part                      | What it does                                                    |
| ------------------------- | --------------------------------------------------------------- |
| Frontmatter `description` | Helps a reader decide whether to open the file.                 |
| Frontmatter `tags`        | Classify the file and, for a few defined tags, control loading. |
| Title and question        | Name the category and the one question it answers.              |
| `Axioms`                  | Required rules for this route. Loaded descendants inherit them. |
| `Entries`                 | One line per direct child. This is the navigation.              |

`index.md`, `_index.md`, `references.md`, and `_references.md` are accepted as compatibility names for an entrypoint.

## Entries

Each line under `Entries` is one **entry**: a link, the child's description, and its tags. If the source declares `applyTo`, the entry also displays those patterns after its tags.

```md
- [Run the tests before calling a change done](testing.md) - #LoadNow #Directive #Testing
- [C# rules](csharp.md) - #LoadNow #Directive - applies to `**/*.cs`
```

Entries are navigation, not content. An agent reads the line, decides whether the child matters for the task, and opens it only if it does. Descriptions matter because they're what the agent uses to decide. The exception is an entry tagged `#LoadNow` or `#KeepInMind`: the agent reads its file as soon as the parent loads, if its own and inherited file conditions allow it. [Loading and tags](loading-and-tags.md) explains both, and how [`applyTo` patterns](loading-and-tags.md#file-conditions) filter entries by the files a task works on.

An entrypoint with no children has a placeholder line so the section is never ambiguous:

```md
- none - No entries - #Empty
```

`open-forge index` rebuilds `Entries` from the files' frontmatter. Without the CLI, update the list by hand whenever a file's path, description, or tags change.

## How an agent navigates

1. Read an entrypoint, including its Axioms.
2. Read every child whose entry is tagged `#LoadNow` or `#KeepInMind`, in listed order.
3. Scan the other entries. Open the children whose descriptions, tags, or paths matter for the task.
4. Repeat for each selected child entrypoint.
5. Skip everything else. An unselected branch costs one line of context, not its whole contents.

When search or a direct link selects a file deep in the tree, the agent loads that file's ancestor entrypoints before using it, so inherited rules still apply.

## Axioms and inheritance

Axioms are required rules defined only in the loader and in entrypoints. A loaded child inherits every ancestor's Axioms and adds only what's specific to its narrower scope. An entrypoint with no local rules says so explicitly:

```md
## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.
```

A missing or empty `Axioms` section means the same thing. Adding a rule in a child never cancels a rule from its parent. When two loaded rules conflict, the loader asks the agent to report the conflict rather than silently pick one.

Next: [Scopes](scopes.md).
