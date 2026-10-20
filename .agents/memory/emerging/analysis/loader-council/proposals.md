---
open-forge:
  description: Sanitized Stage 1 proposals from the ten loader council members, each with a complete replacement loader
  tags: [Memory, Analysis, Contextual, Evidence, Framework, Loader]
---

# Loader Council Proposals

Stage 1 output, unchanged except for removed local paths and runtime labels. Each member wrote independently from the same brief. Letters are the anonymous labels used in peer review. The [record](record.md) holds the synthesis.

## Prosecutor (Proposal D)

#### Thesis

Required context should be an explicit, path-matched checklist whose completeness can be inspected. Keep recursive navigation for optional knowledge, but remove model-selected ancestors from mandatory rule discovery. Let checklist links point directly to ordinary project documents wherever they already live.

#### Diagnosis

1. **Mandatory discovery depends on optional judgment.**  
   “Select what matters from descriptions, tags, paths, ancestor routes, and explicit links.” Later, “`applyTo` never loads a file by itself.” Both appear in [src/open-forge/.agents/loader.md](src/open-forge/.agents/loader.md:54). A correct path match cannot rescue a missed parent. The model must discover every relevant branch before the supposedly mechanical conditions help.

2. **Instruction syntax carries unnecessary bureaucracy.**  
   “Existing missing or empty local sections also add no rules.” — [src/open-forge/.agents/loader.md](src/open-forge/.agents/loader.md:37).  
   “Each Directive file keeps its instructions under a non-empty `## Instructions` heading.” — [src/open-forge/.agents/directives/_directives.md](src/open-forge/.agents/directives/_directives.md:16). Readers learn inherited Axioms, separate Instructions, and an explicit empty marker that means the same thing as absence. These distinctions increase interpretation without improving the resulting behavior.

3. **There are two required-read contracts.**  
   “The CLI follows loading tags, not prose instructions.” — [src/open-forge/.agents/skills/open-forge-cli/SKILL.md](src/open-forge/.agents/skills/open-forge-cli/SKILL.md:35). The Skills entrypoint requires the CLI guide through prose. Consequently, CLI startup closure and required startup reading differ. A successful command is insufficient evidence of complete retrieval.

4. **Refresh obligations contain an unobservable judgment loop.**  
   “At each applicable #KeepInMind refresh, check visible `Entries` and save useful candidate material before it is lost.” — [src/open-forge/.agents/memory/emerging/_emerging.md](src/open-forge/.agents/memory/emerging/_emerging.md:17). Applicability depends on earlier selection, while usefulness depends on predicting future value. An auditor cannot distinguish thoughtful omission from forgotten capture.

5. **Acceptance lacks a required witness.**  
   “Acceptance establishes what may be treated as current within its scope.” — [src/open-forge/.agents/loader.md](src/open-forge/.agents/loader.md:31). The distinction from validation is sound. However, this sentence does not require identifying who accepted what. A named authority or acceptance reference would make the claim inspectable.

6. **Every task receives installer training.**  
   “Each manager defines the route shapes it recognizes.” — [src/open-forge/.agents/loader.md](src/open-forge/.agents/loader.md:67). Management shapes, ownership, exclusions, and restoration belong in maintenance documentation. They consume attention during a date-test repair without helping select its rules.

7. **Ordinary documentation remains outside the primary interface.**  
   “A source reference is an exact source ID, such as `memory/crystallized`, or an exact `.agents/...` Markdown path.” — [src/open-forge/.agents/skills/open-forge-cli/references/common.md](src/open-forge/.agents/skills/open-forge-cli/references/common.md:19). Maps provide useful pointers, but `docs/billing/dates.md` cannot participate on equal terms as a directly addressable context source.

These are mostly underspecified procedures and interacting conditions, rather than literal contradictions. Forward references such as `#Contextual` appearing before its definition add friction, but are less consequential than the discovery gap.

Relative links, selection descriptions, early required reads, and source-local overwrites are worth keeping. Templates already stay on demand.

I ran read-only tabletop traces against a constructed fixture. These are reproducible counterexamples, not measurements of actual model or human behavior.

The fixture uses `src/billing/date.test.ts` and `src/billing/date.ts`. Required files sit at the workspace level and beneath three nested scopes, `backend/billing/calendar`. Their conditions match those working paths.

| Defendant | Step-by-step trace | Exact failure boundary |
|---|---|---|
| Fresh frontier agent | Reads startup → supplies both paths to `context` → supplies no selected-scope operands → receives only the workspace rule | Treating successful startup retrieval as complete applicable context misses all three scoped files |
| Small model | Reads startup → selects backend → selects billing → stops before calendar | One omitted parent hides the calendar rule, including its matching condition |
| New senior developer | Reads AGENTS → loader → Directives → Guidance → Maps | At an assumed 250 words per minute, ten minutes ends during Maps. Memory, Patterns, Skills, and the required CLI guide remain unread |

The actual startup reading totals **4,563 whitespace-delimited words**, including the prose-required CLI guide. The proposed flat-list trace selects all four fixture rules without scope-selection operands.

#### Concept budget

I count distinct named roles, states, fields, and operating mechanisms. Compatibility filenames and individual commands are spellings, not additional concepts.

| Current inventory | Count | Disposition |
|---|---:|---|
| Entrypoint, entry, route, root route, scope, slug, ancestor chain, generated-navigation boundary | 8 | Merge navigation into catalogs. Replace applicability with explicit conditions. Delete runtime root identity, slug semantics, and ancestor recovery |
| Axiom, Directive, additive inheritance, inherited marker, nonbinding specialization, scoped authority, conflict handling | 7 | Merge required behavior into Instructions and declared authority. Keep conflict handling. Delete inheritance and its marker. Leave optional adaptation to ordinary judgment |
| Guidance, Map, Pattern, Skill, Template, Memory | 6 | Keep as optional organizational choices |
| Working, Emerging, Crystallized, Archived | 4 | Keep in the narrower Memory documentation |
| LoadNow, KeepInMind, Core, Memory, Extension, Contextual, CurrentTruth, Evergreen | 8 | Keep LoadNow operational. Delete KeepInMind. Make composition tags descriptive. Move acceptance and update obligations into their specific sources |
| Frontmatter placement, description, responsibility, tags, applyTo | 5 | Keep authoring metadata. Move required conditions into checklist rows |
| On-demand selection, listed traversal, refresh passes, working files, pending conditions, ancestor-condition intersection | 6 | Keep optional selection, order, working paths, and pending conditions. Replace refresh passes with explicit restart steps. Delete inherited intersections |
| Overwrite companion, managed route, manager ownership, persistent exclusions | 4 | Keep overwrites. Merge management concepts into optional package ownership documentation |
| Acceptance, validation, external authority, record transitions, independent template copies, native Skill control, prose-versus-tag required reads | 7 | Keep the first six, mostly in narrower files. Delete the competing required-read mechanisms |

**Today: 55 concepts.**

**Replacement: 34 concepts**, including two additions, the required-read list and context receipt.

The **13 startup concepts** are catalog, required-read list, required file, Instructions, working path, path condition, LoadNow, pending condition, context receipt, declared authority, conflict, overwrite companion, and optional reference.

The **21 narrower concepts** are frontmatter placement, description, responsibility, descriptive tags, the six optional categories, four Memory states, acceptance, validation, native Skill activation, independent template copies, record transitions, package ownership, and persistent exclusions.

Moving a concept out of startup reduces compulsory attention. It does not erase its maintenance cost.

#### The new loader

**610 whitespace-delimited words**, including headings and Entries. This is a proposed versioned replacement, requiring the implementation changes below.

```markdown
# Open Forge Loader

Read this at task start, resume, and context restoration. Project files may live under `.agents/`, `docs/`, or beside code.

## Axioms

### Required reads

1. Identify the concrete paths you will investigate, create, change, delete, rename, or review. These are working paths. Opening a document only for context does not make it a working path.
2. Open each #LoadNow link under this loader's Entries. It is a flat required-read list. Read its matching #LoadNow files in listed order. These files are listed directly, even when stored in nested folders. Do not select required reads by their descriptions.
3. A row without `applies to` matches every task. Otherwise, it matches when any working path matches any listed pattern. Conditions belong to that row alone. Apply its rules only to matching working paths.
4. Patterns are case-sensitive and start at the workspace root. `*` stays within one path segment. A whole `**` segment matches any depth. `src/billing/**` matches billing files. `?` matches one character. Braces list alternatives. Character classes match a listed or ranged character, and `[!...]` negates a class. A leading `!` is literal, not an exclusion.
5. Unknown paths leave conditioned rows pending. Read unconditioned files and inspect filenames to identify working paths. Before investigating a file body or editing it, resolve its conditions and read every matching required file. Report missing files and unresolved conditions before the affected work.
6. Before that work, record one context receipt in the task transcript: working paths, required files read, and pending or missing files. Update it when another working path is added. At resume or context restoration, repeat these steps.

### Using sources

- Follow this loader and the `## Instructions` sections in matching required files. Required project documents supply the requirements or knowledge they define. Loading order and folder depth do not resolve conflicting rules.
- Read each file's adjacent `{name}.overwrite.md`, if present, immediately afterward. It inherits the base file's conditions and changes only corresponding content in that source. Report an unclear replacement as a conflict.
- Follow current user direction within platform constraints and granted permissions. Continue routine work within that direction. State assumptions that affect the result.
- Tags and folder names do not establish acceptance. Identify the user direction or declared authoritative source supporting a requirement. Keep proposals and history distinguishable from current requirements.
- For conflicting instructions, quote both passages and identify their sources. Leave the affected action pending until the conflict is resolved. Continue independent work.

### Optional context

Other Entries are catalogs of optional references. Follow their links using descriptions and the task. Catalogs contain navigation, not inherited rules. Direct links need no ancestor reads.

#LoadNow controls required reading only in this loader and its required-read lists. Put every workspace-required dependency directly in a required-read list. Other tags help search. Selected Skills follow their native runtime and resource-loading rules.

### Finishing

Report changed paths, checks and results, unresolved conflicts, and remaining work. For unfinished work, state the next action. Update a task note when the request or a required instruction names one.

### Optional CLI

With a CLI supporting this loader's format, `open-forge context --for <path>` reads the matching required set. Repeat `--for` for all working paths. Otherwise follow the lists directly. Read [the CLI guide](skills/open-forge-cli/SKILL.md) before using maintenance commands.

## Entries

- [Required reads for this workspace](directives/_directives.md) - #LoadNow #Directive
- [Advice for recurring choices](guidance/_guidance.md) - #Guidance
- [Pointers to local and external sources](maps/_maps.md) - #Map
- [Notes for ongoing work, candidates, accepted knowledge, and history](memory/_memory.md) - #Memory
- [Reusable shapes and examples](patterns/_patterns.md) - #Pattern
- [Native capability packages](skills/_skills.md) - #Skill
- [Starting files to copy and adapt](templates/_templates.md) - #Template
```

#### Other Markdown changes

- **AGENTS.md:** Keep the managed boundary and replace its handoff with “Before starting work, read `.agents/loader.md`.” CLAUDE.md remains a native bridge.
- **Category entrypoints:** Keep a short explanation and Entries. Remove inherited Axioms. Move surviving mandatory rules into explicitly registered instruction files. Move authoring advice into templates or an authoring guide.
- **Memory:** Keep all four state names as removable defaults. Remove automatic startup and refresh tags. Records identify their status and acceptance reference in readable prose. Required continuity records receive explicit checklist entries. Existing task notes are updated when requested or required.
- **Tags:** LoadNow is the sole general loading tag. Other tags classify content. Existing acceptance distinctions remain documented without making a tag evidence of acceptance.
- **Frontmatter:** Keep root and scoped forms for descriptions, tags, and responsibility. Required applicability lives in authored checklist rows. Ordinary linked documents need no Open Forge frontmatter.
- **CLI Skill:** Load on demand. Its opening teaches `context --for`, exact workspace paths, and result interpretation. Maintenance procedures remain in references.

A complete illustrative `.agents/directives/_directives.md`:

```markdown
---
description: Required reads matched to the files being worked on
tags: [Directive]
---

# Directives

This is the workspace's required-read checklist.

Read every matching file before working on its paths. Rows without conditions
apply to every task. Conditions use workspace-relative globs. Required behavior
appears under `## Instructions` in the linked files.

This checklist also works directly, without the loader or CLI.

## Entries

- [Workspace working rules](workspace.md) - #LoadNow #Directive
- [Backend constraints](../../docs/backend.md) - #LoadNow - applies to `src/**`
- [Billing working rules](../../docs/billing/rules.md) - #LoadNow #Directive - applies to `src/billing/**`
- [Billing date requirements](../../docs/billing/dates.md) - #LoadNow - applies to `src/billing/**`
```

#### Decisions: model vs mechanism

| Decision today | Replacement |
|---|---|
| Which hidden scope contains required rules? | Removed. Required sources are directly listed |
| Which paths the task investigates | Model identifies them and exposes them in its receipt |
| Which required conditions match | Mechanical glob evaluation, by CLI or manual checklist |
| Which ancestor rules and conditions accumulate | Removed. Every row states its complete condition |
| Which optional reference helps | Model judgment, supported by descriptions and search |
| Which conflicting source governs | Model investigates declared authority and reports unresolved passages |
| Whether an overwrite replaces corresponding content | Model interprets it. Ambiguity produces a visible conflict |
| Whether something deserves speculative Memory capture | Removed from universal startup. Optional workflows may require specific records |
| Whether package files may change | Optional lifecycle mechanism and explicit ownership |
| Whether retrieved instructions were understood and followed | Model execution plus outcome review. Retrieval evidence alone cannot establish this |

#### Customization, independence and blending

Users remove a component by removing its loader link and, when desired, its files. Managed installations also record the omission. Replacement means changing the link or editing the source. An overwrite changes only its base source. New required material receives a direct checklist row with its complete conditions.

Both loader Entries and required lists are authored policy. Index must preserve their membership, loading flags, and conditions. Optional catalog Entries may still be generated.

Each optional category remains an ordinary Markdown collection. A checklist works as a human review checklist. A Pattern remains an example. A Template remains copyable. Native Skills keep their runtime behavior. Memory does not become a prerequisite for using any of them.

A small customized workspace could contain:

```text
AGENTS.md
CLAUDE.md
.agents/
  loader.md
  directives/
    _directives.md
    workspace.md
  memory/
    _memory.md
    working/fix-date.md
  skills/
    _skills.md
    open-forge-cli/SKILL.md
docs/
  _docs.md
  backend.md
  billing/
    rules.md
    dates.md
  adr/
    001-billing-clock.md
src/billing/
  date.ts
  date.test.ts
```

Here the loader keeps links only to retained components and adds `../docs/_docs.md` as an optional catalog. Required billing documents are linked directly from the checklist.

This preserves the [routing-surfaces decision](.agents/memory/crystallized/decisions/framework/routing-surfaces.md) for optional selection and the [Map decision](.agents/memory/crystallized/decisions/framework/map-primitive.md) boundary between pointers and content. It changes the restriction that routed content belongs beneath `.agents/`. Libraries remain useful for projection and sharing, but local documents require neither projection nor duplicate agent copies.

#### Cost and migration

This is a **large behavioral change with a small syntax change**.

The existing headings, link lines, condition suffixes, glob engine, and overwrite filenames remain usable. The substantial work changes their interpretation:

- Context reads explicit required lists without semantic ancestor selection.
- Source resolution admits directly linked workspace Markdown outside `.agents/`.
- Index distinguishes authored policy from generated navigation and preserves authored cross-folder registrations.
- Route inspection, references, status, and maintenance commands must understand the broader source graph.
- Path containment, duplicate destinations, cycles, and package write ownership need appropriate coverage. Linking a document must not make it installer-owned.

Migration enumerates existing required closures, extracts inherited Axioms into instruction files, and writes direct registrations. Inherited conditions must be converted into complete row predicates. Arbitrary glob intersections cannot safely be concatenated, so difficult cases require reviewed expansion or manual conditions.

Open Forge `applyTo` declarations then move from source metadata to registrations. Existing KeepInMind records require individual decisions about required startup, resumption notes, or optional retention.

Tests and snapshots must cover selection, unknown paths, path expansion, external documentation, overwrites, and preservation of authored lists. Public concepts, maintenance contracts, payloads, and Extensions need coordinated revision.

Ship an explicit format migration. New tooling should reject unsupported versions. Older binaries cannot be assumed to reject a future format safely.

The proposal preserves the accepted identity principles. It challenges the current routing decision that rejects flat global enumeration, specifically for required context.

#### Risks and proof

The main risk is checklist growth. Required metadata now scales with all registered obligations. That is an intentional tradeoff for inspectable completeness. File bodies remain selective, but a large workspace may still make manual startup expensive.

Registration omissions remain possible. Path conditions also cannot express every semantic trigger. A short unconditional rule must carry triggers such as deployment authorization. Receipts can become theater, and fewer automatic Memory reads may weaken continuity.

The cheapest useful experiment is a disposable billing fixture with the three nested scopes above, a UTC invariant, an unrelated branch, and a later expansion into another path.

Run three fresh sessions per model against each loader, twelve sessions across a frontier and a small model. Give a new developer ten minutes with each format. Record:

- Required files retrieved before affected work.
- Preservation of the UTC invariant.
- Additional required reads after path expansion.
- Unnecessary document reads and startup words.
- Receipt claims unsupported by retrieval evidence.
- Time to explain how to add one required document.

Advance only if required-read misses decrease without worse task correctness or unacceptable checklist cost. The tabletop results justify this experiment. They do not establish improved model compliance.

#### The one idea to steal

Make every required context dependency a directly inspectable checklist entry whose applicability can be checked without guessing which ancestor to open.

## Demolitionist (Proposal I)

#### Thesis

Open Forge should provide a required reading list, useful links and a place to resume. Make required rules explicit, filter them by working files, and leave reference selection to ordinary agent judgment. This trades recursive scope machinery for a 264-word loader and a 397-word default startup set.

#### Diagnosis

1. **The loader prescribes conversation as well as loading.**  
   `src/open-forge/.agents/loader.md`:
   > Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.

   This consumes permanent attention without helping retrieve project rules. People must distinguish the framework’s routing contract from its preferred conversational style.

2. **Two places define mandatory behavior.**  
   `src/open-forge/.agents/loader.md`:
   > Only this loader and recognized loaded entrypoints define active Axioms.

   A reader must understand Axioms, Directive Instructions and inheritance before knowing which sentences bind. Empty entrypoints still need an explanation of inherited rules.

3. **Physical nesting becomes a language.**  
   `src/open-forge/.agents/loader.md`:
   > Any number of routed slugs may narrow a route below its root.

   Folder position carries meaning beyond organization. People must learn that language, while models must reconstruct it after searches and direct links.

4. **Required reading depends on an earlier relevance judgment.**  
   `src/open-forge/.agents/loader.md`:
   > Neither activates an unselected ancestor or scope.

   A required rule can remain invisible because the agent skipped its parent. Task 53 identifies this exact problem with active Checkpoints below an on-demand category.

5. **Memory introduces governance before there is anything to remember.**  
   `src/open-forge/.agents/memory/_memory.md`:
   > Get explicit user direction before creating a new top-level Memory state.

   Four empty states bring classification and transition rules into every installation. The useful distinction between a proposal and an accepted result does not require four prescribed folders.

6. **The optional tool has compulsory startup documentation.**  
   `src/open-forge/.agents/skills/_skills.md`:
   > When `open-forge-cli/SKILL.md` is present, read it when this entrypoint loads so command options and context-loading advice are available from the start.

   This adds substantial context even when the task needs no CLI. It also creates a prose-only loading requirement outside the CLI’s calculated closure.

7. **Tags carry several unrelated systems.**  
   `src/open-forge/.agents/loader.md`:
   > Defined tags keep the meanings below wherever they appear.

   Eight definitions cover timing, composition, acceptance and maintenance. Readers must remember which tags merely classify and which create continuing obligations.

8. **Human documentation participates through an intermediary.**  
   `.agents/memory/crystallized/decisions/framework/map-primitive.md`:
   > Its canonical root is `.agents/maps/`, its entrypoint is `_maps.md`, and its type tag is #Map.

   Maps correctly preserve the destination’s authority. But an existing engineering document still needs agent-specific navigation around it. Direct participation should be possible at its existing path.

#### Concept budget

I count distinct named meanings and special behaviors, excluding ordinary Markdown syntax and individual commands.

**Today: 45 concepts.**

- **1–9:** entrypoint, entry, route, root route, scope, slug, Axiom, inherited rules, inherited marker.
- **10–20:** Directive, Guidance, Map, Pattern, Skill, Template, Memory, Working, Emerging, Crystallized, Archived.
- **21–27:** `#LoadNow`, `#KeepInMind`, `#Core`, `#Extension`, `#Contextual`, `#CurrentTruth`, `#Evergreen`. The `#Memory` meaning is included above.
- **28–33:** managed route, exclusion, overwrite companion, acceptance, validation, declared source authority.
- **34–42:** frontmatter, description, responsibility, tags, `applyTo`, working files, inherited matching, pending conditions, frontmatter placement setting.
- **43–45:** on-demand selection, generated Entries, Instructions.

**Proposed: 12 operational concepts.**

Catalog, description, descriptive tags, `#LoadNow`, rule source, frontmatter, `applyTo`, working files, pending conditions, references, accepted source, checkpoint.

| Existing concepts | Disposition and practical loss |
|---|---|
| Entrypoints, entries, routes and root routes | Merged into catalogs containing explicit links. Existing filenames remain usable conventions. |
| Scope slugs, inherited rules, inherited matching and the inherited marker | Deleted. Each rule declares its own conditions. Users lose compact ancestor-wide restrictions and must maintain explicit applicability. |
| Axioms, Directives and Instructions | Merged into registered rule sources with `## Instructions`. Users lose inline binding rules in catalogs. |
| Guidance, Maps, Patterns, Templates and Skills | Moved to optional examples and native capabilities. Patterns lose automatic default-conformance semantics unless a registered rule requires them. |
| Memory and its four states | Merged into checkpoints and ordinary records. Users lose folder-based lifecycle classification. Records state their status, provenance and uncertainty directly. |
| Seven reserved tags besides `#LoadNow` | Removed as reserved vocabulary. Ordinary search tags remain. Candidate surveillance through `#KeepInMind` disappears. |
| Managed routes and exclusions | Made mechanical and documented in maintenance help. Readers no longer need lifecycle knowledge to follow rules. |
| Overwrite companions | Deleted from the new format. Users lose small overlays that automatically follow upstream base updates. Replacement sources and reviewed edits take their place. |
| Acceptance, validation and declared authority | Merged into an accepted-source distinction. Evidence still supports claims, while user direction establishes acceptance. |
| Metadata details and placement preferences | Kept where needed or moved to authoring help. `responsibility` remains optional metadata without startup explanation. |
| Generated Entries and on-demand selection | Merged into catalogs and references. Catalog membership becomes authored, while metadata summaries can remain generated. |
| Conversation and general writing prescriptions | Deleted from Core. Workspaces can register their preferred rules explicitly. |

This preserves user-owned meaning, native capability, continuity and honest reliability. It explicitly relaxes **Context By Relevance’s** selected-scope scaling promise for required-rule metadata. A mandatory list grows with the workspace, while rule bodies remain filtered.

#### The new loader

Complete replacement for the new format. **264 whitespace-separated words, including headings and entry lines.**

```markdown
# Open Forge Loader

At task start or resume, read every #LoadNow entry below. Follow #LoadNow links under Entries recursively, in listed order, reading each file once. Other links wait until useful.

Directives lists every required rule source directly, each with #LoadNow. Read each applicable source before work. Its `## Instructions` section is binding within its stated scope. Catalogs contain navigation, not inherited rules. Folder names create no authority or scope. Catalogs have no file conditions.

An `applies to` condition filters its target only. No condition means all work. Matching any listed pattern is enough. Match its workspace-relative globs against files you investigate, create, change or review, including planned files. Reading context alone does not count. Matching is case-sensitive. `**/*.cs` matches C# files at any depth. When working files are unknown, keep conditioned sources pending and resolve them before affected work.

The user sets direction. Report unresolved rule conflicts or missing required files. A saved proposal is not accepted knowledge. Follow the source declared authoritative for a subject.

When working files or direction change, recheck automatic reads. After context restoration, restart here. Before stopping, update affected accepted sources within existing authorization. If work needs to survive this session, save the goal, actual state, evidence, uncertainty and next step in the active checkpoint.

The CLI is optional. `open-forge context --for <path>` batches these reads. Without it, follow the links. For other commands, use `open-forge --help` or the CLI Skill.

## Entries

- [Required rules](directives/_directives.md) - #LoadNow
- [Advice](guidance/_guidance.md)
- [Useful sources](maps/_maps.md)
- [Checkpoints and notes](memory/_memory.md) - #LoadNow
- [Examples](patterns/_patterns.md)
- [Native capabilities](skills/_skills.md)
- [Starting files](templates/_templates.md)
```

