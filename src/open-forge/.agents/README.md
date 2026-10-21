# Open Forge in this project

This page is for the people who run this project. Agents read it only when they write workspace files.

Open Forge tells agents where your rules and knowledge live. You decide what they use by editing the list in [loader.md](loader.md). Every file here is yours, and everything works without the CLI.

## Start with one existing document

Add a line under `## Entries` in the loader:

```markdown
- [Contribution rules to follow](../CONTRIBUTING.md) - #LoadNow
```

The document stays where it is and needs no frontmatter. `#LoadNow` means agents read it before every task. To limit it to some work, add `- applies to` and a pattern in backticks, such as `**/*.cs`. Leave the tag off when agents should open the file only when its description fits the task.

Any text file works, including rules you wrote for other tools:

```markdown
- [C# conventions](../docs/contributing/csharp.md) - #LoadNow - applies to `**/*.cs`
- [Testing rules from Cursor](../.cursor/rules/testing.mdc) - #LoadNow - applies to `src/**`
- [Architecture overview](../docs/architecture.md) - #Project
```

Open Forge reads a linked file as plain text. It ignores another tool's frontmatter and activation settings, so the line's tags and conditions decide when agents read it. Link files one by one rather than whole folders.

A link asks agents to read a file. It does not make the file's claims accepted, and it does not let agents edit it. Your accepted requirements keep their authority.

## Add only what helps

Each folder is a place for one kind of content, and you add only the ones you want:

- **Directives** hold rules agents must follow.
- **Memory** holds notes that help work continue. Its states hold accepted knowledge (Crystallized), work in progress (Working), findings not accepted yet (Emerging) and history (Archived).
- **Guidance** offers advice for recurring choices.
- **Patterns** show reusable shapes for code and documents.
- **Templates** provide starting files to copy.
- **Maps** point to important sources.
- **Skills** expose capabilities through native `SKILL.md` packages.

Existing ADRs and docs can stay where they are. A rule can live in `docs/agent-rules.md` and an accepted fact in `docs/export-format.md`, each linked from the loader like any other document. Named record kinds and workflows come from Extensions.

## Turn something off

Remove its line from the loader. The whole branch switches off and its files stay as notes. Remove or replace any part that depends on it first. Your accepted requirements, and instructions your agent tool loads on its own, still apply.

Inside a catalogue, the list follows the files on disk. To keep a file as a note there, move it out of the routed folder and remove its line. To get rid of it, delete the file and its line. When the CLI manages updates, record the removal too, so updates don't bring it back. Custom's `remove` choice only stops Open Forge managing the defaults. It keeps existing files and routes, and it doesn't switch them off.

## Instructions

These apply whenever you or an agent write workspace context, catalogues or metadata.

- Connect related material with relative links to local sources and precise references. Reuse consistent descriptive tags for shared subjects. Add links and tags only when they help someone find or retrieve something.
- Put detail in the narrowest source that defines it. A parent catalogue keeps only the summary and link needed to choose that source. Put material at workspace level only when it applies across the workspace. Don't repeat inherited rules.
- A folder has one catalogue, named `_{folder-name}.md`. The compatibility names `index.md`, `_index.md`, `references.md` and `_references.md` also work.
- For a catalogue without rules of its own, put only `- inherited - No local axioms; loaded ancestor axioms remain active.` under `## Axioms`. A missing or empty Axioms section also adds no rules. Never mix that line with local rules.
- Scopes can nest before, between or after deeper route segments, and each one narrows everything after it.
- A scope holds only the routes that are useful there. It doesn't need to copy another scope or the installed defaults.
- New routed files in `.agents/` need YAML `description` and `tags`, with optional `applyTo` and `responsibility`. The description helps readers choose the file. The responsibility says what the file defines and adds no authority. A native `SKILL.md` keeps its own metadata.
- Follow the `frontmatter` setting in `open-forge.json`. `root` puts the fields at the top level, and `scoped` puts them under `open-forge:`. A missing setting means scoped. Keep all Open Forge fields together, and write tags without `#`.
- Write `applyTo` as a list of glob patterns or a comma-separated string. Both the root and the scoped form are read. Equivalent sets count once, and conflicting sets are invalid. Repeat the conditions after an entry's tags as `applies to` patterns.
- Keep each entry's relative path, description, tags and conditions current. With the CLI, preview `open-forge index --dry-run` before you run `open-forge index`. Without it, update Entries by hand.
- For a change that should survive updates, use an `{name}.overwrite.md` next to the file. Updates can replace direct edits to shipped files, so review the update plan before you apply it.

## Terms you may meet

- A **route** is a path through catalogues and Entries. A **root route** starts in the loader.
- A **scope** narrows the content that follows it, and a **slug** is a concrete folder name in a route.
- **#Core** labels base routing, loading, workspace orientation and reusable agent-facing roles.
- **#Memory** labels self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history.
- **#Extension** labels optional packaged routes, capabilities, integrations and support files.

These labels don't switch anything on and don't make Open Forge manage a file.

For example, a C# scope might hold a #LoadNow design file and an on-demand Windows scope. Selecting C# loads its design file. A #LoadNow file under Windows loads only once Windows is selected.

## What the CLI adds

`open-forge context --for <file>`, with `--for` once for each working file, gathers the required reads in one call. Agents still choose the other relevant entries and follow explicit reading instructions.

`open-forge install --preset minimal` installs the loader, this page, Directives, Crystallized Memory and the managed sections in `AGENTS.md` and `CLAUDE.md`. `open-forge install --configure` adds or leaves out parts later, and `open-forge route init <category> --framework` restores one. The CLI can also maintain navigation, preview installation changes and record removals. It never decides which knowledge you have accepted. Its settings and ownership files support maintenance and give agents no authority.
