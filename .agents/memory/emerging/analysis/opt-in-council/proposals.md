---
open-forge:
  description: Sanitized Stage 1 proposals from the eight opt-in council members, each with an audience verdict and a complete loader
  tags: [Memory, Analysis, Contextual, Evidence, Framework, Audience]
---

# Opt-In Council Proposals

Stage 1 output, unchanged except for removed local paths and runtime labels. Each member wrote independently from the same brief and the first council record. Letters are the anonymous labels used in peer review. The [record](record.md) holds the synthesis.

## Audience Strategist (Proposal G)

#### Thesis

Open Forge should help maintainers make existing project knowledge usable by the next agent session. Its promise should be: **“Your agents find the rules and decisions you already maintain, without requiring you to reorganize your project.”** Make routing the default, then let users deliberately add memory, content conventions, and methods.

#### Who it is for

The “hobby without an audience” critique identifies a positioning problem, but the repository contains a coherent underlying need. [Vision](.agents/memory/crystallized/documents/vision.md) describes users repeatedly teaching settled direction to new sessions. [Highlights](src/docusaurus/docs/highlights.md) supplies concrete examples involving existing codebases, ADRs, monorepos, and handoffs.

The problem is that these buyers encounter a general framework before seeing their particular problem solved.

| Audience | What they want | Appropriate offer |
|---|---|---|
| **Primary: maintainers of established repositories using coding agents** | Stop repeating constraints without migrating existing documentation | Routes to existing sources, explicit required reads, maintainable links |
| Teams maintaining several services or languages | Keep specialized rules applicable to the right changes | Scoped catalogues and file conditions |
| Solo developers returning across sessions | Resume work and preserve accepted reasoning | Optional continuity and decision records |
| People coordinating substantial agent workflows | Consistent delegation, review evidence, and integration | Optional workflow and coordination packs |

These are audience hypotheses supported by repository examples, not demonstrated market demand.

The maintainer’s [findings log](.agents/memory/emerging/authors-findings/open-forge-findings-log.md) explains the need for recoverability, deletion, and avoiding duplicated knowledge. Those needs generalize. Exact task IDs, prescribed progress formats, orchestration ledgers, council budgets, and detailed review provenance serve a more specialized operation. They appear in local [orchestration](.agents/directives/hierarchical-orchestration.md) and [review](.agents/directives/review-evidence.md) rules. They should remain optional recipes and should not define the introductory example.

The competitive position should be precise:

