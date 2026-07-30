---
open-forge:
  description: Session proposing the next design decisions after the first framework shape, processed one priority at a time
  tags: [Session, FrameworkDesign, Contextual, Historical]
---

# Open Forge Next Design Decisions

## Status

This session captures proposed changes that require analysis and approval before implementation. Work proceeds one priority at a time. Later priorities remain pending until every earlier priority is approved, documented, implemented, and reviewed.

## Original Proposal

### Merge the category contract and category index

The current `_{category}.md` and `_{category}-open-forge.md` files are separate, but both are framework-managed category entry points that users normally do not change. Consider merging them into one `_{category}.md` file.

The merged file would contain the small set of framework rules for the category near the beginning and a generated list near the end. The generated list needs a stable heading and Open Forge ownership markers. If the generated section does not exist, the CLI can add it at the end. If it exists, the CLI must update only that section.

This could remove files, reduce initial reading, and give each category one obvious entry point. The result must still be clear about what is generated and what is stable framework content.

The larger goal is to avoid the experience of other agent and spec-driven development frameworks that install many files immediately and leave the user in a difficult-to-understand system. Open Forge must feel like the user's own organized space, remain comfortable to inspect, and scale without turning into a large up-front prompt. Prompt injection is a real concern, so routing, ownership, and authority boundaries must stay explicit.

Analyze this design first. Do not implement it until its exact contract is approved.

### Refine the workspace category

The workspace category must support a single project, a monorepo, multiple projects, and larger workspaces without imposing one organization. Users may create one route file per important area or project, group several manual routes into one file, use nested folders and indexes, or choose another structure that suits the workspace.

A scalable convention may be one file per important route or area, with a short description of what an agent will find there. That convention is useful but can be tedious, so it belongs in optional guidance rather than framework axioms.

The workspace category must route important meaning and context. It must not require a complete map of every file and folder. Rune may eventually offer deeper mapping, but Open Forge workspace routing must remain focused and flexible.

### Separate directives, guides, and patterns

- A guide describes a preferred or historically successful approach, explains why it is useful, and permits a reasoned alternative when the preferred approach does not fit.
- A directive states an authoritative requirement. The agent must follow it. If it cannot be followed, the agent must report the conflict and request a decision or explicit exception.
- A pattern captures a concise, repeatable implementation or structural shape already used by the project, such as file placement, component file sets, member ordering, or the use of a particular API.

Their authority, interaction, contents, and routing still require analysis and approval.

### Create concise user documentation

Open Forge needs streamlined user-facing documentation in addition to maintainer descriptors and installed payload files:

- `README.md` and short user documents introduce the framework to humans.
- `docs/framework/` governs the meaning and limits of maintained framework and payload files.
- `src/` contains the implementation and the concise files installed into a user's workspace.

The README should retain only the essential why, what, and how. Usage should explain automatic loading through `AGENTS.md` and manual loading through the single Open Forge skill. Small linked documents can explain how Open Forge grows with a person, team, project, monorepo, or larger workspace; acts as durable project memory; routes agents to owned context; controls context and model cost; and remains the user's own space.

### Define category and route meaning ownership

The loader currently gives each Open Forge category a short routing description. The framework also needs a clear, maintainable place for the fuller meaning, boundaries, and interaction of categories and routes.

The design must keep the loader cheap while preventing category meaning from becoming duplicated central documentation. It must define the shared category and route primitives and establish where each category owns its detailed meaning.

Approved on 2026-06-20: `docs/framework/concepts/routing.md` governs the shared primitives, the loader contains a generated registry of direct active categories, and each `_{category}.md` entrypoint owns that category's detailed installed meaning.

Refined on 2026-06-20: the CLI generates loader paths, descriptions, and tags from direct child category entrypoints. This gives agents enough information to select a category without probing every folder or duplicating detailed category truth.

Compatibility refinement on 2026-06-20: the CLI accepts `index.md`, `_index.md`, `references.md`, and `_references.md` as category entrypoint aliases for external tools. Open Forge-authored categories keep `_{category}.md`. Each folder contains at most one recognized entrypoint so routing remains deterministic.

