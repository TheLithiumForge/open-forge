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

## Priority List

1. **Completed: decide the merged category-file contract.** Approved and implemented on 2026-06-20. Category meaning and generated navigation now share `_{category}.md`; only the final marker-bounded region is generated.
2. **Completed: refine the workspace category.** Approved and implemented on 2026-06-20. Workspace routing is selective, workspace-owned, and recursively scalable without seeded project-layout assumptions.
3. **Completed: define category and route meaning ownership.** Approved and implemented on 2026-06-20. The routing governor owns shared semantics, the CLI generates active category discovery in the loader, and each category entrypoint owns its detailed meaning.
4. **Active: define directive, guide, and pattern semantics.** Establish authority, required contents, interaction, naming, and routing.
5. **Define the user-documentation architecture.** Decide README and short-guide responsibilities, location, navigation, and voice.
6. **Rewrite the human onboarding.** Keep the README essential and add only the small linked documents justified by priority 5.
7. **Resume file-by-file framework work.** Review each descriptor before rewriting its `src/` counterpart; start patterns only after priority 4.
8. **Run a consistency and security review.** Verify routing, indexes, frontmatter, generated-region safety, updates, prompt-injection boundaries, links, and CLI tests.

## Current Focus

Priorities 1 through 3 are approved and implemented. Priority 4 is active. Priorities 5 through 8 are pending.