| Neighbor | Implication for Open Forge |
|---|---|
| [BMAD](https://docs.bmad-method.org/), [GitHub Spec Kit](https://github.com/github/spec-kit), [Agent OS](https://buildermethods.com/agent-os/v2/standards) | They already offer methods, specifications, or standards. Competing through another comprehensive development system weakens Open Forge’s distinction. BMAD also offers small-change paths, so file-count comparisons alone are unconvincing. |
| [Cline Memory Bank](https://docs.cline.bot/best-practices/memory-bank) | Its published instructions require reading the whole bank at task start. Selective retrieval and explicit acceptance boundaries offer Open Forge a useful distinction. |
| [Kiro steering](https://kiro.dev/docs/steering/), [Cursor rules](https://cursor.com/docs/rules), [Claude Code memory](https://code.claude.com/docs/en/memory) | Native products already support persistent instructions and selective context. Open Forge must earn its extra files through shared knowledge maintenance across tools. |
| [Plain `AGENTS.md`](https://agents.md/) | This is sufficient for many small projects. Open Forge earns adoption when linked knowledge becomes difficult to find or maintain. |

The README’s first screen should name the primary audience, show an existing document becoming reachable, and demonstrate reuse in a fresh session. ACE terminology, the complete taxonomy, and coordination examples can follow later.

#### Why it feels complicated and controlling

1. **The ambition exceeds the initial buying problem.** Vision says, “Development is a proving ground, not a boundary.” Supporting many disciplines is reasonable, but this leaves newcomers without a clear first use.  
   File: `.agents/memory/crystallized/documents/vision.md`

2. **Required rules can remain undiscovered.** “These tags act only through loaded parents. Neither activates an unselected ancestor or scope.” A required child still depends on a relevance judgment above it.  
   File: `src/open-forge/.agents/loader.md`

3. **An optional accelerator becomes startup curriculum.** “When `open-forge-cli/SKILL.md` is present, read it when this entrypoint loads so command options and context-loading advice are available from the start.”  
   File: `src/open-forge/.agents/skills/_skills.md`

4. **The memory taxonomy governs ordinary organization.** “Get explicit user direction before creating a new top-level Memory state.” A useful convention becomes an organizational permission boundary.  
   File: `src/open-forge/.agents/memory/_memory.md`

5. **Reusable advice acquires review ceremony.** “A justified adaptation is still a departure, so a review reports it separately from conformance.” This is valuable for some teams, but excessive as a general expectation.  
   File: `src/open-forge/.agents/patterns/_patterns.md`

6. **Direct customization is temporary under updates.** The customization table says, “Replaces your edited copy with the current shipped version and reports it.” Users must learn companions to preserve an ordinary edit.  
   File: `src/docusaurus/docs/concepts/customizing.md`

7. **The recommended selection misses the clearest adoption path.** “Essentials omits Guidance, Maps, Templates, and Archived Memory.” Yet the existing-project guide recommends beginning with a Map.  
   File: `src/docusaurus/docs/getting-started/installation.md`

#### What to keep from Council 1

- **Keep the procedure-first loader and shorter entrypoints.** They reduce interpretation before work begins.
- **Keep unbroken loading chains.** Required startup discovery should be inspectable.
- **Keep same-file condition matching and pending unknown paths.** Preserve the CLI’s actual applicability semantics.
- **Change required Directives and Memory defaults.** They become deliberate selections.
- **Keep Guidance, Patterns, Skills, and Templates on demand.** Make Maps the small default navigation surface.
- **Keep the CLI Skill out of startup.** Put a usable context command directly beside the reading procedure.
- **Change tag-definition removal.** Move component-specific meanings into those components rather than silently erasing their obligations.
- **Change the refresh simplification.** Retain start, resume, restoration, and closeout refresh points for selected `#KeepInMind` sources.
- **Keep inherited rules and acceptance distinctions.** Scope depth and saved notes must not manufacture authority.
- **Keep blended documentation and explainable context.** Existing sources should participate directly.
- **Defer `load.d/` as an alternative experiment.** A second loading model adds migration before demonstrating user value.
- **Defer checked replacements.** Preserve current companion semantics and first make ordinary edits survive updates.
- **Keep trace-based experiments.** Word reduction alone does not prove correct behavior.
- **Drop the universal response recipe from the kernel.** Communication preferences belong in selected project rules.

#### The tap-in model

The irreducible kernel is a host handoff and a readable routing procedure. It defines required reading, conditions, scope, and source boundaries. It requires no memory store or development method.

The recommended default adds one small index. Every further component earns its installation through a concrete need.

| Opinion | How to reject it | Value when retained |
|---|---|---|
| Six content roles | Keep documents in existing folders and route directly to them | Distinguishes mandatory instructions, advice, shapes, and starters |
| Four Memory states | Omit Memory entirely | Separates temporary, candidate, accepted, and historical material |
| Automatic continuity capture | Omit its component or remove that obligation at its source | Reduces lost work across sessions |
| Pattern conformance reporting | Edit the Pattern component’s rules | Makes deliberate structural consistency reviewable |
| Workflow and coordination methods | Leave their packs unselected | Supplies repeatable methods for larger operations |
| CLI assistance | Follow and edit the same files manually | Automates retrieval and maintenance |

Disabling a component removes its root entry while retaining user content. A missing target that remains linked is an error. Disabled components stay disabled through Index and Update.

Editing a component’s rules must also be supported. Framework and Extension updates should preserve edited files and offer a diff against the new source. Existing overwrite companions remain supported.

This changes default composition and update behavior. No identity principle needs reversal. Broad applicability remains possible through the same files and extensions.

Smallest setup, with an empty `Entries` section:

```text
AGENTS.md
.agents/
  loader.md
```

Fullest built-in setup, before arbitrary Extension additions:

```text
AGENTS.md
CLAUDE.md
.agents/
  loader.md
  directives/_directives.md
  guidance/_guidance.md
  maps/_maps.md
  patterns/_patterns.md
  templates/_templates.md
  skills/
    _skills.md
    open-forge-cli/
      SKILL.md
      references/
        common.md
        discovery.md
        packages.md
        routes.md
  memory/
    _memory.md
    working/_working.md
    emerging/_emerging.md
    crystallized/_crystallized.md
    archived/_archived.md
```

Optional Extensions add their declared files beneath these routes. Existing `docs/`, ADRs, and source-adjacent notes remain at their current paths. CLI-managed installations additionally maintain the two JSON files listed below.

#### Behavior without a harness

The model still interprets the request, identifies working files, selects relevant optional knowledge, and notices conflicts. Markdown cannot reliably automate those judgments.

Structure handles required-read discovery, ordering, applicability conditions, refresh points, and component activation. Authors put required sources behind complete tagged chains. Descriptions explain optional selection triggers.

The CLI computes those chains, checks conditions, explains missing and pending sources, and assists with navigation maintenance. It does not establish acceptance or guarantee compliance.

Make the agent reach for it at the moment it would otherwise perform several reads. Give the exact `context` command in the loader, allow one availability check, and advertise a native Skill when the host supports discovery. Avoid requiring a long usage guide before the first useful command. Measure invocation rates rather than promising that models will always comply.

Without the executable, the same procedure and metadata remain readable. Plain-file behavior is the reference contract.

#### The new loader

This is the complete fresh-default loader for the proposed release. Section 9’s CLI changes are required for parity. Existing workspaces retain their selected root entries.

Whitespace counts, including headings and Markdown syntax: **575 words for the loader**, **29 words for the managed `AGENTS.md` block**.

```markdown
# Open Forge Loader

Use this file to find the context for your task. Keep project knowledge where it already belongs.

## Axioms

### Read before affected work

1. At task start, resume, or context restoration, read entries tagged #LoadNow or #KeepInMind under `Entries`, in order. Repeat inside every entrypoint you load. Read each file once per pass, followed immediately by its `{name}.overwrite.md` companion when present.
2. A condition after `applies to` filters an entry. Read it only when its conditions match. Working files are paths you investigate, review, create, change, rename, or delete, including planned paths. Background reading does not count. One working file must match a pattern in every condition along the entry's chain. Patterns within one condition are alternatives. Apply conditioned rules only to matching files.
3. Patterns are case-sensitive globs from the workspace root. `**/*.cs` matches C# files at any depth and `docs/**` matches that folder's contents. Unknown working files leave conditioned required reads pending. Resolve them before affected work. Recheck when the task or working files change.
4. Other entries are on demand unless a loaded instruction requires them. Select by their descriptions and the task. Read an entrypoint before its children. For routed material found through search or a direct link, read its ancestor entrypoints and their required context before using it.
5. Reread active #KeepInMind entries before handoff or completion, and when they may have changed.

### Use what you read

- Follow loaded entrypoints' `## Axioms` and applicable required instructions. Ancestor rules remain active within their scope. Report unresolved conflicts. User direction and platform constraints retain their authority.
- Each source keeps its stated purpose, scope, and authority. A link or tag does not establish acceptance. Drafts, observations, and history remain distinct from accepted decisions and current requirements.
- Linking a source grants neither permission to edit it nor ownership to Open Forge. Use existing project sources for their subjects.

### Maintain only what is enabled

- This file's Entries expose the enabled roots. Omitted components create no duties. Do not recreate them unless asked. Report missing linked files rather than treating them as disabled.
- An entrypoint is `_{folder-name}.md`. Existing `index.md`, `_index.md`, `references.md`, and `_references.md` also work. Keep descriptions, links, tags, and conditions under `## Entries` accurate.
- Automatic startup reading needs an unbroken chain of #LoadNow or #KeepInMind entries from here. Put specialized conditions on the narrowest applicable branch. Other tags are labels whose meaning comes from the selected component.
- Entries may link to files anywhere inside the workspace. Existing documents need no relocation or new frontmatter. Their authored Entries supply routing metadata. For routed files inside `.agents/`, use `description`, `tags`, and optional `applyTo` in frontmatter, at the root or under `open-forge:`. Follow the workspace's chosen form and mirror the metadata in Entries.
- Edit component rules at their source to reject an opinion. An overwrite changes only corresponding content in its base, never unrelated sources. Keep authored links when updating. Record lifecycle exclusions if using CLI-managed updates.

### Optional CLI

When available and permitted, prefer `open-forge context --for <path>` to separate startup reads. Repeat `--for` for every working file. Omit it while paths are unknown. Read the returned sources and any further explicit required reads. Select relevant on-demand sources with `context <source>`. If unavailable, follow the same Entries yourself. Use command help or the installed CLI Skill for maintenance.

## Entries

- [Project sources and when to read them](maps/_maps.md) - #LoadNow #Core #Map
```

Managed `AGENTS.md` block:

```markdown
# Open Forge

Before working, read `.agents/loader.md` and follow its reading steps.
Use the project's existing sources. Add only components the user chooses.

**End of Open Forge managed section.**
```

#### The rest of the default install

The default supplies exactly three Markdown files:

- `AGENTS.md` adds the managed handoff while preserving existing instructions.
- `.agents/loader.md` defines reading and source boundaries.
- `.agents/maps/_maps.md` provides a small index for existing project sources.

A CLI installation additionally creates:

- `.agents/open-forge.json` stores maintenance settings and exclusions.
- `.agents/open-forge.lock.json` records managed files and navigation.

The default adds no ignore rules. `CLAUDE.md` is an optional host bridge.

Complete default entrypoint:

```markdown
---
description: Existing project sources and when to read them
tags: [LoadNow, Core, Map]
---

# Maps

Link to useful project sources where they already live.

## Axioms

- Read entries according to their tags and conditions.
- Each destination keeps its existing purpose and authority.

## Entries

- none - No entries - #Empty
```

The first useful customization is one real link, added only when its destination exists:

```markdown
- [Contribution requirements before changing code](../../CONTRIBUTING.md) - #LoadNow
```

Replace the recommended Essentials selection with **Existing project**, containing these three files. Keep Full Core and Custom available. Present optional choices through needs such as “resume unfinished work” and “preserve decisions”, with category details underneath.

Separate “disable this component” from “stop managing its files”. Today’s Custom Remove preserves routing, so it cannot represent both actions.

Offer the same small payload as a manual download. Maps becomes a direct project index. Memory, taxonomy conventions, and methods remain optional. Consider splitting lightweight decision capture from Planning so preserving one choice does not require adopting task workflows.

#### Cost and migration

This requires substantive work beyond replacing Markdown.

| Area | Required change |
|---|---|
| CLI source resolution | Accept contained workspace links using `..`, support ordinary documentation targets, reject escapes, cycles, and ambiguous catalogue ancestry |
| Metadata | Read authored entry metadata for existing external-to-`.agents/` documents without requiring their frontmatter to change |
| Index | Preserve authored root membership and outside-document links, refresh supported metadata without re-enabling disabled components |
| Lifecycle | Distinguish disabling from detaching management, preserve edited Framework and Extension files, offer source diffs without semantic merging |
| Installation | Add the smaller preset and manual payload, retain current selections on upgrades |
| Packaging | Review prerequisite handling and any split of Decisions from Planning |
| Tests | Update payload snapshots and Install, Update, Index, Context, ownership, and removal fixtures |
| Documentation | Rewrite onboarding around existing sources, update Maps, loading, customization, and maintenance contracts |

Entrypoint filenames, `## Entries`, `## Axioms`, generated entry syntax, loading tags, and same-file condition matching remain.

Outside documents stay read-only to the CLI initially. Linking them does not establish installer ownership.

For installed workspaces, migration must preview current required reads and proposed required reads. Preserve selected categories, local rules, companions, and exclusions. Do not automatically shrink existing installations or relocate records.

Move retired global obligations into their corresponding selected components where needed to preserve existing behavior. Make deliberate reductions reviewable. Council 1’s draft remains evidence, not accepted authority.

#### Risks and proof

The smaller default could look empty, hide useful capabilities, or provide too little advantage over `AGENTS.md`. Edited-file preservation could leave workspaces on older rules. Direct documentation routes introduce ancestry and metadata ambiguity.

The cheapest useful experiment needs no CLI implementation. Recruit six people outside this repository, covering established repositories, mixed-language teams, and solo projects. Give each the three-file payload and one task involving an existing documented constraint.

Observe whether they can, within five minutes:

1. Link the relevant source without moving it.
2. Complete the task in a fresh session using that source.
3. Disable an unwanted convention without breaking unrelated reading.

Compare against their existing instructions and Council 1’s draft. Record actual reads, missed constraints, irrelevant reads, setup time, and assistance needed. Test CLI parity later against the same expected read sets.

Proceed only if at least five participants finish onboarding unaided, required-read accuracy does not regress, and disabling works for everyone. If plain `AGENTS.md` performs equally well with less maintenance, narrow the target further.

#### The one idea to steal

Make the first success an existing project document being used correctly in a fresh agent session.

## Minimalist Adopter (Proposal D)

#### Thesis

Open Forge should let a project connect one useful source without adopting its categories, Memory model or collaboration style. Its kernel should be a readable reading list, file conditions and an optional CLI that interprets the same list. Everything else should be an independently removable choice.

#### Who it is for

The critique identifies an unfocused entry point more than an absence of users. [Highlights](src/docusaurus/docs/highlights.md) already names existing-codebase contributors, monorepo developers, people keeping ADRs and people switching sessions. Prioritize four concrete users:

- An established-project developer wants matching conventions loaded before changes, while keeping existing `AGENTS.md`.
- A developer working across sessions wants the current state, evidence and next step preserved in one note.
- A team maintaining docs and ADRs wants agents to find those sources without duplicating them.
- A contributor using several agents wants `context` to assemble the same project context for each.

Together these form a broad potential audience because adoption fits existing projects and independent workflows. The repository supplies evidence of these jobs, while actual market size remains unmeasured.

The maintainer’s [findings log](.agents/memory/emerging/authors-findings/open-forge-findings-log.md) describes changing one concept across “30 or 40 documents”. The four-state tree, typed knowledge roles, inherited authority and managed overwrite system especially suit that heavily documented workspace. Session continuity and scoped rules have wider usefulness.

#### Why it feels complicated and controlling

1. **The product boundary bundles opinions.** “Yes. This is Open Forge.” in [src/docusaurus/docs/intro.md](src/docusaurus/docs/intro.md) describes the Framework including both Core roles and Memory. Independent benefits appear downstream of accepting that model.

2. **Routing makes users remodel their files.** “Every folder in a route chain has exactly one recognized entrypoint.” and “Routed Markdown files start with YAML frontmatter.” in [src/open-forge/.agents/loader.md](src/open-forge/.agents/loader.md) add naming and metadata work before a link becomes useful.

3. **Required discovery depends on judgment.** “These tags act only through loaded parents. Neither activates an unselected ancestor or scope.” in [src/open-forge/.agents/loader.md](src/open-forge/.agents/loader.md) makes selecting the right parent a prerequisite for seeing its mandatory rules.

4. **The kernel prescribes conversation.** “Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.” in [src/open-forge/.agents/loader.md](src/open-forge/.agents/loader.md) competes with an adopter’s existing collaboration preferences.

5. **Remembering imports governance.** “Get explicit user direction before creating a new top-level Memory state.” in [src/open-forge/.agents/memory/_memory.md](src/open-forge/.agents/memory/_memory.md) turns note placement into a Framework boundary. States are not compulsory stages today, but shared lifecycle rules still arrive with Memory.

6. **Optional tooling adds compulsory reading.** “When `open-forge-cli/SKILL.md` is present, read it when this entrypoint loads so command options and context-loading advice are available from the start.” in [src/open-forge/.agents/skills/_skills.md](src/open-forge/.agents/skills/_skills.md) imposes startup cost even without using commands.

7. **Editing requires learning update policy.** “Replaces your edited copy with the current shipped version and reports it.” in [src/docusaurus/docs/concepts/customizing.md](src/docusaurus/docs/concepts/customizing.md) explains ordinary Update. Durable customization therefore requires companions or releasing management.

#### What to keep from Council 1

- **Keep** the reading procedure first and move management vocabulary out of startup.
- **Keep** unbroken required chains, same-working-file condition matching and pending unknown paths.
- **Change** mandatory Directives, Memory, Working and Crystallized defaults to optional selections.
- **Keep** Guidance, Maps, Patterns, Skills and Templates on demand.
- **Keep** CLI guidance off startup and permit `context` without reading the full Skill.
- **Keep** two loading tags, explicit refresh points and the separation of recording from acceptance.
- **Drop** the universal response recipe, including its shortened replacement.
- **Advance** Step 2’s workspace links and read explanations, adding direct ordinary-file targets without surrounding entrypoints.
- **Keep** existing catalogue inheritance for users choosing it, with one loading ancestry per source.
- **Drop** Step 3’s `load.d/` mechanism. The loader’s existing `Entries` is enough for a small explicit list.
- **Defer** checked replacements and preserve today’s partial overwrite behavior.
- **Keep** trace-based experiments and the distinction between a missing configured source and a deliberately empty list.

#### The tap-in model

The irreducible kernel is `.agents/loader.md` plus a pointer from the project’s existing host instructions. Command-only use needs just the loader. It adds no saving, conversation, directory-taxonomy or workflow policy.

| Take | What current adoption involves | Proposed adoption |
| --- | --- | --- |
| File-scoped rules only | Custom can omit other categories, but Directives still introduces its entrypoint, metadata, `Instructions` and loading chains | Link an existing convention file directly with `#LoadNow` and its `applies to` glob |
| Resumable notes only | Working alone is already selectable, but brings the Memory parent and shared lifecycle rules. Named Checkpoints additionally come from Planning | Link one note wherever the user keeps it. Its own instructions define when to update it |
| Existing docs and ADRs only | Maps supplies pointers, with another entrypoint and metadata layer | Link existing docs directly. Add an index only when useful |
| CLI context only | Context operates through the existing Framework routing contract | Read the thin loader without requiring Install, ownership records, categories or target-file rewrites |

A resumable note can say: “When resuming this work, verify the recorded state against current files. Before pausing, update the goal, state, evidence, next step and blockers.” Mark its entry `#KeepInMind` when those refreshes are wanted. It needs no other Memory state or Planning dependency.

Independent opinions get independent entries. Remove an entry to disable its contribution, leaving files available to people. Check incoming required links before removing a grouped option. Neither Index nor Update may recreate user-owned activation entries.

To reject part of a retained managed file, use its existing overwrite companion or link a user-owned replacement. Removing the full Memory option does not remove a separately linked resume note. Existing host-native instructions continue through their own mechanism.

Each richer choice retains a benefit: categories distinguish rules from advice, Memory states distinguish trust and lifetime, Maps curate knowledge, Patterns support consistency, Templates save authoring, and managed packs plus overwrites support upstream maintenance.

This explicitly reconsiders Product Direction’s rejection of “An empty routing library” in [.agents/memory/crystallized/decisions/product/product-direction.md](.agents/memory/crystallized/decisions/product/product-direction.md). A new install should connect one useful source immediately. A deliberately empty list remains a valid deactivated configuration.

Smallest useful setup, with only two files touched:

```text
AGENTS.md                 existing, append the pointer
.agents/loader.md         new, links ../docs/csharp.md
docs/csharp.md            existing, unchanged
```

Full bundled setup, selecting every Core and Memory option plus CLI guidance. Existing docs remain outside it. Extensions can add further files.

```text
AGENTS.md
CLAUDE.md
.agents/
  loader.md
  directives/_directives.md
  guidance/_guidance.md
  maps/_maps.md
  patterns/_patterns.md
  templates/_templates.md
  skills/
    _skills.md
    open-forge-cli/
      SKILL.md
      references/
        common.md
        discovery.md
        packages.md
        routes.md
  memory/
    _memory.md
    working/_working.md
    emerging/_emerging.md
    crystallized/_crystallized.md
    archived/_archived.md
docs/                     existing project sources
```

CLI-managed installations additionally contain the two maintenance JSON files listed below.

#### Behavior without a harness

The model still identifies working files, judges optional relevance, interprets prose, detects substantive conflicts and decides what deserves saving when saving is enabled. Structure determines mandatory reachability, file conditions, read order and explicit refreshes. Required rules should have direct entries or unbroken tagged chains.

The CLI batches bodies, evaluates conditions, explains selection paths, and reports pending and missing sources. It grants no authority. Plain readers follow identical entries and conditions manually. Required reads hidden in prose remain obligations in both paths, with an authoring recommendation to expose them as entries.

Give the model a concrete action at the loading trigger: `open-forge context` with every known `--for` path. Put that instruction in the loader itself. A native Skill can reinforce it when installed, and the installation receipt can show the first command. Complete, useful output must make this cheaper than separate reads. Measure actual invocations because Markdown cannot force tool use.

#### The new loader

This replacement requires the linked-source CLI changes below before being shipped with the CLI. Counts use whitespace-separated words, including Markdown markers: loader **403**, managed host block **21**, combined **424**. During adoption, replace the empty entry with the user’s chosen link.

```markdown
# Open Forge Loader

This file adds the context listed below to your existing project instructions.

## Axioms

### Read

1. At task start or resume, and after context restoration, identify the files you will investigate, review, create, change, rename or delete. Include planned paths. Background reading does not count.
2. Read applicable #LoadNow and #KeepInMind entries in listed order, then repeat inside each loaded entrypoint. Read each source once per pass. Other entries are on demand, selected by their descriptions or the user's request.
3. An `applies to` list contains alternative case-sensitive globs from the workspace root. `**/*.cs` matches C# files at any depth. `docs/**` matches that subtree. A leading `!` is literal. One working file must match every condition along the selected chain. Apply conditioned rules only to files matching that chain.
4. While working paths are unknown, leave conditioned required reads pending. Resolve them before affected work. Recheck when the goal or working paths change.
5. Reread active #KeepInMind sources before handoff or finish, and when they change. Follow explicit required-read instructions in loaded sources too. Report missing required sources instead of treating them as disabled.

### Use

Follow project rules within their stated scopes. Entrypoint Axioms apply to selected descendants. Narrower rules add to ancestor rules. When search reaches a routed source, read its entrypoint ancestry before applying it. Report unresolved conflicts.

Reading a link changes neither authority, acceptance nor edit permission. A saved note is not evidence of acceptance.

Read an adjacent `{name}.overwrite.md` immediately after its base. It wins only for corresponding content in that source.

### Maintain

Links are relative to the containing file and may reach anywhere inside the workspace. Ordinary files need no frontmatter or surrounding entrypoints. Their entry lines supply descriptions, tags and conditions. Existing routed metadata must agree with its Entries.

Optional entrypoints use `_{folder-name}.md`, `index.md`, `_index.md`, `references.md` or `_references.md`. Each source has one loading ancestry. Use ordinary body links for other references.

Give every automatically required source an unbroken loading-tag chain from this file. Prefer a direct entry for an independent rule. Edit or remove entries to change what is enabled. An explicitly empty list enables nothing.

### CLI

When available, prefer `open-forge context`, repeating `--for <path>` for every known working file, over separate reads. Read its output and resolve pending or missing sources. Without the CLI, follow the same reading steps directly.

## Entries

- none - No entries - #Empty
```

Managed `AGENTS.md` block, appended without replacing existing instructions:

```markdown
# Open Forge

Before starting or resuming work, read `.agents/loader.md` and follow its reading steps.

**End of Open Forge managed section.**
```

#### The rest of the default install

The default connects one user-selected existing source. Its complete Markdown manifest is:

- `AGENTS.md`, the managed pointer block within the existing file.
- `.agents/loader.md`, copied once as a user-owned reading list.

A CLI-managed installation also creates:

- `.agents/open-forge.json`, maintenance settings.
- `.agents/open-forge.lock.json`, ownership receipts.

Neither JSON file is needed by Context or plain readers. `CLAUDE.md` is a selected host bridge, not an automatic addition. No default Git-ignore edits are made.

Keep Essentials, Full Core and Custom available with their existing selections. Add a recommended “Connect one source” choice ahead of them. Present capability examples before category names. Ask about metadata form only when selected content needs it. Sharing choices remain explicit. Existing Extension dependency graphs need not change for these four standalone capabilities.

An optional grouping entrypoint, `docs/_docs.md`, after choosing documentation and scoped rules:

```markdown
# Project sources

These sources retain their existing authority and ownership.

## Entries

- [C# conventions for C# work](contributing/csharp.md) - #LoadNow - applies to `**/*.cs`
- [Architecture for design changes](architecture.md)
- [Accepted storage decision](adr/0001-storage.md)
```

Link it from the loader as `#LoadNow` to make its conditioned rule automatically reachable. The containing documentation folders need no other entrypoints.

#### Cost and migration

**CLI:** The main cost is a read-path expansion, not merely shorter Markdown. Admit ordinary leaf links and workspace-contained `..` paths, allow skipped physical folders, and obtain metadata from authored entries for linked files without Open Forge metadata. Preserve the existing routed-frontmatter contract and diagnose disagreement. Handle cycles, escapes, missing targets and conflicting loading ancestry explicitly.

Context must work without installation receipts. Index must preserve authored membership and never add metadata to imported docs. Installer and Update must recognize the user-owned loader and preserve its activation list. Optional managed packs retain their lifecycle and overwrite behavior. These changes require C# parser, source-resolution, ownership and command work.

**Tests:** Add focused fixtures for all four independent takes, workspace links, source preservation, missing targets, same-file inherited conditions, pending paths and updates that keep disabled options disabled. Update payload and install snapshots. Require CLI read sets to match hand-written expectations.

**Docs:** Rewrite the quick start around four standalone recipes. Move taxonomy, full Memory governance and collaboration policy into their selected options. Update identity documents and maintenance contracts deliberately.

**Installed workspaces:** Preserve current behavior until an explicit migration is selected. Preview a compact loader built from existing choices, preserving every inherited rule and condition. Never move existing notes or docs automatically. Stop migration where equivalent scope cannot be established. Manual adopters receive the same replacement text and checklist. No automatic conversion to flat lists is required.

#### Risks and proof

Small installs may omit helpful continuity habits. Flat lists can grow awkward, handwritten links can drift, and imported documents can contain stale or conflicting instructions. Retain optional grouping and richer starters, report broken required links, and measure actual behavior.

Before funding parser work, hand-assemble the four independent configurations in four non-maintainer repositories. Recruit one developer per take. Each should adopt it within ten minutes, explain what was enabled, and complete a cold-session task without the CLI. Compare fresh-session read traces against Council 1 and a hand-written expected set. Include a new file type midway and a resume from a saved note.

Pass only if no take requires another capability, no required rule is missed, unrelated rules stay inactive, and existing sources remain unchanged. Then implement only linked-source Context support and compare its output with those same expected sets. File inspection supports this proposal. These experiments and implementation have not run.

#### The one idea to steal

Make the unit of adoption one useful link.

## Essence Guardian (Proposal F)

#### Thesis

Open Forge should defend its opinions about relevance, acceptance, source authority and continuity, while making its filing conventions and methods replaceable. Recommend seven useful files, with a two-file kernel for existing workspaces. An opinion becomes controlling when rejecting its implementation also disables an unrelated benefit or forces the user to reorganize their project.

#### Who it is for

The audience critique is partly correct. The repository identifies concrete problems, but its positioning spreads from software development to personal knowledge vaults before demonstrating adoption across those audiences.

Start with three user types already supported by the documentation:

| User | What they want | Repository evidence |
| --- | --- | --- |
| Maintainer of an existing codebase | Agents should find existing documentation and preserve reasons behind changes | `getting-started/greenfield-and-brownfield.md` recommends Maps and existing ADRs |
| Solo developer working across sessions | Resume unfinished work without explaining everything again | `getting-started/ten-minute-guide.md` demonstrates Working records and explicit resumption |
| Small team using several agents or a monorepo | Shared conventions should apply consistently within the relevant packages | `highlights.md` identifies scopes, Decisions and handoffs |

These paths are under `src/docusaurus/docs/`. They establish plausible users, not validated demand. The statement that early feedback valued Decisions in `highlights.md` is encouraging but limited evidence.

The maintainer’s recoverability problem generalizes. Particular solutions need less prominence. The sealed Analysis-to-Task lifecycle in `.agents/memory/emerging/authors-findings/open-forge-findings-log.md`, exact progress formats in `.agents/directives/hierarchical-orchestration.md`, and extensive maintenance contracts reflect this repository’s needs. The latter two are repository practices, not the shipped product.

#### Why it feels complicated and controlling

1. **Navigation requires learning a structural theory.** “Every folder in a route chain has exactly one recognized entrypoint.”  
   `src/open-forge/.agents/loader.md` makes even navigation impose scaffolding.

2. **Mandatory reading still depends on discretionary discovery.** “Neither activates an unselected ancestor or scope.”  
   `src/open-forge/.agents/loader.md` lets an agent miss a required descendant by skipping its parent.

3. **Optional tooling occupies mandatory startup attention.** “read it when this entrypoint loads so command options and context-loading advice are available from the start.”  
   `src/open-forge/.agents/skills/_skills.md` requires the CLI guide even when no command will run.

4. **Saving something becomes a classification exercise.** “Pick the category by the question the content answers:”  
   `src/docusaurus/docs/getting-started/grow-your-framework.md` introduces a useful taxonomy as the placement interface.

5. **Adaptable conventions still generate compliance bookkeeping.** “A justified adaptation is still a departure, so a review reports it separately from conformance.”  
   `src/open-forge/.agents/patterns/_patterns.md` makes ordinary adaptation feel exceptional.

6. **Folder organization becomes an authority boundary.** “Get explicit user direction before creating a new top-level Memory state.”  
   `src/open-forge/.agents/memory/_memory.md` couples epistemic discipline to one folder model.

7. **Ownership promises require learning update exceptions.** “In a CLI-managed install, a direct edit to a shipped file lasts until the next update.”  
   `src/docusaurus/docs/concepts/customizing.md` explains a real constraint that weakens the intuitive meaning of user-owned files.

#### What to keep from Council 1

| Decision | Verdict and reason |
| --- | --- |
| Procedure first, shorter entrypoints and managed block | Keep. Teach the next action before the vocabulary |
| Existing loading closure, unbroken required chains and same-file condition matching | Keep. These preserve one reading meaning with and without the CLI |
| Unknown working files leave conditions pending | Keep. Resolve applicability before affected work |
| Guidance, Maps, Patterns, Skills and Templates on demand | Keep. Availability should not imply startup consumption |
| Directives, Working and accepted-knowledge navigation required | Keep in the recommended setup, removable when existing sources provide those functions |
| Remove blanket Emerging refresh | Keep. Candidate preservation matters, repeated reading of every candidate catalogue does not |
| Shorten `#KeepInMind` refresh semantics | Change. Retain start, resume, restoration and closeout refresh for deliberately selected continuity records |
| Retire six tag definitions | Change. Reduce vocabulary, but preserve acceptance distinctions and current-source maintenance duties explicitly |
| Blended documentation in Step 2 | Keep. Reading links must neither transfer ownership nor grant write permission |
| Explicit `load.d/` lists | Defer. Benchmark against checked chains before introducing another loading contract |
| Checked replacements later | Keep as an optional improvement, preserving current partial overwrite semantics |
| Reject unconditional loading, silent missing configuration and host-dependent project precedence | Keep. Each would weaken correctness or portability |
| Judge actual reads and behavior | Keep. Word count is a cost measure, not proof of reliability |

#### The tap-in model

The kernel preserves six commitments: readable user-owned meaning, context selected by relevance, explicit acceptance, one authoritative detailed answer, deliberate continuity, and honest reliability claims. These make Open Forge worth choosing. Removing them may leave useful routing, but the same context and continuity promises would no longer apply.

The implementation choices remain independent:

| Optional choice | Value when retained | Alternative and loss |
| --- | --- | --- |
| Directives | Reusable required corrections with explicit scope | Route existing project instructions, losing the supplied authoring convention |
| Four Memory states | Visible separation of active work, candidates, accepted knowledge and history | Use existing documents and explicit status headings, losing ready-made filing |
| Maps | Curated discovery across several destinations | Link directly to existing sources, losing the curated overview |
| Guidance, Patterns and Templates | Adaptable advice, inspectable shapes and starting files | Keep local conventions, losing these reusable aids |
| Skills and workflows | Discoverable capabilities and repeatable methods | Use native discovery and ordinary requests, losing the packaged methods |
| `#KeepInMind` records | Repeated attention to selected continuity state | Resume from an explicit record path, losing scheduled refresh instructions |

Disabling a small opinion means replacing its clause through an overwrite. Disabling a category means removing its behavioral clauses and activating links. Preserve useful notes through a neutral, on-demand catalogue. Record exclusions in CLI-managed installations.

Today’s Custom Remove only releases management. Label that accurately and add a separate previewed disable operation that preserves authored material and reports remaining required references. Runtime meaning stays in Markdown.

The smallest setup uses existing project sources and removes the two default Entries:

```text
AGENTS.md
.agents/
  loader.md
```

The fullest built-in content setup, before optional Extensions and project records, remains:

```text
AGENTS.md
CLAUDE.md
.agents/
  loader.md
  directives/_directives.md
  guidance/_guidance.md
  maps/_maps.md
  patterns/_patterns.md
  templates/_templates.md
  skills/
    _skills.md
    open-forge-cli/
      SKILL.md
      references/
        common.md
        discovery.md
        packages.md
        routes.md
  memory/
    _memory.md
    working/_working.md
    emerging/_emerging.md
    crystallized/_crystallized.md
    archived/_archived.md
```

This preserves the current 19-file Full Core inventory. Extensions retain their separately selected package trees.

#### Behavior without a harness

The model still identifies working files, selects optional context, interprets requirements, judges future usefulness and surfaces consequential ambiguity. Markdown cannot eliminate those decisions.

Structure should settle mandatory reachability, declared scope, conditions, source responsibilities and recorded acceptance. Authors specify those relationships once. Agents follow them instead of reconstructing intent from folder names.

The CLI adds mechanical traversal, condition matching, missing-source diagnostics, navigation maintenance and reference inspection. Context output should explain required, selected, pending and unavailable sources. It never accepts a proposal or proves compliance.

Give the model an immediate reason to use it: one concrete `context` recipe in the loader, an action-oriented native Skill description, and useful output that replaces several file reads. Check availability once per session. Require no CLI-guide startup read. If unavailable, disallowed or unsuccessful, the same Entries and reading procedure remain sufficient.

#### The new loader

The replacement below is for the proposed release after blended-link support lands. Counts use whitespace-delimited words, including headings and route syntax: **575 words for the loader and 20 for the managed block**.

```markdown
# Open Forge Loader

Read this at task start or resume and after context restoration. These files work without the CLI.

## Axioms

### Read

1. Identify the files you will investigate, review, create, change, rename or delete, including planned files. Reading background context does not make it a working file.
2. Starting with Entries below, read applicable #LoadNow and #KeepInMind destinations in listed order. Repeat inside each loaded entrypoint. Read each source once per pass. Other entries open when relevant or explicitly required by a loaded instruction.
3. An entry with `applies to` patterns loads and applies only when one working file matches a pattern at that entry and at every conditioned ancestor. Patterns within one list are alternatives. Globs are case-sensitive and workspace-relative. `*.cs` matches root files, `**/*.cs` any depth, and `docs/**` a subtree. With unknown working files, report conditioned required reads as pending and resolve them before affected work.
4. Before using routed material found through search or a direct link, read its entrypoint chain. Report missing required sources or conflicting declarations before relying on the affected context.
5. Recheck affected routes when the goal or working files change. Reread active #KeepInMind sources before handoff or finish and when they may have changed.

### Use

- Follow the user's direction within platform constraints. Make routine choices within the request. Surface consequential unresolved choices.
- Follow loaded entrypoint `## Axioms`, Directive `## Instructions`, and accepted requirements within their stated scope. Child rules add to parent rules. Advice remains adaptable. Report unresolved conflicts.
- Use the source designated for each question, including declared external authorities. Saving, moving, tagging or repeating material never establishes acceptance. Keep candidates, accepted knowledge and history distinguishable wherever they live.
- Keep each detailed answer in one source and link to it. When accepted direction changes it, update that source within your authority.
- Before pausing unfinished work, preserve the goal, current state, next step and blockers in the workspace's chosen location. Save other outcomes only when future work needs them. If no writable destination is configured, return a handoff in your reply and say it was not saved.
- Links and package installation grant no permissions. Explicit instructions improve reliability but do not guarantee compliance.

### Maintain

An entrypoint is `_{folder-name}.md`, with `index.md`, `_index.md`, `references.md` and `_references.md` also supported. Its Entries expose routes, its Axioms govern their scope. Native Skills keep their native layout.

For routed Markdown under `.agents`, frontmatter supplies `description`, `tags` and optional `applyTo` and `responsibility`. Use the workspace's chosen root or `open-forge:` form. Keep entry descriptions, tags and conditions aligned with their targets. Required sources need an unbroken loading-tag chain from this loader. Use narrow conditions rather than loading unrelated rules.

Entries may link anywhere inside the workspace, including existing documentation. Ordinary cross-links do not create rule inheritance. Existing documents need no new folder taxonomy.

A `{name}.overwrite.md` companion loads immediately after its base and changes only corresponding content within that source's scope. Removed routes must not be recreated. In a CLI-managed workspace, record exclusions too so updates preserve removals.

When available and allowed, prefer `open-forge context --for <path>` to batch reading. Repeat `--for` for every known working file, or omit it while unknown. Read returned content and explicit required links. Otherwise follow Read above. Before other commands, read the installed CLI Skill if present and the command's `--help`.

## Entries

- [Required workspace behavior](directives/_directives.md) - #LoadNow #Core #Directive
- [Continuity and accepted knowledge](memory/_memory.md) - #LoadNow #Memory
```

Managed `AGENTS.md` block:

```markdown
# Open Forge

Before starting a task, read `.agents/loader.md` and follow its reading steps.

**End of Open Forge managed section.**
```

#### The rest of the default install

Recommend these seven content files:

- `AGENTS.md` introduces the loader.
- `.agents/loader.md` defines reading and the minimum trust contract.
- `.agents/directives/_directives.md` holds required corrections and scoped rule routes.
- `.agents/memory/_memory.md` explains deliberate capture and lists installed states.
- `.agents/memory/working/_working.md` supports resumption and checks relevant records at startup.
- `.agents/memory/emerging/_emerging.md` preserves candidates on demand.
- `.agents/memory/crystallized/_crystallized.md` exposes accepted knowledge at startup without duplicating existing sources.

The Memory entrypoint marks Working and Crystallized `#LoadNow`. Emerging remains untagged. Empty entrypoints use the existing `#Empty` placeholder.

A CLI installation also creates `.agents/open-forge.json` for maintenance settings and `.agents/open-forge.lock.json` for ownership. Retaining Essentials’ private Working policy also edits `.gitignore`. These are maintenance artifacts, not behavioral context.

Offer the `CLAUDE.md` bridge and CLI Skill files separately. Keep Custom’s individual route and sharing choices. Add a two-file minimal preset, revise recommended Essentials to seven content files, and preserve Full Core. Existing installations keep their selections until explicitly reconfigured.

For blended documentation, add `- [Project documentation](../docs/_docs.md) - #LoadNow` to loader Entries. A complete illustrative `docs/_docs.md` is:

```markdown
---
description: Architecture and contribution rules for this project
tags: [LoadNow, Map]
---

# Project documentation

These sources remain in their existing locations.

## Axioms

- The architecture document defines accepted system structure.
- Contribution rules apply only within their declared file conditions.

## Entries

- [Accepted system architecture](architecture.md) - #Document #CurrentTruth
- [C# contribution rules](contributing/csharp.md) - #LoadNow #Directive - applies to `**/*.cs`
```

This catalogue is authored and accepted by the workspace. Linking it does not manufacture acceptance.

#### Cost and migration

**Markdown and installation work:** compress entrypoints, revise presets, remove compulsory CLI-guide loading, support disabling independently from management release, and preserve intentional omissions. These change product contracts even where parsing stays unchanged.

**CLI work:** permit contained `..` links, workspace-wide source identities and direct document targets without intermediate entrypoints. Preserve one unambiguous catalogue ancestry. Ordinary references add none. Reject workspace escapes and competing inherited parents.

For existing documents without frontmatter, authored Entries must supply navigation metadata. Diagnose conflicting declarations rather than silently selecting one. Keep outside documents read-only to indexing initially.

**Tests:** update payload snapshots and Install, Update, Context and navigation fixtures. Add focused cases for document targets, same-file inherited conditions, unknown paths, missing required sources, disabling with retained notes, overwrites and workspace containment.

**Documentation:** rewrite onboarding around one correction, one existing source and one resumed task. Explain category benefits when users choose them. Update architecture, maintenance contracts and tag decisions together.

**Installed workspaces:** never silently represet or relocate records. Preview managed text changes, preserve overwrites and exclusions, and retain current loading semantics. Ship the blended loader only with matching CLI support. Older executables must report unsupported links, leaving plain-file reading available.

#### Risks and proof

A smaller baseline may hide useful capabilities. Seven files may still feel excessive. Manual navigation can become stale. External catalogues can introduce ambiguous scope. Users who decline recording lose automatic continuity, which must be stated plainly.

The cheapest useful experiment involves three non-maintainers bringing existing repositories: a solo project, a team codebase with ADRs, and a monorepo. Each performs one ordinary task with a pause and resume, then disables one unwanted convention.

Compare the current payload, Council 1 and this proposal in fresh sessions, without the CLI first. Capture actual file reads and resulting work. Measure missed required context, false acceptance, irrelevant reads, repeated explanation, setup time and whether disabled behavior returns. Then enable the CLI on the hardest case and observe whether the agent reaches for it without reminders.

Proceed only if required-read coverage and acceptance discipline hold, resumption works, and disabling preserves useful content. Three users provide a falsification opportunity, not evidence of universal reliability.

#### The one idea to steal

Make Open Forge’s trust contract consistent across every setup, while letting users keep their own places, names and methods.

## No-CLI Realist (Proposal C)

#### Thesis

Open Forge should make required context one explicit, user-owned reading list and offer its organizational opinions as independent modules. Plain-file readers should follow that list without reconstructing a routing tree. The CLI should earn use by returning the same sources, with complete bodies and selection reasons, in one call.

#### Who it is for

The repository shows a clear problem, but an excessively broad audience. `vision.md` describes repeated direction, stale knowledge, expensive retrieval and lost continuity, then extends the product to any discipline or collection of projects. That is coherent intent, not yet evidence of broad demand.

Three concrete audiences deserve priority:

| User | What they want | Repository evidence |
| --- | --- | --- |
| A solo developer changing sessions or agents | Resume without explaining settled choices again | `src/docusaurus/docs/highlights.md` presents Checkpoints and Handoffs |
| A team adopting agents in an existing codebase | Honor existing documentation and preserve reasons behind changes | `getting-started/greenfield-and-brownfield.md` recommends Maps and Decisions |
| A monorepo maintainer | Apply local conventions without loading unrelated rules | `concepts/scopes.md` describes package-specific context |

Those documentation paths are under `src/docusaurus/docs/`.

The maintainer’s more particular concerns are visible in `.agents/memory/emerging/authors-findings/open-forge-findings-log.md`: maximum recoverability, minimizing active context, avoiding changes across 30 or 40 documents, and sealing analysis into executable Tasks. The repository’s permanent task identities and detailed orchestration controls address another specialized working style. These are useful local experiments, not evidence that every adopter needs their organization.

The critique therefore lands on product focus and adoption cost, rather than absence of purpose.

#### Why it feels complicated and controlling

1. **Scope discovery is an open-ended obligation.** “Use it to select every relevant scope, including nested scopes.” — `src/open-forge/AGENTS.md`. Correctness begins with the model judging what it has not read.

2. **Required rules can remain hidden.** “These tags act only through loaded parents. Neither activates an unselected ancestor or scope.” — `src/open-forge/.agents/loader.md`. A mandatory descendant still depends on discretionary navigation.

3. **Navigation requires scaffolding.** “Every folder in a route chain has exactly one recognized entrypoint.” — the same loader. Existing documentation must acquire framework structure or be reached indirectly.

4. **Conditions require accumulated bookkeeping.** “only while one working file matches its patterns and those of every ancestor with `applyTo`.” — the same loader. The reader must retain the complete predicate chain.

5. **Adaptable conventions still create reporting duties.** “A justified adaptation is still a departure, so a review reports it separately from conformance.” — `src/open-forge/.agents/patterns/_patterns.md`.

6. **Storage organization becomes a decision boundary.** “Get explicit user direction before creating a new top-level Memory state.” — `src/open-forge/.agents/memory/_memory.md`.

7. **The accelerator adds startup reading before providing acceleration.** “The Skills entrypoint requires this guide at startup.” — `src/open-forge/.agents/skills/open-forge-cli/SKILL.md`.

8. **Customization needs protection from maintenance.** “Replaces your edited copy with the current shipped version and reports it.” — `src/docusaurus/docs/concepts/customizing.md`. Users must learn companions or exclusions to preserve straightforward edits.

#### What to keep from Council 1

Council 1 remains an unaccepted input.

| Decision | Disposition and reason |
| --- | --- |
| Teach a reading procedure first | Keep. It answers the newcomer’s immediate question. |
| Reduce startup and move authoring detail out | Keep. Its reported reduction from 4,563 to 1,425 words is substantial. |
| Make required chains unbroken | Keep as a compatibility repair, not the destination. Chains still require runtime bookkeeping. |
| Make Guidance, Maps, Patterns, Skills and Templates on demand | Keep. Availability need not imply startup policy. |
| Require Directives and Memory categories | Change. Require individually selected rule sources and an optional continuity module. |
| Remove universal Emerging refresh | Keep. Refresh actual continuation needs. |
| Treat unknown file conditions as pending | Keep. Broad loading would penalize plain-file users. |
| Separate loading, acceptance and ownership | Keep. Reading cannot establish truth or write permission. |
| Support documents outside `.agents/` | Keep, including read-only CLI treatment initially. |
| Explicit lists and navigation-only catalogues | Keep the destination, but begin with one list in the loader rather than mandatory `load.d/`. |
| Preserve companion semantics and investigate checked replacements | Keep compatibility. Defer another customization language. |
| Benchmark behavior before migration | Keep. Shorter instructions alone prove little. |

#### The tap-in model

The kernel has two files: a host entry and an explicit reading contract. It requires no categories, Memory states, metadata on linked documents, or CLI.

The recommended default adds one small continuity module. Every other opinion has three choices: use its policy, keep its files as reference, or leave it out. Sharing policy is a separate choice.

Removing a required row disables that framework policy. Its notes can remain. Other modules must not recreate it or require its taxonomy. Mandatory dependencies must appear explicitly in the reading list.

Individual opinions remain editable in their defining files. In the new installation format, updates preserve edited files and offer reviewable upstream differences. Ordinary customization should not require an overwrite companion.

| Optional opinion | Value when retained |
| --- | --- |
| Directives | Clear, scoped obligations |
| Guidance | Reusable advice with reasons and tradeoffs |
| Patterns | Comparable structural examples |
| Skills | Native capability discovery |
| Templates | Faster creation without ongoing coupling |
| Maps | Curated pointers, especially to external systems |
| Four Memory states | Explicit distinctions between active work, candidates, accepted knowledge and history |
| Continuity | Recoverable unfinished work without adopting those four states |

This changes accepted routing and Core/Memory contracts. It also challenges `product-direction.md`’s rejection of an empty routing library by supporting that smallest installation, while recommending a useful continuity default. It preserves user ownership, relevance and honest reliability.

Smallest installation:

```text
AGENTS.md
.agents/
  loader.md
```

Its loader uses `- none - No entries - #Empty` under Entries.

Fullest built-in installation follows below. Extensions and user content are unbounded. Installing every built-in makes it available without automatically activating every policy.

```text
AGENTS.md
CLAUDE.md
.agents/
  loader.md
  continuity/_continuity.md
  directives/_directives.md
  guidance/_guidance.md
  maps/_maps.md
  patterns/_patterns.md
  templates/_templates.md
  skills/
    _skills.md
    open-forge-cli/
      SKILL.md
      references/
        discovery.md
        routes.md
        packages.md
        common.md
  memory/
    _memory.md
    working/_working.md
    emerging/_emerging.md
    crystallized/_crystallized.md
    archived/_archived.md
  open-forge.json          CLI maintenance only
  open-forge.lock.json     CLI maintenance only
```

That is 20 Markdown files, plus two records when CLI-managed.

#### Behavior without a harness

A desk trace of a fresh agent reviewing `src/cli/root/OpenForge.Cli/Program.cs` here exposes eight bookkeeping points:

1. Enter through `AGENTS.md` and identify working paths.
2. Walk startup tag chains and check adjacent companions.
3. Recognize that the unselected C# and Open Forge CLI scopes matter.
4. Carry ancestor rules and same-file condition intersections.
5. Follow prose-required reads beyond the CLI’s computed closure.
6. Recover ancestor context for anything found through search.
7. Recalculate selection when another working file joins.
8. Remember active refresh obligations and appropriate closeout storage.

This is a source-based trace, not an observed agent trial.

The replacement keeps steps 1 and 7, makes required selection one independent row check, and removes ancestor recovery. Scope belongs in each rule source. Optional navigation cannot introduce hidden mandatory rules.

The model still identifies working files, interprets requirements, selects useful background and judges what deserves preservation. Structure handles declared required reads, their conditions and refresh markers.

The CLI evaluates that same list, checks links and companions, deduplicates sources and returns bodies. Proposed output begins:

```text
Reading mode: flat-v1
Working files: src/Orders.cs
Required: docs/contributing/csharp.md
Reason: matches **/*.cs
Pending: none
Missing: none

[complete source bodies follow]
```

Make the first call obviously worthwhile:

- Put the exact `context --for` shortcut in both entry files.
- Require no help, route selection or Skill read before `context`.
- Give the native Skill a task-start and changed-files trigger.
- End Context output with the command to repeat when working paths change.

These are incentives, not enforcement.

CLI-assisted migration, Index and Repair also improve the ordinary files read by agents that never invoke the CLI. Manifest membership and conditions remain user-owned. Defer task snapshots because freshness checking would reintroduce bookkeeping.

#### The new loader

This introduces proposed `flat-v1` semantics. Existing CLI behavior must not be represented as supporting them.

Whitespace counts include headings and metadata, excluding fences: **497 words** for the loader and **44 words** for the managed block.

```markdown
---
open-forge:
  loading: flat-v1
---

# Open Forge Loader

This file lists what to read for a task. It works without the CLI.

## Axioms

### Read

1. At task start, resume, or context restoration, identify known working files, including planned files and both paths of a rename. These are files you investigate, review, create, change, or delete. Reading background context does not make it a working file.
2. Read every applicable #LoadNow or #KeepInMind destination under Entries below, in listed order. Read each source once per pass. Descriptions do not make a required read optional.
3. An entry with `applies to` matches when any working file matches any listed pattern. Patterns are case-sensitive globs from the workspace root, using `/`. Use simple patterns such as `docs/**` or `**/*.cs`. Conditions belong only to their row and never inherit. Apply conditioned rules only to matching working files.
4. When working files are unknown, report conditioned required rows as pending. Resolve them before affected work. Recheck this list when the goal or working files change.
5. Other entries are navigation. Open them when useful. Their child lists do not schedule required reads, and folder ancestry adds no rules.
6. Before handoff or finish, reread matching #KeepInMind sources. Reread changed active sources before relying on them.

Read an existing adjacent `{name}.overwrite.md` immediately after its base. Its changes win only for corresponding base content, within the same scope.

### Use

Project rule sources are active when listed as required here or when the user explicitly adopts their instructions for the task. Follow accepted rules within their stated scope. Sources must state their complete scope and obligations. Reading order does not resolve conflicts. Follow clear user direction within platform constraints and report unresolved conflicts.

Advice can be adapted. A note, tag, location, or repeated claim does not establish acceptance. Use accepted sources for the facts they define. Keep unaccepted proposals distinct.

### Customize

This list defines loading tags and conditions in flat-v1. Other source metadata does not add required reads. List every framework-required source directly here. Do not hide required rule dependencies in prose or navigation.

Keep documents where they already live. Links resolve relative to this file and may use `..` while staying inside the workspace. Linked documents need no Open Forge frontmatter or parent catalogues.

Edit or remove a row to change the reading contract. Omitted modules impose no work and must not be recreated. A missing linked destination is an error to report.

### Optional shortcut

Try `open-forge context --for <path>` to receive required sources together. Repeat `--for` for every working path, or omit it while paths are unknown. No Skill or help read is needed first.

Use the computed set only when the result header says `Reading mode: flat-v1`. Otherwise follow this file's links. Read the returned bodies and the task inputs you need. For maintenance commands, consult the installed CLI Skill or command help.

## Entries

- [Preserve useful state when work pauses or resumes](continuity/_continuity.md) - #LoadNow
```

Managed `AGENTS.md` block:

```markdown
# Open Forge

Read `.agents/loader.md` before each task or resume and follow its reading steps.

Shortcut when installed: `open-forge context --for <path>` returns required sources together. Repeat `--for` for every working path. If unavailable, follow the loader's links.

**End of Open Forge managed section.**
```

#### The rest of the default install

The recommended default ships exactly three Markdown files:

- `AGENTS.md` provides the host entry and command shortcut.
- `.agents/loader.md` defines the complete framework reading contract.
- `.agents/continuity/_continuity.md` preserves useful continuation state without prescribing storage states.

CLI installation additionally creates its settings and ownership records. A selected `CLAUDE.md` bridge imports the canonical files. Git-ignore rules are added only when sharing choices request them.

Complete default entrypoint, **123 words**:

```markdown
---
description: Preserve useful state when work pauses or resumes
tags: [Continuity]
---

# Continuity

These rules apply to every task while this file is required by the loader.

## Axioms

- When unfinished work needs to survive a pause or handoff, update an existing task note or create one at a suitable user-owned location.
- Record the goal, actual state, next action, decisive evidence, and unresolved choices. Distinguish accepted direction from proposals.
- Include the note's path in the handoff. Resume from the note supplied with the task and check it against current files.
- Preserve only what another session needs. Completed work needs no separate record unless its outcome has continuing value.

## Entries

- none - No entries - #Empty
```

Default startup totals **664 words**, before project content.

Existing documentation joins through a direct loader row:

```markdown
- [C# conventions](../docs/contributing/csharp.md) - #LoadNow - applies to `**/*.cs`
```

Fresh Essentials becomes this three-file default. Existing selections remain pinned. Full Core supplies all built-ins. Custom separates policy activation, reference availability and sharing. Interactive and unattended fresh installation should use the same default.

#### Cost and migration

This is substantial contract work, not a Markdown-only simplification.

**CLI:** Add the loading-mode field, direct workspace-wide source identity, contained `..` links, independent row conditions and body output with reasons. Remove inherited interpretation only in the new mode. Preserve existing entrypoint names, Entries syntax and metadata forms. Extend installer choices and change Update to preserve edited files and omitted memberships. Index must not rebuild the required list from folder contents.

**Tests:** Add paired legacy/flat fixtures for Context, installation, updates, navigation, missing targets, unknown paths, companions and external documentation. Update public output snapshots. Verify that disabling a policy survives maintenance.

**Docs:** Rewrite onboarding around one rule and one existing document. Update routing, authority, customization, loading, maintenance contracts and Extension starters. Remove assumptions that every workspace has Core categories or Memory states.

**Installed workspaces:** Never silently switch semantics. Extract inherited rules into explicitly scoped sources and register them once. Flatten condition intersections only when equivalence can be established. Otherwise retain legacy mode pending review.

Offer a previewable migration, preserve original bytes and use a versioned maintenance format. New tooling should reject unknown modes. The loader’s required result header protects the reading path when an older CLI is encountered.

#### Risks and proof

A flat list grows with required obligations. Registration can be forgotten. Removing inherited scope makes authors responsible for stating scope correctly. Lighter continuity may capture less, and preserving edits makes upstream improvements less automatic.

The cheapest useful experiment needs no CLI implementation:

1. Recruit two people with existing repositories and existing documentation.
2. Give each two short tasks involving a new working path and a pause/resume.
3. Compare Council 1 Step 1 with a hand-authored flat layout in fresh sessions, with the CLI absent.
4. Ask each person to add one scoped rule and disable continuity without adopting a new taxonomy.
5. Replay representative tasks with a prototype Context command afterward.

Observe actual file reads, missed rules, rules applied outside scope, draft material treated as accepted, continuation quality, unnecessary reading, configuration mistakes and voluntary CLI uptake.

Reject the design if it introduces a required-read miss the comparison catches, loses necessary continuation state, or restores a rejected opinion. Advance only if outsiders can understand and maintain the reading contract with less effort.

This proposal is based on read-only inspection. No files were changed and no builds or test suites were run.

#### The one idea to steal

Make required context one explicit, user-owned reading list that the CLI executes and every other agent can follow directly.

## Behavior Engineer (Proposal A)

#### Thesis

Open Forge should make required reading explicit while letting users choose its organizational and behavioral opinions independently. Keep a small reading contract, make activation visible in the loader, and offer conversation style, memory capture and verification routines as separate choices.

#### Who it is for

The critique is partly right. The repository identifies concrete needs, but its ambition exceeds its demonstrated audience. `highlights.md` names existing codebases, monorepos and session changes, while `vision.md` extends the product to “any person, project, team, discipline, collection of projects, or shared source of truth”.

Prioritize three users:

| User | What they actually want |
| --- | --- |
| Maintainer of an existing codebase | Agents that respect existing documentation and preserve reasons behind changes without reorganizing the repository |
| Monorepo contributor | Applicable rules loaded before affected work, without unrelated package rules entering context |
| Solo builder using several sessions or agents | A reliable resumption point without adopting a complete planning methodology |

The maintainer’s own files demonstrate narrower needs. The findings log describes changes that require editing “30 or 40 documents” and proposes “Analysis → conclusion → sealed Analysis → actionable Task”. Hierarchical orchestration, protected findings notebooks and extensive maintenance contracts address that environment. These are useful local examples, not evidence that every installation needs their lifecycle or reporting conventions.

The strongest initial positioning is therefore: **make existing project instructions and knowledge reliably usable across agent sessions**. Broader applications can remain possible without driving the default install.

#### Why it feels complicated and controlling

1. **Mandatory discovery depends on discretionary navigation.** “These tags act only through loaded parents. Neither activates an unselected ancestor or scope.” A model must discover the correct parent before an ostensibly required rule becomes visible.  
   `src/open-forge/.agents/loader.md`

2. **Physical organization becomes a protocol.** “Every folder in a route chain has exactly one recognized entrypoint.” Ordinary documentation acquires scaffolding requirements before participating directly.  
   `src/open-forge/.agents/loader.md`

3. **The routing layer prescribes conversation.** “Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.” This is a collaboration preference embedded in universal infrastructure.  
   `src/open-forge/.agents/loader.md`

4. **Authoring introduces another representation to maintain.** “Routed Markdown files start with YAML frontmatter.” Entries then repeat that metadata, adding synchronization work for manual users.  
   `src/open-forge/.agents/loader.md`

5. **Continuity creates recurring housekeeping.** “At each applicable #KeepInMind refresh, check visible `Entries` and save useful candidate material before it is lost.” Merely installing a state introduces an ongoing capture procedure.  
   `src/open-forge/.agents/memory/emerging/_emerging.md`

6. **The supplied taxonomy protects itself.** “Get explicit user direction before creating a new top-level Memory state.” A storage convention becomes a permission boundary.  
   `src/open-forge/.agents/memory/_memory.md`

7. **Editing does not necessarily preserve customization.** “Replaces your edited copy with the current shipped version and reports it.” Users must understand overwrite companions to retain their preferences through updates.  
   `src/docusaurus/docs/concepts/customizing.md`

8. **Leaving something out does not mean disabling its behavior.** “Existing files, notes, and overwrite companions stay in place and remain routable.” Installation choices concern management, while users may understand them as activation choices.  
   `src/docusaurus/docs/getting-started/installation.md`

#### What to keep from Council 1

These remain proposals, as `artifacts/council-1/record.md` explicitly states.

| Decision | Recommendation |
| --- | --- |
| Teach a reading procedure first | Keep. Readers need the next action before the vocabulary |
| Require unbroken loading-tag chains | Keep. Required descendants must be reachable without guessing |
| Preserve same-file condition matching | Keep. Different working files must not jointly satisfy one inherited chain |
| Leave unknown conditions pending | Keep. Resolve them before affected work |
| Move most categories on demand | Extend. Let users omit their organization entirely |
| Require Memory, Working and Crystallized | Change. Continuity should work with existing project records too |
| Remove Emerging’s default refresh | Keep. Refresh active records rather than empty storage categories |
| Reduce KeepInMind to a final reread | Change. Retain resume, restoration and changed-source triggers for selected records |
| Remove six universal tag definitions | Keep. Preserve acceptance distinctions in plain language and optional modules |
| Remove CLI Skill from startup | Keep. Put one executable reading instruction directly in the loader |
| Shorten entrypoints and move maintenance detail out | Keep. Retain enough manual authoring guidance for CLI independence |
| Keep a universal response recipe | Drop. Offer it as an optional behavior |
| Support existing `docs/` paths | Keep. Linking must not imply acceptance, management or write permission |
| Introduce `load.d/` | Defer to the proposed comparison experiment |
| Preserve overwrite semantics and defer checked replacements | Keep. Avoid coupling customization improvements to routing migration |
| Measure actual reads and behavior | Keep. Word counts alone cannot establish reliability |

#### The tap-in model

Separate **having files** from **activating their Open Forge behavior**.

The loader’s `Entries` section becomes the user-owned list of active roots. A listed root can be required or on demand. Removing its entry disables its Open Forge procedures while preserving its files. Searching an inactive catalogue does not activate its Axioms.

Activation references between modules must be visible. Disabling a module also removes incoming activation references, with dependent procedures reported for adjustment. Ordinary knowledge links remain usable.

The irreducible kernel is:

- A host entry that reaches the loader.
- Explicit required reading, applicability and refresh rules.
- Clear distinctions between instructions, accepted requirements and proposals.
- Visible handling of missing sources and unresolved conflicts.

The following opinions remain independently useful:

| Optional opinion | Value when selected |
| --- | --- |
| Six Core categories | Distinguishes required behavior, advice, shapes, capabilities, starters and pointers |
| Four Memory states | Separates active state, candidates, accepted knowledge and history |
| Communication pack | Establishes preferred verbosity, recommendations and clarification behavior |
| Capture pack | Preserves resumable state at explicit boundaries |
| Verification pack | Requires observed checks and a concise completion receipt |
| Existing Extensions | Supplies methods and record shapes when their goals fit |

Rejecting one rule means editing its defining source or companion, rather than adding a contradictory child rule.

Existing documentation can participate directly:

```markdown
- [Project architecture and contribution rules](../docs/_docs.md) - #LoadNow
```

The smallest setup is a manual two-file installation with empty loader Entries:

```text
AGENTS.md
.agents/
  loader.md
```

The fullest framework setup enables every foundation option and all three behavior packs. Existing Extensions populate these catalogues with their published content.

```text
AGENTS.md
CLAUDE.md
.agents/
  loader.md
  open-forge.json                    [CLI maintenance only]
  open-forge.lock.json               [CLI maintenance only]
  directives/_directives.md
  guidance/_guidance.md
  maps/_maps.md
  patterns/_patterns.md
  templates/_templates.md
  skills/
    _skills.md
    open-forge-cli/
      SKILL.md
      references/
        common.md
        discovery.md
        packages.md
        routes.md
  memory/
    _memory.md
    working/_working.md
    emerging/_emerging.md
    crystallized/_crystallized.md
    archived/_archived.md
  communication/_communication.md
  capture/_capture.md
  verification/_verification.md
```

This changes accepted routing, installation and behavior contracts. It does not reject an identity principle. It makes “sensible removable defaults” operational.

#### Behavior without a harness

| Decision | Treatment |
| --- | --- |
| Which mandatory sources to read | Structure. Unbroken tagged chains and explicit conditions |
| Which optional knowledge is relevant | Model judgment, aided by concrete descriptions |
| Where to save | Selected capture policy names a destination. Otherwise use requested or existing project locations |
| Whether something is accepted | User direction or designated authority. Recording never supplies acceptance |
| When to refresh | Explicit lifecycle triggers on selected KeepInMind sources |
| Which rule wins | Resolve declared authority and scope. Report genuinely unresolved conflicts |
| How much to plan or explain | Host defaults, user requests or selected behavior packs |
| How to report verification | Optional pack requires command, observed result and omitted checks |

The capture pack should trigger before an unfinished handoff or planned pause, not after every message. Its record needs only the goal, current state, decisive evidence and next action.

The verification pack should trigger before claiming a code change complete. Its receipt reports actual checks and remaining limitations. Neither pack requires a ceremony for ordinary discussion.

The CLI adds computed reading sets, condition evaluation, route explanations, navigation maintenance and diagnostics. It does not decide acceptance.

To encourage CLI use, put an executable instruction near the beginning of the loader. Avoid making agents read a command manual before their first context request. Give the native Skill a description that explicitly mentions task startup and context expansion.

Users may configure existing host hooks to inject read-only context output. Those hooks are optional adapters with the same file procedure as fallback. Open Forge cannot guarantee that a model follows instructions, or deactivate instructions independently loaded by its host.

#### The new loader

This is the proposed replacement after the CLI changes described below. Its default Entries reference the recommended Directives scaffold.

Counts use whitespace-separated units, including headings and Markdown syntax. The loader contains **631 words**, the managed block **21**, and the complete default startup **735** with the 83-word entrypoint below.

```markdown
# Open Forge Loader

Read the entries selected by this file before working. Files outside this list remain ordinary project material.

When tool use is allowed and `open-forge` is available, run `open-forge context`, repeating `--for <path>` for every known working file. Read its returned sources and any reads their instructions require. If it is unavailable, fails, or cannot represent these routes, follow the procedure below with file tools. Do not install or repair the CLI as part of startup.

## Axioms

### Read

1. At task start, resume, or context restoration, identify known working files, including planned files and both paths of renames. These are files you investigate, review, create, change, rename, or delete. Background reading does not make a file a working file.
2. Starting at Entries below, read applicable #LoadNow and #KeepInMind targets in listed order. Repeat inside each loaded entrypoint. Read each source once per pass, followed immediately by its `{name}.overwrite.md` companion when present.
3. An `applies to` condition filters reading and application. At least one working file must match a pattern at every conditioned point in the chain. The same file must satisfy the whole chain. Apply rules only to matching files. Patterns are case-sensitive workspace-relative globs. `**/*.cs` matches C# files at any depth. `docs/**` matches files under docs. A leading `!` is literal.
4. Unknown working files leave conditioned required entries pending. Resolve them before affected work. Other entries open when their descriptions fit the task or a loaded instruction explicitly requires them. Before using a routed source found through search, read its active ancestor entrypoints.
5. When the goal or working files change, check the active routes again. Refresh #KeepInMind sources at resume, context restoration, before handoff or finish, and when they may have changed. Report missing required sources and unresolved conditions that block affected work.

An entrypoint is normally `_{folder-name}.md`. `index.md`, `_index.md`, `references.md`, and `_references.md` also qualify. Its Entries list destinations. Its Axioms apply to its selected descendants.

### Use

- Follow loaded Axioms and the Instructions of sources explicitly designated as rules, within their conditions. Narrower rules add requirements. They do not silently cancel broader rules. Follow current user direction within platform constraints and report unresolved project-rule conflicts before affected work.
- Other sources retain their declared purpose and authority. Reading, tagging, copying, or moving a proposal does not accept it. Acceptance requires user direction or an authority the user has designated.
- An overwrite changes only the corresponding content of its base source. It does not override unrelated rules.
- This loader requires no planning method, response format, storage taxonomy, or automatic note capture. Follow those practices only when requested or required by an active source.

### Customize

Entries below define the active Open Forge roots, including on-demand roots. Remove a root entry to deactivate its Open Forge behavior without deleting its files. Reading an inactive catalogue for information does not activate its Axioms. Deactivation does not revoke accepted project requirements. Independent host instructions still apply.

Link existing documents where they live, including `../docs/_docs.md`. Links grant neither acceptance nor write permission. Ordinary folders need no entrypoint unless used as catalogues.

Tag an automatically required rule and every entrypoint in its chain #LoadNow. Narrow the branch with `applyTo`. Keep optional reading untagged. Only #LoadNow and #KeepInMind control reading.

When adding routed Markdown, provide frontmatter `description`, `tags`, and optional `applyTo`. Use the workspace's existing root or `open-forge:` form. Repeat these values in its parent's Entries, updating links when files move. Native Skills keep their native layout.

The CLI may maintain metadata and navigation inside active roots. Loader membership is user-owned and must survive Index, Install, and Update unchanged unless a reviewed activation change was requested. Without the CLI, maintain the same lists by hand.

## Entries

- [Required workspace rules](directives/_directives.md) - #LoadNow #Core #Directive
```

The managed `AGENTS.md` block is:

```markdown
# Open Forge

Before each task or resume, read `.agents/loader.md` and follow its reading procedure.

**End of Open Forge managed section.**
```

#### The rest of the default install

The recommended default ships three Markdown files:

| File | Purpose |
| --- | --- |
| `AGENTS.md` | Canonical handoff to the reading procedure |
| `.agents/loader.md` | Reading contract and user-owned activation list |
| `.agents/directives/_directives.md` | Ready location for recurring corrections, without supplying a project methodology |

A CLI installation additionally creates `.agents/open-forge.json` for maintenance choices and `.agents/open-forge.lock.json` for ownership. Neither determines behavior. The default adds no Git-ignore rules.

The complete shipped entrypoint is:

```markdown
---
description: Required workspace rules
tags: [LoadNow, Core, Directive]
---

# Directives

Keep required agent behavior here when this organization helps.

## Axioms

- State each rule's obligations, conditions, and exceptions under `## Instructions`.
- Tag required rule files #LoadNow. For required descendants, tag every intermediate entrypoint too.
- Use `applyTo` for file conditions. Task-dependent branches stay on demand with a clear trigger in their description.
- Report unresolved rule conflicts before affected work.

## Entries

- none - No entries - #Empty
```

Add a recommended **Router** preset rather than silently changing existing preset flags. Retain Essentials, Full Core and Custom for compatibility.

Custom should distinguish activation from retention and sharing. “Disable, keep files” must be an explicit operation. Removing files remains separate.

Offer behavior packs separately from category scaffolding. Installing Memory must not automatically enable capture. Install the `CLAUDE.md` bridge only when selected, preserving its native imports. Provide the same Router payload for manual copying.

#### Cost and migration

**Markdown cleanup is relatively small.** Rewrite category openings and move collaboration and capture rules into their selected modules. Preserve all useful categories and Extension methods.

**CLI work is material.** Keep existing entrypoint names, Entries syntax, frontmatter fields and loading tags. Change source resolution to admit contained `..` links and catalogues outside `.agents/`. Permit ordinary intermediate documentation folders without mandatory entrypoints. Preserve one unambiguous catalogue ancestry for inherited rules and conditions.

Loader Entries must become an authored membership region. Index and Update may refresh descriptions without reintroducing inactive roots. Install and package plans must show activation changes explicitly. Inspecting an inactive source must not implicitly activate it.

Initially, outside catalogues remain manually maintained. Reading them must not establish CLI write ownership.

Tests need coverage for workspace containment, ambiguous ancestry, missing required targets, pending conditions, same-file matching, inactive roots, overwrite behavior and activation preservation. Update payload snapshots and public command journeys.

Docs must explain activation before taxonomy, show manual and CLI paths together, and replace startup diagrams and install inventories.

Existing workspaces retain their behavior until a reviewed migration. Preserve current active roots, capture obligations, companions and exclusions. Do not move documents or disable Memory automatically. Changes to acceptance labels require explicit replacement prose where their meaning remains necessary.

#### Risks and proof

A smaller default can look empty. Optional packs can acquire hidden dependencies. Authored activation lists can become stale. Removing automatic capture can lose useful continuity. A retained inactive catalogue may also be independently activated by host configuration.

The cheapest useful experiment uses three people outside the project, each bringing one real task involving existing documentation. Include unknown initial paths, a scope change and a pause across the three tasks.

Run fresh sessions against the current payload, Council 1 and this proposal without the CLI. Replay the proposed setup with the CLI. This produces twelve sessions.

Inspect tool traces and results for missed required reads, rules applied outside scope, accidental acceptance, disabled behavior reappearing, unnecessary reads and lost resumption state. Ask each participant to disable one disliked opinion without assistance.

Advance only if required behavior remains intact, disabled behavior stays off, and users can explain and customize the reading path themselves.

#### The one idea to steal

Make the loader’s active-root list user-owned, so users can turn an opinion off without deleting their knowledge or having an update turn it back on.

## Modular Architect (Proposal H)

#### Thesis

Open Forge should have a routing kernel, independently removable modules and presets that select those modules. Categories should organize content only when users want them, while behavioral opinions should be separate choices. The loader’s links should show what is active, with no hidden activation state.

#### Who it is for

The “hobby without a clear audience” critique is partly justified. The repository identifies credible needs, but the vision extends to “any person, project, team, discipline” without demonstrating which audience benefits most.

Three concrete audiences emerge from `README.md`, `highlights.md` and the getting-started guides:

| User | What they want | Relevant value |
| --- | --- | --- |
| A solo developer returning across sessions | Stop repeating corrections and recover unfinished work | Scoped rules and optional Working Memory |
| A team maintaining an established codebase | Agents that respect existing documentation and ADRs | Direct documentation routes, Maps and recorded decisions |
| Contributors working across a monorepo | Relevant conventions without loading every team’s rules | File conditions, scoped entrypoints and CLI context retrieval |

These are plausible targets, not demonstrated adoption.

The maintainer’s own pain points are more specific. The findings log emphasizes maximum recoverability, sealed analysis, executable Tasks, document lifecycle and duplicated knowledge. This repository’s orchestration directives add elaborate ownership and review arrangements. Those are useful evidence for optional continuity and delivery modules. They should not define every installation.

#### Why it feels complicated and controlling

1. **The taxonomy claims too much territory.** `src/docusaurus/docs/intro.md` says, “Everything in the workspace answers one of two questions:”. Existing documentation then feels subordinate to Core and Memory.

2. **Navigation requires structural compliance.** `src/open-forge/.agents/loader.md` says, “Every folder in a route chain has exactly one recognized entrypoint.” This makes ordinary documentation expensive to adopt.

3. **Required context can remain hidden behind judgment.** The loader says, “These tags act only through loaded parents. Neither activates an unselected ancestor or scope.” Mandatory descendants still depend on choosing their parent.

4. **Communication preferences become universal rules.** The loader says, “Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.” A response style is bundled with routing.

5. **Memory introduces automatic work.** `src/open-forge/.agents/memory/_memory.md` says, “Before closeout, also preserve unsettled findings worth revisiting and temporary state needed to resume or transfer unfinished work.” Selecting storage also selects capture behavior.

6. **An optional accelerator creates compulsory reading.** `src/open-forge/.agents/skills/open-forge-cli/SKILL.md` says, “The Skills entrypoint requires this guide at startup.”

7. **Ownership and updates pull in different directions.** `src/docusaurus/docs/concepts/customizing.md` describes updating a directly edited shipped file as, “Replaces your edited copy with the current shipped version and reports it.” Users need another mechanism to keep their preferences.

#### What to keep from Council 1

| Decision | Verdict and reason |
| --- | --- |
| Procedure before vocabulary | Keep. Readers need an executable reading sequence first. |
| Shorter entrypoints and authoring details outside startup | Keep. Package maintenance is not task context. |
| Unbroken tagged chains | Keep. Required reading must be reachable without guessing. |
| Same working file matches every inherited condition | Keep, including pending conditions while paths are unknown. |
| Guidance, Maps, Patterns, Skills and Templates on demand | Keep. Availability does not justify compulsory reading. |
| Directives, Memory, Working and Crystallized required by default | Change. Only explicitly selected required policies should impose startup reads. |
| CLI guide read when needed | Keep, with a small optional adapter supplying the startup command trigger. |
| Simplified communication and acceptance rules | Split. Minimal acceptance protection belongs in the kernel, richer policies and response style belong in modules. |
| Non-loading tags become labels | Keep, while offering their former maintenance behavior through explicit policies. |
| Blended documentation with contained links and one route ancestry | Keep. Reading must remain separate from ownership and write permission. |
| Explicit `load.d/` lists | Defer. They introduce another loading contract before modularity requires one. |
| Checked replacements | Keep as a later optional capability, preserving today’s companion meaning meanwhile. |
| Observe actual reads and behavior | Keep. Word count alone proves little. |

#### The tap-in model

The kernel consists of `AGENTS.md` and `.agents/loader.md`. It defines links, required reading, conditions, inherited Axioms, companion interpretation and activation boundaries. Every module may rely on that protocol and nothing else.

Loading tags and overwrites are optional capabilities with stable kernel meanings. Users turn off their use by removing loading tags or companions from the affected routes. Changing their interpretation globally would make identical files behave differently across installations.

The **opinion** is the unit of behavioral choice. Categories are optional containers. Presets are convenient selections.

Every module declares what it provides, what it requires and what happens when absent. Required cross-module dependencies must appear in its contract and installation plan. Required reads must also be represented by tagged Entries. No module may silently require a sibling.

| Module | Adds | Requires beyond kernel | When absent |
| --- | --- | --- | --- |
| Project sources | Navigation to existing knowledge | Nothing | Normal project navigation continues |
| Directives | Binding `Instructions` and scoped rule organization | Nothing | Existing native project instructions still work |
| Guidance | Adaptable recommendations | Nothing | Ordinary reasoning chooses approaches |
| Maps | Curated destinations and reading triggers | Nothing | Direct links remain available |
| Patterns | Inspectable reusable shapes | Nothing | No Open Forge shape convention |
| Skills | Discovery of native capability packages | Host capability only for execution | Native Skills remain usable independently |
| Templates | Copyable, independently maintained starting files | Nothing | Create artifacts normally |
| Memory container | Navigation to selected states | Nothing | No Memory namespace |
| Working | Temporary resumable records | Memory container | No prescribed work-record location |
| Emerging | Candidates with visible uncertainty | Memory container | Candidates may remain in existing notes |
| Crystallized | Accepted knowledge and local record maintenance | Memory container | Existing current documentation remains valid |
| Archived | Historical records outside current authority | Memory container | Existing history and retention practices continue |
| Loading tags | Eager reads and refresh points | Kernel protocol | Untagged routes remain on demand |
| Overwrites | Local changes to corresponding base content | A linked base | Edit or replace user-owned sources normally |
| Acceptance policy | Explicit acceptance evidence and conflict procedure | Nothing | Kernel still prevents tags and storage from establishing acceptance |
| Communication policy | Preferred response structure and detail | Nothing | User and host preferences govern |
| Capture policy | Deliberate automatic saving at stated triggers | A named destination | Saving happens when requested |
| Evergreen policy | Duties to update named current sources | Those sources | No framework-wide update duty |
| Pattern-default policy | Default conformance and departure reporting | Patterns | Shapes remain useful reference material |
| CLI adapter | Command triggers and usage guidance | Executable for acceleration only | Follow plain files |
| Extensions | Packaged methods, records and starters | Explicit package and module dependencies | No additional behavior |

Memory states do not require each other. Their entrypoints must never tell users to move something into an absent state. The container supplies navigation only.

Disabling removes an activation link and revises or disables its active dependents. Notes can remain on disk. Index and Update must preserve that decision. Removing Memory must also remove capture rules targeting it, rather than recreate Memory later.

Smallest installation:

```text
AGENTS.md
.agents/
  loader.md                 Entries empty
```

Full built-in installation:

```text
AGENTS.md
CLAUDE.md                   optional host bridge
.agents/
  loader.md
  project/_project.md
  directives/_directives.md
  guidance/_guidance.md
  maps/_maps.md
  patterns/_patterns.md
  skills/_skills.md
  templates/_templates.md
  memory/
    _memory.md
    working/_working.md
    emerging/_emerging.md
    crystallized/_crystallized.md
    archived/_archived.md
  policies/
    _policies.md
    acceptance.md
    communication.md
    capture.md
    evergreen.md
    pattern-defaults.md
  cli/
    _cli.md
    open-forge-cli/
      SKILL.md
      references/
        common.md
        discovery.md
        routes.md
        packages.md
  open-forge.json            CLI maintenance only
  open-forge.lock.json       CLI maintenance only
```

“Full” selects every built-in module. Extensions remain separately selectable and add their manifest-declared files. There is no finite fullest custom workspace.

This challenges the accepted product direction’s rejection of an empty routing substrate. An empty kernel should be a supported endpoint. The recommended installation should still provide useful navigation immediately.

#### Behavior without a harness

The model still judges relevance, understands task intent, identifies working files and resolves substantive uncertainty.

Structure determines required reads, conditions, active modules and dependencies. No required source should be hidden exclusively in prose.

The CLI batches reads, evaluates conditions, explains selection, checks references and previews lifecycle changes. The same Markdown defines behavior when it is unavailable.

Make CLI use concrete through the optional adapter. Its required entrypoint should say:

> Before opening startup routes separately, use `open-forge context` if available. Supply every known working path with `--for`. Read returned sources and resolve pending conditions. If unavailable, follow the loader directly.

This trigger belongs in the adapter, which does not require the Skills category. Its detailed Skill loads before maintenance commands. Native Skill discovery can expose it too.

That improves the likelihood of use without claiming enforcement.

#### The new loader

The default loader below is **474 words**, counting whitespace-separated words including headings and syntax. With its project entry removed, the kernel-only version is **464 words**. The managed `AGENTS.md` block is **20 words**.

```markdown
# Open Forge Loader

Read this at task start or resume, and after context restoration. These files work without a CLI.

## Axioms

### Read the active routes

1. Identify the files the task investigates, reviews, creates, changes, renames or deletes, including planned paths. Background reading does not make a source a working file.
2. Under `Entries`, read applicable #LoadNow and #KeepInMind destinations in listed order. Repeat inside each loaded entrypoint. Read each source once per pass.
3. Other entries open when their descriptions fit the task. Read the active ancestor entrypoints before using a routed source found through search.
4. Recheck when the goal or working files change. Reread active #KeepInMind sources before handoff or finish.

A condition shown as `applies to` filters reading and application. One working file must match a pattern in every conditioned entry along the chain. Apply conditioned rules only to files matching that whole chain. Patterns are case-sensitive workspace-relative globs. `**/*.cs` matches C# files at any depth. Unknown working files leave conditioned required reads pending. Resolve them before affected work.

### Use what is active

- An entrypoint's `## Axioms` bind its selected descendants. Narrower rules add to broader rules. Report unresolved conflicts.
- Each module defines its own content roles and any required instructions. A link, tag, folder name or saved note does not establish acceptance or grant permission.
- Only routes exposed through this loader are active Open Forge modules. Unlinked files may supply evidence without activating their former policies. Do not recreate omitted modules.
- A missing linked source is an error to report. An empty `Entries` section requires nothing.
- Read an adjacent `{name}.overwrite.md` immediately after its base. It replaces corresponding content within that source's role and scope, not other active rules.

### Maintain the links

Use `_{folder-name}.md` for an entrypoint. Existing `index.md`, `_index.md`, `references.md` and `_references.md` also work. Ordinary documentation needs no entrypoint unless it exposes routes.

Entries use relative Markdown links, descriptions, tags and optional file conditions. Links may reach elsewhere inside the workspace. Reading does not confer installer ownership or write permission.

For managed routed files, frontmatter supplies `description`, `tags`, optional `responsibility` and `applyTo`, at the root or under `open-forge:`. Follow the workspace's chosen form. Existing documentation and native Skills retain their own formats.

#LoadNow means read when the parent loads. #KeepInMind additionally means refresh at resume, after context restoration and before handoff or finish. Other tags are labels unless an active module defines a policy.

Required sources need an unbroken tagged chain from this loader. Required reads belong in Entries, not hidden prose. Keep optional reading untagged.

To disable a module, unlink its activation route and remove or revise active dependents. Preserve its notes if wanted. Record managed omissions for future updates. Manual users maintain these links directly.

## Entries

- [Project sources to consult for the task](project/_project.md) - #LoadNow
```

Managed `AGENTS.md` block:

```markdown
# Open Forge

Before starting a task, read `.agents/loader.md` and follow its reading steps.

**End of Open Forge managed section.**
```

#### The rest of the default install

The recommended default supplies three Markdown files:

- `AGENTS.md` supplies the managed handoff.
- `.agents/loader.md` supplies the kernel and project route.
- `.agents/project/_project.md` links existing project sources without relocating them.

A CLI-managed default additionally creates:

- `.agents/open-forge.json` records maintenance preferences and exclusions.
- `.agents/open-forge.lock.json` records managed ownership.

Offer `CLAUDE.md` when that bridge is wanted. It retains native imports rather than duplicating policy.

Complete example for a project that already has both linked documents:

```markdown
---
description: Project sources to consult for the task
tags: [LoadNow]
---

# Project sources

Provides: Navigation to existing project knowledge.
Requires: The Open Forge kernel.
When absent: Use the project's normal navigation.

## Axioms

## Entries

- [Project purpose, setup and commands](../../README.md) - #Project
- [Current system architecture](../../docs/architecture.md) - #Architecture
```

Installation links only existing destinations. With none available, it leaves Entries empty.

Replace fresh-install Essentials with this small default. Retain Full and Custom, adding opinion choices alongside category choices. Keep add, private-content and omit choices, but distinguish **disable behavior** from **release management** and **delete files**. Existing preset selections must not change silently.

#### Cost and migration

**CLI.** Preserve entrypoint names, Entries syntax, Axioms, frontmatter fields, same-file condition matching and companion behavior. Do not introduce a second loading mode.

Blended documentation requires real C# work. Admit contained `..` links, workspace-relative source identities, ordinary documents without Open Forge frontmatter and skipped intermediate folders. Reject ambiguous route ancestry. Keep ordinary cross-links separate from routing ancestry.

Root Entries must preserve authored membership. Index must not rediscover disabled roots. Installer selection needs module dependencies and safe deactivation, reusing package dependency machinery where appropriate. Module contracts remain readable Markdown, with matching installation metadata validated during development.

**Tests.** Update payload snapshots and install fixtures. Add focused checks for isolated modules, disabled roots surviving Index and Update, dependency removal, contained documentation links and plain-file versus CLI required-read parity.

**Docs.** Rewrite onboarding around one useful route. Move taxonomy, lifecycle and response preferences into module documentation. Update maintenance contracts and accepted decisions explicitly.

**Installed workspaces.** Offer a reviewed migration that preserves current behavior by enabling equivalent policies. Preserve overwrites and native imports. Show every changed activation link. Do not shrink existing installations merely because the new default is smaller.

Existing Extensions need explicit module requirements and removal checks. A method that supports existing `docs/` should declare that destination instead of assuming Crystallized Memory.

#### Risks and proof

Smaller defaults may provide too little immediate value. More choices may overwhelm users. Disabled content may still be imported through host-native files. Module removal can expose dependencies that were previously implicit. The kernel itself remains a non-optional protocol.

The cheapest useful experiment uses three unfamiliar projects, one for each audience above. Give their users the current setup and this design in separate fresh agent sessions.

Ask each to add one rule, route existing documentation, resume unfinished work and reject one opinion. Include a task whose paths become known later and a disabled module containing obsolete instructions.

Run without the CLI first, then repeat with it. Inspect actual reads, unintended policy activation, required-rule misses, files created and time until useful work. Require users to explain how to disable a policy without deleting their notes.

Success means fewer setup explanations and unwanted actions without additional required-rule misses. These are proposed experiments, not executed results. No files were changed and no builds, test suites or Doctor commands were run.

#### The one idea to steal

Make the loader’s links the complete, readable record of which Open Forge opinions are active.

## Editor (Proposal B)

#### 1. Thesis

Make Open Forge a reading contract with optional conventions. Ship two files, then let users add rules, continuity, reusable shapes, or methods independently. Spend startup words on behavior that models would otherwise miss.

#### 2. Who it is for

The repository shows a coherent problem, but no proven primary audience. The hobby critique is fair about focus and unsupported as a verdict on usefulness. `.agents/memory/crystallized/documents/vision.md` reaches toward “any person, project, team, discipline”, while `src/docusaurus/docs/highlights.md` names concrete problems.

- Solo developers repeating corrections want persistent rules without a planning system.
- Maintainers of existing projects want agents to consult their docs and ADRs without relocating them.
- Monorepo teams want file-specific conventions applied consistently across packages.
- People switching sessions or agents want reliable resumption without recording every conversation.

The protected Authors’ Findings notebook, sealed Analysis-to-Task lifecycle, exact orchestration progress formats, and top-down document maintenance chiefly address this maintainer’s accumulated context. Evidence is in `.agents/memory/emerging/authors-findings/open-forge-findings-log.md`, `.agents/memory/emerging/authors-findings/_authors-findings.md`, and `.agents/directives/hierarchical-orchestration.md`. These are useful specialization examples, not a universal product baseline.

#### 3. Why it feels complicated and controlling

1. It teaches ordinary filesystem knowledge. “Concrete folder name in a route path.” appears in `src/open-forge/.agents/loader.md:49`.
2. Required behavior can hide behind relevance judgment. “Neither activates an unselected ancestor or scope.” appears in that loader at line 81.
3. It schedules reflection universally. “At each applicable #KeepInMind refresh, check visible `Entries` and save useful candidate material before it is lost.” appears in `src/open-forge/.agents/memory/emerging/_emerging.md:17`.
4. It makes taxonomy a permission boundary. “Get explicit user direction before creating a new top-level Memory state.” appears in `src/open-forge/.agents/memory/_memory.md:37`.
5. Optional tooling consumes mandatory context. “The Skills entrypoint requires this guide at startup.” appears in `src/open-forge/.agents/skills/open-forge-cli/SKILL.md:8`.
6. Customization requires understanding update mechanics. “Replaces your edited copy with the current shipped version and reports it.” appears in `src/docusaurus/docs/concepts/customizing.md`.

The sentence audit below uses physical line numbers. **K** keeps, **C** compresses, **M** moves to its point of use, and **D** deletes. Every unlisted prose sentence is D. K covers titles, Axioms and Entries identifiers, metadata keys, link syntax, and empty placeholders. Compress descriptions and regenerate membership. Removing explanatory prose does not change parser syntax.

Here, P is `src/open-forge/` and S is `artifacts/council-1/step1-files/open-forge/`. Paths below are relative to either.

| File | P disposition | S disposition |
| --- | --- | --- |
| AGENTS.md | C5–7 | C5 |
| .agents/loader.md | C9,26–27,30–32,36,38–40,54,60–62,70,74–75,79–94,118–122. M7,28–29,46–59,66–69,95–117,130–138 to authoring and command references, excluding C lines | C9–15,19,21,30–31,33. M22–23 to Memory, M28–29,32,34 to authoring references |
| .agents/directives/_directives.md | C15–18. M20–22 to loader | C13–15. M16 to loader |
| .agents/guidance/_guidance.md | C17. M16 to starter instructions | C15. M14 to starter instructions |
| .agents/maps/_maps.md | C18,25 | C14 |
| .agents/patterns/_patterns.md | C19,23–29 | C14–16 |
| .agents/skills/_skills.md | C17. M15’s command-use advice to CLI Skill | C13. M14 to CLI Skill |
| .agents/templates/_templates.md | C18–24. M28–30 to starter instructions | C14–15 |
| .agents/memory/_memory.md | C24–28,32–36. M19–20 to loader | C13–15,17. M16 to loader |
| .agents/memory/working/_working.md | C17–24 | C13–14,16 |
| .agents/memory/crystallized/_crystallized.md | C16,18–19. M17 to loader | C14–15, acceptance explanation moved to loader |
| .agents/memory/emerging/_emerging.md | C19,23–25 | C13–15 |
| .agents/memory/archived/_archived.md | C16–21 | C13–16 |
| .agents/skills/open-forge-cli/SKILL.md | C25,27–35,39–44. M10–12,18–23 to existing references | Same, deleting the old startup obligation |

For mixed lines, the replacement texts below define the retained behavior. Deliberate drops include prescribed response format, category question headings, obligatory taxonomy approval, mandatory Emerging refresh, separate Pattern-departure reporting, and broad-only Maps. These are policy changes, not lossless shortening.

#### 4. What to keep from Council 1

- **Keep** procedure first, shorter entrypoints, and removal of the inherited marker. They remove teaching without removing obligations.
- **Keep** unbroken required chains, same-file condition matching, pending unknown paths, search recovery, and additive ancestor rules. They prevent real loading errors.
- **Keep** on-demand Guidance, Maps, Patterns, Skills, and Templates. Their presence need not impose startup reads.
- **Change** mandatory Directives and Memory installation to opt-in modules. Installed rules still bind.
- **Change** simplified KeepInMind to retain all current refresh points. Timing costs few words and supports continuity.
- **Change** retirement of six tag definitions into explicit migration. Preserve existing acceptance meanings and translate Evergreen duties into chosen rules.
- **Keep** acceptance boundaries and corresponding-content overwrites. Neither needs a new parser.
- **Keep** direct documentation links and separate reading from write ownership. Adopt existing project structure.
- **Move** authoring, package rules, source maintenance, and continuity to their optional destinations. Drop universal reply coaching.
- **Defer** load.d and checked replacements. Benchmark them before adding another loading mode or syntax.
- **Keep** trace-based experiments. Word counts alone cannot establish reliability.

#### 5. The tap-in model

The kernel is the host handoff, a required-reading procedure, scope and overwrite rules, and the distinction between reading, acceptance, and permission. No content taxonomy is required.

Choose conventions independently. Directives prevent repeated mistakes. Guidance preserves tradeoffs. Maps reuse existing knowledge. Patterns preserve useful shapes. Templates save setup. Skills expose capabilities. Memory preserves continuity.

Reject a rule by editing or removing it, or replacing its corresponding content through an overwrite. Removing a loading tag changes timing, not the rule’s force when read. For example, replace Memory’s closeout axiom with “Save only when asked.” Remove an entire module by removing its loader entry and repairing incoming references. Existing user records remain yours. With managed updates, record exclusions too. Without the CLI, edit the files directly.

This challenges `.agents/memory/crystallized/decisions/product/product-direction.md`’s rejection of an empty routing substrate. It also challenges the Principles’ requirement that defaults provide more value than an empty substrate. A guided first link must prove that tradeoff. User ownership, relevance, acceptance, and honest reliability remain foundational.

Smallest and recommended default:

```text
AGENTS.md
.agents/loader.md
```

Fullest built-in setup, with CLI help and the optional host bridge:

```text
AGENTS.md
CLAUDE.md
.agents/
  loader.md
  directives/_directives.md
  guidance/_guidance.md
  maps/_maps.md
  patterns/_patterns.md
  templates/_templates.md
  skills/_skills.md
  skills/open-forge-cli/SKILL.md
  skills/open-forge-cli/references/common.md
  skills/open-forge-cli/references/discovery.md
  skills/open-forge-cli/references/packages.md
  skills/open-forge-cli/references/routes.md
  memory/_memory.md
  memory/working/_working.md
  memory/emerging/_emerging.md
  memory/crystallized/_crystallized.md
  memory/archived/_archived.md
```

Extensions remain separately selectable complete-file packages. An existing human document can participate directly:

```markdown
- [C# conventions](../docs/contributing/csharp.md) - #LoadNow - applies to `**/*.cs`
```

This row belongs in the loader. It requires no catalogue in either intermediate docs folder.

#### 6. Behavior without a harness

The model still judges relevance, working files, factual conflicts, and worthwhile memories. Structure determines required reads, inherited conditions, refresh timing, and overwrite order.

The CLI computes reading closure, batches bodies, checks paths, maintains navigation, and previews changes. It cannot infer acceptance or prove compliance. Put the executable context command beside the reading procedure, and expose the Skill through native discovery when available. Give the model an immediate saving in file calls.

Plain-file readers execute the same procedure. Ordinary docs need neither relocation nor Open Forge frontmatter merely to be linked. Each routed source has one catalogue parent. Other cross-links remain references. Index must preserve authored external membership.

#### 7. The new loader

Counts use whitespace-separated words, including metadata, headings, and Entries. The loader is **2,052 → 326**, compared with **799** in the Step 1 artifact. The managed block is **43 → 18**. Empty-kernel startup is **344 words**.

```markdown
# Open Forge Loader

## Axioms

At task start, resume, or context restoration:

1. Identify known working files, including planned files and both paths of renames. Background reading does not count.
2. Under `## Entries`, read applicable #LoadNow and #KeepInMind links in listed order, recursively, once per pass. Descriptions cannot excuse skipping a required read.
3. Select other branches by relevance. Follow explicit required reads in loaded sources. Before using routed Markdown found through search or a direct link, read its ancestor entrypoints.

`applyTo`, shown as `applies to`, filters reading and application. Patterns within one list are alternatives. One working file must satisfy every conditioned ancestor and the destination. Apply conditioned rules only to files satisfying that chain. Unknown working files leave required conditioned entries pending. Resolve them before affected work. Globs are case-sensitive and workspace-relative. `*.py` matches root files, `**/*.py` any depth. Leading `!` is literal.

Read an adjacent `{name}.overwrite.md` immediately after `{name}.md`. It changes corresponding base content only, within the same scope.

Recheck selection when the goal or working files change. Refresh active #KeepInMind sources at start, resume, context restoration, before handoff or finish, and when they may have changed.

Follow this loader's and loaded entrypoints' `## Axioms`, plus loaded Directives' `## Instructions`, within their scopes. Descendant rules add to ancestor rules. Report unresolved conflicts rather than choosing precedence.

Other sources define accepted requirements only through user acceptance or declared authority. Saving, moving, tagging, linking, or loading never establishes acceptance or permission. Investigate conflicting accepted sources before changing either.

Use `open-forge context` to batch reading when available, repeating `--for <path>` for every known working file. Read its output and explicit required links. Otherwise follow the same rules with file tools. Report missing required sources.

Entries can link anywhere inside the workspace. Only #LoadNow and #KeepInMind change loading. Always-required sources need an unbroken loading-tag chain from here. Other tags are labels. Omitted modules impose no duties.

## Entries

- none - No entries - #Empty
```

Managed `AGENTS.md` block:

```markdown
# Open Forge

Before working, read `.agents/loader.md` and follow its reading rules.

**End of Open Forge managed section.**
```

#### 8. The rest of the default install

The plain-file default ships exactly two files:

- `AGENTS.md` provides the host handoff.
- `.agents/loader.md` provides the reading contract.

A CLI installation also generates `.agents/open-forge.json` for maintenance settings and `.agents/open-forge.lock.json` for ownership. Neither is needed for reading. Everything below is optional. The table gives shipped → Step 1 → proposed counts and each file’s purpose. Entrypoint texts are complete, including the full Memory example.

| File under .agents/ | Words | Purpose |
| --- | --- | --- |
| directives/_directives.md | 176 → 162 → 51 | Binding rules |
| guidance/_guidance.md | 115 → 90 → 34 | Adaptable advice |
| maps/_maps.md | 168 → 110 → 40 | Existing sources |
| patterns/_patterns.md | 363 → 167 → 63 | Reusable shapes |
| skills/_skills.md | 174 → 131 → 34 | Capability discovery |
| templates/_templates.md | 246 → 98 → 53 | Independent copies |
| memory/_memory.md | 467 → 230 → 96 | Selective capture |
| memory/working/_working.md | 127 → 112 → 59 | Resumption |
| memory/crystallized/_crystallized.md | 120 → 91 → 61 | Accepted knowledge |
| memory/emerging/_emerging.md | 168 → 103 → 52 | Candidates |
| memory/archived/_archived.md | 178 → 109 → 69 | Retained history |
| skills/open-forge-cli/SKILL.md | 590 → 590 → 131 | Command selection |

`CLAUDE.md` stays at 11 words. Its complete bridge is:

```markdown
# Open Forge

@AGENTS.md
@.agents/loader.md

**End of Open Forge managed section.**
```

`.agents/directives/_directives.md`

```markdown
---
description: Binding rules for this scope
tags: [LoadNow, Core, Directive]
---

# Directives

## Axioms

- Put binding rules under `## Instructions`, with their conditions and exceptions.
- Tag rule files and required child entrypoints #LoadNow. Narrow file-specific branches with `applyTo`.

## Entries

- none - No entries - #Empty
```

`.agents/guidance/_guidance.md`

```markdown
---
description: Advice for recurring choices
tags: [Core, Guidance]
---

# Guidance

## Axioms

- Adapt recommendations when another approach fits better. Explain material differences.

## Entries

- none - No entries - #Empty
```

`.agents/maps/_maps.md`

```markdown
---
description: Sources to consult and when
tags: [Core, Map]
---

# Maps

## Axioms

- Link existing sources and say when to read them. Keep their detail at the destination.

## Entries

- none - No entries - #Empty
```

`.agents/patterns/_patterns.md`

```markdown
---
description: Default shapes for related work
tags: [Core, Pattern]
---

# Patterns

## Axioms

- Show one accepted reusable shape with a valid example, or label it schematic. Label illustrative parts.
- Follow matching shapes by default. Explain material departures before dependent work.
- Link binding rules rather than embedding them in examples.

## Entries

- none - No entries - #Empty
```

`.agents/skills/_skills.md`

```markdown
---
description: Capabilities for specialized work
tags: [Core, Skill]
---

# Skills

## Axioms

- Read the matching `SKILL.md` before using a capability.

## Entries

- [Batch context reads and maintain navigation](open-forge-cli/SKILL.md) - #Skill
```

`.agents/templates/_templates.md`

```markdown
---
description: Starting files to copy and adapt
tags: [Core, Template]
---

# Templates

## Axioms

- Keep needed sections, replace placeholders and metadata, repair relative links, and remove irrelevant tags, including #Template unless creating another Template.
- Copies evolve independently of their starters.

## Entries

- none - No entries - #Empty
```

`.agents/memory/_memory.md`

```markdown
---
description: Save useful context for later work
tags: [LoadNow, Memory]
---

# Memory

## Axioms

- Before handoff or finish, save only what future work needs in an installed state or existing project source. Reuse records.
- Distinguish stated, observed, and inferred material when trust depends on it.
- Do not recreate omitted states or categories.

## Entries

- [Useful history](archived/_archived.md) - #Memory #Archived #Contextual #Historical
- [Accepted current knowledge](crystallized/_crystallized.md) - #LoadNow #Memory #Crystallized #CurrentTruth
- [Unaccepted findings and ideas](emerging/_emerging.md) - #Memory #Emerging #Contextual #Candidate
- [State needed to resume](working/_working.md) - #LoadNow #Memory #Working #Contextual
```

`.agents/memory/working/_working.md`

```markdown
---
description: State needed to resume unfinished work
tags: [LoadNow, Memory, Working, Contextual]
---

# Working Memory

## Axioms

- At start or resume, read the relevant work record.
- Keep the goal, progress, next step, and blockers current.
- When work ends, preserve useful results before retiring the record.

## Entries

- none - No entries - #Empty
```

`.agents/memory/crystallized/_crystallized.md`

```markdown
---
description: Accepted knowledge that stays current
tags: [LoadNow, Memory, Crystallized, CurrentTruth]
---

# Crystallized Memory

## Axioms

- Keep only accepted current knowledge here.
- Update its defining source when accepted direction changes, before dependent work and no later than closeout.
- Link useful history instead of keeping competing current copies.

## Entries

- none - No entries - #Empty
```

`.agents/memory/emerging/_emerging.md`

```markdown
---
description: Findings and ideas awaiting acceptance
tags: [Memory, Emerging, Contextual, Candidate]
---

# Emerging Memory

## Axioms

- Record source, uncertainty, and scope.
- Move accepted results to their defining sources.
- Consolidate or retire stale candidates when useful, retaining valuable reasoning.

## Entries

- none - No entries - #Empty
```

`.agents/memory/archived/_archived.md`

```markdown
---
description: History that no longer governs work
tags: [Memory, Archived, Contextual, Historical]
---

# Archived Memory

## Axioms

- Preserve current results elsewhere before archiving. Record provenance, the reason, and any replacement.
- Remove loading tags and claims of current authority.
- Delete only under user direction or an accepted retention rule.
- Revalidate and establish acceptance before restoration.

## Entries

- none - No entries - #Empty
```

`.agents/skills/open-forge-cli/SKILL.md`

```markdown
---
name: open-forge-cli
description: Batch context reads and maintain Open Forge navigation with the optional CLI.
---

# Open Forge CLI

Run `open-forge context` with repeated `--for <path>` for known working files. Batch selected source IDs in one later call. Path filters do not select hidden scopes. Read returned instructions and their explicit required links. Resolve pending conditions before affected work.

Before other operations, read the matching reference:

- [Discovery](references/discovery.md): search, context, references, and inspection.
- [Routes](references/routes.md): metadata, navigation, moves, repair, and removals.
- [Packages](references/packages.md): installation, configuration, Extensions, and Libraries.
- [Results](references/common.md): flags, previews, permissions, and exit codes.

Use command help for installed syntax. Preview mutations with `--dry-run` and inspect the plan before applying authorized changes. Check semantic status and exit code. Report unresolved boundaries. Tool flags grant no user authorization.
```

The four optional CLI references retain command detail and absorb moved authoring rules. Their current counts are common 828, discovery 1,013, packages 2,850, and routes 1,892. They cover result handling, retrieval, lifecycle, and file maintenance respectively.

Add a recommended Kernel preset. Keep Essentials and Full Core as explicit choices. Custom retains Add, Git-ignore, and Leave out. Offer CLI help independently of other conventions. Generate Entries only for present children. Selecting Memory without a state must use existing project sources, never recreate omitted folders.

#### 9. Cost and migration

Text reduction, loading-tag defaults, and Skill trimming need payload snapshots and installation/context expectation updates, not new loading syntax.

Blended routing needs C# changes. Admit workspace-contained `..` links, ordinary Markdown targets without required frontmatter, skipped intermediate folders, stable source identity, and one unambiguous catalogue ancestry. Reject escapes and competing parents. Preserve authored links during Index. Keep outside sources read-only to maintenance commands. Test containment, conditions, missing sources, overwrites, and CLI/manual read-set equivalence. Release the external-link promise with that support.

The Kernel preset needs installer choices, ownership and exclusion handling, help, snapshots, and unattended-mode tests. Settings and locks remain maintenance records, never runtime requirements.

Do not shrink existing workspaces automatically. Preview a versioned conversion retaining current reads and obligations. Translate retired tag duties into explicit rules, preserve custom content and private records, and leave ambiguous cases unchanged. Update dogfood contracts, README, installation, concepts, limits, demos, and maintenance references together. Put the manual two-file start on the front page.

#### 10. Risks and proof

The empty start may feel useless. Optional Memory may lose continuity. Shorter prose may conceal scope errors. Required chains still depend on correct authoring, and overwrites can drift.

Recruit three non-maintainers with a small project, existing documentation, and a monorepo. Give each loader one minute, then ask them to predict required reads and disable one opinion. Compare current, Step 1, and this design on the same small task and resume, without the CLI. Include unknown paths, a second scope, an overwrite, and an unaccepted conflicting note. Hand-author expected reads. Match enabled policies across versions so deleting obligations cannot masquerade as better compliance. Evaluate the no-Memory choice separately, with resumption left to the user's existing practice.

Score actual traces, scope leakage, lost resume state, unaccepted claims, configuration mistakes, and time to first useful work. Then enable the CLI and compare read sets and file-call counts. Require zero missed applicable obligations before claiming success. This proposal used read-only inspection. No builds or tests were run.

#### 11. The one idea to steal

Make every default removable without making the user learn the rest of the framework.

## Council 1 Critic (Proposal E)

#### 1. Thesis

Council 1 repaired loading mechanics, but left the adoption contract largely intact. Open Forge should contribute exactly the context a workspace chooses, without requiring its taxonomy, Memory lifecycle or working methods. Keep reliable reading, make existing documentation directly usable, and turn the rest into independent choices.

#### 2. Who it is for

The audience critique is partly right. The repository identifies useful jobs, but presents a broad operating layer before choosing which users it serves best. `vision.md` extends the ambition across people, projects and disciplines, while `highlights.md` provides much more concrete reasons to adopt it.

Prioritize three users:

| User | What they actually want | Useful capabilities |
| --- | --- | --- |
| Solo developer repeatedly correcting an agent | Make one correction survive the next session | Small required rules, optional continuation notes |
| Team maintaining an existing codebase | Agents respect existing documentation and preserve important reasons | Direct documentation links, scoped rules, existing ADRs |
| Maintainer using several agents or working across packages | Share conventions without loading every package’s context | Scoped catalogues, native Skills, optional CLI retrieval |

These are plausible audiences, not validated market segments.

The maintainer’s own findings explain the emphasis on document ownership, sealed Analysis, actionable Tasks and lifecycle transitions. `.agents/memory/emerging/authors-findings/open-forge-findings-log.md` explicitly describes those concerns. The repository’s orchestration directives and project-control machinery also serve sustained, coordinated development. They are useful evidence and optional capabilities, but weak candidates for universal defaults.

#### 3. Why it feels complicated and controlling

1. **Optional tooling leads the adoption story.** `src/docusaurus/docs/getting-started/installation.md` begins: “Setup has three steps: install the CLI, use it to install the Framework into your project, then review and commit the result.” The subsequent optionality qualification has to undo that first impression.

2. **Users encounter a classification system before obtaining value.** `src/docusaurus/docs/concepts/core-categories.md` says: “Core gives content six roles, called categories.” Together with four Memory states, this creates substantial conceptual onboarding despite most installed categories starting empty.

3. **Navigation dictates physical structure.** `src/open-forge/.agents/loader.md` says: “Every folder in a route chain has exactly one recognized entrypoint.” Ordinary documentation inherits an agent-oriented maintenance requirement if this becomes the only integration model.

4. **Required discovery depends on earlier selection.** The same loader says: “These tags act only through loaded parents. Neither activates an unselected ancestor or scope.” Council 1 repairs reachability through authoring discipline, but that discipline is another concept users must maintain.

5. **Local disagreement cannot cancel a broad opinion.** The loader says: “Adding a child instruction does not cancel an active ancestor rule.” This is defensible rule composition, but makes editing the original source essential. Customization cannot merely mean adding another file.

6. **Memory introduces work beyond the requested task.** `src/open-forge/.agents/memory/_memory.md` says: “Before closeout, also preserve unsettled findings worth revisiting and temporary state needed to resume or transfer unfinished work.” It also requires: “Get explicit user direction before creating a new top-level Memory state.” Capture and placement become framework concerns even when users already have suitable conventions.

7. **The loader governs conversation style.** It says: “Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.” That is a collaboration preference, not necessary routing machinery.

8. **Direct customization is temporary under updates.** `src/docusaurus/docs/concepts/customizing.md` describes Update this way: “Replaces your edited copy with the current shipped version and reports it.” Users must learn overwrites to make ordinary disagreement durable.

#### 4. What to keep from Council 1

| Decision | Disposition and reason |
| --- | --- |
| Teach reading before vocabulary | **Keep.** Its clearest improvement needs no qualification. |
| Compute required reads through loading tags | **Keep.** Preserve existing semantics rather than invent another reading language. |
| Same-file matching across inherited conditions | **Keep unchanged.** Different files must not accidentally satisfy different parts of one chain. |
| Unknown paths leave conditions pending | **Keep unchanged.** Resolve them before affected work. |
| Unbroken required chains | **Change presentation.** Keep the invariant for nested collections, but start users with direct links rather than teaching chains first. |
| Guidance, Maps, Patterns, Skills and Templates on demand | **Keep, then extend.** Directives and Memory must also be optional installation choices. |
| Working and Crystallized required by default | **Drop.** Continuity organization is an opinion, not the reading kernel. |
| Emerging loses its default refresh tag | **Keep unchanged.** Refresh actual continuation records when needed. |
| Move authoring detail and CLI documentation off startup | **Keep.** A small authoring guide supplies the missing explanation. |
| Universal response recipe | **Drop from the kernel.** Offer it as editable collaboration advice. |
| Blended documentation | **Keep and prioritize.** Individual documents must work without a new catalogue or metadata. |
| `load.d/` as the possible destination | **Defer.** One authored loader list is enough until package composition demonstrates otherwise. |
| Checked overwrites | **Keep deferred.** Preserve existing companion semantics without making companions necessary for durable customization. |
| Behavioral experiments | **Keep and broaden.** Test adoption, rejection and upgrades alongside required reads. |

One Step 1 regression deserves correction. Its Templates entrypoint says, “Before creating a new artifact, check these Entries for a matching Template.” Today’s text says, “Check `Entries` when copy-ready starting content would help with a new artifact.” Restore the conditional trigger. Moving a category on demand should not strengthen its obligations.

#### 5. The tap-in model

The irreducible kernel is a handoff from `AGENTS.md` and a readable context list in `.agents/loader.md`. It defines required versus optional reading, applicability and explicit source relationships.

The recommended default adds one authoring guide. It ships no mandatory content categories or Memory policy.

This explicitly challenges `product-direction.md`’s rejection of an “empty routing library.” Precise reading rules plus reuse of existing project knowledge may provide sufficient starting value. That is a proposed product decision requiring evidence, not an accepted change.

Users can:

- Link existing instructions directly, without adopting Directives.
- Keep ADRs in `docs/adr/`, without adopting Decisions.
- Use a continuation note without installing four Memory states.
- Add one category or Extension when its specific benefit becomes useful.
- Keep the complete opinionated organization when they want it.

Reject one opinion by editing its defining text. Reject a collection by removing its loader entry. Preserve its files if useful. An unlisted collection does not activate merely because its directory exists.

The loader list becomes user-controlled. Index must not recreate removed root entries. Update preserves edited and missing files, reports divergence and offers upstream changes for review. Restoration requires an explicit request.

Nested catalogue inheritance remains available. Rejecting an inherited rule means changing its defining source, not creating a contradictory child.

The smallest installation is:

```text
AGENTS.md
.agents/
  loader.md
```

The fullest built-in preset is:

```text
AGENTS.md
CLAUDE.md
.agents/
  README.md
  loader.md
  open-forge.json
  open-forge.lock.json
  directives/_directives.md
  guidance/_guidance.md
  maps/_maps.md
  patterns/_patterns.md
  templates/_templates.md
  skills/
    _skills.md
    open-forge-cli/
      SKILL.md
      references/
        common.md
        discovery.md
        packages.md
        routes.md
  memory/
    _memory.md
    working/_working.md
    emerging/_emerging.md
    crystallized/_crystallized.md
    archived/_archived.md
```

This is 20 Markdown files, plus CLI maintenance files when managed. Extensions remain separate selections.

Each retained opinion still earns its place. Directives make corrections explicit. Guidance explains tradeoffs. Patterns expose expected structure. Templates reduce repeated writing. Maps curate sources. Memory distinguishes temporary, candidate, accepted and historical material. Skills preserve native capability packaging. None requires adopting the others.

#### 6. Behavior without a harness

The model still identifies working files, selects relevant optional knowledge, interprets meaning and judges whether a finding is useful. Markdown cannot eliminate those decisions.

Structure handles required reading: explicit links, loading tags, visible conditions and declared ancestry. Required file rules must be reachable without a relevance judgment about an optional parent.

The CLI batches bodies, evaluates conditions, explains selection reasons and reports unresolved targets. It does not infer acceptance or prove that the model followed instructions. Prose-required reads remain additional obligations.

Plain files expose the same links, conditions and instructions. Neither JSON maintenance records nor CLI availability determines correct behavior.

To encourage actual CLI use, put one concrete `context` command at the start of the reading procedure. Offer the CLI Skill through native discovery when supported. Demonstrate the saved calls and returned bodies. After an unavailable or incompatible command, fall back to files without repeated probing. Measure command use rather than assuming the instruction succeeds.

#### 7. The new loader

This is the target text after the CLI compatibility work below. Counts use whitespace-separated words, including Markdown syntax: **426 words** for the loader and **22** for the managed block. Empty startup therefore contributes **448 words**, excluding existing host instructions. The authoring guide is not a startup read.

```markdown
# Open Forge Loader

This file chooses the context Open Forge contributes. Existing project instructions remain in effect. Use the entries you need and leave the rest out.

## Axioms

### Read context

1. At task start, resume, or context restoration, read this file. If `open-forge` is available, prefer `open-forge context` with a repeated `--for <path>` for every known working file. Otherwise follow the same links manually. Read the returned bodies, not just their paths.
2. Under `## Entries`, read applicable #LoadNow and #KeepInMind links in listed order. Repeat inside each entrypoint you read. An entrypoint is a folder index such as `_directives.md`. Read each source once per pass, including files explicitly required by its instructions.
3. An `applies to` list contains alternative case-sensitive globs from the workspace root. `**/*.cs` matches C# files at any depth. A source applies only where one working file matches its own list and every conditioned ancestor's list. Working files are files being investigated, reviewed, created, changed, renamed or deleted, including planned files. For renames, include both paths. Background reads do not count. Keep unknown conditions pending until paths are known, before affected work.
4. Open other entries when their descriptions fit the task. Read ancestor entrypoints before using a routed search result. Ordinary documents linked directly need no new folders, entrypoints or frontmatter.
5. Recheck required links when the task or working files change. Read newly applicable sources. Refresh active #KeepInMind sources at task start or resume, after context restoration, and before handoff or finish.
6. Read an adjacent `{name}.overwrite.md` immediately after its base. It replaces corresponding base content within that source's scope.

### Use context

- Follow project instructions within their stated scope, including loaded entrypoint `## Axioms` and Directive `## Instructions`. Descendants add rules and do not cancel ancestors. Report unresolved conflicts. Advice remains advice. Reading a document or a tag does not make a proposal accepted.
- Keep the project's declared sources authoritative for their subjects. A link grants neither edit permission nor installer ownership.
- An omitted category creates no obligation. Do not recreate it or impose its placement, capture, workflow or review conventions.
- Report a missing required target. An intentionally empty Entries section is valid.

### Change this list

Link existing files with relative paths. Keep links inside the workspace. Add #LoadNow only for required reads and #KeepInMind only when rereading matters. For nested required reads, every exposing entrypoint must also be required. Other tags are search labels. Update entries by hand without the CLI.

## Entries

- none - No entries - #Empty
```

Managed `AGENTS.md` block:

```markdown
# Open Forge

Before working, read `.agents/loader.md` for the context this workspace has chosen to use.

**End of Open Forge managed section.**
```

Unlike Council 1, changing working files reevaluates applicability without requiring another complete read of unchanged startup material.

#### 8. The rest of the default install

The manual default ships exactly three files:

| File | Purpose |
| --- | --- |
| `AGENTS.md` | Managed handoff, preserving surrounding project instructions |
| `.agents/loader.md` | Reading contract and user-controlled context selection |
| `.agents/README.md` | Instructions for linking one existing document, adding one rule, maintaining metadata, rejecting opinions and updating safely |

A CLI-managed installation also creates:

| File | Purpose |
| --- | --- |
| `.agents/open-forge.json` | Existing settings and exclusions, never required for semantic interpretation |
| `.agents/open-forge.lock.json` | Existing maintenance ownership record, never behavioral authority |

The optional `CLAUDE.md` bridge retains native imports of the canonical files.

Essentials becomes this small default. Full Core retains the existing collections and CLI Skill, with Directives exposed for startup and other root collections on demand. Custom retains add, Git-ignore and leave-out choices. Configuration must distinguish releasing management from removing a context link, since today’s Custom Remove deliberately keeps existing content routable.

An optional collection can be this simple. Save it as `.agents/context/_context.md` and expose it through a loader entry tagged `#LoadNow`:

```markdown
---
description: Project documentation to consult for implementation
tags: [Context]
---

# Project context

These documents stay in their existing locations and retain their declared roles.

## Entries

- [System architecture](../../docs/architecture.md)
- [C# contribution rules](../../docs/contributing/csharp.md) - #LoadNow - applies to `**/*.cs`
```

The documents need no Open Forge frontmatter. This collection is an example, not another default.

#### 9. Cost and migration

**CLI.** Keep entrypoint names, headings, metadata forms, tag meanings and inherited condition matching. Add workspace-contained links outside `.agents/`, direct document targets and source identities for those targets. For ordinary external documents, authored entries supply routing metadata without rewriting their contents. Preserve unique declared ancestry, reject cycles and ambiguous parents, and keep external sources read-only during retrieval.

Index needs an authored-membership boundary for loader Entries. It may refresh navigation within selected managed collections, but cannot enroll another root merely because a folder exists.

Update changes materially. It must preserve edited and missing files, report differences and reserve restoration for explicit configuration. Existing overwrite companions retain their meaning.

**Tests.** Update payload snapshots and Install, Configure, Update, Index and Context fixtures. Add cases for metadata-free external documents, retained opt-outs, missing required targets, inherited conditions, private records and authored loader membership. Verify plain-file and CLI required sets against the same expectations.

**Documentation.** Rewrite installation around the first useful link. Update the audience promise, startup counts, customization guarantees and relevant maintenance contracts. Clearly document the change from replace-on-update behavior.

**Installed workspaces.** Preserve current route selections and loading behavior until an explicit migration. Do not delete categories, move documents or reinterpret overwrites automatically. Preview changes, retain old bytes and report unresolved references. Older CLIs must not be presented as supporting the new external-link contract.

These are several bounded changes, not a Markdown-only release.

#### 10. Risks and proof

The default may feel too empty. An authored loader list can become stale. Retaining edited files can leave users on old framework behavior. Optional Memory may capture less useful continuity. Nested chains remain a learning cost for larger installations.

The cheapest useful experiment recruits one person from each proposed audience. Give each an existing repository and compare Council 1 with this design in fresh sessions, without the CLI first.

Ask them to connect an existing document, add a scoped correction, reject one opinion, resume work and inspect an update preview. Record actual file reads and resulting changes.

The design earns further work if users obtain useful behavior within ten minutes, required rules are not missed, rejected opinions remain absent after update, and existing documentation needs no relocation. Repeat one task with the CLI to measure retrieval effort and actual command uptake.

This pilot can falsify the proposal cheaply. It cannot establish broad reliability.

#### 11. The one idea to steal

Make the loader an explicit statement of what this workspace chooses to use, and make updates preserve that choice.
