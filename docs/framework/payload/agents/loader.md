# Loader

## Description

This descriptor governs `src/open-forge/.agents/loader.md`.

The loader is the first Open Forge `entrypoint` after `AGENTS.md`. It owns only the universal top-down authority, inheritance, routing, load-tag, and CLI-assistance contract plus the generated registry of direct root routes.

## Represents

The loader represents how an agent enters an installed workspace and follows visible routes without needing a tool or category-specific bootstrap knowledge.

## Contains

The installed loader contains a short introduction, terms, one grouped `## Axioms` section, and a final marker-bounded `## Entries` registry.

Its Axioms use these groups in order:

1. `Authority And Inheritance`
2. `Routing`
3. `Tags And Loading`, with a `Defined Tags` subgroup
4. `CLI`, with an `Applicable Commands` subgroup

Workflow execution, Memory state lifecycle, skill behavior, directive scope details, and other category behavior belong to their routed #LoadNow owners. The loader keeps only universal authority and defined-tag behavior.

## Authority Contract

Loaded ancestor Axioms remain active below them, and child `entrypoints` add only scope-specific behavior. Loaded Axioms remain mandatory unless higher-priority user instructions, platform constraints, runtime safety, or a declared external source of truth conflicts.

Local active truth overrides Open Forge defaults. A clear user instruction, correction, or confirmation is accepted within its stated scope without another confirmation. A request to act also accepts any decision required to perform that action. Ambiguous direction stays contextual until it is clarified before work depends on it.

Agents investigate apparent conflicts with #CurrentTruth before changing either side and report conflicts that remain unresolved. When higher-authority accepted direction changes #CurrentTruth, they update its owning route or system and preserve useful superseded context.

Narrower selected non-directive material is preferred over broader material of the same type when safe and allowed. Loaded directive scopes are additive and report conflicts rather than silently overriding ancestors.

## Routing Contract

The loader defines `entrypoint`, `entry`, `root route`, `framework route`, `scope route`, `scoped framework route`, `slug`, and `axiom` in installed language.

The loader exposes direct active root routes. Every routed folder has exactly one recognized `entrypoint`, and every folder in a nested route path has its own `entrypoint`. A scope route uses the same mechanism with a concrete slug.

Generated `Entries` remain navigation metadata. Entries without a load-policy tag are selected on demand from their path, description, tags, and current request. Routed owners contain detailed behavior.

When a user-owned `{name}.overwrite.md` exists, agents read it immediately after `{name}.md`. It inherits the base route and has final precedence within that file's scope.

## Loading Contract

#LoadNow is relative to an already-loaded parent. Agents read each tagged entry immediately in listed order; when the target is an `entrypoint`, its own visible Entries then apply the same rule.

#KeepInMind is the complete routed continuity set. Agents read it at task start or resume, after detected context restoration, and before a handoff or closeout. They recheck it during work only when its follow-ups may have changed. The owning content retains its normal authority.

## CLI Assistance Contract

When the Open Forge CLI is available, agents use each command that applies to the current work. The loader gives exact triggers for loading baseline context, inspecting inherited Axioms, regenerating route metadata, and validating structural changes.

Every command automates the same plain-file contract. CLI availability never becomes a prerequisite for reading or maintaining an Open Forge workspace manually.

## Category Registry

The loader ends with one generated entry for each direct child folder under `.agents/` that contains exactly one recognized category `entrypoint`.

Link destinations are concrete and relative to `.agents/loader.md`, such as `workspace/_workspace.md`. Link labels and tags derive from each category `entrypoint`; the registry never flattens nested category contents. CLI route identities remain workspace-relative, such as `.agents/workspace/_workspace.md`.

## Tags Contract

The loader defines #LoadNow, #KeepInMind, #Core, #Memory, #Extension, #Contextual, #CurrentTruth, and #Evergreen positively. Undefined tags remain routing and search signals. Load-policy tags change loading only; they do not create authority, scope, or precedence.

#CurrentTruth marks accepted current state within its stated scope.

#Evergreen marks material that must stay aligned with accepted current state. It creates no authority or load policy. When accepted state changes, agents update only affected #Evergreen material they may edit before work depends on it and no later than closeout, batching related updates when safe. They keep that material coherent with what it represents now, preserve useful superseded context in the matching decision or archive, and report affected material they cannot update.

## Why

The loader gives an agent one small universal contract while routed owners provide all specialized behavior.

## Alignment Checks

The implementation is aligned when it:

- is immediately routable after `AGENTS.md`
- keeps the source and dogfood authored loader body aligned
- groups Axioms in the required order and keeps `## Entries` last
- defines routing terms consistently, including `scope route`
- applies ancestor Axioms before child additions
- leaves category behavior to routed #LoadNow owners
- accepts clear user direction without redundant confirmation
- treats an action request as acceptance of decisions required to perform it
- keeps ambiguous direction contextual until work depends on it
- investigates apparent #CurrentTruth conflicts before changing either side
- updates an owned #CurrentTruth route when higher-authority accepted direction changes it
- defines #LoadNow relative to an already-loaded parent
- uses the narrowed complete #KeepInMind refresh triggers
- loads only user-owned `.overwrite.md` after its base
- gives compact, conditional triggers for `load --bodies`, `chain`, `index`, and `doctor`
- introduces tag definitions and CLI commands with prose plus explicit subheadings rather than ambiguous adjacent peer lists
- keeps CLI assistance equivalent to the complete plain-file contract
- generates direct root route entries only
- generates loader links relative to `.agents/loader.md` while leaving CLI route identities workspace-relative
- defines reserved tags without making them authority
- defines #CurrentTruth authority and #Evergreen synchronization as orthogonal
- synchronizes only affected editable #Evergreen material at a safe boundary
- keeps the authored portion within 35 to 70 non-empty lines
