# Extracted Decisions

These are the accepted decisions extracted from old sessions and idea notes.

## Product Direction

- Open Forge is markdown-first, repo-native, vendor-agnostic, and reviewable through plain files.
- The framework should keep the human in front and treat AI as a tool, not the owner of the work.
- The default install stays small. Opinionated patterns, workflows, skills, and technology packs belong in local files or extensions.
- The system optimizes for recursive customization and organic growth, not a complete predefined methodology.

## Source And Packaging

- Users receive the implementation payload under `src/open-forge/`.
- `docs/framework/` governs maintainers and AI working on this repository.
- `docs/framework/` content must not be required hidden runtime context for installed users.
- Framework files should be concise, explicit, and easy to diff.
- Users may edit framework files, but durable customization should prefer local sibling files, child routes, or `.overwrite.md` companions.

## Routing Model

- Open Forge routes agents through small markdown `entrypoints`.
- A folder is routable only when it contains exactly one recognized `entrypoint`.
- Open Forge-authored entrypoints use `_{folder-name}.md`.
- Compatibility entrypoints are `_index.md`, `index.md`, `_references.md`, and `references.md`.
- Generated `Entries` list direct sibling markdown files and direct child `entrypoints`.
- Nested routing requires an `entrypoint` at every visible folder level.
- The loader exposes only direct active root routes under `.agents/`.
- Loose markdown files beside `loader.md` are not root routes.

## Scope And Slugs

- A `framework route` is installed and managed by Open Forge.
- A `scope route` is a routed local folder used to narrow meaning or ownership below it.
- A `scoped framework route` is a framework route initialized inside a scope route.
- A `slug` is the concrete folder name used in a route path.
- Route-template placeholders such as `[scope]` are documentation, maintainer, CLI, and extension-author notation only. Installed workspaces receive concrete slug folders.
- Slug placement changes meaning. For example, `memory/crystallized/mobile-app/documents/` scopes documents inside crystallized memory, while `memory/mobile-app/crystallized/documents/` gives `mobile-app` its own memory states.

## Core

- The #Core layer contains the required base routing and primitive routes.
- Core installs `directives/`, `guidance/`, `patterns/`, `skills/`, `workflows/`, and `workspace/`.
- Directives are mandatory instructions agents must follow when they apply.
- Patterns are concrete reusable shapes for code, files, APIs, documents, and other inspectable work.
- Guidance is contextual advice for recurring choices, tradeoffs, and scenarios.
- Skills are bounded reusable agent capabilities with clear use cases and expected results.
- Workflows are repeatable agent workflows for reaching defined goals.
- Workspace routes point to important project locations and explain when to use them.
- Workflows may own local `directives/`, `patterns/`, `guidance/`, and `skills/` categories when those routes are essential to that workflow.

## Memory

- The #Memory layer is self-growing markdown memory for workspace state, AI communication, current records, historical records, and learning.
- Memory records state and must not own operational behavior.
- Operational behavior, reusable form, guidance, capability, workflow, or workspace routing must move to the matching #Core route.
- Memory states are `working/`, `emerging/`, `crystallized/`, and `archived/`.
- `working/` is temporary memory for active or recently interrupted work.
- `emerging/` is candidate memory that may be useful but is not accepted truth yet.
- `crystallized/` is accepted durable memory and current truth.
- `archived/` is historical memory kept for context after it is no longer current truth.
- `decisions/` is installed under `crystallized/` because decisions are accepted rationale, not a lifecycle state.
- `documents/` is installed under `crystallized/` for durable accepted records or routes to those records.
- `references/` is not installed for now.
- `archived/` remains a root memory state. Archive child routes can be created when they preserve origin, ownership, or clarity better than a flat archive.

## Tags

- Defined loader tags with behavior or truth-status semantics are #LoadWithParentEntrypoint, #LoadForPostWorkReview, #Contextual, and #CurrentTruth.
- #Core, #Memory, and #Extension are layer/routing tags.
- Route type tags such as #Directive, #Pattern, #Guidance, #Skill, #Workflow, and #Workspace are routing/search signals unless a loaded entrypoint defines more.
- Tags stay bare in markdown so tools can parse and graph them.
- Use normal words when defining the local concept itself; use tags when pointing to routed ownership, classification, promotion, load policy, truth status, search, or references.

## Extensions And CLI

- #Extension is optional installable material added on top of #Core and #Memory.
- Extensions should add files into the existing routed structure instead of creating another framework root.
- The current `extend` command installs local overlays and bundled first-party extensions as a dogfooding MVP.
- Bundled first-party extensions live under `src/extensions/{id}/payload` in the CLI package.
- Final extension design still needs manifests, route templates, scaffold content, preview, trust/provenance, install/update/remove behavior, and authoring helpers.
- Installed files remain runtime truth. Manifests may help install and migrate, but agents should not need hidden manifests to route at runtime.

## Do Not Revive Without A New Decision

- Do not restore `constants.md` as a default runtime primitive.
- Do not restore `local.md` as the required workspace route.
- Do not restore `_open-forge.md` companion files.
- Do not install `references/` by default.
- Do not make tasks/backlog a base Memory route before planning ownership is designed.
- Do not move default Memory outside `.agents/` yet.
- Do not add hidden version/source metadata to installed framework files just to support updates.
- Do not flatten nested category entries into the loader.
