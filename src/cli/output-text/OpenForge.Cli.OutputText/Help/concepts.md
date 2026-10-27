# Open Forge concepts

Open Forge is a set of plain Markdown files that tells AI coding agents where a project's rules and knowledge live and when to read them. Everything works with ordinary file reads. The CLI only makes it faster and keeps navigation current.

This topic explains the ideas. The selected workspace loader is the authority for how agents read it. The default is `.agents/loader.md`. First installation may choose another context folder, while native Skills, settings and ownership stay in `.agents` and runtime storage stays in `.open-forge-state/`.

## Start here

Agents read the selected loader before every task. It says what to read, how to use it and how to change what agents use.

- Without the CLI, open the loader and follow its steps.
- With the CLI, `open-forge context --for <file>`, once for each file you will work on, returns the required reads in one call. You still choose the other relevant entries yourself.

`AGENTS.md` and `CLAUDE.md` at the workspace root point agent tools at the loader.

## What agents use

The list under `## Entries` at the bottom of the loader is the whole setup: the folders of rules and notes, and any other documents, that agents use in the workspace.

- Anything not listed is only background reading.
- Lines are read in the order they are listed.
- A listed file that is missing is an error. An empty list is valid.
- Removing a line stops agents from using its whole branch. Its files stay where they are.
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

- **File Include** uses a document already in this project, where it lives. `open-forge route include CONTRIBUTING.md --description "Contribution rules" --tag LoadNow` adds its line to the loader, and `open-forge route exclude CONTRIBUTING.md` removes it. A line written by hand works the same way.
- A **Library** projects a folder registered in this workspace into another place through relative file links.
- An **Extension** packages files you can install in any project and keeps them up to date.

An included file needs no frontmatter. Its line carries the description, tags and conditions, and another tool's frontmatter never decides when it loads. A Markdown file with its own `## Entries` brings its listed files along. `.agents/open-forge.json` records included files under `roots`, but the loader's line decides whether and how each one loads.

Folder Include instead registers a safe existing directory outside the main root as a maintained route. `open-forge route include docs/team --dry-run` previews registration in settings v5 `outsideRoutes`, a Loader row and creation of only the missing canonical `docs/team/_team.md`. An existing catalogue keeps its bytes. File inclusion alone never grants folder maintenance. Folder Exclude removes the registration and matching Loader rows without deleting content.

Use exact source references with filenames, such as `./docs/team/_team.md`. Registration supplies inventory, while authored rows, loading tags and conditions decide what agents read. An unlisted registration can be maintained explicitly without relisting it. Outside `SKILL.md` files are ordinary Markdown. Registration grants no package ownership or implicit Extension or Library permission. Older CLIs do not maintain directory registrations, and preserving an unknown settings key does not make them compatible.

Management decides which files the CLI may install, update or remove. It never decides what a file means or what is accepted.

## Customizing safely

- **Overwrites.** To change a shipped file in a way that survives updates, put the change in `{name}.overwrite.md` next to it. It is read right after its base and wins only where the two say different things about the same content.
- **Removing a line.** Remove it from the loader, or run `open-forge route exclude` with the file or catalogue. Before you remove a line, remove or replace the parts that need it.
- **Removing.** `open-forge remove <path>` deletes a file and records it in the settings so updates don't bring it back. Deleting a file by hand records nothing.
- **Restoring.** `open-forge route init <category> --framework` restores a category.

## Keeping navigation current

`open-forge index` fills missing metadata and generated `Entries` in selected primary or registered outside routes. Bare Index maintains listed roots. Exact explicit selection can maintain an unlisted outside tree without relisting it. File-only inclusion does not make its target writable. Retained authored rows keep their policy and mutual order. Preview with `open-forge index --dry-run`.

Route Create, generic Route Init, Route Update, Route Move and Route Remove also work within admitted outside boundaries. Create needs an existing safe anchor. Init owns missing-chain restoration. Existing compatibility anchors keep their filenames. Move stays in one registration, and Move and Remove protect its directory and root entrypoints. Required foreign rewrites stop the complete plan. Writes retain expected-state checks, locking and recovery at the fixed runtime location.

`open-forge status` summarizes the workspace and its startup cost. `open-forge doctor` diagnoses routes, links and lifecycle without changing anything.

## Build your own

The rules that matter most are the ones only your project knows. Write them down as Directives, keep accepted knowledge in Memory, and include the documents you already have. Rename, restructure and replace anything to fit how your team works. When something proves useful in more than one project, share it as a Library or an Extension.
