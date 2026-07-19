# Open Forge Loader

This file is the required Open Forge `entrypoint` after `AGENTS.md`.

It defines how agents start loading this workspace.

Use generated `Entries` in this file to select relevant root routes. Load a selected `entrypoint` before exploring its routed files.

## Terms

- `entrypoint` - Markdown file that makes a folder routable. Open Forge uses `_{folder-name}.md`; compatibility names are `index.md`, `_index.md`, `references.md`, and `_references.md`.
- `entry` - Generated line under `Entries` that points to a sibling markdown file, direct child `entrypoint`, or native skill package entrypoint.
- `framework route` - Core route installed and managed by Open Forge.
- `scope route` - Local route used to narrow meaning or ownership for routes below it.
- `scoped framework route` - `framework route` initialized inside a `scope route`.
- `slug` - Concrete folder name used in a route path.
- `axiom` - Mandatory instruction in a loaded Open Forge file.

## Axioms

- Open Forge routes agents through small markdown `entrypoints`.
- Follow all loaded `axioms` unless a higher-priority user, platform, safety, or external source-of-truth instruction conflicts; report unresolved conflicts.
- A required memory action is complete only after its file is written or a blocker is reported; a promise to write it later does not satisfy closeout.
- A folder is routable only when it contains one recognized `entrypoint`.
- An `entrypoint` explains what its folder is for and ends with generated `Entries`.
- `Entries` list sibling markdown files, direct child `entrypoints`, and supported native skill packages inside skills routes.
- Native skill package folders are routed by their `SKILL.md`; package internals are loaded only when the skill says they are relevant.
- To route into nested folders, every folder in the path needs its own `entrypoint`.
- `scope routes` use the same mechanism: add `slug` folders with `entrypoints` before, after, or between `framework routes` when they make ownership clearer.
- `scoped framework routes` work only when their framework `entrypoint` exists inside the scope.
- In every loaded `entrypoint`, read `Entries` and load entries that fit the request or carry a defined load-policy tag.
- Before non-trivial work, infer the established development state from routed current truth and the transition requested by the user, then select the installed workflow whose Goal best covers that transition.
- Recommend at most one prerequisite workflow first only when a concrete missing or contradictory input would make the requested work unreliable; name the gap and ask once whether to use that handoff or proceed with explicit assumptions.
- Workflow phases are wayfinding, not a waterfall: work may start anywhere, skip, repeat, or move backward, and an explicit workflow choice or opt-out wins.
- Honor an explicit user request to use no workflow, proceed directly, or equivalent language without asking again.
<!-- open-forge-augment.workflow-selection:start -->
<!-- open-forge-augment.workflow-selection:end -->
- Axioms of loaded ancestor `entrypoints` apply to all routes below them; a child `entrypoint` adds only what is specific to its scope.
- Prefer material in a narrower selected scope over broader material of the same type when safe and allowed; report unresolved conflicts.
- Immediately read #LoadNow entries when they appear in loaded `Entries`, in listed order.
- Treat the complete routed #KeepInMind catalogue as baseline-loaded, binding follow-up context. At task start or resume, after context restoration or compaction, at meaningful phase transitions or handoffs, and before closeout, read or recheck every result; when the CLI is available, use `open-forge find --tag KeepInMind --bodies` as the single complete lookup.
- For long-running work, maintain one concise active working-session checkpoint with the accepted direction, current phase, decisions, evidence, unresolved questions, and next action. Refresh it at the same continuity boundaries and reread it with this loader after context restoration.
- Load an `entrypoint` before its routed files.
- Load a file's `.overwrite.md` companion after it; the overwrite takes precedence within the base file's scope.
- User instructions apply when safe and allowed.
- Local active truth overrides Open Forge defaults.
- Prefer written #Memory routes over private or opaque agent memory; durable memory belongs in markdown routes and remembered context must be validated against loaded files.
- Writing files inside existing routes is normal use; add child categories when they improve routing, ownership, or clarity, and give new root routes clear scope.
- Generated `Entries` are navigation metadata; only reserved load-policy tags affect loading.
- Treat archived memory, historical material, examples, external methods, temporary continuation material, and candidate learning as context unless restored or promoted.
- Detailed behavior belongs in the routed file or concept that owns it.
- When the Open Forge CLI is available, prefer it for deterministic route lookup, inherited-chain inspection, index regeneration, and structural validation; plain-file traversal remains the fallback.

