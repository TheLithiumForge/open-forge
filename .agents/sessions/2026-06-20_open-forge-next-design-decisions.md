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

Loose markdown files beside `loader.md` are not root routes. The loader remains a registry of direct categories; users extend the root by adding a category folder and its entrypoint. Folders without a recognized entrypoint remain ordinary folders and are not indexed accidentally.

This recursive composition is the primary customization and scalability model. Future user documentation must explain it plainly and show that the same small convention supports personal projects, technology-specific knowledge, monorepos, shared knowledge trees, and arbitrarily deep local organization.

### Postpone loading metadata

Optional `when` metadata is postponed. Scope uses recursive placement and category descriptions, with a bias against new fields, states, inheritance, or propagation rules unless they remove a demonstrated routing ambiguity.

Approved on 2026-06-22: direct files inherit their containing category's positive scope. The root directives category is workspace-wide. Nested categories narrow or explicitly preserve that scope. Root files, direct work-scope categories, optional `global/` plus `scoped/` folders, or a combination are valid. Paths and category descriptions expose scope; tags reinforce it with compact domain, work-type, topic, technology, artifact, and primitive signals. Tags alone do not make a directive workspace-wide, mandatory, or active.

Approved on 2026-06-22: directives are mandatory modifiers, patterns are concrete reusable shapes, and guidance provides adaptable contextual judgment. Skills retain their established meaning as bounded reusable agent capabilities. Workflows retain their established meaning as larger goal-oriented agent workflows such as brainstorming, task creation, test-driven development, review, and implementation.

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

Load tag refinement on 2026-06-27: introduce #LoadWithParentEntrypoint as the only built-in reserved load-policy tag. It means a generated entry is loaded immediately after its parent entrypoint, in listed order. It affects loading only and does not create authority, scope, or precedence. It is not a global search; nested autoload requires a loaded parent chain. The installed loader has a `## Tags` section so users can define local tag meanings with small, diffable edits or overwrites. In the default payload, root `directives/` and `memory/` use it from the loader, and Memory `working/` and `crystallized/` use it as baseline Memory routing context. `emerging/` and `archived/` remain relevance-routed.

Tag taxonomy refinement on 2026-06-27: use singular PascalCase tags for Open Forge-authored route types. Layer tags classify material: #Core, #Memory, and #Extension. Route type tags identify the kind of route: #Directive, #Pattern, #Guidance, #Skill, #Workflow, and #Workspace. Layer and route type tags are routing/search signals unless a loaded entrypoint defines more; they do not create authority. #Memory promotion language should reference the matching #Core route tag instead of hard-coded folder names when that is clearer.

Tag definition refinement on 2026-06-28: the installed loader should define only tags with framework behavior or truth-status semantics. #LoadWithParentEntrypoint is operational loading behavior. #Contextual and #CurrentTruth define memory truth status. Other tags, including #Core, #Memory, and primitive route tags, remain routing and search signals whose meaning comes from path, description, and loaded entrypoint content. Memory should say operational material moves to the matching #Core route without enumerating every #Core primitive tag.

Tag section structure refinement on 2026-06-28: the installed loader `## Tags` section should split tag axioms from defined tags. Defined tags now include #LoadWithParentEntrypoint, #Core, #Memory, #Extension, #Contextual, and #CurrentTruth. Route type tags remain search/routing labels unless a loaded entrypoint defines more.

Tag wording refinement on 2026-06-29: use normal words when naming, defining, or explaining the local concept itself. Use bare tags when pointing to routed ownership, classification, promotion, load policy, truth status, search, or reference targets. Example: the Memory entrypoint says "Memory records state"; promotion text can say "move to #Core".

Directive routing refinement on 2026-06-28: `global/` and `scoped/` folder names are user documentation examples, not framework behavior. The installed directives entrypoint should state only that direct directive files are workspace-wide, child directive entrypoints define positive scope, and child routes load when path, description, tags, or defined tag behavior match current work. Do not add #ConditionalLoad; entries without a load-policy tag are already on-demand routes selected by the current request.

