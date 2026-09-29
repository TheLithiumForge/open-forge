---
title: Loading and tags
description: What the defined tags mean, which ones control loading, how applyTo narrows loading to matching files, and how to choose between on demand, LoadNow, and KeepInMind.
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
- **applyTo** (optional) narrows the file to tasks that work on matching files. [File conditions](#file-conditions) explains how.

## Loading tags

Entries stay **on demand** unless a loading tag says otherwise. There are exactly two loading tags:

| Tag           | Behavior                                                                                                                                                               |
| ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `#LoadNow`    | Read the file when its already-loaded parent exposes it, in listed order. If it's an entrypoint, apply its own children's loading rules too.                           |
| `#KeepInMind` | Read it when its parent loads, then refresh it at task start or resume, after context restoration, and before handoff or closeout, for as long as its scope is active. |

Loading tags act only through a parent that's already loaded. They can't pull in a scope nobody selected.

### Choosing a loading behavior

- **On demand** (no tag) is the default. Use it for anything optional.
- **`#LoadNow`** is for content needed every time its parent loads, where missing it would cost more than reading it each time. Every Directive file in a Directives folder must carry it, because Directives are mandatory within their scope.
- **`#KeepInMind`** is for continuity that must be re-read at the refresh points. The Emerging Memory entrypoint uses it, so the agent is prompted to review unsettled findings before they're lost. It's not an importance label. Like `#LoadNow`, it acts only through a loaded parent: an active Checkpoint from the [Planning](../extensions/planning.md) Extension is tagged `#KeepInMind`, but it's refreshed only once its Checkpoints category has been opened.

## File conditions

A routed file can list `applyTo` patterns to narrow it to the files a task works on. For example, this Directive applies only to C# files:

```yaml
open-forge:
  description: C# rules
  tags: [Directive, LoadNow]
  applyTo: ["**/*.cs"]
```

`applyTo` only filters what a loading tag or relevance would load. It never selects or loads a file by itself. When the task works on a matching file, the source behaves as it would without a condition. A C# rule that should load automatically therefore uses `#LoadNow` with `applyTo`. The entry shows the patterns after its tags, so the agent can check them without opening the file. An overwrite companion follows its base.

- **Working files** are the files the task investigates, creates, changes, deletes, renames, or reviews, including planned files that don't exist yet. A rename counts both paths. Reading a Directive or following a link doesn't make that Markdown file a working file.
- **Related files join as the work reaches them.** If a C# change also needs its TypeScript caller updated, add the caller and load its context. The C# rules still cover only the C# file. A condition never grants or restricts permission to edit.
- **Ancestors combine.** Patterns in one list are alternatives. Conditions on parent entrypoints also apply, and the same working file must match all of them.
- **Hidden scopes stay hidden.** A matching file never opens an ancestor nobody selected. If `directives/backend/` hasn't been selected, a `csharp.md` inside it stays closed even for `src/Order.cs`.
- **Unknown paths are pending.** Until the task's files are known, only conditioned entries that a loading tag would otherwise load are reported as pending, not as matches or mismatches. Untagged conditioned entries remain on demand. Planning and research can still select files by relevance.
- **Inspection isn't application.** Asking to read a nonmatching file, or following a link to it, shows its content without making it apply.

Patterns use workspace-relative paths with `/` separators. A quoted string can contain comma-separated patterns, while each list entry is one pattern. Braces expand alternatives, as in `**/*.{ts,tsx}`, and character classes such as `[ab]` match one character from a set. A leading `!` is an ordinary character, as in other glob tools. `[!...]` negates a character class. `*` and `?` match within one path segment, and a whole `**` segment matches zero or more segments. Matching is case-sensitive on every platform. `*.py` matches only the workspace root, while `**/*.py` matches at any depth. `applyTo` can sit at the frontmatter root or under `open-forge:`, and both mean the same thing.

## What loads at startup in a fresh install

An agent enters through `AGENTS.md` (or `CLAUDE.md` in Claude Code), which points it to the loader. Both are read in full. Everything below them is reached through entrypoints, and the table uses two terms for them. They describe the instructions, not a record of what an agent actually read:

- **Entrypoint at startup:** the entrypoint is read before the task begins. The entries it lists open on demand, unless their line is tagged.
- **Entrypoint on demand:** the entrypoint isn't read until a task opens it.

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

Loading an entrypoint doesn't load what it lists. The agent reads the entrypoint itself: its purpose, its rules, and one line per item under `Entries`. The files and folders those lines point to stay closed until a task selects them, unless an entry is tagged `#LoadNow` or `#KeepInMind`. For example, at startup the agent sees the one-line entry for `open-forge-cli` in `skills/_skills.md`, but it doesn't read the Skill.

The Memory rows show that exception. `memory/_memory.md` lists Working and Crystallized with `#LoadNow` and Emerging with `#KeepInMind`, so those three entrypoints load too. Archived has no loading tag, so it stays closed. Directives work the same way: each Directive file directly in `directives/` must carry `#LoadNow`, so it loads at startup once you add one. A fresh install has none.

Extension Memory categories, such as Decisions from [Planning](../extensions/planning.md) or Documents from [Project Documents](../extensions/project-documents.md), aren't part of a fresh install and carry no loading tag. Once installed, only their one-line entry in the parent state's `Entries` is visible at startup. The category entrypoint and its records open on demand.

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
