# Open Forge Loader

Agents read this file before every task. It says what to read, how to use it, and how to change what is switched on.

The list under `Entries` at the bottom is the whole setup. Each line switches something on: a folder of rules or notes, or any document in your repository. Anything not listed is off and serves only as background reading. Your project's own requirements, and the instructions your agent tool already loads, still apply. Everything works with plain files, and the CLI only makes it faster.

## Axioms

### Read before you work

1. **List your working files.** Do this when a task starts or resumes, and again after your context is restored. Working files are the files you will investigate, review, create, change, rename or delete, including planned files and both paths of a rename. A file you only read for background is not a working file.
2. **Use the CLI if you can.** If the `open-forge` CLI is available and allowed, run `open-forge context` with `--for <file>` once for each working file, and read its output. It covers steps 3 and 4. You still do step 5 yourself. If the command is missing, fails or cannot follow a link, do steps 3 to 5 with your own file tools.
3. **Check conditions first.** Some entries end with `applies to` and one or more patterns. Such an entry applies only when one of your working files matches it, and that same file also matches every conditioned catalogue above it. Within one list, any pattern may match. Patterns are case-sensitive globs from the workspace root: `*.py` matches root files, `**/*.py` matches any depth, and `docs/**` matches a folder's contents. A leading `!` is literal, not an exclusion. Conditions never open a branch on their own and never grant edit permission. If you don't know your working files yet, report conditioned required entries as pending, and resolve them before you touch a matching file.
4. **Read what is required.** Read every applicable entry tagged #LoadNow or #KeepInMind, in the order listed. A catalogue is a file that lists a folder's context under its own `Entries`. When you read one, repeat steps 3 and 4 for its Entries. Never skip a required read because its description looks unrelated. Read each file once per pass.
5. **Open what fits the task.** Check the descriptions of the other listed entries and open the ones that fit. Follow any link that a file you read says to read. If search or a direct link leads you to a listed file, read the catalogues above it before you apply it. Keep separately selected branches separate. Use catalogues to find things, and don't open ordinary files just to discover more links or tags.

Start again from step 1 when your goal or working files change. Before you hand off or finish, reread the #KeepInMind entries in the branches you are using. During work, reread them only when they may have changed.

### Use what you read

- **Rules.** Follow the `## Axioms` of every catalogue you load and the `## Instructions` of every rule file, within the scope each one states. Only this loader and loaded catalogues define Axioms. A narrower rule adds to a broader one and never outranks it. If two rules conflict, or you cannot follow one, say so and explain why.
- **The user.** Follow the user's direction within platform and safety limits. Take routine, reversible steps inside the request without asking again, and state the assumptions that affect the result. Ask first when an open choice could significantly change the result, scope, risk, cost, external effects or the ability to undo the work.
- **Answers.** Match explanation and planning to the request. Unless the user asks for deeper analysis, start with your current understanding, one recommendation and at most one important open choice.
- **Words.** In replies and in what you write, use the fewest words that keep the meaning, readability and logical order. Cut filler and repetition, but keep the context, conditions and exceptions a reader needs to apply the result.
- **What counts as accepted.** Reading, linking, saving or tagging something does not make it accepted, and does not give you permission to edit it. Content is accepted when the user agrees, or when a source the user named for that question says so. Use declared external authorities for the facts they own. #Contextual marks content that is not accepted, and #CurrentTruth marks accepted current state within its scope. Tags, repetition and your own confidence never make something accepted. Evidence can support a claim without making it accepted.
- **One question per source.** Let each source answer one clear question, and link to related sources instead of repeating their detail. A link never changes either source's authority, scope, loading or lifecycle. Selected advice or other non-binding material may specialize broader material of the same kind within its accepted scope.
- **Conflicts and change.** When two accepted sources disagree, investigate before you edit either. When accepted direction changes the current state, update the source that defines it and keep useful prior context.
- **#Evergreen.** This tag marks material that must stay in step with accepted current state. It grants no authority and loads nothing. Update affected #Evergreen sources you may edit before other work depends on them, and by closeout at the latest. Batch related updates when that is safe.
- **Staying current.** Keep each source true to what it represents now. Keep useful prior context in an enabled Memory route or another existing source. Report updates you could not make or were not allowed to make.

### Change what is switched on

- **Turn something on.** Add a line under `Entries`. It can link a catalogue or any text file in the workspace, relative to this file, such as `../docs/architecture.md` or `../.cursor/rules/testing.mdc`. The document stays where it is and needs no frontmatter, because the line carries its description, tags and conditions. If the linked file is a Markdown catalogue, such as `../docs/_docs.md`, its Entries are followed too. Without a loading tag, the file is opened when its description fits the task. With the CLI, `open-forge route include <file>` writes the line for you.
- **Turn something off.** Remove its line. Its files stay as plain notes, and reading them for background does not bring their rules back. Turning a part off does not cancel accepted project requirements or the instructions your agent tool loads on its own. A removed line stays removed until the user asks to restore it.
- **Check what depends on it.** Before you turn a part off, remove or replace the enabled parts that need it, and report anything you cannot resolve. Inside other catalogues, the list follows the files in that folder, so move or delete a file to keep it out. When the CLI manages updates, also record the removal in its exclusions before you update.
- **Missing files.** Report a listed file or required dependency that is missing as an error. An empty `Entries` list is valid.
- **Required reads.** #LoadNow means a file is read whenever its parent loads. #KeepInMind adds the rereads described above. Both act only through catalogues you have read, so neither opens a branch nobody selected. Keep a required file's catalogue chain unbroken. Add these tags only when the read is really needed, and narrow specialized content before you make it required. No other tag changes loading.
- **Writing context.** Before you author workspace context, catalogues or metadata, read [authoring.md](authoring.md), which describes the file shapes agents and the CLI read. With the CLI, `open-forge context .agents/authoring.md` returns it with any overwrite. New routed files carry `description`, `tags` and optional `applyTo`. A native `SKILL.md` keeps its own metadata.
- **Changes that survive updates.** Put your change in `{name}.overwrite.md` next to the shipped file. It is read right after its base, with the same role, scope and loading conditions. It wins only where the two say different things about the same content, and it is not listed or selected on its own.

## Entries

- [Rules agents must follow in this workspace](directives/_directives.md) - #LoadNow #Core #Directive
- [Advice for recurring choices, to check before choosing an approach](guidance/_guidance.md) - #Core #Guidance
- [Where to find project knowledge, checked before relying on it](maps/_maps.md) - #LoadNow #Core #Map
- [Notes that help work continue, such as resume points, accepted knowledge, ideas and history](memory/_memory.md) - #LoadNow #Memory
- [Reusable default shapes for code, files, APIs, documents, and other work](patterns/_patterns.md) - #Core #Pattern
- [Capabilities to check before specialized work](skills/_skills.md) - #Core #Skill
- [Starting files to copy when creating something new](templates/_templates.md) - #Core #Template