Memory promotion refinement on 2026-06-28: every Memory state may produce operational material that belongs in Core. Emerging memory is especially likely to produce patterns, guidance, directives, workflows, or other Core routes after validation. Promotion wording should use the generic matching #Core route and explicitly include user-created #Core categories and files instead of listing only built-in folders.

Memory child-route refinement on 2026-06-28: base Memory should install only universal child routes: `working/sessions/`, `working/handoffs/`, `emerging/observations/`, `emerging/ideas/`, `emerging/analysis/`, and `crystallized/documents/`. Do not install global `decisions/`, `brainstorming/`, `tasks/`, `backlog/`, or archive mirrors by default. Decisions are scoped on demand under whichever route owns the rationale. Archive remains generic historical Memory, but scoped `archived/` routes may also live under any owning route when they preserve origin better than a global archive route.

Extension and CLI backlog on 2026-06-28: Extensions should use standalone payloads such as `src/extensions/{id}/payload/.agents/...`. If an extension targets a deep route with missing ancestor category entrypoints, its manifest must provide scaffold metadata for those missing categories. The CLI should also later support user category scaffolding and extension authoring through the same template logic, but this remains last-priority work after core Memory and user docs.

Guidance naming and flexible route templates on 2026-06-30: the Core contextual-judgment primitive was renamed from `guidelines/`, `_guidelines.md`, and #Guideline to `guidance/`, `_guidance.md`, and #Guidance. Flexible Memory extras such as documents, references, scoped decisions, scoped archives, tasks, backlog, project records, and workflow outputs need a route-template design pass so they can be installed globally or under an owning route without pinning a single taxonomy into the base payload. Template paths may include user-provided slug segments such as `[project]`, `[scope]`, or `[state]`; those placeholders exist only for maintainers, extension authors, CLI prompts, and user-facing explanations. Installed workspaces receive ordinary concrete folders such as `mobile-app` or `billing-api`. The CLI should accept concrete slugs and scaffold only when every missing ancestor category has meaningful metadata from the user, an extension manifest, or a reviewed template. Category folder names should be slug-safe by default so users, agents, CLI updates, and extensions can target the same paths predictably without making folder names carry authority. Current bias: installed paths plus entrypoint content are runtime truth; JSON-style manifests are extension/template packaging metadata for scaffold, aliases, migrations, compatibility, provenance, and preview, not a hidden routing registry agents must read.

CLI slug and extension-authoring backlog on 2026-06-30: future CLI work should support generating concrete slugged paths from user intent, such as a new project route or project-scoped crystallized memory. It should preview concrete paths, create ordinary folders and entrypoints, generate missing ancestors only with meaningful metadata, detect collisions, and rebuild indexes. Future CLI work should also support generating extension templates for maintainers, including extension metadata, route templates, payload files, scaffold metadata, previews, compatibility notes, and local install/update/remove testing support.

Observation wording priority on 2026-06-30: `ideas/` and `observations/` need a sharper distinction before their implementation files are approved. Ideas are usually user-requested captures of possibilities. Observations are mainly agent-written notes extracted after work, especially across sessions, when the agent notices facts, recurring behavior, constraints, risks, or signals that may matter later. Observation wording must make that agent-initiated use clear while keeping observations contextual until validated or promoted.