Path refinement on 2026-06-20: runtime path constants were removed. Generated loader paths are concrete and relative to the active workspace root, defined as the directory whose `AGENTS.md` selected the loader. Git repositories, submodules, and symlink targets do not redefine that logical root. User or agent identity is runtime context and does not belong in core framework constants.

### Preserve the recursive category tree

The merged category contract and index create a recursively composable routing tree. Any folder can opt into routing by containing exactly one recognized category entrypoint. Its direct markdown files and direct child categories become generated entries, and the same rule continues at every depth.

For example, a workspace can add `.agents/patterns/react/_react.md`, place React pattern files beside it, and add more category folders below it without changing the loader or CLI configuration. A new direct category under `.agents/` becomes a loader route. Nested categories remain behind their parent entrypoints instead of being flattened into the loader.

Loose markdown files beside `loader.md` are not root routes. The loader remains a registry of direct categories; users extend the root by adding a category folder and its entrypoint. Folders without a recognized entrypoint stay outside Open Forge routing and are not indexed accidentally.

This recursive composition is the primary customization and scalability model. Future user documentation must explain it plainly and show that the same small convention supports personal projects, technology-specific knowledge, monorepos, shared knowledge trees, and arbitrarily deep local organization.

### Postpone loading metadata

Optional `when` metadata is postponed. Scope uses recursive placement and category descriptions, with a bias against new fields, states, inheritance, or propagation rules unless they remove a demonstrated routing ambiguity.

Approved on 2026-06-22: direct files inherit their containing category's positive scope. The root directives category is workspace-wide. Nested categories narrow or explicitly preserve that scope. Root files, direct work-scope categories, optional `global/` plus `scoped/` folders, or a combination are valid. Paths and category descriptions expose scope; tags reinforce it with compact domain, work-type, topic, technology, artifact, and primitive signals. Tags alone do not make a directive workspace-wide, mandatory, or active.

Approved on 2026-06-22: directives are mandatory modifiers, patterns are concrete reusable shapes, and guidance provides adaptable contextual judgment. Skills retain their established meaning as bounded reusable agent capabilities. Workflows retain their established meaning as repeatable agent workflows for reaching defined goals such as brainstorming, task creation, test-driven development, review, and implementation.

Approved on 2026-06-22: workflows may own local directive, pattern, guidance, and skill categories. Workspace directives remain active inside every workflow. Workflow-local material applies only while that workflow is active. Mixed local primitive bundles are limited to workflows and reuse the root loader and routing contract rather than embedding another framework installation.

Refined on 2026-06-22: within an active workflow, its local directives, patterns, guidance, and skills are preferred over broader workspace material when safe and allowed. Workspace directives remain active, and unresolved conflicts must be reported rather than silently discarded.

Ownership refinement on 2026-06-22: each primitive category defines only its own recursive behavior. Material in a narrower selected scope is preferred over broader material of the same primitive when safe and allowed. Workflow-local composition and cross-primitive preference belong to the workflows category rather than being repeated in the loader or sibling primitive entrypoints.

Approved on 2026-06-22: the #Core payload installs root directories for directives, patterns, guidance, skills, and workflows. Each directory begins with only its minimum category entrypoint and generated region. Local files and #Extension payloads add actual content; #Core does not seed an opinionated methodology.

Implemented on 2026-06-22: the directives governor, minimum `_directives.md` payload, loader-first directive rule, README install shape, and installation coverage are aligned and verified.

Backlog refinement on 2026-06-23: installable material should be considered in layers. Layer 1 maps to #Core, Layer 2 maps to #Memory, and Layer 3 maps to #Extension. #Memory covers active memory, history, seeds, observations, ideas, analysis, decisions, crystallized files, documents, handoffs, and possible tasks or backlog. Extensions are the concrete CLI package unit. #Memory needs a separate design pass before user documentation is finalized.

Review refinement on 2026-06-25: the framework governance descriptors are approved. The shipped implementation files still need a terminology and readability pass. Reserved framework terms should likely use backticks, concrete route names should replace vague `category` wording where possible, and implementation axioms should be specific enough that humans and weaker agents do not need to infer the intended loading behavior.

Memory goal refinement on 2026-06-25: #Memory should behave like a self-growing memory system. Active memory decays, raw memory is archived, useful truth is extracted and validated against current reality, and crystallized memory is consolidated into compact current understanding without erasing history.

