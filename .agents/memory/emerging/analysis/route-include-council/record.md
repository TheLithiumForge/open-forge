---
open-forge:
  description: What the route include council decided about reusing project knowledge in place, help concepts, the removable guide and a configurable main root
  responsibility: Record council 4's question, method, rankings, chair decisions, declined ideas and open choices for the maintainer
  tags: [Memory, Analysis, Contextual, Candidate, Framework, CLI, Council, Routing, Include, Settings, Help]
---

# Route Include Council

This is council 4. It follows the [lossless and readable kernel council](../lossless-council/record.md), whose branches are PRs #4 and #5. Nothing here is accepted direction until the maintainer accepts it.

## What the maintainer asked

1. **A stratification of reusable knowledge.** An Extension is reusable knowledge you can share anywhere. A Library is reusable knowledge you share on the same computer. The new thing is reusable knowledge that is already in this project: "please use it."
2. **A command for it, not called link.** Candidates were "route include", "adding an extra route" and "some root-level command that basically says we allow multiple roots". Folding it into `route create` or `route init` was an open question.
3. **Files or entrypoints.** Either an `_{folder}.md` catalogue or individual files.
4. **Tracked in the config,** and used by `index` and the other commands.
5. **A configurable main root,** as its own later PR, asked about during install. Skills can't then be a category under it.
6. **`open-forge help concepts`,** with `.agents/README.md` marked as removable and reduced to a guide.
7. **No shipped authoring Directive.** The maintainer did not know what it would contain or why it was needed.
8. **Encourage people to build their own** Directives and reuse their knowledge.
9. **Docs in step, nothing broken,** and every big change to CLI usage reported.

## Method

Eight read-only Sol 6.1 xhigh members worked from a snapshot of the #5 branch: a CLI product designer, a CLI architect, a contract guardian, a brownfield team lead, a Unix minimalist, a model-behavior realist, a migration and safety engineer and a contrarian. Stage 1 produced eight independent proposals, kept in [proposals](proposals.md). In Stage 2 each member ranked the seven others four times without knowing their authors, kept in [reviews](reviews.md). The chair is the overseeing agent.

## Rankings

Average rank from seven reviewers, lower is better. CONCEPT judged the concept and name, SURFACE the CLI surface and config, GUIDE `help concepts` and the README, ROOT the configurable main root.

| Member | CONCEPT | SURFACE | GUIDE | ROOT |
| --- | --- | --- | --- | --- |
| CLI product designer | **1.14** | 5.43 | 4.43 | 4.14 |
| Unix minimalist | 2.86 | **1.14** | 4.57 | 2.00 |
| Model-behavior realist | 6.00 | 6.29 | **1.00** | 6.57 |
| Migration and safety engineer | 2.43 | 4.00 | 1.86 | **1.00** |
| Contract guardian | 3.71 | 2.71 | 5.14 | 3.00 |
| CLI architect | 4.00 | 3.14 | 5.29 | 4.57 |
| Brownfield team lead | 6.14 | 4.71 | 2.71 | 4.14 |
| Contrarian | 5.71 | 4.57 | 7.00 | 6.57 |

The realist won GUIDE and the migration engineer won ROOT unanimously. All eight reviews recommend the same name.

## Decisions

### The concept and its name

The three ways to reuse knowledge form one family:

| You have | Use | What happens |
| --- | --- | --- |
| A package you can share anywhere | `extension install` | Its files are copied in and kept up to date |
| A folder registered in this workspace | `library attach` | Its files are projected elsewhere through relative links |
| A document already in this project | `route include` | The loader lists it where it lives |

The word users learn is **include**. The commands are `open-forge route include <workspace-file>` and its inverse `open-forge route exclude <workspace-file>`.

It belongs under `route` because an included file becomes a root route, which is the loader's own term for a line it lists directly. That is also the maintainer's "multiple roots" idea: the settings key that records included files is `roots`, and #7 adds `mainRoot` beside it.

