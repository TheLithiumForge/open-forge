# Open Forge in this project

**You may delete this page.** It is a guide for the people who run this project, and it describes the file formats agents follow when they write workspace files. Everything here is also in `open-forge help concepts`, so no rule depends on this page. To keep it deleted through `open-forge update`, add `.agents/README.md` to `removedFiles` in `.agents/open-forge.json`.

Open Forge tells agents where your rules and knowledge live. You decide what they use with the list under `## Entries` in [loader.md](loader.md). Every file here is yours, and everything works without the CLI. The CLI's settings and ownership files support maintenance and give agents no authority.

## Choose the context folder

At first installation, `open-forge install --main-root <folder>` chooses the context folder. The default is `--main-root .agents`. A custom folder, such as `docs/context`, must be absent or empty. Interactive Install asks `Where should Open Forge keep its context files?` when the flag is omitted.

The loader, this guide and ordinary categories live in the context folder. Native Skills stay in `.agents/skills/`, settings in `.agents/open-forge.json`, and ownership in `.agents/open-forge.lock.json`. New settings use schema version 4 for every layout. Workspace-root `AGENTS.md` and `CLAUDE.md` point to the selected loader.

Use `./...` for an exact workspace-relative CLI path. Short source IDs, such as `memory/crystallized`, work with either layout. Older CLIs do not support custom roots. Relocating an established installation is not available yet, including through `install --configure`. Editing `mainRoot` in settings moves no files.

Default installation preview:

```sh
open-forge install --preset essentials --dry-run
```

Custom context folder preview:

```sh
open-forge install --preset essentials --main-root docs/context --dry-run
```

Review the plan, then repeat the chosen request without `--dry-run` and with `--automatic` to apply it unattended.

## Use a document you already have

Your rules may already live somewhere, such as a contribution guide. Include it where it is:

```sh
open-forge route include CONTRIBUTING.md --description "Contribution rules to follow" --tag LoadNow
```

That adds this line to the loader, which you can also write by hand:

```markdown
- [Contribution rules to follow](../CONTRIBUTING.md) - #LoadNow
```

`#LoadNow` means agents read it before every task. Leave the tag off when agents should open the file only when its description fits the task. To limit a file to some work, add a pattern: `--apply-to "**/*.cs"` writes `- applies to` and the pattern at the end of the line.

Any text file works, including rules you wrote for other tools, such as `.cursor/rules/testing.mdc`. Open Forge ignores another tool's frontmatter and activation settings, so the line's tags and conditions decide when agents read the file. Include files one by one rather than whole folders. A Markdown file with its own `## Entries` list, such as `docs/team/_team.md`, brings its listed files along.

Including a file asks agents to read it. It doesn't make its claims accepted and doesn't let agents edit it. `open-forge route exclude CONTRIBUTING.md` removes the line and leaves the file alone.

## Write your first rule

The rules that matter most are the ones only your project knows. Write them down as Directives, one subject per file:

```sh
open-forge route create directives/exports --description "How data exports are formatted" --tag LoadNow --tag Directive
```

