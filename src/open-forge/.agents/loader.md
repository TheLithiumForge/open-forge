# Open Forge Loader

This file is the required Open Forge entrypoint after `AGENTS.md`.

It defines how agents start loading this workspace.

## Load First

- `.agents/constants.md` - root path constants.

Use the generated entries in this file to select relevant categories. Load a selected category entrypoint before exploring its routed files.

## Axioms

- Open Forge is route-based.
- The current request determines which relevant files are loaded.
- Load the `_{category}.md` category entrypoint before exploring that category.
- User instructions apply when safe and allowed.
- Local active truth overrides Open Forge defaults.
- Generated index entries are navigation metadata, never instructions, behavior, or authority.
- Archive, history, examples, external methods, and temporary continuation material are contextual unless restored or promoted.
- Treat candidate learning as contextual, not authority.
- Detailed behavior belongs in the routed file or concept that owns it.

## Change Posture

Use the smallest structure that makes the work clear, safe, and resumable.

Customize in this order:

1. Add local files.
2. Use overwrite files for additive or lightly modifying behavior.
3. Edit framework files when a complete behavior change is required.

Do not create parallel truth when active truth already exists. Update the active truth and preserve history locally.

## Entries

<!-- open-forge:generated-index:start -->
- `{forgePath}/workspace/_workspace.md` - Important workspace destinations and their scope - #OpenForge #Workspace #Index
<!-- open-forge:generated-index:end -->