Rejected names:

- `link` could mean a Markdown link, a filesystem link or a Library projection. An error such as "cannot link this file" would not say which failed.
- `route add` reads like creating content. Beside `route create`, people would expect the two to be aliases.
- A top-level `root` group asks people to think about a filesystem model when they only want to use one file.
- Folding into `route create` or `route init` breaks their contracts. Create makes one new routed file and Init makes missing catalogues. Include makes neither and only changes what the loader lists.
- Reusing `route remove` for the inverse would be dangerous, because Remove deletes content. Exclude never does.

Libraries today project a contained folder inside the same workspace, not across the computer. #6 does not change that. The guide and docs describe Libraries as they work now.

### `route include` and `route exclude`

```text
open-forge route include <workspace-file>
  [--description <text>]
  [--tag <tag>]...
  [--apply-to <glob>]...
  [--dry-run]

open-forge route exclude <workspace-file>
  [--dry-run]
```

Both apply directly after preflight, like `route create`, and preview with `--dry-run`. They take the workspace mutation lock and use the same mutation and recovery path as the other route commands.

**The operand** is one existing workspace-relative file. The CLI accepts `/` or `\` and a leading `./`, and writes the canonical form. It rejects directories, naming the folder's `_{folder}.md` when one exists. It also rejects globs, URLs, fragments, paths that leave the workspace lexically or through a link, Git metadata, the loader, the settings and lock files, and files that are not UTF-8 text.

**Outside the main root**, any text file can be included. A Markdown file with an `## Entries` section, normally named `_{folder}.md`, is followed as a catalogue, exactly as hand-written loader links are since #4. Every other file is a leaf. The path is recorded in `roots`.

**Inside the main root** (`.agents/`), the target must be the catalogue of a direct child folder, such as `.agents/patterns/_patterns.md`. Including it switches that folder's line back on, and excluding it switches the line off, exactly like editing the loader by hand. Its line is generated from the catalogue's own frontmatter, so the metadata options are rejected with a pointer to that file. These targets are not recorded in `roots`, because a later main-root move would change their paths. Deeper files are rejected with the name of the catalogue that already lists them. Files directly in `.agents/` are rejected, because they are not routed.

**Metadata for a new row.** `--description` is required. `--tag` repeats, keeps the given order and accepts the name with or without `#`. Without a tag the file is opened on demand. `--apply-to` repeats and uses the `applyTo` glob grammar. A file's own frontmatter, from Open Forge or another tool, never sets its loading.

**An existing row** for the same target, hand-written or from an earlier include, keeps every field the request omits. A supplied option replaces only its own field. A repeated identical request is a verified no-op. Including a hand-written row registers it without rewriting it. To clear tags or conditions, edit the line, or exclude and include again. The preview shows the loader line before and after.

**New rows** are appended after the last row in `Entries`, where Index already keeps authored links.

**Exclude** removes every loader row whose destination is the target, normally one, and its `roots` registration. It never touches the target file, works when the target is missing and works on hand-written rows. When nothing lists or registers the target, it reports that nothing changed and exits successfully.

### What the other commands do

| Command | With included files |
| --- | --- |
| `context` | Follows active included rows with the usual tags, conditions and catalogue rules. Selected files outside `.agents/` are named by their exact path, such as `./CONTRIBUTING.md`. |
| `index` | Keeps included rows and their order, and validates their targets. It never writes a file outside the main root, never registers a hand-written row and never restores a row from `roots`. |
| `status` | Reports registered includes as on, off (registered with no loader row) or missing, and counts hand-written rows separately. The startup estimate counts required included files outside `.agents/`, each physical file once. |
| `doctor` | Reports a missing included target as an error, an off registration as information and a registered path that Include would now reject as a warning. |
| `route list` | Lists included rows among the root routes. An off registration appears labelled, without children. |
| `find` | Searches active included leaves and the descendants of included catalogues, by their row metadata and Markdown headings and tags. It never scans a folder because one file in it was included. |
| `references` | Counts the authored links inside active included Markdown files. Loader rows stay navigation and are not counted, as today. Other text formats are opaque. |

