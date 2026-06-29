# Open Forge Loader

This file is the required Open Forge entrypoint after `AGENTS.md`.

It defines how agents start loading this workspace.

Use the generated entries in this file to select relevant categories. Load a selected category entrypoint before exploring its routed files.

## Axioms

- Open Forge is a routing system.
- Read `Entries` in every loaded entrypoint before deciding what to load next.
- Apply defined tag behavior when reading generated `Entries`.
- Let the current request select all other entries by path, description, and tags.
- Load a category entrypoint before its routed files.
- Load a file's `.overwrite.md` companion after it; the overwrite takes precedence within the base file's scope.
- User instructions apply when safe and allowed.
- Local active truth overrides Open Forge defaults.
- Generated `Entries` are navigation metadata; only reserved load-policy tags affect loading.
- Treat archived memory, historical material, examples, external methods, temporary continuation material, and candidate learning as context unless restored or promoted.
- Detailed behavior belongs in the routed file or concept that owns it.

## Tags

### Axioms

- Defined tags have framework meaning when they appear in loaded content or generated `Entries`.
- Undefined tags are routing and search signals; read the entry path, description, and loaded entrypoint for their meaning.
- Entries without a load-policy tag are on-demand routes selected by the current request.
- Tag spelling and casing are stable.
- Workspace-wide tag behavior belongs here and must stay short.

### Defined Tags

- #LoadWithParentEntrypoint - Load this entry immediately after its parent entrypoint, in listed order. Applies only inside already loaded `Entries`.
- #Core - Base routing, workspace orientation, and agent primitive routes.
- #Memory - Persisted workspace state and memory routes.
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
- `.agents/directives/_directives.md` - Mandatory workspace modifiers; load for every request - #OpenForge #Core #Directive #Index #LoadWithParentEntrypoint
- `.agents/guidelines/_guidelines.md` - Contextual guidance for recurring decisions and scenarios - #OpenForge #Core #Guideline #Index
- `.agents/memory/_memory.md` - Memory state routes for human-AI work - #OpenForge #Memory #Index #LoadWithParentEntrypoint
- `.agents/patterns/_patterns.md` - Concrete reusable shapes for inspectable work - #OpenForge #Core #Pattern #Index
- `.agents/skills/_skills.md` - Bounded reusable agent capabilities and scoped routes - #OpenForge #Core #Skill #Index
- `.agents/workflows/_workflows.md` - Goal-oriented workflows and scoped workflow routes - #OpenForge #Core #Workflow #Index
- `.agents/workspace/_workspace.md` - Important workspace destinations and their scope - #OpenForge #Core #Workspace #Index
<!-- open-forge:generated-index:end -->
