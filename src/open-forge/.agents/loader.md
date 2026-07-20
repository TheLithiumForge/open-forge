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
- Prefer material in a narrower selected non-directive scope over broader material of the same type when safe and allowed; loaded directives add to ancestors, and conflicts are reported

### Routing

- Open Forge routes through small Markdown `entrypoints` whose generated `Entries` expose direct routes
- Load an `entrypoint` before opening its routed files, then use its `Entries` to select what the request needs
- A folder is routable only when it contains one recognized `entrypoint`; every folder in a nested route path needs its own `entrypoint`
- A `scope route` uses the same mechanism with a concrete `slug`; initialize a framework `entrypoint` inside it only when that scoped framework route is needed
- Generated `Entries` are navigation metadata. Entries without a load-policy tag are on-demand, and detailed behavior belongs to the routed owner.
- When `{filename}.overwrite.md` exists, read it immediately after `{filename}.md`; it inherits the base route and has final precedence within that file's scope

### Tags And Loading

- Defined tags have the meanings below when they appear in loaded content or generated `Entries`; undefined tags remain routing and search signals
- #LoadNow - When this `entry` appears in an already-loaded parent's `Entries`, read it immediately in listed order. When it points to an `entrypoint`, apply the same rule to that file's `Entries`.
- #KeepInMind - Read the complete routed set at task start or resume, after detected context restoration, and before a handoff or closeout. Recheck it during work only when its follow-ups may have changed, and follow each result within its owner's authority.
- #Core - Base routing, workspace orientation, and agent primitive routes
- #Memory - Self-growing Markdown memory for workspace state, AI communication, current records, historical records, and learning
- #Extension - Optional extension payload, template, integration, and support routes
- #Contextual - Supporting context, not accepted current truth unless restored, validated, accepted, or promoted
- #CurrentTruth - Accepted current memory within its stated scope, below user instructions, runtime safety, platform constraints, and declared external sources of truth
- `open-forge load --bodies` may batch this same traversal when available; plain-file traversal remains complete

## Entries

<!-- open-forge:generated-index:start -->

- `.agents/directives/_directives.md` - Binding instructions whose route is selected before their contents are loaded - #LoadNow #Core #Directive
- `.agents/guidance/_guidance.md` - Contextual advice for recurring choices, tradeoffs, and work scenarios - #LoadNow #Core #Guidance
- `.agents/memory/_memory.md` - Self-growing markdown memory for workspace state, AI communication, current records, historical records, and learning - #LoadNow #Memory #OrganicGrowth
- `.agents/patterns/_patterns.md` - Concrete reusable shapes for code, files, APIs, documents, and other inspectable work - #LoadNow #Core #Pattern
- `.agents/skills/_skills.md` - Skills available through standard SKILL.md files - #LoadNow #Core #Skill
- `.agents/workflows/_workflows.md` - Repeatable markdown workflow recipes for reaching a defined goal - #LoadNow #Core #Workflow
- `.agents/workspace/_workspace.md` - Workspace routes that point to important project locations and explain when to use them - #LoadNow #Core #Workspace
  <!-- open-forge:generated-index:end -->