**Source references** gain one form. `./<workspace-path>` names an exact file anywhere in the workspace, such as `context ./CONTRIBUTING.md`. `.agents/<path>` and `./.agents/<path>` keep their meaning, and any other value is still a source ID. Files outside `.agents/` have no short ID.

### Settings

#6 publishes `schemas/v3/open-forge.schema.json`, which is v2 plus `roots`. v2 stays unchanged, as the [versioned configuration rule](../../../../directives/software/versioned-configuration-schema.md) requires.

```json
{
  "$schema": "https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v3/open-forge.schema.json",
  "schemaVersion": 3,
  "roots": [
    "CONTRIBUTING.md",
    ".cursor/rules/testing.mdc",
    "docs/team/_team.md"
  ]
}
```

`roots` is a list of unique canonical workspace-relative file paths outside the main root. It holds no descriptions, tags, conditions, IDs or flags. An absent `roots` is empty. A new settings file is written as `schemaVersion` 3 with the v3 `$schema`. An existing file keeps the `$schema` and `schemaVersion` it declares, as the configuration rule requires, and stays valid because every published schema accepts unknown keys. Absent, lower and higher versions are read as today.

**The loader wins.** Its rows decide what loads, in which order, and with which description, tags and conditions. `roots` only records what Include brought in, so Status and Doctor can report it and #7 can rewrite its links when the main root moves. Deleting a row by hand leaves an off registration, and no command restores it. A row without a registration is an active hand-written include. Adding a path to `roots` by hand switches nothing on.

### `help concepts` and the removable guide

**What the authoring rules are.** They are the file formats the framework and the CLI read: how a catalogue is named, which frontmatter fields a routed file carries, how an `Entries` line is written, where `## Axioms` and `## Instructions` go, how an `applyTo` condition is written and how an overwrite companion is named. They matter because a file in the wrong shape is ignored or reported as malformed, by agents following the loader and by the CLI. They say nothing about how to do the work, so they are not a Directive and are never read at startup. They are a reference an agent reads before it writes workspace files.

**Their home is the README's last section, "Writing workspace files".** The council proposed a separate `.agents/authoring.md`. The maintainer chose the README instead on 2026-10-21: "Let's keep them in the readme, since that is more of a helpful file for anyone to read and if they wish to delete, and in it we should anyway say, hey all of this info is also found out in the clis help section anyway." No authoring Directive and no separate authoring file ship.

**`help concepts` prints that same section** from the shipped README, so the two cannot drift, and a project that deletes its README still has the formats in Help.

**The loader** keeps the whole reading procedure, authority, conditions, the off switch and missing-file rules in plain words. Its "Writing context" line sends agents to the file formats in `README.md`, to `open-forge help concepts` when that page was deleted, and otherwise to the shape of a neighbouring file. It names the README by path, not by link, so deleting the guide leaves no broken link. Its "Turn something on" line mentions `open-forge route include`.

**`open-forge help [<topic>]`** is a new top-level command. With no topic it lists the topics and says that `open-forge <command> --help` shows a command's options. The first and only topic is `concepts`. An unknown topic is invalid input that lists the topics, and a topic that names a command suggests its `--help`. `--help` on every command is unchanged.

`help concepts` is built into the CLI, so it works outside a workspace. Its sections:

1. **Start here.** Agents read the workspace loader before work, from the file or through `context`.
2. **What is switched on.** Loader rows, their order, missing files and a valid empty list.
3. **Choosing what to read.** Required reads, on-demand selection, explicit reading instructions and finding things by search.
4. **Conditions.** Glob rules, matching through the whole catalogue chain, and pending entries while working files are unknown.
5. **Rules and acceptance.** Axioms, Instructions, narrower rules adding to broader ones, conflicts, and why reading is not accepting.
6. **Loading tags.** #LoadNow, #KeepInMind and its refresh points, and descriptive tags.
7. **Content kinds.** Directives, Guidance, Maps, Patterns, Templates, Skills and the Memory states.
8. **Reusing knowledge.** Include, Libraries, Extensions and hand-written rows, and what management means.
9. **Customizing safely.** Overwrites, exclusions, switching off, stopping management and deleting.
10. **Keeping navigation current.** What Index writes and what it leaves alone.
11. **Build your own.** Write the rules and structure your project needs and share them when they prove useful.
12. **Writing workspace files.** The file-format section of the shipped README, printed from the bundled copy.

#7 adds a section on the workspace layout.

**The README becomes a guide you may delete.** Its header says so, says that everything in it is also in `open-forge help concepts`, and says how to keep it deleted through updates by listing `.agents/README.md` in `removedFiles`. Its sections:

1. **Use a document you already have,** with a `route include` example and the hand-written line it writes.
2. **Write your first rule,** encouraging the project's own Directives.
3. **Add only what helps,** the content kinds in one line each.
4. **Turn something off.**
5. **Reuse your knowledge elsewhere,** when an include, a Library or an Extension fits.
6. **Find the details,** pointing to the loader, `open-forge help concepts` and the website.
7. **Writing workspace files,** the file formats.

### The configurable main root (#7)

The design follows the migration engineer's proposal, which every reviewer ranked first.

- **`mainRoot` names where the knowledge lives,** not where the CLI keeps its controls. `.agents/open-forge.json` and `.agents/open-forge.lock.json` stay where they are, so a custom main root does not remove `.agents/` entirely. Recovery bundles and the mutation lock stay keyed by the physical workspace.
- **Settings v4** adds `"mainRoot": "docs/forge"`, a contained directory that defaults to `.agents`. It cannot be the workspace root, Git metadata or a control location.
- **One resolved workspace layout** replaces the hard-coded `SourceLogicalPath.AgentsRoot` assumption and is passed to every consumer. Source IDs such as `memory/crystallized` stay stable. Exact paths stay literal and never redirect.
- **Install** takes `--main-root <directory>`, and the wizard asks "Where should Open Forge keep its context files?" with `.agents` as the default. Previews, JSON and unattended runs never ask.
- **Existing workspaces move** through `install --configure --main-root <directory>`, previewed with `--dry-run`. Editing `mainRoot` by hand moves nothing: commands report the mismatch and writing commands stop until a configure plan resolves it.
- **Migration** inventories the real files, including ignored and untracked ones such as private Working notes, overwrites and user files. It blocks collisions, unsafe links and unresolved recovery operations, verifies copied bytes before removing originals, translates Install-owned ignore rules with their sharing records, and keeps authored ignore rules byte for byte. Historical recovery manifests keep their original paths.
- **Skills stay where hosts discover them.** A custom main root cannot make a host find Skills elsewhere, so `.agents/skills/` stays put and the custom loader can include its catalogue. No Skills category is created under the custom root.
- **`AGENTS.md` and `CLAUDE.md`** stay at the workspace root and point to the selected loader.
- **Extensions** map their `.agents` destinations through the layout and record where they actually installed. **Libraries** keep their sources, and only projections inside the moved folder move.
- **Included files** keep their workspace paths in `roots`, and their loader links are rewritten for the new loader location.

Slices, in order:

1. Freeze the layout, migration, ownership and recovery contracts. Add the layout resolver with unchanged default behavior.
2. Move source identity, discovery and loading onto the resolver. Prove stable IDs and unchanged default output.
3. Move generated navigation, privacy filtering and the guide's path.
4. Move route operations, the Framework lifecycle, exclusions and permission checks.
5. Move Extension destination mapping and ownership publication.
6. Move Library projection, relative links, ignore ownership and recovery attribution.
7. Add Install migration and the managed host pointers.
8. Finish help, docs and the published-process journeys on every platform.