## Route Patterns

`[scope]` means a concrete `slug` folder with its own `entrypoint`. These patterns explain shape; generated `entries` use concrete paths.

- `.agents/memory/crystallized/documents/_documents.md` - `framework route` without extra scope.
- `.agents/memory/[scope]/crystallized/documents/_documents.md` - `scoped framework route` under a scope that owns memory states.
- `.agents/memory/crystallized/[scope]/documents/_documents.md` - `scoped framework route` inside crystallized memory.

## Tags

### Axioms

- Defined tags have framework meaning when they appear in loaded content or generated `Entries`.
- Undefined tags are routing and search signals; read the `entry` path, description, and loaded `entrypoint` for their meaning.
- `Entries` without a load-policy tag are on-demand routes selected by the current request.
- Tag spelling and casing are stable.
- Workspace-wide tag behavior belongs here and must stay short.

### Defined Tags

- #LoadNow - Read this `entry` when it appears in loaded `Entries`, in listed order. If the target is a category `entrypoint`, read that file first; its own `Entries` then apply the same rule.
- #KeepInMind - Baseline-loaded, binding follow-up context. Read the complete routed set at every loader or continuity refresh and perform its follow-ups at the stated trigger.
- #Core - Base routing, workspace orientation, and agent primitive routes.
- #Memory - Self-growing markdown memory for workspace state, AI communication, current records, historical records, and learning.
- #Extension - Optional extension payload, template, integration, and support routes.
- #Contextual - Supporting context, not accepted current truth unless restored, validated, accepted, or promoted.
- #CurrentTruth - Accepted current memory within its stated scope; still below user instructions, runtime safety, platform constraints, and declared external sources of truth.

## CLI

- `open-forge find` - find routed context by tag or route and optionally follow Required Routes.
- `open-forge chain <route> --heading <title>` - inspect loader, ancestor, skill, target, and overwrite context for any Markdown heading such as Axioms, Mode, Goal, Constraints, or a local category heading.
- `open-forge doctor` - validate routing, workflow shape and phase, directive binding, generated regions, and route dependencies.
- `open-forge index` - rebuild generated routing after adding, moving, or removing routed material.
- `open-forge extend --list` - inspect optional capabilities before choosing what earns installation and context cost.
- CLI reads and writes reject route trees that escape the selected target through a symbolic link or junction; an explicitly trusted external Markdown mount is a manual plain-file boundary.

## Customization

Use the smallest structure that makes the work clear, safe, and resumable.

Customize in this order:

1. Add local files.
2. Use overwrite files (`{file-name}.overwrite.md`) for additive or lightly modifying behavior.
3. Edit framework files when a complete behavior change is required.

Do not create parallel truth when active truth already exists. Update the active truth and preserve history locally.

## Entries

<!-- open-forge:generated-index:start -->
- `.agents/directives/_directives.md` - Binding instructions whose route is selected before their contents are loaded - #LoadNow #Core #Directive
- `.agents/guidance/_guidance.md` - Contextual advice for recurring choices, tradeoffs, and work scenarios - #LoadNow #Core #Guidance
- `.agents/memory/_memory.md` - Self-growing markdown memory for workspace state, AI communication, current records, historical records, and learning - #LoadNow #Memory #OrganicGrowth
- `.agents/patterns/_patterns.md` - Concrete reusable shapes for code, files, APIs, documents, and other inspectable work - #LoadNow #Core #Pattern
- `.agents/skills/_skills.md` - Reusable agent capability packages with clear use cases and expected results - #LoadNow #Core #Skill
- `.agents/workflows/_workflows.md` - Repeatable markdown workflow recipes for reaching a defined goal - #LoadNow #Core #Workflow
- `.agents/workspace/_workspace.md` - Workspace routes that point to important project locations and explain when to use them - #LoadNow #Core #Workspace
<!-- open-forge:generated-index:end -->