#### Other Markdown changes

`AGENTS.md` becomes:

```markdown
# Open Forge

Before work, read `.agents/loader.md` and follow its required reading list.

**End of Open Forge managed section.**
```

`CLAUDE.md` imports `AGENTS.md` once. It drops its separate loader import.

The shipped Directives entrypoint becomes:

```markdown
---
description: Required rules for this workspace
tags: [LoadNow]
---

# Directives

Every entry below registers a required rule source. Put its requirements under `## Instructions`. Use `applyTo` when the rule is limited to matching work files.

## Entries
```

Required sources are listed directly here. They cannot be hidden beneath optional catalogs. Non-file conditions such as “during review” remain explicit conditions inside Instructions.

Other category entrypoints keep a short purpose statement and `## Entries`. Remove their Axioms and authoring manuals. Their names remain optional organizational choices.

Memory becomes:

```markdown
---
description: Work to resume and notes worth recovering
tags: [LoadNow]
---

# Memory

Keep the active checkpoint short. Link to evidence and the documents that own accepted results. Record a note's source, date and uncertainty when they affect later use. Recording a note does not accept it or make it a rule.

## Entries

- [Active checkpoint](checkpoint.md) - #LoadNow
```

The initial checkpoint is:

```markdown
---
description: Current work and next action
tags: [LoadNow]
---

# Checkpoint

No active work.
```

Stop shipping the four state folders. Existing records remain where they are until deliberately reorganized. Human documents can retain native conventions such as proposed, accepted and superseded ADRs.

Keep existing frontmatter fields and both placement forms. Only `#LoadNow` has framework-wide tag semantics. Participating documents retain ordinary descriptive metadata. Catalogs cannot declare `applyTo`.

The CLI Skill becomes optional and short:

```markdown
---
name: open-forge-cli
description: Read and maintain registered Markdown with the optional open-forge CLI.
---

# Open Forge CLI

Check `open-forge --help` for availability and exact commands.

Use `context --for <path>` for required reading. Repeat `--for` for all known or planned working files. Resolve pending conditions before affected work.

Use `find`, `references`, `route list` and `route inspect` for discovery.

Before changing navigation, read [route maintenance](references/routes.md).
Before package operations, read [packages](references/packages.md).
Preview mutations with `--dry-run` and inspect the plan.
```

The five default startup files total **397 words**, counting metadata and Markdown markers. The import bridge and optional shortened CLI Skill keep the shipped context comfortably below 1,000 words. Workspace-specific rules add their own measurable cost.

#### Decisions: model vs mechanism

| Decision today | Proposed responsibility |
|---|---|
| Which branch contains required rules? | **Mechanical.** Directives explicitly lists every required source. |
| Which files does the task concern? | **Model.** Identify investigated, reviewed, changed and planned files. |
| Does a rule match those files? | **Mechanical.** Evaluate that source’s `applyTo`. Manual readers perform the same check. |
| Which ancestor rules and filters apply? | **Removed.** Sources have explicit, independent applicability. |
| Which references or Skills help? | **Model.** Use descriptions, search and ordinary links. |
| Which conflicting rule wins? | **User direction.** The model reports unresolved conflicts. Depth and order provide no precedence. |
| What needs remembering? | **Model.** Preserve useful continuation state and uncertain findings. |
| When should context be refreshed? | **Explicit loop.** Start, resume, context restoration and changed working files. Tooling can retrieve the set without enforcing compliance. |
| Is a proposal accepted? | **User or declared authority.** Neither recording nor tagging accepts it. |
| Who owns installation changes? | **Mechanical.** CLI ownership records and exclusions, outside the loading contract. |

#### Customization, independence and blending

A user adds a rule by registering its existing file. Removing that registration ends its framework instruction role. Replacing the destination replaces the rule. Changing its own Instructions or conditions supplies an override without another precedence layer.

For CLI-managed files, updates preserve edited content and report divergence for review. Intentional removal uses the existing exclusion mechanism. This changes current Update behavior and eliminates the need for new overwrite companions.

Each rule is an ordinary document usable independently. A checkpoint can stand alone. Catalogs work as human navigation. Skills retain their native contract. Optional catalogs can be removed with their loader links.

A small blended workspace could contain:

```text
AGENTS.md
CLAUDE.md
.agents/
  loader.md
  directives/_directives.md
  guidance/_guidance.md
  maps/_maps.md
  memory/_memory.md
  memory/checkpoint.md
  patterns/_patterns.md
  skills/_skills.md
  skills/open-forge-cli/SKILL.md
  templates/_templates.md
docs/
  README.md
  engineering.md
  decisions/0001-storage.md
src/
  api/
```

The Directives catalog registers the real document:

```markdown
- [API engineering requirements](../../docs/engineering.md) - #LoadNow - applies to `src/api/**`
```

Its metadata agrees with that row:

```yaml
description: API engineering requirements
tags: [LoadNow]
applyTo: ["src/api/**"]
```

The document contains its required behavior under `## Instructions` alongside its human explanation.

The loader can additionally expose:

```markdown
- [Project documentation](../docs/README.md)
```

`docs/README.md` serves people and agents through ordinary prose and an Entries section. No projection or intermediate Map is required. Maps retain their useful pointer function for external sources and curated overviews.

#### Cost and migration

This is a **medium-to-large CLI change**, despite the smaller Markdown.

- **Syntax:** Keep `## Entries`, current row syntax and existing frontmatter parsing. Untagged rows already parse. Retain compatibility filenames.
- **Source model:** Follow explicitly registered links anywhere inside the workspace. Any registered Markdown file with Entries can act as a catalog. Intermediate folders need no entrypoint.
- **Loading:** Replace parent-chain topology with explicit links and target-local conditions. The current loading resolver calls `FindDirectChild`, while selected Context resolves ancestor chains. Both assumptions change.
- **Index:** Preserve authored membership and order. Refresh row metadata from destinations. Do not automatically add or remove registrations based on directory contents.
- **Lifecycle:** Preserve edited managed Markdown during Update. Report divergence rather than replacing it automatically.
- **Safety:** Retain existing workspace containment and filesystem boundaries. Registration grants reading behavior, not additional write permission.

Introduce an explicit format selection for the new reader. Existing workspaces keep their current semantics during transition and require a matching CLI release after migration.

Migration must inventory user Axioms, inherited filters and overwrites. Extract retained Axioms into registered rule sources. Rewrite inherited conditions into explicit per-source conditions. Complex intersections need review rather than guessed glob conversion. Fold overwrites into reviewed replacement sources. Preserve record contents and provenance without automatically moving Memory.

Tests and snapshots must cover external-path registration, matching and unknown files, scope changes, duplicate links, cycles, index preservation, edited-file updates and migration failures.

Public concepts, maintenance contracts, first-party Extensions and examples all need revision. Planning’s checkpoint loading and extension rules need explicit registration. The abandoned simplified-core loader is useful evidence that trimming prose alone leaves most of the machinery intact.

#### Risks and proof

A flat required list can become long. Explicit conditions can drift when files move. Removing inherited rules increases authoring responsibility. Removing overwrite companions makes upstream changes harder to combine. Plain record status may be overlooked more often than a prescribed Memory state.

The current loading-reliability decision supports retaining unconditional root Directives loading. It does not establish that these other changes improve behavior.

The cheapest experiment is twelve short, paired sessions using the same model and disposable fixtures. Compare both loaders on six cases: a scoped rule, newly discovered working files, unknown paths, resumption, a proposed ADR and a rule stored under `docs/`.

Record actual file reads, resulting work and saved checkpoints. Score missed rules, rules applied outside scope, proposals treated as accepted, recoverable continuation state, startup words and task latency. Require fewer misses on cases the current loader fails, with no regression elsewhere. Start without the CLI to test whether the Markdown stands independently.

That experiment is a screening test. Broader adoption needs repeated runs across runtimes.

#### The one idea to steal

Put every mandatory rule on one explicit reading list so required-rule discovery never depends on a model selecting the right reference branch.

## Newcomer (Proposal G)

#### Thesis

Make the loader a short reading recipe, with required rules reached through explicit loading tags and file conditions. Keep the optional categories, introduce their details when needed, and let the same catalogues lead directly into ordinary project documentation.

#### Diagnosis

My reading diary follows the startup order, then the supporting material.

1. **I smiled at the clear first step, then immediately needed a glossary.**

   `src/open-forge/AGENTS.md`:

   > Before starting a task, read `.agents/loader.md`.

   Then `src/open-forge/.agents/loader.md`:

   > `axiom` - Required rule under `Axioms` in this loader or a recognized loaded entrypoint, inherited by selected descendants.

   The first instruction feels approachable. The definition introduces several other concepts before showing what I should actually do. I expected “read required links, then choose relevant links,” followed by an example.

2. **I reread the folder rules and started drawing them myself.**

   `src/open-forge/.agents/loader.md`:

   > Any number of routed slugs may narrow a route below its root. They may appear before, between, or after deeper route segments. Each narrows everything that follows it.

   I guessed that “slug” meant a folder name. Understanding every permitted arrangement seems unnecessary before my first task. I expected one ordinary folder example, with advanced arrangements explained during authoring.

3. **I understood why acceptance matters, but lost the reading instructions among governance rules.**

   `src/open-forge/.agents/loader.md`:

   > Validation establishes whether evidence supports a claim. Acceptance establishes what may be treated as current within its scope. Moving or restoring a record does not establish acceptance.

   This protects a valuable distinction. Its placement asks every reader to learn the knowledge lifecycle alongside navigation. I expected the practical version first: a supported proposal is still a proposal until accepted.

4. **I reread the loading rule because “required” can still be hidden.**

   `src/open-forge/.agents/loader.md`:

   > Entries stay on demand unless marked #LoadNow or #KeepInMind. These tags act only through loaded parents. Neither activates an unselected ancestor or scope.

   A model must choose the correct parent before discovering required descendants. Matching a file condition does not solve that discovery problem. I expected file-specific required branches to be explicitly reachable through tagged parents, leaving fewer relevance decisions.

5. **I guessed the meaning of the Memory names, then gave up trying to remember the whole routine.**

   `src/open-forge/.agents/memory/crystallized/_crystallized.md`:

   > Crystallized Memory holds accepted knowledge that should remain current within its scope.

   `src/open-forge/.agents/memory/emerging/_emerging.md`:

   > At each applicable #KeepInMind refresh, check visible `Entries` and save useful candidate material before it is lost.

   The states are useful, but their names and maintenance duties arrive before I have anything to remember. I expected “work in progress” and “accepted knowledge,” with ideas and history available when needed.

6. **The CLI guide became another startup assignment.**

   `src/open-forge/.agents/skills/_skills.md`:

   > When `open-forge-cli/SKILL.md` is present, read it when this entrypoint loads so command options and context-loading advice are available from the start.

   This required read lives in prose, beyond the loading-tag closure computed by `context`. It also repeats the loader’s command orientation. I expected to read CLI help when using the CLI.

7. **My existing documentation still felt outside the front door.**

   `src/open-forge/.agents/skills/open-forge-cli/references/packages.md`:

   > A Library registers a shared folder and projects its files through relative file links.

   Maps and Libraries offer useful connections, but ordinary documentation still needs an intermediary to participate in routing. I expected to add `docs/_docs.md` and keep reading the actual architecture and ADR files where people already maintain them.

#### Concept budget

I count distinctions that affect reading, application, or maintenance, excluding individual commands and compatibility aliases.

**Today: 50 concepts.**

| Group | Concepts today | What happens |
|---|---|---|
| Navigation, 8 | Entrypoint, entry, route, root route, scope, slug, ancestor chain, direct-child navigation | Merge into catalogue, described link, and scope. Delete “slug” vocabulary. Make parent lookup and child validation mechanical |
| Rules and trust, 7 | Axiom, Directive Instructions, inherited marker, additive inheritance, non-binding specialization, validation, acceptance | Merge required instructions and inheritance. Delete the marker. Move specialization into advice. Explain evidence and acceptance together |
| Loading, 7 | On demand, LoadNow, KeepInMind, loaded-parent boundary, listed order, refresh pass, working file | Default links remain optional. Fold traversal details into required reading and refresh. Merge working files with file conditions |
| Metadata, 6 | Frontmatter, root/scoped form, description, responsibility, tags, applyTo | Keep authoring details in references. Description becomes the link’s explanation. Forms remain one metadata concept |
| Defined classification tags, 6 | Core, Memory, Extension, Contextual, CurrentTruth, Evergreen | Core and Extension become ordinary labels. Memory keeps its module. Put status and maintenance meaning into readable source prose |
| Categories, 6 | Directives, Guidance, Maps, Patterns, Skills, Templates | Keep their capabilities. Merge Directives with required rules. Introduce the other five when selected |
| Memory states, 4 | Working, Emerging, Crystallized, Archived | Keep the distinctions locally, using everyday display names |
| Maintenance, 6 | Manager, managed route, exclusion, overwrite companion, ownership versus authority, generated navigation | Merge management vocabulary. Keep omissions, adjustments, and generated Entries in authoring and maintenance guidance |

**Proposed design: 25 concepts, including optional modules.**

The nine operating ideas are catalogue, described link, scope, required rule, required read, refresh, file condition, local adjustment, and accepted source.

The sixteen optional or authoring ideas are advice, map, reusable shape, skill, starting file, memory, work in progress, ideas and findings, accepted knowledge, history, frontmatter, file purpose, search tags, package maintenance, persistent omission, and generated Entries.

Some reductions are merges. Others move knowledge out of startup rather than pretending the capability disappeared.

#### The new loader

**622 whitespace-separated words, including headings and Entries.**

```markdown
# Open Forge

Start here to find the rules, documents, and notes needed for your task. Everything works by reading files. The CLI is optional.

A catalogue lists useful links under `## Entries` and explains their scope. Its filename is `_{folder-name}.md`. This loader is the starting catalogue.

## Axioms

### Read what the task needs

1. At task start or resume, identify the goal and the files you will work on, including planned files. Scan the Entries below.
2. In each catalogue you open, read applicable #LoadNow and #KeepInMind entries in listed order. Repeat for any catalogues they open. These tags act only through an open parent.
3. Follow other links whose descriptions fit the task. Keep unrelated branches closed. Read a selected catalogue before its children.
4. When the goal or working files change, check the catalogues again. If search takes you directly to a routed file, read its parent catalogue chain before using it.

### Check file conditions

An entry's `applies to` patterns repeat its file's `applyTo` metadata. Check them before reading or applying that file.

Patterns in one list are alternatives. Conditions on a file and its parent catalogues must all match the same working file. Paths are workspace-relative and case-sensitive. `**/*.cs` matches C# files at any depth, while `src/**` matches files under `src/`.

Investigated, reviewed, changed, and planned files count. Reading background material alone does not add its path. Unknown paths leave conditioned required reads pending. Resolve them before dependent work. A condition filters a link that would otherwise load. It does not open a closed parent or grant edit permission.

### Follow the applicable rules

- Follow `## Axioms` in this loader and selected catalogues, and `## Instructions` in selected Directives. Parent rules remain active within their scope. Report unresolved conflicts.
- Follow user direction within platform and safety constraints. Continue routine, reversible work within the request. Ask when an unresolved choice would materially change the result or authority needed.
- Use each accepted document or declared external source for the subject it defines. Evidence can support a proposal without making it accepted. Links, tags, folder names, and loading order do not establish acceptance or override rules.

### Refresh and finish

#KeepInMind means read now, then reread while its scope is active at task start or resume, after context restoration, before handoff or finish, and when it may have changed.

Before relying on changed accepted knowledge, update the document that defines it. By finish, update other current documents this work makes inaccurate and save progress or findings that future work will need. Follow the selected Memory instructions for placement. Keep the result and any unresolved gaps clear and concise.

### Make it your own

Read an adjacent `{name}.overwrite.md` immediately after its base, before following its links. It changes corresponding base content within the same scope. It cannot cancel unrelated rules.

You control this loader's Entries. Add, replace, or remove links to participating folders anywhere inside the workspace, including `docs/`. Other catalogues list their direct children. Keep one catalogue per participating folder. Ordinary links elsewhere provide references without changing scope.

If using the CLI, read its installed `open-forge-cli` Skill when present, or check `open-forge --help`. `open-forge context --for <path>` batches required reading. Supply every known working path and resolve incomplete results. Without the CLI, follow these steps directly.

## Entries

- [Required instructions for this workspace and matching files](directives/_directives.md) - #LoadNow #Directive
- [Advice for recurring choices](guidance/_guidance.md) - #Guidance
- [Pointers to useful local and external sources](maps/_maps.md) - #Map
- [Work in progress, accepted knowledge, ideas, and history](memory/_memory.md) - #LoadNow #Memory
- [Examples of reusable shapes](patterns/_patterns.md) - #Pattern
- [Skills to use when a task needs them](skills/_skills.md) - #Skill
- [Starting files to copy and adapt](templates/_templates.md) - #Template
```

#### Other Markdown changes

`AGENTS.md` becomes:

```markdown
# Open Forge

Before starting work, read [.agents/loader.md](.agents/loader.md)
and follow its reading steps.

**End of Open Forge managed section.**
```

Keep `CLAUDE.md` as a native bridge without duplicating policy.

Category entrypoints get a short purpose, their essential application rules, and Entries. Move detailed authoring requirements into existing references and optional templates. Omit `## Axioms` when there are no local rules.

For example, the complete shipped Directives catalogue becomes:

```markdown
---
open-forge:
  description: Required instructions for this workspace and matching files
  tags: [LoadNow, Directive]
---

# Directives

These files contain required instructions.

## Axioms

- Each Directive carries #LoadNow and puts its requirements under `## Instructions`.
- For file-specific rules, mark every catalogue in the required branch #LoadNow and use `applyTo` conditions to limit it.
- When a branch depends on the task's goal rather than file paths, describe that trigger clearly and select it before that work.
- Keep workspace-wide instructions short. Child instructions add requirements within their scope. Report unresolved conflicts.

## Entries

- none - No entries - #Empty
```

Memory stays required, but its entrypoint becomes a short placement guide:

| Existing path | Display name | Default reading |
|---|---|---|
| `working/` | Work in progress | Read its compact catalogue |
| `crystallized/` | Accepted knowledge | Read its compact catalogue |
| `emerging/` | Ideas and findings | On demand |
| `archived/` | History | On demand |

Each state needs its purpose and a few local rules. Keep acceptance provenance visible. Reserve `#KeepInMind` for actual continuity needs rather than the whole Emerging state. Ensure active records have an exposed parent chain.

Keep both frontmatter forms and all existing fields. Introduce `description`, `tags`, and optional `applyTo` first. Explain `responsibility`, form selection, and detailed glob syntax in authoring references.

Only `#LoadNow` and `#KeepInMind` need global loading definitions. The other six currently defined tags become optional search labels. Preserve any status or maintenance obligation they carried in explicit source prose.

The CLI Skill becomes optional and opens with three needs: read context, find a source, maintain files. Its existing references retain detailed command and lifecycle instructions.

The startup target is **under 1,000 words before project content**, including the loader and four compact catalogues. That is a target, not a measured result.

#### Decisions: model vs mechanism

| Decision today | Proposed responsibility |
|---|---|
| Identify the task and working files | Model, with user correction and explicit pending conditions |
| Discover mandatory file-specific branches | Mechanism through fully tagged parent chains and `applyTo` |
| Evaluate patterns and inherited conditions | CLI when available, the same explicit procedure manually |
| Choose advice, examples, skills, and relevant knowledge | Model using descriptions |
| Recover parents after a search hit | CLI resolves the chain, manual traversal remains supported |
| Decide when required context refreshes | Fixed schedule for active `#KeepInMind` sources |
| Decide whether a source is accepted | User or declared authority, never inferred from confidence |
| Resolve contradictory binding rules | Model investigates and reports unresolved conflict |
| Generate navigation and check reachability | CLI, with editable Markdown equivalents |
| Choose root participation | User-authored links |
| Interpret package provenance as authority | Removed |
| Learn authoring and package mechanics at startup | Removed from startup |

Path conditions cannot express every task trigger. Discussion, architecture, and review scopes still need semantic selection. The design makes that remaining judgment explicit.

#### Customization, independence and blending

Removing a loader link disables that component. Removing managed files permanently also records the existing exclusions, so updates respect the omission. Replacement files can occupy the same role. Adjacent overwrites preserve small local changes through managed updates.

Each category contains enough instructions to work when selected independently. Templates remain independent copies. Skills retain their native runtime contracts. Memory can be omitted when existing task records and documentation already provide continuity.

For blending, **permit loader links to catalogues anywhere inside the workspace**. Add, for example:

```markdown
- [Application architecture and accepted decisions](../docs/_docs.md) - #LoadNow #Document - applies to `src/**`, `docs/**`
```

The destination carries matching metadata. Its architecture file can be `#LoadNow`, while its ADR catalogue stays on demand.

After removing unused categories, a small workspace could look like:

```text
AGENTS.md
README.md
.agents/
  loader.md
  directives/
    _directives.md
    testing.md
  memory/
    _memory.md
    working/
      _working.md
      checkout-progress.md
    crystallized/
      _crystallized.md
docs/
  _docs.md
  architecture.md
  adr/
    _adr.md
    001-storage.md
src/
  checkout/
    _checkout.md
    design.md
    Checkout.cs
```

People follow links from the README. Agents reach the same files through the loader. Code-side notes can be another explicitly linked root.

Keep roots non-overlapping so each routed file has one catalogue ancestry. Ordinary cross-links remain references. Nothing scans every Markdown file automatically.

This extends the current routing-surfaces decision: descriptions still select, bodies still define detail. It also preserves the Map decision’s useful boundary. Maps remain useful for curated destinations and external systems, while local documentation no longer requires a Map or Library projection merely to participate.

#### Cost and migration

**The Markdown reduction is modest implementation work. Native blended routing is a substantial CLI feature.**

- **Representation:** retain `## Entries`, its line grammar, `## Axioms`, frontmatter fields, and entrypoint compatibility names. No new metadata language.
- **Paths and identities:** generalize source inventory, reference parsing, ancestry, containment, and identity beyond `.agents`. Today `SourceLogicalPath` requires that prefix and `SourceLoaderDestinationParser` rejects `..` segments.
- **Root maintenance:** preserve the user’s chosen root links. Index may refresh their metadata, but must not discover, remove, or retarget roots on its own. Updates must preserve this selection.
- **Writes:** routing visibility must not grant write ownership over `docs/`. Reuse explicit path permissions and existing ownership boundaries.
- **Tests:** cover native documentation roots, ancestry, conditions, identity collisions, containment, root preservation, and unchanged legacy routes. Refresh payload and startup snapshots.
- **Documentation:** revise installation, concepts, CLI references, maintenance contracts, and Extension starters together.

Offer a versioned, opt-in migration. Preserve files, paths, acceptance evidence, overwrites, exclusions, and active continuity chains. Report loading-tag changes before applying them. Preserve old short IDs where unambiguous and accept exact workspace-relative paths for new roots.

Do not silently convert existing workspaces to the reduced startup policy.

This challenges current startup and routing decisions. It preserves the accepted identity principles, including user-owned meaning, relevance, optional tooling, and honest reliability. The abandoned simplified-core attempt shortened wording but retained much of the same conceptual structure. This proposal changes what startup requires readers to learn.

#### Risks and proof

Optional catalogues could hide useful context. File conditions could be authored too narrowly. Removing the Emerging refresh could reduce useful capture. Moving tag meaning into prose could expose previously implicit obligations.

The loading-reliability decision reports repeated failures with deferred critical context. Keep baseline Directives early and test reductions before adopting them broadly. Structural checks should flag required file-specific branches with unreachable parents, although tools cannot verify that every semantic trigger is correct.

The cheapest useful experiment is six fixed tasks against both designs in fresh sessions:

- Follow a workspace-wide instruction.
- Reach a nested file-specific instruction.
- Handle initially unknown paths.
- Rescope after adding another language.
- Resume through an active memory record.
- Follow a direct documentation link with an overwrite.

Score actual read traces and resulting behavior. Compare missed required rules, unrelated reads, startup words, and recovery mistakes. Require no new critical misses and a substantial context reduction before a larger trial. A small trial would provide a useful signal without establishing a broad reliability guarantee.

The four-sentence retelling is:

> Open Forge starts with a small catalogue of links. Read the required rules, then follow descriptions that fit your task. The same links can lead into project documentation, code-side notes, or optional memory. When the task changes or finishes, check the relevant context and leave useful documents and progress current.

This proposal was prepared read-only at `e9d4fc9a7`. No files, builds, test suites, or Doctor runs were changed or executed.

#### The one idea to steal

Make required reading visible in the link structure, so agents spend their judgment on the task rather than discovering which instructions were mandatory.

## Unix Greybeard (Proposal F)

#### 1. Thesis

Make mandatory loading a predictable directory operation, then leave discovery to ordinary Markdown links. Put small read lists in `.agents/load.d/`, let them point anywhere inside the workspace, and make the seven existing catalogues optional. Preserve the accepted identity principles while replacing the current routing, inheritance and loading machinery.

#### 2. Diagnosis

1. **Every folder becomes infrastructure.**  
   `src/open-forge/.agents/loader.md` says:

   > Every folder in a route chain has exactly one recognized entrypoint.

   A person cannot simply add a useful document several folders deep. They must maintain a navigation chain before the document participates. An agent reaching it through search must reconstruct that chain.

2. **Rules depend on how the reader arrived.**  
   `src/open-forge/.agents/loader.md` says:

   > Only this loader and recognized loaded entrypoints define active Axioms.

   Understanding a leaf requires discovering which ancestors were loaded and remembering their instructions. The leaf is not independently usable.

3. **Mandatory loading follows a discretionary decision.**  
   `src/open-forge/.agents/loader.md` says:

   > These tags act only through loaded parents. Neither activates an unselected ancestor or scope.

   A required rule can remain invisible because the model did not select its parent. Better descriptions help discovery, but they do not remove that failure point.

4. **Refresh combines retrieval with housekeeping.**  
   `src/open-forge/.agents/memory/emerging/_emerging.md` says:

   > At each applicable #KeepInMind refresh, check visible `Entries` and save useful candidate material before it is lost.

   Reading context, searching for unsettled findings and deciding what to save become one recurring operation. Even an empty installation teaches this lifecycle before it has records.

5. **File conditions require substantial bookkeeping.**  
   `src/open-forge/.agents/loader.md` says:

   > only while one working file matches its patterns and those of every ancestor with `applyTo`

   The reader must distinguish working files from context sources, combine ancestor predicates against the same path, and handle unknown paths. The useful feature is file matching. The inherited predicate system is additional machinery.

6. **Startup teaches maintenance that most tasks never perform.**  
   `src/open-forge/.agents/loader.md` says:

   > Each manager defines the route shapes it recognizes. It preserves their segment order through scopes.

   Package ownership and installation topology belong in maintenance documentation. They distract both people and models from the immediate question of what to read.

7. **An optional tool has a compulsory manual.**  
   `src/open-forge/.agents/skills/open-forge-cli/SKILL.md` says:

   > The Skills entrypoint requires this guide at startup.

   Every task pays for CLI operation advice, including tasks that never invoke it. The loader already contains a command inventory.

8. **Human documentation remains outside the native structure.**  
   `.agents/memory/crystallized/decisions/framework/map-primitive.md` says:

   > Its canonical root is `.agents/maps/`, its entrypoint is `_maps.md`, and its type tag is #Map.

   Maps correctly preserve destination authority. However, ordinary documentation participates through an agent-specific intermediary rather than through the same loading mechanism. The useful distinction is between navigation and content, not between `.agents/` and `docs/`.

The [routing-surfaces decision](.agents/memory/crystallized/decisions/framework/routing-surfaces.md) gets one important thing right: readable descriptions should let readers choose before opening bodies. Keep that property without requiring a complete folder hierarchy.

#### 3. Concept budget

This inventory counts **52 protocol concepts exposed by startup reading**, excluding ordinary Markdown syntax and individual command names. The proposed startup contract requires **14**. Optional features remain available, but their vocabulary stops being compulsory.

| Current concepts | Disposition |
|---|---|
| **1** loader, **2** entrypoint, **3** entry, **4** route, **5** root route, **6** scope, **7** slug, **45** generated navigation, **46** ancestor chain, **48** CLI source reference | Merge 1–3 into read lists and literal links. Delete 4–7 and 46 as activation machinery. Make 45 optional generation. Replace 48 with literal workspace paths in the new mode. |
| **9** Axiom, **10** inherited marker, **11** Directive Instructions, **40** acceptance, **41** validation, **42** external authority, **52** required-read prose links | Merge 9 and 11 into directly stated source requirements. Delete 10. Keep 40–42. Express 52 as explicit dependencies. |
| **12** Guidance, **13** Maps, **14** Patterns, **15** Skills, **16** Templates, **17** Memory, **18** Working, **19** Emerging, **20** Crystallized, **21** Archived, **43** native Skill activation | Move all into independently useful optional files. |
| **22** LoadNow, **23** KeepInMind, **29** on demand, **30** refresh pass, **39** pending conditions, **49** context closure | Keep 22 and 30. Merge 23 into the common refresh pass. Express 29 through optional catalogues. Delete 39 through conservative reading when paths are unknown. Make 49 mechanical. |
| **24** Core, **25** Extension, **26** Contextual, **27** CurrentTruth, **28** Evergreen | Make 24–25 ordinary labels. Merge 26–27 into explicit source status and acceptance. Replace 28 with a direct documentation update duty. |
| **31** overwrite companion, **32** description, **33** responsibility, **34** tags, **35** frontmatter-form selection, **36** applyTo, **37** glob language, **38** working file | Delete implicit overwrite behavior. Keep descriptions as link labels. Move 33–35 into optional authoring help. Keep only LoadNow as an operational tag. Keep per-source conditions, globs and working paths. |
| **8** managed route, **44** exclusions, **47** inherited-condition conjunction, **50** Index/Repair ordering, **51** preview/result contract | Move 8, 44, 50 and 51 into maintenance help. Delete 47. |

The **14 required concepts** are: drop-in directory, read list, entry link, LoadNow, optional catalogue, working path, glob condition, refresh pass, explicit dependency, source scope, acceptance, evidence, external authority and source update duty.

This is a reduction in compulsory understanding, not a claim that optional package management or Memory has disappeared.

#### 4. The new loader

**496 words**, counted by whitespace, including headings and Entries.

```markdown
# Open Forge Loader

Open Forge reads required context and provides optional catalogues. The files work without a CLI.

## Required reading

1. At task start or resume, after context restoration, and before handoff or closeout, read every immediate `*.md` file in `.agents/load.d/`, in case-sensitive filename order. A missing or empty directory requires nothing. Repeat when working paths or required sources change.
2. Follow `## Entries` links marked `#LoadNow`, in listed order, recursively through required read lists. Read each destination once per pass. Repeated links and cycles add no reads. Keep mandatory sources reachable from `load.d/`.
3. Resolve links relative to their containing file. Destinations may be anywhere inside the workspace. URLs are references, not automatic reads.
4. A required entry may end with `- applies to` followed by backtick-delimited glob patterns separated by commas. Read it when any working path matches any pattern. Conditions belong to links to individual sources, not read lists. They do not carry through links. Without known working paths, read every conditional entry.
5. Working paths are files being investigated, created, changed, deleted, renamed or reviewed, including planned files. A rename includes both paths. Reading a context source alone does not add its path.
6. Patterns are case-sensitive and workspace-relative. `*.py` matches root files, `**/*.py` matches any depth, and `docs/**` matches a folder. A leading `!` is literal. Matching controls reading, not edit permission.
7. Report missing required files and invalid conditions. Stop work that depends on unavailable context.

## Using sources

Follow applicable project rules within platform constraints and user direction. Continue routine reversible work without asking for confirmation again. State consequential assumptions and ask about unresolved choices that materially change the result or risk.

Each source states its requirements, scope and status. Read lists select sources. They do not approve proposals or give documents authority. Use declared external authorities for their assigned facts. Investigate disagreements before changing accepted state. Report unresolved rule conflicts before dependent work.

Folders and load order do not create inherited rules or precedence. Each policy must state its scope and required companions directly. An overwrite filename has no special behavior. Replace a source by changing its read-list link.

Use the optional catalogues below to find advice, examples, skills, templates and records. Follow useful ordinary links without reconstructing ancestor chains. Catalogues contain ordinary navigation links. Native skills retain their runtime's activation rules.

When accepted changes affect documentation, update the source that defines the result. Save unfinished state only when future work needs it.

## CLI

The CLI is optional. Use `open-forge context --layout lists` only when the installed help advertises that mode. Otherwise perform the reads above. See the [CLI guide](skills/open-forge-cli/SKILL.md) when using the CLI.

## Entries

- [Project rules](directives/_directives.md) - #Directive
- [Recommended approaches](guidance/_guidance.md) - #Guidance
- [Useful local and external sources](maps/_maps.md) - #Map
- [Working notes, candidates, accepted knowledge and history](memory/_memory.md) - #Memory
- [Reusable structural examples](patterns/_patterns.md) - #Pattern
- [Native skill packages](skills/_skills.md) - #Skill
- [Starting files to copy and adapt](templates/_templates.md) - #Template
```

#### 5. Other Markdown changes

**AGENTS.md** becomes a direct handoff:

```markdown
# Open Forge

Before starting work, read `.agents/loader.md` and perform its required reading.

**End of Open Forge managed section.**
```

Keep `CLAUDE.md` as native references to AGENTS and the loader.

**Category entrypoints** become short optional catalogues. Remove inherited markers, loading tags and category-wide authoring regulations. Put useful authoring instructions beside the relevant starter or procedure.

For the example workspace below, `.agents/patterns/_patterns.md` is complete as follows:

```markdown
# Patterns

Examples of reusable structures. Each source states when it fits
and which details may change.

## Entries

- [HTTP response shape](../../docs/api/response-shape.md) - #HTTP
```

**Memory** remains an optional, self-growing notebook. Retain the four familiar folders without automatic loading or inherited status:

- Working holds temporary continuation state.
- Emerging holds candidates and visible uncertainty.
- Crystallized holds records with explicit acceptance provenance.
- Archived holds history and identifies replacements where useful.

Each record states its own status. Moving it does not approve it. Users needing continuity can put the exact active checkpoint in a read list rather than refresh an entire state catalogue.

**Tags** have one reserved operational meaning, LoadNow. Other tags support search. Accepted status belongs in readable source text.

**Frontmatter** becomes optional for participation. Existing fields and both metadata forms remain readable by authoring tools. Loading conditions live on read-list entries. Frontmatter can supply generation suggestions, but cannot silently add loading policy.

**The CLI Skill** becomes optional. Its first screen explains lists mode, path inputs and result handling. Mutation details remain in its references.

#### 6. Decisions: model vs mechanism

| Decision today | Proposed owner and rule |
|---|---|
| Which mandatory scopes to select | **Mechanical:** enumerate load.d and follow required links |
| Whether a required source seems relevant | **Removed:** relevance cannot veto a declared required read |
| Which working files the task involves | **Model:** identify actual and planned paths |
| Whether a file condition matches | **Mechanical:** existing glob matcher, with conditions on individual source links |
| What to do before paths are known | **Mechanical:** read conditional entries conservatively |
| Which ancestors to recover after search | **Removed:** sources state dependencies directly |
| Which optional explanation would help | **Model:** descriptions, search and ordinary links |
| Whether evidence establishes acceptance | **Model:** inspect provenance and applicable user authority |
| Which conflicting instruction wins | **Model:** investigate authority, report unresolved conflicts |
| When required context refreshes | **Fixed checkpoints:** the same pass at each named boundary |
| What deserves a durable record | **Model:** optional Memory policy and future value |
| Who may update packaged files | **Mechanical:** ownership and exclusions, documented outside startup |
| How navigation is produced | **Mechanical:** emit candidate Markdown for an explicit authoring operation |

The CLI can compute a read set. It cannot prove that the agent understood or followed it.

#### 7. Customization, independence and blending

A small workspace can look like this:

```text
AGENTS.md
.agents/
  loader.md
  load.d/
    10-project.md
    30-continuity.md
  directives/_directives.md
  maps/_maps.md
  patterns/_patterns.md
  memory/working/current.md
docs/
  README.md
  contributing.md
  api/
    http.md
    response-shape.md
  adr/
    0001-storage.md
src/
  api/
    orders.cs
```

Other catalogue folders can be added when useful. They are not prerequisites.

`.agents/load.d/10-project.md` contains:

```markdown
# Required project context

## Entries

- [Contribution rules](../../docs/contributing.md) - #LoadNow
- [HTTP API contract](../../docs/api/http.md) - #LoadNow - applies to `src/api/**`
```

The contribution rules and HTTP contract are the same files people read. They need no projection, duplicate agent version or entrypoints in their parent folders. The contract states its own accepted scope. An unaccepted document remains a reference even when reading it is mandatory.

A user can:

- **Add** a policy by dropping a file into load.d or adding a required link.
- **Disable** a drop-in by moving it out of that directory or changing its extension.
- **Replace** a source by pointing its link to a local alternative.
- **Remove** a catalogue without changing mandatory loading.
- **Adopt** only templates, patterns or Memory by linking those files from existing project documentation.

Filename order determines reading order, not precedence. Local changes take effect through explicit replacement rather than competing layers.

For large projects, use one drop-in per subsystem with conditions on its leaf links. Every drop-in is inspected, but unrelated source bodies stay closed when working paths are known.

Maps remain useful curated navigation. Libraries remain optional distribution and projection tools. Neither is needed merely to make `docs/` participate.

#### 8. Cost and migration

This is **moderate parser work and substantial semantic migration**, not a prose-only patch.

| Area | Required change |
|---|---|
| CLI loading | Add explicit `--layout lists` mode. Enumerate drop-ins, resolve workspace paths, evaluate entry conditions and deduplicate destinations without ancestor activation. |
| Parsing | Reuse Entries and link grammar. Accept links beyond direct children and beyond `.agents/`. Validate that conditions target sources rather than read lists. |
| Index | Add a proposed `index --emit <directory>` operation that prints candidate Entries without editing sources or existing lists. Never infer mandatory loading. |
| Discovery | Make file paths first-class identities in lists mode. Allow catalogues and referenced documentation without intermediate entrypoints. |
| Installation | Treat loader, catalogues and load.d as local configuration. Preserve edits and offer proposed updates separately. Retain exclusions for intentional removals. |
| Tests | Update payload and CLI snapshots. Cover traversal order, duplicate paths, cycles, missing targets, conditions, unknown paths and untouched human documents. |
| Documentation | Rewrite startup, routing, customization and blending explanations. Update affected maintenance contracts and decisions. |

Migration must enumerate existing required reads before changing behavior.

Move inherited obligations into focused, self-contained policy files and link them explicitly. Convert scoped loading into leaf conditions where meaning is equivalent. Some inherited condition intersections cannot be represented by one alternative-pattern list, so report those for author decisions.

For overwrite companions, produce a comparison and a proposed replacement source. Do not concatenate contradictory instructions or silently discard local edits.

Keep legacy workspaces on their current contract until explicitly migrated. The new flag prevents agents from assuming an old CLI implements this design. A manual Markdown trial can precede all CLI implementation.

#### 9. Risks and proof

The principal risk is exchanging recursive discovery cost for a growing mandatory read inventory. Small drop-ins help maintainability, but do not make excessive required reading cheap.

Other risks are:

- File-less discussion conservatively reads more conditional material.
- Refreshing all required sources costs more than refreshing selected continuity records.
- Removing inheritance can lose rules during migration.
- Preserving editable configuration changes updater behavior.
- Optional catalogues may be overlooked more often.

The cheapest useful experiment is a disposable fixture, not a CLI rewrite. Compare the current payload and proposed files on six known-answer tasks:

1. Follow a workspace rule during a small change.
2. Discover rules for planned files and a second language.
3. Handle a task whose working paths are initially unknown.
4. Use an accepted document in docs while rejecting a conflicting candidate.
5. Resume work and preserve required closeout state.
6. Replace one policy and remove an unrelated catalogue.

Run two isolated sessions per task and format, giving 24 sessions with the same model and tools. Score actual task results against predetermined obligations, missed sources, irrelevant words read and clarification requests. Separately compare the manual read set against a handwritten expected list.

Adopt the design only if mandatory-rule adherence improves without losing documentation correctness or continuity. This review changed no files and ran no diagnostics, builds or tests.

#### 10. The one idea to steal

Separate mandatory reading from optional discovery through a small drop-in directory of literal Markdown links that can target any project file.

## Steward (Proposal J)

#### Thesis

Ship a shorter operating guide over the existing Markdown syntax, preserving scoped loading, additive rules, user ownership, and the distinction between candidates and accepted knowledge. Reduce startup context by shortening entrypoints and removing the unconditional CLI Skill read. Add ordinary documentation as explicitly linked, workspace-contained routing roots, while keeping installation ownership separate from readable meaning.

#### Diagnosis

1. **The loader teaches installation machinery during every task.**

   > “Each manager defines the route shapes it recognizes. It preserves their segment order through scopes.”

   Source: `src/open-forge/.agents/loader.md`

   This matters when maintaining installed files. It does not help an agent select the rules for its current task. Readers must learn lifecycle vocabulary before doing ordinary work.

2. **One navigation mechanism has too many names.**

   > “Any number of routed slugs may narrow a route below its root. They may appear before, between, or after deeper route segments.”

   Source: `src/open-forge/.agents/loader.md`

   The flexibility is useful. Explaining it through routes, roots, slugs, segments, and scopes makes a folder hierarchy feel like a programming language. Most readers need to understand entrypoints, links, and narrowing scope.

3. **An optional authoring convention becomes mandatory startup knowledge.**

   > “Never combine the inherited marker with local rules.”

   Source: `src/open-forge/.agents/loader.md`

   Missing and empty local sections already mean no additional rules. The marker adds a spelling rule without adding runtime capability. Stop requiring it in newly authored files.

4. **Useful native judgment is surrounded by prescriptive conversation rules.**

   > “Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.”

   Source: `src/open-forge/.agents/loader.md`

   This imposes a response formula on unrelated tasks. Preserve clear communication, user direction, and consequential decision boundaries. Remove the universal presentation recipe.

5. **An empty category brings a substantial authoring and review policy.**

   > “A justified adaptation is still a departure, so a review reports it separately from conformance.”

   Source: `src/open-forge/.agents/patterns/_patterns.md`

   A new user encounters this before having any Patterns. Keep the essential default-shape behavior in the entrypoint. Put detailed authoring and review methods in the sources selected for those activities.

6. **Startup has two loading mechanisms.**

   > “When `open-forge-cli/SKILL.md` is present, read it when this entrypoint loads so command options and context-loading advice are available from the start.”

   Source: `src/open-forge/.agents/skills/_skills.md`

   Meanwhile:

   > “The CLI follows loading tags, not prose instructions.”

   Source: `src/open-forge/.agents/skills/open-forge-cli/SKILL.md`

   An agent following only the computed startup closure misses the prose-required guide. Remove this shipped exception. Keep a small CLI bootstrap in the loader and load the guide when using its capabilities.

7. **Navigation and knowledge are correctly separated, but participation remains tied to location.**

   > “Map is the Core primitive for durable navigation to important local and external destinations.”

   Source: `.agents/memory/crystallized/decisions/framework/map-primitive.md`

   Maps should remain useful pointers. They should not be compulsory adapters for every existing documentation tree. The routing-surfaces decision already provides the right selection interface for documents wherever they live. Today, source identity explicitly requires `.agents/`, while linked outside documents are represented as unrouted sources in `SourceLogicalPath.cs` and `ContextLinkedSourceReader.cs`.

#### Concept budget

Count named protocol ideas, rather than every filename, command, or Markdown spelling. Count `#Memory` with Memory rather than twice.

| Area | Current concepts | Count |
|---|---|---:|
| Navigation | Loader, entrypoint, entry, route, root route, scope, slug, managed route | 8 |
| Rules and knowledge | Axiom, inherited marker, validation, acceptance | 4 |
| Content roles | Directive, Guidance, Map, Pattern, Skill, Template, Memory | 7 |
| Memory states | Working, Emerging, Crystallized, Archived | 4 |
| Other defined tags | LoadNow, KeepInMind, Core, Extension, Contextual, CurrentTruth, Evergreen | 7 |
| Metadata | Frontmatter, root/scoped placement, description, responsibility, tags, applyTo, working files | 7 |
| Customization | Overwrite companion, removal exclusions | 2 |
| CLI addressing | Source reference | 1 |
| Loading lifecycle | Loading/refresh pass | 1 |
| Unresolved applicability | Pending conditions | 1 |
| **Total** | | **42** |

The proposed startup vocabulary has **25 concepts**:

- Navigation: entrypoint, entry, scope.
- Rules and knowledge: required rule, validation, acceptance.
- Other roles: Guidance, Map, Pattern, Skill, Template, Memory.
- Four Memory states.
- Five operational tags: LoadNow, KeepInMind, Contextual, CurrentTruth, Evergreen.
- File conditions: applyTo, working files, pending conditions.
- Overwrite companion.

The reductions are deliberate:

| Removed startup concept | Disposition |
|---|---|
| Loader as a separate navigation abstraction | Merged into entrypoint |
| Route and root route | Merged into linked Entries and explicit starting branches |
| Slug | Merged into ordinary folder scope |
| Managed route | Moved to package maintenance |
| Axiom and Directive as separate kinds of obligation | Presented as required rules, retaining their existing headings and locations |
| Inherited marker | Deleted as an authoring requirement |
| Core and Extension tag definitions | Moved to package documentation, retaining their classification meanings |
| Frontmatter, placement, description, responsibility, tag syntax | Moved to authoring references |
| Removal exclusions | Moved to managed maintenance instructions |
| Source-reference grammar | Moved to the CLI Skill |
| Loading/refresh pass as separate terminology | Explained through the loading tags |

This reduces required vocabulary. It does not pretend that scope, acceptance, or uncertainty can disappear.

#### The new loader

**829 whitespace-delimited words**, including headings and Entries, compared with **2,052** in the current shipped file.

```markdown
# Open Forge Loader

Use this file to find the rules and knowledge for your task. The Markdown works with or without the CLI.

## Axioms

### Find the context

An entrypoint is a folder's navigation file, normally `_{folder-name}.md`. Compatibility names are `index.md`, `_index.md`, `references.md`, and `_references.md`. Each folder in a selected chain has exactly one entrypoint.

1. Start with `Entries` below. Use descriptions, paths, tags, and explicit links to select relevant branches. Read each selected entrypoint before its children. Use Entries for discovery rather than opening every file.
2. Folder scopes narrow the context below them. Keep independently selected branches separate. Only this loader exposes independent starting branches. Reusing a familiar folder name or tag creates no additional role.
3. Read required context before working in its scope. Search results and direct links to routed files still require their ancestor entrypoints and required context.
4. Recheck selection when the goal or working files change. Before handoff or closeout, follow the loaded completion and Memory rules.

### Read required files

- #LoadNow means read the linked file when its parent loads.
- #KeepInMind means read it then, and refresh it at task start or resume, after context restoration, and before handoff or closeout while its scope remains active. Recheck during work when those sources may have changed.
- Both tags operate only through loaded parents. Neither selects an unopened branch. For an entrypoint, continue through its required children.
- Read required files in listed order, even when an individual description seems less relevant. Read each source once per loading or refresh pass. Changed sources may need another read.
- Other entries stay on demand unless a loaded instruction explicitly requires them.
- Read an adjacent `{name}.overwrite.md` immediately after `{name}.md`. It shares the base's role, scope, and loading. It replaces corresponding base content, never unrelated sources, and is not selected separately.

### Check file conditions

An entry's `applies to` patterns come from `applyTo` and filter loading and applicability. They never select a branch or grant edit permission.

One working file must match a pattern at that entry and at every ancestor that declares patterns. Working files are those the task investigates, creates, changes, deletes, renames, or reviews, including planned files. Reading a file for context does not make it a working file.

Patterns are case-sensitive and relative to the workspace root. `*.py` matches root files, `**/*.py` any depth, and `docs/**` a folder. A leading `!` is literal.

While working files are unknown, report conditioned #LoadNow and #KeepInMind entries as pending. Resolve their conditions before relying on affected rules.

### Follow rules and preserve knowledge

- Follow platform constraints, runtime safety, and allowed user direction. Continue routine reversible work within the request. State material assumptions. Keep communication and authored content clear and concise. Ask about unresolved choices that significantly affect scope, results, risk, cost, external effects, or reversibility.
- `Axioms` in this loader and selected entrypoints, and `Instructions` in selected Directives, are required within their scope. Child rules add to ancestor rules. They never silently cancel them. Missing or empty local Axioms add no rules. Report unresolved conflicts.
- Accepted sources define their assigned subjects. Use declared external authorities for the facts assigned to them. Links and tags do not grant authority or turn recorded instructions into active rules.
- #Contextual marks material not accepted as current unless applicable authority establishes otherwise. #CurrentTruth marks accepted current state within its stated scope.
- Evidence supports claims. Acceptance establishes current state. Confidence, repetition, tags, or moving a record do not establish acceptance. Investigate apparent conflicts before changing accepted sources.
- Update the source that defines changed knowledge, keeping useful prior reasoning. #Evergreen requires authorized updates before work depends on that source and no later than closeout. Report blocked updates.
- Keep one detailed source for each question. Link to related detail with relative local links. Put specialized content in the narrowest useful scope. Other tags describe content for selection and search.

### Use and customize the files

Edit, replace, add, or remove pieces as needed. Overwrite companions preserve local adjustments to managed defaults. When using managed updates, record intentional removals through the manager's exclusions so defaults stay removed. Management determines which files tools may change, never their authority.

The optional CLI can batch these reads with `open-forge context`. Supply known working paths with repeated `--for`. It computes loading tags, file conditions, and companions, not relevance or prose-required reads. The CLI Skill, when present, explains discovery, file authoring, and managed changes. Otherwise read and maintain the plain files directly.

## Entries

- [Required rules for selected scopes](directives/_directives.md) - #LoadNow #Core #Directive
- [Advice for recurring choices](guidance/_guidance.md) - #LoadNow #Core #Guidance
- [Where project sources live and when to use them](maps/_maps.md) - #LoadNow #Core #Map
- [Active work, candidates, accepted knowledge, and history](memory/_memory.md) - #LoadNow #Memory
- [Reusable default shapes](patterns/_patterns.md) - #LoadNow #Core #Pattern
- [Native capabilities entered through SKILL.md](skills/_skills.md) - #LoadNow #Core #Skill
- [Starting files to copy and maintain independently](templates/_templates.md) - #Core #Template
```

#### Other Markdown changes

**AGENTS.md** becomes:

```markdown
# Open Forge

Before starting work, read `.agents/loader.md` and follow its selected rules.

**End of Open Forge managed section.**
```

Keep the Claude bridge unchanged.

**Category entrypoints** retain a purpose sentence, their essential behavioral rules, and Entries. Remove repeated loader rules, question headings, and extensive authoring checklists. Preserve Guidance as adaptable advice, Patterns as default shapes, Templates as independent copies, Maps as pointers, and Skills as runtime-owned capabilities.

For example, the complete Directives entrypoint becomes:

```markdown
---
open-forge:
  description: Required rules for selected scopes
  tags: [LoadNow, Core, Directive]
---

# Directives

Directives require behavior within their selected scope.

## Axioms

- Every Directive file beside this entrypoint must have #LoadNow and keep its required behavior under a non-empty `## Instructions` heading.
- Follow those Instructions when this scope is selected. Files at this root apply workspace-wide.
- Select child entrypoints for relevant work. Their rules add narrower requirements.
- State obligations, conditions, and meaningful exceptions explicitly. Examples clarify the instructions.

## Entries

- none - No entries - #Empty
```

**Memory** keeps its four paths and existing loading tags. Shorten its shared rules to useful capture, visible uncertainty, accepted-source updates, movement without competing copies, and authorized retention. State entrypoints carry only their distinct obligations:

- Working preserves resumable state and expires when its active need ends.
- Emerging preserves useful candidates and reviews them at refresh boundaries.
- Crystallized contains accepted knowledge and consolidates competing versions.
- Archived preserves useful history without restoring authority automatically.

Records need no mandatory progression through these states.

**Tags and frontmatter** keep their existing spellings and parser behavior. Keep full metadata syntax, frontmatter placement, optional `responsibility`, and loading-tag authoring advice in installed authoring references. Stop emitting the inherited marker. Existing markers remain valid.

**The CLI Skill** becomes an on-demand guide with a short command map and links to its existing references. Read it before unfamiliar CLI operations and its maintenance references before mutations. Remove the unconditional read from Skills. This changes startup behavior and needs evidence, rather than being presented as a wording-only edit.

Target a fresh startup below **1,800 words**. That is an acceptance budget for the completed payload, not a measured result of this proposal.

#### Decisions: model vs mechanism

| Decision today | Proposed responsibility |
|---|---|
| Which task branches matter | Model, using compact descriptions and explicit links |
| Which exposed files must load | Tags and conditions, computed by CLI or followed manually |
| Whether a tagged descendant activates its parents | Mechanism, it does not |
| Whether one working path satisfies inherited conditions | Mechanism |
| Whether a context read expands the working-file set | Removed, it does not |
| Which ancestor rules apply | Explicit entrypoint chain |
| Whether a narrower rule cancels an ancestor | Removed, report conflicts |
| Whether evidence establishes acceptance | Model checks declared acceptance, tools never infer it |
| Which overwrite content replaces the base | Model interpretation within one source |
| Which files an updater may change | Existing lifecycle ownership and exclusions |
| Whether an empty scope needs a marker | Removed |
| How metadata and Entries are formatted | Existing parser and Index |
| Whether a discovery merits Memory | Model, with “nothing useful to save” valid |
| Whether documentation must move into `.agents` | Removed by additional routing roots |

#### Customization, independence and blending

Users retain direct editing, replacement, overwrite companions, and persistent removal exclusions. No category requires installing its neighbors. A Directive can be read as instructions, a Pattern as an example, a Template as starting content, and Memory as ordinary records. Entrypoints add selective discovery when useful.

For blending, extend the existing link graph rather than introduce a second manifest. An explicitly exposed entrypoint may live anywhere inside the workspace:

```markdown
- [Architecture and decisions for product work](../docs/_docs.md) - #LoadNow
```

That entry loads the small documentation index. Individual documents remain on demand unless tagged otherwise. The same `docs/architecture.md` serves people and agents.

```text
AGENTS.md
README.md
.agents/
  loader.md
  directives/
    _directives.md
    testing.md
  maps/
    _maps.md
    project.md
  memory/
    _memory.md
    working/
      _working.md
      checkout.md
    emerging/
      _emerging.md
      retry-idea.md
    crystallized/
      _crystallized.md
docs/
  _docs.md
  architecture.md
  adr/
    _adr.md
    001-storage.md
src/
  checkout/
    design.md
```

Unused defaults are intentionally omitted in this example. `project.md` can still point to the README, documentation tree, and adjacent design note. A further explicit root could expose a component’s own entrypoint.

Keep the reasoning behind routing surfaces: descriptions permit selection before bodies are read. Keep the Map boundary: pointers do not replace destination knowledge. Change the physical restriction that makes Maps compulsory adapters.

Documentation gains no directive authority merely by becoming routed. Drafts remain candidates. Declared current documents remain current sources. Memory can preserve reasoning and link to them without keeping another current copy.

This follows the accepted principles. It deliberately revises the present `.agents` routing boundary.

#### Cost and migration

| Delivery slice | Cost and boundary |
|---|---|
| **Ship now without CLI changes** | Replace loader prose, shorten entrypoints, remove the CLI startup exception, and move authoring detail into existing references. Preserve headings, metadata grammar, paths, loading tags, and overwrite semantics. |
| **Small CLI changes** | None are required for the immediate reduction. Avoid inventing a new schema merely to justify tooling work. |
| **Next additive feature** | Support explicitly linked documentation roots in read-only routing. This is a small syntax change but a **medium implementation change**. |
| **Later** | Generalize authoring and lifecycle operations across those roots, with explicit write permissions. |

Additional roots affect source identity, argument binding, inventory, route construction, Context, Find, References, and route inspection. Navigation generation must preserve those roots even when their contents are not managed. Status and Doctor must describe the same graph.

Retain existing source IDs. Initially address additional sources by exact workspace-relative paths. Require unambiguous entrypoints, reject overlapping roots and escapes, and preserve workspace containment. Root discovery must remain explicit.

The Markdown parser needs no new Entries or frontmatter grammar. C# identity and routing validation do change. Add focused coverage for external roots, inherited conditions, companions, duplicate exposure, containment, and preservation during indexing.

Existing workspaces need no folder migration. Update managed prose through existing previews and recovery. Review overwrites against changed wording. Enable additional roots only with a supporting CLI, since older navigation writers cannot be assumed to preserve them.

Update maintenance contracts, concepts, loading tables, CLI examples, glossary explanations, and startup measurements. Keep historical rationale historical. The abandoned simplified-core attempt retained most of the same machinery, which helps explain its limited reduction.

I expect alternatives involving three universal folders, a manifest, mandatory compiled context, and automatic directory precedence. Explicit document roots are worth their cost. Wholesale taxonomy migration and new precedence rules are poor beta-closeout investments. Optional computed context is already valuable through Context, without becoming mandatory or privately authoritative.

#### Risks and proof

Shortening can hide important qualifications. Removing the CLI startup read could increase command misuse. Additional roots could make a large documentation tree expensive to inspect or accidentally broaden rule scope.

Preserve the current early loading of baseline indexes. The loading-reliability decision records failures from deferring critical context. Do not solve startup size by making every important file optional.

The cheapest useful experiment is **12 fresh sessions**: six tasks using the current payload and six using the proposal, initially without the CLI. Cover nested mandatory rules, an irrelevant sibling, pending file conditions, overwrite conflicts, candidate instructions, and resume/closeout behavior. Include ordinary documentation in the blended fixture.

Score actual file reads and resulting answers or patches against predetermined expectations. Do not score self-reported compliance. Measure startup words and unnecessary reads.

Any scope, acceptance, or precedence regression blocks shipment. Repeat ambiguous results and run the same fixtures with CLI assistance before accepting the routing increment. A small pilot can falsify the proposal, but cannot establish universal reliability.

This proposal was developed through read-only inspection. No files were changed, and no Doctor, builds, or test suites were run.

#### The one idea to steal

Separate the instructions needed to use context from the instructions needed to author and manage it, while letting both people and agents reach the same files.

## Model Whisperer (Proposal E)

#### Thesis

Turn the loader into a short procedure for starting, reading, changing scope and finishing. Keep small navigation indexes visible early, move authoring and package detail to their moment of use, and let explicitly enrolled documentation folders participate directly. Preserve the accepted principles, especially user-owned meaning, context by relevance and honest reliability.

#### Diagnosis

Whitespace counting gives **2,052 words for the current loader** and **4,563 words for startup through AGENTS.md**, including the required CLI guide.

1. **Vocabulary arrives before executable behavior.**  
   `src/open-forge/.agents/loader.md` defines:

   > `slug` - Concrete folder name in a route path.

   A reader must learn several names before receiving a startup procedure. The model can reproduce these definitions without using them to discover a single required rule. Use ordinary folder paths and introduce the few necessary terms inside the procedure.

2. **Selection has inputs but no completion condition.**  
   `src/open-forge/.agents/loader.md` says:

   > Select what matters from descriptions, tags, paths, ancestor routes, and explicit links.

   This permits a superficial relevance judgment. Require comparison of every visible description with the task’s action, subject and working area, followed by repetition until every selected index has been checked.

3. **The startup contract is distributed across prose.**  
   `src/open-forge/.agents/skills/_skills.md` says:

   > When `open-forge-cli/SKILL.md` is present, read it when this entrypoint loads so command options and context-loading advice are available from the start.

   The CLI cannot discover this dependency from loading tags. People must reconstruct startup across several files, while models receive command maintenance detail before knowing whether they need it. Remove this compulsory tutorial and retain a short CLI entry in the loader.

4. **Authoring conventions consume runtime attention.**  
   `src/open-forge/.agents/loader.md` says:

   > Never combine the inherited marker with local rules.

   This matters while editing an index, not while implementing a feature. Existing missing or empty sections already mean no additional rules. Stop requiring the marker and move detailed authoring conventions to authoring references.

5. **Refresh combines reading with deciding what to save.**  
   `src/open-forge/.agents/memory/emerging/_emerging.md` says:

   > At each applicable #KeepInMind refresh, check visible `Entries` and save useful candidate material before it is lost.

   Refreshing known sources and discovering new durable findings are different actions. Give rereading its existing boundaries, then place capture explicitly before handoff or closeout.

6. **Composition labels compete with operational instructions.**  
   `src/open-forge/.agents/loader.md` defines:

   > #Core - Base routing and loading, workspace orientation, and reusable agent-facing roles.

   Knowing that something is Core does not determine whether to read, obey, refresh or update it. Keep composition tags searchable, but stop making their definitions part of the universal operating vocabulary.

7. **Documentation is reachable without being a full participant.**  
   `src/open-forge/.agents/skills/open-forge-cli/references/common.md` says:

   > A source reference is an exact source ID, such as `memory/crystallized`, or an exact `.agents/...` Markdown path.

   Maps preserve useful navigation to ordinary documentation, but they leave that documentation outside normal routed context operations. Explicitly enroll documentation indexes while retaining the destination’s own meaning and authority.

#### Concept budget

The count below treats each independently explained operational distinction as one concept. Compatibility spellings are syntax variants, not additional concepts.

| Area | Concepts today | Count |
|---|---|---:|
| Navigation | entrypoint, entry, route, root route, scope, slug, ancestor chain, generated Entries | 8 |
| Rules and roles | Axiom, Directive, Guidance, Pattern, Skill, Template, Map, role authority | 8 |
| State and trust | Memory, Working, Emerging, Crystallized, Archived, validation, acceptance, declared external authority, overwrite | 9 |
| Defined tags | LoadNow, KeepInMind, Core, Memory, Extension, Contextual, CurrentTruth, Evergreen | 8 |
| Metadata | frontmatter, root/scoped form, description, responsibility, tags field, applyTo, working file, pending condition | 8 |
| Management | managed route, manager, exclusion | 3 |
| **Total** | | **44** |

The design keeps **34 concepts**, including optional modules:

- **Navigation, 3:** index, entry, scope.
- **Rules and roles, 7:** required rule, Guidance, Pattern, Skill, Template, Map, role authority.
- **State and trust, 9:** the same nine distinctions.
- **Defined tags, 5:** LoadNow, KeepInMind, Contextual, CurrentTruth, Evergreen.
- **Metadata, 8:** the same eight distinctions.
- **Management, 2:** management, exclusion.

The reductions are deliberate:

- Merge entrypoint into **index**. Merge route, root route, slug and ancestor chain into the visible index hierarchy and scope. Merge generated Entries into the entry concept.
- Merge Axiom and Directive as **required rules** for readers. Retain their existing headings and file organization.
- Delete the reserved meanings of Core, Memory and Extension tags. Their spellings remain ordinary search labels.
- Merge manager and managed route into **management**, explained only during package maintenance.
- Delete the inherited-marker requirement.
- Move metadata authoring, compatibility syntax and lifecycle detail to narrower references. Keep the four Memory states inside the Memory module.

This reduces vocabulary without pretending that acceptance, conditions or customization have become unnecessary.

#### The new loader

**775 words**, including headings and Entries, counted by whitespace. This is approximately 62% shorter than the current loader.

```markdown
# Open Forge Loader

## Axioms

### Start or resume

1. Read `loader.overwrite.md` if present. Identify the goal and working files, including files you will investigate, review, create, change, rename or delete. Reading a source only for context does not make it a working file.
2. Follow `Entries` below. Read entries marked #LoadNow or #KeepInMind in listed order, checking file conditions before opening them. Repeat this step inside each index you read.
3. Compare every other visible description with the task's action, subject and working area. Open matching indexes before their children. Repeat until every selected index has been checked. Before creating an artifact, also check Templates.
4. Read the selected sources and their explicit required-read links before relying on them. Read each source once per pass. If search or a direct link found an indexed source first, read its index chain from this loader before applying its rules.
5. Before acting on a working file, resolve its pending conditions. Report missing required sources or unresolved conflicts and continue only work independent of those gaps.

An index lists its folder's direct children under `## Entries`. Use `_{folder-name}.md` for new indexes. A loader entry may instead lead to an index elsewhere inside the workspace, such as `../docs/_docs.md`. Ordinary links are references, not additional instruction scopes.

### When reading a source

- Read its adjacent `{name}.overwrite.md` immediately after `{name}.md` when present, including this loader's companion. Interpret the pair as one source. The companion replaces corresponding base content only.
- Follow this loader, `## Axioms` in selected indexes, and instructions those indexes declare required. Each child narrows the scope and adds rules. It cannot cancel parent rules. Names, tags and reading order do not grant precedence.
- Follow higher-priority platform instructions and clear user direction. Make routine reversible choices within the request. Ask about unresolved choices that materially change the outcome, scope, risk, cost or external effects.
- Use accepted documents for the requirements and facts they define. Follow declared external authorities for their assigned facts. Investigate conflicting sources before changing either, then report any unresolved conflict.
- Treat #Contextual material as unaccepted unless acceptance is explicit. #CurrentTruth describes accepted state within its stated scope. Evidence, confidence and moving a file do not establish acceptance.
- Treat other tags as search labels, except #Evergreen, which requires keeping the source aligned with accepted changes before work depends on it and by closeout.

### Check file conditions before loading

`applies to` in an entry repeats its source's `applyTo` metadata. A source loads and applies only if one working file matches a pattern at every conditioned parent and at the source itself. Patterns within one list are alternatives. Apply conditioned rules only to matching working files. A match filters a selected branch and never selects a hidden branch.

Match case-sensitive globs from the workspace root. `*.py` matches root files, `**/*.py` matches any depth, and `docs/**` matches that folder. A leading `!` is literal.

When working files are unknown, mark conditioned mandatory entries pending and continue unconditioned investigation. Check them again when paths become known. Conditions do not grant edit permission.

### Recheck at the moment work changes

- Before work reaches a new path or subject, repeat selection for that area.
- After context restoration, repeat the startup procedure. At task start or resume and before handoff or closeout, reread #KeepInMind sources in selected scopes and apply their child loading rules. Also reread active sources when they may have changed.
- Before finishing, check the result against the loaded rules and requirements. Update documents affected by accepted changes and useful unfinished state within your authority. Preserve worthwhile unsettled findings without presenting them as accepted. Report blocked updates and missing evidence.

#LoadNow means read when its parent is read. #KeepInMind adds the refresh points above. Untagged entries wait for selection. Neither tag opens an unselected parent.

The optional CLI batches these reads. Use `open-forge context` with selected sources and `--for` for each working path. It follows loading tags, not required-read prose or semantic relevance. Read the CLI Skill before unfamiliar commands, or use `--help` if it is absent. Without the CLI, follow the same links manually.

## Entries

- [Required instructions for this task and its working areas](directives/_directives.md) - #LoadNow #Directive
- [Advice to check before choosing an approach](guidance/_guidance.md) - #LoadNow #Guidance
- [Important sources to check before relying on project knowledge](maps/_maps.md) - #LoadNow #Map
- [Saved work, accepted knowledge, candidates and history](memory/_memory.md) - #LoadNow #Memory
- [Default shapes to check before creating or reviewing structure](patterns/_patterns.md) - #LoadNow #Pattern
- [Capabilities and procedures to check before specialized work](skills/_skills.md) - #LoadNow #Skill
- [Starting files to check before creating an artifact](templates/_templates.md) - #Template
```

#### Other Markdown changes

**AGENTS.md** becomes a direct handoff:

```markdown
# Open Forge

Before starting or resuming work, read `.agents/loader.md`
and execute its startup procedure.

**End of Open Forge managed section.**
```

Keep CLAUDE.md as the existing native imports.

**Category indexes** retain a short purpose, action-triggered rules and Entries. Remove the introductory question headings and repeated definitions. Keep the existing startup loading choices, except the compulsory CLI guide.

Use concrete triggers:

- Guidance: before choosing an approach, read matching advice.
- Patterns: before choosing or reviewing structure, read matching shapes.
- Skills: before using a capability, read its SKILL.md.
- Templates: before creating an artifact, select and adapt a starter.
- Maps: before relying on project knowledge, follow the pointer to its defining source.

A complete replacement Directives index:

```markdown
---
open-forge:
  description: Required instructions for this task and its working areas
  tags: [LoadNow, Directive]
---

# Directives

## Axioms

- Read every listed rule file in order after checking its file conditions. Follow its `## Instructions`. Select child indexes by their descriptions before reading their rules.
- Rules here cover the workspace. Child indexes narrow their rules and retain parent rules.
- When adding a rule file, give it #LoadNow and put complete obligations under `## Instructions`. Put specialized rules in a matching child index.

## Entries

- none - No entries - #Empty
```

**Memory** retains its four optional states and existing loading tags. Shorten each state to its operational distinction:

- Working: load the matching continuation record before resuming. Update it before transfer.
- Emerging: retain evidence and uncertainty. Capture worthwhile findings before closeout.
- Crystallized: use accepted knowledge and update its existing defining source.
- Archived: use as history. Validate and establish acceptance before restoring current meaning.

Refreshing Emerging still rereads its exposed navigation. It no longer independently starts a capture or cleanup exercise.

**Tags** retain five reserved meanings. Core, Memory and Extension become search labels. Keep Template and other category labels usable by existing command contracts.

**Frontmatter** keeps both supported forms and all existing fields. Move writing instructions and detailed syntax to the authoring references and templates. No new metadata language is needed.

**CLI Skill** loads before unfamiliar CLI use. Its references retain authoring and package instructions, usable as documentation without the executable.

Set a fresh-install startup target of **at most 1,500 words** after rewriting the remaining indexes. That is a proposed budget, not a measured completed payload.

#### Decisions: model vs mechanism

| Decision today | Proposed treatment |
|---|---|
| Which task subjects matter | Model compares every visible description with action, subject and working area |
| Which mandatory children to read | Mechanical tag traversal through selected parents |
| Whether file conditions match | Mechanical glob evaluation through the complete condition chain |
| What to do with unknown paths | Explicit pending state, resolved before acting on the affected path |
| Whether search bypasses parent rules | Mechanical parent-chain recovery |
| Which source answers a project question | Model follows declared responsibility and authority |
| Whether evidence establishes acceptance | It does not. Acceptance remains a user or declared-authority decision |
| Whether a narrower rule wins | Remove this inference. Required rules accumulate |
| Whether context needs refreshing | Explicit events, plus evidence that active sources changed |
| Whether to preserve a finding | Model judgment at handoff or closeout |
| How Entries and metadata stay synchronized | Optional CLI generation, with manual authoring still supported |
| Whether package provenance grants authority | Remove this inference |
| Whether a familiar folder name creates a role | Remove this inference |
| Which files an updater may change | Mechanical ownership and exclusions, outside runtime selection |

#### Customization, independence and blending

Keep the current edit, replace, overwrite and remove choices. Managed direct edits can be replaced by updates. Overwrite companions survive them. Persistent removal still requires recording exclusions, which belongs in package maintenance instructions.

Each optional category carries its own use instructions. A workspace can retain only Directives, adopt Maps independently, use native Skills without Memory, or keep documentation without any Memory taxonomy. Removing a category removes its loader entry and dependent references, without recreating defaults.

Enroll ordinary documentation through explicit loader links:

```markdown
- [Current architecture and accepted decisions for product work](../docs/_docs.md) - #Document
- [Payment behavior before changing or reviewing payments](../src/payments/notes/_notes.md) - #Document - applies to `src/payments/**`
```

The second index carries matching `applyTo` metadata. Both trees then participate directly in context, discovery and reference operations.

A small customized workspace could be:

```text
AGENTS.md
README.md
.agents/
  loader.md
  directives/
    _directives.md
    verification.md
  memory/
    _memory.md
    working/
      _working.md
      payment-task.md
docs/
  _docs.md
  architecture.md
  adr/
    _adr.md
    0001-storage.md
src/
  payments/
    notes/
      _notes.md
      refunds.md
```

README links to `docs/_docs.md` for people. The loader links to the same file for agents. Architecture and payment explanations have one body each.

Enrollment does not make every document a behavioral instruction. An index explicitly identifies defining documents and required instructions. Ordinary links remain references.

Preserve the reasoning in `routing-surfaces.md`: descriptions support selection before bodies are read. Preserve `map-primitive.md`’s separation between pointers and destination knowledge. Change the `.agents/` location boundary, not those distinctions.

Only explicitly enrolled, non-overlapping directory trees participate. Below each starting index, retain direct-child navigation and one recognized index per routed folder. Maps remain useful for external sources and unenrolled documentation. Libraries remain useful for projection, but blending no longer requires them.

#### Cost and migration

**The prompt rewrite is relatively small.** Existing Entries, Axioms, frontmatter, loading tags, conditions and overwrite parsing remain. Reword payload files, update the matching maintenance contracts, and revise startup documentation and snapshots.

**Blending is a substantial CLI change.** Source resolution and inventory must support enrolled trees outside `.agents/`. Context, Find, References, route operations, Index, Status and diagnostics need consistent identities and containment checks. Detect overlapping roots, ambiguous identities and unsafe traversal. This is broader than accepting `../` in a link.

Writing outside `.agents/` must preserve explicit destination permissions. Extend permission handling where Index or route mutations currently lack it. Enrollment grants reading participation, not automatic write or package ownership.

Existing workspaces need no file moves. Keep compatibility index names, frontmatter forms, paths, tags and companions readable. Stop requiring inherited markers without rewriting historical files. Keep composition tags during migration as harmless search labels.

External documentation enrollment is opt-in. Do not automatically mount all of `docs/`, relocate accepted records or convert existing Maps.

Ship CLI support before presenting external enrollment as supported behavior. The Markdown-only simplification can be evaluated independently.

#### Risks and proof

Description matching remains a model decision. A vague description can still hide a required scope. Keeping compact indexes eager limits that risk, but does not eliminate it.

Shorter indexes could omit useful qualifications. Moving authoring detail could make occasional maintenance harder. Documentation enrollment could expose stale or conflicting sources more often. The repository’s loading-reliability decision favors early critical loading, which is why this proposal preserves it.

The cheapest useful experiment is **24 isolated runs**: six fixed fixtures, two loader versions and two repetitions with the same model settings.

Cover mandatory child loading, a new working path introduced mid-task, pending conditions, search with ancestor recovery and overwrites, resume and closeout, and direct documentation enrollment. Include unrelated distractors and a predetermined required-read set.

Score tool-read traces and observable actions, not claims of compliance. Measure missed rules, incorrect application, irrelevant body reads, startup tokens and closeout omissions. Run the prompt comparison with ordinary file tools first, then separately test CLI parity.

Reject the rewrite if mandatory-rule misses increase. Require fewer behavioral failures and substantially lower startup cost before adoption. A clean small experiment supports further testing, not a reliability guarantee.

#### The one idea to steal

Give every important instruction a concrete trigger at the moment the agent must act on it.

## Scout (Proposal B)

#### Thesis

Make Open Forge a portable context catalog built around native instruction files, native Skills and ordinary project documentation. Keep required reads, file conditions and description-based selection, while removing catalog inheritance and category-specific loading ceremonies. Its differentiators should be discoverable context, explainable retrieval and continuity that preserves uncertainty.

#### Diagnosis

1. **Navigation has become a language to learn.**

   `src/open-forge/.agents/loader.md`:

   > Any number of routed slugs may narrow a route below its root.

   Readers must distinguish routes, roots, scopes and slugs before opening useful material. Familiar folder navigation now requires understanding a separate structural grammar.

2. **Binding rules have two representations.**

   `src/open-forge/.agents/loader.md`:

   > Only this loader and recognized loaded entrypoints define active Axioms.

   Directives also contain binding instructions. Models must remember which headings become rules, which files can define them and how their inheritance works. People encounter empty-rule declarations that exist to explain the machinery.

3. **Narrower instructions cannot resolve broader defaults.**

   `src/open-forge/.agents/loader.md`:

   > Adding a child instruction does not cancel an active ancestor rule.

   This differs from the familiar expectation that local instruction files can specialize broader defaults. An ordinary project exception can become a conflict requiring source replacement or an overwrite. Native tools differ, so Open Forge should follow the host’s precedence rather than introduce another hierarchy.

4. **File matching still depends on discretionary discovery.**

   `src/open-forge/.agents/loader.md`:

   > `applyTo` never loads a file by itself.

   A matching rule can remain invisible behind an unselected scope. The field resembles familiar automatic attachment, but its effect depends on another model decision. Mandatory file rules need an uninterrupted required discovery path.

5. **Progressive disclosure stops at the CLI guide.**

   `src/open-forge/.agents/skills/open-forge-cli/SKILL.md`:

   > The Skills entrypoint requires this guide at startup.

   The startup path includes command guidance before anyone knows whether a command is needed. The loader and Skill also repeat parts of the command interface. Native Skill descriptions already provide a cheaper selection surface.

6. **Continuity includes a universal candidate-review obligation.**

   `src/open-forge/.agents/memory/emerging/_emerging.md`:

   > At each applicable #KeepInMind refresh, check visible `Entries` and save useful candidate material before it is lost.

   Resuming a task becomes partly a memory-maintenance exercise. Current working state deserves refresh. An entire candidate catalog does not necessarily deserve repeated attention.

7. **Human documentation participates through an intermediary.**

   `.agents/memory/crystallized/decisions/framework/map-primitive.md`:

   > A Map points to destinations and explains their relevance.

   Maps solve useful navigation problems, but ordinary documentation remains outside the routing model described in the brief. A local architecture document should also be directly selectable and conditionally loadable. A Map should earn its place through curation, rather than provide a compulsory bridge.

The September simplified loader retained most of this machinery. Reducing wording alone cannot remove its learning cost.

#### Concept budget

I count **48 distinct ideas in the shipped startup contract**, counting each field or rule family once, rather than every command or filename alias.

The ecosystem parallels below use the conventions named in the brief. They do not imply identical syntax or precedence across tools.

| Current concepts, numbered | Nearest familiar equivalent | Treatment |
|---|---|---|
| 1. Canonical entry and bridges, 2. loader | `AGENTS.md`, `CLAUDE.md` imports | Keep native entry, merge loader into catalog instructions |
| 3. Entrypoint, 4. entry, 5. route, 6. root route, 7. scope, 8. slug | Linked indexes and nested instruction files | Merge navigation into catalogs and entries. Delete root and slug runtime distinctions. Express applicability through native instructions or file conditions |
| 9. Axiom, 10. inherited marker, 11. Directive, 12. additive rule inheritance | Project rules, Windsurf and Cline instruction files | Merge binding content into rule files. Delete marker and catalog inheritance |
| 13. LoadNow, 14. KeepInMind, 15. on demand, 16. refresh pass | Cursor and Kiro inclusion modes | Keep required and on demand. Merge refresh into task resumption. Delete KeepInMind as a universal mode |
| 17. description, 18. responsibility, 19. tags, 20. applyTo, 21. frontmatter form | Skill descriptions, Copilot file conditions, ordinary metadata | Keep existing fields. Move authoring details into the authoring guide |
| 22. Working files versus context reads, 23. pending conditions | File attachment inputs | Make mechanical in CLI output, retain explicit manual fallback |
| 24. Core, 25. Memory, 26. Extension, 27. managed route | Content collections and optional packages | Move packaging distinctions into installation guidance |
| 28. Contextual, 29. CurrentTruth, 30. Evergreen, 31. acceptance versus validation | Draft and accepted documents, maintained references | Merge into source status and maintenance guidance. Keep acceptance distinct from evidence |
| 32. Guidance, 33. Map, 34. Pattern, 35. Skill, 36. Template | Advice, curated links, examples, native Skills, starter files | Keep familiar artifact purposes. Move category authoring contracts into narrower guides |
| 37. Working, 38. Emerging, 39. Crystallized, 40. Archived, 41. continuity capture and transitions | Memory-bank working notes, drafts, accepted docs and history | Keep optional organization. Merge runtime meaning into working state and source status |
| 42. Overwrite, 43. removal exclusions, 44. optional CLI assistance | Local configuration layers and package maintenance | Keep overwrite behavior. Move exclusions and command details into lifecycle guidance |
| 45. Defining source, 46. relationship links, 47. ancestor-chain recovery, 48. empty Entries marker | Canonical documents, hyperlinks, instruction discovery | Keep defining sources and links. Remove catalog ancestor recovery and empty markers |

The new reader budget is **12 concepts**:

1. Native instruction files
2. Catalog
3. Entry and description
4. Rule file
5. Required read
6. File condition
7. On-demand selection
8. Metadata
9. Source status and acceptance
10. Native Skill
11. Working note and resumption
12. Local overwrite

This counts what a reader must understand to use the system. Optional authoring and installation guides still explain their own operations.

#### The new loader

**445 whitespace-separated words**, including headings and entry lines.

```markdown
# Open Forge

Use this catalogue to find the instructions, knowledge and working notes needed for the task. Files may live in `.agents/`, `docs/` or beside the code.

## Follow instructions

Follow the agent tool's instruction precedence and applicable native instruction files, including nested `AGENTS.md` where supported. Open Forge adds no competing directory precedence.

An entry tagged #Directive identifies a rule file. Follow its instructions when selected and applicable. Other sources explain their own requirements, status and limits. Loading a source does not establish acceptance. Treat proposals and history as context, and investigate conflicting current sources before changing them.

Catalogues provide navigation, not inherited rules. A folder name creates no authority. Read a selected file's explicitly required sources before relying on it.

## Read the required context

1. Start with `Entries` below. Read every #LoadNow destination, in listed order. Repeat for each catalogue opened.
2. A file with `applyTo` applies only when a known or planned working path matches. Skip nonmatching required files. Explicit inspection does not activate their rules. Context-only reads are not working paths. Patterns are case-sensitive and workspace-relative. `*.py` matches root files, `**/*.py` any depth and `docs/**` a folder. A leading `!` is literal. If paths are unknown, report the affected required files as pending and resolve them before the work they govern.
3. Everything else is on demand. Choose from descriptions and links, then read the selected sources. Recheck when the goal or working paths change.
4. Read each source once per pass. Direct links may reach files without intermediate catalogues. When a source has a `{name}.overwrite.md` companion, read it next. Its changes replace corresponding base content only.

Keep file conditions on individual sources. Catalogue files have no `applyTo`. A source has the same meaning whichever link reaches it.

Without the CLI, follow these links directly. With it, `open-forge context --for <path>` batches required context. Use the [CLI Skill](skills/open-forge-cli/SKILL.md) for selected sources and other commands. Native Skills keep their own activation and resource rules.

## Resume and finish

After resuming or restoring context, reread the applicable rules and the current working note for this task. Reread a source when it may have changed.

Before handoff or closeout, update affected current documentation and preserve only the outcome, uncertainty or next step that future work needs. Use existing suitable files. Saving a note does not accept its conclusions.

## Entries

- [Required instructions](directives/_directives.md) - #LoadNow
- [Advice for recurring choices](guidance/_guidance.md) - #Guidance
- [Pointers to useful sources](maps/_maps.md) - #Map
- [Working notes, candidates, accepted knowledge and history](memory/_memory.md) - #Memory
- [Reusable examples and structures](patterns/_patterns.md) - #Pattern
- [Native capabilities selected when useful](skills/_skills.md) - #Skill
- [Starting files to copy and adapt](templates/_templates.md) - #Template
```

#### Other Markdown changes

**Root entry.** `AGENTS.md` says to read `.agents/loader.md` before working. `CLAUDE.md` retains native imports without copying policy. Host-specific bridges remain small.

**Category entrypoints.** Each contains its purpose and Entries. Remove inherited markers and authoring rules. Preserve genuinely binding content by moving it into explicit rule files before retiring catalog Axioms. Detailed instructions for writing patterns, templates or records become on-demand authoring guidance.

A populated Directives catalog would be:

```markdown
---
description: Required workspace rules
tags: [LoadNow]
---

# Directives

These files define required behavior.
Required entries load when their file conditions match.
Other entries are selected by description.

## Entries

- [Rules for every task](workspace.md) - #LoadNow #Directive
- [C# conventions](../../docs/contributing/csharp.md) - #LoadNow #Directive - applies to `**/*.cs`
```