Memory state refinement on 2026-06-26: #Memory should use state containers named `working/`, `emerging/`, `crystallized/`, and `archived/`. These read as working memory, emerging memory, crystallized memory, and archived memory. The lifecycle is `working -> emerging -> crystallized -> archived`. The structure must remain recursively customizable: subcategories are encouraged when they improve routing, but new top-level #Memory states are rare and should be discussed with the user.

Memory governance draft on 2026-06-26: descriptors were drafted for `memory/`, `working/`, `emerging/`, `crystallized/`, and `archived/`. The loader and routing governors were updated to require loading `memory/_memory.md` for every request when the Memory layer is installed. Implementation payload files are not created yet and remain pending review.

Memory implementation draft on 2026-06-26: minimum payload entrypoints were drafted for `.agents/memory/_memory.md`, `.agents/memory/working/_working.md`, `.agents/memory/emerging/_emerging.md`, `.agents/memory/crystallized/_crystallized.md`, and `.agents/memory/archived/_archived.md`. The payload loader now loads `memory` after `directives` when present, and the README install shape includes Memory.

Memory refinement on 2026-06-26: #Memory implementation files and governors must not seed concrete child taxonomies or example folder structures. The installed files should define only the state meaning, loading behavior, authority boundary, and extension rules. Concrete folder recipes belong in user documentation, local files, or #Extension payloads.

Memory taxonomy boundary on 2026-06-26: future child folders such as sessions, ideas, analysis, decisions, documents, handoffs, tasks, or backlog may still become #Core #Memory children, #Extension children, or user-owned local routes. They should not be mentioned directly in base #Memory state files unless they are actually installed and governed. User documentation may show them as examples without turning them into base behavior.

Payload boundary refinement on 2026-06-27: users receive only the installed implementation files from `src/open-forge/`. Governance descriptors in `docs/framework/` are for maintainers and AI working on this repository. Installed files must contain all definitions, loading rules, authority boundaries, and route usage needed by users and agents; descriptors must not become hidden runtime context.

Load tag refinement on 2026-06-27: introduced #LoadWithParentEntrypoint as the first built-in reserved load-policy tag. It means a generated entry is loaded immediately after its parent entrypoint, in listed order. It affects loading only and does not create authority, scope, or precedence. It is not a global search; nested autoload requires a loaded parent chain. The installed loader has a `## Tags` section so users can define local tag meanings with small, diffable edits or overwrites. In the default payload, root `directives/` and `memory/` use it from the loader, and Memory `working/` and `crystallized/` use it as baseline Memory routing context.

Tag taxonomy refinement on 2026-06-27: use singular PascalCase tags for Open Forge-authored route types. Layer tags classify material: #Core, #Memory, and #Extension. Route type tags identify the kind of route: #Directive, #Pattern, #Guidance, #Skill, #Workflow, and #Workspace. Layer and route type tags are routing/search signals unless a loaded entrypoint defines more; they do not create authority. #Memory promotion language should reference the matching #Core route tag instead of hard-coded folder names when that is clearer.

Tag definition refinement on 2026-06-28: the installed loader should define only tags with framework behavior or truth-status semantics. #LoadWithParentEntrypoint and #LoadForPostWorkReview are operational loading behavior. #Contextual and #CurrentTruth define memory truth status. Other tags, including #Core, #Memory, and primitive route tags, remain routing and search signals whose meaning comes from path, description, and loaded entrypoint content. Memory should say operational material moves to the matching #Core route without enumerating every #Core primitive tag.

Tag section structure refinement on 2026-06-28: the installed loader `## Tags` section should split tag axioms from defined tags. Defined tags now include #LoadWithParentEntrypoint, #LoadForPostWorkReview, #Core, #Memory, #Extension, #Contextual, and #CurrentTruth. Route type tags remain search/routing labels unless a loaded entrypoint defines more.

Tag wording refinement on 2026-06-29: use normal words when naming, defining, or explaining the local concept itself. Use bare tags when pointing to routed ownership, classification, promotion, load policy, truth status, search, or reference targets. Example: the Memory entrypoint says "Memory records state"; promotion text can say "move to #Core".

