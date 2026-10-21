# Writing workspace files

Read this before you write or change workspace context: catalogues, rule files, metadata or lines under `Entries`. It describes the file shapes that agents and the Open Forge CLI both read. A file in another shape is ignored or reported as malformed.

This page describes formats, not how to do your work. To change it in a way that survives updates, put your version in `authoring.overwrite.md` next to it.

## Lines under Entries

Each entry is one line:

```markdown
- [C# rules to follow when changing C# code](csharp/_csharp.md) - #Directive #KeepInMind - applies to `**/*.cs`, `**/*.csproj`
```

- The link text is the description. Write it so a reader can decide whether to open the file.
- The path is relative to the file that holds the list. In the loader it can leave the folder, such as `../CONTRIBUTING.md`.
- Tags come after ` - `, each with `#`. Only #LoadNow and #KeepInMind change loading. Leave them off when the file should be opened only when its description fits the task.
- Conditions come last, as `applies to` followed by patterns in backticks, separated by a comma and a space. For a file in `.agents/`, they repeat the `applyTo` patterns in its frontmatter. For a file outside `.agents/`, the line is where they are set.

Keep each entry's path, description, tags and conditions current. With the CLI, preview `open-forge index --dry-run`, then run `open-forge index`. Without it, update the lines by hand.

## Catalogues

A catalogue lists a folder's context under its own `## Entries`. A folder has one, named `_{folder-name}.md`. The older names `index.md`, `_index.md`, `references.md` and `_references.md` also work.

```markdown
---
open-forge:
  description: Rules for testing, read before changing tests
  tags: [Directive, Testing]
---

# Testing

Rules that apply to every test in this repository.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [How to name and place tests](naming.md) - #Directive
```

- Put rules for everything below the catalogue under `## Axioms`. A catalogue without rules of its own has only the `inherited` line shown above. A missing or empty Axioms section also adds no rules. Never mix that line with local rules.
- List the folder's files and the catalogues of its child folders under `## Entries`. An empty list is valid.

## Rule files

A rule file, such as a Directive, puts its rules under a non-empty `## Instructions` section. They apply within the scope the file states.

```markdown
---
open-forge:
  description: Run the full test suite before you hand off a change
  tags: [Directive, Testing]
---

# Test before handoff

## Instructions

- Run the full test suite before you hand off a change, and report any failure with its output.
```

## Metadata

New routed files in `.agents/` start with YAML frontmatter:

- `description` is required. It helps readers decide whether to open the file.
- `tags` is required. Write tag names without `#`.
- `applyTo` is optional. It holds glob patterns, as a list or a comma-separated string, that limit when the file applies.
- `responsibility` is optional. It says what the file defines, so an editor knows what belongs there. It adds no authority.

Follow the `frontmatter` setting in `open-forge.json`. `root` puts the fields at the top level, and `scoped` puts them under `open-forge:`, as in the examples above. A missing setting means scoped. Keep all Open Forge fields together. Both forms of `applyTo` are read. Equal pattern sets count once, and different sets are invalid.

A native `SKILL.md` keeps its own metadata. A file outside `.agents/` that the loader lists needs no frontmatter, because its line carries the description, tags and conditions.

## Where things go

- Connect related material with relative links to local sources and precise references. Reuse consistent descriptive tags for shared subjects. Add links and tags only when they help someone find or retrieve something.
- Put detail in the narrowest source that defines it. A parent catalogue keeps only the summary and link needed to choose that source. Put material at workspace level only when it applies across the workspace. Don't repeat inherited rules.
- A scope is a folder that narrows everything after it. Scopes can nest before, between or after deeper route segments. A scope holds only the routes that are useful there, and doesn't need to copy another scope or the installed defaults.

For example, a C# scope might hold a #LoadNow design file and an on-demand Windows scope. Selecting C# loads its design file. A #LoadNow file under Windows loads only once Windows is selected.

## Changes that survive updates

Updates can replace direct edits to shipped files. For a change that should survive them, write `{name}.overwrite.md` next to the file. It is read right after its base and wins only where the two say different things about the same content. Review the update plan before you apply it.

## Terms

- A **route** is a path through catalogues and Entries. A **root route** is a line in the loader.
- A **slug** is a concrete folder name in a route.
- **#Core** labels base routing, loading, workspace orientation and reusable agent-facing roles.
- **#Memory** labels self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history.
- **#Extension** labels optional packaged routes, capabilities, integrations and support files.

These labels don't switch anything on and don't make Open Forge manage a file.
