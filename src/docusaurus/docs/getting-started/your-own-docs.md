---
title: Your own docs
description: Keep the documents your team writes in your own folders, let agents read them through the loader, and choose which ones are accepted sources.
---

# Your own docs

Open Forge reads the documents your team writes where they already live, so you don't have to move them into the context folder.

Agent context lives in the main root, `.agents` by default. Your documents can stay in folders such as `docs/team`, written by hand or with AI help. E.g. your team keeps its testing rules in `docs/team/testing.md`. You register `docs/team` as a route, the loader lists its catalogue, and a testing task reads the rules in place.

The loader asks agents to treat content as accepted only when you agree, or when a source you named for that question says so. Linking, tagging or reading a document doesn't count, and it doesn't give an agent permission to edit it. These are instructions the agent follows, so review its work as usual. [Mark accepted sources](#mark-accepted-sources) shows how to name them.

## Two kinds of content

A workspace keeps two kinds of content apart:

- The main root - the context folder. It holds the loader, the Framework's rules and the agents' Memory.
- Outside routes - folders you register, such as `docs/team`. They hold your team's own documents.

```text
AGENTS.md
.agents/           <- main root: loader, rules, Memory
docs/
  team/            <- outside route: your own documents
    _team.md       <- its catalogue
    testing.md
```

You get documents into agent context in one of two ways:

| You want                                                     | Use            | What the CLI does afterwards                       |
| ------------------------------------------------------------ | -------------- | -------------------------------------------------- |
| One document to stay read-only input                         | File Include   | Lists it in the loader and reports it if it breaks |
| Open Forge to maintain a safe existing folder where it lives | Folder Include | Maintains the folder's catalogue and files         |

Folder registration is separate from the [main-root choice](installation.md#choose-the-context-folder). It doesn't move files or grant package ownership. Loader rows still decide loading.

## Include a file

The loader can list any text file in the workspace, including files outside the primary content locations. Run `route include` with the file:

```sh
open-forge route include CONTRIBUTING.md --description "Contribution rules to follow" --tag LoadNow
```

That writes one line under the loader's `Entries`, which you can also write by hand:

```md
- [Contribution rules to follow](../CONTRIBUTING.md) - #LoadNow
```

The included file follows these rules:

- The line carries the description, tags and conditions, so the file needs no frontmatter. Another tool's frontmatter, such as a Cursor rule's `globs`, never decides when it loads.
- A Markdown file with its own `## Entries`, such as `docs/team/_team.md`, is followed as a catalogue.
- File inclusion alone never makes the target writable. Retained authored lines keep their policy and mutual order.
- `route include` records the files it adds under `roots` in `.agents/open-forge.json`, so `open-forge status` and `open-forge doctor` can report one whose line was removed or whose file went missing. The loader line still decides whether and how each file loads.
- `open-forge route exclude CONTRIBUTING.md` removes the line and leaves the file alone. Deleting the line by hand has the same effect on what agents read.
- Commands name these files by their path, such as `open-forge context ./CONTRIBUTING.md`.

## Include a folder

To maintain a whole folder, include the folder instead. At first installation, the install wizard can do this for one folder: it asks for a docs folder right after the context folder. For an existing installation, or another folder, preview the plan, then apply it:

```sh
open-forge route include docs/team --dry-run
open-forge route include docs/team
```

This registers a safe existing directory in settings v5 `outsideRoutes` and lists its catalogue, `docs/team/_team.md`. The command creates the catalogue only when it's missing, and keeps an existing one byte for byte. The separate `roots` list stays files only.

Registration gives discovery and bounded maintenance. It doesn't decide loading, and it grants no package ownership or permission. The Loader row still decides whether and when the branch loads. If you delete that row, the registration stays, unlisted, and scanning it never activates its content. An omitted Loader row stays omitted during explicit maintenance. `open-forge route exclude docs/team` removes the registration and its matching rows without deleting content.

[Maintain a folder where it lives](../cli/flows.md#maintain-a-folder-where-it-lives) walks through registration, explicit Index and unlisted maintenance step by step.

### Work inside the folder

After registration, the CLI works inside the folder the same way it works in the main root. Use exact source paths with the filename:

```sh
open-forge route create ./docs/team/testing.md --description "Team testing rules" --tag Testing --dry-run
open-forge index ./docs/team/_team.md --dry-run
```

Route create, init, update, move and remove, Index and Repair all work within the registration. A few rules keep that work bounded:

- Move stays inside one registration. The registered directory and its root entrypoints stay protected.
- A required rewrite of foreign content blocks the complete Move or Remove plan.
- Existing recognized compatibility anchors can stay in a registered tree. Source references keep the filename, such as `./docs/team/index.md`, and get no short IDs.
- Folder names such as `memory` or `skills` don't get the standard category defaults. An outside `SKILL.md` stays ordinary Markdown.
- The folder can hold scoped context. Loaded entrypoints, authored rows and conditions still decide what applies, as [Scopes](../concepts/scopes.md) describes.

### Read an unlisted folder

`route list ./docs/team/_team.md --depth all` or `route inspect ./docs/team/_team.md` selects a registered catalogue even when it has no Loader row. Bare `route list` keeps its Loader-root selection and doesn't add unlisted directories as roots.

Applicable List rows and Inspect source data include optional `maintenanceRoot` at every JSON detail. It names the workspace-relative registered directory, such as `docs/team`, without `./` or a trailing slash. Standard, full and debug text can show `Maintenance root`. The field is left out when it doesn't apply, and it implies no loading, direct Loader listing, ownership or package permission. Exact source references still keep `./` and the filename.

### Status, Doctor and updates

`status` reports each registered directory's availability and whether the loader lists it directly. `doctor` diagnoses the folder's structure without granting restoration authority, and gives only grounded maintenance advice.

Ordinary Install, configure, frontmatter conversion and Update keep outside registrations and the exact unlisted state:

- Authored outside rows keep their bytes, conditions and mutual order, including stale destinations.
- When a target is both inventoried and included, authored rows keep their tags, conditions, aliases, order and first-reaching metadata.
- Generated primary rows keep their own order.
- These operations don't relist or restore outside anchors.

Directory registrations need settings v5 support. An older CLI doesn't maintain outside routes, and keeping an unknown property doesn't mean a downgrade is safe.

[Outside routes](/guides/cli#outside-routes) in the CLI reference covers exact paths, unlisted maintenance and the safety limits.

## Mark accepted sources

If you keep rules and facts in your own `docs/` folder instead of in `.agents`, include them like any other document. Then say in `AGENTS.md`, outside the marked section, which of them are accepted sources.

You can also point agents at docs you don't want listed in the loader, such as a wiki, ADRs or API specs. A [Map](../concepts/core-categories.md#maps) says where each source lives and when to read it, and the sources keep their own authority.

## Next

[Share with your team](share-with-your-team.md) covers sharing a folder across repos through a Library, and sharing packages across teams through Extensions.