Directive routing refinement on 2026-06-28: `global/` and `scoped/` folder names are user documentation examples, not framework behavior. The installed directives entrypoint should state only that direct directive files are workspace-wide, child directive entrypoints define positive scope, and child routes load when path, description, tags, or defined tag behavior match current work. Do not add #ConditionalLoad; entries without a load-policy tag are already on-demand routes selected by the current request.

Memory promotion refinement on 2026-06-28: every Memory state may produce operational material that belongs in Core. Emerging memory is especially likely to produce patterns, guidance, directives, workflows, or other Core routes after validation. Promotion wording should use the generic matching #Core route and explicitly include user-created #Core categories and files instead of listing only built-in folders.

Memory child-route refinement on 2026-06-28, superseded by the 2026-07-01 P14 decision where noted: base Memory should install only universal child routes and avoid vague future buckets such as `brainstorming/`, `tasks/`, `backlog/`, or archive mirrors by default. `crystallized/decisions/` is now installed by default because decisions are accepted rationale, not a separate lifecycle state. Archive remains the historical Memory state; archive child routes get their scope from slug placement and entrypoint descriptions.

Extension and CLI backlog on 2026-06-28: Extensions should use standalone payloads such as `src/extensions/{id}/payload/.agents/...`. If an extension targets a deep route with missing ancestor category entrypoints, its manifest must provide scaffold content for those missing categories. The CLI should also later support user category scaffolding and extension authoring through the same template logic, but this remains last-priority work after core Memory and user docs.

Guidance naming and flexible route templates on 2026-06-30: the Core contextual-judgment primitive was renamed from `guidelines/`, `_guidelines.md`, and #Guideline to `guidance/`, `_guidance.md`, and #Guidance. Flexible Memory extras such as documents, references, decision routes, archive child routes, tasks, backlog, records under scoped slug routes, and workflow outputs need a route-template design pass so they can be installed at concrete routed locations without pinning a single taxonomy into the base payload. Template paths may include named user-provided slug segments such as `[scope]`, `[route]`, or `[state]`; those placeholders exist only for maintainers, extension authors, CLI prompts, and user-facing explanations. Installed workspaces receive concrete folders such as `mobile-app` or `billing-api`. The CLI should accept concrete slugs and scaffold only when every missing ancestor category has meaningful entrypoint content from the user, an extension manifest, or a reviewed template. Category folder names should be slug-safe by default so users, agents, CLI updates, and extensions can target the same paths predictably without making folder names carry authority. Current bias: installed paths plus entrypoint content are runtime truth; JSON-style manifests are extension/template packaging data for scaffold, aliases, migrations, compatibility, provenance, and preview, not a hidden routing registry agents must read.

CLI route and extension-authoring backlog on 2026-06-30: future CLI work should support generating concrete routed paths from user intent, such as a new scoped slug route or crystallized memory under an owner route. It should preview concrete paths, create folders and entrypoints, generate missing ancestors only with meaningful entrypoint content, detect collisions, and rebuild indexes. It should also update every path-recognized framework-owned scoped entrypoint when the framework wording changes, while preserving overwrite companions and leaving git diff to expose manual edits. Future CLI work should also support generating extension templates for maintainers, including extension metadata, route templates, payload files, scaffold content, previews, compatibility notes, and local install/update/remove testing support.

Route-template MVP on 2026-07-01: the current CLI now updates framework-owned scoped entrypoints during `install` by matching concrete path shapes and canonical entrypoint filenames. No template, source, or version metadata was added to installed files. The indexer already supports arbitrary nested routed folders when entrypoints exist. The MVP CLI does not scaffold slug routes from templates yet; that remains future CLI work. `docs/cli.md`, the README, the routing and formatting governors, and the installed loader now describe scoped slug routes, visible entrypoint chains, and the current CLI boundary.

P14 flexible extras decision on 2026-07-01: do not add `references/` for now. Keep `archived/` as a root #Memory state. Install `crystallized/decisions/` by default because decisions are accepted rationale for important choices, not a separate lifecycle state. A decision about #Core material stays in #Memory as rationale while the behavior itself stays in its matching #Core route. Slug placement changes meaning: a slug under `crystallized/` scopes current memory inside that state, while a slug above `crystallized/` owns its own Memory states. Archive metadata blocks and forceful/soft CLI upgrade modes are deferred ideas.

