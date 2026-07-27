---
open-forge:
  description: Current maintenance contract for the installable Open Forge loader and its dogfood counterpart
  responsibility: Preserve the loader's authored contract, source and dogfood alignment, generated boundary, and verification
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Framework, Loader, Routing]
---

# Loader Maintenance Contract

## Source

[`src/open-forge/.agents/loader.md`](../../../../../../../src/open-forge/.agents/loader.md) is the canonical installed Framework loader. The repository [loader](../../../../../../loader.md) dogfoods the same authored contract with its locally generated root entries.

The [Framework Architecture](../../../framework/architecture.md#canonical-entry) is authoritative for the loader's structural role. The [top architecture](../../../architecture.md#framework-composition) is authoritative for the relationship among Core, Memory, and Extensions. The [Open Forge Routing scope](../../../framework/routing/_routing.md) defines the detailed route, scope, loading, path, and [overwrite](../../../framework/routing/overwrites.md) contracts. The [routed Markdown contract](../../../framework/markdown/routes.md) is authoritative for its canonical authored and generated representation. The [routing model](../../../../decisions/routing-model.md), [routing surfaces](../../../../decisions/routing-surfaces.md), [scope and slugs](../../../../decisions/scope-and-slugs.md), [tags](../../../../decisions/tags.md), [loading reliability](../../../../decisions/loading-reliability.md), [typed authority terminology](../../../../decisions/authoritative-source-terminology.md), and [source and packaging](../../../../decisions/source-and-packaging.md) decisions preserve accepted rationale.

## Contract

### Responsibility And Shape

- The loader is the first Open Forge `entrypoint` after the canonical workspace entry.
- It contains only the universal terms and Axioms required to enter, route, and interpret an installed Framework plus compact deterministic assistance and a generated registry of direct root routes.
- Its authored sections remain, in order, the introduction, `Terms`, one grouped `Axioms` section, and `Entries`.
- The Axioms groups remain, in order, `Authority And Inheritance`, `Routing`, `Tags And Loading` with `Defined Tags`, and `CLI` with `Applicable Commands`.
- Category-specific behavior stays in routed sources. Workflow execution, Memory transitions, primitive details, Extension mechanics, and complete CLI behavior do not move into the loader.
- The installed loader remains understandable without repository governance, source history, or the CLI.

### Authority And Inheritance

- Authority is expressed by role and scope rather than one undifferentiated file-order hierarchy.
- Platform constraints and runtime safety bound every action; clear user direction governs goals, priorities, consequential tradeoffs, and accepted changes within scope; declared external sources remain authoritative for delegated facts.
- Loaded ancestor Axioms remain active below them, while a child adds only scope-specific Axioms.
- Clear direction is not reconfirmed, an action request accepts decisions required to perform it, and unresolved ambiguity remains #Contextual until dependent work requires clarification.
- Apparent #CurrentTruth conflicts are investigated before either side changes. Accepted changes update their authoritative route or system and preserve useful context from the previous state.
- Narrower selected non-directive material may specialize broader material of the same type. Loaded directives add to ancestors and report conflicts instead of silently overriding them.

### Routing And Loading

- The loader defines only the terms required before navigation: `entrypoint`, `entry`, `description`, `responsibility`, `root route`, `framework route`, `scope route`, `scoped framework route`, `slug`, and `axiom`.
- `description` remains the pre-load route-selection surface. Optional `responsibility` bounds what an opened file is responsible for defining without creating authority or loading behavior.
- A Framework route is a standard Core or Memory route whose role and default contract are defined by Open Forge.
- Scope routes use concrete slugs and may appear before, after, or between Framework route segments. Only needed scoped Framework routes are initialized, and placement narrows their subject without changing their roles.
- Generated `Entries` remain navigation metadata. Detailed meaning comes from the routed destination or the authoritative source it identifies.
- Loading and tags change visibility, timing, or classification without creating authority.
- The loader remains authoritative for the meanings of #LoadNow, #KeepInMind, #Core, #Memory, #Extension, #Contextual, #CurrentTruth, and #Evergreen. Undefined tags remain routing and search signals.
- #LoadNow traverses only visible children of an already-loaded parent. #KeepInMind discovers every routed #KeepInMind result across the workspace and follows each result's visible #LoadNow closure without loading unrelated descendants.
- Every recovered result retains the authority and scope established by its route and content.
- A user-owned `{name}.overwrite.md` is not an independent route and loads immediately after its base.
- It inherits the base route, scope, and loading behavior, is not independently indexed or selected, and has final precedence only within that file's scope.

### Deterministic Assistance

- CLI commands automate the complete plain-file contract and never become prerequisites for ordinary inspection.
- `load --bodies` applies at every required #KeepInMind boundary, including handoff and closeout, rather than only task entry.
- The loader names only commands with compact universal triggers: `load --bodies`, `chain`, `index`, and `doctor`.
- Unimplemented commands, complete CLI help, installation lifecycles, and component internals stay outside the loader.

### Distribution And Generation

- Source and dogfood authored loader content remain identical.
- The final marker-bounded `Entries` body is derived locally and may differ when the installed root routes differ.
- Generated loader entries expose direct active root entrypoints only. They never flatten nested routes.
- Generated links resolve relative to `.agents/loader.md`; CLI route identities remain workspace-relative.
- The authored loader stays within 35 to 70 non-empty lines unless an accepted Framework change explicitly revises that review budget.

## Verification

- [`src/cli/cli.closure.test.ts`](../../../../../../../src/cli/cli.closure.test.ts) compares source and dogfood authored loader content while excluding generated entries.
- Loader registry tests verify direct root generation, compatibility entrypoint names, and failure before mutation when one folder has multiple recognized entrypoints.
- Load and chain tests verify loader-first order, visible transitive #LoadNow traversal, complete #KeepInMind discovery, inherited route order, and overwrite adjacency.
- Doctor tests verify generated-region integrity, route resolution, containment, retired tags, and structural Framework requirements.
- Core installation tests verify selected authority, truth, Template, and deterministic-assistance wording in the installed payload.
- Review the source and dogfood authored line count and exact alignment whenever the loader contract changes.