After slice 1, workers can own source reading, packages and Libraries separately. One owner keeps the shared layout and ownership contracts. The estimate is 15 to 25 engineering days.

## Declined

- **Metadata in `roots`.** Copying description, tags and conditions into settings creates a second policy that drifts from the loader.
- **Replacing a row's whole policy on a repeated include.** An omitted `--apply-to` would silently widen a conditioned rule to every task.
- **Index maintaining included catalogues outside `.agents/`.** #6 keeps them read-only. Writing navigation into a project's own docs can come later as an explicit opt-in.
- **A separate `authoring.md`, `context --authoring`, `help authoring` and `route list --includes`.** The README and `help concepts` hold the formats, and existing listings cover the rest.
- **Moving the controls or recovery storage in #7.** That adds settings discovery and control-store migration that a content move doesn't need.
- **A separate `relocate` command.** Configure already owns reviewed layout changes.

## Big changes to CLI usage

#6:

- New `route include` and `route exclude`.
- New `open-forge help` with the `concepts` topic.
- `./<workspace-path>` source references for files outside `.agents/`.
- `context`, `find`, `route list` and `status` show included files outside `.agents/`, and startup estimates can grow.
- The settings file is written as schema v3.
- The README becomes optional and holds the file formats that `help concepts` also prints.

#7:

- `install --main-root` and its wizard question, and `install --configure --main-root` to move an existing workspace.
- Settings v4 with `mainRoot`.
- With a custom main root, `.agents/` still holds the settings, the lock and native Skills.
- An older CLI ignores `mainRoot`, so a custom-root workspace needs the new CLI.

## Documents to update

#6:

- Payload and dogfood: `loader.md` under `src/open-forge/.agents/` and `.agents/`, and the payload `README.md`.
- `schemas/v3/open-forge.schema.json`.
- `docs/cli.md`, root `README.md`, `docs/extensions.md`.
- Docs site: `concepts/index.md`, `routing.md`, `loading-and-tags.md`, `customizing.md`, `getting-started/installation.md`, `grow-your-framework.md`, `greenfield-and-brownfield.md`, `cli/index.md`, `cli/flows.md` and `glossary.md`.
- The CLI Skill `open-forge-cli/SKILL.md` and its `common.md`, `discovery.md`, `routes.md` and `packages.md` references, in the payload and the dogfood copy.
- Contracts: `command-contract-set.md`, new Include, Exclude and Help contract sets, the route catalogue, the shared source-reference contract, and the Route List, Context, Find, References, Index, Status and Doctor contracts.

#7 adds `AGENTS.md` and `CLAUDE.md` in the payload and the repository, the lock schema if its shape changes, `architecture.md`, `technical-designs/mutation-and-recovery.md`, `workspace-libraries.md`, a new `workspace-layout.md`, the Install, Update, Remove, Repair, Cleanup, Route, Extension and Library contracts, the source-reference and workspace-permission contracts and the payload maintenance contracts.

## Required evidence

- **Three reading paths agree.** One fixture with an included foreign file, a nested conditional included catalogue, an off registration, a hand-written row and an overwrite. A loader-only reader, a `context`-only reader and a `help concepts`-led reader must reach the same ordered reads. The same fixture with the README deleted must still let each path write a valid Directive and catalogue.
- **No silent reactivation.** Include a file, delete its row by hand, then run Index, Update, Configure and an Extension install. It stays off.
- **Included files never change.** Hash every included file through Include, Index, Update and Exclude.

## Open choices for the maintainer

- **Fixed `.agents/` controls in #7.** With a custom main root, `.agents/` still holds `open-forge.json`, the lock file and native Skills. Moving the controls too would need settings discovery and control-store migration, so the council recommends against it for now.
- **When to build #7.** The contrarian argued it should wait until after the first stable release because of its cost. The chair recommends building #6 first and starting #7 as its own PR afterward, as the maintainer asked.
