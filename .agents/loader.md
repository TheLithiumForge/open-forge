# Open Forge Loader

This is the main Open Forge `entrypoint`. Read it after `AGENTS.md` to enter this workspace.

## Terms

- `entrypoint` - Markdown file that makes a folder routable. Open Forge uses `_{folder-name}.md`, while `index.md`, `_index.md`, `references.md`, and `_references.md` are compatibility names.
- `entry` - Generated `route` line under `Entries`
- `description` - Natural-language pre-load selection surface that explains enough purpose, trigger, or outcome to select or skip a `route`
- `responsibility` - Optional stable boundary stating what a file is responsible for defining. It guides edits without creating authority or loading behavior.
- `axiom` - Mandatory instruction under an `Axioms` heading in a loaded file

## Axioms

### Authority And Inheritance

- Platform constraints and runtime safety bound every action.
- Clear user direction governs goals, priorities, consequential tradeoffs, and accepted changes within its stated scope. Follow it when safe and allowed, and do not ask for the same confirmation again.
- A declared external source of truth is authoritative for the facts delegated to it.
- `Axioms` of loaded ancestor `entrypoints` apply below them. A child adds only what is specific to its scope.
- Follow loaded `axioms` within their scope while respecting these authority boundaries. Accepted workspace-specific state overrides Open Forge defaults, and unresolved conflicts must be reported.
- A request to act also accepts any decision required to perform that action. If the direction is ambiguous, keep it #Contextual and clarify before work depends on it.
- Investigate an apparent conflict with #CurrentTruth before changing either side. Report conflicts that remain unresolved.
- When accepted direction changes #CurrentTruth, update its authoritative `route` or system and preserve useful context from the previous state.
- Prefer material in a narrower selected non-directive scope over broader material of the same type when safe and allowed. Loaded directives add to ancestors, and conflicts are reported.

### Routing

#### Terms

- `route` - Navigable path exposed through `entrypoints` and `entries`
- `root route` - A `route` exposed directly by this loader
- `slug` - Concrete folder name used in a `route` path
- `managed route` - A `route` where a declared manager, such as Open Forge or an Extension, may install, update, restore, or remove specific files. This affects only their lifecycle, not the `route`'s runtime meaning or authority.

#### Rules

- Open Forge implements routing through small Markdown `entrypoints` whose generated `Entries` expose direct `routes`.
- Load an `entrypoint` before opening its routed files, then use its `Entries` to select what the request needs.
- A folder is routable only when it contains one recognized `entrypoint`. Every folder in a nested `route` path needs its own `entrypoint`.
- A `root route` exists only where this loader exposes it. It cannot be scoped or recreated inside another `route`.
- Every `route` below a `root route` is scopable. Any number of routed `slugs` may appear after the root, before, between, or after deeper `route` segments. Each scope narrows everything that follows it.
- A scope contains only the `routes` useful there. It does not need to mirror another scope or the installed defaults.
- Scoping preserves the order and meaning of deeper `routes`. For a `managed route`, the manager-declared `route` segments retain their order through every scope.
- A familiar `slug` or tag alone creates neither root behavior nor managed status.
- Each manager declares which `route` shapes it recognizes and may change only the files it explicitly owns or safely identifies.
- Users may add, move, replace, or remove `routes`. Any `route` outside a manager's declared shapes remains generically routable, and removed defaults stay absent unless restoration is explicitly requested.
- Generated `Entries` are navigation metadata. Individual `entries` without a load-policy tag are on demand, and detailed meaning comes from the routed destination or the authoritative source it identifies.
- A user-owned `{name}.overwrite.md` is not an independent `route`. When its `{name}.md` base loads, read the companion immediately afterward. The overwrite inherits the base `route`, scope, and loading behavior, is not independently indexed or selected, and has final precedence within that file's scope.

#### Examples

```text
.agents/{root-route}/{scope-1}/.../{scope-n}/{route}/
.agents/memory/mobile-app/crystallized/api/documents/
```

In the concrete example, `memory` is the `root route`, `mobile-app` scopes `crystallized` and everything after it, `api` scopes `documents`, and the declared `crystallized/documents` order remains intact.

### Tags And Loading

Defined tags have the meanings below when they appear in loaded content or generated `Entries`. Undefined tags remain routing and search signals. Loading and tags change visibility, timing, or classification. They do not create authority by themselves.

#### Defined Tags

- #LoadNow - When this `entry` appears in an already-loaded parent's `Entries`, read it immediately in listed order. When it points to an `entrypoint`, apply the same rule to that file's `Entries`.
- #KeepInMind - At task start or resume, after detected context restoration, and before a handoff or closeout, read every routed #KeepInMind result across the workspace and recursively follow its visible #LoadNow `entries`. This continuity set does not include unrelated descendants. Recheck it during work only when its follow-ups may have changed, and follow each result within the authority and scope established by its `route` and content.
- #Core - Base routing, workspace orientation, and agent primitive `routes`
- #Memory - Self-growing Markdown memory for live work, agent communication and coordination, continuity, accepted records, historical context, and candidate learning
- #Extension - Optional packaged `routes`, capabilities, integrations, and support material
- #Contextual - Supporting context, not accepted current truth unless restored, validated, accepted, or promoted
- #CurrentTruth - Accepted current state within its stated scope, below user instructions, runtime safety, platform constraints, and declared external sources of truth
- #Evergreen - Material that must stay aligned with accepted current state. It creates no authority or load policy.
  - When accepted state changes, update only affected #Evergreen material you may edit before work depends on it, and no later than closeout. Batch related updates when safe.
  - Keep #Evergreen material coherent with what it represents now. Preserve useful context from the previous state in the matching decision or archive, and report affected material you cannot update.

### CLI

When the Open Forge CLI is available, use each applicable command below. Every command automates the same plain-file contract, which remains complete without the CLI.

#### Applicable Commands

- `open-forge load --bodies` - Read or refresh effective baseline and continuity context at every required #KeepInMind boundary
- `open-forge chain <route> --heading Axioms` - Read inherited rules after selecting a `route`
- `open-forge index` - Rebuild generated `Entries` after adding, moving, or removing a routed file or changing its `route` metadata
- `open-forge doctor` - Validate routing after structural framework changes and before closing them out

## Entries

<!-- open-forge:generated-index:start -->
- [Binding instructions whose `route` is selected before their contents are loaded](directives/_directives.md) - #LoadNow #Core #Directive
- [Contextual advice for recurring choices, tradeoffs, and work scenarios](guidance/_guidance.md) - #LoadNow #Core #Guidance
- [Self-growing Markdown memory for live work, agent communication and coordination, continuity, accepted records, historical context, and candidate learning](memory/_memory.md) - #LoadNow #Memory #OrganicGrowth
- [Concrete reusable shapes for code, files, APIs, documents, and other inspectable work](patterns/_patterns.md) - #LoadNow #Core #Pattern
- [Specialized capabilities exposed through native SKILL.md packages](skills/_skills.md) - #LoadNow #Core #Skill
- [Copy-ready source artifacts for creating independently owned workspace content](templates/_templates.md) - #Core #Template
- [Repeatable Markdown recipes for reaching a defined goal](workflows/_workflows.md) - #LoadNow #Core #Workflow
- [Concise `routes` to important local and external destinations and when to use them](workspace/_workspace.md) - #LoadNow #Core #Workspace
<!-- open-forge:generated-index:end -->