A fresh installation has an empty Entries section. It needs no empty marker.

**Memory.** Keep the existing four folders for compatibility, but make all state catalogs on demand. Explain them as working notes, candidates, accepted knowledge and history. No required progression or universal Emerging refresh remains. Refresh the working note selected for the task.

**Tags.** Retain `#LoadNow` and the existing `#Directive` role marker. Category and topical tags remain searchable labels. Move status-tag explanations into the relevant record guidance. Retire `#KeepInMind` through reviewed migration. Replace Evergreen’s special startup semantics with source-specific maintenance instructions and the loader’s closeout rule.

**Frontmatter.** Preserve existing field names and both supported forms. Descriptions remain the selection surface, as the routing-surfaces decision recommends. Human documents may remain unannotated on demand. Automatic loading or filtering requires explicit metadata. Conditions belong to individual sources, rather than catalogs.

**CLI Skill.** Remove compulsory startup reading. Keep a short task-to-command table and progressively disclosed references. Preserve native `name` and `description`.

#### Decisions: model vs mechanism

| Decision today | Proposed responsibility |
|---|---|
| Which root categories to open | Required tags determine baseline. Model selects optional catalogs |
| Whether a mandatory rule seems relevant | Remove that judgment after required selection |
| Whether paths match `applyTo` | CLI evaluates known paths. Agent performs the same check without CLI |
| Whether a hidden scope contains matching rules | Authoring mechanism requires mandatory file rules to have a required discovery chain |
| Which advice, example or document helps | Model selects from descriptions |
| Which ancestors supply Axioms | Remove catalog inheritance. Host handles native instruction precedence |
| Whether a document is accepted | Source and user establish status. Model checks evidence and limits |
| Whether a Skill should activate | Native runtime and Skill description |
| What must survive resumption | Model selects a useful working note |
| When selected rules and working state refresh | Fixed resumption boundary, plus possible source changes |
| Which files an updater may change | Existing ownership and exclusion mechanisms |
| Whether behavior was followed | Evidence and proportionate review |

