---
open-forge:
  description: Where every rule of the old loader went in the opt-in kernel, what was dropped and why, and what is new
  responsibility: Trace each statement of the shipped loader at e9d4fc9a7 to its new location, status and startup visibility
  tags: [Memory, Analysis, Contextual, Candidate, Framework, Loader, ChangeMap]
---

# Loader Change Map

This maps the shipped loader at `e9d4fc9a7` (`src/open-forge/.agents/loader.md`, 2,052 words) to the opt-in kernel on branch `council/opt-in-framework` (823 words, including its Entries). Line numbers are the old file's own line numbers, as `git show e9d4fc9a7:src/open-forge/.agents/loader.md` prints them.

## The shape, before and after

| Before | After |
| ------ | ----- |
| Terms: entrypoint, entry, axiom | No glossary. "Catalogue" is used and defined where it is first needed |
| Authority And Inheritance: working with the user, sources and acceptance, how rules combine | **Use what you read**: three bullets |
| Routing: terms, rules, management and customization, overwrites | **Read before you work**: five numbered steps |
| Tags And Loading: eight defined tags, refresh rules, choosing loading tags | Steps 3 and 5 plus one bullet. Only #LoadNow and #KeepInMind have meaning |
| Frontmatter | One bullet |
| CLI: nine commands | Step 2: one command, with a fallback |
| Entries: generated root list | **Change what is on**: the Entries are the on and off switch, and may link to files anywhere in the workspace |

The new loader reads as a procedure followed by two short reference sections. An agent meets every rule at the step where it acts on it.

## Where each rule went

Legend: **Kept** means the same behavior in the new loader. **Moved** means it now lives in another file. **CLI** means the CLI handles it, and the plain files still work without it. **Dropped** means it is not in the shipped Core any more.

