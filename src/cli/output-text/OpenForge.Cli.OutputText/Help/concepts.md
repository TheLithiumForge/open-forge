# Open Forge concepts

Open Forge is a set of plain Markdown files that tells AI coding agents where a project's rules and knowledge live and when to read them. Everything works with ordinary file reads. The CLI only makes it faster and keeps navigation current.

This topic explains the ideas. Your workspace's own `.agents/loader.md` is the authority for how agents read it, so read that file before you work in a workspace.

## Start here

Agents read `.agents/loader.md` before every task. It says what to read, how to use it and how to change what is switched on.

- Without the CLI, open the loader and follow its steps.
- With the CLI, `open-forge context --for <file>`, once for each file you will work on, returns the required reads in one call. You still choose the other relevant entries yourself.

`AGENTS.md` and `CLAUDE.md` at the workspace root point agent tools at the loader.

## What is switched on

The list under `## Entries` at the bottom of the loader is the whole setup. Each line switches something on: a folder of rules or notes, or any document in the repository.

- A line that is listed is on. Anything not listed is off and serves only as background reading.
- Lines are read in the order they are listed.
- A listed file that is missing is an error. An empty list is valid.
- Removing a line switches its whole branch off. Its files stay where they are.
- The instructions your agent tool loads on its own, and your project's accepted requirements, still apply whatever the list says.

## Choosing what to read

A catalogue is a file that lists a folder's context under its own `## Entries`. Each entry carries a description, optional tags and optional conditions.

- **Required reads.** Read every applicable entry tagged #LoadNow or #KeepInMind, in the listed order. When it is a catalogue, apply the same rule to its Entries.
- **On demand.** Read the descriptions of the other entries and open the ones that fit the task.
- **Explicit instructions.** Follow any link that a file you read says to read.
- **Search.** When search or a link leads you to a listed file, read the catalogues above it before you apply it.

Use catalogues to find things. Don't open ordinary files just to discover more links or tags. Start again when your goal or working files change.

## Conditions

An entry can end with `applies to` and one or more patterns. It then applies only when one of your working files matches it, and that same file also matches every conditioned catalogue above it.

- Working files are the files you will investigate, review, create, change, rename or delete, including planned ones. A file you only read for background is not one.
- Patterns are case-sensitive globs from the workspace root. `*.py` matches root files, `**/*.py` matches any depth and `docs/**` matches a folder's contents. A leading `!` is literal, not an exclusion.
- Conditions never open a branch on their own and never grant edit permission.
- While your working files are unknown, report conditioned required entries as pending, and resolve them before you touch a matching file.

## Rules and acceptance

- **Axioms** are the rules under `## Axioms` in the loader and in catalogues. They apply to everything below them.
- **Instructions** are the rules under `## Instructions` in a rule file, such as a Directive. They apply within the scope the file states.
- A narrower rule adds to a broader one and never outranks it. When two rules conflict, or one cannot be followed, say so and explain why.
- **Reading is not accepting.** Reading, linking, saving or tagging something does not make it accepted, and does not give permission to edit it. Content is accepted when the user agrees, or when a source the user named for that question says so. #Contextual marks content that is not accepted. #CurrentTruth marks accepted current state within its scope.

## Loading tags

- **#LoadNow** means a file is read whenever its parent loads.
- **#KeepInMind** means it is read when its parent loads and read again when a task starts or resumes, after context is restored, and before handoff or closeout.
- Both act only through catalogues that were read, so neither opens a branch nobody selected.
- Every other tag describes content for search and selection. #Evergreen marks material that must stay in step with accepted current state. #Core, #Memory and #Extension are labels.

## Content kinds

Each top-level folder holds one kind of content, and a workspace adds only the ones it wants.

- **Directives** hold rules agents must follow.
- **Guidance** offers advice for recurring choices.
- **Maps** point to important sources.
- **Patterns** show reusable shapes for code and documents.
- **Templates** provide starting files to copy.
- **Skills** expose capabilities through native `SKILL.md` packages that the agent tool discovers and runs.
- **Memory** holds notes that help work continue: accepted knowledge (Crystallized), work in progress (Working), findings not accepted yet (Emerging) and history (Archived).

`open-forge install --preset minimal` installs the loader, Directives and Crystallized Memory. The other presets add more, and `open-forge install --configure` adds or leaves out parts later.

## Reusing knowledge

There are three ways to bring reusable knowledge in:

- **Include** uses a document already in this project, where it lives. `open-forge route include CONTRIBUTING.md --description "Contribution rules" --tag LoadNow` adds its line to the loader, and `open-forge route exclude CONTRIBUTING.md` removes it. A line written by hand works the same way.
- A **Library** projects a folder registered in this workspace into another place through relative file links.
- An **Extension** packages files you can install in any project and keeps them up to date.

An included file needs no frontmatter. Its line carries the description, tags and conditions, and another tool's frontmatter never decides when it loads. A Markdown file with its own `## Entries` brings its listed files along. `.agents/open-forge.json` records included files under `roots`, but the loader's line decides whether and how each one loads.

Management decides which files the CLI may install, update or remove. It never decides what a file means or what is accepted.

## Customizing safely

- **Overwrites.** To change a shipped file in a way that survives updates, put the change in `{name}.overwrite.md` next to it. It is read right after its base and wins only where the two say different things about the same content.
- **Switching off.** Remove a line from the loader, or run `open-forge route exclude` with the catalogue. Before you switch a part off, remove or replace the parts that need it.
- **Removing.** `open-forge remove <path>` deletes a file and records it in the settings so updates don't bring it back. Deleting a file by hand records nothing.
- **Restoring.** `open-forge route init <category> --framework` restores a category.

## Keeping navigation current

`open-forge index` fills in missing metadata and the generated `Entries` lists inside `.agents/`. It keeps the loader lines you or `route include` wrote, in their order, and never writes to included files outside `.agents/`. Preview with `open-forge index --dry-run`.

`open-forge status` summarizes the workspace and its startup cost. `open-forge doctor` diagnoses routes, links and lifecycle without changing anything.

## Build your own

The rules that matter most are the ones only your project knows. Write them down as Directives, keep accepted knowledge in Memory, and include the documents you already have. Rename, restructure and replace anything to fit how your team works. When something proves useful in more than one project, share it as a Library or an Extension.