The CLI follows declared structure. It still cannot infer every semantic dependency expressed in prose or guarantee compliance.

#### Customization, independence and blending

Keep every default removable. A manual installation removes its link and files. A managed installation records exclusions through the existing lifecycle commands.

Users can replace a source directly or retain a small adjustment in its existing overwrite companion. An overwrite changes its base source only. It does not outrank unrelated instructions or the host’s hierarchy.

Add a document by linking directly to it. Add a catalog when grouping earns its navigation cost. Intermediate directories need no entrypoints. Catalogs may form a graph, with duplicate reads suppressed. Local destinations must remain inside the workspace. URLs remain ordinary links.

For example, add this loader entry alongside the seven defaults:

```markdown
- [System architecture and accepted decisions](../docs/_docs.md) - #Reference
```

A small workspace could look like:

```text
AGENTS.md
CLAUDE.md
.agents/
  loader.md
  directives/
    _directives.md
    workspace.md
  memory/
    _memory.md
    working/
      checkout.md
docs/
  _docs.md
  architecture.md
  adr/
    0001-storage.md
  contributing/
    csharp.md
src/
  payments/
    AGENTS.md
    design.md
```

`docs/architecture.md` serves the same human and agent readers. Its status and requirements remain in that file. Maps remain useful for curated collections and external systems. Libraries remain useful for shared-folder projections. Neither is required to route an ordinary local document.