| Old | Rule, shortened | Status | Where it is now | Read at startup |
| --- | --- | --- | --- | --- |
| 3 | Read this after AGENTS.md | Kept | Intro and AGENTS.md block | Yes |
| 7 | `entrypoint` = `_{folder}.md`, plus compatibility names | Kept, shorter | "Change what is on": "A folder's catalogue is named `_{folder-name}.md`". Compatibility names stay in the CLI routes reference | Yes |
| 8 | `entry` = one generated route line | Kept, implicit | "Change what is on", first bullet: a line carries description, tags and conditions | Yes |
| 9 | `axiom` = required rule, inherited by descendants | Kept | "Use what you read": Axioms of a catalogue cover everything it lists | Yes |
| 17 | Respect platform constraints and runtime safety | Kept | "Use what you read": within platform and safety limits | Yes |
| 18 | Follow clear user direction, do not reconfirm | Kept | "Use what you read": take routine steps without asking again | Yes |
| 19 | A request allows routine reversible in-scope choices | Kept | Same bullet | Yes |
| 20 | Ask before choices that significantly change result, scope, risk, cost | Kept | Same bullet. Council 2 had cut it. The chair restored it because it is an authority rule, not a style rule | Yes |
| 21 | Begin with understanding, one recommendation, one open choice | **Dropped** | Nowhere in Core. A communication preference that belongs in an optional pack, such as the Collaboration Extension | No |
| 22 | Use the fewest words that preserve meaning | **Dropped** | Nowhere in Core. This repository keeps it in its own writing Directive | No |
| 26 | Use declared external authority for its facts | Kept, merged | "Use what you read": "a source the user named says so" | Yes |
| 27 | Each source answers one question, link instead of repeating | **Dropped** from the kernel | Crystallized Memory keeps the core of it: update the record instead of adding a competing one | Only with Memory |
| 28 | Use relative links and consistent tags | **Dropped** | Ordinary authoring practice that capable models follow without being told | No |
| 29 | Put detail in the narrowest source | Moved | Directives catalogue: "subfolders narrow them to an area" | With Directives |
| 30 | Axioms and Directives govern behavior, documents define knowledge | Kept | "Use what you read", first and third bullets | Yes |
| 31 | Validation versus acceptance, moving a record is not acceptance | Kept | "Use what you read": reading, linking, saving or tagging does not make content accepted | Yes |
| 32 | Investigate a CurrentTruth conflict before changing either source | Partly kept | Crystallized Memory: update the record instead of competing. The explicit "investigate first" sentence was dropped | Only with Memory |
| 36 | Only loaded entrypoints define Axioms, children inherit | Kept | "Use what you read", first bullet | Yes |
| 37 | Inherited-axioms marker convention | **Dropped** | Still valid if present. Route scaffolding still writes it | No |
| 38 | Child rules add, never cancel. Report conflicts | Kept | "Use what you read", first bullet | Yes |
| 39 | Customize by editing, replacing or an overwrite companion | Kept | "Change what is on", last bullet | Yes |
| 40 | Non-binding material may specialize broader material | **Dropped** | Rarely matters, and Guidance still says to adapt advice that fits | No |
| 46 | `route` | **Dropped** term | Behavior lives in the steps | No |
| 47 | `root route` | **Dropped** term | The loader's Entries are simply the starting list | No |
| 48 | `scope` | **Dropped** term | "Narrower rules add to broader ones" and catalogue conditions carry the meaning | No |
| 49 | `slug` | **Dropped** term | Ordinary folder names | No |
| 50 | `managed route` | Moved | CLI Skill references (install, update, remove) | No |
| 54 | Entries list direct routes, select by description, parent before children | Kept | Steps 3 and 5 | Yes |
| 55 | Follow branches recursively, one entrypoint per folder | Kept | Step 3 and the catalogue naming bullet | Yes |
| 56 | A root route exists only where the loader exposes it | **Dropped** | No longer true: Entries may link to files anywhere in the workspace | No |
| 57 | Slugs may narrow a route anywhere | **Dropped** | Ordinary folders | No |
| 58 | A scope contains only useful routes | **Dropped** | Obvious once Entries are the switch | No |
| 59 | Narrowest useful scope, workspace-wide only when it applies everywhere | Moved | Directives catalogue | With Directives |
| 60 | Keep scopes as separate chains, recheck after a task change | Kept, simpler | "Start again from step 1 when your working files or goal change" | Yes |
| 61 | Search hit: load its ancestor chain first | Kept | Step 5, now limited to listed catalogues | Yes |
| 62 | Do not open bodies just to discover routes | **Dropped** | Descriptions on Entries lines make it unnecessary | No |
| 66 | Management decides who may change files, not meaning | Moved | CLI Skill references | No |
| 67 | Manager route shapes and segment order | Moved | CLI Skill references | No |
| 68 | Users may add, move, replace, remove routes | Kept | "Change what is on": add a line, remove a line | Yes |
| 69 | Removed defaults stay removed, record exclusions | Kept, stronger | "Nothing should bring it back unless the user asks", and `index` now keeps removals (see below). Inside other catalogues, lines follow their folder, so a removal there means deleting or moving the file | Yes |
| 70 | Generated Entries are navigation, not authority | Kept | Acceptance bullet | Yes |
| 74 | Overwrite loads right after its base, same scope | Kept | "Change what is on", last bullet | Yes |
| 75 | Overwrite wins only for corresponding content | Kept | Same bullet: "for that file only" | Yes |
| 79 | Defined tags keep their meanings, others are search signals | Kept | "Every other tag is a search label" | Yes |
| 81 | On demand unless #LoadNow or #KeepInMind, through loaded parents | Kept | Steps 3 and 5, and "a required file inside a catalogue needs #LoadNow on that catalogue too" | Yes |
| 83 | Never skip a required file for its description, once per pass | Kept | Step 3 | Yes |
| 85 | C# and Windows scope example | **Dropped** | The demo workspace and README show linked examples instead | No |
| 89 | #LoadNow definition | Kept | Step 3 and the tagging bullet | Yes |
| 90–94 | #KeepInMind timing, order, boundaries, recheck | Kept, simpler | Read at start (step 3) and reread before handoff or finish. "Recheck when changed" and "refresh only active scopes" were dropped | Yes |
| 95 | #Core | **Dropped** definition | Still used as a label | No |
| 96 | #Memory | **Dropped** definition | Memory's own catalogue says what it is | No |
| 97 | #Extension | **Dropped** definition | Still used as a label | No |
| 98 | #Contextual | Kept as behavior | Acceptance bullet. The tag stays as a label | Yes |
| 99 | #CurrentTruth | Kept as behavior | Acceptance bullet and Crystallized Memory | Yes |
| 100–102 | #Evergreen update duty and blocked updates | Partly kept | Crystallized Memory: update the record when accepted direction changes. "Report blocked updates" was dropped | Only with Memory |
| 106 | Keep optional content on demand | Kept | Step 5 | Yes |
| 107 | #LoadNow only when missing it costs more than reading it | Moved | Directives catalogue: tag rule files #LoadNow, leave task-dependent areas untagged | With Directives |
| 108 | Narrow the scope before making content mandatory | Moved | Directives catalogue: limit a subfolder with `applyTo` | With Directives |
| 109 | #KeepInMind is not an importance label | Kept | "Change what is on": "#KeepInMind when it must also be reread before a handoff" says what it is for | Yes |
| 113 | Frontmatter at the root or under `open-forge:`, per `open-forge.json` | Kept, shorter | "in the same form as the files around them". The setting is still read by the CLI | Yes |
| 115 | `description` | Kept | Frontmatter bullet | Yes |
| 116 | `responsibility` | Moved | CLI routes reference. Still valid metadata | No |
| 117 | `tags` | Kept | Frontmatter bullet | Yes |
| 118 | `applyTo` lists, root and scoped both read, duplicates | Moved | CLI routes reference | No |
| 119 | One working file must match the whole condition chain | Kept | Step 4 | Yes |
| 120 | Case-sensitive globs, examples | Kept | Step 4 | Yes |
| 121 | Working files, context reads do not count, no edit permission | Kept | Step 1, and "gives no permission to edit it" | Yes |
| 122 | Unknown working files leave conditions pending | Kept | Step 4 | Yes |
| 126 | The plain files are complete without the CLI | Kept | Intro and step 2's fallback | Yes |
| 130–138 | Nine CLI commands | Moved | Step 2 names `context`. The Skills catalogue says to read the CLI Skill before any other command | Yes, `context` only |

