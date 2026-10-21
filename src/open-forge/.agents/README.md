# Open Forge in this project

Open Forge tells agents where this project's rules and knowledge live. You choose the links in [loader.md](loader.md). The files stay yours, and everything works without the CLI.

Agents read the loader before work. They read this page when authoring workspace context, catalogues or metadata. People can start here.

## Start with one existing document

Add a line under the loader's `## Entries`:

```markdown
- [Contribution rules to follow](../CONTRIBUTING.md) - #LoadNow
```

The document stays where it is and needs no frontmatter. #LoadNow requires reading it whenever the loader loads. Add `- applies to` and a pattern in backticks, such as `**/*.cs`, to limit it to matching work. Without a loading tag, agents open it when its description fits the task.

Any text file works, including rules you wrote for other tools:

```markdown
- [C# conventions](../docs/contributing/csharp.md) - #LoadNow - applies to `**/*.cs`
- [Testing rules from Cursor](../.cursor/rules/testing.mdc) - #LoadNow - applies to `src/**`
- [Architecture overview](../docs/architecture.md) - #Project
```

Open Forge reads a linked file as text. It does not interpret another tool's frontmatter or activation settings, so the entry's tags and conditions decide when agents read it. Link individual files rather than whole folders.

Linking does not establish acceptance or grant permission to edit the destination. Existing accepted requirements keep their authority.

## Add only what helps

Directives give required behavior a home. Memory can hold accepted knowledge, work in progress, unsettled findings or history. Each Memory state needs Memory, but the states do not need each other. The saving policy needs Working Memory.

Guidance offers advice. Patterns show reusable shapes. Templates provide starting files. Maps point to sources. Skills expose capabilities through native `SKILL.md` packages. Each needs only the loader unless its own content declares another dependency.

You may keep existing ADRs and docs instead of adopting any of these folders. A rule can stay in `docs/agent-rules.md` and an accepted fact in `docs/export-format.md`, linked from the loader like any other document. Named record categories and workflows are optional Extensions.

## Turn something off

Remove its loader entry to disable an entire branch and keep its notes. Remove or replace enabled dependents first. Existing accepted requirements and independently loaded agent instructions still apply.

Inside a catalogue, the list follows files on disk. Move a file outside that routed folder and remove its entry to keep it as a note, or delete the file and its entry if you want it gone. CLI-managed removals also need recorded exclusions before updates. Custom's `remove` action releases management and keeps existing routes. It does not disable them.

To stop automatic progress saving while keeping Working records, delete `memory/saving.md` and remove its entry from `memory/_memory.md`. Record that file removal when using managed updates. Emerging's candidate capture is a separate choice and stays active while that state is enabled with #KeepInMind.

## Instructions

These instructions apply when authoring workspace context, catalogues or metadata.

- Connect related material with relative links to local sources and precise references. Reuse consistent descriptive tags for shared subjects. Add links and tags only when they help navigation or retrieval.
- Put detail in the narrowest source that defines it. Parent catalogues keep the summary and link needed to select that source. Use workspace-wide placement only for material that applies across the workspace. Do not repeat inherited rules.
- A folder has one catalogue named `_{folder-name}.md`. Compatibility names are `index.md`, `_index.md`, `references.md` and `_references.md`.
- For a catalogue without local rules, use `## Axioms` containing only `- inherited - No local axioms; loaded ancestor axioms remain active.` Existing absent or empty sections also add no rules. Never combine that marker with local rules.
- Scopes may nest before, between or after deeper route segments. Each narrows everything that follows it.
- A scope contains only useful routes. It need not copy another scope or the installed defaults.
- New routed `.agents/` files need YAML `description` and `tags`, with optional `applyTo` and `responsibility`. Descriptions help readers choose files. Responsibility states what a file defines and adds no authority. Native `SKILL.md` keeps its native metadata.
- Follow `frontmatter` in `open-forge.json`: `root` puts fields at the top level and `scoped` puts them under `open-forge:`. A missing setting means scoped. Keep all Open Forge fields together. Tags are names without `#`.
- Write `applyTo` as a list of glob patterns or a comma-separated string. Root and scoped conditions are both read. Equivalent sets count once and conflicting sets are invalid. Repeat those conditions after an entry's tags as `applies to` patterns.
- Keep each entry's relative path, description, tags and conditions current. With the CLI, preview `open-forge index --dry-run` before applying `open-forge index`. Without it, maintain Entries by hand.
- For an adjustment that survives updates, use an adjacent `{name}.overwrite.md`. Direct edits to shipped files may be replaced by Update. Review its plan before applying it.

## Terms you may meet

A route is a navigable path through catalogues and Entries. A root route starts in the loader. A scope narrows the content that follows it. A slug is a concrete folder name in a route.

#Core labels base routing, loading, workspace orientation and reusable agent-facing roles. #Memory labels self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history. #Extension labels optional packaged routes, capabilities, integrations and support files. These labels do not activate content or establish management.

For example, a C# scope may contain a #LoadNow design file and an on-demand Windows scope. Selecting C# loads its design file. A #LoadNow file under Windows loads only after Windows is selected.

## What the CLI adds

`open-forge context --for <file>` gathers required reads. Repeat `--for` for all working files. Agents still choose relevant entries and follow explicit reading instructions.

`open-forge install --preset minimal` installs the loader, this page, Directives, Crystallized Memory and the managed `AGENTS.md` and `CLAUDE.md` sections. `open-forge install --configure` adds or leaves out parts later, and `open-forge route init <category> --framework` restores one. The CLI can maintain navigation, preview installation changes and record removals. It does not decide which knowledge you have accepted. Settings and ownership JSON support maintenance and add no agent authority.