The six categories become optional organizational choices. An individual rule, native Skill, template or documentation catalog remains useful independently.

Three differentiators justify their learning cost:

- **Portable descriptive catalogs** expose useful context across tools and project folders.
- **Explainable retrieval** shows selected, skipped and pending sources without hiding meaning in a service.
- **Continuity with explicit acceptance** preserves useful uncertainty without turning accumulated notes into instructions.

These preserve the accepted principles. They deliberately change the implementation decisions about routing authority, Memory placement and loading.

#### Cost and migration

This is a **major contract migration with moderate-to-large CLI work**, rather than a prose-only update.

Retain the Entries grammar, existing frontmatter fields, overwrite naming and recognized entrypoint filenames. No new metadata field is required.

Change the C# routing model to admit workspace-local leaves and catalogs outside `.agents/`, including direct destinations without intermediate entrypoints. Extend source resolution, Context, Find, References and Index accordingly. Detect cycles and deduplicate reads. Read access to documents must not expand installer write ownership.

Remove catalog-Axiom inheritance and reject catalog `applyTo` in the new contract. Existing ancestor conditions cannot always be flattened into equivalent glob lists. Migration must report those cases for review.

For unannotated documents, Index derives descriptions from headings and emits a descriptive `#Reference` label without adding destination metadata. Existing document frontmatter remains untouched unless an explicit authoring operation requests changes.

Migration should:

1. Inventory active Axioms, conditions, refresh obligations and overwrites.
2. Separate behavioral rules from authoring guidance.
3. Preserve mandatory rule discovery through required catalog chains.
4. Review inherited-condition conversions and working-note refresh needs.
5. Generate a proposed patch with unresolved cases listed.
6. Retain old-contract support until the workspace explicitly migrates.

Update parser and command tests, payload snapshots, installation fixtures, Extensions, maintenance contracts and concept documentation. Test read-only access to ordinary docs separately from mutation permissions.

Fresh installs can use the new payload directly. Existing workspaces need a versioned migration. No files or tests were changed or run for this proposal.

#### Risks and proof

Removing inheritance can weaken an obligation if migration misclassifies it. Required-chain checks and explicit unresolved cases reduce that risk.

Optional catalogs may be overlooked. That tradeoff is deliberate for advice and history. Mandatory rules remain on required paths. The loading-reliability decision’s reported success with unconditional root directives argues strongly for preserving that baseline.

Native hosts differ in instruction precedence. Bridges need focused compatibility checks, without claiming one universal override model.

A shorter startup may also produce worse behavior. Word count alone cannot establish improvement.

The cheapest useful experiment is a small paired evaluation of the current and proposed Markdown:

- A file rule below a formerly discretionary scope
- Unknown paths that later become known
- A task crossing into another language
- A directly linked human document
- A draft conflicting with accepted knowledge
- A native instruction exception
- A task requiring one Skill
- Resumption from a working note

Use identical tasks and inspect actual file-read traces, missed obligations, inappropriate rule application, clarification count and context size. Repeat failures before attributing them to the design.

Aim for shipped startup below 1,000 words, while preserving mandatory-rule discovery and meeting or improving observed compliance. The reduction is an estimate until measured.

#### The one idea to steal

Make catalogs describe where to read, while native instructions and explicitly selected sources define what to follow.

## Wildcard Inventor (Proposal C)

#### 1. Thesis

Replace inherited routing with one workspace-owned reading table that connects explicit conditions directly to required sources. Keep category trees as optional catalogues, with documents anywhere in the workspace serving people and agents directly. This challenges the strict scaling interpretation of Context By Relevance because every task scans the required-reading table, while document bodies remain selective.

#### 2. Diagnosis

Quotations below come from `src/open-forge/.agents/loader.md` unless another path is named.

1. **Applicable rules can remain undiscovered.**

   > These tags act only through loaded parents. Neither activates an unselected ancestor or scope.

   > `applyTo` never loads a file by itself.

   A matching C# file does not reveal its applicable rules. The model must first discover and select the correct branch. This puts the most consequential omission risk before deterministic assistance can help.

2. **Folder placement carries too much meaning.**

   > Any number of routed slugs may narrow a route below its root.

   People must understand how every intermediate folder changes applicability. Moving a document can change its meaning even when its text stays identical. Models must reconstruct that meaning across several files.

3. **Required instructions have several representations.**

   > Only this loader and recognized loaded entrypoints define active Axioms.

   Directives use another heading and inherit surrounding Axioms. A person must distinguish instruction location, source role, inherited scope and loading status before understanding an obligation.

4. **Tags operate on unrelated axes.**

   > Defined tags keep the meanings below wherever they appear.

   The eight definitions mix reading schedules, package composition, acceptance status and maintenance duties. They are individually defensible, but collectively turn a navigation system into a small policy language.

5. **Startup teaches authorship before task execution needs it.**

   > Routed Markdown files start with YAML frontmatter.

   The loader then explains metadata forms, duplicate declarations, glob behavior and generated entries. Index can assist authors, but every reader still receives the authoring manual.

6. **Required reading has two implementations.**

   > When `open-forge-cli/SKILL.md` is present, read it when this entrypoint loads so command options and context-loading advice are available from the start.

   Source: `src/open-forge/.agents/skills/_skills.md`.

   This requirement lives in prose rather than loading tags. As `references/discovery.md` explains, Context follows tags while explicit prose reads remain the agent’s responsibility. Startup consequently requires both mechanisms.

7. **Ordinary documentation participates indirectly.**

   > A source reference is an exact source ID, such as `memory/crystallized`, or an exact `.agents/...` Markdown path.

   Source: `src/open-forge/.agents/skills/open-forge-cli/references/common.md`.

   Maps preserve a useful navigation boundary, and Libraries provide projections. Neither makes the original `docs/contributing.md` an ordinary first-class context source. Contributors and agents should read the same file at its existing path.

**Design search**

Scores are design judgments from 1 to 5, not measured results.

| Reinvention | Smaller | Easier | Rule following | Customizable | Independent |
|---|---:|---:|---:|---:|---:|
| Directory rules, with each source directory’s README defining local behavior | 5 | 4 | 4 | 3 | 5 |
| Trigger sentences only, written as “When X, do Y” | 4 | 5 | 3 | 5 | 5 |
| Frontmatter envelopes declaring every condition and dependency | 5 | 3 | 5 | 4 | 3 |
| CLI compiles a complete task packet from registered sources | 5 | 4 | 5 | 5 | 2 |
| Every rule carries a required observable self-check | 2 | 3 | 4 | 4 | 5 |
| Explicit required-reading table plus optional catalogues | 5 | 5 | 4 | 5 | 5 |

The table wins because it exposes decisions without requiring a compiler or directory semantics.

**Runner-up appendix:** A compiled task packet would make an excellent optional view of the reading table. It should contain source paths and matching reasons, remain disposable, and never become another maintained authority.

#### 3. Concept budget

This counts named operating concepts and metadata mechanisms, excluding ordinary Markdown syntax and individual CLI commands.

**Today: 50.**

| Numbers | Concepts |
|---|---|
| 1–10 | Loader, entrypoint, entry, Axiom, route, root route, scope, slug, managed route, ancestor inheritance |
| 11–20 | Inherited marker, overwrite companion, on-demand selection, parent-gated loading, refresh boundaries, working files, pending conditions, inherited glob intersection, frontmatter, root/scoped forms |
| 21–30 | Description, responsibility, tags field, applyTo, LoadNow, KeepInMind, Core, Memory tag, Extension, Contextual |
| 31–40 | CurrentTruth, Evergreen, acceptance, validation, declared authority, removal exclusions, Directives, Guidance, Maps, Patterns |
| 41–50 | Skills, Templates, Memory role, Working, Emerging, Crystallized, Archived, native runtime boundary, ordinary source links, generated Entries synchronization |

**New universal contract: 18.**

Loader, reading table, row, `always`, `files`, `task`, `instructions`, `reference`, `state`, working paths, globs, pending conditions, catalogue, refresh, declared authority, acceptance, validation and links.

**Another 18 survive only in optional feature or authoring material.**

The six categories, Memory and its four states, frontmatter, root/scoped forms, description, responsibility, descriptive tags, the native runtime boundary and removal exclusions.

That leaves **36 named concepts overall, with 18 required to operate the system**. Moving concepts out of startup is an attention reduction, not a claim that their functionality disappeared.

| Former concept | Disposition |
|---|---|
| Entrypoint, entry, route and root route | Merged into ordinary linked catalogues |
| Axioms and binding Directive behavior | Merged into explicit `instructions` use |
| Scope and inherited file conditions | Made explicit in each reading row |
| Slug semantics, ancestor inheritance and inherited marker | Deleted |
| Parent-gated loading | Deleted |
| LoadNow and KeepInMind | Replaced by matching rows and `state` refresh |
| Overwrite companions | Deleted from the new model, replaced by explicit source substitution |
| Composition and status tags | Ordinary search labels, with meaning stated in documents |
| Evergreen | Merged into updating affected defining sources |
| Managed-route interpretation | Moved to CLI lifecycle documentation |
| Generated Entries synchronization | Made mechanical through Index |

#### 4. The new loader

**476 words**, counting whitespace-separated tokens, including headings and Entries.