## Deliberately dropped, with no new home in Core

These are the cuts a reviewer should look at. Each can come back as one line, or as an optional pack.

1. **Response recipe** (old line 21). Communication style is the user's or the host's.
2. **Concision rule** (22). A writing preference.
3. **One question per source and link instead of repeating** (27). The strongest candidate to restore, because it is a core Open Forge principle. Today it only survives inside Crystallized Memory.
4. **Investigate conflicting accepted sources first** (32) and **report blocked updates** (100–102). Survive only when Crystallized Memory is installed.
5. **#KeepInMind recheck-when-changed and active-scope boundaries** (93–94). Simplified to start plus before handoff.
6. **Defined meanings of #Core, #Memory, #Extension** (95–97). They remain labels.

## New in the kernel

- The Entries list is the switch. Listed is on, unlisted is off, and removing a line keeps the files as notes.
- Entries may link straight to any Markdown file in the workspace. Linked files need no frontmatter, because the entry line carries the description, tags and `applies to`.
- A missing listed file is an error. An empty list is valid.
- Turning something off does not cancel accepted requirements or the host's own instructions.
- The CLI step covers steps 3 and 4 only, keeps step 5 with the agent, and falls back to file tools.
- People get `.agents/README.md`, which agents never read at startup.

## CLI behavior now on this branch

| Command | Before | Now |
| ------- | ------ | --- |
| `context` | Reads only `.agents/`. A loader link with `..` makes the loader malformed | Follows Entries links to files outside `.agents/`, including linked catalogues such as `docs/_docs.md`, applies the line's tags and `applies to`, reports missing required links, and does not ask linked documents for frontmatter |
| `index` | Rewrites the loader's Entries from every root folder on disk, so removed lines come back and outside links are deleted | Keeps the roots you listed, refreshes their descriptions, keeps authored `..` links in place, and counts them |
| `status`, `doctor`, route readers | Would report the loader as needing an update, every outside link as an entry with no file, and every unlisted category folder as unreachable | Use the same membership rule. Authored links are not stale navigation. An unlisted category folder is turned off, not unreachable. `.agents/README.md` is outside routing |
| Install, Configure, Extension install | Add the roots they install | Unchanged |

Known limits: `status` counts only files inside `.agents/` in its startup figure. There is no test yet that pins the new `context` and `index` behavior, only the demo and the updated existing suites. A missing required link shows the general "Startup context is unavailable" title, with the missing file named in the explanation. If every root line is removed but the folders stay, `index` refills the list, because an empty list is treated as a fresh workspace.

## Try it

The demo workspace is at `artifacts/demo/` on this branch (Git-ignored). It links `CONTRIBUTING.md`, `docs/contributing/csharp.md` and `docs/_docs.md`, and has Guidance, Maps, Patterns and Templates turned off while their folders stay on disk.

```sh
open-forge-dev context                          # C# rules and the storage ADR stay pending
open-forge-dev context --for src/Orders/Order.cs # both load
open-forge-dev context --for web/app.ts          # neither loads
open-forge-dev index --dry-run                   # nothing to do, removals stay removed
```

The executable is `artifacts/publish/open-forge-dev/Debug/open-forge-dev.exe` after a Debug build of `src/cli/root/OpenForge.Cli`.
