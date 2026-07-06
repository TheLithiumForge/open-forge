# Open Forge Loader

This file is the required Open Forge `entrypoint` after `AGENTS.md`.

It defines how agents start loading this workspace.

Use generated `Entries` in this file to select relevant root routes. Load a selected `entrypoint` before exploring its routed files.

## Terms

- `entrypoint` - Markdown file that makes a folder routable. Open Forge uses `_{folder-name}.md`; compatibility names are `index.md`, `_index.md`, `references.md`, and `_references.md`.
- `entry` - Generated line under `Entries` that points to a sibling markdown file or direct child `entrypoint`.
- `framework route` - Route installed and managed by Open Forge.
- `scope route` - Local route used to narrow meaning or ownership for routes below it.
- `scoped framework route` - `framework route` initialized inside a `scope route`.
- `slug` - Concrete folder name used in a route path.

## Axioms

- Open Forge routes agents through small markdown `entrypoints`.
- A folder is routable only when it contains one recognized `entrypoint`.
- An `entrypoint` explains what its folder is for and ends with generated `Entries`.
- `Entries` list sibling markdown files and direct child `entrypoints`.
- To route into nested folders, every folder in the path needs its own `entrypoint`.
- `scope routes` use the same mechanism: add `slug` folders with `entrypoints` before, after, or between `framework routes` when they make ownership clearer.
- `scoped framework routes` work only when their framework `entrypoint` exists inside the scope.
- Use `Entries` to choose the next file or child `entrypoint` by path, description, and tags.
- Apply defined tag behavior when reading generated `Entries`.
- Load an `entrypoint` before its routed files.
- Load a file's `.overwrite.md` companion after it; the overwrite takes precedence within the base file's scope.
- User instructions apply when safe and allowed.
- Local active truth overrides Open Forge defaults.
- Prefer written #Memory routes over private or opaque agent memory; durable memory belongs in markdown routes and remembered context must be validated against loaded files.
- Generated `Entries` are navigation metadata; only reserved load-policy tags affect loading.
- Treat archived memory, historical material, examples, external methods, temporary continuation material, and candidate learning as context unless restored or promoted.
- Detailed behavior belongs in the routed file or concept that owns it.

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

- #LoadWithParentEntrypoint - Load this `entry` immediately after its parent `entrypoint`, in listed order. Applies only inside already loaded `Entries`.
- #LoadForPostWorkReview - Load this `entry` before ending meaningful work to route useful material produced during the work. Applies only inside already loaded `Entries`.
- #Core - Base routing, workspace orientation, and agent primitive routes.
- #Memory - Self-growing markdown memory for workspace state, AI communication, current records, historical records, and learning.
- #Extension - Optional extension payload, template, integration, and support routes.
- #Contextual - Supporting context, not accepted current truth unless restored, validated, accepted, or promoted.
- #CurrentTruth - Accepted current memory within its stated scope; still below user instructions, runtime safety, platform constraints, and declared external sources of truth.

## Customization

Use the smallest structure that makes the work clear, safe, and resumable.

Customize in this order:

1. Add local files.
2. Use overwrite files for additive or lightly modifying behavior.
3. Edit framework files when a complete behavior change is required.

Do not create parallel truth when active truth already exists. Update the active truth and preserve history locally.

## Entries

<!-- open-forge:generated-index:start -->
- `.agents/directives/_directives.md` - Mandatory instructions agents must follow when they apply to the current work - #OpenForge #Core #Directive #Index #LoadWithParentEntrypoint
- `.agents/guidance/_guidance.md` - Contextual advice for recurring choices, tradeoffs, and work scenarios - #OpenForge #Core #Guidance #Index
- `.agents/memory/_memory.md` - Self-growing markdown memory for workspace state, AI communication, current records, historical records, and learning - #OpenForge #Memory #OrganicGrowth #Index #LoadWithParentEntrypoint
- `.agents/patterns/_patterns.md` - Concrete reusable shapes for code, files, APIs, documents, and other inspectable work - #OpenForge #Core #Pattern #Index
- `.agents/skills/_skills.md` - Reusable agent capabilities with clear use cases and expected results - #OpenForge #Core #Skill #Index
- `.agents/workflows/_workflows.md` - Repeatable agent workflows for reaching a defined goal - #OpenForge #Core #Workflow #Index
- `.agents/workspace/_workspace.md` - Workspace routes that point to important project locations and explain when to use them - #OpenForge #Core #Workspace #Index
<!-- open-forge:generated-index:end -->