Observation wording pass on 2026-06-30: `ideas/` and `observations/` now have a sharper distinction. Ideas are usually user-requested captures of possibilities. Observations are grounded findings noticed during work and may be written by agents after work when a noticed finding may matter later but is not accepted truth. Observations remain contextual until validated, promoted, or explicitly accepted.

Observation routing refinement on 2026-07-01: because Open Forge is a routing system, proactive observation writing must be discoverable from already loaded routes without hard-coding child route names in root axioms. Add #LoadForPostWorkReview as a reserved load-policy tag. It loads tagged entries before ending meaningful work, only inside already loaded `Entries`, so agents can route useful material produced during the work. The default #Memory payload uses it on candidate-memory routes that should participate in post-work review.

Memory promotion direction priority on 2026-06-30: promotion applies across #Memory, not only observations. Memory material can move into another #Memory state or child route when its memory state changes, or out to #Core when it becomes operational behavior, reusable form, guidance, capability, workflow, workspace routing, or another #Core primitive. Observations are the clearest example because repeated or validated agent notes may become crystallized memory, archived history, patterns, directives, guidance files, skills, workflows, or workspace routes. Each memory type file must state this direction clearly enough that agents do not need to infer #Core promotion only from the root Memory file.

Alpha sequence refinement on 2026-07-06: finish the alpha version first, then dogfood Open Forge by migrating this project's current notes into its own workflow, then create meaningful patterns/extensions from what the dogfooding reveals, then restructure and re-review every maintained file. Do not over-design final user documentation before dogfooding exposes what the framework actually needs to explain.

Terminology backlog on 2026-07-06: after alpha dogfooding, define a stable vocabulary for terms such as current truth, durable memory, historical memory, accepted memory, contextual memory, transfer notes, and learning. Use that vocabulary to clean route descriptions, loader tag meanings, user-facing docs, and maintainer governors. The goal is concise wording that remains understandable to humans and agents from generated `Entries`, not headline-style fragments.

Extension MVP refinement on 2026-07-06: after the alpha wording pass, the MVP CLI should support installing a local extension overlay and rebuilding indexes. This is only a dogfooding capability, not the final extension registry, wizard, manifest, preview, update, or removal design. Final extension UX remains a later priority after route templates and dogfooding.

## Priority List

