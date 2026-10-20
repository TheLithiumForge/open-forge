# Open Forge in this project

Agents read [`loader.md`](loader.md) before they work. Its `Entries` list is the whole setup: each line points to something agents should use, and anything not listed is off. Everything here is plain Markdown that you own. The `open-forge` CLI is optional and only makes reading and upkeep faster.

## Point agents at a document you already have

Add one line under `Entries` in `loader.md`. The document stays where it is and needs no changes.

```markdown
- [C# conventions to follow](../docs/contributing/csharp.md) - #LoadNow - applies to `**/*.cs`
```

`#LoadNow` means agents always read it. `applies to` limits it to work on matching files. Leave both off for something agents should open only when it fits the task.

## Add a rule

Write the rule in a file under `directives/`, put it under a `## Instructions` heading, and tag the file `#LoadNow`. Give it `applyTo` if it only matters for some files.

## Turn something off

Delete its line from `loader.md`. Its files stay as notes and their rules stop applying, while your accepted project requirements and other agent instructions stay in effect. Nothing else depends on an optional folder unless its own file says so. If you update through the CLI, record the removal so updates keep it off.

## Keep work going across sessions

Turn on Memory. Agents read the record of the work at the start, and `memory/working/saving.md` tells them to update it before they pause. To stop the automatic saving and keep the records, delete `saving.md` and remove its entry from `memory/working/_working.md`.

## Change a shipped file

Edit it, or write only your change in `{name}.overwrite.md` next to it. The overwrite wins where they differ and survives updates.

## Without the CLI

Everything above works by editing files. With the CLI, `open-forge context --for <file>` gathers the required reads in one call, and `open-forge index` keeps `Entries` in step after you add or move files.
