# Open Forge Loader

This file is the required Open Forge entrypoint after `AGENTS.md`.

It defines how agents start loading this workspace.

Use the generated entries in this file to select relevant categories. Load a selected category entrypoint before exploring its routed files.

## Axioms

- Open Forge is route-based.
- Load the directives category for every request when it appears in Entries.
- Let the current request select all other relevant files.
- Load a category entrypoint before its routed files.
- Load a file's `.overwrite.md` companion after it; the overwrite takes precedence within the base file's scope.
- User instructions apply when safe and allowed.
- Local active truth overrides Open Forge defaults.
- Generated index entries are navigation metadata, never instructions, behavior, or authority.
- Treat archives, history, examples, external methods, temporary continuation material, and candidate learning as context unless restored or promoted.
- Detailed behavior belongs in the routed file or concept that owns it.

## Change Posture (use other name please)

Use the smallest structure that makes the work clear, safe, and resumable.

Customize in this order:

1. Add local files.
2. Use overwrite files for additive or lightly modifying behavior.
3. Edit framework files when a complete behavior change is required.

Do not create parallel truth when active truth already exists. Update the active truth and preserve history locally.

## Entries

<!-- open-forge:generated-index:start -->

- `.agents/directives/_directives.md` - Mandatory workspace modifiers; load for every request - #OpenForge #Directives #Global #Index
- `.agents/guidelines/_guidelines.md` - Contextual guidance for recurring decisions and scenarios - #OpenForge #Guidelines #Index
- `.agents/patterns/_patterns.md` - Concrete reusable shapes for inspectable work - #OpenForge #Patterns #Index
- `.agents/skills/_skills.md` - Bounded reusable agent capabilities and scoped routes - #OpenForge #Skills #Index
- `.agents/workflows/_workflows.md` - Goal-oriented agent modules and scoped workflow routes - #OpenForge #Workflows #Index
- `.agents/workspace/_workspace.md` - Important workspace destinations and their scope - #OpenForge #Workspace #Index
  <!-- open-forge:generated-index:end -->
