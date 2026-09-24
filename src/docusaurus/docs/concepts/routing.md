---
title: Routing
description: The loader, entrypoints, and Entries, and how an agent uses them to find context without reading everything.
---

# Routing

Routing is how an agent finds the right context. Instead of reading every file, it starts at the loader and follows short links whose descriptions tell it whether a branch is worth opening.

## The loader

`.agents/loader.md` is the first Open Forge file an agent reads. `AGENTS.md` points to it. It defines:

- the terms used everywhere else, such as `entrypoint`, `route`, and `scope`
- the rules (called **Axioms**) that apply to the whole workspace
- what each loading tag means
- the **root routes**: Directives, Guidance, Maps, Memory, Patterns, Skills, and Templates

A root route exists only because the loader lists it. A folder called `patterns/` somewhere deeper in the tree is just a folder with that name. It doesn't gain the Patterns rules.

## Entrypoints

Every routed folder has exactly one **entrypoint**, named after the folder with a leading underscore: `directives/_directives.md`, `memory/working/_working.md`, `directives/frontend/_frontend.md`.

An entrypoint has a predictable shape:

```md title=".agents/patterns/_patterns.md (shortened)"
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

| Part                      | What it does                                                    |
| ------------------------- | --------------------------------------------------------------- |
| Frontmatter `description` | Helps a reader decide whether to open the file.                 |
| Frontmatter `tags`        | Classify the file and, for a few defined tags, control loading. |
| Title and question        | Name the category and the one question it answers.              |
| `Axioms`                  | Rules for everything below this folder. Children inherit them.  |
| `Entries`                 | One line per direct child. This is the navigation.              |

`index.md`, `_index.md`, `references.md`, and `_references.md` are accepted as compatibility names for an entrypoint.

## Entries

Each line under `Entries` is one **entry**: a link, the child's description, and its tags.

```md
- [Run the tests before calling a change done](testing.md) - #LoadNow #Directive #Testing
```

Entries are navigation, not content. An agent reads the line, decides whether the child matters for the task, and opens it only if it does. That's why descriptions matter: they're what the agent uses to decide.

An entrypoint with no children has a placeholder line so the section is never ambiguous:

```md
- none - No entries - #Empty
```

`open-forge index` rebuilds `Entries` from the files' frontmatter. Without the CLI, keep each list in step with the files by hand.

## How an agent navigates

1. Read an entrypoint, including its Axioms.
2. Scan its `Entries`. Open the children whose descriptions, tags, or paths matter for the task.
3. Repeat for each selected child entrypoint.
4. Skip everything else. An unselected branch costs one line of context, not its whole contents.

When search or a direct link lands an agent on a file deep in the tree, it loads that file's ancestor entrypoints first, so inherited rules still apply.

## Axioms and inheritance

Axioms are required rules defined only in the loader and in entrypoints. A loaded child inherits every ancestor's Axioms and adds only what's specific to its narrower scope. An entrypoint with no local rules says so explicitly:

```md
## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.
```

Adding a rule in a child never cancels a rule from its parent. When two loaded rules conflict, the agent reports the conflict rather than silently picking one.

Next: [Scopes](scopes.md).
