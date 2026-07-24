# Open Forge Loader

This is the main Open Forge `entrypoint`. Read it after `AGENTS.md` to enter this workspace.

## Terms

- `entrypoint` - Markdown file that makes a folder routable; Open Forge uses `_{folder-name}.md`, while `index.md`, `_index.md`, `references.md`, and `_references.md` are compatibility names
- `entry` - Generated route line under `Entries`
- `root route` - Route exposed directly by this loader
- `framework route` - Core route installed and managed by Open Forge
- `scope route` - Local route that narrows meaning or ownership below it
- `scoped framework route` - Framework route initialized inside a `scope route`
- `slug` - Concrete folder name used in a route path
- `axiom` - Mandatory instruction under an `Axioms` heading in a loaded file

## Axioms

### Authority And Inheritance

- Axioms of loaded ancestor `entrypoints` apply below them; a child adds only what is specific to its scope
- Follow loaded `axioms` unless a higher-priority user, platform, safety, or declared external source-of-truth instruction conflicts; report unresolved conflicts
- User instructions apply when safe and allowed, and local active truth overrides Open Forge defaults
- A clear user instruction, correction, or confirmation is accepted within its stated scope; do not ask for the same confirmation again
- A request to act also accepts any decision required to perform that action; if the direction is ambiguous, keep it #Contextual and clarify before work depends on it
- Investigate an apparent conflict with #CurrentTruth before changing either side; report conflicts that remain unresolved
- When higher-authority accepted direction changes #CurrentTruth, update its owning route or system and preserve useful superseded context
- Prefer material in a narrower selected non-directive scope over broader material of the same type when safe and allowed; loaded directives add to ancestors, and conflicts are reported

### Routing

- Open Forge routes through small Markdown `entrypoints` whose generated `Entries` expose direct routes
- Load an `entrypoint` before opening its routed files, then use its `Entries` to select what the request needs
- A folder is routable only when it contains one recognized `entrypoint`; every folder in a nested route path needs its own `entrypoint`
- A `scope route` uses the same mechanism with a concrete `slug`; initialize a framework `entrypoint` inside it only when that scoped framework route is needed
- Generated `Entries` are navigation metadata. Entries without a load-policy tag are on-demand, and detailed behavior belongs to the routed owner.
- When `{name}.overwrite.md` exists, read it immediately after `{name}.md`; it inherits the base route and has final precedence within that file's scope

### Tags And Loading

Defined tags have the meanings below when they appear in loaded content or generated `Entries`; undefined tags remain routing and search signals.

#### Defined Tags

- #LoadNow - When this `entry` appears in an already-loaded parent's `Entries`, read it immediately in listed order. When it points to an `entrypoint`, apply the same rule to that file's `Entries`.
- #KeepInMind - Read the complete routed set at task start or resume, after detected context restoration, and before a handoff or closeout. Recheck it during work only when its follow-ups may have changed, and follow each result within its owner's authority.
- #Core - Base routing, workspace orientation, and agent primitive routes
- #Memory - Self-growing Markdown memory for workspace state, AI communication, current records, historical records, and learning
- #Extension - Optional extension payload, template, integration, and support routes
- #Contextual - Supporting context, not accepted current truth unless restored, validated, accepted, or promoted
- #CurrentTruth - Accepted current state within its stated scope, below user instructions, runtime safety, platform constraints, and declared external sources of truth
- #Evergreen - Material that must stay aligned with accepted current state. It creates no authority or load policy.
  - When accepted state changes, update only affected #Evergreen material you may edit before work depends on it, and no later than closeout; batch related updates when safe
  - Keep #Evergreen material coherent with what it represents now; preserve useful superseded context in the matching decision or archive, and report affected material you cannot update

### CLI

When the Open Forge CLI is available, use each applicable command below. Every command automates the same plain-file contract, which remains complete without the CLI.

#### Applicable Commands

- `open-forge load --bodies` - Read effective baseline context at task start or resume
- `open-forge chain <route> --heading Axioms` - Read inherited rules after selecting a route
- `open-forge index` - Rebuild generated `Entries` after adding, moving, or removing a routed file or changing its route metadata
- `open-forge doctor` - Validate routing after structural framework changes and before closing them out

## Entries

<!-- open-forge:generated-index:start -->
- [Binding instructions whose route is selected before their contents are loaded](directives/_directives.md) - #LoadNow #Core #Directive
- [Contextual advice for recurring choices, tradeoffs, and work scenarios](guidance/_guidance.md) - #LoadNow #Core #Guidance
- [Self-growing markdown memory for workspace state, AI communication, current records, historical records, and learning](memory/_memory.md) - #LoadNow #Memory #OrganicGrowth
- [Concrete reusable shapes for code, files, APIs, documents, and other inspectable work](patterns/_patterns.md) - #LoadNow #Core #Pattern
- [Skills available through standard SKILL.md files](skills/_skills.md) - #LoadNow #Core #Skill
- [Repeatable markdown workflow recipes for reaching a defined goal](workflows/_workflows.md) - #LoadNow #Core #Workflow
- [Workspace routes that point to important project locations and explain when to use them](workspace/_workspace.md) - #LoadNow #Core #Workspace
<!-- open-forge:generated-index:end -->
