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

Approved on 2026-06-22: directives are mandatory modifiers, patterns are concrete reusable shapes, and guidelines provide adaptable contextual judgment. Skills retain their established meaning as bounded reusable agent capabilities. Workflows retain their established meaning as larger goal-oriented modules such as brainstorming, task creation, test-driven development, review, and implementation.

Approved on 2026-06-22: workflows may own local directive, pattern, guideline, and skill categories. Workspace directives remain active inside every workflow. Workflow-local material applies only while that workflow is active. Mixed local primitive bundles are limited to workflows and reuse the root loader and routing contract rather than embedding another framework installation.

Refined on 2026-06-22: within an active workflow, its local directives, patterns, guidelines, and skills are preferred over broader workspace material when safe and allowed. Workspace directives remain active, and unresolved conflicts must be reported rather than silently discarded.

Ownership refinement on 2026-06-22: each primitive category defines only its own recursive behavior. Material in a narrower selected scope is preferred over broader material of the same primitive when safe and allowed. Workflow-local composition and cross-primitive preference belong to the workflows category rather than being repeated in the loader or sibling primitive entrypoints.

Approved on 2026-06-22: the core payload installs root directories for directives, patterns, guidelines, skills, and workflows. Each directory begins with only its minimum category entrypoint and generated region. Local files and optional modules add actual content; the core does not seed an opinionated methodology.

Implemented on 2026-06-22: the directives governor, minimum `_directives.md` payload, loader-first directive rule, README install shape, and installation coverage are aligned and verified.

Backlog refinement on 2026-06-23: installable material should be considered in layers. Layer 1 is Core or Core Context. Layer 2 is Memory, covering active memory, sessions, ideas, observations, analysis, decisions, documents, handoffs, and possible tasks or backlog. Layer 3 is Extensions, with modules as the likely concrete CLI package unit. Memory needs a separate design pass before user documentation is finalized.

Review refinement on 2026-06-25: the framework governance descriptors are approved. The shipped implementation files still need a terminology and readability pass. Reserved framework terms should likely use backticks, concrete route names should replace vague `category` wording where possible, and implementation axioms should be specific enough that humans and weaker agents do not need to infer the intended loading behavior.

## Priority List

1. **Completed: decide the merged category-file contract.** Approved and implemented on 2026-06-20. Category meaning and generated navigation now share `_{category}.md`; only the final marker-bounded region is generated.
2. **Completed: refine the workspace category.** Approved and implemented on 2026-06-20. Workspace routing is selective, workspace-owned, and recursively scalable without seeded project-layout assumptions.
3. **Completed: define category and route meaning ownership.** Approved and implemented on 2026-06-20. The routing governor owns shared semantics, the CLI generates active category discovery in the loader, and each category entrypoint owns its detailed meaning.
4. **Completed: lock the recursive category-tree format.** Verified arbitrary-depth indexing, defined the category-only root boundary, and reserved recursive composition as a central point for future user documentation.
5. **Completed: define directive, pattern, guideline, skill, and workflow semantics.** Approved visible recursive scope, permissive directive layouts, mandatory directive behavior, established AI meanings for skills and workflows, and workflow-local primitive bundles without activation metadata.
6. **Completed: implement directives file by file.** Approved the directives payload governor, updated the loader contract, wrote the minimum `_directives.md` payload, and verified installation and routing.
7. **Completed: audit approved payload axioms.** Confirmed `AGENTS.md`, simplified loader and workspace axioms, made overwrite loading explicit, and kept recursive precedence in the category that owns it.
8. **Completed: implement patterns file by file.** Approved and implemented the patterns payload governor and minimum `_patterns.md` with selective routing, concrete inspectable shapes, established-default authority, and recursive scope without seeded pattern content or workflow-specific rules.
9. **Active review: implement guidelines file by file.** The guidelines payload governor and minimum `_guidelines.md` are implemented for review. They define selective routing, contextual judgment, reasoned adaptation, and recursive scope without seeded guidance or cross-primitive rules.
10. **Active review: implement skills file by file.** The skills payload governor and minimum `_skills.md` are implemented for review. They preserve established runtime meanings, define bounded reusable capabilities, and route scoped skills without seeded skill content.
11. **Active review: implement workflows file by file.** The workflows payload governor and minimum `_workflows.md` are implemented for review. They define goal-oriented modules, workflow-local support bundles, and scoped workflow routing without seeded workflow content.
12. **Refine shipped Core implementation wording.** Define reserved terminology, decide backtick usage, replace unclear `category` wording with concrete route names where possible, fold user review notes into the implementation files, and preserve the approved governance meaning.
13. **Define the layered install model and Memory.** Decide whether Layer 2 is one umbrella category or several root categories; decide active memory, sessions, ideas, observations, analysis, decisions, documents, handoffs, tasks, backlog, and archive semantics; then write governors and minimum payload files for the accepted shape.
14. **Define the user-documentation architecture.** Decide README and short-guide responsibilities, location, navigation, primitive glossary, layer glossary, and voice.
15. **Rewrite the human onboarding.** Keep the README essential, explain recursive customization, scalability, Core, Memory, and Extensions, and add only the small linked documents justified by priority 14.
16. **Run a consistency and security review.** Verify routing, indexes, frontmatter, generated-region safety, updates, prompt-injection boundaries, links, and CLI tests.
17. **Explore optional modules and CLI experience.** After the core framework and trust model are stable, define intuitive core update commands and optional-module discovery, preview, selection, installation, ownership, updates, removal, provenance, compatibility, and an optional interactive interface.

## Current Focus

Priorities 1 through 8 are approved and implemented. Priorities 9 through 11 have approved governance and pending implementation wording review. Priorities 12 through 17 are pending.