```markdown
# Open Forge

Use the reading table to load required context. Use the catalogues below to find anything else.

## Start and rescope

1. Read [the reading table](reads.md) at task start, resume, and after context restoration.
2. Check every row against the task and its working paths. Working paths include files being investigated, reviewed, changed, or planned. Reading context alone does not add a working path.
3. Read every matching source before the affected work. Recheck the table when the goal or working paths change.

An empty table adds no required reading. A missing table, invalid row, or unreadable matching source leaves required context unresolved. Report the problem before proceeding with affected work.

## Reading table

The table has three columns: `When`, `Read`, and `Use`.

`When` is one of:

- `always`
- `files:` followed by comma-separated globs
- `task:` followed by a short description of the task

File conditions match any working path. Use case-sensitive paths from the workspace root. `*` matches within a path segment and `**` spans segments. Patterns in one row are alternatives. Unknown working paths leave file rows pending. Resolve them before affected work. Judge task descriptions against the objective and include plausible matches.

`Read` links directly to one local Markdown source. Resolve links relative to the table. Sources may live anywhere inside the workspace. Read duplicate destinations once per pass.

`Use` is one of:

- `instructions`: follow the source's requirements for the matching work
- `reference`: use its subject knowledge with its stated authority and acceptance status
- `state`: use its temporary context and reread it before handoff or closeout

Read each source's explicit required links. An ordinary link, filename, folder, or tag adds no required reading or inherited rules.

## Apply and finish

Follow platform constraints and clear user direction. Continue routine work within the accepted request. Report unresolved instruction conflicts rather than deciding by folder depth or reading order.

Evidence supports claims. Acceptance establishes current project decisions. Reading, moving, or tagging a document establishes neither. Keep uncertainty visible and use declared authorities for their assigned facts.

When accepted changes affect current documentation, update its defining source within the authorized work. Before finishing, preserve only useful continuation state and durable outcomes, and report blocked updates.

## Optional discovery

Entries are navigation. Open relevant catalogues and follow useful links. Their descriptions help selection. Consult a native Skill only when needed and follow the runtime's Skill rules.

The optional CLI can assist reading and navigation. Use `open-forge --help` or the [CLI Skill](skills/open-forge-cli/SKILL.md) when using it. If the CLI does not report reading-table matches, read the files directly.

## Entries

- [Required behavior](directives/_directives.md) - #Directive
- [Advice and tradeoffs](guidance/_guidance.md) - #Guidance
- [Pointers to useful sources](maps/_maps.md) - #Map
- [Saved work, knowledge, candidates, and history](memory/_memory.md) - #Memory
- [Reusable shapes](patterns/_patterns.md) - #Pattern
- [Native capabilities](skills/_skills.md) - #Skill
- [Copy-ready starting files](templates/_templates.md) - #Template
```

#### 5. Other Markdown changes

**AGENTS.md** becomes:

```markdown
# Open Forge

Before acting on a task, read `.agents/loader.md` and follow its reading table.

**End of Open Forge managed section.**
```

CLAUDE.md keeps its native references to AGENTS.md and the loader.

A new installation creates this user-owned file once:

```markdown
# Required reading

## Reads

| When | Read | Use |
|---|---|---|
```

Only rows beneath `## Reads` participate. Each row has one condition, one local Markdown link and one allowed use. Multiple matching rows combine their applicable uses. Reading order establishes no precedence.

**Category entrypoints** become short optional catalogues. They contain a definition and Entries. Binding rules move into explicitly listed instruction sources. Authoring conventions move into optional templates and guides.

A complete shipped example:

```markdown
---
open-forge:
  description: Advice for recurring choices and tradeoffs
  tags: [Guidance]
---

# Guidance

Guidance explains a situation, a recommended approach, its reasons, and its tradeoffs.

Choose advice that fits the work. Adapt it when another approach fits better.

## Entries

- none - No entries - #Empty
```

**Memory states** retain their useful distinctions, with short definitions and optional Entries. Working records state their expiration. Emerging records expose uncertainty. Crystallized records identify acceptance. Archived records identify their historical status. No state entrypoint loads automatically. An active continuation record receives a matching `state` row.

**Tags** become descriptive search labels. None controls reading, acceptance or maintenance.

**Frontmatter** becomes optional for participation. Existing root and scoped forms remain supported for descriptions, responsibility and tags. New reading conditions live only in the table. Legacy Open Forge `applyTo` declarations require migration rather than silent reinterpretation.

**The CLI Skill** becomes entirely on demand. Its first example shows Context resolving the reading table. Route maintenance and package operations remain in their existing focused references.

#### 6. Decisions: model vs mechanism

| Decision today | Proposed responsibility |
|---|---|
| Discover branches containing mandatory rules | Mechanical scan of one reading table |
| Decide whether a file condition matches | Mechanical glob matching, manually or through CLI |
| Interpret ancestor scope and condition intersections | Removed from runtime |
| Match a task description to an objective | Model, with plausible matches included |
| Decide whether a required source seems useful | Removed after its row matches |
| Distinguish instructions from supporting knowledge | Explicit `Use` value |
| Infer authority from folder role | Removed |
| Resolve conflicting instructions | Model investigates and reports unresolved conflicts |
| Follow explicit required reads inside source prose | Model, with no claim of CLI enforcement |
| Select optional knowledge or capabilities | Model through descriptions and links |
| Refresh continuation state | Mechanical boundary attached to `state` |
| Decide what deserves durable retention | Model under workspace instructions |
| Maintain generated catalogue entries | Optional Index mechanism |
| Determine package ownership and exclusions | CLI lifecycle mechanism |

#### 7. Customization, independence and blending

A user removes required behavior by removing its row. Replacing or overriding behavior means changing that row’s destination to the chosen complete source. Adding another conflicting row does not create an override.

The table is user-owned. Updates may propose changes but never regenerate or replace it. Managed-file removals still use existing exclusions. Without the CLI, users edit links and files directly.

A document can appear in several catalogues without gaining extra authority. Catalogues can be removed independently. A Skill remains a native package. A template remains a copyable file. Memory remains optional saved Markdown.

This retains the selection-surface principle from `routing-surfaces.md`. Descriptions precede bodies. It changes the Map boundary deliberately: Maps remain useful curated pointers, but cease to be the admission path for ordinary documentation. Libraries remain useful for genuine sharing and projection needs.

A small workspace could contain the following. Unused catalogue links are removed from its loader.

```text
AGENTS.md
.agents/
  loader.md
  reads.md
  directives/_directives.md
  guidance/_guidance.md
  skills/open-forge-cli/SKILL.md
docs/
  README.md
  contributing.md
  api.md
  decisions/0001-checkout.md
  work/checkout.md
src/
  checkout/
```

Its `.agents/reads.md` could be:

```markdown
# Required reading

## Reads

| When | Read | Use |
|---|---|---|
| always | [Contributor rules](../docs/contributing.md) | instructions |
| files: src/checkout/** | [Checkout contract](../docs/api.md) | reference |
| task: changing checkout behavior | [Accepted checkout decision](../docs/decisions/0001-checkout.md) | reference |
| task: continuing checkout work | [Checkout checkpoint](../docs/work/checkout.md) | state |
```

The contributor rules and checkout contract are the documents people already maintain. Neither needs an `.agents/` copy, wrapper entrypoint or added frontmatter.

#### 8. Cost and migration

This is a **medium-to-large versioned change**, not a loader-only rewrite.

- **Parser:** Add the three-column reading-table grammar. Reuse the current glob matcher. Validate local containment, destinations and allowed values. Preserve existing Entries parsing for catalogues.
- **Context:** Resolve `always` and file rows directly. Return task rows as explicit decisions needing model judgment. Explain matching reasons. Do not claim semantic completeness.
- **Navigation:** Accept explicit workspace Markdown paths outside `.agents/`. Drop the requirement for an entrypoint in every intermediate directory. Existing filenames and IDs need compatibility handling.
- **Index:** Retain generated descriptions and navigation. Never infer required-reading rows from filenames, tags or folder membership.
- **Lifecycle:** Keep ownership and exclusions. Treat the reading table as user-owned. Existing Library machinery does not need replacement.
- **Tests:** Update Context, route inspection, indexing, source identity, path containment, package integration and payload snapshots. Verify that unregistered workspace files remain untouched.
- **Documentation:** Rewrite the routing, scope, loading, customization and startup explanations. Update extensions that currently depend on inherited Axioms.

Migration should first produce a proposed table and explain each row’s origin. Global rules become `always` rows. Scoped requirements need explicit file or task conditions.

Ancestor conjunctions cannot simply become alternative globs. Where equivalent conditions cannot be expressed confidently, migration stops for a reviewed rewrite. Overwrite prose also needs review before producing a complete replacement source.

Keep the old format intact until the converted workspace is accepted. Reject mixed loading models. Preserve metadata belonging to other tools.

#### 9. Risks and proof

The main risks are a long global table, stale registrations, broad task descriptions, and loss of useful inheritance during migration. Repeated `always` sources could recreate today’s startup cost.

The cheapest experiment needs no CLI changes. Prepare equivalent baseline and proposed fixtures, then run twelve paired scenarios covering known paths, unknown paths, two languages, conceptual work, resume, changed scope, handoff, missing sources and direct documentation access.

Record:

- Required sources actually read before affected work
- Correct handling of instruction conflicts and candidate knowledge
- Unnecessary source bodies read
- Startup tokens and routing turns
- Whether readers can explain applicability after one reading

Require no missed critical rule in the fixture set, fewer routing errors, and a substantial startup reduction. A comprehension-only win is insufficient because the current loading-reliability decision records better compliance from early required reading.

If the flat table becomes expensive in a large fixture, reject the representation or add an independently evaluated partitioning design. Do not conceal that failure behind shorter loader prose.

No files were changed, and no builds, tests or Doctor commands were run.

#### 10. The one idea to steal

Make required context an explicit reading list that links directly to the same documents people maintain, wherever those documents live.

## Tinkerer (Proposal A)

#### Thesis

Make Open Forge a small reading protocol with replaceable Markdown modules. Keep required reading explicit, make local replacements checkable, and let routed documentation live anywhere inside the workspace. A first safe customization should require understanding only an entrypoint, its links, and `#LoadNow`.

#### Diagnosis

These observations concern the shipped payload at `e9d4fc9a7`.

1. **Routing vocabulary exceeds the decisions readers need to make.**

   `src/open-forge/.agents/loader.md`:

   > Any number of routed slugs may narrow a route below its root.

   A person organizing two folders must first distinguish routes, roots, scopes and slugs. A model must reconstruct their relationships. Keep recursive navigation and inherited rules, but remove the separate vocabulary for ordinary path segments.

2. **Startup loads instructions about capabilities before those capabilities are needed.**

   `src/open-forge/.agents/skills/_skills.md`:

   > When `open-forge-cli/SKILL.md` is present, read it when this entrypoint loads so command options and context-loading advice are available from the start.

   Presence becomes a reason to consume context. This also creates a required read outside the CLI’s tag-derived startup closure. Load the CLI guide when using the CLI.

3. **Removal requires understanding a second representation of intent.**

   `src/open-forge/.agents/loader.md`:

   > For managed files, record intentional removals in the manager's exclusions before updating.

   Deleting unwanted categories is supported, but deletion alone is insufficient for managed updates. The person must understand navigation, ownership and exclusions. Keep persistent exclusions, but explain them only in the removal instructions.

4. **Memory customization changes a shared framework model.**

   `src/open-forge/.agents/memory/_memory.md`:

   > Get explicit user direction before creating a new top-level Memory state.

   Replacing four states with `notes/` and `decisions/` requires understanding which folders are states and which are scopes. Make state names an optional organization recipe. Preserve acceptance distinctions in record text.

5. **Small overrides depend on semantic matching.**

   `src/open-forge/.agents/loader.md`:

   > Where they answer the same question differently, the overwrite wins only for the corresponding content.

   “Same question” and “corresponding content” ask the model to perform a merge. Neither the author nor updater can reliably detect when an upstream revision invalidates an exception. Require an exact target and expected text.

6. **Custom tags have a fixed loading ceiling.**

   `src/open-forge/.agents/loader.md`:

   > Other tags remain search and routing signals.

   A personal loading vocabulary needs additional prose that `context` does not interpret. Support explicit expansions into the standard loading operations, while making genuinely new timing behavior an optional capability.

7. **Ordinary documentation enters through a pointer category.**

   `src/open-forge/.agents/maps/_maps.md`:

   > Each Map route links to one or more destinations and says what they contain and when to use them.

   Maps correctly preserve destination authority. The unnecessary restriction is requiring Maps or Library projections to connect ordinary documentation to routing. Admit `docs/` directly, while keeping Maps useful for curated navigation.

8. **Portability depends on framework metadata and inherited meaning.**

   `src/open-forge/.agents/loader.md`:

   > Routed Markdown files start with YAML frontmatter.

   A useful document needs extra preparation to participate, while a copied rule can lose meaning supplied by its parent category. Make metadata optional for ordinary documents and make each shipped module state its purpose and applicability locally.

#### Concept budget

I count distinct taught concepts, rather than every sentence, command or syntax example.

| Current concepts | Count |
|---|---:|
| Entrypoint, entry, axiom, route, root route, scope, slug, managed route, overwrite companion, ancestor inheritance, inherited marker, management versus authority | 12 |
| On-demand loading, parent activation, refresh pass | 3 |
| LoadNow, KeepInMind, Core, Memory, Extension, Contextual, CurrentTruth, Evergreen | 8 |
| Frontmatter forms, description, responsibility, tags, applyTo, working files | 6 |
| Directives, Guidance, Maps, Patterns, Skills, Templates | 6 |
| Working, Emerging, Crystallized, Archived | 4 |
| Validation, acceptance, declared authority | 3 |
| Source IDs, generated navigation, exclusions, update ownership, Index versus Repair, durable capture | 6 |
| **Total** | **48** |

The replacement loader teaches **16 concepts**: entrypoint, Entries, description, descriptive tags, on-demand loading, parent activation, LoadNow, KeepInMind, refresh pass, inherited scope, applyTo, working files, source authority, acceptance, overwrite companion and checked replacement.

The full design has **20 additional optional concepts**, introduced where used: seven content modules including Memory, four stock Memory states, frontmatter forms, responsibility, source IDs, generated navigation, exclusions, update ownership, Index versus Repair, document-maintenance conventions and tag expansion. That makes **36 overall**, rather than pretending deferred complexity disappeared.

The dispositions are:

- **Merged:** route, root route and slug become ordinary navigation through entrypoints and paths. Axiom vocabulary becomes inherited rules. Validation remains distinct from acceptance within one short explanation.
- **Deleted:** the inherited marker, special runtime meaning for familiar folder names, and manager-specific segment ordering from the reading protocol.
- **Moved to narrower files:** package management, exclusions, ownership, metadata authoring, CLI maintenance, category authoring and Memory lifecycle recipes.
- **Merged into descriptive tags:** Core and Extension.
- **Moved into explicit record text:** Contextual and CurrentTruth. Their tags may remain searchable.
- **Moved into optional maintenance instructions:** Evergreen.
- **Merged into Memory:** durable capture.
- **Made mechanical:** exact replacement checks and optional tag expansion.

#### The new loader

**478 words**, counting whitespace-separated words, including headings and Entries.

```markdown
# Open Forge Loader

Use this file to find the instructions and context needed for the task.

## Axioms

### Read

An entrypoint is a folder's navigation file, normally `_{folder-name}.md`. Its `Entries` describe linked files and folders. Follow relative links from the file containing them. Links may reach any ordinary folder inside this workspace, including `docs/`.

Read relevant entrypoints before their children. Read their required entries before acting. Everything else is on demand. Use descriptions and task paths to choose branches, then repeat when the task changes. Folder names and descriptive tags give no special authority.

- `#LoadNow` means read the linked source when its parent is read.
- `#KeepInMind` also means reread it at task start or resume, after context restoration, and before handoff or closeout while its scope remains active.

These tags never activate an unselected parent. Follow required child entries recursively, in listed order. Read each source once per pass. Search results need their applicable ancestor entrypoints too.

### Apply

Follow `Axioms` in selected entrypoints and `Instructions` in files described as project rules, within their stated scope. Loaded ancestor rules remain applicable. A narrower rule adds requirements rather than cancelling another source.

An `applies to` condition, mirrored from optional `applyTo` frontmatter, filters a source rather than selecting it. Patterns within one condition are alternatives. One working file must satisfy every condition along the selected chain. Working files include investigated, reviewed and planned paths, not files read only for context. If paths are unknown, report required conditioned entries as pending before relying on them.

Globs are case-sensitive and workspace-relative. `*.py` matches root files, `**/*.py` any depth, and `docs/**` that subtree. A leading `!` is literal.

Respect platform constraints and clear user direction. Use sources for the questions they define. Evidence can support a claim without establishing acceptance. A tag, folder or move never establishes acceptance. Report unresolved conflicts instead of choosing by depth or reading order.

### Customize

Each listed category is optional. Remove unused entries and folders, or replace their links. Keep managed removal and update records consistent if using the CLI.

Read an adjacent `{name}.overwrite.md` after its base. Additions supplement that source. A replacement names a base heading, quotes the exact old text and supplies replacement text. Replace only a unique match within that heading. A missing or ambiguous match blocks use of the affected source until resolved. This changes only the addressed base content.

The CLI is optional. `open-forge context` can batch these reads. Read the CLI Skill when using the tool, or maintain the links and files directly.

## Entries

- [Required project instructions](directives/_directives.md) - #LoadNow
- [Advice for recurring choices](guidance/_guidance.md) - #Guidance
- [Pointers to useful sources](maps/_maps.md) - #Map
- [Notes worth carrying between tasks](memory/_memory.md) - #LoadNow
- [Reusable shapes and examples](patterns/_patterns.md) - #Pattern
- [Native capabilities and their instructions](skills/_skills.md) - #Skill
- [Starting files to copy and adapt](templates/_templates.md) - #Template
```

#### Other Markdown changes

**AGENTS.md** keeps its managed boundaries, with this handoff:

> Read `.agents/loader.md` before work and follow its required links.

**CLAUDE.md** retains its native references.

**Category entrypoints** become short, independently understandable introductions and local rules. Authoring specifications move to optional templates or authoring guides. Guidance, Maps, Patterns, Skills and Templates stop loading at startup.

**Memory** starts without a required state taxonomy. The four existing states remain available as an optional recipe. Existing installations retain their records and layout.

A complete replacement `.agents/memory/_memory.md`:

```markdown
---
description: Keep useful outcomes and enough state to resume
tags: [LoadNow, Memory]
---

# Memory

Keep useful outcomes and enough state to resume work.
The rules below apply whenever this file is adopted.
Records may live directly here or in folders you choose.

## Axioms

### capture

Save durable outcomes before closing a task.

### acceptance

State whether a record is accepted, proposed or historical.
Identify its source when trust depends on it.
Recording or moving material does not establish acceptance.
Update the document that defines an accepted result instead
of leaving the change only in a note.

## Entries

- none - No entries - #Empty
```

**Tags** retain two universal loading operations. Other tags describe content unless an optional expansion emits standard loading tags.

**Frontmatter** retains existing keys and both supported forms. For a document without frontmatter, its parent entry supplies its description, tags and conditions. Index preserves those authored values. It does not insert metadata into human documentation without an explicit request.

**The CLI Skill** becomes a short command-selection guide loaded when needed. Installation, removal and recovery remain in its references. Required reads in first-party content must be represented by loading entries, eliminating the startup exception hidden in Skills prose.

#### Decisions: model vs mechanism

| Decision today | Proposed responsibility |
|---|---|
| Which subject branches matter | Model, using descriptions and task intent |
| Whether a required child seems relevant enough | Removed |
| Which required files follow selected parents | Mechanical closure of loading tags |
| Whether file conditions match | Mechanical glob evaluation |
| Which paths the task investigates or changes | Model, made explicit as working paths |
| Which ancestor rules apply | Mechanical route traversal |
| Whether a child silently overrides another source | Removed |
| Which base sentence an overwrite replaces | Mechanical heading and exact-text check |
| Whether evidence establishes accepted direction | Model and the declared decision authority |
| Which Memory state a record must enter | Removed from the kernel |
| Whether local edits survive updates | Mechanical ownership and divergence checks |
| Whether documentation outside `.agents/` participates | Explicit links, resolved mechanically |
| Whether knowledge deserves saving | Model, guided by the selected Memory module |

#### Customization, independence and blending

| Customization | Concrete behavior |
|---|---|
| Keep only Directives and Memory | Remove the other root entries and categories. Managed removal records persistent exclusions |
| Replace four Memory states with two | Replace the Memory index’s entries with Notes and Decisions. No reserved state names or state whitelist |
| Change one shipped sentence | Add a checked replacement companion. The base continues receiving compatible updates |
| Add a loading tag | Define an explicit expansion in a selected entrypoint, such as `#BeforeReview` to `#LoadNow` |
| Copy one shipped piece elsewhere | Its opening explains its purpose and applicability. Supporting files are linked explicitly |
| Operate without the CLI | Follow links, maintain Entries and compare replacement text manually |

For the Memory example, `_memory.overwrite.md` could contain:

```markdown
# Local Memory changes

## Replace capture

### Expected

> Save durable outcomes before closing a task.

### With

Save durable outcomes when a task pauses or closes.
```