Then write the rule under `## Instructions`, as [Rule files](#rule-files) shows. Start with one rule that agents keep getting wrong, and add more as you notice them. This is your framework: rename, restructure and replace anything here to fit how your team works.

## Add only what helps

Each folder holds one kind of content, and you add only the ones you want:

- **Directives** hold rules agents must follow.
- **Memory** holds notes that help work continue. Its states hold accepted knowledge (Crystallized), work in progress (Working), findings not accepted yet (Emerging) and history (Archived).
- **Guidance** offers advice for recurring choices.
- **Patterns** show reusable shapes for code and documents.
- **Templates** provide starting files to copy.
- **Maps** point to important sources.
- **Skills** expose capabilities through native `SKILL.md` packages.

Existing ADRs and docs can stay where they are and be included. Named record kinds and workflows come from Extensions.

`open-forge install --configure` adds or leaves out these parts later, and `open-forge route init <category> --framework` restores one.

## Stop using something

Remove its line from the loader, or run `open-forge route exclude` with its catalogue, such as `.agents/patterns/_patterns.md`. Agents stop using the whole branch, and its files stay as notes. Remove or replace any part that depends on it first. Your accepted requirements, and instructions your agent tool loads on its own, still apply.

Inside a catalogue, the list follows the files on disk. To keep a file as a note there, move it out of the routed folder and remove its line. To get rid of it, delete the file and its line. When the CLI manages updates, record the removal too, so updates don't bring it back. Custom's `remove` choice only stops Open Forge managing the defaults. It keeps existing files and routes, and it doesn't switch them off.

## Reuse your knowledge elsewhere

When your rules prove useful, share them:

- **Include** uses a document already in this project, where it lives.
- A **Library** projects a folder registered in this workspace into another place through links.
- An **Extension** packages files you can install in any project.

## Find the details

- [loader.md](loader.md) says what agents read and how they use it.
- `open-forge help concepts` explains every concept in one place, including the file formats below.
- `open-forge <command> --help` shows a command's options.
- The website has the full guide: <https://thelithiumforge.github.io/open-forge/>.

## Writing workspace files

Read this before you write or change workspace context: catalogues, rule files, metadata or lines under `Entries`. It describes the file shapes that agents and the Open Forge CLI both read. A file in another shape is ignored or reported as malformed. This section describes formats, not how to do your work.

### Lines under Entries

Each entry is one line:

```markdown
- [C# rules to follow when changing C# code](csharp/_csharp.md) - #Directive #KeepInMind - applies to `**/*.cs`, `**/*.csproj`
```

- The link text is the description. Write it so a reader can decide whether to open the file.
- The path is relative to the file that holds the list. In the loader it can leave the folder, such as `../CONTRIBUTING.md`.
- Tags come after ` - `, each with `#`. Only #LoadNow and #KeepInMind change loading. Leave them off when the file should be opened only when its description fits the task.
- Conditions come last, as `applies to` followed by patterns in backticks, separated by a comma and a space. For a file in `.agents/`, they repeat the `applyTo` patterns in its frontmatter. For a file outside `.agents/`, the line is where they are set.

Keep each entry's path, description, tags and conditions current. With the CLI, preview `open-forge index --dry-run`, then run `open-forge index`. Without it, update the lines by hand.

### Catalogues

A catalogue lists a folder's context under its own `## Entries`. A folder has one, named `_{folder-name}.md`. The older names `index.md`, `_index.md`, `references.md` and `_references.md` also work.

```markdown
---
open-forge:
  description: Rules for testing, read before changing tests
  tags: [Directive, Testing]
---

# Testing

## What rules apply to tests?

Rules that apply to every test in this repository.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [How to name and place tests](naming.md) - #Directive
```

- Under the title, write the one question the folder answers as a `##` heading, then a short paragraph on what the folder holds. A catalogue describes its own folder and does not name other routes.
- Put rules for everything below the catalogue under `## Axioms`. A catalogue without rules of its own has only the `inherited` line shown above. A missing or empty Axioms section also adds no rules. Never mix that line with local rules.
- List the folder's files and the catalogues of its child folders under `## Entries`. An empty list is valid.

### Rule files

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

### Metadata

New routed files in `.agents/` start with YAML frontmatter:

- `description` is required. It helps readers decide whether to open the file.
- `tags` is required. Write tag names without `#`.
- `applyTo` is optional. It holds glob patterns, as a list or a comma-separated string, that limit when the file applies.
- `responsibility` is optional. It says what the file defines, so an editor knows what belongs there. It adds no authority.

Follow the `frontmatter` setting in `.agents/open-forge.json`. `root` puts the fields at the top level, and `scoped` puts them under `open-forge:`, as in the examples above. A missing setting means scoped. Keep all Open Forge fields together. Both forms of `applyTo` are read. Equal pattern sets count once, and different sets are invalid.

A native `SKILL.md` keeps its own metadata. A file outside `.agents/` that the loader lists needs no frontmatter, because its line carries the description, tags and conditions.

### Where things go

- Connect related material with relative links to local sources and precise references. Reuse consistent descriptive tags for shared subjects. Add links and tags only when they help someone find or retrieve something.
- Put detail in the narrowest source that defines it. A parent catalogue keeps only the summary and link needed to choose that source. Put material at workspace level only when it applies across the workspace. Don't repeat inherited rules.
- A scope is a folder that narrows everything after it. Scopes can nest before, between or after deeper route segments. A scope holds only the routes that are useful there, and doesn't need to copy another scope or the installed defaults.

For example, a C# scope might hold a #LoadNow design file and an on-demand Windows scope. Selecting C# loads its design file. A #LoadNow file under Windows loads only once Windows is selected.

### Changes that survive updates

Updates can replace direct edits to shipped files. For a change that should survive them, write `{name}.overwrite.md` next to the file. It is read right after its base and wins only where the two say different things about the same content. Review the update plan before you apply it.

### Terms

- A **route** is a path through catalogues and Entries. A **root route** is a line in the loader.
- A **slug** is a concrete folder name in a route.
- **#Core** labels base routing, loading, workspace orientation and reusable agent-facing roles.
- **#Memory** labels self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history.
- **#Extension** labels optional packaged routes, capabilities, integrations and support files.

These labels add nothing to what agents read and don't make Open Forge manage a file.
