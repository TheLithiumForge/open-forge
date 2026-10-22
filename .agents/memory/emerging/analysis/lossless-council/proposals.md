---
open-forge:
  description: The twelve Stage 1 proposals of the lossless and readable kernel council, kept as evidence
  tags: [Memory, Analysis, Contextual, Candidate, Framework, Council]
---

# Council 3 Proposals

The twelve Stage 1 proposals, each under its perspective and the letter it carried in the anonymous Stage 2 review. Kept as evidence for the record, not as accepted direction.

## Adoption Strategist (Proposal A)

/root - GPT-6 - high

### Thesis

Lead with one promise: **help your agent use the project knowledge you already have**. Keep v2’s reading procedure and direct links, restore obligations that disappeared, and let installation choices determine how much Framework a project receives. The first success should be an agent following one existing document in a fresh session.

### How v2 removed so many rules

The [statement ledger](artifacts/council-3/ledger.md) contains five different kinds of reduction.

1. **Removed vocabulary.** Loader L46 defined `route` and L49 defined `slug`. Removing names that the reading procedure no longer needs is sound.
2. **Removed classification explanations.** Loader L95 defined `#Core` and L96 defined `#Memory`. Their definitions can leave startup, but should remain available to readers interpreting the labels.
3. **Removed authoring controls.** Maps L23 controlled pointer granularity and Templates L30 controlled specialization. These were instructions, not explanations. Assuming competent authors will reproduce them loses their binding strength.
4. **Narrowed shared duties to one category.** Loader L27 survives partly in Crystallized Memory and L100 survives partly as its update rule. Neither preserves duties for ordinary linked documents, other categories, or installations without Memory.
5. **Removed behavioral defaults and timing.** Loader L21 prescribed the initial response and L94 prescribed rereading changed tagged files. These might be debatable defaults, but removing them is a product change rather than lossless editing.

Much of the word reduction also came from shorter definitions and consolidating navigation into five steps. Those changes are sound. “Ordinary authoring practice” and “rarely matters” do not establish redundancy.

### The lossless ledger

Destination paths below are relative to installed `.agents/`. Old filenames and line numbers refer to the shipped files in the ledger. Insert each quoted sentence once. Loader restorations are incorporated into section 8.

**`loader.md`, “Follow rules and accepted direction”**

- `loader.md L21` — **Restore:** “Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.”
- `loader.md L22` — **Restore:** “Use the fewest words that preserve meaning, readability, and logical progression in communication and authored content. Remove filler and repetition. Keep the context, conditions, and exceptions needed to understand and apply the result.”
- `loader.md L40` — **Restore:** “Selected non-binding material may specialize broader material of the same kind within its accepted scope.”

**`loader.md`, “Keep sources trustworthy”**

- `loader.md L27` — **Restore:** “Let each source answer one clear question. Keep detail in the narrowest source that defines it, and link to related sources instead of repeating their detail. A link changes neither source's authority, scope, loading rules nor lifecycle.”
- `loader.md L32` — **Restore:** “Investigate apparent conflicts between accepted current sources before changing either. When accepted direction changes current state, update its defining source and preserve useful prior context.”
- `loader.md L100` — **Restore:** “An #Evergreen source must stay aligned with the accepted current state it represents. The tag adds no authority or loading behavior.”
- `loader.md L101` — **Restore:** “Update affected sources you may edit before other work depends on the outcome and no later than closeout.”
- `loader.md L102` — **Restore:** “Report updates that are blocked or unauthorized.” Alignment and preservation are restored immediately above.
- `memory/_memory.md L27` — **Relocate:** “Update affected sources you may edit before other work depends on the outcome and no later than closeout. Keep useful context and reasoning in Memory when it is enabled. Accepted durable knowledge must reach its defining source rather than remain only in chat or an unaccepted record.”

**`loader.md`, “Read before you work”**

- `loader.md L62` — **Restore:** “Use catalogue descriptions for discovery rather than opening ordinary file bodies just to find routes.”
- `loader.md L85` — **Redundant:** the example follows from surviving rules “Open other entries when their description fits the task” and “A required file inside a catalogue needs #LoadNow on that catalogue too.”
- `loader.md L93` — **Restore:** “Before handoff or closeout, reread #KeepInMind files in active branches and follow their child loading rules.” L62 restores the discovery boundary.
- `loader.md L94` — **Restore:** “During work, reread tagged files when they may have changed.”

**`loader.md`, “Maintain the files”**

- `loader.md L28` — **Restore:** “Use relative links to local sources and precise references. Reuse consistent descriptive tags for shared subjects. Add links and tags only when they help navigation or retrieval.”
- `loader.md L37` — **Restore:** “When authoring a catalogue without local rules, put `- inherited - No local axioms. Loaded ancestor axioms remain active.` under `## Axioms`. Do not combine the marker with local rules. Existing missing or empty sections add no local rules.”
- `loader.md L57` — **Restore:** “Catalogues may nest without a fixed depth limit. Each narrows everything below it.”
- `loader.md L58` — **Restore:** “Include only the categories useful there rather than copying the defaults.”

**`maps/_maps.md`, `## Axioms`**

- `_maps.md L23` — **Restore:** “Keep Maps broad. Prefer modules, projects, repositories, systems or scopes over members and functions unless finer routing clearly helps.”
- `_maps.md L24` — **Restore:** “Each workspace chooses its own Map filenames, grouping and nesting depth.”
- `_maps.md L25` — **Restore:** “Maps do not replace Memory or the destination's current information.”

**`patterns/_patterns.md`, `## Axioms`**

- `_patterns.md L19` — **Restore:** “Create or update a Pattern only when an accepted shape should guide future related work. Structure alone does not justify promoting one-off work, temporary transitions or unsettled candidates into a Pattern.”
- `_patterns.md L25` — **Restore:** “Link related sources for required rules, advice, maps, procedures and starting files, and say which bind the shape. Sources that apply the shape should link back to its Pattern.”
- `_patterns.md L26` — **Restore:** “Keep required behavior, procedures and substantial choice rationale in their defining sources. Example names and values illustrate the shape unless an applicable requirement fixes them.”
- `_patterns.md L28` — **Restore:** “Adapt only within the authority granted for the work. In reviews, report justified adaptations separately from conformance.”

**`templates/_templates.md`, `## Axioms`**

- `_templates.md L24` — **Restore:** “If the new artifact needs continuing guidance or requirements, link the source that defines them. The Template provides only starting content.”
- `_templates.md L28` — **Restore:** “State the need and primary question each Template answers in its description and source instructions.”
- `_templates.md L29` — **Restore:** “Use focused prompts that request necessary content once, in a readable order. Mark optional sections clearly.”
- `_templates.md L30` — **Restore:** “Treat generic Templates as fallbacks. Add a specialized Template only when it provides meaningfully different starting content.”

**`memory/_memory.md`, `## Axioms`**

- `_memory.md L13` — **Restore:** “Memory is self-growing: people and agents may add useful records and routed scopes without a fixed structural limit. Unselected branches do not load their file contents. Records need not pass through every state.”
- `_memory.md L19` — **Restore:** “An accepted record can define current knowledge or decisions. Recording another category's content does not give the record that category's role.”
- `_memory.md L26` — **Restore:** “Distinguish what was stated, observed or inferred.”
- `_memory.md L32` — **Restore:** “Add records and routed scopes when their likely future value justifies the cost of finding and reviewing them.”
- `_memory.md L35` — **Restore:** “Named record categories and Templates are optional. Use an existing state and the narrowest useful scope. Records may live directly under a state.”
- `_memory.md L36` — **Restore:** “Within granted authority, add an ordinary record or scoped route when needed. Ask before placement changes accepted meaning or crosses an unresolved authority boundary.”

**`memory/working/_working.md`, `## Axioms`**

- `_working.md L19` — **Restore:** “An explicitly accepted temporary choice may stay here when its source, scope and expected expiration are clear.”

**`memory/emerging/_emerging.md`, `## Axioms`**

- `_emerging.md L17` — **Restore:** “At each applicable #KeepInMind refresh, check visible Entries and save useful candidate material before it is lost.”
- `_emerging.md L23` — **Restore:** “Refine material while its validity, use or final destination remains unsettled.”
- `_emerging.md L25` — **Restore:** “Revisit repeated or stale material and decide whether to consolidate, promote, archive or prune it. Review may leave a record unchanged.”

**`memory/archived/_archived.md`, `## Axioms`**

- `_archived.md L17` — **Restore:** “Retain, consolidate or transform history according to its future value. Archival need not preserve every detail.”

**`README.md`, “Labels”**

- `loader.md L95` — **Relocate:** “#Core identifies base routing and loading, workspace orientation, and reusable agent-facing roles.”
- `loader.md L96` — **Relocate:** “#Memory identifies self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history.”
- `loader.md L97` — **Relocate:** “#Extension identifies optional packaged routes, capabilities, integrations and support files.”

These classifications confer no behavior. Reading their definitions only when classification matters is therefore appropriate.

**No destination**

- `loader.md L46` — **Accept loss:** the glossary name `route` adds no behavior beyond following Entries.
- `loader.md L47` — **Accept loss:** `root route` becomes the directly visible loader entry.
- `loader.md L48` — **Accept loss:** the procedure explicitly explains narrowing without requiring this glossary definition.
- `loader.md L49` — **Accept loss:** `slug` becomes ordinary “folder name”, which nobody needs translated.

For Emerging L17 to retain its original reach, restore `KeepInMind` in its frontmatter and `#KeepInMind` on its entry in `memory/_memory.md`. Smaller installations can omit Emerging entirely.

The audit must also correct misleading **Kept/Moved** classifications. Add these exact catalogue Axioms: Memory, “Get explicit user direction before creating a new top-level Memory state.” Patterns, “Label required, optional and repeatable parts, their nesting and relationships, and fixed or illustrative names, values and orders.” Templates, “Remove package tags that do not describe the destination.” Archived, “Preserve useful provenance, why the material was archived and what replaced it. Remove metadata that keeps its former behavioral role active or claims current authority.”

### The complexity ladder

These are useful stopping points, not prerequisites. Someone can add Crystallized Memory before Directives.

Let **B** mean `AGENTS.md`, the `CLAUDE.md` bridge, and `.agents/{loader.md,README.md}`. CLI installation also writes settings and ownership JSON.

| Level | Installed tree | What it gives and costs | Move one level |
|---|---|---|---|
| 0. Connect | B, one loader link | Existing knowledge reaches the agent. Cost is the loader and one document, with no new taxonomy | Up: add Directives. Down: remove the managed handoff and trial files |
| 1. Remember a rule | Level 0 plus `directives/{_directives.md,testing.md}` | A recurring correction becomes a required instruction. Cost is maintaining one rule and its scope | Up: add Memory and Crystallized. Down: remove the Directives loader entry, preserving notes |
| 2. Keep accepted knowledge | Level 1 plus `memory/{_memory.md,crystallized/_crystallized.md,crystallized/testing.md}` | Accepted knowledge stays discoverable and current. Cost is acceptance and update discipline | Up: add Working. Down: unlink Memory while retaining authoritative documents |
| 3. Resume work | Level 2 plus `memory/working/{_working.md,saving.md,resume.md}` | Unfinished work survives sessions. Cost is small checkpoints and deliberate sharing choices | Up: select remaining categories and states. Down: remove saving policy and its entry, then unlink Working |
| 4. Full Framework | B plus all six Core catalogues, all four Memory states, saving policy, CLI Skill and four references, plus chosen Extensions | Advice, shapes, starters, capabilities, candidates and history. Cost is maintaining the parts actually used | Down: disable unnecessary entries, then separately release management or delete reviewed files |

Brownfield projects should start at **level 0**. They already possess knowledge and instructions. Their first experiment should prove that Open Forge helps agents use them.

### Ultra-lightweight install

Add **`--preset tiny`**, displayed as **“Tiny: connect existing documents”**. Recommend it for the first interactive installation. Keep existing unattended defaults for compatibility, and use explicit presets in documentation.

On a fresh workspace it writes B, `.agents/open-forge.json` and `.agents/open-forge.lock.json`. The loader has an empty `## Entries`. It installs no category folders, Memory container, saving policy, CLI Skill, Extensions or Git-ignore section. Existing host content remains outside the managed blocks.

Allow `--route` with `tiny` as well as `custom`. Tiny starts with all ten rows omitted, then applies explicit overrides:

`open-forge install --preset tiny --route directives=add --route memory/crystallized=add --dry-run`

Required ancestors are included, siblings remain omitted, and `git-ignore` retains its current meaning. Persist concrete selections through existing exclusions rather than adding a mode setting.

The plan says: “Install Tiny with no optional categories. Existing project documents stay unchanged. Next, add one document under loader Entries.” It lists every physical effect and any retained existing content.

Grow with `route init directives --framework` or `install --configure --preset custom --route memory/crystallized=add`. On existing installations, Tiny releases omitted management without deleting notes or silently disabling existing links.

C# changes belong in `InstallConfiguration.cs`, `InstallConfigurationChoices.cs`, `InstallBindingInputReader.cs`, `InstallBindingModels.cs`, `InstallSetupResolver.cs`, `InstallSetupQuestions.cs`, `InstallSetupRecapSelector.cs`, `InstallReportSelector.cs`, `InstallNextCommand.cs` and the Install text classes. Check minimal ancestor selection in `FrameworkPayloadSelection.cs`. Fix empty membership preservation in `GeneratedNavigationRegionPlanner.cs`.

Cost is a bounded installer and presentation change, focused unit and public CLI journeys, and help/result snapshots. It requires no Markdown grammar or configuration-schema migration.

### Brownfield adoption

The following uses the proposed Tiny preset. Assume an existing source file `src/Orders/Order.cs` and architecture document `docs/architecture.md`, substituting real paths where necessary.

1. Inspect `AGENTS.md`, `CLAUDE.md`, `CONTRIBUTING.md` and `README.md`. Record the existing instructions and authoritative documents. The agent currently reads whatever its host already loads.
2. Run `open-forge install --preset tiny --dry-run`, inspect the plan, then `open-forge install --preset tiny --automatic`. Inspect `git status --short` and `git diff`, and open new files. The agent now reads existing host instructions and the loader.
3. Add under loader Entries: `- [Contribution rules for this project](../CONTRIBUTING.md) - #LoadNow`. A fresh task now reads CONTRIBUTING too.
4. Add `- [Project overview and documentation links](../README.md) - #Project` and `- [Architecture for source changes](../docs/architecture.md) - #LoadNow - applies to ` followed by `` `src/**` ``. The architecture loads for source work. README opens when relevant. Neither document moves or gains frontmatter.
5. Run `open-forge context --for src/Orders/Order.cs`. Ask a fresh agent to explain a proposed change and cite the contribution rule and architecture constraint it used. This is the adoption proof.
6. After an actual recurring correction, preview and apply `open-forge route init directives --framework`. Add its rule and catalogue entry. After accepting genuinely new knowledge, preview and apply `open-forge install --configure --preset custom --route memory/crystallized=add`.

Open Forge must never rewrite existing documentation into its taxonomy, infer acceptance from code, duplicate historical ADRs, alter unrelated host text, or claim user documents as managed payload.

Worth building next is a read-only “suggest existing documents” step that prints candidate entry lines with their reasons. Users choose the links. Automatic document conversion and a new adoption wizard are unnecessary.

### The introduction

This replacement copy targets the release containing Tiny.

````markdown
## Open Forge

Open Forge helps AI coding agents use the project knowledge you already have. Start with one existing document, keep its location and format, and choose exactly what agents should read.

It's a plain Markdown framework that you own. You can use just a reading list, add a few rules, or grow into memory and reusable methods. The CLI is optional.

### Try it in an existing project

With Node.js 22.18 or later, run these commands from your project:

```sh
npm install -g @thelithiumforge/open-forge@beta
open-forge install --preset tiny --dry-run
open-forge install --preset tiny --automatic
```

Review the preview before applying it. Tiny adds the loader and a short project guide, plus managed sections in `AGENTS.md` and `CLAUDE.md`. Your existing instructions stay in place. It also records CLI settings and ownership.

Add this line under `## Entries` in `.agents/loader.md`:

```markdown
- [Contribution rules for this project](../CONTRIBUTING.md) - #LoadNow
```

Use a document your project already has. It needs no new headings or frontmatter.

Now start a fresh agent session:

> Explain how this project runs its tests. Follow AGENTS.md and name the contribution rule you used.

Check the answer against your document. That's the first useful result: existing project knowledge reached a new session without being copied into another system.

### Add one rule when you need it

If the agent keeps repeating a mistake, add Directives:

```sh
open-forge route init directives --framework --dry-run
open-forge route init directives --framework
```

Review the preview first. Create `.agents/directives/testing.md`, using the metadata form selected for your workspace:

```markdown
---
description: Check code changes before calling them done
tags: [LoadNow, Directive, Testing]
---

## Testing

### Instructions

- Run the relevant tests before reporting a code change as done. Include the command and result. If you cannot run them, explain why.
```

Add its entry to `.agents/directives/_directives.md`:

```markdown
- [Check code changes before calling them done](testing.md) - #LoadNow #Directive #Testing
```

You can maintain these links by hand. With the CLI, preview `open-forge index --dry-run`, then run `open-forge index`.

For a rule that applies only to C# files, add `applyTo: ["**/*.cs"]` to its metadata and ` - applies to ` followed by the backticked pattern `**/*.cs` to its entry.

### Keep one accepted fact

Add Crystallized Memory when useful knowledge would otherwise disappear into chat:

```sh
open-forge install --configure --preset custom --route memory/crystallized=add --dry-run
open-forge install --configure --preset custom --route memory/crystallized=add --automatic
```

Then ask:

> Keep the test command we confirmed in Crystallized Memory. If it is already documented, link to that source instead of copying its detail.

Records can live directly under `.agents/memory/crystallized/`. You don't need Planning, a Decision template, or a new documentation hierarchy.

### Choose the amount that helps

| Need | Add |
|---|---|
| Use existing docs | A loader entry |
| Stop repeating a correction | A Directive |
| Keep accepted knowledge current | Crystallized Memory |
| Resume unfinished work | Working Memory and its saving policy |
| Reuse shapes, advice or starting files | Patterns, Guidance or Templates |
| Use a specialized method | A selected Skill or Extension |

Essentials and Full Core remain available. Neither installs optional Extensions. The [installation guide](https://thelithiumforge.github.io/open-forge/docs/getting-started/installation) explains their contents and how to change selections.

You can stop at any row, combine rows, or remove parts later. Removing a loader entry disables that part's Open Forge rules while keeping its notes. Existing accepted project requirements and instructions your host loads independently still apply.

Everything works without the CLI. Copy the loader and project guide from [the Framework files](src/open-forge/), append the managed handoff to your agent instructions, and keep only the Entries you want. Merge with an existing `.agents/` folder deliberately.

Open Forge supplies instructions and navigation, not an agent runtime. Your agent tool must read `AGENTS.md` or the `CLAUDE.md` bridge, and agents can still miss instructions. Review their work as usual.
````

Complete `.agents/README.md`:

````markdown
## Open Forge in this project

This folder tells agents where to find the rules and knowledge this project has chosen to use. Start with `loader.md`. Its Entries list enables the linked parts.

Everything is plain Markdown. The CLI makes retrieval and maintenance faster, but the files work without it. This guide is for people and is not a startup read.

### Connect an existing document

Add a line under `## Entries` in `loader.md`:

```markdown
- [Contribution rules](../CONTRIBUTING.md) - #LoadNow
- [Architecture for C# work](../docs/architecture.md) - #LoadNow - applies to `**/*.cs`
- [Project overview](../README.md) - #Project
```

Use paths that exist in your project. Links are relative to the loader.

`#LoadNow` requires reading when the parent loads. A condition limits reading and application to matching working files. Untagged entries open when their descriptions fit the task.

Existing documents stay where they are and need no new frontmatter. Linking a document does not accept its contents, grant edit permission or make the CLI manage it.

### Choose your parts

A reading list is a complete useful setup. Add optional folders only when you need their behavior.

Directives hold required instructions. Crystallized Memory holds accepted knowledge. Working Memory holds resume notes. Other categories provide advice, shapes, capabilities, starting files and curated pointers.

You can use any suitable combination. No category requires the full Framework.

With the CLI, use `install --configure --preset custom` to change selections. Its `remove` row releases management but keeps existing files routable. Disabling behavior is a separate edit to loader Entries.

Without the CLI, copy the selected catalogue and any required ancestors from the Framework files, then link them yourself.

### Add a rule

When Directives is enabled, create a file under `directives/` with description and tags in frontmatter. Put required behavior under `## Instructions` and include `LoadNow` in its tags.

Use `applyTo` for file-specific rules. Metadata follows `frontmatter` in `open-forge.json`, using root fields or an `open-forge:` mapping. A missing setting means scoped.

Add the file to its catalogue Entries. The CLI can maintain navigation with `index`, after you review its dry run.

### Keep useful knowledge

Reuse an existing record when one fits. Record accepted knowledge in Crystallized Memory, or link to its existing authoritative document. Keep unaccepted findings separate.

Records may live directly under a Memory state. Named categories and Extensions are optional. Memory grows deliberately through useful records rather than recording every conversation.

Working Memory's `saving.md` asks agents to save unfinished progress before pausing. Emerging Memory has its own candidate-capture rule when enabled.

To stop automatic progress saving, remove `memory/working/saving.md` and its entry in `_working.md`. Keep the working records.

For CLI-managed removal, preview `open-forge remove .agents/memory/working/saving.md --dry-run`, then apply the reviewed request without `--dry-run`.

### Turn a part off

Remove its entry from `loader.md`. Its files remain notes. Searching or reading them for background does not reactivate their Open Forge rules.

Check dependencies first. Accepted project requirements and independently loaded host instructions still apply.

Inside another catalogue, Entries follow the folder's files. Move an unwanted note outside that enabled tree, or remove it deliberately.

For managed updates, record an omitted category in `removedCategories`, or an omitted file in `removedFiles`, in `open-forge.json`. Preserve the other settings. These exclusions prevent restoration and do not themselves delete notes.

An empty loader Entries list is valid.

### Customize shipped text

You may edit, replace or remove defaults. Ordinary CLI Update can replace directly edited managed files.

For a local adjustment that survives updates, create `{name}.overwrite.md` beside the base. It loads immediately afterward, shares its scope and wins only for corresponding content in that source. It does not override unrelated rules.

Preview updates before applying them.

### Labels

These labels classify content and do not grant authority or change loading.

- #Core identifies base routing and loading, workspace orientation, and reusable agent-facing roles.
- #Memory identifies self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history.
- #Extension identifies optional packaged routes, capabilities, integrations and support files.
````

### v3

This is the Full Core loader. Installation projects its Entries to the selected files. Tiny receives an empty Entries section.

```markdown
## Open Forge Loader

This file tells agents which project context to use and when to read it. It adds to the project's other instructions. Everything works with plain files.

Each line under `Entries` enables one file or catalogue. A catalogue is a Markdown file that lists other files under its own `Entries`. Unlisted parts are off.

### Axioms

### Read before you work

1. At task start or resume, and after context restoration, identify your working files. Include files you will investigate, review, create, change, rename or delete, including planned files and both rename paths. Reading a file for background does not make it a working file.
2. If the CLI is available and allowed, run `open-forge context`, repeating `--for <file>` for every known working file, and read its output. This performs steps 3 and 4 for the returned files. You must still perform step 5. If the command is unavailable, fails or cannot follow a required link, use your file tools for steps 3 to 5.
3. Starting with the Entries below, read files tagged #LoadNow or #KeepInMind in listed order. Repeat inside each catalogue you read. Do not skip a required file because its description seems unrelated. Read each file once per pass, including its overwrite immediately afterward.
4. Apply an entry's `applies to` conditions only to matching working files. Patterns within one list are alternatives. The same working file must match a pattern at every conditioned level of its catalogue chain. Patterns are case-sensitive globs from the workspace root, such as `**/*.cs` or `docs/**`. If working files are unknown, keep required conditioned entries pending. Resolve them before working on a matching file. A match does not select an unopened catalogue or grant edit permission.
5. Open other entries when their descriptions fit the task. Follow explicit required-read links in everything you read. Before using a listed file found through search or a direct link, read its catalogue chain. Follow separately selected chains separately. Unlisted parts remain background even when search finds them. Use catalogue descriptions for discovery rather than opening ordinary file bodies just to find routes.

Repeat from step 1 when your goal or working files change. Before handoff or closeout, reread #KeepInMind files in active branches and follow their child loading rules. During work, reread tagged files when they may have changed.

### Follow rules and accepted direction

- Follow this loader's Axioms, loaded catalogues' `## Axioms`, and rule files' `## Instructions` within their scope. Catalogue rules cover all listed descendants. Narrower rules add to broader rules and cannot cancel them. Report unresolved conflicts and instructions you cannot follow.
- Follow user direction within platform and safety limits. Take routine, reversible steps within the request without asking again. State assumptions that affect the result. Ask before relying on an unresolved choice that significantly changes the result, scope, risk, cost, external effects or ability to undo the work.
- Reading, linking, saving, moving or tagging content does not establish acceptance or edit permission. Acceptance comes from the user or an authority they declared for that question. Use declared external authorities for the facts assigned to them. Tags, repetition and agent confidence do not establish acceptance.
- Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.
- Use the fewest words that preserve meaning, readability, and logical progression in communication and authored content. Remove filler and repetition. Keep the context, conditions, and exceptions needed to understand and apply the result.
- Selected non-binding material may specialize broader material of the same kind within its accepted scope.

### Keep sources trustworthy

- Let each source answer one clear question. Keep detail in the narrowest source that defines it, and link to related sources instead of repeating their detail. A link changes neither source's authority, scope, loading rules nor lifecycle.
- Investigate apparent conflicts between accepted current sources before changing either. When accepted direction changes current state, update its defining source and preserve useful prior context.
- An #Evergreen source must stay aligned with the accepted current state it represents. The tag adds no authority or loading behavior.
- Update affected sources you may edit before other work depends on the outcome and no later than closeout. Keep useful context and reasoning in Memory when it is enabled. Accepted durable knowledge must reach its defining source rather than remain only in chat or an unaccepted record. Report updates that are blocked or unauthorized.

### Choose what is enabled

- Add a line under this loader's `Entries` to enable a catalogue or a Markdown file anywhere in the workspace. For example, `../docs/architecture.md` keeps an existing document in place. It needs no new frontmatter. Its entry supplies the description, tags and conditions.
- Remove a line to disable that part's Open Forge rules while keeping its files as notes. This does not cancel accepted project requirements or instructions the host loads independently. Report dependencies from other enabled parts before disabling it. Do not recreate disabled parts unless the user asks.
- Inside other catalogues, Entries follow the files in their folder. To remove an entry there, move or delete its file. For CLI-managed files, also record intentional removals in the manager's exclusions before updating. The CLI Skill explains this.
- Report a missing listed file as an error. An empty Entries list is valid and must stay empty during maintenance unless the user enables something.
- Use #LoadNow only when a file is needed whenever its parent loads and missing it would cost more than reading it each time. Use #KeepInMind when it also needs the refreshes above. Every catalogue between an automatically required file and this loader needs #LoadNow or #KeepInMind with appropriate conditions. Other tags do not change loading or establish authority. #Contextual marks unaccepted context and #CurrentTruth marks accepted current state within its scope. Read the [label definitions](README.md#labels) when their classification matters.

### Maintain the files

- Catalogues may nest without a fixed depth limit. Each narrows everything below it. Include only the categories useful there rather than copying the defaults. Place specialized content in the narrowest useful scope. Do not mark a broad parent #LoadNow merely to expose an important descendant.
- Use relative links to local sources and precise references. Reuse consistent descriptive tags for shared subjects. Add links and tags only when they help navigation or retrieval.
- New routed files inside `.agents/` use frontmatter with `description`, `tags`, and optional `applyTo`. Follow `frontmatter` in `.agents/open-forge.json`, using root fields or an `open-forge:` mapping. A missing setting means scoped. Native `SKILL.md` keeps its native metadata.
- Each routed folder has one catalogue named `_{folder-name}.md`. Existing `index.md`, `_index.md`, `references.md` and `_references.md` remain compatible. When authoring a catalogue without local rules, put `- inherited - No local axioms. Loaded ancestor axioms remain active.` under `## Axioms`. Do not combine the marker with local rules. Existing missing or empty sections add no local rules. Do not repeat inherited rules.
- You may edit, replace or remove defaults. For changes that survive CLI updates, put an adjacent `{name}.overwrite.md` beside the base. Read it as part of that source, with the same scope and loading behavior. It wins only for corresponding content in that source, never unrelated sources. Do not index or select it separately.
- The optional CLI can maintain navigation with `open-forge index`. Read the installed CLI Skill before using commands other than `context`. If it was intentionally omitted, use command help and these file rules.

### Entries

- [Rules agents must follow in this workspace](directives/_directives.md) - #LoadNow #Core #Directive
- [Advice for recurring choices, to check before choosing an approach](guidance/_guidance.md) - #Core #Guidance
- [Where to find project knowledge, checked before relying on it](maps/_maps.md) - #Core #Map
- [Notes that help work continue, such as resume points, accepted knowledge, ideas and history](memory/_memory.md) - #LoadNow #Memory
- [Default shapes to check before creating or reviewing structure](patterns/_patterns.md) - #Core #Pattern
- [Capabilities to check before specialized work](skills/_skills.md) - #Core #Skill
- [Starting files to copy when creating something new](templates/_templates.md) - #Core #Template
```

Managed `AGENTS.md` block:

```markdown
## Open Forge

Before starting a task, read `.agents/loader.md` and follow its reading steps. It tells you which project rules and knowledge to use.

If the CLI is available and allowed, `open-forge context --for <file>` gathers the required reads. Repeat `--for` for every working file. The loader explains what else you must read and how to continue without the CLI.

**End of Open Forge managed section.**
```

Five consequential sentence changes:

| Before | After | Why |
|---|---|---|
| “The `Entries` below are the switch: what is listed is on, and anything not listed is off.” | “Each line under `Entries` enables one file or catalogue.” | Names the action and introduces catalogue immediately |
| “Read and apply an entry with `applies to` patterns only when the same working file matches its patterns and those of every conditioned catalogue above it.” | “Patterns within one list are alternatives. The same working file must match a pattern at every conditioned level of its catalogue chain.” | Separates alternatives from inherited conditions |
| “Accepted means the user agreed, or a source the user named says so.” | “Acceptance comes from the user or an authority they declared for that question.” | Naming a source alone does not delegate every question to it |
| “Every other tag is a search label.” | “Other tags do not change loading or establish authority.” | Allows restored maintenance duties without inventing CLI behavior |
| “New files inside `.agents/` start with frontmatter … in the same form as the files around them.” | “Follow `frontmatter` in `.agents/open-forge.json`, using root fields or an `open-forge:` mapping.” | Makes manual authoring deterministic |

Whitespace-separated counts, including headings and Entries: **loader 1,349 words**, **managed block 66 words**. The loader is longer than v2’s recorded 823 words because restored duties have an explicit cost.

### Risks and proof

The main risk is offering a tiny file tree while retaining a substantial shared rule set. Lossless does not mean effortless. Test whether the five-step opening is understandable before trying another compression pass.

Other risks are confusing management removal with disabling, hiding accepted knowledge behind an optional catalogue, and restoring Emerging capture without restoring its loading tag. Documentation must update together, especially the current startup tables, “19 files” inventory and CLI Skill startup claims.

The cheapest useful experiment is three developers unfamiliar with Open Forge, each using an existing repository. Give them only the replacement introduction. Within five minutes, each should connect a document, obtain an agent response citing its real contents, explain what changed, and identify how to turn it off. At least one should work without the CLI.

Then ask each to add one scoped rule and disable one part. Check matching and nonmatching tasks, and verify that Index and Update preserve the disabled choice. Any lost host text, unexpected document rewrite or resurrected empty Entries list fails the experiment.

Before shipping, pin Tiny preview/apply parity, repeat no-op, row overrides, ancestor-only restoration, existing-content preservation and empty-list maintenance through the public CLI. This proposal is based on read-only source inspection. No files, builds, tests or Doctor runs were performed.

### The one idea to steal

Make the first installation an editable reading list for one document the project already trusts.

---

## Unix Minimalist (Proposal I)

/root - GPT-6 - high

### Thesis

Keep v2’s reading procedure and make the smallest useful setup two files that point to one document the project already has. Restore missing behavior where it is needed, with optional policies carrying preferences and feature catalogues carrying their own rules. Growth should add one useful capability at a time, and removing it should preserve the user’s documents and choices.

### How v2 removed so many rules

The [statement ledger](artifacts/council-3/ledger.md) contains several different operations under “simplification”.

| Kind | Two examples | Verdict |
| --- | --- | --- |
| Consolidating repeated rules | Directives L21 moved inheritance into the loader. Templates L20 moved metadata requirements there. | Sound when the surviving text preserves every condition. |
| Moving tool instructions | Loader L130 moved `--help` into the CLI Skill. L132 moved `route list` there. | Sound for CLI users, provided an omitted Skill leaves command help available. |
| Removing vocabulary | Loader L46 removed `route`. L49 removed `slug`. | Sound for startup reading. Keep definitions available to people reading CLI help. |
| Replacing unconditional discovery with relevance | Guidance and Maps lost #LoadNow. | Sound when their loader descriptions still tell agents when to open them. Their feature rules remain binding once selected. |
| Treating binding authoring rules as optional advice | Patterns L19 lost its acceptance threshold. Templates L24 lost links to continuing requirements. | Unsound. These affect what future work may rely on. Restore them in their catalogues. |
| Compressing general duties into Memory | Loader L32 lost “investigate first”. L101 lost the update deadline. | Unsound. Existing documents need these protections even without Memory. |

The word reduction therefore mixes legitimate compression, narrower discovery, and actual behavioral loss. “A capable model probably does this” does not establish redundancy.

### The lossless ledger

All destinations below are relative to `src/open-forge/`. Retain v2 text and add the quoted text under the stated section unless the complete replacement in **v3** already includes it.

New `communication.md` and `authoring.md` use scoped frontmatter with `description` and `tags: [LoadNow]`, followed by a title and `## Instructions`. Full Core includes both. Existing installations retain previously enabled obligations through these files during migration. Selecting the new minimum explicitly omits these preferences. Neither policy requires a category.

**`.agents/loader.md`, `## Axioms`**

- `loader.md L27` — **Restore**, under `Apply the context`: “Let each source answer one clear question. Link to related sources instead of repeating their detail. A link does not change either source's authority, scope, loading rules or lifecycle.”
- `loader.md L32` — **Restore**, under `Apply the context`: “Investigate apparent conflicts between accepted current sources before changing either. When accepted direction changes current state, update its defining source and preserve useful prior context.”
- `loader.md L40` — **Restore**, under `Apply the context`: “Selected non-binding material may specialize broader material of the same kind within its accepted scope.”
- `loader.md L62` — **Restore**, reading step 5: “Use catalogues for navigation. Do not open ordinary file bodies just to discover entries.”
- `loader.md L93` — **Restore**, after the reading steps: “Reread #KeepInMind entries before handoff or finish, only in active branches.” Also: “Discover tagged files through catalogues.”
- `loader.md L94` — **Restore**, after the reading steps: “During work, recheck them only when they may have changed.”
- `loader.md L100` — **Restore**, under `Apply the context`: “#Evergreen means a source must stay aligned with accepted current state. It grants neither authority nor loading behavior.”
- `loader.md L101` — **Restore**, under `Apply the context`: “Update affected sources you may edit before work depends on them and by closeout. Batch related updates when safe.”
- `loader.md L102` — **Restore**, under `Apply the context`: “Keep each source aligned with what it represents now. Preserve useful prior context in existing history or Memory when enabled. Report updates that are blocked or unauthorized.”

These remain in the kernel because directly linked brownfield documents need them. Preserving history does not require installing Memory.

**`.agents/communication.md`, `## Instructions`**

- `loader.md L21` — **Relocate**: “Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.”
- `loader.md L22` — **Relocate**: “Use the fewest words that preserve meaning, readability, and logical progression in communication and authored content. Remove filler and repetition. Keep the context, conditions, and exceptions needed to understand and apply the result.”

**`.agents/authoring.md`, `## Instructions`**

- `loader.md L28` — **Relocate**: “Connect related material with relative links to local sources and precise references. Reuse consistent descriptive tags for shared subjects so tag and reference searches can find those connections. Add links and tags only when they help navigation or retrieval.”
- `loader.md L37` — **Relocate**: “For a catalogue without local rules, use `## Axioms` with only `- inherited - No local axioms. Loaded ancestor axioms remain active.` Missing or empty existing sections add no rules. Never combine the marker with local rules.”
- `loader.md L57` — **Relocate**: “Folders may narrow a route at any depth, including before, between or after category segments. Each scope narrows everything below it.”
- `loader.md L58` — **Relocate**: “A scope contains only the routes useful there. It need not copy another scope or the installed defaults.”

**`.agents/README.md`, `## Reference`**

- `loader.md L46` — **Relocate**: “A route is a navigable path exposed through catalogues and Entries.”
- `loader.md L47` — **Relocate**: “A root route is exposed directly by the loader.”
- `loader.md L48` — **Relocate**: “A scope narrows where the following content applies.”
- `loader.md L49` — **Relocate**: “A slug is a concrete folder name in a route.”
- `loader.md L85` — **Relocate**: “Selecting a C# catalogue reads its #LoadNow design file. An unselected Windows catalogue beneath it stays closed, including its #LoadNow children.”
- `loader.md L95` — **Relocate**: “#Core labels routing, loading, workspace orientation and reusable content roles.”
- `loader.md L96` — **Relocate**: “#Memory labels self-growing Markdown state for work, coordination, accepted knowledge, candidates and history.”
- `loader.md L97` — **Relocate**: “#Extension labels optional packaged routes, capabilities, integrations and support files.”

These definitions create no additional loading duty. The procedure preserves their operational meaning.

**`.agents/maps/_maps.md`, `## Axioms`**

- `maps L23` — **Restore**: “Keep Maps broad. Prefer modules, projects, repositories, systems or scopes over members and functions unless finer routing clearly helps.”
- `maps L24` — **Restore**: “Each workspace chooses its own Map filenames, grouping and nesting depth.”
- `maps L25` — **Restore**: “Maps do not replace Memory or the destination's current information.”

**`.agents/patterns/_patterns.md`, `## Axioms`**

- `patterns L19` — **Restore**: “Create or update a Pattern only when an accepted shape should guide future related work. Structure alone does not justify promoting one-off work, temporary transitions or unsettled candidates into a Pattern.”
- `patterns L25` — **Restore**: “Link related sources for required rules, advice, maps, procedures and starting files, and say which of them bind the shape. Sources that apply the shape should link back to its Pattern.”
- `patterns L26` — **Restore**: “Keep required behavior, procedures and substantial choice rationale in their defining sources. Example names and values illustrate the shape unless an applicable requirement fixes them.”
- `patterns L28` — **Restore**: “A justified adaptation remains a departure. In review, report it separately from conformance.”

**`.agents/templates/_templates.md`, `## Axioms`**

- `templates L24` — **Restore**: “If the new artifact needs continuing guidance or requirements, link the source that defines them. The Template provides only starting content.”
- `templates L28` — **Restore**: “State the need and primary question each Template answers in its description and source instructions.”
- `templates L29` — **Restore**: “Use focused prompts that request necessary content once, in a readable order. Mark optional sections clearly.”
- `templates L30` — **Restore**: “Treat generic Templates as fallbacks. Add a specialized Template only when it provides meaningfully different starting content.”

**`.agents/memory/_memory.md`**

- `memory L13` — **Restore**, opening paragraph: “Memory is self-growing. People and agents may add useful records and routed scopes without a fixed structural limit. Unselected branches do not load their file contents. Records need not pass through every state.”
- `memory L19` — **Restore**, `Axioms`: “An accepted record can define current knowledge or decisions. Recording another category's content does not give the record that category's role.”
- `memory L26` — **Restore**, `Axioms`: “Distinguish what was stated, observed or inferred.”
- `memory L27` — **Restore**, `Axioms`: “Update affected sources before other work depends on the outcome and no later than closeout. Keep useful context and reasoning in Memory. Accepted durable knowledge must reach its defining source, not remain only in chat or an unaccepted record.”
- `memory L32` — **Restore**, `Axioms`: “Add records and routed scopes when their likely future value justifies the cost of finding and reviewing them.”
- `memory L35` — **Restore**, `Axioms`: “Named record categories and Templates are optional. Use an existing state and the narrowest useful scope. A record may live directly under that state.”
- `memory L36` — **Restore**, `Axioms`: “Within the authority granted for the work, add an ordinary record or scoped route when needed. Ask when its placement would change accepted meaning or cross an unresolved authority boundary.”

**`.agents/memory/working/_working.md`, `## Axioms`**

- `working L19` — **Restore**: “An explicitly accepted temporary choice may remain here when its source, scope and expected expiration are clear.”

**`.agents/memory/emerging/_emerging.md`, `## Axioms`**

- `emerging L17` — **Restore**: “At each applicable #KeepInMind refresh, check visible Entries and save useful candidate material before it is lost.”
- `emerging L23` — **Restore**: “Refine material while its validity, use or final destination remains unsettled.”
- `emerging L25` — **Restore**: “Revisit repeated or stale material and decide whether to consolidate, promote, archive or prune it. Review may leave a record unchanged.”

**`.agents/memory/archived/_archived.md`, `## Axioms`**

- `archived L17` — **Restore**: “Retain, consolidate or transform history according to its future value. Archival need not preserve every detail.”

All 34 Dropped and 12 Partly rows have a disposition. None requires accepting a silent loss.

The ledger’s Kept and Moved labels also need corrections. Add “Preserve required structure and remove package tags that do not describe the destination” to Templates’ Axioms. Restore removal of metadata claiming current authority in Archived, alongside loading tags, and allow deletion under accepted retention preferences. Restore explicit user direction for new top-level Memory states. Patterns must again identify optional and repeatable parts, relationships, fixed values and schematic omissions. The v3 loader also restores declared external authority, consequential external effects, reversibility and narrower defining sources.

### The complexity ladder

Let **K** mean `AGENTS.md` and `.agents/loader.md`. These are examples of useful endpoints, not prerequisite bundles.

| Level | Installed tree | Benefit and cost | Move up or down |
| --- | --- | --- | --- |
| 1. Connect existing knowledge | K, with an entry pointing to existing `CONTRIBUTING.md` | Required reading without duplicated documentation. Maintain one link and its condition. | Add one capability. Remove the entry to return to an empty, valid loader. |
| 2. Add one capability | K plus `directives/{_directives.md,testing.md}`, **or** `memory/{_memory.md,crystallized/{_crystallized.md,testing.md}}` | A recurring rule, or accepted knowledge with maintenance rules. Maintain the chosen content and navigation. | Add continuity when needed. Remove the loader branch to retain ordinary notes. |
| 3. Preserve unfinished work | K plus `memory/{_memory.md,working/{_working.md,saving.md,resume.md}}` | Resume from recorded state. Pay for deliberate capture and occasional retirement. | Add other useful categories. Remove `saving.md` and its entry to retain records without automatic saving. |
| 4. Use the full framework | K, human README, the two optional policies, six Core category catalogues, Memory and four state catalogues, saving policy, CLI Skill and four references, optional Claude bridge | Explicit roles and reusable methods. More navigation and maintenance. | Add selected Extensions with declared dependencies. Disable unused loader branches before removing their scaffolding. |

Each endpoint works without the CLI. Directives needs no Memory. Crystallized needs its small Memory parent, with only Crystallized listed. Working needs neither Crystallized nor Emerging. Its saving policy is independently removable. Guidance, Maps, Patterns, Templates and Skills each work with K and their own catalogue. Extension dependencies remain explicit package costs.

For a nested item, moving its files outside the indexed folder and updating navigation preserves them as notes. CLI-managed removal also needs the recorded exclusion. Custom’s current `remove` action releases management while retaining routability, so it must never be presented as disabling behavior.

Brownfield projects should begin at level 1. Their existing documents already contain useful answers.

### Ultra-lightweight install

Add **`--preset minimal`**. This is proposed syntax, absent from v2.

On a fresh repository it writes only:

- The managed block in `AGENTS.md`.
- The v3 loader with an empty `Entries` section.

It writes no catalogue, README, policy, Memory, Skill, Claude bridge, settings, ownership record or Git-ignore rule. Existing host text is preserved. An unrelated existing loader is preserved and reported for manual reconciliation. Repeating installation preserves authored entries.

Allow existing `--route <id>=<action>` syntax with `minimal`. Its base selection is empty. `add` installs only the requested route and required ancestors. `git-ignore` also establishes the existing sharing policy. These managed selections write `.agents/open-forge.json` and `.agents/open-forge.lock.json`, plus `.gitignore` when requested. The plan names that transition. `remove` retains its existing management-release meaning.

The seed plan says: “Add the Open Forge section to AGENTS.md and create .agents/loader.md. No optional content or lifecycle records will be installed.” A route selection adds its exact files and administrative records to that sentence.

Growth uses the same preset with explicit routes, or existing `route init <category> --framework` after management is established. Existing presets and unattended defaults retain their current behavior. Existing managed installations retain provenance when reducing active content.

The main changes are under `src/cli/operations/OpenForge.Cli.Operations/Commands/Install/`:

- `Models/Configuration/InstallConfiguration.cs`
- `Shared/Binding/InstallBindingInputReader.cs`
- `Shared/Configuration/InstallConfigurationChoices.cs`
- `Shared/Configuration/InstallSetupResolver.cs`
- `Shared/Configuration/InstallConfigurationIntendedStateBuilder.cs`
- `Shared/Planning/InstallIntendedStateBuilder.cs`
- `Shared/Planning/InstallEstablishmentPlanner.cs`

Also change `InstallSetupQuestions.cs`, `InstallHelpSections.cs`, `InstallSetupText.cs` and recap selectors.

This is a medium lifecycle change, not just another enum value. Reuse the existing planner and mutation safeguards, with different asset and ownership selections. No new persisted preset identity or Markdown grammar is needed. Cost includes focused selection, preservation, promotion, repeat-install and public command tests, plus snapshots. Marker punctuation also affects `RouteScaffoldComposer.cs`, `WorkspaceAdoptionEntrypointComposer.cs` and its inspection profile.

### Brownfield adoption

This walkthrough uses the proposed v3 preset.

1. Inspect the existing instructions and imports. Run `git status --short`, `git diff`, and `rg --files docs`. Read `README.md`, `CONTRIBUTING.md` and whichever host files exist. Nothing new loads yet.

2. Run `open-forge install --preset minimal --dry-run`, review its two-file plan, then run `open-forge install --preset minimal`. The agent now reads the existing host instructions and loader. If Claude starts only from `CLAUDE.md`, add `@AGENTS.md` there once after checking that it creates no import cycle.

3. Put these exact lines under the loader’s `Entries`:

   ```markdown
   - [Contribution rules to follow](../CONTRIBUTING.md) - #LoadNow
   - [Project overview and documentation links](../README.md) - #Project
   ```

   Startup now includes CONTRIBUTING. README opens when its description fits. Both remain in place without metadata changes.

4. Select an actual Markdown document found under `docs/`. For example, if `docs/architecture.md` exists, add:

   ```markdown
   - [Architecture for source changes](../docs/architecture.md) - #LoadNow - applies to `src/**`
   ```

   Source work now reads that document. Other tasks leave it closed. Existing host instructions still apply independently.

5. Run `open-forge context --for src/Order.cs`, substituting a real or planned working path. Compare the returned set with the expected documents. Ask a fresh agent to explain a relevant convention and identify its source.

6. When a recurring correction earns a new rule, preview and apply:

   ```sh
   open-forge install --configure --preset minimal --route directives=add --frontmatter root --dry-run
   open-forge install --configure --preset minimal --route directives=add --frontmatter root
   ```

   Create the rule with `## Instructions` and `tags: [LoadNow, Directive]`, then preview and run `open-forge index`. Startup now includes Directives and the rule, alongside existing links. Add `memory/crystallized=add` only when new accepted knowledge needs a maintained home.

Open Forge must never relocate existing docs, rewrite their meaning, infer acceptance from code, replace unrelated instructions, adopt every nearby file, manufacture historical Decisions, or install Extensions silently.

Useful CLI help is a small “connect an existing document” example and a plan showing the resulting reading chain. An automatic repository reorganization wizard is unnecessary.

### The introduction

This replacement targets the v3 release after `minimal` exists.

````markdown
## Open Forge

Open Forge helps coding agents find and use the rules, docs and decisions your project already has. It is plain Markdown: a starting instruction, a list of useful links, and optional pieces you can add when they earn their place.

You can begin with two files. Your documentation stays where it is.

### Start with an existing document

With Node.js 22.18 or later:

```sh
npm install -g @thelithiumforge/open-forge@beta
open-forge install --preset minimal --dry-run
open-forge install --preset minimal
```

Review the preview before applying it. Minimal adds an Open Forge section to `AGENTS.md` and creates `.agents/loader.md`. Existing project instructions stay in place.

Add this under `## Entries` in the loader:

```markdown
- [Contribution rules to follow](../CONTRIBUTING.md) - #LoadNow
```

Use a document your project actually has. `#LoadNow` asks the agent to read it before working. Without a loading tag, the agent opens it when its description fits the task.

Now try a normal request:

> Explain how this project runs its tests. Follow AGENTS.md and identify the documents you used.

Check the answer against your project's instructions. Open Forge makes those instructions easier to reach. Your agent still needs review.

### Add one useful capability

If a correction keeps recurring, add Directives. If accepted knowledge needs a maintained home, add Crystallized Memory. You can use either independently.

For example, preview and install both:

```sh
open-forge install --configure --preset minimal --route directives=add --route memory/crystallized=add --frontmatter root --dry-run
open-forge install --configure --preset minimal --route directives=add --route memory/crystallized=add --frontmatter root
```

The plan also shows the settings and ownership records used for managed updates.

A rule can be this small:

```markdown
---
description: Report how code changes were checked
tags: [LoadNow, Directive, Testing]
---

## Check code changes

### Instructions

- Run the relevant project checks before reporting a code change as complete.
- Report the command and result, or explain why a check could not run.
```

Save it as `.agents/directives/testing.md` and add its entry to `_directives.md`. The optional CLI can maintain the entry for you with `open-forge index`, after reviewing `--dry-run`.

Accepted knowledge can live directly under `memory/crystallized/`. Named records and Templates are optional. When the answer already lives elsewhere, link to that source.

### Keep unfinished work across sessions

Add Working Memory when restarting a task costs you context.

```sh
open-forge route init memory/working --framework --dry-run
open-forge route init memory/working --framework
```

Review any reported ancestor exclusion before continuing. The command adds only the requested state and required ancestors.

Working records hold the goal, current state, next step and open questions. Its separate `saving.md` policy asks agents to update the record before pausing. Remove that policy and its entry when you want records without automatic saving.

### Choose your own size

The two-file setup is a supported endpoint. You can also choose Essentials, select individual routes with Custom, or install Full Core.

Full Core adds advice, pointers, reusable shapes, starting files, capabilities and all four Memory states. Extensions supply optional methods such as Planning and Development. Their dependencies appear in the install plan.

These are choices about files and behavior. You do not need a workflow, a specification or the complete folder structure to begin.

To disable an Open Forge branch, remove its loader entry. Its files remain readable notes. Accepted project requirements and instructions your agent tool loads independently still apply. Managed removals also need a recorded exclusion so updates preserve your choice.

### Without the CLI

Copy the managed section from `src/open-forge/AGENTS.md` and the loader from `src/open-forge/.agents/loader.md`. Keep an empty `Entries` section, then add your chosen links.

Append the managed section when `AGENTS.md` already exists. If your tool starts from `CLAUDE.md`, make sure it reads `AGENTS.md` without creating an import cycle.

Maintain links with your editor. Every instruction needed to use the framework lives in the files you select.
````

Complete `.agents/README.md`:

````markdown
## Open Forge in this project

Start with `loader.md`. Its Entries list selects this workspace's Open Forge context. Everything here is plain Markdown you can inspect, adapt and remove. The CLI is optional.

This guide is for people. It is not a required startup read.

### Connect an existing document

Add a line under the loader's `## Entries`:

```markdown
- [Contribution rules to follow](../CONTRIBUTING.md) - #LoadNow
- [C# conventions](../docs/contributing/csharp.md) - #LoadNow - applies to `**/*.cs`
```

The destination stays where it is and needs no frontmatter. Use paths relative to the loader.

A loading tag requires reading when its parent loads. Conditions limit it to matching working files. Without a loading tag, the description tells the agent when to open it.

Every catalogue above a required startup file must also be on the required reading chain.

### Add only the pieces you need

Directives holds required behavior. Crystallized Memory holds accepted knowledge. Working Memory holds unfinished work. Each can be your first addition.

The Memory parent lists only the states you selected. Installing one state does not require the others.

Guidance offers advice, Patterns shows reusable shapes, Templates supplies starting files, Maps points to sources, and Skills exposes capabilities. Extensions add optional methods with declared dependencies.

### Write a rule

Use `## Instructions` for the required behavior, conditions and exceptions. Give a Directive `LoadNow`, and `applyTo` when it applies only to matching files.

New routed Markdown uses description and tags in frontmatter. Follow `.agents/open-forge.json`. Without a declared preference, use:

```yaml
---
open-forge:
  description: When this file is useful
  tags: [Directive, LoadNow]
  applyTo: ["**/*.cs"]
---
```

Native Skills keep their own metadata. Existing directly linked documents need no metadata changes.

Maintain catalogue Entries manually, or preview and run `open-forge index`.

### Turn a part off

Remove its loader entry to disable the branch and keep its files as notes. Background reading does not reactivate its rules.

Accepted project requirements and independently loaded host instructions remain in force. Check enabled dependents first.

Other catalogues follow their folder contents. Move or delete an item and update its entry to remove it there. For managed removals, also record the exclusion.

Custom's `remove` choice releases management and leaves existing files routable. It does not disable their behavior.

### Keep work across sessions

Working records describe the goal, current state, next step and open questions.

Its optional `saving.md` policy asks agents to update records before pausing unfinished work. To stop automatic saving, remove both that file and its entry. Keep the records. Record a managed removal before updating.

### Customize shipped content

Edit or replace a source, or put lasting adjustments in its adjacent `{name}.overwrite.md`.

The overwrite loads immediately after its base and wins only for corresponding content in that source. Updates preserve it. Ordinary Update can replace direct edits, so inspect its preview.

### Use the CLI when helpful

`open-forge context --for <file>` gathers required reads. Repeat `--for` for every working file, including planned files. The agent still selects relevant optional context and follows explicit reading requirements.

Read the installed CLI Skill before other commands. When it is absent, use command help. Preview mutations with `--dry-run`.

The minimal two-file seed has no lifecycle records. Managed route installation adds settings and ownership records, named in its plan.

### Reference

A catalogue is the Markdown file listing a folder's content. The CLI also calls it an entrypoint.

A route is a navigable path exposed through catalogues and Entries.

A root route is exposed directly by the loader.

A scope narrows where the following content applies.

A slug is a concrete folder name in a route.

Selecting a C# catalogue reads its #LoadNow design file. An unselected Windows catalogue beneath it stays closed, including its #LoadNow children.

#Core labels routing, loading, workspace orientation and reusable content roles.

#Memory labels self-growing Markdown state for work, coordination, accepted knowledge, candidates and history.

#Extension labels optional packaged routes, capabilities, integrations and support files.

Only #LoadNow and #KeepInMind control loading. #Evergreen creates the maintenance duty described in the loader. Tags do not establish acceptance.
````

### v3

This is the complete Full Core loader. Minimal uses the same text with an empty `Entries` section. Installation renders only selected entries and preserves authored links.

```markdown
## Open Forge Loader

This workspace uses the documents listed under `Entries`. Follow this file alongside the project's other instructions. Listed parts are enabled. Unlisted parts are disabled.

### Axioms

### Read before working

1. At task start or resume, and after context restoration, identify your working files. Include files you will investigate, review, create, change, rename or delete, including planned files. Background reading does not make a file a working file.
2. When available and allowed, run `open-forge context`, adding `--for <file>` for every known working file. Read its output. The command handles steps 3 and 4. Always do step 5 yourself. If it is unavailable, fails or cannot follow a link, use your file tools for steps 3 to 5.
3. Before opening a conditioned entry, check its `applies to` patterns. One working file must match a pattern in every condition list along that entry's catalogue chain. Globs are case-sensitive and start at the workspace root. `**/*.cs` matches C# files at any depth. A leading `!` is literal. Report required conditioned entries as pending while working files are unknown, then resolve them before matching work.
4. Read every applicable #LoadNow and #KeepInMind entry in listed order. A catalogue lists other files. Whenever you read one, repeat this procedure for its entries. Never skip a required file because its description seems unrelated. Read each file once per pass.
5. Open other entries when their descriptions fit the task. Follow explicit reading requirements in files you read. Before using a search result or direct link, read its enabled ancestor catalogues. Use catalogues for navigation. Do not open ordinary file bodies just to discover entries. Disabled branches remain background.

Restart at step 1 when your goal or working files change. Reread #KeepInMind entries before handoff or finish, only in active branches. During work, recheck them only when they may have changed. Discover tagged files through catalogues.

### Apply the context

- Follow rule files' `## Instructions` and loaded catalogues' `## Axioms` within their stated scope. Catalogue rules cover their listed descendants. Narrower rules add to broader rules and do not cancel them. Report conflicts and instructions you cannot follow, with reasons.
- Follow user direction within platform and safety limits. Take routine, reversible steps within the request without asking again. State assumptions that affect the result. Ask before unresolved choices materially change the result, scope, risk, cost, external effects or ability to undo the work.
- Reading, linking, saving or tagging establishes neither acceptance nor permission to edit. Acceptance comes from the user or an authority they designated for that question. Use declared external authorities for the facts assigned to them.
- Let each source answer one clear question. Link to related sources instead of repeating their detail. A link does not change either source's authority, scope, loading rules or lifecycle. Put detail in its narrowest defining source, with selection summaries and links above it.
- Investigate apparent conflicts between accepted current sources before changing either. When accepted direction changes current state, update its defining source and preserve useful prior context.
- #Evergreen means a source must stay aligned with accepted current state. It grants neither authority nor loading behavior. Update affected sources you may edit before work depends on them and by closeout. Batch related updates when safe. Keep each source aligned with what it represents now. Preserve useful prior context in existing history or Memory when enabled. Report updates that are blocked or unauthorized.
- Selected non-binding material may specialize broader material of the same kind within its accepted scope.

### Choose and maintain the setup

- Add an `Entries` line to enable a catalogue or Markdown file anywhere in the workspace, such as `../docs/architecture.md`. Existing documents need neither moving nor frontmatter. Their entry supplies description, tags and conditions.
- Remove a loader entry to disable that branch while retaining its files. Background reading does not reactivate it. Accepted project requirements and independently loaded host instructions remain in force. Report enabled dependents before disabling their source. Unlisted parts create no Open Forge duty. Do not restore them without user direction.
- Other catalogues follow their folder contents. To disable an item there, move or delete its file and update the list. For managed removals, also record the exclusion. Before using CLI commands other than `context`, read its Skill when installed, otherwise use command help.
- Report missing listed files. An empty `Entries` section is valid and must stay empty unless the user enables something.
- #LoadNow requires reading when its parent loads. #KeepInMind also requires the refreshes above. For nested startup reads, tag every parent catalogue #LoadNow too. Narrow specialized context before making it mandatory. Other tags classify or support search, with the maintenance duty for #Evergreen defined above.
- New routed Markdown under `.agents/` needs frontmatter with `description`, `tags` and optional `applyTo`. Follow `.agents/open-forge.json`, defaulting to an `open-forge:` mapping when no preference exists. Native Skills retain their metadata. Use one catalogue per folder, named `_{folder-name}.md`. Existing `index.md`, `_index.md`, `references.md` and `_references.md` remain valid. Maintain Entries manually or with `open-forge index`.
- Customize a shipped source by editing or replacing it, or place lasting adjustments in its adjacent `{name}.overwrite.md`. Read the overwrite immediately after its base, with the same scope and loading conditions. It wins only for corresponding content in that source. Do not index or select it separately.

### Entries

- [Communication preferences](communication.md) - #LoadNow
- [Rules for authoring context files and catalogues](authoring.md) - #LoadNow
- [Rules agents must follow in this workspace](directives/_directives.md) - #LoadNow #Core #Directive
- [Advice to check before choosing an approach](guidance/_guidance.md) - #Core #Guidance
- [Pointers to project knowledge](maps/_maps.md) - #Core #Map
- [Notes for accepted knowledge, unfinished work, ideas and history](memory/_memory.md) - #LoadNow #Memory
- [Default shapes to check before creating or reviewing structure](patterns/_patterns.md) - #Core #Pattern
- [Capabilities to check before specialized work](skills/_skills.md) - #Core #Skill
- [Starting files to copy when useful](templates/_templates.md) - #Core #Template
```

Managed `AGENTS.md` block:

```markdown
## Open Forge

Before starting or resuming work, read `.agents/loader.md` and follow its procedure. It identifies this workspace's selected context and adds to the instructions already here.

If available and allowed, `open-forge context --for <file>` gathers required reads. Repeat `--for` for every working file. The loader explains the remaining reading and the fallback when the command cannot run.

**End of Open Forge managed section.**
```

| Before | After | Why |
| --- | --- | --- |
| “The `Entries` below are the switch: what is listed is on, and anything not listed is off.” | “Listed parts are enabled. Unlisted parts are disabled.” | Short, direct statements. |
| “That covers steps 3 and 4. Still do step 5 yourself.” | “The command handles steps 3 and 4. Always do step 5 yourself.” | Names the actor and preserves the obligation. |
| “only when the same working file matches its patterns and those of every conditioned catalogue above it” | “One working file must match a pattern in every condition list along that entry's catalogue chain.” | Makes alternatives within a list explicit. |
| “Nothing that is not listed creates a duty” | “Unlisted parts create no Open Forge duty.” | Preserves independent project and host obligations. |
| “in the same form as the files around them” | “Follow `.agents/open-forge.json`, defaulting to an `open-forge:` mapping when no preference exists.” | Replaces inference with the actual metadata rule. |

Whitespace-delimited counts, including headings and entry lines: v3 loader **984 words**, empty-entry loader **892**, managed block **64**. The two-file minimum is **956 words**. V2’s corresponding full loader and block were **823** and **56**. The increase restores general duties and removes compressed ambiguities.

### Risks and proof

The minimum may still feel long. Full Core gains two policy files. Unmanaged seeds introduce a promotion boundary. Nested catalogue removal remains less convenient than removing a loader line.

The change map records an empty-Entries resurrection case. Pin that case before claiming disabled content stays disabled. Also verify that Update preserves authored loader membership and that directly edited managed files can still be replaced. Overwrite companions remain the supported persistent customization mechanism.

The cheapest adoption experiment needs three strangers and disposable repositories, not another council. Give each only the replacement introduction. Ask them to connect one existing document, add one scoped rule, and disable it. Then ask a fresh agent to perform matching and unrelated tasks. Repeat once without the CLI.

Pass when all three finish without coaching, can explain what is active, and observe the expected reads. Require zero disabled-rule applications and zero unintended source-document edits. Separately run a small fixture for each standalone capability, checking that no sibling module is required or recreated.

This proposal is based on read-only inspection. No files were modified and no doctor, builds or test suites were run.

### The one idea to steal

Make the first successful adoption one existing document correctly read in a fresh task.

---

## Essence Guardian (Proposal G)

/root - GPT-6 - high

### 1. Thesis

Keep v2’s reading procedure, direct links and optional folders, but restore the obligations that its shorter wording no longer guarantees. Complexity should grow through useful additions, while user authority, acceptance boundaries and the currency of accepted knowledge remain consistent at every level. A brownfield project should begin by connecting one document it already trusts.

### 2. How v2 removed so many rules

The [ledger](artifacts/council-3/ledger.md) shows six kinds of removal.

| Kind | Two examples | Verdict |
| --- | --- | --- |
| Removing vocabulary | Loader L46, `route`. Loader L49, `slug` | Sound compression when definitions remain available to readers who meet those terms. |
| Moving tool details | Loader L50, managed routes. Loader L130, command help | Sound when the CLI Skill explains lifecycle operations without becoming necessary for plain-file use. |
| Narrowing general obligations to Memory | Loader L27, one question per source. Loader L32, investigate conflicting current sources | Loss. Existing documents need these protections even without Memory. |
| Dropping timing and refresh conditions | Loader L94, recheck changed context. Loader L101, update before dependent work | Loss. “Update” without a deadline permits stale information to guide subsequent work. |
| Removing category-specific safeguards | Patterns L19, accepted reusable shapes. Templates L24, continuing requirements | Loss. These distinguish reusable conventions from unsettled examples and copied starting content. Restore them where those categories are used. |
| Treating binding communication rules as preferences | Loader L21, proportionate explanation. Loader L22, concise but complete writing | A reasonable product discussion, but not a lossless rewrite. Preserve their force until the maintainer explicitly changes that policy. |

Descriptions can replace definitions and selection explanations. They cannot imply omitted prohibitions, deadlines or acceptance conditions.

### 3. The lossless ledger

All destination paths below are relative to `.agents/`. Old line numbers identify the source files in the ledger. Quoted wording is the proposed replacement text. Catalogue changes belong under `## Axioms` unless another section is named.

**Destination: `loader.md`, “Use what you read”**

- `loader.md L21` — **Restore:** “Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.”
- `loader.md L22` — **Restore:** “Use the fewest words that preserve meaning, readability and logical progression. Remove filler and repetition. Keep necessary context, conditions and exceptions.”
- `loader.md L27` — **Restore:** “Let each source answer one clear question. Link to related sources instead of repeating their detail. A link changes neither source's authority, scope, loading rules or lifecycle.”
- `loader.md L28` — **Restore:** “Connect related material with relative local links and precise references. Reuse descriptive tags for shared subjects. Add links and tags only when they help navigation or retrieval.”
- `loader.md L32` — **Restore:** “Investigate apparent #CurrentTruth conflicts before changing either source. When accepted direction changes current state, update its defining source and preserve useful prior context.”
- `loader.md L40` — **Restore:** “Selected non-binding material may specialize broader material of the same kind within its accepted scope.”
- `loader.md L100` — **Restore:** “#Evergreen material must stay aligned with accepted current state. The tag grants neither authority nor loading.”
- `loader.md L101` — **Restore:** “Update affected #Evergreen sources you may edit before work depends on them and no later than closeout. Batch related updates when safe.”
- `loader.md L102` — **Restore:** “Keep each source aligned with what it represents now. Preserve useful prior context in enabled Memory. Report blocked or unauthorized updates.”
- `memory/_memory.md L27` — **Relocate:** “Update other affected defining sources before dependent work and no later than closeout. Accepted durable knowledge must reach its defining source, not remain only in chat or an unaccepted record.” Keep useful reasoning under Memory’s own rules. This duty must survive removal of `saving.md`.

**Destination: `loader.md`, “Read before you work”**

- `loader.md L62` — **Restore:** “Use catalogues for navigation, not file bodies merely to discover routes or tagged files.”
- `loader.md L93` — **Restore:** “Refresh only active scopes.” The preceding navigation sentence also preserves its prohibition on discovery through file bodies.
- `loader.md L94` — **Restore:** “During work, recheck them only when they may have changed.”

**Destination: `loader.md`, “Change what is on”**

- `loader.md L37` — **Restore:** “For a new catalogue without local rules, use `## Axioms` containing only `- inherited - No local axioms. Loaded ancestor axioms remain active.` Existing empty or missing sections add no rules. Never mix the inherited marker with local rules.” Retain recognition of the existing marker spelling.
- `loader.md L57` — **Restore:** “A route may contain any number of narrowing folders before, between or after deeper route segments. Each narrows what follows.”
- `loader.md L58` — **Restore:** “Each catalogue needs only the routes useful there, regardless of installed defaults.”

**Destination: `README.md`, “Terms you may encounter”**

- `loader.md L46` — **Relocate:** “A route is a navigable path exposed through catalogues and Entries.”
- `loader.md L47` — **Relocate:** “A root route is exposed directly by the loader.”
- `loader.md L48` — **Relocate:** “A scope is part of a route that narrows where following content applies.”
- `loader.md L49` — **Relocate:** “A slug is a concrete folder name in a route path.”
- `loader.md L95` — **Relocate:** “#Core labels base routing, loading, workspace orientation and reusable content roles.”
- `loader.md L97` — **Relocate:** “#Extension labels optional packaged routes, capabilities, integrations and support files.”

**Destination: `README.md`, “Required reading inside a scope”**

- `loader.md L85` — **Relocate:** “Selecting a C# catalogue loads its #LoadNow design file. An on-demand Windows catalogue beneath it stays closed. Its #LoadNow files load only after Windows is selected.”

**Destination: `maps/_maps.md`**

- `maps/_maps.md L23` — **Restore:** “Keep Maps broad. Prefer modules, projects, repositories, systems or scopes over members and functions unless finer routing clearly helps.”
- `maps/_maps.md L24` — **Restore:** “Each workspace chooses its Map filenames, grouping and nesting depth.”
- `maps/_maps.md L25` — **Restore:** “Maps do not replace Memory or their destinations' current information.”

**Destination: `patterns/_patterns.md`**

- `patterns/_patterns.md L19` — **Restore:** “Create or update a Pattern only when an accepted shape should guide future related work. Do not promote one-off work, temporary transitions or unsettled candidates merely because they have structure.”
- `patterns/_patterns.md L25` — **Restore:** “Link related rules, advice, maps, procedures and starting files, identifying which sources bind the shape. Sources applying the shape should link back to its Pattern.”
- `patterns/_patterns.md L26` — **Restore:** “Keep required behavior, procedures and substantial choice rationale in their defining sources. Example names and values are illustrative unless an applicable requirement fixes them.”
- `patterns/_patterns.md L28` — **Restore:** “Adapt only within authority already granted. In reviews, report justified adaptations separately from conformance.” Retain v2’s requirement to explain departures before dependent work.

**Destination: `templates/_templates.md`**

- `templates/_templates.md L24` — **Restore:** “If a copy needs continuing guidance or requirements, link their defining sources. The Template supplies starting content only.”
- `templates/_templates.md L28` — **Restore:** “State each Template's need and primary question in its description and source instructions.”
- `templates/_templates.md L29` — **Restore:** “Use focused prompts that request necessary content once, in a readable order. Mark optional sections clearly.”
- `templates/_templates.md L30` — **Restore:** “Treat generic Templates as fallbacks. Add a specialized Template only when its starting content differs meaningfully.”

**Destination: `memory/_memory.md`, opening**

- `loader.md L96` — **Relocate:** “Memory holds active work, coordination, accepted knowledge, candidates and history.”
- `memory/_memory.md L13` — **Restore:** “Memory is self-growing Markdown state. People and agents may add useful records and routed scopes without a fixed structural limit. Unselected branches do not load their contents. Records need not pass through every state.”

**Destination: `memory/_memory.md`, `## Axioms`**

- `memory/_memory.md L19` — **Restore:** “An accepted record may define current knowledge or decisions. Recording another category's content does not give the record that category's role.”
- `memory/_memory.md L26` — **Restore:** “Distinguish what was stated, observed or inferred when it affects trust or later acceptance.” Retain v2’s source and uncertainty requirement.
- `memory/_memory.md L32` — **Restore:** “Add records and routed scopes when their likely future value justifies their retrieval and review cost.”
- `memory/_memory.md L35` — **Restore:** “Named categories and Templates are optional. Use the narrowest suitable scope in an existing state. Records may live directly under that state. Do not recreate removed categories.”
- `memory/_memory.md L36` — **Restore:** “Within granted authority, add records or scoped routes when needed. Ask before placement changes accepted meaning or crosses an unresolved authority boundary.” Retain the existing-record-first rule.

**Destination: `memory/working/_working.md`**

- `memory/working/_working.md L19` — **Restore:** “An explicitly accepted temporary choice may remain here when its source, scope and expected expiration are clear.”

**Destination: `memory/emerging/_emerging.md`**

- `memory/emerging/_emerging.md L17` — **Restore:** “At each applicable #KeepInMind refresh, check visible Entries and save useful candidate material before it is lost.”
- `memory/emerging/_emerging.md L23` — **Restore:** “Refine material while its validity, use or destination remains unsettled.”
- `memory/emerging/_emerging.md L25` — **Restore:** “Revisit repeated or stale material and decide whether to consolidate, promote, archive or prune it. Review may leave a record unchanged.”

**Destination: `memory/archived/_archived.md`**

- `memory/archived/_archived.md L17` — **Restore:** “Retain, consolidate or transform history according to future value. Archiving need not preserve every detail.”

These cover all 34 Dropped and 12 Partly rows. None needs an accepted behavioral loss.

The remaining statuses also need correction. Restore Emerging’s default `#KeepInMind` in its frontmatter and parent entry. Add “Get explicit user direction before creating a new top-level Memory state” to Memory’s Axioms. Restore removal of metadata claiming former behavioral roles or current authority when archiving, and permit deletion under accepted retention preferences. Preserve required destination structure in Template copies and optional, repeatable and fixed parts in Pattern descriptions.

The loader below also repairs authority delegation, direct-link ancestry, metadata forms, matching-file scope and overwrite boundaries. Category catalogues may remain on demand because step 5 explicitly reaches their rules before category use or authorship.

### 4. The complexity ladder

The levels describe useful stopping points, not separate framework engines. CLI installations also keep the human guide, harness bridges, settings and ownership record.

| Level | Installed tree | Benefit and cost | Adjacent movement |
| --- | --- | --- | --- |
| 0. One rule | Existing `AGENTS.md` containing one instruction | Immediate correction with almost no upkeep. No routed context or Memory. | Add the loader to reach 1. Remove only the added instruction to leave. |
| 1. Connect existing knowledge | `AGENTS.md`, `.agents/loader.md`, `.agents/README.md`, one link to an existing document | Relevant reading without relocating documents. Costs the loader and maintaining the link. | Add Directives and Crystallized for 2. Remove the managed bridge to return to 0. |
| 2. Rules and accepted knowledge | Level 1 plus `directives/_directives.md`, `memory/_memory.md`, `memory/crystallized/_crystallized.md` and useful records | Binding conventions and current accepted knowledge. Costs acceptance review and source updates. | Add selected continuity and reuse tools for 3. Disable the added parts to return to 1. |
| 3. Essentials | Level 2 plus Patterns, Skills and CLI guides, Working with `saving.md`, and Emerging | Resume work, retain candidates and reuse shapes. Costs capture, refresh and retirement duties. Working records remain private under the CLI preset. | Add the remaining categories for 4. Disable the added branches to return to 2. |
| 4. Full Core and chosen Extensions | Level 3 plus Guidance, Maps, Templates and Archived. Extensions add only their selected subtrees | Complete role separation and optional methods. Costs more navigation and lifecycle maintenance. | Disable unwanted additions to return to 3. Extensions remain individually removable. |

For nested branches, disabling requires moving their files outside an enabled catalogue’s folder and removing their links. Keep notes and record managed exclusions. Accepted requirements survive these changes.

Start a brownfield project at **level 1**. Its first useful result should come from its existing contribution rules, without reconstructing accepted knowledge in new folders.

### 5. Ultra-lightweight install

Add **`--preset minimal`**. It selects no built-in category rows and does not introduce a persisted mode.

A fresh installation writes `AGENTS.md` and `CLAUDE.md` managed sections, `.agents/loader.md` with empty `## Entries`, `.agents/README.md`, `.agents/open-forge.json` and `.agents/open-forge.lock.json`. It installs no category folders, Memory parent, CLI Skill, saving policy, Extensions or Git-ignore section. Existing host content remains intact.

Allow existing syntax with Minimal:

`open-forge install --preset minimal --route directives=add --route memory/crystallized=add`

Resolve overrides against ten omitted rows. Add necessary ancestors only. Crystallized adds `_memory.md` and `_crystallized.md`, without siblings. Existing installations still require `--configure`. Existing files are retained, so a retained route must never be described as disabled.

Plan wording:

> Minimal installs the loader and human guide, adds the Open Forge sections to your agent instruction files, and records maintenance settings. No optional folders or documents are enabled. Existing project content is kept.

Grow through `route init <category> --framework`, or Custom configuration starting from current choices.

Under `src/cli/operations/OpenForge.Cli.Operations/Commands/Install/`, change:

- `Models/Configuration/InstallConfiguration.cs`
- `Models/Binding/InstallBindingModels.cs`
- `Shared/Binding/InstallBindingInputReader.cs`
- `Shared/Configuration/InstallConfigurationChoices.cs`
- `Shared/Configuration/InstallSetupResolver.cs`
- `Shared/Configuration/InstallConfigurationIntendedStateBuilder.cs`

Also change Framework’s `Distribution/Shared/Sources/FrameworkPayloadSelection.cs` for necessary-ancestor selection, Rendering’s `InstallReportSelector.cs`, and OutputText’s `InstallSetupText.cs` and `InstallInputText.cs`.

Cost is a bounded preset change with selection, empty-loader, preservation, growth, help and plan snapshots. Catalogue grammar stays unchanged.

Two separate compatibility repairs accompany v3. Support both inherited-marker spellings in `RouteInspectAxiomsProfileBuilder.cs`, and emit the new spelling from `RouteScaffoldComposer.cs` and `WorkspaceAdoptionEntrypointComposer.cs`. Extend sharing facts so enabled packaged policies can remain shared alongside an ignored route’s catalogue. That touches `InstallIgnoreSection.cs`, `SourceSharing.cs` and its sharing model, with lock compatibility and focused navigation tests. Private record paths must remain excluded.

### 6. Brownfield adoption

The following walkthrough targets the proposed v3 CLI. `minimal` does not exist today.

1. Read the existing `AGENTS.md` or `CLAUDE.md`, `README.md` and `CONTRIBUTING.md`. Establish which documents already define accepted requirements. Review the current checkout with `git status --short`.
2. Run `open-forge install --preset minimal --frontmatter root --dry-run`. Inspect every planned path. Run the same command without `--dry-run`, then inspect `git status --short`, `git diff` and new files. The agent now reads existing host instructions and the loader. No optional catalogue loads.
3. Add these lines under the loader’s `## Entries`:

   ```markdown
   - [Contribution rules for code changes](../CONTRIBUTING.md) - #LoadNow - applies to `src/**`
   - [Project purpose and usage](../README.md)
   ```

   Work on `src/example.cs` now reads `CONTRIBUTING.md`. README opens when relevant. Neither link establishes acceptance.
4. Run `open-forge context --for src/example.cs`. Check the read set against the files manually. Ask the agent to explain the test command and identify its source.
5. When a specific architecture document becomes useful, link its actual filename directly. Do not import the whole `docs/` folder.
6. When a recurring correction warrants Directives, preview and apply `open-forge route init directives --framework`. Add one rule and its entry. The required Directives chain now loads it.
7. When newly accepted knowledge needs a maintained record, preview and apply `open-forge route init memory/crystallized --framework`. Record only the new accepted result or a pointer to its existing defining source.

Without the CLI, copy the selected payload files, append the managed block and maintain those same Entries manually.

Open Forge must never relocate existing docs, infer acceptance from installation, replace unrelated host instructions, normalize unselected content, invent historical decisions or restore disabled opinions.

Useful CLI help is a read-only suggestion of existing documents with proposed entry lines and explicit loading choices. Selection must remain with the user. No automatic repository-wide ingestion is needed.

### 7. The introduction

Replacement README introduction and start section:

````markdown
## Open Forge

Open Forge helps coding agents find the rules, documents and decisions your project already has. It uses plain Markdown files and links, with an optional CLI for reading and maintenance.

Start with one useful connection. Add rules, accepted knowledge or notes for resuming work when you need them. Your documents can stay where they are, and you choose which Open Forge parts to use.

### Start with an existing document

With Node.js 22.18 or later:

```sh
npm install -g @thelithiumforge/open-forge@beta
open-forge install --preset minimal --frontmatter root --dry-run
open-forge install --preset minimal --frontmatter root
```

Review the plan before applying it. Existing `AGENTS.md` and `CLAUDE.md` text stays in place.

Open `.agents/loader.md` and add this under `## Entries`:

```markdown
- [Contribution rules for code changes](../CONTRIBUTING.md) - #LoadNow - applies to `src/**`
```

Use your project's actual document and paths. The document needs no frontmatter or other changes. This line requires reading it for work under `src/`. A link gives reading instructions, not acceptance or permission to edit.

Ask your agent:

> Explain how this project runs its tests. Follow AGENTS.md and name the source of the test command.

If your agent tool does not read `AGENTS.md` automatically, ask it to read that file first.

### Add rules and accepted knowledge

When a correction keeps recurring, add Directives. When an accepted result needs to stay current, add Crystallized Memory:

```sh
open-forge route init directives --framework --dry-run
open-forge route init directives --framework
open-forge route init memory/crystallized --framework --dry-run
open-forge route init memory/crystallized --framework
```

A rule belongs under `## Instructions` in a Directive file, with `LoadNow` in its tags and an entry in its parent catalogue. For example:

```markdown
---
description: Check code changes before finishing
tags: [LoadNow, Directive, Testing]
---

## Check code changes

### Instructions

- Run the relevant project checks before reporting completion. Give the command and result, or explain why they could not run.
```

Keep accepted knowledge in its existing defining document when one exists. A Memory record can point there instead of copying it. Saving a finding does not make it accepted.

### Add continuity when work spans sessions

Working Memory keeps resume notes. Its separate saving policy asks agents to update those notes before pausing unfinished work:

```sh
open-forge route init memory/working --framework --dry-run
open-forge route init memory/working --framework
```

You can remove the saving policy while keeping the records. Emerging Memory can hold useful findings that are not accepted yet.

Essentials combines rules, accepted knowledge, continuity, Patterns and Skills. Full Core adds advice, source Maps, Templates and history. Custom lets you select individual parts. Extensions add optional methods and record shapes.

### Keep only what helps

Remove a loader entry to turn that part off while retaining its files. Accepted project requirements and instructions your agent tool loads independently still apply. Record managed removals so updates respect them.

Every file is yours to inspect and adapt. Overwrite companions preserve local adjustments through CLI updates.

The CLI is optional. You can copy the selected files from `src/open-forge`, append the managed agent instructions and edit Entries yourself. `.agents/README.md` explains the local setup.

Open Forge supplies instructions and context. Your agent tool still runs the agent, and ordinary review remains necessary.
````

Complete `.agents/README.md`:

````markdown
## Open Forge in this project

Start with [`loader.md`](loader.md). Its Entries select the Open Forge context agents use. This guide is for people and is not required startup reading.

The files work without the CLI. You can use one existing document, selected folders, or the full Framework.

### Connect a document

Add a line under the loader's `## Entries`:

```markdown
- [C# contribution rules](../docs/contributing/csharp.md) - #LoadNow - applies to `**/*.cs`
```

Use a file that exists. Paths are relative to the catalogue containing the entry. Existing documents need no move or frontmatter.

`#LoadNow` requires reading when the parent loads and the conditions match. Without a loading tag, agents open the document when relevant. Conditions use case-sensitive paths from the workspace root.

Reading a document does not make its contents accepted or grant permission to change it.

### Add a rule

Enable `directives/_directives.md` through the loader. Create a file under `directives/` with a description, tags including `LoadNow`, and required rules under `## Instructions`. Follow the workspace's metadata form.

Add the file to its catalogue's Entries, or preview and run `open-forge index`. Use `applyTo` for file-specific rules.

### Keep accepted knowledge

Enable `memory/_memory.md` and its Crystallized entry. Records may live directly under `memory/crystallized/`.

Keep only accepted knowledge there. Identify its source and scope. Update an existing defining document instead of creating a competing account. Tags and folder placement do not establish acceptance.

Other Memory states are optional. Working holds temporary context, Emerging holds unsettled findings, and Archived holds useful history.

### Resume unfinished work

Enable Working Memory to give resume records a home. Its separate `saving.md` policy requires updating a record before pausing unfinished work.

To stop automatic saving, remove `saving.md` and its entry in `_working.md`. Keep the records. Record the file removal in managed exclusions if you use CLI updates.

### Turn a part off

Remove its loader entry. Keep its files as background notes.

Inside other catalogues, Entries follow the files in their folders. Move a disabled branch outside its enabled parent folder and remove its entry, or delete it when that is what you intend.

Disabling does not revoke accepted project requirements or instructions loaded independently by your agent tool. Check enabled dependants first.

For CLI-managed removals, merge the route name into `removedCategories` in `.agents/open-forge.json`, or the exact file path into `removedFiles`. Preserve the other settings. The CLI Remove command can delete files, so inspect its preview before choosing it.

### Customize shipped files

Edit or replace a file directly, or put a lasting adjustment in `{name}.overwrite.md` beside it. Updates can replace direct edits.

An overwrite loads immediately after its base. It changes only corresponding content in that source, within the same role and scope. It cannot cancel unrelated rules.

### Required reading inside a scope

Selecting a C# catalogue loads its #LoadNow design file. An on-demand Windows catalogue beneath it stays closed. Its #LoadNow files load only after Windows is selected.

Required reading needs an unbroken chain of loaded parents.

### Terms you may encounter

A catalogue is a folder's navigation file, normally named `_{folder-name}.md`.

A route is a navigable path exposed through catalogues and Entries.
A root route is exposed directly by the loader.
A scope is part of a route that narrows where following content applies.
A slug is a concrete folder name in a route path.

#Core labels base routing, loading, workspace orientation and reusable content roles.
#Extension labels optional packaged routes, capabilities, integrations and support files.

### With or without the CLI

`open-forge context --for <file>` gathers required reads for the working paths supplied. Agents must also follow task-specific and explicit reading requirements.

If the CLI cannot run, read the same files directly. Maintain Entries by hand when adding or moving files. Read the CLI Skill, when installed, before other CLI commands.
````

### 8. v3

Complete `.agents/loader.md`. This is the Full Core payload. Install selection removes entries for omitted parts.

```markdown
## Open Forge Loader

This file selects the Open Forge context used by this project. Read it after `AGENTS.md`. The files work without the CLI. Each line under `Entries` enables a file or catalogue. Unlisted parts stay off.

### Axioms

### Read before you work

1. At task start or resume, and after context restoration, list the files you will investigate, review, create, change, rename or delete. Include planned files and both paths of a rename. Reading a file only for background does not make it a working file.
2. If available and allowed, run `open-forge context` with `--for <file>` for every known working file. Read its output. This covers steps 3 and 4, but you must still do step 5. If the command is unavailable, fails or cannot follow a link, use your file tools for steps 3 to 5.
3. In this loader and each catalogue you read, read entries tagged #LoadNow or #KeepInMind in listed order, subject to step 4. Follow their catalogue children the same way. A catalogue is a folder's navigation file. Read each file once per pass. Never skip a required read because its description seems unrelated.
4. A conditioned entry loads automatically, refreshes and applies only when one working file matches its `applies to` patterns and every ancestor's conditions. Apply its rules only to matching working files. You may inspect a nonmatch for background without activating it. Patterns within one list are alternatives. Conditions do not select an unopened parent or grant edit permission. Use case-sensitive globs from the workspace root. `*.py` matches root files, `**/*.py` any depth, and `docs/**` a folder. A leading `!` is literal. Report required conditioned entries as pending while paths are unknown, then resolve them before matching work.
5. Open other entries when their descriptions fit the task, including before using or authoring their category's content. Read files explicitly required by loaded instructions. Before using a search result or direct link, read its applicable listed ancestor catalogues and required context. Unlisted Open Forge parts remain background. Use catalogues for navigation, not file bodies merely to discover routes or tagged files.

Restart at step 1 when working files or the goal change. Refresh only active scopes. Reread their #KeepInMind entries before handoff or finish, including overwrites and required children. During work, recheck them only when they may have changed.

### Use what you read

- Follow platform constraints and runtime safety. Follow clear user direction without asking again. Take routine, reversible steps within the request and state assumptions that affect the result. Use accepted context to resolve choices. Ask before relying on an unresolved choice that significantly changes the result, scope, risk, cost, external effects or ability to undo the work. Keep it unaccepted meanwhile.
- Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.
- Use the fewest words that preserve meaning, readability and logical progression. Remove filler and repetition. Keep necessary context, conditions and exceptions.
- Follow this loader's Axioms, loaded catalogues' `## Axioms`, and rule files' `## Instructions` within their stated scope. Children inherit ancestor rules and add narrower requirements without cancelling or repeating them. Report unresolved conflicts instead of choosing a winner. Selected non-binding material may specialize broader material of the same kind within its accepted scope.
- Accepted documents define their subjects' current requirements, design or knowledge. Use declared external authorities for the facts assigned to them. Acceptance requires user agreement or an authority the user designated for that question. Reading, linking, saving, moving, restoring, tagging, repetition and agent confidence establish neither acceptance nor edit permission. Evidence can validate a claim without accepting it. Treat #Contextual material as unaccepted unless applicable authority accepts it. #CurrentTruth identifies accepted current state within its scope, subject to user direction and the limits above.
- Let each source answer one clear question. Link to related sources instead of repeating their detail. A link changes neither source's authority, scope, loading rules or lifecycle. Put detail in the narrowest source that defines the question, with linked summaries above it.
- Connect related material with relative local links and precise references. Reuse descriptive tags for shared subjects. Add links and tags only when they help navigation or retrieval.
- Investigate apparent #CurrentTruth conflicts before changing either source. When accepted direction changes current state, update its defining source and preserve useful prior context.
- #Evergreen material must stay aligned with accepted current state. The tag grants neither authority nor loading. Update affected #Evergreen sources you may edit before work depends on them and no later than closeout. Batch related updates when safe. Keep each source aligned with what it represents now. Preserve useful prior context in enabled Memory. Report blocked or unauthorized updates. Update other affected defining sources before dependent work and no later than closeout. Accepted durable knowledge must reach its defining source, not remain only in chat or an unaccepted record.

### Change what is on

- Add an `Entries` line to enable a catalogue or Markdown file anywhere in the workspace, such as `../docs/architecture.md`. Its entry carries the description, tags and conditions, so an existing document needs no move or frontmatter.
- Remove a loader entry to disable that part while keeping its files. Background reading cannot reactivate it. Disabling does not revoke accepted project requirements or instructions loaded independently by the host. Report enabled dependants first. Do not restore removed parts without the user's request. Other catalogues follow files in their folders, so disable their entries by moving or deleting the files. Record managed removals in the manager's exclusions before updates. Read the installed CLI Skill before commands other than `context`, or use command help when that Skill is absent.
- Report missing listed files. An empty `Entries` list is valid.
- #LoadNow requires reading whenever its parent loads. #KeepInMind also requires the refreshes above. Required descendants need an unbroken chain of loaded catalogues. Keep optional content on demand. Narrow specialized content before making it mandatory. Do not require a broad parent merely to reach one descendant. Use loading tags only when those reads are needed. Other tags classify content or aid search and do not grant authority.
- A route may contain any number of narrowing folders before, between or after deeper route segments. Each narrows what follows. Each catalogue needs only the routes useful there, regardless of installed defaults. Keep separately selected chains separate and follow their explicit links.
- New routed Markdown uses `description`, `tags` without `#`, and optional `applyTo`. Follow `frontmatter` in `.agents/open-forge.json`. Root fields and an `open-forge:` mapping are supported. Missing settings mean scoped. When `open-forge:` exists, its description and tags take precedence without merging root values. Use `applyTo` as a list or comma-separated string. Read both metadata locations for it. Equivalent pattern sets count once, conflicting sets are invalid. Native Skills keep native metadata. A folder has one catalogue, normally `_{folder-name}.md`. Compatibility names are `index.md`, `_index.md`, `references.md` and `_references.md`. Maintain Entries manually or with `open-forge index`.
- For a new catalogue without local rules, use `## Axioms` containing only `- inherited - No local axioms. Loaded ancestor axioms remain active.` Existing empty or missing sections add no rules. Never mix the inherited marker with local rules.
- Customize a shipped source by editing, replacing or removing it. For changes that survive CLI updates, use `{name}.overwrite.md` beside its base. Read it immediately after the base, with the same role, scope and loading behavior. Do not index it separately. It wins only for corresponding content in that base and cannot override unrelated sources.

### Entries

- [Rules agents must follow in this workspace](directives/_directives.md) - #LoadNow #Core #Directive
- [Advice for recurring choices, to check before choosing an approach](guidance/_guidance.md) - #Core #Guidance
- [Where to find project knowledge, checked before relying on it](maps/_maps.md) - #Core #Map
- [Notes that help work continue, such as resume points, accepted knowledge, ideas and history](memory/_memory.md) - #LoadNow #Memory
- [Default shapes to check before creating or reviewing structure](patterns/_patterns.md) - #Core #Pattern
- [Capabilities to check before specialized work](skills/_skills.md) - #Core #Skill
- [Starting files to copy when creating something new](templates/_templates.md) - #Core #Template
```

Complete managed `AGENTS.md` block:

```markdown
## Open Forge

Before starting or resuming work, read `.agents/loader.md` and follow its reading steps. It selects this project's Open Forge context and adds to the other project instructions.

If available and allowed, `open-forge context --for <file>` gathers required reads for the working files you supply. The loader also requires task-specific reading and explains the fallback when the CLI cannot run.

**End of Open Forge managed section.**
```

Five substantial sentence changes:

| Before | After | Why |
| --- | --- | --- |
| “The `Entries` below are the switch: what is listed is on, and anything not listed is off.” | “Each line under `Entries` enables a file or catalogue. Unlisted parts stay off.” | Names the action and separates its consequence. |
| “Read every entry below tagged #LoadNow or #KeepInMind, in listed order, then do the same inside each catalogue you read.” | “In this loader and each catalogue you read, read entries tagged #LoadNow or #KeepInMind in listed order, subject to step 4.” | Makes recursion and conditions explicit. |
| “Read and apply an entry with `applies to` patterns only when the same working file matches its patterns and those of every conditioned catalogue above it.” | “A conditioned entry loads automatically, refreshes and applies only when one working file matches its `applies to` patterns and every ancestor's conditions.” | Includes refresh and distinguishes automatic loading from inspection. |
| “Accepted means the user agreed, or a source the user named says so.” | “Acceptance requires user agreement or an authority the user designated for that question.” | Naming a source is not delegation of authority. |
| “It is read right after its base and wins where they differ, for that file only.” | “It wins only for corresponding content in that base and cannot override unrelated sources.” | Preserves the boundary between customization and conflicting rules. |

Whitespace-delimited counts, including headings and Entries: **loader 1,361 words**, versus the ledger’s v2 count of 823. The empty-entry version is **1,268 words**. The managed block is **67 words**. Restored meaning costs reading, but the loader remains substantially below the original 2,052 words.

### 9. Risks and proof

The principal risk is trading a lighter appearance for obligations that readers cannot find. Restoring them increases startup cost. The experiment must compare equal obligations, rather than rewarding a version for silently doing less.

Other concrete risks are empty Entries being repopulated, direct edits being replaced by Update, category constraints being missed on demand, and Minimal accidentally installing an empty Memory parent. `SourceSharing.Includes` currently excludes every ignored-route child except its catalogue. Prove that enabled `saving.md` remains reachable without publishing private record names.

The cheapest useful study needs three developers unfamiliar with Open Forge and an existing repository each. Give them only the replacement introduction. Ask them to connect one document, explain what loads, add one scoped rule, add accepted knowledge, and disable one addition. Begin without the CLI, then repeat maintenance with it.

Use fresh agent sessions containing a conflicting draft, an accepted current source, an unauthorized update and a second language introduced midway. Record actual reads and resulting behavior.

Pass only if all three reach useful work without coaching, no draft gains authority, required reads stay correctly scoped, blocked updates are reported, and disabled parts stay disabled after maintenance. Ask each person to explain the next ladder step in their own words.

This proposal is a read-only audit. No files were modified and no Doctor, build or test suite was run.

### 10. The one idea to steal

Make the first successful adoption one existing document used correctly in a fresh agent session.

---

## Lossless Auditor (Proposal D)

/root - GPT-6 - high

### Thesis

Keep v2’s opt-in structure and direct links to existing documents. Restore lost obligations at their original strength, moving authoring details behind an explicit required read before authoring. A smaller installation should be a deliberate choice, while an update preserves the behavior an existing workspace already selected.

### How v2 removed so many rules

Six kinds of removal explain the reduction.

- **Vocabulary disappeared.** `route` and `slug` lost their definitions. Moving vocabulary out of startup is sound, provided the authoring reference defines it.
- **Examples and customization permissions disappeared.** The C#/Windows example and permission to choose Map filenames were removed. These can move, but neither is proved redundant.
- **Authoring requirements became assumed competence.** Patterns lost their acceptance threshold, and Templates lost the requirement to link continuing requirements. These are behavioral losses.
- **Specific obligations became general summaries.** “Investigate … before changing either source” became an update rule. Evergreen’s deadline became general maintenance language. These lose sequence and timing.
- **Lifecycle rules became optional or less frequent.** Saving moved behind Working Memory, and Emerging lost `#KeepInMind`. These are reasonable explicit customization choices, but unsafe silent upgrade changes.
- **Communication policy was discarded.** The response recipe and concision rule disappeared. They were binding defaults, so moving them into an optional Extension would not make the rewrite lossless.

The ledger’s explanation “capable models follow [this] without being told” is an argument for changing policy, not evidence that meaning survived.

### The lossless ledger

All source references below use the old payload’s line numbers. Destination paths are relative to `src/open-forge/`. There are **34 Restore and 12 Relocate decisions**, covering all 46 Dropped and Partly rows. None needs an unsupported redundancy claim.

For each destination, append the quoted text under the named section unless section 8 supplies its complete replacement.

**`.agents/loader.md`, `## Axioms`, `### Use the context`**

- `loader.md:L21` — **Restore:** “Match explanation and planning to the request. Unless deeper analysis is requested, begin with your current understanding, one recommendation and at most one important open choice.”
- `loader.md:L22` — **Restore:** “Use the fewest words that preserve meaning, readability and logical progression. Remove filler and repetition. Keep necessary context, conditions and exceptions.”
- `loader.md:L27` — **Restore:** “Let each source answer one clear question. Link related detail rather than repeating it. A link does not merge authority, scope, loading rules or lifecycle.”
- `loader.md:L32` — **Restore:** “Investigate apparent conflicts between accepted sources before changing either. When accepted direction changes current state, update its defining source and preserve useful prior context.”
- `loader.md:L40` — **Restore:** “Selected non-binding material may specialize broader material of the same kind within its accepted scope.”
- `loader.md:L100` — **Restore:** “#Evergreen marks content that must stay aligned with accepted current state. It grants neither authority nor automatic reading.”
- `loader.md:L101` — **Restore:** “Update affected sources you may edit before work depends on them and by closeout. Batch related updates when safe.”
- `loader.md:L102` — **Restore:** “Keep each source aligned with what it represents now. Preserve useful prior context in an appropriate enabled Memory state. Report blocked or unauthorized updates.”

**`.agents/loader.md`, `## Axioms`, `### Read before working`**

- `loader.md:L62` — **Restore:** “Use catalogues for navigation. Do not open ordinary file bodies just to discover routes.”
- `loader.md:L93` — **Restore:** “Refresh #KeepInMind sources only in active branches, before handoff or finish.”
- `loader.md:L94` — **Restore:** “During work, reread them only when they may have changed.”

**`.agents/README.md`, `## Authoring`**

- `loader.md:L28` — **Relocate:** “Connect related material with relative local links and precise references. Reuse consistent descriptive tags for shared subjects. Add links and tags only when they help navigation or retrieval.”
- `loader.md:L37` — **Relocate:** “For a new catalogue without local rules, put only `- inherited - No local axioms. Loaded ancestor axioms remain active.` under `## Axioms`. Missing or empty Axioms sections add no rules. Never combine the inherited marker with local rules.” The punctuation change needs compatibility handling described below.
- `loader.md:L46` — **Relocate:** “A route is a navigable path exposed through catalogues and Entries.”
- `loader.md:L47` — **Relocate:** “A root route is exposed directly by the loader.”
- `loader.md:L48` — **Relocate:** “A scope is part of a route that narrows where following content applies.”
- `loader.md:L49` — **Relocate:** “A slug is a concrete folder name in a route.”
- `loader.md:L57` — **Relocate:** “Scopes can appear before, between or after deeper route segments. Each narrows everything after it, with no fixed depth.”
- `loader.md:L58` — **Relocate:** “A scope contains only useful routes. It need not copy another scope or the installed defaults.”
- `loader.md:L85` — **Relocate:** “For example, a C# scope may contain a #LoadNow design file and an on-demand Windows scope. Selecting C# loads its design rules, not Windows. A #LoadNow file under Windows loads only after Windows is selected.”
- `loader.md:L95` — **Relocate:** “#Core identifies base routing, loading, workspace orientation and reusable agent-facing roles.”
- `loader.md:L96` — **Relocate:** “#Memory identifies self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history.”
- `loader.md:L97` — **Relocate:** “#Extension identifies optional packaged routes, capabilities, integrations and support files.”

These relocations narrow reading to the activity they govern. The loader explicitly requires the reference before authoring. They do not depend on installing Skills or using the CLI.

**`.agents/maps/_maps.md`, `## Axioms`**

- `maps/_maps.md:L23` — **Restore:** “Keep Maps broad. Prefer modules, projects, repositories, systems or scopes over members and functions unless finer routing clearly helps.”
- `maps/_maps.md:L24` — **Restore:** “Choose Map filenames, grouping and nesting depth to suit the workspace.”
- `maps/_maps.md:L25` — **Restore:** “Maps do not replace Memory or the destination’s current information.”

**`.agents/patterns/_patterns.md`, `## Axioms`**

- `patterns/_patterns.md:L19` — **Restore:** “Create or update a Pattern only for an accepted shape that should guide future related work. Do not promote one-off work, temporary transitions or unsettled candidates merely because they have structure.”
- `patterns/_patterns.md:L25` — **Restore:** “Link related rules, advice, maps, procedures and starting files. Identify which sources bind the shape. Sources applying the shape must link back to its Pattern.”
- `patterns/_patterns.md:L26` — **Restore:** “Keep required behavior, procedures and substantial choice rationale in their defining sources. Example names and values are illustrative unless an applicable requirement fixes them.”
- `patterns/_patterns.md:L28` — **Restore:** “Adapt a default shape only within granted authority. Explain material departures before other work depends on them. Report justified adaptations separately from conformance in a review.”

**`.agents/templates/_templates.md`, `## Axioms`**

- `templates/_templates.md:L24` — **Restore:** “Link the sources of continuing guidance or requirements from the new artifact. A Template supplies starting content only.”
- `templates/_templates.md:L28` — **Restore:** “State the need and primary question each Template answers in its description and source instructions.”
- `templates/_templates.md:L29` — **Restore:** “Request necessary content once, in a readable order. Mark optional sections clearly.”
- `templates/_templates.md:L30` — **Restore:** “Treat generic Templates as fallbacks. Add a specialized Template only when its starting content is meaningfully different.”

**`.agents/memory/_memory.md`, `## Axioms`**

- `memory/_memory.md:L13` — **Restore:** “Memory grows through useful records and routed scopes without a fixed structural limit. Unselected branches remain unread. Records need not pass through every state.”
- `memory/_memory.md:L19` — **Restore:** “An accepted record may define current knowledge or decisions. Recording another category’s content does not give the record that category’s role.”
- `memory/_memory.md:L26` — **Restore:** “Show source and uncertainty when they affect trust or acceptance. Distinguish what was stated, observed or inferred.”
- `memory/_memory.md:L27` — **Restore:** “Update affected defining sources before other work depends on the outcome and by closeout. Keep useful context and reasoning in Memory. Accepted durable knowledge must not remain only in chat or a contextual record.”
- `memory/_memory.md:L32` — **Restore:** “Add records and scopes only when their likely future value justifies finding and reviewing them.”
- `memory/_memory.md:L35` — **Restore:** “Named categories and Templates are optional. Use an existing state and the narrowest useful scope. Records may live directly under a state. Do not recreate removed categories to save a record.”
- `memory/_memory.md:L36` — **Restore:** “Reuse a suitable record. Within granted authority, add records or scopes when needed. Ask before placement changes accepted meaning or crosses unresolved authority.”

**`.agents/memory/working/_working.md`, `## Axioms`**

- `memory/working/_working.md:L19` — **Restore:** “An explicitly accepted temporary choice may remain here when its source, scope and expected expiration are clear.”

**`.agents/memory/emerging/_emerging.md`, `## Axioms`**

- `memory/emerging/_emerging.md:L17` — **Restore:** “At each applicable #KeepInMind refresh, check visible Entries and save useful candidate material before it is lost.”
- `memory/emerging/_emerging.md:L23` — **Restore:** “Refine material while its validity, use or final destination remains unsettled.”
- `memory/emerging/_emerging.md:L25` — **Restore:** “Revisit repeated or stale material and decide whether to consolidate, promote, archive or prune it. Review may leave it unchanged.”

**`.agents/memory/archived/_archived.md`, `## Axioms`**

- `memory/archived/_archived.md:L17` — **Restore:** “Retain, consolidate or transform history according to its future value. Archival need not preserve every detail.”

**Additional repairs missed by the ledger**

- **Loader, section 8:** restore stated assumptions, external-effect and reversibility boundaries, declared external authorities, validation versus acceptance, direct-link ancestor recovery, separate chains, inability reporting and overwrite scope. “Narrower rules add to broader ones” does not preserve all these distinctions.
- **README Authoring:** restore compatibility catalogue names, one catalogue per routed folder, metadata defaults, source locality and management boundaries. Contrary to the ledger, `references/routes.md` does not list the compatibility names.
- **Directives/Axioms:** add “Keep Instructions non-empty and understandable without examples. A scoped catalogue may remain on demand. Automatically required rules need a loading tag on every catalogue in their chain.”
- **Patterns/Axioms:** add “State applicability. Label required, optional and repeatable parts, their nesting and relationships, and fixed or illustrative names, values and order. Make comparison with real work possible. Identify omissions in schematic examples.”
- **Templates/Axioms:** add “Preserve required destination structure. Replace metadata to describe ownership, scope, state, authority and relationships. Remove irrelevant package tags. Native SKILL.md keeps native metadata.”
- **Skills/Axioms:** add “Follow the selected SKILL.md for metadata, use, instructions and resources. The agent runtime controls installation, activation, invocation and execution.”
- **Memory/Axioms:** add “Save communication only when it must outlive this context. Preserve outcomes future work needs and worthwhile unsettled findings before closeout. Move records when state, scope or intended use changes. The destination governs their new role. Put accepted behavioral instructions in their defining source. Get explicit user direction before creating a top-level Memory state.”
- **Working/Axioms:** add “Check Entries before deciding that no Working record applies.” **Crystallized/Axioms:** restore “Keep only accepted current knowledge here” and “Keep enough prior context to understand a change.”
- **Archived/Axioms:** restore “Preserve useful provenance. Remove metadata claiming former behavioral roles or current authority. Delete only under user direction or accepted retention preferences. Validate restoration against current conditions, name its destination and establish acceptance before using it as current.”
- **Emerging metadata and Memory/Entries:** restore `KeepInMind`. The folder remains optional. Removing its refresh behavior requires an explicit customization, including during upgrades.
- **CLI Skill introduction:** replace its metadata pointer with “Before authoring routed context, read `../../README.md#authoring`.” In `references/routes.md`, add the same requirement using `../../../README.md#authoring`. In `references/packages.md`, state “Management grants no semantic authority. Linked workspace documents remain user-owned.”
- **Public documentation:** update README’s later startup descriptions, `intro.md`, `getting-started/first-task.md`, `concepts/{index,routing,loading-and-tags,core-categories,memory,customizing}.md`, and diagram annotations together. Replace obsolete universal startup claims with “Startup follows the installed loader’s tagged Entries and explicit required reads.” Full Core currently contains **21 Markdown files**, not the documented 19. Update current contracts and the dictionary alongside them, preserving historical council records.

### The complexity ladder

These are useful stopping points, not runtime modes.

Let **B** mean `AGENTS.md`, the `CLAUDE.md` bridge and `.agents/{loader.md,README.md}`. CLI installations also maintain settings and lock files.

| Level | Installed tree | Benefit and cost | Adjacent movement |
| --- | --- | --- | --- |
| Link existing context | B, linking an existing document | Reliable entry path. Maintain descriptions and links | Add Directives and Crystallized. Remove only Open Forge additions to leave |
| Rules and accepted knowledge | B plus `directives/_directives.md`, `memory/_memory.md`, `memory/crystallized/_crystallized.md` | Corrections and accepted knowledge have homes. Review authority and maintenance | Add Essentials’ remaining routes, or disable these loader entries |
| Continuity | Essentials, including Patterns, Skills, Emerging, Working and `saving.md` | Resume work and review candidates. More reading and record upkeep | Add omitted Full Core routes, or release and disable unwanted routes |
| Full framework | Previous level plus Guidance, Maps, Templates and Archived | All built-in roles. Maintain only useful content | Add selected Extensions, or return to Essentials |
| Specialized methods | Full framework plus selected Extension payloads and dependencies | Planning, document methods or coordination. Package-specific obligations | Remove reviewed packages to return |

A brownfield project should start with **one existing link**. It can test retrieval before accepting new storage or working habits. It may remain there indefinitely.

Moving down means disabling loader entries and separately releasing management. Custom `remove` preserves files and routability. It is not a disable command.

### Ultra-lightweight install

Add **`--preset minimal`**, displayed as “Existing documents”. It writes B, settings and ownership records. Its loader has an empty Entries list. It creates no category folders, CLI Skill, Memory states, saving policy, Extensions or Git-ignore rules.

Allow `--route` with `minimal` as well as `custom`. Minimal starts with all ten rows removed, then applies overrides. Selecting a Memory state writes its necessary Memory parent without enabling siblings. Preserve existing authored entries and report retained active content.

Example:

```sh
open-forge install --preset minimal --route directives=add --route memory/crystallized=add --dry-run
```

Plan wording: “Install the reading procedure and human guide. Add Directives and Crystallized Memory. Leave other supplied categories out. Existing documents stay in place.” List exact host-block, settings, navigation and file effects.

Grow through existing `route init <category> --framework`, or reviewed `install --configure`. Keep existing unattended defaults unchanged.

C# work includes:

- Install’s `Models/Configuration/InstallConfiguration.cs`, `InstallConfigurationChoices.cs`, `InstallBindingInputReader.cs`, `InstallBindingModels.cs`, `InstallSetupResolver.cs` and selection planning.
- `FrameworkPayloadSelection.cs`, ensuring an empty selection omits Memory’s container.
- Rendering’s `InstallSetupQuestions.cs`, `InstallSetupRecapSelector.cs`, `InstallReportSelector.cs`, plus `InstallSetupText.cs` and `InstallInputText.cs`.
- `GeneratedNavigationRegionPlanner.cs`: when preserving membership, an empty listed set must remain empty. Its current `listed.Count == 0` branch returns structural roots.
- `RouteScaffoldComposer.cs`, `WorkspaceAdoptionEntrypointComposer.cs` and `RouteInspectAxiomsProfileBuilder.cs` for the period-separated inherited marker, retaining recognition of the old marker.

Cost is preset policy, help, focused install/index/upgrade tests and snapshots. The marker adds a small compatibility change. Entries syntax, frontmatter schema and loading-tag parsing remain unchanged.

### Brownfield adoption

These commands target the proposed v3 CLI. Today’s CLI has no `minimal` preset.

1. Inspect `git status --short` and `rg --files docs`. Identify existing instruction entry points and authoritative documents. Agents still follow the repository’s existing instructions.
2. Preview and apply:

   ```sh
   open-forge install --preset minimal --frontmatter root --dry-run
   open-forge install --preset minimal --frontmatter root --automatic
   ```

   Review the first plan before the second command. Agents now read the existing host instructions and loader. No category policy is active.

3. Add these loader Entries. Here the inspected repository contains `docs/architecture.md`. Use the actual existing filename:

   ```markdown
   - [Contribution rules for every task](../CONTRIBUTING.md) - #LoadNow
   - [Project overview when orientation is needed](../README.md)
   - [Architecture for source work](../docs/architecture.md) - #LoadNow - applies to `src/**`
   ```

   For source work, agents read CONTRIBUTING and architecture. README stays available by relevance. Existing host instructions remain active.

4. Check the intended read set for an actual or planned working path:

   ```sh
   open-forge context --for src/Orders.cs
   ```

   Compare the returned files with the entries. Plain-file readers perform the same checks.

5. When repeated corrections or accepted knowledge need their own homes:

   ```sh
   open-forge route init directives --framework --dry-run
   open-forge route init directives --framework
   open-forge route init memory/crystallized --framework --dry-run
   open-forge route init memory/crystallized --framework
   ```

   Review each preview. Agents additionally read Directives, Memory and Crystallized catalogues. They gain no Working or saving obligation.

Open Forge must never move existing documents, rewrite their authority, replace host text outside its block, infer acceptance from code, or activate omitted categories during maintenance.

Worth building: install help explaining these boundaries and a plan distinguishing retained files from active entries. Automatic document classification can wait.

### The introduction

Replace README’s opening through the section preceding `## How it works` with:

````markdown
## Open Forge

Open Forge helps coding agents find and use the rules, documentation and decisions your project already has. You choose what they read through a short Markdown list. Add places for project rules, accepted knowledge or resumable work when those become useful.

Your documents stay where they are. Open Forge works with plain files and any agent tool that reads `AGENTS.md`, or the `CLAUDE.md` bridge. The optional CLI gathers context and maintains links.

### Start with one existing document

With the CLI installed, run this from your project:

```sh
open-forge install --preset minimal --dry-run
```

Review the plan, then apply the same selection:

```sh
open-forge install --preset minimal --automatic
```

Minimal adds the reading procedure and a human guide. It adds no category folders or automatic saving policy. Existing `AGENTS.md` and `CLAUDE.md` text stays outside the marked Open Forge section.

Add one line under `## Entries` in `.agents/loader.md`:

```markdown
- [Contribution rules for every task](../CONTRIBUTING.md) - #LoadNow
```

Use a document your repository actually has. It needs no move or frontmatter. `#LoadNow` asks agents to read it whenever its parent loads.

Now give your agent a normal task:

> Explain how this project runs its tests. Follow AGENTS.md and name the sources you used.

Check that it used your existing instructions. Open Forge gives the agent a reading procedure. Review still matters.

### Add only what helps

You can stop with that one link, or choose another useful setup.

### Project rules and accepted knowledge

Preview a small setup with Directives and Crystallized Memory:

```sh
open-forge install --configure --preset minimal --route directives=add --route memory/crystallized=add --dry-run
```

Apply the reviewed request without `--dry-run`, adding `--automatic` for unattended execution.

Directives hold required agent behavior. Crystallized Memory holds accepted knowledge that should stay current. Its Memory parent is included. Working records, candidate review and automatic saving remain absent.

For an existing C# conventions document, another loader entry is enough:

```markdown
- [C# conventions for C# work](../docs/csharp.md) - #LoadNow - applies to `**/*.cs`
```

The condition limits reading and application to matching working files.

### Work that continues across sessions

Essentials adds Working and Emerging Memory, reusable shapes and native Skills:

```sh
open-forge install --configure --preset essentials --dry-run
```

Working includes a separate policy for saving unfinished work before a pause. You can remove that policy while keeping records. Essentials keeps new Working contents out of ordinary Git commits and leaves its catalogue shareable. Already tracked files stay tracked. Copy private notes separately when changing machines.

### All built-in roles

Choose `full-core` when you also want advice, curated pointers, starting files and archived history:

```sh
open-forge install --configure --preset full-core --dry-run
```

Full Core installs every built-in category and Memory state. Extensions remain separate choices.

### Keep your own structure

Link existing documentation directly. Use Open Forge folders where they help. Keep ADRs, contribution guides and architecture documents at their current paths.

Removing a loader entry disables that branch while preserving its files. Custom's `remove` action releases supplied-default management and keeps existing files routable. Deleting files is a separate operation.

Every default is editable and removable. For an adjustment that survives CLI updates, put it in an adjacent `{name}.overwrite.md`. Review update plans before applying them.

### Without the CLI

Copy `AGENTS.md`, the `CLAUDE.md` bridge if needed, `.agents/loader.md` and `.agents/README.md` from `src/open-forge/`. Merge host blocks into existing files. Start the loader with an empty Entries list, then add your first link.

Maintain Entries by hand. The reading rules, authority boundaries and customization all work without the CLI.

See the [installation guide](https://thelithiumforge.github.io/open-forge/docs/getting-started/installation) for CLI installation methods and the [growth guide](https://thelithiumforge.github.io/open-forge/docs/getting-started/grow-your-framework) for adding individual categories.
````

Complete `.agents/README.md`:

````markdown
## Open Forge in this project

Start with `loader.md`. Its Entries enable project context, while tags and the task determine when agents read it. This file explains customization and supplies the authoring reference required by the loader.

### Link an existing document

Add a loader entry. The document stays in place and needs no frontmatter:

```markdown
- [Contribution rules](../CONTRIBUTING.md) - #LoadNow
- [C# conventions](../docs/csharp.md) - #LoadNow - applies to `**/*.cs`
```

Use actual paths. Leave off the loading tag for material opened by relevance. Conditions filter reading and application. They do not grant edit permission.

### Add a category

With the CLI, preview `open-forge route init directives --framework --dry-run`, then apply it without `--dry-run`.

Without the CLI, copy the wanted catalogue from the payload and link it from the loader. A Memory state also needs its Memory parent. Keep only entries for installed states.

A Directive keeps required behavior under a non-empty `## Instructions` heading and carries `LoadNow`. State its conditions and exceptions. Optional categories may remain absent.

### Disable, stop managing or delete

Remove a loader entry to disable a branch while keeping its files. Accepted project requirements and independently loaded host instructions remain active. Check enabled dependencies first.

Inside category catalogues, Entries follow the files. Move or delete a child to remove its entry. For CLI-managed files, record intentional omissions before updating. Settings use `removedCategories`, `removedDirectories` and `removedFiles`. Preview a Remove command when deletion is intended.

Custom's `remove` action releases Framework management and preserves files and routability. It does not disable their loader entry.

### Keep work between sessions

Working Memory holds resumable records. Its optional `memory/working/saving.md` policy asks agents to save unfinished work before pausing.

To stop that policy, remove its file and its entry from `_working.md`, and record the omission for CLI updates. Records remain. Removing Working does not require replacing it with another state.

### Keep local changes through updates

An adjacent `{name}.overwrite.md` loads after its base and wins only for corresponding content. It shares the base's route, role, scope and loading behavior. Do not index it separately.

A direct edit to a managed base can be replaced by Update. Preview updates and inspect their recovery information.

### Authoring

Read this section before creating, editing or moving routed context, catalogues or overwrites. These rules apply with or without the CLI.

Connect related material with relative local links and precise references. Reuse consistent descriptive tags for shared subjects. Add links and tags only when they help navigation or retrieval.

Put detail in the narrowest source that defines its question. Higher-level sources keep only the summary needed to select that detail and a link. Do not repeat inherited rules.

A route is a navigable path exposed through catalogues and Entries.

A root route is exposed directly by the loader.

A scope is part of a route that narrows where following content applies.

A slug is a concrete folder name in a route.

Scopes can appear before, between or after deeper route segments. Each narrows everything after it, with no fixed depth.

A scope contains only useful routes. It need not copy another scope or the installed defaults.

For example, a C# scope may contain a #LoadNow design file and an on-demand Windows scope. Selecting C# loads its design rules, not Windows. A #LoadNow file under Windows loads only after Windows is selected.

Each routed folder has one catalogue, normally `_{folder-name}.md`. Compatibility names are `index.md`, `_index.md`, `references.md` and `_references.md`.

For a new catalogue without local rules, put only `- inherited - No local axioms. Loaded ancestor axioms remain active.` under `## Axioms`. Missing or empty Axioms sections add no rules. Never combine the inherited marker with local rules.

New routed Markdown uses `description`, `tags` and optional `applyTo` frontmatter. Follow `frontmatter` in `.agents/open-forge.json`. Values are `root` or `scoped`. Missing means scoped. Scoped fields sit together under `open-forge:`. Native SKILL.md keeps native metadata.

Descriptions help readers decide whether to open a source. Optional `responsibility` states what it defines and grants neither authority nor loading behavior. Tags are bare names without `#` in frontmatter.

An `open-forge:` mapping supplies ordinary Open Forge metadata without merging root fields. `applyTo` is read at both locations. Equivalent pattern sets count once and conflicting sets are invalid. Lists contain atomic patterns. String expressions may contain comma-separated patterns.

Entries repeat descriptions, tags and conditions. External linked documents need none of this metadata added to their bodies. Keep their metadata on the entry line.

#Core identifies base routing, loading, workspace orientation and reusable agent-facing roles.

#Memory identifies self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history.

#Extension identifies optional packaged routes, capabilities, integrations and support files.

Each Guidance file states its situation, recommended approach, reasons and tradeoffs. Each Map destination says what it contains and when to use it.

Management determines which files a manager may change, not their authority. A manager changes only files it owns or can safely identify for the requested operation, using its supported route shapes and preserving segment order through scopes. Familiar names and tags confer neither a Core role nor management.

With the CLI, preview Index after additions and Repair after moves. Read the installed CLI Skill before commands other than Context when that Skill is enabled. If absent, use command help. Without the CLI, maintain metadata, Entries and affected links directly.
````

### v3

Complete `.agents/loader.md` for the Full Core payload. Install filters Entries to the selected setup.

```markdown
## Open Forge Loader

This file tells agents how to use the context this project has chosen. It works with plain files and adds to the project's other instructions. The Entries list enables project context. Tags and the task determine when to read it.

### Axioms

### Read before working

1. At task start, on resume and after context restoration, identify your working files. Include files you will investigate, review, create, change, rename or delete, including planned paths and both sides of a rename. Reading a file for background does not make it a working file.
2. If the CLI is available and allowed, run `open-forge context` with `--for <file>` for every known working file, then read its output. Omit `--for` while paths are unknown. This gathers the tagged reads in steps 3 and 4. You must still do step 5. If the command fails or cannot follow a link, complete the reads with file tools.
3. Starting with Entries below, read each applicable #LoadNow or #KeepInMind destination in listed order. Check step 4 before opening it. A catalogue, also called an entrypoint, lists further destinations. Repeat this process inside every catalogue you read. Do not skip a required read because its description seems unrelated. Read each source once per pass.
4. Conditions filter reading and application. Patterns within one `applies to` list are alternatives. The same working file must match that list and every conditioned ancestor. Apply the source only to matching files. Globs are case-sensitive and workspace-relative. `*.py` matches root files, `**/*.py` any depth and `docs/**` a folder's contents. A leading `!` is literal. Report required conditioned reads as pending while paths are unknown and resolve them before affected work. Other conditioned entries stay on demand. Conditions neither select an unopened branch nor grant or restrict edit permission.
5. Use catalogues for navigation. Do not open ordinary file bodies just to discover routes. Open other entries when their descriptions, tags, paths or explicit links fit the task. Read anything a loaded source explicitly requires. Such links cannot reactivate disabled branches. Report dependencies on disabled context. Before using a routed file found through search or a direct link, read its listed catalogue ancestors and required context. Keep separately selected chains separate.

Restart when the goal or working files change. Reread a source if changes make its earlier reading obsolete. Refresh #KeepInMind sources only in active branches, before handoff or finish. During work, reread them only when they may have changed. Refresh a catalogue's required children too. Read each overwrite immediately after its base.

### Use the context

- Follow this loader's Axioms, loaded catalogue Axioms and applicable rule-file Instructions. Children add narrower rules without cancelling ancestors or gaining higher authority. Report conflicts and instructions you cannot follow, with the reason.
- Follow user direction within platform and safety limits. Make routine, reversible choices within the request without asking again. State assumptions affecting the result. Use accepted context to resolve uncertainty. Ask before relying on an unresolved choice that significantly changes the result, scope, risk, cost, external effects or ability to undo the work.
- Match explanation and planning to the request. Unless deeper analysis is requested, begin with your current understanding, one recommendation and at most one important open choice.
- Use the fewest words that preserve meaning, readability and logical progression. Remove filler and repetition. Keep necessary context, conditions and exceptions.
- Reading, linking, saving, moving, tagging, repetition and confidence establish neither acceptance nor permission to edit. Accepted means agreed by the user or established by a declared authority within its assigned scope. Use external authorities for the facts assigned to them. Validation checks evidence. Acceptance determines what may be treated as current.
- #Contextual marks unaccepted context unless applicable authority establishes acceptance. #CurrentTruth identifies accepted current state within its scope. Neither tag grants authority. Selected non-binding material may specialize broader material of the same kind within its accepted scope.
- Let each source answer one clear question. Link related detail rather than repeating it. A link does not merge authority, scope, loading rules or lifecycle.
- Investigate apparent conflicts between accepted sources before changing either. When accepted direction changes current state, update its defining source and preserve useful prior context.
- #Evergreen marks content that must stay aligned with accepted current state. It grants neither authority nor automatic reading. Update affected sources you may edit before work depends on them and by closeout. Batch related updates when safe. Keep each source aligned with what it represents now. Preserve useful prior context in an appropriate enabled Memory state. Report blocked or unauthorized updates.

### Change the setup

Before creating, editing or moving routed context, catalogues or overwrites, read the [authoring reference](README.md#authoring). This reference is part of the reading procedure, even in a setup with no category folders.

- Add an Entries line to enable a catalogue or Markdown file anywhere inside the workspace. For example, `../docs/architecture.md` keeps the existing document in place. It needs no frontmatter. The entry carries its description, tags and conditions.
- Remove a loader entry to disable that branch while keeping its files. Reading disabled files for background does not reactivate their rules. Accepted project requirements and instructions loaded independently by the host remain in force. Report any enabled dependent before disabling its source. Do not restore disabled content unless asked.
- Inside category catalogues, Entries follow the folder's files. Move or delete a child file to remove its entry. If the CLI manages it, also record the exclusion as the authoring reference explains. Disabling, releasing management and deleting files are different actions.
- Report missing listed files as errors. An empty Entries list is valid and must stay empty during indexing.
- #LoadNow requires reading when its parent loads. #KeepInMind also requires refreshes above. A required descendant needs a loading tag on every catalogue between it and this loader. Narrow specialized branches before making them mandatory. Use #LoadNow only when missing the content costs more than reading it each time. #KeepInMind is for rereading, not importance. Other tags support classification and search.
- An adjacent `{name}.overwrite.md` loads immediately after `{name}.md`, with the same route, role, scope and loading behavior. Do not list it separately. It wins only for corresponding content in its base, never over unrelated sources or active ancestor rules.
- Before other CLI commands, read the enabled CLI Skill when present. If it was omitted, use the command's `--help` and the authoring reference. The CLI is optional.

### Entries

- [Rules agents must follow in this workspace](directives/_directives.md) - #LoadNow #Core #Directive
- [Advice to check before recurring choices](guidance/_guidance.md) - #Core #Guidance
- [Pointers to check before seeking project knowledge](maps/_maps.md) - #Core #Map
- [Accepted knowledge and notes for continuing work](memory/_memory.md) - #LoadNow #Memory
- [Default shapes to check before creating, changing or reviewing structure](patterns/_patterns.md) - #Core #Pattern
- [Capabilities to check before specialized work](skills/_skills.md) - #Core #Skill
- [Starting files to check when copy-ready content would help](templates/_templates.md) - #Core #Template
```

Complete managed `AGENTS.md` block:

```markdown
## Open Forge

Before starting work, read `.agents/loader.md` and follow its reading procedure and applicable rules throughout the task.

If the CLI is available and allowed, `open-forge context --for <file>` gathers tagged required reads. Repeat `--for` for each working file. The loader explains conditions, additional reads and the fallback to file tools.

**End of Open Forge managed section.**
```

The five largest sentence-level repairs are:

| Before | After | Why |
| --- | --- | --- |
| “That covers steps 3 and 4.” | “This gathers the tagged reads in steps 3 and 4.” | Bounds the CLI promise |
| “Narrower rules add to broader ones.” | “Children add narrower rules without cancelling ancestors or gaining higher authority.” | Restores precedence limits |
| “Every other tag is a search label.” | “Other tags support classification and search.” | Allows retained semantic duties |
| “Accepted means the user agreed, or a source the user named says so.” | “Accepted means agreed by the user or established by a declared authority within its assigned scope.” | Restores assigned authority |
| “Lines inside other catalogues follow the files in their folder, so to turn one of those off, delete or move its file.” | “Inside category catalogues, Entries follow the folder's files.” | Separates the mechanism from the action |

Whitespace-delimited counts, including headings and Entries: **loader 1,159 words**, **managed block 58 words**. The loader is 336 words longer than v2 and 893 shorter than the original. Restored obligations explain the increase.

### Risks and proof

The authoring reference could become an unread appendix. Its explicit loader trigger needs observation. Restoring Emerging refreshes increases context cost. Existing installations must retain their loading tags, overwrites and saving behavior through an upgrade. Git-ignored Working policy reachability also needs a fresh-checkout check.

The cheapest introduction experiment uses three unfamiliar maintainers and their existing repositories. Give them only the replacement introduction. Ask each to link one document, add a scoped rule, move up one level and disable one policy. Start without the CLI, then repeat retrieval with it. Observe actual reads, completion time, assistance needed and whether disabled behavior returns after maintenance.

Pair this with small fixtures covering all restored timing and authority obligations. Preserve intentionally empty Entries through Index. Report startup counts that exclude outside documents as partial estimates.

This proposal is based on read-only inspection at `f5b469d64`. No files, builds or test suites were changed or run.

### The one idea to steal

Make every simplification carry an auditable receipt showing where its old obligation lives and exactly when the person or agent who needs it will encounter it.

---

## Model-Behavior Realist (Proposal C)

/root - GPT-6 - high

### Thesis

Keep v2’s activation model and restore duties where they apply. Every installation should use one mandatory reading procedure, with an optional CLI shortcut and a complete file-tool fallback. Users choose the destinations, and enabling a destination brings its full rules.

### How v2 removed so many rules

I see six kinds of removal in the ledger.

| Kind | Two examples | Verdict |
| --- | --- | --- |
| Removed vocabulary and illustrations | Loader L46 defines `route`, L85 illustrates nested loading | Sound reductions in startup reading when explanations remain available to people |
| Compressed several obligations into one sentence | Memory L26 loses stated versus observed versus inferred, Maps L25 loses its explicit Memory boundary | Sound only where the surviving sentence actually implies every obligation |
| Moved general duties into a particular category | Loader L27 survives mainly in Crystallized, L100’s Evergreen duty becomes record maintenance | Lossy for sources outside that category |
| Removed timing or verification details | Loader L94 loses refresh after possible changes, Patterns L28 loses separate reporting of departures | Lossy because the remaining action does not imply its timing or reporting requirement |
| Treated authoring rules as ordinary competence | Patterns L25 loses backlinks, Templates L24 loses links to continuing requirements | Lossy because competent agents still produce disconnected copies and examples |
| Delegated specialist detail to CLI material | Loader L50 moves managed-route detail, L116 moves `responsibility` metadata detail | Sound for CLI operations, provided manual operation remains documented |

The large reduction therefore came from both better editing and a smaller behavioral contract. “Obvious” does not establish redundancy, and moving a general duty into optional Crystallized Memory does not preserve its original reach.

### The lossless ledger

Paths below describe the installed tree. Their shipped counterparts are under `src/open-forge/`. Each quoted addition is binding unless identified as explanatory text. Retain surviving v2 text around these additions.

**Destination: `.agents/loader.md`, `## Axioms / Use what you read`**

- Loader L21 — **Restore:** “Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.”
- Loader L22 — **Restore:** “Use the fewest words that preserve meaning, readability and logical progression. Remove filler and repetition, while retaining necessary context, conditions and exceptions in communication and authored content.”
- Loader L27 — **Restore:** “Let each source answer one clear question. Link related detail instead of repeating it. Links do not change authority, scope, loading or lifecycle.”
- Loader L32 — **Restore:** “Investigate conflicting accepted current sources before changing either. When accepted direction changes current state, update its defining source and preserve useful prior context.”
- Loader L40 — **Restore:** “Selected non-binding material may specialize broader material of the same kind within its accepted scope.”
- Loader L100 — **Restore:** “#Evergreen requires alignment with accepted current state, without creating authority or loading behavior.”
- Loader L101 — **Restore:** “Update affected sources you may edit before work relies on them and no later than closeout. Batch related updates when safe.”
- Loader L102 — **Restore:** “Keep each source aligned with what it represents now. Preserve useful prior context in an enabled Memory route. Report blocked or unauthorized updates.”

**Destination: `.agents/loader.md`, `## Axioms / Maintain the setup`**

- Loader L28 — **Restore:** “Connect related material with relative local links and precise references. Reuse consistent descriptive tags for shared subjects. Add links and tags only when they help navigation or retrieval.”
- Loader L37 — **Restore:** Use the complete inherited-marker instruction reproduced in v3 below, including the existing literal marker, the treatment of empty sections and the prohibition on combining the marker with local rules.
- Loader L57 — **Restore:** “Folders can narrow a chain at any depth, including before, between or after category segments. Each narrows everything that follows it.”
- Loader L58 — **Restore:** “Include only useful routes, without copying another scope or the defaults.”

**Destination: `.agents/loader.md`, `## Axioms / Read before you work`**

- Loader L62 — **Restore:** “Navigate through catalogues, without opening other file bodies merely to discover routes or tags.”
- Loader L93 — **Restore:** “Before handoff or finish, reread #KeepInMind sources in active scopes only.” Step 3 also restores navigation without opening ordinary bodies.
- Loader L94 — **Restore:** “During work, refresh those sources only when they may have changed.”

**Destination: `.agents/README.md`, `## Terms you may encounter`**

These are explanations, so moving them out of startup preserves their useful reach.

- Loader L46 — **Relocate:** “A route is a navigable path exposed through catalogues and Entries.”
- Loader L47 — **Relocate:** “A root route is a route exposed directly by the loader.”
- Loader L48 — **Relocate:** “A scope is a part of a route that narrows where the following content applies.”
- Loader L49 — **Relocate:** “A slug is a concrete folder name in a route path.”
- Loader L95 — **Relocate:** “#Core labels base routing and loading, workspace orientation, and reusable agent-facing roles.”
- Loader L96 — **Relocate:** “#Memory labels self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history.”
- Loader L97 — **Relocate:** “#Extension labels optional packaged routes, capabilities, integrations and support files.”

**Destination: `.agents/README.md`, `## Keep specialized context local`**

- Loader L85 — **Relocate:** “A C# catalogue may contain a #LoadNow design file and an on-demand Windows catalogue. Selecting C# loads its design file, not Windows. Required files inside Windows load only after Windows is selected.”

**Destination: `.agents/maps/_maps.md`, `## Axioms`**

- Maps L23 — **Restore:** “Keep Maps broad. Prefer modules, projects, repositories, systems or scopes over members and functions unless finer routing clearly helps.”
- Maps L24 — **Restore:** “Choose Map filenames, grouping and nesting depth for this workspace.”
- Maps L25 — **Restore:** “Maps do not replace Memory or the destination’s current information.”

**Destination: `.agents/patterns/_patterns.md`, `## Axioms`**

- Patterns L19 — **Restore:** “Create or update a Pattern only when an accepted shape should guide future related work. Do not promote one-off work, temporary transitions or unsettled candidates merely because they have structure.”
- Patterns L25 — **Restore:** “Link related rules, advice, maps, procedures and starting files. Identify which sources bind the shape. Sources applying the shape should link back to its Pattern.”
- Patterns L26 — **Restore:** “Keep required behavior, procedures and substantial choice rationale in their defining sources. Example names and values are illustrative unless an applicable requirement fixes them.”
- Patterns L28 — **Restore:** “Adapt within the authority granted for the work. Explain material departures before dependent work. Reviews report justified adaptations as departures, separately from conformance. Follow shapes made mandatory by applicable rules or accepted requirements.”

**Destination: `.agents/templates/_templates.md`, `## Axioms`**

- Templates L24 — **Restore:** “If the new artifact needs continuing guidance or requirements, link their defining source. A Template supplies starting content only.”
- Templates L28 — **Restore:** “State each Template’s need and primary question in its description and source instructions.”
- Templates L29 — **Restore:** “Request necessary content once, in a readable order. Mark optional sections clearly.”
- Templates L30 — **Restore:** “Treat generic Templates as fallbacks. Add a specialized Template only for meaningfully different starting content.”

**Destination: `.agents/memory/_memory.md`**

- Memory L13 — **Restore**, opening: “Memory is self-growing Markdown state. People and agents can add useful records and routed scopes without a fixed structural limit. Unselected branches do not load their contents. Records need not pass through every state.”
- Memory L19 — **Restore**, `## Axioms`: “An accepted record can define current knowledge or decisions. Recording another category’s content does not give the record that category’s role.”
- Memory L26 — **Restore**, `## Axioms`: “Distinguish what was stated, observed or inferred.”
- Memory L27 — **Restore**, `## Axioms`: “Update affected sources before other work depends on the outcome and no later than closeout. Keep useful context and reasoning in Memory. Accepted durable knowledge must reach its defining source, rather than remain only in chat or an unaccepted record.”
- Memory L32 — **Restore**, `## Axioms`: “Add records or routed scopes only when likely future value justifies the cost of finding and reviewing them.”
- Memory L35 — **Restore**, `## Axioms`: “Named record categories and Templates are optional. Use an existing state and the narrowest useful scope. Records may live directly under a state.”
- Memory L36 — **Restore**, `## Axioms`: “Within granted authority, add an ordinary record or scoped route when needed. Ask before placement changes accepted meaning or crosses an unresolved authority boundary.”

**Destination: `.agents/memory/working/_working.md`, `## Axioms`**

- Working L19 — **Restore:** “An explicitly accepted temporary choice may remain here when its source, scope and expected expiration are clear.”

**Destination: `.agents/memory/emerging/_emerging.md`, `## Axioms`**

- Emerging L17 — **Restore:** “At each applicable #KeepInMind refresh, check visible Entries and save useful candidate material before it is lost. Nothing useful to save is a valid result. Do not record raw activity.”
- Emerging L23 — **Restore:** “Refine material while its validity, use or final destination remains unsettled.”
- Emerging L25 — **Restore:** “Revisit repeated or stale material and decide whether to consolidate, promote, archive or prune it. Review may leave a record unchanged.”

Restore `KeepInMind` in Emerging’s frontmatter and its parent entry too. Otherwise restoring L17 would leave its default trigger absent. The saving policy remains separately removable.

**Destination: `.agents/memory/archived/_archived.md`, `## Axioms`**

- Archived L17 — **Restore:** “Retain, consolidate or transform history according to its future value. Archival need not preserve every detail.”

These are all 34 Dropped and 12 Partly rows. None needs an unsupported claim of redundancy.

### The complexity ladder

These are useful stopping points, not mandatory stages. The common base is the host instruction block plus `.agents/loader.md` and `.agents/README.md`. CLI installs also maintain the two existing JSON files.

| Level | Installed tree beyond the common base | What users get and pay | Move one level |
| --- | --- | --- | --- |
| 1. Connect a document | No category folders. One loader link to existing Markdown | Existing instructions become reachable. Cost is the loader and the chosen document’s reads | Up: add Directives. Down: remove the managed block and retain project documents |
| 2. Add project rules | `directives/_directives.md` and chosen rule files | Binding rules with explicit scope. Cost is maintaining those rules and reading applicable ones | Up: add Memory and Crystallized. Down: remove the Directives loader entry |
| 3. Keep accepted knowledge | `memory/_memory.md`, `memory/crystallized/_crystallized.md`, ordinary records | Accepted knowledge stays current. No mandatory Decision taxonomy. Cost is acceptance and maintenance | Up: add Working and `saving.md`. Down: disable Memory after preserving accepted requirements in their defining sources |
| 4. Resume work | `memory/working/_working.md`, `saving.md`, resume records | Continuity across sessions. Cost is concise updates before pausing | Up: add other categories and states. Down: remove the saving policy, then disable Working if unwanted |
| 5. Use the full framework | Guidance, Maps, Patterns, Skills with CLI references, Templates, Emerging and Archived catalogues | Advice, shapes, discovery, candidates and history. Extensions remain separate choices | Down: remove unwanted loader entries and record lifecycle omissions |

Brownfield projects should start at level 1. Their first success should be an existing instruction used correctly in a fresh session. They can also choose Directives plus Crystallized immediately without installing Working, Emerging or an Extension.

### Ultra-lightweight install

Add **`--preset minimal`**. It uses the ordinary loader and existing selection machinery.

On a fresh workspace it writes:

- Managed sections in `AGENTS.md` and `CLAUDE.md`
- `.agents/loader.md` with an empty `## Entries`
- `.agents/README.md`
- `.agents/open-forge.json` and `.agents/open-forge.lock.json`

It installs no category folders, Skill, Memory state, saving policy, Extension or Git-ignore section.

Allow existing `--route <id>=<add|remove|git-ignore>` overrides with `minimal` as well as `custom`. Unspecified Minimal rows mean `remove`. For example:

`open-forge install --preset minimal --route directives=add --route memory/crystallized=add --dry-run`

That installs only those categories and Crystallized’s necessary Memory parent. Derive delivery of the Memory parent from selected states. Avoid a broad Memory exclusion that would obstruct later one-state restoration.

The plan should say: “Create the loader and human guide, add the managed agent entry sections, and record management. No categories or automatic saving are selected.” It should list every touched path and retain existing documents.

Existing installations require `--configure`. Existing files remain preserved and routable under current `remove` semantics. Disabling remains an explicit loader edit. Keep the unattended no-preset default unchanged for compatibility.

The C# work is bounded but crosses several surfaces:

- Under `Commands/Install/`: `Models/Configuration/InstallConfiguration.cs`, `InstallConfigurationChoices.cs`, `InstallBindingInputReader.cs`, `InstallBindingModels.cs`, `InstallSetupResolver.cs`, `InstallConfigurationSettings.cs` and `InstallNextCommand.cs`
- Rendering: `InstallSetupQuestions.cs`, `InstallReportSelector.cs` and `InstallSetupRecapSelector.cs`
- Output text: `InstallSetupText.cs`, `InstallInputText.cs` and `InstallText.cs`
- Framework: `FrameworkPayloadSelection.cs` and `GeneratedNavigationRegionPlanner.cs`

The last file currently returns structural roots when the preserved loader list is empty. Fix that before advertising Minimal.

Cost: an additive preset, selection and retry coverage, empty-membership regression coverage, install/update/Index journeys, help and output snapshots. No Markdown grammar change, new settings schema or persisted mode registry is needed.

### Brownfield adoption

The following describes the proposed v3 interface. Commands are instructions for adoption, not actions performed during this review.

1. Inspect the existing instructions and documents with `git status --short`, `git diff`, and `rg --files docs`. Read `README.md`, `CONTRIBUTING.md`, and whichever agent host files exist. Nothing changes.
2. Run `open-forge install --preset minimal --frontmatter root --dry-run`. Inspect the complete plan, then run the same command with `--automatic` instead of `--dry-run`. The agent now reads its existing host instructions and the loader. An empty loader adds no document reads.
3. Add these exact lines under the loader’s `## Entries`:

   `- [Contribution instructions](../CONTRIBUTING.md) - #LoadNow`

   `- [Project overview and documentation links](../README.md)`

   The agent now always reads contribution instructions. It opens the overview when relevant and follows required reads inside loaded documents.
4. Leave `docs/` in place. Add individual documents only after inspecting them, using their actual relative paths and useful descriptions. Add conditions where necessary. Linking a document does not declare its contents accepted.
5. When a recurring correction needs a dedicated rule, preview and apply `open-forge route init directives --framework --dry-run`, then `open-forge route init directives --framework`. Add the rule and its catalogue entry as shown below. The agent now reads the catalogue and applicable required rules.
6. When accepted knowledge needs a home, similarly preview and apply `open-forge route init memory/crystallized --framework`. Only the necessary parent and selected state should appear. The agent checks their Entries before deciding no accepted knowledge applies.

Open Forge must preserve existing host text, document locations, valid metadata, native Skills and authored notes. It must not convert all documentation into its taxonomy, retroactively invent decisions, silently enable siblings, or treat existing code as accepted intent.

Worthwhile CLI help is a plan suggesting existing documents and showing copyable entry lines. Automatic classification or a new adoption wizard is unnecessary for the first release.

### The introduction

Replacement README introduction and starting section:

```markdown
## Open Forge

Open Forge helps coding agents find and use the rules, documents and decisions your project already has. It is plain Markdown with a reading procedure and links. You choose how much structure to add.

Start with one existing document. Add project rules, accepted knowledge or resumable notes when they solve a problem you have.

### Get started

With Node.js 22.18 or later, install the optional CLI:

~~~sh
npm install -g @thelithiumforge/open-forge@beta
~~~

From your project, preview the smallest installation:

~~~sh
open-forge install --preset minimal --frontmatter root --dry-run
~~~

Review the listed paths, then apply that selection:

~~~sh
open-forge install --preset minimal --frontmatter root --automatic
~~~

Install adds a marked section to AGENTS.md and CLAUDE.md and preserves your existing instructions. It creates the loader, a human guide and CLI management files. It adds no category folders.

Inspect new files as well as tracked changes with git status and git diff.

### Start with an existing document

Add this under `## Entries` in `.agents/loader.md`:

~~~markdown
- [Contribution instructions](../CONTRIBUTING.md) - #LoadNow
~~~

Use a file that exists in your project. It stays where it is and needs no frontmatter.

Now ask your agent:

> Follow AGENTS.md. Explain how this project expects contributors to check a change, and identify the instructions you used.

`#LoadNow` requires reading when the parent loads. Without a loading tag, an entry opens when it fits the task. Linking requires reading without making an unaccepted proposal authoritative.

For a document needed only during C# work:

~~~markdown
- [C# conventions](../docs/contributing/csharp.md) - #LoadNow - applies to `**/*.cs`
~~~

That example assumes the named file exists. Use your actual path.

### Add project rules when you need them

A Directive is a rule the agent must follow within its scope. To add this category, preview the change, then apply it:

~~~sh
open-forge route init directives --framework --dry-run
open-forge route init directives --framework
~~~

Create `.agents/directives/checks.md`:

~~~markdown
---
description: Relevant checks for source changes
tags: [LoadNow, Directive, Testing]
applyTo: ["src/**"]
---

## Check source changes

### Instructions

- Before reporting a source change complete, run the relevant checks documented in this repository. Report their commands and results, or explain why they could not run.
~~~

The example uses root metadata, selected by the install commands above.

Add this line to `.agents/directives/_directives.md` under `## Entries`, replacing its empty placeholder:

~~~markdown
- [Relevant checks for source changes](checks.md) - #LoadNow #Directive #Testing - applies to `src/**`
~~~

You can maintain Entries yourself. With the CLI, preview `open-forge index --dry-run` before applying `open-forge index`.

### Keep accepted knowledge without adopting every Memory state

To add only Crystallized Memory:

~~~sh
open-forge route init memory/crystallized --framework --dry-run
open-forge route init memory/crystallized --framework
~~~

Crystallized holds accepted knowledge that should stay current. Records can live directly in that folder. You do not need a Decision template, a workflow or the other Memory states.

After accepting a decision, ask:

> Save this accepted decision in Crystallized Memory. Update its defining document and keep the useful reasoning. Link existing detail instead of copying it.

Working Memory and its separate saving policy are available when you want unfinished work to survive pauses.

### Choose the parts that help

These are independent choices. You can use one link, Directives alone, Crystallized alone, or a combination.

Essentials supplies a larger useful selection. Full Core supplies every built-in category and Memory state. Custom lets you select individual routes. Extensions add optional methods and content.

All setups use the same reading procedure. Deeper folders and file conditions keep specialized context with the work that needs it.

To disable a part, remove its loader entry. Its files remain notes. Existing accepted requirements and instructions your agent tool loads independently still apply. CLI-managed removals also need lifecycle exclusions so updates preserve your choice. `.agents/README.md` explains the distinction.

### Without the CLI

Copy only `src/open-forge/.agents/loader.md` and `src/open-forge/.agents/README.md` into your project. Append the managed block from `src/open-forge/AGENTS.md` to your existing AGENTS.md.

Replace the loader's Entries with the links you want before asking an agent to work. You can add catalogue files later by hand. Merge an existing `.agents/` tree carefully.

For tools that read CLAUDE.md, preserve existing content and add the supplied bridge. If your tool reads neither host file automatically, ask it to read AGENTS.md first.

The files contain the complete framework. The CLI gathers context and maintains navigation faster. It does not run your agent, and the instructions cannot guarantee correct model behavior.

Read the [installation guide](src/docusaurus/docs/getting-started/installation.md) for other installation methods and [the human guide](src/open-forge/.agents/README.md) for customization.
```

Complete `.agents/README.md`:

```markdown
## Open Forge in this project

Start with the context you already have. `loader.md` tells agents how to read it, and its Entries select the Open Forge parts this workspace uses.

Everything is plain Markdown. The CLI is optional. This guide is for people and is not a required startup read.

### Link an existing document

Add a line under `## Entries` in `loader.md`:

~~~markdown
- [Contribution instructions](../CONTRIBUTING.md) - #LoadNow
~~~

The path is relative to the loader. Use an existing Markdown file anywhere inside the workspace. It needs no move or new frontmatter.

`#LoadNow` requires reading when the parent loads and the file conditions match. Without a loading tag, the document opens when relevant.

To limit a required read:

~~~markdown
- [C# conventions](../docs/contributing/csharp.md) - #LoadNow - applies to `**/*.cs`
~~~

Use your actual path. Conditions are case-sensitive and refer to working files from the workspace root. Reading a document does not make its path a working file or establish acceptance.

### Add a rule

If Directives is absent, preview and apply:

~~~sh
open-forge route init directives --framework --dry-run
open-forge route init directives --framework
~~~

Without the CLI, copy `directives/_directives.md` from the framework and add its #LoadNow link to the loader.

Put each rule file's obligations under `## Instructions`. Give the file description and tags frontmatter, including LoadNow, and optional applyTo conditions. Follow the metadata form selected in `open-forge.json`. With no setting, use an `open-forge:` mapping.

List the file under its catalogue's Entries. Repeat its description, tags and conditions there. Keep that list current manually, or preview and run Index.

### Keep accepted knowledge

You can install Crystallized without the other Memory states:

~~~sh
open-forge route init memory/crystallized --framework --dry-run
open-forge route init memory/crystallized --framework
~~~

This also supplies its necessary Memory parent. Ordinary records can live directly under the state. Named categories and Templates are optional.

Keep existing documents authoritative for their subjects. A record can preserve a decision's reason and link to its current result.

### Keep unfinished work

Working Memory holds resume records. Its separate `memory/working/saving.md` policy requires updating the record before pausing unfinished work.

To stop automatic saving while retaining records, remove that policy file and its entry. Record a managed removal before updating through the CLI.

Emerging holds useful unsettled material. Archived holds useful history. Neither is required to use Crystallized or Working.

### Keep specialized context local

Use nested catalogues to narrow context by subject. Use file conditions when paths identify the scope.

A C# catalogue may contain a #LoadNow design file and an on-demand Windows catalogue. Selecting C# loads its design file, not Windows. Required files inside Windows load only after Windows is selected.

A condition filters a selected chain. It does not open a hidden parent.

### Turn a part off

Remove its line from the loader. Its files remain notes and its Open Forge rules stop applying. Accepted requirements and instructions loaded independently by your agent tool still apply.

Inside other catalogues, Index derives Entries from files on disk. Move a file outside that enabled subtree or remove it if you want it to stay out.

Disabling behavior, releasing management and deleting files are different operations. Install's remove row releases management while retaining existing files and routability. CLI Remove can delete files, so inspect its dry-run plan.

Report dependencies before disabling something another enabled part requires. Removed parts must stay removed unless you ask to restore them.

### Customize shipped files

Edit or replace a shipped file when you want a different version. An ordinary CLI Update may replace those direct edits.

For an adjustment that should survive updates, create `{name}.overwrite.md` beside its base. It loads immediately after the base and wins only for corresponding content in that source. It does not override unrelated rules.

### Terms you may encounter

A catalogue is the navigation file normally named `_{folder-name}.md`. Older documentation calls it an entrypoint.

A route is a navigable path exposed through catalogues and Entries.
A root route is a route exposed directly by the loader.
A scope is a part of a route that narrows where the following content applies.
A slug is a concrete folder name in a route path.

#Core labels base routing and loading, workspace orientation, and reusable agent-facing roles.
#Memory labels self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history.
#Extension labels optional packaged routes, capabilities, integrations and support files.

These labels create no loading behavior or management ownership. Only #LoadNow and #KeepInMind control automatic reading. The loader defines acceptance and Evergreen maintenance duties.

### Use the CLI when helpful

`open-forge context --for <file>` gathers required context. Repeat --for for every known working file. Agents still select relevant additional sources and follow required-read links.

Read the installed CLI Skill before other commands. If it is absent, use command help. Preview mutations with --dry-run.

`open-forge index` maintains metadata and Entries. Keep the same information current by hand when working without it.
```

### v3

Complete `.agents/loader.md` for Full Core. Presets emit only their selected root entries.

```markdown
## Open Forge Loader

This file defines how to read the context this workspace has enabled. The `Entries` below select its Open Forge parts. Unlisted parts are off. Existing project requirements and instructions loaded by your agent tool still apply.

### Axioms

### Read before you work

1. At task start, resume, or context restoration, list your working files, including planned files and both paths of a rename. Working files are those you investigate, review, create, change, rename or delete. Reading a file for background does not make it a working file.
2. If the CLI is available and allowed, run `open-forge context` with one `--for <file>` for every known working file. Read the returned content. This performs steps 3 and 4 only. Always perform step 5 yourself. If the CLI is unavailable, fails, or cannot follow a required link, use your file tools for steps 3 to 5.
3. Read every applicable #LoadNow and #KeepInMind entry in listed order. A catalogue is a folder's navigation file. Read its required children before continuing to the next entry. Never skip a required read because its description seems unrelated. Read each file once per pass, followed immediately by its overwrite companion if present. Navigate through catalogues, without opening other file bodies merely to discover routes or tags.
4. Check conditions before reading or applying a conditioned entry. Patterns within one list are alternatives. One working file must match that entry and every conditioned ancestor. Patterns are case-sensitive globs from the workspace root, such as `**/*.cs` and `docs/**`. A leading `!` is literal. A condition never selects a hidden scope or grants or restricts edit permission. When working files are unknown, report exposed conditioned required entries as pending. Resolve them before working on a matching file.
5. Open other enabled entries when their descriptions fit the task. Check Guidance for recurring choices, Patterns before creating, changing or reviewing structure, Skills before specialized work, Maps when locating important sources, and Templates when copy-ready starting content would help. Follow explicit required-read links. Before using a routed source found through search or a direct link, read its enabled ancestor catalogues and required context. Reading a disabled part for background does not reactivate its rules. Report a dependency on a disabled part instead of enabling it yourself.

Restart at step 1 when your goal or working files change. Before handoff or finish, reread #KeepInMind sources in active scopes only. During work, refresh those sources only when they may have changed.

### Use what you read

- Follow this loader's Axioms, loaded catalogues' `## Axioms`, and rule files' `## Instructions` within their scope. Catalogue rules cover their selected descendants. Narrower rules add to broader rules. Report unresolved conflicts instead of choosing silently.
- Follow user direction within platform and safety limits. Take routine, reversible steps inside the request without asking again. State assumptions that affect the result. Ask before unresolved choices materially change results, scope, risk, cost, external effects or reversibility.
- Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.
- Use the fewest words that preserve meaning, readability and logical progression. Remove filler and repetition, while retaining necessary context, conditions and exceptions in communication and authored content.
- Reading, linking, saving or tagging does not establish acceptance or permission to edit. Acceptance comes from the user or an authority the user declared for that subject. Use declared external authorities for the facts assigned to them.
- Let each source answer one clear question. Link related detail instead of repeating it. Links do not change authority, scope, loading or lifecycle. Keep detail in its narrowest defining source, with summaries and links above it.
- Investigate conflicting accepted current sources before changing either. When accepted direction changes current state, update its defining source and preserve useful prior context.
- Selected non-binding material may specialize broader material of the same kind within its accepted scope.
- #Contextual identifies unaccepted context. #CurrentTruth identifies accepted current state within its scope. Neither tag establishes acceptance.
- #Evergreen requires alignment with accepted current state, without creating authority or loading behavior. Update affected sources you may edit before work relies on them and no later than closeout. Batch related updates when safe. Keep each source aligned with what it represents now. Preserve useful prior context in an enabled Memory route. Report blocked or unauthorized updates.

### Maintain the setup

- Add an `Entries` line to enable a catalogue or Markdown file anywhere in the workspace, such as `../docs/architecture.md`. Existing documents need no move or frontmatter. Their entry supplies the description, tags and conditions.
- Remove a loader entry to disable that part, leaving its files as notes. This does not cancel accepted requirements or instructions your agent tool loads independently. Report dependencies before disabling a part. Do not recreate disabled parts unless the user asks. Within other catalogues, Entries follow files on disk, so move or remove a file to keep it out. Record managed removals in CLI exclusions before updates.
- Report missing listed files. An empty `Entries` list is valid.
- Only #LoadNow and #KeepInMind control automatic reading. They act through loaded parents. Every ancestor needed for automatic reading must expose the chain through #LoadNow. Scope specialized content before making it mandatory. Use #LoadNow only when missing the content costs more than reading it whenever its parent loads. Use #KeepInMind for required refreshes, not importance.
- New routed Markdown needs `description`, `tags` without `#`, and optional `applyTo` frontmatter. Follow `frontmatter` in `.agents/open-forge.json`, using root keys or one `open-forge:` mapping. An absent setting means scoped. Write `applyTo` as a list or comma-separated string. If root and scoped declarations coexist, their pattern sets must agree. Native `SKILL.md` keeps its native metadata.
- Each routed folder has one catalogue, normally `_{folder-name}.md`. Compatibility names are `index.md`, `_index.md`, `references.md` and `_references.md`. Folders can narrow a chain at any depth, including before, between or after category segments. Each narrows everything that follows it. Include only useful routes, without copying another scope or the defaults.
- With no local rules, write `## Axioms` containing only `- inherited - No local axioms; loaded ancestor axioms remain active.` Existing missing or empty sections add no local rules. Never combine the marker with local rules or repeat inherited rules.
- Connect related material with relative local links and precise references. Reuse consistent descriptive tags for shared subjects. Add links and tags only when they help navigation or retrieval.
- Edit or replace a shipped source, or add `{name}.overwrite.md` beside it to keep local changes through updates. The companion shares its base's scope and loading, is not indexed separately, and wins only for corresponding content in that base. It does not override other sources. Direct edits to managed files may be replaced by Update.
- Keep Entries current manually or with `open-forge index`. If the CLI Skill is installed, read it before commands other than `context`. Otherwise consult command help. Preview mutations with `--dry-run` before applying them.

### Entries

- [Rules for this workspace](directives/_directives.md) - #LoadNow #Core #Directive
- [Advice for recurring choices](guidance/_guidance.md) - #Core #Guidance
- [Pointers to project knowledge](maps/_maps.md) - #Core #Map
- [Useful records for current and future work](memory/_memory.md) - #LoadNow #Memory
- [Default shapes for creating and reviewing structure](patterns/_patterns.md) - #Core #Pattern
- [Capabilities for specialized work](skills/_skills.md) - #Core #Skill
- [Starting content to copy and adapt](templates/_templates.md) - #Core #Template
```

Managed `AGENTS.md` block:

```markdown
## Open Forge

Before starting or resuming a task, read `.agents/loader.md` and follow its five reading steps. Existing project instructions still apply.

If the CLI is available and allowed, `open-forge context --for <file>` gathers required context. Repeat `--for` for every known working file. Read its output, then complete step 5 yourself. If the command fails or is unavailable, follow the loader with your file tools.

**End of Open Forge managed section.**
```

The five most consequential sentence changes:

| Before | After | Why |
| --- | --- | --- |
| “That covers steps 3 and 4. Still do step 5 yourself.” | “This performs steps 3 and 4 only. Always perform step 5 yourself.” | Makes the remaining duty harder to skim past |
| “Read every entry below tagged #LoadNow or #KeepInMind” | “Read every applicable #LoadNow and #KeepInMind entry in listed order.” | Conditions govern reading before bodies open |
| “then do the same inside each catalogue you read” | “Read its required children before continuing to the next entry.” | States traversal order directly |
| “when the same working file matches its patterns and those of every conditioned catalogue above it” | “One working file must match that entry and every conditioned ancestor.” | Prevents satisfying different conditions with different files |
| “Every other tag is a search label.” | “Only #LoadNow and #KeepInMind control automatic reading.” | Restores Evergreen’s maintenance duty without suggesting extra CLI loading behavior |

Whitespace-separated word counts, including headings and Entries: loader **1,238**, managed block **71**. The checked v2 equivalents are **823** and **56**. Restoring duties costs 415 loader words, while the loader remains 814 words shorter than the shipped version.

### Risks and proof

The loader is larger, and its maintenance reference can still be skimmed. Keep imperative verbs and explicit triggers in the reading procedure. Put friendly explanation and worked examples in the human introduction.

The 46-row audit alone does not certify the whole diff. For example, mandatory Pattern shapes need protection against v2’s broad adaptation wording. The ledger restoration above makes that boundary explicit.

The cheapest useful experiment is three unfamiliar developers, each given fifteen minutes and an existing repository. Ask them to connect one document, add one scoped rule, preserve one accepted fact and disable one part. Observe tool reads and resulting files, rather than relying on an agent’s claimed reading list. Require unaided first setup within five minutes and no authority or scope mistakes.

Then use one 200-file task fixture with CLI available and unavailable. Check required reads, mixed-file condition chains, pending paths, changed KeepInMind content, missing links and disabled folders. Preview Index and Update against an intentionally empty loader with folders still present. Neither may reactivate them.

No files were modified, and no doctor, build or test suite was run for this proposal.

### The one idea to steal

Use one reading procedure at every size, and let the enabled links determine how much framework the workspace uses.

---

## CLI Engineer (Proposal K)

/root - GPT-6 - high

### Thesis

Keep v2’s five-step procedure, direct links, optional folders and existing Markdown grammar. Restore lost obligations where their users will encounter them, including framework-wide duties in the loader. Add `--preset minimal` through the existing install selection model, with one linked document as the recommended brownfield starting point.

### How v2 removed so many rules

1. **It compressed repeated instructions.** Loader L17’s platform constraints and L18’s user-direction rule survive together. This was sound where conditions and strength survived.
2. **It moved operational detail into the CLI Skill.** Loader L116’s `responsibility` explanation and L130–138’s command catalogue moved out of startup reading. This is sound for CLI operations when the Skill remains required before those operations.
3. **It removed vocabulary.** Loader L46’s `route` and L49’s `slug` disappeared. Moving their definitions to a human reference is sound because neither definition controls loading.
4. **It separated optional policies.** Memory L24’s coordination capture and L28’s unfinished-work preservation moved toward `saving.md`. That separation supports deliberate opt-out. It does not justify losing accepted-source maintenance when Working Memory is absent.
5. **It deleted authoring criteria.** Patterns L19’s promotion threshold and Templates L30’s specialization threshold disappeared. These were opinions users could rely on, so they need to return to their catalogues.
6. **It treated explicit safeguards as obvious implications.** Loader L32’s investigate-first requirement and Patterns L28’s separate reporting of adaptations were weakened. Neither follows from “update its record” or “explain the difference.” These were losses.

The reduction was therefore partly better organization and partly fewer obligations. Word counts alone cannot establish equivalence.

### The lossless ledger

Source references use the ledger’s old line numbers. Destination paths below are installed paths beneath `.agents/`. Quoted text is the proposed replacement or addition in the named section. Every Dropped and Partly row is accounted for, with no reliance on an unquoted redundancy claim.

**Destination: `loader.md`, `## Axioms`, `### Use what you read`**

- `loader.md:L21` — **Restore**: “Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation and at most one important open choice.”
- `loader.md:L22` — **Restore**: “Use the fewest words that preserve meaning, readability and logical progression in communication and authored content. Remove filler and repetition. Keep needed context, conditions and exceptions.”
- `loader.md:L27` — **Restore**: “Let each source answer one clear question. Link to related sources instead of repeating their detail. A link does not change either source's authority, scope, loading rules or lifecycle.”
- `loader.md:L32` — **Restore**: “Investigate an apparent conflict between accepted sources before changing either. When accepted direction changes current state, update its defining source and preserve useful prior context.”
- `loader.md:L40` — **Restore**: “Selected non-binding material may specialize broader material of the same kind within its accepted scope.”

**Destination: `loader.md`, `## Axioms`, `### Read before you work`**

- `loader.md:L62` — **Restore**: “Use catalogues for navigation. Do not open ordinary file bodies just to discover routes.”
- `loader.md:L93` — **Restore**: “Refresh only active scopes.”
- `loader.md:L94` — **Restore**: “During work, recheck them only when their tagged sources may have changed.”

**Destination: `loader.md`, `## Axioms`, `### Keep current sources current`**

- `loader.md:L100` — **Restore**: “#Evergreen marks material that must stay aligned with accepted current state. It grants no authority and changes no loading behavior.”
- `loader.md:L101` — **Restore**: “Update affected #Evergreen sources you may edit before work depends on them and no later than closeout. Batch related updates when safe.”
- `loader.md:L102` — **Restore**: “Keep each source aligned with what it represents now. Preserve useful prior context in the appropriate enabled Memory route, or its existing source when Memory is off. Report needed updates that are blocked or not authorized.”

**Destination: `loader.md`, `## Axioms`, `### Change the setup and author sources`**

- `loader.md:L28` — **Restore**: “Connect related material with relative local links and precise references. Reuse consistent descriptive tags. Add links and tags only when they help navigation or retrieval.”
- `loader.md:L37` — **Restore**: “In a new catalogue without local rules, write `## Axioms` with `- inherited - No local axioms; loaded ancestor axioms remain active.` Existing missing or empty sections also add no rules. Never combine this marker with local rules.”
- `loader.md:L57` — **Restore**: “Folders may narrow a route at any depth and narrow everything below them.”
- `loader.md:L58` — **Restore**: “Each folder lists only what it needs and need not copy the defaults.”

**Destination: `README.md`, `## Terms and labels`**

- `loader.md:L46` — **Relocate**: “A route is a navigable path exposed through catalogues and Entries.”
- `loader.md:L47` — **Relocate**: “A root route is exposed directly by the loader.”
- `loader.md:L48` — **Relocate**: “A scope is part of a route that narrows where the following content applies.”
- `loader.md:L49` — **Relocate**: “A slug is a concrete folder name in a route path.”
- `loader.md:L95` — **Relocate**: “#Core labels base routing, loading, workspace orientation and reusable agent-facing roles.”
- `loader.md:L96` — **Relocate**: “#Memory labels self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history.”
- `loader.md:L97` — **Relocate**: “#Extension labels optional packaged routes, capabilities, integrations and support files.”

**Destination: `README.md`, `## Nested loading`**

- `loader.md:L85` — **Relocate**: “A C# scope may contain a #LoadNow design file and an on-demand Windows scope. Selecting C# loads its design rules, not Windows. A #LoadNow file under Windows loads only after Windows is selected.”

**Destination: `maps/_maps.md`, `## Axioms`**

- `maps:L23` — **Restore**: “Keep Maps broad. Prefer modules, projects, repositories, systems or scopes. Use finer routing only when it clearly helps.”
- `maps:L24` — **Restore**: “Each workspace chooses its own Map filenames, grouping and nesting depth.”
- `maps:L25` — **Restore**: “Maps do not replace Memory or the destination's current information.”

**Destination: `patterns/_patterns.md`, `## Axioms`**

- `patterns:L19` — **Restore**: “Create or update a Pattern only when an accepted shape should guide future related work. Structure alone does not justify promoting one-off work, temporary transitions or unsettled candidates into a Pattern.”
- `patterns:L25` — **Restore**: “Link related rules, advice, maps, procedures and starting files. Say which sources bind the shape. Sources applying the shape should link back to its Pattern.”
- `patterns:L26` — **Restore**: “Keep required behavior, procedures and substantial choice rationale in their defining sources. Example names and values illustrate the shape unless an applicable requirement fixes them.”
- `patterns:L28` — **Restore**: “Treat an applicable Pattern as the default shape in its scope. Adapt it only within the authority already granted. Explain material departures before other work depends on them. Reviews report justified adaptations as departures, separately from conformance.”

**Destination: `templates/_templates.md`, `## Axioms`**

- `templates:L24` — **Restore**: “If the new artifact needs continuing guidance or requirements, link their defining source. The Template provides only starting content.”
- `templates:L28` — **Restore**: “State the need and primary question each Template answers in its description and source instructions.”
- `templates:L29` — **Restore**: “Use focused prompts that request necessary content once, in a readable order. Mark optional sections clearly.”
- `templates:L30` — **Restore**: “Treat generic Templates as fallbacks. Add a specialized Template only when it provides meaningfully different starting content.”

**Destination: `memory/_memory.md`, introduction**

- `memory:L13` — **Restore**: “Memory grows through useful records and routed scopes without a fixed structural limit. Unselected branches do not load their contents. Records need not pass through every state.”

**Destination: `memory/_memory.md`, `## Axioms`**

- `memory:L19` — **Restore**: “An accepted record can define current knowledge or decisions. Recording another category's content does not give the record that category's role.”
- `memory:L26` — **Restore**: “Show the source and uncertainty when they affect trust or later acceptance. Distinguish what was stated, observed or inferred.”
- `memory:L27` — **Restore**: “Update affected sources before other work depends on the outcome and no later than closeout. Keep useful context and reasoning in Memory. Accepted durable knowledge must reach its defining source, not remain only in chat or a #Contextual record.”
- `memory:L32` — **Restore**: “Add records and routed scopes when their likely future value justifies the cost of finding and reviewing them.”
- `memory:L35` — **Restore**: “Named record categories and Templates are optional. Use an existing state and the narrowest useful scope. Records may live directly under a state. Do not recreate a removed category merely to save a record.”
- `memory:L36` — **Restore**: “Use an existing suitable record before creating another. Within the authority granted for the work, add an ordinary record or scoped route when needed. Ask when placement would change accepted meaning or cross an unresolved authority boundary.”

**Destination: `memory/working/_working.md`, `## Axioms`**

- `working:L19` — **Restore**: “An explicitly accepted temporary choice may remain here when its source, scope and expected expiration are clear.”

**Destination: `memory/emerging/_emerging.md`, `## Axioms`**

- `emerging:L17` — **Restore**: “At each applicable #KeepInMind refresh, check visible Entries and save useful candidate material before it is lost.”
- `emerging:L23` — **Restore**: “Refine material while its validity, use or final destination remains unsettled.”
- `emerging:L25` — **Restore**: “Revisit repeated or stale material and decide whether to consolidate, promote, archive or prune it. Review may leave a record unchanged.”

**Destination: `memory/archived/_archived.md`, `## Axioms`**

- `archived:L17` — **Restore**: “Retain, consolidate or transform history according to its future value. Archival need not preserve every detail.”

Several other ledger classifications also need correction before claiming losslessness:

- Restore Emerging’s `#KeepInMind` in its frontmatter and parent entry. Otherwise its restored refresh obligation has reduced reach.
- Restore explicit user direction before adding a top-level Memory state.
- Archive instructions must remove metadata claiming former authority, not only loading tags. Preserve the accepted-retention-preferences exception.
- Patterns must retain the requirement to follow shapes made mandatory by applicable rules.
- Template copies must remove irrelevant package tags.
- Working must explicitly check Entries before concluding no record applies.

Mirror accepted payload changes into the repository’s own copy while preserving its project-specific entries.

### The complexity ladder

Let **B** mean the existing host instructions plus `.agents/loader.md` and the human README. CLI installations additionally have settings and ownership records. Each routed folder below has its catalogue.

| Level | Installed tree beyond B | Value and cost | Moving one level |
| --- | --- | --- | --- |
| 1. One useful link | No category folders. Loader links existing `CONTRIBUTING.md` | Existing knowledge reaches relevant tasks. Maintain one entry | Up: add Directives or Crystallized. Down: remove the link |
| 2. Rules and accepted knowledge | `directives/`, `memory/_memory.md`, `memory/crystallized/` | Durable rules and accepted facts. Maintain only records actually needed | Up: add Working. Down: return knowledge to existing documents and disable the added roots |
| 3. Continuity | Level 2 plus `memory/working/` and `saving.md`. Emerging is optional | Resume unfinished work. Agents must maintain useful state | Up: add the remaining categories. Down: remove the saving policy and its entry, then retire unused records |
| 4. Full Core | All six Core categories, four Memory states and bundled CLI Skill | Complete vocabulary and reusable defaults. More navigation and maintenance | Up: select Extensions. Down: disable unused roots, then deliberately retire unwanted child states |
| 5. Selected Extensions | Level 4 plus selected package content and dependencies | Workflows, planning or coordination where useful. Package and process costs | Down: preview package removal and preserve user-created results |

These are examples, not mandatory bundles. Level 2 may contain Directives alone or Crystallized alone. Brownfield projects should start at Level 1 because useful context already exists.

Disabling, stopping management and deleting remain distinct. For child-state removal, preserve useful records first and preview `route remove`. Merely selecting install’s `remove` action does not disable existing content.

### Ultra-lightweight install

Add **`--preset minimal`**. Commands using this preset are proposed release behavior.

On a fresh repository it writes `AGENTS.md`, `CLAUDE.md`, `.agents/loader.md`, `.agents/README.md`, `.agents/open-forge.json` and `.agents/open-forge.lock.json`. Existing host files receive only their managed sections. Entries start empty. No category folders, Skill, saving policy or Git-ignore rules are installed.

All ten existing route rows default to `remove`. Permit overrides with `minimal` and `custom`, retaining the current restriction for `essentials` and `full-core`:

```sh
open-forge install --preset minimal --route directives=add --dry-run
open-forge install --preset minimal --route memory/crystallized=add --dry-run
```

Unknown names and conflicting repeats remain invalid. Existing workspaces still require `--configure`. Keep current interactive and unattended defaults unchanged.

Two details require real implementation:

- `InstallConfigurationSettings.PlanChoices` currently excludes individual Memory states but leaves their parent catalogue eligible. Minimal must exclude Memory’s parent when no state is selected. Adding a state must clear that parent exclusion.
- `GeneratedNavigationRegionPlanner.ReadLoaderRootPaths` currently returns structural roots when the listed count is zero. With membership preservation enabled, a valid empty list must stay empty. Install and Configure must add explicitly selected roots without reviving other disabled roots.

Plan wording: “Setup: Minimal. No built-in routes will be installed. Review the listed host sections and management files, then add an Entries line to connect an existing document.”

Fresh receipt wording: “Installed Open Forge with no built-in routes. Connect an existing document in `.agents/loader.md`.” Configure retains the existing warning that files stay routable.

JSON keeps schema version 3 and the current envelope. `data.configuration.preset` becomes `"minimal"`, with all ten resolved route rows. Existing effects, omissions, verification and continuation fields remain authoritative. No preset identity is persisted.

Implementation locations, using existing project roots:

| Files | Change and estimated effort |
| --- | --- |
| Operations `Commands/Install/Models/Configuration/InstallConfiguration.cs`, `Shared/Configuration/InstallConfigurationChoices.cs`, `Shared/Binding/InstallBindingInputReader.cs` | Enum, mappings, defaults and validation. Half a day |
| Operations `Shared/Configuration/InstallConfigurationSettings.cs`, `InstallConfigurationIntendedStateBuilder.cs` | Parent exclusion and membership-preserving growth. One day |
| Framework `Framework/GeneratedNavigation/GeneratedNavigationRegionPlanner.cs` | Preserve valid empty membership. Half a day |
| Rendering `Presentation/Install/Shared/Prompts/InstallSetupQuestions.cs`, `Shared/Selection/InstallSetupRecapSelector.cs`, `InstallReportSelector.cs` | Fourth wizard choice, complete reordering, labels and JSON mapping. Half a day |
| OutputText `Install/InstallInputText.cs`, `InstallSetupText.cs`, `InstallText.cs`, `Shared/SharedText.cs` | Validation, help examples, plan and result text. Half a day |

Allow another one to two days for focused verification and documentation. Extend `InstallConfigurationTests`, `InstallBindingTests`, `InstallSetupPromptsTests`, install configuration/composition/recap integration tests and `PublishedInstallConfigurationProcessTests`. Add empty-membership projection and Index regressions.

Update root/install help, invalid-preset recovery and setup recap snapshots across supported text and JSON detail levels. Pin fresh six-file installation, cancellation, repeat no-op, Memory-only growth, external-link preservation and unchanged existing host prose.

This is approximately **four to six engineer-days**. It requires no new command subsystem, Markdown grammar, settings schema or migration.

### Brownfield adoption

Assume the CLI is already available. Save the repository’s current state through its normal process.

1. Preview `open-forge install --preset minimal --dry-run`. Inspect every effect. Before application, agents continue using existing host instructions.
2. Apply `open-forge install --preset minimal --automatic`. Review `git diff`. Agents now read the host instructions and loader, whose empty Entries add no project documents.
3. Add this exact line under the loader’s `## Entries`:

   ```markdown
   - [Contribution requirements](../CONTRIBUTING.md) - #LoadNow
   ```

   Agents now read that existing file at task start. It keeps its location and content. Linking does not establish acceptance.
4. Add a conditional source only after inspecting its actual purpose. For an existing C# guide:

   ```markdown
   - [C# conventions](../docs/csharp.md) - #LoadNow - applies to `**/*.cs`
   ```

   This example requires that file to exist. Agents read it for matching work and report it pending while working files are unknown.
5. Preview `open-forge context --for src/Example.cs`, substituting a real working path. Compare its reads with a manual walk. A background README need not become mandatory.
6. When records become useful, preview and apply:

   ```sh
   open-forge install --configure --preset custom --route directives=add --route memory/crystallized=add --dry-run
   open-forge install --configure --preset custom --route directives=add --route memory/crystallized=add --automatic
   ```

   Custom preserves current route choices. Agents additionally read Directives, Memory and Crystallized catalogues, then applicable records.

Keep existing `AGENTS.md` and `CLAUDE.md` prose. Inspect possible conflicts before making another instruction source mandatory. Keep `README.md` and `docs/` where they are.

Open Forge must never relocate or rewrite those existing documents merely to link them, invent their authority, normalize unrelated frontmatter, claim whole-file ownership of adopted content, or reactivate disabled context during maintenance.

A new `link` command is not justified yet. Safe insertion, escaping, duplicate identity, ownership and continuation behavior would cost roughly three to five engineer-days plus permanent support. Ship the copyable entry and clearer help first.

### The introduction

Complete replacement README introduction and start section:

````markdown
## Open Forge

Open Forge helps AI coding agents find the rules, docs and decisions your project already has. You choose what they read before a task and what they open when it becomes relevant.

The setup is plain Markdown. Start with one useful link, add a home for rules or accepted knowledge, and introduce more structure when the work needs it. Your existing documents can stay where they are.

### Start with an existing project

With Node.js 22.18 or later, install the optional CLI, then preview a minimal setup from your repository:

```sh
npm install -g @thelithiumforge/open-forge@beta
open-forge install --preset minimal --dry-run
```

Review the plan, then apply it:

```sh
open-forge install --preset minimal --automatic
```

Existing `AGENTS.md` and `CLAUDE.md` instructions stay. Open Forge adds its managed sections and a small `.agents/` setup.

Add this line under `## Entries` in `.agents/loader.md`:

```markdown
- [Contribution requirements](../CONTRIBUTING.md) - #LoadNow
```

Use a file your project actually has. The document needs no move or frontmatter.

Now ask your agent:

> Explain how this project runs its tests. Follow AGENTS.md and name the sources you read.

`#LoadNow` makes the linked document a required read. An optional `applies to` condition limits it to work on matching files:

```markdown
- [C# conventions](../docs/csharp.md) - #LoadNow - applies to `**/*.cs`
```

Without a loading tag, an entry is opened when its description fits the task.

### Add rules and accepted knowledge

When existing documents no longer give a useful home to new material, add only the folders you need:

```sh
open-forge install --configure --preset custom --route directives=add --route memory/crystallized=add --dry-run
```

Review the plan, then repeat without `--dry-run` and with `--automatic`.

Directives hold rules agents must follow. Crystallized Memory holds accepted knowledge and decisions that should stay current. Either can be used alone. You do not need task records, workflows or every Memory state.

### Use the full framework

For all built-in categories and Memory states:

```sh
open-forge install --configure --preset full-core --dry-run
```

Review before applying. Optional Extensions add workflows, planning records and other methods separately.

Every level is a useful place to stop. Remove a loader entry to disable that branch while keeping its files. Existing project requirements and independently loaded host instructions still apply.

### Use plain files

The CLI makes reading and maintenance faster. It is optional.

Without it, copy `src/open-forge/.agents/loader.md` into your project's `.agents/` folder, replace its Entries with the links you want, and add an instruction to your existing agent file telling the agent to read the loader before work.

You own the files, their organization and the choices they express. Open Forge supplies opinions about reading, authority and continuity. It cannot mechanically guarantee that an agent follows them.

See [the project guide](src/open-forge/.agents/README.md), [installation](src/docusaurus/docs/getting-started/installation.md) and [customizing](src/docusaurus/docs/concepts/customizing.md) for details.
````

Complete `.agents/README.md`:

````markdown
## Open Forge in this project

Agents read `loader.md` before work. Its Entries choose this project's Open Forge context. This file explains the setup for people and is not a required startup read.

### Connect existing documents

Add a line beneath `## Entries` in the loader:

```markdown
- [Contribution requirements](../CONTRIBUTING.md) - #LoadNow
```

Paths are relative to the loader. Use an existing file. It needs no move or frontmatter.

`#LoadNow` requires reading whenever its parent is selected and its conditions match. `#KeepInMind` also requires refreshes at the points defined in the loader. Without either tag, the entry opens when relevant.

To limit a source to matching work:

```markdown
- [C# conventions](../docs/csharp.md) - #LoadNow - applies to `**/*.cs`
```

A link selects reading. It does not accept the document or grant permission to edit it.

### Add only what helps

Directives hold required behavior. Put rules under `## Instructions`.

Crystallized Memory holds accepted knowledge. Working Memory holds unfinished work. Other folders provide advice, pointers, reusable shapes, capabilities, starting files, candidates and history.

You can use one folder, several folders or none. Records may live directly under a Memory state.

With the CLI, preview adding an omitted category:

```sh
open-forge route init directives --framework --dry-run
```

Review before repeating without `--dry-run`. If a broader Memory exclusion exists, use Custom configuration to add the wanted state and clear its parent exclusion together.

### Turn context off

Remove its line from the loader. Its files stay as notes and their Open Forge rules stop applying.

Accepted project requirements and instructions your agent tool loads independently still apply. Check for enabled dependents before disabling their source.

Inside other catalogues, Index follows the files in their folder. Move or remove a file to stop indexing it there.

Install's `remove` route choice stops managing supplied defaults while preserving existing files and routability. It does not disable their context. Commands that delete files have separate previews.

Record intentional managed removals through the CLI or its exclusions before updating.

### Keep or stop automatic saving

Working Memory includes `saving.md`, which requires useful unfinished state to be saved before pausing or handing off.

To stop that policy, remove `saving.md` and its entry from `_working.md`. Keep the records you still need. In a managed installation, record the removal so updates preserve it.

### Customize shipped files

You may edit or replace them. A later managed Update can replace direct edits.

For changes that should survive updates, place `{name}.overwrite.md` beside its base. It loads immediately afterward and wins only for corresponding content in that source.

### Maintain the setup

Without the CLI, maintain metadata, links and Entries directly.

With it, `open-forge context --for <file>` gathers required context. The loader still requires relevance choices and explicit prose-linked reads.

Preview Index after manual additions. After renames or moves, preview Repair. Read the installed CLI Skill when available, or command help, before unfamiliar operations.

### Nested loading

A C# scope may contain a #LoadNow design file and an on-demand Windows scope. Selecting C# loads its design rules, not Windows. A #LoadNow file under Windows loads only after Windows is selected.

### Terms and labels

A catalogue is a folder's navigation file, also called an entrypoint.

A route is a navigable path exposed through catalogues and Entries.

A root route is exposed directly by the loader.

A scope is part of a route that narrows where the following content applies.

A slug is a concrete folder name in a route path.

#Core labels base routing, loading, workspace orientation and reusable agent-facing roles.

#Memory labels self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history.

#Extension labels optional packaged routes, capabilities, integrations and support files.

These labels do not change loading or grant authority. The loader defines the two loading tags and the obligations associated with accepted and current material.
````

### v3

Complete `.agents/loader.md` for Full Core. Installation filters its Entries to the selected setup.

```markdown
## Open Forge Loader

This file chooses the context used in this workspace. It adds to the project's other instructions. Everything works with plain files. The lines under `Entries` choose what is enabled. Unlisted Open Forge files add no duties.

### Axioms

### Read before you work

1. At task start, resume and context restoration, list your working files. Include files you will investigate, review, create, change, rename or delete, including planned files. Background reading does not make a file a working file.
2. When the CLI is available and allowed, run `open-forge context` with `--for <file>` for every known working file and read its output. This covers steps 3 and 4. You must still do step 5. If the command fails, is unavailable or cannot follow a link, use your file tools for steps 3 to 5.
3. Read entries tagged #LoadNow or #KeepInMind in listed order. Apply step 4 before opening a conditioned entry. Repeat inside each catalogue you read. A catalogue is a folder's navigation file. Required reads cannot be skipped because their descriptions seem unrelated. Read each file once per pass.
4. For a conditioned entry, one working file must match both its patterns and every conditioned ancestor's patterns. These are case-sensitive globs from the workspace root, such as `**/*.cs` or `docs/**`. Conditions never grant edit permission or select an unexposed branch. When working files are unknown, report conditioned required entries as pending. Resolve them before touching a matching file.
5. Open other entries when their descriptions fit the task, and follow explicit required-read instructions in files you read. This includes checking enabled Guidance for recurring choices, Maps for source discovery, Patterns for structural work, Skills for specialized work and Templates when starting content would help. If search or a direct link finds a listed file, read its catalogues first. Use catalogues for navigation. Do not open ordinary file bodies just to discover routes.

Repeat from step 1 when your goal or working files change. Before handoff or finish, reread #KeepInMind entries in active scopes, including their required children and overwrites. Refresh only active scopes. During work, recheck them only when their tagged sources may have changed.

### Use what you read

- Follow `## Axioms` in this loader and loaded catalogues, and `## Instructions` in rule files, within their stated scopes. A catalogue's rules cover its listed descendants. Child rules add to ancestor rules and cannot cancel them. Report conflicts or instructions you cannot follow.
- Follow the user's direction within platform and safety limits. Take routine, reversible steps within the request without asking again. State assumptions that affect the result. Ask before unresolved choices significantly change the result, scope, risk, cost, external effects or ability to undo the work.
- Reading, linking, saving or tagging does not establish acceptance or permission to edit. Accepted means the user agreed or a declared authority establishes it within its assigned scope. Evidence validates a claim, but does not accept it. Treat #Contextual content as unaccepted unless applicable authority accepts it. #CurrentTruth identifies accepted current state within its scope.
- Let each source answer one clear question. Link to related sources instead of repeating their detail. A link does not change either source's authority, scope, loading rules or lifecycle.
- Investigate an apparent conflict between accepted sources before changing either. When accepted direction changes current state, update its defining source and preserve useful prior context.
- Selected non-binding material may specialize broader material of the same kind within its accepted scope.
- Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation and at most one important open choice.
- Use the fewest words that preserve meaning, readability and logical progression in communication and authored content. Remove filler and repetition. Keep needed context, conditions and exceptions.

### Keep current sources current

- #Evergreen marks material that must stay aligned with accepted current state. It grants no authority and changes no loading behavior.
- Update affected #Evergreen sources you may edit before work depends on them and no later than closeout. Batch related updates when safe.
- Keep each source aligned with what it represents now. Preserve useful prior context in the appropriate enabled Memory route, or its existing source when Memory is off. Report needed updates that are blocked or not authorized.

### Change the setup and author sources

- Add an `Entries` line to enable a catalogue or a Markdown file anywhere inside the workspace, such as `../docs/architecture.md`. Existing linked documents need no move or frontmatter. The line supplies their description, tags and conditions.
- Remove a loader entry to disable that part. Its files remain notes. Reading them as background does not reactivate their rules. Accepted project requirements and instructions loaded independently by the host remain in force. Report enabled dependents first. Never restore disabled parts unless the user asks.
- Inside other catalogues, Entries follow the folder's files. Move or delete a file to stop indexing it there. Before CLI-managed updates, record intentional removals through the managing command or its exclusions. Use the installed CLI Skill or command help for syntax.
- Report a missing listed file. An empty `Entries` section is valid.
- Use #LoadNow only when the file is needed whenever its parent loads and missing it costs more than reading it. Use #KeepInMind only when the defined refreshes are needed. Required files need #LoadNow on their catalogue ancestors too. Neither tag activates an unselected branch. Other tags classify content without changing loading or authority.
- Folders may narrow a route at any depth and narrow everything below them. Each folder lists only what it needs and need not copy the defaults. Keep separately selected chains separate. Put specialized detail in the narrowest useful source and link to it from summaries.
- Connect related material with relative local links and precise references. Reuse consistent descriptive tags. Add links and tags only when they help navigation or retrieval.
- Each routed folder has one catalogue named `_{folder-name}.md`. Compatibility names are `index.md`, `_index.md`, `references.md` and `_references.md`. For terms and a nested-loading example, see `README.md` when needed.
- In a new catalogue without local rules, write `## Axioms` with `- inherited - No local axioms; loaded ancestor axioms remain active.` Existing missing or empty sections also add no rules. Never combine this marker with local rules.
- New routed Markdown inside `.agents/` needs frontmatter with `description`, `tags` and optional `applyTo`. Use the form selected by `frontmatter` in `.agents/open-forge.json`, root fields or fields under `open-forge:`. An absent setting means scoped. Native Skills keep native metadata. Maintain Entries by hand or preview `open-forge index --dry-run` before applying Index.
- You may edit or replace shipped files. Put update-safe changes in `{name}.overwrite.md` beside the shipped file. Read it immediately after its base, with the same scope and loading behavior. Do not index or select it separately. It wins only where the two answer the same question differently and cannot override unrelated sources.

### Entries

- [Rules agents must follow in this workspace](directives/_directives.md) - #LoadNow #Core #Directive
- [Advice for recurring choices](guidance/_guidance.md) - #Core #Guidance
- [Pointers to project knowledge](maps/_maps.md) - #Core #Map
- [Continuity, accepted knowledge, ideas and history](memory/_memory.md) - #LoadNow #Memory
- [Default shapes for creating or reviewing structure](patterns/_patterns.md) - #Core #Pattern
- [Capabilities for specialized work](skills/_skills.md) - #Core #Skill
- [Starting files to copy and adapt](templates/_templates.md) - #Core #Template
```

Complete managed `AGENTS.md` block:

```markdown
## Open Forge

Before starting or resuming a task, read `.agents/loader.md` and follow its steps. It selects this workspace's Open Forge context alongside the project's other instructions.

If the optional CLI is available and allowed, `open-forge context --for <file>` gathers required reads for each known working file. The loader explains the remaining reads and the fallback to file tools.

**End of Open Forge managed section.**
```

Counts using whitespace-separated words, including Markdown syntax: **loader 1,231**, **managed block 65**. The loader is longer than v2 because restored behavior has a visible cost. The inherited marker’s punctuation is preserved as literal existing syntax.

| Before | After | Why |
| --- | --- | --- |
| “The `Entries` below are the switch: what is listed is on, and anything not listed is off.” | “The lines under `Entries` choose what is enabled. Unlisted Open Forge files add no duties.” | Names the effect and its boundary |
| “Read and apply an entry with `applies to` patterns only when the same working file matches its patterns and those of every conditioned catalogue above it.” | “For a conditioned entry, one working file must match both its patterns and every conditioned ancestor's patterns.” | Makes the same-file requirement easier to parse |
| “That covers steps 3 and 4. Still do step 5 yourself.” | “This covers steps 3 and 4. You must still do step 5.” | States the remaining obligation directly |
| “Before applying a file you reached through search, read the listed catalogues above it.” | “If search or a direct link finds a listed file, read its catalogues first.” | Restores the direct-link case |
| “To change a shipped file and keep updates, write your change in `{name}.overwrite.md` beside it.” | “Put update-safe changes in `{name}.overwrite.md` beside the shipped file.” | Separates customization from update behavior |

### Risks and proof

Restoring duties increases startup reading. Restoring Emerging refreshes also restores capture work. Report those costs instead of retaining v2’s smaller count as if nothing changed.

Minimal’s largest risks are the leftover Memory parent, empty membership being repopulated, and growth reactivating disabled roots. The cheapest technical proof is a small fixture matrix covering those three cases, one external document, preserved host prose and a second no-op run. Parser changes are unnecessary.

For the introduction, recruit three developers unfamiliar with Open Forge. Give each a repository with existing contribution docs and twenty minutes. Ask them to connect one source, explain when it loads, add one scoped rule and disable it. Then ask them to find how to add accepted knowledge.

Run fresh agent sessions with and without the CLI. Success means all three developers complete the first useful setup without explanation, predict the scoped reads correctly and retain the disabled choice after maintenance. If several fail at the same entry edit, reconsider a link helper.

This proposal is based on read-only inspection. No files were changed, and no builds, tests or `doctor` runs were performed.

### The one idea to steal

Make the smallest unit of adoption one useful link to knowledge the repository already trusts.

---

## Layering Architect (Proposal H)

/root - GPT-6 - high

### Thesis

Keep v2’s procedure, direct links and explicit activation list, then restore missing duties where they are used. Present adoption as cumulative setups whose components declare their dependencies, so adding complexity is deliberate and removing it leaves a complete working setup. An existing repository should begin by linking one useful document.

### How v2 removed so many rules

Five kinds of removal explain the reduction.

- **Compression preserved much of the behavior.** Loader L38’s inheritance and conflict rules survive as “Narrower rules add to broader ones” and “When two rules conflict, say so instead of picking one.” L75’s overwrite boundary survives as “wins where they differ, for that file only.” These cuts were sound.
- **Rules moved beside their operations.** Working L18 moved to `saving.md`. Loader L132–138 moved into the CLI Skill and its references. These moves were sound when users still encounter the destination before acting. Moving Memory’s update deadline into removable saving policy was not sufficient.
- **Definitions and examples disappeared.** Loader L46–49 removed routing vocabulary. L85 removed the C# and Windows example. These belong outside startup reading, but removing their explanatory home made customization harder.
- **Authoring instructions were treated as ordinary competence.** Loader L28’s useful links and consistent tags disappeared. Patterns L19’s acceptance gate also disappeared. Neither behavior follows merely from having descriptions or capable models. Both need explicit homes.
- **Operational safeguards became implications.** Loader L32 lost investigation before editing conflicting accepted sources. L101 lost the deadline before dependent work. “Update its record instead of adding a competing one” implies neither investigation nor timing. These were losses.

The ledger’s “Kept” classifications also need checking. For example, the shortened Pattern rule omits optional and repeatable parts, while Archived’s “Remove loading tags” does not cover metadata claiming current authority.

### The lossless ledger

All paths below are relative to `src/open-forge/`. Each quotation is proposed exact replacement or additional text. Retain surviving v2 text unless explicitly replaced.

**`.agents/loader.md`, `### Use what you read`**

- loader L21 — **Restore:** “Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.”
- loader L22 — **Restore:** “Use the fewest words that preserve meaning, readability, and logical progression. Remove filler and repetition. Keep the context, conditions, and exceptions needed to understand and apply the result.”
- loader L27 — **Restore:** “Let each source answer one clear question. Link to related sources instead of repeating their detail. A link does not change either source’s authority, scope, loading rules or lifecycle.”
- loader L32 — **Restore:** “Investigate conflicting accepted sources before editing either. When accepted direction changes current state, update its defining source and preserve useful prior context.”
- loader L40 — **Restore:** “Selected non-binding material may specialize broader material of the same kind within its accepted scope.”
- loader L100 — **Restore:** “#Evergreen means material must stay aligned with accepted current state. It creates no authority or loading behavior.”
- loader L101 — **Restore:** “Update affected #Evergreen sources you may edit before work depends on them and no later than closeout. Batch related updates when safe.”
- loader L102 — **Restore:** “Keep each source aligned with what it represents now. Preserve useful prior context in an enabled Memory route or another existing project source. Report needed updates that are blocked or not authorized.”

**`.agents/loader.md`, `### Read before you work`**

- loader L62 — **Restore:** “Use catalogues for navigation. Do not open ordinary file bodies just to discover routes or tagged files.”
- loader L93 — **Restore:** “Reread #KeepInMind entries before handoff or finish, only in active branches.” The preceding navigation rule preserves the other boundary.
- loader L94 — **Restore:** “During work, recheck them only when they may have changed.”

**`.agents/README.md`, `## Instructions`**

- loader L28 — **Relocate:** “Connect related material with relative links to local sources and precise references. Reuse consistent descriptive tags for shared subjects. Add links and tags only when they help navigation or retrieval.”
- loader L37 — **Relocate:** “For a catalogue without local rules, use `## Axioms` containing only `- inherited - No local axioms; loaded ancestor axioms remain active.` Existing absent or empty sections also add no rules. Never combine that marker with local rules.”
- loader L57 — **Relocate:** “Scopes may nest before, between or after deeper route segments. Each narrows everything that follows it.”
- loader L58 — **Relocate:** “A scope contains only useful routes. It need not copy another scope or the installed defaults.”

These instructions become required before authoring context. Their narrower reading trigger matches their original authoring scope.

**`.agents/README.md`, `## Terms you may meet`**

- loader L46 — **Relocate:** “A route is a navigable path through catalogues and Entries.”
- loader L47 — **Relocate:** “A root route starts in the loader.”
- loader L48 — **Relocate:** “A scope narrows the content that follows it.”
- loader L49 — **Relocate:** “A slug is a concrete folder name in a route.”
- loader L85 — **Relocate:** “For example, a C# scope may contain a #LoadNow design file and an on-demand Windows scope. Selecting C# loads its design file. A #LoadNow file under Windows loads only after Windows is selected.”
- loader L95 — **Relocate:** “#Core labels base routing, loading, workspace orientation and reusable agent-facing roles.”
- loader L96 — **Relocate:** “#Memory labels self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history.”
- loader L97 — **Relocate:** “#Extension labels optional packaged routes, capabilities, integrations and support files.”

**`.agents/maps/_maps.md`, `## Axioms`**

- maps L23 — **Restore:** “Keep Maps broad. Prefer modules, projects, repositories, systems or scopes over members and functions unless finer routing clearly helps.”
- maps L24 — **Restore:** “Choose Map filenames, grouping and nesting depth to suit the workspace.”
- maps L25 — **Restore:** “Maps do not replace Memory or the destination’s current information.”

**`.agents/patterns/_patterns.md`, `## Axioms`**

- patterns L19 — **Restore:** “Create or update a Pattern only for an accepted shape that should guide future related work. Do not promote one-off work, temporary transitions or unsettled candidates into a Pattern.”
- patterns L25 — **Restore:** “Link related rules, advice, maps, procedures and starting files, and identify which sources bind the shape. Sources that apply the shape must link back to its Pattern.”
- patterns L26 — **Restore:** “Keep required behavior, procedures and substantial choice rationale in their defining sources. Example names and values illustrate the shape unless an applicable requirement fixes them.”
- patterns L28 — **Restore:** “Adapt only within authority granted for the work. Report justified adaptations as departures, separately from conformance.”

**`.agents/templates/_templates.md`, `## Axioms`**

- templates L24 — **Restore:** “If a copied artifact needs continuing guidance or requirements, link their defining source. The Template supplies starting content only.”
- templates L28 — **Restore:** “State the need and primary question each Template answers in its description and source instructions.”
- templates L29 — **Restore:** “Use focused prompts that request necessary content once, in a readable order. Mark optional sections clearly.”
- templates L30 — **Restore:** “Treat generic Templates as fallbacks. Add a specialized Template only when it provides meaningfully different starting content.”

**`.agents/memory/_memory.md`, opening paragraph**

- memory L13 — **Restore:** “Memory is self-growing Markdown state. People and agents may add useful records and routed scopes without a fixed structural limit. Unselected branches do not load their contents. Records need not pass through every state.”

**`.agents/memory/_memory.md`, `## Axioms`**

- memory L19 — **Restore:** “An accepted record can define current knowledge or decisions. Recording another category’s content does not give the record that category’s role.”
- memory L26 — **Restore:** “Distinguish what was stated, observed or inferred.”
- memory L27 — **Restore:** “Update affected sources before other work depends on the outcome and no later than closeout. Keep useful context and reasoning in Memory. Accepted durable knowledge must reach its defining source rather than remain only in chat or an unaccepted record.”
- memory L32 — **Restore:** “Add records and routed scopes when their likely future value justifies the cost of finding and reviewing them.”
- memory L35 — **Restore:** “Named record categories and Templates are optional. Use an existing state and the narrowest useful scope. A record may live directly under that state. Do not recreate a removed category merely to save a record.”
- memory L36 — **Restore:** “Use an existing suitable record before creating another. Within granted authority, add an ordinary record or scoped route when needed. Ask when placement would change accepted meaning or cross an unresolved authority boundary.”

**`.agents/memory/working/_working.md`, `## Axioms`**

- working L19 — **Restore:** “An explicitly accepted temporary choice may remain here when its source, scope and expected expiration are clear.”

**`.agents/memory/emerging/_emerging.md`, `## Axioms`**

- emerging L17 — **Restore:** “At each applicable #KeepInMind refresh, check visible Entries and save useful candidate material before it is lost.”
- emerging L23 — **Restore:** “Refine material while its validity, use or final destination remains unsettled.”
- emerging L25 — **Restore:** “Revisit repeated or stale material and decide whether to consolidate, promote, archive or prune it. Review may leave a record unchanged.”

Restore Emerging’s original refresh reach as well. Its frontmatter becomes `tags: [KeepInMind, Memory, Emerging, Contextual, Candidate]`. Its line in `.agents/memory/_memory.md` becomes:

```markdown
- [Ideas, findings and analysis that are not accepted yet](emerging/_emerging.md) - #KeepInMind #Memory #Emerging #Contextual #Candidate
```

**`.agents/memory/archived/_archived.md`, `## Axioms`**

- archived L17 — **Restore:** “Retain, consolidate or transform history according to its future value. Archival need not preserve every detail.”

Those are all 46 Dropped and Partly rows. No dropped requirement is dismissed as implied.

Further patches correct losses hidden by other classifications:

- Patterns, `## Axioms`: “Label required, optional and repeatable parts, their nesting and relationships, and fixed or illustrative names, values and orders. Label incomplete examples schematic and identify omissions.”
- Templates, `## Axioms`: “Preserve the destination’s required structure. Make metadata describe its ownership, scope, state, authority and relationships. Remove package tags that do not describe the destination.”
- Archived, `## Axioms`: “Remove metadata that would keep its former behavioral role active or claim current authority.” Replace its deletion sentence with “Delete archived material only under user direction or accepted retention preferences.”
- Memory, `## Axioms`: “Get explicit user direction before creating a new top-level Memory state.”
- Directives, `## Axioms`: replace the unconditional child-catalogue requirement with “A child catalogue may be on demand. Use #LoadNow and applyTo when its rules must load automatically for matching work.”

### The complexity ladder

Let **K** mean `AGENTS.md`, `.agents/loader.md` and `.agents/README.md`. A `CLAUDE.md` bridge is added where needed. CLI installations also have settings and ownership JSON, which add no agent context.

Every row includes the preceding row’s files and existing document links.

| Level | Installed tree added | What it provides and costs | Move up or down |
|---|---|---|---|
| 0. Existing documents | K, with a loader entry pointing to an existing file | Required or relevant reading without reorganizing docs. Cost is maintaining one entry and reading the loader. | Add Directives. Down means removing the Open Forge handoff and retaining project documents. |
| 1. Project rules | `.agents/directives/_directives.md`, plus authored rules | Required behavior with explicit conditions. Cost is reviewing rules and their startup reading. | Add accepted knowledge. Down removes the Directives loader entry after resolving independent requirements. |
| 2. Accepted knowledge | `memory/_memory.md`, `memory/crystallized/_crystallized.md`, plus records | Accepted facts stay current without named record categories. Cost is maintaining acceptance and avoiding competing sources. | Add Working. Before moving down, transfer needed facts into an existing linked source, then disable Memory. |
| 3. Continuity | `memory/working/_working.md`, `memory/working/saving.md`, plus records | Resume reading and automatic progress capture. Cost is writing and retiring concise records. | Add the remaining components. Down preserves unfinished state elsewhere and removes Working’s folder and entry together. |
| 4. Full Core | Guidance, Maps, Patterns, Skills and Templates catalogues, CLI Skill and its four references, Emerging and Archived catalogues | Every built-in role and Memory state. Cost includes Emerging refreshes, more navigation and selected content. | Add a chosen Extension. Down disables added roots and moves unwanted nested Memory states outside the enabled tree before removing their entries. |
| 5. Selected methods | Complete `content/` trees of chosen Extensions and their dependency closure | Workflows and richer records. Cost is each selected method’s requirements and package maintenance. | Add another package deliberately. Down removes dependents first and preserves independently maintained copies. |

The dependency graph is small:

- Direct links and each Core category need K.
- Every Memory state needs Memory.
- Saving needs Working.
- States do not require their siblings.
- Extensions require their declared package dependencies and the Framework destinations their usable content needs.

These are reference setups, not mandatory steps. Working can be selected without Patterns. Existing Essentials remains a shortcut combining level 3 with Patterns, Skills and Emerging.

An enabled component must be reachable through listed Entries and have its required dependencies enabled. Tags and conditions then determine reading. The CLI enforces built-in and manifest dependencies. Agents check dependencies declared in custom prose.

Start brownfield adoption at **level 0**. It demonstrates value using knowledge the repository already maintains.

### Ultra-lightweight install

Add **`--preset minimal`**, displayed as **Existing documents**. It is a selection preset using the existing installer, not another runtime mode.

On a fresh repository it writes or appends exactly:

```text
AGENTS.md                         managed block
CLAUDE.md                         existing native bridge form
.agents/loader.md                 empty Entries
.agents/README.md
.agents/open-forge.json
.agents/open-forge.lock.json
```

It installs no category folders, Memory, saving policy, CLI Skill, Extensions or Git-ignore rules. Existing files remain untouched outside recognized managed host spans. With no selected routed metadata, defer the frontmatter question and use the existing preference or the fresh unattended root default.

Allow `--route` with `minimal` and `custom`. Minimal starts with every row omitted, then applies explicit overrides:

```sh
open-forge install --preset minimal --route directives=add --route memory/crystallized=add --dry-run
```

This adds Directives, Memory and Crystallized only. `git-ignore` retains its sharing meaning. `remove` retains its existing management-release meaning. Conflicting duplicate selections remain invalid.

Plan wording: “Install the reading procedure and project guide. No categories or automatic saving are selected. Existing project documents stay where they are.” Overrides list their added files and required ancestors.

Under `src/cli/operations/OpenForge.Cli.Operations/Commands/Install/`, change:

- `Models/Configuration/InstallConfiguration.cs`
- `Models/Binding/InstallBindingModels.cs`
- `Shared/Binding/InstallBindingInputReader.cs`
- `Shared/Configuration/InstallConfigurationChoices.cs`
- `InstallSetupResolver.cs`, `InstallFrontmatterResolver.cs`, `InstallConfigurationSettings.cs` and `InstallConfigurationIntendedStateBuilder.cs` in that configuration folder
- `InstallIgnoreSection.cs` and `InstallConfigurationSharing.cs` to keep the saving policy shareable when Working records are private

Also change Rendering’s `InstallSetupQuestions.cs`, `InstallSetupRecapSelector.cs` and `InstallReportSelector.cs`, plus corresponding Install OutputText files. Fix `Framework/GeneratedNavigation/GeneratedNavigationRegionPlanner.cs` so preserving an explicitly empty loader never repopulates it.

This is a medium installer and presentation change. It needs selection, preservation, fresh-checkout and process tests plus snapshots. Entry syntax, Markdown parsers and settings schema remain unchanged. Existing installations retain their choices until explicitly configured.

### Brownfield adoption

The following is the proposed v3 journey. Assume the existing `docs/` contains `architecture.md`. Substitute a real discovered source otherwise.

1. Inspect the baseline and preview installation:

   ```sh
   git status --short
   git diff
   rg --files docs
   open-forge install --preset minimal --dry-run --detail standard
   open-forge install --preset minimal --automatic
   ```

   The agent now reads the existing host instructions and loader. No optional component is active.

2. Select currently accepted sources and add these loader entries:

   ```markdown
   - [Project overview and setup](../README.md) - #Project
   - [Contribution instructions to follow](../CONTRIBUTING.md) - #LoadNow
   - [Accepted architecture for implementation work](../docs/architecture.md) - #LoadNow - applies to `src/**`
   ```

   Do not link `AGENTS.md` or `CLAUDE.md` back through the loader. Their host loading already applies.

3. Check the two reading cases:

   ```sh
   open-forge context --for src/example.cs
   open-forge context --for docs/example.md
   ```

   Both require CONTRIBUTING. Only the source task requires architecture through this condition. README remains relevant reading. Prose-required reads still apply.

4. Add one new rule when a real correction warrants it:

   ```sh
   open-forge route init directives --framework --dry-run
   open-forge route init directives --framework
   ```

   Write the rule with `## Instructions`, loading metadata and any file condition. Preview and apply Index. The agent then reads Directives and matching mandatory rules.

5. Add accepted knowledge without installing other Memory states:

   ```sh
   open-forge install --configure --preset custom --route memory/crystallized=add --dry-run
   open-forge install --configure --preset custom --route memory/crystallized=add --automatic
   ```

   The agent additionally reads Memory and Crystallized catalogues, then relevant records. Existing ADRs stay in `docs/`.

Open Forge must never relocate existing docs, manufacture historical acceptance, normalize their frontmatter, replace unrelated host text, or silently enable omitted components.

Build clear minimal-preset help and copyable direct-entry examples. A document-discovery wizard can wait until strangers demonstrate a need for it.

### The introduction

This replacement targets v3, including the proposed `minimal` preset.

````markdown
## Open Forge

Open Forge helps coding agents use the rules, docs and decisions your project already has. It is a plain Markdown framework: you choose what agents should read, when it applies, and which useful outcomes should survive the next session.

Start with one existing document. Add rules, memory or reusable methods when they solve a problem you have. Your documents can stay where they are, and the CLI is optional.

### Get started

With Node.js 22.18 or later, install the CLI and preview the smallest setup from your project:

```sh
npm install -g @thelithiumforge/open-forge@beta
open-forge install --preset minimal --dry-run --detail standard
```

Review the plan, then apply it:

```sh
open-forge install --preset minimal --automatic
```

Existing text in `AGENTS.md` and `CLAUDE.md` stays outside the marked Open Forge section. Review new files with `git status` as well as `git diff`.

### Use an existing document

Add this line under `## Entries` in `.agents/loader.md`:

```markdown
- [Contribution instructions to follow](../CONTRIBUTING.md) - #LoadNow
```

That file stays where it is and needs no frontmatter. `#LoadNow` requires reading it when the loader loads. Leave the tag off for a document agents should open when it fits the task.

Try a normal request:

> Explain how this project runs its tests. Follow AGENTS.md and name the instructions you used.

Check that the answer follows your existing contribution instructions. Linking a document does not make unaccepted claims authoritative.

### Add rules and accepted knowledge

If you want dedicated homes for new rules and accepted facts, preview and apply each addition:

```sh
open-forge route init directives --framework --dry-run
open-forge route init directives --framework
open-forge install --configure --preset custom --route memory/crystallized=add --dry-run
open-forge install --configure --preset custom --route memory/crystallized=add --automatic
```

For example, create `.agents/directives/verification.md`:

```markdown
---
description: Report verification for source changes
tags: [LoadNow, Directive, Testing]
applyTo: ["src/**"]
---

## Verification

### Instructions

- Report the checks run for a source change and their results. Explain any required check you could not run.
```

Preview `open-forge index --dry-run`, then run `open-forge index`. Without the CLI, add its entry to `directives/_directives.md` yourself:

```markdown
- [Report verification for source changes](verification.md) - #LoadNow #Directive #Testing - applies to `src/**`
```

Crystallized Memory holds accepted knowledge. Put a new record there only when an existing document does not already define the answer. Keep existing ADRs and specifications in place and link to them.

### Resume work across sessions

Add Working Memory when unfinished work needs to survive a pause:

```sh
open-forge install --configure --preset custom --route memory/working=add --dry-run
open-forge install --configure --preset custom --route memory/working=add --automatic
```

Working reads the relevant resume record at task start. Its separate saving policy asks agents to record the goal, current state, next step and open questions before pausing unfinished work.

You can remove that policy while keeping Working records. You can also keep progress in your existing task documents instead.

### Choose how much to adopt

| You need | Start with |
|---|---|
| Agents using existing instructions | Minimal and one document link |
| New project rules and accepted facts | Directives and Crystallized Memory |
| Work continuing across sessions | Working Memory and its saving policy |
| Every built-in content role | Full Core |
| A particular planning or development method | A selected Extension |

These are useful combinations. Every category is optional, and you can select components individually with Custom. Memory states need Memory, but they do not require each other. Extensions declare their dependencies.

### Without the CLI

Copy `.agents/loader.md` and `.agents/README.md` from `src/open-forge/`, then append the supplied managed block to your existing `AGENTS.md`. Retain an existing `.agents/` tree and merge deliberately. Add the supplied Claude bridge if your tool needs it.

Start with an empty loader Entries section, then add your document links. Maintain those links with your editor. The files contain the reading procedure and authoring instructions.

### Change your setup

The loader's Entries select what Open Forge uses. Remove a loader entry to disable a branch while retaining its notes. Resolve dependencies first. Disabling a branch does not revoke accepted project requirements or instructions your agent tool loads independently.

CLI updates can replace directly edited shipped files. Use an adjacent `{name}.overwrite.md` for local adjustments that should survive updates, and record managed removals before updating.

Open Forge supplies context and instructions. Your agent tool supplies execution, and you still review its work.

Read [.agents/README.md](src/open-forge/.agents/README.md) for customization, or continue with the [installation guide](src/docusaurus/docs/getting-started/installation.md).
````

Complete `.agents/README.md`:

````markdown
## Open Forge in this project

Open Forge tells agents where this project's rules and knowledge live. You choose the links in [loader.md](loader.md). The files stay yours, and everything works without the CLI.

Agents read the loader before work. They read this page when authoring workspace context, catalogues or metadata. People can start here.

### Start with one existing document

Add a line under the loader's `## Entries`:

```markdown
- [Contribution rules to follow](../CONTRIBUTING.md) - #LoadNow
```

The document stays where it is and needs no frontmatter. #LoadNow requires reading it whenever the loader loads. Add `- applies to` and a pattern in backticks, such as `**/*.cs`, to limit it to matching work. Without a loading tag, agents open it when its description fits the task.

Linking does not establish acceptance or grant permission to edit the destination. Existing accepted requirements retain their authority.

### Add only what helps

Directives provide a home for required behavior. Memory can hold accepted knowledge, work in progress, unsettled findings or history. Each Memory state needs Memory, but the states do not need each other. The saving policy needs Working Memory.

Guidance offers advice. Patterns show reusable shapes. Templates provide starting files. Maps point to sources. Skills expose capabilities through native `SKILL.md` packages. Each needs only the loader unless its own content declares another dependency.

You may keep existing ADRs and docs instead of adopting any of these folders. Named record categories and workflows are optional Extensions.

### Turn something off

Remove its loader entry to disable an entire branch and retain its notes. Remove or replace enabled dependents first. Existing accepted requirements and independently loaded agent instructions still apply.

Inside a catalogue, the list follows files on disk. Move a file outside that routed folder and remove its entry to retain it as a note, or delete the file and its entry if you want it gone. CLI-managed removals also need recorded exclusions before updates. Custom's `remove` action releases management and retains existing routes. It does not disable them.

To stop automatic progress saving while keeping Working records, remove `memory/working/saving.md` and its entry in `memory/working/_working.md`. Record that file removal when using managed updates. Emerging's candidate capture is a separate choice and remains active while that state is enabled with #KeepInMind.

### Instructions

These instructions apply when authoring workspace context, catalogues or metadata.

- Connect related material with relative links to local sources and precise references. Reuse consistent descriptive tags for shared subjects. Add links and tags only when they help navigation or retrieval.
- Put detail in the narrowest source that defines it. Parent catalogues keep the summary and link needed to select that source. Do not repeat inherited rules.
- A folder has one catalogue named `_{folder-name}.md`. Compatibility names are `index.md`, `_index.md`, `references.md` and `_references.md`.
- For a catalogue without local rules, use `## Axioms` containing only `- inherited - No local axioms; loaded ancestor axioms remain active.` Existing absent or empty sections also add no rules. Never combine that marker with local rules.
- Scopes may nest before, between or after deeper route segments. Each narrows everything that follows it.
- A scope contains only useful routes. It need not copy another scope or the installed defaults.
- New routed `.agents/` files need YAML `description` and `tags`, with optional `applyTo` and `responsibility`. Descriptions help readers choose files. Responsibility states what a file defines and adds no authority. Native `SKILL.md` keeps its native metadata.
- Follow `frontmatter` in `open-forge.json`: `root` puts fields at the top level and `scoped` puts them under `open-forge:`. A missing setting means scoped. Keep all Open Forge fields together. Tags are names without `#`.
- Write `applyTo` as a list of glob patterns or a comma-separated string. Root and scoped conditions are both read. Equivalent sets count once and conflicting sets are invalid. Repeat those conditions after an entry's tags as `applies to` patterns.
- Keep each entry's relative path, description, tags and conditions current. With the CLI, preview `open-forge index --dry-run` before applying `open-forge index`. Without it, maintain Entries by hand.
- For an adjustment that survives updates, use an adjacent `{name}.overwrite.md`. Direct edits to shipped files may be replaced by Update. Review its plan before applying it.

### Terms you may meet

A route is a navigable path through catalogues and Entries. A root route starts in the loader. A scope narrows the content that follows it. A slug is a concrete folder name in a route.

#Core labels base routing, loading, workspace orientation and reusable agent-facing roles. #Memory labels self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history. #Extension labels optional packaged routes, capabilities, integrations and support files. These labels do not activate content or establish management.

For example, a C# scope may contain a #LoadNow design file and an on-demand Windows scope. Selecting C# loads its design file. A #LoadNow file under Windows loads only after Windows is selected.

### What the CLI adds

`open-forge context --for <file>` gathers required reads. Repeat `--for` for all working files. Agents still choose relevant entries and follow explicit reading instructions.

The CLI can maintain navigation, preview installation changes and record removals. It does not decide which knowledge you have accepted. Settings and ownership JSON support maintenance and add no agent authority.
````

### v3

This is the Full Core loader. Minimal uses the same text with an empty Entries section. Other selections emit only their chosen entries.

```markdown
## Open Forge Loader

This workspace uses the context linked under `Entries`. Listing a file makes it available to Open Forge. Loading tags and task relevance decide when to read it. Unlisted files remain background notes. Existing project requirements and instructions loaded by your agent tool still apply.

### Axioms

### Read before you work

1. At task start or resume, and after context restoration, identify your working files. Include files you will investigate, review, create, change, rename or delete, including planned files and both paths of a rename. Background reading does not make a file a working file.
2. If the CLI is available and allowed, run `open-forge context` with `--for <file>` repeated for every known working file. Read its output. This covers the required reads and conditions in steps 3 and 4. Always do step 5 yourself. If the command fails or cannot follow a link, complete steps 3 to 5 with file tools.
3. Read entries tagged #LoadNow or #KeepInMind in listed order. Check step 4 before opening a conditioned entry. A catalogue lists a folder's context under Entries. In each catalogue you read, repeat this procedure for its Entries. Never skip a required read because its description seems unrelated. Read each file once per pass.
4. Conditions filter reading and application. Within one `applies to` list, any pattern may match. One working file must match a pattern at every conditioned ancestor and at the entry itself. Patterns are case-sensitive globs from the workspace root. For example, `*.py` matches root files, `**/*.py` matches any depth, and `docs/**` matches a folder's contents. A leading `!` is literal. Conditions never select an unopened branch or grant edit permission. If working files are unknown, report required conditioned entries as pending and resolve them before matching work.
5. Check other enabled entry descriptions and open those that fit the task. Follow explicit required-read links. Before applying a listed source found through search or a direct link, read its catalogues first. Keep separately selected branches separate.

Restart when the goal or working files change. Reread #KeepInMind entries before handoff or finish, only in active branches. During work, recheck them only when they may have changed. Use catalogues for navigation. Do not open ordinary file bodies just to discover routes or tagged files.

### Use what you read

- Follow loaded catalogue `## Axioms` and rule-file `## Instructions` within their stated scope. Only this loader and recognized loaded catalogues define Axioms. Narrower rules add to broader rules. Report conflicts or instructions you cannot follow, and explain why.
- Follow user direction within platform and safety limits. Take routine, reversible steps within the request without asking again. State assumptions that affect the result. Ask before unresolved choices significantly change the result, scope, risk, cost, external effects or ability to undo the work.
- Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.
- Use the fewest words that preserve meaning, readability, and logical progression. Remove filler and repetition. Keep the context, conditions, and exceptions needed to understand and apply the result.
- Reading, linking, saving or tagging content does not make it accepted or permit editing it. Acceptance comes from the user or an authority they declared for that question. Use declared external authorities for their assigned facts. #Contextual marks unaccepted context and #CurrentTruth marks accepted current state within its scope. Neither tags, repetition nor agent confidence establish acceptance.
- Let each source answer one clear question. Link to related sources instead of repeating their detail. A link does not change either source's authority, scope, loading rules or lifecycle.
- Selected non-binding material may specialize broader material of the same kind within its accepted scope.
- Investigate conflicting accepted sources before editing either. When accepted direction changes current state, update its defining source and preserve useful prior context.
- #Evergreen means material must stay aligned with accepted current state. It creates no authority or loading behavior.
- Update affected #Evergreen sources you may edit before work depends on them and no later than closeout. Batch related updates when safe.
- Keep each source aligned with what it represents now. Preserve useful prior context in an enabled Memory route or another existing project source. Report needed updates that are blocked or not authorized.

### Change what is enabled

- Add a line under Entries to enable a file or catalogue. Links are relative to this file and may reach anywhere in the workspace, such as `../docs/architecture.md`. Existing documents need no move or new frontmatter. Their entry supplies the description, tags and conditions.
- Remove a loader entry to disable that branch while keeping its files as notes. Background reading does not reactivate its rules. Disabling it does not cancel accepted project requirements or instructions loaded independently by your agent tool. Removed choices stay removed until the user requests restoration.
- Before disabling a part, remove or replace enabled dependents that need it. Report unresolved dependencies. Inside other catalogues, Entries follow the folder's files, so remove or move a file to keep its entry out. Record managed removals in the CLI's exclusions before updating.
- Report missing listed files or required dependencies as errors. An empty Entries section is valid.
- #LoadNow requires reading whenever its parent loads. #KeepInMind also requires the refreshes above. Keep required catalogue chains unbroken. Use loading tags only when those reads are needed, and narrow specialized content before making it mandatory. Only these two tags control loading.
- Before authoring workspace context, catalogues or metadata, read [the authoring instructions](README.md#instructions). New routed files use `description`, `tags` and optional `applyTo`. Native `SKILL.md` keeps its native metadata.
- To keep local changes through updates, place them in `{name}.overwrite.md` beside the base file. Read it immediately after the base, with the same role, scope and loading conditions. It wins only for corresponding content in that source. Do not index or select it separately.

### Entries

- [Rules agents must follow in this workspace](directives/_directives.md) - #LoadNow #Core #Directive
- [Advice for recurring choices, to check before choosing an approach](guidance/_guidance.md) - #Core #Guidance
- [Where to find project knowledge, checked before relying on it](maps/_maps.md) - #Core #Map
- [Notes that help work continue, such as resume points, accepted knowledge, ideas and history](memory/_memory.md) - #LoadNow #Memory
- [Default shapes to check before creating or reviewing structure](patterns/_patterns.md) - #Core #Pattern
- [Capabilities to check before specialized work](skills/_skills.md) - #Core #Skill
- [Starting files to copy when creating something new](templates/_templates.md) - #Core #Template
```

Managed `AGENTS.md` block:

```markdown
## Open Forge

Before starting or resuming a task, read `.agents/loader.md` and follow it throughout the task.

If the CLI is available and allowed, `open-forge context --for <file>` gathers required context. Repeat `--for` for every working file. The loader explains which additional files to select and how to continue with file tools if the CLI is unavailable.

**End of Open Forge managed section.**
```

| v2 sentence | v3 sentence | Why |
|---|---|---|
| “what is listed is on, and anything not listed is off” | “Listing a file makes it available to Open Forge.” | Separates activation from automatic reading. |
| “Read and apply an entry with `applies to` patterns only when the same working file matches its patterns…” | “One working file must match a pattern at every conditioned ancestor and at the entry itself.” | Makes alternatives within lists and conjunction across ancestors explicit. |
| “Files under an entry that is not listed are background only.” | “Unlisted files remain background notes.” | States the boundary before the procedure. |
| “Remove a line to turn that part off.” | “Remove a loader entry to disable that branch while keeping its files as notes.” | Avoids implying nested generated entries behave identically. |
| “Every other tag is a search label.” | “Only these two tags control loading.” | Restores semantic maintenance tags without adding CLI behavior. |

Whitespace-delimited counts, including headings, paths and tags: **v2 loader 823 words, v3 loader 1,090 words, managed block 63 words**. The additional 267 loader words restore shared duties and explain conditions more directly.

### Risks and proof

The main risks are higher startup reading, a ladder mistaken for mandatory progression, and dependencies hidden in authored links. Restoring Emerging refreshes also restores candidate-writing work. That cost must be visible at the level where Emerging appears.

Two source-backed release blockers deserve focused proof. `GeneratedNavigationRegionPlanner.ReadLoaderRootPaths` currently returns structural roots when the listed count is zero. Essentials’ documented ignore pattern excludes Working children, so the new saving policy needs an explicit sharing exception and a fresh-checkout test.

The cheapest usability experiment needs three unfamiliar developers and ordinary repositories. Give them only the introduction. Ask each to link one existing document, add one scoped rule, enable continuity, then move down while retaining useful knowledge. Repeat agent tasks with and without the CLI.

Pass when all three finish without coaching, correctly predict required and irrelevant reads, preserve existing documents, and keep disabled behavior off after maintenance. Record time and mistakes instead of asking whether the framework feels simple.

This proposal is based on read-only source inspection and the payload diff. No files were changed, and no Doctor command, build or test suite was run. The CLI launcher was blocked by PowerShell execution policy, so inspection followed the file-reading fallback.

### The one idea to steal

Make every optional component state what it needs, what it adds, and how to remove it while keeping the remaining setup complete.

---

## Brownfield Team Lead (Proposal E)

/root - GPT-6 - high

### Thesis

Keep v2’s reading procedure, direct links, and optional folders, while restoring duties that disappeared rather than merely moved. Make the recommended first trial one link to an existing document, with every write visible and reversible. Complexity should grow when a team wants another capability, without requiring its existing knowledge to change address.

### How v2 removed so many rules

The [ledger](artifacts/council-3/ledger.md) shows five kinds of reduction:

- **Compressed equivalent statements.** Loader L18 becomes “The user’s direction comes first.” Crystallized L18 becomes “update its record instead of adding a competing one.” Sound where conditions and strength survive.
- **Moved instructions beside their use.** Working L18 moves to `saving.md`. Templates L20 moves to the loader’s metadata rule. Sound when the destination is reached before the action, including without the CLI.
- **Removed vocabulary and examples.** Loader L46 defines `route`. L85 illustrates C# and Windows scopes. Moving these to the human guide is sound because the procedure can express their behavior directly.
- **Assumed authoring rules were obvious.** Maps L23 loses its granularity rule. Patterns L19 loses the accepted-shape threshold. These are substantive product opinions, not guaranteed model habits. Restore them where authors work.
- **Narrowed or removed continuing duties.** Loader L101 loses update timing outside Crystallized. Emerging L17 loses capture at refresh. These are losses. “Capable models probably do this” does not preserve a requirement.

The ledger also contains reach changes under **Changed**. Removing startup tags and the CLI Skill’s startup requirement needs an explicit decision. My proposal retains those guarantees when their modules are selected.

### The lossless ledger

Paths below are relative to `.agents/`. Each quoted passage is exact replacement or additional text. Retain v2’s other clauses. All 46 Dropped and Partly rows receive a home.

**Destination: `loader.md`**

- `loader.md` L21 — **Restore**, `Use what you read`: “Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.”
- `loader.md` L22 — **Restore**, `Use what you read`: “Use the fewest words that preserve meaning, readability, and logical progression in communication and authored content. Remove filler and repetition. Keep the context, conditions, and exceptions needed to understand and apply the result.”
- `loader.md` L27 — **Restore**, `Keep sources reliable`: “Give each source one clear question. Keep detail in its narrowest defining source and link instead of repeating it. Links do not change authority, scope, loading or lifecycle.”
- `loader.md` L28 — **Restore**, `Keep sources reliable`: “Use relative local links, precise references and consistent subject tags when they improve navigation or retrieval.”
- `loader.md` L32 — **Restore**, `Keep sources reliable`: “Investigate an apparent conflict between accepted sources before changing either one. When accepted direction changes current state, update its defining source and preserve useful prior context.”
- `loader.md` L37 — **Restore**, `Change what is on`: “For a new catalogue without local rules, put only `- inherited - No local axioms. Loaded ancestor axioms remain active.` under `## Axioms`. Existing empty or missing sections add no rules. Never mix this marker with local rules or repeat inherited rules.”
- `loader.md` L40 — **Restore**, `Use what you read`: “Selected advice and shapes may specialize broader material of the same kind within their accepted scope.”
- `loader.md` L57 — **Restore**, `Change what is on`: “Catalogued folders may nest anywhere below a listed root. Each folder narrows everything after it.”
- `loader.md` L62 — **Restore**, `Read before you work`, step 5: “Use catalogues for navigation. Do not open ordinary file bodies just to discover routes.”
- `loader.md` L93 — **Restore**, `Read before you work`, refresh paragraph: “Refresh only active scopes. Do not open file bodies just to discover tagged files.”
- `loader.md` L94 — **Restore**, same paragraph: “During work, recheck #KeepInMind files only when they may have changed.”
- `loader.md` L100 — **Restore**, `Keep sources reliable`: “#Evergreen marks material that must stay aligned with accepted current state. It grants no authority and does not cause loading.”
- `loader.md` L101 — **Restore**, same section: “Update affected #Evergreen sources you may edit before work depends on them and no later than closeout. Batch related updates when safe.”
- `loader.md` L102 — **Restore**, same section: “Keep each source aligned with what it represents now. Preserve useful prior context where the project keeps history. Report needed updates that are blocked or unauthorized.”

**Destination: `README.md`**

- `loader.md` L46 — **Relocate**, `Terms`: “A route is a navigable path exposed through catalogues and Entries.”
- `loader.md` L47 — **Relocate**, `Terms`: “A root route is exposed directly by the loader.”
- `loader.md` L48 — **Relocate**, `Terms`: “A scope narrows where the content after it applies.”
- `loader.md` L49 — **Relocate**, `Terms`: “A slug is a concrete folder name in a route path.”
- `loader.md` L58 — **Relocate**, `Choose folders`: “Each scope contains only the routes useful there. It need not copy another scope or the installed defaults.”
- `loader.md` L85 — **Relocate**, `Nested context`: “For example, a C# scope may contain a #LoadNow design file and an on-demand Windows scope. Selecting C# loads its design rules, not Windows. A #LoadNow file under Windows loads only after Windows is selected.”
- `loader.md` L95 — **Relocate**, `Terms`: “#Core labels routing, loading, workspace orientation and reusable content roles.”
- `loader.md` L96 — **Relocate**, `Terms`: “#Memory labels self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history.”
- `loader.md` L97 — **Relocate**, `Terms`: “#Extension labels optional packaged routes, capabilities, integrations and support files.”

**Destination: `maps/_maps.md`, `Axioms`**

- Maps L23 — **Restore**: “Keep Maps broad. Prefer modules, projects, repositories, systems or scopes over members and functions unless finer routing clearly helps.”
- Maps L24 — **Restore**: “Each workspace chooses its Map filenames, grouping and nesting depth.”
- Maps L25 — **Restore**: “Maps do not replace Memory or the destination’s current information.”

**Destination: `patterns/_patterns.md`, `Axioms`**

- Patterns L19 — **Restore**: “Create or update a Pattern only when an accepted shape should guide future related work. Do not promote one-off work, temporary transitions or unsettled candidates merely because they have structure.”
- Patterns L25 — **Restore**: “Link related rules, advice, maps, procedures and starting files, and say which bind the shape. Sources using the shape should link back to its Pattern.”
- Patterns L26 — **Restore**: “Keep required behavior, procedures and substantial choice rationale in their defining sources. Example names and values are illustrative unless an applicable requirement fixes them.”
- Patterns L28 — **Restore**: “Reviews report even justified adaptations as departures, separately from conformance.”

**Destination: `templates/_templates.md`, `Axioms`**

- Templates L24 — **Restore**: “Link continuing guidance or requirements from the new artifact to their defining source. The Template supplies starting content only.”
- Templates L28 — **Restore**: “State the need and primary question each Template answers in its description and source instructions.”
- Templates L29 — **Restore**: “Use focused prompts that request necessary content once, in a readable order. Mark optional sections clearly.”
- Templates L30 — **Restore**: “Treat generic Templates as fallbacks. Add a specialized Template only when its starting content meaningfully differs.”

**Destination: `memory/_memory.md`**

- Memory L13 — **Restore**, opening: “Memory is self-growing Markdown state. People and agents can add useful records and routed scopes without a fixed structural limit. Unselected branches do not load their contents. Records need not pass through every state.”
- Memory L19 — **Restore**, `Axioms`: “An accepted record can define current knowledge or decisions. Recording another category’s content does not give the record that category’s role.”
- Memory L26 — **Restore**, `Axioms`: “Distinguish what was stated, observed or inferred when that affects trust or later acceptance.”
- Memory L27 — **Restore**, `Axioms`: “Update affected sources before other work depends on the outcome and no later than closeout. Keep useful context and reasoning in Memory. Accepted durable knowledge must reach its defining source, not remain only in chat or a #Contextual record.”
- Memory L32 — **Restore**, `Axioms`: “Add records and routed scopes when their likely future value justifies the cost of finding and reviewing them.”
- Memory L35 — **Restore**, `Axioms`: “Named record categories and Templates are optional. Use an existing state and the narrowest useful scope. Records may live directly under a state. Do not recreate a removed category merely to save a record.”
- Memory L36 — **Restore**, `Axioms`: “Within the authority granted for the work, add an ordinary record or scoped route when needed. Ask when placement would change accepted meaning or cross an unresolved authority boundary.”

**Destination: `memory/working/_working.md`, `Axioms`**

- Working L19 — **Restore**: “An explicitly accepted temporary choice may remain here when its source, scope and expected expiration are clear.”

**Destination: `memory/emerging/_emerging.md`, `Axioms`**

- Emerging L17 — **Restore**: “At each applicable #KeepInMind refresh, check visible Entries and save useful candidate material before it is lost.”
- Emerging L23 — **Restore**: “Refine material while its validity, use or final destination remains unsettled.”
- Emerging L25 — **Restore**: “Revisit repeated or stale material and decide whether to consolidate, promote, archive or prune it. Review may leave a record unchanged.”

**Destination: `memory/archived/_archived.md`, `Axioms`**

- Archived L17 — **Restore**: “Retain, consolidate or transform history according to its future value. Archival need not preserve every detail.”

The remaining classifications also need corrections. Restore Emerging’s `KeepInMind` metadata and parent entry. Preserve selected catalogues’ original startup tags, and restore the selected Skills catalogue’s startup read of an intentionally present CLI Skill. Restore explicit permission for a new top-level Memory state, accepted retention preferences, removal of former authority metadata when archiving, irrelevant package-tag removal from Template copies, and Pattern labels for optionality, repetition, nesting and omitted schematic detail.

These restore behavior. Users can still explicitly turn modules off.

### The complexity ladder

The levels are useful stopping places. Directives alone or Crystallized alone are also valid choices.

| Level | Installed tree, beyond existing project files | Benefit and cost | One step up or down |
| --- | --- | --- | --- |
| **1. Connect a document** | `AGENTS.md` block, `.agents/loader.md`. CLI also writes `.agents/README.md` and both `open-forge*.json` files | Existing conventions become required reads. Cost is the loader plus selected documents | Add one category. To leave, remove the block and only trial-created files |
| **2. Keep rules and accepted knowledge** | Level 1 plus `directives/_directives.md`, `memory/_memory.md`, `memory/crystallized/_crystallized.md`, useful records | Keep new corrections and accepted knowledge. Existing ADRs remain where they are. Cost is maintaining those sources | Add Working when resumptions hurt. Remove Directives and Memory loader entries to return to level 1 |
| **3. Continue across sessions** | Level 2 plus `memory/{working,emerging,archived}/_{state}.md` and `memory/working/saving.md` | Preserve unfinished work and uncertainty. Cost is saving and reviewing useful state | Add remaining Core categories individually. To shrink, preserve records first, then remove unwanted nested routes and record exclusions |
| **4. Full framework** | Level 3 plus `guidance/_guidance.md`, `maps/_maps.md`, `patterns/_patterns.md`, `templates/_templates.md`, `skills/_skills.md`, the CLI Skill and its four references | Shared advice, navigation, shapes, starters and capability discovery. Cost includes their reading and maintenance rules | Add only useful Extensions. Step down by disabling their roots and releasing unwanted management |

A brownfield repository should start at **level 1**. Its first result should prove that an existing instruction reaches a fresh agent. Folder creation earns its place after that.

Nested routes require special care because v2 regenerates their Entries from files. Preserve records before removal. Custom `remove` alone does not disable retained content.

### Ultra-lightweight install

Add **`--preset minimal`**. It selects no category routes and starts with empty loader Entries. It writes the loader, human README, settings and lock record, plus the managed `AGENTS.md` block.

Add **`--bridge agents|both`**. Minimal defaults to `agents`. Existing presets keep their current bridge behavior. Persist the omitted Claude bridge through existing exclusions, without changing user text. Later configuration retains that choice unless explicitly changed.

Allow existing `--route <id>=<add|remove|git-ignore>` overrides with Minimal as well as Custom. Minimal begins with every row omitted. Custom retains its existing baseline. Record separate state exclusions so restoring Crystallized does not require enabling unrelated Memory states.

The fresh plan says:

> Append the Open Forge block to AGENTS.md and create loader.md, README.md, open-forge.json and open-forge.lock.json under .agents/. Enable no category routes and leave CLAUDE.md, project documents and .gitignore unchanged.

Growth uses existing `route init <category> --framework`, or explicit configuration.

C# changes belong in Install’s `InstallConfiguration.cs`, `InstallConfigurationChoices.cs`, `InstallBindingModels.cs`, `InstallBindingInputReader.cs`, `InstallSetupResolver.cs`, `InstallConfigurationSettings.cs`, `InstallConfigurationIntendedStateBuilder.cs` and `InstallNextCommand.cs`. Update `FrameworkPayloadSelection.cs`, `GeneratedNavigationRegionPlanner.cs`, `InstallSetupQuestions.cs`, `InstallReportSelector.cs`, `InstallSetupRecapSelector.cs`, and Install’s three text files.

Cost is a moderate installer change with focused selection, preservation, repeat-install, argv, JSON and snapshot coverage. Keep the Markdown grammar unchanged. The revised inherited marker additionally requires both spellings in `RouteInspectAxiomsProfileBuilder.cs`, and new output in `RouteScaffoldComposer.cs` and `WorkspaceAdoptionEntrypointComposer.cs`. Existing files need no marker migration.

### Brownfield adoption

Commands using `minimal` or `--bridge` below are proposed interfaces, not currently available flags.

1. **Inspect the existing entry instructions.**

   ```sh
   git status --short
   git diff -- AGENTS.md CLAUDE.md CONTRIBUTING.md README.md
   git ls-files -- docs
   ```

   Read both agent instruction files and check existing imports. The agent still reads exactly what its host currently loads.

2. **Preview the tiny installation.**

   ```sh
   open-forge install --preset minimal --bridge agents --dry-run --detail standard
   ```

   No reading changes yet. I cancel here if the plan proposes moving an ADR, rewriting a guide, adopting unrelated Skills, or replacing an existing instruction file.

3. **Apply the reviewed request.**

   ```sh
   open-forge install --preset minimal --bridge agents --automatic
   git status --short
   git diff -- AGENTS.md CLAUDE.md CONTRIBUTING.md README.md
   ```

   Inspect every new file too. The agent now reads existing host instructions and the loader. Empty Entries adds no category behavior.

4. **Add these exact loader entries.**

   ```markdown
   - [Contribution rules for all work](../CONTRIBUTING.md) - #LoadNow
   - [Repository introduction and documentation pointers](../README.md) - #Project
   ```

   The agent must read `CONTRIBUTING.md`. It opens README and existing `docs/` sources when relevant. No document receives frontmatter.

5. **Try one fresh session.**

   > Explain this project’s test commands using its contribution guide. Locate the relevant existing documentation. Name the sources you read and report conflicting instructions. Do not edit files.

   For a Claude trial, retain an existing `AGENTS.md` import. If none exists, append `@AGENTS.md` once to `CLAUDE.md` after inspecting its current imports.

6. **Grow only after success.**

   ```sh
   open-forge route init memory/crystallized --framework --dry-run
   open-forge route init memory/crystallized --framework
   ```

   The agent additionally reads Memory and Crystallized catalogues, then relevant accepted records. Start with a new accepted outcome, linking existing ADRs rather than copying them.

Open Forge must never silently replace host instructions, move existing documents, convert historical ADRs into newly accepted Decisions, rewrite another assistant’s rules, claim linked documents as managed files, or reactivate disabled roots.

Useful CLI help is a Minimal example, explicit retention explanations, and readable context reasons. Automatic document migration is unnecessary. If a fresh agent still misses the linked contribution rules after a second controlled attempt, I remove the trial instead of installing more modules.

### The introduction

The following publication copy assumes the proposed installer is shipped.

````markdown
## Open Forge

Open Forge helps coding agents find and use the rules, documents and decisions your project already has. You choose what they read before work and what they open when a task needs it.

Start with one existing document. Keep your README, contribution guide, ADRs and assistant configuration where they are. Add places for new rules or useful memory when you want them.

### Start with one document

With Node.js 22.18 or later:

```sh
npm install -g @thelithiumforge/open-forge@beta
cd /path/to/your-project
open-forge install --preset minimal --dry-run --detail standard
```

Review the plan, then apply the same selection:

```sh
open-forge install --preset minimal
```

Minimal adds an Open Forge section to AGENTS.md, a loader, a human guide, and two CLI bookkeeping files. It enables no category folders. Existing text outside the managed section stays yours. Use `--bridge both` if you also want the managed Claude bridge.

Add this line under `## Entries` in `.agents/loader.md`:

```markdown
- [Contribution rules for this project](../CONTRIBUTING.md) - #LoadNow
```

Use an existing document that matters to your project. It needs no move or new frontmatter.

Now start a fresh agent session:

> Follow AGENTS.md. Explain how this project runs its tests, and name the instructions you read.

Check that the agent used your contribution guide. Open Forge makes the reading instructions explicit. Your review still checks whether the agent followed them.

### Choose how much to use

You can stop at any of these setups.

### Existing documents only

Minimal plus the link above is enough. To limit a document to matching work:

```markdown
- [C# conventions](../docs/contributing/csharp.md) - #LoadNow - applies to `**/*.cs`
```

Leave off the loading tag for a source the agent should open only when relevant.

### Rules and accepted knowledge

When a correction keeps recurring, add Directives. When a useful accepted outcome needs a home, add Crystallized Memory. Choose either or both:

```sh
open-forge install --configure --preset custom --route directives=add --route memory/crystallized=add --dry-run
open-forge install --configure --preset custom --route directives=add --route memory/crystallized=add
```

Directives hold required behavior. Crystallized Memory holds accepted knowledge. Existing documents can remain their defining sources, with links from the new records.

### Work that survives a pause

Add Working Memory when you need resume notes:

```sh
open-forge route init memory/working --framework --dry-run
open-forge route init memory/working --framework
```

Its separate saving policy asks agents to update unfinished work before pausing. You can remove that policy while keeping your records.

### The full framework

Full Core adds advice, maps, reusable shapes, Skills, starting files and all Memory states:

```sh
open-forge install --configure --preset full-core --dry-run
open-forge install --configure --preset full-core
```

Extensions add optional methods and record types. Your existing ADR convention can remain your decision system.

### Your files, your setup

Open Forge is plain Markdown. The CLI gathers context, previews changes and maintains navigation. Without it, follow the same files and maintain the links yourself.

A manual minimal setup needs only the Open Forge block in AGENTS.md and `.agents/loader.md`. Copy the supplied loader, replace its Entries with your chosen links, and append the managed block without replacing existing instructions.

You can edit, replace or remove every part. Removing a loader entry disables that part's Open Forge behavior while keeping its files. It does not revoke accepted project requirements or instructions another tool loads independently.

CLI updates can replace direct edits to shipped files. Keep lasting adjustments in adjacent overwrite companions, and review update plans.

Read [.agents/README.md](.agents/README.md) for everyday changes, or the [CLI reference](docs/cli.md) for installation and maintenance details.
````

Complete `.agents/README.md`:

````markdown
## Open Forge in this project

[loader.md](loader.md) selects the context agents use. Its Entries can link to existing documents anywhere inside this workspace. Those documents stay where they are and need no new frontmatter.

This guide is for people. Agents follow the loader and selected sources.

### Connect an existing document

Add a line under the loader's `## Entries`:

```markdown
- [Contribution rules](../CONTRIBUTING.md) - #LoadNow
```

#LoadNow means read it whenever its parent loads, subject to file conditions. For example:

```markdown
- [C# conventions](../docs/contributing/csharp.md) - #LoadNow - applies to `**/*.cs`
```

Patterns start at the workspace root and are case-sensitive. Leave off the loading tag for relevant reading on demand. Linking a document does not accept its claims or authorize edits.

### Choose folders

Each scope contains only the routes useful there. It need not copy another scope or the installed defaults.

Directives hold required behavior. Crystallized Memory holds accepted knowledge. Working holds resume state. Emerging holds useful unsettled material. Archived holds history.

Guidance supplies advice, Patterns supply default shapes, Templates supply starting files, Maps supply pointers, and Skills supply capabilities.

Install only what helps. Memory states need their Memory parent. Read a category's catalogue before authoring or reviewing its content.

### Add a rule or record

For a new Directive, create a file under `directives/` with a description and tags in frontmatter. Put the required behavior under `## Instructions` and include LoadNow in its tags. Add applyTo when it should cover only matching files.

Records may live directly under a selected Memory state. Reuse an existing record when it answers the same question. Keep the detailed answer in its defining source.

Maintain catalogue Entries by hand, or preview and run:

```sh
open-forge index --dry-run
open-forge index
```

### Turn something off

Remove its entry from the loader. Its files remain notes. Accepted requirements and independently loaded host instructions still apply.

Other catalogues reflect files in their folders. Removing one of those lines alone is temporary. Preserve useful records before moving or removing their route.

Custom's remove choice releases Framework management and retains existing files and routes. It does not disable them.

For managed-file deletion, preview `open-forge remove <path> --dry-run`. Inspect every deletion before applying it. The exclusion prevents updates from restoring that default.

### Control automatic saving

Working Memory includes `memory/working/saving.md`. To stop its automatic saving policy while keeping records, preview:

```sh
open-forge remove .agents/memory/working/saving.md --dry-run
```

Apply the reviewed removal without --dry-run. Without the CLI, delete that policy file and remove its entry from `_working.md`. Record its exclusion before using CLI updates later.

### Change a shipped file

Use an adjacent `{name}.overwrite.md` for a lasting adjustment. It loads after its base and overrides only corresponding content in that source.

Direct edits are possible, but CLI updates may replace them. Review the update plan.

### Nested context

For example, a C# scope may contain a #LoadNow design file and an on-demand Windows scope. Selecting C# loads its design rules, not Windows. A #LoadNow file under Windows loads only after Windows is selected.

### Terms

A catalogue is the navigation file for a folder, normally `_{folder-name}.md`. The CLI also calls it an entrypoint.

A route is a navigable path exposed through catalogues and Entries.

A root route is exposed directly by the loader.

A scope narrows where the content after it applies.

A slug is a concrete folder name in a route path.

#Core labels routing, loading, workspace orientation and reusable content roles.

#Memory labels self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history.

#Extension labels optional packaged routes, capabilities, integrations and support files.

#Contextual marks material that is not accepted merely by being recorded. #CurrentTruth describes accepted current state within its scope. #Evergreen creates a duty to keep material current, without granting authority or loading it.

Only #LoadNow and #KeepInMind cause automatic reading. #KeepInMind also requires refreshes at the points defined in the loader.

### With or without the CLI

`open-forge context --for <file>` gathers tagged context for known working files. Agents still select relevant sources and follow explicit reading instructions.

Everything also works through ordinary file reads and edits. Settings and the lock record support CLI maintenance. They do not establish project meaning.
````

### v3

This is the complete Full Core loader. Minimal renders the same rules with empty Entries. Selecting fewer modules reduces reading without weakening the modules selected.

```markdown
## Open Forge Loader

This file tells agents which context this workspace uses and how to read it. It adds to the project's existing instructions. Everything works with plain files.

The `Entries` at the end select what Open Forge turns on. A folder on disk is not enough. An empty list is valid.

### Axioms

### Read before you work

1. At task start, resume, or context restoration, identify your working files. Include files you will investigate, review, create, change, rename or delete, including planned files. For a rename, include both paths. Reading a file only for background does not make it a working file.
2. If the CLI is available and allowed, run `open-forge context`, repeating `--for <file>` for every known working file. Read its output. It gathers the tagged reads in steps 3 and 4. You must still do step 5. If the command fails, is unavailable, or cannot follow a link, follow these steps with your file tools.
3. Read applicable entries tagged #LoadNow or #KeepInMind in listed order. When an entry opens a catalogue, repeat this step inside it. A catalogue is a folder's navigation file. Read each source once per pass. Do not skip a required read because its description looks unrelated.
4. Check conditions before loading or applying an entry. Patterns in one `applies to` list are alternatives. The same working file must match that entry and every conditioned catalogue above it. Globs are case-sensitive and start at the workspace root, such as `**/*.cs` or `docs/**`. Conditions filter reading and application, not edit permission. While paths are unknown, report tagged conditioned entries as pending and resolve them before matching work.
5. Open other entries when their descriptions fit the task, including the category you will author or review. Follow explicit required-read links too. Before applying a search result, read its listed ancestor catalogues. Keep separately selected branches separate. Use catalogues for navigation. Do not open ordinary file bodies just to discover routes.

Restart at step 1 when the goal or working files change. Reread #KeepInMind entries before handoff or closeout. Refresh only active scopes. Do not open file bodies just to discover tagged files. During work, recheck #KeepInMind files only when they may have changed.

### Use what you read

- Follow applicable rules. Rule files put them under `## Instructions`. Only this loader and loaded catalogues define active `## Axioms`, inherited by their selected descendants. Narrower rules add to broader rules and do not cancel them. Report unresolved conflicts instead of choosing by depth or reading order.
- Follow clear user direction within platform and safety limits. Take routine, reversible steps within the request without asking again. State assumptions that affect the result. Ask before unresolved choices that significantly change the result, scope, risk, cost, external effects or ability to undo the work.
- Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.
- Use the fewest words that preserve meaning, readability, and logical progression in communication and authored content. Remove filler and repetition. Keep the context, conditions, and exceptions needed to understand and apply the result.
- Selected advice and shapes may specialize broader material of the same kind within their accepted scope.

### Keep sources reliable

- Use declared external authorities for the facts assigned to them. Accepted documents define their subjects. Reading, linking, saving, moving or tagging content does not establish acceptance or grant edit permission. Acceptance comes from the user or an authority declared for that question. Evidence supporting a claim does not itself establish acceptance.
- Give each source one clear question. Keep detail in its narrowest defining source and link instead of repeating it. Links do not change authority, scope, loading or lifecycle.
- Use relative local links, precise references and consistent subject tags when they improve navigation or retrieval.
- Investigate an apparent conflict between accepted sources before changing either one. When accepted direction changes current state, update its defining source and preserve useful prior context.
- #Evergreen marks material that must stay aligned with accepted current state. It grants no authority and does not cause loading. Update affected #Evergreen sources you may edit before work depends on them and no later than closeout. Batch related updates when safe. Keep each source aligned with what it represents now. Preserve useful prior context where the project keeps history. Report needed updates that are blocked or unauthorized.

### Change what is on

- Add an entry to turn on a file or catalogue. Link anywhere inside the workspace, such as `../docs/architecture.md`. Existing documents need no move or frontmatter. Their entry carries the description, tags and conditions.
- Remove a loader entry to turn that part off. Its files remain notes, and finding them through search does not reactivate its Open Forge rules. This does not cancel accepted project requirements or instructions your host loads independently. Report dependencies before disabling a part. Unlisted parts create no Open Forge duties and must not return unless requested.
- Other catalogues describe their folders' files. Deleting a line there alone does not disable a file. Move or remove the file too. For CLI-managed defaults, record the exclusion so updates respect it. Read the CLI Skill before commands other than `context`, and follow its removal instructions.
- Report missing listed files. Read an adjacent `{name}.overwrite.md` immediately after its base. It shares that base's scope and loading behavior and wins only for corresponding content. Do not list or select it separately. Use a companion for local changes that must survive CLI updates.
- Tag an entry #LoadNow when it must be read whenever its parent loads. Use #KeepInMind when it also needs the refreshes above. Neither tag opens an unselected parent. Keep optional material on demand. Narrow specialized content before making it required. Other tags classify content or support search and do not load files.
- Put new Open Forge Markdown metadata in frontmatter with `description`, `tags` and optional `applyTo` and `responsibility`. Follow `frontmatter` in `.agents/open-forge.json`, with scoped fields when absent. Native Skills keep native metadata. A folder has one catalogue, named `_{folder-name}.md`. Compatibility names are `index.md`, `_index.md`, `references.md` and `_references.md`.
- Catalogued folders may nest anywhere below a listed root. Each folder narrows everything after it. Put specialized material in the narrowest useful scope. Reserve workspace-wide placement for material that applies everywhere.
- For a new catalogue without local rules, put only `- inherited - No local axioms. Loaded ancestor axioms remain active.` under `## Axioms`. Existing empty or missing sections add no rules. Never mix this marker with local rules or repeat inherited rules.

### Entries

- [Rules agents must follow in this workspace](directives/_directives.md) - #LoadNow #Core #Directive
- [Advice for recurring choices](guidance/_guidance.md) - #LoadNow #Core #Guidance
- [Where to find project knowledge](maps/_maps.md) - #LoadNow #Core #Map
- [Records for continuity, accepted knowledge, ideas and history](memory/_memory.md) - #LoadNow #Memory
- [Default shapes for creating or reviewing structure](patterns/_patterns.md) - #LoadNow #Core #Pattern
- [Capabilities for specialized work](skills/_skills.md) - #LoadNow #Core #Skill
- [Starting files to copy when useful](templates/_templates.md) - #Core #Template
```

Complete managed `AGENTS.md` block:

```markdown
## Open Forge

Before starting or resuming a task, read `.agents/loader.md` and follow its reading steps and applicable rules. It adds the context selected for this workspace to the project's existing instructions. The loader explains the optional `open-forge context` shortcut and what you must still read yourself.

**End of Open Forge managed section.**
```

| Before in v2 | After in v3 | Why |
| --- | --- | --- |
| “This file lists the context this workspace has chosen to use.” | “This file tells agents which context this workspace uses and how to read it.” | States both selection and procedure |
| “The `Entries` below are the switch: what is listed is on, and anything not listed is off.” | “The `Entries` at the end select what Open Forge turns on.” | Introduces the action directly |
| “That covers steps 3 and 4.” | “It gathers the tagged reads in steps 3 and 4.” | Names the CLI’s actual boundary |
| “Read and apply an entry with `applies to` patterns only when the same working file matches its patterns and those of every conditioned catalogue above it.” | “The same working file must match that entry and every conditioned catalogue above it.” | Separates condition checking from loading |
| “Lines inside other catalogues follow the files in their folder, so to turn one of those off, delete or move its file.” | “Other catalogues describe their folders’ files. Deleting a line there alone does not disable a file.” | Makes the surprising limitation explicit |

Whitespace-delimited counts, including headings and Entries: **loader 1,185 words**, **managed block 53 words**. The loader exceeds v2’s reported 823 words because it restores common duties. Restored module loading also increases Full Core startup reading. Minimal avoids those modules altogether.

### Risks and proof

**R1, medium, activation:** [GeneratedNavigationRegionPlanner.cs:148](/src/cli/framework/OpenForge.Cli.Framework/Framework/GeneratedNavigation/GeneratedNavigationRegionPlanner.cs:148) returns structural roots when the listed destination count is zero. An intentionally empty loader can therefore regain disabled roots. Preserve the empty set when membership preservation is requested, and let installation explicitly request initial population. This must be fixed before advertising Minimal.

Other risks are a longer loader, larger startup context for selected modules, conflicting existing host rules, and confusion between disabling, releasing management and deleting. The introduction must state those distinctions without promising model compliance.

The cheapest reader experiment uses three unfamiliar maintainers and one existing repository. Give them only the introduction. One works without the CLI. Ask each to connect a guide, verify its use in a fresh session, add only Crystallized, and undo the trial. Record time, unexpected writes and explanations they needed. Success means useful first reading within ten minutes, no document moves, and correct explanation of what remains active after removal.

Then compare CLI and manual reading sets for matching, nonmatching and unknown paths. Verify empty Entries, repeat installation, bridge preservation and byte preservation outside managed blocks.

This is a read-only proposal. No files changed, and no Doctor, builds or test suites ran.

### The one idea to steal

Let people connect one useful document before asking them to adopt a place to put documents.

---

## Impatient Evaluator (Proposal J)

/root - GPT-6 - high

### 1. Thesis

Keep v2’s reading procedure and direct links, then restore the obligations that compression removed. Introduce Open Forge through one existing document, with optional folders explained only when someone needs them. A smaller installation should mean fewer enabled responsibilities, with every enabled responsibility preserved.

### 2. How v2 removed so many rules

The [ledger](artifacts/council-3/ledger.md) shows six different operations.

| Kind | Two examples | Verdict |
| --- | --- | --- |
| Removing vocabulary | Loader L46 defines `route`, L49 defines `slug` | Sound when the instructions work without those terms |
| Combining repeated boundaries | Loader L31 distinguishes validation from acceptance, Crystallized L17 rejects acceptance through tags or confidence | Sound when the surviving acceptance rule covers both |
| Moving rules to their point of use | Working L18 moves saving into `saving.md`, loader L130–138 moves command details into the CLI Skill | Sound when the relevant user or agent necessarily reaches the destination |
| Replacing startup reads with task triggers | Guidance L143 and Skills L147 lose #LoadNow | Defensible for task-dependent advice and capabilities, provided the loader explicitly requires checking them when relevant |
| Dropping communication defaults | Loader L21’s response shape and L22’s concision requirement | A product choice, but not a lossless rewrite of previously binding defaults |
| Treating specific obligations as obvious | Patterns L19’s acceptance gate and loader L101’s update deadline | Unsound. “A capable model probably does this” does not preserve an obligation |

The reduction was possible because the original repeated definitions, boundaries and navigation instructions. Its size does not establish that every deletion was redundant.

### 3. The lossless ledger

Paths below are relative to the shipped `.agents/` directory. Old lines refer to the ledger. Quoted text is exact proposed text. Additions belong under `## Axioms` unless another section is named. Section 8 supplies the complete replacement loader.

Authoring rules relocated to `README.md` remain binding because the loader requires reading its Instructions before authoring context or navigation. This conditional reach matches when those rules are needed.

**Destination: `loader.md`, Use what you read**

- `loader.md L21` — **Restore**: “Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.”
- `loader.md L22` — **Restore**: “Use the fewest words that preserve meaning, readability and logical progression. Remove filler and repetition while keeping necessary context, conditions and exceptions.”
- `loader.md L27` — **Restore**: “Let each source answer one clear question. Link to related sources instead of repeating their detail. A link changes neither source’s authority, scope, loading rules or lifecycle.”
- `loader.md L32` — **Restore**: “Investigate conflicting accepted sources before changing either. When accepted direction changes current state, update its defining source and preserve useful prior context.”
- `loader.md L40` — **Restore**: “Selected non-binding material may specialize broader material of the same kind within its accepted scope.”
- `loader.md L100` — **Restore**: “Keep #Evergreen material aligned with accepted current state. This tag grants neither authority nor automatic reading.”
- `loader.md L101` — **Restore**: “Update affected #Evergreen sources you may edit before work depends on them and by closeout. Batch related updates when safe.”
- `loader.md L102` — **Restore**: “Keep each source aligned with what it represents now. Preserve useful prior context where the workspace keeps history. Report needed updates that are blocked or not authorized.”

**Destination: `loader.md`, Read before you work**

- `loader.md L62` — **Restore**, step 3: “Use catalogue Entries for navigation, rather than opening ordinary file bodies to discover branches.”
- `loader.md L85` — **Redundant**. The example adds no behavior beyond the surviving rule: “Read every entry below tagged #LoadNow or #KeepInMind, in listed order, then do the same inside each catalogue you read.”
- `loader.md L93` — **Restore**, refresh paragraph: “Refresh only active branches. Do not open file bodies just to discover tagged files.”
- `loader.md L94` — **Restore**, refresh paragraph: “During work, reread active #KeepInMind entries only when their contents may have changed.”

**Destination: `README.md`, Instructions**

- `loader.md L28` — **Relocate**: “Connect related material with relative links to local sources and precise references. Reuse consistent descriptive tags for shared subjects so tag and reference searches can find those connections. Add links and tags only when they help navigation or retrieval.”
- `loader.md L37` — **Relocate**: “When authoring a catalogue with no local rules, use `## Axioms` with one entry: `- inherited - No local axioms; loaded ancestor axioms remain active.` Existing missing or empty local sections also add no rules. Never combine the inherited marker with local rules.”
- `loader.md L57` — **Relocate**: “Any number of folders may narrow a branch before, between or after its deeper segments. Each narrows everything that follows it.”
- `loader.md L58` — **Relocate**: “Include only the branches useful in a scope. It need not copy another scope or the installed defaults.”

**Destination: `maps/_maps.md`**

- `_maps.md L23` — **Restore**: “Keep Maps broad. Prefer modules, projects, repositories, systems or scopes over members and functions unless finer routing clearly helps.”
- `_maps.md L24` — **Restore**: “Each workspace chooses its own Map filenames, grouping and nesting depth.”
- `_maps.md L25` — **Restore**: “Maps do not replace Memory or a destination’s current information.”

**Destination: `patterns/_patterns.md`**

- `_patterns.md L19` — **Restore**: “Create or update a Pattern only when an accepted shape should guide future related work. Structure alone does not justify promoting one-off work, temporary transitions or unsettled candidates into a Pattern.”
- `_patterns.md L25` — **Restore**: “Link related sources for required rules, advice, maps, procedures and starting files, and say which of them bind the shape. Sources that apply the shape should link back to its Pattern.”
- `_patterns.md L26` — **Restore**: “Keep required behavior, procedures and substantial choice rationale in their defining sources. Example names and values illustrate the shape unless an applicable requirement fixes them.”
- `_patterns.md L28` — **Restore**: “A justified adaptation is still a departure, so a review reports it separately from conformance.”

**Destination: `templates/_templates.md`**

- `_templates.md L24` — **Restore**: “If the new artifact needs continuing guidance or requirements, link the source that defines them. The Template provides only starting content.”
- `_templates.md L28` — **Restore**: “State the need and primary question each Template answers in its description and source instructions.”
- `_templates.md L29` — **Restore**: “Use focused prompts that request necessary content once, in a readable order. Mark optional sections clearly.”
- `_templates.md L30` — **Restore**: “Treat generic Templates as fallbacks. Add a specialized Template only when it provides meaningfully different starting content.”

**Destination: `memory/_memory.md`**

- `_memory.md L13` — **Restore**, introduction: “Memory is self-growing. People and agents can add useful records and routed scopes without a fixed structural limit. Unselected branches do not load their file contents. Records need not pass through every state.”
- `_memory.md L19` — **Restore**: “An accepted record can define current knowledge or decisions. Recording another category’s content does not give the record that category’s role.”
- `_memory.md L26` — **Restore**: “Distinguish what was stated, observed or inferred.”
- `_memory.md L27` — **Restore**: “Update affected sources before other work depends on the outcome and no later than closeout. Keep useful context and reasoning in Memory. Accepted durable knowledge must reach the source that defines it, not remain only in chat or a #Contextual record.”
- `_memory.md L32` — **Restore**: “Add records and routed scopes when their likely future value justifies the cost of finding and reviewing them.”
- `_memory.md L35` — **Restore**: “Named record categories and Templates are optional. Use an existing state and the narrowest useful scope. A record may live directly under that state.”
- `_memory.md L36` — **Restore**: “Within the authority granted for the work, add an ordinary record or scoped route when needed. Ask when its placement would change accepted meaning or cross an unresolved authority boundary.”

**Destination: `memory/working/_working.md`**

- `_working.md L19` — **Restore**: “An explicitly accepted temporary choice may remain here when its source, scope and expected expiration are clear.”

**Destination: `memory/emerging/_emerging.md`**

- `_emerging.md L17` — **Restore**: “At each applicable #KeepInMind refresh, check visible Entries and save useful candidate material before it is lost.”
- `_emerging.md L23` — **Restore**: “Refine material while its validity, use or final destination remains unsettled.”
- `_emerging.md L25` — **Restore**: “Revisit repeated or stale material and decide whether to consolidate, promote, archive or prune it. Review may leave a record unchanged.”

**Destination: `memory/archived/_archived.md`**

- `_archived.md L17` — **Restore**: “Retain, consolidate or transform history according to its future value. Archival need not preserve every detail.”

**No destination, retired glossary entries**

- `loader.md L46` — **Accept loss**. Removing the definition of `route` removes no behavior. CLI references still explain their terminology.
- `loader.md L47` — **Accept loss**. “Root route” becomes “loader entry” in introductory instructions.
- `loader.md L48` — **Accept loss**. Narrowing remains explicit without requiring a separate glossary definition of `scope`.
- `loader.md L49` — **Accept loss**. Readers can understand “folder name” without learning `slug`.
- `loader.md L95` — **Accept loss**. #Core remains a descriptive label with no additional loading behavior.
- `loader.md L96` — **Accept loss**. #Memory remains a label. Memory’s meaning and deliberate growth return to its own introduction.
- `loader.md L97` — **Accept loss**. #Extension remains a label. Package behavior remains documented where packages are installed and used.

Several losses also hide behind other ledger statuses. Restore #KeepInMind to Emerging’s frontmatter and its parent entry, otherwise its restored refresh obligation is unreachable by default. Also restore these exact clauses:

- Memory Axioms: “Get explicit user direction before creating a new top-level Memory state.”
- Working Axioms: “Check these Entries before deciding that no Working Memory applies.”
- Templates Axioms: “Preserve the destination’s required structure. Remove package tags that do not describe the destination.”
- Archived Axioms: “Remove metadata that would keep a former behavioral role active or claim current authority.”
- Skills Axioms: “Follow the selected SKILL.md and its resource-loading instructions.”

### 4. The complexity ladder

These are useful stopping places, not mandatory stages.

Let **B** mean `AGENTS.md`, `CLAUDE.md`, `.agents/loader.md` and `.agents/README.md`. CLI installation also writes `.agents/open-forge.json` and `.agents/open-forge.lock.json`.

| Level | Installed tree | Benefit and cost | Moving |
| --- | --- | --- | --- |
| 1. Connect existing documents | B, with one or more loader links | Existing rules become discoverable across sessions. Cost is maintaining a short reading list | Up: add Directives or Crystallized. Down: remove the link, leaving a valid empty list |
| 2. Keep rules and accepted facts | B plus `directives/_directives.md`, `memory/_memory.md`, `memory/crystallized/_crystallized.md` | A place for recurring corrections and accepted knowledge. Cost is reviewing and maintaining what gets added | Up: choose Essentials. Down: disable the added root links and retain the notes |
| 3. Continue across sessions | Essentials, adding Patterns, Skills with the CLI Skill and four references, Working with `saving.md`, and Emerging | Resume points and candidate capture, plus reusable shapes and capabilities. Cost is maintaining useful records and accepting the enabled saving policies | Up: add omitted categories individually. Down: disable the extra roots and move unwanted Memory states outside their enabled parent |
| 4. Full Core | Level 3 plus `guidance/_guidance.md`, `maps/_maps.md`, `templates/_templates.md`, `memory/archived/_archived.md` | Every built-in content role. Cost is more visible choices and their maintenance obligations | Down: disable the additional roots and move Archived outside Memory |

For managed child removals, record exclusions before updating. Custom’s `remove` action alone does not disable existing content.

Brownfield projects should start at level 1. It tests whether agents use an existing trusted source before asking the team to create another information structure. Level 2 can contain only Directives, only Crystallized Memory, or both.

Extensions add selected methods and record shapes wherever their required routes exist. Full Core does not imply installing every Extension.

### 5. Ultra-lightweight install

Add **`--preset minimal`**. This is proposed syntax, not an option in the inspected CLI.

On a fresh repository it writes B and the two CLI state files, with an empty loader Entries section. It installs none of the ten optional route rows, performs no adoption inside omitted routes, and creates no ignore rules.

Allow existing row syntax with Minimal:

`open-forge install --preset minimal --route directives=add --route memory/crystallized=add`

Unspecified rows remain omitted. Necessary ancestors are included. `git-ignore` retains its existing meaning. Keep Essentials, Full Core and Custom unchanged. Minimal becomes the first interactive recommendation, while unqualified unattended installation retains Full Core for compatibility.

The plan should say: “Add the reading list and setup guide, and add or refresh the marked sections in AGENTS.md and CLAUDE.md. No optional folders or automatic progress-saving policy will be installed.” Standard detail lists every path.

Grow through existing `route init <category> --framework` or `install --configure --preset custom`. Configuration must continue to distinguish releasing management from disabling behavior.

C# changes, under the existing Install owners:

- Operations: `Models/Configuration/InstallConfiguration.cs`, `Shared/Configuration/InstallConfigurationChoices.cs`, `Shared/Configuration/InstallSetupResolver.cs`, `Shared/Binding/InstallBindingInputReader.cs`
- Rendering: `Presentation/Install/Shared/Prompts/InstallSetupQuestions.cs`
- Output text: `Install/InstallSetupText.cs`, `Install/InstallInputText.cs`, and affected root help in `Shared/SharedText.cs`

Cost is a small preset implementation with medium verification scope: selection tests, first-install and configure journeys, help and output snapshots, and published argument coverage. No Markdown parser, settings-schema migration or persisted mode identity is needed. Task 73 already establishes concrete route choices as the persisted model.

### 6. Brownfield adoption

The following walkthrough assumes the existing `docs/` contains `architecture.md`. Otherwise select an actual document from `rg --files docs`.

1. Inspect `git status --short` and `git diff`. Save existing changes and untracked material before an installation trial. Inspect the existing agent instructions and run `rg --files docs`. The agent still reads only the repository’s existing setup.
2. Preview `open-forge install --preset minimal --dry-run --detail standard`. Review the named paths, then run `open-forge install --preset minimal`. Existing host text stays outside the managed blocks. The next task reads its host instructions and the loader, with no optional branches.
3. Add these exact loader entries:  
   `- [Contribution rules](../CONTRIBUTING.md) - #LoadNow`  
   `- [Project overview](../README.md)`  
   `- [Architecture to consult before structural changes](../docs/architecture.md)`  
   Tell the agent which documents define accepted rules and architecture. Linking alone does not establish acceptance.
4. Run `open-forge context --for src/example.cs`, using a real working path. It should include CONTRIBUTING. A task involving structure must also select Architecture through step 5. Start a fresh agent session and request a small explanation, asking it to name the sources read.
5. When a recurring correction appears, preview and run `open-forge route init directives --framework --dry-run` and `open-forge route init directives --framework`. Add a rule with metadata, `## Instructions` and #LoadNow, then maintain its catalogue entry. The agent now reads that rule through the required Directives chain.
6. When accepted knowledge needs a new home, preview and run `open-forge route init memory/crystallized --framework --dry-run` and `open-forge route init memory/crystallized --framework`. Record only the new accepted result. Existing architecture and ADRs stay where they are. The agent reads the Memory and Crystallized catalogues, then relevant records.
7. To return to level 1, remove the Directives and Memory loader entries. Keep their files. Record management exclusions before updating. Independent accepted requirements remain in effect.

Open Forge must never silently relocate or rewrite the repository’s documents, infer acceptance from existing code, replace unrelated host instructions, reconstruct historical decisions as newly accepted choices, or enable omitted siblings.

The useful CLI help is a concrete preview and a suggested first Context command. Automatic document importing is unnecessary.

### 7. The introduction

I would stop at these points:

- Root README: “Keep Root keys unless your workspace already declares another metadata form.” I am being asked about representation before seeing one useful project link.
- Docs intro: the same Root keys instruction interrupts the trial. Later, “Three parts, one of them required” presents the Framework as a package commitment.
- v2 `.agents/README.md`: “tag the file `#LoadNow`” in Add a rule. It needs a copyable example showing where that tag goes.
- v2 loader: “then do the same inside each catalogue you read.” “Catalogue” has not yet been explained.

The following is proposed release text, assuming Minimal ships.

````markdown
## Open Forge

Stop explaining the same project rules to every new coding-agent session.

Open Forge gives your agent a reading list for the rules, docs and decisions your project already has. Start with one link. Add places for new rules or useful notes when you need them.

It adds a loader and setup guide under `.agents/`, plus marked sections in `AGENTS.md` and `CLAUDE.md`. Your existing documentation stays where it is. Everything is plain Markdown, and the CLI is optional.

### Start with an existing document

With Node.js 22.18 or later:

```sh
npm install -g @thelithiumforge/open-forge@beta
cd /path/to/your-project
open-forge install --preset minimal --dry-run
open-forge install --preset minimal
```

Review the preview before applying it. Inspect new files with `git status` and read changes with `git diff`.

Minimal installs no optional folders. Open `.agents/loader.md` and add this under `## Entries`:

```markdown
- [Contribution rules](../CONTRIBUTING.md) - #LoadNow
```

Use a document your project already has. Tell your agent that it defines the project's contribution rules, then try a normal task in a fresh session:

> Read AGENTS.md, explain how this project runs its tests, and name the instruction files you used.

`#LoadNow` requires reading the document whenever its parent list loads. Without the tag, the agent opens it when its description fits the task.

A reading link does not approve everything in a document or grant permission to edit it.

### Add rules when corrections repeat

You can keep rules in existing documents. If you want separate rule files, add Directives, the optional folder for required agent behavior:

```sh
open-forge route init directives --framework --dry-run
open-forge route init directives --framework
```

In a fresh Minimal CLI installation, create `.agents/directives/checks.md`:

```markdown
---
description: Check code changes before reporting completion
tags: [LoadNow, Directive]
---

## Check code changes

### Instructions

- For code changes, run the checks described in CONTRIBUTING.md before reporting completion.
```

Add this line to `.agents/directives/_directives.md` under `## Entries`, replacing its empty placeholder:

```markdown
- [Check code changes before reporting completion](checks.md) - #LoadNow #Directive
```

Use your workspace's chosen metadata form if it differs from this example. The CLI can maintain the list with `open-forge index`.

### Keep accepted knowledge when it needs a home

If useful accepted knowledge has nowhere suitable to live, add Crystallized Memory, the optional folder for knowledge that should stay current:

```sh
open-forge route init memory/crystallized --framework --dry-run
open-forge route init memory/crystallized --framework
```

Keep a short accepted result there and link to its evidence or reasoning. Existing architecture documents and ADRs can stay in `docs/`. Findings you have not accepted remain candidates.

You can use this folder without installing Directives or the other Memory folders.

### Choose more only when it helps

Essentials adds rules, reusable shapes, capabilities and project memory, including a policy for saving unfinished work. Full Core adds every built-in category. Custom lets you choose individual parts.

For example, a fresh setup containing only rules and accepted knowledge is:

```sh
open-forge install --preset minimal --route directives=add --route memory/crystallized=add --dry-run
```

Review it, then repeat without `--dry-run`.

On an existing installation, use `open-forge install --configure` or add one category with `route init`. A preset selects files. It does not lock your project into a mode.

Remove a loader entry to disable that branch while keeping its files. Your independently accepted requirements and host instructions still apply. The setup guide explains removals, updates and local customizations.

### Without the CLI

Copy `loader.md` and `README.md` from `src/open-forge/.agents/` into your project's `.agents/` folder. Append the supplied Open Forge block to your existing `AGENTS.md`, preserving its text. Add the supplied CLAUDE bridge if your agent uses it.

Keep only the loader entries you want. You can link existing documents and maintain every list with your normal editor.

Your agent tool must read `AGENTS.md` or the CLAUDE bridge. Open Forge supplies instructions and context, while you continue to review the agent's work.
````

Complete `.agents/README.md`:

````markdown
## Open Forge in this project

This folder gives agents a reading list and optional places for project context. Start at [`loader.md`](loader.md). Each line under its `Entries` heading enables a file or a branch.

You can keep using your existing documents. You can also stop at any useful setup.

### Link an existing document

Add a line under the loader's `## Entries`:

```markdown
- [Contribution rules](../CONTRIBUTING.md) - #LoadNow
- [Architecture for structural changes](../docs/architecture.md)
```

Paths are relative to the file containing the link. Use documents that exist in your project.

#LoadNow requires reading whenever the parent list loads. Without a loading tag, an entry opens when it fits the task.

For rules that apply only to particular files:

```markdown
- [C# conventions](../docs/csharp.md) - #LoadNow - applies to `**/*.cs`
```

Patterns match paths from the workspace root and are case-sensitive. A match filters an enabled entry. It does not open an unselected branch.

Tell the agent which source defines accepted requirements. A link schedules reading without establishing acceptance or edit permission.

### Add only the folders you need

Directives hold required behavior. Put rules under `## Instructions`, with #LoadNow in the file's metadata and catalogue entry.

Crystallized Memory holds accepted knowledge that should stay current. Working Memory holds temporary resume notes. Emerging Memory holds candidates. Archived Memory holds useful history.

These folders are optional. Memory can grow through useful records and narrower folders without loading every record into every task. Existing docs and ADRs need no replacement.

With the CLI, preview `open-forge route init directives --framework --dry-run`, then repeat without `--dry-run`. Substitute `memory/crystallized` for accepted notes.

### Turn a part off

Remove its loader entry to disable the whole branch and keep its files. Background reading does not reactivate its rules. Accepted project requirements and independently loaded host instructions remain in effect.

Inside other catalogues, Entries track files in their folders. Move a child outside that enabled folder, or delete it, and remove its entry. Report enabled dependents before disabling their dependency.

For CLI-managed removals, record the corresponding exclusion before updating. Command help explains supported removal operations. Custom's `remove` choice releases management while retaining existing content and routability.

To stop automatic progress saving while retaining Working records, delete `memory/working/saving.md`, remove its entry, and record its managed removal. Emerging candidate capture is a separate enabled policy.

### Update and customize

Ordinary CLI Update can replace edited managed files. Put a lasting local adjustment in an adjacent `{name}.overwrite.md`. It loads after its base and wins only for corresponding content in that source.

Maintain Entries by hand, or preview and run `open-forge index`. The CLI is optional.

### Instructions

Follow these rules when authoring Open Forge context or navigation:

- Connect related material with relative links to local sources and precise references. Reuse consistent descriptive tags for shared subjects so tag and reference searches can find those connections. Add links and tags only when they help navigation or retrieval.
- When authoring a catalogue with no local rules, use `## Axioms` with one entry: `- inherited - No local axioms; loaded ancestor axioms remain active.` Existing missing or empty local sections also add no rules. Never combine the inherited marker with local rules.
- Any number of folders may narrow a branch before, between or after its deeper segments. Each narrows everything that follows it.
- Include only the branches useful in a scope. It need not copy another scope or the installed defaults.

For new metadata, follow `frontmatter` in `.agents/open-forge.json`. Its values are `root` and `scoped`. Without a setting, use scoped fields under `open-forge:`. Native Skills retain their own metadata. Documents linked from elsewhere in the workspace need no new frontmatter.
````

### 8. v3

Complete Full Core loader below. Minimal uses the same text with an empty Entries section.

```markdown
## Open Forge Loader

This file tells agents which project context to read. It works with ordinary file tools and adds to the project's other instructions. Only the branches listed under `Entries` are enabled.

### Axioms

### Read before you work

1. At task start, resumption or context restoration, list the files you will investigate, review, create, change, rename or delete. Include planned files. These are your working files. Reading a file only for background does not make it a working file.
2. When available and allowed, run `open-forge context`, repeating `--for <file>` for every known working file, and read its output. This performs steps 3 and 4. You must still perform step 5. If the command fails, is unavailable or cannot follow a required link, use file tools for steps 3 to 5.
3. Read every applicable #LoadNow or #KeepInMind entry in listed order. If it opens a catalogue, meaning a folder's list of files, repeat this step there. Never skip a required read because its description seems unrelated. Read each file once per pass. Use catalogue Entries for navigation, rather than opening ordinary file bodies to discover branches.
4. An entry marked `applies to` is applicable only when one working file matches its patterns and every ancestor's patterns. Use the same working file for the entire chain. Patterns are case-sensitive globs from the workspace root, such as `**/*.cs` or `docs/**`. Unknown working files leave required conditions pending. Resolve them before work reaches a matching file. A condition never selects a branch by itself.
5. Read other entries whose descriptions fit the task, and follow required-read links in everything you read. Check relevant advice before choosing an approach, source pointers before relying on knowledge, shapes before creating or reviewing structure, and capabilities before specialized work. For a search result or direct link, read its enabled catalogue chain before applying it. Keep separate chains separate. Unlisted branches remain background only.

Restart at step 1 when the goal or working files change. Before handoff or completion, reread active #KeepInMind entries and their required descendants. Refresh only active branches. Do not open file bodies just to discover tagged files. During work, reread active #KeepInMind entries only when their contents may have changed.

### Use what you read

- Follow `## Axioms` in this loader and loaded catalogues, and `## Instructions` in loaded rule files, within their stated scope. Catalogue rules cover their listed descendants. Narrower rules add to broader rules. Report conflicts and instructions you cannot follow, with the reason. Selected non-binding material may specialize broader material of the same kind within its accepted scope.
- Follow user direction within platform and safety limits. Take routine, reversible steps within the request without asking again. State assumptions that affect the result. Ask before unresolved choices materially change the result, scope, risk, cost, external effects or ability to undo the work.
- Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice. Use the fewest words that preserve meaning, readability and logical progression. Remove filler and repetition while keeping necessary context, conditions and exceptions.
- Reading, linking, saving or tagging does not establish acceptance or permission to edit. Accepted means the user agreed, or an authority the user declared establishes it for that subject. Let each source answer one clear question. Link to related sources instead of repeating their detail. A link changes neither source's authority, scope, loading rules or lifecycle.
- Put detail in the narrowest source that defines the question. Higher-level lists and records summarize and link to it. Investigate conflicting accepted sources before changing either. When accepted direction changes current state, update its defining source and preserve useful prior context.
- Keep #Evergreen material aligned with accepted current state. This tag grants neither authority nor automatic reading. Update affected #Evergreen sources you may edit before work depends on them and by closeout. Batch related updates when safe. Keep each source aligned with what it represents now. Preserve useful prior context where the workspace keeps history. Report needed updates that are blocked or not authorized.

### Change the setup

- Add an entry to enable a file or catalogue. Links may point anywhere inside the workspace, such as `../docs/architecture.md`. Existing documents need no move or frontmatter. The entry supplies their description, tags and conditions.
- Remove a loader entry to disable that branch while keeping its files. Background reading does not reactivate its rules. This does not revoke accepted project requirements or instructions loaded independently by the host. Report enabled dependents first. Unlisted branches create no Open Forge duties and must stay disabled until the user requests restoration.
- Other catalogues track files in their folders. To disable one of their children, move or delete its file and remove its entry. Record managed removals in the CLI's exclusions before updating. When present, the CLI Skill explains the procedure. Otherwise use command help or maintain exclusions by hand.
- Report a listed file that is missing. An empty `Entries` section is valid.
- #LoadNow requires reading whenever its parent loads. #KeepInMind also requires the refreshes above. Required descendants need an unbroken chain of #LoadNow parents. Neither tag opens an unselected branch. Other tags support description and search, with the #Evergreen duty defined above.
- Before authoring Open Forge context or navigation, read and follow [the authoring instructions](README.md#instructions). New `.agents/` files use `description`, `tags` and optional `applyTo` frontmatter in the workspace's chosen form. Native Skills keep their own metadata.
- Give each routed folder one catalogue named `_{folder-name}.md`. Compatibility names are `index.md`, `_index.md`, `references.md` and `_references.md`. Maintain Entries by hand or with `open-forge index`.
- For a local change that survives updates, place `{name}.overwrite.md` beside `{name}.md`. Read it immediately after the base. It shares the base's role, scope and loading behavior, is not indexed separately, and wins only for corresponding content in that source.

### Entries

- [Rules agents must follow in this workspace](directives/_directives.md) - #LoadNow #Core #Directive
- [Advice to check before choosing an approach](guidance/_guidance.md) - #Core #Guidance
- [Pointers to check before relying on project knowledge](maps/_maps.md) - #Core #Map
- [Notes for accepted knowledge, unfinished work, ideas and history](memory/_memory.md) - #LoadNow #Memory
- [Default shapes to check before creating or reviewing structure](patterns/_patterns.md) - #Core #Pattern
- [Capabilities to check before specialized work](skills/_skills.md) - #Core #Skill
- [Starting files to check when copying would help](templates/_templates.md) - #Core #Template
```

Complete managed `AGENTS.md` block:

```markdown
## Open Forge

Before working on a task, read `.agents/loader.md` and follow its reading steps and applicable rules. With the optional CLI, `open-forge context --for <file>` gathers the tagged required reads. Repeat `--for` for each working file. The loader explains the remaining reads and the fallback to file tools.

**End of Open Forge managed section.**
```

| Before | After | Why |
| --- | --- | --- |
| “The Entries below are the switch: what is listed is on, and anything not listed is off.” | “Only the branches listed under Entries are enabled.” | States the boundary directly |
| “then do the same inside each catalogue you read” | “If it opens a catalogue, meaning a folder’s list of files, repeat this step there.” | Defines the unfamiliar word immediately |
| “Read and apply an entry with applies to patterns only when the same working file matches its patterns and those of every conditioned catalogue above it.” | “An entry marked applies to is applicable only when one working file matches its patterns and every ancestor’s patterns.” | Separates applicability from the following matching instructions |
| “Nothing that is not listed creates a duty, and nothing should bring it back unless the user asks.” | “Unlisted branches create no Open Forge duties and must stay disabled until the user requests restoration.” | Names the duty and the restoration authority |
| “It is read right after its base and wins where they differ, for that file only.” | “It shares the base’s role, scope and loading behavior, is not indexed separately, and wins only for corresponding content in that source.” | Restores the precise overwrite boundary |

Whitespace-delimited counts, including Markdown markers: loader **1,071 words**, or **988** with empty Entries. Managed block **55 words**. The ledger records v2 at 823 and the shipped loader at 2,052.

### 9. Risks and proof

The loader grows because actual obligations return. Authoring also gains a required guide read. Those costs should be measured separately from ordinary task startup.

Moving Guidance, Maps, Patterns and Skills off startup is justified only because their obligations concern particular uses. Step 5 now states those triggers explicitly. Emerging’s capture obligation operates at refresh boundaries, so its default #KeepInMind returns.

Two product risks deserve explicit checks. The change map records an empty-loader case where Index could repopulate removed roots. Also, existing public inventories and startup tables still describe the earlier payload. Neither should be treated as corrected merely because this proposal says otherwise.

The cheapest introduction experiment uses five unfamiliar developers and disposable repositories:

1. Give each ninety seconds with the introduction. Ask what problem it solves, which files change, and the smallest trial.
2. Give them five minutes to connect CONTRIBUTING without help.
3. Start a fresh agent session and verify that it reads that document.
4. Remove its entry and verify that it stays disabled after Index and Update.
5. Repeat one matched and one unmatched file condition, with and without the CLI.

Proceed if at least four understand and complete the trial unaided, and every required-read and disabled-state check succeeds. Compare against the current README to distinguish improvement from mere feasibility.

This proposal was based on local source reads and the `origin/develop` payload diff. No files were changed, and no Doctor command, build or test suite was run.

### 10. The one idea to steal

Make the first successful use a fresh agent reading one document the project already trusts.

---

## Onboarding Designer (Proposal L)

/root - GPT-6 - high

### 1. Thesis

Keep v2’s activation model and introduce Open Forge through four useful examples: one existing document, one rule and accepted fact, resumable work, then the wider framework. Restore obligations where they are needed, with general authority and maintenance duties remaining in the loader. A smaller installation should change the files selected, without introducing another framework language.

### 2. How v2 removed so many rules

The [statement ledger](artifacts/council-3/ledger.md) shows six kinds of reduction.

| Kind | Two examples | Verdict |
| --- | --- | --- |
| Combining rules | Platform constraints became “within platform and safety limits”. Routine autonomy became “Take routine, reversible steps inside the request”. | Sound where every qualification survives. External effects and reversibility need explicit wording again. |
| Removing vocabulary | Loader L46’s `route` definition and L95’s `#Core` definition disappeared. | Sound for startup reading. Put the vocabulary beside the instructions for authors who need it. |
| Moving instructions to their use | Working L18 moved into `saving.md`. The loader’s command inventory moved into the CLI Skill. | Sound when an explicit reading requirement reaches the destination before the action. Saving must not become the only home for unrelated durable-update duties. |
| Cutting authoring safeguards | Patterns L19 restricted promotion of unsettled shapes. Templates L29 required focused prompts and clearly marked optional sections. | Losses. Capable models can still create unnecessary Patterns and sprawling Templates. Restore these locally. |
| Dropping lifecycle details | Loader L101 required updates before dependent work. Patterns L28 distinguished adaptations from conformance in review. | Losses. “Update” does not imply a deadline, and “explain” does not imply a review classification. |
| Treating permissions and examples as obvious | Working L19 allowed accepted temporary choices. Loader L85 illustrated loading inside C# and Windows scopes. | Preserve the permission locally and move the worked example into authoring help. Neither needs startup exposition. |

The reduction was therefore partly compression, partly relocation, and partly actual subtraction. “Ordinary authoring practice” and “rarely matters” do not establish redundancy.

### 3. The lossless ledger

These are all **34 Dropped and 12 Partly rows**. Paths below are installed paths. Quoted text is the exact proposed replacement. Replace overlapping v2 clauses instead of appending duplicates.

General duties stay required. Category rules are encountered before using or authoring that category. Catalogue-authoring instructions become a required read immediately before that action.

**Destination `.agents/loader.md`, `### Follow rules and accepted knowledge`**

- `loader.md` L21 — **Restore**: “Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.”
- `loader.md` L22 — **Restore**: “Use the fewest words that preserve meaning, readability, and logical progression in communication and authored content. Remove filler and repetition. Keep the context, conditions, and exceptions needed to understand and apply the result.”
- `loader.md` L40 — **Restore**: “Selected non-binding material may specialize broader material of the same kind within its accepted scope.”

**Destination `.agents/loader.md`, `### Keep sources useful and current`**

- `loader.md` L27 — **Restore**: “Let each source answer one clear question. Link to related sources instead of repeating their detail. A link does not change either source's authority, scope, loading rules, or lifecycle.”
- `loader.md` L28 — **Restore**: “Connect related material with relative links to local sources and precise references. Reuse consistent descriptive tags for shared subjects so tag and reference searches can find those connections. Add links and tags only when they help navigation or retrieval.”
- `loader.md` L32 — **Restore**: “Investigate an apparent #CurrentTruth conflict before changing either source. When accepted direction changes current state, update the source that defines it and preserve useful prior context.”
- `loader.md` L100 — **Restore**: “#Evergreen means material must stay aligned with accepted current state. It grants neither authority nor automatic loading.”
- `loader.md` L101 — **Restore**: “Update affected #Evergreen sources you may edit before work depends on them and no later than closeout. Batch related updates when safe.”
- `loader.md` L102 — **Restore**: “Keep each source aligned with what it represents now. Preserve useful prior context in an enabled Memory state, or beside the source when Memory is off. Report needed updates that are blocked or not authorized.”

**Destination `.agents/loader.md`, `### Read before you work`**

- `loader.md` L62 — **Restore**: “Use catalogues for navigation. Do not open file bodies just to discover routes or loading tags.”
- `loader.md` L93 — **Restore**: “Before handoff or finish, reread #KeepInMind files only in active scopes, with their overwrites and applicable required children in listed order.” Also: “Do not open file bodies just to discover routes or loading tags.”
- `loader.md` L94 — **Restore**: “During work, recheck them only when they may have changed.”

**Destination `.agents/README.md`, `## Catalogue authoring`**

- `loader.md` L37 — **Relocate**: “When authoring an entrypoint with no local rules, use `## Axioms` with one entry: `- inherited - No local axioms; loaded ancestor axioms remain active.` Existing missing or empty local sections also add no rules. Never combine the inherited marker with local rules.”
- `loader.md` L57 — **Relocate**: “You may nest scope folders before, between or after deeper route segments without a fixed depth limit. Each narrows the material below it.”
- `loader.md` L58 — **Relocate**: “Include only the routes useful in each scope. You do not need to mirror another scope or the defaults.”
- `loader.md` L85 — **Relocate**: “For example, a C# scope may contain a #LoadNow design file and an on-demand Windows scope. Selecting C# loads its design rules, not Windows. A #LoadNow file under Windows loads only after Windows is selected.”

**Destination `.agents/README.md`, `## Vocabulary`**

- `loader.md` L46 — **Relocate**: “`route`: Navigable path exposed through entrypoints and `Entries`.”
- `loader.md` L47 — **Relocate**: “`root route`: Route exposed directly by this loader.”
- `loader.md` L48 — **Relocate**: “`scope`: Part of a route that narrows where the following content applies.”
- `loader.md` L49 — **Relocate**: “`slug`: Concrete folder name in a route path.”
- `loader.md` L95 — **Relocate**: “`#Core`: Base routing and loading, workspace orientation, and reusable agent-facing roles.”
- `loader.md` L96 — **Relocate**: “`#Memory`: Self-growing Markdown state for active work, coordination, accepted knowledge, candidates, and history.”
- `loader.md` L97 — **Relocate**: “`#Extension`: Optional packaged routes, capabilities, integrations, and support files.”

**Destination `.agents/maps/_maps.md`, `## Axioms`**

- Maps L23 — **Restore**: “Keep Maps broad. Prefer modules, projects, repositories, systems, or scopes over members and functions unless finer routing clearly helps.”
- Maps L24 — **Restore**: “Each workspace chooses its own Map filenames, grouping, and nesting depth.”
- Maps L25 — **Restore**: “Maps do not replace Memory or the destination's current information.”

**Destination `.agents/patterns/_patterns.md`, `## Axioms`**

- Patterns L19 — **Restore**: “Create or update a Pattern only when an accepted shape should guide future related work. Structure alone does not justify promoting one-off work, temporary transitions, or unsettled candidates into a Pattern.”
- Patterns L25 — **Restore**: “Link related sources for required rules, advice, maps, procedures, and starting files, and say which of them bind the shape. Sources that apply the shape should link back to its Pattern.”
- Patterns L26 — **Restore**: “Keep required behavior, procedures, and substantial choice rationale in their defining sources. Example names and values illustrate the shape unless an applicable requirement fixes them.”
- Patterns L28 — **Restore**: “Adaptation stays within the authority granted for the work. In a review, report justified adaptations separately from conformance.” Keep v2’s default-shape rule and explanation deadline.

**Destination `.agents/templates/_templates.md`, `## Axioms`**

- Templates L24 — **Restore**: “If the new artifact needs continuing guidance or requirements, link the source that defines them. The Template provides only starting content.”
- Templates L28 — **Restore**: “State the need and primary question each Template answers in its description and source instructions.”
- Templates L29 — **Restore**: “Use focused prompts that request necessary content once, in a readable order. Mark optional sections clearly.”
- Templates L30 — **Restore**: “Treat generic Templates as fallbacks. Add a specialized Template only when it provides meaningfully different starting content.”

**Destination `.agents/memory/_memory.md`, introduction**

- Memory L13 — **Restore**: “Memory is self-growing. People and agents can add useful records and routed scopes without a fixed structural limit. Unselected branches do not load their file contents. Records need not pass through every state.”

**Destination `.agents/memory/_memory.md`, `## Axioms`**

- Memory L19 — **Restore**: “An accepted record can define current knowledge or decisions. Recording another category's content does not give the record that category's role.”
- Memory L26 — **Restore**: “Show the source and uncertainty when they affect trust or later acceptance. Distinguish what was stated, observed, or inferred.”
- Memory L27 — **Restore**: “Update affected sources before other work depends on the outcome and no later than closeout. Keep useful context and reasoning in Memory. Accepted durable knowledge must reach the source that defines it, not remain only in chat or a #Contextual record.”
- Memory L32 — **Restore**: “Add records and routed scopes when their likely future value justifies the cost of finding and reviewing them.”
- Memory L35 — **Restore**: “Named record categories and Templates are optional. Use an existing state and the narrowest useful scope. A record may live directly under that state. Do not recreate a removed category merely to save a record.”
- Memory L36 — **Restore**: “Use an existing suitable record before creating another. Within the authority granted for the work, add an ordinary record or scoped route when needed. Ask when its placement would change accepted meaning or cross an unresolved authority boundary.”

**Destination `.agents/memory/working/_working.md`, `## Axioms`**

- Working L19 — **Restore**: “An explicitly accepted temporary choice may remain here when its source, scope, and expected expiration are clear.”

**Destination `.agents/memory/emerging/_emerging.md`, `## Axioms`**

- Emerging L17 — **Restore**: “At each applicable #KeepInMind refresh, check visible `Entries` and save useful candidate material before it is lost.”
- Emerging L23 — **Restore**: “Refine material while its validity, use, or final destination remains unsettled.”
- Emerging L25 — **Restore**: “Revisit repeated or stale material and decide whether to consolidate, promote, archive, or prune it. Review may leave a record unchanged.”

**Destination `.agents/memory/archived/_archived.md`, `## Axioms`**

- Archived L17 — **Restore**: “Retain, consolidate, or transform history according to its future value. Archival need not preserve every detail.”

The ledger’s other classifications also need checking. Restore Emerging’s `#KeepInMind` in its frontmatter and parent entry, otherwise restoring its conditional capture rule would leave its default trigger absent. Restore explicit permission requirements for new top-level Memory states and accepted archival retention preferences. Restore Pattern annotations for optional and repeatable parts, nesting, fixed values and schematic omissions, plus Templates’ preservation of required structure and removal of irrelevant package tags.

Keep subject-based Directive catalogues available on demand. Require an unbroken `#LoadNow` chain specifically when rules must load automatically for matching files. Moving Guidance, Maps, Patterns and Skills off startup is justified only because the loader still requires opening them at their stated task triggers, before their content is needed.

### 4. The complexity ladder

The levels describe useful stopping points, not enforced modes. Their trees are cumulative.

```text
Level 1
AGENTS.md
CLAUDE.md                         optional bridge for manual installation
.agents/loader.md
.agents/README.md
CONTRIBUTING.md                    existing, linked without changes

Level 2 adds these example documents, or links existing equivalents
docs/agent-rules.md
docs/export-format.md

Level 3 adds
.agents/directives/_directives.md
.agents/memory/_memory.md
.agents/memory/crystallized/_crystallized.md
.agents/memory/working/_working.md
.agents/memory/working/saving.md
.agents/memory/working/export.md   created when useful

Level 4 adds
.agents/guidance/_guidance.md
.agents/maps/_maps.md
.agents/patterns/_patterns.md
.agents/templates/_templates.md
.agents/skills/_skills.md
.agents/skills/open-forge-cli/SKILL.md
.agents/skills/open-forge-cli/references/common.md
.agents/skills/open-forge-cli/references/discovery.md
.agents/skills/open-forge-cli/references/routes.md
.agents/skills/open-forge-cli/references/packages.md
.agents/memory/emerging/_emerging.md
.agents/memory/archived/_archived.md
```

CLI installations also maintain `.agents/open-forge.json` and `.agents/open-forge.lock.json`.

| Level | Benefit | Cost | Move one level |
| --- | --- | --- | --- |
| 1. Connect | Agents encounter one existing source before work. | One entry to maintain and the loader’s reading contract. | Up: add a real rule and accepted fact. Down: remove the managed handoff after reviewing dependencies. |
| 2. Specify | Required behavior and accepted knowledge become explicit without adopting folders. | Maintain two focused sources and their authority. | Up: add categories for new rules and resumable records. Down: remove their routing entries, while independently accepted requirements remain binding. |
| 3. Continue | Rules have a dedicated home, accepted knowledge is maintained, unfinished work survives sessions. | Five scaffolding files and deliberate record upkeep. | Up: select the other roles when useful. Down: disable their root entries, retain records, and record installation omissions. |
| 4. Organize | Full Core and all Memory states support larger collections. | More catalogues, selection decisions and maintenance. | Down: disable unused roots or states and release their management. Extensions remain separate choices. |

A brownfield project should start at Level 1. Its first success should use documentation it already trusts.

### 5. Ultra-lightweight install

Add **`--preset minimal`**. Commands using that value below are proposed v3 commands.

On a fresh workspace it writes the managed `AGENTS.md` and `CLAUDE.md` blocks, `.agents/loader.md` with an empty `Entries` section, `.agents/README.md`, settings and ownership records. It installs no category catalogues, CLI Skill, Memory states, saving policy, Extensions or Git-ignore section.

Allow `--route` with `minimal` as well as `custom`. Minimal starts with every row omitted, then applies explicit overrides:

```sh
open-forge install --preset minimal --route directives=add --route memory/crystallized=add --dry-run
```

This also includes the necessary Memory parent, without sibling states. `git-ignore` retains its existing meaning. Existing presets and unattended defaults retain their meanings.

The plan says: “Install Minimal: the loader, human guide and agent handoffs. No categories are selected. Existing project text stays outside the managed blocks.” It lists every physical effect and suggests adding one existing-document entry.

Persist concrete exclusions, without storing a new authoritative mode identity. On existing installations, require `--configure`. Explain that omissions preserve existing files and do not themselves disable their routing.

Expected C# changes:

| Area | Files |
| --- | --- |
| Selection and binding | `InstallConfiguration.cs`, `InstallConfigurationChoices.cs`, `InstallBindingInputReader.cs`, `InstallSetupResolver.cs` |
| Empty Memory selection | `InstallConfigurationSettings.cs`, with payload selection checked in `FrameworkPayloadSelection.cs` |
| Questions, recap and retries | `InstallSetupQuestions.cs`, `InstallSetupRecapSelector.cs`, `InstallNextCommand.cs` |
| Help and messages | `InstallInputText.cs`, `InstallSetupText.cs`, `InstallText.cs`, `SharedText.cs` |
| Empty activation preservation | `GeneratedNavigationRegionPlanner.cs` |

Cost is bounded installation and navigation work, focused tests, help snapshots and public command coverage. No Markdown parser or settings-schema migration is required. Growth uses existing Custom overrides or `route init <category> --framework`.

### 6. Brownfield adoption

Use an existing CSV-export repository as the worked example. Its `docs/architecture.md` exists, and its maintainer accepts the export convention shown below.

1. Inspect the starting point:

   ```sh
   git status --short
   rg --files docs
   ```

   Read the existing `README.md`, `CONTRIBUTING.md` and host instruction file. Open Forge has introduced no reading duties yet.

2. Preview and install:

   ```sh
   open-forge install --preset minimal --dry-run
   open-forge install --preset minimal --automatic
   git status --short
   git diff -- AGENTS.md CLAUDE.md .agents
   ```

   Inspect new files directly too. The agent now reads the existing host instructions and loader. The empty list adds no category reads.

3. Replace the loader’s empty placeholder with:

   ```markdown
   - [Contribution rules for this project](../CONTRIBUTING.md) - #LoadNow
   ```

   A fresh session now reads `CONTRIBUTING.md`. Ask: “Explain the project’s test procedure and name the files you read.”

4. Add discovery without broad startup reading:

   ```markdown
   - [Project overview and development commands](../README.md)
   - [Current system architecture](../docs/architecture.md)
   ```

   The agent opens these when their descriptions fit the task. Merely linking them establishes no new acceptance.

5. Add the rule and accepted fact from the README example below. Add the stated authority sentence outside the managed `AGENTS.md` block. Export work now reads both documents, while unrelated work avoids them.

6. When interruptions become costly, preview and apply:

   ```sh
   open-forge install --configure --preset custom --route directives=add --route memory/crystallized=add --route memory/working=add --dry-run
   open-forge install --configure --preset custom --route directives=add --route memory/crystallized=add --route memory/working=add --automatic
   ```

   The agent additionally reads the selected catalogues and saving policy, then the relevant work record. Existing docs remain in place.

Open Forge must never overwrite host text outside its recognized block, move documentation merely to adopt it, infer acceptance from code or tags, silently restore disabled parts, or claim ownership of existing documents.

Useful CLI help is a copyable “link an existing document” example and a recap distinguishing omitted defaults, enabled routes and deletion. Automatic document classification can wait.

### 7. The introduction

The root README should demonstrate value and provide the next action. The installed guide should explain local editing. The docs site should hold the extended walkthrough, troubleshooting, scope examples and package choices, with existing URLs retained and stale startup descriptions corrected.

Complete replacement opening and start section:

````markdown
## Open Forge

Open Forge helps coding agents use the rules, docs and decisions your project already has. You choose what they read through a short list of Markdown links. Start with one document, then add rules or memory when they solve a problem.

The files work without the CLI. Your agent tool supplies the runtime and must read `AGENTS.md`, or the `CLAUDE.md` bridge. Open Forge gives it instructions and context, so ordinary review still matters.

### Start with one existing document

With the CLI installed, preview the smallest setup:

```sh
open-forge install --preset minimal --dry-run
```

Review the plan, then apply that selection:

```sh
open-forge install --preset minimal --automatic
```

Install preserves your existing text outside the marked Open Forge sections. Review new files with `git status` and inspect the changes with `git diff`.

Under `## Entries` in `.agents/loader.md`, replace the empty placeholder with:

```markdown
- [Contribution rules for this project](../CONTRIBUTING.md) - #LoadNow
```

Use a document that exists in your repository. It stays where it is and needs no frontmatter.

Now ask your agent:

> Explain how this project runs its tests. Follow AGENTS.md and name the files you read.

The loader asks it to read `CONTRIBUTING.md` before the task. Check the answer against that document.

No CLI? Copy `loader.md` and `.agents/README.md` from the [Framework source](src/open-forge/.agents/), append the [AGENTS.md block](src/open-forge/AGENTS.md) to your existing file, and replace the loader's Entries with the single line above. Append the [CLAUDE.md bridge](src/open-forge/CLAUDE.md) too if your tool uses it. Merge existing files by hand.

You can stop here.

### Add one rule and one accepted fact

Here is a worked example for a project with CSV exports under `src/export/`. Use conventions your project actually needs.

Create `docs/agent-rules.md`:

```markdown
## Export rules

### Instructions

- When changing CSV exports, run the relevant export tests before reporting completion. Include the command and result. If the tests cannot run, explain why.
```

After accepting UTF-8 as the export encoding, create `docs/export-format.md`:

```markdown
## Export format

CSV exports use UTF-8.
```

Outside the managed block in `AGENTS.md`, record the authority you are assigning:

```markdown
Use docs/export-format.md as the accepted source for CSV export encoding.
```

Add these loader entries:

```markdown
- [Required behavior for export changes](../docs/agent-rules.md) - #LoadNow - applies to `src/export/**`
- [Accepted CSV export encoding](../docs/export-format.md) - #LoadNow - applies to `src/export/**`
```

An export task reads the rule and format before working. An unrelated task does not load them through these entries. Linking a document asks the agent to read it. Your acceptance determines which project facts it may rely on.

These files can stay in `docs/`. You do not need Open Forge categories to use them.

### Keep unfinished work across sessions

When you want dedicated places for new rules, accepted knowledge and resume notes, add only those parts:

```sh
open-forge install --configure --preset custom --route directives=add --route memory/crystallized=add --route memory/working=add --dry-run
```

Review the plan, then apply the same command with `--automatic` instead of `--dry-run`.

Directives hold required behavior. Crystallized Memory holds accepted knowledge. Working Memory holds temporary resume notes. Existing documents can remain linked where they are.

With Working's saving policy enabled, the agent records unfinished progress before pausing. Ask it to pause an export task, then start a fresh session with:

> Resume the export task from its Working Memory record. Check the record against the current files before continuing.

The record should identify the goal, current state, next step and open questions. No Planning Extension is needed.

### Add the wider framework when useful

For every built-in category and Memory state:

```sh
open-forge install --configure --preset full-core --dry-run
```

Review the plan, then apply the same selection with `--automatic`.

This adds places for advice, reusable shapes, starting files, source maps, capabilities, unsettled findings and history. Their files explain how each role works. Full Core does not install Extensions or require a workflow.

You can also add one omitted category:

```sh
open-forge route init patterns --framework --dry-run
```

After reviewing it, run the same command without `--dry-run`. A broader excluded ancestor must be restored explicitly first.

### Make it yours

Every category and Memory state is optional. Edit the links, use your own folder names, keep existing docs, and remove parts that do not help. Removing a loader entry disables that part's Open Forge behavior while keeping its files. Accepted project requirements and instructions your agent tool loads independently still apply.

Installation choices, activation and deletion are different operations. Custom's `remove` choice preserves existing files and routes. The installed `.agents/README.md` explains how to turn behavior off and keep updates from restoring omitted content.

Open Forge is built around Adaptive Context Engineering: bring in the context a task needs, then keep useful outcomes for later. Your setup can remain one link or grow through the parts you choose.

Continue with the [ten-minute guide](https://thelithiumforge.github.io/open-forge/docs/getting-started/ten-minute-guide), or read the [installation guide](https://thelithiumforge.github.io/open-forge/docs/getting-started/installation) for CLI installation, other setup methods and existing-workspace details.
````

Complete `.agents/README.md`:

````markdown
## Open Forge in this project

Your agent starts at `AGENTS.md` and follows [loader.md](loader.md). Its Entries show which parts this workspace has enabled. Everything is plain Markdown. The CLI makes reading and maintenance faster.

### Link a document you already have

Add a line under the loader's `## Entries`:

```markdown
- [Contribution rules](../CONTRIBUTING.md) - #LoadNow
```

The document stays where it is and needs no frontmatter. `#LoadNow` requires reading when its parent loads.

For a rule that only concerns C# work:

```markdown
- [C# conventions](../docs/contributing/csharp.md) - #LoadNow - applies to `**/*.cs`
```

Use a path that exists. Omit the loading tag for a source the agent should open only when its description fits the task. Conditions limit applicability and grant no edit permission.

### Add structure when it helps

Directives hold required behavior under `## Instructions`. Crystallized Memory holds accepted knowledge. Working Memory holds resume notes. Other categories and Memory states are optional.

Keep existing documents where they are. New Memory records may live directly under an installed state. Named record categories and Extensions are optional.

With the CLI, preview a narrow addition:

```sh
open-forge route init patterns --framework --dry-run
```

Review it, then run the same command without `--dry-run`. Restore any broader excluded ancestor explicitly first. Use `install --configure` to change several installation choices.

Without the CLI, copy the selected category files, list only the states you want, and maintain their Entries yourself.

### Turn behavior off

Remove its loader entry to disable a whole part and retain its files. Check enabled dependents first. Accepted project requirements and instructions loaded independently by your agent tool still apply.

Inside another catalogue, Entries follow the files in that folder. Move or delete a child and remove its entry to stop using it there.

To stop automatic progress saving while keeping Working records, delete `memory/working/saving.md` and its entry in `_working.md`. In a CLI-managed installation, also add `.agents/memory/working/saving.md` to `removedFiles` in `.agents/open-forge.json`, preserving the other settings.

Installation omission, disabling behavior and deleting files are separate actions. Custom's `remove` choice releases Framework management and retains existing files and routes. Never use a deletion command merely to disable behavior.

### Keep local changes through updates

For a change that should survive CLI updates, create `{name}.overwrite.md` beside its base. It loads immediately after the base and changes only the corresponding content of that source.

You may edit a shipped file directly too. Ordinary CLI Update can replace that edit, with prior bytes retained in recovery data. Review the update preview.

### Catalogue authoring

A catalogue is also called an entrypoint. Name it `_{folder-name}.md`. The compatibility names are `index.md`, `_index.md`, `references.md` and `_references.md`. Keep one recognized catalogue per routed folder.

When authoring an entrypoint with no local rules, use `## Axioms` with one entry: `- inherited - No local axioms; loaded ancestor axioms remain active.` Existing missing or empty local sections also add no rules. Never combine the inherited marker with local rules.

You may nest scope folders before, between or after deeper route segments without a fixed depth limit. Each narrows the material below it.

Include only the routes useful in each scope. You do not need to mirror another scope or the defaults.

For example, a C# scope may contain a #LoadNow design file and an on-demand Windows scope. Selecting C# loads its design rules, not Windows. A #LoadNow file under Windows loads only after Windows is selected.

Keep specialized content in the narrowest useful scope. Use an unbroken required chain and appropriate file conditions when it must load automatically.

New routed Markdown follows the loader's frontmatter rules. Tags in frontmatter omit `#`. Maintain descriptions, tags and conditions in Entries as well. Preview `open-forge index --dry-run` before using Index to maintain them.

### Vocabulary

- `route`: Navigable path exposed through entrypoints and `Entries`.
- `root route`: Route exposed directly by this loader.
- `scope`: Part of a route that narrows where the following content applies.
- `slug`: Concrete folder name in a route path.
- `#Core`: Base routing and loading, workspace orientation, and reusable agent-facing roles.
- `#Memory`: Self-growing Markdown state for active work, coordination, accepted knowledge, candidates, and history.
- `#Extension`: Optional packaged routes, capabilities, integrations, and support files.

These labels grant no authority or automatic loading.

### Reading and maintenance

`open-forge context --for <file>` gathers tagged required context. The agent still follows relevant links and explicit reading instructions.

Before other CLI operations, read the installed CLI Skill when present and check the command's help. Preview mutations and inspect their plans.

The [growth guide](https://thelithiumforge.github.io/open-forge/docs/getting-started/grow-your-framework) gives longer examples. The installed files define this workspace's actual behavior.
````

### 8. v3

This is the complete Full Core loader. Installation filters its Entries to the selected parts. Minimal starts with the same rules and an empty list.

```markdown
## Open Forge Loader

This file tells agents which context this workspace uses. It adds to the project's other instructions and works without the CLI. Under `Entries`, a listed part is enabled and an unlisted part is disabled. A catalogue, also called an entrypoint, is a Markdown file that lists other files under `## Entries`.

### Axioms

### Read before you work

1. At task start or resume, and after context restoration, list your working files. Include files you will investigate, review, create, change, rename or delete, including planned files and both paths of a rename. Reading a file for background does not make it a working file.
2. If the CLI is available and allowed, run `open-forge context` with one `--for <file>` for each known working file. Read its output. This covers the tagged reads and file conditions in steps 3 and 4. Always complete step 5 yourself. If the command is missing, fails or cannot follow a link, use your file tools for steps 3 to 5.
3. Subject to step 4, read every #LoadNow and #KeepInMind entry in listed order. Read each catalogue before its children and repeat this procedure inside it. A description that looks unrelated never excuses a required read. Read each file once per pass.
4. An entry with `applies to` conditions loads and applies only when one working file matches its conditions and every ancestor catalogue's conditions. Patterns in one list are alternatives. Match case-sensitive globs from the workspace root. While working files are unknown, report exposed conditioned required entries as pending. Resolve them before working on a matching file. Conditions filter reading and application, not edit permission.
5. Open other entries when their descriptions fit the task. Follow explicit required-read instructions in every file you read. Before using a routed file found through search or a direct link, read its enabled ancestor catalogues and required context. Use catalogues for navigation. Do not open file bodies just to discover routes or loading tags. Disabled files provide background only, subject to the independent requirements below.

Restart at step 1 when your goal or working files change. Before handoff or finish, reread #KeepInMind files only in active scopes, with their overwrites and applicable required children in listed order. During work, recheck them only when they may have changed.

### Follow rules and accepted knowledge

- Follow loaded catalogue `## Axioms` and rule-file `## Instructions` within their stated scope. Only the loader and loaded catalogues define Axioms. Child rules add to ancestor rules and never cancel them. Do not repeat inherited rules. Report conflicts or instructions you cannot follow, with the reason.
- Follow the user's direction within platform and safety limits. Take routine, reversible steps within the request without asking again. State assumptions that affect the result. Ask before unresolved choices significantly change the result, scope, risk, cost, external effects or ability to undo the work.
- Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.
- Use the fewest words that preserve meaning, readability, and logical progression in communication and authored content. Remove filler and repetition. Keep the context, conditions, and exceptions needed to understand and apply the result.
- Reading, linking, saving, moving or tagging a file grants neither acceptance nor edit permission. Repetition and agent confidence do not establish acceptance. Accepted means the user agreed, or an authority the user declared establishes it within its scope. Follow declared external authorities for the facts assigned to them. Validation supplies evidence, not acceptance.
- Selected non-binding material may specialize broader material of the same kind within its accepted scope.

### Keep sources useful and current

- Let each source answer one clear question. Link to related sources instead of repeating their detail. A link does not change either source's authority, scope, loading rules, or lifecycle. Put detail in the narrowest source that defines it and keep only selection summaries above it.
- Connect related material with relative links to local sources and precise references. Reuse consistent descriptive tags for shared subjects so tag and reference searches can find those connections. Add links and tags only when they help navigation or retrieval.
- Investigate an apparent #CurrentTruth conflict before changing either source. When accepted direction changes current state, update the source that defines it and preserve useful prior context. #CurrentTruth labels accepted current state within its stated scope. #Contextual labels useful material that is not accepted unless applicable authority establishes acceptance.
- #Evergreen means material must stay aligned with accepted current state. It grants neither authority nor automatic loading. Update affected #Evergreen sources you may edit before work depends on them and no later than closeout. Batch related updates when safe.
- Keep each source aligned with what it represents now. Preserve useful prior context in an enabled Memory state, or beside the source when Memory is off. Report needed updates that are blocked or not authorized.

### Change the setup

- Add an `Entries` line to enable a catalogue or a Markdown file anywhere inside the workspace. For example, `../docs/architecture.md` links an existing document without moving it or adding frontmatter. The entry supplies its description, tags and conditions.
- Remove a loader entry to disable that part while keeping its files. Background reading does not reactivate its Open Forge rules. Disabling never cancels accepted project requirements or instructions the host loads independently. Report enabled dependents before disabling a part. Never recreate a disabled part unless the user asks.
- Other catalogues index the files in their folders. To disable a child there, move or delete its file and remove its entry. For CLI-managed content, also record the omission in `.agents/open-forge.json` before updating. Disabling, releasing management and deleting files are different actions.
- Report a missing listed file as an error. An empty `Entries` section is valid.
- #LoadNow requires reading when its enabled parent loads. #KeepInMind also requires the refreshes above. Neither tag activates an unselected ancestor. Keep an unbroken required chain to files that must load automatically, with conditions narrow enough for their scope. Other tags classify content, including the #Evergreen duty above, and help search.
- Before creating or changing a catalogue, read [Catalogue authoring](README.md#catalogue-authoring). New routed Markdown inside `.agents/` needs `description`, `tags` and optional `applyTo` frontmatter. Follow `frontmatter` in `.agents/open-forge.json`, using scoped `open-forge:` fields when the setting is absent. Native Skills keep their own metadata. Maintain Entries by hand or with `open-forge index`.
- A user-owned `{name}.overwrite.md` loads immediately after `{name}.md`, sharing its role, scope and loading conditions. Do not index or select it separately. It wins only where the base answers the same question differently, and never over unrelated sources. Updates preserve the companion.

### Entries

- [Rules agents must follow in this workspace](directives/_directives.md) - #LoadNow #Core #Directive
- [Advice for recurring choices, to check before choosing an approach](guidance/_guidance.md) - #Core #Guidance
- [Where to find project knowledge, checked before relying on it](maps/_maps.md) - #Core #Map
- [Notes that help work continue, such as resume points, accepted knowledge, ideas and history](memory/_memory.md) - #LoadNow #Memory
- [Default shapes to check before creating or reviewing structure](patterns/_patterns.md) - #Core #Pattern
- [Capabilities to check before specialized work](skills/_skills.md) - #Core #Skill
- [Starting files to copy when creating something new](templates/_templates.md) - #Core #Template
```

Complete managed `AGENTS.md` block:

```markdown
## Open Forge

Open Forge links this workspace's chosen rules and context.

Before starting or resuming a task, read `.agents/loader.md` and follow it throughout the work. If the CLI is available and allowed, `open-forge context --for <file>` gathers the tagged required reads for each known working file. The loader explains the remaining reads and the fallback to plain files.

**End of Open Forge managed section.**
```

| Before | After | Why |
| --- | --- | --- |
| “The `Entries` below are the switch: what is listed is on, and anything not listed is off.” | “Under `Entries`, a listed part is enabled and an unlisted part is disabled.” | Names the states directly. |
| “That covers steps 3 and 4.” | “This covers the tagged reads and file conditions in steps 3 and 4.” | Explains the shortcut’s boundary. |
| “Read and apply an entry with `applies to` patterns only when the same working file matches its patterns and those of every conditioned catalogue above it.” | “An entry with `applies to` conditions loads and applies only when one working file matches its conditions and every ancestor catalogue's conditions.” | Makes the single-file witness explicit. |
| “Nothing that is not listed creates a duty, and nothing should bring it back unless the user asks.” | “Never recreate a disabled part unless the user asks.” | Avoids contradicting independent accepted requirements. |
| “It is read right after its base and wins where they differ, for that file only.” | “It wins only where the base answers the same question differently, and never over unrelated sources.” | Explains corresponding-content precedence. |

Whitespace-delimited word counts, including headings and Entries: **loader 1,216**, up from v2’s **823**. Managed block **65**, up from **56**. Restored obligations account for the increase. Full Core’s total startup cost must be measured after the catalogue restorations.

### 9. Risks and proof

The longer loader could reduce completion rates. Direct links could accumulate into an indiscriminate startup list. Users could confuse disabling a rule with revoking an accepted requirement.

There is also a concrete implementation gap. `GeneratedNavigationRegionPlanner.ReadLoaderRootPaths` returns structural roots when the parsed list is empty, even while preserving membership. An intentionally empty loader must remain empty. Install and explicit restoration should supply their selected additions separately.

Before release, prove empty-list preservation, direct-link preservation, required-chain loading, matching and nonmatching paths, cancelled installation, repeat no-op, retained host text, and omissions surviving updates. Include a fresh-checkout check for any Git-ignored Working setup.

The cheapest onboarding experiment needs three unfamiliar developers and a small existing-repository fixture. Give them only the proposed introduction. Ask each to link one document, add the scoped rule and accepted fact, resume a task, then disable saving. Begin with hand-assembled files and no CLI to isolate the explanation.

Require all three to reach the first useful result within five minutes without help. Inspect actual reads, scope mistakes, premature acceptance and retained work. Then repeat maintenance with the CLI. Task 73 explicitly leaves stranger usability unproven, so historical test counts cannot substitute for this experiment.

This proposal is based on read-only source inspection. No files were changed, and no builds, test suites or `doctor` runs were performed.

### 10. The one idea to steal

Make every onboarding step a useful setup someone can keep forever.

---

## Plain-Language Writer (Proposal F)

/root - GPT-6 - high

### Thesis

Keep v2’s reading procedure and explicit on/off switch. Restore missing obligations where people and agents need them, then explain adoption through useful actions rather than category names. The first success should be an agent using one document the repository already has.

### How v2 removed so many rules

I see six kinds of removal.

| Kind | Two examples | Verdict |
| --- | --- | --- |
| Removing technical vocabulary | Loader L46 defines `route`. L49 defines `slug`. | Sound when ordinary words express the surviving behavior. |
| Compressing several obligations into one | Loader L27 becomes “update its record instead of adding a competing one”. Maps L25 becomes “Each destination keeps its own detail and authority”. | Neither replacement implies the complete original. Restore the missing boundaries. |
| Moving lifecycle rules | Memory L24 moves communication capture into `saving.md`. L25 moves durable-outcome capture there too. | A separate saving policy is useful, but optional Working policy cannot carry every Memory-wide duty. |
| Moving tool details to the CLI Skill | Loader L116 moves `responsibility` documentation. L130–138 move the command inventory. | Sound when the Skill is reached before those commands are used. Plain-file operation must remain complete. |
| Removing authoring rules as presumed competence | Patterns L19 limits creation to accepted reusable shapes. Templates L29 requires focused, ordered prompts and clear optional sections. | These are instructions, not explanations of ordinary intelligence. Restore them in their catalogues. |
| Shortening timing and review requirements | Loader L101 loses “before work depends on them”. Patterns L28 loses separate reporting of departures during review. | These are behavioral losses. The shorter wording does not imply them. |

The ledger sometimes treats a related sentence as an equivalent sentence. That explains much of the reduction. The defensible savings come from fewer terms, fewer examples in startup context and details placed beside their use.

### The lossless ledger

Paths below are relative to the shipped `.agents/` directory. Old line numbers identify rows in `artifacts/council-3/ledger.md`. Each quoted passage is proposed replacement text.

**Destination: `loader.md`, `### Use what you read`**

- `loader.md:L21` — **Restore:** “Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation and at most one important open choice.”
- `loader.md:L22` — **Restore:** “Use the fewest words that preserve meaning, readability and logical progression. Remove filler and repetition. Keep the context, conditions and exceptions needed to understand and apply the result.”
- `loader.md:L27` — **Restore:** “Let each source answer one clear question. Link to related sources instead of repeating their detail. A link changes neither source’s authority, scope, loading rules or lifecycle.”
- `loader.md:L28` — **Restore:** “Use relative links and precise references to connect local sources. Reuse consistent descriptive tags for shared subjects. Add links and tags only when they help navigation or retrieval.”
- `loader.md:L32` — **Restore:** “Investigate an apparent conflict in accepted current knowledge before changing either source. When accepted direction changes current state, update the source that defines it and preserve useful prior context.”
- `loader.md:L40` — **Restore:** “Selected advice or other non-binding material may specialize broader material of the same kind within its accepted scope.”
- `loader.md:L100` — **Restore:** “#Evergreen marks material that must stay aligned with accepted current state. It grants no authority and adds no loading behavior.”
- `loader.md:L101` — **Restore:** “Update affected #Evergreen sources you may edit before work depends on them and by closeout at the latest. Batch related updates when safe.”
- `loader.md:L102` — **Restore:** “Keep each source aligned with what it represents now. Preserve useful prior context where this workspace keeps it. Report needed updates that are blocked or not authorized.”

**Destination: `loader.md`, `### Read before you work`**

- `loader.md:L62` — **Restore:** “Use catalogues to discover context instead of opening ordinary file bodies just to find links or tags.”
- `loader.md:L93` — **Restore:** “Before handing off or finishing, reread #KeepInMind entries in the context still selected for the task.” The preceding navigation sentence also preserves the prohibition on opening bodies merely to discover tags.
- `loader.md:L94` — **Restore:** “During work, reread them only when they may have changed.”

**Destination: `loader.md`, `### Change what is on`**

- `loader.md:L37` — **Restore:** “When writing a catalogue with no local rules, put `- inherited - No local axioms; loaded ancestor axioms remain active.` under `## Axioms`. Never combine this marker with local rules. An existing missing or empty Axioms section also adds no rules.”
- `loader.md:L57` — **Restore:** “You may nest catalogues at any depth and place them before, between or after deeper folders. Each narrows everything below it.”
- `loader.md:L58` — **Restore:** “Each catalogue may list only the parts useful there.”

**Destination: `README.md`, `## Link an existing document`**

- `loader.md:L85` — **Relocate:** “A loading tag cannot open an unselected parent. For example, selecting a C# catalogue reads its #LoadNow design file. An on-demand Windows catalogue inside it stays closed. Its #LoadNow files become required only after Windows is selected.”

**Destination: `README.md`, `## Names you may meet`**

- `loader.md:L95` — **Relocate:** “Core means routing, loading, workspace orientation and reusable agent-facing content roles.”
- `loader.md:L96` — **Relocate:** “Memory means self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history.”
- `loader.md:L97` — **Relocate:** “An Extension is an optional package of routes, capabilities, integrations and support files.”

These definitions classify content. Moving them out of startup reading changes no loading or authority behavior.

**Destination: `maps/_maps.md`, `## Axioms`**

- `_maps.md:L23` — **Restore:** “Keep Maps broad. Prefer modules, projects, repositories, systems or scopes over members and functions unless finer routing clearly helps.”
- `_maps.md:L24` — **Restore:** “Choose Map filenames, grouping and nesting depth to suit the workspace.”
- `_maps.md:L25` — **Restore:** “Maps point to sources. They do not replace Memory or the destination’s current information.”

**Destination: `patterns/_patterns.md`, `## Axioms`**

- `_patterns.md:L19` — **Restore:** “Create or update a Pattern only when an accepted shape should guide future related work. A one-off structure, temporary transition or unsettled candidate is not enough.”
- `_patterns.md:L25` — **Restore:** “Link related rules, advice, maps, procedures and starting files. Say which sources bind the shape. Sources that use the shape should link back to its Pattern.”
- `_patterns.md:L26` — **Restore:** “Keep required behavior, procedures and substantial choice rationale in their defining sources. Example names and values illustrate the shape unless an applicable requirement fixes them.”
- `_patterns.md:L28` — **Restore:** “Adapt a Pattern only within the authority already granted for the work. Explain material departures before other work depends on them. In review, report justified adaptations separately from conformance.”

**Destination: `templates/_templates.md`, `## Axioms`**

- `_templates.md:L24` — **Restore:** “When a copy needs continuing guidance or requirements, link the source that defines them. A Template supplies starting content only.”
- `_templates.md:L28` — **Restore:** “State the need and primary question each Template answers in its description and source instructions.”
- `_templates.md:L29` — **Restore:** “Use focused prompts that request necessary content once, in a readable order. Mark optional sections clearly.”
- `_templates.md:L30` — **Restore:** “Use generic Templates as fallbacks. Add a specialized Template only when it provides meaningfully different starting content.”

**Destination: `memory/_memory.md`, opening paragraph**

- `_memory.md:L13` — **Restore:** “Memory is self-growing. Add useful records and routed scopes without a fixed structural limit. Unselected branches stay closed. Records need not pass through every state.”

**Destination: `memory/_memory.md`, `## Axioms`**

- `_memory.md:L19` — **Restore:** “An accepted record may define current knowledge or decisions. Recording another category’s content does not give the record that category’s role.”
- `_memory.md:L26` — **Restore:** “Show source and uncertainty when they affect trust or later acceptance. Distinguish what was stated, observed or inferred.”
- `_memory.md:L27` — **Restore:** “Update affected sources before other work depends on the outcome and by closeout at the latest. Keep useful context and reasoning in Memory. Accepted durable knowledge must reach its defining source rather than remain only in chat or an unaccepted record.”
- `_memory.md:L32` — **Restore:** “Add records and routed scopes when their likely future value justifies the cost of finding and reviewing them.”
- `_memory.md:L35` — **Restore:** “Named record categories and Templates are optional. Use an existing state and the narrowest useful scope. A record may live directly under its state. Do not recreate a removed category merely to save a record.”
- `_memory.md:L36` — **Restore:** “Use an existing suitable record before creating another. Within the authority granted for the work, add an ordinary record or scoped route when needed. Ask before placement changes accepted meaning or crosses an unresolved authority boundary.”

**Destination: `memory/working/_working.md`, `## Axioms`**

- `_working.md:L19` — **Restore:** “An explicitly accepted temporary choice may stay here when its source, scope and expected expiration are clear.”

**Destination: `memory/emerging/_emerging.md`, `## Axioms`**

- `_emerging.md:L17` — **Restore:** “At each applicable #KeepInMind refresh, check visible Entries and save useful candidate material before it is lost.”
- `_emerging.md:L23` — **Restore:** “Refine material while its validity, use or final destination remains unsettled.”
- `_emerging.md:L25` — **Restore:** “Revisit repeated or stale material and decide whether to consolidate, promote, archive or prune it. Review may leave a record unchanged.”

**Destination: `memory/archived/_archived.md`, `## Axioms`**

- `_archived.md:L17` — **Restore:** “Retain, consolidate or transform history according to its future value. Archival need not preserve every detail.”

**No destination**

- `loader.md:L46` — **Accept loss:** Remove the technical definition of `route`. The procedure explains which links to follow without requiring this vocabulary.
- `loader.md:L47` — **Accept loss:** Remove `root route`. Loader Entries identify the starting context directly.
- `loader.md:L48` — **Accept loss:** Remove the glossary definition of `scope`. Stated applicability, inherited rules and narrowing catalogues retain its behavior.
- `loader.md:L49` — **Accept loss:** Remove `slug`. Readers can use “folder name”.

That accounts for all 34 Dropped and 12 Partly rows.

A complete patch must also correct misleadingly optimistic Kept and Moved rows. Restore explicit user direction before creating a top-level Memory state. Restore Emerging’s #KeepInMind metadata and parent entry when Emerging is installed. Preserve Pattern labels for optional and repeatable parts, Template copies’ required structure, and removal of archived metadata that still claims current authority. These obligations are not implied by the shorter replacements.

### The complexity ladder

Let **B** mean this installed tree:

```text
AGENTS.md
CLAUDE.md
.agents/
  loader.md
  README.md
```

CLI installations additionally keep `open-forge.json` and `open-forge.lock.json`. These are maintenance files, not agent context.

| Level | Installed tree | Benefit and cost | Move one level |
| --- | --- | --- | --- |
| 1. Connect existing documents | B, with Entries linking existing files | Predictable discovery without moving documents. Maintain a few links and reading conditions. | Up: add Directives or Crystallized. Down: remove the managed host block and retain the files as ordinary notes. |
| 2. Keep rules and accepted knowledge | B plus `directives/_directives.md`, `memory/_memory.md`, `memory/crystallized/_crystallized.md` and chosen records | Corrections and accepted knowledge have explicit homes. Review changes and keep current knowledge current. | Up: add Working. Down: unlink Directives and Memory, preserving independently accepted project requirements. |
| 3. Resume unfinished work | Level 2 plus `memory/working/_working.md`, `saving.md` and resume records | Sessions can continue from recorded state. Agents maintain short records before pausing. | Up: add the remaining useful categories. Down: move Working outside the enabled tree and remove its entry. |
| 4. Use the full framework | Level 3 plus Guidance, Maps, Patterns, Skills, Templates, Emerging and Archived catalogues. Skills includes the CLI Skill and its four references. | Reusable advice, shapes, capabilities, starters and differentiated Memory. More descriptions and sources need maintenance. | Down: disable the added roots and move unwanted Memory states outside the enabled tree. |

Record CLI delivery exclusions when stepping down. Removing a loader line disables behavior while keeping files. Removing a nested entry requires moving or deleting its file as v2 specifies.

A brownfield repository should start at level 1. It can establish value using existing authority before adopting a new place for knowledge. Level 2 is appropriate when repeated corrections or newly accepted facts need somewhere to live.

These levels explain choices. They are not stored modes. Extensions remain independent selections at any level whose dependencies are available.

### Ultra-lightweight install

Add **`--preset minimal`**.

On a fresh repository it writes B, settings and ownership records. Loader Entries starts empty. It installs no category, Memory state, saving policy, CLI Skill or Extension. It leaves `.gitignore` unchanged unless a selected route uses `git-ignore`.

Allow the existing row grammar with `minimal` as well as `custom`:

```sh
open-forge install --preset minimal --dry-run
open-forge install --preset minimal --automatic

open-forge install --preset minimal \
  --route directives=add \
  --route memory/crystallized=add \
  --dry-run
```

Minimal starts every row at `remove`, then applies explicit overrides. Preserve identical-repeat and conflicting-repeat behavior. Persist concrete omissions using existing settings, without a preset identity. Omit the Memory parent when no state is selected, without adding a broad exclusion that obstructs later state restoration.

The plan says: “Add the loader, human guide and managed agent handoffs. No optional folders are selected.” Standard detail lists every file and reports existing content retained and still active.

Existing installations require `--configure`. Existing omitted content remains routable under the current retention contract. The plan must explain that configuring Minimal does not disable retained routes.

Grow with the existing `route init <category> --framework` command, previewing each addition.

C# changes include these files under `src/cli/operations/OpenForge.Cli.Operations/Commands/Install/`:

- `Models/Configuration/InstallConfiguration.cs`
- `Models/Binding/InstallBindingModels.cs`
- `Shared/Binding/InstallBindingInputReader.cs`
- `Shared/Configuration/InstallConfigurationChoices.cs`
- `Shared/Configuration/InstallSetupResolver.cs`
- `Shared/Configuration/InstallConfigurationSettings.cs`

Also update `FrameworkPayloadSelection.cs`, Install prompt and report selection, `InstallSetupText.cs`, `InstallInputText.cs` and help examples. Cost is a bounded installer change with unit, integration, published-command and output-snapshot coverage. The Markdown parser grammar stays unchanged. Prove empty Entries survives Index and Update before shipping.

### Brownfield adoption

The following walkthrough assumes Minimal has been implemented. Today, the same first setup can be assembled by copying the loader and human guide and appending the managed host block.

1. Inspect existing instructions and documentation. Run `rg --files docs --glob '*.md'`. Keep `AGENTS.md` and `CLAUDE.md` as host entry files rather than routing them back through the loader.
2. Preview and apply the same selection:

   ```sh
   open-forge install --preset minimal --frontmatter root --dry-run
   open-forge install --preset minimal --frontmatter root --automatic
   ```

   Review the plan first. A fresh agent now reads the host’s existing instructions and the loader. No optional catalogue is required.
3. Add these loader Entries:

   ```markdown
   - [Contribution policy to follow](../CONTRIBUTING.md) - #LoadNow
   - [Project purpose, setup and usage](../README.md)
   - [Existing documentation, when the task needs it](docs.md) - #Map
   ```

   Create `.agents/docs.md`:

   ```markdown
   ---
   description: Where to find existing project documentation
   tags: [Map]
   ---

   # Project documentation

   For work that needs project documentation, inspect [docs](../docs/)
   and read the sources relevant to the task.
   ```

   The required set now includes `CONTRIBUTING.md`. README opens when relevant. Documentation work also opens `docs.md` and relevant existing sources, without loading the entire folder.
4. Ask: “Explain this project’s test command. Follow AGENTS.md and name the sources you used.” Check the answer against existing documentation. Selecting documents for reading does not accept every claim they contain.
5. When a new rule or accepted fact needs a home, preview and apply:

   ```sh
   open-forge route init directives --framework --dry-run
   open-forge route init directives --framework
   open-forge route init memory/crystallized --framework --dry-run
   open-forge route init memory/crystallized --framework
   ```

   Agents now also read Directives, Memory and Crystallized catalogues. Individual knowledge records remain selected by relevance unless explicitly required. Record new decisions prospectively rather than reconstructing an invented history.

Open Forge must preserve text outside managed host boundaries, existing documentation locations, native Skill contracts and user ownership. It must never manufacture acceptance, copy existing specifications into competing sources or silently activate omitted categories.

The worthwhile CLI help is a bounded list of candidate documents with proposed entry lines and previewed edits. It should ask users to select sources, without inferring their authority or reorganizing them.

### The introduction

These blocks are proposed release copy. The `minimal` commands depend on the installer change above.

Replacement README introduction and starting section:

````markdown
## Open Forge

Open Forge helps coding agents find and use your project's rules,
documentation and accepted decisions. It connects plain Markdown files through
a short reading procedure and descriptive links.

Start with a document you already have. Add places for rules, accepted
knowledge or resume notes when they solve a problem you have.

### Get started

With Node.js 22.18 or later:

```sh
npm install -g @thelithiumforge/open-forge@beta
cd /path/to/your-project
open-forge install --preset minimal --frontmatter root --dry-run
```

Review the plan, then apply the same selection:

```sh
open-forge install --preset minimal --frontmatter root --automatic
```

Minimal adds the loader, a human guide and marked sections in the agent
instruction files. Existing text outside those sections stays yours. It adds
no optional folders.

Add this line under `## Entries` in `.agents/loader.md`:

```markdown
- [Contribution rules to follow](../CONTRIBUTING.md) - #LoadNow
```

Use a document that exists in your repository. It stays where it is and needs
no frontmatter. `#LoadNow` requires reading it when its parent loads.

Ask your agent:

> Explain how this project runs its tests. Follow AGENTS.md and name the
> sources you used.

Check the answer against your existing instructions.

### Choose how much to use

### Connect existing documents

Keep your README, contribution guide, ADRs and docs where they are. Add entry
lines describing when each source matters.

For an existing C# guide:

```markdown
- [C# conventions](../docs/contributing/csharp.md) - #LoadNow - applies to `**/*.cs`
```

The condition limits reading to matching working files. Without a loading tag,
a document opens when its description fits the task.

### Keep rules and accepted knowledge

Add Directives when a correction should become a rule. Add Crystallized Memory
when accepted knowledge needs a maintained home.

Preview each addition, review it, then repeat the command without `--dry-run`:

```sh
open-forge route init directives --framework --dry-run
open-forge route init memory/crystallized --framework --dry-run
```

You can use either one alone. Existing specifications remain where they are.

### Resume work across sessions

Add Working Memory when unfinished work needs a resume point:

```sh
open-forge route init memory/working --framework --dry-run
```

Review the plan and apply it without `--dry-run`. Its separate saving policy
asks agents to record the goal, current state, next step and open questions
before pausing unfinished work. You can remove that policy and keep the notes.

### Add the rest when useful

Full Core supplies every built-in category and Memory state. Essentials and
Custom remain available. Optional Extensions add methods and starting files.
None supplies an agent runtime or requires you to adopt every other part.

### Customize the files

Each installed catalogue explains its purpose and lists its contents. Add,
replace or remove the parts you use. The loader's Entries list shows what is on.

Removing a loader entry turns that part off while retaining its files.
Accepted project requirements and instructions your host loads independently
still apply. Inside other catalogues, entries follow files, so move or remove
the file and its entry together. Record managed removals so updates respect them.

For a local adjustment that should survive CLI updates, put the change in an
adjacent `{name}.overwrite.md` file. It applies to its base source only.

The CLI is optional. For a manual setup, copy `.agents/loader.md` and
`.agents/README.md` from `src/open-forge/`, then append the shipped managed
`AGENTS.md` block to your existing file. Add the `CLAUDE.md` bridge if your
agent tool needs it. Merge existing `.agents/` content by hand.

The files contain the framework. The CLI gathers required context, maintains
navigation and previews maintenance changes. Your agent can still miss an
instruction, so review its work as usual.

Read [the installed guide](src/open-forge/.agents/README.md), follow the
[installation guide](https://thelithiumforge.github.io/open-forge/docs/getting-started/installation),
or try the [ten-minute guide](https://thelithiumforge.github.io/open-forge/docs/getting-started/ten-minute-guide).
````

Complete `.agents/README.md`:

````markdown
## Open Forge in this project

Open Forge connects an agent to the rules and knowledge this project chooses to use. Start at [`loader.md`](loader.md). Its Entries list shows what is on. You can use one existing document, a few folders or the full framework.

Everything is plain Markdown. The CLI is optional. Agents read the loader before working, but this README is a guide for people and is not a startup read.

### Link an existing document

Add this line under Entries in `loader.md`, using a path that exists:

```markdown
- [Contributor rules to follow](../CONTRIBUTING.md) - #LoadNow
```

The document stays where it is and needs no frontmatter. The entry makes it a required read. Linking does not accept its claims or give permission to edit it.

For rules limited to C# work:

```markdown
- [C# conventions](../docs/contributing/csharp.md) - #LoadNow - applies to `**/*.cs`
```

`#LoadNow` requires reading when the parent is loaded and the file conditions match. Leave it off for a source to open when its description fits the task. Use `#KeepInMind` when the selected context also needs rereading at task start or resume, after context restoration and before handoff or finish.

A loading tag cannot open an unselected parent. For example, selecting a C# catalogue reads its #LoadNow design file. An on-demand Windows catalogue inside it stays closed. Its #LoadNow files become required only after Windows is selected.

### Add only what helps

Directives hold required behavior under `## Instructions`. Tag each rule file #LoadNow and limit it with `applyTo` when appropriate. Crystallized Memory holds accepted knowledge. Working Memory holds resume notes. None requires you to install the other.

Memory states use the Memory catalogue as their parent. You may install just Crystallized, just Working or any useful combination. With the CLI, preview `open-forge route init memory/crystallized --framework --dry-run`, review it, then run the same command without `--dry-run`. Restore an explicitly excluded ancestor first.

Without the CLI, copy only the catalogues you want and list them in their parents. Follow the loader when writing metadata and catalogue rules.

### Turn something off

Remove its line from the loader. Its files remain as notes, and reading them does not reactivate their rules. Accepted project requirements and instructions your host loads independently still apply. Report any enabled part that depends on it before removing the line.

Inside other catalogues, Entries follow files. Delete or move the file and remove its entry together. For CLI-managed updates, record an appropriate `removedCategories`, `removedDirectories` or `removedFiles` exclusion in `open-forge.json` so delivery also respects the removal.

Custom's `--route ...=remove` releases Framework management while keeping existing files routable. Git-ignore controls sharing and leaves local reading available. Neither action turns an existing route off.

### Save unfinished work

Working Memory reads the relevant resume record when work starts or resumes. Its separate `memory/working/saving.md` policy requires an update before unfinished work pauses or changes hands.

To stop automatic saving while keeping records, remove `saving.md` and its entry from `_working.md`. Record its removal for CLI-managed updates. Accepted knowledge still needs to reach its defining source when Memory is enabled.

### Keep a local change through updates

Edit a shipped file directly, or put your change in `{name}.overwrite.md` beside it. The overwrite is read immediately after its base and wins for corresponding content in that source. It does not override other active rules. CLI updates leave the companion alone and can replace a directly edited managed base.

### Names you may meet

- Core means routing, loading, workspace orientation and reusable agent-facing content roles.
- Memory means self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history.
- An Extension is an optional package of routes, capabilities, integrations and support files.

These names and their tags classify content. They grant no authority or loading behavior.

### Use the CLI when it helps

`open-forge context --for <file>` gathers required tagged reads. Repeat `--for` for every known working file, then follow the loader for relevance choices and explicit reading instructions.

Preview `open-forge index --dry-run` after adding files. Run `open-forge index` after reviewing the plan. It maintains Entries inside enabled folders. Existing documents linked directly from the loader keep their own content and metadata. All of this can also be maintained by hand.
````

### v3

This is the Full Core loader template. Install writes only the selected Entries. The rewrite preserves v2’s required reads, conditions, fallback, refresh, authority, activation, metadata and overwrite obligations. Restored obligations are explicit additions from the ledger.

```markdown
## Open Forge Loader

This file tells agents which context this workspace uses and how to read it. It adds to the project's other instructions. Everything works with plain files.

Each line under `## Entries` turns on the context it links to. Anything not listed is off. A catalogue is a Markdown file that lists more context under `## Entries`.

### Axioms

### Read before you work

1. List your working files when a task starts or resumes, and after your context is restored. Include files you will investigate, review, create, change, rename or delete, even if they do not exist yet. Files read only for background do not count.
2. When the `open-forge` CLI is available and allowed, run `open-forge context`. Repeat `--for <file>` for every known working file and read the output. This completes steps 3 and 4. You must still do step 5. If the command is missing, fails or reports a link it cannot follow, do steps 3 to 5 with your own file tools.
3. Check conditions before opening an entry for use. An entry with `applies to` patterns applies only when one working file matches its patterns and every condition on the catalogues above it. Patterns are case-sensitive globs from the workspace root, such as `**/*.cs` or `docs/**`. If working files are unknown, leave these entries pending. Resolve them before touching a matching file.
4. Read every applicable #LoadNow or #KeepInMind entry in listed order. When you read a catalogue, repeat this step for its Entries. These reads are required even when a description looks unrelated. Read each file once per pass.
5. Open other entries when their description fits the task, before the action they describe. Read any file that a file you read requires. Before applying a file found through search or a direct link, read the listed catalogues above it. Files beneath an unlisted entry are background only. Use catalogues to discover context instead of opening ordinary file bodies just to find links or tags.

Start again at step 1 when your working files or goal change. Before handing off or finishing, reread #KeepInMind entries in the context still selected for the task. During work, reread them only when they may have changed.

### Use what you read

- Follow rule files' `## Instructions` within their stated scope. Follow catalogues' `## Axioms` for everything they list. Narrower rules add to broader rules. Report conflicts and instructions you cannot follow, and explain why.
- Follow the user's direction within platform and safety limits. Take routine, reversible steps within the request without asking again. State assumptions that affect the result. Ask before an unresolved choice could significantly change the result, scope, risk, cost, external effects or ability to undo the work.
- Reading, linking, saving or tagging content does not make it accepted or give permission to edit it. Acceptance requires the user's agreement or a source the user named for those facts. Evidence can support a claim without establishing acceptance.
- Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation and at most one important open choice.
- Use the fewest words that preserve meaning, readability and logical progression. Remove filler and repetition. Keep the context, conditions and exceptions needed to understand and apply the result.
- Let each source answer one clear question. Link to related sources instead of repeating their detail. A link changes neither source's authority, scope, loading rules or lifecycle. Put detail in the narrowest source that defines the question. Higher-level catalogues and records keep the summary needed to select that source and link to it.
- Use relative links and precise references to connect local sources. Reuse consistent descriptive tags for shared subjects. Add links and tags only when they help navigation or retrieval.
- Investigate an apparent conflict in accepted current knowledge before changing either source. When accepted direction changes current state, update the source that defines it and preserve useful prior context.
- Selected advice or other non-binding material may specialize broader material of the same kind within its accepted scope.
- #Evergreen marks material that must stay aligned with accepted current state. It grants no authority and adds no loading behavior. Update affected #Evergreen sources you may edit before work depends on them and by closeout at the latest. Batch related updates when safe. Keep each source aligned with what it represents now. Preserve useful prior context where this workspace keeps it. Report needed updates that are blocked or not authorized.

### Change what is on

- Add a line under Entries to turn context on. Link to a catalogue or directly to Markdown anywhere in the workspace, such as `../docs/architecture.md`. The entry carries the description, tags and conditions, so an existing document needs no move or frontmatter.
- Remove a loader entry to turn that part off. Its files stay as plain notes. Reading them for background does not reactivate their rules. Accepted project requirements and instructions the host loads independently remain in effect. Report any listed part that depends on the part being removed first. Nothing unlisted creates a duty. Nothing should restore it unless the user asks.
- Inside other catalogues, Entries follow the files in that folder. To turn one of those entries off, delete or move its file. For CLI-managed content, record the removal as well. The CLI Skill explains how when installed. Otherwise, use CLI help.
- Report a listed file that is missing. An empty Entries list is valid.
- Tag an entry #LoadNow when it must be read whenever its parent loads and its conditions match. Use #KeepInMind when it must also be reread at the refresh points above. A required descendant needs an unbroken #LoadNow chain through its catalogues. Other tags are search labels, with the #Evergreen update duty stated above.
- New files inside `.agents/` start with frontmatter containing `description`, `tags` and optional `applyTo`. Follow `frontmatter` in `.agents/open-forge.json` and the matching form in nearby files. A missing setting means scoped. Native `SKILL.md` keeps its native metadata. A folder's catalogue is named `_{folder-name}.md`. `open-forge index` keeps Entries aligned with the files.
- You may nest catalogues at any depth and place them before, between or after deeper folders. Each narrows everything below it. Each catalogue may list only the parts useful there.
- When writing a catalogue with no local rules, put `- inherited - No local axioms; loaded ancestor axioms remain active.` under `## Axioms`. Never combine this marker with local rules. An existing missing or empty Axioms section also adds no rules.
- To change a shipped file while keeping updates, put the change in `{name}.overwrite.md` beside it. Read it immediately after its base. It wins only for corresponding content in that file and shares its scope. It is not listed or selected separately.

### Entries

- [Rules agents must follow in this workspace](directives/_directives.md) - #LoadNow #Core #Directive
- [Advice for recurring choices, to check before choosing an approach](guidance/_guidance.md) - #Core #Guidance
- [Where to find project knowledge, checked before relying on it](maps/_maps.md) - #Core #Map
- [Notes that help work continue, such as resume points, accepted knowledge, ideas and history](memory/_memory.md) - #LoadNow #Memory
- [Default shapes to check before creating or reviewing structure](patterns/_patterns.md) - #Core #Pattern
- [Capabilities to check before specialized work](skills/_skills.md) - #Core #Skill
- [Starting files to copy when creating something new](templates/_templates.md) - #Core #Template
```

Managed `AGENTS.md` block:

```markdown
## Open Forge

Before starting a task, read `.agents/loader.md` and follow its reading steps. The loader selects this workspace's Open Forge rules and context.

When the `open-forge` CLI is installed and allowed, `open-forge context --for <file>` gathers the required reads. Repeat `--for` for every known working file. Follow the loader for any further reading and for the fallback when the command cannot complete.

**End of Open Forge managed section.**
```

Five representative sentence changes:

| Before | After | Why |
| --- | --- | --- |
| “The `Entries` below are the switch: what is listed is on, and anything not listed is off.” | “Each line under `## Entries` turns on the context it links to. Anything not listed is off.” | Replaces the metaphor with the action. |
| “At the start or resume of a task, and after your context is restored, list your working files.” | “List your working files when a task starts or resumes, and after your context is restored.” | Puts the action first without changing timing. |
| “Read and apply an entry with `applies to` patterns only when the same working file matches its patterns and those of every conditioned catalogue above it.” | “An entry with `applies to` patterns applies only when one working file matches its patterns and every condition on the catalogues above it.” | Removes the noun stack and retains same-file matching. |
| “That covers steps 3 and 4. Still do step 5 yourself.” | “This completes steps 3 and 4. You must still do step 5.” | Makes the remaining obligation explicit. |
| “It is read right after its base and wins where they differ, for that file only.” | “Read it immediately after its base. It wins only for corresponding content in that file and shares its scope.” | Separates sequence from precedence and bounds the replacement. |

Whitespace-separated counts, including headings, syntax and Entries: loader **823 → 1,236**, managed block **56 → 69**, installed README **322 → 702**. The human guide grows because examples and explanations move there. The loader remains substantially shorter than the original 2,052 words.

### Risks and proof

The principal risk is restoring enough prose to obscure the reading procedure. Keep the five steps first and group restored rules by action. The larger human guide should be tested for findability rather than treated as compulsory reading.

Another risk is calling an installation “minimal” while maintenance recreates omitted context. Council 2’s change map records an empty-list refill limitation. Establish whether it remains, then prove that an intentionally empty list survives Index, repeated Install and Update. Also prove that restoring Crystallized does not activate sibling states.

Public documentation still describes older startup behavior, including automatic CLI Skill reading. Update the introduction, installation, first-task, loading, customization and diagram claims together. The records for Tasks 70 and 73 support preservation, concrete selections and explicit restoration. They do not establish that strangers understand the introduction.

The cheapest useful experiment needs three developers unfamiliar with Open Forge and an existing repository each. Give them only the proposed README. Ask them to connect one existing document, add one scoped rule, grow to Working, then disable saving while retaining notes. Run fresh agent sessions without the CLI first, then with it. Observe completion time, requests for help, missed reads, incorrect scope application and whether disabled behavior stays off.

Require all three to complete the first useful setup without intervention. Any mistaken acceptance, overwritten project content or silently restored behavior fails the trial.

This proposal is read-only. No files, builds, test suites or Doctor runs were changed or executed. Git rejected repository access because of ownership checks, so the historical comparison uses the ledger’s real quotations and council records rather than a successfully obtained `origin/develop` diff.

### The one idea to steal

Make the first installation connect one existing document, then let every additional folder earn its place through a problem the user already has.

---

## Contrarian (Proposal B)

/root - GPT-6 - high

### Thesis

Keep v2’s opt-in structure, but reject word reduction as evidence of losslessness. Several cuts removed guarantees about acceptance, maintenance and review, while the empty-list implementation contradicts the advertised off switch. Restore those guarantees where their consumers will encounter them, even if the loader becomes somewhat longer.

### How v2 removed so many rules

The dangerous mistake is treating every successful shortening as removal of redundancy.

| Kind | Two examples | Verdict |
| --- | --- | --- |
| Removed vocabulary | Loader L46 defines `route`. L49 defines `slug`. | Sound to remove from startup. Retain a human reference while public documentation uses these terms. |
| Replaced explicit rules with assumed competence | Loader L28 requires useful links and consistent tags. Maps L23 requires broad rather than member-level navigation. | These are opinions users could rely on. “Models know this” does not preserve them. |
| Narrowed a general rule to one optional category | Loader L27’s source separation survives only in Crystallized. L100’s Evergreen duty also survives there. | Unsound. Existing linked documents need both guarantees without installing Memory. |
| Compressed away a qualification | Patterns L28 loses separate reporting of adaptations. Memory L26 loses the distinction between stated, observed and inferred information. | Unsound. The remaining sentence does not imply the missing qualification. |
| Moved behavior into a separate policy | Memory L24’s selective coordination capture and L25’s durable-outcome capture move into `saving.md`. | Separating automatic capture is sound. The replacement is incomplete because its principal obligation covers unfinished work. |
| Replaced unconditional catalogue reading with task triggers | Loader L146 removes Patterns’ startup tag. Skills L15 removes the CLI guide’s startup read. | Defensible when every consumer still meets an explicit trigger. Loading an empty advice catalogue on unrelated work adds no useful behavior. Mandatory rules and required reads cannot rely on this argument. |

The [change map](.agents/memory/emerging/analysis/opt-in-council/loader-change-map.md) itself identifies missing conflict investigation, blocked-update reporting and refresh boundaries. Those are acknowledged omissions, not redundant wording.

### The lossless ledger

A rule relocated somewhere its consumer never visits is still lost. Below, paths are relative to `src/open-forge/.agents/`, and old line numbers come from the supplied ledger. Quoted text is the exact addition. Retain surviving v2 clauses. Catalogue additions go under `## Axioms` unless another section is named.

**Destination: `loader.md`, `### Use what you read`**

- `loader.md:L21` — **Restore**: “Match explanations and plans to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation and at most one important open choice.”
- `loader.md:L22` — **Restore**: “Use the fewest words that preserve meaning, readability and logical progression. Remove filler and repetition. Keep the context, conditions and exceptions needed to understand and apply the result.”
- `loader.md:L27` — **Restore**: “Let each source answer one clear question. Link related detail instead of repeating it. Links change neither source's authority, scope, loading rules or lifecycle.”
- `loader.md:L32` — **Restore**: “Investigate an apparent conflict between accepted current sources before changing either. When accepted direction changes current state, update its defining source and preserve useful prior context.”
- `loader.md:L40` — **Restore**: “Selected non-binding material may specialize broader material of the same kind within its accepted scope.”
- `loader.md:L100` — **Restore**, in `### Change what is on`: “#Evergreen requires alignment with accepted current state, without granting authority or changing loading.”
- `loader.md:L101` — **Restore**: “Update affected sources you may edit before work depends on the outcome and no later than closeout. Batch related updates when safe.”
- `loader.md:L102` — **Restore**: “Keep sources aligned with what they represent now. Preserve useful context and reasoning in Memory when enabled.” Also: “Report blocked or unauthorized updates.”
- `memory/_memory.md:L27` — **Relocate**: “Update affected sources you may edit before work depends on the outcome and no later than closeout.” Also: “Accepted durable knowledge must reach its defining source, not remain only in chat or a candidate record.”

**Destination: `loader.md`, `### Read before you work`**

- `loader.md:L62` — **Restore**: “Use catalogue entries for discovery rather than opening file bodies merely to find routes or tags.”
- `loader.md:L93` — **Restore**: “Refresh only active branches. Use catalogue entries for discovery rather than opening file bodies merely to find routes or tags.”
- `loader.md:L94` — **Restore**: “Recheck during work only when tagged files in active branches may have changed.”

**Destination: `loader.md`, `### Change what is on`**

- `loader.md:L28` — **Restore**: “When writing sources, connect related material with relative local links, precise references and consistent descriptive tags. Add links and tags only when they help navigation or retrieval.”
- `loader.md:L37` — **Restore**: “For a catalogue without local rules, use `## Axioms` with only `- inherited - No local axioms; loaded ancestor axioms remain active.` Missing or empty sections also add no rules. Never combine this marker with local rules.”
- `loader.md:L57` — **Restore**: “Catalogue folders may nest at any depth, before, between or after other route segments. Each narrows what follows.”
- `loader.md:L58` — **Restore**: “Include only useful parts. You need not mirror another scope or the installed defaults.”

**Destination: `README.md`, `## Terms`**

These definitions need a reference home, not compulsory startup reading.

- `loader.md:L46` — **Relocate**: “A route is a navigable path exposed through catalogues and Entries.”
- `loader.md:L47` — **Relocate**: “A root route is a route listed directly by the loader.”
- `loader.md:L48` — **Relocate**: “A scope is the part of a route that narrows where later content applies.”
- `loader.md:L49` — **Relocate**: “A slug is a concrete folder name in a route path.”
- `loader.md:L95` — **Relocate**: “#Core identifies base routing and loading, workspace orientation, and reusable agent-facing roles.”
- `loader.md:L96` — **Relocate**: “#Memory identifies self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history.”
- `loader.md:L97` — **Relocate**: “#Extension identifies optional packaged routes, capabilities, integrations and support files.”

**Destination: `README.md`, `## Nested reading`**

- `loader.md:L85` — **Relocate**: “A C# catalogue can list a #LoadNow design file and an on-demand Windows catalogue. Selecting C# reads its design rules and leaves Windows closed. A #LoadNow file beneath Windows is read only after Windows is selected.”

**Destination: `maps/_maps.md`**

- `maps/_maps.md:L23` — **Restore**: “Keep Maps broad. Prefer modules, projects, repositories, systems or scopes over members and functions unless finer routing clearly helps.”
- `maps/_maps.md:L24` — **Restore**: “Each workspace chooses its own Map filenames, grouping and nesting depth.”
- `maps/_maps.md:L25` — **Restore**: “Maps do not replace Memory or the destination's current information.”

**Destination: `patterns/_patterns.md`**

- `patterns/_patterns.md:L19` — **Restore**: “Create or update a Pattern only when an accepted shape should guide future related work. Structure alone does not justify promoting one-off work, temporary transitions or unsettled candidates into a Pattern.”
- `patterns/_patterns.md:L25` — **Restore**: “Link related sources for required rules, advice, maps, procedures and starting files, and say which of them bind the shape. Sources that apply the shape should link back to its Pattern.”
- `patterns/_patterns.md:L26` — **Restore**: “Keep required behavior, procedures and substantial choice rationale in their defining sources. Example names and values illustrate the shape unless an applicable requirement fixes them.”
- `patterns/_patterns.md:L28` — **Restore**: “Adapt only within the authority already granted for the work. Reviews report justified adaptations as departures, separately from conformance.”

**Destination: `templates/_templates.md`**

- `templates/_templates.md:L24` — **Restore**: “If the new artifact needs continuing guidance or requirements, link the source that defines them. The Template provides only starting content.”
- `templates/_templates.md:L28` — **Restore**: “State the need and primary question each Template answers in its description and source instructions.”
- `templates/_templates.md:L29` — **Restore**: “Use focused prompts that request necessary content once, in a readable order. Mark optional sections clearly.”
- `templates/_templates.md:L30` — **Restore**: “Treat generic Templates as fallbacks. Add a specialized Template only when it provides meaningfully different starting content.”

**Destination: `memory/_memory.md`**

- `memory/_memory.md:L13` — **Restore**, opening paragraph: “Memory is self-growing. People and agents can add useful records and routed scopes without a fixed structural limit. Unselected branches stay closed. Records need not pass through every state.”
- `memory/_memory.md:L19` — **Restore**: “An accepted record can define current knowledge or decisions. Recording another category's content does not give the record that category's role.”
- `memory/_memory.md:L26` — **Restore**: “Distinguish what was stated, observed or inferred.”
- `memory/_memory.md:L32` — **Restore**: “Add records and routed scopes when their likely future value justifies the cost of finding and reviewing them.”
- `memory/_memory.md:L35` — **Restore**: “Named record categories and Templates are optional. Use an existing state and the narrowest useful scope. A record may live directly under that state.”
- `memory/_memory.md:L36` — **Restore**: “Within the authority granted for the work, add an ordinary record or scoped route when needed. Ask when its placement would change accepted meaning or cross an unresolved authority boundary.”

**Destination: `memory/working/_working.md`**

- `memory/working/_working.md:L19` — **Restore**: “An explicitly accepted temporary choice may remain here when its source, scope and expected expiration are clear.”

**Destination: `memory/emerging/_emerging.md`**

- `memory/emerging/_emerging.md:L17` — **Restore**: “At each applicable #KeepInMind refresh, check visible Entries and save useful candidate material before it is lost.”
- `memory/emerging/_emerging.md:L23` — **Restore**: “Refine material while its validity, use or final destination remains unsettled.”
- `memory/emerging/_emerging.md:L25` — **Restore**: “Revisit repeated or stale material and decide whether to consolidate, promote, archive or prune it. Review may leave a record unchanged.”

**Destination: `memory/archived/_archived.md`**

- `memory/archived/_archived.md:L17` — **Restore**: “Retain, consolidate or transform history according to its future value. Archival need not preserve every detail.”

That accounts for all **34 Dropped and 12 Partly rows** without declaring a behavioral loss acceptable.

The classifications are not sufficient proof. Also repair these misleadingly retained or intentionally changed statements:

- Restore `#KeepInMind` on Emerging’s frontmatter and its Memory entry. Otherwise its restored refresh obligation loses its previous default reach.
- In Memory Axioms add: “Get explicit user direction before creating a new top-level Memory state.”
- In Archived Axioms use: “Delete archived material only under user direction or accepted retention preferences.” Preserve source provenance as well as retirement reasons.
- In Patterns Axioms retain optional and repeatable parts, nesting, fixed names, values and order. Schematic examples must identify omissions.
- In Templates Axioms retain the destination’s required structure and remove package tags that no longer describe it.

### The complexity ladder

The failure would be presenting these as maturity ranks. A repository can remain at the first level indefinitely.

Let **B** mean `AGENTS.md`, `CLAUDE.md`, `.agents/loader.md`, `.agents/README.md`, and the CLI’s settings and lock files.

| Level | Installed tree | Benefit and cost | One step up or down |
| --- | --- | --- | --- |
| 1. Existing sources | B, plus loader links to existing files | Reliable discovery without moving documentation. Costs the loader’s reading contract and maintenance of a few links. | Up: add Directives and Crystallized. Down: remove the managed handoff and retain project documents. |
| 2. Rules and accepted knowledge | Level 1 plus `directives/_directives.md`, `memory/_memory.md`, `memory/crystallized/_crystallized.md`, and useful records | Repeated corrections and accepted knowledge have explicit homes. Costs acceptance and freshness review. | Up: add continuity states. Down: preserve useful content in existing project sources, then disable the optional roots. |
| 3. Continuity | Level 2 plus Working, `saving.md`, Emerging and Archived | Resume unfinished work and distinguish candidates from history. Costs selective capture, refresh and retirement. | Up: add reusable content roles. Down: release omitted states’ management, move their folders outside routed Memory, repair links and remove their entries. |
| 4. Full framework | Level 3 plus Guidance, Maps, Patterns, Skills with its CLI package and references, and Templates | More consistent choices and reusable work. Costs catalogue and example maintenance. | Up: install one justified Extension. Down: remove unnecessary loader entries, preserving files and recording lifecycle omissions separately. |

Brownfield projects should start at **Level 1**. The project already has knowledge and conventions. Adoption should first prove that an agent finds and uses them.

### Ultra-lightweight install

Add **`--preset minimal`**, an explicit opt-in preset. Its failure mode is a beautifully empty installation, so successful onboarding must include one useful existing-source link.

Fresh Minimal writes B and an empty `## Entries`. It installs no category catalogues, Memory, saving policy, CLI Skill or Extensions. It creates no Git-ignore section unless a selected row requests one.

Permit `--route <id>=<add|remove|git-ignore>` with **Minimal or Custom**. Minimal starts with every row removed, then applies overrides. Existing presets retain their current meanings. Existing installations still require `--configure`, and omitted existing files remain routable until explicitly disabled.

The plan should say: “Minimal adds the loader, agent handoffs and a guide for people. No optional folders or automatic saving are enabled.” It must list exact files, managed-block edits, overrides and preserved occupants.

Growth uses existing Configure and Framework Route Init operations. Persist concrete selections, not another runtime mode.

C# changes, under `src/cli/operations/OpenForge.Cli.Operations/Commands/Install/`:

- `Models/Configuration/InstallConfiguration.cs`
- `Shared/Configuration/InstallConfigurationChoices.cs`
- `Shared/Configuration/InstallSetupResolver.cs`
- `Shared/Binding/InstallBindingInputReader.cs`
- `Models/Binding/InstallBindingModels.cs`

Update Rendering’s `InstallSetupQuestions.cs`, `InstallSetupRecapSelector.cs`, `InstallReportSelector.cs`, and corresponding OutputText help. Fix `GeneratedNavigationRegionPlanner.cs`’s empty-membership fallback.

Cost: a modest installation change plus binding, preview, preservation, retry and snapshot coverage. There is **no Markdown grammar change or settings migration**. Keep unqualified installation defaults unchanged until the introduction experiment succeeds.

### Brownfield adoption

The danger is turning adoption into a repository migration. This walkthrough assumes the existing architecture document is `docs/architecture.md`. Substitute its actual path when different. `minimal` below is proposed syntax.

1. Save the current baseline and inspect `git status --short`. Read existing host instructions. Before installation, the agent reads only what that host and those instructions already require.

2. Preview and apply the same selection:

   ```sh
   open-forge install --preset minimal --frontmatter root --dry-run
   open-forge install --preset minimal --frontmatter root --automatic
   ```

   Inspect managed-block changes and every new file. The agent now reads its existing host instructions and the loader. Empty Entries add no optional reads.

3. The maintainer confirms that CONTRIBUTING defines contribution rules and the architecture document defines current structure. Add these lines under loader Entries:

   ```markdown
   - [Contribution rules to follow](../CONTRIBUTING.md) - #LoadNow
   - [Project overview and documentation links](../README.md) - #Project
   - [Current architecture, before changing system structure](../docs/architecture.md) - #Architecture
   ```

   Now every task reads CONTRIBUTING. Relevant tasks select README or architecture. Linking alone does not establish acceptance.

4. Try a read-only task:

   ```sh
   open-forge context --for src/example.cs
   ```

   Use a real working path. Ask a fresh agent to explain test execution and identify its sources. Check that it follows required links that Context does not interpret.

5. Add homes only after a demonstrated need:

   ```sh
   open-forge install --configure --preset custom --route directives=add --route memory/crystallized=add --dry-run
   open-forge install --configure --preset custom --route directives=add --route memory/crystallized=add --automatic
   ```

   The agent additionally reads Directives, Memory and Crystallized catalogues, then applicable records.

Open Forge must never relocate existing documentation, rewrite host text outside its managed block, infer authority from filenames, manufacture historical Decisions, or silently enable excluded capabilities.

Worth building: clearer “stop managing” wording for Custom’s `remove`, and a copyable existing-document Entry example in Minimal’s receipt. Automatic documentation classification is unnecessary.

### The introduction

The failure would be introducing a second product story while old pages still promise different startup behavior. Ship this replacement with synchronized introduction, installation, loading, customization and Extension guidance.

Replacement README introduction and start section:

````markdown
## Open Forge

Open Forge helps AI coding agents find and follow the rules and knowledge your project already has. It uses plain Markdown files and links, with an optional CLI for reading and maintenance.

Start with one existing document. Add places for rules, accepted knowledge or unfinished work when you need them. You choose which parts are enabled, where your documents live and which methods your agents use.

### Get started

With Node.js 22.18 or later, install the CLI and run it from your project:

```sh
npm install -g @thelithiumforge/open-forge@beta
open-forge install --preset minimal --dry-run
open-forge install --preset minimal
```

Review the plan before confirming. Minimal adds an agent handoff, a loader and a short guide. It adds no optional folders or automatic saving. Existing text outside the marked sections in `AGENTS.md` and `CLAUDE.md` stays yours.

Inspect new files as well as `git diff`. For an experiment, use a separate local clone and preserve any uncommitted files you need.

### 1. Use documents you already trust

Under `## Entries` in `.agents/loader.md`, add:

```markdown
- [Contribution rules to follow](../CONTRIBUTING.md) - #LoadNow
- [Architecture to read before changing system structure](../docs/architecture.md) - #Architecture
```

Use paths that exist in your project. A document stays where it is and needs no new metadata.

`#LoadNow` requires reading whenever the parent loads. Without a loading tag, a document opens when its description fits the task. Linking does not make a document authoritative, so identify which existing sources define the rules and facts the agent should follow.

Ask your agent:

> Explain how this project runs its tests. Follow AGENTS.md and name the sources you used.

Check its answer against the project’s instructions. That is a useful first setup.

### 2. Keep repeated corrections and accepted knowledge

When you want dedicated homes for those things, add Directives and Crystallized Memory:

```sh
open-forge install --configure --preset custom --route directives=add --route memory/crystallized=add --dry-run
open-forge install --configure --preset custom --route directives=add --route memory/crystallized=add
```

For example, create `.agents/directives/testing.md` using your configured metadata form:

```markdown
---
description: Check code changes before calling them done
tags: [LoadNow, Directive, Testing]
---

## Testing

### Instructions

- Run the relevant project checks before reporting a code change as complete. Give the command and result, or explain why it could not run.
```

Add its entry manually, or preview and run `open-forge index`. Keep accepted facts in Crystallized only when they need a new home. Link existing specifications instead of copying them.

### 3. Resume work across sessions

Add Working Memory when unfinished work repeatedly loses context:

```sh
open-forge install --configure --preset custom --route memory/working=add --dry-run
open-forge install --configure --preset custom --route memory/working=add
```

Its saving policy asks agents to preserve the goal, current state, next step and open questions before pausing unfinished work. You can remove that policy while keeping the records.

Use `git-ignore` instead of `add` when new Working contents should stay out of ordinary Git commits. The catalogue remains shareable. Already tracked files stay tracked, and local notes need your own backup.

### Choose what helps

Essentials supplies a broader selection. Full Core supplies every built-in category and Memory state. Custom lets you choose each one. Extensions add optional methods and record shapes. None supplies an agent runtime.

You can stop at any size. Removing a loader entry disables that Open Forge branch while preserving its files. It does not cancel independent host instructions or accepted project requirements.

The CLI is optional. For a manual Minimal setup, copy `loader.md` and the guide from `src/open-forge/.agents/`, keep an empty `## Entries`, and append the managed `AGENTS.md` block to your existing file. Add the `CLAUDE.md` bridge if your harness uses it. Merge existing `.agents/` content by hand.

The files ask agents to follow a reading procedure. They cannot guarantee compliance, so review the result.

See [installation](src/docusaurus/docs/getting-started/installation.md), [customization](src/docusaurus/docs/concepts/customizing.md), and the [CLI reference](docs/cli.md).
````

Complete `.agents/README.md`:

````markdown
## Open Forge in this project

Agents enter through `loader.md`. People can use this guide to understand and change the setup.

The loader’s Entries select enabled branches. Existing project instructions remain active. Everything works without the CLI.

### Start with an existing document

Add a line under `## Entries` in `loader.md`:

```markdown
- [C# conventions to follow](../docs/contributing/csharp.md) - #LoadNow - applies to `**/*.cs`
```

The document stays in place and needs no new frontmatter. Use a real path.

`#LoadNow` requires reading when the parent opens. The condition limits loading and application to work involving a matching file. Leave out the tag for reading only when relevant.

Linking provides access. The user or a declared authority establishes which facts and requirements are accepted.

### Add a rule

Enable Directives before adding rules there. Create a file under `directives/` with a description, tags including `LoadNow`, and the rule under `## Instructions`.

Follow the frontmatter form in `open-forge.json`. If the setting is absent, use an `open-forge:` mapping.

Add its entry to `_directives.md`, or preview and run `open-forge index`. Put specialized rules in a narrower catalogue and use file conditions where appropriate.

### Keep useful knowledge

Crystallized Memory holds accepted knowledge. Existing specifications can stay where they are.

Working Memory holds unfinished work. Its separate `saving.md` policy asks agents to update a short resume record before pausing. To stop automatic saving, remove that policy and its entry. Preserve the records you need.

Emerging holds candidates. Archived holds useful history. Neither becomes current authority merely because it was saved.

Install only the states you want. Records need not pass through all four.

### Turn a part off

Remove its loader entry. Its files remain notes, and background reading does not reactivate its rules. Accepted requirements and independently loaded host instructions remain in effect.

Entries inside other catalogues follow their folders. To disable an item there, move its file outside that routed folder or delete it, then update the entry. Check links and preserve useful content.

CLI management is separate. Custom’s `remove` releases management while preserving existing files and routability. Record intended lifecycle omissions too. Preview destructive removal separately.

### Change shipped content

A direct edit can be replaced by Update.

For a local adjustment that survives updates, create `{name}.overwrite.md` beside its base. It loads immediately afterward and wins only for corresponding content in that source. It does not override other active rules and needs its base.

Preview maintenance commands before applying them. Read the installed CLI Skill when present, or the command’s help when absent.

### Nested reading

A C# catalogue can list a #LoadNow design file and an on-demand Windows catalogue. Selecting C# reads its design rules and leaves Windows closed. A #LoadNow file beneath Windows is read only after Windows is selected.

A loading tag cannot open an unselected ancestor.

### Terms

A route is a navigable path exposed through catalogues and Entries.

A root route is a route listed directly by the loader.

A scope is the part of a route that narrows where later content applies.

A slug is a concrete folder name in a route path.

#Core identifies base routing and loading, workspace orientation, and reusable agent-facing roles.

#Memory identifies self-growing Markdown state for active work, coordination, accepted knowledge, candidates and history.

#Extension identifies optional packaged routes, capabilities, integrations and support files.

These labels do not determine loading or CLI ownership.

### Without the CLI

Follow the same reading rules. Maintain metadata, relative links and Entries by hand. Settings and ownership records support CLI maintenance and grant no authority.
````

### v3

The failure would be replacing binding language with friendly suggestions. This draft keeps explicit duties and restores universal guarantees universally. The Entries shown are the Full Core payload selection. Minimal projects the same loader with no optional entries.

Complete `.agents/loader.md`:

```markdown
## Open Forge Loader

This file tells you which Open Forge context this workspace uses and how to read it. It adds to the project's other instructions. Everything works with plain files.

A catalogue is a Markdown file that lists a folder's contents. The Entries below choose the enabled branches. Unlisted branches stay off.

### Axioms

### Read before you work

1. At task start or resume, and after context restoration, list your working files. Include files you will investigate, review, create, change, rename or delete, including planned files and both rename paths. Background reading does not make a file a working file.
2. When the CLI is available and allowed, run `open-forge context` with `--for <file>` for every known working file and read its output. This covers steps 3 and 4. Always complete step 5 yourself. If the command is missing, fails or cannot follow a link, complete steps 3 to 5 with your file tools.
3. Read each applicable #LoadNow or #KeepInMind entry in listed order. Apply step 4 before opening conditioned entries. Repeat inside every catalogue you open. These reads are required even when a description seems unrelated. Read each file once per pass.
4. A conditioned entry loads and applies only when one working file matches its patterns and every conditioned ancestor's patterns. Patterns within one list are alternatives. Globs are case-sensitive and workspace-relative, such as `**/*.cs` and `docs/**`. A leading `!` is literal. Conditions filter selection. They never select a branch or grant edit permission. When paths are unknown, report exposed required entries as pending and resolve them before touching matching files.
5. Open other entries when their descriptions fit the task. Follow explicit required-read links too. Before using a search result, read its enabled ancestor catalogues. Unlisted branches remain background only, even when search or a link reaches them.

Keep separately selected branches as separate chains. Start again when the goal or working files change. Reread active #KeepInMind entries in listed order before handoff or finish. Recheck during work only when tagged files in active branches may have changed. Refresh only active branches. Use catalogue entries for discovery rather than opening file bodies merely to find routes or tags.

### Use what you read

- Follow this loader's Axioms, loaded catalogues' Axioms and rule files' Instructions within their scope. Catalogue Axioms cover listed descendants. Narrower rules add to broader rules and never cancel them. Report conflicts and instructions you cannot follow, with the reason.
- Follow user direction within platform and safety limits. Take routine, reversible steps within the request without asking again. State assumptions that affect the result. Keep unresolved choices contextual and ask before relying on a choice that significantly changes the result, scope, risk, cost, external effects or ability to undo the work.
- Reading, linking, saving, moving or tagging content does not establish acceptance or edit permission. Acceptance comes from the user or an authority they declared for that question. Use declared external authorities for their assigned facts. Validation supplies evidence, not acceptance. Repetition and confidence establish neither.
- Let each source answer one clear question. Link related detail instead of repeating it. Links change neither source's authority, scope, loading rules or lifecycle. Put detail in the narrowest source that defines its question. Catalogues keep enough summary to select it. Use workspace-wide placement only for material that applies throughout the workspace.
- Investigate an apparent conflict between accepted current sources before changing either. When accepted direction changes current state, update its defining source and preserve useful prior context.
- Update affected sources you may edit before work depends on the outcome and no later than closeout. Batch related updates when safe. Keep sources aligned with what they represent now. Preserve useful context and reasoning in Memory when enabled. Accepted durable knowledge must reach its defining source, not remain only in chat or a candidate record. Report blocked or unauthorized updates.
- Match explanations and plans to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation and at most one important open choice.
- Use the fewest words that preserve meaning, readability and logical progression. Remove filler and repetition. Keep the context, conditions and exceptions needed to understand and apply the result.
- Selected non-binding material may specialize broader material of the same kind within its accepted scope.

### Change what is on

- Add an Entries line to enable a catalogue or a Markdown file anywhere in the workspace, such as `../docs/architecture.md`. Existing documents need no move or new frontmatter. Their entry carries the description, tags and conditions.
- Remove a loader entry to disable that branch. Its files remain notes. Background reading does not reactivate its rules. This does not cancel accepted project requirements or instructions the host loads independently. Report dependent enabled parts first. Do not recreate disabled parts unless the user asks.
- Inside other catalogues, Entries follow files in their folder. To disable one of those items, move or delete its file and update the entry. In CLI-managed installations, record file removals through the CLI so updates preserve them. Consult the installed CLI Skill before maintenance commands when present, or the command's help when absent.
- Report a missing listed file. An empty Entries list is valid and maintenance must preserve it.
- Use #LoadNow only when the content is needed whenever its parent loads and missing it costs more than reading it. Narrow specialized content before making it required. A loading tag acts only after its parent opens. Keep an unbroken chain of loading tags for required startup reads. Use #KeepInMind only for content that needs rereading, not as an importance label.
- #Contextual marks unaccepted context. #CurrentTruth identifies accepted current state within its scope. Neither establishes acceptance. #Evergreen requires alignment with accepted current state, without granting authority or changing loading. Other tags are search labels. Only #LoadNow and #KeepInMind change CLI loading.
- When writing sources, connect related material with relative local links, precise references and consistent descriptive tags. Add links and tags only when they help navigation or retrieval.
- New routed files inside `.agents/` need frontmatter with `description`, `tags` without `#`, and optional `applyTo`. Follow `frontmatter` in `.agents/open-forge.json`. Use scoped metadata when that setting is absent. Native `SKILL.md` retains its native format. Optional `responsibility` states what a file defines and grants no authority.
- A folder has one catalogue, named `_{folder-name}.md`. Existing `index.md`, `_index.md`, `references.md` and `_references.md` remain valid. Catalogue folders may nest at any depth, before, between or after other route segments. Each narrows what follows. Include only useful parts. You need not mirror another scope or the installed defaults.
- For a catalogue without local rules, use `## Axioms` with only `- inherited - No local axioms; loaded ancestor axioms remain active.` Missing or empty sections also add no rules. Never combine this marker with local rules.
- Maintain Entries when files or metadata change, manually or with `open-forge index`. `applyTo` accepts a list or comma-separated string. Root and scoped declarations must agree when both occur.
- Customize shipped content by editing, replacing or removing its source. To retain a local change through updates, put it in `{name}.overwrite.md` beside the base. Read it immediately after the base with the same scope and loading behavior. It wins only for corresponding content in that file, without overriding other active rules. It needs its base and is not listed separately.

### Entries

- [Rules agents must follow in this workspace](directives/_directives.md) - #LoadNow #Core #Directive
- [Advice to check before choosing an approach](guidance/_guidance.md) - #Core #Guidance
- [Sources to check before relying on project knowledge](maps/_maps.md) - #Core #Map
- [Resume records, accepted knowledge, candidates and history](memory/_memory.md) - #LoadNow #Memory
- [Default shapes to check before creating, changing or reviewing structure](patterns/_patterns.md) - #Core #Pattern
- [Capabilities to check before specialized work](skills/_skills.md) - #Core #Skill
- [Starting files to check when copy-ready content would help](templates/_templates.md) - #Core #Template
```

Complete managed `AGENTS.md` block:

```markdown
## Open Forge

Open Forge adds the context selected by this workspace to its existing instructions.

Before starting or resuming work, read `.agents/loader.md` and follow its reading steps. When available and allowed, `open-forge context --for <file>` gathers the tagged required reads for each working file. The loader explains the remaining reads and the fallback without the CLI.

**End of Open Forge managed section.**
```

| Before | After | Why |
| --- | --- | --- |
| “The `Entries` below are the switch: what is listed is on, and anything not listed is off.” | “The Entries below choose the enabled branches. Unlisted branches stay off.” | Names the scope of activation without suggesting other project instructions disappear. |
| “Read and apply an entry with `applies to` patterns only when the same working file matches its patterns and those of every conditioned catalogue above it.” | “A conditioned entry loads and applies only when one working file matches its patterns and every conditioned ancestor's patterns.” | Makes pre-load filtering and the single matching file explicit. |
| “Follow the rules you read within their stated scope: the `## Instructions` of rule files and the `## Axioms` of catalogues, which cover everything a catalogue lists.” | “Follow this loader's Axioms, loaded catalogues' Axioms and rule files' Instructions within their scope.” | Separates obligation from the following inheritance explanation. |
| “Nothing that is not listed creates a duty, and nothing should bring it back unless the user asks.” | “Do not recreate disabled parts unless the user asks.” | States the actual actor and action, alongside explicit exceptions for independent authority. |
| “New files inside `.agents/` start with frontmatter: `description`, `tags`, and optional `applyTo`, in the same form as the files around them.” | “Follow `frontmatter` in `.agents/open-forge.json`. Use scoped metadata when that setting is absent.” | Replaces an unreliable visual inference with the existing configuration contract. |

Whitespace-delimited counts, including headings and Entries: **loader 1,317 words**, against v2’s **823**. Managed block **63 words**, against **56**. The increase pays for restored universal obligations. Category-specific restorations remain outside the loader.

### Risks and proof

The largest risk is claiming compatibility from this ledger alone. Rows marked Kept also conceal weakened details, and category loading changes need consumer-level justification.

Before release, cheaply pin these behaviors through focused fixtures:

- Empty and partially disabled loader lists survive Index and Update.
- Outside documents load without frontmatter and remain unmodified.
- One working file satisfies the complete inherited condition chain.
- Search cannot reactivate disabled rules.
- Removing saving does not remove accepted-source maintenance duties.
- Overwrites preserve their base’s scope and affect only corresponding content.
- Existing host prefixes, suffixes and documentation bytes survive installation.

The [source](src/cli/framework/OpenForge.Cli.Framework/Framework/GeneratedNavigation/GeneratedNavigationRegionPlanner.cs:147) confirms that an empty parsed destination list currently returns structural roots. That must be fixed before Minimal ships. Startup estimates also omit linked documents outside `.agents/`, as the change map acknowledges, so advertised costs must state that limit.

For comprehension, recruit three unfamiliar developers for twenty minutes each. Without coaching, have them link an existing rule, explain what loads, add one capability and disable it. Require at least two to finish, with no mistaken authority promotion or disabled behavior returning. Compare against plain `AGENTS.md` on equivalent tasks. If the extra machinery does not help, keep the smaller approach recommended.

This review made no file changes and ran no builds, Doctor or test suites. Git comparison was blocked by repository ownership checks, so before-text evidence comes from the supplied ledger and Council 2 records.

### The one idea to steal

Make the first installation prove that one existing rule reaches a fresh agent before asking the project to adopt another folder.