This changes one sentence. Update checks the expected text against the candidate upstream file before replacing the installed base. A mismatch preserves the installed version and reports the conflict. Context reports an invalid installed replacement as incomplete.

Additions use `## Additions`. Replacement companions cannot change unrelated sources, metadata or navigation. Those changes belong in the relevant source.

Optional tag expansion uses a small table:

```markdown
## Tag aliases

| Tag | Expansion |
|---|---|
| #BeforeReview | #LoadNow |
```

Index materializes both tags in Entries, so manual readers see the loading operation. A genuinely new event, such as a release-specific checkpoint, belongs in a selected Skill with explicit steps. The base CLI does not infer a scheduler from arbitrary tag prose.

A small blended workspace could be:

```text
AGENTS.md
README.md
.agents/
  loader.md
  directives/
    _directives.md
    contribution-rules.md
docs/
  _docs.md
  architecture.md
  notes/
    _notes.md
    current-work.md
  decisions/
    _decisions.md
    storage-choice.md
```

Here the loader’s Memory entry targets `../docs/_docs.md`. That index offers Notes and Decisions, alongside ordinary documentation. README links to the same files. There are no copies or projected links.

Entrypoint filenames and Entries syntax remain consistent across folders. Registered roots cannot overlap. Cross-root references retain the destination’s own ancestor chain rather than transferring the referring source’s rules.

Maps remain optional curated pointers, especially for external sources. Libraries remain optional sharing machinery.

For removal without the CLI, delete the entry and unwanted files. Before subsequently using managed updates, record the corresponding exclusions. Deleting files does not silently edit package ownership.

Updates preserve divergent user edits and offer a proposed change. They no longer replace an edited managed file merely because recovery bytes exist.

#### Cost and migration

This preserves the accepted identity principles. It deliberately revises routing boundaries, category-based instruction identification, overwrite behavior and update reconciliation.

| Area | Cost |
|---|---|
| Loader and entrypoints | Small authored rewrite, followed by comprehension checks |
| Entries and frontmatter grammar | Existing syntax retained |
| Headerless documents | Medium change to metadata fallback and Index preservation |
| Roots outside `.agents/` | Large change across source identity, traversal, references, indexing and mutation boundaries |
| Checked replacements | Medium new companion parser, validation and effective-content rendering |
| Tag expansion | Small optional parser and generation feature |
| Update behavior | Medium to large change to divergence handling and proposed-change reporting |
| Tests and snapshots | Broad updates across context, routes, Index, references and lifecycle behavior |
| Documentation | Rewrite conceptual introductions and customization examples, retain detailed references |

Routing a folder does not grant permission to write there. Existing destination permissions remain applicable.

Migration must be explicit and previewable. Preserve current layouts and exclusions. Convert only unedited defaults automatically. Convert legacy overwrite prose only when its target is unambiguous, otherwise request an authored replacement. Do not move Memory records or delete state folders automatically.

The complete design is a substantial CLI release, not a Markdown-only patch. Ship the shorter startup and optional modules first. Introduce checked replacements and blended roots as separately verified capabilities.

#### Risks and proof

On-demand catalogues may hide useful context. The loading-reliability decision reports strong results for early root instructions, so Directives remain mandatory. Important specialized rules still need selected parents and explicit loading tags.

Checked replacements can become stale. That produces a visible interruption instead of an inferred merge. Unchanged quoted text also cannot prove that surrounding upstream changes are harmless, so update diffs still matter.

Headerless documents make parent entries responsible for metadata. Multiple competing routing parents must therefore be rejected or resolved explicitly.

The cheapest experiment needs no production parser changes. Prepare current and proposed fixture workspaces and give independent sessions the six customization tasks, plus one cross-language change with mandatory scoped rules. Compare:

- Correct required reads and missed obligations.
- Safe completion of each customization.
- Time before the first correct edit.
- Startup words and unnecessary reads.
- Detection of stale replacements.

Reject the simplification if fewer words produce more missed mandatory rules. Test manual traversal first, then compare CLI output against the same expected source sets.

The loader count is measured. The proposed fresh-startup target is under 800 words, but that needs verification after implementation. No repository files were changed, and no builds, test suites or Doctor commands were run.

#### The one idea to steal

Make a local override a checked replacement of named source text, so customization survives updates without asking a model to guess what it replaces.

## Determinist (Proposal H)

#### Thesis

Make mandatory context a computation over visible files and working paths. Keep judgment for understanding the task, choosing additional knowledge, resolving meaning, and accepting changes. Navigation should help readers find material without controlling whether required instructions become discoverable.

#### Diagnosis

1. **Recursive discovery is mandatory bookkeeping.** `src/open-forge/.agents/loader.md`:

   > Follow relevant branches recursively. Every folder in a route chain has exactly one recognized entrypoint.

   The agent must discover the complete relevant graph before knowing it has all required rules. The abandoned simplified-core draft retains this procedure.

2. **Required reads depend on an earlier discretionary choice.** `src/open-forge/.agents/loader.md`:

   > These tags act only through loaded parents. Neither activates an unselected ancestor or scope.

   A correctly tagged instruction can remain invisible because the agent skipped its parent. Task 53 identifies this problem for active Checkpoints.

3. **Instructions have two containers and inherited semantics.** `src/open-forge/.agents/loader.md`:

   > Only this loader and recognized loaded entrypoints define active Axioms.

   Readers must distinguish Axioms from Directive Instructions, recover ancestors, and understand the inherited marker. These distinctions increase bookkeeping without improving the instruction itself.

4. **Applicability requires maintaining conjunctions across ancestors.** `src/open-forge/.agents/loader.md`:

   > A file that would load or refresh does so, and applies, only while one working file matches its patterns and those of every ancestor with `applyTo`.

   Remembering the same matching path across several pattern sets is a parser task. It is especially fragile while planned working files change.

5. **The advertised computation is incomplete.** `src/open-forge/.agents/skills/open-forge-cli/SKILL.md`:

   > The CLI follows loading tags, not prose instructions.

   Startup requires an additional Skill read through prose. Agents must maintain a second dependency system after receiving computed context.

6. **Installer concerns occupy runtime attention.** `src/open-forge/.agents/loader.md`:

   > Each manager defines the route shapes it recognizes.

   Manager topology matters during installation and maintenance. Every ordinary task currently pays to learn it.

7. **Overwrites require semantic patch application.** `src/open-forge/.agents/loader.md`:

   > Where they answer the same question differently, the overwrite wins only for the corresponding content.

   Neither people nor tools have a precise replacement boundary. The model must infer which obligations survived.

8. **Source addressing reinforces the agent silo.** `src/open-forge/.agents/skills/open-forge-cli/references/common.md`:

   > A source reference is an exact source ID, such as `memory/crystallized`, or an exact `.agents/...` Markdown path.

   Ordinary documentation needs an intermediary representation before participating in the same retrieval system.

#### Concept budget

Counting separately specified mechanisms, roles, and procedural obligations gives **62 current concepts**:

| Group | Concepts |
|---|---|
| Navigation, 14 | Root handoff, recognized entrypoint names, Entries, route, root route, scope, slug, manager route shapes, recursive selection, separate active chains, ancestor recovery, Axioms, inherited obligations, inherited marker |
| Loading, 8 | On demand, LoadNow, KeepInMind, refresh schedule, listed order, once per pass, selected-parent activation, prose-required reads |
| Metadata, 10 | Frontmatter, root/scoped form, description, responsibility, tags field, applyTo, glob dialect, ancestor condition conjunction, working paths, unknown-path pending state |
| Classification, 7 | Core, Memory, Extension, Contextual, CurrentTruth, Evergreen, ordinary topical tags |
| Sources, 5 | User direction and autonomy, source authority and conflicts, validation versus acceptance, one defining source and relationships, corresponding-content overwrite precedence |
| Content roles, 6 | Directives, Guidance, Maps, Patterns, Skills, Templates |
| Memory, 6 | Working, Emerging, Crystallized, Archived, capture, transitions |
| Operations, 6 | Optional CLI, generated navigation maintenance, management versus meaning, removal exclusions, native Skill activation, independent Template copies |

The proposed design has **31 concepts**.

Its **13 shared concepts** are handoff, registered folder, index/Entries, description, LoadNow, local applyTo, working paths, frontmatter, source authority, acceptance, ordinary links, whole-file replacement, and the work loop.

Its **18 module concepts** are the six content roles, four Memory states, capture, transitions, responsibility, topical tags, native activation, Template copying, package ownership/exclusions, and CLI operation. Their detailed contracts load when those modules are used.

The reductions are deliberate:

- **Merged:** routes, root routes, slugs, and scopes become registered folders, optional indexes, and explicit applicability.
- **Merged:** Axioms become ordinary required instructions. Evergreen becomes the general duty to update affected accepted sources.
- **Deleted:** inherited markers, parent-gated loading, active chains, inherited file conditions, and unknown-path pending bookkeeping.
- **Merged:** KeepInMind and its refresh rules become one work loop.
- **Made mechanical:** inventory, filename compatibility, matching, replacement selection, ordering, deduplication, and generated navigation.
- **Moved narrower:** metadata authoring details, category contracts, Memory transitions, package topology, exclusions, and command procedures.
- **Merged:** composition tags become ordinary classification. Acceptance status is stated in readable source text.
- **Replaced:** semantic overwrite patches become complete replacement files.

#### The new loader

**571 words**, including Entries, using the same whitespace counting method that gives the current loader 2,052 words. This is proposed version-two behavior. `--required` needs implementation.

```markdown
# Open Forge Loader

Use the workspace's files for rules, knowledge, and work in progress. The CLI helps read them.

## Working rules

- Follow clear user direction within platform and safety constraints. Continue routine, reversible work within that direction. Ask about unresolved choices that materially change the result, authority, risk, or cost.
- Directives state required behavior. Guidance recommends approaches. Patterns show default shapes. Templates provide starting files. Maps point to sources. Skills follow their native runtime.
- Follow each instruction within the scope stated by its source. Reading advice, proposals, or history does not make them accepted requirements. Investigate conflicting requirements rather than choosing by file order or depth.
- Update the source that defines an accepted change. Preserve useful reasoning and unfinished work when future work needs them. Keep communication concise.

## Read context

1. At task start, resume, or context restoration, identify the working files, including planned paths. Investigation and review count as work. Reading a file only for context does not.
2. If the installed CLI supports this loading format, run `open-forge context --required`, adding `--for <path>` for every known working file. With no paths, it returns all required files.
3. Read the complete result. Resolve incomplete results before relying on them. Then choose additional knowledge, advice, or capabilities through descriptions and ordinary links. Optional sources do not hide required instructions.
4. Repeat the required read before working on additional paths, after context files change, and before handoff or completion. Save only the outcomes and continuation state worth retaining.

If the CLI is absent or lacks `--required`, use the plain-file procedure below.

## Required files

Each link under this loader's `Entries` registers the linked file's containing folder and its descendants. Folders may be anywhere inside the workspace. Nested indexes help navigation but are optional.

Required reads have `LoadNow` in their YAML frontmatter `tags`. This is the only loading tag. It works throughout registered folders, independently of parent indexes. Every mandatory read must be represented this way.

A required file may have its own `applyTo` list of workspace-relative glob patterns. Read it automatically when any working path matches any pattern. Without patterns, read it for every task. Conditions do not inherit from folders. Each instruction also states its applicability for ordinary readers.

Metadata may use the workspace's root or `open-forge:` form. Files without metadata remain optional. Descriptions and other tags help discovery and do not establish authority.

Native Skill packages retain their own loading rules. Discover their `SKILL.md` descriptions without scanning their resources for Open Forge loading tags.

## Without the CLI

Apply replacements before inspecting frontmatter. Inspect Markdown frontmatter throughout registered folders. Read every file tagged `LoadNow`, without filtering by `applyTo`. Apply instructions only within their stated scope. Then follow relevant ordinary links.

This reads more context, but requires no route selection, inherited conditions, or refresh-tag bookkeeping.

## Customization

Add, edit, replace, or remove the pieces useful to the workspace. Update `Entries` when registered folders change. Removed managed defaults need a recorded exclusion before a package update.

For ordinary Markdown outside native Skill packages, `name.overwrite.md` replaces `name.md` completely, including metadata. Read only the replacement. Other files retain their own requirements.

## Entries

- [Required behavior](directives/_directives.md) - #Directive
- [Recommended approaches](guidance/_guidance.md) - #Guidance
- [Pointers to useful sources](maps/_maps.md) - #Map
- [Work in progress, findings, accepted knowledge, and history](memory/_memory.md) - #Memory
- [Default shapes](patterns/_patterns.md) - #Pattern
- [Native capabilities](skills/_skills.md) - #Skill
- [Copy-ready starting files](templates/_templates.md) - #Template
```

#### Other Markdown changes

**AGENTS.md** keeps one handoff:

```markdown
# Open Forge

Before starting work, read `.agents/loader.md` and follow its context procedure.

**End of Open Forge managed section.**
```

CLAUDE.md remains a native bridge to AGENTS.md and the loader.

**Category entrypoints** become short definitions and navigation. Remove their startup tags and inherited Axioms. Move authoring detail to focused guides. Required authoring guides receive LoadNow with suitable file conditions.

A complete shipped replacement for `directives/_directives.md`:

```markdown
---
open-forge:
  description: Required behavior
  tags: [Directive]
---

# Directives

These files state required behavior. Each instruction states when it applies.

Required files carry LoadNow. Folder names organize them without creating inherited instructions or applicability.

## Entries

- none - No entries - #Empty
```

**Memory** retains its four optional states and their distinctions. Empty state entrypoints stop loading at startup. A continuation record genuinely needed for matching work receives LoadNow directly. Capture and transition guidance moves to the Memory authoring guide.

**Tags:** LoadNow is the sole loading tag. Retire KeepInMind. Core, Memory, Extension, Contextual, CurrentTruth, and Evergreen may remain searchable labels. Sources explicitly identify accepted, proposed, or historical content and its basis.

**Frontmatter:** retain existing fields and both containers. Required files need tags, with optional local applyTo. Ordinary documentation can remain metadata-free. Responsibility remains an authoring aid.

**CLI Skill:** remove its startup requirement. Its opening becomes a short command chooser. Discovery, authoring, and lifecycle references load for their respective tasks.

#### Decisions: model vs mechanism

| Decision or procedure today | Proposed responsibility |
|---|---|
| Understand goals, autonomy, consequential uncertainty | Model |
| Select additional knowledge, advice, Patterns, Skills | Model |
| Discover mandatory branches and recover ancestor context | Mechanism inventories registered folders |
| Interpret loading tags and parent activation | One mechanical LoadNow test |
| Evaluate globs and unknown working paths | Mechanism filters, or includes everything when paths are unknown |
| Supply investigated, planned, and reviewed paths | Model |
| Order, deduplicate, compose overwrites | Mechanism selects complete replacements and sorts paths |
| Refresh context | Agent or host recognizes work-loop events, command recomputes the set |
| Infer rule authority, resolve conflicts, establish acceptance | Model, under user direction |
| Choose narrow placement and loading policy | Author judgment, supported by coverage and size reports |
| Maintain Entries and validate metadata | Mechanism |
| Maintain accepted knowledge and useful Memory | Model |
| Interpret manager topology and preserve omissions | Lifecycle tools and their focused guide |
| Activate native capabilities | Native runtime |

The precise new CLI behavior is:

```text
open-forge context --required [<source-reference>...] --for <path> ...
```

Inputs are the registered folders, effective file metadata, explicit optional selections, and supplied working paths. Git changes cannot substitute for investigated or planned paths.

The command:

1. Resolves registered folders inside the workspace and inventories Markdown metadata, independently of nested indexes.
2. Applies complete replacements before reading metadata.
3. Selects every LoadNow file whose local condition matches. Without working paths, selects all LoadNow files.
4. Adds explicitly selected sources. Ancestor bodies are unnecessary because instructions are self-contained.
5. Deduplicates sources and emits them in workspace-path order.

The agent reads **verbatim effective bodies**, each preceded by its path and selection reason. Invalid loading metadata or unavailable registered roots produce an incomplete result, never an apparently complete empty set.

No session database or refresh manifest is required. Each invocation recomputes current context. Runtime hooks can invoke it at supported boundaries, but the CLI cannot guarantee that an agent calls it or follows its output.

Validation checks structural facts such as malformed patterns, missing registered roots, orphan replacements, stale Entries, and Directives missing LoadNow. It cannot prove semantic completeness.

#### Customization, independence and blending

Remove a component by removing its loader entry and, if desired, its files. Record managed exclusions through lifecycle tooling. Remaining components do not require its catalogue or terminology.

Replace an entire source through a complete overwrite companion. Add a rule or required reference with LoadNow and its own applicability. A rule without a useful file condition stays globally loaded and states its semantic conditions plainly.

A small workspace might contain:

```text
AGENTS.md
.agents/
  loader.md
  directives/
    _directives.md
    review.md
  memory/
    _memory.md
    working/
      release.md
  skills/
    _skills.md
    open-forge-cli/
      SKILL.md
docs/
  _docs.md
  engineering.md
  architecture.md
  adr/
    0001-storage.md
src/
  orders/
    README.md
```

Alongside the retained component entries, the loader contains:

```markdown
- [Project documentation](../docs/_docs.md) - #Documentation
- [Orders module](../src/orders/README.md) - #Orders
```

`docs/engineering.md` can carry LoadNow and `applyTo: ["src/**"]`. People and agents read that same engineering document. ADRs remain ordinary optional documentation.

This preserves *Routing Surfaces’* distinction between selection descriptions and complete destination meaning. It extends *Map Primitive’s* boundary that destinations retain their own knowledge. Maps remain useful for curated orientation and external sources. Libraries remain useful for sharing projected files. Neither is required to admit local documentation.

Enumeration cost grows with registered files. Prompt cost grows with matching required content.

I propose one narrow exception to **Context By Relevance**: without the CLI, read the larger required set to avoid omissions. **User-Owned Meaning** remains intact because the selection rules and complete sources are readable without tools.

#### Cost and migration

This is a versioned loading-model change with **medium CLI implementation work and substantial migration and documentation work**.

Reuse the YAML, Entries, and glob parsers. Change source inventory, containment, source identity, context selection, overwrite projection, and navigation generation. Explicit root links must accept ordinary files such as README.md. Intermediate entrypoints become optional.

Add a version-gated `loading: flat-v2` workspace setting. Legacy workspaces retain legacy interpretation until explicitly migrated. The marker selects a public contract described in Markdown.

Extend Context, Find, References, Route Inspect, and Status across registered folders. Index should preserve metadata-free documentation and modify only explicitly selected navigation regions outside `.agents/`.

Migration needs a reviewed plan:

- Preserve existing omissions and ownership boundaries.
- Move meaningful Axioms into self-contained instruction sources.
- Replace parent-gated required reads with explicit metadata.
- Translate contextual scope into readable applicability.
- Review inherited glob intersections. Concatenating ancestor lists changes AND into OR. Where a local list cannot express the intersection, load more broadly and preserve the conditions in the instructions.
- Expand partial overwrites into complete replacements with human review.
- Reclassify KeepInMind records individually.

Tests and snapshots must cover flat selection, unknown paths, external documentation, replacements, overlapping roots, native package boundaries, and legacy compatibility. Rewrite the loader maintenance contract, routing decisions, concepts documentation, CLI references, Extension scaffolds, and startup measurements.

The 571-word loader is implemented prose. Its behavioral improvement remains unproven.

#### Risks and proof

Global discovery can expose excessive LoadNow use previously hidden behind scope selection. Self-contained instructions require clearer applicability. Complete replacements can drift from updated defaults. Incorrect metadata can still hide an important rule.

The fallback requires the model to respect stated scope while reading a superset. That can produce inappropriate rule application. Tools can validate patterns and report matching files, but cannot prove that metadata expresses the intended policy.

The cheapest useful experiment needs no new CLI implementation:

- Create four tiny fixtures covering nested mandatory rules, mixed working paths, required documentation outside `.agents/`, and replacement plus resume behavior.
- Hand-assemble the exact proposed CLI output from an independently checked file-set oracle.
- Run the current loader, proposed plain-file fallback, and proposed computed-context path with the same model and task budget.
- Measure missed required reads, observable rule violations, incorrectly applied rules, total context consumed, and closeout omissions.

Use three repetitions per fixture. Reject or revise the design if it loses previously followed critical instructions. Then implement the command against the same oracle and repeat the behavioral probes.

#### The one idea to steal

Compute required reads from visible metadata and working paths, independently of navigation choices.
