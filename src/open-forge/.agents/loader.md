# Open Forge Loader

This workspace uses the context linked under `Entries`. Listing a file makes it available to Open Forge. Loading tags and task relevance decide when to read it. Unlisted files remain background notes. Existing project requirements and instructions loaded by your agent tool still apply. Everything works with plain files, and the CLI only makes it faster.

## Axioms

### Read before you work

1. At task start or resume, and after context restoration, identify your working files. Include files you will investigate, review, create, change, rename or delete, including planned files and both paths of a rename. Background reading does not make a file a working file.
2. If the `open-forge` CLI is available and allowed, run `open-forge context` with `--for <file>` repeated for every known working file. Read its output. This covers the required reads and conditions in steps 3 and 4. Always do step 5 yourself. If the command is missing, fails or cannot follow a link, complete steps 3 to 5 with your own file tools.
3. Read entries tagged #LoadNow or #KeepInMind in listed order. Check step 4 before opening a conditioned entry. A catalogue lists a folder's context under `Entries`. In each catalogue you read, repeat this procedure for its Entries. Never skip a required read because its description seems unrelated. Read each file once per pass.
4. Conditions filter reading and application. Within one `applies to` list, any pattern may match. One working file must match a pattern at every conditioned catalogue above the entry and at the entry itself. Patterns are case-sensitive globs from the workspace root. For example, `*.py` matches root files, `**/*.py` matches any depth, and `docs/**` matches a folder's contents. A leading `!` is literal. Conditions never select an unopened branch or grant edit permission. If working files are unknown, report required conditioned entries as pending and resolve them before matching work.
5. Check the descriptions of other listed entries and open those that fit the task. Follow explicit required-read links. Before applying a listed source found through search or a direct link, read the catalogues above it first. Keep separately selected branches separate.

Restart when the goal or working files change. Reread #KeepInMind entries before handoff or finish, only in active branches. During work, recheck them only when they may have changed. Use catalogues for navigation. Do not open ordinary file bodies just to discover routes or tagged files.

### Use what you read

- Follow the `## Axioms` of loaded catalogues and the `## Instructions` of rule files within their stated scope. Only this loader and recognized loaded catalogues define Axioms. Narrower rules add to broader rules and never outrank them. Report conflicts or instructions you cannot follow, and explain why.
- Follow user direction within platform and safety limits. Take routine, reversible steps within the request without asking again. State assumptions that affect the result. Ask before an unresolved choice could significantly change the result, scope, risk, cost, external effects or ability to undo the work.
- Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.
- Use the fewest words that preserve meaning, readability, and logical progression in communication and authored content. Remove filler and repetition. Keep the context, conditions, and exceptions needed to understand and apply the result.
- Reading, linking, saving or tagging content does not make it accepted or permit editing it. Acceptance comes from the user or an authority they declared for that question. Use declared external authorities for their assigned facts. #Contextual marks unaccepted context and #CurrentTruth marks accepted current state within its scope. Tags, repetition and agent confidence do not establish acceptance. Evidence can support a claim without establishing acceptance.
- Let each source answer one clear question. Link to related sources instead of repeating their detail. A link does not change either source's authority, scope, loading rules or lifecycle.
- Selected non-binding material may specialize broader material of the same kind within its accepted scope.
- Investigate conflicting accepted sources before editing either. When accepted direction changes current state, update its defining source and preserve useful prior context.
- #Evergreen means material must stay aligned with accepted current state. It creates no authority or loading behavior. Update affected #Evergreen sources you may edit before work depends on them and no later than closeout. Batch related updates when safe.
- Keep each source aligned with what it represents now. Preserve useful prior context in an enabled Memory route or another existing project source. Report needed updates that are blocked or not authorized.

### Change what is enabled

- Add a line under `Entries` to enable a file or catalogue. Links are relative to this file and may reach any text file in the workspace, such as `../docs/architecture.md` or `../.cursor/rules/testing.mdc`. Existing documents need no move or new frontmatter, because their entry supplies the description, tags and conditions. A linked Markdown catalogue, such as `../docs/_docs.md`, has its Entries followed too. A link without a loading tag is opened when its description fits the task.
- Remove a loader entry to disable that branch while keeping its files as notes. Background reading does not reactivate its rules. Disabling it does not cancel accepted project requirements or instructions loaded independently by your agent tool. Removed choices stay removed until the user requests restoration.
- Before disabling a part, remove or replace enabled dependents that need it, and report unresolved dependencies. Inside other catalogues, Entries follow the folder's files, so remove or move a file to keep its entry out. Record managed removals in the CLI's exclusions before updating.
- Report missing listed files or required dependencies as errors. An empty `Entries` section is valid.
- #LoadNow requires reading whenever its parent loads. #KeepInMind also requires the refreshes above. Both act only through catalogues you have read, so neither opens a branch nobody selected. Keep required catalogue chains unbroken. Use loading tags only when those reads are needed, and narrow specialized content before making it mandatory. Only these two tags control loading.
- Before authoring workspace context, catalogues or metadata, read [the authoring instructions](README.md#instructions). New routed files use `description`, `tags` and optional `applyTo`. Native `SKILL.md` keeps its native metadata.
- To keep local changes through updates, place them in `{name}.overwrite.md` beside the base file. Read it immediately after the base, with the same role, scope and loading conditions. It wins only for corresponding content in that source. Do not index or select it separately.

## Entries

- [Rules agents must follow in this workspace](directives/_directives.md) - #LoadNow #Core #Directive
- [Advice for recurring choices, to check before choosing an approach](guidance/_guidance.md) - #Core #Guidance
- [Where to find project knowledge, checked before relying on it](maps/_maps.md) - #Core #Map
- [Notes that help work continue, such as resume points, accepted knowledge, ideas and history](memory/_memory.md) - #LoadNow #Memory
- [Default shapes to check before creating or reviewing structure](patterns/_patterns.md) - #Core #Pattern
- [Capabilities to check before specialized work](skills/_skills.md) - #Core #Skill
- [Starting files to copy when creating something new](templates/_templates.md) - #Core #Template
