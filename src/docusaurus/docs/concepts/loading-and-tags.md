---
title: Loading and tags
description: How loading tags and file conditions work together, and how to choose between on demand, LoadNow, and KeepInMind.
---

# Loading and tags

Tags help readers, human or agent, decide what to open and find related files. The loader defines eight tags: two control loading, and six classify content. None of them grants authority. Any other tag is yours to describe your own subjects.

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
- **applyTo** (optional) lists the workspace file patterns that make the source applicable to task files. It can appear at the frontmatter root or under `open-forge:`. Both locations mean the same thing. Use a quoted string for one pattern or a list of quoted strings.

## Loading tags

Without a loading tag or an effective file condition, an entry stays **on demand**. There are exactly two loading tags:

| Tag           | Behavior                                                                                                                                                               |
| ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `#LoadNow`    | Read the file when its already-loaded parent exposes it, in listed order, subject to any file condition. If it's an entrypoint, apply its own children's loading rules too. |
| `#KeepInMind` | Read it when its parent loads, subject to any file condition, then refresh it at task start or resume, after context restoration, and before handoff or closeout while its scope remains active. |

Loading tags and matching `applyTo` conditions act only through entries exposed by a parent that's already loaded. Neither can pull in a scope nobody selected.

### Choosing a loading behavior

- **On demand** (no loading tag and no effective file condition) is the default. Use it for content that should open only when the task selects it by relevance.
- **`#LoadNow`** is for content whose omission would cost more than reading it whenever it applies. Every Directive file in a Directives folder must carry it. An unconditioned Directive loads with its parent; check an `applyTo` condition before loading a conditioned Directive.
- **`#KeepInMind`** is for continuity that must be re-read at the refresh points. The Emerging Memory entrypoint uses it, so the agent is prompted to review unsettled findings before they're lost. It's not an importance label. Like `#LoadNow`, it acts only through a loaded parent: an active Checkpoint from the [Planning](../extensions/planning.md) Extension is tagged `#KeepInMind`, but it's refreshed only once its Checkpoints category has been opened.

## File conditions

A routed source can use the optional `applyTo` field to limit when its context applies to files in a task. For example, this condition applies the Directive to C# files:

```yaml
open-forge:
  description: C# rules
  tags: [Directive, LoadNow]
  applyTo: ["**/*.cs"]
```

Patterns in one field are alternatives. Conditions on a source and its selected route ancestors combine: the same working file must match every condition in that chain. The source applies when at least one working file matches the whole chain. A child with no local condition inherits its ancestors' conditions. Patterns use workspace-relative paths with `/` separators. `*` and `?` match within one path segment. `**` must be a whole segment and matches zero or more segments. Matching is case-sensitive on every platform.

Check conditions on entries exposed by loaded parents before loading them. A visible matching entry opens before work on its file, even if it has no loading tag. A visible nonmatching entry does not load automatically. An unconditioned entry keeps its existing behavior: `#LoadNow` and `#KeepInMind` load it through its parent, and an untagged entry stays on demand. On a conditioned entry, `#LoadNow` remains valid but adds no extra first-read behavior after a match. `#KeepInMind` still adds its usual refreshes while a matching path remains in the task. An unconditioned `#KeepInMind` source keeps its existing refresh behavior when no file paths are supplied.

A file condition never reveals a hidden ancestor. If `directives/backend/_backend.md` has not been selected, a `csharp.md` child there stays hidden even when `src/Order.cs` matches its pattern. Select the `backend` scope through its entry or an explicit source request before expecting the child to load. The [routing guide](routing.md) explains how entrypoints expose their children.

A task can include planned paths that do not exist yet. When the task needs a related file outside the first set of paths, add that file and load the context that applies to it. For example, a C# change may also require updating a TypeScript caller. Include both paths, and keep each rule limited to its matching files. Reading a Directive or following a link does not add that Markdown source's path to the task's working files. If you edit the context source itself, add its Markdown path as a working path. A condition does not grant or restrict permission to edit.

When task paths are unknown, an unconditioned source keeps its existing loading behavior. Treat encountered conditions as pending, not as matches or mismatches, and do not claim that context is complete for an unspecified file set. Planning and research can still select sources by ordinary relevance. An explicit source request or reference can also retrieve a nonmatching source with its necessary ancestor context for inspection; that inspection does not activate its automatic children. An overwrite companion stays with its base and shares the base's effective condition.

