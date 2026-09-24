---
title: Loading and tags
description: What the defined tags mean, which ones control loading, and how to choose between on demand, LoadNow, and KeepInMind.
---

# Loading and tags

Tags help readers, human or agent, decide what to open and what belongs where. A few of them have defined behavior. The rest are yours to describe your own subjects.

## Frontmatter

Routed Markdown files carry a small frontmatter block:

```yaml
---
open-forge:
  description: Understand the service boundaries and how requests move through the system
  responsibility: Define the current service structure and dependency boundaries
  tags: [Memory, Document, Architecture]
---
```

- **description** helps a reader decide whether to open the file. It becomes the text of the file's entry.
- **responsibility** (optional) helps an editor decide what belongs in the file. It creates no authority or loading behavior.
- **tags** are bare values without `#`. In `Entries` and prose they're written with `#`, like `#LoadNow`.

## Loading tags

Entries stay **on demand** unless a loading tag says otherwise. There are exactly two loading tags:

| Tag           | Behavior                                                                                                                                                               |
| ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `#LoadNow`    | Read the file when its already-loaded parent exposes it, in listed order. If it's an entrypoint, apply its own children's loading rules too.                           |
| `#KeepInMind` | Read it when its parent loads, then refresh it at task start or resume, after context restoration, and before handoff or closeout, for as long as its scope is active. |

Loading tags act only through a parent that's already loaded. They can't pull in a scope nobody selected.

### Choosing a loading behavior

- **On demand** (no tag) is the default. Use it for anything optional.
- **`#LoadNow`** is for content needed every time its parent loads, where missing it would cost more than reading it each time. Every Directive file in a Directives folder carries it, because Directives are mandatory within their scope.
- **`#KeepInMind`** is for continuity that must be re-read at the refresh points, such as an active Checkpoint. It's not an importance label.

## Status tags

These classify how much weight content carries. They don't grant authority on their own.

| Tag             | Meaning                                                                                                 |
| --------------- | ------------------------------------------------------------------------------------------------------- |
| `#Contextual`   | Useful context that isn't accepted. Treat it as a candidate unless something with authority accepts it. |
| `#CurrentTruth` | Accepted current state within its stated scope.                                                         |
| `#Evergreen`    | Must be kept aligned with accepted current state. It creates an update duty, not authority or loading.  |

## Part tags

| Tag          | Meaning                                                                       |
| ------------ | ----------------------------------------------------------------------------- |
| `#Core`      | Base routing, loading, workspace orientation, and the reusable content roles. |
| `#Memory`    | Memory states and records.                                                    |
| `#Extension` | Content that arrived in an optional package.                                  |

## Your own tags

Any other tag, such as `#Testing`, `#Frontend`, or `#Decision`, is a search and selection signal. It adds no loading behavior. Use them to make descriptions easier to scan and to find related files:

```sh
open-forge find --tag=Decision
```

A tag starts with a letter and contains letters or digits, with single internal hyphens allowed.

## What loads at startup in a fresh install

| File                                   | Tag           | Loads at startup?             |
| -------------------------------------- | ------------- | ----------------------------- |
| `loader.md`                            |               | Yes, it's the starting point. |
| `directives/_directives.md`            | `#LoadNow`    | Yes                           |
| `guidance/_guidance.md`                | `#LoadNow`    | Yes                           |
| `maps/_maps.md`                        | `#LoadNow`    | Yes                           |
| `memory/_memory.md`                    | `#LoadNow`    | Yes                           |
| `memory/working/_working.md`           | `#LoadNow`    | Yes                           |
| `memory/crystallized/_crystallized.md` | `#LoadNow`    | Yes                           |
| `memory/emerging/_emerging.md`         | `#KeepInMind` | Yes, and refreshed later      |
| `patterns/_patterns.md`                | `#LoadNow`    | Yes                           |
| `skills/_skills.md`                    | `#LoadNow`    | Yes                           |
| `templates/_templates.md`              |               | No, on demand                 |
| `memory/archived/_archived.md`         |               | No, on demand                 |

Run `open-forge context` to see the exact startup context of your own workspace.

Next: [Core categories](core-categories.md).