1. **Completed: decide the merged category-file contract.** Approved and implemented on 2026-06-20. Category meaning and generated navigation now share `_{category}.md`; only the final marker-bounded region is generated.
2. **Completed: refine the workspace category.** Approved and implemented on 2026-06-20. Workspace routing is selective, workspace-owned, and recursively scalable without seeded project-layout assumptions.
3. **Completed: define category and route meaning ownership.** Approved and implemented on 2026-06-20. The routing governor owns shared semantics, the CLI generates active category discovery in the loader, and each category entrypoint owns its detailed meaning.
4. **Completed: lock the recursive category-tree format.** Verified arbitrary-depth indexing, defined the category-only root boundary, and reserved recursive composition as a central point for future user documentation.
5. **Completed: define directive, pattern, guidance, skill, and workflow semantics.** Approved visible recursive scope, permissive directive layouts, mandatory directive behavior, established AI meanings for skills and workflows, and workflow-local primitive bundles without activation metadata.
6. **Completed: implement directives file by file.** Approved the directives payload governor, updated the loader contract, wrote the minimum `_directives.md` payload, and verified installation and routing.
7. **Completed: audit approved payload axioms.** Confirmed `AGENTS.md`, simplified loader and workspace axioms, made overwrite loading explicit, and kept recursive precedence in the category that owns it.
8. **Completed: implement patterns file by file.** Approved and implemented the patterns payload governor and minimum `_patterns.md` with selective routing, concrete inspectable shapes, established-default authority, and recursive scope without seeded pattern content or workflow-specific rules.
9. **Completed: rename guidance.** `guidelines/`, `_guidelines.md`, and #Guideline were renamed to `guidance/`, `_guidance.md`, and #Guidance across governance descriptors, installed payload files, README structure, generated indexes, and CLI tests.
10. **Completed: implement guidance file by file.** The guidance payload governor and minimum `_guidance.md` were reviewed and committed under the final primitive name. They define selective routing, contextual judgment, reasoned adaptation, and recursive scope without seeded guidance or cross-primitive rules.
11. **Refine shipped Core implementation wording.** Define reserved terminology, tag/backtick usage, and clear references for installed files; replace unclear `category` wording with concrete route or entrypoint names where possible; fold user review notes into implementation files; and preserve approved governance meaning.
12. **Settle Memory model closeout decisions.** Confirm the accepted `working/`, `emerging/`, `crystallized/`, and `archived/` states; define recursive child-category growth; define how archived memory avoids becoming a dump; confirm decisions as crystallized rationale; confirm archive child routing derives from the shared route model; and confirm default child-route boundaries for the first Memory package.
13. **Define scoped slug naming.** Decide the slug contract for user-created and extension-created route folders; keep canonical framework roots and Memory state anchors pinned; prefer stable lowercase kebab-case for concrete scope segments; define optional scoped slug insertion around and between pinned route segments; keep display names in titles, descriptions, or frontmatter; define collision handling; ensure existing user paths are not renamed without explicit approval; decide how filesystem slugs interact with extension/template manifests; define path-pattern identification for framework-owned scoped entrypoint updates without adding template or version metadata; and define CLI support for generating concrete routed paths from user intent.
14. **Define route templates for flexible extras.** Define pinned routes versus movable route templates; decide named template notation such as `[scope]`, `[route]`, and `[state]`; decide how extras such as documents, references, decisions, archives, tasks, backlog, records under scoped slug routes, and workflow outputs can be installed at concrete routed locations; require scaffold content for missing ancestor entrypoints; preserve user choice without creating vague default buckets; and design CLI presets or wizards for Memory route creation.
15. **Define the user-documentation architecture.** Decide README and short-guide responsibilities, location, navigation, primitive glossary, layer glossary, scoped slug route and route-template explanation, and voice.
16. **Rewrite the human onboarding.** Keep the README essential, explain recursive customization, scalability, #Core, #Memory, route templates, and #Extension, and add only the small linked documents justified by priority 15.
17. **Explore extensions, route templates, and CLI experience.** After the #Core framework and trust model are stable, define extension discovery, extension payloads, category scaffolding, route-template installation, extension-template authoring for maintainers, decision/archive routing guidance, previews, installation, ownership, updates, removal, provenance, compatibility, local extension testing, and an optional interactive interface.
18. **Final review: skills.** The skills governance descriptor and minimum `_skills.md` payload exist. Do a final wording and contract pass after Core wording, route-template, and extension terminology are stable.
19. **Final review: workflows.** The workflows governance descriptor and minimum `_workflows.md` payload exist. Do a final wording and contract pass after Core wording, route-template, and extension terminology are stable.
20. **Final review: reread every maintained file.** Re-review every governance descriptor, installable payload file, generated entrypoint, README or user-facing document, and maintained handover/idea file that still affects the framework. Make sure the wording is streamlined, behavior is correct, scopes are not mixed, tags and terminology are consistent, and no file depends on hidden inference or outdated concepts.
21. **Low priority: review implementation-file repetition after Memory settles.** Recheck the installed implementation files, especially #Memory, after the current work is mostly done. Decide whether repeated axioms should stay local for clarity or be moved into shared Memory axioms/common knowledge without making agents rely on hidden inference.
22. **Run a consistency and security review.** Verify routing, indexes, frontmatter, generated-region safety, updates, prompt-injection boundaries, links, CLI tests, and final terminology consistency across governance, payload, README, and generated entries.

## Current Focus

Priorities 1 through 10 are approved and implemented. The previous active review items were moved to final review passes at priorities 18 through 20, because the user has reviewed the files produced so far and they should no longer block the next design work.

What appears already done in the meantime: skills and workflows both have governance descriptors and installed payload entrypoints; the Memory state names are chosen; Memory governance descriptors and installable payload files exist; the loader defines #LoadWithParentEntrypoint, #LoadForPostWorkReview, #Core, #Memory, #Extension, #Contextual, and #CurrentTruth; the base Memory payload includes working sessions and handoffs, emerging analysis, ideas, and observations, crystallized documents and decisions, and archived memory; and the README already reflects the current broad structure. These still need later final review, wording cleanup, and consistency checks rather than first-pass creation.

Priority 13 has an MVP implementation ready for review. Priority 20 is the later full-file review after the structural decisions settle.