## What loads at startup in a fresh install

An agent enters through `AGENTS.md` (or `CLAUDE.md` in Claude Code), which points it to the loader. Both are read in full. Everything below them is reached through entrypoints, and the table uses two terms for them. They describe the instructions, not a record of what an agent actually read:

- **Entrypoint at startup:** the entrypoint is read before the task begins. Its entries open on demand unless a loading tag or matching file condition applies.
- **Entrypoint on demand:** the entrypoint isn't read until a task opens it. A file condition below it does not open the entrypoint by itself.

Paths are relative to `.agents/`.

| File                                   | Tag           | When it's read                                    |
| -------------------------------------- | ------------- | ------------------------------------------------- |
| `loader.md`                            | none          | In full, at startup                               |
| `directives/_directives.md`            | `#LoadNow`    | Entrypoint at startup                             |
| `guidance/_guidance.md`                | `#LoadNow`    | Entrypoint at startup                             |
| `maps/_maps.md`                        | `#LoadNow`    | Entrypoint at startup                             |
| `memory/_memory.md`                    | `#LoadNow`    | Entrypoint at startup                             |
| `memory/working/_working.md`           | `#LoadNow`    | Entrypoint at startup                             |
| `memory/crystallized/_crystallized.md` | `#LoadNow`    | Entrypoint at startup                             |
| `memory/emerging/_emerging.md`         | `#KeepInMind` | Entrypoint at startup, and at every refresh point |
| `patterns/_patterns.md`                | `#LoadNow`    | Entrypoint at startup                             |
| `skills/_skills.md`                    | `#LoadNow`    | Entrypoint at startup                             |
| `skills/open-forge-cli/SKILL.md`       | none          | On demand                                         |
| `templates/_templates.md`              | none          | Entrypoint on demand                              |
| `memory/archived/_archived.md`         | none          | Entrypoint on demand                              |

Loading an entrypoint doesn't load what it lists. The agent reads the entrypoint itself: its purpose, its rules, and one line per item under `Entries`. Its children stay on demand unless a loading tag applies or a matching file condition makes a visible entry applicable. For example, at startup the agent sees the one-line entry for `open-forge-cli` in `skills/_skills.md`, but it doesn't read the Skill.

The Memory rows show that exception. `memory/_memory.md` lists Working and Crystallized with `#LoadNow` and Emerging with `#KeepInMind`, so those three entrypoints load too. Archived has no loading tag, so it stays closed. Directive files directly under the root Directives entrypoint must carry `#LoadNow`. An unconditioned Directive loads at startup; if it declares `applyTo`, the condition first limits which working files make it applicable. A fresh install has no Directive files.

Extension Memory categories, such as Decisions from [Planning](../extensions/planning.md) or Documents from [Project Documents](../extensions/project-documents.md), aren't part of a fresh install and carry no loading tag. Once installed, only their one-line entry in the parent state's `Entries` is visible at startup. By default, the category entrypoint and its records open on demand. A matching file condition can also load a visible entry.

Run `open-forge context` to see the startup context of your workspace. When task paths are unknown, the output reports encountered file conditions as pending. Pass known or planned working paths with `--for` to check the matching entries.

## Status tags

These classify whether content is accepted and whether it must stay current. They don't grant authority on their own.

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

These tags classify content. They don't decide which files the CLI manages.

## Your own tags

Any other tag, such as `#Testing`, `#Frontend`, or `#Decision`, is a search and selection signal. It adds no loading behavior. Use them to make descriptions easier to scan and to find related files:

```sh
open-forge find --tag=Decision
```

A tag starts with a letter and contains letters or digits, with single internal hyphens allowed.

## Tags and the CLI

Tags are plain text, so the files work without the CLI. The CLI reads them too:

- `open-forge context` lists startup and selected-route context by following loaded routes, loading tags, and matching `applyTo` conditions. Pass each known or planned working path with `--for`.
- `open-forge route inspect` explains how one file loads, and why.
- `open-forge status` reports how many tokens load at startup, and how many may load again at refresh points.
- `open-forge route create --template` copies only from a file tagged `#Template`.
- `open-forge find --tag` searches by any tag, including your own.

The other defined tags, such as `#Contextual`, `#CurrentTruth`, and `#Evergreen`, have no CLI behavior beyond search. They tell the agent how to treat a file.

Next: [Core categories](core-categories.md).