Memory promotion direction priority on 2026-06-30: promotion applies across #Memory, not only observations. Memory material can move into another #Memory state or child route when its memory state changes, or out to #Core when it becomes operational behavior, reusable form, guidance, capability, workflow, workspace routing, or another #Core primitive. Observations are the clearest example because repeated or validated agent notes may become crystallized memory, archived history, patterns, directives, guidance files, skills, workflows, or workspace routes. Each memory type file must state this direction clearly enough that agents do not need to infer #Core promotion only from the root Memory file.

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
10. **Active review: implement guidance file by file.** The guidance payload governor and minimum `_guidance.md` are implemented for review under the final primitive name. They define selective routing, contextual judgment, reasoned adaptation, and recursive scope without seeded guidance or cross-primitive rules.
11. **Active review: implement skills file by file.** The skills payload governor and minimum `_skills.md` are implemented for review. They preserve established runtime meanings, define bounded reusable capabilities, and route scoped skills without seeded skill content.
12. **Active review: implement workflows file by file.** The workflows payload governor and minimum `_workflows.md` are implemented for review. They define goal-oriented agent workflows, workflow-local support bundles, and scoped workflow routing without seeded workflow content.
13. **Refine shipped Core implementation wording.** Define reserved terminology, decide backtick usage, replace unclear `category` wording with concrete route names where possible, fold user review notes into the implementation files, and preserve the approved governance meaning.
14. **Active: finalize the Memory state model.** Save and review the accepted `working/`, `emerging/`, `crystallized/`, and `archived/` containers; define how recursive subcategories grow; define how archived memory avoids becoming a dump; and decide the default child folders for the first Memory package.
15. **Active review: write Memory governance descriptors.** The `docs/framework` governors for `memory/`, `working/`, `emerging/`, `crystallized/`, and `archived/` are drafted for review. Each governor defines what the state is, what it contains, when to use it, why it exists, its axioms, and how recursive customization works.
16. **Active review: update the loader contract for Memory.** The loader and routing governors now specify that, when Memory is installed, the `memory/` entrypoint is always loaded so agents understand what Memory is, where each state lives, and how to route into current or durable memory.
17. **Active review: create Memory implementation files.** The installable payload files for `memory/`, `working/`, `emerging/`, `crystallized/`, and `archived/` are drafted for review, including their minimum `_{category}.md` entrypoints and generated regions. The `observations/` route needs a wording pass to emphasize agent-written post-work notes while keeping observations contextual. Memory promotion wording must cover both movement inside #Memory and extraction into matching #Core routes, with enough explicit wording in each memory type file to avoid relying on root-level inference.
18. **Define slugified category and route naming.** Decide the slug contract for user-created and extension-created category folders; keep canonical framework roots pinned; prefer stable lowercase kebab-case for slug segments; keep display names in titles, descriptions, or frontmatter; define collision handling; ensure existing user paths are not renamed without explicit approval; decide how filesystem slugs interact with extension/template manifests; and define CLI support for generating concrete slugged paths from user intent.
19. **Define route templates for flexible scoped extras.** Define pinned routes versus movable route templates; decide template notation such as `[project]`, `[scope]`, and `[state]`; decide how extras such as documents, references, decisions, archives, projects, tasks, backlog, and workflow outputs can be installed globally, project-scoped, custom-scoped, or under any owning route; require scaffold metadata for missing ancestor categories; preserve user choice without creating vague default buckets; and design CLI presets or wizards for scoped Memory route creation.
20. **Define the user-documentation architecture.** Decide README and short-guide responsibilities, location, navigation, primitive glossary, layer glossary, slug/route-template explanation, and voice.
21. **Rewrite the human onboarding.** Keep the README essential, explain recursive customization, scalability, Core, Memory, slugified route templates, and Extensions, and add only the small linked documents justified by priority 20.
22. **Run a consistency and security review.** Verify routing, indexes, frontmatter, generated-region safety, updates, prompt-injection boundaries, links, and CLI tests.
23. **Low priority: review implementation-file repetition after Memory settles.** Recheck the installed implementation files, especially #Memory, after the current work is mostly done. Decide whether repeated axioms should stay local for clarity or be moved into shared Memory axioms/common knowledge without making agents rely on hidden inference.
24. **Explore extensions, scoped containers, and CLI experience.** After the #Core framework and trust model are stable, define extension discovery, extension payloads, category scaffolding, route-template installation, extension-template authoring for maintainers, scoped decision/archive guidance, previews, installation, ownership, updates, removal, provenance, compatibility, local extension testing, and an optional interactive interface.

## Current Focus

Priorities 1 through 9 are approved and implemented. Priority 10 is implemented under the final `guidance/` name and remains in review. Priorities 11 and 12 have approved governance and pending implementation wording review. Priorities 14 through 17 are drafted for Memory review. Priorities 13 and 18 through 23 are pending.
