# Open Forge Loader

This file lists the context this workspace has chosen to use. It adds to the project's other instructions and works with plain files. The `Entries` below are the switch: what is listed is on, and anything not listed is off.

## Axioms

### Read before you work

1. At the start or resume of a task, and after your context is restored, list your working files. These are the files you will investigate, review, create, change, rename or delete, including planned ones. A file you only read for background is not a working file.
2. If the `open-forge` CLI is available and allowed, run `open-forge context` with `--for <file>` for each known working file, and read what it returns. That covers steps 3 and 4. Still do step 5 yourself. If the command is missing, fails or reports a link it cannot follow, do steps 3 to 5 with your own file tools.
3. Read every entry below tagged #LoadNow or #KeepInMind, in listed order, then do the same inside each catalogue you read. These are required reads. Never skip one because its description looks unrelated. Read each file once per pass.
4. Read and apply an entry with `applies to` patterns only when the same working file matches its patterns and those of every conditioned catalogue above it. Patterns are case-sensitive globs from the workspace root, such as `**/*.cs` or `docs/**`. While your working files are unknown, treat these entries as pending and resolve them before you touch a matching file.
5. Open other entries when their description fits the task, and read any file that something you read requires. Before applying a file you reached through search, read the listed catalogues above it. Files under an entry that is not listed are background only.

Start again from step 1 when your working files or goal change. Reread #KeepInMind entries before you hand off or finish.

### Use what you read

- Follow the rules you read within their stated scope: the `## Instructions` of rule files and the `## Axioms` of catalogues, which cover everything a catalogue lists. Narrower rules add to broader ones. When two rules conflict, say so instead of picking one.
- The user's direction comes first, within platform and safety limits. Take routine, reversible steps inside the request without asking again, and ask before a choice that would significantly change the result, scope, risk or cost.
- Reading, linking, saving or tagging a file does not make its content accepted, and gives no permission to edit it. Accepted means the user agreed, or a source the user named says so.

### Change what is on

- Add a line under `Entries` to turn something on. It can point to a catalogue or straight to a file anywhere in the workspace, such as `../docs/architecture.md`. A linked document needs no move and no new frontmatter, because its entry line carries the description, tags and conditions.
- Remove a line to turn that part off. Its files stay as plain notes, and reading them for background does not bring back their rules. Turning a part off does not cancel accepted project requirements or instructions the host loads on its own. If another listed part depends on it, say so first. Nothing that is not listed creates a duty, and nothing should bring it back unless the user asks. Lines inside other catalogues follow the files in their folder, so to turn one of those off, delete or move its file. When the CLI manages updates, record the removal too. The CLI Skill explains how.
- A listed file that is missing is an error to report. An empty `Entries` list is valid.
- Tag an entry #LoadNow when it must always be read, and #KeepInMind when it must also be reread before a handoff. A required file inside a catalogue needs #LoadNow on that catalogue too. Every other tag is a search label.
- New files inside `.agents/` start with frontmatter: `description`, `tags`, and optional `applyTo`, in the same form as the files around them. A folder's catalogue is named `_{folder-name}.md`. `open-forge index` keeps Entries in step with them.
- To change a shipped file and keep updates, write your change in `{name}.overwrite.md` beside it. It is read right after its base and wins where they differ, for that file only.

## Entries

- [Rules agents must follow in this workspace](directives/_directives.md) - #LoadNow #Core #Directive
- [Advice for recurring choices, to check before choosing an approach](guidance/_guidance.md) - #Core #Guidance
- [Where to find project knowledge, checked before relying on it](maps/_maps.md) - #LoadNow #Core #Map
- [Notes that help work continue, such as resume points, accepted knowledge, ideas and history](memory/_memory.md) - #LoadNow #Memory
- [Reusable default shapes for code, files, APIs, documents, and other work](patterns/_patterns.md) - #Core #Pattern
- [Capabilities to check before specialized work](skills/_skills.md) - #Core #Skill
- [Starting files to copy when creating something new](templates/_templates.md) - #Core #Template
